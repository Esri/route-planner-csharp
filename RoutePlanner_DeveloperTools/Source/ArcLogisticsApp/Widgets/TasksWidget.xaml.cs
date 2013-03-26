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
using System.Windows.Data;
using System.Windows.Media;
using System.Collections.Generic;
using System.Diagnostics;

using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Commands;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// Tasks widget.
    /// </summary>
    internal partial class TasksWidget : PageWidgetBase
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public TasksWidget()
        {
            InitializeComponent();
        }

        #endregion // Constructors

        #region Override properties & methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override void Initialize(Page page)
        {
            base.Initialize(page);

            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);
            this.Loaded += new RoutedEventHandler(TasksWidget_Loaded);
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("TasksWidgetCaption"); }
        }

        #endregion // Public properties

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void App_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitTasks();
        }

        private void TasksWidget_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_widgetInitialized)
                _InitTasks();
        }

        private void _InitTasks()
        {
            if (App.Current.CommandManager != null)
            {
                TasksStack.Children.Clear();

                string categoryName = ((PageBase)this._Page).PageCommandsCategoryName;
                Debug.Assert(!String.IsNullOrEmpty(categoryName));

                ICollection<AppCommands.ICommand> commands = App.Current.CommandManager.GetCategoryCommands(categoryName);
                if (0 < commands.Count)
                {
                    foreach (ESRI.ArcLogistics.App.Commands.ICommand command in commands)
                    {
                        System.Windows.Controls.Button button = null;
                        if (command is ISupportOptions)
                        {
                            OptionsCommandButton ocbtn = new OptionsCommandButton();
                            ocbtn.ApplicationCommand = command;
                            ocbtn.Style = (Style)App.Current.FindResource("CommandOptionsButtonInWidgetStyle");
                            ocbtn.Margin = new Thickness(14, 0, 0, 0);
                            button = ocbtn;
                        }
                        else
                        {
                            CommandButton cbtn = new CommandButton();
                            cbtn.ApplicationCommand = command;
                            button = cbtn;
                        }

                        button.Content = command.Title;
                        TasksStack.Children.Add(button);
                    }
                }

                _widgetInitialized = true;
            }
        }

        /// <summary>
        /// Widget initialize status
        /// </summary>
        private bool _widgetInitialized;

        #endregion // Private methods
    }
}
