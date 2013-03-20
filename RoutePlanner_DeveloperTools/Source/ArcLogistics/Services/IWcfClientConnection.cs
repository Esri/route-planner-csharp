using System;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides access to the WCF service client channel.
    /// </summary>
    /// <typeparam name="TService">The type of the service interface to provide channel for.
    /// </typeparam>
    internal interface IWcfClientConnection<TService> : IDisposable
    {
        /// <summary>
        /// Gets reference to the client-side WCF service interface.
        /// </summary>
        TService Client
        {
            get;
        }
    }
}
