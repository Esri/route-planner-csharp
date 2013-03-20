using System.Collections.Generic;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Provides access to data from feature layer query response.
    /// </summary>
    [DataContract]
    internal class FeatureRecordSetLayer : GPResponse
    {
        /// <summary>
        /// Gets or sets name of the object ID field.
        /// </summary>
        [DataMember(Name = "objectIdFieldName")]
        public string ObjectIDFieldName { get; set; }

        /// <summary>
        /// Gets or sets type of the geometry used by the feature layer.
        /// </summary>
        [DataMember(Name = "geometryType")]
        public string GeometryType { get; set; }

        /// <summary>
        /// Gets or sets a spatial reference used by the feature layer.
        /// </summary>
        [DataMember(Name = "spatialReference")]
        public GPSpatialReference SpatialReference { get; set; }

        /// <summary>
        /// Gets or sets a reference to the collection of features retrieved by the query.
        /// </summary>
        [DataMember(Name = "features")]
        public IEnumerable<FeatureRecordData> Features { get; set; }
    }
}
