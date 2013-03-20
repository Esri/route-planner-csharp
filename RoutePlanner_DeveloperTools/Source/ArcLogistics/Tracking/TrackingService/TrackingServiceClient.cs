using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Implements <see cref="ITrackingService"/> interface for ArcGIS Feature Service.
    /// </summary>
    internal sealed class TrackingServiceClient : ITrackingService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the TrackingServiceClient class with the specified
        /// feature service object.
        /// </summary>
        /// <param name="featureService">The reference to the feature service object.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="featureService"/>
        /// argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve necessary layers from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public TrackingServiceClient(IFeatureService featureService)
        {
            if (featureService == null)
            {
                throw new ArgumentNullException("featureService");
            }

            _devicesLayer = featureService.OpenLayer<Device>(DEVICES_LAYER);
            _stopsLayer = featureService.OpenLayer<Stop>(STOPS_LAYER);
            _eventsLayer = featureService.OpenLayer<Event>(EVENTS_LAYER);
            _settingsLayer = featureService.OpenLayer<Setting>(SETTINGS_LAYER);
            _pointBarriersLayer = featureService.OpenLayer<PointBarrier>(POINT_BARRIERS_LAYER);
            _lineBarriersLayer = featureService.OpenLayer<LineBarrier>(LINE_BARRIERS_LAYER);
            _polygonBarriersLayer = featureService.OpenLayer<PolygonBarrier>(
                POLYGON_BARRIERS_LAYER);
            _routesLayer = featureService.OpenLayer<Route>(ROUTES_LAYER);
        }
        #endregion

        #region ITrackingService Members
        /// <summary>
        /// Gets all mobile devices availlable at the service.
        /// </summary>
        /// <returns>Collection of all mobile devices available at the
        /// tracking service.</returns>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve devices from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<Device> GetAllMobileDevices()
        {
            var allDevices = _devicesLayer.QueryData(
                UNFILTERED_QUERY,
                ALL_FIELDS,
                GeometryReturningPolicy.WithoutGeometry).ToList();

            return allDevices
                .Where(device => device.Deleted == DeletionStatus.NotDeleted)
                .ToList();
        }

        /// <summary>
        /// Gets mobile devices with the specified object IDs from the tracking service.
        /// </summary>
        /// <param name="mobileDeviceIDs">Collection of object IDs identifying mobile devices
        /// to be retrieved.</param>
        /// <returns>Collection of mobile devices with the specified object IDs available at the
        /// tracking service.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="mobileDeviceIDs"/>
        /// argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve devices from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<Device> GetMobileDevices(IEnumerable<long> mobileDeviceIDs)
        {
            if (mobileDeviceIDs == null)
            {
                throw new ArgumentNullException("mobileDeviceIDs");
            }

            return _devicesLayer.QueryObjectsByIDs(mobileDeviceIDs);
        }

        /// <summary>
        /// Adds specified mobile devices to the tracking service.
        /// </summary>
        /// <param name="mobileDevices">Collection of mobile devices to be added.</param>
        /// <returns>Collection of object IDs identifying added mobile devices.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="mobileDevices"/>
        /// argument or any of it's elements is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to add devices to the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<long> AddMobileDevices(IEnumerable<Device> mobileDevices)
        {
            if (mobileDevices == null || mobileDevices.Any(device => device == null))
            {
                throw new ArgumentNullException("mobileDevices");
            }

            // Check that any mobile device has too long name.
            if (mobileDevices.Any(device => device.Name.Length > Tracker.DefaultStringFieldMaxLength))
                throw new ArgumentException(Properties.Messages.Error_MobileDeviceNameIsTooLong);

            var newDevices = mobileDevices.ToList();
            _PrepareAddition(newDevices);

            return _devicesLayer.AddObjects(newDevices);
        }

        /// <summary>
        /// Adds specified routes to the tracking service.
        /// </summary>
        /// <param name="routesToAdd">Collection of stops to be added.</param>
        /// <param name="routesToDelete">Collection of stops to be deleted.</param>
        /// <returns>Collection of object IDs identifying added stops.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Arguments or any of it's elements is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to add routes to the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<long> UpdateRoutes(IEnumerable<Route> routesToAdd, 
            IEnumerable<Route> routesToDelete)
        {
            if (routesToAdd == null || routesToAdd.Any(route => route == null))
                throw new ArgumentNullException("routes");
            if (routesToDelete == null || routesToDelete.Any(route => route == null))
                throw new ArgumentNullException("updatedRoutes");

            // Mark all routes as deleted and update them on Feature Service.
            routesToDelete.Do((x) => { x.Deleted = DeletionStatus.Deleted; });
            _routesLayer.ApplyEdits(Enumerable.Empty<Route>(), routesToDelete, Enumerable.Empty<long>());

            var newRoutes = routesToAdd.ToList();
            _PrepareAddition(newRoutes);

            // List with IDs of routes which was added to FS.
            var addedRoutes = new List<long>();

            var list = new List<Route>(routesToAdd);
            int index = 0;

            // Split added routes to chunks and send each chunk to Feature Service in separate 
            // request. Routes in one chunk must contain no more then 50.000 points to prevent too
            // big requst to FS.
            while (index < list.Count())
            {
                var chunkToSend = new List<Route>();

                // Number of points in routes which are in chunk.
                var chunkPointsCount = 0;

                do
                {
                    chunkPointsCount = _PointsCount(list[index], chunkPointsCount);
                    chunkToSend.Add(list[index]);
                    index++;
                }
                // Check that we are in bounds of array and that when we add next route
                // to chunk total number of points will be less then 50k.
                while (index < list.Count && 
                    _PointsCount(list[index], chunkPointsCount) < MAXIMUM_NUMBER_OF_POINTS_IN_CHUNK);

                // Send chunk to FS.
                addedRoutes.AddRange(_routesLayer.ApplyEdits(chunkToSend,
                    Enumerable.Empty<Route>(), Enumerable.Empty<long>()));
            }

            return addedRoutes;
        }

        /// <summary>
        /// Updates specified mobile devices at the tracking service.
        /// </summary>
        /// <param name="mobileDevices">Collection of mobile devices to be updated.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="mobileDevices"/>
        /// argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to update mobile devices at the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public void UpdateMobileDevices(IEnumerable<Device> mobileDevices)
        {
            if (mobileDevices == null || mobileDevices.Any(device => device == null))
            {
                throw new ArgumentNullException("mobileDevices");
            }

            var newDevices = mobileDevices.ToList();
            _PrepareAddition(newDevices);

            _devicesLayer.UpdateObjects(newDevices);
        }

        /// <summary>
        /// Deletes specified mobile devices from the tracking service.
        /// </summary>
        /// <param name="mobileDevices">Collection of mobile devices to be deleted.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="mobileDeviceIDs"/>
        /// argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to delete mobile devices from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public void DeleteMobileDevices(IEnumerable<Device> mobileDevices)
        {
            if (mobileDevices == null)
            {
                throw new ArgumentNullException("mobileDevices");
            }

            // Remove duplicated devices.
            var distinctDevices = mobileDevices
                .ToLookup(device => device.ObjectID)
                .Select(group => group.First());

            // Preserve existing field values except for Deleted field.
            var deletedDevices = distinctDevices
                .Select(device =>
                new Device
                {
                    ObjectID = device.ObjectID,
                    Deleted = DeletionStatus.Deleted,
                    Location = device.Location,
                    Name = device.Name,
                    Timestamp = device.Timestamp,
                });

            // Perform devices deletion.
            _devicesLayer.UpdateObjects(deletedDevices);
        }

        /// <summary>
        /// Gets list with non deleted stops for the mobile device
        /// with the specified object ID from the tracking service.
        /// </summary>
        /// <param name="mobileDeviceID">The object ID of the mobile device to get stops
        /// for.</param>
        /// <param name="plannedDate">The date/time to retrieve stops for.</param>
        /// <returns>Collection of non deleted stops for the mobile device with the specified
        /// object ID.</returns>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve stops from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<Stop> GetNonDeletedStops(long mobileDeviceID, DateTime plannedDate)
        {
            return GetAllStops(mobileDeviceID, plannedDate)
                .Where(stop => stop.Deleted == DeletionStatus.NotDeleted)
                .ToList();
        }

        /// <summary>
        /// Gets list with all(either deleted or non deleted) stops for the mobile device
        /// with the specified object ID from the tracking service.
        /// </summary>
        /// <param name="mobileDeviceID">The object ID of the mobile device to get stops
        /// for.</param>
        /// <param name="plannedDate">The date/time to retrieve stops for.</param>
        /// <returns>Collection of stops for the mobile device with the specified 
        /// object ID.</returns>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve stops from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<Stop> GetAllStops(long mobileDeviceID, DateTime plannedDate)
        {
            var whereClause = string.Format(
                QUERY_FORMATTER,
                STOPS_BY_DEVICE_QUERY_WHERE_CLAUSE,
                mobileDeviceID,
                GPObjectHelper.DateTimeToGPDateTime(plannedDate));
            var deviceStops = _stopsLayer.QueryData(
                whereClause,
                ALL_FIELDS,
                GeometryReturningPolicy.WithoutGeometry).ToList();

            return deviceStops.ToList();
        }

        /// <summary>
        /// Gets non deleted routes for the mobile devices with the 
        /// specified mobile devices ObjectIDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">Mobile devices IDs, which routes should be found.</param>
        /// <param name="plannedDate">The date/time to retrieve routes for.</param>
        /// <returns>Routes associated with specified mobile devices ids.</returns>
        public IEnumerable<Route> GetNotDeletedRoutes(IEnumerable<long> mobileDevicesIDs, 
            DateTime plannedDate)
        {
            return _GetRoutesByMobileDevices(mobileDevicesIDs, plannedDate, ALL_FIELDS);
        }

        /// <summary>
        /// Gets ObjectIDs of non deleted routes for the mobile devices with the 
        /// specified mobile devices ObjectIDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">Mobile devices IDs, which routes should be found.</param>
        /// <param name="plannedDate">The date/time to retrieve routes for.</param>
        /// <returns>Routes ObjectIDs associated with specified mobile devices ids.</returns>
        public IEnumerable<long> GetNotDeletedRoutesIDs(IEnumerable<long> mobileDevicesIDs, 
            DateTime plannedDate)
        {
            return _GetRoutesByMobileDevices(mobileDevicesIDs, plannedDate, OBJECTID_FIELD).
                Select(route => route.ObjectID);
        }

        /// <summary>
        /// Gets non deleted stops for the mobile devices with the 
        /// specified objects IDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">Mobile devices IDs, which stops should be found.</param>
        /// <param name="plannedDate">The date/time to retrieve stops for.</param>
        /// <returns>Stops associated with specified mobile devices ids.</returns>
        public IEnumerable<Stop> GetNotDeletedStops(IEnumerable<long> mobileDevicesIDs, DateTime plannedDate)
        {
            return _stopsLayer.QueryData(
                _FormatUndeletedObjectsForPlannedDateQuery(mobileDevicesIDs, plannedDate),
                ALL_FIELDS,
                GeometryReturningPolicy.WithoutGeometry).ToList();
        }

        /// <summary>
        /// Adds specified stops to the tracking service.
        /// </summary>
        /// <param name="stops">Collection of stops to be added.</param>
        /// <returns>Collection of object IDs identifying added stops.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="stops"/>
        /// argument or any of it's elements is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to add stops to the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<long> AddStops(IEnumerable<Stop> stops)
        {
            if (stops == null || stops.Any(stop => stop == null))
            {
                throw new ArgumentNullException("stops");
            }

            var newStops = stops.ToList();
            _PrepareAddition(newStops);

            return _stopsLayer.AddObjects(newStops);
        }

        /// <summary>
        /// Adds and updates specified stops at the tracking service.
        /// </summary>
        /// <param name="newStops">Collection of stops to be added.</param>
        /// <param name="updatedStops">Collection of stops to be updated.</param>
        /// <returns>Collection of object IDs identifying added stops.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="newStops"/> or
        /// <paramref name="updatedStops"/> any of their elements is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to add and update stops at the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<long> UpdateStops(
            IEnumerable<Stop> newStops,
            IEnumerable<Stop> updatedStops)
        {
            if (newStops == null || newStops.Any(stop => stop == null))
            {
                throw new ArgumentNullException("newStops");
            }

            if (updatedStops == null || updatedStops.Any(stop => stop == null))
            {
                throw new ArgumentNullException("updatedStops");
            }

            // Split edited stops to chunks and send each 
            // chunk to Feature Service in separate request.
            foreach (var chunk in updatedStops.SplitToChunks(MAX_NUMBER_OF_STOPS_IN_ONE_CHUNK))
                _stopsLayer.ApplyEdits(Enumerable.Empty<Stop>(), chunk, Enumerable.Empty<long>());

            var addedStops = newStops.ToList();
            _PrepareAddition(addedStops);

            // Split added stops to chunks and send each 
            // chunk to Feature Service in separate request.
            var addedIDs = new List<long>();
            foreach (var chunk in addedStops.SplitToChunks(MAX_NUMBER_OF_STOPS_IN_ONE_CHUNK))
            {
                addedIDs.AddRange(_stopsLayer.ApplyEdits(
                    chunk, Enumerable.Empty<Stop>(), Enumerable.Empty<long>()));
            }

            return addedIDs;
        }

        /// <summary>
        /// Deletes stops with the specified object IDs from the tracking service.
        /// </summary>
        /// <param name="stopIDs">Collection of object IDs identifying stops to be deleted.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="stopIDs"/>
        /// argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to delete stops from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public void DeleteStops(IEnumerable<long> stopIDs)
        {
            if (stopIDs == null)
            {
                throw new ArgumentNullException("stopIDs");
            }

            stopIDs = _stopsLayer.FilterObjectIDs(stopIDs);

            var deletedStops = _PrepareDeletion<Stop>(stopIDs);
            _stopsLayer.UpdateObjects(deletedStops);
        }

        /// <summary>
        /// Gets all events for the specified mobile device at the specified dates range.
        /// </summary>
        /// <param name="deviceID">Object ID identifying mobile device to get events for.</param>
        /// <param name="fromTime">Starting date time to get events for.</param>
        /// <param name="toTime">Ending date time to get events for.</param>
        /// <returns>Collection of events for the specified mobile device the specified
        /// dates range.</returns>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve events from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public IEnumerable<Event> GetEvents(long deviceID, DateTime fromTime, DateTime toTime)
        {
            var whereClause = string.Format(
                QUERY_FORMATTER,
                EVENTS_QUERY_WHERE_CLAUSE,
                GPObjectHelper.DateTimeToGPDateTime(fromTime),
                GPObjectHelper.DateTimeToGPDateTime(toTime),
                deviceID);

            var ids = _eventsLayer.QueryObjectIDs(whereClause);

            return _eventsLayer.QueryObjectsByIDs(ids);
        }

        /// <summary>
        /// Updates route settings for the specified date.
        /// </summary>
        /// <param name="plannedDate">The date to update route settings for.</param>
        /// <param name="routeSettings">The reference to serialized route settings.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="routeSettings"/>
        /// argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to update route settings at the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public void UpdateRouteSettings(DateTime plannedDate, string routeSettings)
        {
            if (routeSettings == null)
            {
                throw new ArgumentNullException("routeSettings");
            }

            plannedDate = plannedDate.Date;

            // Split route settings into multiple records due to feature service limitation.
            var parts = routeSettings.SplitToChunks(MAX_STRING_SIZE);
            var settings = parts
                .Select((part, index) =>
                    new Setting
                    {
                        KeyID = ROUTE_SETTINGS_KEY,
                        PlannedDate = plannedDate,
                        PartIndex = index,
                        Value = new string(part.ToArray()),
                    })
                .ToList();
            _PrepareAddition(settings);

            // Retrieve IDs of existing route settings for planned date.
            var whereClause = string.Format(
                QUERY_FORMATTER,
                SETTINGS_QUERY_WHERE_CLAUSE,
                ROUTE_SETTINGS_KEY,
                GPObjectHelper.DateTimeToGPDateTime(plannedDate));
            var existingIDs = _settingsLayer.QueryObjectIDs(whereClause);
            var deletedSettings = _PrepareDeletion<Setting>(existingIDs);

            // Delete current settings and add new ones in a single transaction.
            _settingsLayer.ApplyEdits(
                settings,
                deletedSettings,
                Enumerable.Empty<long>());
        }

        /// <summary>
        /// Updates barriers for the specified date.
        /// </summary>
        /// <param name="plannedDate">The date to update barriers for.</param>
        /// <param name="pointBarriers">The reference to the collection of point barriers
        /// to be used for the <paramref name="plannedDate"/>.</param>
        /// <param name="lineBarriers">The reference to the collection of line barriers
        /// to be used for the <paramref name="plannedDate"/>.</param>
        /// <param name="polygonBarriers">The reference to the collection of polygon barriers
        /// to be used for the <paramref name="plannedDate"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="pointBarriers"/>,
        /// <paramref name="lineBarriers"/> or <paramref name="polygonBarriers"/> argument
        /// is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to update barriers at the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        public void UpdateBarriers(
            DateTime plannedDate,
            IEnumerable<PointBarrier> pointBarriers,
            IEnumerable<LineBarrier> lineBarriers,
            IEnumerable<PolygonBarrier> polygonBarriers)
        {
            if (pointBarriers == null || pointBarriers.Any(barrier => barrier == null))
            {
                throw new ArgumentNullException("pointBarriers");
            }

            if (lineBarriers == null || lineBarriers.Any(barrier => barrier == null))
            {
                throw new ArgumentNullException("lineBarriers");
            }

            if (polygonBarriers == null || polygonBarriers.Any(barrier => barrier == null))
            {
                throw new ArgumentNullException("polygonBarriers");
            }

            _UpdateBarriers(_pointBarriersLayer, plannedDate, pointBarriers);
            _UpdateBarriers(_lineBarriersLayer, plannedDate, lineBarriers);
            _UpdateBarriers(_polygonBarriersLayer, plannedDate, polygonBarriers);
        }
        #endregion

        #region private methods

        /// <summary>
        /// Gets non deleted routes for the mobile devices with the 
        /// specified mobile devices ObjectIDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">Mobile devices IDs, which routes should be found.</param>
        /// <param name="plannedDate">The date/time to retrieve routes for.</param>
        /// <param name="returnFields">The reference to the collection of field names to
        /// retrieve values for. A wildcard value '*' could be used to retrieve all fields.</param>
        /// <returns>Routes associated with specified mobile devices ids.</returns>
        private IEnumerable<Route> _GetRoutesByMobileDevices(IEnumerable<long> mobileDevicesIDs,
            DateTime plannedDate,
            IEnumerable<string> returnFields)
        {
            return _routesLayer.QueryData(
                 _FormatUndeletedObjectsForPlannedDateQuery(mobileDevicesIDs, plannedDate),
                 returnFields,
                 GeometryReturningPolicy.WithoutGeometry);
        }

        /// <summary>
        /// Get new points count.
        /// </summary>
        /// <param name="route">Route, which points count must be added.</param>
        /// <param name="currentPointsCount">Current points number in chunk.</param>
        /// <returns>Number of points which will be in chunk if route will be added to it.</returns>
        private int _PointsCount(Route route, int currentPointsCount)
        {
            // Get route points count.
            var count = route.Shape != null ? route.Shape.TotalPointCount : 0;

            return currentPointsCount + count;
        }

        /// <summary>
        /// Format query which will return undeleted object for planned date with the 
        /// specified mobile devices IDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">List of mobile devices IDs.</param>
        /// <param name="plannedDate">The date/time to retrieve objects for.</param>
        /// <returns>Query string.</returns>
        private string _FormatUndeletedObjectsForPlannedDateQuery
            (IEnumerable<long> mobileDevicesIDs, DateTime plannedDate)
        {
            // Create query string for undeleted objects for specified date.
            var sb = new StringBuilder();
            sb.AppendFormat(
                QUERY_FORMATTER,
                NON_DELETED_OBJECTS_FOR_DATE_QUERY_WHERE_CLAUSE,
                GPObjectHelper.DateTimeToGPDateTime(plannedDate));

            // Add mobile devices IDs condition.
            sb.Append(AND_CLAUSE);
            sb.Append(OPEN_BRACKET);
            foreach (var deviceID in mobileDevicesIDs)
                sb.AppendFormat(QUERY_FORMATTER, DEVICEID_OR_CLAUSE, deviceID);
            // Remove last 'OR' from sb.
            sb.Remove(sb.Length - 1 - OR_LENGTH, OR_LENGTH);
            sb.Append(CLOSING_BRACKET);

            return sb.ToString();
        }

        /// <summary>
        /// Prepares the specified collection of objects for addition to feature service.
        /// </summary>
        /// <typeparam name="T">The type of objects to be prepared.</typeparam>
        /// <param name="newObjects">The reference to the collection of objects to be
        /// prepared.</param>
        private void _PrepareAddition<T>(IEnumerable<T> newObjects)
            where T : DataRecordBase
        {
            Debug.Assert(newObjects != null);

            foreach (var item in newObjects)
            {
                item.Deleted = DeletionStatus.NotDeleted;
            }
        }

        /// <summary>
        /// Prepares the specified collection of object IDs for deletion from feature service.
        /// </summary>
        /// <typeparam name="T">The type of objects to be prepared.</typeparam>
        /// <param name="deletedObjectIDs">The reference to the collection of object IDs to be
        /// prepared.</param>
        /// <returns>A reference to the collection of prepared objects.</returns>
        private IEnumerable<T> _PrepareDeletion<T>(IEnumerable<long> deletedObjectIDs)
            where T : DataRecordBase, new()
        {
            Debug.Assert(deletedObjectIDs != null);

            return deletedObjectIDs
                .Select(id =>
                    new T
                    {
                        ObjectID = id,
                        Deleted = DeletionStatus.Deleted,
                    })
                .ToList();
        }

        private void _UpdateBarriers<TBarrier>(
            IFeatureLayer<TBarrier> barriersLayer,
            DateTime plannedDate,
            IEnumerable<TBarrier> barriers)
            where TBarrier : BarrierBase, new()
        {
            Debug.Assert(barriersLayer != null);
            Debug.Assert(barriers != null && barriers.All(barrier => barrier != null));

            // Retrieve IDs of existing barriers for planned date.
            var whereClause = string.Format(
                QUERY_FORMATTER,
                NON_DELETED_OBJECTS_FOR_DATE_QUERY_WHERE_CLAUSE,
                GPObjectHelper.DateTimeToGPDateTime(plannedDate));
            var existingIDs = barriersLayer.QueryObjectIDs(whereClause);
            var deletedBarriers = _PrepareDeletion<TBarrier>(existingIDs);

            var newBarriers = barriers.ToList();
            _PrepareAddition(newBarriers);

            foreach (var barrier in newBarriers)
            {
                barrier.PlannedDate = plannedDate;
            }

            // Delete current barriers and add new ones in a single transaction.
            barriersLayer.ApplyEdits(
                newBarriers,
                deletedBarriers,
                Enumerable.Empty<long>());
        }
        #endregion

        #region private constants
        /// <summary>
        /// The name of the mobile devices feature layer.
        /// </summary>
        private const string DEVICES_LAYER = "Devices";
        
        /// <summary>
        /// The name of the stops feature layer.
        /// </summary>
        private const string STOPS_LAYER = "Stops";

        /// <summary>
        /// The name of the events feature layer.
        /// </summary>
        private const string EVENTS_LAYER = "Events";

        /// <summary>
        /// The name of the settings feature layer.
        /// </summary>
        private const string SETTINGS_LAYER = "Settings";

        /// <summary>
        /// The name of the point barriers feature layer.
        /// </summary>
        private const string POINT_BARRIERS_LAYER = "PointBarriers";

        /// <summary>
        /// The name of the line barriers feature layer.
        /// </summary>
        private const string LINE_BARRIERS_LAYER = "LineBarriers";

        /// <summary>
        /// The name of the polygon barriers feature layer.
        /// </summary>
        private const string POLYGON_BARRIERS_LAYER = "PolygonBarriers";

        /// <summary>
        /// The name of the routes feature layer.
        /// </summary>
        private const string ROUTES_LAYER = "Routes";

        /// <summary>
        /// The where clause format for retrieving events.
        /// </summary>
        private const string EVENTS_QUERY_WHERE_CLAUSE =
            "Timestamp>{0} AND Timestamp<{1} AND DeviceID={2} AND Deleted=0";

        /// <summary>
        /// The where clause format for retrieving route settings.
        /// </summary>
        /// TODO WORKAROUND remove when feature service will be fixed.
        private const string SETTINGS_QUERY_WHERE_CLAUSE =
            "KeyID='{0}' AND PlannedDate={1} AND Deleted=0";

        /// <summary>
        /// The where clause format for retrieving barriers.
        /// </summary>
        private const string NON_DELETED_OBJECTS_FOR_DATE_QUERY_WHERE_CLAUSE =
            "PlannedDate={0} AND Deleted=0";

        /// <summary>
        /// The where clause format for retrieving stops using device ID.
        /// </summary>
        private const string STOPS_BY_DEVICE_QUERY_WHERE_CLAUSE =
            "DeviceID={0} AND PlannedDate={1}";

        /// <summary>
        /// The where clause format for retrieving stops using device ID.
        /// </summary>
        private const string ROUTES_BY_DEVICE_QUERY_WHERE_CLAUSE =
            "DeviceID={0} AND PlannedDate={1}";

        /// <summary>
        /// Stores where clause which selects all non deleted records, i.e. the query with this
        /// where clause should return all available records.
        /// </summary>
        private const string EMPTY_FILTER = "Deleted=0";

        /// <summary>
        /// Stores where clause which does not filter any records, so the query with this where
        /// clause should return all records including deleted and existing ones.
        /// </summary>
        private const string UNFILTERED_QUERY = "1=1";

        /// <summary>
        /// The maximum number of character which can be stored in a single field of a feature
        /// service record.
        /// </summary>
        private const int MAX_STRING_SIZE = 500;

        /// <summary>
        /// Maximum number of points in one routes chunk.
        /// </summary>
        private const int MAXIMUM_NUMBER_OF_POINTS_IN_CHUNK = 50000;

        /// <summary>
        /// Maximum number of stops in one chunk.
        /// </summary>
        private const int MAX_NUMBER_OF_STOPS_IN_ONE_CHUNK = MAXIMUM_NUMBER_OF_POINTS_IN_CHUNK /2;

        /// <summary>
        /// And clause.
        /// </summary>
        private const string AND_CLAUSE = @" AND ";

        /// <summary>
        /// Format for device id clause.
        /// </summary>
        private const string DEVICEID_OR_CLAUSE = @"DeviceID={0} OR ";

        /// <summary>
        /// Length of 'OR' clause.
        /// </summary>
        private const int OR_LENGTH = 2;

        /// <summary>
        /// Opening and closing bracket.
        /// </summary>
        private const string OPEN_BRACKET = @"(";
        private const string CLOSING_BRACKET = @")";

        /// <summary>
        /// The key identifying route settings in the "Settings" layer.
        /// </summary>
        private static readonly Guid ROUTE_SETTINGS_KEY =
            new Guid("6A7FD758-E0D5-4116-B1CC-51BD2AB43E0E");

        /// <summary>
        /// Represents a record fields collection resulting in returning all available fields.
        /// </summary>
        private static readonly IEnumerable<string> ALL_FIELDS = EnumerableEx.Return("*");

        /// <summary>
        /// Represents a record fields collection resulting in returning Object ID field.
        /// </summary>
        private static readonly IEnumerable<string> OBJECTID_FIELD = EnumerableEx.Return("OBJECTID");
        
        /// <summary>
        /// The format provider to be used for formatting queries where clauses.
        /// </summary>
        private static readonly IFormatProvider QUERY_FORMATTER = CultureInfo.InvariantCulture;
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the feature layer with mobile devices.
        /// </summary>
        private IFeatureLayer<Device> _devicesLayer;
        
        /// <summary>
        /// The reference to the feature layer with stops.
        /// </summary>
        private IFeatureLayer<Stop> _stopsLayer;

        /// <summary>
        /// The reference to the feature layer with events.
        /// </summary>
        private IFeatureLayer<Event> _eventsLayer;

        /// <summary>
        /// The reference to the feature layer with settings.
        /// </summary>
        private IFeatureLayer<Setting> _settingsLayer;

        /// <summary>
        /// The reference to the feature layer with point barriers.
        /// </summary>
        private IFeatureLayer<PointBarrier> _pointBarriersLayer;

        /// <summary>
        /// The reference to the feature layer with line barriers.
        /// </summary>
        private IFeatureLayer<LineBarrier> _lineBarriersLayer;

        /// <summary>
        /// The reference to the feature layer with polygon barriers.
        /// </summary>
        private IFeatureLayer<PolygonBarrier> _polygonBarriersLayer;

        /// <summary>
        /// The reference to the feature layer with routes.
        /// </summary>
        private IFeatureLayer<Route> _routesLayer;

        #endregion
    }
}
