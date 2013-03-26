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

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Connection to the ArcGIS server.
    /// </summary>
    internal sealed class AgsServerConnection
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the AgsServerConnection class
        /// with the specified authenticator instance, connection parameters
        /// and ArcGIS server to connect to.
        /// </summary>
        /// <param name="authenticator">Authenticator instance to be used
        /// for accessing ArcGIS server.</param>
        /// <param name="parameters">Connection parameters instance.</param>
        /// <param name="server">ArcGIS server instance to connect to.</param>
        public AgsServerConnection(
            IAgsServerAuthenticator authenticator,
            AgsServerConnectionParameters parameters,
            AgsServer server)
        {
            Debug.Assert(authenticator != null);
            Debug.Assert(parameters != null);
            Debug.Assert(server != null);

            _authenticator = authenticator;
            _parameters = parameters;
            _server = server;

            this.RequiresHttpAuthentication = _server.RequiresHttpAuthentication;
        }
        #endregion

        #region public properties
        /// <summary>
        /// Gets title of the ArcGIS server this instance is connected to.
        /// </summary>
        public string Title
        {
            get
            {
                return _server.Title;
            }
        }

        /// <summary>
        /// Gets url of the ArcGIS server this instance is connected to.
        /// </summary>
        public string Url
        {
            get
            {
                return _parameters.SoapUrl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether ArcGIS server requests require
        /// tokens.
        /// </summary>
        public bool RequiresTokens
        {
            get
            {
                return _authenticator.RequiresTokens;
            }
        }

        /// <summary>
        /// Gets last token instance.
        /// </summary>
        public string LastToken
        {
            get
            {
                return _authenticator.LastToken;
            }
        }

        /// <summary>
        /// Gets a value indicating if the ArcGIS server requires HTTP
        /// authentication.
        /// </summary>
        public bool RequiresHttpAuthentication
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets credential to be used for accessing ArcGIS server.
        /// </summary>
        public NetworkCredential Credentials
        {
            get
            {
                return _parameters.Credentials;
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Generates new token to be used for ArcGIS server requests.
        /// </summary>
        /// <returns>New token which can be used for ArcGIS server requests.</returns>
        public string GenerateToken()
        {
            if (!_authenticator.RequiresTokens)
            {
                throw new InvalidOperationException(
                    Properties.Messages.Error_TokenAuthNotSupported);
            }

            string token = null;
            try
            {
                token = _authenticator.GenerateToken(_parameters.Credentials);
            }
            catch (AuthenticationException e)
            {
                throw ServiceHelper.CreateAuthException(_server, e);
            }
            catch (Exception e)
            {
                if (ServiceHelper.IsCommunicationError(e))
                {
                    throw ServiceHelper.CreateCommException(_server, e);
                }

                throw;
            }

            return token;
        }
        #endregion

        #region private fields
        /// <summary>Authenticator instance to be used
        /// for accessing ArcGIS server.
        /// </summary>
        private IAgsServerAuthenticator _authenticator;

        /// <summary>
        /// Connection parameters instance.
        /// </summary>
        private AgsServerConnectionParameters _parameters;

        /// <summary>
        /// ArcGIS server instance to connect to.
        /// </summary>
        private AgsServer _server;
        #endregion
    }
}
