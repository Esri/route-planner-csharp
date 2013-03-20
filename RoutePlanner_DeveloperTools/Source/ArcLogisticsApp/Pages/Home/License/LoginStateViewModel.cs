using System.Diagnostics;
using System.Windows.Input;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility.ComponentModel;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Represents a method to be used for handling login.
    /// </summary>
    /// <param name="username">The username to be used for login.</param>
    /// <param name="password">The password to be used for login.</param>
    /// <param name="rememberCredentials">A value indicating if the specified
    /// credentials should be remembered.</param>
    internal delegate void LoginActionHandler(
        string username,
        string password,
        bool rememberCredentials);

    /// <summary>
    /// The model for the login state of the license page.
    /// </summary>
    internal sealed class LoginStateViewModel : NotifyPropertyChangedBase
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the LoginStateViewModel class.
        /// </summary>
        /// <param name="licensePageCommands">The reference to the
        /// license page commands object.</param>
        /// <param name="executeLogin">The action to be executed when login
        /// is requested.</param>
        public LoginStateViewModel(
            ILicensePageCommands licensePageCommands,
            LoginActionHandler executeLogin)
        {
            Debug.Assert(licensePageCommands != null);
            Debug.Assert(executeLogin != null);

            this.RememberCredentialCommand = new DelegateCommand(
                _ => _ExecuteRememberCredential(),
                _ => _CanLogin());
            this.LoginCommand = new DelegateCommand(
                _ => executeLogin(this.Username, this.Password, _rememberCredential),
                _ => _CanLogin());
            this.CreateAccountCommand = licensePageCommands.CreateAccount;
            this.RecoverCredentialsCommand = licensePageCommands.RecoverCredentials;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets or sets a reference to the login view hosting object.
        /// </summary>
        public ILoginViewHost LoginViewHost
        {
            get
            {
                return _loginViewHost;
            }
            set
            {
                if (_loginViewHost != value)
                {
                    _loginViewHost = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LOGIN_VIEW_HOST);
                }
            }
        }

        /// <summary>
        /// Gets or sets a username.
        /// </summary>
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_USERNAME);
                }
            }
        }

        /// <summary>
        /// Gets or sets a password.
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_PASSWORD);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the password could be entered.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_USERNAME)]
        public bool CanEnterPassword
        {
            get
            {
                return !string.IsNullOrEmpty(this.Username);
            }
        }

        /// <summary>
        /// Gets a reference to the command for remembering credential.
        /// </summary>
        public ICommand RememberCredentialCommand
        {
            get
            {
                return _rememberCredentialCommand;
            }
            private set
            {
                if (_rememberCredentialCommand != value)
                {
                    _rememberCredentialCommand = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_REMEMBER_CREDENTIAL_COMMAND);
                }
            }
        }

        /// <summary>
        /// Gets a reference to the login command.
        /// </summary>
        public ICommand LoginCommand
        {
            get
            {
                return _loginCommand;
            }
            private set
            {
                if (_loginCommand != value)
                {
                    _loginCommand = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_LOGIN_COMMAND);
                }
            }
        }

        /// <summary>
        /// Gets a reference to the account creation command.
        /// </summary>
        public ICommand CreateAccountCommand
        {
            get
            {
                return _createAccountCommand;
            }
            private set
            {
                if (_createAccountCommand != value)
                {
                    _createAccountCommand = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_CREATE_ACCOUNT_COMMAND);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the account creation command property has a valid value.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_CREATE_ACCOUNT_COMMAND)]
        public bool HasCreateAccountCommand
        {
            get
            {
                return this.CreateAccountCommand != null;
            }
        }

        /// <summary>
        /// Gets a reference to the credentials recovering command.
        /// </summary>
        public ICommand RecoverCredentialsCommand
        {
            get
            {
                return _recoverCredentialsCommand;
            }
            private set
            {
                if (_recoverCredentialsCommand != value)
                {
                    _recoverCredentialsCommand = value;
                    this.NotifyPropertyChanged(PROPERTY_NAME_RECOVER_CREDENTIALS_COMMAND);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if the credentials recovering command property
        /// has a valid value.
        /// </summary>
        [PropertyDependsOn(PROPERTY_NAME_RECOVER_CREDENTIALS_COMMAND)]
        public bool HasRecoverCredentialsCommand
        {
            get
            {
                return this.RecoverCredentialsCommand != null;
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Checks if logging in to the server could be attempted.
        /// </summary>
        /// <returns>True if and only if the login could be attempted.</returns>
        private bool _CanLogin()
        {
            return
                !string.IsNullOrEmpty(this.Username) &&
                !string.IsNullOrEmpty(this.Password);
        }

        /// <summary>
        /// Executes credentials remembering state changes.
        /// </summary>
        private void _ExecuteRememberCredential()
        {
            _rememberCredential = !_rememberCredential;
        }
        #endregion

        #region private constants
        /// <summary>
        /// Name of the LoginViewHost property.
        /// </summary>
        private const string PROPERTY_NAME_LOGIN_VIEW_HOST = "LoginViewHost";

        /// <summary>
        /// Name of the Username property.
        /// </summary>
        private const string PROPERTY_NAME_USERNAME = "Username";

        /// <summary>
        /// Name of the Password property.
        /// </summary>
        private const string PROPERTY_NAME_PASSWORD = "Password";

        /// <summary>
        /// Name of the RememberCredentialCommand property.
        /// </summary>
        private const string PROPERTY_NAME_REMEMBER_CREDENTIAL_COMMAND = "RememberCredentialCommand";

        /// <summary>
        /// Name of the LoginCommand property.
        /// </summary>
        private const string PROPERTY_NAME_LOGIN_COMMAND = "LoginCommand";

        /// <summary>
        /// Name of the CreateAccountCommand property.
        /// </summary>
        private const string PROPERTY_NAME_CREATE_ACCOUNT_COMMAND = "CreateAccountCommand";

        /// <summary>
        /// Name of the RecoverCredentialsCommand property.
        /// </summary>
        private const string PROPERTY_NAME_RECOVER_CREDENTIALS_COMMAND = "RecoverCredentialsCommand";
        #endregion

        #region private fields
        /// <summary>
        /// Stores value of the LoginViewHost property.
        /// </summary>
        private ILoginViewHost _loginViewHost;

        /// <summary>
        /// Stores value of the Username property.
        /// </summary>
        private string _username;

        /// <summary>
        /// Stores value of the Password property.
        /// </summary>
        private string _password;

        /// <summary>
        /// Stores value of the RememberCredentialCommand property.
        /// </summary>
        private ICommand _rememberCredentialCommand;

        /// <summary>
        /// Stores value of the LoginCommand property.
        /// </summary>
        private ICommand _loginCommand;

        /// <summary>
        /// Stores value of the CreateAccountCommand property.
        /// </summary>
        private ICommand _createAccountCommand;

        /// <summary>
        /// Stores value of the RecoverCredentialsCommand property.
        /// </summary>
        private ICommand _recoverCredentialsCommand;

        /// <summary>
        /// Stores a value indicating if the credential should be remembered upon
        /// login.
        /// </summary>
        private bool _rememberCredential;
        #endregion
    }
}
