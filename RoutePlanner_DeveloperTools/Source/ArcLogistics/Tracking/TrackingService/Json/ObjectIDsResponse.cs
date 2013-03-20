using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to data from object IDs query response.
    /// </summary>
    [DataContract]
    internal sealed class ObjectIDsResponse : GPResponse
    {
        /// <summary>
        /// Gets or sets name of the object ID field.
        /// </summary>
        [DataMember(Name = "objectIdFieldName")]
        public string ObjectIDFieldName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to the collection of object IDs retrieved by the query.
        /// </summary>
        [DataMember(Name = "objectIds")]
        public IEnumerable<long> ObjectIDs
        {
            get;
            set;
        }
    }
}
