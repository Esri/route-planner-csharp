using System;
using System.Diagnostics;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides work with network coverage service client.
    /// </summary>
    internal sealed class DiscoveryClient
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url">Service url.</param>
        /// <param name="connection">ArcGIS Server Connection.</param>
        public DiscoveryClient(string url, AgsServerConnection connection)
        {
            Debug.Assert(!string.IsNullOrEmpty(url));
            Debug.Assert(connection != null);

            _serviceUrl = url;
            _connection = connection;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets geographic region name.
        /// </summary>
        /// <param name="query">Query to get region name.</param>
        /// <param name="context">Request context.</param>
        /// <returns>Discovery service response.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="query"/> or <paramref name="context"/> is null reference.
        /// </exception>
        public DiscoveryServiceResponse GetRegionName(string query,
            RequestContext context)
        {
            // Validate inputs.
            if (query == null)
                throw new ArgumentNullException("query");

            if (context == null)
                throw new ArgumentNullException("context");

            // Service should use json pre-processor because output json string
            // contains spaces in some required attributes names.
            RestService service = new RestService(
                JsonProcHelper.ReplaceSpacesToUnderscores);

            // Create HTTP appropriate request.
            HttpRequestOptions opt = _GetHTTPRequestOptions();

            // Send request and return vrp response.
            return service.SendRequest<DiscoveryServiceResponse>(context,
                _serviceUrl, query, opt);
        }

        /// <summary>
        /// Gets discovery service information.
        /// </summary>
        /// <param name="query">Query to get region name.</param>
        /// <param name="context">Request context.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="query"/> or <paramref name="context"/> is null reference.
        /// </exception>
        /// <returns>Discovery service information response.</returns>
        public DiscoveryServiceInfoResponse GetDiscoveryServiceInfo(string query,
            RequestContext context)
        {
            // Validate inputs.
            if (query == null)
                throw new ArgumentNullException("query");

            if (context == null)
                throw new ArgumentNullException("context");

            RestService service = new RestService();

            // Create HTTP appropriate request.
            HttpRequestOptions opt = _GetHTTPRequestOptions();

            // Send request and return vrp response.
            return service.SendRequest<DiscoveryServiceInfoResponse>(context,
                _serviceUrl, query, opt);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets common http requests options.
        /// </summary>
        /// <returns>Common http requests options.</returns>
        private HttpRequestOptions _GetHTTPRequestOptions()
        {
            HttpRequestOptions options = new HttpRequestOptions();
            options.Method = HttpMethod.Get;
            options.UseGZipEncoding = true;
            options.Timeout = DEFAULT_REQ_TIMEOUT;

            return options;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Timeouts (milliseconds).
        /// </summary> 
        private const int DEFAULT_REQ_TIMEOUT = 15 * 60 * 1000;

        #endregion

        #region Private fields

        /// <summary>
        /// Service url.
        /// </summary>
        private string _serviceUrl;

        /// <summary>
        /// Server connection.
        /// </summary>
        private AgsServerConnection _connection;

        #endregion
    }
}
