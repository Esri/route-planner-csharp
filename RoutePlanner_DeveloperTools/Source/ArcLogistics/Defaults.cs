using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using ESRI.ArcLogistics.Serialization;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Application defaults (Singleton)
    /// </summary>
    internal sealed class Defaults
    {
        #region Static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets singletone instance
        /// </summary>
        public static Defaults Instance
        {
            get
            {
                Debug.Assert(_defaults != null);

                return _defaults;
            }
        }

        private static Defaults _defaults = null;

        /// <summary>
        /// Gets full name of file with defaults settings.
        /// </summary>
        public static string DefaultsFilePath
        {
            get
            {
                return Path.Combine(DataFolder.Path, DEFAULTS_FILE_NAME);
            }
        }

        #endregion // Static properties

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cosntructor.
        /// </summary>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Config file is invalid.
        /// In property "Source" there is path to invalid config file.</exception>
        private Defaults()
        {
            _Initialize(DefaultsFilePath);
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Application defaults capacities info
        /// </summary>
        public CapacitiesInfo CapacitiesInfo
        {
            get { return _capacitiesInfo; }
        }

        /// <summary>
        /// Application defaults fueltypes info
        /// </summary>
        public FuelTypesInfo FuelTypesInfo
        {
            get { return _fuelTypesInfo; }
        }

        /// <summary>
        /// Application OrderCustomPropertiesInfo.
        /// </summary>
        public OrderCustomPropertiesInfo OrderCustomPropertiesInfo
        {
            get { return _orderCustomPropertiesInfo; }

            set {
                    Debug.Assert(value != null);
                    _orderCustomPropertiesInfo = value;

                    // Update _defaultConfig.
                    _defaultConfig.CustomOrderProperties = _ConvertToCustomOrderProperties(value);
                }
        }

        // APIREV: exposing LocationsDefaults, OrderDefaults, RoutesDefaults, VehiclesDefaults, DriversDefaults should be replaced with solid interfaces
        /// <summary>
        /// Application locations defaults
        /// </summary>
        public LocationsDefaults LocationsDefaults
        {
            get { return _defaultConfig.LocationsDefaults; }
        }

        /// <summary>
        /// Application order defaults
        /// </summary>
        public OrdersDefaults OrdersDefaults
        {
            get { return _defaultConfig.OrdersDefaults; }
        }

        /// <summary>
        /// Application locations defaults
        /// </summary>
        public RoutesDefaults RoutesDefaults
        {
            get { return _defaultConfig.RoutesDefaults; }
        }

        /// <summary>
        /// Application locations defaults
        /// </summary>
        public VehiclesDefaults VehiclesDefaults
        {
            get { return _defaultConfig.VehiclesDefaults; }
        }

        /// <summary>
        /// Application locations defaults
        /// </summary>
        public DriversDefaults DriversDefaults
        {
            get { return _defaultConfig.DriversDefaults; }
        }
        #endregion // Public properties

        #region Public methods

        /// <summary>
        /// Saves defaults settings.
        /// </summary>
        /// <exception cref="SettingsException">Failed to save defaults.</exception>
        public void Save()
        {
            XmlWriter xmlWriter = null;

            try
            {
                // Create XML serializer.
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(DefaultsConfig));

                // XML settings.
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = CommonHelpers.XML_SETTINGS_INDENT_CHARS;

                // Create XML writer.
                xmlWriter = XmlWriter.Create(DefaultsFilePath, settings);

                Debug.Assert(_defaultConfig != null);

                // Convert defaults configuration to the format which can be serialized.
                DefaultsConfig defaultsConfig = _ConvertToDefaultsConfig(_defaultConfig);

                // Serialize data to the XML file.
                xmlSerializer.Serialize(xmlWriter, defaultsConfig);
            }
            catch (InvalidOperationException ex)
            {
                throw new SettingsException(ex.Message, ex);
            }
            finally
            {
                if (xmlWriter != null)
                    xmlWriter.Close();
            }
        }

        #endregion Public methods

        #region public static methods
        /// <summary>
        /// Initializes <see cref="T:ESRI.ArcLogistics.Defaults"/> class instance. The method should
        /// be called prior to first use of any other public member of the class.
        /// </summary>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Config file is invalid.
        /// In property "Source" there is path to invalid config file.</exception>
        public static void Initialize()
        {
            _defaults = new Defaults();
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A new configuration object with values stored in the user configuration
        /// if there are any or values stored in the default configuration otherwise.</returns>
        private static DefaultsConfiguration _Merge(
            DefaultsConfig userConfig,
            DefaultsConfig defaultConfig)
        {
            return new DefaultsConfiguration
            {
                CapacitiesDefaults = _Merge(
                    userConfig.CapacitiesDefaults,
                    defaultConfig.CapacitiesDefaults),
                FuelTypesDefaults = _Merge(
                    userConfig.FuelTypesDefaults,
                    defaultConfig.FuelTypesDefaults),
                LocationsDefaults = _Merge(
                    userConfig.LocationsDefaults,
                    defaultConfig.LocationsDefaults),
                OrdersDefaults = _Merge(
                    userConfig.OrdersDefaults,
                    defaultConfig.OrdersDefaults),
                RoutesDefaults = _Merge(
                    userConfig.RoutesDefaults,
                    defaultConfig.RoutesDefaults),
                VehiclesDefaults = _Merge(
                    userConfig.VehiclesDefaults,
                    defaultConfig.VehiclesDefaults),
                DriversDefaults = _Merge(
                    userConfig.DriversDefaults,
                    defaultConfig.DriversDefaults),
                CustomOrderProperties = _Merge(
                    userConfig.CustomOrderProperties,
                    defaultConfig.CustomOrderProperties),
            };
        }

        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A new configuration object with values stored in the user configuration
        /// if there are any or values stored in the default configuration otherwise.</returns>
        private static LocationsDefaults _Merge(
            LocationsDefaultsConfig userConfig,
            LocationsDefaultsConfig defaultConfig)
        {
            if (userConfig == null)
            {
                userConfig = new LocationsDefaultsConfig();
            }

            return new LocationsDefaults
            {
                CurbApproach = _Merge(userConfig.CurbApproach, defaultConfig.CurbApproach),
                TimeWindow = _Merge(userConfig.TimeWindow, defaultConfig.TimeWindow),
            };
        }

        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A new configuration object with values stored in the user configuration
        /// if there are any or values stored in the default configuration otherwise.</returns>
        private static OrdersDefaults _Merge(
            OrdersDefaultsConfig userConfig,
            OrdersDefaultsConfig defaultConfig)
        {
            if (userConfig == null)
            {
                userConfig = new OrdersDefaultsConfig();
            }

            return new OrdersDefaults
            {
                CurbApproach = _Merge(userConfig.CurbApproach, defaultConfig.CurbApproach),
                MaxViolationTime = _Merge(
                    userConfig.MaxViolationTime,
                    defaultConfig.MaxViolationTime),
                OrderType = _Merge(
                    userConfig.OrderType,
                    defaultConfig.OrderType),
                Priority = _Merge(
                    userConfig.Priority,
                    defaultConfig.Priority),
                ServiceTime = _Merge(
                    userConfig.ServiceTime,
                    defaultConfig.ServiceTime),
                TimeWindow = _Merge(
                    userConfig.TimeWindow,
                    defaultConfig.TimeWindow),
                TimeWindow2 = _Merge(
                    userConfig.TimeWindow2,
                    defaultConfig.TimeWindow2),
            };
        }

        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A new configuration object with values stored in the user configuration
        /// if there are any or values stored in the default configuration otherwise.</returns>
        private static RoutesDefaults _Merge(
            RoutesDefaultsConfig userConfig,
            RoutesDefaultsConfig defaultConfig)
        {
            if (userConfig == null)
            {
                userConfig = new RoutesDefaultsConfig();
            }

            return new RoutesDefaults
            {
                MaxOrder = _Merge(userConfig.MaxOrder, defaultConfig.MaxOrder),
                MaxTotalDuration = _Merge(
                    userConfig.MaxTotalDuration,
                    defaultConfig.MaxTotalDuration),
                MaxTravelDistance = _Merge(
                    userConfig.MaxTravelDistance,
                    defaultConfig.MaxTravelDistance),
                MaxTravelDuration = _Merge(
                    userConfig.MaxTravelDuration,
                    defaultConfig.MaxTravelDuration),
                StartTimeWindow = _Merge(userConfig.StartTimeWindow, defaultConfig.StartTimeWindow),
                TimeAtEnd = _Merge(userConfig.TimeAtEnd, defaultConfig.TimeAtEnd),
                TimeAtRenewal = _Merge(userConfig.TimeAtRenewal, defaultConfig.TimeAtRenewal),
                TimeAtStart = _Merge(userConfig.TimeAtStart, defaultConfig.TimeAtStart),
            };
        }

        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A new configuration object with values stored in the user configuration
        /// if there are any or values stored in the default configuration otherwise.</returns>
        private static VehiclesDefaults _Merge(
            VehiclesDefaultsConfig userConfig,
            VehiclesDefaultsConfig defaultConfig)
        {
            if (userConfig == null)
            {
                userConfig = new VehiclesDefaultsConfig();
            }

            return new VehiclesDefaults
            {
                CapacitiesDefaultValues = _Merge(
                    userConfig.CapacitiesDefaultValues,
                    defaultConfig.CapacitiesDefaultValues),
                FuelEconomy = _Merge(userConfig.FuelEconomy, defaultConfig.FuelEconomy),
            };
        }

        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A new configuration object with values stored in the user configuration
        /// if there are any or values stored in the default configuration otherwise.</returns>
        private static DriversDefaults _Merge(
            DriversDefaultsConfig userConfig,
            DriversDefaultsConfig defaultConfig)
        {
            if (userConfig == null)
            {
                userConfig = new DriversDefaultsConfig();
            }

            return new DriversDefaults
            {
                PerHour = _Merge(userConfig.PerHour, defaultConfig.PerHour),
                PerHourOT = _Merge(userConfig.PerHourOT, defaultConfig.PerHourOT),
                TimeBeforeOT = _Merge(userConfig.TimeBeforeOT, defaultConfig.TimeBeforeOT),
            };
        }

        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <typeparam name="T">The type of the configuration parameter.</typeparam>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A value stored in the user configuration if there is any or value stored in
        /// the default configuration otherwise.</returns>
        private static T _Merge<T>(T userConfig, T defaultConfig)
            where T : class
        {
            return userConfig ?? defaultConfig;
        }

        /// <summary>
        /// Merges specified user and default configurations.
        /// </summary>
        /// <typeparam name="T">The type of the configuration parameter.</typeparam>
        /// <param name="userConfig">The user configuration.</param>
        /// <param name="defaultConfig">The default configuration.</param>
        /// <returns>A value stored in the user configuration if there is any or value stored in
        /// the default configuration otherwise.</returns>
        private static T _Merge<T>(T? userConfig, T? defaultConfig)
            where T : struct
        {
            return userConfig.GetValueOrDefault(defaultConfig.GetValueOrDefault());
        }

        /// <summary>
        /// Loads object of the specified type from a file with the specified path.
        /// </summary>
        /// <typeparam name="T">The type of an object to load.</typeparam>
        /// <param name="filePath">The path to the file storing object of the specified
        /// type.</param>
        /// <returns>A new instance of an object of the specified type loaded from a file with the
        /// specified path.</returns>
        /// <exception cref="T:System.IO.FileNotFoundException">The specified file cannot be
        /// found.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invliad.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">I/O error occured during reading the
        /// specified file.</exception>
        /// <exception cref="T:System.InvalidOperationException">An error occured during object
        /// deserialization.</exception>
        private static T _Load<T>(string filePath)
        {
            return _Load<T>(() => new StreamReader(filePath));
        }

        /// <summary>
        /// Loads defaults configuration.
        /// </summary>
        /// <returns>A new instance of defaults configuration object.</returns>
        private static DefaultsConfig _LoadConfig()
        {
            return _Load<DefaultsConfig>(() => new StringReader
                (ResourceLoader.ReadFileAsString(DEFAULTS_FILE_NAME)));
        }

        /// <summary>
        /// Loads object of the specified type from a text reader obtained with the specified
        /// factory function.
        /// </summary>
        /// <typeparam name="T">The type of an object to load.</typeparam>
        /// <param name="readerFactory">The function to be used for creating a text reader object
        /// providing access to a serialized representation of an object of the specified type.
        /// </param>
        /// <returns>A new instance of an object of the specified type loaded from a text reader
        /// obtained with the specified factory function.</returns>
        /// <exception cref="T:System.InvalidOperationException">An error occured during object
        /// deserialization.</exception>
        /// <remarks>The function rethrows any exception occured during call to the
        /// <paramref name="readerFactory"/> function.</remarks>
        private static T _Load<T>(Func<TextReader> readerFactory)
        {
            using (var reader = readerFactory())
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(reader);
            }
        }
        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create DefaultsFile instance from file
        /// </summary>
        /// <returns>Application defaults</returns>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Config file is invalid.
        /// In property "Source" there is path to invalid config file.</exception>
        private void _Initialize(string defaultsFilePath)
        {
            _CreateDefaultsFileIfNotExists(defaultsFilePath);

            var userConfig = default(DefaultsConfig);
            try
            {
                userConfig = _Load<DefaultsConfig>(defaultsFilePath);
            }
            catch (Exception ex)
            {
                if (ex is FileNotFoundException ||
                    ex is DirectoryNotFoundException ||
                    ex is IOException)
                {
                    Logger.Error(ex);
                }
                else if (ex is InvalidOperationException)
                {
                    // If XML corrupted - wrap exceptin and save path to file.
                    SettingsException settingsException = new SettingsException(ex.Message, ex);
                    settingsException.Source = defaultsFilePath;
                    throw settingsException;
                }
                else
                {
                    throw;
                }
            }

            var defaultConfig = _LoadConfig();

            _defaultConfig = _Merge(userConfig, defaultConfig);

            _capacitiesInfo = _LoadCapacitiesDefaults(_defaultConfig.CapacitiesDefaults);
            _fuelTypesInfo = _LoadFuelTypesDefaults(_defaultConfig.FuelTypesDefaults);
            _orderCustomPropertiesInfo = _LoadOrderCustomPropertiesDefaults(_defaultConfig.CustomOrderProperties);

            if (_defaultConfig.CapacitiesDefaults == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsCapacities);
            if (_defaultConfig.FuelTypesDefaults == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsFuelTypes);
            if (_defaultConfig.LocationsDefaults == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsLocation);
            if (_defaultConfig.OrdersDefaults == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsOrder);
            if (_defaultConfig.RoutesDefaults == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsRoute);
            if (_defaultConfig.VehiclesDefaults == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsVehicle);
            if (_defaultConfig.DriversDefaults == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsDriver);
            if (_defaultConfig.CustomOrderProperties == null)
                throw new SettingsException(Properties.Messages.Error_NoDefaultsOrderCustomProperties);
        }

        // Load capacity defaults
        private CapacitiesInfo _LoadCapacitiesDefaults(CapacitiesDefaults capacitiesConfig)
        {
            CapacitiesInfo capacitiesInfo = new CapacitiesInfo();

            StringCollection uniqueNames = new StringCollection();
            foreach (CapacityConfig capacity in capacitiesConfig.Capacity)
            {
                if (!uniqueNames.Contains(capacity.Name))
                {   // added only unique named capacities
                    capacitiesInfo.Add(new CapacityInfo(capacity.Name, capacity.DisplayUnitUS, capacity.DisplayUnitMetric));
                    uniqueNames.Add(capacity.Name);
                }
            }

            return capacitiesInfo;
        }

        // Load fueltype defaults
        private FuelTypesInfo _LoadFuelTypesDefaults(FuelTypesDefaults fuelTypesConfig)
        {
            FuelTypesInfo fuelTypesInfo = new FuelTypesInfo();

            StringCollection uniqueNames = new StringCollection();
            foreach (FuelTypeConfig fuelTypeConfig in fuelTypesConfig.FuelTypeConfig)
            {
                if (!uniqueNames.Contains(fuelTypeConfig.Name))
                {   // added only unique named types
                    FuelTypeInfo fuelTypeInfo = new FuelTypeInfo(fuelTypeConfig.Name, fuelTypeConfig.Price, fuelTypeConfig.Co2Emission);
                    fuelTypesInfo.Add(fuelTypeInfo);
                    uniqueNames.Add(fuelTypeConfig.Name);
                }
            }

            return fuelTypesInfo;
        }

        // Load order custom properties defaults
        private OrderCustomPropertiesInfo _LoadOrderCustomPropertiesDefaults(CustomOrderProperties customOrderProperties)
        {
            OrderCustomPropertiesInfo propertiesInfo = new OrderCustomPropertiesInfo();

            StringCollection uniqueNames = new StringCollection();
            foreach (CustomOrderProperty customOrderProperty in customOrderProperties.CustomOrderProperty)
            {
                if (!uniqueNames.Contains(customOrderProperty.Name))
                {   // added only unique named properties
                    OrderCustomProperty property = new OrderCustomProperty(customOrderProperty.Name,
                                                                           customOrderProperty.Type,
                                                                           customOrderProperty.MaxLength,
                                                                           customOrderProperty.Description,
                                                                           customOrderProperty.OrderPairKey);
                    propertiesInfo.Add(property);
                    uniqueNames.Add(customOrderProperty.Name);
                }
            }

            return propertiesInfo;
        }

        // Check existance of defaults file
        private void _CreateDefaultsFileIfNotExists(string defaultsFilePath)
        {
            string commonAppData = Path.GetDirectoryName(defaultsFilePath);
            if (!Directory.Exists(commonAppData))
                Directory.CreateDirectory(commonAppData);

            if (!File.Exists(defaultsFilePath))
            {
                using (TextWriter writer = new StreamWriter(defaultsFilePath))
                {
                    string defaultsFileContent = ResourceLoader.ReadFileAsString(DEFAULTS_FILE_NAME);
                    writer.Write(defaultsFileContent);
                }
            }
        }

        /// <summary>
        /// Converts OrderCustomProperty to CustomOrderProperty.
        /// </summary>
        /// <param name="orderCustomProperty">OrderCustomProperty object to convert.</param>
        /// <returns>Converted object.</returns>
        private CustomOrderProperty _ConvertToCustomOrderProperty(OrderCustomProperty orderCustomProperty)
        {
            CustomOrderProperty customOrderPropery = new CustomOrderProperty();

            customOrderPropery.Name = orderCustomProperty.Name;
            customOrderPropery.Type = orderCustomProperty.Type;
            customOrderPropery.MaxLength = orderCustomProperty.Length;
            customOrderPropery.Description = orderCustomProperty.Description;
            customOrderPropery.OrderPairKey = orderCustomProperty.OrderPairKey;

            return customOrderPropery;
        }

        /// <summary>
        /// Converts OrderCustomPropertiesInfo to CustomOrderProperties.
        /// </summary>
        /// <param name="customOrderPropertiesInfo">OrderCustomPropertiesInfo object to convert.</param>
        /// <returns>Converted object.</returns>
        private CustomOrderProperties _ConvertToCustomOrderProperties(OrderCustomPropertiesInfo customOrderPropertiesInfo)
        {
            Debug.Assert(customOrderPropertiesInfo != null);

            // Create custom order properties.
            CustomOrderProperties customOrderProperties = new CustomOrderProperties();

            // Create array of CustomOrderProperty objects.
            customOrderProperties.CustomOrderProperty = new CustomOrderProperty[customOrderPropertiesInfo.Count];

            // Convert each object of collection customOrderPropertiesInfo to OrderCustomProperty and store to array.
            for (int i = 0; i < customOrderPropertiesInfo.Count; i++)
            {
                OrderCustomProperty orderCustomProperty = customOrderPropertiesInfo[i];

                customOrderProperties.CustomOrderProperty[i] = _ConvertToCustomOrderProperty(orderCustomProperty);
            }

            return customOrderProperties;
        }

        /// <summary>
        /// Converts LocationsDefaults to LocationsDefaultsConfig.
        /// </summary>
        /// <param name="locationDefaults">LocationsDefaults object.</param>
        /// <returns>Converted object.</returns>
        private LocationsDefaultsConfig _ConvertLocationsDefaultsToConfig(LocationsDefaults locationDefaults)
        {
            Debug.Assert(locationDefaults != null);

            LocationsDefaultsConfig locationsDefaultsConfig = new LocationsDefaultsConfig();

            locationsDefaultsConfig.CurbApproach = locationDefaults.CurbApproach;
            locationsDefaultsConfig.TimeWindow = locationDefaults.TimeWindow;

            return locationsDefaultsConfig;
        }

        /// <summary>
        /// Convert OrdersDefaults to OrdersDefaultsConfig.
        /// </summary>
        /// <param name="orderDefaults">OrdersDefaults object.</param>
        /// <returns>Converted object.</returns>
        private OrdersDefaultsConfig _ConvertOrderDefaultsToConfig(OrdersDefaults orderDefaults)
        {
            Debug.Assert(orderDefaults != null);

            OrdersDefaultsConfig ordersDefaultsConfig = new OrdersDefaultsConfig();

            ordersDefaultsConfig.OrderType = orderDefaults.OrderType;
            ordersDefaultsConfig.Priority = orderDefaults.Priority;
            ordersDefaultsConfig.CurbApproach = orderDefaults.CurbApproach;
            ordersDefaultsConfig.ServiceTime = orderDefaults.ServiceTime;
            ordersDefaultsConfig.TimeWindow = orderDefaults.TimeWindow;
            ordersDefaultsConfig.TimeWindow2 = orderDefaults.TimeWindow2;
            ordersDefaultsConfig.MaxViolationTime = orderDefaults.MaxViolationTime;

            return ordersDefaultsConfig;
        }

        /// <summary>
        /// Converts RoutesDefaults to RoutesDefaultsConfig.
        /// </summary>
        /// <param name="routesDefaults">RoutesDefaults object.</param>
        /// <returns>Converted object.</returns>
        private RoutesDefaultsConfig _ConvertRoutesDefaultsToConfig(RoutesDefaults routesDefaults)
        {
            Debug.Assert(routesDefaults != null);

            RoutesDefaultsConfig routesDefaultsConfig = new RoutesDefaultsConfig();

            routesDefaultsConfig.StartTimeWindow = routesDefaults.StartTimeWindow;
            routesDefaultsConfig.TimeAtStart = routesDefaults.TimeAtStart;
            routesDefaultsConfig.TimeAtEnd = routesDefaults.TimeAtEnd;
            routesDefaultsConfig.TimeAtRenewal = routesDefaults.TimeAtRenewal;
            routesDefaultsConfig.MaxOrder = routesDefaults.MaxOrder;
            routesDefaultsConfig.MaxTravelDistance = routesDefaults.MaxTravelDistance;
            routesDefaultsConfig.MaxTravelDuration = routesDefaults.MaxTravelDuration;
            routesDefaultsConfig.MaxTotalDuration = routesDefaults.MaxTotalDuration;

            return routesDefaultsConfig;
        }

        /// <summary>
        /// Converts VehiclesDefaults to VehiclesDefaultsConfig.
        /// </summary>
        /// <param name="vehiclesDefaults">VehiclesDefaults object.</param>
        /// <returns>Converted object.</returns>
        private VehiclesDefaultsConfig _ConvertVehiclesDefaultsToConfig(VehiclesDefaults vehiclesDefaults)
        {
            Debug.Assert(vehiclesDefaults != null);

            VehiclesDefaultsConfig vehiclesDefaultsConfig = new VehiclesDefaultsConfig();

            vehiclesDefaultsConfig.FuelEconomy = vehiclesDefaults.FuelEconomy;
            vehiclesDefaultsConfig.CapacitiesDefaultValues = vehiclesDefaults.CapacitiesDefaultValues;

            return vehiclesDefaultsConfig;
        }

        /// <summary>
        /// Converts DriversDefaults to DriversDefaultsConfig.
        /// </summary>
        /// <param name="driversDefaults">DriversDefaults object.</param>
        /// <returns>Converted object.</returns>
        private DriversDefaultsConfig _ConvertDriversDefaultsToConfig(DriversDefaults driversDefaults)
        {
            Debug.Assert(driversDefaults != null);

            DriversDefaultsConfig driversDefaultsConfig = new DriversDefaultsConfig();

            driversDefaultsConfig.PerHour = driversDefaults.PerHour;
            driversDefaultsConfig.PerHourOT = driversDefaults.PerHourOT;
            driversDefaultsConfig.TimeBeforeOT = driversDefaults.TimeBeforeOT;

            return driversDefaultsConfig;
        }

        /// <summary>
        /// Converts DefaultsConfiguration to DefaultsConfig which can be serialized to the XML file.
        /// </summary>
        /// <param name="defaultsConfiguration">DefaultsConfiguration object.</param>
        /// <returns>Converted object.</returns>
        private DefaultsConfig _ConvertToDefaultsConfig(DefaultsConfiguration defaultsConfiguration)
        {
            Debug.Assert(defaultsConfiguration != null);

            DefaultsConfig defaultsConfig = new DefaultsConfig();

            defaultsConfig.CapacitiesDefaults = defaultsConfiguration.CapacitiesDefaults;
            defaultsConfig.FuelTypesDefaults = defaultsConfiguration.FuelTypesDefaults;
            defaultsConfig.LocationsDefaults =
                _ConvertLocationsDefaultsToConfig(defaultsConfiguration.LocationsDefaults);
            defaultsConfig.OrdersDefaults =
                _ConvertOrderDefaultsToConfig(defaultsConfiguration.OrdersDefaults);
            defaultsConfig.RoutesDefaults =
                _ConvertRoutesDefaultsToConfig(defaultsConfiguration.RoutesDefaults);
            defaultsConfig.VehiclesDefaults =
                _ConvertVehiclesDefaultsToConfig(defaultsConfiguration.VehiclesDefaults);
            defaultsConfig.DriversDefaults =
                _ConvertDriversDefaultsToConfig(defaultsConfiguration.DriversDefaults);
            defaultsConfig.CustomOrderProperties = defaultsConfiguration.CustomOrderProperties;

            return defaultsConfig;
        }

        #endregion // Private methods

        #region Private constants

        /// <summary>
        /// Name of the file with default configurations.
        /// </summary>
        private const string DEFAULTS_FILE_NAME = "defaults.xml";

        #endregion

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DefaultsConfiguration _defaultConfig = null;
        private CapacitiesInfo _capacitiesInfo = null;
        private FuelTypesInfo _fuelTypesInfo = null;
        private OrderCustomPropertiesInfo _orderCustomPropertiesInfo = null;

        #endregion // Private members
    }
}
