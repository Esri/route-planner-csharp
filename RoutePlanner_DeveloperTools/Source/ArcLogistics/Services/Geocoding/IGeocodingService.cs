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
using System.ServiceModel;
using System.ServiceModel.Web;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Services.Geocoding
{
    /// <summary>
    /// Represents World geocoder REST service contract.
    /// </summary>
    [ServiceContract]
    internal interface IGeocodingService
    {
        /// <summary>
        /// Searches the best match for the specified address.
        /// </summary>
        /// <param name="address">Single line address to find.</param>
        /// <param name="country">The country in which the address resides. For best results,
        /// this parameter should be specified when possible.</param>
        /// <param name="outSr">The spatial reference in which to return address points.
        /// The default is WKID 102100.</param>
        /// <returns>The best match for the specified address.</returns>
        [OperationContract]
        [WebGet(
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/LocationServer/findAddress?" +
                "address={address}&" +
                "country={country}&" +
                "outSR={outSr}&" +
                "bbox=&" +
                "localCode=&" +
                "f=json")]
        FindAddressCandidateResponse FindAddress(string address, string country, int? outSr);

        /// <summary>
        /// Searches for a list of address candidate points for the specified address.
        /// </summary>
        /// <param name="address">Single line address to find.</param>
        /// <param name="country">The country in which the address resides. For best results,
        /// this parameter should be specified when possible.</param>
        /// <param name="outSr">The spatial reference in which to return address
        /// points. The default is WKID 102100.</param>
        /// <returns>A list of address candidate points for the specified address.</returns>
        [OperationContract]
        [WebGet(
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/LocationServer/findAddressCandidates?" +
                "address={address}&" +
                "outSR={outSr}&" +
                "country={country}&" +
                "bbox=&" +
                "max=&" +
                "localeCode=&" +
                "f=json")]
        FindAddressCandidatesReponse FindAddressCandidates(
            string address,
            string country,
            int? outSr);

        /// <summary>
        /// Searches for an address corresponding to the specified location.
        /// </summary>
        /// <param name="location">The point at which to search for the closest address.</param>
        /// <param name="outSr">The spatial reference in which to return address
        /// points. The default is WKID 4326.</param>
        /// <param name="distance">The distance in meters from the given location within which
        /// a matching address should be searched. The default is 0.</param>
        /// <returns>A reverse geocoded address and its exact location.</returns>
        [OperationContract]
        [WebGet(
            BodyStyle = WebMessageBodyStyle.Bare,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/LocationServer/findLocation?" +
                "location={location}&" +
                "outSR={outSr}&" +
                "distance={distance}&" +
                "f=json")]
        ReverseGeocodeResponse ReverseGeocode(Point location, int? outSr, double? distance);
    }
}
