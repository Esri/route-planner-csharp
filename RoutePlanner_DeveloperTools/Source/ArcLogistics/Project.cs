using System;
using System.Collections.Generic;
using System.Data;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using ESRI.ArcLogistics.Archiving;
using ESRI.ArcLogistics.BreaksHelpers;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Data.DataModel;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.CoreEx;
using DataModel = ESRI.ArcLogistics.Data.DataModel;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Project class contains all the information about an ArcLogistics project.
    /// </summary>
    public class Project : ESRI.ArcLogistics.IProject
    {
        #region constructors
        /// <summary>
        /// Creates a new instance of the <c>Project</c> class.
        /// </summary>
        public Project()
        {
        }

        /// <summary>
        /// Creates a new instance of the <c>Project</c> class.
        /// </summary>
        public Project(string projectConfigPath, IProjectSaveExceptionHandler handler)
        {
            _handler = handler;

            if (projectConfigPath == null)
                throw new ArgumentNullException("projectConfigPath");

            _OpenProject(projectConfigPath);
        }

        internal Project(string projectConfigPath, CapacitiesInfo capacitiesInfo,
            OrderCustomPropertiesInfo orderCustomPropertiesInfo, 
            IProjectSaveExceptionHandler handler)
        {
            _handler = handler;

            if (projectConfigPath == null)
                throw new ArgumentNullException("projectConfigPath");

            _CreateProject(projectConfigPath, capacitiesInfo, orderCustomPropertiesInfo);
        }

        #endregion

        #region public events

        // TODO: DataObjectManager workaround
        public event SaveChangesCompletedEventHandler SaveChangesCompleted;

        /// <summary>
        /// Raises on saving database changes.
        /// </summary>
        public event SavingChangesEventHandler SavingChanges;

        #endregion

        #region public methods

        /// <summary>
        /// Opens project.
        /// </summary>
        /// <param name="projectConfigPath">Path to project configuration file.</param>
        public void Open(string projectConfigPath)
        {
            if (projectConfigPath == null)
                throw new ArgumentNullException("projectConfigPath");

            if (_isOpened)
                Close();

            _OpenProject(projectConfigPath);
        }

        /// <summary>
        /// Closes project.
        /// </summary>
        public void Close()
        {
            if (!_isOpened)
                return; // project isn't opened

            _DisposeCollections();

            _projectCfg = null;
            if (_dataContext != null)
            {
                _dataContext.SaveChangesCompleted -= _dataContext_SaveChangesCompleted;
                _dataContext.PostSavingChanges -= _dataContext_PostSavingChanges;
                _dataContext.Dispose();
                _dataContext = null;
            }

            _isOpened = false;
        }
        #endregion

        #region IProject Members
        /// <summary>
        /// Projects's description. This is displayed on the Projects action panel on the Home tab of the application.
        /// </summary>
        public string Description
        {
            get
            {
                if (!_isOpened)
                    throw new InvalidOperationException(Properties.Resources.ProjectIsNotOpened);

                return _projectCfg.Description;
            }

            set { _projectCfg.Description = value; }
        }
        /// <summary>
        /// Project's name.
        /// </summary>
        public string Name
        {
            get
            {
                if (!_isOpened)
                    throw new InvalidOperationException(Properties.Resources.ProjectIsNotOpened);

                return _projectCfg.Name;
            }
        }
        /// <summary>
        /// Project's filepath.
        /// </summary>
        public string Path
        {
            get
            {
                if (!_isOpened)
                    throw new InvalidOperationException(Properties.Resources.ProjectIsNotOpened);

                return _projectCfg.FilePath;
            }
        }
        /// <summary>
        /// Project's properties.
        /// </summary>
        public IProjectProperties ProjectProperties
        {
            get
            {
                if (!_isOpened)
                    throw new InvalidOperationException(Properties.Resources.ProjectIsNotOpened);

                return _projectCfg.ProjectProperties;
            }
        }
        /// <summary>
        /// Project's archiving settings.
        /// </summary>
        public ProjectArchivingSettings ProjectArchivingSettings
        {
            get
            {
                if (!_isOpened)
                    throw new InvalidOperationException(Properties.Resources.ProjectIsNotOpened);

                return _projectCfg.ProjectArchivingSettings;
            }
        }

        /// <summary>
        /// Gets the project's breaks settings.
        /// </summary>
        /// <remarks>Can be null, if not supported.</remarks>
        public BreaksSettings BreaksSettings
        {
            get
            {
                if (!_isOpened)
                    throw new InvalidOperationException(Properties.Resources.ProjectIsNotOpened);

                return _projectCfg.BreaksSettings;
            }
        }

        /// <summary>
        /// Returns a boolean value based on whether or not
        /// the project instance is open.
        /// </summary>
        public bool IsOpened
        {
            get { return _isOpened; }
        }
        /// <summary>
        /// The DefaultRoutes collection contains information about the default routes of the project.
        /// </summary>
        /// <remarks>
        ///  A default route is a combination of a driver and a vehicle that can be assigned to service orders for any given day.
        /// </remarks>
        public IDataObjectCollection<Route> DefaultRoutes
        {
            get { return _routesDefault; }
        }
        /// <summary>
        /// The Drivers collection contains information about all drivers added to the project.
        /// </summary>
        public IDataObjectCollection<Driver> Drivers
        {
            get { return _drivers; }
        }
        /// <summary>
        /// The DriverSpecialties collection contains information about all driver specialities added to the project.
        /// </summary>
        public IDataObjectCollection<DriverSpecialty> DriverSpecialties
        {
            get { return _driverSpecialties; }
        }
        /// <summary>
        /// The Locations collection contains information about all locations added to the project.
        /// </summary>
        public IDataObjectCollection<Location> Locations
        {
            get { return _locations; }
        }
        /// <summary>
        /// The MobileDevices collection contains information about all mobile devices added to the project.
        /// </summary>
        public IDataObjectCollection<MobileDevice> MobileDevices
        {
            get { return _mobileDevices; }
        }
        /// <summary>
        /// The Vehicles collection contains information about all vehicles added to the project.
        /// </summary>
        public IDataObjectCollection<Vehicle> Vehicles
        {
            get { return _vehicles; }
        }

        /// <summary>
        /// The VehicleSpecialties collection contains information about all vehicle specialities added to the project.
        /// </summary>
        public IDataObjectCollection<VehicleSpecialty> VehicleSpecialties
        {
            get { return _vehicleSpecialties; }
        }
        /// <summary>
        /// The Zones collection contains information about all zones added to the project.
        /// </summary>
        public IDataObjectCollection<Zone> Zones
        {
            get { return _zones; }
        }
        /// <summary>
        /// The FuelTypes collection contains information about all fuel types added to the project.
        /// </summary>
        public IDataObjectCollection<FuelType> FuelTypes
        {
            get { return _fuelTypes; }
        }
        /// <summary>
        /// The Barriers collection contains information about all barriers added to the project.
        /// </summary>
        public BarrierManager Barriers
        {
            get { return _barrierManager; }
        }
        /// <summary>
        /// The Orders collection contains information about all orders added to the project.
        /// </summary>
        public OrderManager Orders
        {
            get { return _orderManager; }
        }
        /// <summary>
        /// The Schedules collection contains information about all schedules belonging to the project.
        /// </summary>
        /// /// <remarks>
        /// A Schedule is a solution of a routing problem, and contains a set of routes with assigned orders.
        /// </remarks>
        public ScheduleManager Schedules
        {
            get { return _scheduleManager; }
        }

        internal GenericDataObjectManager<Location> LocationManager
        {
            get { return _locationManager; }
        }

        /// <summary>
        /// Gets a reference to the deletion checking service.
        /// </summary>
        public IDeletionCheckingService DeletionCheckingService
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates <c>Route</c> with default breaks.
        /// </summary>
        /// <returns></returns>
        public Route CreateRoute()
        {
            Route route = new Route(CapacitiesInfo);
            route.Breaks = BreaksSettings.DefaultBreaks.Clone() as Breaks;
            return route;
        }

        /// <summary>
        /// Saves all unsaved changes to the project.
        /// </summary>
        public void Save()
        {
            _ExecuteDatabaseCommand(_Save);
        }
        #endregion

        #region public properties

        /// <summary>
        /// Project's CapacitiesInfo.
        /// </summary>
        public CapacitiesInfo CapacitiesInfo
        {
            get { return _dataContext.CapacitiesInfo; }
        }

        /// <summary>
        /// Project's OrderCustomPropertiesInfo.
        /// </summary>
        public OrderCustomPropertiesInfo OrderCustomPropertiesInfo
        {
            get { return _dataContext.OrderCustomPropertiesInfo; }
        }

        #endregion

        #region internal methods
        /// <summary>
        /// Adds new 'Current' schedule with the specified name for the specified planned date.
        /// </summary>
        /// <param name="plannedDate">The data/time to add schedule for.</param>
        /// <param name="name">The name of the schedule to be added.</param>
        /// <returns>An instance of the added schedule.</returns>
        internal Schedule AddNewSchedule(DateTime plannedDate, string name)
        {
            Debug.Assert(name != null);

            var scheduleId = Guid.NewGuid();
            _ExecuteWithTransaction(context =>
            {
                // Insert a new schedule to the database.
                var scheduleIdParameter = _CreateParameter(
                    SCHEDULE_ID_PARAMETER_NAME,
                    scheduleId);
                var creationTimeParameter = _CreateParameter(
                    CREATION_TIME_PARAMETER_NAME,
                    DateTime.Now.Ticks);
                var plannedDateParameter = _CreateParameter(
                    PLANNED_DATE_PARAMETER_NAME,
                    plannedDate);
                var nameParameter = _CreateParameter(
                    NAME_PARAMETER_NAME,
                    name);
                var scheduleTypeParameter = _CreateParameter(
                    SCHEDULE_TYPE_PARAMETER_NAME,
                    (int)ScheduleType.Current);
                context.ExecuteStoreCommand(
                    _createScheduleScript,
                    scheduleIdParameter,
                    nameParameter,
                    plannedDateParameter,
                    scheduleTypeParameter,
                    creationTimeParameter);

                _LoadDefaultRoutesForSchedule(context, scheduleId, plannedDate);
            });

            // Refresh project-wide data context to see new changes.
            _dataContext.Refresh(RefreshMode.StoreWins, _dataContext.Schedules);

            var result = this.Schedules.SearchById(scheduleId);

            return result;
        }

        /// <summary>
        /// Copies default routes to the schedule with the specified ID.
        /// </summary>
        /// <param name="scheduleId">The primary key of the schedule to copy routes to.</param>
        /// <param name="plannedDate">The planned date of the schedule to copy routes to.</param>
        internal void LoadDefaultRoutesForSchedule(Guid scheduleId, DateTime plannedDate)
        {
            _ExecuteWithTransaction(context =>
                _LoadDefaultRoutesForSchedule(context, scheduleId, plannedDate));

            // Refresh project-wide data context to see new changes.
            var scheduleRoutes = _dataContext.Routes
                .Where(route => route.Schedules.Id == scheduleId);
            _dataContext.Refresh(RefreshMode.StoreWins, scheduleRoutes);
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Create database command parameter with the specified name, value and type.
        /// </summary>
        /// <typeparam name="TValue">The type of the parameter value.</typeparam>
        /// <param name="name">The name of the parameter to be created.</param>
        /// <param name="value">The reference to the value of the parameter to be created.</param>
        /// <returns>A new instance of the database command parameter.</returns>
        private static SqlCeParameter _CreateParameter<TValue>(string name, TValue value)
        {
            var sqlType = default(SqlDbType);
            var knownType = TYPE_MAPPING.TryGetValue(typeof(TValue), out sqlType);
            Debug.Assert(knownType);

            var parameter = new SqlCeParameter
            {
                Value = value,
                ParameterName = name,
                SqlDbType = sqlType,
            };

            return parameter;
        }
        #endregion

        #region private methods

        private void _CreateProject(string projectConfigPath, CapacitiesInfo capacitiesInfo,
            OrderCustomPropertiesInfo orderCustomPropertiesInfo)
        {
            try
            {
                // TODO : add advanced error reporting
                _projectCfg = ProjectConfiguration.Load(projectConfigPath);
                _dataContext = DatabaseOpener.OpenDatabase(_projectCfg.DatabasePath);
                _dataContext.PostInit(capacitiesInfo, orderCustomPropertiesInfo);
                _CreateCollections();

                // TODO: DataObjectManager workaround
                _dataContext.SaveChangesCompleted += new SaveChangesCompletedEventHandler(_dataContext_SaveChangesCompleted);
                _dataContext.PostSavingChanges += new SavingChangesEventHandler(_dataContext_PostSavingChanges);
                this.DeletionCheckingService = new DeletionCheckingService(_dataContext);
                _isOpened = true;
            }
            catch(Exception)
            {
                _Clean();
                throw;
            }
        }

        /// <summary>
        /// Opens project. Project must be closed.
        /// </summary>
        /// <param name="projectConfigPath">Path to project configuration file.</param>
        private void _OpenProject(string projectConfigPath)
        {
            try
            {
                // TODO : add advanced error reporting
                _projectCfg = ProjectConfiguration.Load(projectConfigPath);
                _dataContext = DatabaseOpener.OpenDatabase(_projectCfg.DatabasePath);
                _CreateCollections();

                // If we are upgrading project, then we need to update breaks configuration.
                _UpdateBreaksConfig();

                // TODO: DataObjectManager workaround
                _dataContext.SaveChangesCompleted += new SaveChangesCompletedEventHandler(_dataContext_SaveChangesCompleted);
                _dataContext.PostSavingChanges += new SavingChangesEventHandler(_dataContext_PostSavingChanges);
                this.DeletionCheckingService = new DeletionCheckingService(_dataContext);
                _isOpened = true;
            }
            catch
            {
                _Clean();
                throw;
            }
        }

        // TODO: DataObjectManager workaround
        private void _dataContext_SaveChangesCompleted(object sender, SaveChangesCompletedEventArgs e)
        {
            if (SaveChangesCompleted != null)
                SaveChangesCompleted(this, e);
        }

        private void _dataContext_PostSavingChanges(object sender, SavingChangesEventArgs e)
        {
            if (SavingChanges != null)
                SavingChanges(this, e);
        }

        private void _CreateCollections()
        {
            Debug.Assert(_dataContext != null); // must be initialized

            // locations
            DataService<Location> locationDS = new DataService<Location>(
                _dataContext,
                "Locations",
                new SpecFields("Id", "Deleted"));

            _locations = new DataObjectOwnerCollection<Location, DataModel.Locations>(
                locationDS,
                false);
            _locations.Initialize(true, true);

            _locationManager = new GenericDataObjectManager<Location>(
                locationDS);

            // driver specialties
            _driverSpecialties = new DataObjectOwnerCollection<DriverSpecialty, DataModel.DriverSpecialties>(
                _dataContext,
                "DriverSpecialties",
                new SpecFields("Id", "Deleted"),
                false);
            _driverSpecialties.Initialize(true, true);

            // vehicle specialties
            _vehicleSpecialties = new DataObjectOwnerCollection<VehicleSpecialty, DataModel.VehicleSpecialties>(
                _dataContext,
                "VehicleSpecialties",
                new SpecFields("Id", "Deleted"),
                false);
            _vehicleSpecialties.Initialize(true, true);

            // mobile devices
            _mobileDevices = new DataObjectOwnerCollection<MobileDevice, DataModel.MobileDevices>(
                _dataContext,
                "MobileDevices",
                new SpecFields("Id", "Deleted"),
                false);
            _mobileDevices.Initialize(true, true);

            // drivers
            _drivers = new DataObjectOwnerCollection<Driver, DataModel.Drivers>(
                _dataContext,
                "Drivers",
                new SpecFields("Id", "Deleted"),
                false);
            _drivers.Initialize(true, true);

            // vehicles
            _vehicles = new DataObjectOwnerCollection<Vehicle, DataModel.Vehicles>(
                _dataContext,
                "Vehicles",
                new SpecFields("Id", "Deleted"),
                false);
            _vehicles.Initialize(true, true);

            // fuelTypes
            _fuelTypes = new DataObjectOwnerCollection<FuelType, DataModel.FuelTypes>(
                _dataContext,
                "FuelTypes",
                new SpecFields("Id", "Deleted"),
                false);
            _fuelTypes.Initialize(true, true);

            // route data service
            DataService<Route> routeDS = new DataService<Route>(_dataContext,
                "Routes",
                new SpecFields("Id"));

            // default routes
            _routesDefault = new RouteCollection(routeDS, true);

            var routesClause = Functional.MakeExpression((Routes route) => route.Default);
            _routesDefault.Initialize(routesClause, routesClause);

            _createScheduleScript = ResourceLoader.ReadFileAsString(
                CREATE_SCHEDULE_SCRIPT);
            _copyRoutesScript = ResourceLoader.ReadFileAsString(
                COPY_DEFAULT_ROUTES_FOR_SCHEDULE_SCRIPT);
            _copyRenewalLocationsScript = ResourceLoader.ReadFileAsString(
                COPY_RENEWAL_LOCATIONS_SCRIPT);

            // zones
            _zones = new DataObjectOwnerCollection<Zone, DataModel.Zones>(
                _dataContext,
                "Zones",
                new SpecFields("Id", "Deleted"),
                false);
            _zones.Initialize(true, true);

            _barrierManager = new BarrierManager(_dataContext, "Barriers", new SpecFields("Id"));
            _orderManager = new OrderManager(_dataContext, "Orders", new SpecFields("Id")); 
            _scheduleManager = new ScheduleManager(_dataContext, "Schedules", new SpecFields("Id"),
                routeDS);
        }

        /// <summary>
        /// Class need for detecting most popular break in project.
        /// </summary>
        private class MaxBreak
        {
            public int Count ;
            public TimeWindowBreak Break ;
        }

        /// <summary>
        /// If we upgrading from old version of project we need to update breaks settings.
        /// So project's Breaks Type will be set to TimeWindowBreak and as a default break
        /// will be used default routes most popular break.
        /// </summary>
        private void _UpdateBreaksConfig()
        {
            // Check that we need to create default breaks.
            if (_projectCfg.BreaksSettings.BreaksType == null && DefaultRoutes.Count != 0)
            {
                var breaks = new List<MaxBreak>();
                MaxBreak breakObj = null;

                // Analyze each route in project.
                foreach (Route route in DefaultRoutes)
                {
                    // Check that route has break and this is TimeWindowBreak.
                    if (route.Breaks.Count == 1 && route.Breaks[0] is TimeWindowBreak)
                    {
                        // Get route's break.
                        TimeWindowBreak routeBreak = route.Breaks[0] as TimeWindowBreak;

                        // If break isn't first, try find same break in list.
                        if (breaks.Count != 0)
                            breakObj = breaks.FirstOrDefault(x => x.Break.To == routeBreak.To &&
                                x.Break.From == routeBreak.From && x.Break.Duration == routeBreak.Duration);

                        // If there is same break in a list - just increase records count.
                        if (breakObj != null)
                            breakObj.Count++;
                        // If there is no such break - add new record to list.
                        else
                            breaks.Add(new MaxBreak { Count = 1, Break = routeBreak });
                    }
                }

                // Try to find most popular break in list.
                MaxBreak mostPopularBreak = breaks.FirstOrDefault
                    (x => x.Count == breaks.Max(br => br.Count));

                // Set default breaks type as timewindow break.
                _projectCfg.BreaksSettings.BreaksType = BreakType.TimeWindow;

                // If we found most popular break - set it as default, otherwise left default 
                // breaks collection empty.
                if (mostPopularBreak != null)
                {
                    _projectCfg.BreaksSettings.DefaultBreaks.Add(
                        mostPopularBreak.Break.Clone() as TimeWindowBreak);
                }
            }
        }
        private void _DisposeCollections()
        {
            if (null != _locations)
            {
                _locations.Dispose();
                _locations = null;
            }

            if (null != _driverSpecialties)
            {
                _driverSpecialties.Dispose();
                _driverSpecialties = null;
            }

            if (null != _vehicleSpecialties)
            {
                _vehicleSpecialties.Dispose();
                _vehicleSpecialties = null;
            }

            if (null != _mobileDevices)
            {
                _mobileDevices.Dispose();
                _mobileDevices = null;
            }

            if (null != _drivers)
            {
                _drivers.Dispose();
                _drivers = null;
            }

            if (null != _vehicles)
            {
                _vehicles.Dispose();
                _vehicles = null;
            }

            if (null != _fuelTypes)
            {
                _fuelTypes.Dispose();
                _fuelTypes = null;
            }

            if (null != _routesDefault)
            {
                _routesDefault.Dispose();
                _routesDefault = null;
            }

            if (null != _zones)
            {
                _zones.Dispose();
                _zones = null;
            }

            _orderManager = null;
            _barrierManager = null;
            _scheduleManager = null;
            _locationManager = null;
        }

        private void _Clean()
        {
            _DisposeCollections();
            if (_dataContext != null)
            {
                _dataContext.Dispose();
                _dataContext = null;
            }
            _projectCfg = null;
        }

        /// <summary>
        /// Saves all unsaved changes to the project.
        /// </summary>
        private void _Save()
        {
            _projectCfg.Save();

            // save DB changes
            _dataContext.SaveChanges();
        }

        /// <summary>
        /// Executes the specified database command performing common exception handling.
        /// </summary>
        /// <param name="command">The database command to be executed.</param>
        private void _ExecuteDatabaseCommand(Action command)
        {
            _ExecuteDatabaseCommand(command.ToFunc());
        }

        /// <summary>
        /// Executes the specified database command performing common exception handling.
        /// </summary>
        /// <typeparam name="TResult">The type of the command result.</typeparam>
        /// <param name="command">The database command to be executed.</param>
        /// <returns>The result of the command execution.</returns>
        private TResult _ExecuteDatabaseCommand<TResult>(Func<TResult> command)
        {
            if (!_isOpened)
            {
                throw new InvalidOperationException(Properties.Resources.ProjectIsNotOpened);
            }

            var result = default(TResult);
            try
            {
                result = command();
            }
            catch (Exception e)
            {
                if (_handler == null || !_handler.HandleException(e))
                {
                    throw;
                }
            }

            return result;
        }

        /// <summary>
        /// Executes the specified action inside transaction providing a new instance of the
        /// object context for the current project and handling project saving exceptions when
        /// necessary.
        /// </summary>
        /// <param name="body">The action to be performed within transaction.</param>
        private void _ExecuteWithTransaction(Action<Entities> body)
        {
            _ExecuteDatabaseCommand(() =>
            {
                using (var context = new Entities((EntityConnection)_dataContext.Connection))
                using (var transaction = new TransactionScope())
                {
                    body(context);

                    transaction.Complete();
                }
            });
        }

        /// <summary>
        /// Copies default routes to the schedule with the specified ID.
        /// </summary>
        /// <param name="context">The object context to be used for routes copying.</param>
        /// <param name="scheduleId">The primary key of the schedule to copy routes to.</param>
        /// <param name="plannedDate">The planned date of the schedule to copy routes to.</param>
        private void _LoadDefaultRoutesForSchedule(
            Entities context,
            Guid scheduleId,
            DateTime plannedDate)
        {
            // Copy default routes to the new schedule.
            // There is no way to pass an array as SqlCe parameter, so we just replace
            // all occurrences of the @defaultRoutes with it's value in the SQL script.
            var defaultRouteIds = this.DefaultRoutes
                .Where(route => route.Days.DoesDaySatisfy(plannedDate))
                .Select(route => string.Format(ID_FORMAT, route.Id))
                .ToList();
            if (!defaultRouteIds.Any())
            {
                // There are no default routes to copy.
                return;
            }

            var copyRoutesScript = _copyRoutesScript.Replace(
                DEFAULT_ROUTE_IDS_PARAMETER_NAME,
                string.Join(ID_SEPARATOR, defaultRouteIds));

            var scheduleIdParameter = _CreateParameter(
                SCHEDULE_ID_PARAMETER_NAME,
                scheduleId);
            var creationTimeParameter = _CreateParameter(
                CREATION_TIME_PARAMETER_NAME,
                DateTime.Now.Ticks);
            context.ExecuteStoreCommand(
                copyRoutesScript,
                creationTimeParameter,
                scheduleIdParameter);

            // Copy renewal locations for default routes to the new schedule.
            scheduleIdParameter = _CreateParameter(SCHEDULE_ID_PARAMETER_NAME, scheduleId);
            context.ExecuteStoreCommand(
                _copyRenewalLocationsScript,
                scheduleIdParameter);
        }
        #endregion

        #region private constants
        /// <summary>
        /// The name of the database script for creating new schedule.
        /// </summary>
        private const string CREATE_SCHEDULE_SCRIPT = "aldb_create_schedule.sql";

        /// <summary>
        /// The name of the database script for copying default routes to the specified schedule.
        /// </summary>
        private const string COPY_DEFAULT_ROUTES_FOR_SCHEDULE_SCRIPT =
            "aldb_copy_default_routes_for_schedule.sql";

        /// <summary>
        /// The name of the database script for copying renewal locations from default routes to
        /// routes of the specified schedule.
        /// </summary>
        private const string COPY_RENEWAL_LOCATIONS_SCRIPT =
            "aldb_update_renewal_locations.sql";

        /// <summary>
        /// The name of the parameter for passing IDs of default routes to database commands.
        /// </summary>
        private const string DEFAULT_ROUTE_IDS_PARAMETER_NAME = "@defaultRouteIds";

        /// <summary>
        /// The character to be used for separating multiple database IDs.
        /// </summary>
        private const string ID_SEPARATOR = ",";

        /// <summary>
        /// Format string to be used for converting database IDs into string representation.
        /// </summary>
        private const string ID_FORMAT = "'{0}'";

        /// <summary>
        /// The name of the parameter for passing ID of the owner schedule to database commands.
        /// </summary>
        private const string SCHEDULE_ID_PARAMETER_NAME = "scheduleId";

        /// <summary>
        /// The name of the parameter for passing time of the object creation to database commands.
        /// </summary>
        private const string CREATION_TIME_PARAMETER_NAME = "creationTime";

        /// <summary>
        /// The name of the parameter for passing planned date to database commands.
        /// </summary>
        private const string PLANNED_DATE_PARAMETER_NAME = "plannedDate";

        /// <summary>
        /// The name of the parameter for passing object name to database commands.
        /// </summary>
        private const string NAME_PARAMETER_NAME = "name";

        /// <summary>
        /// The name of the parameter for passing schedule type database commands.
        /// </summary>
        private const string SCHEDULE_TYPE_PARAMETER_NAME = "scheduleType";

        /// <summary>
        /// Maps CLR types into SQL DB ones.
        /// </summary>
        private static readonly Dictionary<Type, SqlDbType> TYPE_MAPPING =
            new Dictionary<Type, SqlDbType>
            {
                { typeof(Guid), SqlDbType.UniqueIdentifier },
                { typeof(long), SqlDbType.BigInt },
                { typeof(DateTime), SqlDbType.DateTime },
                { typeof(int), SqlDbType.Int },
                { typeof(string), SqlDbType.NVarChar },
            };
        #endregion

        #region private members

        /// <summary>
        /// Exception handler for Save method.
        /// </summary>
        private IProjectSaveExceptionHandler _handler;

        private bool _isOpened = false;
        private ProjectConfiguration _projectCfg;
        private DataObjectContext _dataContext;

        // data collections
        private DataObjectCollection<Location, DataModel.Locations> _locations;
        private DataObjectCollection<DriverSpecialty, DataModel.DriverSpecialties> _driverSpecialties;
        private DataObjectCollection<VehicleSpecialty, DataModel.VehicleSpecialties> _vehicleSpecialties;
        private DataObjectCollection<MobileDevice, DataModel.MobileDevices> _mobileDevices;
        private DataObjectCollection<Driver, DataModel.Drivers> _drivers;
        private DataObjectCollection<Vehicle, DataModel.Vehicles> _vehicles;
        private DataObjectCollection<FuelType, DataModel.FuelTypes> _fuelTypes;
        private DataObjectCollection<Route, DataModel.Routes> _routesDefault;
        private DataObjectCollection<Zone, DataModel.Zones> _zones;

        private OrderManager _orderManager;
        private BarrierManager _barrierManager;
        private ScheduleManager _scheduleManager;
        private GenericDataObjectManager<Location> _locationManager;

        /// <summary>
        /// Stores schedule creation SQL script.
        /// </summary>
        private string _createScheduleScript;

        /// <summary>
        /// Stores default routes copying SQL script.
        /// </summary>
        private string _copyRoutesScript;

        /// <summary>
        /// Stores renewal location copying SQL script.
        /// </summary>
        private string _copyRenewalLocationsScript;
        #endregion
    }
}
