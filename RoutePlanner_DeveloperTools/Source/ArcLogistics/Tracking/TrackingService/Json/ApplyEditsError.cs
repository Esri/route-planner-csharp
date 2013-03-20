using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to apply edits operation error.
    /// </summary>
    [DataContract]
    internal sealed class ApplyEditsError
    {
        /// <summary>
        /// Gets or sets a code specified the error.
        /// </summary>
        [DataMember(Name = "code")]
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets error description.
        /// </summary>
        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}
