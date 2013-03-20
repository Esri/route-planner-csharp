using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// U-Turn policy enumeration.
    /// Indicates how the U-turns at junctions that could occur during network traversal between
    /// stops are being handled by the solver.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum UTurnPolicy
    {
        /// <summary>
        /// U-turns are prohibited at all junctions. Note, however, that U-turns are still permitted
        /// at network locations even when this setting is chosen; however, you can set the
        /// individual network locations' CurbApproach property to prohibit U-turns.
        /// </summary>
        Nowhere = 1,
        /// <summary>
        /// U-turns are prohibited at all junctions, except those that have only one adjacent edge
        /// (a dead end).
        /// </summary>
        AtDeadEnds = 2,
        /// <summary>
        /// U-turns are prohibited at junctions where exactly two adjacent edges meet but are
        /// permitted at intersections (any junction with three or more adjacent edges) or dead
        /// ends (junctions with exactly one adjacent edge).
        /// </summary>
        AtDeadEndsAndIntersections = 3,
    }

    /// <summary>
    /// Restriction class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Restriction : INotifyPropertyChanged
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string PROP_NAME_IsEnabled = "IsEnabled";

        #endregion constants

        #region INotifyPropertyChanged members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged members

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal Restriction(string name, string description, bool editable,
            bool enabled)
        {
            _name = name;
            _description = description;
            _editable = editable;
            _enabled = enabled;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string NetworkAttributeName
        {
            get { return _name; }
        }

        public string Description
        {
            get { return _description; }
        }

        public bool IsEditable
        {
            get { return _editable; }
        }

        public bool IsEnabled
        {
            get { return _enabled; }
            set
            {
                if (!_editable)
                    throw new InvalidOperationException(Properties.Messages.Error_RestrictionIsNotEditable);

                if (value != _enabled)
                {
                    _enabled = value;
                    _NotifyPropertyChanged(PROP_NAME_IsEnabled);
                }
            }
        }

        #endregion public properties

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name;
        private string _description;
        private bool _editable;
        private bool _enabled;

        #endregion private fields
    }

    /// <summary>
    /// SolverSettings class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SolverSettings
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        private const string LOG_UNKNOWN_RESTRICTION = "Restriction {0} does not exist in network description.";
        private const string LOG_INVALID_NETWORK_ATTR = "Invalid network attribute: {0}, {1}";

        #endregion constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal SolverSettings(SolveInfoWrap settings,
            NetworkDescription netDesc)
        {
            Debug.Assert(settings != null);
            Debug.Assert(netDesc != null);

            _settings = settings;
            _netDesc = netDesc;

            // Init restrictions.
            if (_netDesc != null)
                _InitRestrictions(settings, netDesc);

            // parameters
            _InitAttrParameters(settings);

            // Initialize U-Turn and curb approach policies.
            _InitUTurnPolicies(settings);
            _InitCurbApproachPolicies(settings);
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ICollection<Restriction> Restrictions
        {
            get { return _restrictions.AsReadOnly(); }
        }

        /// <summary>
        /// UTurn at intersections.
        /// </summary>
        public bool UTurnAtIntersections
        {
            get
            {
                return _settings.UTurnAtIntersections;
            }

            set
            {
                _settings.UTurnAtIntersections = value;
            }
        }

        /// <summary>
        /// UTurn at dead ends.
        /// </summary>
        public bool UTurnAtDeadEnds
        {
            get
            {
                return _settings.UTurnAtDeadEnds;
            }

            set
            {
                _settings.UTurnAtDeadEnds = value;
            }
        }

        /// <summary>
        /// UTurn at stops.
        /// </summary>
        public bool UTurnAtStops
        {
            get
            {
                return _settings.UTurnAtStops;
            }

            set
            {
                _settings.UTurnAtStops = value;
            }
        }

        /// <summary>
        /// Stop on order side.
        /// </summary>
        public bool StopOnOrderSide
        {
            get
            {
                return _settings.StopOnOrderSide;
            }

            set
            {
                _settings.StopOnOrderSide = value;
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
                return _settings.DriveOnRightSideOfTheRoad;
            }

            set
            {
                _settings.DriveOnRightSideOfTheRoad = value;
            }
        }

        public bool UseDynamicPoints
        {
            get { return _settings.UseDynamicPoints; }
            //set { _settings.UseDynamicPoints = value; }
        }

        public string TWPreference
        {
            get { return _settings.TWPreference; }
            //set { _settings.TWPreference = value; }
        }

        public bool SaveOutputLayer
        {
            get { return _settings.SaveOutputLayer; }
            //set { _settings.SaveOutputLayer = value; }
        }

        public bool ExcludeRestrictedStreets
        {
            get { return _settings.ExcludeRestrictedStreets; }
            //set { _settings.ExcludeRestrictedStreets = value; }
        }

        /// <summary>
        /// Arrive and depart delay.
        /// </summary>
        public int ArriveDepartDelay
        {
            get { return _settings.ArriveDepartDelay; }
            set { _settings.ArriveDepartDelay = value; }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public bool GetNetworkAttributeParameterValue(string attrName,
            string paramName,
            out object paramValue)
        {
            Debug.Assert(attrName != null);
            Debug.Assert(paramName != null);

            paramValue = null;
            bool res = false;

            // find parameter in network description
            NetworkAttributeParameter param = null;
            if (_FindParamInNetworkDesc(attrName, paramName, out param))
            {
                // try to get value from attribute settings
                RouteAttrInfo attrInfo = _settings.GetAttrParameter(attrName, paramName);
                if (attrInfo != null)
                {
                    if (_DeserializeParamValue(attrInfo.Value, param.Type, out paramValue))
                        res = true;
                }
                else
                {
                    // get default value from network description
                    paramValue = param.DefaultValue;
                    res = true;
                }
            }

            return res;
        }

        public void SetNetworkAttributeParameterValue(string attrName,
            string paramName,
            object paramValue)
        {
            Debug.Assert(attrName != null);
            Debug.Assert(paramName != null);

            // find parameter in network description
            NetworkAttributeParameter param = null;
            if (!_FindParamInNetworkDesc(attrName, paramName, out param))
            {
                // cannot find attribute
                throw new ArgumentException(
                    Properties.Messages.Error_InvalidNetworkAttr);
            }

            string serializedValue = String.Empty;

            if (paramValue != null)
            {
                // If new value is not empty string or if parameter doesnt accept empty string - 
                // try to serialize current value.
                if (paramValue as string != string.Empty || !param.IsEmptyStringValid)
                {
                    object convertedValue = paramValue;

                    // check if value type is correct
                    if (!param.Type.Equals(paramValue.GetType()))
                    {
                        if (!_ConvertParamValue(paramValue, param.Type, out convertedValue))
                        {
                            // provided value cannot be converted to required type
                            throw new ArgumentException(
                                Properties.Messages.Error_InvalidNetworkParamType);
                        }
                    }

                    // serialize value for storing
                    serializedValue = _SerializeParamValue(convertedValue);
                }
            }

            // try to find attribute in settings
            RouteAttrInfo attrInfo = _settings.GetUserAttrParameter(attrName, paramName);
            if (attrInfo != null)
            {
                // update existing attribute entry
                attrInfo.Value = serializedValue;
            }
            else
            {
                // add new attribute entry
                attrInfo = new RouteAttrInfo();
                attrInfo.AttrName = attrName;
                attrInfo.ParamName = paramName;
                attrInfo.Value = serializedValue;
                _settings.AddAttrParameter(attrInfo);
            }
        }

        /// <summary>
        /// Method gets U-Turn policy for orders according to settings.
        /// </summary>
        /// <returns>U-Turn policy.</returns>
        public UTurnPolicy GetUTurnPolicy()
        {
            UTurnPolicy policy = new UTurnPolicy();

            if (UTurnAtDeadEnds && UTurnAtIntersections)
            {
                policy = UTurnPolicy.AtDeadEndsAndIntersections;
            }
            else if (!UTurnAtDeadEnds && !UTurnAtIntersections)
            {
                policy = UTurnPolicy.Nowhere;
            }
            else if (UTurnAtDeadEnds && !UTurnAtIntersections)
            {
                policy = UTurnPolicy.AtDeadEnds;
            }
            else
            {
                // Not supported. Set default one.
                policy = UTurnPolicy.Nowhere;
            }

            return policy;
        }

        /// <summary>
        /// Method gets Curb Approach for orders according to settings.
        /// </summary>
        /// <returns>Curb approach.</returns>
        public CurbApproach GetOrderCurbApproach()
        {
            CurbApproach policy = new CurbApproach();

            if (UTurnAtStops && StopOnOrderSide)
            {
                policy = CurbApproach.Both;
            }
            else if (!UTurnAtStops && !StopOnOrderSide)
            {
                policy = CurbApproach.NoUTurns;
            }
            else if (!UTurnAtStops && StopOnOrderSide)
            {
                // Set Left side or Right side Curb Approach
                // depending on override setting or Country locale.
                policy = _GetLeftOrRightSideCurbApproach();
            }
            else
            {
                // Not supported. Set default one.
                policy = CurbApproach.Both;
            }

            return policy;
        }

        /// <summary>
        /// Method gets Curb Approach for depots according to settings.
        /// Depots doesn't support NoUTurns option as Orders.
        /// </summary>
        /// <returns>Curb approach.</returns>
        public CurbApproach GetDepotCurbApproach()
        {
            CurbApproach policy = new CurbApproach();

            if (UTurnAtStops && StopOnOrderSide)
            {
                policy = CurbApproach.Both;
            }
            else if (UTurnAtStops && !StopOnOrderSide)
            {
                policy = CurbApproach.Both;
            }
            else
            {
                // Set Left side or Right side Curb Approach
                // depending on override setting or Country locale.
                policy = _GetLeftOrRightSideCurbApproach();
            }

            return policy;
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _InitRestrictions(SolveInfoWrap settings,
            NetworkDescription netDesc)
        {
            Debug.Assert(settings != null);
            Debug.Assert(netDesc != null);

            Dictionary<string, Restriction> restrictions = new Dictionary<
                string, Restriction>();

            // add all available restrictions with default data
            foreach (NetworkAttribute attr in netDesc.NetworkAttributes)
            {
                if (attr.UsageType == NetworkAttributeUsageType.Restriction &&
                    !String.IsNullOrEmpty(attr.Name))
                {
                    restrictions.Add(attr.Name.ToLower(), new Restriction(
                        attr.Name,
                        String.Empty, // description
                        true, // editable is true by default
                        _IsRestrictionEnabled(attr.Name,
                            netDesc.EnabledRestrictionNames)));
                }
            }

            // override restrictions according to settings
            foreach (string name in settings.RestrictionNames)
            {
                RestrictionInfo ri = settings.GetRestriction(name);
                if (ri != null)
                {
                    string key = name.ToLower();

                    Restriction rest = null;
                    if (restrictions.TryGetValue(key, out rest))
                    {
                        // override restriction
                        restrictions[key] = new Restriction(
                            rest.NetworkAttributeName, // NOTE: use name from server
                            ri.Description,
                            ri.IsEditable,
                            ri.IsTurnedOn);
                    }
                    else
                        Logger.Warning(String.Format(LOG_UNKNOWN_RESTRICTION, name));
                }
            }

            // attach to events
            foreach (Restriction rest in restrictions.Values)
                rest.PropertyChanged += new PropertyChangedEventHandler(Restriction_PropertyChanged);

            _restrictions.AddRange(restrictions.Values);
        }

        private void Restriction_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Restriction rest = sender as Restriction;
            Debug.Assert(rest != null);

            RestrictionInfo info = _settings.GetUserRestriction(
                rest.NetworkAttributeName);

            if (info != null)
            {
                // currently only Enabled property can be modified
                info.IsTurnedOn = rest.IsEnabled;
            }
            else
            {
                // add new restriction entry to config
                info = new RestrictionInfo();
                info.Name = rest.NetworkAttributeName;
                info.Description = rest.Description;
                info.IsEditable = rest.IsEditable;
                info.IsTurnedOn = rest.IsEnabled;
                _settings.AddRestriction(info);
            }
        }

        private void _InitAttrParameters(SolveInfoWrap settings)
        {
            Debug.Assert(settings != null);

            foreach (RouteAttrInfo attr in settings.AttrParameters)
            {
                bool isValid = false;

                // check if attribute settings are valid
                if (!String.IsNullOrEmpty(attr.AttrName) &&
                    !String.IsNullOrEmpty(attr.ParamName) &&
                    attr.Value != null)
                {
                    // find parameter in network description
                    NetworkAttributeParameter param = null;
                    if (_FindParamInNetworkDesc(attr.AttrName, attr.ParamName, out param))
                    {
                        // check if parameter value is valid
                        if (_IsParamValueValid(attr.Value, param.Type))
                            isValid = true;
                    }
                }

                if (!isValid)
                {
                    Logger.Warning(String.Format(LOG_INVALID_NETWORK_ATTR,
                        attr.AttrName,
                        attr.ParamName));
                }
            }
        }


        /// <summary>
        /// Method provides initialization of U-Turn policies.
        /// </summary>
        /// <param name="settings">Settings.</param>
        private void _InitUTurnPolicies(SolveInfoWrap settings)
        {
            // Validate settings for U-Turn policies.
            if (settings.UTurnAtIntersections)
            {
                settings.UTurnAtDeadEnds = true;
            }
        }

        /// <summary>
        /// Method provides initialization of Curb approach policies.
        /// </summary>
        /// <param name="settings">Settings.</param>
        private void _InitCurbApproachPolicies(SolveInfoWrap settings)
        {
            // Validate settings for Curb Approach policies.
            if (settings.UTurnAtStops)
            {
                settings.StopOnOrderSide = true;
            }
        }

        private bool _FindParamInNetworkDesc(string attrName, string paramName,
            out NetworkAttributeParameter attrParameter)
        {
            Debug.Assert(attrName != null);
            Debug.Assert(paramName != null);

            attrParameter = null;

            bool found = false;
            foreach (NetworkAttribute attr in _netDesc.NetworkAttributes)
            {
                if (attr.Name.Equals(attrName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (NetworkAttributeParameter param in attr.Parameters)
                    {
                        if (param.Name.Equals(paramName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            attrParameter = param;
                            found = true;
                            break;
                        }
                    }

                    if (found)
                        break;
                }
            }

            return found;
        }

        private static bool _IsParamValueValid(string value, Type type)
        {
            object obj;
            return _DeserializeParamValue(value, type, out obj);
        }

        private static bool _IsRestrictionEnabled(string restrictionName,
            string[] enabledRestrictionNames)
        {
            Debug.Assert(restrictionName != null);
            Debug.Assert(enabledRestrictionNames != null);

            bool enabled = false;
            foreach (string name in enabledRestrictionNames)
            {
                if (name.Equals(restrictionName,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    enabled = true;
                    break;
                }
            }

            return enabled;
        }

        private static bool _ConvertParamValue(object value, Type conversionType,
            out object convertedValue)
        {
            Debug.Assert(value != null);
            Debug.Assert(conversionType != null);

            convertedValue = null;

            bool res = false;
            try
            {
                // try to convert value to required type
                convertedValue = Convert.ChangeType(value, conversionType,
                    fmtProvider);

                res = true;
            }
            catch { }

            return res;
        }

        private static bool _DeserializeParamValue(string valueStr, Type type,
            out object valueObj)
        {
            Debug.Assert(type != null);

            bool res = false;
            if (String.IsNullOrEmpty(valueStr))
            {
                if (type == typeof(string))
                    valueObj = valueStr;
                else
                    valueObj = null;

                res = true;
            }
            else
                res = _ConvertParamValue(valueStr, type, out valueObj);

            return res;
        }

        private static string _SerializeParamValue(object value)
        {
            Debug.Assert(value != null);
            return Convert.ToString(value, fmtProvider);
        }

        /// <summary>
        /// Method set Left side or Right side Curb Approach
        /// depending on override setting or Country locale.
        /// </summary>
        /// <returns>Left or Right Side Curb Approach.</returns>
        private CurbApproach _GetLeftOrRightSideCurbApproach()
        {
            CurbApproach policy = CurbApproach.Right;

            if (DriveOnRightSideOfTheRoad == null)
            {
                // No need to override settings,
                // determine curb approach automatically.
                if (_IsRuleToDriveOnRightSideOfTheRoad())
                    policy = CurbApproach.Right;
                else
                    policy = CurbApproach.Left;
            }
            else if (DriveOnRightSideOfTheRoad == true)
            {
                policy = CurbApproach.Right;
            }
            else if (DriveOnRightSideOfTheRoad == false)
            {
                policy = CurbApproach.Left;
            }
            else
            {
                Debug.Assert(false);
            }

            return policy;
        }

        /// <summary>
        /// Method determines if current region country has right side
        /// driving rules or left side.
        /// </summary>
        /// <returns>True - if right side driving, otherwise False.</returns>
        private bool _IsRuleToDriveOnRightSideOfTheRoad()
        {
            int majorVersion = System.Environment.OSVersion.Version.Major;
            int minorVersion = System.Environment.OSVersion.Version.Minor;

            bool result = true;

            // Only for WindowsXP or later...
            if ((majorVersion == WINDOWS_XP_MAJOR_VERSION &&
                 minorVersion >= WINDOWS_XP_MINOR_VERSION) ||
                 (majorVersion > WINDOWS_XP_MAJOR_VERSION))
            {
                // Get current Geo Id from region information.
                RegionInfo ri = new RegionInfo(
                    System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName);
                int geoId = ri.GeoId;

                switch (geoId)
                {
                    // Check countries which have left side driving rules.
                    case 0xA:      // American Samoa
                    case 0x12C:    // Anguilla
                    case 0x2:      // Antigua and Barbuda
                    case 0xC:      // Australia
                    case 0x16:     // Bahamas, The
                    case 0x17:     // Bangladesh
                    case 0x12:     // Barbados
                    case 0x14:     // Bermuda
                    case 0x22:     // Bhutan
                    case 0x13:     // Botswana
                    case 0x25:     // Brunei
                    case 0x133:    // Cayman Islands
                    case 0x135:    // Christmas Island
                    case 0x137:    // Cocos (Keeling) Islands
                    case 0x138:    // Cook Islands
                    case 0x3B:     // Cyprus
                    case 0x3F:     // Dominica
                    case 0x6F60E7: // Timor-Leste
                    case 0x13B:    // Falkland Islands (Islas Malvinas)
                    case 0x4E:     // Fiji Islands
                    case 0x5B:     // Grenada
                    case 0x144:    // Guernsey
                    case 0x65:     // Guyana
                    case 0x68:     // Hong Kong S.A.R.
                    case 0x71:     // India
                    case 0x6F:     // Indonesia
                    case 0x44:     // Ireland
                    case 0x3B16:   // Isle of Man
                    case 0x7C:     // Jamaica
                    case 0x7A:     // Japan
                    case 0x148:    // Jersey
                    case 0x81:     // Kenya
                    case 0x85:     // Kiribati
                    case 0x92:     // Lesotho
                    case 0x97:     // Macao S.A.R.
                    case 0x9C:     // Malawi
                    case 0xA7:     // Malaysia
                    case 0xA5:     // Maldives
                    case 0xA3:     // Malta
                    case 0xA0:     // Mauritius
                    case 0x14C:    // Montserrat
                    case 0xA8:     // Mozambique
                    case 0xFE:     // Namibia
                    case 0xB4:     // Nauru
                    case 0xB2:     // Nepal
                    case 0xB7:     // New Zealand
                    case 0x14F:    // Niue
                    case 0x150:    // Norfolk Island
                    case 0xBE:     // Pakistan
                    case 0xC2:     // Papua New Guinea
                    case 0x153:    // Pitcairn Islands
                    case 0x157:    // St. Helena
                    case 0xCF:     // St. Kitts and Nevis
                    case 0xDA:     // St. Lucia
                    case 0xF8:     // St. Vincent and the Grenadines
                    case 0xD0:     // Seychelles
                    case 0xD7:     // Singapore
                    case 0x1E:     // Solomon Islands
                    case 0xD1:     // South Africa
                    case 0x2A:     // Sri Lanka
                    case 0xB5:     // Suriname
                    case 0x104:    // Swaziland
                    case 0xEF:     // Tanzania
                    case 0xE3:     // Thailand
                    case 0x15B:    // Tokelau
                    case 0xE7:     // Tonga
                    case 0xE1:     // Trinidad and Tobago 
                    case 0x15D:    // Turks and Caicos Islands
                    case 0xEC:     // Tuvalu
                    case 0xF0:     // Uganda
                    case 0xF2:     // United Kingdom
                    case 0x15F:    // Virgin Islands, British
                    case 0xFC:     // Virgin Islands
                    case 0x107:    // Zambia
                    case 0x108:    // Zimbabwe
                        {
                            result = false;
                        }
                        break;
                    default:
                        // All other countries have
                        // right side driving rules.
                        result = true;
                        break;
                }

            }
            else
            {
                // Can't determine region information
                // for Windows before Windows XP.
                return result;
            }

            return result;
        }

        #endregion private methods

        #region private constants

        /// <summary>
        /// Windows XP major version. It is used to determine if
        /// we can get Region Information.
        /// </summary>
        private const int WINDOWS_XP_MAJOR_VERSION = 5;

        /// <summary>
        /// Windows XP minor version. It is used to determine if
        /// we can get Region Information.
        /// </summary>
        private const int WINDOWS_XP_MINOR_VERSION = 1;

        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // locale for conversions
        private static readonly CultureInfo fmtProvider = new CultureInfo("en-US");

        private List<Restriction> _restrictions = new List<Restriction>();
        private SolveInfoWrap _settings;
        private NetworkDescription _netDesc;

        #endregion private fields
    }
}
