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
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// BuildRoutesReqBuilder class.
    /// </summary>
    internal class BuildRoutesReqBuilder : VrpRequestBuilder
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public BuildRoutesReqBuilder(SolverContext context)
            : base(context)
        {
        }

        #endregion constructors

        #region protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override void SetOrderAssignment(AttrDictionary attrs,
            AssignedOrder assignedOrder)
        {
            if (assignedOrder.Route.IsLocked)
            {
                // order is assigned to a locked route
                attrs.Set(NAAttribute.ASSIGNMENT_RULE,
                    (int)NAOrderAssignmentRule.esriNAOrderExcludeFromSolve);
            }
            else if (assignedOrder.Stop.IsLocked)
            {
                // order is locked itself
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

        #endregion protected methods
    }
}
