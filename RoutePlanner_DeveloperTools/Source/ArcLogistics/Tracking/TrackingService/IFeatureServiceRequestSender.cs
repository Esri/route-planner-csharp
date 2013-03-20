using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Encapsulates details of sending requests to the feature service.
    /// </summary>
    internal interface IFeatureServiceRequestSender
    {
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
        TResult SendRequest<TResult>(string url, string query, HttpRequestOptions options)
            where TResult : GPResponse;
    }
}
