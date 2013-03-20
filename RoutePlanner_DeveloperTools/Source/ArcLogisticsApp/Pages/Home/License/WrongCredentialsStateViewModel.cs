using System.Diagnostics;
using System.Windows.Documents;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents view model for the "Wrong Credentials" state of the license information
    /// view.
    /// </summary>
    internal sealed class WrongCredentialsStateViewModel
    {
        #region constructor
        /// <summary>
        /// Initializes a new instance of the ActivatedStateViewModel class.
        /// </summary>
        /// <param name="licensePageCommands">The reference to the common license
        /// page command container object.</param>
        public WrongCredentialsStateViewModel(
            ILicensePageCommands licensePageCommands)
        {
            Debug.Assert(licensePageCommands != null);

            _InitContent(licensePageCommands);
        }
        #endregion

        #region public properties
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
        /// <param name="licensePageCommands">The reference to the common license
        /// page command container object.</param>
        private void _InitContent(ILicensePageCommands licensePageCommands)
        {
            Debug.Assert(licensePageCommands != null);

            this.ViewHeader = App.Current.FindString(VIEW_HEADER_KEY);

            var warningString = App.Current.FindString(CONTENT_KEY);
            var content = (FlowDocument)XamlReaderHelpers.Load(warningString);

            var command = licensePageCommands.RecoverCredentials;
            var recoverCredentialsPart = (Section)content.FindName(RECOVER_CREDENTIALS_PART);
            if (command == null)
            {
                recoverCredentialsPart.Blocks.Clear();
            }
            else
            {
                var recoverCredentialsLink = (Hyperlink)recoverCredentialsPart.FindName(
                    RECOVER_CREDENTIALS_LINK_NAME);
                recoverCredentialsLink.Command = command;
            }

            this.ContentFirstParagraph = FlowDocumentHelpers.ExtractFirstBlock(content);
            this.ContentSecondParagraph = content;
        }
        #endregion

        #region private constants
        /// <summary>
        /// The name of the resource storing wrong credentials notes header.
        /// </summary>
        private const string VIEW_HEADER_KEY = "LicenseWrongCredentialsNotesHeader";

        /// <summary>
        /// The name of the resource storing wrong credentials notes.
        /// </summary>
        private const string CONTENT_KEY = "LicenseWrongCredentialsNotes";

        /// <summary>
        /// The name of the 'recover credentials' part of the license notes.
        /// </summary>
        private const string RECOVER_CREDENTIALS_PART = "LicenseRecoverCredentials";

        /// <summary>
        /// The name of the 'recover credentials link' part of the license notes.
        /// </summary>
        private const string RECOVER_CREDENTIALS_LINK_NAME = "LicenseRecoverCredentialsLink";
        #endregion
    }
}
