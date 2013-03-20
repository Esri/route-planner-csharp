using System.Diagnostics;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Implements <see cref="IRoutingServiceUrlProvider"/> using some predefined
    /// service url.
    /// </summary>
    internal class SimpleRoutingServiceUrlProvider : IRoutingServiceUrlProvider
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the SimpleRoutingServiceUrlProvider
        /// class with the url to the VRP/Routing server.
        /// </summary>
        /// <param name="serviceUrl">Url to the VRP/Routing server.</param>
        public SimpleRoutingServiceUrlProvider(string serviceUrl)
        {
            Debug.Assert(serviceUrl != null, "serviceUrl should not be null");
            _serviceUrl = serviceUrl;
        }
        #endregion

        #region IRoutingServiceUrlProvider Members
        /// <summary>
        /// Returns url passed to the constructor as the service url.
        /// </summary>
        /// <returns>Url to the route service.</returns>
        public string QueryServiceUrl()
        {
            return _serviceUrl;
        }
        #endregion

        #region private fields
        /// <summary>
        /// Stores url to the VRP/Routing server.
        /// </summary>
        private string _serviceUrl;
        #endregion
    }
}
