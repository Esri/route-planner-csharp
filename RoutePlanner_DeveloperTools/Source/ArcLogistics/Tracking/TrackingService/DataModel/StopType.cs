namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Specifies the type of the stop stored in the feature service.
    /// </summary>
    internal enum StopType
    {
        /// <summary>
        /// The stop is a start location.
        /// </summary>
        StartLocation = 0,

        /// <summary>
        /// The stop is an order.
        /// </summary>
        Order = 1,

        /// <summary>
        /// The stop is a lunch break.
        /// </summary>
        Break = 2,

        /// <summary>
        /// The stop is a renewal location.
        /// </summary>
        RenewalLocation = 3,

        /// <summary>
        /// The stops is an ending location.
        /// </summary>
        FinishLocation = 4,
    }
}
