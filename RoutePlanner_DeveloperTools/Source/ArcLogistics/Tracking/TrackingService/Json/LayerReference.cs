using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to feature layer/table ID and name.
    /// </summary>
    [DataContract]
    internal sealed class LayerReference
    {
        /// <summary>
        /// Gets or sets a value uniquely identifying feature layers.
        /// </summary>
        [DataMember(Name = "id")]
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets name of the feature layer.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name
        {
            get;
            set;
        }
    }

}
