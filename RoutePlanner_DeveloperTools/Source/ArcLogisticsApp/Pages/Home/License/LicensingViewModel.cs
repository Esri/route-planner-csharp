using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Xml;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// The model for the license activation on the license page.
    /// </summary>
    internal sealed class LicensingViewModel : LoginViewModelBase
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the LicensingViewModel class.
        /// </summary>
        /// <param name="messenger">The messenger object to be used for
        /// notifications.</param>
        /// <param name="workingStatusController">The object to be used for
        /// managing application working status.</param>
        /// <param name="uriNavigator">The reference to the object to be used
        /// for navigating to URI's.</param>
        /// <param name="licenseManager">The reference to the license manager
        /// object.</param>
        public LicensingViewModel(
            IMessenger messenger,
            IWorkingStatusController workingStatusController,
            IUriNavigator uriNavigator,
            ILicenseManager licenseManager)
        {
            Debug.Assert(messenger != null);
            Debug.Assert(workingStatusController != null);
            Debug.Assert(uriNavigator != null);
            Debug.Assert(licenseManager != null);

            _licenseManager = licenseManager;
            _licenseExpirationChecker = licenseManager.LicenseExpirationChecker;
            _messenger = messenger;
            _workingStatusController = workingStatusController;
            _uriNavigator = uriNavigator;

            _licensePageCommands = new LicensePageCommands()
            {
                CreateAccount = _CreateUrlNavigationCommand(
                    _licenseManager.LicenseComponent.CreateAccountURL,
                    _uriNavigator),
                RecoverCredentials = _CreateUrlNavigationCommand(
                    _licenseManager.LicenseComponent.RecoverCredentialsURL,
                    _uriNavigator),
                UpgradeLicense = _CreateUrlNavigationCommand(
                    _licenseManager.LicenseComponent.UpgradeLicenseURL,
                    _uriNavigator),
            };

            this.LicenseActivationStatus = _licenseManager.LicenseActivationStatus;
            this.LicenseState = this.LicenseActivationStatus == LicenseActivationStatus.Activated ?
                AgsServerState.Authorized : AgsServerState.Unauthorized;
            _UpdateExpirationWarningDisplayState();

            foreach (var item in EnumHelpers.GetValues<AgsServerState>())
            {
                this.RegisterHeader(
                    item,
                    _licenseManager.LicenseComponent.LoginPrompt);
            }

            _CreateLoginState();
            _CreateConnectedState();
            _CreateNotConnectedState();

            this.LicensingNotes = _CreateLicensingNotes(
                _licenseManager.LicenseComponent.LicensingNotes);
            this.TroubleshootingNotes = _CreateTroubleshootingNotes(
                _licenseManager.LicenseComponent.TroubleshootingNotes);

            _stateFactories = new Dictionary<LicenseActivationStatus, Func<LicenseActivationViewState>>()
            {
                {
                    LicenseActivationStatus.None,
                    () => new LicenseActivationViewState
                    {
                        LoginViewState = this.LoginState,
                        InformationViewState = (object)null,
                    }
                },
                {
                    LicenseActivationStatus.Activated,
                    () => new LicenseActivationViewState
                    {
                        LoginViewState = this.ConnectedState,
                        InformationViewState = _CreateActivatedState(),
                    }
                },
                {
                    LicenseActivationStatus.Expired,
                    () => new LicenseActivationViewState
                    {
                        LoginViewState = this.LoginState,
                        InformationViewState = _CreateExpiredState(),
                    }
                },
                {
                    LicenseActivationStatus.WrongCredentials,
                    () => new LicenseActivationViewState
                    {
                        LoginViewState = this.LoginState,
                        InformationViewState = _CreateWrongCredentialsState(),
                    }
                },
                {
                    LicenseActivationStatus.NoSubscription,
                    () => new LicenseActivationViewState
                    {
                        LoginViewState = this.LoginState,
                        InformationViewState = _CreateNoSubscriptionState(),
                    }
                },
                {
                    LicenseActivationStatus.Failed,
                    () => new LicenseActivationViewState
                    {
                        LoginViewState = this.NotConnectedState,
                        InformationViewState = (object)null,
                    }
                },
            };
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets a document with licensing notes.
        /// </summary>
        public FlowDocument LicensingNotes
        {
            get
            {
                return _licensingNotes;
            }
            private set
            {
                if (_licensingNotes != value)
                {
                    _licensingNotes = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LICENSING_NOTES);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the licensing notes document is present.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_LICENSING_NOTES)]
        public bool ShowLicensingNotes
        {
            get
            {
                return this.LicensingNotes != null;
            }
        }

        /// <summary>
        /// Gets a document with troubleshooting notes.
        /// </summary>
        public FlowDocument TroubleshootingNotes
        {
            get
            {
                return _troubleshootingNotes;
            }
            private set
            {
                if (_troubleshootingNotes != value)
                {
                    _troubleshootingNotes = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_TROUBLESHOOTING_NOTES);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the troubleshooting notes document is present.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_TROUBLESHOOTING_NOTES)]
        [PropertyDependsOn(PROPERTY_NAME_LICENSE_ACTIVATION_STATUS)]
        public bool ShowTroubleshootingNotes
        {
            get
            {
                return
                    this.TroubleshootingNotes != null &&
                    (this.LicenseActivationStatus == LicenseActivationStatus.WrongCredentials ||
                    this.LicenseActivationStatus == LicenseActivationStatus.NoSubscription);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating license activation status.
        /// </summary>
        public LicenseActivationStatus LicenseActivationStatus
        {
            get
            {
                return _licenseActivationStatus;
            }
            private set
            {
                if (_licenseActivationStatus != value)
                {
                    _licenseActivationStatus = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LICENSE_ACTIVATION_STATUS);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the current license is restricted.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_LICENSE_ACTIVATION_STATUS)]
        public bool IsRestricted
        {
            get
            {
                var license = _licenseManager.AppLicense;
                var result =
                    license == null ||
                    license.IsRestricted;

                return result;
            }
        }

        /// <summary>
        /// Gets reference to the current model for license activation status view.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_LICENSE_ACTIVATION_STATUS)]
        public LicenseActivationViewState CurrentLicenseActivationState
        {
            get
            {
                LicenseActivationViewState result = null;

                Func<LicenseActivationViewState> state;
                if (_stateFactories.TryGetValue(this.LicenseActivationStatus, out state))
                {
                    result = state();
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating if license expiration warning should be shown
        /// for an activated license state.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_LICENSE_ACTIVATION_STATUS)]
        public bool ShowExpirationWarning
        {
            get
            {
                var result = _licenseExpirationChecker.LicenseIsExpiring(
                    _licenseManager.AppLicense,
                    _licenseManager.AppLicenseValidationDate);

                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating if the application should display license
        /// expiration warning to the user.
        /// </summary>
        public bool RequiresExpirationWarning
        {
            get
            {
                return _requiresExpirationWarning;
            }

            private set
            {
                if (_requiresExpirationWarning != value)
                {
                    _requiresExpirationWarning = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_REQUIRES_EXPIRATION_WARNING);
                }
            }
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Creates command for navigating to the specified URL.
        /// </summary>
        /// <param name="url">The url to navigate to with the resulting command.</param>
        /// <param name="uriNavigator">The uri navigator to be used for navigating to
        /// the specified URL.</param>
        /// <returns>A new command object for navigating to the specified URL or null
        /// if the <paramref name="url"/> is null or empty.</returns>
        private static ICommand _CreateUrlNavigationCommand(
            string url,
            IUriNavigator uriNavigator)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            return new DelegateCommand(_ => uriNavigator.NavigateToUri(url));
        }
        #endregion

        #region private methods
        /// <summary>
        /// Creates view model for the login state.
        /// </summary>
        private void _CreateLoginState()
        {
            this.LoginState = new LoginStateViewModel(
                _licensePageCommands,
                _ExecuteLogin)
            {
                Username = _licenseManager.AuthorizedUserName,
            };
        }

        /// <summary>
        /// Creates view model for the connected state.
        /// </summary>
        private void _CreateConnectedState()
        {
            if (_licenseManager.AppLicense == null)
            {
                this.ConnectedState = null;

                return;
            }

            var switchUserCommand = new DelegateCommand(_ =>
            {
                this.LicenseState = AgsServerState.Unauthorized;
                this.LicenseActivationStatus = LicenseActivationStatus.None;
                this.RequiresExpirationWarning = false;
            });
            if (!_licenseManager.AppLicense.IsRestricted)
            {
                switchUserCommand = null;
            }

            var licenseInfoText = _CreateLicenseInfoText()
                .Select(block => ApplyStyle(new FlowDocument(block)))
                .ToList();

            this.ConnectedState = new ConnectedStateViewModel(
                _licensePageCommands,
                licenseInfoText)
            {
                SwitchUserCommand = switchUserCommand,
            };
        }

        /// <summary>
        /// Creates view model for the not connected state.
        /// </summary>
        private void _CreateNotConnectedState()
        {
            var connectionFailureInfo = new FlowDocument(
                new Paragraph(
                    new Run(App.Current.FindString(
                        "ArcLogisticsLicenseServiceUnavailableText")
                    )
                )
            );
            ApplyStyle(connectionFailureInfo);

            this.NotConnectedState = new NotConnectedStateViewModel()
            {
                ConnectionFailureInfo = connectionFailureInfo,
            };
        }

        /// <summary>
        /// Creates view model for providing information about activated license.
        /// </summary>
        /// <returns>View model for the activated license state.</returns>
        private ActivatedStateViewModel _CreateActivatedState()
        {
            var state = new ActivatedStateViewModel(
                _licenseManager,
                _licensePageCommands,
                this.ShowExpirationWarning);

            return state;
        }

        /// <summary>
        /// Creates view model for providing information about expired license.
        /// </summary>
        /// <returns>View model for the expired license state.</returns>
        private ExpiredStateViewModel _CreateExpiredState()
        {
            var state = new ExpiredStateViewModel(
                _licenseManager,
                _licensePageCommands,
                _ExecuteSingleVehicleLogin);

            return state;
        }

        /// <summary>
        /// Creates view model for providing information about cases when license
        /// activation credentials were incorrect.
        /// </summary>
        /// <returns>View model for the wrong license credentials state.</returns>
        private WrongCredentialsStateViewModel _CreateWrongCredentialsState()
        {
            var state = new WrongCredentialsStateViewModel(
                _licensePageCommands);
            return state;
        }

        /// <summary>
        /// Creates view model for providing information about cases when there is
        /// no license subsription for current license activation credentials.
        /// </summary>
        /// <returns>View model for the no license subsription state.</returns>
        private NoSubscriptionStateViewModel _CreateNoSubscriptionState()
        {
            var state = new NoSubscriptionStateViewModel(
                _licensePageCommands);
            return state;
        }

        /// <summary>
        /// Executes license activation.
        /// </summary>
        /// <param name="username">The user name to be used for license activation.</param>
        /// <param name="password">The password to be used for license activation.</param>
        /// <param name="rememberCredentials">Indicates if credentials should be saved
        /// and reused upon application startup.</param>
        private void _ExecuteLogin(
            string username,
            string password,
            bool rememberCredentials)
        {
            _ActivateLicense(() => _licenseManager.ActivateLicense(
                username,
                password,
                rememberCredentials));
        }

        /// <summary>
        /// Executes activation for the free single vehicle license.
        /// </summary>
        private void _ExecuteSingleVehicleLogin()
        {
            _ActivateLicense(_licenseManager.ActivateFreeLicense);
        }

        /// <summary>
        /// Activates license with the specified license activator action.
        /// </summary>
        /// <param name="licenseActivator">The delegate performing license activation.</param>
        private void _ActivateLicense(Action licenseActivator)
        {
            Debug.Assert(licenseActivator != null);

            using (_workingStatusController.EnterBusyState(null))
            {
                try
                {
                    licenseActivator();
                    _UpdateExpirationWarningDisplayState();
                    _CreateConnectedState();
                    this.LicenseState = AgsServerState.Authorized;
                }
                catch (LicenseException ex)
                {
                    Logger.Error(ex);
                    this.LicenseState = AgsServerState.Unauthorized;
                }
                catch (CommunicationException ex)
                {
                    Logger.Error(ex);
                    this.LicenseState = AgsServerState.Unavailable;
                }

                this.LicenseActivationStatus = _licenseManager.LicenseActivationStatus;
            }
        }

        /// <summary>
        /// Creates document with the licensing notes from the specified string.
        /// </summary>
        /// <param name="notes">The string value containing licensing notes document.</param>
        /// <returns>A new document with the licensing notes.</returns>
        private FlowDocument _CreateLicensingNotes(string notes)
        {
            var document = _GetDocumentFromString(notes);
            if (document == null)
            {
                return null;
            }

            // add handlers to hyperlink RequesNavigate for open reques page in browser
            Hyperlink subscription = document.FindName(HYPERLINK_NAME) as Hyperlink;
            if (subscription != null)
            {
                subscription.RequestNavigate += _HyperlinkRequestNavigate;
            }

            return document;
        }

        /// <summary>
        /// Creates document with the troubleshooting notes from the specified string.
        /// </summary>
        /// <param name="notes">The string value containing troubleshooting notes document.</param>
        /// <returns>A new document with the troubleshooting notes.</returns>
        private FlowDocument _CreateTroubleshootingNotes(string notes)
        {
            var document = _GetDocumentFromString(notes);
            if (document == null)
            {
                return null;
            }

            // add handlers to hyperlink RequesNavigate for open reques page in browser
            Hyperlink subscription = document.FindName(HYPERLINK_NAME) as Hyperlink;
            if (subscription != null)
            {
                subscription.RequestNavigate += _HyperlinkRequestNavigate;
            }

            Hyperlink webaccounts = document.FindName(WEBACCOUNTS_HYPERLINK) as Hyperlink;
            if (webaccounts != null)
            {
                webaccounts.RequestNavigate += _HyperlinkRequestNavigate;
            }

            Hyperlink purchase = document.FindName(PURCHASE_HUPERLINK) as Hyperlink;
            if (purchase != null)
            {
                purchase.RequestNavigate += _HyperlinkRequestNavigate;
            }

            return document;
        }

        /// <summary>
        /// Method converts string to Flow Document
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private FlowDocument _GetDocumentFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            FlowDocument result = null;
            XmlTextReader reader = default(XmlTextReader);

            try
            {
                // Loading document from input string (in common case string can be empty)
                reader = new XmlTextReader(new StringReader(str));
                result = (FlowDocument)XamlReader.Load(reader);
            }
            catch (Exception)
            {
                // if we cannot convert string to FlowDocument and string isn't empty - add this string to document as text
                // if string is empty - return nulls
                if (result == null && !string.IsNullOrEmpty(str))
                    result = new FlowDocument(new Paragraph(new Run(str)));
            }
            finally
            {
                reader.Close();
            }

            if (result != null)
            {
                ApplyStyle(result);
            }

            return result;
        }

        /// <summary>
        /// Method adds info string with stated values into inlines collection
        /// </summary>
        /// <returns></returns>
        private void _AddInfoString(
            string titleString,
            string valueString,
            ICollection<Inline> inlines)
        {
            // title's always white
            Run title = new Run(titleString);
            inlines.Add(title);

            Bold value = new Bold(new Run(valueString));
            inlines.Add(value);
        }

        /// <summary>
        /// Method creates license info strings
        /// </summary>
        /// <returns></returns>
        private List<Block> _CreateLicenseInfoText()
        {
            var blocks = new List<Block>();

            var activatedLicense = _licenseManager.AppLicense;

            // if user use services license
            if (activatedLicense.IsRestricted)
            {
                var paragraph = new Paragraph();
                _AddInfoString(
                    (string)App.Current.FindResource("CurrentArcLogisticsSubscriptionString") + GAP_STRING,
                    activatedLicense.ProductCode,
                    paragraph.Inlines);
                blocks.Add(paragraph);

                paragraph = new Paragraph();
                _AddInfoString(
                    (string)App.Current.FindResource("MaximumNumberOfRoutes") + GAP_STRING,
                    activatedLicense.PermittedRouteNumber.ToString(),
                    paragraph.Inlines);
                blocks.Add(paragraph);

                paragraph = new Paragraph();
                _AddInfoString(
                    (string)App.Current.FindResource("SubscriptionExpirationDate") + GAP_STRING,
                    ((DateTime)activatedLicense.ExpirationDate).ToShortDateString(),
                    paragraph.Inlines);
                blocks.Add(paragraph);
            }
            else // if user use Enterprise license
            {
                Run info = new Run(activatedLicense.Description);

                var paragraph = new Paragraph(info);
                blocks.Add(paragraph);
            }

            return blocks;
        }

        /// <summary>
        /// Handles hyperlink navigation requests.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void _HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var link = (Hyperlink)sender;
            _NavigateToUrl(link.NavigateUri.ToString());
            e.Handled = true;
        }

        /// <summary>
        /// Navigates to the specified url.
        /// </summary>
        /// <param name="url">The url to navigate to.</param>
        private void _NavigateToUrl(string url)
        {
            _uriNavigator.NavigateToUri(url);
        }

        /// <summary>
        /// Updates current value of the <see cref="P:RequiresExpirationWarning"/>
        /// property.
        /// </summary>
        private void _UpdateExpirationWarningDisplayState()
        {
            var result = _licenseExpirationChecker.RequiresExpirationWarning(
                _licenseManager.AuthorizedUserName,
                _licenseManager.AppLicense,
                _licenseManager.AppLicenseValidationDate);

            this.RequiresExpirationWarning = result;
        }
        #endregion

        #region private constants
        private const string GAP_STRING = " ";
        private const string HYPERLINK_NAME = "hyperlink";
        private const string WEBACCOUNTS_HYPERLINK = "webaccounts_hyperlink";
        private const string PURCHASE_HUPERLINK = "purchase_hyperlink";

        /// <summary>
        /// Name of the LicensingNotes property.
        /// </summary>
        private const string PROPERTY_NAME_LICENSING_NOTES = "LicensingNotes";

        /// <summary>
        /// Name of the TroubleshootingNotes property.
        /// </summary>
        private const string PROPERTY_NAME_TROUBLESHOOTING_NOTES = "TroubleshootingNotes";

        /// <summary>
        /// Name of the LicenseActivationStatus property.
        /// </summary>
        private const string PROPERTY_NAME_LICENSE_ACTIVATION_STATUS = "LicenseActivationStatus";

        /// <summary>
        /// Name of the RequiresExpirationWarning property.
        /// </summary>
        private const string PROPERTY_NAME_REQUIRES_EXPIRATION_WARNING =
            "RequiresExpirationWarning";
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the license manager object.
        /// </summary>
        private ILicenseManager _licenseManager;

        /// <summary>
        /// The messenger object to be used for notifications.
        /// </summary>
        private IMessenger _messenger;

        /// <summary>
        /// The object to be used for managing application working status.
        /// </summary>
        private IWorkingStatusController _workingStatusController;

        /// <summary>
        /// The reference to the object to be used for navigating to URI's.
        /// </summary>
        private IUriNavigator _uriNavigator;

        /// <summary>
        /// The reference to the license expiration checker object.
        /// </summary>
        private ILicenseExpirationChecker _licenseExpirationChecker;

        /// <summary>
        /// The reference to the license page commands object.
        /// </summary>
        private ILicensePageCommands _licensePageCommands;

        /// <summary>
        /// Stores a document with licensing notes.
        /// </summary>
        private FlowDocument _licensingNotes;

        /// <summary>
        /// Stores a document with troubleshooting notes.
        /// </summary>
        private FlowDocument _troubleshootingNotes;

        /// <summary>
        /// Stores license activation status value.
        /// </summary>
        private LicenseActivationStatus _licenseActivationStatus;

        /// <summary>
        /// The dictionary mapping license activation status into factory function
        /// for creation of the corresponding view model.
        /// </summary>
        private Dictionary<LicenseActivationStatus, Func<LicenseActivationViewState>> _stateFactories;

        /// <summary>
        /// Stores value of the RequiresExpirationWarning property.
        /// </summary>
        private bool _requiresExpirationWarning;
        #endregion
    }
}
