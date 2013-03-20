using System;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Provides access to global setting. 
    /// </summary>
    [DataContract]
    internal sealed class Setting : DataRecordBase
    {
        /// <summary>
        /// Gets or sets a value identifying specific setting.
        /// </summary>
        [DataMember]
        public Guid KeyID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value the date to use route settings for.
        /// </summary>
        [DataMember]
        public DateTime? PlannedDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value specifying setting value part stored in the current object.
        /// </summary>
        [DataMember]
        public int PartIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets setting value.
        /// </summary>
        [DataMember]
        public string Value
        {
            get;
            set;
        }
    }
}
