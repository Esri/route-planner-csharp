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
using System.Collections.Generic;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// ServersInfoWrap class.
    /// </summary>
    internal class ServersInfoWrap
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ServersInfoWrap(ServersInfo settings,
            UserServersInfo userSettings)
        {
            Debug.Assert(settings != null);
            Debug.Assert(userSettings != null);

            if (userSettings.Servers == null)
                userSettings.Servers = new List<UserServerInfo>();

            List<UserServerInfo> userServers = userSettings.Servers;
            foreach (ServerInfo info in settings.Servers)
            {
                if (!String.IsNullOrEmpty(info.Name))
                {
                    UserServerInfo userInfo = _FindServerInfo(info.Name,
                        userServers);

                    if (userInfo == null)
                    {
                        userInfo = new UserServerInfo();
                        userInfo.Name = info.Name;
                        userServers.Add(userInfo);
                    }

                    ServerInfoWrap wrap = new ServerInfoWrap(info, userInfo);
                    _servers.Add(wrap);
                }
                else
                    Logger.Warning("Server configuration error: invalid name.");
            }
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ICollection<ServerInfoWrap> Servers
        {
            get { return _servers; }
        }

        #endregion public properties

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static UserServerInfo _FindServerInfo(string name,
            List<UserServerInfo> coll)
        {
            Debug.Assert(name != null);
            Debug.Assert(coll != null);

            UserServerInfo resInfo = null;
            foreach (UserServerInfo info in coll)
            {
                if (!String.IsNullOrEmpty(info.Name) &&
                    info.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    resInfo = info;
                    break;
                }
            }

            return resInfo;
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<ServerInfoWrap> _servers = new List<ServerInfoWrap>();

        #endregion private members
    }

    /// <summary>
    /// ServerInfoWrap class.
    /// </summary>
    internal class ServerInfoWrap
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ServerInfoWrap(ServerInfo settings, UserServerInfo userSettings)
        {
            Debug.Assert(settings != null);
            Debug.Assert(userSettings != null);

            _settings = settings;
            _userSettings = userSettings;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string Name
        {
            get { return _settings.Name; }
        }

        public string Title
        {
            get { return _settings.Title; }
        }

        public string Description
        {
            get { return _settings.Description; }
        }

        public string HelpPrompt
        {
            get { return _settings.HelpPrompt; }
        }

        public string Authentication
        {
            get { return _settings.Authentication; }
        }

        public string Url
        {
            get { return _settings.Url; }
        }

        public CredentialsInfo Credentials
        {
            get { return _userSettings.Credentials; }
            set { _userSettings.Credentials = value; }
        }

        /// <summary>
        /// Token type.
        /// </summary>
        public TokenType TokenType
        {
            get { return _settings.TokenType; }
        }

        #endregion public properties

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ServerInfo _settings;
        private UserServerInfo _userSettings;

        #endregion private members
    }
}
