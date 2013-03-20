using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Services.Serialization;
using ESRI.ArcLogistics.Threading;
using ESRI.ArcLogistics.Tracking;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Class that represents catalog of available services.
    /// </summary>
    internal class ServiceCatalog
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the ServiceCatalog class.
        /// </summary>
        /// <param name="progressReporter">The reference to the progress reporter
        /// object.</param>
        /// <param name="configPath"></param>
        /// <param name="userConfigPath"></param>
        /// <param name="certificateValidationSettings">The reference to the certificate
        /// validation settings object.</param>
        /// <param name="exceptionsHandler">Exceptions handler.</param>
        /// <exception cref="T:System.ArgumentNullException">Any of <paramref name="configPath"/>,
        /// <paramref name="userConfigPath"/>, <paramref name="progressReporter"/> or
        /// <paramref name="certificateValidationSettings"/> parameters is a null reference.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Config file is invalid.
        /// In property "Source" there is path to invalid config file.</exception>
        public ServiceCatalog(
            IProgressReporter progressReporter,
            string configPath,
            string userConfigPath,
            ICertificateValidationSettings certificateValidationSettings,
            IServiceExceptionHandler exceptionsHandler)
        {
            Debug.Assert(exceptionsHandler != null);

            if (progressReporter == null)
            {
                throw new ArgumentNullException("progressReporter");
            }

            if (configPath == null)
            {
                throw new ArgumentNullException("configPath");
            }

            if (userConfigPath == null)
            {
                throw new ArgumentNullException("userConfigPath");
            }

            if (certificateValidationSettings == null)
            {
                throw new ArgumentNullException("certificateValidationSettings");
            }

            _exceptionHandler = exceptionsHandler;

            _certificateValidationSettings = certificateValidationSettings;

            // load configuration
            _servicesInfo = _LoadServices(configPath);
            _userServicesInfo = _LoadUserConfig(userConfigPath);

            // If we have loaded services info - init servers.
            if (_servicesInfo != null)
            {
                // validate configuration
                _Validate();

           		_solveInfo = _CreateSolveInfo();

                // init servers
                _InitServers();
            }


            progressReporter.Step();

            // If we have loaded services info - init services.
            if (_servicesInfo != null)
                // init services
                _InitServices();

            Licenser.LicenseActivated += new EventHandler(Licenser_LicenseActivated);
            _userConfigPath = userConfigPath;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns 'true' if services info exist, 'false' otherwise.
        /// </summary>
        public bool ServicesInfoExist
        {
            get
            {
                return _servicesInfo != null;
            }
        }

        /// <summary>
        /// Returns read-only collection of servers.
        /// </summary>
        public ICollection<AgsServer> Servers
        {
            get { return _servers.AsReadOnly(); }
        }

        public Map Map
        {
            get { return _map; }
        }

        public GeocoderBase Geocoder
        {
            get { return _geocoder; }
        }

        /// <summary>
        /// Gets reference to the streets geocoder object.
        /// </summary>
        public GeocoderBase StreetsGeocoder
        {
            get;
            private set;
        }

        public IVrpSolver Solver
        {
            get { return _solver; }
        }

        /// <summary>
        /// Gets reference to the tracker provider object.
        /// </summary>
        public TrackerProvider TrackerProvider
        {
            get;
            private set;
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void Save()
        {
            Debug.Assert(_userServicesInfo != null);

            XmlSerializer ser = new XmlSerializer(typeof(UserServicesInfo));
            using (TextWriter writer = new StreamWriter(_userConfigPath))
            {
                ser.Serialize(writer, _userServicesInfo);
            }
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _InitServices()
        {
            _taskRunner.Invoke(
                _CreateMap,
                _CreateGeocoder,
                _CreateSolver,
                _CreateTrackerProvider);
        }

        private void _CreateMap()
        {
            if (_userServicesInfo.MapInfo == null)
                _userServicesInfo.MapInfo = new UserMapInfo();

            MapInfoWrap wrap = new MapInfoWrap(_servicesInfo.MapInfo,
                _userServicesInfo.MapInfo);

            _map = new Map(wrap, _servers, _exceptionHandler);
        }

        private void _CreateSolver()
        {
            Debug.Assert(null != _servicesInfo); // NOTE: load first

            var serviceOptions = ServiceOptions.None;
            if (_servicesInfo.SolveInfo.SolverSettingsInfo.UseSyncronousVrp)
                serviceOptions |= ServiceOptions.UseSyncVrp;

            _solver = new VrpSolver(
                _solveInfo,
                _servers,
                _solveServiceValidator,
                serviceOptions);
        }

        /// <summary>
        /// Creates new tracker provider instance.
        /// </summary>
        private void _CreateTrackerProvider()
        {
            // If we have tracking settings and tracking is enabled - create tracker.
            if(_servicesInfo.TrackingInfo != null && _servicesInfo.TrackingInfo.Enabled)
                this.TrackerProvider = new TrackerProvider(_servicesInfo.TrackingInfo, _servers);
        }

        /// <summary>
        /// Creates new instance of the <see cref="SolveInfoWrap"/> class with
        /// current solve settings.
        /// </summary>
        /// <returns>Object holding current solve settings.</returns>
        private SolveInfoWrap _CreateSolveInfo()
        {
            if (_userServicesInfo.SolveInfo == null)
                _userServicesInfo.SolveInfo = new UserSolveInfo();

            SolveInfoWrap wrap = new SolveInfoWrap(
                _servicesInfo.SolveInfo,
                _userServicesInfo.SolveInfo);

            _solveServiceValidator.Validate(wrap.VrpService);
            _solveServiceValidator.Validate(wrap.RouteService);
            _solveServiceValidator.Validate(wrap.DiscoveryService);

            // We need to check sync VRP settings only if we have to use it.
            if (_servicesInfo.SolveInfo.SolverSettingsInfo.UseSyncronousVrp)
                _solveServiceValidator.Validate(wrap.SyncVrpService);

            return wrap;
        }

        private void _CreateGeocoder()
        {
            GeocodingServiceInfo currentGeocodingServiceInfo = null;
            for (int index = 0; index < _servicesInfo.GeocodingInfo.GeocodingServiceInfo.Length; index++)
            {
                GeocodingServiceInfo geocodingServiceInfo = _servicesInfo.GeocodingInfo.GeocodingServiceInfo[index];
                if (geocodingServiceInfo.current)
                {
                    if (currentGeocodingServiceInfo != null)
                    {
                        throw new ApplicationException(Properties.Resources.DefaultGeocodingInfoIsNotUnique);
                    }
                    currentGeocodingServiceInfo = geocodingServiceInfo;
                }

                var isStreetsGeocoder = string.Equals(
                    geocodingServiceInfo.type,
                    STREETS_GEOCODER_TYPE,
                    StringComparison.OrdinalIgnoreCase);
                if (isStreetsGeocoder && this.StreetsGeocoder == null)
                {
                    var streetsGeocoderServer = _GetCurrentGcServer(geocodingServiceInfo);
                    var streetsGeocoder = new Geocoder(geocodingServiceInfo,
                        streetsGeocoderServer, _exceptionHandler);
                    this.StreetsGeocoder = streetsGeocoder;
                }
            }

            // Detect that geocoder type is ArcGisGeocoder.
            var isArcGisGeocoder = string.Equals(
                currentGeocodingServiceInfo.type,
                ARCGIS_GEOCODER_TYPE,
                StringComparison.OrdinalIgnoreCase);

            var isWorldGeocoder = string.Equals(
                currentGeocodingServiceInfo.type,
                "WorldGeocoder",
                StringComparison.OrdinalIgnoreCase);

            // If it is arcgis geocoder - create it and use as default.
            if (isArcGisGeocoder)
            {
                AgsServer geocodeServer = _GetCurrentGcServer(currentGeocodingServiceInfo);
                _geocoder = new ArcGiscomGeocoder(currentGeocodingServiceInfo,
                    geocodeServer, _exceptionHandler);
                this.StreetsGeocoder = _geocoder;
            }
            else if (isWorldGeocoder)
            {
                var worldGeocoder = WorldGeocoder.CreateWorldGeocoder(
                    currentGeocodingServiceInfo, _exceptionHandler);
                _geocoder = worldGeocoder;
                this.StreetsGeocoder = worldGeocoder;
            }
            else
            {
                AgsServer geocodeServer = _GetCurrentGcServer(currentGeocodingServiceInfo);
                _geocoder = new Geocoder(currentGeocodingServiceInfo,
                    geocodeServer, _exceptionHandler);
            }

            if (this.StreetsGeocoder == null)
            {
                this.StreetsGeocoder = _geocoder;
            }
        }

        /// <summary>
        /// Load services info. First load url from local file and then load settings from this URL.
        /// </summary>
        /// <param name="configPath">Path to local settings file.</param>
        /// <returns>Services settings.</returns>
        /// <exception cref="ESRI.ArcLogistics.SettingsException">Thrown if local config file 
        /// has wrong format, config file URL is bad/unavailible or if config 
        /// file has wrong format.</exception>
        private ServicesInfo _LoadServices(string configPath)
        {
            try
            {
                // Get url where services configuration can be found.
                // There can be exception if config file has wrong format.
                var servicesURL = _Load<ServiceURL>(configPath).URL;

                try
                {
                    // Get configuration file content.
                    var response = WebHelper.SendRequest(servicesURL, string.Empty, new HttpRequestOptions());

                    // Parse configuration file content.
                    using (TextReader reader = new StringReader(response))
                    {
                        XmlSerializer ser = new XmlSerializer(typeof(ServicesInfo));
                        return (ServicesInfo)ser.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    // If file wasn't found or string isn't valid URL - return null as services info.
                    if (ex is WebException || ex is InvalidCastException)
                        return null;
                    //throw new SettingsException(Properties.Messages.Error_WrongConfigURL, ex, servicesURL);
                    // If config file cannot be parsed.
                    else if (ex is System.InvalidOperationException)
                        throw new SettingsException(Properties.Messages.Error_WrongConfigFormat, ex, servicesURL);
                    else throw;
                }
            }
            // If we hadn't found "Configuration file" element in services.xml - try load
            // XML as configuration file.
            catch (SettingsException ex)
            {
                return _Load<ServicesInfo>(configPath);
            }
        }

        /// <summary>
        /// Load config from file.
        /// </summary>
        /// <typeparam name="T">Type of config.</typeparam>
        /// <param name="configPath">Path to config file.</param>
        /// <returns>Config of type T.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Config file is invalid.
        /// In property "Source" there is path to invalid config file.</exception>
        private T _Load<T>(string configPath)
        {
            Debug.Assert(configPath != null);
            XmlSerializer ser = new XmlSerializer(typeof(T));
            
            // Try to read file.
            try
            {
                using (TextReader reader = new StreamReader(configPath))
                {
                    return (T)ser.Deserialize(reader);
                }
            }
            catch(InvalidOperationException e)
            {
                // If XML corrupted - wrap exception and save path to file.
                throw new SettingsException(Properties.Messages.Error_WrongConfigFormat, e, configPath);
            }
        }

        private void _Validate()
        {
            // check servers section
            if (_servicesInfo.ServersInfo == null)
                throw new ApplicationException(Properties.Messages.Error_GetServersConfigFailed);

            // check map section
            if (_servicesInfo.MapInfo == null)
                throw new ApplicationException(Properties.Messages.Error_GetMapServicesConfigFailed);

            // check geocoding section
            if (_servicesInfo.GeocodingInfo == null)
                throw new ApplicationException(Properties.Messages.Error_GetGeocodeServicesConfigFailed);

            // check routing section
            if (_servicesInfo.SolveInfo == null)
                throw new ApplicationException(Properties.Messages.Error_GetRouteServicesConfigFailed);
        }

        private void _InitServers()
        {
            if (_userServicesInfo.ServersInfo == null)
                _userServicesInfo.ServersInfo = new UserServersInfo();

            ServersInfoWrap wrap = new ServersInfoWrap(
                _servicesInfo.ServersInfo,
                _userServicesInfo.ServersInfo);

            var servers = wrap.Servers.ToArray();
            _servers = servers.Select(_ => default(AgsServer)).ToList();
            _taskRunner.For(0, servers.Length, index =>
            {
                var serverInfo = servers[index];

                var server = new AgsServer(
                    serverInfo,
                    Licenser.LicenseAccount);

                _servers[index] = server;
            });
        }

        private AgsServer _GetCurrentGcServer(GeocodingServiceInfo geocodingServiceInfo)
        {
            AgsServer currentServer = null;

            foreach (AgsServer server in _servers)
            {
                if (server.Name.Equals(geocodingServiceInfo.serverName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Assert(currentServer == null);
                    currentServer = server;
                }
            }

            if (currentServer == null)
            {
                string message = Properties.Resources.ServerNameNotFound;
                throw new SettingsException(message);
            }

            return currentServer;
        }

        private void Licenser_LicenseActivated(object sender, EventArgs e)
        {
            NetworkCredential licAccount = Licenser.LicenseAccount;
            Debug.Assert(licAccount != null);

            foreach (AgsServer server in _servers)
            {
                try
                {
                    if (server.AuthenticationType == AgsServerAuthenticationType.UseApplicationLicenseCredentials)
                        server.Authorize(licAccount.UserName, licAccount.Password, false);
                }
                catch { }
            }
        }

        private UserServicesInfo _LoadUserConfig(string path)
        {
            Debug.Assert(path != null);

            if (!File.Exists(path))
                _CreateUserConfig(path);

            return _Load<UserServicesInfo>(path);
        }

        private void _CreateUserConfig(string path)
        {
            Debug.Assert(path != null);

            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", "");
            doc.AppendChild(dec);

            XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, NODE_USER_ROOT, "");
            doc.AppendChild(rootNode);

            doc.Save(path);
        }

        #endregion private methods

        #region private constants
        /// <summary>
        /// The type of the geocoder which could be used as a streets geocoder.
        /// </summary>
        private const string STREETS_GEOCODER_TYPE = "ArcGIS.Streets";
        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string NODE_USER_ROOT = "services";
        private const string ARCGIS_GEOCODER_TYPE = "Arcgis_com";

        private string _userConfigPath;
        private ServicesInfo _servicesInfo;
        private UserServicesInfo _userServicesInfo;
        private List<AgsServer> _servers = new List<AgsServer>();
        private Map _map;
        private GeocoderBase _geocoder;
        private VrpSolver _solver;

        /// <summary>
        /// The reference to the service validator object.
        /// </summary>
        private ISolveServiceValidator _solveServiceValidator = new SolveServiceValidator();

        /// <summary>
        /// The reference to the solve services info.
        /// </summary>
        private SolveInfoWrap _solveInfo;

        /// <summary>
        /// The reference to the tasks runner object to be used for parallelizing
        /// services initialization.
        /// </summary>
        private ITaskRunner _taskRunner = new ParallelTaskRunner();

        /// <summary>
        /// The reference to the remote certificate validation settings object.
        /// </summary>
        private ICertificateValidationSettings _certificateValidationSettings;

        /// <summary>
        /// Exceptions handler.
        /// </summary>
        private IServiceExceptionHandler _exceptionHandler;

        #endregion private members
    }
}
