using System;
using System.Collections.Generic;
using System.Net;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides context for REST requests.
    /// </summary>
    internal interface IRestRequestContext
    {
        /// <summary>
        /// Gets an instance of the connection to the ArcGIS server hosting
        /// REST service.
        /// </summary>
        AgsServerConnection Connection
        {
            get;
        }

        /// <summary>
        /// Gets collection of types that might be present
        /// in REST service requests and responses.
        /// </summary>
        IEnumerable<Type> KnownTypes
        {
            get;
        }

        /// <summary>
        /// Gets or sets a cookie identifying current session.
        /// </summary>
        Cookie SessionCookie
        {
            get;
            set;
        }
    }
}
