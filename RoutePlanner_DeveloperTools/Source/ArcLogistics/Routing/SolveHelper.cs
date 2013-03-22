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
using System.Globalization;
using System.Collections.Generic;

using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Class contains helper method for solve operations.
    /// </summary>
    internal static class SolveHelper
    {
        #region Public static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks is order locked.
        /// </summary>
        /// <param name="order">Order to check.</param>
        /// <param name="schedule">Schedule to check.</param>
        /// <returns>TRUE if order has stops belong schedule and this route or stop is locked.</returns>
        public static bool IsOrderLocked(Order order, Schedule schedule)
        {
            Debug.Assert(order != null);
            Debug.Assert(schedule != null);

            bool isLocked = order.Stops.Any(stop =>
                                                (null != stop.Route) &&
                                                stop.Route.Schedule.Equals(schedule) &&
                                                (stop.Route.IsLocked || stop.IsLocked));
            return isLocked;
        }

        /// <summary>
        /// Gets assigned orders from route.
        /// </summary>
        /// <param name="route">Route as source for assigned orders.</param>
        /// <returns></returns>
        public static IEnumerable<Order> GetAssignedOrders(Route route)
        {
            Debug.Assert(route != null);

            var orders =
                from stop in route.Stops
                where stop.StopType == StopType.Order
                let order = stop.AssociatedObject as Order
                select order;

            return orders;
        }

        /// <summary>
        /// Creates RouteException object by REST service exception.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="serviceEx">Service exception to conversion.</param>
        public static RouteException ConvertServiceException(string message, RestException serviceEx)
        {
            Debug.Assert(!string.IsNullOrEmpty(message));
            Debug.Assert(serviceEx != null);

            var errorMessage = _GetServiceExceptionMessage(message, serviceEx);
            return new RouteException(errorMessage, serviceEx); // exception
        }

        /// <summary>
        /// Gets enabled restriction names.
        /// </summary>
        /// <param name="restrictions">Restriciton to filtration.</param>
        /// <returns>Returns collection of names of restrictions that have "enabled" flag set to true.</returns>
        public static ICollection<string> GetEnabledRestrictionNames(
            ICollection<Restriction> restrictions)
        {
            Debug.Assert(restrictions != null);

            var enabledRestrictionNames =
                from restriction in restrictions
                where restriction.IsEnabled
                select restriction.NetworkAttributeName;

            return enabledRestrictionNames.ToList();
        }

        #endregion // Public static methods

        #region Internal static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sorts StopData objects respecting sequence number.
        /// </summary>
        /// <param name="stops">Stops to sorting.</param>
        internal static void SortBySequence(List<StopData> stops)
        {
            Debug.Assert(stops != null);

            stops.Sort(delegate(StopData s1, StopData s2)
            {
                return s1.SequenceNumber.CompareTo(s2.SequenceNumber);
            });
        }

        /// <summary>
        /// Considers arrival delay in stops.
        /// </summary>
        /// <param name="arrivalDelay">Arrival delay value.</param>
        /// <param name="stops">Stops to update.</param>
        internal static void ConsiderArrivalDelayInStops(int arrivalDelay, IList<StopData> stops)
        {
            Debug.Assert(null != stops);

            if (0 == stops.Count)
                return; // stop process

            int startIndex = stops.Count - 1; // last index
            StopData last = stops[startIndex];

            Point? prevStopPoint = null;
            if (StopType.Lunch != last.StopType)
                prevStopPoint = _GetStopPoint(last);

            --startIndex; // start from before last
            // (last one on a route - don't add the value to its service time)
            Debug.Assert(0 <= startIndex);

            for (int index = startIndex; 0 <= index; --index)
            {
                StopData stop = stops[index];

                int delay = 0;
                if (StopType.Lunch != stop.StopType)
                    delay = _GetArrivalDelay(arrivalDelay, stop, ref prevStopPoint);

                stop.TimeAtStop += delay;
            }
        }

        /// <summary>
        /// Gets stop for a stop where lunch stop was placed (Export version).
        /// </summary>
        /// <param name="sortedStops">A collection of route stops sorted by their sequence
        /// numbers.</param>
        /// <param name="lunchIndex">The lunch stop index to get actual stop for.</param>
        /// <returns>A stop for an actual stop where lunch was placed.</returns>
        internal static Stop GetActualLunchStop(IList<Stop> sortedStops, int lunchIndex)
        {
            Debug.Assert(null != sortedStops);

            Stop actualStop = null;
            if ((0 <= lunchIndex) && (lunchIndex < sortedStops.Count))
            {
                Stop stop = sortedStops[lunchIndex];
                Debug.Assert(stop.StopType == StopType.Lunch);

                // use previously stop as actual
                int indexStep = -1;

                // find actual stop in selected direction
                actualStop = _GetActualLunchStop(sortedStops, lunchIndex + indexStep, indexStep);

                // not found in selected direction
                if (null == actualStop)
                {   // find in invert direction
                    indexStep = -indexStep;
                    actualStop = _GetActualLunchStop(sortedStops, lunchIndex + indexStep, indexStep);
                }
            }

            // not found - stop process
            if (null == actualStop)
            {
                string message = Properties.Messages.Error_GrfExporterNoBreakStopLocation;
                throw new InvalidOperationException(message); // exception
            }

            return actualStop;
        }

        /// <summary>
        /// Gets stop data for a stop where lunch stop was placed (Routing version).
        /// </summary>
        /// <param name="sortedStops">A collection of route stops sorted by their sequence
        /// numbers.</param>
        /// <param name="lunchIndex">The lunch stop index to get actual stop for.</param>
        /// <returns>A stop data for an actual stop where lunch was placed.</returns>
        internal static StopData GetActualLunchStop(IList<StopData> sortedStops, int lunchIndex)
        {
            Debug.Assert(null != sortedStops);

            StopData actualStop = null;
            if ((0 <= lunchIndex) && (lunchIndex < sortedStops.Count))
            {
                StopData stop = sortedStops[lunchIndex];
                Debug.Assert(stop.StopType == StopType.Lunch);

                // use previously stop as actual
                int indexStep = -1;

                // find actual stop in selected direction
                actualStop = _GetActualLunchStop(sortedStops, lunchIndex + indexStep, indexStep);

                // not found in seleted direction
                if (null == actualStop)
                {   // find in invert direction
                    indexStep = -indexStep;
                    actualStop = _GetActualLunchStop(sortedStops, lunchIndex + indexStep, indexStep);
                }
            }

            // not found - stop process
            if (null == actualStop)
            {
                string message = Properties.Messages.Error_RoutingMissingBreakStop;
                throw new InvalidOperationException(message); // exception
            }

            return actualStop;
        }

        #endregion // Internal static methods

        #region Private static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets stop's point.
        /// </summary>
        /// <param name="stop">Stop as source for point.</param>
        /// <returns>Stop's related point.</returns>
        private static Point _GetStopPoint(StopData stop)
        {
            Debug.Assert(null != stop);
            Debug.Assert(StopType.Lunch != stop.StopType);
            Debug.Assert(null != stop.AssociatedObject);

            var geocodable = stop.AssociatedObject as IGeocodable;
            Debug.Assert(null != geocodable);

            Point? pt = geocodable.GeoLocation;
            Debug.Assert(pt != null);

            return pt.Value;
        }

        /// <summary>
        /// Gets arrival delay for selected stop.
        /// </summary>
        /// <param name="arrivalDelay">Arrival delay value.</param>
        /// <param name="stop">Stop as source.</param>
        /// <param name="prevStopPoint">Previously stop point.</param>
        /// <returns>Arrival delay value or 0 if not need considers arrival delay.</returns>
        private static int _GetArrivalDelay(int arrivalDelay,
                                            StopData stop,
                                            ref Point? prevStopPoint)
        {
            Debug.Assert(null != stop);

            Point point = _GetStopPoint(stop);

            // if location changed
            bool needDelay = (!prevStopPoint.HasValue ||
                              (prevStopPoint.HasValue && (point != prevStopPoint)));
            prevStopPoint = point;

            return needDelay ? arrivalDelay : 0;
        }

        /// <summary>
        /// Gets message for the specified service exception.
        /// </summary>
        /// <param name="message">The text to prefix generated exception message
        /// with.</param>
        /// <param name="serviceException">The exception to generate message for.</param>
        /// <returns>Text describing the specified service exception.</returns>
        private static string _GetServiceExceptionMessage(string message,
                                                          RestException serviceException)
        {
            Debug.Assert(message != null);
            Debug.Assert(serviceException != null);

            string serviceMsg = serviceException.Message;
            if (!string.IsNullOrEmpty(serviceMsg))
            {
                return string.Format(Properties.Messages.Error_ArcgisRestError,
                                     message,
                                     serviceMsg);
            }

            var errorCode = string.Format(CultureInfo.InvariantCulture,
                                          Properties.Resources.ArcGisRestErrorCodeFormat,
                                          serviceException.ErrorCode);

            var errorMessage = string.Format(Properties.Messages.Error_ArcgisRestUnspecifiedError,
                                             message,
                                             errorCode);
            return errorMessage;
        }

        /// <summary>
        /// Gets stop data for a stop where lunch stop was placed.
        /// </summary>
        /// <param name="sortedStops">A collection of route stops sorted by their sequence
        /// numbers.</param>
        /// <param name="startIndex">Index to start search in sortedStops.</param>
        /// <param name="indexStep">Index increment value (1 or -1).</param>
        /// <returns>A stop for an actual stop where lunch was placed or NULL.</returns>
        private static Stop _GetActualLunchStop(IList<Stop> sortedStops,
                                                int startIndex,
                                                int indexStep)
        {
            Debug.Assert(null != sortedStops);

            Stop actualStop = null;
            if (startIndex < sortedStops.Count)
            {
                for (int index = startIndex;
                     (0 <= index) && (index < sortedStops.Count);
                     index += indexStep)
                {
                    Stop currStop = sortedStops[index];
                    if (currStop.StopType != StopType.Lunch)
                    {   // actual stop can't be lunch
                        actualStop = currStop;
                        break; // result found
                    }
                }
            }

            return actualStop;
        }

        /// <summary>
        /// Gets stop data for a stop where lunch stop was placed.
        /// </summary>
        /// <param name="sortedStops">A collection of route stops sorted by their sequence
        /// numbers.</param>
        /// <param name="startIndex">Index to srart search in sortedStops.</param>
        /// <param name="indexStep">Index incrementation value (1 or -1).</param>
        /// <returns>A stop data for an actual stop where lunch was placed or NULL.</returns>
        private static StopData _GetActualLunchStop(IList<StopData> sortedStops,
                                                    int startIndex,
                                                    int indexStep)
        {
            Debug.Assert(null != sortedStops);
            Debug.Assert(startIndex < sortedStops.Count);

            StopData actualStop = null;
            for (int index = startIndex;
                 (0 <= index) && (index < sortedStops.Count);
                 index += indexStep)
            {
                StopData currStop = sortedStops[index];
                if (currStop.StopType != StopType.Lunch)
                {   // actual stop can't be lunch
                    actualStop = currStop;
                    break; // result found
                }
            }

            return actualStop;
        }

        #endregion // Private static methods
    }
}
