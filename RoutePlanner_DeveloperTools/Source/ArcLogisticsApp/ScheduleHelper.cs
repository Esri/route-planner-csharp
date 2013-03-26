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

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// ScheduleHelper class.
    /// </summary>
    internal class ScheduleHelper
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get schedule by day
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public static Schedule GetCurrentScheduleByDay(DateTime day)
        {
            IDataObjectCollection<Schedule> schedules = App.Current.Project.Schedules.Search(day);

            Schedule selectedSchedule = null;
            foreach (Schedule schedule in schedules)
            {
                if (ScheduleType.Current == schedule.Type)
                {
                    selectedSchedule = schedule;
                    break;
                }
            }

            return selectedSchedule;
        }

        /// <summary>
        /// Shows does schedule have built routes
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public static bool DoesScheduleHaveBuiltRoutes(Schedule schedule)
        {
            bool isEmpty = true;
            if (null != schedule)
            {
                foreach (Route route in schedule.Routes)
                {
                    IDataObjectCollection<Stop> stops = route.Stops;
                    if ((null != stops) && (0 < stops.Count))
                    {
                        isEmpty = false;
                        break;
                    }
                }
            }

            return !isEmpty;
        }

        /// <summary>
        /// Get schedule list for dates range
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public static ICollection<Schedule> GetCurrentSchedulesByDates(DateTime dayStart, DateTime dayFinish,
                                                                       bool excludeEmpty)
        {
            List<Schedule> schedules = new List<Schedule>();

            DateTime day = dayStart;
            while (day <= dayFinish)
            {
                Schedule schedule = GetCurrentScheduleByDay(day);
                if (null != schedule)
                {
                    if (!excludeEmpty || DoesScheduleHaveBuiltRoutes(schedule))
                        schedules.Add(schedule);
                }

                day = day.AddDays(1);
            }

            return schedules.AsReadOnly();
        }

        /// <summary>
        /// Method returns common collection of assigned and unassigned orders
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        public static ICollection<Order> GetScheduleOrders(Schedule schedule)
        {
            List<Order> orders = new List<Order>();

            foreach (Order unassignedOrder in schedule.UnassignedOrders)
                orders.Add(unassignedOrder);

            foreach (Route route in schedule.Routes)
            {
                if (route.Stops.Count > 0)
                {
                    foreach (Stop stop in route.Stops)
                        if (stop.AssociatedObject is Order)
                            orders.Add((Order)stop.AssociatedObject);
                }
            }

            return orders.AsReadOnly();
        }

        /// <summary>
        /// Method checks is any schedule in date bound with any order
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        public static bool IsAnyOrderAssignedToSchedule(IList<Order> orders, DateTime targetDate)
        {
            bool ordersHaveBoundSchedule = false;

            if (orders.Count == 0)
                return false;

            IList<Schedule> schedules = App.Current.Project.Schedules.Search(targetDate);
            foreach (Schedule schedule in schedules)
            {
                if (schedule.UnassignedOrders != null)
                {
                    if (schedule.UnassignedOrders.Count == 0)
                        ordersHaveBoundSchedule = true;
                    else
                    {
                        foreach (Order order in orders)
                        {
                            if (!schedule.UnassignedOrders.Contains(order))
                            {
                                ordersHaveBoundSchedule = true;
                                break;
                            }
                        }
                    }
                }
                else if (schedule.Routes != null && DoesScheduleHaveBuiltRoutes(schedule))
                {
                    foreach (Route route in schedule.Routes)
                    {
                        if (route.Stops.Count > 0)
                        {
                            foreach (Stop stop in route.Stops)
                                if (stop.AssociatedObject is Order && orders.Contains((Order)stop.AssociatedObject))
                                {
                                    ordersHaveBoundSchedule = true;
                                    break;
                                }
                        }
                        if (ordersHaveBoundSchedule)
                            break;
                    }
                }
                if (ordersHaveBoundSchedule)
                    break;
            }

            return ordersHaveBoundSchedule;
        }

        /// <summary>
        /// Method returns collection of locked orders and appropriate schedules
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="targetDate"></param>
        /// <returns></returns>
        public static Dictionary<Schedule, Collection<Order>> GetLockedOrdersSchedules(ICollection<Order> orders, DateTime targetDate)
        {
            Dictionary<Schedule, Collection<Order>> lockedOrderSchedules = new Dictionary<Schedule, Collection<Order>>();

            IList<Schedule> schedules = App.Current.Project.Schedules.Search(targetDate);
            foreach (Schedule schedule in schedules)
            {
                if (schedule.Routes != null)
                    foreach (Route route in schedule.Routes)
                    {
                        foreach (Stop stop in route.Stops)
                            if (stop.AssociatedObject is Order && orders.Contains((Order)stop.AssociatedObject) && (stop.IsLocked || route.IsLocked))
                            {
                                if (!lockedOrderSchedules.ContainsKey(schedule))
                                    lockedOrderSchedules.Add(schedule, new Collection<Order>());
                                lockedOrderSchedules[schedule].Add((Order)stop.AssociatedObject);
                            }
                    }
            }

            return lockedOrderSchedules;
        }

        #endregion // Public methods
    }
}
