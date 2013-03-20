using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// SequenceRoutesReqBuilder class.
    /// </summary>
    internal class SequenceRoutesReqBuilder : VrpRequestBuilder
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public SequenceRoutesReqBuilder(SolverContext context)
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
            if (assignedOrder.Route.IsLocked ||
                assignedOrder.Stop.IsLocked)
            {
                attrs.Set(NAAttribute.ROUTE_NAME, assignedOrder.Route.Id.ToString());
                attrs.Set(NAAttribute.SEQUENCE, assignedOrder.SequenceNumber);
                attrs.Set(NAAttribute.ASSIGNMENT_RULE,
                    (int)NAOrderAssignmentRule.esriNAOrderPreserveRouteAndRelativeSequence);
            }
            else
            {
                attrs.Set(NAAttribute.ROUTE_NAME, assignedOrder.Route.Id.ToString());
                attrs.Set(NAAttribute.SEQUENCE, assignedOrder.SequenceNumber);
                attrs.Set(NAAttribute.ASSIGNMENT_RULE,
                    (int)NAOrderAssignmentRule.esriNAOrderPreserveRoute);
            }
        }
        
        /// <summary>
        /// Method converts breaks from routes to GPRecordSet.
        /// </summary>
        /// <param name="routes">Routes collection to get breaks.</param>
        /// <returns>Breaks GPRecordSet.</returns>
        protected override GPRecordSet ConvertBreaks(ICollection<Route> routes)
        {
            var converter = CreateBreaksConverter();

            // We need to add "sequence" attribute to record set.
            return converter.ConvertBreaks(routes, true);
        }

        #endregion protected methods
    }
}
