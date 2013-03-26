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
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geometry;
using DM = ESRI.ArcLogistics.Tracking.TrackingService.DataModel;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Class contains helper methods for tracking operations.
    /// </summary>
    internal static class TrackingHelper
    {
        #region public methods
        /// <summary>
        /// Method gets assigned mobile device from route.
        /// </summary>
        /// <param name="route">Route to get device.</param>
        /// <returns>Mobile device assigned to route.</returns>
        public static MobileDevice GetDeviceByRoute(Route route)
        {
            MobileDevice device = null;

            if (route == null)
                return null;

            if (route.Driver == null)
                return null;

            // Driver is more priority than vehicle.
            device = route.Driver.MobileDevice;
            if (device == null && route.Vehicle != null)
            {
                device = route.Vehicle.MobileDevice;
            }

            return device;
        }
        #endregion

        #region internal methods

        /// <summary>
        /// Applies arrival delay to stops:
        /// Adds Arrival Delay value to Service Time value for every stop,
        /// which has unique locations except last one stop.
        /// </summary>
        /// <param name="arrivalDelay">Arrival delay value.</param>
        /// <param name="stops">Stops collection to update.</param>
        internal static void ApplyArrivalDelayToStops(int arrivalDelay, IList<DM.Stop> stops)
        {
            Debug.Assert(stops != null);

            if (stops.Count == 0)
                return;

            // Get last stop.
            int startIndex = stops.Count - 1;
            DM.Stop last = stops[startIndex];

            // Remember location point from last stop.
            Point? prevStopPoint = null;
            if (DM.StopType.Break != last.Type)
                prevStopPoint = last.Location.GetValueOrDefault();

            // Start from previous stop,
            // because we don't need to consider Arrival Delay at last one.
            --startIndex;

            for (int index = startIndex; index >= 0; index--)
            {
                DM.Stop stop = stops[index];

                Point currentPoint = stop.Location.GetValueOrDefault();

                // Apply arrival delay for every unique location.
                if (DM.StopType.Break == stop.Type || currentPoint == prevStopPoint)
                    continue;

                stop.ServiceTime += arrivalDelay;
                prevStopPoint = currentPoint;
            }
        }

        #endregion
    }
}
