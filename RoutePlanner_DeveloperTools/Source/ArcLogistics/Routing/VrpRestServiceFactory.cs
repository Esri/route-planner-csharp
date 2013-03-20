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
