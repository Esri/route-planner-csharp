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
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.DomainObjects;
using AppData = ESRI.ArcLogistics.Data;
using AppGeometry = ESRI.ArcLogistics.Geometry;

using ESRI.ArcLogistics.App.Geocode;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class that provides geocode functionality.
    /// </summary>
    internal sealed class Geocoder
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>Geocoder</c> class.
        /// </summary>
        /// <param name="informer">Progress informer.</param>
        public Geocoder(IProgressInformer informer)
        {
            Debug.Assert(null != informer); // created

            _informer = informer;
        }

        #endregion // Constructors

        #region Public types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Geocoding type.
        /// </summary>
        public enum GeocodeType
        {
            /// <summary>
            /// Not supported geocoding.
            /// </summary>
            NotSet,
            /// <summary>
            /// Reverse geocoding.
            /// </summary>
            Reverse,
            /// <summary>
            /// Batch geocoding.
            /// </summary>
            Batch,
            /// <summary>
            /// Full source geocoding.
            /// </summary>
            Complete
        }

        #endregion // Public types

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Count of geocoded objects.
        /// Valid after Geocode call.
        /// </summary>
        public int GeocodedCount
        {
            get { return _geocodedCount; }
        }

        /// <summary>
        /// Detected exception.
        /// Valid after Geocode call.
        /// </summary>
        public Exception Exception
        {
            get { return _detectedException; }
        }

        /// <summary>
        /// Geocode procedure detail list.
        /// Valid after Geocode call.
        /// </summary>
        public IList<MessageDetail> Details
        {
            get { return _details; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Geocodes objects
        /// </summary>
        /// <param name="objects">Objects to geocoding.</param>
        /// <param name="type">Geocoding type.</param>
        /// <param name="checker">Cancellation checker.</param>
        public void Geocode(IList<AppData.DataObject> objects,
                            GeocodeType type,
                            ICancellationChecker checker)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _GetGeocodable(objects[0])); // valid call
            Debug.Assert(null != checker); // created

            // reset internal state first
            _detectedException = null;
            _geocodedCount = 0;
            _details.Clear();

            // store checker
            _checker = checker;

            // start geocode
            _Geocode(objects, type);
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets geocodable interface from data object.
        /// </summary>
        /// <param name="obj">Data object to conversion.</param>
        /// <returns>Geocodable interface.</returns>
        private IGeocodable _GetGeocodable(AppData.DataObject obj)
        {
            var geocodable = obj as IGeocodable;
            Debug.Assert(null != geocodable); // valid type
            return geocodable;
        }

        /// <summary>
        /// Updates geocodable information.
        /// </summary>
        /// <param name="needMessage">Need show message.</param>
        /// <param name="candidate">Geocoded candidate (can be NULL).</param>
        /// <param name="obj">Source object to geocoded candidate.</param>
        private void _UpdateGeocodableInfo(bool needMessage,
                                           AddressCandidate candidate,
                                           AppData.DataObject obj)
        {
            Debug.Assert(null != obj); // Created.

            IGeocodable geocodable = _GetGeocodable(obj);
            geocodable.GeoLocation = null;

            if ((null == candidate) || (candidate.Score <= App.Current.Geocoder.MinimumMatchScore))
            {
                // Not geocoded.
                geocodable.Address.MatchMethod = string.Empty;
            }
            else
            {
                GeocodeHelpers.SetCandidate(geocodable, candidate);
                ++_geocodedCount;

                // Store warning.
                if (needMessage)
                {
                    string objectName = _informer.ObjectName;
                    string errorTextFormat =
                        App.Current.GetString("ImportProcessStatusRecordOutMapExtendGeocodedFormat",
                                              objectName.ToLower(),
                                              "{0}",
                                              objectName);
                    var description =
                        new MessageDetail(MessageType.Warning, errorTextFormat, obj);
                    _details.Add(description);
                }
            }
        }

        /// <summary>
        /// Parses batch geocoding result. Set geocode candidates values to relative object.
        /// </summary>
        /// <param name="candidates">Candidates from geocoding.</param>
        /// <param name="objects">Objects to geocoding.</param>
        private void _ParseBatchGeocodeResult(AddressCandidate[] candidates,
                                              IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(null != candidates); // created
            Debug.Assert(candidates.Length == objects.Count); // valid state
            Debug.Assert(null != _checker); // inited

            // valid check extent
            var importExtent = App.Current.Map.ImportCheckExtent;

            // init progress
            int count = objects.Count;
            for (int index = 0; index < count; ++index)
            {
                _checker.ThrowIfCancellationRequested();

                // get current object
                AppData.DataObject obj = objects[index];
                IGeocodable geocodable = _GetGeocodable(obj);

                bool needMessage = false;
                if (geocodable.IsGeocoded)
                {   // add message - Could not locate object using X/Y attributes.
                    // Order was located using address information instead
                    Debug.Assert(!importExtent.IsPointIn(geocodable.GeoLocation.Value));
                    needMessage = true;
                }

                AddressCandidate candidate = candidates[index];
                _UpdateGeocodableInfo(needMessage, candidate, obj);
            }
        }

        /// <summary>
        /// Creates address list for input objects.
        /// </summary>
        /// <param name="objects">Source object.</param>
        /// <returns>Created address list for input objects.</returns>
        /// <remarks>Progress show.</remarks>
        private Address[] _CreateAddressList(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _checker); // inited

            // init progress
            int count = objects.Count;
            var addresses = new Address[count];
            for (int index = 0; index < count; ++index)
            {
                _checker.ThrowIfCancellationRequested();

                IGeocodable geocodable = _GetGeocodable(objects[index]);

                addresses[index] = geocodable.Address;
            }

            return addresses;
        }

        /// <summary>
        /// Creates named address list for input objects.
        /// </summary>
        /// <param name="objects">Source object.</param>
        /// <returns>Created named address list for input objects.</returns>
        /// <remarks>Progress show.</remarks>
        private NameAddress[] _CreateNamedAddressList(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _checker); // inited

            // init progress
            int count = objects.Count;
            var addresses = new NameAddress[count];
            for (int index = 0; index < count; ++index)
            {
                _checker.ThrowIfCancellationRequested();

                IGeocodable geocodable = _GetGeocodable(objects[index]);
                var nameAddress = new NameAddress();
                nameAddress.Name = geocodable.ToString();
                nameAddress.Address = geocodable.Address;
                addresses[index] = nameAddress;
            }

            return addresses;
        }

        /// <summary>
        /// Validates geolocation candidates by streets geocoder.
        /// </summary>
        /// <param name="addresses">Address to geocode.</param>
        /// <param name="candidates">Geocded candidates.</param>
        private void _ValidateLocation(Address[] addresses, AddressCandidate[] candidates)
        {
            Debug.Assert(null != addresses); // created
            Debug.Assert(null != candidates); // created
            Debug.Assert(candidates.Length == addresses.Length); // valid stated
            Debug.Assert(null != _checker); // inited

            // init location validator
            var streetsGeocoder = App.Current.StreetsGeocoder;
            var locationValidator = new LocationValidator(streetsGeocoder);

            // do validation
            var incorrectCandidates = locationValidator
                .FindIncorrectLocations(candidates)
                .GroupBy(index => addresses[index])
                .ToArray();

            // get incorrect candidate indexes
            var addressesForGeocoding = incorrectCandidates
                .Select(item => item.Key)
                .ToArray();

            _checker.ThrowIfCancellationRequested();

            // regeocoding by streets geocoder
            var fixedCandidates = streetsGeocoder.BatchGeocode(addressesForGeocoding);

            // update regeocoded candidates
            for (var index = 0; index < fixedCandidates.Length; ++index)
            {
                _checker.ThrowIfCancellationRequested();
                foreach (var j in incorrectCandidates[index])
                {
                    candidates[j] = fixedCandidates[index];
                }
            }
        }

        /// <summary>
        /// Does batch geocoding.
        /// </summary>
        /// <param name="objects">Objects to geocoding.</param>
        private void _BatchGeocode(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _checker); // inited

            try
            {
                // create list of addresses
                Address[] addresses = _CreateAddressList(objects);

                // start geocode
                App currentApp = App.Current;
                AddressCandidate[] candidates = null;
                if (objects[0] is Order)
                {   // for orders - use local geocoding
                    NameAddress[] namedAddress = _CreateNamedAddressList(objects);

                    var localGeocoder = new LocalGeocoder(currentApp.Geocoder,
                                                          currentApp.NameAddressStorage);
                    candidates = localGeocoder.BatchGeocode(namedAddress);
                }
                else
                {   // for other object - use server geocoder
                    candidates = currentApp.Geocoder.BatchGeocode(addresses);
                }

                _checker.ThrowIfCancellationRequested();

                // validate geocode
                _ValidateLocation(addresses, candidates);

                // If current geocoder is arcgiscomgeocoder - check that we got
                // candidates from "good" locators.                
                var arcgisgeocoder = App.Current.Geocoder as ArcGiscomGeocoder;
                if (arcgisgeocoder != null)
                {
                    for (int i = 0; i < candidates.Count(); i++)
                    {
                        var goodAddressType =
                            arcgisgeocoder.ExactLocatorsTypesNames.Contains(candidates[i].AddressType);
                        if (!goodAddressType)
                            candidates[i] = new AddressCandidate();
                    }
                }

                // parse result
                _ParseBatchGeocodeResult(candidates, objects);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                // store exception
                _detectedException = ex;
            }
        }

        /// <summary>
        /// Does object reverse geocoding safelly.
        /// </summary>
        /// <param name="point">Point to geocoding.</param>
        /// <returns>Address by point or NULL.</returns>
        private Address _ReverseGeocodeSave(AppGeometry.Point point)
        {
            Debug.Assert(null != point); // created

            Address geocodedAddress = null;
            try
            {
                geocodedAddress = App.Current.Geocoder.ReverseGeocode(point);
            }
            catch (Exception ex)
            {
                // store exception
                _detectedException = ex;
            }

            return geocodedAddress;
        }

        /// <summary>
        /// Does objects reverse geocoding.
        /// </summary>
        /// <param name="objects">Objects to geocoding.</param>
        private void _ReverseGeocode(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _checker); // inited

            AppGeometry.Envelope extent = App.Current.Map.ImportCheckExtent;

            int count = objects.Count;
            for (int index = 0; index < count; ++index)
            {
                _checker.ThrowIfCancellationRequested();

                if (null == _detectedException)
                {   // NOTE: do if geocoder in valid state
                    IGeocodable geocodable = _GetGeocodable(objects[index]);
                    if (geocodable.GeoLocation.HasValue)
                    {
                        if (extent.IsPointIn(geocodable.GeoLocation.Value))
                        {   // reverse geocode
                            Address geocodedAddress =
                                _ReverseGeocodeSave(geocodable.GeoLocation.Value);
                            if (null != geocodedAddress)
                            {
                                geocodedAddress.CopyTo(geocodable.Address);
                                ++_geocodedCount;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Does empty geocoding.
        /// </summary>
        /// <param name="objects">Objects to geocoding.</param>
        private void _EmptyGeocode(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _checker); // inited

            int count = objects.Count;
            for (int index = 0; index < objects.Count; ++index)
            {
                _checker.ThrowIfCancellationRequested();

                IGeocodable geocodable = _GetGeocodable(objects[index]);
                geocodable.GeoLocation = null;
                Debug.Assert(null != geocodable.Address); // created
                geocodable.Address.MatchMethod = string.Empty;
            }
        }

        /// <summary>
        /// Check is location of geocodable object valid and replace it by candidate from local
        /// geocoder if exists.
        /// </summary>
        /// <param name="geocodable">Geocodable object to check.</param>
        /// <returns>Is geocodable object valid.</returns>
        private bool _SourceGeocodedObject(IGeocodable geocodable)
        {
            Debug.Assert(null != geocodable); // created

            bool isObjectGeocoded = false;

            // First check if name address pair exists in local geocoder database.
            // If exists - overwrite geolocation and address from it.
            App currentApp = App.Current;
            var localGeocoder =
                new LocalGeocoder(currentApp.Geocoder, currentApp.NameAddressStorage);

            var nameAddress = new NameAddress();
            nameAddress.Name = geocodable.ToString();
            nameAddress.Address = geocodable.Address.Clone() as Address;

            AddressCandidate localCandidate = localGeocoder.Geocode(nameAddress);
            if (localCandidate == null)
            {   // Update from internal object information.
                AppGeometry.Envelope extent = currentApp.Map.ImportCheckExtent;
                if (extent.IsPointIn(geocodable.GeoLocation.Value))
                {
                    geocodable.Address.MatchMethod =
                        currentApp.FindString("ImportSourceMatchMethod");
                    isObjectGeocoded = true;

                    // Full address property must be combined from other fields, because
                    // it is not importing field.
                    if (App.Current.Geocoder.AddressFormat == AddressFormat.MultipleFields)
                        GeocodeHelpers.SetFullAddress(geocodable.Address);
                    else
                    {
                        // Do nothing: do not update full address since
                        // it is used for geocoding in Single Address Field Format.
                    }
                }
                // Else could not locate object using X/Y attributes - need geocode.
            }
            else
            {   // Init from local candidate.
                // Set location.
                geocodable.GeoLocation = new AppGeometry.Point(localCandidate.GeoLocation.X,
                                                               localCandidate.GeoLocation.Y);
                // Set address.
                localCandidate.Address.CopyTo(geocodable.Address);

                isObjectGeocoded = true;
            }

            return isObjectGeocoded;
        }

        /// <summary>
        /// Does source geocoding.
        /// </summary>
        /// <param name="objects">Objects to geocoding.</param>
        private void _SourceGeocode(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _checker); // inited

            // init progress
            int count = objects.Count;

            // geocode process
            var ungeocoded = new List<AppData.DataObject>();
            for (int index = 0; index < count; ++index)
            {
                _checker.ThrowIfCancellationRequested();

                AppData.DataObject obj = objects[index];
                IGeocodable geocodable = _GetGeocodable(obj);

                bool isObjectGeocoded = false;
                if (geocodable.IsGeocoded)
                {
                    // check is geocodable object valid and set candidate
                    // from local geocoder if exists
                    isObjectGeocoded = _SourceGeocodedObject(geocodable);
                    if (isObjectGeocoded)
                    {
                        ++_geocodedCount;
                    }
                }

                if (!isObjectGeocoded)
                    ungeocoded.Add(obj);
            }

            _checker.ThrowIfCancellationRequested();

            // geocode all ungeocoded object by batch geocode
            if (0 < ungeocoded.Count)
                _BatchGeocode(ungeocoded);
        }

        /// <summary>
        /// Start geocode.
        /// </summary>
        /// <param name="objects">Objects to geocoding.</param>
        /// <param name="type">Geocode type.</param>
        private void _Geocode(IList<AppData.DataObject> objects, GeocodeType type)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != _checker); // inited

            _checker.ThrowIfCancellationRequested();

            switch (type)
            {
                case GeocodeType.Batch:
                    _BatchGeocode(objects);
                    break;

                case GeocodeType.Reverse:
                    _ReverseGeocode(objects);
                    break;

                case GeocodeType.Complete:
                    _SourceGeocode(objects);
                    break;

                case GeocodeType.NotSet:
                    _EmptyGeocode(objects);
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Maximum address candidate score.
        /// </summary>
        private const int MAXIMUM_SCORE = 100;

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Progress tracker.
        /// </summary>
        private IProgressInformer _informer;
        /// <summary>
        /// Cancellation checker.
        /// </summary>
        private ICancellationChecker _checker;

        /// <summary>
        /// Geocoded objects count.
        /// </summary>
        private int _geocodedCount;
        /// <summary>
        /// Detected exception.
        /// </summary>
        private Exception _detectedException;
        /// <summary>
        /// Process detail list.
        /// </summary>
        private List<MessageDetail> _details = new List<MessageDetail> ();

        #endregion // Private fields
    }
}
