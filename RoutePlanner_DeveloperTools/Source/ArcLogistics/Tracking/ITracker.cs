using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Provides access to the workflow management services.
    /// </summary>
    internal interface ITracker
    {
        /// <summary>
        /// Deploys specified routes to the tracking service.
        /// </summary>
        /// <param name="routes">A collection of routes to be deployed.</param>
        /// <param name="deploymentDate">The date to deploy routes for.</param>
        /// <returns>'True' if any information was sent, 'false' otherwise.</returns>
        bool Deploy(IEnumerable<Route> routes, DateTime deploymentDate);
    }
}
