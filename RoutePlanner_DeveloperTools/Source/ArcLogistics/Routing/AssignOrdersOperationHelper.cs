using System;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Common helpers for orders assigning operations.
    /// </summary>
    internal static class AssignOrdersOperationHelper
    {
        #region public static methods
        /// <summary>
        /// Checks if orders for the specified schedule could be assigned
        /// without solving.
        /// </summary>
        /// <param name="context">Solve operation context.</param>
        /// <param name="schedule">Schedule to take orders from.</param>
        /// <returns>True if and only if orders for the specified schedule
        /// could be assigned without solving</returns>
        public static bool CanGetResultWithoutSolve(
            SolverContext context,
            Schedule schedule)
        {
            // get orders planned on schedule's date
            var dayOrders = context.Project.Orders.Search(
                (DateTime)schedule.PlannedDate);

            // check if we have at least one geocoded order
            bool haveGeocodedOrders = false;
            foreach (var order in dayOrders)
            {
                if (order.IsGeocoded)
                {
                    haveGeocodedOrders = true;
                    break;
                }
            }

            return !haveGeocodedOrders;
        }

        /// <summary>
        /// Creates orders assigning operation result without solving.
        /// </summary>
        /// <param name="context">Solve operation context.</param>
        /// <param name="requestData">Solve operation request data.</param>
        /// <param name="violations">Violations found upon request building.</param>
        /// <returns>Result of the solve for the specified request.</returns>
        public static SolveResult CreateResultWithoutSolve(
            SolverContext context,
            SolveRequestData requestData,
            IEnumerable<Violation> violations)
        {
            foreach (var route in requestData.Routes)
            {
                context.Project.Schedules.ClearRouteResults(route);
            }

            return new SolveResult(null, violations.ToArray(), false);
        }
        #endregion
    }
}
