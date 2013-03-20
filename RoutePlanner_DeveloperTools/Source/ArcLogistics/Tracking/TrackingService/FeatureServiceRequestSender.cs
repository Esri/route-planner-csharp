using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services;
using System.Diagnostics;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Implements <see cref="IFeatureServiceRequestSender"/> using web requests.
    /// </summary>
    internal class FeatureServiceRequestSender : IFeatureServiceRequestSender
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the FeatureServiceRequestSender class.
        /// </summary>
        /// <param name="server">The reference to the feature services server object.</param>
        public FeatureServiceRequestSender(AgsServer server)
        {
            Debug.Assert(server != null);

            _server = server;
        }

        #endregion

        #region IFeatureServiceRequestSender members

        /// <summary>
        /// Sends requests to the specified url.
        /// </summary>
        /// <typeparam name="TResult">The type of the request result.</typeparam>
        /// <param name="url">The url to send request to.</param>
        /// <param name="query">The query to be sent.</param>
        /// <param name="options">The options specifying how request should be sent.</param>
        /// <returns>A response received for the specified query.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="url"/>,
        /// <paramref name="query"/> or <paramref name="options"/> argument is a null
        /// reference.</exception>
        /// <exception cref="ESRI.ArcLogistics.Tracking.TrackingService.TrackingServiceException">
        /// Failed to get valid response from the feature layer.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service.</exception>
        public TResult SendRequest<TResult>(string url, string query, HttpRequestOptions options)
            where TResult : GPResponse
        {
            var service = new RestService();

            var context = new RequestContext(_server.OpenConnection(), VrpRequestBuilder.JsonTypes);

            return service.SendRequest<TResult>(context, url, query, options);
        }

        #endregion

        #region private fields

        /// <summary>
        /// The reference to the feature services server object.
        /// </summary>
        private AgsServer _server;
        
        #endregion
    }
}
