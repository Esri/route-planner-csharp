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
using System.Windows;
using System.Diagnostics;
using System.Collections.Generic;

using AppData = ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Dialogs;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// ConstraintViolationsChecker class
    /// </summary>
    internal class ConstraintViolationsChecker
    {
        #region Public helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static bool Check(Schedule schedule, ICollection<Route> routes, ICollection<Order> orders)
        {
            Debug.Assert(null != schedule);
            Debug.Assert(null != routes);
            Debug.Assert(null != orders);

            bool doesProcessStart = true;
            if (Properties.Settings.Default.IsRoutingConstraintCheckEnabled)
            {
                ICollection<MessageDetail> details = _Check(schedule, routes, orders);
                if (0 < details.Count)
                {
                    Debug.Assert(schedule.PlannedDate.HasValue);
                    DateTime date = schedule.PlannedDate.Value;

                    string text = App.Current.GetString("ConstraintsCheckerHeadMessageFmt", date.ToShortDateString());
                    App.Current.Messenger.AddWarning(text, details);

                    bool ignoreCheck = false;
                    MessageBoxExButtonType result = MessageBoxEx.Show(App.Current.MainWindow,
                                                                      App.Current.FindString("ConstraintsCannotBeMetText"),
                                                                      App.Current.FindString("ConstraintsCannotBeMetTitle"),
                                                                      System.Windows.Forms.MessageBoxButtons.YesNo,
                                                                      MessageBoxImage.Warning,
                                                                      App.Current.FindString("ConstraintsCannotBeMetCheckBoxText"),
                                                                      ref ignoreCheck);
                    if (ignoreCheck)
                    {   // update response
                        Properties.Settings.Default.IsRoutingConstraintCheckEnabled = false;
                        Properties.Settings.Default.Save();
                    }

                    doesProcessStart = (MessageBoxExButtonType.Yes == result);
                }
            }

            return doesProcessStart;
        }

        /// <summary>
        /// Checks if the route the specified order is assigned to is locked.
        /// </summary>
        /// <param name="order">The order to be checked.</param>
        /// <param name="schedule">The schedule to take locks from.</param>
        /// <returns>True if and only if the route containig stop associated with
        /// the order is locked.</returns>
        public static bool IsOrderRouteLocked(Order order, Schedule schedule)
        {
            Debug.Assert(order != null);
            Debug.Assert(schedule != null);

            bool isLocked = false;

            // Go thru all the order stops.
            for (int i = 0; i < order.Stops.Count; i++)
            {
                var stopRoute = order.Stops[i].Route;
                
                if (stopRoute != null && stopRoute.Schedule == schedule)
                {
                    // Route found.
                    isLocked = stopRoute.IsLocked;
                    break;
                }

            }

            return isLocked;
        }

        /// <summary>
        /// Checks if the specified order is locked in the specified schedule.
        /// </summary>
        /// <param name="order">The order to be checked.</param>
        /// <param name="schedule">The schedule to take locks from.</param>
        /// <returns>True if and only if either stop for the specified order
        /// is locked or the route (if any) containig such stop is locked.</returns>
        public static bool IsOrderLocked(Order order, Schedule schedule)
        {
            Debug.Assert(order != null);
            Debug.Assert(schedule != null);

            if (IsOrderRouteLocked(order, schedule))
            {
                return true;
            }

            if (order.Stops.Any((stop) => stop.IsLocked))
            {
                return true;
            }

            return false;
        }


        #endregion // Public helpers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static void _GetOrdersDeliveryType(ICollection<Order> orders, ref OrderType type,
                                                   ref bool isMixedCase)
        {
            isMixedCase = false;
            type = OrderType.Delivery;
            bool isFirst = true;
            foreach (Order order in orders)
            {
                if (isFirst)
                    type = order.Type;
                else if (type != order.Type)
                    isMixedCase = true;
            }
        }

        private static List<MessageDetail> _CheckMaxOrderCount(int routesSupportedOrderCount,
                                                               int ordersCount,
                                                               ICollection<Route> routes)
        {
            List<MessageDetail> details = new List<MessageDetail>();
            if (routesSupportedOrderCount < ordersCount)
            {
                int supportedOrderCount = Math.Max(routesSupportedOrderCount, 0);
                if (1 == routes.Count)
                {
                    string format = App.Current.FindString("ConstraintsCheckerMaxOrderCntRouteMessageFmt");
                    format = string.Format(format, ordersCount.ToString(), "{0}", supportedOrderCount.ToString());
                    details.Add(new MessageDetail(MessageType.Warning, format, routes.First()));
                }
                else
                {
                    string format = App.Current.FindString("ConstraintsCheckerMaxOrderCntRoutesMessageFmt");
                    string text = string.Format(format, ordersCount, supportedOrderCount);
                    details.Add(new MessageDetail(MessageType.Warning, text));
                }
            }

            return details;
        }

        private static ICollection<T> _CanAccommodateOrderSpecialties<T>(ICollection<T> supportedSpecialities,
                                                                         ICollection<T> orderSpecialities)
        {
            Debug.Assert(null != supportedSpecialities);
            Debug.Assert(null != orderSpecialities);

            List<T> notSupportedSpecilaities = new List<T>();
            foreach (T specialty in orderSpecialities)
            {
                if (!supportedSpecialities.Contains(specialty))
                    notSupportedSpecilaities.Add(specialty);
            }

            return (0 == notSupportedSpecilaities.Count) ? null : notSupportedSpecilaities;
        }

        private static ICollection<VehicleSpecialty> _CanAccommodateOrderVehicleSpecialties(ICollection<Route> routes,
                                                                                            Order order)
        {
            Debug.Assert(null != routes);
            Debug.Assert(null != order);

            List<VehicleSpecialty> routesSpecialities = new List<VehicleSpecialty>();
            foreach (Route route in routes)
            {
                AppData.IDataObjectCollection<VehicleSpecialty> specialities = route.Vehicle.Specialties;
                foreach (VehicleSpecialty specialty in specialities)
                {
                    if (!routesSpecialities.Contains(specialty))
                        routesSpecialities.Add(specialty);
                }
            }

            return _CanAccommodateOrderSpecialties(routesSpecialities, order.VehicleSpecialties);
        }

        private static ICollection<DriverSpecialty> _CanAccommodateOrderDriverSpecialties(ICollection<Route> routes,
                                                                                          Order order)
        {
            Debug.Assert(null != routes);
            Debug.Assert(null != order);

            List<DriverSpecialty> routesSpecialities = new List<DriverSpecialty>();
            foreach (Route route in routes)
            {
                AppData.IDataObjectCollection<DriverSpecialty> specialities = route.Driver.Specialties;
                foreach (DriverSpecialty specialty in specialities)
                {
                    if (!routesSpecialities.Contains(specialty))
                        routesSpecialities.Add(specialty);
                }
            }

            return _CanAccommodateOrderSpecialties(routesSpecialities, order.DriverSpecialties);
        }

        private static List<MessageDetail> _CheckCapacities(Capacities routesAvailableCapacities,
                                                            Capacities ordersCapacities,
                                                            ICollection<Route> routes)
        {
            List<MessageDetail> details = new List<MessageDetail>();

            bool checkOneRoute = (1 == routes.Count);
            Route route = routes.First();

            Debug.Assert(routesAvailableCapacities.Count == ordersCapacities.Count);
            for (int cap = 0; cap < routesAvailableCapacities.Count; ++cap)
            {
                if (routesAvailableCapacities[cap] < ordersCapacities[cap])
                {
                    if (checkOneRoute)
                    {
                        string format = App.Current.FindString("ConstraintsCheckerCapacitiesRouteMessageFmt");
                        format = string.Format(format, route.CapacitiesInfo[cap].Name, ordersCapacities[cap].ToString(), "{0}", routesAvailableCapacities[cap].ToString(), (ordersCapacities[cap] - routesAvailableCapacities[cap]).ToString());
                        details.Add(new MessageDetail(MessageType.Warning, format, route.Vehicle));
                    }
                    else
                    {
                        string format = App.Current.FindString("ConstraintsCheckerCapacitiesRoutesMessageFmt");
                        string text = string.Format(format, route.CapacitiesInfo[cap].Name, ordersCapacities[cap], routesAvailableCapacities[cap], ordersCapacities[cap] - routesAvailableCapacities[cap]);
                        details.Add(new MessageDetail(MessageType.Warning, text));
                    }
                }
            }

            return details;
        }

        private static List<MessageDetail> _CheckVehicleSpecialties(ICollection<Route> routes, ICollection<Order> orders)
        {
            List<MessageDetail> details = new List<MessageDetail>();

            bool checkOneRoute = (1 == routes.Count);

            string format = App.Current.FindString((checkOneRoute)? "ConstraintsCheckerVehicleSpecRouteMessageFmt" :
                                                                    "ConstraintsCheckerVehicleSpecRoutesMessageFmt");
            foreach (Order order in orders)
            {
                ICollection<VehicleSpecialty> specialties = (checkOneRoute)?
                        _CanAccommodateOrderSpecialties(routes.First().Vehicle.Specialties, order.VehicleSpecialties) :
                        _CanAccommodateOrderVehicleSpecialties(routes, order);
                if (null != specialties)
                {
                    int paramCount = 1 + specialties.Count;
                    if (checkOneRoute)
                        ++paramCount;
                    AppData.DataObject[] param = new AppData.DataObject[paramCount];

                    int index = 0;
                    if (checkOneRoute)
                        param[index++] = routes.First().Vehicle;
                    param[index++] = order;

                    string specialtiesFormat = ViolationsHelper.GetObjectListFormat(specialties, ref index, param);
                    string messageFormat = (checkOneRoute) ? string.Format(format, "{0}", "{1}", specialtiesFormat) :
                                                             string.Format(format, "{0}", specialtiesFormat);
                    details.Add(new MessageDetail(MessageType.Warning, messageFormat, param));
                }
            }

            return details;
        }

        private static List<MessageDetail> _CheckDriverSpecialties(ICollection<Route> routes, ICollection<Order> orders)
        {
            List<MessageDetail> details = new List<MessageDetail>();

            bool checkOneRoute = (1 == routes.Count);

            string format = App.Current.FindString((checkOneRoute)? "ConstraintsCheckerDriverSpecRouteMessageFmt" :
                                                                    "ConstraintsCheckerDriverSpecRoutesMessageFmt");
            foreach (Order order in orders)
            {
                ICollection<DriverSpecialty> specialties = (checkOneRoute)?
                        _CanAccommodateOrderSpecialties(routes.First().Driver.Specialties, order.DriverSpecialties) :
                        _CanAccommodateOrderDriverSpecialties(routes, order); ;
                if (null != specialties)
                {
                    int paramCount = 1 + specialties.Count;
                    if (checkOneRoute)
                        ++paramCount;
                    AppData.DataObject[] param = new AppData.DataObject[paramCount];

                    int index = 0;
                    if (checkOneRoute)
                        param[index++] = routes.First().Driver;
                    param[index++] = order;

                    string specialtiesFormat = ViolationsHelper.GetObjectListFormat(specialties, ref index, param);
                    string messageFormat = (checkOneRoute) ? string.Format(format, "{0}", "{1}", specialtiesFormat) :
                                                             string.Format(format, "{0}", specialtiesFormat);
                    details.Add(new MessageDetail(MessageType.Warning, messageFormat, param));
                }
            }

            return details;
        }

        private static ICollection<MessageDetail> _Check(Schedule schedule, ICollection<Route> routes, ICollection<Order> orders)
        {
            List<MessageDetail> details = new List<MessageDetail>();

            string orderIsNotGeocodedFmt = App.Current.FindString("OrderIsNotGeocodedFormat");

            // get orders requirments
            string farFromRoadMatchMethod = App.Current.FindString("ManuallyEditedXYFarFromNearestRoad");
            string farViolationMessageFormat = App.Current.FindString("OrderNotFoundOnNetworkViolationMessage");

            List<Order> processedOrders = new List<Order>();
            Capacities ordersCapacities = new Capacities(App.Current.Project.CapacitiesInfo);
            foreach (Order order in orders)
            {
                if (!order.IsGeocoded)
                {
                    details.Add(new MessageDetail(MessageType.Warning, orderIsNotGeocodedFmt, order));
                    continue; // NOTE: if orders is ungeocoded skip it
                }

                if ((null != order.Address.MatchMethod) &&
                     order.Address.MatchMethod.Equals(farFromRoadMatchMethod, StringComparison.OrdinalIgnoreCase))
                {
                    details.Add(new MessageDetail(MessageType.Warning, farViolationMessageFormat, order));
                    continue; // NOTE: if orders is far from nearest road skip it
                }

                if (IsOrderLocked(order, schedule))
                    continue; // NOTE: if orders is locked skip it

                // Order capapcities should not be accounted in case order is assigned to route of 
                // the same solution and either new route to assign is the same or the best route.
                if (null != schedule.UnassignedOrders)
                {
                    if (schedule.UnassignedOrders.Contains(order))
                    {   // add these capacities to require capacities
                        // calculate total capacities
                        for (int cap = 0; cap < order.Capacities.Count; ++cap)
                            ordersCapacities[cap] += order.Capacities[cap];

                        processedOrders.Add(order);
                    }
                }
            }

            if (0 < processedOrders.Count)
            {
                OrderType orderType = OrderType.Delivery;
                bool isMixedCase = false;
                _GetOrdersDeliveryType(processedOrders, ref orderType, ref isMixedCase);

                // calculate totals of constraint properties
                bool hasRenewal = false;
                Capacities routesAvailableCapacities = new Capacities(App.Current.Project.CapacitiesInfo);
                int routesSupportedOrderCount = 0;
                foreach (Route route in routes)
                {
                    List<Order> routeOrders = new List<Order>();
                    if (null != route.Stops)
                    {
                        foreach (Stop stop in route.Stops)
                        {
                            if (StopType.Order == stop.StopType)
                            {
                                Debug.Assert(null != stop.AssociatedObject);
                                Debug.Assert(stop.AssociatedObject is Order);

                                routeOrders.Add((Order)stop.AssociatedObject);
                            }
                        }
                    }

                    if (0 < routeOrders.Count)
                    {
                        OrderType orderTypeRt = OrderType.Delivery;
                        bool isMixedCaseRt = false;
                        _GetOrdersDeliveryType(routeOrders, ref orderTypeRt, ref isMixedCaseRt);

                        if (isMixedCaseRt || (orderTypeRt != orderType))
                            isMixedCase = true;
                    }

                    if (route.IsLocked)
                        continue; // NOTE: if route is locked simply skip it

                    // calculate total orders count, subtract presently count
                    routesSupportedOrderCount += route.MaxOrders - route.OrderCount; // or route.OrderCount

                    // calculate total capacities
                    // NOTE: subtract busy capacities from available capacities
                    Debug.Assert(null != route.Vehicle);
                    Capacities vehicleCapacities = route.Vehicle.Capacities;
                    Debug.Assert(vehicleCapacities.Count == route.Capacities.Count);
                    for (int cap = 0; cap < vehicleCapacities.Count; ++cap)
                        routesAvailableCapacities[cap] += vehicleCapacities[cap] - route.Capacities[cap];

                    // check if route has renewal locations
                    hasRenewal |= (0 < route.RenewalLocations.Count);
                }

                // Max Order Count constraint violation
                details.AddRange(_CheckMaxOrderCount(routesSupportedOrderCount, processedOrders.Count, routes));

                // {Capacity} constraint violation
                if (!hasRenewal && !isMixedCase)
                    details.AddRange(_CheckCapacities(routesAvailableCapacities, ordersCapacities, routes));

                // Vehicle specialties violation
                details.AddRange(_CheckVehicleSpecialties(routes, processedOrders));

                // Driver specialties violation
                details.AddRange(_CheckDriverSpecialties(routes, processedOrders));

                // ToDo - Zones violation
            }

            return details;
        }

        #endregion // Private helpers
    }
}
