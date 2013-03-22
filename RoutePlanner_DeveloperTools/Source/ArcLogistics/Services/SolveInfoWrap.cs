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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// SolveInfoWrap class.
    /// </summary>
    internal class SolveInfoWrap
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public SolveInfoWrap(SolveInfo settings, UserSolveInfo userSettings)
        {
            Debug.Assert(settings != null);
            Debug.Assert(userSettings != null);

            if (userSettings.SolverSettingsInfo == null)
                userSettings.SolverSettingsInfo = new UserSolverSettingsInfo();

            // restrictions
            _InitRestrictions(settings, userSettings);

            // parameters
            _InitAttrParameters(settings, userSettings);

            // services
            _InitServices(settings);

            _settings = settings;
            _userSettings = userSettings;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Max allowed number of orders in sync vrp request.
        /// </summary>
        public int MaxSyncVrpRequestOrdersCount
        {
            get { return _settings.SolverSettingsInfo.MaxSyncVrpRequestOrderCount; }
        }

        /// <summary>
        /// Max allowed number of routes in sync vrp request.
        /// </summary>
        public int MaxSyncVrpRequestRoutesCount
        {
            get { return _settings.SolverSettingsInfo.MaxSyncVrpRequestRouteCount; }
        }

        /// <summary>
        /// U-Turn policy option: make a U-Turn at intersections.
        /// </summary>
        public bool UTurnAtIntersections
        {
            get
            {
                var defaultPolicy = _settings.SolverSettingsInfo.UTurnAtIntersections;
                var policy =
                    _userSettings.SolverSettingsInfo.UTurnAtIntersections.GetValueOrDefault(
                    defaultPolicy);

                return policy;
            }

            set
            {
                _userSettings.SolverSettingsInfo.UTurnAtIntersections = value;
            }
        }

        /// <summary>
        /// U-Turn policy option: make a U-Turn at dead ends.
        /// </summary>
        public bool UTurnAtDeadEnds
        {
            get
            {
                var defaultPolicy = _settings.SolverSettingsInfo.UTurnAtDeadEnds;
                var policy =
                    _userSettings.SolverSettingsInfo.UTurnAtDeadEnds.GetValueOrDefault(
                    defaultPolicy);

                return policy;
            }

            set
            {
                _userSettings.SolverSettingsInfo.UTurnAtDeadEnds = value;
            }
        }

        /// <summary>
        /// Curb Approach option: make a U-Turn at stops.
        /// </summary>
        public bool UTurnAtStops
        {
            get
            {
                var defaultPolicy = _settings.SolverSettingsInfo.UTurnAtStops;
                var policy =
                    _userSettings.SolverSettingsInfo.UTurnAtStops.GetValueOrDefault(
                    defaultPolicy);

                return policy;
            }

            set
            {
                _userSettings.SolverSettingsInfo.UTurnAtStops = value;
            }
        }

        /// <summary>
        /// Curb Approach option: must stop on order side of
        /// street Curb Approach option.
        /// </summary>
        public bool StopOnOrderSide
        {
            get
            {
                var defaultPolicy = _settings.SolverSettingsInfo.StopOnOrderSide;
                var policy =
                    _userSettings.SolverSettingsInfo.StopOnOrderSide.GetValueOrDefault(
                    defaultPolicy);

                return policy;
            }

            set
            {
                _userSettings.SolverSettingsInfo.StopOnOrderSide = value;
            }
        }

        /// <summary>
        /// This property allows to override Driving Side rule for cases
        /// when you are in running ArcLogistics in country with Left Side driving rules, but
        /// generating routes for country with Right Side driving rules.
        /// </summary>
        public bool? DriveOnRightSideOfTheRoad
        {
            get
            {
                string value = _settings.SolverSettingsInfo.DriveOnRightSideOfTheRoad;

                if (string.IsNullOrEmpty(value))
                    return null;
                else
                {
                    bool result = true;

                    if (bool.TryParse(value, out result))
                        return result;
                    else
                        return null;
                }
            }

            set
            {
                _settings.SolverSettingsInfo.DriveOnRightSideOfTheRoad = value.ToString();
            }
        }

        public bool UseDynamicPoints
        {
            get { return _settings.SolverSettingsInfo.UseDynamicPoints; }
        }

        public string TWPreference
        {
            get { return _settings.SolverSettingsInfo.TWPreference; }
        }

        public bool SaveOutputLayer
        {
            get { return _settings.SolverSettingsInfo.SaveOutputLayer; }
        }

        public bool ExcludeRestrictedStreets
        {
            get { return _settings.SolverSettingsInfo.ExcludeRestrictedStreets; }
        }

        /// <summary>
        /// Arrive and depart delay.
        /// </summary>
        public int ArriveDepartDelay
        {
            get
            {
                var defaultDelay = _settings.SolverSettingsInfo.ArriveDepartDelay;
                var delay = _userSettings.SolverSettingsInfo.ArriveDepartDelay
                    .GetValueOrDefault(defaultDelay);

                return delay;
            }

            set { _userSettings.SolverSettingsInfo.ArriveDepartDelay = value; }
        }

        public VrpServiceInfo VrpService
        {
            get { return _vrpService; }
        }

        /// <summary>
        /// Gets reference to the service info for the synchronous VRP service.
        /// </summary>
        public VrpServiceInfo SyncVrpService
        {
            get;
            private set;
        }

        public RouteServiceInfo RouteService
        {
            get { return _routeService; }
        }

        /// <summary>
        /// Discovery service.
        /// </summary>
        public DiscoveryServiceInfo DiscoveryService
        {
            get { return _discoveryService; }
        }

        public ICollection<string> RestrictionNames
        {
            get { return _restrictionNames.AsReadOnly();  }
        }

        public ICollection<RouteAttrInfo> AttrParameters
        {
            get { return _mergedParams.AsReadOnly(); }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RestrictionInfo GetRestriction(string name)
        {
            RestrictionInfo info = _FindRestriction(name, _userRestrictions);
            if (info == null)
                info = _FindRestriction(name, _defaultRestrictions);

            return info;
        }

        public RestrictionInfo GetUserRestriction(string name)
        {
            return _FindRestriction(name, _userRestrictions);
        }

        public void AddRestriction(RestrictionInfo info)
        {
            Debug.Assert(info != null);
            _userRestrictions.Add(info);
        }

        public RouteAttrInfo GetAttrParameter(string attrName,
            string paramName)
        {
            RouteAttrInfo info = _FindAttrParameter(attrName, paramName,
                _userParams);

            if (info == null)
                info = _FindAttrParameter(attrName, paramName, _defaultParams);

            return info;
        }

        public RouteAttrInfo GetUserAttrParameter(string attrName,
            string paramName)
        {
            return _FindAttrParameter(attrName, paramName,
                _userParams);
        }

        public void AddAttrParameter(RouteAttrInfo info)
        {
            Debug.Assert(info != null);
            _userParams.Add(info);
        }

        /// <summary>
        /// Gets first VRP service info from the VRP info object.
        /// </summary>
        /// <param name="vrpInfo">The reference to the
        /// <see cref="T:ESRI.ArcLogistics.Services.Serialization.VrpInfo"/>
        /// object to get servce info from.</param>
        /// <returns>Reference to the first VRP service info or null if there
        /// is no one.</returns>
        private static VrpServiceInfo _GetFirstServiceInfo(VrpInfo vrpInfo)
        {
            if (vrpInfo == null)
            {
                return null;
            }

            if (vrpInfo.ServiceInfo == null)
            {
                return null;
            }

            return vrpInfo.ServiceInfo.FirstOrDefault();
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _InitRestrictions(SolveInfo settings,
            UserSolveInfo userSettings)
        {
            // user restrictions
            if (userSettings.SolverSettingsInfo.RestrictionsInfo == null)
                userSettings.SolverSettingsInfo.RestrictionsInfo = new UserRestrictionsInfo();

            if (userSettings.SolverSettingsInfo.RestrictionsInfo.Restrictions == null)
                userSettings.SolverSettingsInfo.RestrictionsInfo.Restrictions = new List<RestrictionInfo>();

            _userRestrictions = userSettings.SolverSettingsInfo.RestrictionsInfo.Restrictions;

            // default (read-only) restrictions
            if (settings.SolverSettingsInfo != null &&
                settings.SolverSettingsInfo.RestrictionsInfo != null &&
                settings.SolverSettingsInfo.RestrictionsInfo.Restrictions != null)
            {
                _defaultRestrictions = new List<RestrictionInfo>(
                    settings.SolverSettingsInfo.RestrictionsInfo.Restrictions);
            }

            if (_defaultRestrictions == null)
                _defaultRestrictions = new List<RestrictionInfo>();

            // fill merged names list
            _AddRestrictionNames(_defaultRestrictions);
            _AddRestrictionNames(_userRestrictions);
        }

        private void _InitAttrParameters(SolveInfo settings,
            UserSolveInfo userSettings)
        {
            // user parameters
            if (userSettings.SolverSettingsInfo.AttributeParamsInfo == null)
                userSettings.SolverSettingsInfo.AttributeParamsInfo = new UserRouteAttrsInfo();

            if (userSettings.SolverSettingsInfo.AttributeParamsInfo.AttributeParams == null)
                userSettings.SolverSettingsInfo.AttributeParamsInfo.AttributeParams = new List<RouteAttrInfo>();

            _userParams = userSettings.SolverSettingsInfo.AttributeParamsInfo.AttributeParams;

            // default (read-only) parameters
            if (settings.SolverSettingsInfo != null &&
                settings.SolverSettingsInfo.AttributeParamsInfo != null &&
                settings.SolverSettingsInfo.AttributeParamsInfo.AttributeParams != null)
            {
                _defaultParams = new List<RouteAttrInfo>(
                    settings.SolverSettingsInfo.AttributeParamsInfo.AttributeParams);
            }

            if (_defaultParams == null)
                _defaultParams = new List<RouteAttrInfo>();

            // fill merged list
            _mergedParams = new List<RouteAttrInfo>(_userParams);
            foreach (RouteAttrInfo defParam in _defaultParams)
            {
                if (!String.IsNullOrEmpty(defParam.AttrName) &&
                    !String.IsNullOrEmpty(defParam.ParamName))
                {
                    RouteAttrInfo param = _FindAttrParameter(defParam.AttrName,
                        defParam.ParamName,
                        _mergedParams);

                    if (param == null)
                        _mergedParams.Add(defParam);
                }
            }
        }

        /// <summary>
        /// Initialize services.
        /// </summary>
        /// <param name="settings">Configuration settings.</param>
        private void _InitServices(SolveInfo settings)
        {
            // Initialize VRP and Sync VRP services.
            _vrpService = _GetFirstServiceInfo(settings.VrpInfo);
            this.SyncVrpService = _GetFirstServiceInfo(settings.SyncVrpInfo);

            // Initialize route service.
            if (settings.RoutingInfo != null &&
                settings.RoutingInfo.ServiceInfo != null &&
                settings.RoutingInfo.ServiceInfo.Length > 0)
            {
                _routeService = settings.RoutingInfo.ServiceInfo[0];
            }

            // Initialize discovery service.
            if (settings.DiscoveryInfo != null &&
                settings.DiscoveryInfo.ServiceInfo != null)
            {
                _discoveryService = settings.DiscoveryInfo.ServiceInfo.FirstOrDefault();
            }
        }

        private void _AddRestrictionNames(List<RestrictionInfo> coll)
        {
            Debug.Assert(coll != null);

            foreach (RestrictionInfo info in coll)
            {
                if (!String.IsNullOrEmpty(info.Name) &&
                    !_restrictionNames.Contains(info.Name))
                {
                    _restrictionNames.Add(info.Name);
                }
            }
        }

        private static RestrictionInfo _FindRestriction(string name,
            List<RestrictionInfo> coll)
        {
            Debug.Assert(name != null);
            Debug.Assert(coll != null);

            RestrictionInfo info = null;
            foreach (RestrictionInfo ri in coll)
            {
                if (ri.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    info = ri;
                    break;
                }
            }

            return info;
        }

        private static RouteAttrInfo _FindAttrParameter(string attrName,
            string paramName,
            List<RouteAttrInfo> coll)
        {
            Debug.Assert(attrName != null);
            Debug.Assert(paramName != null);
            Debug.Assert(coll != null);

            RouteAttrInfo attr = null;
            foreach (RouteAttrInfo param in coll)
            {
                if (param.AttrName.Equals(attrName, StringComparison.InvariantCultureIgnoreCase) &&
                    param.ParamName.Equals(paramName, StringComparison.InvariantCultureIgnoreCase))
                {
                    attr = param;
                    break;
                }
            }

            return attr;
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<string> _restrictionNames = new List<string>();
        private List<RestrictionInfo> _defaultRestrictions;
        private List<RestrictionInfo> _userRestrictions;

        private List<RouteAttrInfo> _defaultParams;
        private List<RouteAttrInfo> _userParams;
        private List<RouteAttrInfo> _mergedParams;

        private VrpServiceInfo _vrpService;
        private RouteServiceInfo _routeService;

        /// <summary>
        /// Discovery service information.
        /// </summary>
        private DiscoveryServiceInfo _discoveryService;

        private SolveInfo _settings;
        private UserSolveInfo _userSettings;

        #endregion private members
    }
}
