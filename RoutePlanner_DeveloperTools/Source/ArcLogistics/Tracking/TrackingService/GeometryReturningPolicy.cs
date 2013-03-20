namespace ESRI.ArcLogistics.Tracking.TrackingService
{
    /// <summary>
    /// Specifies geometry retrieval policy for feature layer requests.
    /// </summary>
    internal enum GeometryReturningPolicy
    {
        /// <summary>
        /// Do not return geometry with feature layer query response.
        /// </summary>
        WithoutGeometry,

        /// <summary>
        /// Return geometry with feature layer query response.
        /// </summary>
        WithGeometry,
    }
}
