using System;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents view model for the "Expired" state of the license information
    /// view.
    /// </summary>
    internal sealed class ExpiredStateViewModel
    {
        #region constructor
        /// <summary>
        /// Initializes a new instance of the ExpiredStateViewModel class.
        /// </summary>
        /// <param name="licenseManager">The reference to the license manager
        /// object providing necessary license information.</param>
        /// <param name="licensePageCommands">The reference to the common license
        /// page command container object.</param>
        public ExpiredStateViewModel(
            ILicenseManager licenseManager,
            ILicensePageCommands licensePageCommands,
            Action startSingleVehicleMode)
        {
            Debug.Assert(licenseManager != null);
            Debug.Assert(licensePageCommands != null);
            Debug.Assert(startSingleVehicleMode != null);

            this.UpgradeLicenseCommand = licensePageCommands.UpgradeLicense;
            this.SingleVehicleModeCommand = new DelegateCommand(
                _ => startSingleVehicleMode());

            _InitContent(licenseManager.ExpiredLicense);
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
        /// Gets header text for the view.
        /// </summary>
        public string ViewHeader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets view content document.
        /// </summary>
        public FlowDocument Content
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the single vehicle mode command.
        /// </summary>
        public ICommand SingleVehicleModeCommand
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
        private void _InitContent(License license)
        {
            this.ViewHeader = App.Current.FindString(VIEW_HEADER_KEY);

            var warningString = App.Current.FindString(CONTENT_KEY);
            var content = (FlowDocument)XamlReaderHelpers.Load(warningString);

            var vehicleCountPart = (Run)content.FindName(VEHICLE_COUNT_NOTES_PART);
            if (license == null)
            {
                vehicleCountPart.Text = string.Empty;
            }
            else
            {
                vehicleCountPart.Text = string.Format(
                    vehicleCountPart.Text,
                    license.PermittedRouteNumber);
            }

            this.Content = content;
        }
        #endregion

        #region private constants
        /// <summary>
        /// The name of the resource storing license expiration notes header.
        /// </summary>
        private const string VIEW_HEADER_KEY = "LicenseExpiredNotesHeader";

        /// <summary>
        /// The name of the resource storing license expiration notes.
        /// </summary>
        private const string CONTENT_KEY = "LicenseExpiredNotes";

        /// <summary>
        /// The name of the 'vehicle count' part of the license notes.
        /// </summary>
        private const string VEHICLE_COUNT_NOTES_PART = "LicenseVehicleCount";
        #endregion
    }
}
