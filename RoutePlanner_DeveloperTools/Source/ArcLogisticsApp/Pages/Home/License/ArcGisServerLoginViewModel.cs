using System;
using System.Diagnostics;
using System.Windows.Documents;
using ESRI.ArcLogistics.Services;
using System.Linq;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// The model for the server login on the license page.
    /// </summary>
    internal sealed class ArcGisServerLoginViewModel : LoginViewModelBase
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the ArcGisServerLoginViewModel class.
        /// </summary>
        /// <param name="server">The server to provide authentication for.</param>
        /// <param name="messenger">The messenger object to be used for
        /// notifications.</param>
        /// <param name="workingStatusController">The object to be used for
        /// managing application working status.</param>
        public ArcGisServerLoginViewModel(
            AgsServer server,
            IMessenger messenger,
            IWorkingStatusController workingStatusController)
        {
            Debug.Assert(server != null);
            Debug.Assert(messenger != null);
            Debug.Assert(workingStatusController != null);

            _server = server;
            _server.StateChanged += _ServerStateChanged;

            this.LicenseState = server.State;
            this.RegisterHeader(
                AgsServerState.Unauthorized,
                App.Current.GetString("LoginToServerString", _server.Title));

            _messenger = messenger;
            _workingStatusController = workingStatusController;

            var licensePageCommands = new LicensePageCommands()
            {
                CreateAccount = null,
                RecoverCredentials = null,
                UpgradeLicense = null,
            };

            this.LoginState = new LoginStateViewModel(
                licensePageCommands,
                _ExecuteLogin);

            var licenseInfoText = EnumerableEx.Return(ApplyStyle(new FlowDocument(
                new Paragraph()
                {
                    Inlines =
                    {
                        new Run(App.Current.GetString("LoggedToServerString", _server.Title)),
                        new Run(NEW_LINE),
                        new Run(_server.Description)
                    },
                }
            )));

            this.ConnectedState = new ConnectedStateViewModel(
                licensePageCommands,
                licenseInfoText)
            {
                SwitchUserCommand = new DelegateCommand(
                    _ => this.LicenseState = AgsServerState.Unauthorized),
            };

            var connectionFailureInfo = ApplyStyle(new FlowDocument(
                new Paragraph(
                    new Run(App.Current.GetString(
                        "ServerIsUnavailableString",
                        _server.Title)
                    )
                )
            ));

            this.NotConnectedState = new NotConnectedStateViewModel()
            {
                ConnectionFailureInfo = connectionFailureInfo,
            };
        }
        #endregion

        #region private methods
        /// <summary>
        /// Executes server login.
        /// </summary>
        /// <param name="username">The user name to be used for login.</param>
        /// <param name="password">The password to be used for login.</param>
        /// <param name="rememberCredentials">Indicated if the specified credentials
        /// should be saved.</param>
        private void _ExecuteLogin(string username, string password, bool rememberCredentials)
        {
            using (_workingStatusController.EnterBusyState(null))
            {
                try
                {
                    _server.Authorize(
                        username,
                        password,
                        rememberCredentials);
                    this.LicenseState = _server.State;
                }
                catch (AuthenticationException ex)
                {
                    var message = App.Current.GetString("ServerAuthError", _server.Title);
                    _messenger.AddWarning(message);
                    Logger.Error(ex);
                    this.LicenseState = AgsServerState.Unauthorized;
                }
                catch (Exception ex)
                {
                    var message = App.Current.GetString("ServerConnectionError", _server.Title);
                    _messenger.AddWarning(message);
                    Logger.Error(ex);
                    this.LicenseState = AgsServerState.Unavailable;
                }
            }
        }

        /// <summary>
        /// Handles server state changes.
        /// </summary>
        /// <param name="sender">The reference to the event sender object.</param>
        /// <param name="e">The reference to the event arguments.</param>
        private void _ServerStateChanged(object sender, EventArgs e)
        {
            this.LicenseState = _server.State;
        }
        #endregion

        #region private constants
        private const string NEW_LINE = "\n\n";
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the ArcGIS server object to login to.
        /// </summary>
        private AgsServer _server;

        /// <summary>
        /// The messenger object to be used for notifications.
        /// </summary>
        private IMessenger _messenger;

        /// <summary>
        /// The object to be used for managing application working status.
        /// </summary>
        private IWorkingStatusController _workingStatusController;
        #endregion
    }
}
