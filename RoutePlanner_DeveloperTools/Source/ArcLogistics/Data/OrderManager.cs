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
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Class that manages orders of the project.
    /// </summary>
    public class OrderManager : DataObjectManager<Order>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal OrderManager(DataObjectContext context, string entityName, SpecFields specFields)
            : base(context, entityName, specFields)
        { }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns orders planned on a specified date.
        /// </summary>
        /// <param name="plannedDate">Date used to query orders.</param>
        /// <param name="asSynchronized">Indicates whether collection remains syncronized
        /// when orders are added or deleted to the project database.</param>
        /// <returns>Syncronized collection of found orders.</returns>
        public IDataObjectCollection<Order> Search(DateTime plannedDate, bool asSynchronized)
        {
            // find all orders by date
            DateTime dayStart = plannedDate.Date;
            DateTime dayEnd = plannedDate.Date.AddDays(1);

            return _SearchOrders((DataModel.Orders order) =>
                                    (order.PlannedDate >= dayStart && order.PlannedDate < dayEnd),
                                    asSynchronized);
        }

        /// <summary>
        /// Returns orders planned on a specified date.
        /// </summary>
        /// <param name="plannedDate">Date used to query orders.</param>
        /// <returns>Non-syncronized collection of found orders.</returns>
        public IDataObjectCollection<Order> Search(DateTime plannedDate)
        {
            return Search(plannedDate, false);
        }

        /// <summary>
        /// Returns orders planned on a specified date that are still unassigned.
        /// </summary>
        /// <param name="schedule">Schedule to search.</param>
        /// <param name="asSynchronized">Indicates whether collection remains synchronized
        /// when orders are added or deleted to the project database.</param>
        /// <returns>Synchronized collection of found unassigned orders for the schedule.</returns>
        /// <remarks>
        /// Collection isn't automatically updated when some of the orders become unassigned.
        /// Repeat call of this method after any routing operation for the schedule to get updated
        /// collection of unassigned orders.
        /// </remarks>
        public IDataObjectCollection<Order> SearchUnassignedOrders(Schedule schedule,
                                                                   bool asSynchronized)
        {
            Debug.Assert(schedule != null);
            Debug.Assert(schedule.PlannedDate != null);

            var plannedDate = schedule.PlannedDate.Value;
            DateTime dayStart = plannedDate.Date;
            DateTime dayEnd = plannedDate.Date.AddDays(1);

            // filter for synchronized result collections
            var filter = Functional.MakeExpression((DataModel.Orders order) =>
                (dayStart <= order.PlannedDate && order.PlannedDate < dayEnd) &&
                !order.Stops.Any(stop =>
                    stop.Routes != null &&
                    stop.Routes.Schedules != null &&
                    stop.Routes.Schedules.Id == schedule.Id));
            var orders = UNASSIGNED_ORDERS_QUERY(_Context, schedule.Id, dayStart, dayEnd);

            return this.Query(orders, asSynchronized ? filter : null);
        }

        /// <summary>
        /// Returns orders planned on a specified date that are still unassigned.
        /// </summary>
        /// <param name="schedule">Schedule to search.</param>
        /// <returns>Non-syncronized collection of found unassigned orders for the schedule.</returns>
        public IDataObjectCollection<Order> SearchUnassignedOrders(Schedule schedule)
        {
            return SearchUnassignedOrders(schedule, false);
        }

        /// <summary>
        /// Returns list of dates from the range that contain at least one order.
        /// </summary>
        /// <param name="dateFrom">Start date used to query orders.</param>
        /// <param name="dateTo">Finish date used to query orders.</param>
        /// <returns>List of dates from the range that contain at least one order.</returns>
        public IList<DateTime> SearchDaysWithOrders(DateTime dateFrom, DateTime dateTo)
        {
            ObjectQuery<DataModel.Orders> orders =
                _DataService.CreateDefaultQuery<DataModel.Orders>();

            IQueryable query =
                (from order in orders
                    where order.PlannedDate >= dateFrom && order.PlannedDate <= dateTo
                    select order.PlannedDate).Distinct();

            var dates = new List<DateTime>();
            foreach (DateTime date in query)
                dates.Add(date);

            return dates;
        }

        /// <summary>
        /// Returns orders planned on a date from the range, which string properties contain
        /// the keyword.
        /// </summary>
        /// <param name="startDate">Start date used to query orders.</param>
        /// <param name="endDate">Finish date used to query orders.</param>
        /// <param name="keyword">The keyword to find.</param>
        /// <param name="asSynchronized">Indicates whether collection remains syncronized
        /// when orders are added or deleted to the project database.</param>
        /// <returns>Syncronized collection of found unassigned orders for the schedule.</returns>
        /// <remarks>
        /// <para>Method searches in the following order properties: <c>Name</c>, <c>Address</c>,
        /// <c>CustomProperties</c></para>
        /// <para>Collection isn't automatically updated when some of the found orders
        /// properties changes.</para>
        /// </remarks>
        public IDataObjectCollection<Order> SearchByKeyword(DateTime startDate,
                                                            DateTime endDate,
                                                            string keyword,
                                                            bool asSynchronized)
        {
            Debug.Assert(keyword != null);

            Expression<Func<DataModel.Orders, bool>> filter =
                ((DataModel.Orders order) =>
                    ((order.PlannedDate >= startDate && order.PlannedDate < endDate) &&
                     ((order.Name != null && order.Name.Contains(keyword)) ||
                      (order.FullAddress != null && order.FullAddress.Contains(keyword)) ||
                      (order.Unit != null && order.Unit.Contains(keyword)) ||
                      (order.AddressLine != null && order.AddressLine.Contains(keyword)) ||
                      (order.Locality1 != null && order.Locality1.Contains(keyword)) ||
                      (order.Locality2 != null && order.Locality2.Contains(keyword)) ||
                      (order.Locality3 != null && order.Locality3.Contains(keyword)) ||
                      (order.CountyPrefecture != null && order.CountyPrefecture.Contains(keyword)) ||
                      (order.PostalCode1 != null && order.PostalCode1.Contains(keyword)) ||
                      (order.PostalCode2 != null && order.PostalCode2.Contains(keyword)) ||
                      (order.StateProvince != null && order.StateProvince.Contains(keyword)) ||
                      (order.Country != null && order.Country.Contains(keyword)) ||
                      (order.CustomProperties != null && order.CustomProperties.Contains(keyword)))));

            var qp = new ObjectParameter[]
            {
                new ObjectParameter("start_date", startDate),
                new ObjectParameter("end_date", endDate),
                new ObjectParameter("keyword",
                                    "%" + SqlFormatHelper.EscapeLikeString(keyword, '~') + "%")
            };

            Expression<Func<DataModel.Orders, bool>> expression = asSynchronized ? filter : null;
            return _SearchOrders(ORDERS_BY_KEYWORD, qp, expression);
        }

        /// <summary>
        /// Returns orders planned on a date from the range, which string properties contain
        /// the keyword.
        /// </summary>
        /// <param name="startDate">Start date used to query orders.</param>
        /// <param name="endDate">Finish date used to query orders.</param>
        /// <param name="keyword">The keyword to find.</param>
        /// <returns>Non-syncronized collection of found unassigned orders for the schedule.</returns>
        /// <remarks>
        /// Method searches in the following order properties: <c>Name</c>, <c>Address</c>,
        /// <c>CustomProperties</c>
        /// </remarks>
        public IDataObjectCollection<Order> SearchByKeyword(DateTime startDate,
                                                            DateTime endDate,
                                                            string keyword)
        {
            return SearchByKeyword(startDate, endDate, keyword, false);
        }

        /// <summary>
        /// Returns order count on a specified date.
        /// </summary>
        /// <param name="plannedDate">Any date.</param>
        /// <returns></returns>
        public int GetCount(DateTime plannedDate)
        {
            DateTime dayStart = plannedDate.Date;
            DateTime dayEnd = plannedDate.Date.AddDays(1);

            var orders = _DataService.CreateDefaultQuery<DataModel.Orders>();
            return (from order in orders
                        where order.PlannedDate >= dayStart && order.PlannedDate < dayEnd
                        select order).Count();
        }

        /// <summary>
        /// Indicates whether order can be removed from the project database.
        /// Order cannot be removed when it is assigned to a route of at least one schedule.
        /// </summary>
        /// <param name="order">Order to check.</param>
        /// <returns>Returns <c>true</c> if order can be removed from the project database
        /// or <c>false</c> otherwise.</returns>
        public override bool CanRemove(Order order)
        {
            Debug.Assert(order != null);

            return (order.Stops.Count == 0);
        }

        #endregion // Public methods

        #region Protected members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Remove error message.
        /// </summary>
        protected override string _RemoveErrorMessage
        {
            get { return Properties.Messages.Error_OrderRemovalFailed; }
        }

        /// <summary>
        /// Returns orders where clause on a expression.
        /// </summary>
        /// <param name="whereClause">Expression for filtation collections.</param>
        /// <param name="asSynchronized">Indicates whether collection remains syncronized
        /// when orders are added or deleted to the project database.</param>
        /// <returns>Syncronized collection of found orders.</returns>
        protected IDataObjectCollection<Order> _SearchOrders(
            Expression<Func<DataModel.Orders, bool>> whereClause,
            bool asSynchronized)
        {
            Expression<Func<DataModel.Orders, bool>> expression =
                asSynchronized ? whereClause : null;
            IDataObjectCollection<Order> result = _Search(whereClause, expression);

            return result;
        }

        /// <summary>
        /// Returns orders.
        /// </summary>
        /// <param name="initialQuery">Intial query to the database.</param>
        /// <param name="queryParams">Parameters for the query.</param>
        /// <param name="filterClause">Filter clause used for syncronized data object collections.</param>
        /// <returns>Collection of founded orders.
        /// It is syncronized in <c>filterClause</c> isn't null.</returns>
        protected IDataObjectCollection<Order> _SearchOrders(string initialQuery,
            ObjectParameter[] queryParams,
            Expression<Func<DataModel.Orders, bool>> filterClause)
        {
            IDataObjectCollection<Order> result = _Search(initialQuery, queryParams, filterClause);

            return result;
        }

        #endregion // Protected members

        #region Private const
        
        /// <summary>
        /// Query to DataBase.
        /// </summary>
        private const string ORDERS_BY_KEYWORD = @"select value o from Orders as o where (o.PlannedDate >= @start_date and o.PlannedDate < @end_date) and (o.Name like @keyword escape '~' or o.FullAddress like @keyword escape '~' or o.AddressLine like @keyword escape '~' or o.Locality1 like @keyword escape '~' or o.Locality2 like @keyword escape '~' or o.Locality3 like @keyword escape '~' or o.CountyPrefecture like @keyword escape '~' or o.PostalCode1 like @keyword escape '~' or o.PostalCode2 like @keyword escape '~' or o.StateProvince like @keyword escape '~' or o.Country like @keyword escape '~' or o.Unit like @keyword escape '~' or o.CustomProperties like @keyword escape '~')";

        /// <summary>
        /// Compiled query for searching unassigned orders.
        /// </summary>
        private static readonly Func<
            DataModel.Entities,
            Guid,
            DateTime,
            DateTime,
            IQueryable<DataModel.Orders>> UNASSIGNED_ORDERS_QUERY =
                CompiledQuery.Compile<
                    DataModel.Entities,
                    Guid,
                    DateTime,
                    DateTime,
                    IQueryable<DataModel.Orders>>((context, scheduleId, startDate, endDate) =>
                        context.Orders.Where(order =>
                            (startDate <= order.PlannedDate && order.PlannedDate < endDate) &&
                            !order.Stops.Any(stop =>
                                stop.Routes != null &&
                                stop.Routes.Schedules != null &&
                                stop.Routes.Schedules.Id == scheduleId)));
        #endregion
    }
}
