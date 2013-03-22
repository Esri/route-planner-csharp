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
using System.Diagnostics;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers;
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Services;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Tracking;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command deletes mobile devices
    /// </summary>
    internal sealed class DeleteMobileDevices : DeleteCommandBase<MobileDevice>
    {
        #region Public Fields
        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        #endregion

        #region CommandBase Memebers
        public override bool IsEnabled
        {
            get
            {
                return base.IsEnabled && _CheckSelectedDevices();
            }
        }

        /// <summary>
        /// Initializes command instance.
        /// </summary>
        /// <param name="app">The application object this command belongs to.</param>
        public override void Initialize(App app)
        {
            base.Initialize(app);

            _stateService = new TrackingCommandStateService(app);
            _stateService.StateChanged += _StateServiceStateChanged;
            _IsTrackerEnabled = _stateService.IsEnabled;
        }
        #endregion

        #region DeleteCommandBase Protected Methods
        /// <summary>
        /// Deletes mobile devices
        /// </summary>
        protected override void _Delete(IList<MobileDevice> selectedObjects)
        {
            Debug.Assert(selectedObjects != null);

            var deletionChecker = _Application.Project.DeletionCheckingService;

            var device = deletionChecker.QueryAssignedToDriver(selectedObjects)
                .FirstOrDefault();
            if (device != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_DRIVER_KEY, device);
                _Application.Messenger.AddError(message);

                return;
            }

            device = deletionChecker.QueryAssignedToVehicle(selectedObjects)
                .FirstOrDefault();
            if (device != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_VEHICLE_KEY, device);
                _Application.Messenger.AddError(message);

                return;
            }

            try
            {
                // cast to an array to detach enumerable from selected objects collection.
                var delettees = selectedObjects.ToArray();

                var busyMessage = _Application.FindString(COMMAND_EXECUTING_STATUS_NAME);
                using (WorkingStatusHelper.EnterBusyState(busyMessage))
                {
                    // Delete devices from the project.
                    var project = _Application.Project;
                    foreach (var item in delettees)
                    {
                        project.MobileDevices.Remove(item);
                    }

                    project.Save();

                    // Now we should have less devices locally than on WFMS so they should be
                    // deleted there also.
                    var syncService = _Application.Tracker.SynchronizationService;
                    var deletedTrackingIds = delettees
                        .Where(deletedDevice => deletedDevice.SyncType == SyncType.WMServer)
                        .Select(deletedDevice => deletedDevice.TrackingId);
                    syncService.DeleteDevices(project, deletedTrackingIds);
                }
            }
            catch (Exception e)
            {
                if (!_exceptionHandler.HandleException(e))
                {
                    throw;
                }
            }
        }

        #endregion DeleteCommandBase Protected Methods

        #region DeleteCommandBase Protected Properties

        protected override ISupportDataObjectEditing ParentPage
        {
            get 
            {
                if (_parentPage == null)
                {
                    MobileDevicesPage page = (MobileDevicesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.MobileDevicesPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion DeleteCommandBase Protected Properties

        #region Private Methods

        /// <summary>
        /// If there is feature service devices in selection - we can delete it only
        /// if feature services is enable.
        /// </summary>
        /// <returns>'True' if there is no feature service devices in selection,
        /// otherwise it returns feature service enable status.</returns>
        private bool _CheckSelectedDevices()
        {
            var selection = ((ISupportSelection)ParentPage).SelectedItems;
            foreach (MobileDevice device in selection)
            {
                if (device.SyncType == SyncType.WMServer)
                    return _IsTrackerEnabled;
            }

            return true;
        }

        /// <summary>
        /// Handles command state changes.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments instance.</param>
        private void _StateServiceStateChanged(
            object sender,
            StateChangedEventArgs e)
        {
            _IsTrackerEnabled = e.IsEnabled;
        }
        #endregion

        #region Private Constants
        private const string COMMAND_NAME = "ArcLogistics.Commands.DeleteMobileDevices";

        /// <summary>
        /// Specifies name of the property allowing command enabling and disabling.
        /// </summary>
        private const string ISENABLED_PROPERTY_NAME = "IsEnabled";

        /// <summary>
        /// Name of the status string to be displayed upon command executing.
        /// </summary>
        private const string COMMAND_EXECUTING_STATUS_NAME = "DevicesDeletionStatus";

        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// driver.
        /// </summary>
        private const string ASSIGNED_TO_DRIVER_KEY = "MobileDeviceAssignedToDriver";

        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// vehicle.
        /// </summary>
        private const string ASSIGNED_TO_VEHICLE_KEY = "MobileDeviceAssignedToVehicle";
        #endregion

        #region Private Properties
        private bool _IsTrackerEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    _NotifyPropertyChanged(ISENABLED_PROPERTY_NAME);
                }
            }
        }
        #endregion

        #region Private Fields

        private ISupportDataObjectEditing _parentPage;

        /// <summary>
        /// Instance of the tracking command state service.
        /// </summary>
        private TrackingCommandStateService _stateService;

        /// <summary>
        /// Indicates whether the command is enabled.
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// The exception handler for mobile devices deletion operation.
        /// </summary>
        private IExceptionHandler _exceptionHandler = new TrackingServiceExceptionHandler();
        #endregion Private Members
    }
}
