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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers;
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Utility.ComponentModel;
using ESRI.ArcLogistics.Utility.Reflection;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Provides routes sending to the tracking server.
    /// </summary>
    internal sealed class SendRoutesCommand :
        NotifyPropertyChangedBase,
        ICommand,
        INotifyPropertyChanged
    {
        #region ICommand Members
        /// <summary>
        /// Gets name of the command.
        /// </summary>
        public string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        /// <summary>
        /// Gets command title.
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
            private set
            {
                if (_title != value)
                {
                    _title = value;
                    this.NotifyPropertyChanged(TITLE_PROPERTY_NAME);
                }
            }
        }

        /// <summary>
        /// Gets or sets command tooltip.
        /// </summary>
        [PropertyDependsOn(ISENABLED_PROPERTY_NAME)]
        public string TooltipText
        {
            get
            {
                return this.IsEnabled ?
                    _application.FindString(ENABLED_TOOLTIP) :
                    _application.FindString(DISABLED_TOOLTIP);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the command is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            private set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    this.NotifyPropertyChanged(ISENABLED_PROPERTY_NAME);
                }
            }
        }

        /// <summary>
        /// Gets key combination to invoke the command.
        /// </summary>
        public KeyGesture KeyGesture
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes command instance.
        /// </summary>
        /// <param name="app">The application object this command belongs to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="app"/> is a null
        /// reference.</exception>
        public void Initialize(App app)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }

            _application = app;
            _application.ProjectLoaded += _ApplicationProjectLoaded;
            _application.ProjectClosing += _ApplicationProjectClosing;

            _sendRoutesTask = app.RoutesWorkflowManager.SendRoutesTask;
            _sendRoutesTask.StateChanged += _StateServiceStateChanged;

            _HandleProjectChange();
        }

        /// <summary>
        /// Executes command.
        /// </summary>
        /// <param name="args">Arguments passed to the command.</param>
        public void Execute(params object[] args)
        {
            Debug.Assert(this.IsEnabled);

            try
            {
                using (WorkingStatusHelper.EnterBusyState(_executionStatusMessage))
                {
                    if (_RoutesMustBeSent())
                    {
                        _sendRoutesTask.Execute();
                        _UpdateState();
                    }
                }
            }
            catch (Exception e)
            {
                var handler = _application.WorkflowManagementExceptionHandler;
                if (!handler.HandleException(e))
                {
                    throw;
                }
            }
        }
        #endregion

        #region private methods

        /// <summary>
        /// Check that routes should be sent to feature services.
        /// </summary>
        /// <returns>'True' if routes must be send, 'false' otherwise.</returns>
        private bool _RoutesMustBeSent()
        {
            var date = (DateTime)_sendRoutesTask.GetDeploymentDate();

            // If we haven't sent routes before - we could sent it now without asking user.
            if(App.Current.Tracker.CheckRoutesHasntBeenSent(
                _sendRoutesTask.QueryRoutesToBeSent(), (DateTime)date))
                return true;

            // Ask user that he want to sent new routes to feature server and overwrite existing ones.
            MessageBoxExButtonType pressedButton = MessageBoxEx.Show(App.Current.MainWindow,
                Properties.Resources.OverwriteFeatureServiceRoutesMessageBoxText,
                Properties.Resources.OverwriteFeatureServiceRoutesMessageBoxCaption, 
                System.Windows.Forms.MessageBoxButtons.YesNo, MessageBoxImage.Question);

            // Return his answer.
            return (pressedButton == MessageBoxExButtonType.Yes);
        }

        /// <summary>
        /// Handles command state changes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments instance.</param>
        private void _StateServiceStateChanged(object sender, StateChangedEventArgs e)
        {
            _UpdateState();
        }

        /// <summary>
        /// Updates command state upon changes in the application project and/or current schedule.
        /// </summary>
        private void _UpdateState()
        {
            this.IsEnabled = _sendRoutesTask.IsEnabled;

            this.Title = _application.FindString(DEFAULT_COMMAND_TITLE);
            _executionStatusMessage = _application.FindString(COMMAND_EXECUTING_STATUS_NAME);

            var project = _application.Project;
            if (project == null)
            {
                return;
            }

            var schedule = project.Schedules.ActiveSchedule;
            if (schedule == null)
            {
                return;
            }
        }

        /// <summary>
        /// Handles application project closing.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event data object.</param>
        private void _ApplicationProjectClosing(object sender, EventArgs e)
        {
            var project = _application.Project;
            ACTIVE_SCHEDULE_PROPERTY.RemoveValueChanged(
                project.Schedules,
                _ActiveScheduleChanged);
        }

        /// <summary>
        /// Handles application project loading.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event data object.</param>
        private void _ApplicationProjectLoaded(object sender, EventArgs e)
        {
            _HandleProjectChange();
        }

        /// <summary>
        /// Handles application project changes.
        /// </summary>
        private void _HandleProjectChange()
        {
            var project = _application.Project;
            if (project != null)
            {
                ACTIVE_SCHEDULE_PROPERTY.AddValueChanged(
                    project.Schedules,
                    _ActiveScheduleChanged);
            }

            _UpdateState();
        }

        /// <summary>
        /// Handles active schedule changes.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">Event data object.</param>
        private void _ActiveScheduleChanged(object sender, EventArgs e)
        {
            _UpdateState();
        }
        #endregion

        #region private constants
        /// <summary>
        /// Specifies the command name.
        /// </summary>
        private const string COMMAND_NAME = "ArcLogistics.Commands.SendRoutesCommand";

        /// <summary>
        /// Specifies resource key for the routes sending command title.
        /// </summary>
        private const string DEFAULT_COMMAND_TITLE = "SendRoutesCommandTitle";

        /// <summary>
        /// Specifies resource key for the tooltip for enabled state.
        /// </summary>
        private const string ENABLED_TOOLTIP = "SendRoutesCommandEnabledTooltip";

        /// <summary>
        /// Specifies resource key for the tooltip for disabled state.
        /// </summary>
        private const string DISABLED_TOOLTIP = "SendRoutesCommandDisabledTooltip";

        /// <summary>
        /// Specifies name of the property allowing command enabling and disabling.
        /// </summary>
        private const string ISENABLED_PROPERTY_NAME = "IsEnabled";

        /// <summary>
        /// Name of the status string to be displayed upon command executing.
        /// </summary>
        private const string COMMAND_EXECUTING_STATUS_NAME = "SendRoutesCommandStatus";
        
        /// <summary>
        /// Name of the <see cref="Title"/> property.
        /// </summary>
        private const string TITLE_PROPERTY_NAME = "Title";

        /// <summary>
        /// Descriptor for the <see cref="ScheduleManager.ActiveSchedule"/> property.
        /// </summary>
        private static readonly PropertyDescriptor ACTIVE_SCHEDULE_PROPERTY =
            TypeInfoProvider<ScheduleManager>.GetPropertyDescriptor(
                _ => _.ActiveSchedule);

        /// <summary>
        /// The resource key of the abort routes warning dialog title.
        /// </summary>
        private const string ABORT_ROUTES_WARNING_TITLE_KEY = "AbortRoutesWarningTitle";

        /// <summary>
        /// The resource key of the abort routes warning dialog text.
        /// </summary>
        private const string ABORT_ROUTES_WARNING_TEXT_KEY = "AbortRoutesWarningText";
        #endregion

        #region private fields
        /// <summary>
        /// Stores current value of the command title.
        /// </summary>
        private string _title;

        /// <summary>
        /// Indicates whether the command is enabled.
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// Instance of the routes sending task.
        /// </summary>
        private ISendRoutesTask _sendRoutesTask;

        /// <summary>
        /// The reference to the application owning this command.
        /// </summary>
        private App _application;

        /// <summary>
        /// The message to be displayed upon command execution.
        /// </summary>
        private string _executionStatusMessage;
        #endregion
    }
}
