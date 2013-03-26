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
using System.Diagnostics;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Arclogistics actual geocoding service info.
    /// </summary>
    internal class GeocodingInfo
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="geocodingServiceInfo">Geocoding info from data layer.</param>
        internal GeocodingInfo(GeocodingServiceInfo geocodingServiceInfo)
        {
            Debug.Assert(geocodingServiceInfo != null);

            // Copy internal locators info.
            if (geocodingServiceInfo.InternalLocators != null)
            {
                _locators = new List<LocatorInfo>();

                foreach (SublocatorInfo sublocator in geocodingServiceInfo.InternalLocators.SublocatorInfo)
                {
                    LocatorInfo LocatorInfo = LocatorInfo.CreateLocatorInfo(sublocator);
                    _locators.Add(LocatorInfo);
                }
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Locators info.
        /// </summary>
        public LocatorInfo[] Locators
        {
            get
            {
                if (_locators == null)
                {
                    return null;
                }

                return _locators.ToArray();
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Locators info.
        /// </summary>
        private List<LocatorInfo> _locators;

        #endregion
    }

    /// <summary>
    /// Geocoder sublocator info class.
    /// </summary>
    public class LocatorInfo
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Sublocator name.</param>
        /// <param name="title">Sublocator title</param>
        /// <param name="primary">Is primary sublocator.</param>
        /// <param name="enable">Is sublocator enabled.</param>
        /// <param name="type">The type of the sublocator.</param>
        /// <param name="internalFields">Sublocator internal field mappings.</param>
        internal LocatorInfo(
            string name,
            string title,
            bool primary,
            bool enable,
            SublocatorType type,
            IEnumerable<AddressPart> internalFields)
        {
            Debug.Assert(internalFields != null);

            Name = name;
            Title = title;
            Primary = primary;
            this.Enabled = enable;

            _internalFields = new List<AddressPart>(internalFields);
            this.Type = type;
        }

        #endregion

        #region public static methods
        /// <summary>
        /// Creates <see cref="T:ESRI.ArcGIS.Geocoding.LocatorInfo"/> instance from the specified
        /// sub-locator information object.
        /// </summary>
        /// <param name="info">The reference to the sub-locator information object.</param>
        /// <returns>A new <see cref="T:ESRI.ArcGIS.Geocoding.LocatorInfo"/> with the specified
        /// sub-locator information.</returns>
        internal static LocatorInfo CreateLocatorInfo(SublocatorInfo info)
        {
            Debug.Assert(info != null);

            var internalFields = new List<AddressPart>();

            // Copy field mappings.
            foreach (InternalFieldMapping fieldsMapping in info.FieldMappings.FieldMapping)
            {
                string addressPartName = fieldsMapping.AddressField;
                AddressPart addressPart = (AddressPart)Enum.Parse(typeof(AddressPart), addressPartName, true);
                internalFields.Add(addressPart);
            }

            var type = (SublocatorType)Enum.Parse(typeof(SublocatorType), info.Type, true);

            return new LocatorInfo(
                info.name,
                info.title,
                info.primary,
                info.enable,
                type,
                internalFields);

        }
        #endregion

        #region Public properties

        /// <summary>
        /// Locator name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Locator title.
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// Is primary locator.
        /// </summary>
        public bool Primary
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the locator is enabled.
        /// </summary>
        public bool Enabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets type of the locator.
        /// </summary>
        public SublocatorType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Locator fields.
        /// </summary>
        public AddressPart[] InternalFields
        {
            get
            {
                return _internalFields.ToArray();
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Locator fields.
        /// </summary>
        private List<AddressPart> _internalFields;

        #endregion
    }
}
