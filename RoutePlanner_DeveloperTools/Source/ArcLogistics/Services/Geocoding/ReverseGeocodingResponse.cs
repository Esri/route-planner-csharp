using System.Runtime.Serialization;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Services.Geocoding
{
    /// <summary>
    /// Response of the ReverseGeocode operation.
    /// </summary>
    [DataContract]
    internal sealed class ReverseGeocodeResponse : GPResponse
    {
        /// <summary>
        /// Gets or sets the address found by the geocoding service.
        /// </summary>
        [DataMember(Name = "address")]
        public string Address
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the match level of address finding result.
        /// </summary>
        [DataMember(Name = "matchLevel")]
        public string MatchLevel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the location of address found by the geocoding service.
        /// </summary>
        [DataMember(Name = "location")]
        public MapPoint Location
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the extent of the address found by the geocoding service.
        /// </summary>
        [DataMember(Name = "extent")]
        public Envelope Extent
        {
            get;
            set;
        }
    }
}
