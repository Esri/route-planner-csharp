using System;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implements <see cref="T:ILicenseCacheStorage"/> using application settings storage.
    /// </summary>
    internal class ApplicationSettingsLicenseCacheStorage : ILicenseCacheStorage
    {
        #region ILicenseCacheStorage Members
        /// <summary>
        /// Loads license cache from the application settings storage.
        /// </summary>
        /// <returns>Reference to the loaded license cache object.</returns>
        public LicenseCache Load()
        {
            return _storage.Load();
        }

        /// <summary>
        /// Saves license cache to the application settings storage.
        /// </summary>
        /// <param name="licenseCache">The reference to the license cache object
        /// to be saved.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="licenseCache"/> is a null reference.</exception>
        public void Save(LicenseCache licenseCache)
        {
            if (licenseCache == null)
            {
                throw new ArgumentNullException("licenseCache");
            }

            _storage.Save(licenseCache);
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the storage object to be used for storing license cache.
        /// </summary>
        private readonly IGenericStorage<LicenseCache> _storage =
            new ApplicationSettingsGenericStorage<LicenseCache>(_ => _.LicenseCache);
        #endregion
    }
}
