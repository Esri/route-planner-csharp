using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Requests
{
    /// <summary>
    /// The base class for ArcGIS Server REST API requests.
    /// </summary>
    internal class RequestBase
    {
        /// <summary>
        /// Gets a value specified response format.
        /// </summary>
        [QueryParameter(Name = "f")]
        public string ResponseFormat
        {
            get
            {
                return "json";
            }
        }
    }
}
