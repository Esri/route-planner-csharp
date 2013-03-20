namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Specifies options influencing services used by the application.
    /// </summary>
    internal enum ServiceOptions
    {
        /// <summary>
        /// Indicates that default options should be used.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Indicates that support for synchronous VRP service should be enabled.
        /// </summary>
        UseSyncVrp = 0x01,
    }
}
