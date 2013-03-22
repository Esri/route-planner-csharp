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

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Encapsulates license expiration date checking.
    /// </summary>
    internal interface ILicenseExpirationChecker
    {
        /// <summary>
        /// Checks if the license is about to expire.
        /// </summary>
        /// <param name="license">The license to be checked.</param>
        /// <param name="currentDate">The current date to check license for.</param>
        /// <returns>True if and only if the license is about to expire.</returns>
        bool LicenseIsExpiring(License license, DateTime? currentDate);

        /// <summary>
        /// Checks if the license is about to expire and corresponding warning
        /// should be shown to the user.
        /// </summary>
        /// <param name="username">The name of the user the license was issued for.</param>
        /// <param name="license">The license to be checked.</param>
        /// <param name="currentDate">The current date to check license for.</param>
        /// <returns>True if and only if the license is about to expire and
        /// expiration warning should be shown to the user.</returns>
        bool RequiresExpirationWarning(
            string username,
            License license,
            DateTime? currentDate);
    }
}
