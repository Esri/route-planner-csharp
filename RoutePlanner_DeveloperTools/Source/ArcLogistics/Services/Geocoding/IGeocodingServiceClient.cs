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
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Services.Geocoding
{
    /// <summary>
    /// Provides access to the World geocoder service.
    /// </summary>
    internal interface IGeocodingServiceClient
    {
        #region events
        /// <summary>
        /// Occurs when asynchronous reverse geocoding completes.
        /// </summary>
        event EventHandler<AsyncOperationCompletedEventArgs<ReverseGeocodeResponse>>
            ReverseGeocodeCompleted;
        #endregion

        #region methods
        /// <summary>
        /// Cancels asynchronous operation associated with the specified object.
        /// </summary>
        /// <param name="userState">The user state object to cancel asynchronous operation
        /// for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="userState"/> is a null
        /// reference.</exception>
        void CancelAsync(object userState);

        /// <summary>
        /// Searches the best match for the specified address.
        /// </summary>
        /// <param name="address">Single line address to find.</param>
        /// <param name="country">The country in which the address resides. For best results,
        /// this parameter should be specified when possible.</param>
        /// <param name="outputSpatialReference">The spatial reference in which to return address
        /// points. The default is WKID 102100.</param>
        /// <returns>The best match for the specified address.</returns>
        FindAddressCandidateResponse FindAddress(
            string address,
            string country,
            int? outputSpatialReference);

        /// <summary>
        /// Searches for a list of address candidate points for the specified address.
        /// </summary>
        /// <param name="address">Single line address to find.</param>
        /// <param name="country">The country in which the address resides. For best results,
        /// this parameter should be specified when possible.</param>
        /// <param name="outputSpatialReference">The spatial reference in which to return address
        /// points. The default is WKID 102100.</param>
        /// <returns>A list of address candidate points for the specified address.</returns>
        FindAddressCandidatesReponse FindAddressCandidates(
            string address,
            string country,
            int? outputSpatialReference);

        /// <summary>
        /// Searches for an address corresponding to the specified location.
        /// </summary>
        /// <param name="location">The point at which to search for the closest address.</param>
        /// <param name="outputSpatialReference">The spatial reference in which to return address
        /// points. The default is WKID 4326.</param>
        /// <param name="distance">The distance in meters from the given location within which
        /// a matching address should be searched. The default is 0.</param>
        /// <returns>A reverse geocoded address and its exact location.</returns>
        ReverseGeocodeResponse ReverseGeocode(
            Point location,
            int? outputSpatialReference,
            double? distance);

        /// <summary>
        /// Begins an asynchronous operation to reverse geocode the specified location.
        /// </summary>
        /// <param name="location">The point at which to search for the closest address.</param>
        /// <param name="outputSpatialReference">The spatial reference in which to return address
        /// points. The default is WKID 4326.</param>
        /// <param name="distance">The distance in meters from the given location within which
        /// a matching address should be searched. The default is 0.</param>
        /// <param name="userState">An arbitrary user state object which can be used for
        /// asynchronous operation cancellation.</param>
        void ReverseGeocodeAsync(
            Point location,
            int? outputSpatialReference,
            double? distance,
            object userState);
        #endregion
    }
}
