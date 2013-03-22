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
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// RestRouteService class.
    /// </summary>
    internal class RestRouteService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RestRouteService class with
        /// the specified Routing service url, Routing layer name and ArcGIS
        /// server hosting routing service.
        /// </summary>
        /// <param name="serviceUrl">The Routing service url.</param>
        /// <param name="layerName">The name of the Routing layer.</param>
        /// <param name="server">ArcGIS server instance hosting routing service.</param>
        public RestRouteService(
            string serviceUrl,
            string layerName,
            AgsServer server)
        {
            Debug.Assert(server != null);

            _server = server;
            _baseUrl = UriHelper.Concat(serviceUrl, layerName);
            _baseUrl = UriHelper.Concat(_baseUrl, QUERY_OBJ_SOLVE);
            _restService = new RestService();
        }

        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RouteSolveResponse Solve(
            RouteSolveRequest request,
            IEnumerable<Type> knownTypes)
        {
            Debug.Assert(request != null);

            var context = new RequestContext(_server.OpenConnection(), knownTypes);
            var url = UriHelper.Concat(context.Connection.Url, _baseUrl);
            string query = RestHelper.BuildQueryString(request, knownTypes,
                false);

            HttpRequestOptions opt = new HttpRequestOptions();
            opt.Method = HttpMethod.Post;
            opt.UseGZipEncoding = true;
            opt.Timeout = DEFAULT_REQ_TIMEOUT;

            return _restService.SendRequest<RouteSolveResponse>(context, url, query, opt);
        }
        #endregion public methods

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // URL query objects
        private const string QUERY_OBJ_SOLVE = "solve";

        // timeouts (milliseconds)
        private const int DEFAULT_REQ_TIMEOUT = 15 * 60 * 1000;
        #endregion

        #region private fields
        /// <summary>
        /// ArcGIS server instance hosting routing service.
        /// </summary>
        private AgsServer _server;

        /// <summary>
        /// Url to the Routing REST service.
        /// </summary>
        private string _baseUrl;

        /// <summary>
        /// The reference to the rest service object to be used for sending requests.
        /// </summary>
        private RestService _restService;
        #endregion
    }
}
