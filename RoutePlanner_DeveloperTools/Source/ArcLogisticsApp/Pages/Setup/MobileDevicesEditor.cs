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
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Tracking;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Handles mobile devices editing performing necessary synchronizations with tracking
    /// service.
    /// </summary>
    internal sealed class MobileDevicesEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MobileDevicesEditor"/> class.
        /// </summary>
        /// <param name="synchronizationService">The synchronization service to be used
        /// for manipulating devices at the tracking service.</param>
        /// <param name="resourceProvider">The instance of resource provider for obtaining
        /// messages.</param>
        /// <param name="workingStatusController">The instance of the controller to be used
        /// for managing application status.</param>
        /// <param name="exceptionHandler">The exception handler for tracking service
        /// related errors.</param>
        /// <exception cref="ArgumentNullException"><paramref name="synchronizationService"/>,
        /// <paramref name="resourceProvider"/>, <paramref name="workingStatusController"/>,
        /// <paramref name="exceptionHandler"/> or <paramref name="project"/> is a null
        /// reference.</exception>
        public MobileDevicesEditor(
            ISynchronizationService synchronizationService,
            IWorkingStatusController workingStatusController,
            IExceptionHandler exceptionHandler,
            IProject project)
        {
            if (synchronizationService == null)
            {
                throw new ArgumentNullException("synchronizationService");
            }

            if (workingStatusController == null)
            {
                throw new ArgumentNullException("workingStatusController");
            }

            if (exceptionHandler == null)
            {
                throw new ArgumentNullException("exceptionHandler");
            }

            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            _synchronizationService = synchronizationService;
            _workingStatusController = workingStatusController;
            _exceptionHandler = exceptionHandler;
            _project = project;
        }

        /// <summary>
        /// Adds the specified device to the tracking service if device synchronization type
        /// is <see cref="SyncType.WMServer"/>.
        /// </summary>
        /// <param name="device">The reference to the device to be added.</param>
        /// <exception cref="ArgumentNullException"><paramref name="device"/> is a null reference.
        /// </exception>
        internal void AddDevice(MobileDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("devices");
            }

            if (device.SyncType != SyncType.WMServer)
            {
                return;
            }

            var trackingIds = EnumerableEx.Return(device.TrackingId);
            _ExecuteEditingOperation(
                Properties.Resources.DevicesAdditionStatus,
                () => _synchronizationService.AddDevices(trackingIds));
        }

        /// <summary>
        /// Begins editing of the specified device.
        /// </summary>
        /// <param name="device">Begins editing of the specified mobile device.</param>
        /// <exception cref="ArgumentNullException"><paramref name="device"/> is a null reference.
        /// </exception>
        internal void BeginEditing(MobileDevice device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            _editingDevice = device;
            _currentSyncType = device.SyncType;
            _currentTrackingId = device.TrackingId;
        }

        /// <summary>
        /// Finishes editing of the device being edited currently. Should be called only after
        /// the <see cref="BeginEditing"/> method to finish editing.
        /// </summary>
        internal void FinishEditing()
        {
            // Clean up reference to the currently being edited device.
            var editingDevice = _editingDevice;
            _editingDevice = null;

            if (editingDevice == null)
            {
                // No device is being edited now, so nothing do.
                return;
            }

            // Device is not on tracking service, either it was deleted or it was never there.
            if (editingDevice.SyncType != SyncType.WMServer)
            {
                if (_currentSyncType != SyncType.WMServer)
                {
                    // Device was never a tracking one.
                    return;
                }

                // Device was on the tracking service, so we need to delete it.
                var trackingIds = EnumerableEx.Return(_currentTrackingId);
                _ExecuteEditingOperation(
                    Properties.Resources.DevicesDeletionStatus,
                    () => _synchronizationService.DeleteDevices(_project, trackingIds));

                return;
            }

            if (!editingDevice.IsValid)
            {
                // Nothing to do with invalid device.
                return;
            }

            // Device is on the tracking service now, either it was added or updated.
            Debug.Assert(editingDevice.SyncType == SyncType.WMServer);

            if (_currentSyncType == editingDevice.SyncType &&
                _currentTrackingId == editingDevice.TrackingId)
            {
                // No changes in tracking-related properties.
                return;
            }

            if (_currentSyncType != SyncType.WMServer || string.IsNullOrEmpty(_currentTrackingId))
            {
                // A new device was added.
                this.AddDevice(editingDevice);

                return;
            }

            // TrackingId was changed.
            var devices = new Dictionary<string, string>();
            devices[_currentTrackingId] = editingDevice.TrackingId;

            _ExecuteEditingOperation(
                Properties.Resources.DevicesUpdateStatus,
                () => _synchronizationService.UpdateTrackingIds(devices));
        }

        #region private methods
        /// <summary>
        /// Executes the specified editing operation providing services common to all operations.
        /// </summary>
        /// <param name="status">String to be used for status reporting.</param>
        /// <param name="operation">The operation to be executed.</param>
        private void _ExecuteEditingOperation(
            string status,
            Action operation)
        {
            Debug.Assert(status != null);
            Debug.Assert(operation != null);

            try
            {
                using (_workingStatusController.EnterBusyState(status))
                {
                    operation();
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
        #endregion

        #region private fields
        // The synchronization service to be used for manipulating devices at the tracking service.
        private ISynchronizationService _synchronizationService;

        // The instance of the controller to be used for managing application status.
        private IWorkingStatusController _workingStatusController;

        // The exception handler for tracking service related errors.
        private IExceptionHandler _exceptionHandler;

        // Currently being edited device.
        private MobileDevice _editingDevice;

        // The sync type of the device being editing at the beginning of the editing.
        private SyncType? _currentSyncType;

        // The tracking ID of the device being editing at the beginning of the editing.
        private string _currentTrackingId;

        // The reference to the currently opened project.
        private IProject _project;
        #endregion
    }
}
