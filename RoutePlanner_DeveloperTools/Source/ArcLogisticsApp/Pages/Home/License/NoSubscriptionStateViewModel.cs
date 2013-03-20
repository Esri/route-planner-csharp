using System.Diagnostics;
using System.Windows.Documents;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents view model for the "No Subscription" state of the license
    /// information view.
    /// </summary>
    internal sealed class NoSubscriptionStateViewModel
    {
        #region constructor
        /// <summary>
        /// Initializes a new instance of the NoSubscriptionStateViewModel class.
        /// </summary>
        /// <param name="licensePageCommands">The reference to the common license
        /// page command container object.</param>
        public NoSubscriptionStateViewModel(
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

            var command = licensePageCommands.UpgradeLicense;
            var purchaseSubscriptionPart = (Section)content.FindName(PURCHASE_PART);
            if (command == null)
            {
                purchaseSubscriptionPart.Blocks.Clear();
            }
            else
            {
                var purchaseSubscriptionLink = (Hyperlink)purchaseSubscriptionPart.FindName(
                    PURCHASE_LINK_NAME);
                purchaseSubscriptionLink.Command = command;
            }

            this.ContentFirstParagraph = FlowDocumentHelpers.ExtractFirstBlock(content);
            this.ContentSecondParagraph = content;
        }
        #endregion

        #region private constants
        /// <summary>
        /// The name of the resource storing notes header.
        /// </summary>
        private const string VIEW_HEADER_KEY = "LicenseNoSubscriptionNotesHeader";

        /// <summary>
        /// The name of the resource storing notes.
        /// </summary>
        private const string CONTENT_KEY = "LicenseNoSubscriptionNotes";

        /// <summary>
        /// The name of the 'purchase subscription' part of the license notes.
        /// </summary>
        private const string PURCHASE_PART = "LicensePurchaseSubscription";

        /// <summary>
        /// The name of the 'purchase subscription link' part of the license notes.
        /// </summary>
        private const string PURCHASE_LINK_NAME = "LicensePurchaseSubscriptionLink";
        #endregion
    }
}
