namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Specifies the type of the point barrier.
    /// </summary>
    internal enum PointBarrierType
    {
        /// <summary>
        /// The point barrier block travel anywhere on the network edge the barrier is located.
        /// </summary>
        BlockTravel = 0,

        /// <summary>
        /// The point barrier incurs delay upon travelling through the barrier location.
        /// </summary>
        AddDelay = 1,
    }
}
