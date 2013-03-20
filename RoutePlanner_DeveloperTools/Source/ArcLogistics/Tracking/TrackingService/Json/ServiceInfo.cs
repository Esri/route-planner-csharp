using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to feature service information.
    /// </summary>
    [DataContract]
    internal sealed class ServiceInfo : GPResponse
    {
        /// <summary>
        /// Gets or sets a reference to the collection of feature layer references.
        /// </summary>
        [DataMember(Name = "layers")]
        public IEnumerable<LayerReference> Layers
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to the collection of feature table references.
        /// </summary>
        [DataMember(Name = "tables")]
        public IEnumerable<LayerReference> Tables
        {
            get;
            set;
        }

        /// <summary>
        /// Get reference to the collection of feature tables and layers references.
        /// </summary>
        public IEnumerable<LayerReference> AllLayers
        {
            get
            {
                var result = new List<LayerReference>();
                if(Tables != null)
                    result.AddRange(Tables);
                if(Layers != null)
                    result.AddRange(Layers);
                return result;
            }
        }
    }
}
