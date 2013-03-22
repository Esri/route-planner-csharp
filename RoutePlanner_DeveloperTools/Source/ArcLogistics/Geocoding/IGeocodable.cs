/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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
