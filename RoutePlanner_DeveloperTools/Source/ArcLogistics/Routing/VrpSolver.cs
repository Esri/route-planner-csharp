using System;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// SolverContext class.
    /// </summary>
    internal sealed class SolverContext
    {
        public Project Project { get; set; }

        /// <summary>
        /// Gets or sets an instance of the Vrp service factory.
        /// </summary>
        public VrpRestServiceFactory VrpServiceFactory { get; set; }
        public RestRouteService RouteService { get; set; }
        public NetworkDescription NetworkDescription { get; set; }
        public SolverSettings SolverSettings { get; set; }

        /// <summary>
        /// Gets or sets a reference to the VRP requests analyzer object.
        /// </summary>
        public IVrpRequestAnalyzer VrpRequestAnalyzer
        {
            get;
            set;
        }

        /// <summary>
        /// Region name.
        /// </summary>
        public string RegionName
        {
            get;
            set;
        }
    }

    /// <summary>
    /// VrpSolver class.
    /// REST implementation of IVrpSolver interface.
    /// </summary>
    internal sealed class VrpSolver : IVrpSolver
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes solver.
        /// </summary>
        /// <param name="settings">Current solve settings.</param>
        /// <param name="servers">Collection of ags servers.</param>
        /// <param name="solveServiceValidator">Object to perform service validation.</param>
        /// <param name="options">Options influencing services used by the application.</param>
        internal VrpSolver(
            SolveInfoWrap settings,
            ICollection<AgsServer> servers,
            ISolveServiceValidator solveServiceValidator,
            ServiceOptions options)
        {
            _solveServiceValidator = solveServiceValidator;
            _options = options;

            // init phase 1: set/validate configuration
            _InitSettings(settings, servers, _options);

            // Create discovery service.
            var factory = new DiscoveryServiceFactory();
            _discoveryService = factory.CreateService(settings, servers,
                solveServiceValidator);

            // init phase 2: create services
            try
            {
                // check servers state
                ServiceHelper.ValidateServerState(_vrpServer);
                ServiceHelper.ValidateServerState(_routeServer);
                _discoveryService.ValidateServerState();
            }
            catch (Exception e)
            {
                // do not fail here, will try to re-initialize later
                Logger.Error(e);
            }

            // attach events
            _asyncMgr.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(
                _asyncMgr_AsyncSolveCompleted);
        }

        #endregion constructors

        #region public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fired before the asynchronous solve operation is started.
        /// </summary>
        public event EventHandler<AsyncSolveStartingEventArgs> AsyncSolveStarting = delegate { };

        /// <summary>
        /// Raises when async. solve operation started.
        /// </summary>
        public event AsyncSolveStartedEventHandler AsyncSolveStarted;

        /// <summary>
        /// Raises when async. solve operation completed.
        /// </summary>
        public event AsyncSolveCompletedEventHandler AsyncSolveCompleted;

        #endregion public events

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Project Project
        {
            get
            {
                return _context.Project;
            }
            set
            {
                _asyncMgr.ShutdownWorkers();
                _asyncOperations.Clear();
                _context.Project = value;
            }
        }

        /// <summary>
        /// Gets network attributes and parameters.
        /// </summary>
        public NetworkDescription NetworkDescription
        {
            get
            {
                _ValidateSolverState();

                return _context.NetworkDescription;
            }
        }

        /// <summary>
        /// Gets solver configuration settings.
        /// </summary>
        public SolverSettings SolverSettings
        {
            get
            {
                _ValidateSolverState();

                return _context.SolverSettings;
            }
        }

        #endregion public properties

        #region IVrpSolver interface: Solve operations
        /// <summary>
        /// Asynchronously builds routes for specified schedule.
        /// </summary>
        /// <param name="schedule">Schedule object.</param>
        /// <param name="options">Solve options.</param>
        /// <param name="inputParams">Input parameters for build route operation.</param>
        /// <returns>
        /// Operation id.
        /// </returns>
        public Guid BuildRoutesAsync(Schedule schedule,
            SolveOptions options,
            BuildRoutesParameters inputParams)
        {
            BuildRoutesOperation operation = new BuildRoutesOperation(_context,
                schedule,
                options,
                inputParams);

            return _RunAsync(operation);
        }

        public Guid SequenceRoutesAsync(Schedule schedule,
            ICollection<Route> routesToSequence,
            SolveOptions options)
        {
            SequenceRoutesOperation operation = new SequenceRoutesOperation(
                _context,
                schedule,
                new SequenceRoutesParams(routesToSequence),
                options);

            return _RunAsync(operation);
        }

        public Guid UnassignOrdersAsync(Schedule schedule,
            ICollection<Order> ordersToUnassign,
            SolveOptions options)
        {
            UnassignOrdersOperation operation = new UnassignOrdersOperation(
                _context,
                schedule,
                new UnassignOrdersParams(ordersToUnassign),
                options);

            return _RunAsync(operation);
        }

        public Guid AssignOrdersAsync(Schedule schedule,
            ICollection<Order> ordersToAssign,
            ICollection<Route> targetRoutes,
            int? targetSequence,
            bool keepViolatedOrdersUnassigned,
            SolveOptions options)
        {
            AssignOrdersParams inputParams = new AssignOrdersParams(
                ordersToAssign,
                targetRoutes,
                targetSequence,
                keepViolatedOrdersUnassigned);

            AssignOrdersOperation operation = new AssignOrdersOperation(
                _context,
                schedule,
                inputParams,
                options);

            return _RunAsync(operation);
        }

        public Guid GenerateDirectionsAsync(ICollection<Route> routes)
        {
            GenDirectionsParams inputParams = new GenDirectionsParams();
            inputParams.Routes = routes;

            GenDirectionsOperation operation = new GenDirectionsOperation(
                _context,
                inputParams);

            return _RunAsync(operation);
        }

        #endregion IVrpSolver interface: solve operations

        #region IVrpSolver interface: Async. operations management
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a boolean value indicating if there are pending
        /// asynchronous operations.
        /// </summary>
        public bool HasPendingOperations
        {
            get { return _asyncMgr.HasPendingOperations; }
        }

        /// <summary>
        /// Cancels asynchronous operation.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <returns>
        /// true if operation is successfully cancelled, false if operation
        /// with specified id was not found.
        /// </returns>
        public bool CancelAsyncOperation(Guid operationId)
        {
            return _asyncMgr.CancelAsync(operationId);
        }

        /// <summary>
        /// Gets asynchronous operation info.
        /// </summary>
        /// <param name="operationId">Operation id.</param>
        /// <param name="info">AsyncOperationInfo object.</param>
        /// <returns>
        /// true if info object is set, false if operation with specified id
        /// was not found.
        /// </returns>
        public bool GetAsyncOperationInfo(Guid operationId,
            out AsyncOperationInfo info)
        {
            return _asyncOperations.TryGetValue(operationId, out info);
        }

        /// <summary>
        /// Gets asynchronous operations by date.
        /// </summary>
        /// <param name="date">Date to search operations.</param>
        /// <returns>
        /// List of found AsyncOperationInfo objects.
        /// </returns>
        public IList<AsyncOperationInfo> GetAsyncOperations(DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = date.Date.AddDays(1);

            var operationsInfo = _asyncOperations.Values;

            var list = new List<AsyncOperationInfo>();
            foreach (var info in operationsInfo)
            {
                if (info.Schedule == null)
                {
                    continue;
                }

                var plannedDate = info.Schedule.PlannedDate;
                if (plannedDate == null)
                {
                    continue;
                }

                if (dayStart <= plannedDate && plannedDate < dayEnd)
                {
                    list.Add(info);
                }
            }

            return list;
        }

        #endregion IVrpSolver interface: async. operations management

        #region private static methods

        /// <summary>
        /// Creates new instance of the <see cref="ESRI.ArcLogistics.Routing.IVrpRequestAnalyzer"/>
        /// object corresponding to the specified service options.
        /// </summary>
        /// <param name="options">The options specifying the analyzer to be used.</param>
        /// <param name="solverConfig">Solver config, containing info about max orders and routes
        /// count for synq request.</param>
        /// <returns>The reference to the new instance of the VRP requests analyzer.</returns>
        private static IVrpRequestAnalyzer _CreateRequestAnalyzer(ServiceOptions options, 
            SolveInfoWrap solverConfig)
        {
            // Check that sync vrp is allowed.
            if ((options & ServiceOptions.UseSyncVrp) != 0)
            {
                return new SimpleVrpRequestAnalyzer(solverConfig.MaxSyncVrpRequestRoutesCount, 
                    solverConfig.MaxSyncVrpRequestOrdersCount);
            }

            return new ConstantVrpRequestAnalyzer(false);
        }

        #endregion

        #region private methods
        /// <summary>
        /// Raises the <see cref="AsyncSolveStarting"/> event.
        /// </summary>
        /// <param name="e">The reference to the event arguments object.</param>
        private void _NotifyAsyncSolveStarting(AsyncSolveStartingEventArgs e)
        {
            Debug.Assert(e != null);

            this.AsyncSolveStarting(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:AsyncSolveStarted"/> event.
        /// </summary>
        /// <param name="operationID">The identifier of the started operation.</param>
        private void _NotifyAsyncSolveStarted(Guid operationID)
        {
            var temp = this.AsyncSolveStarted;
            if (temp != null)
            {
                temp(this, new AsyncSolveStartedEventArgs(operationID));
            }
        }

        private void _asyncMgr_AsyncSolveCompleted(object sender,
            AsyncSolveCompletedEventArgs e)
        {
            try
            {
                if (AsyncSolveCompleted != null)
                    AsyncSolveCompleted(this, e);
            }
            finally
            {
                _asyncOperations.Remove(e.OperationId);
            }
        }

        /// <summary>
        /// Validates states of a project, all used servers and services.
        /// </summary>
        private void _ValidateSolverState()
        {
            // check if project is set
            if (_context.Project == null)
                throw new InvalidOperationException(Properties.Messages.Error_ProjectIsNotSet);

            var schedule = _context.Project.Schedules.ActiveSchedule;

            if (schedule == null)
                throw new InvalidOperationException(Properties.Messages.Error_ScheduleNotSet);

            lock (_contextGuard)
            {
                // check servers state
                ServiceHelper.ValidateServerState(_vrpServer);
                ServiceHelper.ValidateServerState(_routeServer);

                _discoveryService.ValidateServerState();

                _FixRegionalRoutingToolsInfo(schedule);

                // check if services were created
                if (!_isContextInited)
                    _InitContext();
            }
        }

        /// <summary>
        /// Runs the specified solve operation asynchronously.
        /// </summary>
        /// <typeparam name="TSolveRequest">The type of the solve request for the operation.
        /// </typeparam>
        /// <param name="operation">The solve operation to run.</param>
        /// <returns>Identifier of the started operation.</returns>
        private Guid _RunAsync<TSolveRequest>(
            ISolveOperation<TSolveRequest> operation)
        {
            this._NotifyAsyncSolveStarting(new AsyncSolveStartingEventArgs(operation.Schedule));

            var solveTask = AsyncSolveTask.FromDelegate(tracker =>
            {
                _ValidateSolverState();

                var operationTask = AsyncSolveTask.FromSolveOperation(operation);

                return operationTask.Run(tracker);
            });

            var id = _asyncMgr.RunAsync(solveTask);

            var info = new AsyncOperationInfo
            {
                Id = id,
                InputParams = operation.InputParams,
                OperationType = operation.OperationType,
                Schedule = operation.Schedule,
            };

            _asyncOperations.Add(id, info);

            _NotifyAsyncSolveStarted(id);

            return id;
        }

        private void _InitSettings(
            SolveInfoWrap settings,
            ICollection<AgsServer> servers,
            ServiceOptions options)
        {
            Debug.Assert(settings != null);
            Debug.Assert(servers != null);

            // VRP data
            _InitVrpSettings(settings, servers, options);

            // route data
            _InitRouteSettings(settings, servers);

            // solver settings
            _solverConfig = settings;
        }

        /// <summary>
        /// Gets server with the specified name from the specified collection and
        /// throws an exception if the server was not found.
        /// </summary>
        /// <param name="servers">The reference to the servers collection to
        /// get server from.</param>
        /// <param name="serverName">The name of the server to get.</param>
        /// <returns>The reference to the <see cref="T:ESRI.ArcLogistics.Services.AgsServer"/>
        /// object with the specified name. If the server was not found a
        /// <see cref="T:System.ApplicationException"/> is thrown.</returns>
        /// <exception cref="T:System.ApplicationException">The server was not found.</exception>
        private AgsServer _GetServerByName(
            ICollection<AgsServer> servers,
            string serverName)
        {
            var server = ServiceHelper.FindServerByName(serverName, servers);
            if (server == null)
            {
                throw new ApplicationException(Properties.Messages.Error_InvalidRoutingConfig);
            }

            return server;
        }

        private void _InitVrpSettings(
            SolveInfoWrap settings,
            ICollection<AgsServer> servers,
            ServiceOptions options)
        {
            var service = settings.VrpService;
            _solveServiceValidator.Validate(service);
            _vrpServiceConfig = service;
            _vrpServer = _GetServerByName(servers, service.ServerName);

            if ((options & ServiceOptions.UseSyncVrp) != 0)
            {
                var syncService = settings.SyncVrpService;
                _solveServiceValidator.Validate(syncService);
                _syncVrpServiceConfig = syncService;
                _syncVrpServer = _GetServerByName(servers, syncService.ServerName);
            }
        }

        private void _InitRouteSettings(SolveInfoWrap settings,
            ICollection<AgsServer> servers)
        {
            var service = settings.RouteService;
            _solveServiceValidator.Validate(service);

            _routeServiceConfig = service;
            _routeServer = _GetServerByName(servers, service.ServerName);
        }

        /// <summary>
        /// Initializes solver context.
        /// </summary>
        private void _InitContext()
        {
            Debug.Assert(!_isContextInited); // init once

            _context.VrpRequestAnalyzer = _CreateRequestAnalyzer(_options, _solverConfig);

            // VRP services
            var syncContextProvider = _CreateSyncContextProvider(_options);
            var asyncContextProvider = new RestServiceContextProvider(
                _vrpServiceConfig.RestUrl,
                _vrpServiceConfig.ToolName,
                _vrpServer);
            _context.VrpServiceFactory = new VrpRestServiceFactory(
                syncContextProvider,
                asyncContextProvider,
                _vrpServiceConfig.SoapUrl);

            // route services
            _context.RouteService = new RestRouteService(
                _routeServiceConfig.RestUrl,
                _routeServiceConfig.LayerName,
                _routeServer);

            // Initialize discovery service.
            _discoveryService.Initialize();

            NetworkDescription netDesc = new NetworkDescription(
                _routeServiceConfig.SoapUrl,
                _routeServiceConfig.LayerName,
                _routeServer);

            _context.NetworkDescription = netDesc;

            _context.RegionName = _currentRegionName;

            // solver settings
            _context.SolverSettings = new SolverSettings(
                _solverConfig,
                netDesc);

            _isContextInited = true;
        }

        /// <summary>
        /// Creates new instance of the REST context provider for the synchronous
        /// VRP service.
        /// </summary>
        /// <param name="options">Options for VRP service to create context for.</param>
        /// <returns></returns>
        private IRestServiceContextProvider _CreateSyncContextProvider(ServiceOptions options)
        {
            if ((options & ServiceOptions.UseSyncVrp) == 0)
            {
                return new NullRestServiceContextProvider();
            }

            return new RestServiceContextProvider(
                _syncVrpServiceConfig.RestUrl,
                _syncVrpServiceConfig.ToolName,
                _syncVrpServer);
        }

        /// <summary>
        /// Fixes VRP tool names with correct regional info.
        /// </summary>
        /// <param name="schedule">Current schedule, which is used for routing operation.</param>
        private void _FixRegionalRoutingToolsInfo(Schedule schedule)
        {
            Debug.Assert(schedule != null);

            // Get point at which routes region will be detected.
            var currentRegionPoint = DiscoveryRequestBuilder.GetPointForRegionRequest(schedule.Routes);

            // If region point hasn't been changed - do nothing.
            if (_lastRegionPoint == currentRegionPoint)
                return;

            // Remember current point as last checked point.
            _lastRegionPoint = currentRegionPoint;

            // Detect current region name to create VRP service.
            var builder = new DiscoveryRequestBuilder();

            // Build discovery request.
            var reqData = new DiscoveryRequestData();
            reqData.RegionPoint = currentRegionPoint;
            reqData.MapExtent = _discoveryService.GetFullMapExtent(
                DiscoveryRequestBuilder.JsonTypes);
            var discoveryRequest = builder.BuildRequest(reqData);

            // Make discovery request to get current Region name.
            string regionName = _discoveryService.GetRegionName(discoveryRequest,
                    DiscoveryRequestBuilder.JsonTypes);

            // Do nothing in case region name is empty.
            if (string.IsNullOrEmpty(regionName))
                return;

            // If operation call for new region, we need to reinitialize context.
            if (regionName != _currentRegionName)
                _isContextInited = false;

            _currentRegionName = regionName;

            // Fix route service config with new region name.
            _routeServiceConfig.LayerName =
                _GetRegionalLayerName(_routeServiceConfig.LayerName, regionName);
        }

        /// <summary>
        /// Gets service layer name with included appropriate region name.
        /// </summary>
        /// <param name="layerName">Current layer name.</param>
        /// <param name="regionName">Region name to fill in.</param>
        /// <return>Service layer name with region name filled in.</returns>
        private string _GetRegionalLayerName(string layerName, string regionName)
        {
            Debug.Assert(!string.IsNullOrEmpty(layerName));
            Debug.Assert(!string.IsNullOrEmpty(regionName));

            // Find a place where separator between constant and dynamic parts is.
            int index = layerName.IndexOf(SERVICE_LAYER_NAME_SEPARATOR);

            // Can't create correct tool name if separator not found.
            if (index == -1)
                throw new ApplicationException(Properties.Messages.Error_InvalidRoutingConfig);

            // Get only constant part of tool name, since
            // it could be already concatenated with older region name.
            string constantPart = layerName.Substring(0, index + 1);

            // Return concatenation of constant part and new region name.
            return constantPart + regionName;
        }

        #endregion private methods

        #region private constants

        /// <summary>
        /// Separator between constant and dynamic parts of service Layer Name.
        /// </summary>
        private const string SERVICE_LAYER_NAME_SEPARATOR = "_";

        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // initialization data
        private VrpServiceInfo _vrpServiceConfig;
        private RouteServiceInfo _routeServiceConfig;
        private SolveInfoWrap _solverConfig;
        private bool _isContextInited = false;

        // servers
        private AgsServer _vrpServer;
        private AgsServer _routeServer;

        private SolverContext _context = new SolverContext();

        /// <summary>
        /// An object to be used for serializing context modifications.
        /// </summary>
        private object _contextGuard = new object();

        private AsyncOperationManager _asyncMgr = new AsyncOperationManager();

        /// <summary>
        /// Stores information for running solve operations.
        /// </summary>
        private Dictionary<Guid, AsyncOperationInfo> _asyncOperations =
            new Dictionary<Guid, AsyncOperationInfo>();

        /// <summary>
        /// The reference to the service validator object.
        /// </summary>
        private readonly ISolveServiceValidator _solveServiceValidator;

        /// <summary>
        /// The reference to the synchronous VRP service configuration object.
        /// </summary>
        private VrpServiceInfo _syncVrpServiceConfig;

        /// <summary>
        /// The reference to the synchronous VRP server object.
        /// </summary>
        private AgsServer _syncVrpServer;

        /// <summary>
        /// Stores current set of service options.
        /// </summary>
        private ServiceOptions _options;

        /// <summary>
        /// Discovery service to make requests for region information.
        /// </summary>
        private IDiscoveryService _discoveryService;

        /// <summary>
        /// Current region name.
        /// </summary>
        private string _currentRegionName = string.Empty;

        /// <summary>
        /// Last point for which region was detected.
        /// </summary>
        private Geometry.Point? _lastRegionPoint;

        #endregion private fields
    }
    
}
