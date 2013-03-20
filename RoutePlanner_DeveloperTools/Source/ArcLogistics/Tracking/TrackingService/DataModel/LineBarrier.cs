using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Lines barriers affect travelling through multiple edges at points of intersection of the
    /// barrier line and an edge.
    /// </summary>
    internal sealed class LineBarrier : BarrierBase
    {
        /// <summary>
        /// Gets or sets the barrier location.
        /// </summary>
        public Polyline Location { get; set; }
    }
}
