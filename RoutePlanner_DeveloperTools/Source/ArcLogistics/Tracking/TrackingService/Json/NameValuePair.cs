using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Represents a name/value pair which can be serialized to JSON.
    /// </summary>
    [DataContract]
    internal sealed class NameValuePair
    {
        /// <summary>
        /// Gets or sets the name component of the pair.
        /// </summary>
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value component of the pair.
        /// </summary>
        [DataMember(Name = "value")]
        public string Value { get; set; }
    }
}
