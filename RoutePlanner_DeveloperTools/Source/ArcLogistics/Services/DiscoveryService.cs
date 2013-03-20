using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services.Serialization;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides work with discovery service.
    /// </summary>
    internal class DiscoveryService : IDiscoveryService
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceSettings">Discovery service settings.</param>
        /// <param name="servers">Available arcgis-servers.</param>
        /// <param name="solveServiceValidator">Services validator.</param>
        public DiscoveryService(DiscoveryServiceInfo serviceSettings,
            ICollection<AgsServer> servers,
            ISolveServiceValidator solveServiceValidator)
        {
            Debug.Assert(serviceSettings != null);
            Debug.Assert(servers != null);
            Debug.Assert(solveServiceValidator != null);

            // Validate services config.
            solveServiceValidator.Validate(serviceSettings);

            _discoveryServiceConfig = serviceSettings;

            _server = ServiceHelper.FindServerByName(serviceSettings.ServerName, servers);

            // Initialize service if server was found successfully.
            if (_server != null)
                Initialize();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes discovery service.
        /// </summary>
        public void Initialize()
        {
            _client = new DiscoveryClient(_discoveryServiceConfig.RestUrl,
                _server.OpenConnection());
        }

        /// <summary>
        /// Validates directory server state.
        /// </summary>
        public void ValidateServerState()
        {
            ServiceHelper.ValidateServerState(_server);
        }

        /// <summary>
        /// Gets full map extent from discovery service.
        /// </summary>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Full map extent.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="knownTypes"/> is null reference.</exception>
        public GPEnvelope GetFullMapExtent(IEnumerable<Type> knownTypes)
        {
            // Validate input.
            if (knownTypes == null)
                throw new ArgumentNullException("knownTypes");

            var context = new RequestContext(_server.OpenConnection(), knownTypes);

            // Get appropriate query to discovery service.
            string query = RestHelper.BuildQueryString(new RequestBase(),
                DiscoveryRequestBuilder.JsonTypes, false);

            // Get response from service.
            var response = _client.GetDiscoveryServiceInfo(query, context);

            if (response == null || response.FullExtent == null)
            {
                throw new RouteException(
                    Properties.Messages.Error_GetDiscoveryServiceConfigFailed);
            }

            return response.FullExtent;
        }

        /// <summary>
        /// Gets geographic region name.
        /// </summary>
        /// <param name="request">Discovery request.</param>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Region name if successfully found.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="request"/> or <paramref name="knownTypes"/> is null reference.
        /// </exception>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RouteException">
        /// Result region name is null or empty string.</exception>
        public string GetRegionName(SubmitDiscoveryRequest request, IEnumerable<Type> knownTypes)
        {
            // Validate inputs.
            if (request == null)
                throw new ArgumentNullException("request");

            if (knownTypes == null)
                throw new ArgumentNullException("knownTypes");

            // Create appropriate service client to make requests.
            string baseUrl = UriHelper.Concat(_discoveryServiceConfig.RestUrl,
                QUERY_OBJ_IDENTIFY);
            var client = new DiscoveryClient(baseUrl, _server.OpenConnection());

            // Get query and context.
            string query = RestHelper.BuildQueryString(request,
                DiscoveryRequestBuilder.JsonTypes, false);
            var context = new RequestContext(_server.OpenConnection(), knownTypes);

            // Get response from service.
            var response = client.GetRegionName(query, context);

            // Try to get region name from response.
            string regionName = string.Empty;

            if (response != null && response.Results != null)
            {
                // Get any first description object from the collection.
                var parameterObject = response.Results.FirstOrDefault();

                if (parameterObject != null && parameterObject.Attributes != null)
                {
                    // Get region name from anyone attributes collection.
                    parameterObject.Attributes.TryGet(REGION_MEMBER_NAME, out regionName);
                }
            }

            if (string.IsNullOrEmpty(regionName))
                throw new RouteException(Properties.Messages.Error_InvalidRegionInformation);

            return regionName;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// URL query objects.
        /// </summary>
        private const string QUERY_OBJ_IDENTIFY = "identify";

        /// <summary>
        /// Name of data memeber containing region name.
        /// </summary>
        private const string REGION_MEMBER_NAME = "Region_Name";

        #endregion

        #region Private fields

        /// <summary>
        /// Client for Network Coverage Service.
        /// </summary>
        private DiscoveryClient _client;

        /// <summary>
        /// ArcGIS server instance hosting discovery service.
        /// </summary>
        private AgsServer _server;

        /// <summary>
        /// Discovery service configuration.
        /// </summary>
        private DiscoveryServiceInfo _discoveryServiceConfig;

        #endregion
    }
}
