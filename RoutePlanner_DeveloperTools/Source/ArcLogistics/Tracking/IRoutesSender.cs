using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Provides routes sending facilities.
    /// </summary>
    internal interface IRoutesSender
    {
        /// <summary>
        /// Sends specified routes to the workflow management server.
        /// </summary>
        /// <param name="routes">Routes to be send.</param>
        /// <param name="deploymentDate">Date/time to deply routes for.</param>
        void Send(IEnumerable<Route> routes, DateTime deploymentDate);
    }
}
