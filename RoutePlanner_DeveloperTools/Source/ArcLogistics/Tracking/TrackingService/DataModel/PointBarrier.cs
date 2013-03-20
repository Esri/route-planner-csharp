using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Point barriers affect travelling through some point location.
    /// </summary>
    internal sealed class PointBarrier : BarrierBase
    {
        /// <summary>
        /// Gets or sets the barrier location.
        /// </summary>
        public Point? Location { get; set; }

        /// <summary>
        /// Gets or sets the type of the barrier.
        /// </summary>
        public PointBarrierType? Type { get; set; }

        /// <summary>
        /// Gets or sets the delay time in minutes to be used for
        /// <see cref="PointBarrierType.AddDelay"/> barriers.
        /// </summary>
        public double? DelayValue { get; set; }
    }
}
