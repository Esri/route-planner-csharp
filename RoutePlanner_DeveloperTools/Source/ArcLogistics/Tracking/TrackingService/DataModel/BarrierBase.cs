using System;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    internal class BarrierBase : DataRecordBase
    {
        /// <summary>
        /// Gets or sets the name of the barrier.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date/time when barrier is in effect.
        /// </summary>
        public DateTime? PlannedDate { get; set; }
    }
}
