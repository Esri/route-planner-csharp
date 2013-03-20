using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// The base class for ArcGIS Server REST API requests.
    /// </summary>
    internal class RequestBase
    {
        #region Public properties

        /// <summary>
        /// Gets a value specified response format.
        /// </summary>
        [QueryParameter(Name = "f")]
        public string ResponseFormat
        {
            get
            {
                return JSON_TYPE;
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Json type name.
        /// </summary>
        private const string JSON_TYPE = "json";

        #endregion
    }
}
