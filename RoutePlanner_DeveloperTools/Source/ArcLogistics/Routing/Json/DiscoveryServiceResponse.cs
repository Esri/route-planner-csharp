using System;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// Defines description information returned by dicovery service.
    /// </summary>
    [DataContract]
    internal class DiscoveryDescription
    {
        /// <summary>
        /// Layer Id.
        /// </summary>
        [DataMember(Name = "layerId")]
        public int LayerId
        {
            get;
            set;
        }

        /// <summary>
        /// Layer name.
        /// </summary>
        [DataMember(Name = "layerName")]
        public string LayerName
        {
            get;
            set;
        }

        /// <summary>
        /// Value.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Display field name.
        /// </summary>
        [DataMember(Name = "displayFieldName")]
        public string DisplayFieldName
        {
            get;
            set;
        }

        /// <summary>
        /// Description attributes.
        /// </summary>
        [DataMember(Name = "attributes")]
        public AttrDictionary Attributes
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Defines object returning as a result of discovert service request.
    /// </summary>
    [DataContract]
    internal class DiscoveryServiceResponse : GPResponse
    {
        /// <summary>
        /// Collection of results objects.
        /// </summary>
        [DataMember(Name = "results")]
        public DiscoveryDescription[] Results
        {
            get;
            set;
        }
    }
}
