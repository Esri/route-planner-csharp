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
using System.Linq;
using System.Collections.Generic;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents a new ArcGis geocoder.
    /// </summary>
    public class ArcGiscomGeocoder : Geocoder
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="geocodingServiceInfo">Geocoding service info.</param>
        /// <param name="geocodeServer">Geocode server.</param>
        /// <param name="exceptionHandler">Exception handler.</param>
        /// <exception cref="System.ArgumentException">Is thrown in case if geocodingServiceInfo
        /// or geocodeServer parameters is null.</exception>
        internal ArcGiscomGeocoder(GeocodingServiceInfo geocodingServiceInfo,
            AgsServer geocodeServer, IServiceExceptionHandler exceptionHandler)
            : base(geocodingServiceInfo, geocodeServer, exceptionHandler)
        {
            if (geocodingServiceInfo == null)
                throw new ArgumentException("geocodingServiceInfo");
            if(geocodeServer == null)
                throw new ArgumentException("geocodeServer");

            var list = new List<string>();

            // Fill collection with names of exact locators types.
            foreach (var locator in geocodingServiceInfo.ExactLocators.Locators)
                list.Add(locator.Type);

            // Local storage is exact locator.
            list.Add(LOCAL_STORAGE_ADDRESS_TYPE);

            ExactLocatorsTypesNames = list;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Name of address types for points from local storage.
        /// </summary>
        public static string LocalStorageAddressType
        {
            get
            {
                return LOCAL_STORAGE_ADDRESS_TYPE;
            }
        }

        /// <summary>
        /// Collection with names of exact locators.
        /// </summary>
        public IEnumerable<string> ExactLocatorsTypesNames { get; private set; }

        #endregion

        #region Constants

        /// <summary>
        /// Name of address types for points from local storage.
        /// </summary>
        private const string LOCAL_STORAGE_ADDRESS_TYPE = "User selected";

        #endregion

    }
}
