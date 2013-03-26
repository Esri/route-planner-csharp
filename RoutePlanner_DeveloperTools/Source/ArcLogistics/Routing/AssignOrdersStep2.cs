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
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// AssignOrdersStep2 class.
    /// </summary>
    internal class AssignOrdersStep2 : VrpOperation
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AssignOrdersStep2(SolverContext context,
            Schedule schedule,
            SolveRequestData reqData,
            AssignOrdersParams inputParams,
            IList<RouteResult> prevRouteResults,
            IList<Violation> prevViolations,
            SolveOptions options)
            : base(context, schedule, options)
        {
            _reqData = reqData;
            _inputParams = inputParams;
            _prevRouteResults = prevRouteResults;
            _prevViolations = prevViolations;
        }

        #endregion constructors

        #region public overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override SolveOperationType OperationType
        {
            get { return SolveOperationType.AssignOrders; } // type of origin operation
        }

        public override Object InputParams
        {
            get { return _inputParams; } // input parameters for origin operation
        }

        #endregion public overrides

        #region protected overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override SolveRequestData RequestData
        {
            get { return _reqData; }
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

        protected override IList<RouteResult> ConvertResult(
            VrpResult vrpResult,
            SubmitVrpJobRequest request)
        {
            // convert current route results
            List<RouteResult> routeResults = new List<RouteResult>(
                base.ConvertResult(vrpResult, request));

            // add route results from previous solve for those routes that
            // were not used in the last solve
            List<RouteResult> prevRouteResults = new List<RouteResult>();
            foreach (RouteResult rr in _prevRouteResults)
            {
                if (!_ContainsRoute(routeResults, rr.Route))
                    prevRouteResults.Add(rr);
            }

            routeResults.AddRange(prevRouteResults);

            return routeResults;
        }

        protected override List<Violation> GetViolations(
            VrpResult vrpResult)
        {
            List<Violation> list = base.GetViolations(vrpResult);
            list.AddRange(_prevViolations);

            return list;
        }

        protected override VrpOperation CreateOperation(SolveRequestData reqData,
            List<Violation> violations)
        {
            throw new NotSupportedException();
        }

        #endregion protected overrides

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static bool _ContainsRoute(ICollection<RouteResult> routeResults,
            Route route)
        {
            bool contains = false;
            foreach (RouteResult rr in routeResults)
            {
                if (rr.Route.Equals(route))
                {
                    contains = true;
                    break;
                }
            }

            return contains;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SolveRequestData _reqData;
        private AssignOrdersParams _inputParams;
        private IList<RouteResult> _prevRouteResults;
        private IList<Violation> _prevViolations;

        #endregion private fields
    }
}
