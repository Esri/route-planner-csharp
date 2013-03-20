using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Map serialized configuration
    /// </summary>
    [DataContract]
    internal class MapDisplayConfig
    {
        public MapDisplayConfig()
        {
            OrderSelectedProp = new StringCollection();
            StopSelectedProp = new StringCollection();
        }

        [DataMember]
        public StringCollection OrderSelectedProp
        {
            get;
            set;
        }

        [DataMember]
        public StringCollection StopSelectedProp
        {
            get;
            set;
        }

        [DataMember]
        public bool TrueRoute
        {
            get;
            set;
        }

        [DataMember]
        public bool LabelingEnabled
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowBarriers
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowZones
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowLeadingStemTime
        {
            get;
            set;
        }

        [DataMember]
        public bool ShowTrailingStemTime
        {
            get;
            set;
        }

        [DataMember]
        public bool AutoZoom
        {
            get;
            set;
        }
    }
}
