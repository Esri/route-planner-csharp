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
    /// Encapsulates access to the LicenseManager facilities.
    /// </summary>
    internal interface ILicenseManager
    {
        /// <summary>
        /// See <see cref="P:ESRI.ArcLogistics.App.LicenseManager.LicenseComponent"/>
        /// property for more details.
        /// </summary>
        ILicenser LicenseComponent
        {
            get;
        }

        /// <summary>
        /// See <see cref="P:ESRI.ArcLogistics.App.LicenseManager.AppLicense"/>
        /// property for more details.
        /// </summary>
        License AppLicense
        {
            get;
        }

        /// <summary>
        /// Gets a date/time of the <see cref="P:AppLicense"/> validation.
        /// </summary>
        DateTime? AppLicenseValidationDate
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the license when it is expired.
        /// </summary>
        License ExpiredLicense
        {
            get;
        }

        /// <summary>
        /// See <see cref="P:ESRI.ArcLogistics.App.LicenseManager.AuthorizedUserName"/>
        /// property for more details.
        /// </summary>
        string AuthorizedUserName
        {
            get;
        }

        /// <summary>
        /// See <see cref="P:ESRI.ArcLogistics.App.LicenseManager.HaveStoredCredentials"/>
        /// property for more details.
        /// </summary>
        bool HaveStoredCredentials
        {
            get;
        }

        /// <summary>
        /// Gets current license activation status.
        /// </summary>
        LicenseActivationStatus LicenseActivationStatus
        {
            get;
        }

        /// <summary>
        /// Gets a reference to the license expiration checker object.
        /// </summary>
        ILicenseExpirationChecker LicenseExpirationChecker
        {
            get;
        }

        /// <summary>
        /// See <see cref="M:ESRI.ArcLogistics.App.LicenseManager.ActivateLicenseOnStartup"/>
        /// method for more details.
        /// </summary>
        void ActivateLicenseOnStartup();

        /// <summary>
        /// See <see cref="M:ESRI.ArcLogistics.App.LicenseManager.ActivateLicense"/>
        /// method for more details.
        /// </summary>
        void ActivateLicense(
            string username,
            string password,
            bool saveCredentials);

        /// <summary>
        /// Activates free license.
        /// </summary>
        void ActivateFreeLicense();
    }
}
