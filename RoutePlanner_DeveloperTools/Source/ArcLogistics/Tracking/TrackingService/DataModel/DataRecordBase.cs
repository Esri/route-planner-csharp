namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// A base class for data objects stored in the feature service.
    /// </summary>
    internal abstract class DataRecordBase
    {
        /// <summary>
        /// Gets or sets a value uniquely identifying feature records in a single feature layer.
        /// </summary>
        public long ObjectID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating if the record was deleted.
        /// </summary>
        public DeletionStatus Deleted
        {
            get;
            set;
        }
    }
}
