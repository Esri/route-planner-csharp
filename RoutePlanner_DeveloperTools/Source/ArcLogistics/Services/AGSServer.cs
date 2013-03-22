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
using System.Net;
using ESRI.ArcLogistics.CatalogService;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Server state enumeration.
    /// </summary>
    internal enum AgsServerState
    {
        Authorized,
        Unauthorized,
        Unavailable
    }

    /// <summary>
    /// Type of token for AgsServer.
    /// </summary>
    public enum TokenType
    {
        /// <summary>
        /// Default token.
        /// </summary>
        Default,

        /// <summary>
        /// New ArcGIS.com token type. To get such token, we must specify referer, also
        /// referer string must be added to service requests header.
        /// </summary>
        Arcgis_com
    }

    /// <summary>
    /// AgsServer class.
    /// </summary>
    internal class AgsServer
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal AgsServer(ServerInfoWrap config,
            NetworkCredential licenseAccount)
            : this(config, licenseAccount, null)
        {
        }

        internal AgsServer(ServerInfoWrap config,
            NetworkCredential licenseAccount,
            IRoutingServiceUrlProvider serviceUrlProvider)
        {
            Debug.Assert(config != null);

            // set server parameters
            _name = config.Name;
            _title = config.Title;
            _description = config.Description;
            _helpPrompt = config.HelpPrompt;
            _config = config;

            // init connection
            _InitParameters(config, licenseAccount);

            _serviceUrlProvider = serviceUrlProvider;

            // create authenticator
            _authenticator = _CreateAuthenticator(_parameters, _serviceUrlProvider);

            try
            {
                // connect/authenticate
                _Connect();
                _state = AgsServerState.Authorized;
            }
            catch (AuthenticationException)
            {
                _state = AgsServerState.Unauthorized;
                _initError = _CreateAuthException();
            }
            catch (Exception e)
            {
                _state = AgsServerState.Unavailable;
                _initError = _ConvertException(e);
                Logger.Error(e);
            }
        }

        #endregion constructors

        #region events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when state is changed.
        /// </summary>
        public event EventHandler StateChanged;

        #endregion events

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string Name
        {
            get { return _name; }
        }

        public string Title
        {
            get { return _GetTitle(); }
        }

        public string Description
        {
            get { return _description; }
        }

        public string HelpPrompt
        {
            get { return _helpPrompt; }
        }

        public AgsServerState State
        {
            get { return _state; }
        }

        public Exception InitializationFailure
        {
            get { return _initError; }
        }

        /// <summary>
        /// Gets authentication type for the server.
        /// </summary>
        public AgsServerAuthenticationType AuthenticationType
        {
            get
            {
                return _parameters.AuthenticationType;
            }
        }

        public bool HasCredentials
        {
            get { return (_parameters.Credentials != null); }
        }

        /// <summary>
        /// Gets network credential instance used by the server.
        /// </summary>
        public NetworkCredential Credentials
        {
            get
            {
                return _parameters.Credentials;
            }
        }

        public bool RequiresHttpAuthentication
        {
            get
            {
                _CheckState();
                return _authenticator.RequiresHttpAuthentication;
            }
        }

        public bool RequiresTokens
        {
            get
            {
                _CheckState();
                return _authenticator.RequiresTokens;
            }
        }

        public string LastToken
        {
            get
            {
                _CheckState();
                return _authenticator.LastToken;
            }
        }

        /// <summary>
        /// Time when server's token expired.
        /// </summary>
        public DateTime LastTokenExpirationTime
        {
            get
            {
                return _authenticator.LastTokenExpirationTime;
            }
        }
   
        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void Authorize(string userName, string password,
            bool saveCredentials)
        {
            _CheckState();
            _ValidateInputCredentials(userName, password);

            NetworkCredential credentials = new NetworkCredential(userName,
                password);

            AgsServerState oldState = _state;
            try
            {
                _authenticator.Authenticate(credentials);
                _parameters.Credentials = credentials;
                _state = AgsServerState.Authorized;

                if (saveCredentials &&
                    _authenticator.IsAuthRequired &&
                    _parameters.AuthenticationType == AgsServerAuthenticationType.Yes)
                {
                    _UpdateCredentials(credentials);
                }
            }
            catch (AuthenticationException)
            {
                throw _CreateAuthException();
            }
            catch (Exception e)
            {
                throw _ConvertException(e);
            }
            finally
            {
                if (_state != oldState)
                    _NotifyStateChanged();
            }
        }
         
        /// <summary>
        /// Generate new token for server.
        /// </summary>
        /// <returns>'Null' if no token is required for server, new token otherwise.</returns>
        public string GenerateNewToken()
        {
            if (!_authenticator.RequiresTokens)
                return null;

            return _authenticator.GenerateToken(Credentials);
        }

        public void Reconnect()
        {
            AgsServerState oldState = _state;
            try
            {
                // connect/authenticate
                _Connect();
                _state = AgsServerState.Authorized;
            }
            catch (AuthenticationException)
            {
                _state = AgsServerState.Unauthorized;
                throw _CreateAuthException();
            }
            catch (Exception e)
            {
                _state = AgsServerState.Unavailable;
                throw _ConvertException(e);
            }
            finally
            {
                if (_state != oldState)
                    _NotifyStateChanged();
            }
        }

        /// <summary>
        /// Opens connection to the ArcGIS server, performing authentication
        /// if necessary.
        /// </summary>
        /// <returns>New instance of the ArcGIS server connection.</returns>
        /// <exception cref="System.InvalidOperationException">Server is in
        /// unavailable state.</exception>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">Failed
        /// to authenticate within ArcGIS server.</exception>
        /// <exception cref="ESRI.ArcLogistics.CommunicationException">Failed
        /// to establish connection to the server.</exception>
        public AgsServerConnection OpenConnection()
        {
            _CheckState();

            AgsServerConnection connection = null;
            try
            {
                var parameters = new AgsServerConnectionParameters(_config)
                {
                    SoapUrl = _parameters.SoapUrl,
                    Credentials = _parameters.Credentials,
                    AuthenticationType = _parameters.AuthenticationType
                };

                var authenticator = _authenticator;
                if (_serviceUrlProvider != null)
                {
                    // Get new soap URL.
                    string newSoapUrl = _serviceUrlProvider.QueryServiceUrl();

                    // Replace old url and connect to new one.
                    parameters.SoapUrl = newSoapUrl;
                    authenticator = new Authenticator(newSoapUrl, _config.TokenType);

                    // Use older authenticator for initialization.
                    authenticator.Initialize(_authenticator);
                }

                connection = new AgsServerConnection(
                    authenticator,
                    parameters,
                    this);
            }
            catch (Exception e)
            {
                if (_IsCommError(e))
                {
                    throw _CreateCommException(e);
                }

                throw;
            }

            return connection;
        }
        #endregion public methods

        #region private static methods
        private static bool _IsCommError(Exception ex)
        {
            Debug.Assert(ex != null);

            return (ex is WebException ||
                ex is TimeoutException ||
                ex is System.ServiceModel.CommunicationException);
        }

        private static void _ValidateInputCredentials(string userName,
            string password)
        {
            if (String.IsNullOrEmpty(userName) ||
                String.IsNullOrEmpty(password))
            {
                throw new ArgumentException(
                    Properties.Messages.Error_InvalidCredentials);
            }
        }
        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _InitParameters(
            ServerInfoWrap config,
            NetworkCredential licAccount)
        {
            var parameters = new AgsServerConnectionParameters(config);
            var useLicenseCredentials =
                parameters.AuthenticationType ==
                AgsServerAuthenticationType.UseApplicationLicenseCredentials &&
                licAccount != null;
            if (useLicenseCredentials)
            {
                parameters.Credentials = new NetworkCredential(
                    licAccount.UserName,
                    licAccount.Password);
            }

            _parameters = parameters;
        }

        /// <summary>
        /// Creates authenticator instance.
        /// </summary>
        /// <param name="parameters">Connection parameters to be used.</param>
        /// <param name="serviceUrlProvider">Url provider instance to
        /// retrieve server url from.</param>
        /// <returns>new authenticator instance.</returns>
        private Authenticator _CreateAuthenticator(
            AgsServerConnectionParameters parameters,
            IRoutingServiceUrlProvider serviceUrlProvider)
        {
            var soapUrl = parameters.SoapUrl;
            if (serviceUrlProvider != null)
            {
                soapUrl = _serviceUrlProvider.QueryServiceUrl();
            }

            var authenticator = new Authenticator(soapUrl, _config.TokenType);

            return authenticator;
        }

        private void _Connect()
        {
            // If service need no authentification - connect to service without credentials, even
            // if we have old credenatials for this service.
            if (_parameters.AuthenticationType == AgsServerAuthenticationType.No)
                _authenticator.ConnectWorkaroundForNoAuthentication(null);
            else
                _authenticator.Connect(_parameters.Credentials);
        }

        private void _UpdateCredentials(NetworkCredential credentials)
        {
            if (_config.Credentials == null)
                _config.Credentials = new CredentialsInfo();

            _config.Credentials.UserName = _parameters.Credentials.UserName;
            _config.Credentials.Password = _parameters.Credentials.Password;
        }

        private void _CheckState()
        {
            if (_state == AgsServerState.Unavailable)
            {
                throw new InvalidOperationException(string.Format(
                    Properties.Messages.Error_ServerInUnavailableState, _title));
            }
        }

        private void _NotifyStateChanged()
        {
            if (StateChanged != null)
                StateChanged(this, EventArgs.Empty);
        }

        private string _GetTitle()
        {
            string title = String.Empty;

            if (_title != null)
                title = _title;
            else if (!String.IsNullOrEmpty(this.Name))
                title = this.Name;
            else if (!String.IsNullOrEmpty(_parameters.SoapUrl))
            {
                try
                {
                    title = new Uri(_parameters.SoapUrl).Host;
                }
                catch { }
            }

            return title;
        }

        private AuthenticationException _CreateAuthException()
        {
            return ServiceHelper.CreateAuthException(this);
        }

        private CommunicationException _CreateCommException(Exception ex)
        {
            return ServiceHelper.CreateCommException(this, ex);
        }

        private Exception _ConvertException(Exception ex)
        {
            Debug.Assert(ex != null);
            return _IsCommError(ex) ? _CreateCommException(ex) : ex;
        }
        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name;
        private string _title;
        private string _description;
        private string _helpPrompt;
        private ServerInfoWrap _config;

        private AgsServerState _state = AgsServerState.Unavailable;
        private Exception _initError;
        private Authenticator _authenticator;
        private AgsServerConnectionParameters _parameters;
        private IRoutingServiceUrlProvider _serviceUrlProvider;

        #endregion private fields

        #region private classes
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Authenticator class.
        /// </summary>
        private class Authenticator : IAgsServerAuthenticator
        {
            #region constants
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            // request timeout (milliseconds)
            private const int HTTP_REQ_TIMEOUT = 3 * 60 * 1000;

            #endregion constants

            #region constructors
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="soapUrl">URL.</param>
            /// <param name="sendRefererForToken">Do we need add "refer" string in request to
            /// token service or not.</param>
            public Authenticator(string soapUrl, TokenType tokenType)
            {
                _soapUrl = soapUrl;

                TokenType = tokenType;
            }

            #endregion constructors

            #region public properties
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            public bool RequiresHttpAuthentication
            {
                get
                {
                    _CheckState();
                    return _requiresHttpAuthentication;
                }
            }

            public bool IsAuthRequired
            {
                get
                {
                    _CheckState();
                    return (_requiresHttpAuthentication ||
                        _requiresTokens);
                }
            }

            /// <summary>
            /// Time when token expired.
            /// </summary>
            public DateTime LastTokenExpirationTime
            {
                get
                {
                    lock (_tokenLocker) { return _expirationTime; }
                }
            }

            /// <summary>
            /// Type of the token for services.
            /// </summary>
            public TokenType TokenType
            {
                get;
                set;
            }            

            #endregion public properties

            #region public methods
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Method initializes authenticator by getting information from another authenticator.
            /// </summary>
            /// <param name="authenticator">Authenticator to get information from.</param>
            public void Initialize(Authenticator authenticator)
            {
                _lastToken = authenticator.LastToken;
                _requiresHttpAuthentication = authenticator.RequiresHttpAuthentication;
                _requiresTokens = authenticator.RequiresTokens;
                TokenType = authenticator.TokenType;

                _isInited = true;
            }

            public void Connect(NetworkCredential credentials)
            {
                // init HTTP and token authentication parameters
                _Init(credentials);

                // do token authentication if needed
                if (_requiresTokens)
                {
                    if (credentials != null)
                        _UpdateToken(credentials);
                    else
                        throw new AuthenticationException();
                }
            }

            /// <summary>
            /// WORKAROUND. See CR 222585 Description. 
            /// Now we allways have 'RequireToken' == true for 10.1 servers.
            /// So in case if in services.xml setting "authentication=no" was 
            /// set for current service we will try to use this service without token.
            /// 
            /// When 10.1 servers will return actual value for 'RequireToken' parameter,
            /// delete this method and use "Connect" method instead.
            /// </summary>
            /// <param name="ignored">Ignored parameter. 
            /// This parameter is used to simplify, function deleting.</param>
            public void ConnectWorkaroundForNoAuthentication(object ignored)
            {
                // Init HTTP and token authentication parameters.
                _Init(null);

                // Switch off token for current service.
                _requiresTokens = false;
            }

            public void Authenticate(NetworkCredential credentials)
            {
                Debug.Assert(credentials != null);

                if (_isInited)
                {
                    if (_requiresHttpAuthentication)
                    {
                        _DoHttpAuth(_soapUrl, credentials);
                    }

                    if (_requiresTokens)
                        _UpdateToken(credentials);
                }
                else
                    Connect(credentials);
            }

            #endregion public methods

            #region IAgsServerAuthenticator Members
            public bool RequiresTokens
            {
                get
                {
                    _CheckState();
                    return _requiresTokens;
                }
            }

            public string LastToken
            {
                get
                {
                    _CheckState();
                    lock (_tokenLocker) { return _lastToken; }
                }
            }

            public string GenerateToken(NetworkCredential credentials)
            {
                _CheckState();

                if (!_requiresTokens)
                    throw new InvalidOperationException();

                if (string.IsNullOrEmpty(_tokenServiceUrl))
                    _InitTokenInfo(_soapUrl, credentials);

                return _UpdateToken(credentials);
            }

            #endregion

            #region private methods
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            private void _CheckState()
            {
                if (!_isInited)
                    throw new InvalidOperationException();
            }

            private void _Init(NetworkCredential credentials)
            {
                _isInited = false;

                _requiresHttpAuthentication = credentials != null;
                try
                {
                    _DoHttpAuth(_soapUrl, credentials);
                }
                catch (AuthenticationException)
                {
                    _requiresHttpAuthentication = true;
                    throw;
                }

                // init token authentication parameters
                _InitTokenInfo(_soapUrl, credentials);

                _isInited = true;
            }

            private void _InitTokenInfo(string serviceUrl, NetworkCredential credentials)
            {
                var client = ServiceHelper.CreateServiceClient<ServiceCatalogPortClient>(
                    "CatalogServiceBinding",
                    serviceUrl);

                // legal way to disable channel factory caching
                System.ServiceModel.ChannelFactory<ServiceCatalogPort> cf = client.ChannelFactory;

                try
                {
                    // set credentials to SOAP client if needed
                    if (_requiresHttpAuthentication && credentials != null)
                    {
                        ServiceHelper.SetClientCredentials(client,
                            credentials);
                    }

                    // check if server requires token authentication
                    if (client.RequiresTokens())
                    {
                        _tokenServiceUrl = client.GetTokenServiceURL();
                        _requiresTokens = true;
                    }
                    else
                        _requiresTokens = false;
                }
                finally
                {
                    ServiceHelper.CloseCommObject(client);
                }
            }

            private string _UpdateToken(NetworkCredential credentials)
            {
                Debug.Assert(credentials != null);

                string tokenString;
                try
                {
                    // Get token from token service.
                    Token token;
                    switch(TokenType)
                    {
                        case Services.TokenType.Default:
                            token = AgsHelper.GetServerToken(_tokenServiceUrl, credentials);
                            break;
                        case Services.TokenType.Arcgis_com:
                            token = AgsHelper.GetServerTokenUsingReferer(_tokenServiceUrl, credentials);
                            break;
                        default:
                            // New token type was added.
                            Debug.Assert(false);

                            token = AgsHelper.GetServerToken(_tokenServiceUrl, credentials);
                            break;
                    }

                    lock (_tokenLocker)
                    {
                        _lastToken = token.Value;
                        _expirationTime = token.Expires;

                        tokenString = _lastToken;
                    }
                }
                catch (WebException e)
                {
                    // TODO: Server returns "Forbidden" status on incorrect username/password,
                    // but it's too ambiguous status, we need more specific info to determine
                    // if the error is related to authentication problem

                    // HTTP_STATUS_FORBIDDEN (403): The server understood
                    // the request, but cannot fulfill it.
                    if (e.Response != null &&
                        ((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new AuthenticationException();
                    }
                    else
                        throw;
                }

                return tokenString;
            }


            private bool _IsHttpAuthRequired(string serviceUrl)
            {
                bool needAuth = false;
                try
                {
                    _DoHttpAuth(serviceUrl, null);
                }
                catch (AuthenticationException)
                {
                    needAuth = true;
                }

                return needAuth;
            }

            private void _DoHttpAuth(string serviceUrl, NetworkCredential credentials)
            {
                Debug.Assert(serviceUrl != null);

                try
                {
                    var options = new HttpRequestOptions()
                    {
                        Method = HttpMethod.Get,
                        Timeout = HTTP_REQ_TIMEOUT,
                        UseGZipEncoding = true,
                    };

                    WebHelper.SendRequest(serviceUrl, "wsdl", options);
                }
                catch (WebException e)
                {
                    var response = e.Response as HttpWebResponse;
                    if (response == null || response.StatusCode != HttpStatusCode.Unauthorized)
                    {
                        throw;
                    }

                    // HTTP_STATUS_DENIED (401):
                    // The requested resource requires user authentication.
                    throw new AuthenticationException();
                }
            }

            private static HttpWebRequest _CreateWebRequest(string url,
                NetworkCredential credentials)
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.ContentType = "text/xml;charset=\"utf-8\"";
                req.Method = "GET";
                req.Accept = "text/xml";
                req.Timeout = HTTP_REQ_TIMEOUT;
                req.Credentials = credentials;
                req.Referer = AgsHelper.RefererValue;

                return req;
            }

            #endregion private methods

            #region private fields
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            private string _soapUrl;
            private string _tokenServiceUrl;
            private string _lastToken;    
            private bool _requiresHttpAuthentication;
            private bool _requiresTokens;
            private bool _isInited = false;
            private object _tokenLocker = new object(); // mt

            /// <summary>
            /// Token expiration date.
            /// </summary>
            private DateTime _expirationTime;

            #endregion private fields
        }

        #endregion private classes

    }
}
