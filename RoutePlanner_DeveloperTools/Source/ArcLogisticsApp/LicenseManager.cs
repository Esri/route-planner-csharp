using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using ESRI.ArcLogistics.App.Services;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// LicenseManager class.
    /// </summary>
    internal sealed class LicenseManager : ILicenseManager
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the LicenseManager class.
        /// </summary>
        /// <param name="licenseCacheStorage">The reference to the license cache
        /// storage object.</param>
        /// <param name="proxyConfigurationService">The reference to the proxy configuration
        /// service object.</param>
        public LicenseManager(
            ILicenseCacheStorage licenseCacheStorage,
            IProxyConfigurationService proxyConfigurationService)
        {
            Debug.Assert(licenseCacheStorage != null);
            Debug.Assert(proxyConfigurationService != null);

            _licenseCacheStorage = licenseCacheStorage;
            _proxyConfigurationService = proxyConfigurationService;

            this.LicenseActivationStatus = LicenseActivationStatus.None;
            if (this.AppLicense != null)
            {
                this.LicenseActivationStatus = LicenseActivationStatus.Activated;
            }
            else if (this.ExpiredLicense != null)
            {
                this.LicenseActivationStatus = LicenseActivationStatus.Expired;
            }
        }
        #endregion

        #region ILicenseManager Members
        public ILicenser LicenseComponent
        {
            get { return Licenser.Instance; }
        }

        public License AppLicense
        {
            get { return Licenser.ActivatedLicense; }
        }

        /// <summary>
        /// Gets a date/time of the <see cref="P:AppLicense"/> validation.
        /// </summary>
        public DateTime? AppLicenseValidationDate
        {
            get
            {
                return Licenser.ActivatedLicenseValidationDate;
            }
        }

        /// <summary>
        /// Gets a reference to the license when it is expired.
        /// </summary>
        public License ExpiredLicense
        {
            get
            {
                return Licenser.ExpiredLicense;
            }
        }

        public string AuthorizedUserName
        {
            get { return authorizedUserName;  }
        }

        public bool HaveStoredCredentials
        {
            get
            {
                Properties.Settings settings = Properties.Settings.Default;

                return (!String.IsNullOrEmpty(settings.GlobalAccountUsername) &&
                    !String.IsNullOrEmpty(settings.GlobalAccountPassword));
            }
        }

        /// <summary>
        /// Gets current license activation status.
        /// </summary>
        public LicenseActivationStatus LicenseActivationStatus
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the license expiration checker object.
        /// </summary>
        public ILicenseExpirationChecker LicenseExpirationChecker
        {
            get
            {
                return Licenser.LicenseExpirationChecker;
            }
        }

        public void ActivateLicenseOnStartup()
        {
            _InvokeLicenseActivation(() =>
            {
                if (LicenseComponent.RequireAuthentication)
                {
                    // check if we have stored credentials
                    NetworkCredential account = _LoadGA();
                    if (account != null)
                    {
                        // try to activate license using stored credentials
                        ActivateLicense(account.UserName, account.Password, false);
                    }
                }
                else
                {
                    // credentials are not required (enterprise deployment scenario)
                    LicenseComponent.GetLicense();
                }
            });
        }

        public void ActivateLicense(string username, string password,
            bool saveCredentials)
        {
            _InvokeLicenseActivation(() =>
            {
                try
                {
                    _ActivateLicense(
                        LicenseComponent.GetLicense,
                        username,
                        password,
                        saveCredentials);
                }
                catch (LicenseException e)
                {
                    if (e.ErrorCode == LicenseError.LicenseExpired)
                    {
                        _freeLicenseCredential = new NetworkCredential()
                        {
                            UserName = username,
                            Password = password,
                        };
                    }

                    throw;
                }
            });
        }

        /// <summary>
        /// Activates free license.
        /// </summary>
        public void ActivateFreeLicense()
        {
            _InvokeLicenseActivation(() =>
            {
                if (_freeLicenseCredential == null)
                {
                    var message = App.Current.FindString(MISSING_FREE_LICENSE_CREDENTIALS_KEY);
                    throw new InvalidOperationException(message);
                }

                _ActivateLicense(
                    LicenseComponent.GetFreeLicense,
                    _freeLicenseCredential.UserName,
                    _freeLicenseCredential.Password,
                    false);
            });
        }
        #endregion

        #region private static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static NetworkCredential _LoadGA()
        {
            Properties.Settings settings = Properties.Settings.Default;
            string username = settings.GlobalAccountUsername;
            string password = settings.GlobalAccountPassword;

            NetworkCredential credentials = null;
            if (!String.IsNullOrEmpty(username) &&
                !String.IsNullOrEmpty(password))
            {
                var passwordValue = default(string);
                if (StringProcessor.TryTransformDataBack(password, out passwordValue))
                {
                    credentials = new NetworkCredential(username, passwordValue);
                }
            }

            return credentials;
        }

        private static void _SaveGA(string username, string password)
        {
            if (!String.IsNullOrEmpty(username) &&
                !String.IsNullOrEmpty(password))
            {

                try
                {
                    Properties.Settings settings = Properties.Settings.Default;
                    settings.GlobalAccountUsername = username;
                    settings.GlobalAccountPassword = StringProcessor.TransformData(
                        password);

                    settings.Save();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        /// <summary>
        /// Method show authentication dialog for proxy server until user enter valid credentials or cancels the dialog.
        /// </summary>
        /// <param name="licenseActivator">License activator.</param>
        /// <param name="licenseUserName">License username.</param>
        /// <param name="licensePassword">License password.</param>
        /// <param name="proxyEx">Initial proxy authentication required exception.</param>
        /// <returns></returns>
        private void _AuthenticateProxyServerAndGetLicense(
            Func<string, string, LicenseInfo> licenseActivator,
            string licenseUserName,
            string licensePassword,
            CommunicationException proxyEx)
        {
            bool userPressedOK = false;
            bool wrongCredentials = false;
            License license = null;
            CommunicationException proxyException = proxyEx; // proxy exception that will be thrown if proxy won't be authenticated

            do // repeat showing auth dialog until user either enters correct credentials or presses cancel
            {
                wrongCredentials = false;

                // ask about proxy credentials
                if (userPressedOK = ProxyServerAuthenticator.AskAndSetProxyCredentials(
                    _proxyConfigurationService))
                {
                    try
                    {
                        // and repeat license activation if user entered credential and pressed OK
                        var licenseInfo = licenseActivator(licenseUserName, licensePassword); // exception
                        license = licenseInfo.License;
                    }
                    catch(CommunicationException ex)
                    {
                        if (ex.ErrorCode == CommunicationError.ProxyAuthenticationRequired)
                        {
                            wrongCredentials = true;
                            proxyException = ex;
                        }
                        else
                            throw;
                    }
                }
            } while (userPressedOK && wrongCredentials);

            // if license wasn't obtained - then throw the last proxy exception
            if (license == null)
                throw proxyException;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Performs license activation using the specified license activator action.
        /// </summary>
        /// <param name="licenseActivator">Encapsulates license activation
        /// implementation.</param>
        /// <param name="username">The user name to be used for license activation.</param>
        /// <param name="password">The password to be used for license activation.</param>
        /// <param name="saveCredentials">Indicates if the specified credentials should
        /// be saved for further reuse.</param>
        private void _ActivateLicense(
            Func<string, string, LicenseInfo> licenseActivator,
            string username,
            string password,
            bool saveCredentials)
        {
            if (LicenseComponent.RequireAuthentication)
            {
                try
                {
                    licenseActivator(username, password); // exception
                }
                catch (CommunicationException commEx)
                {
                    // in case there is a proxy issue
                    if (commEx.ErrorCode == CommunicationError.ProxyAuthenticationRequired)
                    {
                        // try to authorize proxy server
                        _AuthenticateProxyServerAndGetLicense(
                            licenseActivator,
                            username,
                            password,
                            commEx); // exception
                    }
                    else
                        throw; // just throw the exception further if it is not proxy issue
                }

                authorizedUserName = username;

                if (saveCredentials)
                    _SaveGA(username, password);
            }
            else
                LicenseComponent.GetLicense();
        }

        /// <summary>
        /// Wraps license activations in order to set correct value for the
        /// <see cref="P:ESRI.ArcLogistics.App.ApplicationLicenseManager.LicenseActivationStatus"/>
        /// property.
        /// </summary>
        /// <param name="body">The license activation to be wrapped.</param>
        private void _InvokeLicenseActivation(Action body)
        {
            Debug.Assert(body != null);

            try
            {
                body();
                if (this.AppLicense != null)
                {
                    this.LicenseActivationStatus = LicenseActivationStatus.Activated;
                }
            }
            catch (LicenseException e)
            {
                switch (e.ErrorCode)
                {
                    case LicenseError.InvalidCredentials:
                        this.LicenseActivationStatus = LicenseActivationStatus.WrongCredentials;
                        break;
                    case LicenseError.NoAllowedRoles:
                        this.LicenseActivationStatus = LicenseActivationStatus.NoSubscription;
                        break;
                    case LicenseError.LicenseExpired:
                        this.LicenseActivationStatus = LicenseActivationStatus.Expired;
                        break;
                    default:
                        this.LicenseActivationStatus = LicenseActivationStatus.None;
                        break;
                }

                throw;
            }
            catch (Exception)
            {
                this.LicenseActivationStatus = LicenseActivationStatus.Failed;

                throw;
            }
        }
        #endregion

        #region private constants
        /// <summary>
        /// Resource key for error message to be used when free license credentials
        /// are missing.
        /// </summary>
        private const string MISSING_FREE_LICENSE_CREDENTIALS_KEY =
            "LicenseErrorMissingFreeLicenseCredentials";
        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string authorizedUserName;

        /// <summary>
        /// Stores credentials to be used for activating free license.
        /// </summary>
        private NetworkCredential _freeLicenseCredential;

        /// <summary>
        /// The reference to the license cache storage object.
        /// </summary>
        private ILicenseCacheStorage _licenseCacheStorage;

        /// <summary>
        /// The reference to the proxy configuration service object.
        /// </summary>
        private IProxyConfigurationService _proxyConfigurationService;

        #endregion private fields
    }
}
