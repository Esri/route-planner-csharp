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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Geocode
{
    /// <summary>
    /// Locator type, depends on smallest field.
    /// </summary>
    internal enum LocatorType
    {
        CityState,
        Zip,
        Street
    }

    /// <summary>
    /// Class, which contains helper function for geocoding.
    /// </summary>
    internal static class GeocodeHelpers
    {
        #region Public static methods

        /// <summary>
        /// Do geocode of geocodable object.
        /// </summary>
        /// <param name="geocodable">Item to geocode.</param>
        /// <param name="useLocalAsPrimary">Is item just created.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Candidates list.</returns>
        public static List<AddressCandidate> DoGeocode(IGeocodable geocodable, bool useLocalAsPrimary, bool includeDisabledLocators)
        {
            List<AddressCandidate> candidates = null;

            App.Current.MainWindow.Cursor = Cursors.Wait;

            try
            {
                Order order = geocodable as Order;
                Location location = geocodable as Location;
                if (order != null)
                {
                    candidates = _DoLocalOrderGeocode(order, useLocalAsPrimary, includeDisabledLocators);
                }
                else if (location != null)
                {
                    candidates = _DoGeocodeLocation(location, includeDisabledLocators);
                }
            }
            catch (Exception ex)
            {
                if (MustThrowException(ex))
                {
                    throw;
                }
            }
            finally
            {
                App.Current.MainWindow.Cursor = Cursors.Arrow;
            }

            if (candidates == null)
            {
                candidates = new List<AddressCandidate>();
            }

            return candidates;
        }

        /// <summary>
        /// Check that exception must be thrown.
        /// </summary>
        /// <param name="ex">Exception to handle.</param>
        /// <returns>True if exception must be thrown, otherwise - false.</returns>
        public static bool MustThrowException(Exception ex)
        {
            Debug.Assert(ex != null);

            // Log exception.
            Logger.Warning(ex);

            // If exception isn't authentification or communication, it must be thrown.
            if (ex is AuthenticationException || ex is CommunicationException)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Set geocoded fields to address.
        /// </summary>
        /// <param name="geocodable">Geocodable object to set address and position.</param>
        /// <param name="candidate">Source candidate.</param>
        public static void SetCandidate(IGeocodable geocodable, AddressCandidate candidate)
        {
            Debug.Assert(geocodable != null);
            Debug.Assert(candidate != null);

            // Set geolocation.
            geocodable.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(
                candidate.GeoLocation.X, candidate.GeoLocation.Y);
            geocodable.Address.MatchMethod = candidate.Address.MatchMethod ?? string.Empty;

            if (App.Current.Geocoder.AddressFormat == AddressFormat.MultipleFields)
            {
                // Concatenate Full Address from other address fields.
                SetFullAddress(geocodable.Address);
            }
            else if (App.Current.Geocoder.AddressFormat == AddressFormat.SingleField)
            {
                // Do nothing: don't touch user entered\imported Full Address
                // and can't parse Full Address to address parts.
            }
            else
            {
                // Do nothing.
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Checks if the specified address is parsed.
        /// </summary>
        /// <param name="address">The reference to the address object to be checked.</param>
        /// <returns>True if and only if the address was parsed.</returns>
        public static bool IsParsed(Address address)
        {
            Debug.Assert(address != null);

            var manuallyEditedXY = Application.Current.FindString(MANUALLY_EDITED_MATCH_METHOD_KEY);

            // Address of candidates with "Edited X/Y" match method is not need to be parsed.
            // Address with filled parts such as postal code, state etc is not need to be parsed.
            var isParsed =
                string.IsNullOrEmpty(address.FullAddress) ||
                string.Equals(address.MatchMethod, manuallyEditedXY) ||
                !_IsAllAddressPartsFieldsEmpty(address);

            return isParsed;
        }

        /// <summary>
        /// Parse full address property and fill address fields.
        /// </summary>
        /// <param name="addressToParse">Address to parse and fill.</param>
        public static void ParseAndFillAddress(Address addressToParse)
        {
            Debug.Assert(addressToParse != null);

            // Get field mappings for locator of candidate.
            AddressPart[] addressFieldTypes = _GetFieldMappingsForLocator(addressToParse.MatchMethod);

            // Split match_addr.
            string[] fieldsSeparator = new string[1];
            fieldsSeparator[0] = ADDRESS_FIELDS_SEPARATOR;
            string[] addressFields;
            addressFields = addressToParse.FullAddress.Split(fieldsSeparator, StringSplitOptions.None);

            // Fill address.
            if (addressFields.Length != addressFieldTypes.Length)
            {
                // In case of field mappings length is not equals to splitted address parts count -
                // fill maximum possible values.
                string errorFmt = (string)App.Current.FindResource("WrongFieldCountInLocatorInfo");
                string errorMessage = string.Format(errorFmt, addressToParse.MatchMethod);
                Logger.Warning(errorMessage);

                // Special case for errors in locator.
                int fieldCount = addressFieldTypes.Length;
                if (fieldCount > addressFields.Length)
                    fieldCount = addressFields.Length;

                for (int index = 0; index < fieldCount; index++)
                {
                    AddressPart addressPart = addressFieldTypes[index];
                    addressToParse[addressPart] = addressFields[index].Trim();
                }
            }
            else
            {
                // Fill address fields in correct order.
                for (int index = 0; index < addressFields.Length; index++)
                {
                    AddressPart addressPart = addressFieldTypes[index];
                    addressToParse[addressPart] = addressFields[index].Trim();
                }
            }
        }

        /// <summary>
        /// Get candidates from primary locator.
        /// </summary>
        /// <param name="candidatesIncludedDisabledLocators">Candidates from all locators.</param>
        /// <returns>Candidates from primary locator.</returns>
        public static List<AddressCandidate> GetCandidatesFromPrimaryLocators(
            IEnumerable<AddressCandidate> candidatesIncludedDisabledLocators)
        {
            Debug.Assert(candidatesIncludedDisabledLocators != null);

            List<AddressCandidate> candidates = new List<AddressCandidate>();

            // If sublocators is absent - use all candidates.
            if (!App.Current.Geocoder.IsCompositeLocator)
            {
                candidates.AddRange(candidatesIncludedDisabledLocators);
            }
            else
            {
                // Find Primary locators.
                List<LocatorInfo> primarySublocators = new List<LocatorInfo>();
                foreach (LocatorInfo sublocator in App.Current.Geocoder.Locators)
                {
                    if (sublocator.Primary)
                    {
                        primarySublocators.Add(sublocator);
                    }
                }

                if (primarySublocators.Count != 0)
                {
                    // Add to result list only candidates from primary locator.
                    foreach (AddressCandidate candidate in candidatesIncludedDisabledLocators)
                    {
                        if (_IsMatchMethodBelongsToLocators(primarySublocators, candidate.Address.MatchMethod))
                        {
                            candidates.Add(candidate);
                        }
                    }
                }
            }

            return candidates;
        }

        /// <summary>
        /// Sorts candidates collection by index of corresponding primary locator filtering
        /// out candidates without one.
        /// </summary>
        /// <param name="geocoder">The reference to the geocoder object containing locators
        /// to be used.</param>
        /// <param name="candidates">Candidates from all locators.</param>
        /// <returns>Candidates from primary locators sorted by index of their primary locator.
        /// </returns>
        public static IEnumerable<AddressCandidate> SortCandidatesByPrimaryLocators(
            IGeocoder geocoder, IEnumerable<AddressCandidate> candidates)
        {
            Debug.Assert(geocoder != null);
            Debug.Assert(candidates != null);
            Debug.Assert(candidates.All(candidate => candidate != null));

            // If geocoder is ArcGiscomGeocoder.
            var arcgisgeocoder = geocoder as ArcGiscomGeocoder;
            if (arcgisgeocoder != null)
            {
                var bestCandidates = _SelectBestCandidates(candidates);
                // Check that best candidates locator type is in exact locators names collection.
                if (bestCandidates.Any() &&
                    arcgisgeocoder.ExactLocatorsTypesNames.Contains(bestCandidates.First().AddressType))
                    return bestCandidates;
                // If not - return empty collection.
                else
                    return new List<AddressCandidate>();
            }

            // If sublocators is absent - use all candidates.
            if (!geocoder.IsCompositeLocator)
            {
                return candidates.ToList();
            }

            var primarySublocators = geocoder.Locators
                .Where(locator => locator.Primary)
                .ToList();

            // Candidate from local geocoder will have empty locator. 
            var manuallyEditedXY = Application.Current.FindString(MANUALLY_EDITED_MATCH_METHOD_KEY);
            var result =
                from candidate in candidates
                let locatorIndex = primarySublocators.IndexOf(candidate.Locator)
                where locatorIndex >= 0 || candidate.Address.MatchMethod.Equals(manuallyEditedXY)
                orderby locatorIndex
                select candidate;

            return result;
        }

        /// <summary>
        /// Is locator address fields empty.
        /// </summary>
        /// <param name="geocodable">Geocodable object.</param>
        /// <returns>Is locator address fields empty.</returns>
        public static bool IsActiveAddressFieldsEmpty(IGeocodable geocodable)
        {
            bool isEmpty = true;

            foreach (AddressField addressField in App.Current.Geocoder.AddressFields)
            {
                if (!string.IsNullOrEmpty(geocodable.Address[addressField.Type]))
                {
                    isEmpty = false;
                    break;
                }
            }

            return isEmpty;
        }

        /// <summary>
        /// Get candidates from not primary sublocators. City, Zip, Street candidates.
        /// </summary>
        /// <param name="geocodable">Geocodable object to validation</param>
        /// <param name="candidatesIncludedDisabledLocators">Candidates from all locators.</param>
        /// <returns>City, Zip, Street candidates.</returns>
        public static IEnumerable<AddressCandidate> GetBestCandidatesFromNotPrimaryLocators(IGeocoder geocoder,
            IGeocodable geocodable, IEnumerable<AddressCandidate> candidatesIncludedDisabledLocators)
        {
            Debug.Assert(geocodable != null);
            Debug.Assert(candidatesIncludedDisabledLocators != null);

            List<AddressCandidate> candidatesFromNotPrimaryLocators = new List<AddressCandidate>();

            // If current geocoder is ArcGiscomGeocoder.
            var arcgisgeocoder = geocoder as ArcGiscomGeocoder;
            if (arcgisgeocoder != null)
            {
                // If we have candidates - return first of them as a result.
                if (candidatesIncludedDisabledLocators.Any())
                    candidatesFromNotPrimaryLocators.Add(candidatesIncludedDisabledLocators.First());
                
                return candidatesFromNotPrimaryLocators;
            }

            if (App.Current.Geocoder.IsCompositeLocator)
            {
                List<LocatorInfo> sublocators = new List<LocatorInfo>();
                sublocators.AddRange(App.Current.Geocoder.Locators);

                // Get best street candidate.
                List<LocatorInfo> streetLocators = _ExtractLocators(AddressPart.AddressLine, sublocators);
                List<AddressCandidate> streetCandidates = _GetStreetCandidates(geocodable, streetLocators, candidatesIncludedDisabledLocators);

                if (streetCandidates != null)
                    candidatesFromNotPrimaryLocators.AddRange(streetCandidates);

                if (candidatesFromNotPrimaryLocators.Count == 0)
                {
                    // Get best zip candidate.
                    List<LocatorInfo> zipLocators = _ExtractLocators(AddressPart.PostalCode1, sublocators);
                    AddressCandidate bestCandidate = _GetBestCandidateFromLocators(zipLocators, candidatesIncludedDisabledLocators);

                    if (bestCandidate == null)
                    {
                        // Get best cityState candidate.
                        List<LocatorInfo> cityStateLocators = _ExtractLocators(AddressPart.StateProvince, sublocators);
                        bestCandidate = _GetBestCandidateFromLocators(cityStateLocators, candidatesIncludedDisabledLocators);
                    }

                    if (bestCandidate != null)
                    {
                        candidatesFromNotPrimaryLocators.Add(bestCandidate);
                    }
                }
            }

            return candidatesFromNotPrimaryLocators;
        }

        /// <summary>
        /// Get locator type of candidate.
        /// </summary>
        /// <param name="candidate">Address candidate.</param>
        /// <returns>Locator type of candidate. Null in case of not composite locator.</returns>
        public static LocatorType? GetLocatorTypeOfCandidate(AddressCandidate candidate)
        {
            Debug.Assert(candidate != null);

            LocatorType? result = null;
            if (App.Current.Geocoder.IsCompositeLocator)
            {
                List<LocatorInfo> sublocators = new List<LocatorInfo>();
                sublocators.AddRange(App.Current.Geocoder.Locators);

                // Check candidate is from street locator.
                List<LocatorInfo> streetLocators = _ExtractLocators(AddressPart.AddressLine, sublocators);
                if (_IsMatchMethodBelongsToLocators(streetLocators, candidate.Address.MatchMethod))
                {
                    result = LocatorType.Street;
                }
                else
                {
                    // Check candidate is from cityState locator.
                    List<LocatorInfo> cityStateLocators = _ExtractLocators(AddressPart.StateProvince, sublocators);
                    if (_IsMatchMethodBelongsToLocators(cityStateLocators, candidate.Address.MatchMethod))
                    {
                        result = LocatorType.CityState;
                    }
                    else
                    {
                        // Check candidate is from zip locator.
                        List<LocatorInfo> zipLocators = _ExtractLocators(AddressPart.PostalCode1, sublocators);
                        if (_IsMatchMethodBelongsToLocators(zipLocators, candidate.Address.MatchMethod))
                        {
                            result = LocatorType.Zip;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Combine full address from address fields.
        /// </summary>
        /// <param name="address">Address to set field.</param>
        public static void SetFullAddress(Address address)
        {
            string fullAddress = string.Empty;

            fullAddress = _AddAddressPart(address[AddressPart.Unit], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.AddressLine], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.Locality1], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.Locality2], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.Locality3], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.CountyPrefecture], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.StateProvince], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.PostalCode1], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.PostalCode2], fullAddress);
            fullAddress = _AddAddressPart(address[AddressPart.Country], fullAddress);

            address.FullAddress = fullAddress;
        }

        /// <summary>
        /// Extract street name.
        /// </summary>
        /// <param name="fullStreetAddress">Street name. Possibly contains house number.</param>
        /// <returns>Street name without house number.</returns>
        public static string ExtractStreet(string fullStreetAddress)
        {
            return _ExtractStreet(fullStreetAddress);
        }

        /// <summary>
        /// Make string parts like 'We couldn’t find… so we’ve zoomed to…'
        /// If street candidate - use Address Line from geocodable item, otherwise(citystate or zip) use full address.
        /// </summary>
        /// <param name="item">Geocodable item.</param>
        /// <param name="candidateToZoom">First candidate to zoom.</param>
        /// <returns>Geocode result string parts.</returns>
        public static string GetZoomedAddress(IGeocodable item, AddressCandidate candidateToZoom)
        {
            LocatorType? locatorType = GeocodeHelpers.GetLocatorTypeOfCandidate(candidateToZoom);

            string zoomedAddress;

            // If locator type of candidate is street than get Address Line.
            if (locatorType.HasValue && locatorType.Value == LocatorType.Street && 
                !string.IsNullOrEmpty(item.Address.AddressLine))
            {
                // Extract street name.
                zoomedAddress = GeocodeHelpers.ExtractStreet(item.Address.AddressLine);
            }
            else
            {
                // Otherwise use full address.
                zoomedAddress = candidateToZoom.Address.FullAddress;
            }

            return zoomedAddress;
        }
        
        /// <summary>
        /// Get geocodable type(location or order) name by type.
        /// </summary>
        /// <param name="geocodableType">Geocodable type.</param>
        /// <returns>Geocodable type(location or order) name by type. Empty string if non geocodable type.</returns>
        internal static string GetGeocodableTypeName(Type geocodableType)
        {
            string result = string.Empty;

            if (geocodableType == typeof(Location))
            {
                result = (string) App.Current.FindResource(LOCATION_TYPE_NAME);
            }
            else if (geocodableType == typeof(Order))
            {
                result = (string)App.Current.FindResource(ORDER_TYPE_NAME);
            }
            else
            {
                Debug.Assert(false);
            }

            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Select best candidates from collection.
        /// </summary>
        /// <param name="candidates">Collection of candidates.</param>
        /// <returns>Selected best candidates.</returns>
        private static IEnumerable<AddressCandidate> _SelectBestCandidates(
            IEnumerable<AddressCandidate> candidates)
        {
            // Check that collection isn't empty.
            if (!candidates.Any())
                return candidates;

            var first = candidates.First();

            // Return candidates which have the same locator type and score as first candidate.
            return candidates.Where(
                x => x.AddressType == first.AddressType && x.Score == first.Score);
        }

        /// <summary>
        /// Do geocode location using server geocoder.
        /// </summary>
        /// <param name="location">Location to geocode.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Candidates list.</returns>
        private static List<AddressCandidate> _DoGeocodeLocation(Location location, bool includeDisabledLocators)
        {
            List<AddressCandidate> candidates = new List<AddressCandidate>();

            Address address = location.Address;

            // Clean old geocoding info.
            address.MatchMethod = string.Empty;

            if (App.Current.Geocoder.AddressFormat == AddressFormat.MultipleFields)
                address.FullAddress = string.Empty;
            else
            {
                // Do nothing: do not clear full address since
                // it is used for geocoding in Single Address Field Format.
            }

            location.GeoLocation = null;

            // Find all candidates.
            AddressCandidate[] candidatesArr = App.Current.Geocoder.GeocodeCandidates(address, includeDisabledLocators);
            if (candidatesArr != null)
            {
                candidates.AddRange(candidatesArr);
            }

            return candidates;
        }

        /// <summary>
        /// Geocode order, using local geocoder.
        /// </summary>
        /// <param name="order">Order to geocode.</param>
        /// <param name="useLocalAsPrimary">If true and record exists in storage, than dont use server geocoder.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Candidates list.</returns>
        private static List<AddressCandidate> _DoLocalOrderGeocode(Order order, bool useLocalAsPrimary, bool includeDisabledLocators)
        {
            LocalGeocoder localGeocoder = new LocalGeocoder(App.Current.Geocoder, App.Current.NameAddressStorage);

            NameAddress nameAddress = new NameAddress();
            nameAddress.Name = order.Name;
            nameAddress.Address = (Address)order.Address.Clone();

            List<AddressCandidate> candidates = new List<AddressCandidate>();

            AddressCandidate candidateFromLocalGeocoder = localGeocoder.Geocode(nameAddress);

            if (useLocalAsPrimary && candidateFromLocalGeocoder != null)
            {
                candidates.Add(candidateFromLocalGeocoder);
            }
            else
            {
                AddressCandidate[] candidatesArray = localGeocoder.GeocodeCandidates(nameAddress, includeDisabledLocators);
                candidates.AddRange(candidatesArray);
            }

            return candidates;
        }

        /// <summary>
        /// Get street candidate. Extract street name, check that part of street name wasn't removed and try to geocode.
        /// </summary>
        /// <param name="geocodable">Geocodable object to geocode,</param>
        /// <param name="streetLocators">Street locators.</param>
        /// <param name="candidatesIncludedDisabledLocators">Geocoded candidates.</param>
        /// <returns>Street candidate.</returns>
        private static List<AddressCandidate> _GetStreetCandidates(
            IGeocodable geocodable,
            IEnumerable<LocatorInfo> streetLocators,
            IEnumerable<AddressCandidate> candidatesIncludedDisabledLocators)
        {
            Debug.Assert(geocodable != null);
            Debug.Assert(streetLocators != null);
            Debug.Assert(candidatesIncludedDisabledLocators != null);

            List<AddressCandidate> streetCandidates = null;


            if (!string.IsNullOrEmpty(geocodable.Address.AddressLine))
            {
                string street = _ExtractStreet(geocodable.Address.AddressLine);
                if (!street.Equals(geocodable.Address.AddressLine, StringComparison.OrdinalIgnoreCase))
                {
                    // If number removed from street address check that number wasn't part of street name. For example "8th ave".
                    ICloneable cloneableItem = geocodable as ICloneable;
                    IGeocodable geocodableWithoutHouseNumber = (IGeocodable)cloneableItem.Clone();

                    geocodableWithoutHouseNumber.Address.AddressLine = street;

                    // Get candidates for address with removed number.
                    List<AddressCandidate> candidatesForRemovedHouseNumber = DoGeocode(geocodableWithoutHouseNumber, true, false);
                    List<AddressCandidate> streetCandidatesForRemovedHouseNumber = _GetCandidatesFromLocators(streetLocators, candidatesForRemovedHouseNumber);

                    // If best candidate from street locator exists - use it.
                    if (streetCandidatesForRemovedHouseNumber.Count > 0)
                    {
                        streetCandidates = streetCandidatesForRemovedHouseNumber;
                    }
                }
            }

            // If street candidate not set yet - find best candidate from already geocoded candidates.
            if (streetCandidates == null)
            {
                streetCandidates = _GetCandidatesFromLocators(streetLocators, candidatesIncludedDisabledLocators);
            }

            // Fill address fields of street candidate.
            foreach (AddressCandidate candidate in streetCandidates)
            {
                ParseAndFillAddress(candidate.Address);
            }

            return streetCandidates;
        }

        /// <summary>
        /// Extract street name.
        /// </summary>
        /// <param name="fullStreetAddress">Street name. Possibly contains house number.</param>
        /// <returns>Street name without house number.</returns>
        private static string _ExtractStreet(string fullStreetAddress)
        {
            Debug.Assert(fullStreetAddress != null);

            string result = string.Empty;

            if (string.IsNullOrEmpty(fullStreetAddress))
                return result;

            result = fullStreetAddress;
            bool numberRemoved = false;

            // Remove number from the beginning of the string.
            if (char.IsDigit(result[0]))
            {
                int index = 0;
                while (result.Length > index && char.IsDigit(result[index]))
                {
                    index++;
                }

                if (result.Length > index && char.IsWhiteSpace(result[index]))
                {
                    numberRemoved = true;
                    result = result.Remove(0, index);
                }
            }

            // If number not removed, remove number from the end of string.
            if (!numberRemoved && char.IsDigit(result[result.Length - 1]))
            {
                int index = result.Length - 1;
                while (index > 0 && char.IsDigit(result[index]))
                {
                    index--;
                }

                if (index > 0 && char.IsWhiteSpace(result[index]))
                {
                    result = result.Remove(index);
                }
            }

            return result.Trim();
        }

        /// <summary>
        /// Get best candidates from locators.
        /// </summary>
        /// <param name="locators">Locators.</param>
        /// <param name="candidates">Candidates.</param>
        /// <returns>Best candidates from locators.</returns>
        private static AddressCandidate _GetBestCandidateFromLocators(
            IEnumerable<LocatorInfo> locators,
            IEnumerable<AddressCandidate> candidates)
        {
            Debug.Assert(locators != null);
            Debug.Assert(candidates != null);

            AddressCandidate bestCandidate = null;

            foreach (AddressCandidate candidate in candidates)
            {
                // Check candidate from locators.
                bool isCandidateFromLocators = _IsMatchMethodBelongsToLocators(locators, candidate.Address.MatchMethod);

                // If candidate from locators - find best.
                if (isCandidateFromLocators)
                {
                    if (bestCandidate != null)
                    {
                        if (bestCandidate.Score < candidate.Score)
                        {
                            bestCandidate = candidate;
                        }
                    }
                    else
                    {
                        bestCandidate = candidate;
                    }
                }
            }

            return bestCandidate;
        }

        /// <summary>
        /// Get candidates from locators.
        /// </summary>
        /// <param name="locators">Locators.</param>
        /// <param name="candidates">Candidates.</param>
        /// <returns>Candidates from locators.</returns>
        private static List<AddressCandidate> _GetCandidatesFromLocators(
            IEnumerable<LocatorInfo> locators,
            IEnumerable<AddressCandidate> candidates)
        {
            Debug.Assert(locators != null);
            Debug.Assert(candidates != null);

            List<AddressCandidate> candidatesFromLocators = new List<AddressCandidate>();

            foreach (AddressCandidate candidate in candidates)
            {
                // Check candidate from locators.
                bool isCandidateFromLocators = _IsMatchMethodBelongsToLocators(locators, candidate.Address.MatchMethod);

                // If candidate from locators - find best.
                if (isCandidateFromLocators)
                {
                    candidatesFromLocators.Add(candidate);
                }
            }

            return candidatesFromLocators;
        }

        /// <summary>
        /// Extract locators, which depends mainly on requested addressPart.
        /// </summary>
        /// <param name="addressPart">Address part.</param>
        /// <param name="sublocators">Sublocators.</param>
        /// <returns>locators, which depends mainly on requested addressPart.</returns>
        private static List<LocatorInfo> _ExtractLocators(AddressPart addressPart, List<LocatorInfo> sublocators)
        {
            Debug.Assert(sublocators != null);

            List<LocatorInfo> extractedLocators = new List<LocatorInfo>();

            // Get locators, which depends mainly on requested addressPart.
            foreach (LocatorInfo locatorInfo in sublocators)
            {
                foreach (AddressPart field in locatorInfo.InternalFields)
                {
                    if (field == addressPart)
                    {
                        extractedLocators.Add(locatorInfo);
                        break;
                    }
                }
            }

            // REV: method shouldn't affect sublocators param, it is input and it is not evident that this param can be changed.
            // Remove extracted locators.
            foreach (LocatorInfo sublocatorInfo in extractedLocators)
            {
                sublocators.Remove(sublocatorInfo);
            }

            return extractedLocators;
        }

        /// <summary>
        /// Is match method belongs to locators.
        /// </summary>
        /// <param name="locators">Locators.</param>
        /// <param name="matchMethod">Match method.</param>
        /// <returns>Is match method belongs to locators.</returns>
        private static bool _IsMatchMethodBelongsToLocators(
            IEnumerable<LocatorInfo> locators,
            string matchMethod)
        {
            Debug.Assert(locators != null);
            Debug.Assert(matchMethod != null);

            bool result = false;

            foreach (LocatorInfo locatorInfo in locators)
            {
                if (_IsMatchMethodBelongsToLocator(locatorInfo, matchMethod))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Is match method belongs to locator.
        /// </summary>
        /// <param name="sublocator">Sublocator.</param>
        /// <param name="matchMethod">Match method.</param>
        /// <returns>Is match method belongs to locator.</returns>
        private static bool _IsMatchMethodBelongsToLocator(LocatorInfo sublocator, string matchMethod)
        {
            Debug.Assert(sublocator != null);
            Debug.Assert(matchMethod != null);

            bool result = sublocator.Name.Equals(matchMethod, StringComparison.OrdinalIgnoreCase) ||
                              sublocator.Title.Equals(matchMethod, StringComparison.OrdinalIgnoreCase);

            return result;
        }

        /// <summary>
        /// Get field mappings for locator.
        /// </summary>
        /// <param name="matchMethod">Locator name or title.</param>
        /// <returns>Field mappings.</returns>
        private static AddressPart[] _GetFieldMappingsForLocator(string matchMethod)
        {
            List<AddressPart> addressFieldTypes = new List<AddressPart>();

            // If composite locator.
            if (!App.Current.Geocoder.IsCompositeLocator || string.IsNullOrEmpty(matchMethod))
            {
                _FillByInputFieldMappings(addressFieldTypes);
            }
            else
            {
                bool locatorFound = false;

                // Get field mapping for composite locator.
                foreach (LocatorInfo sublocator in App.Current.Geocoder.Locators)
                {
                    if (_IsMatchMethodBelongsToLocator(sublocator, matchMethod))
                    {
                        _FillByInternalFieldMappings(addressFieldTypes, sublocator.InternalFields);
                        locatorFound = true;
                        break;
                    }
                }

                if (!locatorFound)
                {
                    string errorMessage = string.Format((string)App.Current.FindResource("SublocatorInfoIsAbsent"), matchMethod);
                    throw new SettingsException(errorMessage);
                }
            }

            return addressFieldTypes.ToArray();
        }

        /// <summary>
        /// Fill address parts list from field mappings of internal locators.
        /// </summary>
        /// <param name="addressFieldTypes">Address parts list.</param>
        /// <param name="internalFields">Internal locator fields.</param>
        private static void _FillByInternalFieldMappings(List<AddressPart> addressFieldTypes,
            AddressPart[] internalFields)
        {
            Debug.Assert(addressFieldTypes != null);
            Debug.Assert(internalFields != null);

            foreach (AddressPart field in internalFields)
            {
                addressFieldTypes.Add(field);
            }
        }

        /// <summary>
        /// Fill address parts list from field mappings of input locators.
        /// </summary>
        /// <param name="addressFieldTypes">Address parts list.</param>
        private static void _FillByInputFieldMappings(List<AddressPart> addressFieldTypes)
        {
            Debug.Assert(addressFieldTypes != null);

            foreach (AddressField addressField in App.Current.Geocoder.AddressFields)
            {
                addressFieldTypes.Add(addressField.Type);
            }
        }

        /// <summary>
        /// Add address part to full address.
        /// </summary>
        /// <param name="addressPart">Address part.</param>
        /// <param name="fullAddress">Full address.</param>
        /// <returns>Full address with added value.</returns>
        private static string _AddAddressPart(string addressPart, string fullAddress)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(fullAddress);

            if (!string.IsNullOrEmpty(addressPart))
            {
                if (fullAddress.Length > 0)
                {
                    stringBuilder.Append(DELIMETER);
                }

                stringBuilder.Append(addressPart);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Check is all address fields is empty.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>Is all fields empty.</returns>
        private static bool _IsAllAddressPartsFieldsEmpty(Address address)
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

        #region Private constants

        /// <summary>
        /// Maximum address candidate score.
        /// </summary>
        private const int MAXIMUM_SCORE = 100;

        /// <summary>
        /// Separator between address fields in full address.
        /// </summary>
        private const string ADDRESS_FIELDS_SEPARATOR = ",";

        /// <summary>
        /// Delimeter for combining full address property.
        /// </summary>
        private const string DELIMETER = ", ";

        /// <summary>
        /// Location type resource name.
        /// </summary>
        private const string LOCATION_TYPE_NAME = "Location";

        /// <summary>
        /// Order type resource name.
        /// </summary>
        private const string ORDER_TYPE_NAME = "Order";

        /// <summary>
        /// The resource key for a match method for addresses with manually edited coordinates.
        /// </summary>
        private const string MANUALLY_EDITED_MATCH_METHOD_KEY = "ManuallyEditedXY";

        /// <summary>
        /// Resource name of geocoding service name.
        /// </summary>
        private const string GEOCODING_SERVICE_NAME_RESOURCE_NAME = "ServiceNameGeocoding";

        #endregion
    }
}
