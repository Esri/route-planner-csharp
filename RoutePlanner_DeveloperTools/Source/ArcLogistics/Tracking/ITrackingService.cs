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
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Encapsulates tracking service implementation details.
    /// </summary>
    internal interface ITrackingService
    {
        /// <summary>
        /// Gets all mobile devices available at the service.
        /// </summary>
        /// <returns>Collection of all mobile devices available at the
        /// tracking service.</returns>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to retrieve devices from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        IEnumerable<Device> GetAllMobileDevices();

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
        IEnumerable<Device> GetMobileDevices(IEnumerable<long> mobileDeviceIDs);

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
        IEnumerable<long> AddMobileDevices(IEnumerable<Device> mobileDevices);

        /// <summary>
        /// Adds specified routes to the tracking service.
        /// </summary>
        /// <param name="newRoutes">Collection of stops to be added.</param>
        /// <param name="routesToDelete">Collection of stops to be deleted.</param>
        /// <returns>Collection of object IDs identifying added stops.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="stops"/>
        /// argument or any of it's elements is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to add routes to the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        IEnumerable<long> UpdateRoutes(IEnumerable<Route> newRoutes, IEnumerable<Route> routesToDelete);

        /// <summary>
        /// Gets non deleted routes for the mobile devices with the 
        /// specified objects IDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">Mobile devices IDs, which routes should be found.</param>
        /// <param name="plannedDate">The date/time to retrieve routes for.</param>
        /// <returns>Routes associated with specified mobile devices ids.</returns>
        IEnumerable<Route> GetNotDeletedRoutes(IEnumerable<long> mobileDevicesIDs, DateTime plannedDate);

        /// <summary>
        /// Gets non deleted stops for the mobile devices with the 
        /// specified objects IDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">Mobile devices IDs, which stops should be found.</param>
        /// <param name="plannedDate">The date/time to retrieve stops for.</param>
        /// <returns>Stops associated with specified mobile devices ids.</returns>
        IEnumerable<Stop> GetNotDeletedStops(IEnumerable<long> mobileDevicesIDs, DateTime plannedDate);

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
        void UpdateMobileDevices(IEnumerable<Device> mobileDevices);

        /// <summary>
        /// Deletes specified mobile devices from the tracking service.
        /// </summary>
        /// <param name="mobileDevices">Collection of mobile devices to be deleted.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="mobileDevices"/>
        /// argument is a null reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to delete mobile devices from the tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        void DeleteMobileDevices(IEnumerable<Device> mobileDevices);

        /// <summary>
        /// Gets non deleted stops for the mobile device with 
        /// the specified object ID from the tracking service.
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
        IEnumerable<Stop> GetNonDeletedStops(long mobileDeviceID, DateTime plannedDate);

        /// <summary>
        /// Gets stops for the mobile device with the specified object ID from the tracking service.
        /// </summary>
        /// <param name="mobileDeviceID"></param>
        /// <param name="plannedDate"></param>
        /// <returns></returns>
        IEnumerable<Stop> GetAllStops(long mobileDeviceID, DateTime plannedDate);

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
        IEnumerable<long> AddStops(IEnumerable<Stop> stops);

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
        IEnumerable<long> UpdateStops(IEnumerable<Stop> newStops, IEnumerable<Stop> updatedStops);

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
        void DeleteStops(IEnumerable<long> stopIDs);

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
        IEnumerable<Event> GetEvents(long deviceID, DateTime fromTime, DateTime toTime);

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
        void UpdateRouteSettings(DateTime plannedDate, string routeSettings);

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
        void UpdateBarriers(
            DateTime plannedDate,
            IEnumerable<PointBarrier> pointBarriers,
            IEnumerable<LineBarrier> lineBarriers,
            IEnumerable<PolygonBarrier> polygonBarriers);

        /// <summary>
        /// Gets IDs of non deleted routes for the mobile devices with the 
        /// specified objects IDs from the tracking service.
        /// </summary>
        /// <param name="mobileDevicesIDs">Mobile devices IDs, which routes should be found.</param>
        /// <param name="plannedDate">The date/time to retrieve routes for.</param>
        /// <returns>Routes IDs associated with specified mobile devices ids.</returns>
        IEnumerable<long> GetNotDeletedRoutesIDs(IEnumerable<long> mobileDevicesIDs,
            DateTime plannedDate);
    }
}
