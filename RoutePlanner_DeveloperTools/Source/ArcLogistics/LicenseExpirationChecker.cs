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
using System.Diagnostics;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Default implementation for the <see cref="T:ESRI.ArcLogistics.App.ILicenseExpirationChecker"/>
    /// interface.
    /// </summary>
    internal sealed class LicenseExpirationChecker : ILicenseExpirationChecker
    {
        #region constructors
        /// <summary>
        /// Initializes new instance of the LicenseExpirationChecker class.
        /// </summary>
        /// <param name="licenseCacheStorage">The reference to the license cache
        /// storage object.</param>
        public LicenseExpirationChecker(ILicenseCacheStorage licenseCacheStorage)
        {
            Debug.Assert(licenseCacheStorage != null);

            _licenseCacheStorage = licenseCacheStorage;
        }
        #endregion

        #region ILicenseExpirationChecker Members
        /// <summary>
        /// Checks if the license is about to expire.
        /// </summary>
        /// <param name="license">The license to be checked.</param>
        /// <param name="currentDate">The current date to check license for.</param>
        /// <returns>True if and only if the license is about to expire.</returns>
        public bool LicenseIsExpiring(License license, DateTime? currentDate)
        {
            var daysLeft = _GetDaysLeft(license, currentDate);
            if (daysLeft <= FIRST_EXPIRATION_WARNING_END)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the license is about to expire and corresponding warning
        /// should be shown to the user.
        /// </summary>
        /// <param name="username">The name of the user the license was issued for.</param>
        /// <param name="license">The license to be checked.</param>
        /// <param name="currentDate">The current date to check license for.</param>
        /// <returns>True if and only if the license is about to expire and
        /// expiration warning should be shown to the user.</returns>
        public bool RequiresExpirationWarning(
            string username,
            License license,
            DateTime? currentDate)
        {
            if (string.IsNullOrEmpty(username))
            {
                return false;
            }

            var daysLeft = _GetDaysLeft(license, currentDate);
            if (daysLeft == null)
            {
                return false;
            }

            Debug.Assert(license != null);
            if (daysLeft > FIRST_EXPIRATION_WARNING_END)
            {
                return false;
            }

            var licenseCache = _licenseCacheStorage.Load();
            LicenseCacheEntry cacheEntry = null;
            licenseCache.Entries.TryGetValue(username, out cacheEntry);
            if (daysLeft > FIRST_EXPIRATION_WARNING_START)
            {
                var showWarning =
                    cacheEntry == null ||
                    cacheEntry.License != license ||
                    !cacheEntry.LicenseExpirationWarningWasShown;
                if (showWarning)
                {
                    licenseCache.Entries[username] = new LicenseCacheEntry()
                    {
                        LicenseExpirationWarningWasShown = true,
                        License = license,
                    };

                    _licenseCacheStorage.Save(licenseCache);

                    return true;
                }

                return false;
            }

            if (daysLeft <= FIRST_EXPIRATION_WARNING_START)
            {
                return true;
            }

            return false;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Gets days left before license will expire.
        /// </summary>
        /// <param name="license">The license to be checked.</param>
        /// <param name="currentDate">The current date to check license for.</param>
        /// <returns><see cref="T:System.TimeSpan"/> instance storing number of
        /// days left before license will expire or null if license will never
        /// expire.</returns>
        private static TimeSpan? _GetDaysLeft(License license, DateTime? currentDate)
        {
            if (license == null || license.ExpirationDate == null || currentDate == null)
            {
                return null;
            }

            var expirationDate = license.ExpirationDate.Value;
            var daysLeft = expirationDate.Date - currentDate.Value.Date;

            return daysLeft;
        }
        #endregion

        #region private constants
        /// <summary>
        /// The minimum number of days before license expiration when first
        /// warning could be displayed.
        /// </summary>
        private static readonly TimeSpan FIRST_EXPIRATION_WARNING_START =
            new TimeSpan(7, 0, 0, 0);

        /// <summary>
        /// The maximum number of days before license expiration when first
        /// warning could be displayed.
        /// </summary>
        private static readonly TimeSpan FIRST_EXPIRATION_WARNING_END =
            new TimeSpan(14, 0, 0, 0);
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the current license cache storage object.
        /// </summary>
        private ILicenseCacheStorage _licenseCacheStorage;
        #endregion
    }
}
