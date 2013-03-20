using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Geocode
{
    /// <summary>
    /// Name address geocoded record.
    /// </summary>
    class NameAddressRecord
    {
        /// <summary>
        /// Name address pair.
        /// </summary>
        public NameAddress NameAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Geocoded position.
        /// </summary>
        public Point GeoLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Matched address.
        /// </summary>
        public Address MatchedAddress
        {
            get;
            set;
        }
    }
}
