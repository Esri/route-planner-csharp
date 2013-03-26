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
