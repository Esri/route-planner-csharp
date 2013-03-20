using System;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Implements <see cref="T:ESRI.ArcLogistics.Routing.IRestServiceContextProvider"/>
    /// interface by always returning null reference for the context.
    /// </summary>
    internal sealed class NullRestServiceContextProvider : IRestServiceContextProvider
    {
        #region IRestServiceContextProvider Members
        /// <summary>
        /// Does nothing but returns null reference immediately.
        /// </summary>
        /// <param name="knownTypes">Collection of types that might be present
        /// in REST service requests and responses.</param>
        /// <returns>The null reference.</returns>
        public RestServiceContext InitializeContext(IEnumerable<Type> knownTypes)
        {
            return null;
        }
        #endregion
    }
}
