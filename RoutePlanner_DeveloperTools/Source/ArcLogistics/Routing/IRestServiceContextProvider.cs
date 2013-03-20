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
