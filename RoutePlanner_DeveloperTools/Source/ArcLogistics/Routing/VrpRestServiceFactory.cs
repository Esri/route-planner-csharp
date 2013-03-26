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

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Creates instances of the VRP REST service clients.
    /// </summary>
    internal sealed class VrpRestServiceFactory
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the VrpRestServiceFactory class.
        /// </summary>
        /// <param name="syncContextProvider">The reference to the context provider object
        /// for synchronous VRP service.</param>
        /// <param name="asyncContextProvider">The reference to the context provider object
        /// for asynchronous VRP service.</param>
        /// <param name="soapUrl">SOAP GP service url.</param>
        public VrpRestServiceFactory(
            IRestServiceContextProvider syncContextProvider,
            IRestServiceContextProvider asyncContextProvider,
            string soapUrl)
        {
            Debug.Assert(syncContextProvider != null);
            Debug.Assert(asyncContextProvider != null);
            Debug.Assert(soapUrl != null);

            _syncContextProvider = syncContextProvider;
            _asyncContextProvider = asyncContextProvider;
            _soapUrl = soapUrl;
        }
        #endregion constructors

        #region public methods
        /// <summary>
        /// Creates new instance of the VRP REST service client.
        /// </summary>
        /// <param name="knownTypes">Collection of types that might be present
        /// in REST service requests and responses.</param>
        /// <returns>new instance of the VRP REST service client.</returns>
        public IVrpRestService CreateService(IEnumerable<Type> knownTypes)
        {
            var asyncContext = _asyncContextProvider.InitializeContext(knownTypes);
            var syncContext = _syncContextProvider.InitializeContext(knownTypes);

            return new RestVrpService(syncContext, asyncContext, _soapUrl);
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the context provider object for synchronous VRP service.
        /// </summary>
        private IRestServiceContextProvider _syncContextProvider;

        /// <summary>
        /// The reference to the context provider object for asynchronous VRP service.
        /// </summary>
        private IRestServiceContextProvider _asyncContextProvider;

        /// <summary>
        /// SOAP GP service url necessary for jobs cancellation.
        /// </summary>
        private string _soapUrl;
        #endregion
    }
}
