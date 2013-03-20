using System.Linq;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Input;
using ESRI.ArcLogistics.Utility.ComponentModel;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// The model for the connected state of the license page.
    /// </summary>
    internal sealed class ConnectedStateViewModel : NotifyPropertyChangedBase
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the ConnectedStateViewModel class.
        /// </summary>
        /// <param name="licensePageCommands">The reference to the common license
        /// page command container object.</param>
        /// <param name="licenseInfoText">The reference to the flow documents
        /// collection representing license information text.</param>
        public ConnectedStateViewModel(
            ILicensePageCommands licensePageCommands,
            IEnumerable<FlowDocument> licenseInfoText)
        {
            Debug.Assert(licensePageCommands != null);
            Debug.Assert(licenseInfoText != null);
            Debug.Assert(licenseInfoText.All(document => document != null));

            this.LicenseInfoText = licenseInfoText.ToList();

            this.UpdateLicenseCommand = licensePageCommands.UpgradeLicense;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets a reference to the document with license information
        /// text.
        /// </summary>
        public List<FlowDocument> LicenseInfoText
        {
            get
            {
                return _licenseInfoText;
            }

            private set
            {
                if (_licenseInfoText != value)
                {
                    _licenseInfoText = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LICENSE_INFO_TEXT);
                }
            }
        }

        /// <summary>
        /// Gets or sets a reference to the user switching command.
        /// </summary>
        public ICommand SwitchUserCommand
        {
            get
            {
                return _switchUserCommand;
            }
            set
            {
                if (_switchUserCommand != value)
                {
                    _switchUserCommand = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_SWITCH_USER_COMMAND);
                }
            }
        }

        /// <summary>
        /// Gets or sets a reference to the license updating command.
        /// </summary>
        public ICommand UpdateLicenseCommand
        {
            get
            {
                return _updateLicenseCommand;
            }
            private set
            {
                if (_updateLicenseCommand != value)
                {
                    _updateLicenseCommand = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_UPDATE_LICENSE_COMMAND);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the license updating command property has a valid value.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_UPDATE_LICENSE_COMMAND)]
        public bool HasUpdateLicenseCommand
        {
            get
            {
                return this.UpdateLicenseCommand != null;
            }
        }
        #endregion

        #region private constants
        /// <summary>
        /// Name of the LicenseInfoText property.
        /// </summary>
        private const string PROPERTY_NAME_LICENSE_INFO_TEXT = "LicenseInfoText";

        /// <summary>
        /// Name of the SwitchUserCommand property.
        /// </summary>
        private const string PROPERTY_NAME_SWITCH_USER_COMMAND = "SwitchUserCommand";

        /// <summary>
        /// Name of the UpdateLicenseCommand property.
        /// </summary>
        private const string PROPERTY_NAME_UPDATE_LICENSE_COMMAND = "UpdateLicenseCommand";
        #endregion

        #region private fields
        /// <summary>
        /// Stores value of the LicenseInfoText property.
        /// </summary>
        private List<FlowDocument> _licenseInfoText;

        /// <summary>
        /// Stores value of the SwitchUserCommand property.
        /// </summary>
        private ICommand _switchUserCommand;

        /// <summary>
        /// Stores value of the UpdateLicenseCommand property.
        /// </summary>
        private ICommand _updateLicenseCommand;
        #endregion
    }
}
