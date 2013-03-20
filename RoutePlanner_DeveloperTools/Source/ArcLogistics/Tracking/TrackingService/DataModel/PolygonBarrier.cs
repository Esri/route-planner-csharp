using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Polygon barriers affect travelling through multiple edges at points of intersection of the
    /// barrier polygon and an edge.
    /// </summary>
    internal sealed class PolygonBarrier : BarrierBase
    {
        /// <summary>
        /// Gets or sets the barrier location.
        /// </summary>
        public Polygon Location { get; set; }

        /// <summary>
        /// Gets or sets the type of the barrier.
        /// </summary>
        public PolygonBarrierType? Type { get; set; }

        /// <summary>
        /// Gets or sets the slowdown factor to be used for
        /// <see cref="PolygonBarrierType.Slowdown"/> barriers.
        /// </summary>
        public double? SlowdownValue { get; set; }
    }
}
