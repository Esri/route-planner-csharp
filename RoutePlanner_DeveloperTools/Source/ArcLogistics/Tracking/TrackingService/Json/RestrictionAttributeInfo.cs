using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Stores information about restriction attribute.
    /// </summary>
    [DataContract]
    internal sealed class RestrictionAttributeInfo
    {
        /// <summary>
        /// Gets or sets a name of the restriction attribute.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the restriction is enabled.
        /// </summary>
        [DataMember]
        public bool IsEnabled { get; set; }
    }
}
