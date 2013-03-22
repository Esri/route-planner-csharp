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
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Default implementation for the
    /// <see cref="T:ESRI.ArcLogistics.Routing.IRestServiceContextProvider"/>.
    /// </summary>
    internal sealed class RestServiceContextProvider : IRestServiceContextProvider
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RestServiceContextProvider class
        /// with the specified REST service url, VRP tool name and ArcGIS server
        /// instance.
        /// </summary>
        /// <param name="serviceUrl">Url of the VRP REST service.</param>
        /// <param name="toolName">VRP service tool name.</param>
        /// <param name="server">ArcGIS server instance the VRP service is located
        /// at.</param>
        public RestServiceContextProvider(
            string serviceUrl,
            string toolName,
            AgsServer server)
        {
            Debug.Assert(!string.IsNullOrEmpty(serviceUrl));
            Debug.Assert(!string.IsNullOrEmpty(toolName));
            Debug.Assert(server != null);

            _baseUrl = UriHelper.Concat(serviceUrl, toolName);
            _server = server;
        }
        #endregion

        #region IRestServiceContextProvider Members
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
        public RestServiceContext InitializeContext(IEnumerable<Type> knownTypes)
        {
            var context = new RequestContext(_server.OpenConnection(), knownTypes);
            var url = UriHelper.Concat(context.Connection.Url, _baseUrl);

            return new RestServiceContext(context, url);
        }
        #endregion

        #region private fields
        /// <summary>
        /// Url of the VRP REST service to be used.
        /// </summary>
        private string _baseUrl;

        /// <summary>
        /// ArcGIS server instance to be used to connect to the VRP services.
        /// </summary>
        private AgsServer _server;
        #endregion
    }
}
