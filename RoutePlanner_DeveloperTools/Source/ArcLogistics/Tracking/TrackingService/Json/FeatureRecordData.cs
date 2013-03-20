using System.Runtime.Serialization;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Tracking.TrackingService.DataModel;

namespace ESRI.ArcLogistics.Tracking.TrackingService.Json
{
    /// <summary>
    /// Stores data for a single feature object.
    /// </summary>
    [DataContract]
    [KnownType(typeof(NameValuePair[]))]
    [KnownType(typeof(GPGeometry))]
    [KnownType(typeof(GPPoint))]
    [KnownType(typeof(GPPolygon))]
    [KnownType(typeof(GPPolyline))]
    internal class FeatureRecordData
    {
        /// <summary>
        /// Gets or sets a reference to the feature location.
        /// </summary>
        [DataMember(Name = "geometry")]
        public GPGeometry Geometry { get; set; }

        /// <summary>
        /// Gets or sets a reference to feature object attributes.
        /// </summary>
        [DataMember(Name = "attributes")]
        public AttrDictionary Attributes { get; set; }
    }
}
