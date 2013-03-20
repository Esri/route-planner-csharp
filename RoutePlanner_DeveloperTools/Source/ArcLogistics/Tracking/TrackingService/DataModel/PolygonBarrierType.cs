namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Specifies the type of the polygon barrier.
    /// </summary>
    internal enum PolygonBarrierType
    {
        /// <summary>
        /// The polygon barrier block travel anywhere the network intersects barrier polygon.
        /// </summary>
        BlockTravel = 0,

        /// <summary>
        /// The polygon barrier scales travelling cost anywhere the network intersects barrier
        /// polygon.
        /// </summary>
        Slowdown = 1,
    }
}
