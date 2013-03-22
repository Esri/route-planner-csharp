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
using System.Net;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Implements REST request context.
    /// </summary>
    internal sealed class RequestContext : IRestRequestContext
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RequestContext class with
        /// the specified connection and known types.
        /// </summary>
        /// <param name="connection">Instance of the connection to the ArcGIS
        /// server hosting REST service.</param>
        /// <param name="knownTypes">collection of types that might be present
        /// in REST service requests and responses.</param>
        public RequestContext(
            AgsServerConnection connection,
            IEnumerable<Type> knownTypes)
        {
            Debug.Assert(connection != null);

            Connection = connection;
            KnownTypes = knownTypes;
        }
        #endregion

        #region IRestRequestContext Members
        /// <summary>
        /// Gets an instance of the connection to the ArcGIS server hosting
        /// REST service.
        /// </summary>
        public AgsServerConnection Connection
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets collection of types that might be present
        /// in REST service requests and responses.
        /// </summary>
        public IEnumerable<Type> KnownTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a cookie identifying current session.
        /// </summary>
        public Cookie SessionCookie
        {
            get;
            set;
        }
        #endregion
    }

}
