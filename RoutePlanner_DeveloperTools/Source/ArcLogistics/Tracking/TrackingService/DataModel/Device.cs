using System;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Stores information about mobile device.
    /// </summary>
    internal sealed class Device : DataRecordBase
    {
        /// <summary>
        /// Gets or sets user friendly mobile device name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets UTC date/time of last location change.
        /// </summary>
        public DateTime? Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets device location.
        /// </summary>
        public Point? Location
        {
            get;
            set;
        }
    }
}
