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
using System.Linq;
using System.Text;
using System.Windows;
using ESRI.ArcLogistics.App.Pages;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App
{
    internal enum BuildStatus
    {
        Empty,
        BuildingRoutes,
        RoutesBuilt,
        HasUnassignedOrders
    }

    /// <summary>
    /// Define day status and day tooltip text
    /// </summary>
    internal struct DayStatus
    {
        public DayStatus(BuildStatus status, string text)
        {
            _status = status;
            _text = text;
        }

        public BuildStatus Status
        {
            get
            {
                return _status;
            }
        }

        public string Text
        {
            get
            {
                return _text;
            }
        }

        private BuildStatus _status;
        private string _text;
    }

    /// <summary>
    /// Class creates and updates collection of "routed" days when necessary
    /// </summary>
    internal class DayStatusesManager
    {
        #region Static Properties

        static DayStatusesManager()
        {
            _routedDaysManager = new DayStatusesManager();
        }

        public static DayStatusesManager Instance
        {
            get
            {
                return _routedDaysManager;
            }
        }

        #endregion

        #region Constructors

        private DayStatusesManager()
        {
            _optimizeAndEditPage = (OptimizeAndEditPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            _optimizeAndEditPage.CurrentScheduleChanged += new EventHandler(optimizeAndEditPage_CurrentScheduleChanged);

            App.Current.ProjectLoaded += new EventHandler(Current_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(Current_ProjectClosing);

            if (App.Current.Project != null)
                App.Current.Project.SavingChanges += new ESRI.ArcLogistics.Data.SavingChangesEventHandler(Project_SavingChanges);

            App.Current.Solver.AsyncSolveStarted += new AsyncSolveStartedEventHandler(OnAsyncSolveStarted);
            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(OnAsyncSolveCompleted);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Raises when collection of routed days changed
        /// </summary>
        public event EventHandler DayStatusesChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Dicrionary contains day statuses
        /// </summary>
        public Dictionary<DateTime, DayStatus> DayStatuses
        {
            get
            {
                return _dayStatuses;
            }
            protected set
            {
                _dayStatuses = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method sets day's status to HasUnassignedOrders or Empty if no BuiltRoutes and Routing operations on this day
        /// </summary>
        /// <param name="date"></param>
        public void UpdateDayStatus(DateTime date)
        {
            if (_dayStatuses[date].Status == BuildStatus.RoutesBuilt || _dayStatuses[date].Status == BuildStatus.BuildingRoutes)
                return;
            else if (_dayStatuses[date].Status == BuildStatus.HasUnassignedOrders || _dayStatuses[date].Status == BuildStatus.Empty)
            {
                if (App.Current.Project.Orders.GetCount(date) > 0)
                    _dayStatuses[date] = new DayStatus(BuildStatus.HasUnassignedOrders, null);
                else
                    _dayStatuses[date] = new DayStatus(BuildStatus.Empty, null);
            }

            // raise event
            if (DayStatusesChanged != null)
                DayStatusesChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Method inits day status for days in range that were not initialized yet.
        /// </summary>
        /// <param name="newStartDate"></param>
        /// <param name="newEndDate"></param>
        public void InitDayStatuses(DateTime newStartDate, DateTime newEndDate)
        {
            if (App.Current.Project == null)
                return;

            bool dayStatusesChanged = false;
            List<KeyValuePair<DateTime, DayStatus>> listOfDayStatuses = _dayStatuses.ToList<KeyValuePair<DateTime, DayStatus>>();

            if (listOfDayStatuses.Count != 0)
            {
                listOfDayStatuses.Sort(new DayStatusesComparer());
                DateTime oldStartDate = listOfDayStatuses[0].Key;
                DateTime oldEndDate = listOfDayStatuses[listOfDayStatuses.Count - 1].Key;

                if (newStartDate < oldStartDate)
                {
                    _CreateNewDayStatuses(newStartDate, oldStartDate);
                    dayStatusesChanged = true;
                }
                if (newEndDate > oldEndDate)
                {
                    _CreateNewDayStatuses(oldEndDate, newEndDate);
                    dayStatusesChanged = true;
                }
            }
            else
            {
                _CreateNewDayStatuses(newStartDate, newEndDate);
                dayStatusesChanged = true;
            }

            if (dayStatusesChanged && DayStatusesChanged != null)
                DayStatusesChanged(null, EventArgs.Empty);
        }
        
        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Occurs when saves completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Project_SavingChanges(object sender, ESRI.ArcLogistics.Data.SavingChangesEventArgs e)
        {
            IList<Order> orders;
            bool statusesChanged = false;

            // if orders was added
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                orders = _GetOrdersFromObjectsCollection(e.AddedItems);
                if (orders.Count > 0)
                {
                    foreach (Order order in orders)
                    {
                        DateTime date = (DateTime)order.PlannedDate;
                        if (_dayStatuses.ContainsKey(date))
                        {
                            if (_dayStatuses[date].Status != BuildStatus.RoutesBuilt)
                            {
                                _dayStatuses[date] = new DayStatus(BuildStatus.HasUnassignedOrders, null);
                                statusesChanged = true;
                            }
                        }
                        else
                        {
                            _dayStatuses[date] = new DayStatus(BuildStatus.HasUnassignedOrders, null);
                            statusesChanged = true;
                        }
                    }
                }
            }
            // if orders was removed
            else if (e.DeletedItems != null && e.DeletedItems.Count > 0)
            {
                orders = _GetOrdersFromObjectsCollection(e.DeletedItems);
                if (orders.Count > 0)
                {
                    Dictionary<DateTime, IList<Order>> deletedOrders = _GetDatesWithDeletedOrders(orders);

                    foreach (KeyValuePair<DateTime, IList<Order>> val in deletedOrders)
                    {
                        DateTime date = (DateTime)val.Key;
                        Debug.Assert(_dayStatuses.ContainsKey(date));

                        if (_dayStatuses[date].Status == BuildStatus.HasUnassignedOrders)
                        {
                            int unassignedOrdersCount = App.Current.Project.Orders.GetCount(date);
                            if (unassignedOrdersCount == ((IList<Order>)val.Value).Count)
                            {
                                _dayStatuses[date] = new DayStatus(BuildStatus.Empty, null);
                                statusesChanged = true;
                            }
                        }
                    }
                }
            }

            if (statusesChanged && DayStatusesChanged != null)
                DayStatusesChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when any solve operation started
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAsyncSolveStarted(object sender, AsyncSolveStartedEventArgs e)
        {
            DateTime plannedDate = new DateTime();
            AsyncOperationInfo info = null;
            Schedule schedule = null;
            if (App.Current.Solver.GetAsyncOperationInfo(e.OperationId, out info))
                schedule = info.Schedule;

            if (schedule != null)
                plannedDate = (DateTime)schedule.PlannedDate;

            _dayStatuses[plannedDate] = new DayStatus(BuildStatus.BuildingRoutes, null);

            if (DayStatusesChanged != null)
                DayStatusesChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when solve operation completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            AsyncOperationInfo info = null;
            Schedule schedule = null;
            if (App.Current.Solver.GetAsyncOperationInfo(e.OperationId, out info))
                schedule = info.Schedule;

            if (schedule != null && !_pendingDatesToUpdate.Contains((DateTime)schedule.PlannedDate))
            {
                _pendingDatesToUpdate.Add((DateTime)schedule.PlannedDate);
                _optimizeAndEditPage.Dispatcher.BeginInvoke(new DayStatusDelegate(_UpdateDayStatus), System.Windows.Threading.DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// Occurs when current schedule changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void optimizeAndEditPage_CurrentScheduleChanged(object sender, EventArgs e)
        {
            DateTime scheduleDate = (DateTime)_optimizeAndEditPage.CurrentSchedule.PlannedDate;

            if (_dayStatuses.ContainsKey(scheduleDate) && _dayStatuses[scheduleDate].Status == BuildStatus.BuildingRoutes)
                return;
            if (!_pendingDatesToUpdate.Contains(scheduleDate))
            {
                _pendingDatesToUpdate.Add(scheduleDate);
                _optimizeAndEditPage.Dispatcher.BeginInvoke(new DayStatusDelegate(_UpdateDayStatus), System.Windows.Threading.DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// Occurs when project loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_ProjectLoaded(object sender, EventArgs e)
        {
            _dayStatuses.Clear();
            App.Current.Project.SavingChanges += new ESRI.ArcLogistics.Data.SavingChangesEventHandler(Project_SavingChanges);
        }

        private void Current_ProjectClosing(object sender, EventArgs e)
        {
            App.Current.Project.SavingChanges -= Project_SavingChanges;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method returns collection of dates by colection of orders
        /// </summary>
        /// <param name="addedOrders"></param>
        /// <returns></returns>
        private Dictionary<DateTime, IList<Order>> _GetDatesWithDeletedOrders(IList<Order> orders)
        {
            Dictionary<DateTime, IList<Order>> deletedOrders = new Dictionary<DateTime, IList<Order>>();

            foreach (Order order in orders)
            {
                if (!deletedOrders.ContainsKey((DateTime)order.PlannedDate))
                    deletedOrders[(DateTime)order.PlannedDate] = new List<Order>();

                deletedOrders[(DateTime)order.PlannedDate].Add(order); 
            }

            return deletedOrders;
        }

        /// <summary>
        /// Method returns collection of orders which contains in input collection
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        private IList<Order> _GetOrdersFromObjectsCollection(IList<ESRI.ArcLogistics.Data.DataObject> collection)
        {
            List<Order> ordersCollection = new List<Order>();

            foreach (ESRI.ArcLogistics.Data.DataObject obj in collection)
            {
                if (obj is Order)
                    ordersCollection.Add((Order)obj);
            }

            return ordersCollection;
        }

        private delegate void DayStatusDelegate();

        /// <summary>
        /// Method upfates day status.
        /// </summary>
        private void _UpdateDayStatus()
        {
            WorkingStatusHelper.SetBusy(null);

            if (_pendingDatesToUpdate.Count == 0)
                return;

            DateTime scheduleDate = _pendingDatesToUpdate[0];
            _pendingDatesToUpdate.RemoveAt(0);

            BuildStatus status = new BuildStatus();

            // If any solve operation is running on current date - define status as "Building".
            if (App.Current.Solver.GetAsyncOperations(scheduleDate).Count != 0)
                status = BuildStatus.BuildingRoutes;

            // If no solve operations running - define completed status by orders and routes collections of each schedule.
            else
                status = _DefineCompletedStatus(scheduleDate);

            DayStatus dayStatus = new DayStatus(status, null);

            _dayStatuses[scheduleDate] = dayStatus;

            // Raise event about status changed.
            if (DayStatusesChanged != null)
                DayStatusesChanged(null, EventArgs.Empty);

            WorkingStatusHelper.SetReleased();
        }

        /// <summary>
        /// Defines completed status  
        /// </summary>
        /// <param name="scheduleDate">date where status should be defined.</param>
        /// <returns>Empty, RoutesBuild or HasUnassignedOrders status.</returns>
        private BuildStatus _DefineCompletedStatus(DateTime scheduleDate)
        {
            // Define default "Empty" status.
            BuildStatus status = new BuildStatus();

            // Get collection of schedules for necessary date.
            ICollection<Schedule> schedules = App.Current.Project.Schedules.Search(scheduleDate);

            foreach (Schedule schedule in schedules)
            {
                // Define "RoutesBuild" status.
                if (ScheduleHelper.DoesScheduleHaveBuiltRoutes(schedule))
                {
                    status = BuildStatus.RoutesBuilt;
                    break;
                }
                // Define "HasUnassignedOrders" status.
                else if (schedule.UnassignedOrders != null && schedule.UnassignedOrders.Count > 0)
                {
                    status = BuildStatus.HasUnassignedOrders;
                    break;
                }
            }

            return status;
        }

        /// <summary>
        /// Method call to schedule manager about routed days in current range and adds received dates to dictionary
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        private void _CreateNewDayStatuses(DateTime startDate, DateTime endDate)
        {
            List<DateTime> datesRange = _GetDaysFromRange(startDate, endDate);
            IList<DateTime> newRoutedDays = App.Current.Project.Schedules.SearchDaysWithBuiltRoutes(startDate, endDate);
            IList<DateTime> newDaysWithUnassignedOrders = App.Current.Project.Orders.SearchDaysWithOrders(startDate, endDate);

            List<KeyValuePair<DateTime, DayStatus>> listOfDayStatuses = _dayStatuses.ToList<KeyValuePair<DateTime, DayStatus>>();
            listOfDayStatuses.Sort(new DayStatusesComparer());

            foreach (DateTime date in datesRange)
            {
                if (newRoutedDays.Contains(date))
                {
                    _dayStatuses[date] = new DayStatus(BuildStatus.RoutesBuilt, null);
                }
                else if (newDaysWithUnassignedOrders.Contains(date))
                {
                    _dayStatuses[date] = new DayStatus(BuildStatus.HasUnassignedOrders, null);
                }
                else
                {
                    _dayStatuses[date] = new DayStatus(BuildStatus.Empty, null);
                }
            }
        }

        /// <summary>
        /// Method gets collection of dates in range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private List<DateTime> _GetDaysFromRange(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dates = new List<DateTime>();

            DateTime currentDay = new DateTime(startDate.Year, startDate.Month, startDate.Day);

            while (currentDay <= endDate)
            {
                dates.Add(currentDay);
                currentDay = currentDay.AddDays(1);
            }

            return dates;
        }

        #endregion

        #region Private Fields

        private static DayStatusesManager _routedDaysManager;
        private Dictionary<DateTime, DayStatus> _dayStatuses = new Dictionary<DateTime, DayStatus>();
        private OptimizeAndEditPage _optimizeAndEditPage;
        private List<DateTime> _pendingDatesToUpdate = new List<DateTime>();

        #endregion
    }

    /// <summary>
    /// Comparer for sorting DayStatuses in ascending Date values.
    /// </summary>
    internal class DayStatusesComparer : IComparer<KeyValuePair<DateTime, DayStatus>>
    {
        #region IComparer<Stop> Members

        int IComparer<KeyValuePair<DateTime, DayStatus>>.Compare(KeyValuePair<DateTime, DayStatus> x, KeyValuePair<DateTime, DayStatus> y)
        {
            if (x.Key > y.Key)
                return 1;
            else if (x.Key < y.Key)
                return -1;
            else
                return 0;
        }

        #endregion
    }
}
