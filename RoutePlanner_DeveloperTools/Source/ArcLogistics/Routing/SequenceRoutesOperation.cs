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
    /// SequenceRoutesParams class.
    /// </summary>
    internal class SequenceRoutesParams
    {
        internal SequenceRoutesParams(ICollection<Route> routesToSequence)
        {
            _routesToSequence = routesToSequence;
        }

        public ICollection<Route> RoutesToSequence
        {
            get { return _routesToSequence; }
        }

        private ICollection<Route> _routesToSequence;
    }

    /// <summary>
    /// SequenceRoutesOperation class.
    /// </summary>
    internal class SequenceRoutesOperation : VrpOperation
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public SequenceRoutesOperation(SolverContext context,
            Schedule schedule,
            SequenceRoutesParams inputParams,
            SolveOptions options)
            : base(context, schedule, options)
        {
            Debug.Assert(inputParams != null);
            _inputParams = inputParams;
        }

        public SequenceRoutesOperation(SolverContext context,
            Schedule schedule,
            SequenceRoutesParams inputParams,
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
            get { return SolveOperationType.SequenceRoutes; }
        }

        public override Object InputParams
        {
            get { return _inputParams; }
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

        protected override VrpRequestBuilder RequestBuilder
        {
            get
            {
                return new SequenceRoutesReqBuilder(SolverContext);
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
            return new SequenceRoutesOperation(base.SolverContext, base.Schedule,
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
            List<Route> routes = new List<Route>();
            List<Order> orders = new List<Order>();

            // get unlocked routes from RoutesToSequence and all orders
            // assigned to converting routes
            foreach (Route route in _inputParams.RoutesToSequence)
            {
                if (route.Schedule.Equals(Schedule))
                {
                    if (!route.IsLocked)
                    {
                        routes.Add(route);
                        orders.AddRange(SolveHelper.GetAssignedOrders(route));
                    }
                }
                else
                {
                    // route does not have corresponding route results that
                    // belongs to schedule
                    throw new RouteException(String.Format(
                        Properties.Messages.Error_InvalidRouteToSequence,
                        route.Id));
                }
            }

            // check if we have unlocked routes
            if (routes.Count == 0)
                throw new RouteException(Properties.Messages.Error_InvalidRoutesToSequenceCount);

            // get barriers planned on schedule's date
            ICollection<Barrier> barriers = SolverContext.Project.Barriers.Search(
                (DateTime)Schedule.PlannedDate);

            SolveRequestData reqData = new SolveRequestData();
            reqData.Routes = routes;
            reqData.Orders = orders;
            reqData.Barriers = barriers;

            return reqData;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SequenceRoutesParams _inputParams;
        private SolveRequestData _reqData;
        private List<Violation> _violations;

        #endregion private fields
    }
}
