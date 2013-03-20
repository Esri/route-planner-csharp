namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Provides access to loading/saving license cache instances.
    /// </summary>
    internal interface ILicenseCacheStorage
    {
        /// <summary>
        /// Loads license cache from implementation specific storage.
        /// </summary>
        /// <returns>Reference to the loaded license cache object.</returns>
        LicenseCache Load();

        /// <summary>
        /// Saves license cache to the implementation specific storage.
        /// </summary>
        /// <param name="licenseCache">The reference to the license cache object
        /// to be saved.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="licenseCache"/> is a null reference.</exception>
        void Save(LicenseCache licenseCache);
    }
}
