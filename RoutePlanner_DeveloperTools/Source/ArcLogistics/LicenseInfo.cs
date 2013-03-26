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
    /// Provides license information
    /// </summary>
    public sealed class LicenseInfo
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the LicenseInfo class.
        /// </summary>
        public LicenseInfo()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the LicenseInfo class.
        /// </summary>
        /// <param name="license">The reference to the license object.</param>
        /// <param name="licenseValidationDate">The date/time of the license validation.</param>
        public LicenseInfo(License license, DateTime? licenseValidationDate)
        {
            this.License = license;
            this.LicenseValidationDate = licenseValidationDate;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a reference to the license object.
        /// </summary>
        public License License
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets date and time when the <see cref="P:License"/> was validated.
        /// </summary>
        public DateTime? LicenseValidationDate
        {
            get;
            private set;
        }
        #endregion
    }
}
