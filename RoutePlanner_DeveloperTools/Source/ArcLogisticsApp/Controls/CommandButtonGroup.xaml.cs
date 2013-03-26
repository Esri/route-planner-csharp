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
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics;

using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Control that represents a group of buttons. Each button has an associated command that will be executed
    /// when button will be pressed.
    /// </summary>
    internal partial class CommandButtonGroup : UserControl
    {
        public CommandButtonGroup()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes command button group.
        /// </summary>
        /// <param name="commandCategoryName">Commands' category name.</param>
        /// <param name="commandContext">Commands' context. Can be null.</param>
        /// <remarks>Class supports repeated initialization.</remarks>
        public void Initialize(string commandCategoryName, object commandContext)
        {
            CommandCategoryName = commandCategoryName;
            CommandContext = commandContext;
            _CreateCommandButtons();
        }

        /// <summary>
        /// Command category that contains commands to show in the button group.
        /// </summary>
        public string CommandCategoryName
        {
            get 
            { 
                return _commandCategoryName; 
            }
            // ToDo CR152906  private 
            set 
            { 
                _commandCategoryName = value;
            }
        }

        /// <summary>
        /// Commands' context.
        /// </summary>
        public object CommandContext
        {
            get { return _commandContext; }
            private set { _commandContext = value; }
        }

        /// <summary>
        /// Creates command buttons.
        /// </summary>
        private void _CreateCommandButtons()
        {
            Debug.Assert(null != App.Current.CommandManager);

            _ClearCommandButtons();

            // If command category name is empty - just clear command buttons.
            if (string.IsNullOrEmpty(CommandCategoryName))
                return;

            ICollection<AppCommands.ICommand> commands = App.Current.CommandManager.GetCategoryCommands(CommandCategoryName);
            foreach (AppCommands.ICommand command in commands)
            {
                AppCommands.ICommand cmd = command;

                // If command supports context - special handling.
                if (command is AppCommands.ISupportContext)
                {
                    // Instantiate new intance of such command to initialize it with the context.
                    cmd = (AppCommands.ICommand)Activator.CreateInstance(command.GetType());

                    // Initialize command.
                    cmd.Initialize(App.Current);

                    // Set command context.
                    ((AppCommands.ISupportContext)cmd).Context = CommandContext;
                }

                if (command is AppCommands.ISupportOptions)
                {
                    OptionsCommandButton ocbtn = new OptionsCommandButton();
                    ocbtn.Content = command.Title;
                    ocbtn.ApplicationCommand = cmd;
                    ocbtn.Style = (Style)App.Current.FindResource("CommandOptionsButtonInGroupStyle");
                    ButtonsWrapPanel.Children.Add(ocbtn);
                }
                else
                {
                    CommandButton cbtn = new CommandButton();
                    cbtn.Content = command.Title;
                    cbtn.ApplicationCommand = cmd;
                    cbtn.Style = (Style)App.Current.FindResource("CommandButtonInGroupStyle");
                    ButtonsWrapPanel.Children.Add(cbtn);
                }
            }
        }

        /// <summary>
        /// Removes all the buttons from panel and dispose commands that support context.
        /// </summary>
        private void _ClearCommandButtons()
        {
            // Dispose commands that support context since they were instantiated by the class.
            foreach (Button button in ButtonsWrapPanel.Children)
            {
                Debug.Assert(button != null);

                // Obtain the command from the button.
                AppCommands.ICommand cmd = null;
                if (button is CommandButton)
                    cmd = ((CommandButton)button).ApplicationCommand;
                else if (button is OptionsCommandButton)
                    cmd = ((OptionsCommandButton)button).ApplicationCommand;
                else
                    Debug.Assert(false); // Not support type of button.

                // If command supports context and can be disposed.
                if (cmd is AppCommands.ISupportContext && cmd is IDisposable)
                {
                    // Dispose command.
                    ((IDisposable)cmd).Dispose();
                }
            }

            // Remove buttons from the panel.
            ButtonsWrapPanel.Children.Clear();
        }

        /// <summary>
        /// Commands category.
        /// </summary>
        private string _commandCategoryName;

        /// <summary>
        /// Commands' context.
        /// </summary>
        private object _commandContext;
    }
}
