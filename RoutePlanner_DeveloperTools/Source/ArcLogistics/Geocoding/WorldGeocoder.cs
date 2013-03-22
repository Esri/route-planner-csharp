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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Geocoding;
using ESRI.ArcLogistics.Services.Serialization;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Geocoding
{
    /// <summary>
    /// Inherits <see cref="GeocoderBase"/> class and reloads its members
    /// for the World geocoder REST service.
    /// </summary>
    internal sealed class WorldGeocoder : GeocoderBase
    {
        #region public static methods
        /// <summary>
        /// Creates a new instance of the <see cref="WorldGeocoder"/> class for the specified
        /// service configuration.
        /// </summary>
        /// <param name="serviceInfo">The instance of the geocoding service configuration
        /// specifying World geocoder service to create geocoder for.</param>
        /// <param name="exceptionHandler">Exception handler.</param>
        /// <returns>A new instance of the <see cref="WorldGeocoder"/> class.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="serviceInfo"/> is a null
        /// reference.</exception>
        public static GeocoderBase CreateWorldGeocoder(GeocodingServiceInfo serviceInfo,
            IServiceExceptionHandler exceptionHandler)
        {
            CodeContract.RequiresNotNull("serviceInfo", serviceInfo);

            // Create binding for the geocoder REST service.
            var webBinding = ServiceHelper.CreateWebHttpBinding("WorldGeocoder");
            var binding = new CustomBinding(webBinding);
            var messageEncodingElement = binding.Elements.Find<WebMessageEncodingBindingElement>();
            messageEncodingElement.ContentTypeMapper = new ArcGisWebContentTypeMapper();

            // Create endpoint for the geocoder REST service.
            var contract = ContractDescription.GetContract(typeof(IGeocodingService));
            var serviceAddress = new EndpointAddress(serviceInfo.RestUrl);
            var endpoint = new WebHttpEndpoint(contract, serviceAddress);
            endpoint.Binding = binding;

            // Replace default endpoint behavior with a customized one.
            endpoint.Behaviors.Remove<WebHttpBehavior>();
            endpoint.Behaviors.Add(new GeocodingServiceWebHttpBehavior());

            // Create the geocoder instance.
            var channelFactory = new WebChannelFactory<IGeocodingService>(endpoint);
            var client = new GeocodingServiceClient(channelFactory, serviceInfo, exceptionHandler);

            return new WorldGeocoder(serviceInfo, client);
        }
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldGeocoder"/> class.
        /// </summary>
        /// <param name="serviceInfo">The geocoding service configuration information.</param>
        /// <param name="client">The geocoding service client object.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="serviceInfo"/> or
        /// <paramref name="client"/> is a null reference.</exception>
        public WorldGeocoder(
            GeocodingServiceInfo serviceInfo,
            IGeocodingServiceClient client)
        {
            CodeContract.RequiresNotNull("serviceInfo", serviceInfo);
            CodeContract.RequiresNotNull("client", client);

            _serviceInfo = serviceInfo;
            _client = client;

            _client.ReverseGeocodeCompleted += _ClientReverseGeocodeCompleted;

            _locatorsInfos = new ReadOnlyCollection<LocatorInfo>(new List<LocatorInfo>());
        }
        #endregion

        #region Internal methods

        /// <summary>
        /// Is geocoder initialized.
        /// </summary>
        /// <returns>Always true.</returns>
        internal override bool IsInitialized()
        {
            return true;
        }

        #endregion

        #region GeocoderBase Members
        /// <summary>
        /// Occurs when asynchronous reverse geocoding operation completes.
        /// </summary>
        public override event AsyncReverseGeocodedEventHandler AsyncReverseGeocodeCompleted = delegate { };

        /// <summary>
        /// Gets a value indicating the minimum score value for an address candidate
        /// retrieved via geocoding to be treated as matched. 
        /// </summary>
        public override int MinimumMatchScore
        {
            get
            {
                return _serviceInfo.MinimumMatchScore.GetValueOrDefault(80);
            }
        }

        /// <summary>
        /// Gets a reference to the collection of locators information for composite locators or
        /// an empty collection for non-composite ones.
        /// </summary>
        public override ReadOnlyCollection<LocatorInfo> Locators
        {
            get
            {
                return _locatorsInfos;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the geocoder represents composite locator.
        /// </summary>
        public override bool IsCompositeLocator
        {
            get
            {
                return _serviceInfo.IsCompositeLocator;
            }
        }

        /// <summary>
        /// Gets geocoders address fields collection.
        /// </summary>
        public override AddressField[] AddressFields
        {
            get
            {
                var result = EnumerableEx.Return(ADDRESS_FIELD).ToArray();

                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating which address format is used.
        /// </summary>
        public override AddressFormat AddressFormat
        {
            get
            {
                return AddressFormat.SingleField;
            }
        }

        /// <summary>
        /// Geocodes an address and returns the best candidate.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <returns>Returns address candidate with max score.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="address"/> is a null
        /// reference.</exception>
        public override AddressCandidate Geocode(Address address)
        {
            CodeContract.RequiresNotNull("address", address);

            var response = _client.FindAddress(address.FullAddress, address.Country, null);

            return _Convert(response);
        }

        /// <summary>
        /// Geocodes array of addresses.
        /// </summary>
        /// <param name="addresses">Array of addresses.</param>
        /// <returns>Returns array of best candidates for each input address.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="addresses"/> or any of
        /// its elements is a null reference.</exception>
        public override AddressCandidate[] BatchGeocode(Address[] addresses)
        {
            // "addresses" array elements will be validated by the Geocode method, so check only
            // the array itself here.
            CodeContract.RequiresNotNull("addresses", addresses);

            var candidates = new List<AddressCandidate>();
            foreach (var address in addresses)
            {
                var candidate = this.Geocode(address);
                if (candidate != null)
                {
                    candidates.Add(candidate);
                }
            }

            return candidates.ToArray();
        }

        /// <summary>
        /// Geocodes address and returns array of found candidates.
        /// </summary>
        /// <param name="address">Address to geocode.</param>
        /// <param name="includeDisabledLocators">Is need to add candidates from disabled locators.</param>
        /// <returns>Returns array of found address candidates.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="address"/> is a null
        /// reference.</exception>
        public override AddressCandidate[] GeocodeCandidates(Address address, bool includeDisabledLocators)
        {
            CodeContract.RequiresNotNull("address", address);

            var response = _client.FindAddressCandidates(
                address.FullAddress,
                address.Country,
                null);

            if (response == null)
                return null;

            // Convert address candidates to the internal application objects.
            var candidates = response.Candidates
                .Select(_Convert)
                .ToArray();

            return candidates;
        }

        /// <summary>
        /// Finds address by geographical location.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <returns>Returns found address.</returns>
        public override Address ReverseGeocode(Geometry.Point location)
        {
            var response = _client.ReverseGeocode(location, null, null);

            if (response == null)
                return null;

            return new Address
            {
                FullAddress = response.Address,
            };
        }

        /// <summary>
        /// Finds address by geographical location. Asynchronous method.
        /// </summary>
        /// <param name="location">Location point.</param>
        /// <param name="userToken">Geocoding operation token.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="userToken"/> is a null
        /// reference.</exception>
        public override void ReverseGeocodeAsync(Geometry.Point location, object userToken)
        {
            CodeContract.RequiresNotNull("userToken", userToken);

            _client.ReverseGeocodeAsync(location, null, null, userToken);
        }

        /// <summary>
        /// Cancels asynchronous reverse geocoding operation.
        /// </summary>
        /// <param name="userToken">Token of the geocoding operation that must be cancelled.</param>
        /// <returns>Returns <c>true</c> if operation successfully cancelled, or <c>false</c> if
        /// operation with the token was not found.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="userToken"/> is a null
        /// reference.</exception>
        public override bool ReverseGeocodeAsyncCancel(object userToken)
        {
            CodeContract.RequiresNotNull("userToken", userToken);

            _client.CancelAsync(userToken);

            return true;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Converts the specified find address candidate response object to an address candidate
        /// object.
        /// </summary>
        /// <param name="response">The instance of the find address candidate response
        /// to be converted.</param>
        /// <returns>A new <see cref="AddressCandidate"/> object based on information from
        /// the response.</returns>
        private AddressCandidate _Convert(FindAddressCandidateResponse response)
        {
            if (response == null || response.Score == null ||
                response.Address == null || response.Location == null ||
                response.Extent == null)
            {
                return null;
            }

            var result = new AddressCandidate
            {
                // Convert candidate match score.
                Score = response.Score.GetValueOrDefault(),

                // Convert candidate address.
                Address = new Address
                {
                    FullAddress = response.Address,
                },

                // Convert candidate geolocation.
                GeoLocation = new Geometry.Point
                {
                    X = response.Location.X,
                    Y = response.Location.Y,
                }
            };

            return result;
        }

        /// <summary>
        /// Handles asynchronous reverse geocoding completion.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments object.</param>
        private void _ClientReverseGeocodeCompleted(
            object sender,
            AsyncOperationCompletedEventArgs<ReverseGeocodeResponse> e)
        {
            // Do nothing if we get no result.
            if (e.Cancelled)
            {
                return;
            }

            // There is just no way to report about errors occurred, so just log it and do nothing.
            if (e.Error != null)
            {
                Logger.Warning(e.Error);

                return;
            }

            if (e.Result == null || e.Result.Address == null)
            {
                return;
            }

            // Translate received event into this.AsyncReverseGeocodeCompleted event.
            var address = new Address
            {
                FullAddress = e.Result.Address,
            };

            var eventArguments = new AsyncReverseGeocodedEventArgs(
                address,
                e.Result.Location,
                e.UserState);
            this.AsyncReverseGeocodeCompleted(this, eventArguments);
        }
        #endregion

        #region private constants
        /// <summary>
        /// The address field object for the World geocoder.
        /// </summary>
        private static readonly AddressField ADDRESS_FIELD = new AddressField(
            "address",
            AddressPart.FullAddress,
            true,
            string.Empty);
        #endregion

        #region private fields
        /// <summary>
        /// Geocoding service configuration object.
        /// </summary>
        private GeocodingServiceInfo _serviceInfo;

        /// <summary>
        /// Geocoding REST service client object.
        /// </summary>
        private IGeocodingServiceClient _client;

        /// <summary>
        /// Location information collection.
        /// </summary>
        private ReadOnlyCollection<LocatorInfo> _locatorsInfos;
        #endregion
    }
}
