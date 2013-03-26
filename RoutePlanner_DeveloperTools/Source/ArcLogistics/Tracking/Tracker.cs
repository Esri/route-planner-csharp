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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Utility;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services.Serialization;
using ESRI.ArcLogistics.Tracking.TrackingService.Json;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.CoreEx;
using DM = ESRI.ArcLogistics.Tracking.TrackingService.DataModel;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Tracker class.
    /// </summary>
    public class Tracker : ITracker
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="settings">Settings to be used by the tracker.</param>
        /// <param name="trackingService">The tracking service client to be used to communicate
        /// with the tracking server.</param>
        /// <param name="synchronizationService">The synchronization service to be used for
        /// synchronizing data in the project and at the tracking service.</param>
        /// <param name="solver">The VRP solver to be used by the tracker.</param>
        /// <param name="geocoder">The geocoder to be used by the tracker.</param>
        /// <param name="messageReporter">The message reporter to be used for reporting
        /// tracking errors.</param>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/>,
        /// <paramref name="trackingService"/>, <paramref name="synchronizationService"/>,
        /// <paramref name="solver"/>, <paramref name="geocoder"/> or
        /// <paramref name="messageReporter"/> is a null reference.</exception>
        internal Tracker(
            TrackingSettings settings,
            ITrackingService trackingService,
            ISynchronizationService synchronizationService,
            IVrpSolver solver,
            IGeocoder geocoder,
            IMessageReporter messageReporter)
        {
            CodeContract.RequiresNotNull("settings", settings);
            CodeContract.RequiresNotNull("trackingService", trackingService);
            CodeContract.RequiresNotNull("synchronizationService", synchronizationService);
            CodeContract.RequiresNotNull("solver", solver);
            CodeContract.RequiresNotNull("geocoder", geocoder);
            CodeContract.RequiresNotNull("messageReporter", messageReporter);

            _settings = settings;
            _trackingService = trackingService;
            _synchronizationService = synchronizationService;

            _solver = solver;

            _geocoder = geocoder;
            _messageReporter = messageReporter;
        }
        #endregion constructors

        #region Public readonly property 
        
        /// <summary>
        /// Default max length of string fields on Feature services.
        /// </summary>
        public static int DefaultStringFieldMaxLength
        {
            get
            {
                return DEFAULT_STRING_FIELD_MAX_LENGTH;
            }
        }

        #endregion

        #region public properties

        public Project Project
        {
            get { return _project; }
            set
            {
                if (_project == value)
                {
                    return;
                }

                _project = value;
                _OnProjectChange();
            }
        }

        public Exception InitError
        {
            get { return _initError; }
        }

        /// <summary>
        /// Gets synchronization service instance associated with the tracker.
        /// </summary>
        internal ISynchronizationService SynchronizationService
        {
            get
            {
                return _synchronizationService;
            }
        }

        #endregion

        #region ITracker Members
        /// <summary>
        /// Deploys specified routes to the tracking service.
        /// </summary>
        /// <param name="routes">A collection of routes to be deployed.</param>
        /// <param name="deploymentDate">The date to deploy routes for.</param>
        /// <returns>'True' if any information was sent, 'false' otherwise.</returns>
        public bool Deploy(IEnumerable<Route> routes, DateTime deploymentDate)
        {
            Debug.Assert(routes != null);

            // validate state
            _ValidateTrackerState();

            // get devices associated with schedule
            var devices = _GetScheduleDevices(routes);
            if (devices.Count == 0)
            {
                throw new TrackingException(Properties.Messages.Error_RoutesDeployment);
            }

            var trackingDevices = _DeployDevices(devices);

            var hasRoutesBeenSent = _DeployRoutes(trackingDevices, routes, deploymentDate);

            _project.Save();

            return hasRoutesBeenSent;
        }

        /// <summary>
        /// Check that non from this routes hasnt been sent for secified date.
        /// </summary>
        /// <param name="routes">Routes to check.</param>
        /// <param name="deploymentDate">Date to check.</param>
        /// <returns>'True' if routes hasn't been sent, 
        /// 'false' if any of routes has been sent to feature service.</returns>
        public bool CheckRoutesHasntBeenSent(IEnumerable<Route> routes, DateTime deploymentDate)
        {
            // Get list of tracking devices assigned to routes
            var trackingDevices = _DeployDevices(_GetScheduleDevices(routes));

            // Get existing routes IDs.
            var existingRoutesIDs = _trackingService.GetNotDeletedRoutesIDs(
                trackingDevices.Select(device => device.ServerId), deploymentDate);

            return !existingRoutesIDs.Any();
        }

        /// <summary>
        /// Deploys the specified devices collection to the tracking service.
        /// </summary>
        /// <param name="devices">Devices collection to be deployed.</param>
        public void DeployDevices(IEnumerable<MobileDevice> devices)
        {
            // Get wmserver devices collection and deploy it to feature service.
            var wmServerDevices = devices.Where(device => device.SyncType == SyncType.WMServer);
            _DeployDevices(wmServerDevices);
        }
      
        #endregion public methods

        #region private classes
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// TrackingDevice class.
        /// </summary>
        private class TrackingDevice
        {
            public MobileDevice Device { get; set; }
            public long ServerId { get; set; }
            public string TrackingId { get; set; }
        }

        /// <summary>
        /// EncodedStringWriter class.
        /// </summary>
        private class EncodedStringWriter : StringWriter
        {
            public EncodedStringWriter(StringBuilder sb, Encoding encoding)
                : base(sb)
            {
                _encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return _encoding; }
            }

            private Encoding _encoding;
        }

        #endregion private classes

        /// <summary>
        /// Called upon changing current project reference.
        /// </summary>
        private void _OnProjectChange()
        {
            if (_project == null)
            {
                return;
            }

            try
            {
                _synchronizationService.UpdateFromServer(_project);
                _initError = null;
            }
            catch (Exception e)
            {
                _initError = e;
            }
        }

        private static TrackingDevice _FindDevice(
            IEnumerable<TrackingDevice> devices,
            MobileDevice device)
        {
            foreach (TrackingDevice td in devices)
            {
                if (td.Device.Equals(device))
                    return td;
            }

            return null;
        }

        private static TrackingDevice _FindDeviceByTrackingId(
            IEnumerable<TrackingDevice> devices,
            string trackingId)
        {
            foreach (TrackingDevice td in devices)
            {
                if (td.TrackingId == trackingId)
                    return td;
            }

            return null;
        }

        private static List<MobileDevice> _GetScheduleDevices(IEnumerable<Route> routes)
        {
            Debug.Assert(routes != null);

            return routes
                .Select(TrackingHelper.GetDeviceByRoute)
                .Where(device =>
                    device != null &&
                    device.SyncType == SyncType.WMServer &&
                    !string.IsNullOrEmpty(device.TrackingId))
                .ToList();
        }

        private static string[] _GetTrackingIds(IList<TrackingDevice> devices)
        {
            List<string> ids = new List<string>();
            foreach (TrackingDevice td in devices)
                ids.Add(td.TrackingId);

            return ids.ToArray();
        }

        private static DM.Device _FindDevice(DM.Device[] devices, string trackingId)
        {
            Debug.Assert(devices != null);
            Debug.Assert(trackingId != null);

            foreach (DM.Device device in devices)
            {
                if (device != null &&
                    !String.IsNullOrEmpty(device.Name) &&
                    device.Name == trackingId)
                {
                    return device;
                }
            }

            return null;
        }

        private static string _CompressString(string str)
        {
            Debug.Assert(str != null);

            string result = null;

            byte[] buf = Encoding.UTF8.GetBytes(str);
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(ms,
                    CompressionMode.Compress, true))
                {
                    zipStream.Write(buf, 0, buf.Length);
                }
                result = Convert.ToBase64String(ms.ToArray());
            }

            return result;
        }

        private static void _ValidateSettings(TrackingInfo settings)
        {
            if (settings == null ||
                settings.TrackingServiceInfo == null ||
                string.IsNullOrEmpty(settings.TrackingServiceInfo.RestUrl) ||
                settings.TrackingSettings == null)
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidTrackingConfig);
            }
        }

        /// <summary>
        /// Fills time window properties for the specified tracking stop using data from
        /// the specified stop.
        /// </summary>
        /// <param name="stop">The reference to the stop object to read time window properties
        /// from.</param>
        /// <param name="plannedDate">The date/time when time window should be applied.</param>
        /// <param name="trackingStop">The reference to the tracking stop object to
        /// fill time window properties for.</param>
        private static void _FillTimeWindows(
            Stop stop,
            DateTime plannedDate,
            DM.Stop trackingStop)
        {
            Debug.Assert(stop != null);

            switch (stop.StopType)
            {
                case StopType.Order:
                    {
                        // Use time windows from Order associated with the stop.
                        var order = (Order)stop.AssociatedObject;
                        var timeWindow = order.TimeWindow.ToDateTime(plannedDate);
                        trackingStop.TimeWindowStart1 = timeWindow.Item1.ToUniversalTime();
                        trackingStop.TimeWindowEnd1 = timeWindow.Item2.ToUniversalTime();

                        timeWindow = order.TimeWindow2.ToDateTime(plannedDate);
                        trackingStop.TimeWindowStart2 = timeWindow.Item1.ToUniversalTime();
                        trackingStop.TimeWindowEnd2 = timeWindow.Item2.ToUniversalTime();
                    }

                    break;

                case StopType.Location:
                    {
                        // Use time windows from Location associated with the stop.
                        var location = (Location)stop.AssociatedObject;
                        var timeWindow = location.TimeWindow.ToDateTime(plannedDate);
                        trackingStop.TimeWindowStart1 = timeWindow.Item1.ToUniversalTime();
                        trackingStop.TimeWindowEnd1 = timeWindow.Item2.ToUniversalTime();
                    }

                    break;

                case StopType.Lunch:
                    {
                        // Use time windows from Break associated with the stop's Route.
                        var lunch = stop.Route.Breaks
                            .OfType<TimeWindowBreak>()
                            .Where(item => item.Duration > 0.0)
                            .FirstOrDefault();
                        if (lunch != null)
                        {
                            // TODO: implement good breaks support.
                            trackingStop.TimeWindowStart1 = null;
                            trackingStop.TimeWindowEnd1 = null;
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Converts the specified attributes dictionary into a collection of name/value pairs.
        /// </summary>
        /// <param name="properties">The reference to the attributes dictionary object to be
        /// converted.</param>
        /// <returns>A reference to the collection of name/value pairs for all dictionary
        /// items.</returns>
        private static IEnumerable<NameValuePair> _ToCollection(AttrDictionary properties)
        {
            Debug.Assert(properties != null);

            return properties
                .Select(item => new NameValuePair
                {
                    Name = item.Key,
                    Value = item.Value.ToString(),
                })
                .ToArray();
        }

        #region private methods
        private void _ValidateTrackerState()
        {
            if (_project == null)
                throw new InvalidOperationException(Properties.Messages.Error_ProjectIsNotSet);
        }

        /// <summary>
        /// Loads devices with the specified tracking IDs.
        /// </summary>
        /// <param name="trackingIDs">The reference to collection of tracking IDs.</param>
        /// <returns>A reference to the collection of devices with the specified tracking IDs
        /// received from the tracking service.</returns>
        private IEnumerable<DM.Device> _LoadDevices(
            IEnumerable<string> trackingIDs)
        {
            Debug.Assert(trackingIDs != null);
            Debug.Assert(trackingIDs.All(id => !string.IsNullOrEmpty(id)));

            var allDevices = _trackingService.GetAllMobileDevices()
                .ToLookup(device => device.Name);

            var result = trackingIDs
                .SelectMany(trackingID => allDevices[trackingID])
                .ToArray();

            return result;
        }
        
        /// <summary>
        /// Deploys the specified devices collection to the tracking service.
        /// </summary>
        /// <param name="devices">The reference to the devices collection to be deployed.</param>
        /// <returns>A reference to the collection of <see cref="TrackingDevice"/> objects
        /// containing information about deployed devices.</returns>
        private IEnumerable<TrackingDevice> _DeployDevices(IEnumerable<MobileDevice> devices)
        {
            Debug.Assert(devices != null);
            Debug.Assert(devices.All(device => device != null));

            var trackingIds = devices.Select(device => device.TrackingId);
            var tsDevices = _LoadDevices(trackingIds)
                .ToLookup(device => device.Name);

            var trackingDevices = devices
                .Select(device =>
                    new TrackingDevice
                    {
                        Device = device,
                        TrackingId = device.TrackingId,
                        ServerId = 0,
                    })
                .ToArray();

            var devicesToDeploy = new List<DM.Device>();
            foreach (var device in trackingDevices)
            {
                var tsDevice = tsDevices[device.TrackingId].SingleOrDefault();
                if (tsDevice == null)
                {
                    tsDevice = new DM.Device();
                    tsDevice.Name = device.TrackingId;
                    devicesToDeploy.Add(tsDevice);
                }
                else
                    device.ServerId = tsDevice.ObjectID;
            }

            if (devicesToDeploy.Count > 0)
            {
                var serverIDs = _trackingService.AddMobileDevices(devicesToDeploy).ToList();
                if (serverIDs.Count != devicesToDeploy.Count)
                    throw new TrackingException(Properties.Messages.Error_InvalidTSVehicleIds);

                for (int i = 0; i < devicesToDeploy.Count; ++i)
                {
                    var tsDevice = devicesToDeploy[i];
                    var td = _FindDeviceByTrackingId(trackingDevices, tsDevice.Name);
                    Debug.Assert(td != null);

                    td.ServerId = serverIDs[i];
                }
            }

            return trackingDevices;
        }

        /// <returns>'True' if any information was sent, 'false' otherwise.</returns>
        private bool _DeployRoutes(
            IEnumerable<TrackingDevice> devices,
            IEnumerable<Route> routes,
            DateTime plannedDate)
        {
            Debug.Assert(devices != null);
            Debug.Assert(routes != null);

            plannedDate = plannedDate.Date;

            var newStops = new List<DM.Stop>();
            var updatedStops = new List<DM.Stop>();

            var deployedRoutes = new List<Route>();
            var deployedStops = new List<Stop>();

            // Get all non deleted stops for current routes for planned date.
            var existingStops = 
                _trackingService.GetNotDeletedStops(devices.Select(x => x.ServerId), plannedDate);

            foreach (var route in routes)
            {
                // check if route has associated device
                MobileDevice device = TrackingHelper.GetDeviceByRoute(route);
                if (device == null)
                {
                    continue;
                }

                // check if device belongs to devices with tracking id
                TrackingDevice td = _FindDevice(devices, device);
                if (td == null)
                {
                    continue;
                }

                // Get all stops for current route.
                var currentDateStops = existingStops.Where(stop => stop.DeviceID == td.ServerId);

                // Get id's of non deleted stops.
                var stopsToDelete = currentDateStops.
                    Where(stop => stop.Deleted == DM.DeletionStatus.NotDeleted).
                    ToDictionary(stop => stop.ObjectID);

                // Get version number for sended stops.
                var version = _GetNewVersion(currentDateStops);

                // Prepare stops to be deployed.
                var sortedStops = CommonHelpers.GetSortedStops(route);
                var trackingStops = _CreateTrackingStops(
                    td.ServerId,
                    version,
                    plannedDate,
                    sortedStops);
                trackingStops = trackingStops.ToList();

                // Add stop to either new stops or updated stops collection.
                foreach (var item in trackingStops.ToIndexed())
                {
                    var trackingStop = item.Value;
                    var existingStop = stopsToDelete.Remove(trackingStop.ObjectID);
                    var stopsToDeploy = existingStop ? updatedStops : newStops;
                    stopsToDeploy.Add(trackingStop);

                    if (!existingStop)
                    {
                        deployedStops.Add(sortedStops[item.Index]);
                    }
                }

                // Deletes from tracking service stops which were deleted locally.
                // When stop is moved to other route we treat it as a deletion from an old route
                // and adding to the new one.
                foreach (var stopToDelete in stopsToDelete.Values)
                {
                    stopToDelete.Deleted = DM.DeletionStatus.Deleted;
                    updatedStops.Add(stopToDelete);
                }

                // We need a list of both new and updated stops in order to apply common properties
                // like arrival delays.
                var allStops = new List<DM.Stop>(trackingStops);

                TrackingHelper.ApplyArrivalDelayToStops(
                    _solver.SolverSettings.ArriveDepartDelay, allStops);

                deployedRoutes.Add(route);
            }

            // We must sent route settings, barriers only if we have stops to update.
            if (newStops.Count > 0 || updatedStops.Count > 0)
            {
                // Update route settings.
                var routeSettings = _SerializeRouteSettings();
                _trackingService.UpdateRouteSettings(plannedDate, routeSettings);
                
                _UpdateRouteTable(routes, devices, plannedDate);
                
                // Update barriers for planned date.
                _UpdateBarriers(plannedDate);

                _trackingService.UpdateStops(newStops, updatedStops);

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Add route info to tracking service.
        /// </summary>
        /// <param name="routes">Current routes.</param>
        /// <param name="devices">Collection of all mobile devices.</param>
        /// <param name="plannedDate">Routes planned date.</param>
        private void _UpdateRouteTable(IEnumerable<Route> routes, 
            IEnumerable<TrackingDevice> devices, DateTime plannedDate)
        {
            var routesToDeploy = new List<DM.Route>();

            foreach (var route in routes)
            {
                // Find tracking device for route.
                var device = _GetTrackingDevice(devices, route);

                routesToDeploy.Add(new DM.Route(route, device.ServerId, plannedDate));
            }

            // Get routes which are non deleted on feature service.
            var devicesIDs = devices.Select(x => x.ServerId);
            var routesToDelete = new List<DM.Route>(
                _trackingService.GetNotDeletedRoutes(devicesIDs, plannedDate));

            _trackingService.UpdateRoutes(routesToDeploy, routesToDelete);
        }

        /// <summary>
        /// Get tracking device for route.
        /// </summary>
        /// <param name="devices">List with all tracking devices.</param>
        /// <param name="route">Route which device should be found.</param>
        /// <returns>Route tracking device or null if route has no device.</returns>
        private TrackingDevice _GetTrackingDevice(IEnumerable<TrackingDevice> devices, Route route)
        {
            // Check if route has associated device.
            MobileDevice device = TrackingHelper.GetDeviceByRoute(route);
            if (device == null)
                return null;

            // Return device.
            return _FindDevice(devices, device);
        }

        /// <summary>
        /// Get new version which will be used for stops.
        /// </summary>
        /// <param name="deviceStops">Current device stops.</param>
        /// <returns>New version, which should be used for sending orders.</returns>
        private int _GetNewVersion(IEnumerable<DM.Stop> deviceStops)
        {
            // If device have stops - find stop with max order, 
            // increase it by 1 and return it as new version.
            if (deviceStops.Count() != 0)
                return deviceStops.Max((stop) => { return stop.Version; }) + 1;
            // If device have no stops - return 0 as version.
            else
                return 0;
        }
        
        /// <summary>
        /// Update barriers for planned date.
        /// </summary>
        /// <param name="plannedDate">Date for which barriers should be updated.</param>
        private void _UpdateBarriers(DateTime plannedDate)
        {
            var allBarriers = _project.Barriers.Search(plannedDate);
            var pointBarriers = allBarriers
                .Where(barrier => barrier.Geometry is Point)
                .Select(barrier => new DM.PointBarrier
                {
                    Name = barrier.Name,
                    Location = (Point)barrier.Geometry,
                    Type = barrier.BarrierEffect.BlockTravel ?
                        DM.PointBarrierType.BlockTravel : DM.PointBarrierType.AddDelay,
                    DelayValue = barrier.BarrierEffect.DelayTime,
                })
                .ToList();

            var lineBarriers = allBarriers
                .Where(barrier => barrier.Geometry is Polyline)
                .Select(barrier => new DM.LineBarrier
                {
                    Name = barrier.Name,
                    Location = (Polyline)barrier.Geometry,
                })
                .ToList();

            var polygonBarriers = allBarriers
                .Where(barrier => barrier.Geometry is Polygon)
                .Select(barrier => new DM.PolygonBarrier
                {
                    Name = barrier.Name,
                    Location = (Polygon)barrier.Geometry,
                    Type = barrier.BarrierEffect.BlockTravel ?
                        DM.PolygonBarrierType.BlockTravel : DM.PolygonBarrierType.Slowdown,
                    SlowdownValue = 100 / (100 + barrier.BarrierEffect.SpeedFactorInPercent)
                })
                .ToList();

            _trackingService.UpdateBarriers(
                plannedDate,
                pointBarriers,
                lineBarriers,
                polygonBarriers);
        }

        /// <summary>
        /// Creates tracking stops for the specified stops collection.
        /// </summary>
        /// <param name="deviceId">The object ID of the device to create stops for.</param>
        /// <param name="currentVersion">The version of stops to be created.</param>
        /// <param name="plannedDate">The date/time to create stops for.</param>
        /// <param name="routeStops">A collection of route stops sorted by their sequence
        /// number to create tracking stops for.</param>
        /// <returns>A collection of tracking stops corresponding to the specified route
        /// stops.</returns>
        private IEnumerable<DM.Stop> _CreateTrackingStops(
            long deviceId,
            int currentVersion,
            DateTime plannedDate,
            IList<Stop> routeStops)
        {
            Debug.Assert(routeStops != null);

            var propertyFilter = Functional.MakeLambda((string _) => true);
            var stopInfos = RouteExporter.ExportStops(
                routeStops,
                _project.CapacitiesInfo,
                _project.OrderCustomPropertiesInfo,
                _geocoder.AddressFields,
                _solver,
                propertyFilter).ToList();

            var trackingStops = routeStops
                .Select((stop, index) =>
                {
                    var stopInfo = stopInfos[index];
                    var objectId = 0;

                    var trackingStop = new DM.Stop
                    {
                        ObjectID = objectId,
                        Version = currentVersion,
                        // Name of the stop can exceed 50 chars, so we need to trim excess chars.
                        Name = _TrimStringField(stopInfo.Name),
                        Location = stopInfo.Location,
                        OrderType = stopInfo.OrderType,
                        Priority = stopInfo.Priority,
                        CurbApproach = stopInfo.CurbApproach,
                        Address = _ToCollection(stopInfo.Address),
                        Capacities = _ToCollection(stopInfo.Capacities),
                        CustomOrderProperties = _ToCollection(stopInfo.CustomOrderProperties),
                        Type = _GetStopType(stop, routeStops),
                        PlannedDate = plannedDate,
                        DeviceID = deviceId,
                        SequenceNumber = stop.SequenceNumber,
                        ServiceTime = (int)stop.TimeAtStop,
                        MaxViolationTime = stopInfo.MaxViolationTime,
                        ArriveTime = stopInfo.ArriveTime.ToUniversalTime(),
                    };

                    _FillTimeWindows(stop, plannedDate, trackingStop);

                    return trackingStop;
                });

            return trackingStops;
        }

        /// <summary>
        /// If name contain more then 50 chars - return first 50 chars, otherwise return full name.
        /// </summary>
        /// <param name="name">Name to trim.</param>
        /// <returns>Trimed name.</returns>
        private static string _TrimStringField(string name)
        {
            if (name.Length > DEFAULT_STRING_FIELD_MAX_LENGTH)
                return name.Substring(0, DEFAULT_STRING_FIELD_MAX_LENGTH);
            else
                return name;
        }

        /// <summary>
        /// Gets tracking server stop type for the specified stop.
        /// </summary>
        /// <param name="stop">The reference to the stop object to get
        /// tracking server stop type for.</param>
        /// <param name="sortedStops">The reference to the list of route stops
        /// sorted by their sequence number.</param>
        /// <returns>Tracking server stop type.</returns>
        private DM.StopType _GetStopType(Stop stop, IList<Stop> sortedStops)
        {
            Debug.Assert(stop != null);
            Debug.Assert(sortedStops != null);

            switch (stop.StopType)
            {
                case StopType.Order:
                    return DM.StopType.Order;

                case StopType.Lunch:
                    return DM.StopType.Break;

                case StopType.Location:
                    if (stop == sortedStops.First())
                    {
                        return DM.StopType.StartLocation;
                    }
                    else if (stop == sortedStops.Last())
                    {
                        return DM.StopType.FinishLocation;
                    }
                    else
                    {
                        return DM.StopType.RenewalLocation;
                    }

                default:
                    Debug.Assert(false);
                    return DM.StopType.Order;
            }
        }

        /// <summary>
        /// Serializes route settings for the specified date.
        /// </summary>
        private string _SerializeRouteSettings()
        {
            var solverSettings = _solver.SolverSettings;

            var restrictions = solverSettings.Restrictions
                .Select(restriction => new RestrictionAttributeInfo
                    {
                        Name = restriction.NetworkAttributeName,
                        IsEnabled = restriction.IsEnabled,
                    })
                .ToArray();

            var value = default(object);
            var attributes =
                (from attribute in _solver.NetworkDescription.NetworkAttributes
                from parameter in attribute.Parameters
                let hasValue = solverSettings.GetNetworkAttributeParameterValue(
                    attribute.Name,
                    parameter.Name,
                    out value)
                where hasValue && value != null
                group new ParameterInfo(
                    parameter.RoutingName,
                    value.ToString(),
                    // Check that parameter is restriction usage parameter.
                    // In case if attribute has restriction usage parameter name check that it is
                    // equal to current parameter name, otherwise, false.
                    attribute.RestrictionUsageParameter != null ?
                    parameter.Name == attribute.RestrictionUsageParameter.Name : false)
                by attribute.RoutingName into info
                select new AttributeInfo
                {
                    Name = info.Key,
                    Parameters = info.ToArray(),
                }).ToArray();

            var settings = new RouteSettings
            {
                UTurnPolicy = solverSettings.GetUTurnPolicy(),
                ImpedanceAttribute = _solver.NetworkDescription.ImpedanceAttributeName,
                DirectionsLengthUnits = RequestBuildingHelper.GetDirectionsLengthUnits(),
                Restrictions = restrictions,
                Attributes = attributes,
                BreakTolerance = _settings.BreakTolerance
            };

            return JsonSerializeHelper.Serialize(settings);
        }


        #endregion private methods

        #region Private constants

        /// <summary>
        /// Default max length of string fields on Feature services.
        /// </summary>
        private const int DEFAULT_STRING_FIELD_MAX_LENGTH = 50;

        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private TrackingSettings _settings;
        private Project _project;

        private IVrpSolver _solver;

        private IGeocoder _geocoder;

        private ITrackingService _trackingService;

        /// <summary>
        /// Dictionary for mappings tracked routes to list of their stops sorted
        /// by sequence number.
        /// </summary>
        private Dictionary<Route, IList<Stop>> _routesSortedStops =
            new Dictionary<Route, IList<Stop>>();

        /// <summary>
        /// List of currently tracked routes.
        /// </summary>
        private List<Route> _trackedRoutes = new List<Route>();

        private Exception _initError;

        /// <summary>
        /// Synchronization service object to be used for synchronizing local
        /// state with tracking service.
        /// </summary>
        ISynchronizationService _synchronizationService;

        /// <summary>
        /// A message reporter object to be used for reporting tracking errors.
        /// </summary>
        private IMessageReporter _messageReporter;

        /// <summary>
        /// Maps tracking ids of invalid stops into corresponding stop.
        /// </summary>
        private Dictionary<long, Stop> _invalidStops;

        #endregion private fields
    }
}
