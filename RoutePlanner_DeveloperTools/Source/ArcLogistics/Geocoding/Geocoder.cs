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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.GeocodeService;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Class that represents a geocoder.
    /// </summary>
    public class Geocoder : GeocoderBase
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="geocodingServiceInfo">Geocoding service info.</param>
        /// <param name="geocodeServer">Geocode server.</param>
        /// <param name="exceptionHandler">Exception handler.</param>
        internal Geocoder(GeocodingServiceInfo geocodingServiceInfo,
            AgsServer geocodeServer, IServiceExceptionHandler exceptionHandler)
        {
            Debug.Assert(exceptionHandler != null);

            _exceptionHandler = exceptionHandler;

            // Init geocoding properties.
            _propMods = new PropertySet();
            _propMods.PropertyArray = new PropertySetProperty[2];

            _propMods.PropertyArray[0] = _CreateProp("WritePercentAlongField", "TRUE");
            _propMods.PropertyArray[1] = _CreateProp("MatchIfScoresTie", "TRUE");

            _geocodingServiceInfo = geocodingServiceInfo;
            if (_geocodingServiceInfo == null)
            {
                throw new SettingsException(Properties.Resources.DefaultGeocodingInfoIsNotSet);
            }

            _geocodingServer = geocodeServer;

            // Create address fields.
            _CreateAddressFields();

            _actualGeocodingInfo = new GeocodingInfo(geocodingServiceInfo);

            _locatorsInfos = new ReadOnlyCollection<LocatorInfo>(
                _actualGeocodingInfo.Locators ?? new LocatorInfo[] { });

            foreach (var locator in _locatorsInfos)
            {
                if (!_locators.ContainsKey(locator.Name))
                {
                    _locators.Add(locator.Name, locator);
                }
            }

            var fields = _geocodingServiceInfo.FieldMappings.FieldMapping.Select(mapping =>
                (AddressPart)Enum.Parse(
                    typeof(AddressPart),
                    mapping.AddressField,
                    true));

            _defaultLocator = new LocatorInfo(
                string.Empty,
                string.Empty,
                true,
                true,
                SublocatorType.Streets,
                fields);

            // Geocoder should be initialized later.
            _inited = false;
        }

        #endregion

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Arclogistics address fields.
        /// </summary>
        public override AddressField[] AddressFields
        {
            get
            {
                if (AddressFormat == AddressFormat.MultipleFields)
                {
                    // Return address parts without FullAddress.
                    List<AddressField> array = new List<AddressField>();

                    foreach (AddressField field in _addressFields)
                        if (field.Type != AddressPart.FullAddress)
                            array.Add(field);

                    return array.ToArray();
                }
                else
                {
                    // Return only FullAddress part.
                    List<AddressField> array = new List<AddressField>();

                    foreach (AddressField field in _addressFields)
                        if (field.Type == AddressPart.FullAddress)
                        {
                            array.Add(field);
                            break; // Work done.
                        }

                    return array.ToArray();
                }
            }
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Is geocoder initialized.
        /// </summary>
        /// <returns>True if geocoder is initialized, otherwise false.</returns>
        internal override bool IsInitialized()
        {
            try
            {
                _ValidateGeocoderState();
            }
            // If we got any exception during validation - geocoder isn't initialized.
            catch (Exception ex)
            {
                return false;
            }

            return _inited;
        }

        #endregion

        #region GeocoderBase Members

        /// <summary>
        /// Get a value indicating which address format is used.
        /// </summary>
        public override AddressFormat AddressFormat
        {
            get
            {
                AddressFormat format = AddressFormat.SingleField;

                // If user set ONLY one field.
                if (_addressFields.Length == 1)
                {
                    // If user set only Full Address field then use Single Field Address Format.
                    if (_addressFields[0].Type == AddressPart.FullAddress)
                        format = AddressFormat.SingleField;
                    else
                        // Otherwise use Multiple Fields Address Format.
                        format = AddressFormat.MultipleFields;
                }
                // If user forgot to set up address fields OR he set up multiple fields, then
                else
                {
                    // Find FullAddress field in address fields.
                    var fullAddressField = from field in _addressFields
                                           where (field.Type == AddressPart.FullAddress)
                                           select field;

                    // If UseSingleLine set to True and there are FullAddress field exists,
                    // then use Single Line Field mode, otherwise - Multiple Fields mode.
                    if (_geocodingServiceInfo.UseSingleLineInput && fullAddressField.Any())
                        format = AddressFormat.SingleField;
                    else
                        format = AddressFormat.MultipleFields;
                }

                return format;
            }
        }

        /// <summary>
        /// Event for notifying about reverse geocoding completed.
        /// </summary>
        public override event AsyncReverseGeocodedEventHandler AsyncReverseGeocodeCompleted;

        /// <summary>
        /// Gets a value indicating the minimum score value for an address candidate
        /// retrieved via geocoding to be treated as matched. 
        /// </summary>
        public override int MinimumMatchScore
        {
            get
            {
                return _geocodingServiceInfo.MinimumMatchScore.GetValueOrDefault(MINIMUM_SCORE);
            }
        }

        /// <summary>
        /// Gets a value indicating if the geocoder represents composite locator.
        /// </summary>
        public override bool IsCompositeLocator
        {
            get
            {
                return _geocodingServiceInfo.IsCompositeLocator;
            }
        }

        /// <summary>
        /// Gets a reference to the collection of locators information for composite locators or
        /// an empty collection for non-composite ones.
        /// </summary>
        public override System.Collections.ObjectModel.ReadOnlyCollection<LocatorInfo> Locators
        {
            get
            {
                return _locatorsInfos;
            }
        }

        /// <summary>
        /// Geocodes an address and returns the best candidate.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <returns>Returns address candidate with max score.</returns>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Is thrown if server state is unauthorized.</exception>
        public override AddressCandidate Geocode(Address address)
        {
            _ValidateGeocoderState();

            Debug.Assert(address != null);

            // Get propertyset for service from address.
            PropertySet addressSet = _GetPropertySet(address);

            PropertySet geocodedAddress = null;

            try
            {
                // Get propertyset of geocoded address.
                geocodedAddress = _client.GeocodeAddress(addressSet, _propMods);
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Logger.Info(ex);
            }

            if (geocodedAddress == null)
            {
                return null;
            }

            // Get address candidate from propertyset of geocoded address.
            var addressCandidate = _GetAddressCandidate(geocodedAddress);
            var hasPrimaryLocator =
                addressCandidate == null ||
                addressCandidate.Locator == null ||
                addressCandidate.Locator.Primary;
            if (!hasPrimaryLocator)
            {
                addressCandidate = null;
            }

            return addressCandidate;
        }

        /// <summary>
        /// Geocodes array of addresses.
        /// </summary>
        /// <param name="addresses">Array of addresses.</param>
        /// <returns>Returns array of best candidates for each input address.</returns>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Is thrown if server state is unauthorized.</exception>
        public override AddressCandidate[] BatchGeocode(Address[] addresses)
        {
            _ValidateGeocoderState();

            Debug.Assert(addresses != null);

            List<Address> addressList = addresses.ToList();
            List<AddressCandidate> candidatesList = new List<AddressCandidate>();

            // Due to ArcGIS Online limitations on batch geocoding max addresses count - split array to several.
            int startIndex = 0;
            while (startIndex < addressList.Count)
            {
                int partCount = Math.Min(_maxAddressCount, addressList.Count - startIndex);
                Address[] addressesPart = new Address[partCount];
                addressList.CopyTo(startIndex, addressesPart, 0, partCount);

                AddressCandidate[] addressCandidatesPart = null;
                addressCandidatesPart = _GeocodePart(addressesPart);

                if (addressCandidatesPart != null)
                {
                    candidatesList.AddRange(addressCandidatesPart);
                }

                startIndex += _maxAddressCount;
            }

            AddressCandidate[] addressCandidates = candidatesList.ToArray();
            return addressCandidates;
        }

        /// <summary>
        /// Geocodes address and returns array of found candidates.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Returns array of found address candidates.</returns>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Is thrown if server state is unauthorized.</exception>
        public override AddressCandidate[] GeocodeCandidates(Address address, bool includeDisabledLocators)
        {
            _ValidateGeocoderState();

            Debug.Assert(address != null);

            // Get propertyset for service from address.
            PropertySet addressSet = _GetPropertySet(address);

            RecordSet geocodedAddress = null;

            try
            {
                // Get recordset of geocoded address.
                geocodedAddress = _client.FindAddressCandidates(addressSet, _propMods);
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Logger.Info(ex);
            }

            // Get address candidate array from recordset of geocoded address.
            if (geocodedAddress == null)
            {
                return null;
            }

            var addressCandidates = _GetNeededAddressCandidates(
                geocodedAddress,
                includeDisabledLocators);

            return addressCandidates;
        }

        /// <summary>
        /// Finds address by geographical location.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <returns>Returns found address.</returns>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Is thrown if server state is unauthorized.</exception>
        public override Address ReverseGeocode(ESRI.ArcLogistics.Geometry.Point location)
        {
            _ValidateGeocoderState();

            Address res = null;

            try
            {
                PointN point = new PointN();
                point.X = location.X;
                point.Y = location.Y;

                // REV: we should pass WPG84 since we store our points in this spatial reference
                SpatialReference sr = new ESRI.ArcLogistics.GeocodeService.GeographicCoordinateSystem();
                sr.WKID = GeometryConst.WKID_WGS84;

                //point.SpatialReference = sr;

                // Set properties for reverse geocoder.
                PropertySet propMods = new PropertySet();
                propMods.PropertyArray = new PropertySetProperty[2];

                // Distance units.
                propMods.PropertyArray[1] = _CreateProp("ReverseDistanceUnits", REV_DISTANCE_UNITS);

                // Distance value.
                propMods.PropertyArray[0] = _CreateProp("ReverseDistance", SNAP_TOL);
                // REV: we should ask server to return point in WGS84

                // First try reverse geocode intersection.
                PropertySet pSet = _ReverseGeocode(point, false, propMods);
                if (pSet != null && pSet.PropertyArray != null)
                    res = _GetAddress(pSet);
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Logger.Error(ex);
            }

            return res;
        }

        /// <summary>
        /// Finds address by geographical location. Asynchronous method.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <param name="userToken">Geocoding operation token.</param>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Is thrown if server state is unauthorized.</exception>
        public override void ReverseGeocodeAsync(ESRI.ArcLogistics.Geometry.Point location, object userToken)
        {
            _ValidateGeocoderState();

            try
            {
                PointN point = new PointN();
                point.X = location.X;
                point.Y = location.Y;

                // REV: we should pass WPG84 since we store our points in this spatial reference
                SpatialReference sr = new ESRI.ArcLogistics.GeocodeService.GeographicCoordinateSystem();
                sr.WKID = GeometryConst.WKID_WGS84;

                //point.SpatialReference = sr;

                PropertySet propMods = new PropertySet();
                propMods.PropertyArray = new PropertySetProperty[2];

                // distance units
                propMods.PropertyArray[1] = _CreateProp("ReverseDistanceUnits", REV_DISTANCE_UNITS);

                // distance value
                propMods.PropertyArray[0] = _CreateProp("ReverseDistance", SNAP_TOL);

                _client.ReverseGeocodeAsync(point, false, propMods, userToken);
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Logger.Info(ex);
            } // eat FaultException if location cannot be reverse geocoded
        }

        /// <summary>
        /// Cancels asynchronous reverse geocoding operation.
        /// </summary>
        /// <param name="userToken">Token of the geocoding operation that must be cancelled.</param>
        /// <returns>Returns <c>true</c> if operation successfully cancelled, or <c>false</c> if operation with the token was not found.
        /// </returns>
        public override bool ReverseGeocodeAsyncCancel(object userToken)
        {
            // WCF does not support canceling async. operations out of the box
            throw new NotSupportedException();
        }

        #endregion

        #region Private properties

        /// <summary>
        /// Gets fields contained in the results of a geocode operation using
        /// the GeocodeAddress method.
        /// </summary>
        private Fields ResultFields
        {
            get
            {
                if (_resFields == null)
                    _resFields = _client.GetResultFields(_propMods);

                return _resFields;
            }
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Check is all address fields is empty.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>Is all fields empty.</returns>
        private static bool _IsAllAddressFieldsEmpty(Address address)
        {
            bool isEmpty = string.IsNullOrEmpty(address.Unit) &&
                string.IsNullOrEmpty(address.AddressLine) &&
                string.IsNullOrEmpty(address.Locality1) &&
                string.IsNullOrEmpty(address.Locality2) &&
                string.IsNullOrEmpty(address.Locality3) &&
                string.IsNullOrEmpty(address.CountyPrefecture) &&
                string.IsNullOrEmpty(address.PostalCode1) &&
                string.IsNullOrEmpty(address.PostalCode2) &&
                string.IsNullOrEmpty(address.StateProvince) &&
                string.IsNullOrEmpty(address.Country);

            return isEmpty;
        }

        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize geocoder.
        /// </summary>
        private void _InitGeocoder()
        {
            string serviceURL = _geocodingServiceInfo.Url;
            _client = new GeocodeServiceClient(serviceURL,
                _geocodingServer.OpenConnection());
            _client.ReverseGeocodeCompleted += new EventHandler<ReverseGeocodeCompletedEventArgs>(_ReverseGeocodeCompleted);

            // Get locator properties.
            PropertySet propSet = _client.GetLocatorProperties();
            foreach (PropertySetProperty prop in propSet.PropertyArray)
            {
                if (prop.Key.Equals(BATCHSIZE_PROPERTY, StringComparison.OrdinalIgnoreCase))
                {
                    _maxAddressCount = (int)prop.Value;
                    break;
                }
            }

            // Get locator fields.
            _addrFields = _client.GetAddressFields();

            // Now geocoder is initialized.
            _inited = true;
        }

        /// <summary>
        /// Validate geocoder state.
        /// </summary>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Is thrown if server state is unauthorized.</exception>
        private void _ValidateGeocoderState()
        {
            try
            {
                // Check servers state.
                ServiceHelper.ValidateServerState(_geocodingServer);

                // Check if services were created.
                if (!_inited)
                {
                    _InitGeocoder();
                }
            }
            catch (Exception ex)
            {
                _exceptionHandler.HandleException(ex, Properties.Resources.ServiceNameGeocode);

                throw;
            }
        }

        /// <summary>
        /// Create property.
        /// </summary>
        /// <param name="key">Property key</param>
        /// <param name="value">Property value.</param>
        /// <returns>Property.</returns>
        private static PropertySetProperty _CreateProp(string key, object value)
        {
            PropertySetProperty prop = new PropertySetProperty();
            prop.Key = key;
            prop.Value = value;

            return prop;
        }

        /// <summary>
        /// Get spatial reference of geocoder.
        /// </summary>
        /// <returns>Spatial reference.</returns>
        private SpatialReference _GetGeocoderSR()
        {
            SpatialReference sr = null;

            Fields resFields = ResultFields;
            foreach (Field field in resFields.FieldArray)
            {
                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    sr = field.GeometryDef.SpatialReference;
                    break;
                }
            }

            return sr;
        }

        /// <summary>
        /// Do batch geocode request for several addresses.
        /// </summary>
        /// <param name="addresses">Source addresses.</param>
        /// <returns>Address candidates.</returns>
        private AddressCandidate[] _GeocodePart(Address[] addresses)
        {
            Debug.Assert(addresses != null);
            RecordSet addressesSet = _GetAddressesRecordSet(addresses);

            // Get propertyset for service from address.
            PropertySet fieldMappingSet = _GetFieldMappingPropertySet();

            RecordSet geocodedAddress = null;
            AddressCandidate[] addressCandidates = null;

            try
            {
                // Get recordset of geocoded address.
                geocodedAddress = _client.GeocodeAddresses(addressesSet, fieldMappingSet, _propMods);
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Logger.Info(ex);
            }

            // Get address candidate array from recordset of geocoded address.
            if (geocodedAddress != null)
            {
                addressCandidates = _GetAllAddressCandidates(geocodedAddress, false);
            }

            return addressCandidates;
        }

        /// <summary>
        /// Do reverse geocode.
        /// </summary>
        /// <param name="point">Source point</param>
        /// <param name="returnIntr">Return intersection.</param>
        /// <param name="propMods">Properties.</param>
        /// <returns>Property set with result of reverse geocoding request.</returns>
        private PropertySet _ReverseGeocode(PointN point, bool returnIntr,
            PropertySet propMods)
        {
            PropertySet res = null;
            try
            {
                res = _client.ReverseGeocode(point, returnIntr, propMods);
            }
            catch (System.ServiceModel.FaultException ex)
            {
                Logger.Info(ex);
            } // eat FaultException if location cannot be reverse geocoded

            return res;
        }

        /// <summary>
        /// Create address fields.
        /// </summary>
        private void _CreateAddressFields()
        {
            if (_addrFields != null)
                _CreateAddressFieldsFromLocatorFields();
            else
                _CreateAddressFieldsFromServiceInfo();
        }

        /// <summary>
        /// Create address fields from service info. To support work without connection.
        /// </summary>
        private void _CreateAddressFieldsFromServiceInfo()
        {
            int fieldsCount = _geocodingServiceInfo.FieldMappings.FieldMapping.Length;
            _addressFields = new AddressField[fieldsCount];
            _locatorFieldNames = new string[fieldsCount];

            // Create address field array.
            for (int index = 0; index < _geocodingServiceInfo.FieldMappings.FieldMapping.Length; index++)
            {
                // Get field mapping for currect address field.
                InputFieldMapping fieldMapping = _geocodingServiceInfo.FieldMappings.FieldMapping[index];

                // Get mapped address part (which part of address will be linked to locator alias name).
                string addressPartName = fieldMapping.AddressField;
                AddressPart addressPart = (AddressPart)Enum.Parse(typeof(AddressPart), addressPartName, true);

                // Get title for current mapping.
                string locatorName = fieldMapping.LocatorField;

                _locatorFieldNames[index] = locatorName;

                // Create address field.
                _addressFields[index] = new AddressField(locatorName, addressPart, fieldMapping.Visible, fieldMapping.Description);
            }
        }

        /// <summary>
        /// Create address fields from locator fields.
        /// </summary>
        private void _CreateAddressFieldsFromLocatorFields()
        {
            int fieldsCount = _addrFields.FieldArray.Length;
            _addressFields = new AddressField[fieldsCount];
            _locatorFieldNames = new string[fieldsCount];

            // Create address field array.
            for (int index = 0; index < _geocodingServiceInfo.FieldMappings.FieldMapping.Length; index++)
            {
                // Get field mapping for currect address field.
                InputFieldMapping fieldMapping = _geocodingServiceInfo.FieldMappings.FieldMapping[index];

                // Get mapped address part (which part of address will be linked to locator alias name).
                string addressPartName = fieldMapping.AddressField;
                AddressPart addressPart = (AddressPart)Enum.Parse(typeof(AddressPart), addressPartName, true);

                // Get title for current mapping.
                string locatorName = fieldMapping.LocatorField;
                string title = null;
                foreach (Field field in _addrFields.FieldArray)
                {
                    if (field.Name.Equals(locatorName, StringComparison.OrdinalIgnoreCase))
                    {
                        title = field.Name;
                        break;
                    }
                }

                _locatorFieldNames[index] = locatorName;

                // Create address field.
                _addressFields[index] = new AddressField(title, addressPart, fieldMapping.Visible, fieldMapping.Description);
            }
        }

        /// <summary>
        /// Get property set for request to service.
        /// </summary>
        /// <param name="address">Source address.</param>
        /// <returns>Property set for request to service.</returns>
        private PropertySet _GetPropertySet(Address address)
        {
            List<PropertySetProperty> propertyArray = new List<PropertySetProperty>();

            if (AddressFormat == AddressFormat.MultipleFields)
            {
                // Fill property set by address parts without FullAddress part.
                for (int index = 0; index < _addressFields.Length; index++)
                    if (_addressFields[index].Type != AddressPart.FullAddress)
                        propertyArray.Add(_GetAddressProperty(index, address));
            }
            else if (AddressFormat == AddressFormat.SingleField)
            {
                // Fill property set with only FullAddress part.
                for (int index = 0; index < _addressFields.Length; index++)
                    if (_addressFields[index].Type == AddressPart.FullAddress)
                    {
                        propertyArray.Add(_GetAddressProperty(index, address));
                        break; // Work done: we don't need any other fields.
                    }
            }
            else
            {
                // Do nothing.
            }

            PropertySet addressSet = new PropertySet();
            addressSet.PropertyArray = propertyArray.ToArray();

            return addressSet;
        }

        /// <summary>
        /// Method gets address property, which has Locator Field Name as a Key and
        /// Address as a Value.
        /// </summary>
        /// <param name="index">Index of address field.</param>
        /// <param name="address">Source address.</param>
        /// <returns>Property Set Property.</returns>
        private PropertySetProperty _GetAddressProperty(int index, Address address)
        {
            PropertySetProperty property = new PropertySetProperty();

            property.Key = _locatorFieldNames[index];

            AddressPart addressPart = _addressFields[index].Type;

            property.Value = address[addressPart];

            return property;
        }

        /// <summary>
        /// Function for replacing space sequence to space.
        /// </summary>
        /// <param name="input">String to replace.</param>
        /// <returns>String with removed spaces sequences.</returns>
        private string _RemoveDuplicateWhiteSpace(string input)
        {
            if (input == null)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                string[] parts = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                    sb.AppendFormat("{0} ", parts[i].Trim());
                return sb.ToString().Trim();
            }
        }

        /// <summary>
        /// Get address from property set response.
        /// </summary>
        /// <param name="propertySet">Property set.</param>
        /// <returns>Address.</returns>
        private Address _GetAddress(PropertySet propertySet)
        {
            Address address = new Address();

            foreach (PropertySetProperty prop in propertySet.PropertyArray)
            {
                // Try to extract address field.
                int fieldCount = _addressFields.Length;
                for (int index = 0; index < fieldCount; index++)
                {
                    if (prop.Key.Equals(_locatorFieldNames[index], StringComparison.OrdinalIgnoreCase))
                    {
                        AddressPart addressPart = _addressFields[index].Type;
                        address[addressPart] = _RemoveDuplicateWhiteSpace((string)prop.Value);
                        break;
                    }
                }
                // Try to extract locator name.
                if (prop.Key.Equals(LOCNAME_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    string locName = (string)prop.Value;
                    address.MatchMethod = locName ?? string.Empty;
                }
            }

            string fullAddress = "";
            for (int index = 0; index < _addressFields.Length; index++)
            {
                AddressPart addressPart = _addressFields[index].Type;
                if (address[addressPart] != null)
                {
                    if (fullAddress.Length != 0 && address[addressPart].Length != 0)
                        fullAddress += ", ";
                    fullAddress += address[addressPart];
                }
            }

            if (fullAddress.Length != 0)
                address.FullAddress = fullAddress;

            var candidate = new AddressCandidate
            {
                Address = address,
            };

            _UpdateLocatorProperties(candidate);

            return address;
        }

        /// <summary>
        /// Updates locator related properties for the specified address candidate.
        /// </summary>
        /// <param name="candidate">The reference to the address candidate to be updated.</param>
        /// <returns>True if and only if the locator corresponding to the address match method
        /// for the specified candidate is enabled.</returns>
        private bool _UpdateLocatorProperties(AddressCandidate candidate)
        {
            if (!this.IsCompositeLocator)
            {
                candidate.Locator = _defaultLocator;

                return true;
            }

            var locatorEnabled = true;
            var locator = default(LocatorInfo);
            var matchMethod = candidate.Address.MatchMethod ?? string.Empty;
            if (_locators.TryGetValue(matchMethod, out locator))
            {
                candidate.Locator = locator;
                locatorEnabled = locator.Enabled;
                if (locatorEnabled)
                {
                    candidate.Address.MatchMethod = locator.Title ?? string.Empty;
                }
            }

            return locatorEnabled;
        }

        /// <summary>
        /// Get AddressCandidate from geocode service response.
        /// </summary>
        /// <param name="geocodedAddress">Geocode service response.</param>
        /// <returns>Geocoded AddressCandidate.</returns>
        private AddressCandidate _GetAddressCandidate(PropertySet geocodedAddress)
        {
            AddressCandidate result = new AddressCandidate();

            Address address = new Address();
            result.Address = address;

            // Look for needed properties and put them in to AddressCandidate.
            foreach (PropertySetProperty property in geocodedAddress.PropertyArray)
            {
                if (property.Key.Equals(SHAPE_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    PointN locationN = (PointN)property.Value;
                    result.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(locationN.X, locationN.Y); ;
                }

                else if (property.Key.Equals(SCORE_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    result.Score = Convert.ToInt16(property.Value);

                else if (property.Key.Equals(MATCHADDR_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    address.FullAddress = (string)property.Value ?? string.Empty;

                else if (property.Key.Equals(LOCNAME_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    address.MatchMethod = (string)property.Value ?? string.Empty;
            }

            if (!_UpdateLocatorProperties(result))
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Get AddressCandidate array from geocode service response.
        /// </summary>
        /// <param name="geocodedAddress">Source address to get candidates.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Address candidates array.</returns>
        private AddressCandidate[] _GetAllAddressCandidates(RecordSet geocodedAddress, bool includeDisabledLocators)
        {
            int recordCount = geocodedAddress.Records.Length;
            List<AddressCandidate> result = new List<AddressCandidate>();

            // Get requred fields indexes.
            int pointFieldIndex = -1;
            int scoreFieldIndex = -1;
            int matchAddrFieldIndex = -1;
            int locNameFieldIndex = -1;
            int addrTypeFieldIndex = -1;
            int addressOIDFieldIndex = -1;

            int fieldCount = geocodedAddress.Fields.FieldArray.Length;
            for (int index = 0; index < fieldCount; index++)
            {
                Field field = geocodedAddress.Fields.FieldArray[index];
                if (field.Name.Equals(SHAPE_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    pointFieldIndex = index;
                else if (field.Name.Equals(SCORE_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    scoreFieldIndex = index;
                else if (field.Name.Equals(MATCHADDR_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    matchAddrFieldIndex = index;
                else if (field.Name.Equals(LOCNAME_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    locNameFieldIndex = index;
                else if (field.Name.Equals(ADDRTYPE_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    addrTypeFieldIndex = index;
                else if (field.Name.Equals(
                    PROPERTY_WHICH_MAP_TO_ADDRESS_OID_PROPERTY_KEY, StringComparison.OrdinalIgnoreCase))
                    addressOIDFieldIndex = index;
            }
            
            // This code is for batch geocoding.
            // We can get address candidates in new order, not in which we send address to batch.
            // Since OID property in request is from 0 to addresses count we should sort 
            // received candidates by property which maps to OID property to make sure 
            // that candidate is matched to corresponding address.
            var records = geocodedAddress.Records.ToList();
            if(addressOIDFieldIndex != -1)
                records = records.OrderBy(record => (int)record.Values[addressOIDFieldIndex]).ToList();

            // Get AddressCandidate from record. 
            for (int index = 0; index < recordCount; index++)
            {
                AddressCandidate candidate = _GetAddressCandidateFromRecord(records[index], 
                    pointFieldIndex, scoreFieldIndex, matchAddrFieldIndex, locNameFieldIndex,
                    includeDisabledLocators, addrTypeFieldIndex);

                // In case of locator is not enable candidate is null.
                result.Add(candidate);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Get AddressCandidate from record.
        /// </summary>
        /// <param name="record">Source record.</param>
        /// <param name="pointFieldIndex">Index of point field.</param>
        /// <param name="scoreFieldIndex">Index of score field.</param>
        /// <param name="matchAddrFieldIndex">Index of match address field.</param>
        /// <param name="locNameFieldIndex">Index of locator name field.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <param name="addrTypeIndex">Index of address type field.</param>
        /// <returns>Address candidate.</returns>
        private AddressCandidate _GetAddressCandidateFromRecord(Record record, int pointFieldIndex,
            int scoreFieldIndex, int matchAddrFieldIndex, int locNameFieldIndex, bool includeDisabledLocators,
            int addrTypeIndex)
        {
            AddressCandidate result = new AddressCandidate();
            Address address = new Address();
            result.Address = address;

            PointN locationN = (PointN)record.Values[pointFieldIndex];

            // Check that we got non null point in record.
            if (locationN != null)
            {
                result.GeoLocation =
                    new ESRI.ArcLogistics.Geometry.Point(locationN.X, locationN.Y);
            }

            result.Score = Convert.ToInt16(record.Values[scoreFieldIndex]);

            string matchAddr = (string)record.Values[matchAddrFieldIndex];
            address.FullAddress = matchAddr ?? string.Empty;

            string locName = string.Empty;
            if (locNameFieldIndex != -1)
            {
                locName = (string)record.Values[locNameFieldIndex];
            }
            address.MatchMethod = locName ?? string.Empty;

            // Assign address type property value.
            string addrType = string.Empty;
            if (addrTypeIndex != -1)
            {
                addrType = (string)record.Values[addrTypeIndex];
            }
            result.AddressType = addrType ?? string.Empty;

            if (!_UpdateLocatorProperties(result) && !includeDisabledLocators)
            {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// Create Field for RecordSet.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <param name="fieldType">Field type.</param>
        /// <param name="length">Field length.</param>
        /// <returns>Field.</returns>
        private Field _CreateField(string name, esriFieldType fieldType, int length)
        {
            Field field = new Field();
            field.Name = name;
            field.Type = fieldType;
            field.Length = length;

            return field;
        }

        /// <summary>
        /// Create RecordSet for Addresses.
        /// </summary>
        /// <param name="addresses">Addresses to make batch geocode.</param>
        /// <returns>Addresses record set.</returns>
        private RecordSet _GetAddressesRecordSet(Address[] addresses)
        {
            RecordSet addressTable = new RecordSet();

            addressTable.Fields = _GetAddressFieldsForBatchGeocoding();
            addressTable.Records = _GetAddressValuesForBatchGeocoding(addresses);

            return addressTable;
        }

        /// <summary>
        /// Method returns address fields collection.
        /// </summary>
        /// <returns>Address Fields.</returns>
        private Fields _GetAddressFieldsForBatchGeocoding()
        {
            List<Field> fieldArray = new List<Field>();

            // Add object id field.
            fieldArray.Add(_CreateField(OBJECT_ID_PROPERTY_KEY,
                esriFieldType.esriFieldTypeOID, FIELD_LENGTH));

            // Add address fields.
            if (AddressFormat == AddressFormat.SingleField)
            {
                // For Batch geocoding in a Single Line mode, we have to use
                // first Address Part field instead of FullAddress field.
                string firstAddressFieldName = _addrFields.FieldArray.First().Name;

                fieldArray.Add(_CreateField(firstAddressFieldName,
                    esriFieldType.esriFieldTypeString, FIELD_LENGTH));
            }
            else if (AddressFormat == AddressFormat.MultipleFields)
            {
                var filteredFields = from field in _addressFields
                                     where (field.Type != AddressPart.FullAddress)
                                     select _CreateField(field.Type.ToString(),
                                     esriFieldType.esriFieldTypeString, FIELD_LENGTH);
                fieldArray.AddRange(filteredFields);
            }
            else
            {
                // Do nothing.
                Debug.Assert(false);
            }

            Fields fields = new Fields();
            fields.FieldArray = fieldArray.ToArray();

            return fields;
        }

        /// <summary>
        /// Method returns address values collection for batch geocoding.
        /// </summary>
        /// <param name="addresses">Addresses to make batch geocode.</param>
        /// <returns>Collection of records with Address Values.</returns>
        private Record[] _GetAddressValuesForBatchGeocoding(Address[] addresses)
        {
            List<Record> records = new List<Record>();

            // Add address fields.
            for (int index = 0; index < addresses.Length; index++)
            {
                List<object> values = new List<object>();

                // Add unique Object Id (OID) to every address.
                values.Add(index);

                // Fill property set with only FullAddress part.
                if (AddressFormat == AddressFormat.SingleField)
                {
                    string value =
                        addresses[index][AddressPart.FullAddress] ?? string.Empty;
                    values.Add(value);
                }
                else if (AddressFormat == AddressFormat.MultipleFields)
                {
                    var filteredValues = from field in _addressFields
                                         where (field.Type != AddressPart.FullAddress)
                                         select (addresses[index][field.Type] ?? string.Empty);
                    values.AddRange(filteredValues);
                }
                else
                {
                    // Do nothing.
                    Debug.Assert(false);
                }

                Record record = new Record();
                record.Values = values.ToArray();
                records.Add(record);
            }

            return records.ToArray();
        }

        /// <summary>
        /// Get field mapping property set for request to batch service.
        /// </summary>
        /// <returns>Field mapping property set for request to batch service.</returns>
        private PropertySet _GetFieldMappingPropertySet()
        {
            List<PropertySetProperty> properties = new List<PropertySetProperty>();

            if (AddressFormat == AddressFormat.MultipleFields)
            {
                // Fill property set by address parts without FullAddress part.
                for (int index = 0; index < _addressFields.Length; index++)
                    if (_addressFields[index].Type != AddressPart.FullAddress)
                    {
                        properties.Add(_GetFieldNamePropertyForMultipleLines(index, index));
                    }
                    else
                    {
                        // Do nothing: don't need other fields.
                    }
            }
            // Fill property set by first address part in locator fields.
            else if (AddressFormat == AddressFormat.SingleField)
            {
                // Get first address field returned from geocoding server.
                string firstAddressFieldName = _addrFields.FieldArray.First().Name;
                properties.Add(_GetFieldNamePropertyForSingleLine(firstAddressFieldName));
            }
            else
            {
                // Do nothing.
            }

            PropertySet addressSet = new PropertySet();
            addressSet.PropertyArray = properties.ToArray();

            return addressSet;
        }

        /// <summary>
        /// Method gets field name property, which has Locator Field Name as a Key and
        /// Address Field Type as a Value.
        /// </summary>
        /// <param name="locatorFieldIndex">Index of a locator field.</param>
        /// <param name="addressFieldIndex">Index of a address field.</param>
        /// <returns>Field name property for field mapping.</returns>
        private PropertySetProperty _GetFieldNamePropertyForMultipleLines(int locatorFieldIndex,
            int addressFieldIndex)
        {
            PropertySetProperty property = new PropertySetProperty();

            property.Key = _locatorFieldNames[locatorFieldIndex];

            property.Value = _addressFields[addressFieldIndex].Type.ToString();

            return property;
        }

        /// <summary>
        /// Method gets field name property, which has Locator Field Name as a Key and
        /// Address Field Type as a Value.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Field name property for field mapping.</returns>
        private PropertySetProperty _GetFieldNamePropertyForSingleLine(string name)
        {
            PropertySetProperty property = new PropertySetProperty();

            property.Key = name;
            property.Value = name;

            return property;
        }

        /// <summary>
        /// React on reverse geocode completed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Reverse geocode completed event args.</param>
        private void _ReverseGeocodeCompleted(object sender, ReverseGeocodeCompletedEventArgs e)
        {
            if (!e.Cancelled && e.Error == null)
            {
                // Geocoded successfully.
                PropertySet set = e.Result;

                // WORKAROUND!!! TODO DT
                // Remove, when geocode service will return good response in case if it 
                // hasn't found candidates by reverse geocoding. 
                // Right now it can return different responses.
                try
                {
                    // In ArcGIS Server 10 exception is not throws on failed reverse geocoding.
                    // Server returns empty result.
                    if (set.PropertyArray != null)
                    {
                        Address address = _GetAddress(set);

                        // WORKAROUND!!!
                        // When service return point without address - do not show anything.
                        if (string.IsNullOrEmpty(address.AddressLine))
                            return;

                        PointN point = (PointN)e.Result.PropertyArray[0].Value;

                        ESRI.ArcGIS.Client.Geometry.MapPoint location = new ESRI.ArcGIS.Client.Geometry.MapPoint(
                            point.X, point.Y);

                        if (AsyncReverseGeocodeCompleted != null)
                            AsyncReverseGeocodeCompleted(this, new AsyncReverseGeocodedEventArgs(address, location, e.UserState));
                    }
                }
                catch (Exception ex)
                {

                }

            }
        }

        /// <summary>
        /// Get all candidates and remove null from list. 
        /// If candidate with maximum score present and locator is primary - remove other candidates.
        /// </summary>
        /// <param name="geocodedAddress">Record set with candidates from server.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Correct candidates list.</returns>
        private AddressCandidate[] _GetNeededAddressCandidates(RecordSet geocodedAddress, bool includeDisabledLocators)
        {
            AddressCandidate[] addressCandidates = _GetAllAddressCandidates(geocodedAddress, includeDisabledLocators);

            List<AddressCandidate> addressCandidatesList = _RemoveNullElements(addressCandidates);

            // Remove candidates with score less than minimum.
            for (int index = addressCandidatesList.Count - 1; index >= 0; index--)
            {
                if (addressCandidatesList[index] != null &&
                    addressCandidatesList[index].Score < _geocodingServiceInfo.MinimumCandidateScore)
                {
                    addressCandidatesList.RemoveAt(index);
                }
            }

            addressCandidates = addressCandidatesList.ToArray();

            return addressCandidates;
        }

        /// <summary>
        /// Return list without empty candidates.
        /// </summary>
        /// <param name="candidatesArr">Candidates array.</param>
        /// <returns>Candidates list without empty items.</returns>
        private List<AddressCandidate> _RemoveNullElements(AddressCandidate[] candidatesArr)
        {
            List<AddressCandidate> candidates = new List<AddressCandidate>();

            foreach (AddressCandidate candidate in candidatesArr)
            {
                if (candidate != null)
                {
                    candidates.Add(candidate);
                }
            }

            return candidates;
        }
        #endregion

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Distance units for reverse geocoding.
        /// </summary>
        private const string REV_DISTANCE_UNITS = "Meters";

        /// <summary>
        /// Shape property name.
        /// </summary>
        private const string SHAPE_PROPERTY_KEY = "Shape";

        /// <summary>
        /// Score property name.
        /// </summary>
        private const string SCORE_PROPERTY_KEY = "Score";

        /// <summary>
        /// Matched address property name.
        /// </summary>
        private const string MATCHADDR_PROPERTY_KEY = "Match_addr";

        /// <summary>
        /// Locator property name.
        /// </summary>
        private const string LOCNAME_PROPERTY_KEY = "Loc_name";

        /// <summary>
        /// Name of the property which should map to OID.
        /// </summary>
        private const string PROPERTY_WHICH_MAP_TO_ADDRESS_OID_PROPERTY_KEY = "ResultID";

        /// <summary>
        /// Locator type property name.
        /// </summary>
        private const string ADDRTYPE_PROPERTY_KEY = "Addr_Type";

        /// <summary>
        /// Suggested batch size property name.
        /// </summary>
        private const string BATCHSIZE_PROPERTY = "SuggestedBatchSize";

        /// <summary>
        /// Reverse geocoding distance tolerance.
        /// </summary>
        private const double SNAP_TOL = 500.0;

        /// <summary>
        /// Minimum address candidate score to treat candidate as matched.
        /// </summary>
        private const int MINIMUM_SCORE = 80;

        /// <summary>
        /// Default address field length.
        /// </summary>
        private const int FIELD_LENGTH = 250;

        /// <summary>
        /// Object Id property key.
        /// </summary>
        private const string OBJECT_ID_PROPERTY_KEY = "OID";

        #endregion

        #region Private members

        /// <summary>
        /// Property modifications for direct geocoding.
        /// </summary>
        private readonly PropertySet _propMods;

        /// <summary>
        /// Geocoding service info.
        /// </summary>
        private GeocodingServiceInfo _geocodingServiceInfo;

        /// <summary>
        /// Geocoding server.
        /// </summary>
        private AgsServer _geocodingServer;

        /// <summary>
        /// Geocoding service client.
        /// </summary>
        private GeocodeServiceClient _client;

        /// <summary>
        /// Address fields.
        /// </summary>
        private Fields _addrFields;

        /// <summary>
        /// Fields contained in the results of a geocode operation using the GeocodeAddress method.
        /// </summary>
        private Fields _resFields;

        /// <summary>
        /// Arclogistics internal address fields.
        /// </summary>
        private AddressField[] _addressFields;

        /// <summary>
        /// Locator field names.
        /// </summary>
        private string[] _locatorFieldNames;

        /// <summary>
        /// Max Address count for batch geocoding.
        /// </summary>
        private int _maxAddressCount;

        /// <summary>
        /// Is geocoder inited.
        /// </summary>
        private bool _inited;

        /// <summary>
        /// Arclogistics actual geocoding service info.
        /// </summary>
        private GeocodingInfo _actualGeocodingInfo;

        /// <summary>
        /// Locator info for non-composite locator.
        /// </summary>
        private LocatorInfo _defaultLocator;

        /// <summary>
        /// A dictionary for mapping match method string into corresponding locator information
        /// object.
        /// </summary>
        private Dictionary<string, LocatorInfo> _locators = new Dictionary<string, LocatorInfo>(
            StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Exceptions handler.
        /// </summary>
        private IServiceExceptionHandler _exceptionHandler;

        /// <summary>
        /// Location information collection.
        /// </summary>
        private ReadOnlyCollection<LocatorInfo> _locatorsInfos;

        #endregion
    }
}
