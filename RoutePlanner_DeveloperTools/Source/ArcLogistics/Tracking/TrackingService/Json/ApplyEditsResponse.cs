using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to data from apply edits operation response.
    /// </summary>
    [DataContract]
    internal sealed class ApplyEditsResponse : GPResponse
    {
        /// <summary>
        /// Gets or sets a reference to the collection of addition results.
        /// </summary>
        [DataMember(Name = "addResults")]
        public IEnumerable<ApplyEditsResult> AddResults { get; set; }

        /// <summary>
        /// Gets or sets a reference to the collection of updating results.
        /// </summary>
        [DataMember(Name = "updateResults")]
        public IEnumerable<ApplyEditsResult> UpdateResults { get; set; }

        /// <summary>
        /// Gets or sets a reference to the collection of deletion results.
        /// </summary>
        [DataMember(Name = "deleteResults")]
        public IEnumerable<ApplyEditsResult> DeleteResults { get; set; }
    }
}
