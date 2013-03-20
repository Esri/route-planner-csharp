using System.ServiceModel;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// A base class for WCF-based VRP/Routing services.
    /// </summary>
    internal abstract class RoutingServiceClientBase<TClient, TChannel>
        : ServiceClientWrap<TClient, TChannel>
        where TClient : ClientBase<TChannel>
        where TChannel : class
    {
        #region constructors
        public RoutingServiceClientBase(
            string url,
            AgsServerConnection connection)
            : base(url, connection)
        {
            _serviceUrl = connection.Url;
            this.PersistConnection = false;
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Obtains url to the routing service.
        /// </summary>
        /// <param name="baseUrl">A url whose path and query components
        /// will be used for making service url.</param>
        /// <returns>Url to the route service.</returns>
        protected string QueryServiceUrl(string baseUrl)
        {
            return UriHelper.Concat(_serviceUrl, baseUrl);
        }
        #endregion

        #region private fields
        /// <summary>
        /// Instance of the routing service urls provider to be used for accessing
        /// VRP/Routing services.
        /// </summary>
        private string _serviceUrl;
        #endregion
    }
}
