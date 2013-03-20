using System.Collections.Generic;
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.DomainObjects;
using System;

namespace ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers
{
    /// <summary>
    /// Provides facilities for sending routes to the tracking server.
    /// </summary>
    internal interface ISendRoutesTask : IStateTrackingService
    {
        /// <summary>
        /// Retrieves a collection of routes which will be send with the <see cref="Execute"/>
        /// method.
        /// </summary>
        /// <returns>A collection of routes to be sent with the <see cref="Execute"/>
        /// method.</returns>
        IEnumerable<Route> QueryRoutesToBeSent();

        /// <summary>
        /// Sends routes to the tracking service.
        /// </summary>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within tracking service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to communicate with the remote service.</exception>
        void Execute();


        DateTime? GetDeploymentDate();
    }
}
