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
