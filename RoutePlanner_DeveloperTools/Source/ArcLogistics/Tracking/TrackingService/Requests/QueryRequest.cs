using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Requests
{
    /// <summary>
    /// Request for feature layer "Query" operation.
    /// </summary>
    internal sealed class QueryRequest : RequestBase
    {
        /// <summary>
        /// Gets or sets a list of object IDs to return features for.
        /// </summary>
        [QueryParameter(Name = "objectIds")]
        public string ObjectIDs { get; set; }

        /// <summary>
        /// Gets or sets a where clause to filter returned features with.
        /// </summary>
        [QueryParameter(Name = "where")]
        public string WhereClause { get; set; }

        /// <summary>
        /// Gets or sets a list of fields to be returned for feature objects.
        /// </summary>
        [QueryParameter(Name = "outFields")]
        public string ReturnFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if only object IDs should be returned.
        /// </summary>
        [QueryParameter(Name = "returnIdsOnly")]
        public bool ReturnIDsOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if feature geometry should be returned.
        /// </summary>
        [QueryParameter(Name = "returnGeometry")]
        public bool ReturnGeometry { get; set; }
    }
}
