using System.Collections.Generic;
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.DomainObjects;
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
