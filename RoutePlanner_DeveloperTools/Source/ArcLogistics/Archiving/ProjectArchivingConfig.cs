using System;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Archiving
{
    /// <summary>
    /// ProjectArchivingConfig class.
    /// </summary>
    [DataContract]
    internal class ProjectArchivingConfig
    {
        [DataMember]
        public bool IsArchive
        {
            get;
            set;
        }

        [DataMember]
        public bool IsAutoArchivingEnabled
        {
            get;
            set;
        }

        [DataMember]
        public DateTime? LastArchivingDate
        {
            get;
            set;
        }

        [DataMember]
        public int AutoArchivingPeriod
        {
            get;
            set;
        }

        [DataMember]
        public int TimeDomain
        {
            get;
            set;
        }
    }
}
