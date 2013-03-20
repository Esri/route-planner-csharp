using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Represents a geocodable entity that has an address and a geographical location.
    /// </summary>
    public interface IGeocodable
    {
        /// <summary>
        /// Address.
        /// </summary>
        Address Address
        {
            get;
            set;
        }

        /// <summary>
        /// Geographical location.
        /// </summary>
        Point? GeoLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the object is geocoded or not.
        /// </summary>
        bool IsGeocoded
        {
            get;
        }

        /// <summary>
        /// Property which turn on/off address validation.
        /// </summary>
        bool IsAddressValidationEnabled
        {
            get;
            set;
        }
    }
}
