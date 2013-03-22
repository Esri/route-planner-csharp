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
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Discovery service interface.
    /// </summary>
    internal interface IDiscoveryService
    {
        /// <summary>
        /// Initializes discovery service.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Validates directory server state.
        /// </summary>
        void ValidateServerState();

        /// <summary>
        /// Gets full map extent from discovery service.
        /// </summary>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Full map extent.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="knownTypes"/> is null reference.</exception>
        GPEnvelope GetFullMapExtent(IEnumerable<Type> knownTypes);

        /// <summary>
        /// Gets geographic region name.
        /// </summary>
        /// <param name="request">Discovery request.</param>
        /// <param name="knownTypes">Collection of known types to parse result.</param>
        /// <returns>Region name if successfully found, otherwise - empty string.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="request"/> or <paramref name="knownTypes"/> is null reference.
        /// </exception>
        string GetRegionName(SubmitDiscoveryRequest request, IEnumerable<Type> knownTypes);
    }
}
