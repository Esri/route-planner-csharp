using System;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// Defines description information returned by dicovery service.
    /// </summary>
    [DataContract]
    internal class DiscoveryServiceInfoResponse : GPResponse
    {
        /// <summary>
        /// Full map extent.
        /// </summary>
        [DataMember(Name = "fullExtent")]
        public GPEnvelope FullExtent
        {
            get;
            set;
        }
    }
}
