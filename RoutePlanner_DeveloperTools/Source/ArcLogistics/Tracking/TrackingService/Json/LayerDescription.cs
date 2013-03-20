using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to feature layer description.
    /// </summary>
    [DataContract]
    internal class LayerDescription : GPResponse
    {
        /// <summary>
        /// Gets or sets a name of the layer.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets object ID field name.
        /// </summary>
        [DataMember(Name = "objectIdField")]
        public string ObjectIDField { get; set; }
    }
}
