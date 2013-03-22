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
using System.Diagnostics;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Creates instances of Discovery services.
    /// </summary>
    internal sealed class DiscoveryServiceFactory
    {
        #region Public methods

        /// <summary>
        /// Creates a new instance of the VRP REST service client.
        /// </summary>
        /// <param name="settings">Solver settngs information.</param>
        /// <param name="servers">Available servers.</param>
        /// <param name="solveServiceValidator">Solver service validator.</param>
        /// <returns>Discovery service.</returns>
        public IDiscoveryService CreateService(SolveInfoWrap settings,
            ICollection<AgsServer> servers,
            ISolveServiceValidator solveServiceValidator)
        {
            Debug.Assert(settings != null);
            Debug.Assert(servers != null);
            Debug.Assert(solveServiceValidator != null);

            var serviceInfo = settings.DiscoveryService;

            if (serviceInfo == null)
            {
                // Creates discovery service stub to do nothing.
                return new DiscoveryServiceStub();
            }
            else
            {
                try
                {
                    // Creates full discovery service.
                    return new DiscoveryService(serviceInfo, servers, solveServiceValidator);
                }
                // If we couldn't connect to service - return stub.
                catch (InvalidOperationException ex)
                {
                    Logger.Error(ex);
                    return new DiscoveryServiceStub();
                }
            }
        }

        #endregion
    }
}
