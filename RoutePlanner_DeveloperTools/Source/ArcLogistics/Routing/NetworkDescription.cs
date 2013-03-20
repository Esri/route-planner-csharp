using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using ESRI.ArcLogistics.NAService;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// NetworkAttributeUsageType enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum NetworkAttributeUsageType
    {
        /// <summary>
        /// Cost.
        /// </summary>
        Cost,

        /// <summary>
        /// Descriptor.
        /// </summary>
        Descriptor,

        /// <summary>
        /// Hierarchy.
        /// </summary>
        Hierarchy,

        /// <summary>
        /// Restriction.
        /// </summary>
        Restriction
    }

    /// <summary>
    /// Network Attribute units enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum NetworkAttributeUnits
    {
        /// <summary>
        /// Unknown unit.
        /// </summary>
        Unknown,

        /// <summary>
        /// Inches unit.
        /// </summary>
        Inches,

        /// <summary>
        /// Points unit.
        /// </summary>
        Points,

        /// <summary>
        /// Feet unit.
        /// </summary>
        Feet,

        /// <summary>
        /// Yards unit.
        /// </summary>
        Yards,

        /// <summary>
        /// Miles unit.
        /// </summary>
        Miles,

        /// <summary>
        /// Nautical miles unit.
        /// </summary>
        NauticalMiles,

        /// <summary>
        /// Millimeters unit.
        /// </summary>
        Millimeters,

        /// <summary>
        /// Centimeters unit.
        /// </summary>
        Centimeters,

        /// <summary>
        /// Meters unit.
        /// </summary>
        Meters,

        /// <summary>
        /// Kilometers unit.
        /// </summary>
        Kilometers,

        /// <summary>
        /// Decimal degrees unit.
        /// </summary>
        DecimalDegrees,

        /// <summary>
        /// Decimeters unit.
        /// </summary>
        Decimeters,

        /// <summary>
        /// Seconds unit.
        /// </summary>
        Seconds,

        /// <summary>
        /// Minutes unit.
        /// </summary>
        Minutes,

        /// <summary>
        /// Hours unit.
        /// </summary>
        Hours,

        /// <summary>
        /// Days unit.
        /// </summary>
        Days
    }

    /// <summary>
    /// NetworkAttributeParameter class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NetworkAttributeParameter
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="routingParameterName">Routing parameter name.</param>
        /// <param name="value">Value.</param>
        /// <param name="type">Type.</param>
        public NetworkAttributeParameter(string name, string routingParameterName, object value,
            Type type)
        {
            Debug.Assert(name != null);
            Debug.Assert(routingParameterName != null);
            Debug.Assert(type != null);

            _name = name;
            this.RoutingName = routingParameterName;
            _value = value;
            _type = type;
        }

        #endregion

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets parameter name to be used by routing service.
        /// </summary>
        public string RoutingName
        {
            get;
            private set;
        }

        /// <summary>
        /// Default value.
        /// </summary>
        public object DefaultValue
        {
            get { return _value; }
        }

        /// <summary>
        /// Type.
        /// </summary>
        public Type Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Check that empty string is valid for current parameter type.
        /// </summary>
        /// <returns>'True' if parameter is string, double or int and 'false' otherwise.</returns>
        public bool IsEmptyStringValid
        {
            get
            {
                return Type == typeof(string) || Type == typeof(double) || Type == typeof(int);
            }
        }

        #endregion

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name;
        private object _value;
        private Type _type;

        #endregion
    }

    /// <summary>
    /// NetworkAttribute class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NetworkAttribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="vrpAttributeName">VRP attribute name.</param>
        /// <param name="routingAttributeName">Routing attribute name.</param>
        /// <param name="unit">Network attribute unit.</param>
        /// <param name="usageType">Network attribute usage type.</param>
        /// <param name="parameters">Collection of network attribute parameters.</param>
        public NetworkAttribute(string vrpAttributeName, string routingAttributeName,
            NetworkAttributeUnits unit, NetworkAttributeUsageType usageType,
            ICollection<NetworkAttributeParameter> parameters) : this(vrpAttributeName, 
            routingAttributeName, unit, usageType, parameters, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="vrpAttributeName">VRP attribute name.</param>
        /// <param name="routingAttributeName">Routing attribute name.</param>
        /// <param name="unit">Network attribute unit.</param>
        /// <param name="usageType">Network attribute usage type.</param>
        /// <param name="parameters">Collection of network attribute parameters.</param>
        public NetworkAttribute(string vrpAttributeName, string routingAttributeName,
            NetworkAttributeUnits unit, NetworkAttributeUsageType usageType,
            ICollection<NetworkAttributeParameter> parameters,
            NetworkAttributeParameter usageParameter)
        {
            _name = vrpAttributeName;
            this.RoutingName = routingAttributeName;
            _unit = unit;
            _usageType = usageType;
            _parameters = parameters;
            _restrictionUsageParameter = usageParameter;
        }

        #endregion

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Network attribute name property.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets attribute name to be used by routing service.
        /// </summary>
        public string RoutingName
        {
            get;
            private set;
        }

        /// <summary>
        /// Network attribute usage type property.
        /// </summary>
        public NetworkAttributeUsageType UsageType
        {
            get { return _usageType; }
        }

        /// <summary>
        /// Network attribute units property.
        /// </summary>
        public NetworkAttributeUnits Unit
        {
            get { return _unit; }
        }

        /// <summary>
        /// Network attribute parameters collection property.
        /// </summary>
        public ICollection<NetworkAttributeParameter> Parameters
        {
            get { return _parameters; }
        }

        /// <summary>
        /// Restriction usage parameter.
        /// If attribute isnt restriction - always null.
        /// </summary>
        public NetworkAttributeParameter RestrictionUsageParameter
        {
            get { return _restrictionUsageParameter; }
        }

        #endregion

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Network attribute name.
        /// </summary>
        private string _name;

        /// <summary>
        /// Network attribute usage type.
        /// </summary>
        private NetworkAttributeUsageType _usageType;

        /// <summary>
        /// Network attribute units.
        /// </summary>
        private NetworkAttributeUnits _unit;

        /// <summary>
        /// Collection of network attribute parameters.
        /// </summary>
        private ICollection<NetworkAttributeParameter> _parameters;

        /// <summary>
        /// Network attribute restriction usage parameter.
        /// </summary>
        private NetworkAttributeParameter _restrictionUsageParameter;

        #endregion
    }

    /// <summary>
    /// NetworkDescription class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class NetworkDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceUrl">Service url.</param>
        /// <param name="layerName">Layer name.</param>
        /// <param name="server">Server.</param>
        internal NetworkDescription(string serviceUrl, string layerName,
            AgsServer server)
        {
            Debug.Assert(serviceUrl != null);

            // Create connection.
            var connection = server.OpenConnection();
            NAServiceClient client = new NAServiceClient(serviceUrl, connection);

            try
            {
                NAServerNetworkDescription desc = client.GetNetworkDescription(layerName);

                NAServerSolverParams solverParams = client.GetSolverParameters(layerName);

                var parameterValues = solverParams.AttributeParameterValues.ToLookup(
                    value => value.AttributeName, StringComparer.OrdinalIgnoreCase);

                // Create attributes.
                foreach (NAServerNetworkAttribute attr in desc.NetworkAttributes)
                {
                    var routingAttributeName = attr.Name;
                    var attributeParameter = parameterValues[attr.Name].FirstOrDefault();
                    if (attributeParameter != null)
                    {
                        routingAttributeName = attributeParameter.AttributeName;
                    }

                    var usageType = _ConvertUsageType(attr.UsageType);

                    var usageParameter =
                        _GetRestrictionUsageParameter(attr.RestrictionUsageParameterName,
                        parameterValues[attr.Name], usageType);

                    var attribute = new NetworkAttribute(attr.Name, routingAttributeName,
                        _ConvertUnits(attr.Units), usageType, 
                        _GetAttrParams(attr, parameterValues[attr.Name]), usageParameter);

                    _attributes.Add(attribute);
                }

                // Enabled restriction names.
                _enabledRestrictionNames = solverParams.RestrictionAttributeNames;
                if (_enabledRestrictionNames == null)
                    _enabledRestrictionNames = new string[0];

                // Get impedance attribute name.
                _impedanceAttrName = solverParams.ImpedanceAttributeName;
            }
            finally
            {
                client.Close();
            }
        }

        /// <summary>
        /// Get restriction usage parameter.
        /// </summary>
        /// <param name="name">Name of the restriction usage parameter. 
        // If null - default name will be used.</param>
        /// <param name="attributeParameters">Collection of attribute parameters.</param>
        /// <param name="usageType">Attribute usage type.</param>
        /// <returns></returns>
        private NetworkAttributeParameter _GetRestrictionUsageParameter(string name,
            IEnumerable<NAAttributeParameterValue> attributeParameters, NetworkAttributeUsageType usageType)
        {
            if (usageType == NetworkAttributeUsageType.Restriction)
            {
                var restrictionUsageParameterName = name ?? RESTRICTION_USAGE_PARAMETER_DEFAULT_NAME;
                var parameter =  attributeParameters.FirstOrDefault
                    (x => x.ParameterName == restrictionUsageParameterName);

                if (parameter != null)
                    return _CreateNetworkAttributeParameter(parameter);
            }
            
            return null;
        }

        #endregion

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Network attributes.
        /// </summary>
        public ICollection<NetworkAttribute> NetworkAttributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// Impedance attribute name.
        /// </summary>
        public string ImpedanceAttributeName
        {
            get { return _impedanceAttrName; }
        }

        /// <summary>
        /// Enabled restriction names.
        /// </summary>
        public string[] EnabledRestrictionNames
        {
            get { return _enabledRestrictionNames; }
        }

        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method gets attribute parameters collection.
        /// </summary>
        /// <param name="attr">Server network attribute to find intersection with 
        /// attribute parameters.</param>
        /// <param name="values">Collection of attribute parameters.</param>
        /// <returns>Collection of network attribute parameters.</returns>
        private static ICollection<NetworkAttributeParameter> _GetAttrParams(
            NAServerNetworkAttribute attr, IEnumerable<NAAttributeParameterValue> values)
        {
            var attributeParameters = values.ToDictionary(value => value.ParameterName,
                StringComparer.OrdinalIgnoreCase);
            var parameterNames = attr.ParameterNames.Intersect(attributeParameters.Keys,
                StringComparer.OrdinalIgnoreCase);
            List<NetworkAttributeParameter> attrParams = new List<NetworkAttributeParameter>();

            foreach (var parameterName in parameterNames)
            {
                var value = attributeParameters[parameterName];
                attrParams.Add(_CreateNetworkAttributeParameter(value));
            }

            return attrParams.AsReadOnly();
        }

        /// <summary>
        /// Convert NAAttributeParameterValue to NetworkAttributeParameter.
        /// </summary>
        /// <param name="value">NAAttributeParameterValue.</param>
        /// <returns>NetworkAttributeParameter</returns>
        private static NetworkAttributeParameter _CreateNetworkAttributeParameter
            (NAAttributeParameterValue value)
        {
            // Determine parameter value type.
            Type type;
            if (value.Value != null)
            {
                type = value.Value.GetType();
            }
            else
            {
                // Determine type by vartype.
                // NOTE: use string type for unknown/unsupported vartypes.
                if (!_ConvertParamType(value.VarType, out type))
                    type = typeof(string);
            }
            return new NetworkAttributeParameter(value.ParameterName,
                value.ParameterName, value.Value, type);
        }


        /// <summary>
        /// Method converts ESRI network attribute usage type into application 
        /// network attributes usage type.
        /// </summary>
        /// <param name="naType">ESRI network attribute usage type.</param>
        /// <returns>Converted network attribute usage type.</returns>
        private static NetworkAttributeUsageType _ConvertUsageType(
            esriNetworkAttributeUsageType naType)
        {
            NetworkAttributeUsageType type;
            switch (naType)
            {
                case esriNetworkAttributeUsageType.esriNAUTCost:
                    type = NetworkAttributeUsageType.Cost;
                    break;
                case esriNetworkAttributeUsageType.esriNAUTDescriptor:
                    type = NetworkAttributeUsageType.Descriptor;
                    break;
                case esriNetworkAttributeUsageType.esriNAUTHierarchy:
                    type = NetworkAttributeUsageType.Hierarchy;
                    break;
                case esriNetworkAttributeUsageType.esriNAUTRestriction:
                    type = NetworkAttributeUsageType.Restriction;
                    break;
                default:
                    throw new RouteException(Properties.Messages.Error_UnsupportedAttrUsageType);
            }

            return type;
        }

        /// <summary>
        /// Method converts ESRI network attribute units into application network attributes units.
        /// </summary>
        /// <param name="naUnits">ESRI Network attribute units to convert.</param>
        /// <returns>Converted network attribute units.</returns>
        private static NetworkAttributeUnits _ConvertUnits(esriNetworkAttributeUnits naUnits)
        {
            NetworkAttributeUnits unit;

            switch (naUnits)
            {
                case esriNetworkAttributeUnits.esriNAUCentimeters:
                    unit = NetworkAttributeUnits.Centimeters;
                    break;
                case esriNetworkAttributeUnits.esriNAUDays:
                    unit = NetworkAttributeUnits.Days;
                    break;
                case esriNetworkAttributeUnits.esriNAUDecimalDegrees:
                    unit = NetworkAttributeUnits.DecimalDegrees;
                    break;
                case esriNetworkAttributeUnits.esriNAUDecimeters:
                    unit = NetworkAttributeUnits.Decimeters;
                    break;
                case esriNetworkAttributeUnits.esriNAUFeet:
                    unit = NetworkAttributeUnits.Feet;
                    break;
                case esriNetworkAttributeUnits.esriNAUHours:
                    unit = NetworkAttributeUnits.Hours;
                    break;
                case esriNetworkAttributeUnits.esriNAUInches:
                    unit = NetworkAttributeUnits.Inches;
                    break;
                case esriNetworkAttributeUnits.esriNAUKilometers:
                    unit = NetworkAttributeUnits.Kilometers;
                    break;
                case esriNetworkAttributeUnits.esriNAUMeters:
                    unit = NetworkAttributeUnits.Meters;
                    break;
                case esriNetworkAttributeUnits.esriNAUMiles:
                    unit = NetworkAttributeUnits.Miles;
                    break;
                case esriNetworkAttributeUnits.esriNAUMillimeters:
                    unit = NetworkAttributeUnits.Millimeters;
                    break;
                case esriNetworkAttributeUnits.esriNAUMinutes:
                    unit = NetworkAttributeUnits.Minutes;
                    break;
                case esriNetworkAttributeUnits.esriNAUNauticalMiles:
                    unit = NetworkAttributeUnits.NauticalMiles;
                    break;
                case esriNetworkAttributeUnits.esriNAUPoints:
                    unit = NetworkAttributeUnits.Points;
                    break;
                case esriNetworkAttributeUnits.esriNAUSeconds:
                    unit = NetworkAttributeUnits.Seconds;
                    break;
                case esriNetworkAttributeUnits.esriNAUUnknown:
                    unit = NetworkAttributeUnits.Unknown;
                    break;
                case esriNetworkAttributeUnits.esriNAUYards:
                    unit = NetworkAttributeUnits.Yards;
                    break;
                default:
                    throw new RouteException(Properties.Messages.Error_UnsupportedAttrUnits);
            }

            return unit;
        }

        /// <summary>
        /// Method tries to convert parameter type into CLR types.
        /// </summary>
        /// <param name="varType">Type to convert.</param>
        /// <param name="type">Result type as output parameter.</param>
        /// <returns>True if succesfully converter, otherwise false.</returns>
        private static bool _ConvertParamType(long varType, out Type type)
        {
            type = null;
            bool isConverted = true;

            VarEnum vt = (VarEnum)varType;
            switch (vt)
            {
                case VarEnum.VT_I2:
                    type = typeof(short);
                    break;
                case VarEnum.VT_I4:
                    type = typeof(int);
                    break;
                case VarEnum.VT_R4:
                    type = typeof(float);
                    break;
                case VarEnum.VT_R8:
                    type = typeof(double);
                    break;
                case VarEnum.VT_DATE:
                    type = typeof(DateTime);
                    break;
                case VarEnum.VT_BSTR:
                    type = typeof(string);
                    break;
                case VarEnum.VT_BOOL:
                    type = typeof(bool);
                    break;
                case VarEnum.VT_I1:
                    type = typeof(char);
                    break;
                case VarEnum.VT_UI1:
                    type = typeof(byte);
                    break;
                case VarEnum.VT_UI2:
                    type = typeof(ushort);
                    break;
                case VarEnum.VT_UI4:
                    type = typeof(uint);
                    break;
                case VarEnum.VT_I8:
                    type = typeof(long);
                    break;
                case VarEnum.VT_UI8:
                    type = typeof(ulong);
                    break;
                default:
                    isConverted = false;
                    break;
            }

            return isConverted;
        }

        #endregion

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Collection of network attributes.
        /// </summary>
        private List<NetworkAttribute> _attributes = new List<NetworkAttribute>();

        /// <summary>
        /// Collection of enabled restriction names.
        /// </summary>
        private string[] _enabledRestrictionNames;

        /// <summary>
        /// Impendance attribute name.
        /// </summary>
        private string _impedanceAttrName;

        /// <summary>
        /// Default name of restriction usage parameter.
        /// </summary>
        private string RESTRICTION_USAGE_PARAMETER_DEFAULT_NAME = @"RestrictionUsage";

        #endregion
    }
}
