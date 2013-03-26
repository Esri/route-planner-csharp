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
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// MapInfoWrap class.
    /// </summary>
    internal class MapInfoWrap
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MapInfoWrap(MapInfo settings, UserMapInfo userSettings)
        {
            Debug.Assert(settings != null);
            Debug.Assert(userSettings != null);

            _services = _CreateServices(settings, userSettings);
            _settings = settings;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ICollection<MapServiceInfoWrap> Services
        {
            get { return _services; }
        }

        public Envelope StartupExtent
        {
            get { return _settings.StartupExtent; }
        }

        public Envelope ImportCheckExtent
        {
            get { return _settings.ImportCheckExtent; }
        }

        #endregion public properties

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<MapServiceInfoWrap> _CreateServices(MapInfo settings,
            UserMapInfo userSettings)
        {
            Debug.Assert(settings != null);
            Debug.Assert(userSettings != null);

            if (userSettings.Services == null)
                userSettings.Services = new List<UserMapServiceInfo>();

            List<UserMapServiceInfo> userServices = userSettings.Services;
            List<MapServiceInfoWrap> wrapServices = new List<MapServiceInfoWrap>();

            foreach (MapServiceInfo info in settings.Services)
            {
                if (!String.IsNullOrEmpty(info.Name))
                {
                    UserMapServiceInfo userInfo = _FindServiceInfo(info.Name,
                        userServices);

                    if (userInfo == null)
                    {
                        userInfo = new UserMapServiceInfo();
                        userInfo.Name = info.Name;
                        userInfo.IsVisible = info.IsVisible;
                        userInfo.Opacity = info.Opacity;
                        userServices.Add(userInfo);
                    }

                    MapServiceInfoWrap wrap = new MapServiceInfoWrap(info,
                        userInfo);

                    wrapServices.Add(wrap);
                }
                else
                    Logger.Warning("Map service configuration error: invalid name.");
            }

            return wrapServices;
        }

        private static UserMapServiceInfo _FindServiceInfo(string name,
            List<UserMapServiceInfo> coll)
        {
            Debug.Assert(name != null);
            Debug.Assert(coll != null);

            UserMapServiceInfo resInfo = null;
            foreach (UserMapServiceInfo info in coll)
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

        private List<MapServiceInfoWrap> _services;
        private MapInfo _settings;

        #endregion private members
    }

    /// <summary>
    /// MapServiceInfoWrap class.
    /// </summary>
    internal class MapServiceInfoWrap
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MapServiceInfoWrap(MapServiceInfo settings,
            UserMapServiceInfo userSettings)
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

        public string Type
        {
            get { return _settings.Type; }
        }

        public string ServerName
        {
            get { return _settings.ServerName; }
        }

        public bool IsBaseMap
        {
            get { return _settings.IsBaseMap; }
        }

        public string Title
        {
            get { return _settings.Title; }
        }

        public string Url
        {
            get { return _settings.Url; }
        }

        public bool IsVisible
        {
            get { return _userSettings.IsVisible; }
            set { _userSettings.IsVisible = value; }
        }

        public double Opacity
        {
            get { return _userSettings.Opacity; }
            set { _userSettings.Opacity = value; }
        }

        #endregion public properties

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private MapServiceInfo _settings;
        private UserMapServiceInfo _userSettings;

        #endregion private members
    }
}
