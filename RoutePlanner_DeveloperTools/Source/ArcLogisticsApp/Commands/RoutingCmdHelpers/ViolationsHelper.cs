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
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// ViolationsHelper class
    /// </summary>
    internal class ViolationsHelper
    {
        #region Public helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static string GetObjectListFormat<T>(ICollection<T> objects, ref int index, DataObject[] param)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T obj in objects)
            {
                if (0 < sb.Length)
                    sb.Append(", ");
                sb.Append('{' + index.ToString() + '}');

                Debug.Assert(index < param.Length);
                param[index] = obj as DataObject;
                ++index;
            }

            return sb.ToString();
        }

        public static ICollection<Route> GetRouteForUnassignOrders(Schedule schedule, ICollection<Order> unassignOrders)
        {
            // get unlocked assigned orders for schedule
            ICollection<Order> unlockedAssignedOrders = _GetUnlockedAssignedOrders(schedule);

            // select unlocked assigned orders from OrdersToUnassign
            List<Order> unlockedOrdersToUnassign = new List<Order>();
            foreach (Order order in unassignOrders)
            {
                if (unlockedAssignedOrders.Contains(order))
                    unlockedOrdersToUnassign.Add(order);
            }

            // get affected routes:
            // routes planned on schedule's day where each route has at
            // least one order from UnlockedOrdersToUnassign assigned to it
            return _GetAffectedRoutes(schedule.Routes, unlockedOrdersToUnassign);
        }

        public static ICollection<Route> GetBuildRoutes(Schedule schedule)
        {
            Collection<Route> routes = new Collection<Route>();
            foreach (Route route in schedule.Routes)
            {
                if (!route.IsLocked) // exclude locked routes
                    routes.Add(route);
            }

            return routes;
        }

        public static ICollection<Route> GetRoutingCommandRoutes(Schedule schedule, AsyncOperationInfo info)
        {
            ICollection<Route> routes = null;
            switch (info.OperationType)
            {
                case SolveOperationType.BuildRoutes:
                    routes = GetBuildRoutes(schedule);
                    break;

                case SolveOperationType.SequenceRoutes:
                {
                    Debug.Assert(null != info.InputParams);
                    SequenceRoutesParams param = info.InputParams as SequenceRoutesParams;
                    routes = param.RoutesToSequence;
                    break;
                }

                case SolveOperationType.UnassignOrders:
                {
                    UnassignOrdersParams param = info.InputParams as UnassignOrdersParams;
                    routes = ViolationsHelper.GetRouteForUnassignOrders(schedule, param.OrdersToUnassign);
                    break;
                }

                case SolveOperationType.AssignOrders:
                {
                    AssignOrdersParams param = info.InputParams as AssignOrdersParams;
                    routes = param.TargetRoutes;
                    break;
                }

                case SolveOperationType.GenerateDirections:
                {
                    GenDirectionsParams param = info.InputParams as GenDirectionsParams;
                    routes = param.Routes;
                    break;
                }

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return routes;
        }

        #endregion // Public helpers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // NOTE: copied from UnassignOrdersOperation - with small chages
        private static List<Order> _GetUnlockedAssignedOrders(Schedule schedule)
        {
            List<Order> orders = new List<Order>();
            foreach (Route route in schedule.Routes)
            {
                if (!route.IsLocked)
                {
                    foreach (Stop stop in route.Stops)
                    {
                        if (stop.StopType == StopType.Order &&
                            !stop.IsLocked)
                        {
                            orders.Add(stop.AssociatedObject as Order);
                        }
                    }
                }
            }

            return orders;
        }

        // NOTE: copied from UnassignOrdersOperation
        private static ICollection<Route> _GetAffectedRoutes(ICollection<Route> dayRoutes, ICollection<Order> unlockedOrders)
        {
            List<Route> routes = new List<Route>();
            foreach (Route route in dayRoutes)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.StopType == StopType.Order)
                    {
                        Order order = stop.AssociatedObject as Order;
                        if (unlockedOrders.Contains(order))
                        {
                            routes.Add(route);
                            break;
                        }
                    }
                }
            }

            return routes;
        }

        #endregion // Private helpers
    }
}
