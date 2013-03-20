namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides access to ArcGIS Routing Service URL allowing load-balancing
    /// and fault-tolerance logic to applied for selecting actual server url.
    /// </summary>
    internal interface IRoutingServiceUrlProvider
    {
        /// <summary>
        /// Obtains url to the routing service.
        /// </summary>
        /// <returns>Url to the route service.</returns>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">An error
        /// occured during retrieval of the service url.
        /// </exception>
        string QueryServiceUrl();
    }
}
