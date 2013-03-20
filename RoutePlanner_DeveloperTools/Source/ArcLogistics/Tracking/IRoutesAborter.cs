using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Aborts routes sent to the workflow management server.
    /// </summary>
    internal interface IRoutesAborter
    {
        /// <summary>
        /// Aborts routes from the specified schedule using current application
        /// tracker component.
        /// </summary>
        /// <param name="routes">The reference to the collection of sent routes.</param>
        /// <param name="deploymentDate">Date/time to abort routes for.</param>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        void Abort(IEnumerable<Route> routes, DateTime deploymentDate);
    }
}
