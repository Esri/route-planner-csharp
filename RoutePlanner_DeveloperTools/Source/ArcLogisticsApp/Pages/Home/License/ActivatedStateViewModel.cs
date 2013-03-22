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
using System.Windows.Documents;
using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents view model for the "Activated" state of the license information
    /// view.
    /// </summary>
    internal sealed class ActivatedStateViewModel
    {
        #region constructor
        /// <summary>
        /// Initializes a new instance of the ActivatedStateViewModel class.
        /// </summary>
        /// <param name="licenseManager">The reference to the license manager
        /// object providing necessary license information.</param>
        /// <param name="licensePageCommands">The reference to the common license
        /// page command container object.</param>
        /// <param name="showExpirationWarning">A value indicating if the license
        /// expiration warning should be displayed.</param>
        public ActivatedStateViewModel(
            ILicenseManager licenseManager,
            ILicensePageCommands licensePageCommands,
            bool showExpirationWarning)
        {
            Debug.Assert(licenseManager != null);
            Debug.Assert(licensePageCommands != null);

            this.UpgradeLicenseCommand = licensePageCommands.UpgradeLicense;
            this.ShowExpirationWarning = showExpirationWarning;

            var license = licenseManager.AppLicense;
            var currentDate = licenseManager.AppLicenseValidationDate;
            _InitContent(license, currentDate);
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets reference to the license upgrade command.
        /// </summary>
        public ICommand UpgradeLicenseCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating if the license expiration warning should be
        /// displayed.
        /// </summary>
        public bool ShowExpirationWarning
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets header text for the view.
        /// </summary>
        public string ViewHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets view content first paragraph document.
        /// </summary>
        public FlowDocument ContentFirstParagraph
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets view content second paragraph document.
        /// </summary>
        public FlowDocument ContentSecondParagraph
        {
            get;
            private set;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Initializes content document and header.
        /// </summary>
        /// <param name="license">The reference to the license object.</param>
        /// <param name="currentDate">The current date/time value.</param>
        private void _InitContent(License license, DateTime? currentDate)
        {
            if (license == null || license.ExpirationDate == null || currentDate == null)
            {
                return;
            }

            var expirationDate = license.ExpirationDate.Value;
            var daysLeft = (int)(expirationDate.Date - currentDate.Value.Date).TotalDays;
            daysLeft = Math.Max(daysLeft, 0);

            this.ViewHeader = App.Current.GetString(VIEW_HEADER_KEY, daysLeft);

            var warningString = App.Current.FindString(CONTENT_KEY);
            var content = (FlowDocument)XamlReaderHelpers.Load(warningString);

            var daysLeftPart = (Run)content.FindName(DAYS_LEFT_NOTES_PART);
            daysLeftPart.Text = string.Format(
                daysLeftPart.Text,
                daysLeft);

            var vehicleCountPart = (Run)content.FindName(VEHICLE_COUNT_NOTES_PART);
            vehicleCountPart.Text = string.Format(
                vehicleCountPart.Text,
                license.PermittedRouteNumber);

            this.ContentFirstParagraph = FlowDocumentHelpers.ExtractFirstBlock(content);
            this.ContentSecondParagraph = content;
        }
        #endregion

        #region private constants
        /// <summary>
        /// The name of the resource storing license subscription notes header.
        /// </summary>
        private const string VIEW_HEADER_KEY = "LicenseSubscriptionNotesHeader";

        /// <summary>
        /// The name of the resource storing license subscription notes.
        /// </summary>
        private const string CONTENT_KEY = "LicenseSubscriptionNotes";

        /// <summary>
        /// The name of the 'days left' part of the license subscription notes.
        /// </summary>
        private const string DAYS_LEFT_NOTES_PART = "LicenseDaysLeft";

        /// <summary>
        /// The name of the 'vehicle count' part of the license subscription notes.
        /// </summary>
        private const string VEHICLE_COUNT_NOTES_PART = "LicenseVehicleCount";
        #endregion
    }
}
