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
