using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using DM = ESRI.ArcLogistics.Tracking.TrackingService.DataModel;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Provides services for synchronizing project state with tracking service data
    /// and vice versa.
    /// </summary>
    internal class SynchronizationService : ISynchronizationService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the SynchronizationService class
        /// with the tracking service instance and the tracking component instance.
        /// </summary>
        /// <param name="trackingService">Instance of the tracking service
        /// to synchronize with.</param>
        public SynchronizationService(ITrackingService trackingService)
        {
            Debug.Assert(trackingService != null);

            _trackingService = trackingService;
        }
        #endregion

        #region ISynchronizationService Members
        /// <summary>
        /// Updates the specified project mobile devices with devices from the
        /// tracking service.
        /// </summary>
        /// <param name="project">Project to be updated.</param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="project"/> is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        public void UpdateFromServer(IProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            var localDevices = _GetTrackingDevices(project);

            // Load devices from server.
            var serverDevices = _trackingService.GetAllMobileDevices()
                .Where(device => !string.IsNullOrEmpty(device.Name))
                .Select(device => new MobileDevice()
                    {
                        Name = device.Name,
                        TrackingId = device.Name,
                        SyncType = SyncType.WMServer,
                    })
                .ToList();

            // Remove local devices absent from the tracking service.
            var knownDevices = new HashSet<string>(
                serverDevices.Select(device => device.TrackingId));
            foreach (var device in localDevices)
            {
                if (knownDevices.Contains(device.TrackingId))
                {
                    continue;
                }

                project.MobileDevices.Remove(device);
            }

            // Make a dictionary mapping device tracking ID to the device object.
            var localDevicesCounter = localDevices
                .GroupBy(device => device.TrackingId)
                .ToDictionary(group => group.Key, group => group.Count());

            // Add server devices missing locally.
            // When there are multiple devices with the same tracking ID we add as much devices
            // as there are on the server if number of local devices does not exceed it.
            foreach (var device in serverDevices)
            {
                var trackingID = device.TrackingId;
                var localDeviceCount = default(int);
                localDevicesCounter.TryGetValue(trackingID, out localDeviceCount);

                if (localDeviceCount > 0)
                {
                    localDevicesCounter[trackingID] = localDeviceCount - 1;

                    continue;
                }

                project.MobileDevices.Add(device);
            }
        }

        /// <summary>
        /// Adds specified devices to the tracking service.
        /// </summary>
        /// <param name="trackingIds">Devices to be added.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        public void AddDevices(IEnumerable<string> trackingIds)
        {
            if (trackingIds == null)
            {
                throw new ArgumentNullException("devices");
            }

            trackingIds = trackingIds.Where(id => !string.IsNullOrEmpty(id));

            var serverIds = _trackingService.GetAllMobileDevices()
                .Select(device => device.Name);
            var serverDevices = new HashSet<string>(serverIds);

            var devicesToAdd = trackingIds
                .Where(id => !serverDevices.Contains(id))
                .Select(id => new DM.Device
                    {
                        Name = id,
                    })
                .ToList();

            if (devicesToAdd.Count == 0)
            {
                return;
            }

            var newIDs = _trackingService.AddMobileDevices(devicesToAdd).ToList();
            if (newIDs.Count != devicesToAdd.Count)
            {
                Logger.Error(Properties.Messages.Error_InvalidTSDevicesIds);
            }
        }

        /// <summary>
        /// Updates specified devices at the tracking service.
        /// </summary>
        /// <param name="devices">Mapping from old tracking ids to new ones.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        public void UpdateTrackingIds(IDictionary<string, string> devices)
        {
            if (devices == null)
            {
                throw new ArgumentNullException("devices");
            }

            var serverDevices = _trackingService.GetAllMobileDevices()
                .ToLookup(device => device.Name);

            var devicesToUpdate =
                (from id in devices.Keys
                 where serverDevices.Contains(id)
                 let serverDevice = serverDevices[id].FirstOrDefault()
                 select new DM.Device
                 {
                    ObjectID = serverDevice.ObjectID,
                    Deleted = DM.DeletionStatus.NotDeleted,
                    Location = serverDevice.Location,
                    Timestamp = serverDevice.Timestamp,
                    Name = devices[id],
                 }).ToList();

            if (devicesToUpdate.Count == 0)
            {
                return;
            }

            _trackingService.UpdateMobileDevices(devicesToUpdate);
        }

        /// <summary>
        /// Deletes specified devices from the tracking service.
        /// </summary>
        /// <param name="project">The project containing devices to be deleted from the server.
        /// </param>
        /// <param name="trackingIds">Devices to be deleted.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> or
        /// <paramref name="project"/> is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the tracking service.</exception>
        public void DeleteDevices(IProject project, IEnumerable<string> trackingIds)
        {
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }

            if (trackingIds == null)
            {
                throw new ArgumentNullException("devices");
            }

            trackingIds = trackingIds.Where(id => !string.IsNullOrEmpty(id));

            // Find devices using their names and delete them from tracking service.
            if (!trackingIds.Any())
            {
                return;
            }

            var allDevices = _trackingService.GetAllMobileDevices()
                .ToLookup(device => device.Name ?? string.Empty);

            // We delete devices from the Tracking Service only when number of local devices
            // is less than number of devices at the Tracking Service. So we select tracking IDs
            // of devices satisfying this constraint.
            var idsToBeDeleted = trackingIds.ToLookup(_ => _)
                .Where(
                    group =>
                    {
                        var id = group.Key;
                        var localCount = project.MobileDevices.Count(
                            device =>
                                device.TrackingId == id &&
                                device.SyncType == SyncType.WMServer);
                        var serverCount = allDevices[id].Count();

                        return localCount < serverCount;
                    })
                .Select(group => group.Key);

            // Now we select devices to be deleted using their tracking IDs.
            var deletedDevices = idsToBeDeleted
                .Select(id => allDevices[id].FirstOrDefault())
                .Where(device => device != null)
                .ToList();

            _trackingService.DeleteMobileDevices(deletedDevices);
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Gets mobile device from the specified project which are used for
        /// workflow management.
        /// </summary>
        /// <param name="project">The project to retrieve devices from.</param>
        /// <returns>A dictionary mapping device tracking ID to the device object.</returns>
        private static IEnumerable<MobileDevice> _GetTrackingDevices(IProject project)
        {
            Debug.Assert(project != null);

            var localDevices = project.MobileDevices
                .Where(device => device.SyncType == SyncType.WMServer &&
                    !string.IsNullOrEmpty(device.TrackingId))
                .ToList();

            return localDevices;
        }
        #endregion

        #region private fields
        /// <summary>
        /// Instance of the tracking service to synchronize with.
        /// </summary>
        private ITrackingService _trackingService;
        #endregion
    }
}
