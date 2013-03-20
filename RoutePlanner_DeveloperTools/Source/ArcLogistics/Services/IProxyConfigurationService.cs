namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Provides access to proxy configuration.
    /// </summary>
    internal interface IProxyConfigurationService
    {
        /// <summary>
        /// Gets a reference to current proxy settings.
        /// </summary>
        ProxySettings Settings
        {
            get;
        }

        /// <summary>
        /// Applies current proxy settings in an implementation-specific way.
        /// </summary>
        void Update();
    }
}
