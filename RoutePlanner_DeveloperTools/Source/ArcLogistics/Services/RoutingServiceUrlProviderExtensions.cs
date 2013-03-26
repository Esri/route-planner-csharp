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
using System.Diagnostics;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Helper extensions for <see cref="IRoutingServiceUrlProvider"/> interface.
    /// </summary>
    internal static class RoutingServiceUrlProviderExtensions
    {
        /// <summary>
        /// Makes service url using <see cref="IRoutingServiceUrlProvider"/>
        /// instance to obtain a server url and path and query components from
        /// <paramref name="baseUrl"/>.
        /// </summary>
        /// <param name="self">An <see cref="IRoutingServiceUrlProvider"/> to
        /// obtain server url from.</param>
        /// <param name="baseUrl">A url whose path and query components
        /// will be used for making service url.</param>
        /// <returns>A new routing service url.</returns>
        public static string MakeServiceUrl(
            this IRoutingServiceUrlProvider self,
            string baseUrl)
        {
            Debug.Assert(self != null, "Expects a non-null service url provider.");

            return UriHelper.Concat(self.QueryServiceUrl(), baseUrl);
        }
    }
}
