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

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides access to REST service contexts.
    /// </summary>
    internal interface IRestServiceContextProvider
    {
        /// <summary>
        /// Initializes REST service connection context.
        /// </summary>
        /// <param name="knownTypes">Collection of types that might be present
        /// in REST service requests and responses.</param>
        /// <returns>The new instance of the REST service context.</returns>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within ArcGIS server providing REST service.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to establish connection to the server providing REST service.</exception>
        RestServiceContext InitializeContext(IEnumerable<Type> knownTypes);
    }
}
