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
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides access to routes data such as collection of route stops in a simple
    /// serializable form.
    /// </summary>
    internal static class RouteExporter
    {
        #region public static methods
        /// <summary>
        /// Exports specified stops to serializable stop information.
        /// </summary>
        /// <param name="stops">The reference to the collection of stops to be exported.</param>
        /// <param name="capacitiesInfo">The reference to capacities info object to be used
        /// for retrieving custom order properties for stops.</param>
        /// <param name="orderCustomPropertiesInfo">The reference custom order properties info
        /// object.</param>
        /// <param name="addressFields">The reference to address fields object to be used
        /// for retrieving custom order properties for stops.</param>
        /// <param name="solver">The reference to VRPSolver to be used for retrieving
        /// curb approach policies for stops.</param>
        /// <param name="orderPropertiesFilter">Function returning true for custom order
        /// property names which should not be exported.</param>
        /// <returns>A reference to the collection of serializable stop information objects.
        /// </returns>
        public static IEnumerable<StopInfo> ExportStops(
            IEnumerable<Stop> stops,
            CapacitiesInfo capacitiesInfo,
            OrderCustomPropertiesInfo orderCustomPropertiesInfo,
            AddressField[] addressFields,
            IVrpSolver solver,
            Func<string, bool> orderPropertiesFilter = null)
        {
            Debug.Assert(stops != null);
            Debug.Assert(stops.All(stop => stop != null));
            Debug.Assert(stops.All(stop => stop.Route != null));
            Debug.Assert(capacitiesInfo != null);
            Debug.Assert(orderCustomPropertiesInfo != null);
            Debug.Assert(addressFields != null);

            if (!stops.Any())
            {
                return Enumerable.Empty<StopInfo>();
            }

            var capacityProperties = Order.GetPropertiesInfo(capacitiesInfo);
            var addressProperties = Order.GetPropertiesInfo(addressFields);
            var customProperties = Order.GetPropertiesInfo(orderCustomPropertiesInfo);

            var exportOrderProperties = _CreateExportOrderProperties(
                capacitiesInfo,
                orderCustomPropertiesInfo,
                addressFields,
                orderPropertiesFilter);

            // Make a dictionary for mapping routes to collection of sorted route stops.
            var routesSortedStops = stops
                .Select(stop => stop.Route)
                .Distinct()
                .ToDictionary(route => route, route => CommonHelpers.GetSortedStops(route));

            // Prepare result by exporting each stop individually.
            var settings = CommonHelpers.GetSolverSettings(solver);
            var result = stops
                .Select(stop => _ExportStop(
                    stop,
                    routesSortedStops[stop.Route],
                    exportOrderProperties,
                    addressProperties,
                    capacityProperties,
                    customProperties,
                    settings))
                .ToList();

            return result;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Creates collection of custom order properties to be exported.
        /// </summary>
        /// <param name="capacitiesInfo">The reference to capacities info object to be used
        /// for retrieving custom order properties for stops.</param>
        /// <param name="orderCustomPropertiesInfo">The reference custom order properties info
        /// object.</param>
        /// <param name="addressFields">The reference to address fields object to be used
        /// for retrieving custom order properties for stops.</param>
        /// <param name="orderPropertiesFilter">Function returning true for custom order
        /// property names which should not be exported.</param>
        /// <returns>A reference to the collection of custom order properties to be exported.
        /// </returns>
        private static IEnumerable<OrderPropertyInfo> _CreateExportOrderProperties(
            CapacitiesInfo capacitiesInfo,
            OrderCustomPropertiesInfo orderCustomPropertiesInfo,
            AddressField[] addressFields,
            Func<string, bool> orderPropertiesFilter)
        {
            Debug.Assert(capacitiesInfo != null);
            Debug.Assert(orderCustomPropertiesInfo != null);
            Debug.Assert(addressFields != null);

            if (orderPropertiesFilter == null)
            {
                orderPropertiesFilter = _ => false;
            }

            var names = new List<string>(Order.GetPropertyNames(
                capacitiesInfo,
                orderCustomPropertiesInfo,
                addressFields));
            var titles = new List<string>(Order.GetPropertyTitles(
                capacitiesInfo,
                orderCustomPropertiesInfo,
                addressFields));
            var orderPropertiesToExport = names
                .Zip(titles, OrderPropertyInfo.Create)
                .Where(info => !orderPropertiesFilter(info.Name))
                .ToArray();

            return orderPropertiesToExport;
        }

        /// <summary>
        /// Exports the specified stop into a serializable stop info object.
        /// </summary>
        /// <param name="stop">The reference to the stop object to be exported.</param>
        /// <param name="sortedRouteStops">A collection of route stops sorted by their sequence
        /// numbers.</param>
        /// <param name="exportOrderProperties">The reference to the collection
        /// of custom order properties to be exported.</param>
        /// <param name="addressProperties">The reference to the collection
        /// of order address properties to be exported.</param>
        /// <param name="capacityProperties">The reference to the collection
        /// of order capacity properties to be exported.</param>
        /// <param name="customProperties">The reference to the collection
        /// of custom order properties to be exported.</param>
        /// <param name="settings">Current solver settings to be used for retrieving stop
        /// properties.</param>
        /// <returns>A stop info object corresponding to the specified stop.</returns>
        private static StopInfo _ExportStop(
            Stop stop,
            IList<Stop> sortedRouteStops,
            IEnumerable<OrderPropertyInfo> exportOrderProperties,
            IEnumerable<OrderPropertyInfo> addressProperties,
            IEnumerable<OrderPropertyInfo> capacityProperties,
            IEnumerable<OrderPropertyInfo> customProperties,
            SolverSettings settings)
        {
            Debug.Assert(stop != null);
            Debug.Assert(sortedRouteStops != null);
            Debug.Assert(sortedRouteStops.All(s => s != null));
            Debug.Assert(sortedRouteStops.All(s => stop.Route == s.Route));

            var commentsProperties = _GetOrderProperties(stop, exportOrderProperties);

            var result = new StopInfo
            {
                Name = _GetStopName(stop, sortedRouteStops),
                Location = _GetStopLocation(stop, sortedRouteStops),
                Address = _GetOrderProperties(stop, addressProperties),
                Capacities = _GetOrderProperties(stop, capacityProperties),
                CustomOrderProperties = _GetOrderProperties(stop, customProperties),
                OrderComments = _GetOrderComments(commentsProperties),
                ArriveTime = stop.ArriveTime,
            };

            // Fill optional order-specific or depot-specific properties.
            var order = stop.AssociatedObject as Order;
            var depot = stop.AssociatedObject as Location;

            if (order != null)
            {
                _FillOrderProperties(order, settings, result);
            }
            else if (depot != null)
            {
                _FillDepotProperties(depot, settings, result);
            }

            return result;
        }

        /// <summary>
        /// Fill order-specific properties.
        /// </summary>
        /// <param name="order">Order.</param>
        /// <param name="settings">Current solver settings.</param>
        /// <param name="result">Stop information to fill in.</param>
        private static void _FillOrderProperties(Order order,
            SolverSettings settings, StopInfo result)
        {
            Debug.Assert(order != null);
            Debug.Assert(result != null);

            result.OrderType = order.Type;
            result.Priority = order.Priority;
            result.MaxViolationTime = (int)order.MaxViolationTime;

            // Fill curb approach policies.
            if (settings != null)
            {
                result.CurbApproach = settings.GetOrderCurbApproach();
            }
        }

        /// <summary>
        /// Fill depot-specific properties.
        /// </summary>
        /// <param name="depot">Depot.</param>
        /// <param name="settings">Current solver settings.</param>
        /// <param name="result">Stop information to fill in.</param>
        private static void _FillDepotProperties(Location depot,
            SolverSettings settings, StopInfo result)
        {
            Debug.Assert(depot != null);
            Debug.Assert(result != null);

            // Fill curb approach policies.
            if (settings != null)
            {
                result.CurbApproach = settings.GetDepotCurbApproach();
            }
        }

        /// <summary>
        /// Gets name for the specified stop.
        /// </summary>
        /// <param name="stop">The reference to the stop object to get name for.</param>
        /// <param name="routeStops">The collection of route stops sorted by their
        /// sequence numbers for the route containing the <paramref name="stop"/>.</param>
        /// <returns>Name of the specified stop.</returns>
        private static string _GetStopName(Stop stop, IList<Stop> routeStops)
        {
            Debug.Assert(stop != null);
            Debug.Assert(routeStops != null);
            Debug.Assert(routeStops.Contains(stop));

            var order = stop.AssociatedObject as Order;
            if (order != null)
            {
                return order.Name;
            }

            var location = stop.AssociatedObject as Location;
            if (location != null)
            {
                var name = location.Name;
                if (stop == routeStops.First())
                    name = string.Format(Properties.Resources.StartLocationString, name);
                else if (stop == routeStops.Last())
                    name = string.Format(Properties.Resources.FinishLocationString, name);
                else
                    name = string.Format(Properties.Resources.RenewalLocationString, name);

                return name;
            }

            // in case of a break
            return stop.Name;
        }

        /// <summary>
        /// Gets location of the specified stop.
        /// </summary>
        /// <param name="stop">The reference to stop to get location for.</param>
        /// <param name="sortedRouteStops">The collection of route stops sorted by their
        /// sequence numbers for the route containing the <paramref name="stop"/>.</param>
        /// <returns></returns>
        private static Point _GetStopLocation(Stop stop, IList<Stop> sortedRouteStops)
        {
            Debug.Assert(stop != null);
            Debug.Assert(sortedRouteStops != null);
            Debug.Assert(sortedRouteStops.All(item => item != null));
            Debug.Assert(sortedRouteStops.All(item => item.Route == stop.Route));

            var mapLocation = stop.MapLocation;
            if (mapLocation.HasValue)
            {
                return mapLocation.Value;
            }

            if (stop.StopType != StopType.Lunch)
            {
                throw new InvalidOperationException(
                    Properties.Messages.Error_GrfExporterNoLocationForStop); // exception
            }

            // find stop index in collection
            var firstStopIndex = sortedRouteStops.First().SequenceNumber;
            var currentIndex = stop.SequenceNumber - firstStopIndex;
            var stopWithLocation = SolveHelper.GetActualLunchStop(sortedRouteStops, currentIndex);
            if (!stopWithLocation.MapLocation.HasValue)
            {
                throw new InvalidOperationException(
                    Properties.Messages.Error_GrfExporterNoLocationForStop); // exception
            }

            return stopWithLocation.MapLocation.Value;
        }

        /// <summary>
        /// Gets comments for the specified stop built of the specified custom order properties.
        /// </summary>
        /// <param name="stop">The reference to the stop object to get comments for.</param>
        /// <param name="orderPropertiesToExport">The reference to the collection
        /// of custom order properties to be exported.</param>
        /// <returns>A string with comments for the specified stop or empty string if
        /// stop is not associated with an order.</returns>
        private static string _GetOrderComments(AttrDictionary commentsProperties)
        {
            Debug.Assert(commentsProperties != null);

            StringBuilder comments = new StringBuilder();
            foreach (var item in commentsProperties)
            {
                var value = item.Value;
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    comments.AppendLine(string.Format("{0}: {1}", item.Key, value));
                }
            }

            return comments.ToString();
        }
        
        /// <summary>
        /// Gets properties dictionary for the specified stop built of the specified order property
        /// info objects.
        /// </summary>
        /// <param name="stop">The reference to the stop object to get properties for.</param>
        /// <param name="orderPropertiesToExport">The reference to the collection
        /// of custom order properties to be exported.</param>
        /// <returns>A dictionary with properties for an order associated with the specified
        /// stop.</returns>
        private static AttrDictionary _GetOrderProperties(
            Stop stop,
            IEnumerable<OrderPropertyInfo> orderPropertiesToExport)
        {
            Debug.Assert(stop != null);
            Debug.Assert(orderPropertiesToExport != null);
            Debug.Assert(orderPropertiesToExport.All(info => info != null));

            var properties = new AttrDictionary();
            var order = stop.AssociatedObject as Order;
            if (order == null)
            {
                return properties;
            }

            object value = null;

            foreach (var info in orderPropertiesToExport)
            {
                if (info.Name == Order.PropertyNamePlannedDate)
                    value = stop.ArriveTime;
                else
                    value = Order.GetPropertyValue(order, info.Name);

                if (value != null && value.ToString().Length > 0)
                {
                    properties.Add(info.Title, value);
                }
            }

            return properties;
        }

        #endregion
    }
}
