namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Specifies deletion status of the feature record.
    /// </summary>
    internal enum DeletionStatus
    {
        /// <summary>
        /// The record was not deleted.
        /// </summary>
        NotDeleted = 0,

        /// <summary>
        /// The record was deleted.
        /// </summary>
        Deleted = 1,
    }
}
