using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to result of single object editing.
    /// </summary>
    [DataContract]
    internal sealed class ApplyEditsResult
    {
        /// <summary>
        /// Gets or sets an object ID of edited feature.
        /// </summary>
        [DataMember(Name = "objectId")]
        public long ObjectID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if editing was successful.
        /// </summary>
        [DataMember(Name = "success")]
        public bool Succeeded { get; set; }

        /// <summary>
        /// Gets or sets a reference to error occurred during object editing.
        /// </summary>
        [DataMember(Name = "error")]
        public ApplyEditsError Error { get; set; }
    }
}
