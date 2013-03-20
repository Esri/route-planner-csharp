using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// ServiceHelper class.
    /// </summary>
    internal static class ServiceHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Finds server by name.
        /// </summary>
        public static AgsServer FindServerByName(string name,
            ICollection<AgsServer> servers)
        {
            Debug.Assert(name != null);
            Debug.Assert(servers != null);

            AgsServer res = null;
            foreach (AgsServer server in servers)
            {
                if (server.Name != null &&
                    server.Name.Equals(name,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    res = server;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Safely closes communication object.
        /// </summary>
        public static void CloseCommObject(ICommunicationObject commObj)
        {
            if (commObj != null)
            {
                if (commObj.State == CommunicationState.Faulted)
                {
                    commObj.Abort();
                }
                else
                {
                    try
                    {
                        commObj.Close();
                    }
                    catch
                    {
                        commObj.Abort();
                    }
                }
            }
        }

        /// <summary>
        /// Validates server state.
        /// </summary>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Is thrown if server state is unauthorized.</exception>
        public static void ValidateServerState(AgsServer server)
        {
            Debug.Assert(server != null);

            if (server.State == AgsServerState.Unauthorized)
            {
                // server is not authorized
                throw CreateAuthException(server);
            }
            else if (server.State == AgsServerState.Unavailable)
            {
                // try to reconnect server
                server.Reconnect();
            }
        }

        /// <summary>
        /// Creates AuthenticationException object.
        /// </summary>
        public static AuthenticationException CreateAuthException(
            AgsServer server)
        {
            Debug.Assert(server != null);
            return CreateAuthException(server, null);
        }

        /// <summary>
        /// Creates AuthenticationException object.
        /// </summary>
        /// <param name="server">The server authentication failed to.</param>
        /// <param name="inner">Exception instance which caused this one
        /// to be thrown.</param>
        /// <returns>AuthenticationException object containing the specified
        /// server name and inner exception.</returns>
        public static AuthenticationException CreateAuthException(
            AgsServer server,
            Exception inner)
        {
            Debug.Assert(server != null);
            return CreateAuthException(server.Title, inner);
        }

        /// <summary>
        /// Creates AuthenticationException object.
        /// </summary>
        /// <param name="serverName">The name of the server authentication
        /// failed to.</param>
        /// <param name="inner">Exception instance which caused this one
        /// to be thrown.</param>
        /// <returns>AuthenticationException object containing the specified
        /// server name and inner exception.</returns>
        public static AuthenticationException CreateAuthException(
            string serverName,
            Exception inner)
        {
            Debug.Assert(serverName != null);

            var message = string.Format(
                Properties.Messages.Error_ServerUnauthorized,
                serverName);

            return new AuthenticationException(message, serverName, inner);
        }

        /// <summary>
        /// Creates CommunicationException object.
        /// </summary>
        public static CommunicationException CreateCommException(
            AgsServer server,
            Exception ex)
        {
            Debug.Assert(server != null);

            return CreateCommException(server.Title, ex);
        }

        /// <summary>
        /// Creates CommunicationException object using the specified server title
        /// and exception instance.
        /// </summary>
        public static CommunicationException CreateCommException(
            string serverTitle,
            Exception ex)
        {
            Debug.Assert(serverTitle != null);

            // get error code
            CommunicationError code = ServiceHelper.GetCommunicationError(ex);

            return new ESRI.ArcLogistics.CommunicationException(
                String.Format(Properties.Messages.Error_ServerUnavailable, serverTitle),
                serverTitle,
                code,
                ex);
        }

        /// <summary>
        /// Returns a boolean value indicating whether exception is related to
        /// communication problems.
        /// </summary>
        public static bool IsCommunicationError(Exception ex)
        {
            return (ex is WebException ||
                ex is TimeoutException ||
                ex is EndpointNotFoundException ||
                (ex is System.ServiceModel.CommunicationException &&
                !(ex is System.ServiceModel.FaultException)));
        }

        /// <summary>
        /// Converts given exception to CommunicationException if it's related
        /// to communication problems.
        /// </summary>
        public static Exception ConvertCommException(AgsServer server,
            Exception ex)
        {
            return IsCommunicationError(ex) ? CreateCommException(server, ex) : ex;
        }

        /// <summary>
        /// Sets credentials to service client.
        /// </summary>
        public static void SetClientCredentials<TChannel>(
            ClientBase<TChannel> client,
            NetworkCredential credentials)
            where TChannel : class
        {
            Debug.Assert(client != null);
            Debug.Assert(credentials != null);

            // set security binding parameters
            BasicHttpBinding httpBinding = client.Endpoint.Binding as
                BasicHttpBinding;

            if (httpBinding == null)
                throw new NotSupportedException(Properties.Messages.Error_UnsupportedHttpBinding);

            if (httpBinding.Security.Mode == BasicHttpSecurityMode.None)
            {
                httpBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            }

            httpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

            // set credentials to SOAP client
            client.ClientCredentials.UserName.UserName = credentials.UserName;
            client.ClientCredentials.UserName.Password = credentials.Password;
        }

        /// <summary>
        /// Creates Web HTTP binding with default parameters.
        /// </summary>
        /// <param name="name">Name for created binding.</param>
        /// <returns>Created default binding.</returns>
        public static WebHttpBinding CreateWebHttpBinding(string name)
        {
            var binding = new WebHttpBinding
            {
                Name = name,

                OpenTimeout = OPEN_TIMEOUT,
                SendTimeout = SEND_TIMEOUT,
                ReceiveTimeout = RECEIVE_TIMEOUT,

                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,

                Security = new WebHttpSecurity
                {
                    Mode = WebHttpSecurityMode.None,
                    Transport = new HttpTransportSecurity
                    {
                        ClientCredentialType = HttpClientCredentialType.None,
                        ProxyCredentialType = HttpProxyCredentialType.None,
                        Realm = string.Empty,
                    },
                },

                AllowCookies = false,
                BypassProxyOnLocal = false,
                MaxBufferSize = 6553600,
                MaxBufferPoolSize = 524288,
                MaxReceivedMessageSize = 6553600,
                WriteEncoding = Encoding.UTF8,
                TransferMode = TransferMode.Buffered,
                UseDefaultWebProxy = true,
            };

            return binding;
        }

        /// <summary>
        /// Creates HTTP binding with default parameters.
        /// </summary>
        /// <param name="name">Name for created binding.</param>
        /// <param name="endpointUrl">The URL to the service endpoint.</param>
        /// <exception cref="T:System.UriFormatException"><paramref name="endpointUrl"/>
        /// contains invalid URL string.</exception>
        /// <returns>Created default binding.</returns>
        public static BasicHttpBinding CreateHttpBinding(
            string name,
            string endpointUrl)
        {
            Debug.Assert(!string.IsNullOrEmpty(endpointUrl));

            var uri = new Uri(endpointUrl);
            var securityMode = BasicHttpSecurityMode.None;
            SECURITY_MODES_FOR_SCHEMES.TryGetValue(uri.Scheme, out securityMode);

            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = name;
            binding.OpenTimeout = OPEN_TIMEOUT;
            binding.SendTimeout = SEND_TIMEOUT;
            binding.ReceiveTimeout = RECEIVE_TIMEOUT;
            binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            binding.Security.Mode = securityMode;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Transport.Realm = "";
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            binding.Security.Message.AlgorithmSuite = System.ServiceModel.Security.SecurityAlgorithmSuite.Default;
            binding.AllowCookies = false;
            binding.BypassProxyOnLocal = false;
            binding.MaxBufferSize = 6553600;
            binding.MaxBufferPoolSize = 524288;
            binding.MaxReceivedMessageSize = 6553600;
            binding.MessageEncoding = WSMessageEncoding.Text;
            binding.TextEncoding = System.Text.Encoding.UTF8;
            binding.TransferMode = TransferMode.Buffered;
            binding.UseDefaultWebProxy = true;

            return binding;
        }

        /// <summary>
        /// Creates client for the service with the specified name and endpoint
        /// URL.
        /// </summary>
        /// <typeparam name="TServiceClient">The type of the service client
        /// to create.</typeparam>
        /// <param name="name">The name of the service binding.</param>
        /// <param name="endpointUrl">The URL to the service endpoint.</param>
        /// <returns>A new instance of the service client.</returns>
        public static TServiceClient CreateServiceClient<TServiceClient>(
            string name,
            string endpointUrl)
        {
            Debug.Assert(!string.IsNullOrEmpty(endpointUrl));

            var binding = CreateHttpBinding(name, endpointUrl);
            var endpointAddress = new EndpointAddress(endpointUrl);

            var client = (TServiceClient)Activator.CreateInstance(
                typeof(TServiceClient),
                new object[] { binding, endpointAddress });

            return client;
        }

        /// <summary>
        /// Gets communication error code for the specified exception.
        /// </summary>
        /// <param name="exception">Exception to get error code for.</param>
        /// <returns>Communication error code corresponding to the specified
        /// exception.</returns>
        public static CommunicationError GetCommunicationError(Exception exception)
        {
            Debug.Assert(exception != null);

            while (exception != null)
            {
                var webException = exception as WebException;
                if (webException != null)
                {
                    return _GetCommunicationError(webException);
                }

                var socketException = exception as SocketException;
                if (socketException != null)
                {
                    return _GetCommunicationError(socketException);
                }

                exception = exception.InnerException;
            }

            return CommunicationError.Unknown;
        }
        #endregion public methods

        #region private methods
        /// <summary>
        /// Gets communication error code for the specified web exception.
        /// </summary>
        /// <param name="exception">Exception to get error code for.</param>
        /// <returns>Communication error code corresponding to the specified
        /// exception.</returns>
        private static CommunicationError _GetCommunicationError(
            WebException exception)
        {
            Debug.Assert(exception != null);

            if (exception.Response != null)
            {
                var response = (HttpWebResponse)exception.Response;
                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.ServiceUnavailable:
                        return CommunicationError.ServiceTemporaryUnavailable;

                    case HttpStatusCode.GatewayTimeout:
                    case HttpStatusCode.RequestTimeout:
                        return CommunicationError.ServiceResponseTimeout;

                    case HttpStatusCode.ProxyAuthenticationRequired:
                        return CommunicationError.ProxyAuthenticationRequired;

                    default:
                        return CommunicationError.Unknown;
                }
            }

            switch (exception.Status)
            {
                case WebExceptionStatus.KeepAliveFailure:
                    return CommunicationError.ServiceTemporaryUnavailable;

                case WebExceptionStatus.Timeout:
                    return CommunicationError.ServiceResponseTimeout;

                default:
                    return CommunicationError.Unknown;
            }
        }

        /// <summary>
        /// Gets communication error code for the specified socket exception.
        /// </summary>
        /// <param name="exception">Exception to get error code for.</param>
        /// <returns>Communication error code corresponding to the specified
        /// exception.</returns>
        private static CommunicationError _GetCommunicationError(
            SocketException exception)
        {
            Debug.Assert(exception != null);

            switch (exception.SocketErrorCode)
            {
                case SocketError.TimedOut:
                    return CommunicationError.ServiceResponseTimeout;

                default:
                    return CommunicationError.Unknown;
            }
        }
        #endregion private methods

        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // WCF service timeouts
        private static readonly TimeSpan OPEN_TIMEOUT = new TimeSpan(0, 5, 0); // 5 min
        private static readonly TimeSpan SEND_TIMEOUT = new TimeSpan(0, 5, 0); // 5 min
        private static readonly TimeSpan RECEIVE_TIMEOUT = new TimeSpan(0, 5, 0); // 5 min

        /// <summary>
        /// Stores mapping between service connection schemes and security modes.
        /// </summary>
        private static readonly Dictionary<string, BasicHttpSecurityMode> SECURITY_MODES_FOR_SCHEMES =
            new Dictionary<string,BasicHttpSecurityMode>()
        {
            { "http", BasicHttpSecurityMode.None },
            { "https", BasicHttpSecurityMode.Transport },
        };

        #endregion constants
    }
}
