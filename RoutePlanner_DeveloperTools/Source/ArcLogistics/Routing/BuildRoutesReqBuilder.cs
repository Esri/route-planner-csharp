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
