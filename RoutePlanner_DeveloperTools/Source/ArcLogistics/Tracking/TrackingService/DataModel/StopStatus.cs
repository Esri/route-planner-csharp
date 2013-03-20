namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Represents stop status.
    /// </summary>
    internal enum StopStatus
    {
        /// <summary>
        /// The driver is driving to the stop.
        /// </summary>
        Started = 0,

        /// <summary>
        /// The driver arrived to the stop.
        /// </summary>
        Arrived = 1,

        /// <summary>
        /// The stop servicing was started.
        /// </summary>
        ServiceStarted = 2,

        /// <summary>
        /// The stop servicing was finished.
        /// </summary>
        ServiceFinished = 3,

        /// <summary>
        /// The stop was cancelled.
        /// </summary>
        CannotBeServiced = 4,
    }
}
