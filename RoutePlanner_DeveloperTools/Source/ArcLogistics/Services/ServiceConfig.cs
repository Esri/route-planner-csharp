using System.ComponentModel;
using System.Xml.Serialization;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Services.Serialization
{
    /// <summary>
    /// ServicesInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [XmlRoot("services")]
    public class ServicesInfo
    {
        [XmlElement("map")]
        public MapInfo MapInfo { get; set; }

        [XmlElement("geocoding")]
        public GeocodingInfo GeocodingInfo { get; set; }

        [XmlElement("solve")]
        public SolveInfo SolveInfo { get; set; }

        [XmlElement("tracking")]
        public TrackingInfo TrackingInfo { get; set; }

        [XmlElement("servers")]
        public ServersInfo ServersInfo { get; set; }
    }

    #region map configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// MapInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MapInfo
    {
        [XmlElement("service")]
        public MapServiceInfo[] Services { get; set; }

        [XmlElement("startup-extent")]
        public Envelope StartupExtent { get; set; }

        [XmlElement("import-check-extent")]
        public Envelope ImportCheckExtent { get; set; }
    }

    /// <summary>
    /// MapServiceInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MapServiceInfo
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("server")]
        public string ServerName { get; set; }

        [XmlAttribute("basemap")]
        public bool IsBaseMap { get; set; }

        [XmlAttribute("visible")]
        public bool IsVisible { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("soapurl")]
        public string Url { get; set; }

        [XmlElement("opacity")]
        public double Opacity { get; set; }
    }

    #endregion map configuration

    #region routing configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// SolveInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SolveInfo
    {
        [XmlElement("vrp")]
        public VrpInfo VrpInfo { get; set; }

        [XmlElement("syncvrp")]
        public VrpInfo SyncVrpInfo { get; set; }

        [XmlElement("routing")]
        public RoutingInfo RoutingInfo { get; set; }

        /// <summary>
        /// Discovery service settings.
        /// </summary>
        [XmlElement("discovery")]
        public DiscoveryInfo DiscoveryInfo { get; set; }

        [XmlElement("solversettings")]
        public SolverSettingsInfo SolverSettingsInfo { get; set; }
    }

    /// <summary>
    /// RoutingInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RoutingInfo
    {
        [XmlElement("service")]
        public RouteServiceInfo[] ServiceInfo { get; set; }
    }

    /// <summary>
    /// VrpInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class VrpInfo
    {
        [XmlElement("service")]
        public VrpServiceInfo[] ServiceInfo { get; set; }
    }

    /// <summary>
    /// Provides information about service,
    /// which allow to get network coverage information.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DiscoveryInfo
    {
        /// <summary>
        /// Discovery services configuration information.
        /// </summary>
        [XmlElement("service")]
        public DiscoveryServiceInfo[] ServiceInfo { get; set; }
    }

    /// <summary>
    /// RestrictionInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RestrictionInfo
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("turnedon")]
        public bool IsTurnedOn { get; set; }

        [XmlAttribute("editable")]
        public bool IsEditable { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }
    }

    /// <summary>
    /// RestrictionsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RestrictionsInfo
    {
        [XmlElement("restriction")]
        public RestrictionInfo[] Restrictions { get; set; }
    }

    /// <summary>
    /// RouteAttrInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RouteAttrInfo
    {
        [XmlAttribute("attrname")]
        public string AttrName { get; set; }

        [XmlAttribute("paramname")]
        public string ParamName { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Discovery service information in configuration files.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DiscoveryServiceInfo
    {
        /// <summary>
        /// Server name used for discovery service.
        /// </summary>
        [XmlAttribute("server")]
        public string ServerName { get; set; }

        /// <summary>
        /// Discovery service title.
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// REST URL for making requests.
        /// </summary>
        [XmlElement("resturl")]
        public string RestUrl { get; set; }
    }

    /// <summary>
    /// RouteAttrsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RouteAttrsInfo
    {
        [XmlElement("param")]
        public RouteAttrInfo[] AttributeParams { get; set; }
    }

    /// <summary>
    /// SolverSettingsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SolverSettingsInfo
    {
        /// <summary>
        /// The maximum number of orders which should be processed synchronously.
        /// </summary>
        [XmlElement("maxordercountforsyncvrp")]
        public int MaxSyncVrpRequestOrderCount { get; set; }

        /// <summary>
        /// The maximum number of routes which should be processed synchronously.
        /// </summary>
        [XmlElement("maxroutecountforsyncvrp")]
        public int MaxSyncVrpRequestRouteCount { get; set; }

        [XmlElement("usesyncronousvrp")]
        public bool UseSyncronousVrp { get; set; }

        [XmlElement("uturnatintersections")]
        public bool UTurnAtIntersections { get; set; }

        [XmlElement("uturnatdeadends")]
        public bool UTurnAtDeadEnds { get; set; }

        [XmlElement("uturnatstops")]
        public bool UTurnAtStops { get; set; }

        [XmlElement("stoponorderside")]
        public bool StopOnOrderSide { get; set; }

        [XmlElement("driveonrightsideoftheroad")]
        public string DriveOnRightSideOfTheRoad { get; set; }

        [XmlElement("usedynamicpoints")]
        public bool UseDynamicPoints { get; set; }

        [XmlElement("twpreference")]
        public string TWPreference { get; set; }

        [XmlElement("savenalayer")]
        public bool SaveOutputLayer { get; set; }

        [XmlElement("excluderestrictedstreets")]
        public bool ExcludeRestrictedStreets { get; set; }

        [XmlElement("arrivedepartdelay")]
        public int ArriveDepartDelay { get; set; }

        [XmlElement("restrictions")]
        public RestrictionsInfo RestrictionsInfo { get; set; }

        [XmlElement("attributeparams")]
        public RouteAttrsInfo AttributeParamsInfo { get; set; }
    }

    /// <summary>
    /// VrpServiceInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class VrpServiceInfo
    {
        [XmlAttribute("server")]
        public string ServerName { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("resturl")]
        public string RestUrl { get; set; }

        [XmlElement("soapurl")]
        public string SoapUrl { get; set; }

        [XmlElement("toolname")]
        public string ToolName { get; set; }
    }

    /// <summary>
    /// RouteServiceInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RouteServiceInfo
    {
        [XmlAttribute("server")]
        public string ServerName { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("soapurl")]
        public string SoapUrl { get; set; }

        [XmlElement("resturl")]
        public string RestUrl { get; set; }

        [XmlElement("layername")]
        public string LayerName { get; set; }
    }

    #endregion routing configuration

    #region tracking configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// TrackingInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TrackingInfo
    {
        /// <summary>
        /// Flag, showing is tracking service enabled.
        /// </summary>
        [XmlAttribute("enable")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Tracking service info.
        /// </summary>
        [XmlElement("trackingservice")]
        public TrackingServiceInfo TrackingServiceInfo { get; set; }

        /// <summary>
        /// Tracking settings.
        /// </summary>
        [XmlElement("trackingsettings")]
        public TrackingSettings TrackingSettings { get; set; }
    }

    /// <summary>
    /// TrackingServiceInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TrackingServiceInfo
    {
        /// <summary>
        /// Gets or sets feature services server name.
        /// </summary>
        [XmlAttribute("server")]
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets URL to the REST API end point for tracking service.
        /// </summary>
        [XmlElement("resturl")]
        public string RestUrl { get; set; }
    }

    /// <summary>
    /// Contains global tracking service settings.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TrackingSettings
    {
        /// <summary>
        /// Gets or sets break tolerance in minutes.
        /// </summary>
        [XmlElement("BreakTolerance")]
        public int? BreakTolerance { get; set; }
    }

    #endregion tracking configuration

    #region geocoding configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// GeocodingInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GeocodingInfo
    {
        [XmlElement("service")]
        public GeocodingServiceInfo[] GeocodingServiceInfo
        {
            get { return _serviceInfo; }
            set { _serviceInfo = value; }
        }

        GeocodingServiceInfo[] _serviceInfo;
    }

    /// <summary>
    /// GeocodingServiceInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GeocodingServiceInfo
    {
        [XmlAttribute("type")]
        public string type;

        [XmlAttribute("server")]
        public string serverName;

        [XmlAttribute("current")]
        public bool current;

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("soapurl")]
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// Gets or sets the URL to the REST service endpoint.
        /// </summary>
        [XmlElement("resturl")]
        public string RestUrl
        {
            get;
            set;
        }

        [XmlElement("inputfieldmapping")]
        public InputFieldMappings FieldMappings
        {
            get { return _fieldsMappings; }
            set { _fieldsMappings = value; }
        }

        /// <summary>
        /// Collection of exact locators.
        /// </summary>
        [XmlElement("exactaddresseslocator")]
        public ExactLocators ExactLocators
        {
            get;
            set;
        }
        
        [XmlElement("usesinglelineinput")]
        public bool UseSingleLineInput;

        [XmlElement("minimumcandidatescore")]
        public long MinimumCandidateScore
        {
            get { return _minimumCandidateScore; }
            set { _minimumCandidateScore = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating the minimum score value for an address candidate
        /// retrieved via geocoding to be treated as matched.
        /// </summary>
        [XmlElement("minimummatchscore")]
        public int? MinimumMatchScore
        {
            get;
            set;
        }

        [XmlElement("internallocators")]
        public SublocatorsInfo InternalLocators
        {
            get { return _sublocatorsInfo; }
            set { _sublocatorsInfo = value; }
        }
                
        public bool IsCompositeLocator
        {
            get { return _sublocatorsInfo != null ? true : false; }
        }

        private string _url;
        private long _minimumCandidateScore;
        private InputFieldMappings _fieldsMappings;
        private SublocatorsInfo _sublocatorsInfo;
    }

    /// <summary>
    /// SublocatorsInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SublocatorsInfo
    {
        [XmlElement("locator")]
        public SublocatorInfo[] SublocatorInfo
        {
            get { return _sublocatorInfo; }
            set { _sublocatorInfo = value; }
        }

        private SublocatorInfo[] _sublocatorInfo;
    }

    /// <summary>
    /// Sublocator class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SublocatorInfo
    {
        [XmlAttribute("name")]
        public string name;

        [XmlAttribute("title")]
        public string title;

        [XmlAttribute("primary")]
        public bool primary;

        [XmlAttribute("enable")]
        public bool enable;

        /// <summary>
        /// Gets or sets the type of the sub-locator.
        /// </summary>
        [XmlAttribute("type")]
        public string Type
        {
            get;
            set;
        }

        [XmlElement("inputfieldmapping")]
        public InternalFieldMappings FieldMappings
        {
            get { return _fieldsMappings; }
            set { _fieldsMappings = value; }
        }

        private InternalFieldMappings _fieldsMappings;
    }

    /// <summary>
    /// InputFieldMappings class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class InputFieldMappings
    {
        [XmlElement("fieldmap")]
        public InputFieldMapping[] FieldMapping
        {
            get { return _fieldMapping; }
            set { _fieldMapping = value; }
        }

        private InputFieldMapping[] _fieldMapping;
    }

    /// <summary>
    /// ExactLocators class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ExactLocators
    {
        [XmlElement("Locator")]
        public ExactLocator[] Locators { get; set; }
    }
    
    /// <summary>
    /// Exact locator.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ExactLocator
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// InputFieldMapping class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class InputFieldMapping
    {
        [XmlAttribute("addressfield")]
        public string AddressField
        {
            get { return _addressField; }
            set { _addressField = value; }
        }

        [XmlAttribute("locatorfield")]
        public string LocatorField
        {
            get { return _locatorField; }
            set { _locatorField = value; }
        }

        [XmlAttribute("visible")]
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        [XmlAttribute("description")]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private string _addressField;
        private string _locatorField;
        private string _description;
        private bool _visible;
    }

    /// <summary>
    /// InputFieldMappings class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class InternalFieldMappings
    {
        [XmlElement("fieldmap")]
        public InternalFieldMapping[] FieldMapping
        {
            get { return _fieldMapping; }
            set { _fieldMapping = value; }
        }

        private InternalFieldMapping[] _fieldMapping;
    }

    /// <summary>
    /// InternalFieldMapping class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class InternalFieldMapping
    {
        [XmlAttribute("addressfield")]
        public string AddressField
        {
            get { return _addressField; }
            set { _addressField = value; }
        }

        [XmlAttribute("locatorfield")]
        public string LocatorField
        {
            get { return _locatorField; }
            set { _locatorField = value; }
        }

        private string _addressField;
        private string _locatorField;
    }

    #endregion geocoding configuration

    #region servers configuration
    ///////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// ServersInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ServersInfo
    {
        [XmlElement("server")]
        public ServerInfo[] Servers { get; set; }
    }

    /// <summary>
    /// ServerInfo class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ServerInfo
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("description")]
        public string Description { get; set; }

        [XmlAttribute("help")]
        public string HelpPrompt { get; set; }

        [XmlAttribute("authentication")]
        public string Authentication { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }

        /// <summary>
        /// Token type.
        /// </summary>
        [XmlAttribute("tokentype")]
        public TokenType TokenType { get; set; }
    }

    #endregion servers configuration
}
