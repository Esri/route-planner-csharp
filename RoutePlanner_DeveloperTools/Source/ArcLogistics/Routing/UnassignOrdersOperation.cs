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
using System.Diagnostics;
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// UnassignOrdersParams class.
    /// </summary>
    internal class UnassignOrdersParams
    {
        internal UnassignOrdersParams(ICollection<Order> ordersToUnassign)
        {
            _ordersToUnassign = ordersToUnassign;
        }

        public ICollection<Order> OrdersToUnassign
        {
            get { return _ordersToUnassign; }
        }

        private ICollection<Order> _ordersToUnassign;
    }

    /// <summary>
    /// UnassignOrdersOperation class.
    /// </summary>
    internal class UnassignOrdersOperation : VrpOperation
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public UnassignOrdersOperation(SolverContext context,
            Schedule schedule,
            UnassignOrdersParams inputParams,
            SolveOptions options)
            : base(context, schedule, options)
        {
            Debug.Assert(inputParams != null);
            _inputParams = inputParams;
        }

        public UnassignOrdersOperation(SolverContext context,
            Schedule schedule,
            UnassignOrdersParams inputParams,
            SolveOptions options,
            SolveRequestData reqData,
            List<Violation> violations)
            : base(context, schedule, options)
        {
            Debug.Assert(inputParams != null);
            Debug.Assert(reqData != null);
            Debug.Assert(violations != null);

            _inputParams = inputParams;
            _reqData = reqData;
            _violations = violations;
        }

        #endregion constructors

        #region public overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override SolveOperationType OperationType
        {
            get { return SolveOperationType.UnassignOrders; }
        }

        public override Object InputParams
        {
            get { return _inputParams; }
        }

        public override bool CanGetResultWithoutSolve
        {
            get
            {
                SolveRequestData reqData = this.RequestData;
                return (reqData.Routes.Count > 0 &&
                    reqData.Orders.Count == 0);
            }
        }

        public override SolveResult CreateResultWithoutSolve()
        {
            SolveRequestData reqData = this.RequestData;
            foreach (Route route in reqData.Routes)
                SolverContext.Project.Schedules.ClearRouteResults(route);

            return new SolveResult(null, null, false);
        }

        #endregion public overrides

        #region protected overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override SolveRequestData RequestData
        {
            get
            {
                if (_reqData == null)
                    _reqData = _BuildRequestData();

                return _reqData;
            }
        }

        protected override SolveRequestOptions RequestOptions
        {
            get
            {
                SolveRequestOptions opt = base.RequestOptions;
                opt.ConvertUnassignedOrders = false;

                return opt;
            }
        }

        protected override VrpOperation CreateOperation(SolveRequestData reqData,
            List<Violation> violations)
        {
            return new UnassignOrdersOperation(base.SolverContext, base.Schedule,
                _inputParams,
                base.Options,
                reqData,
                violations);
        }

        protected override List<Violation> GetViolations(VrpResult vrpResult)
        {
            List<Violation> violations = base.GetViolations(vrpResult);

            if (_violations != null)
                violations.AddRange(_violations);

            return violations;
        }

        #endregion protected overrides

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SolveRequestData _BuildRequestData()
        {
            // get unlocked assigned orders for schedule
            List<Order> unlockedAssignedOrders = _GetUnlockedAssignedOrders();

            // select unlocked assigned orders from OrdersToUnassign
            List<Order> unlockedOrdersToUnassign = new List<Order>();
            foreach (Order order in _inputParams.OrdersToUnassign)
            {
                if (unlockedAssignedOrders.Contains(order))
                    unlockedOrdersToUnassign.Add(order);
            }

            // get affected routes:
            // routes planned on schedule's day where each route has at
            // least one order from UnlockedOrdersToUnassign assigned to it
            List<Route> affectedRoutes = _GetAffectedRoutes(
                Schedule.Routes,
                unlockedOrdersToUnassign);

            // get orders to convert:
            // orders assigned to affected routes except those from
            // UnlockedOrdersToUnassign
            List<Order> ordersToConvert = _GetOrdersToConvert(
                affectedRoutes,
                unlockedOrdersToUnassign);

            // get barriers planned on schedule's date
            ICollection<Barrier> barriers = SolverContext.Project.Barriers.Search(
                (DateTime)Schedule.PlannedDate);

            SolveRequestData reqData = new SolveRequestData();
            reqData.Routes = affectedRoutes;
            reqData.Orders = ordersToConvert;
            reqData.Barriers = barriers;

            return reqData;
        }

        private List<Order> _GetUnlockedAssignedOrders()
        {
            List<Order> orders = new List<Order>();
            foreach (Route route in Schedule.Routes)
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

        private List<Route> _GetAffectedRoutes(
            ICollection<Route> dayRoutes,
            ICollection<Order> unlockedOrders)
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

        private List<Order> _GetOrdersToConvert(
            ICollection<Route> affectedRoutes,
            ICollection<Order> unlockedOrders)
        {
            List<Order> orders = new List<Order>();
            foreach (Route route in affectedRoutes)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.StopType == StopType.Order)
                    {
                        Order order = stop.AssociatedObject as Order;
                        if (!unlockedOrders.Contains(order))
                            orders.Add(order); // TODO: (?) check duplicates
                    }
                }
            }

            return orders;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private UnassignOrdersParams _inputParams;
        private SolveRequestData _reqData;
        private List<Violation> _violations;

        #endregion private fields
    }
}
