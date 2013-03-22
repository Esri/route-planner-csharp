/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Reflection;

using ESRI.ArcLogistics.App.Pages;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class that represents a application's command manager.
    /// </summary>
    public class CommandManager
    {
        #region Constructiors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>CommandManager</c> class.
        /// </summary>
        internal CommandManager()
        {
            // 1. Loads commands from Commands.xml embedded resource file.
            _LoadInternalCommands();

            // 2. Loads custom commands.
            _LoadCustomCommands();
        }

        #endregion // Constructiors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns collection of command for selected category (by name).
        /// </summary>
        /// <remarks>Collection is read-only. Can contain 0 commands.</remarks>
        public ReadOnlyCollection<AppCommands.ICommand> GetCategoryCommands(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
                throw new ArgumentNullException("categoryName"); // exception

            var commands = _categories.ContainsKey(categoryName) ?
                                 (List<AppCommands.ICommand>)_categories[categoryName] :
                                 new List<AppCommands.ICommand>();
            return commands.AsReadOnly();
        }

        #endregion // Public methods

        #region Internal methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Try to execute command.
        /// </summary>
        /// <param name="currentPage">Current application page.</param>
        /// <param name="e">Key down event args.</param>
        internal void ExecuteCommand(Page currentPage, KeyEventArgs e)
        {
            foreach (var command in _FindCommandsForKey(currentPage, e))
                command.Execute();
        }

        #endregion // Internal methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads commands from Commands.xml embedded resource file.
        /// </summary>
        private void _LoadInternalCommands()
        {
            // get commands.xml stream
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream xmlStream = assembly.GetManifestResourceStream(APP_COMMANDS_DESCRIPTION);

            // deserialize Categories Info
            var ser = new XmlSerializer(typeof(CategoriesInfo));
            CategoriesInfo _catsInfo = null;
            try
            {
                _catsInfo = (CategoriesInfo)ser.Deserialize(xmlStream);
            }
            finally
            {
                xmlStream.Close();
            }

            // create commands and categories
            _CreateCommandsAndCategories(_catsInfo);
        }

        /// <summary>
        /// Adds command to category commands.
        /// </summary>
        /// <param name="cnd">Commad to adding.</param>
        /// <param name="categoryCommands">Parent category.</param>
        private void _AddCommandToCategory(ICommand cmd, List<ICommand> categoryCommands)
        {
            ICommand cmdToAdding = null;
            if (_commands.Contains(cmd.Name))
                cmdToAdding = (ICommand)_commands[cmd.Name]; // get command from command cache
            else
            {
                cmdToAdding = cmd;

                // add commands to commands array
                _commands.Add(cmdToAdding.Name, cmdToAdding);
                cmdToAdding.Initialize(App.Current);
            }

            // add comannd to category array
            if (!categoryCommands.Contains(cmdToAdding))
                categoryCommands.Add(cmdToAdding);
        }

        /// <summary>
        /// Creates commands and categories from configuration.
        /// </summary>
        /// <param name="catsInfo">Categories information.</param>
        private void _CreateCommandsAndCategories(CategoriesInfo catsInfo)
        {
            foreach (CategoryInfo catInfo in catsInfo.Categories)
            {
                _pageByGroupDictionary.Add(catInfo.Name, catInfo.PageType);

                var catCommands = new List<ICommand>(catInfo.Commands.Length);
                foreach (CommandInfo cmdInfo in catInfo.Commands)
                {
                    Type type = Type.GetType(cmdInfo.Type);
                    if (type == null)
                    {
                        string error =
                            App.Current.GetString("CommandTypeCannotBeFound", cmdInfo.Type);
                        throw new ApplicationException(error); // exception
                    }

                    var cmd = (ICommand)Activator.CreateInstance(type);
                    _AddCommandToCategory(cmd, catCommands);
                }

                // add category to the categories cache
                Debug.Assert(!_categories.ContainsKey(catInfo.Name)); // only unique
                _categories.Add(catInfo.Name, catCommands);
            }
        }

        /// <summary>
        /// Adds custom commands (safe).
        /// </summary>
        /// <param name="pluginType">Plug-In type.</param>
        private void _AddCustomCommands(Type pluginType)
        {
            try
            {
                var instance = (ICommand)Activator.CreateInstance(pluginType);
                if (!_commands.Contains(instance.Name)) // NOTE: only for unique named command
                {
                    var attribute =
                        (CommandPlugInAttribute)Attribute.GetCustomAttribute(pluginType,
                                                                             typeof(CommandPlugInAttribute));
                    foreach (string categoryName in attribute.Categories)
                    {
                        List<ICommand> catCommands = null;
                        if (_categories.ContainsKey(categoryName))
                            catCommands = (List<ICommand>)_categories[categoryName];
                        else
                        {
                            catCommands = new List<ICommand>();
                            _categories.Add(categoryName, catCommands);
                        }

                        _AddCommandToCategory(instance, catCommands);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        /// <summary>
        /// Loads custom commands (from Plug-Ins).
        /// </summary>
        private void _LoadCustomCommands()
        {
            string typeName = typeof(ICommand).ToString();

            ICollection<string> assemblyFiles = CommonHelpers.GetAssembliesFiles();
            foreach (string assemblyPath in assemblyFiles)
            {
                // safely loading custom commands (founded assembly by assembly)
                try
                {
                    Assembly pluginAssembly = Assembly.LoadFrom(assemblyPath);
                    foreach (Type pluginType in pluginAssembly.GetTypes())
                    {
                        if (!pluginType.IsPublic || pluginType.IsAbstract)
                            continue; // NOTE: skip this type

                        Type typeInterface = pluginType.GetInterface(typeName, true);
                        if ((null != typeInterface) &&
                             Attribute.IsDefined(pluginType, typeof(CommandPlugInAttribute)))
                            // NOTE: specifying this attribute is obligatory
                            _AddCustomCommands(pluginType);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        /// <summary>
        /// Finds collection of commands to be executed for when the specified
        /// key event occured on the specified page.
        /// </summary>
        /// <param name="currentPage">Current application page.</param>
        /// <param name="e">Key down event args.</param>
        private IEnumerable<ICommand> _FindCommandsForKey(Page currentPage, KeyEventArgs e)
        {
            List<ICommand> commands = new List<ICommand>();

            ICollection<string> categories = _categories.Keys;
            foreach (string category in categories)
            {
                if (!_pageByGroupDictionary.ContainsKey(category))
                    continue; // NOTE: skip category

                string pageType = _pageByGroupDictionary[category];
                if (!currentPage.GetType().FullName.Equals(pageType))
                    continue; // NOTE: category not for current page - skip

                foreach (ICommand command in _categories[category])
                {
                    try
                    {
                        // WORKARROUND: safely adding
                        //  KeyGesture in plugin throw exception
                        KeyGesture gesture = command.KeyGesture;
                        if ((gesture != null) &&
                            (gesture.Key == e.Key) &&
                            (gesture.Modifiers == Keyboard.Modifiers) &&
                            command.IsEnabled &&
                            !commands.Contains(command))
                        {
                            commands.Add(command);
                        }
                    }
                    catch
                    { }
                }
            }

            return commands;
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Source application commands description file name.
        /// </summary>
        private const string APP_COMMANDS_DESCRIPTION =
            "ESRI.ArcLogistics.App.Commands.Commands.xml";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Maps category names to the list of commands for the category.
        /// </summary>
        private Dictionary<string, List<ICommand>> _categories =
            new Dictionary<string, List<ICommand>>();

        /// <summary>
        /// Hashtable that contains pairs: Command Name -> ICommand reference.
        /// </summary>
        private Hashtable _commands = new Hashtable();

        /// <summary>
        /// Dictionary to store groups for all pages.
        /// </summary>
        private Dictionary<string, string> _pageByGroupDictionary =
            new Dictionary<string, string>();

        #endregion Private members
    }
}
