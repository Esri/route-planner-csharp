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
using System.ComponentModel;
using System.Data.Objects;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class that manages schedules of the project.
    /// </summary>
    public class ScheduleManager : DataObjectManager<Schedule>, INotifyPropertyChanged
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        internal ScheduleManager(DataObjectContext context, string entitySetName,
            SpecFields specFields,
            DataService<Route> routeDS)
            : base(context, entitySetName, specFields)
        {
            Debug.Assert(routeDS != null);
            _routeDS = routeDS;
        }

        #endregion constructors

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets active schedule.
        /// </summary>
        public Schedule ActiveSchedule
        {
            get
            {
                return _activeSchedule;
            }
            set
            {
                if (value != _activeSchedule)
                {
                    _activeSchedule = value;

                    _OnPropertyChanged(
                        new PropertyChangedEventArgs(ACTIVE_SCHEDULE_PROPERTY_NAME));
                }
            }
        }

        /// <summary>
        /// Returns overall count of schedules in the project.
        /// </summary>
        public int Count
        {
            get
            {
                return SCHEDULE_COUNT_QUERY(_Context);
            }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns schedules in specified dates range.
        /// </summary>
        /// <param name="startDate">Range's start date.</param>
        /// <param name="finishDate">Range's finish date.</param>
        /// <param name="type">Schedule type, if null then search for all types.</param>
        /// <returns>Collection that contains found schedules.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if both 
        /// <paramref name="startDate"/> and <paramref name="finishDate"/> are null.</exception>
        public IDataObjectCollection<Schedule> SearchRange(DateTime? startDate, 
            DateTime? finishDate, ScheduleType? type)
        {
            // Dates cannot be simultaneously null.
            if (startDate == null && finishDate == null)
                throw new ArgumentNullException(Properties.Messages.Error_ArgumentsNullException, 
                    new Exception());

            if (type == null)
            {
                // Find all schedules that are in a given time range.
                if (startDate == null)
                    return _SearchSchedules((DataModel.Schedules schedule) => (
                        schedule.PlannedDate >= ((DateTime)startDate).Date), false);
                else if (finishDate == null)
                    return _SearchSchedules((DataModel.Schedules schedule) => (
                        schedule.PlannedDate <= ((DateTime)finishDate).Date), false);
                else
                    return _SearchSchedules((DataModel.Schedules schedule) => (
                        schedule.PlannedDate >= ((DateTime)startDate).Date &&
                        schedule.PlannedDate <= ((DateTime)finishDate).Date), false);
            }
            else
            {
                // Find all schedules with corresponding type, that are in a given time range.
                int scheduleType = (int)type.Value;
                if (startDate == null)
                    return _SearchSchedules((DataModel.Schedules schedule) => (
                        schedule.PlannedDate <= ((DateTime)finishDate).Date &&
                        schedule.ScheduleType == scheduleType), false);
                else if (finishDate == null)
                    return _SearchSchedules((DataModel.Schedules schedule) => (
                        schedule.PlannedDate >= ((DateTime)startDate).Date &&
                        schedule.ScheduleType == scheduleType), false);
                else
                    return _SearchSchedules((DataModel.Schedules schedule) => (
                        schedule.PlannedDate >= ((DateTime)startDate).Date &&
                        schedule.PlannedDate <= ((DateTime)finishDate).Date &&
                        schedule.ScheduleType == scheduleType), false);
            }
        }

        /// <summary>
        /// Returns schedules by specified date.
        /// </summary>
        /// <param name="day">Date used to query schedules.</param>
        /// <param name="asSynchronized">Indicates whether collection remains syncronized when schedules are added or deleted to the project database.</param>
        /// <returns>Collection that contains found schedules.</returns>
        public IDataObjectCollection<Schedule> Search(DateTime day,
            bool asSynchronized)
        {
            DateTime dayStart = day.Date;
            DateTime dayEnd = day.Date.AddDays(1);

            return _SearchSchedules((DataModel.Schedules schedule) => (
                schedule.PlannedDate >= dayStart && schedule.PlannedDate < dayEnd),
                asSynchronized);
        }

        /// <summary>
        /// Returns schedules by specified date.
        /// </summary>
        /// <param name="day">Date used to query schedules.</param>
        /// <returns>Non-syncronized collection that contains selected schedules.</returns>
        public IDataObjectCollection<Schedule> Search(DateTime day)
        {
            return Search(day, false);
        }

        /// <summary>
        /// Returns list of dates from the range that contain at least one schedule with built routes.
        /// </summary>
        /// <param name="dateFrom">Start date used to query schedules.</param>
        /// <param name="dateTo">Finish date used to query schedules.</param>
        /// <returns>List of dates from the range that contain at least one schedule with built routes.</returns>
        /// <remarks>
        /// Schedule with built routes is a schedule where at least one order is assigned.
        /// </remarks>
        public IList<DateTime> SearchDaysWithBuiltRoutes(DateTime dateFrom,
            DateTime dateTo)
        {
            SqlCeCommand cmd = new SqlCeCommand(
                QUERY_DAYS_WITH_BUILT_ROUTES,
                _DataService.StoreConnection);

            cmd.Parameters.Add(new SqlCeParameter("date_from", dateFrom.Date));
            cmd.Parameters.Add(new SqlCeParameter("date_to", dateTo.Date));

            List<DateTime> dates = new List<DateTime>();
            using (SqlCeDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    dates.Add(reader.GetDateTime(0));
            }

            return dates;
        }

        /// <summary>
        /// Returns schedules count existent on specified date.
        /// </summary>
        /// <param name="plannedDate">Any date.</param>
        /// <returns>Schedules count existent on specified date.</returns>
        public int GetCount(DateTime plannedDate)
        {
            DateTime dayStart = plannedDate.Date;
            DateTime dayEnd = plannedDate.Date.AddDays(1);

            ObjectQuery<DataModel.Schedules> schedules = _DataService.CreateDefaultQuery<DataModel.Schedules>();

            return (from schedule in schedules
                    where schedule.PlannedDate >= dayStart && schedule.PlannedDate < dayEnd
                    select schedule).Count();
        }

        /// <summary>
        /// Returns all schedules available in the project.
        /// </summary>
        /// <returns>Collection of all project's schedules.</returns>
        internal IDataObjectCollection<Schedule> SearchAll(bool asSynchronized)
        {
            return _SearchAll<DataModel.Schedules>(asSynchronized);
        }

        /// <summary>
        /// Retrieves route by specified id.
        /// </summary>
        /// <returns><c>Route</c> object if search succeeded or <c>null</c> otherwise.</returns>
        public Route SearchRoute(Guid routeId)
        {
            return _routeDS.FindObjectById(routeId);
        }

        /// <summary>
        /// Updates route results in specified schedule.
        /// </summary>
        /// <param name="schedule">Schedule object.</param>
        /// <param name="routeResults">List of route results to set.</param>
        internal void SetRouteResults(Schedule schedule,
            IList<RouteResult> routeResults)
        {
            Debug.Assert(schedule != null);
            Debug.Assert(routeResults != null);

            foreach (RouteResult result in routeResults)
            {
                _RemoveStops(result.Route);
                _SetResults(result);

                if (result.Route.Schedule == null ||
                    !result.Route.Schedule.Equals(schedule))
                {
                    schedule.Routes.Add(result.Route);
                }
            }

            schedule.CanSave = true;
        }

        /// <summary>
        /// Clears route results for specified route.
        /// </summary>
        /// <param name="route">Route object.</param>
        internal void ClearRouteResults(Route route)
        {
            Debug.Assert(route != null);

            route.Cost = 0.0;
            route.StartTime = null;
            route.EndTime = null;
            route.Overtime = 0.0;
            route.TotalTime = 0.0;
            route.TotalDistance = 0.0;
            route.TravelTime = 0.0;
            route.ViolationTime = 0.0;
            route.WaitTime = 0.0;
            route.CanSave = true;
            route.Capacities = null;

            _RemoveStops(route);
        }

        #endregion public methods

        #region protected overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // APIREV: we don't need these protected method in public api

        protected override void _Remove(Schedule schedule)
        {
            Debug.Assert(schedule != null);

            // remove stops
            foreach (Route route in schedule.Routes)
                _RemoveStops(route);

            // remove routes
            schedule.Routes.Clear(); // removes objects from DB

            // remove schedule object itself
            base._Remove(schedule);
        }

        #endregion protected overrides

        #region protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected IDataObjectCollection<Schedule> _SearchSchedules(
            Expression<Func<DataModel.Schedules, bool>> whereClause,
            bool asSynchronized)
        {
            return _Search(whereClause, asSynchronized ? whereClause : null);
        }

        #endregion protected methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>
        /// event.
        /// </summary>
        /// <param name="e">The arguments for the event.</param>
        private void _OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, e);
        }

        private void _RemoveStops(Route route)
        {
            Debug.Assert(route != null);

            if (route.Stops.Count > 0)
            {
                List<Stop> stops = new List<Stop>(route.Stops);

                route.Stops.Clear();
                foreach (Stop stop in stops)
                    _RemoveObject(stop);
            }
        }

        private void _RemoveObject(DataObject obj)
        {
            ContextHelper.RemoveObject(_Context, obj);
        }

        private static void _SetResults(RouteResult result)
        {
            Debug.Assert(result != null);

            Route route = result.Route;
            route.Cost = result.Cost;
            route.StartTime = result.StartTime;
            route.EndTime = result.EndTime;
            route.Overtime = result.Overtime;
            route.TotalTime = result.TotalTime;
            route.TotalDistance = result.TotalDistance;
            route.TravelTime = result.TravelTime;
            route.ViolationTime = result.ViolationTime;
            route.WaitTime = result.WaitTime;
            route.IsLocked = result.IsLocked;
            route.Capacities = result.Capacities;

            foreach (StopData stopData in result.Stops)
                route.Stops.Add(Stop.CreateFromData(stopData));
        }

        #endregion private methods

        #region Private constants 
        
        /// <summary>
        /// Query to DataBase.
        /// </summary>
        private const string QUERY_DAYS_WITH_BUILT_ROUTES = @"select distinct sch.[PlannedDate] from [Schedules] as sch inner join [Routes] as rt on sch.[Id] = rt.[ScheduleId] inner join [Stops] as st on rt.[Id] = st.[RouteId] where sch.[PlannedDate] >= @date_from and sch.[PlannedDate] <= @date_to";

        /// <summary>
        /// Active schedule property name.
        /// </summary>
        private const string ACTIVE_SCHEDULE_PROPERTY_NAME = "ActiveSchedule";

        /// <summary>
        /// Compiled query for finding number of schedules in the project.
        /// </summary>
        private static readonly Func<DataModel.Entities, int> SCHEDULE_COUNT_QUERY =
            CompiledQuery.Compile<DataModel.Entities, int>(context => context.Schedules.Count());
        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataService<Route> _routeDS;

        /// <summary>
        /// Active schedule.
        /// </summary>
        private Schedule _activeSchedule;

        #endregion private fields
    }
}
