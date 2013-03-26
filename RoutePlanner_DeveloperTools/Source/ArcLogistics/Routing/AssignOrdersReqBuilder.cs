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

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// AssignOrdersReqBuilder class.
    /// </summary>
    internal class AssignOrdersReqBuilder : VrpRequestBuilder
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // assignment specialty
        public static readonly Guid ASSIGNMENT_SPEC_ID = new Guid(
            "DDE0118A-8082-43e0-ABD9-BBDE95E273CC");

        #endregion constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AssignOrdersReqBuilder(SolverContext context,
            ICollection<Order> unlockedOrdersToAssign,
            ICollection<Route> unlockedTargetRoutes,
            bool setAssignmentSpec)
            : base(context)
        {
            _unlockedOrdersToAssign = unlockedOrdersToAssign;
            _unlockedTargetRoutes = unlockedTargetRoutes;
            _setAssignmentSpec = setAssignmentSpec;
        }

        #endregion constructors

        #region protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override void SetOrderAssignment(AttrDictionary attrs,
            AssignedOrder assignedOrder)
        {
            if (!_unlockedOrdersToAssign.Contains(assignedOrder.Order))
            {
                attrs.Set(NAAttribute.ROUTE_NAME, assignedOrder.Route.Id.ToString());
                attrs.Set(NAAttribute.SEQUENCE, assignedOrder.SequenceNumber);
                attrs.Set(NAAttribute.ASSIGNMENT_RULE,
                    (int)NAOrderAssignmentRule.esriNAOrderPreserveRouteAndRelativeSequence);
            }
            else
            {
                attrs.Set(NAAttribute.ROUTE_NAME, "");
                attrs.Set(NAAttribute.SEQUENCE, null);
                attrs.Set(NAAttribute.ASSIGNMENT_RULE,
                    (int)NAOrderAssignmentRule.esriNAOrderOverride);
            }
        }

        protected override List<Guid> GetRouteSpecIds(Route route)
        {
            List<Guid> specs = base.GetRouteSpecIds(route);

            if (_setAssignmentSpec && _unlockedTargetRoutes.Contains(route))
                specs.Add(ASSIGNMENT_SPEC_ID);

            return specs;
        }

        protected override List<Guid> GetOrderSpecIds(Order order)
        {
            List<Guid> specs = base.GetOrderSpecIds(order);

            if (_setAssignmentSpec && _unlockedOrdersToAssign.Contains(order))
                specs.Add(ASSIGNMENT_SPEC_ID);

            return specs;
        }

        #endregion protected methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ICollection<Order> _unlockedOrdersToAssign;
        private ICollection<Route> _unlockedTargetRoutes;
        private bool _setAssignmentSpec = false;

        #endregion private fields
    }
}
