using System;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Contains stop status changing event information.
    /// </summary>
    internal sealed class Event : DataRecordBase
    {
        /// <summary>
        /// Gets or sets new stop status.
        /// </summary>
        public StopStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets status change date/time in UTC.
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets object ID of the stop the status was changed for.
        /// </summary>
        public long StopID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets object ID of the mobile device the event originated from.
        /// </summary>
        public long DeviceID
        {
            get;
            set;
        }
    }
}
