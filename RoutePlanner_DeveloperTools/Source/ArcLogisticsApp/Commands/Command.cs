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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Basic class for command. It's inherits from Dependency object for automatic update UI
    /// when any property changes
    /// </summary>
    public abstract class Command : DependencyObject
    {
        #region properties

        public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register("IsEnabled", typeof(bool), typeof(Command));

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Title of the command that can be shown in UI.
        /// </summary>
        public abstract string Title
        {
            get;
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public abstract string TooltipText
        {
            get;
        }

        /// <summary>
        /// Indicates either command is enabled.
        /// </summary>
        public abstract bool IsEnabled
        {
            get;
        }

        #endregion

        #region methods

        /// <summary>
        /// Initalizes command with the application.
        /// </summary>
        /// <param name="app"></param>
        public abstract void Initialize(App app);

        /// <summary>
        /// Executes command. Parameters number depends on specific command.
        /// </summary>
        /// <param name="args"></param>
        public abstract void Execute(params object[] args);

        #endregion
    }
}
