using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Requests
{
    /// <summary>
    /// Request for feature layer "ApplyEdits" operation.
    /// </summary>
    internal sealed class ApplyEditsRequest : RequestBase
    {
        /// <summary>
        /// Gets or sets a collection of features to be added.
        /// </summary>
        [QueryParameter(Name = "adds")]
        public string Adds { get; set; }

        /// <summary>
        /// Gets or sets a collection of features to be updated.
        /// </summary>
        [QueryParameter(Name = "updates")]
        public string Updates { get; set; }

        /// <summary>
        /// Gets or sets a collection of object IDs of features to be deleted.
        /// </summary>
        [QueryParameter(Name = "deletes")]
        public string Deletes { get; set; }
    }
}
