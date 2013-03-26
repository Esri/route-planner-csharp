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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class for checking validity of new selection and getting correct items to select. 
    /// </summary>
    internal class SelectionChanger
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="optimizeAndEditPage">Parent page.</param>
        public SelectionChanger(OptimizeAndEditPage optimizeAndEditPage)
        {
            Debug.Assert(optimizeAndEditPage != null);

            _optimizeAndEditPage = optimizeAndEditPage;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Stored item to select after day changed.
        /// </summary>
        public object ItemForSettingContextAfterDayChanged
        {
            get;
            set;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Check validity of new selection and get items to select.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Items to select.</returns>
        public IEnumerable GetItemsToSelect(IEnumerable items)
        {
            Debug.Assert(items != null);

            IEnumerable itemsToSelect = null;

            bool itemsAreRoutes = _IsRoutesItems(items); // exception

            if (itemsAreRoutes)
            {
                _PrepareRoutesSelecting(items); // exception
                itemsToSelect = items;
            }
            else
            {
                itemsToSelect = _GetOrdersAndStopsToSelect(items);
            }

            return itemsToSelect;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Check validity of new selection and get items type.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Is all items are routes.</returns>
        private bool _IsRoutesItems(IEnumerable items)
        {
            Debug.Assert(items != null);

            bool itemsAreRoutes = false;
            bool itemsAreStopOrOrders = false;

            // Check that selection contains either routes or orders and stops.
            foreach (object item in items)
            {
                if (item is Order || item is Stop)
                {
                    if (itemsAreRoutes)
                        throw new ArgumentException((string)App.Current.FindResource("UnableToSelectRoutesAndOrdersMessage"));

                    itemsAreStopOrOrders = true;
                }
                else if (item is Route)
                {
                    if (itemsAreStopOrOrders)
                        throw new ArgumentException((string)App.Current.FindResource("UnableToSelectRoutesAndOrdersMessage"));

                    itemsAreRoutes = true;
                }
                else
                {
                    throw new ArgumentException((string)App.Current.FindResource("AlItemsMustBeSameTypeMessage"));
                }
            }

            Debug.Assert(itemsAreRoutes && !itemsAreStopOrOrders || !itemsAreRoutes && itemsAreStopOrOrders);

            return itemsAreRoutes;
        }

        /// <summary>
        /// Prepare route selection.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Routes to select.</returns>
        private void _PrepareRoutesSelecting(IEnumerable items)
        {
            Debug.Assert(_optimizeAndEditPage != null);
            Debug.Assert(items != null);

            // Check that all routes belong to the same scedhule.
            Schedule rtsSchedule = _GetRoutesSchedule(items); //exception

            // change current date in case schedule is assigned to a different date
            if (rtsSchedule.PlannedDate != App.Current.CurrentDate)
            {
                Debug.Assert(ItemForSettingContextAfterDayChanged == null);
                ItemForSettingContextAfterDayChanged = _GetFirstItem(items);
                App.Current.CurrentDate = rtsSchedule.PlannedDate.Value;
            }

            // Change current schedule if necessary.
            if (_optimizeAndEditPage.CurrentSchedule != rtsSchedule)
            {
                _optimizeAndEditPage.CurrentSchedule = rtsSchedule;
            }
        }

        /// <summary>
        /// Check that all routes belong to the same schedule.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Routes schedule.</returns>
        private Schedule _GetRoutesSchedule(IEnumerable items)
        {
            Debug.Assert(items != null);

            Schedule rtsSchedule = null;

            foreach (Route rt in items)
            {
                if (rt.Schedule == null)
                {
                    throw new ArgumentException((string)App.Current.FindResource("InvalidRoutesScheduleExceptionMessage"));
                }
                else if (rtsSchedule == null)
                {
                    rtsSchedule = rt.Schedule;
                }
                else
                {
                    if (rt.Schedule != rtsSchedule)
                    {
                        throw new ArgumentException((string)App.Current.FindResource("InvalidRoutesScheduleExceptionMessage"));
                    }
                }
            }

            return rtsSchedule;
        }

        /// <summary>
        /// Returns first item from collection. 
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>First item in collection.</returns>
        private object _GetFirstItem(IEnumerable items)
        {
            Debug.Assert(items != null);

            IEnumerator enumerator = items.GetEnumerator();
            enumerator.Reset();
            enumerator.MoveNext();
            Debug.Assert(enumerator.Current != null);
            return enumerator.Current;
        }

        /// <summary>
        /// Get orders to select from orders and stop collection and prepare selection.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Prders to select from orders and stop collection.</returns>
        private List<object> _GetOrdersAndStopsToSelect(IEnumerable items)
        {
            Debug.Assert(items != null);

            // Check that all orders and stops have the same PlannedDate and all stops belong to the same schedule.
            DateTime plannedDate = _GetPlannedDateOfItems(items); // exception

            // Try to get schedule by stops.
            Schedule schedule = _GetStopsSchedule(items); // exception

            // If stops absent, than try to get last used scgedule for date.
            if (schedule == null)
            {
                schedule = _optimizeAndEditPage.GetLastUsedScheduleForDate(plannedDate);
            }

            List<object> itemsWithAssociatedStops = _ChangeOrdersToStopsIfAssociated(items, schedule);

            _ChangeScheduleOrDayIfNecessary(plannedDate, schedule, itemsWithAssociatedStops[0]);

            return itemsWithAssociatedStops;
        }

        /// <summary>
        /// Check that all items belongs to the same date.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Planned date of items.</returns>
        private DateTime _GetPlannedDateOfItems(IEnumerable items)
        {
            Debug.Assert(items != null);

            DateTime? plannedDate = null;

            foreach (object item in items)
            {
                if (item is Stop)
                {
                    Stop stop = (Stop)item;
                    if (stop.Route == null || stop.Route.Schedule == null)
                    {
                        throw new ArgumentException((string)App.Current.FindResource("InvalidStopsExceptionMessage"));
                    }

                    if (!plannedDate.HasValue)
                    {
                        plannedDate = stop.Route.Schedule.PlannedDate;
                    }
                    else if (plannedDate.Value != stop.Route.Schedule.PlannedDate)
                    {
                        throw new ArgumentException((string)App.Current.FindResource("InvalidStopDateExceptionMessage"));
                    }
                }
                else if (item is Order)
                {
                    Order order = (Order)item;

                    if (!plannedDate.HasValue)
                    {
                        plannedDate = order.PlannedDate;
                    }
                    else if (plannedDate.Value != order.PlannedDate)
                    {
                        throw new ArgumentException((string)App.Current.FindResource("InvalidStopDateExceptionMessage"));
                    }
                }
            }

            return plannedDate.Value;
        }

        /// <summary>
        /// Check that all stops belongs to the same schedule.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <returns>Stops schedule.</returns>
        private Schedule _GetStopsSchedule(IEnumerable items)
        {
            Debug.Assert(items != null);

            Schedule stopsSchedule = null;

            foreach (object item in items)
            {
                Stop stop = item as Stop;

                if (stop != null)
                {
                    if (stopsSchedule == null)
                    {
                        stopsSchedule = stop.Route.Schedule;
                    }
                    else if (stopsSchedule != stop.Route.Schedule)
                    {
                        throw new ArgumentException((string)App.Current.FindResource("InvalidStopScheduleExceptionMessage"));
                    }
                }
            }

            return stopsSchedule;
        }

        /// <summary>
        /// Change all orders in items to associated stops, using schedule.
        /// </summary>
        /// <param name="items">Items to select.</param>
        /// <param name="schedule">Stops schedule. Schedule can be null in case of imported orders.</param>
        /// <returns>List of items with stops, if order associated in that schedule.</returns>
        public static List<object> _ChangeOrdersToStopsIfAssociated(IEnumerable items, Schedule schedule)
        {
            Debug.Assert(items != null);

            List<object> correctList = new List<object>();

            foreach (object item in items)
            {
                // Not orders goes to list in any case.
                if (!(item is Order) || schedule == null)
                {
                    correctList.Add(item);
                }
                else
                {
                    // Find stop, which associated to order. 
                    Stop associatedStop = null;

                    foreach (Route route in schedule.Routes)
                    {
                        foreach (Stop stop in route.Stops)
                        {
                            if (stop.AssociatedObject == item)
                            {
                                associatedStop = stop;
                                break;
                            }
                        }

                        if (associatedStop != null)
                            break;
                    }

                    // If associated stop found, than add it. Otherwise - add item.
                    if (associatedStop != null)
                    {
                        correctList.Add(associatedStop);
                    }
                    else
                    {
                        correctList.Add(item);
                    }
                }
            }

            return correctList;
        }

        /// <summary>
        /// Change schedule in case of items to select not belongs to current schedule.
        /// </summary>
        /// <param name="plannedDate">Planned date of items.</param>
        /// <param name="stopsSchedule">Stops schedule.</param>
        /// <param name="firstSelectedItem">First selected item.</param>
        private void _ChangeScheduleOrDayIfNecessary(DateTime plannedDate, Schedule stopsSchedule, object firstSelectedItem)
        {
            Debug.Assert(_optimizeAndEditPage != null);
            Debug.Assert(firstSelectedItem != null);

            ItemForSettingContextAfterDayChanged = firstSelectedItem;

            // Change current date if necessary.
            if (plannedDate != App.Current.CurrentDate)
            {
                App.Current.CurrentDate = plannedDate;
            }
            else
            {
                Debug.Assert(stopsSchedule != null);

                // Change current schedule if necessary.
                if (stopsSchedule != null && _optimizeAndEditPage.CurrentSchedule != stopsSchedule)
                {
                    _optimizeAndEditPage.CurrentSchedule = stopsSchedule;
                }
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _optimizeAndEditPage;

        /// <summary>
        /// Delegate for postponed context changing.
        /// </summary>
        private delegate void PostponedListViewContextChangingDelegate(object item);

        #endregion
    }
}
