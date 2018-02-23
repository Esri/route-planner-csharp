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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Commands.SendRoutesCommandHelpers;
using ESRI.ArcLogistics.App.Commands.TrackingCommandHelpers;
using ESRI.ArcLogistics.App.Commands.Utility;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Import;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.App.Reports;
using ESRI.ArcLogistics.App.Services;
using ESRI.ArcLogistics.App.Widgets;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Export;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Tracking;
using ESRI.ArcLogistics.Utility.CoreEx;
using Microsoft.Practices.Unity;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new instance on the <c>App</c> class.
        /// </summary>
        static App()
        {
        }

        #endregion // Constructors

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fires when project is loaded.
        /// </summary>
        public event EventHandler ProjectLoaded;

        /// <summary>
        /// Fires when current project starts closing.
        /// </summary>
        public event EventHandler ProjectClosing;

        /// <summary>
        /// Fires when current project is closed.
        /// </summary>
        public event EventHandler ProjectClosed;

        /// <summary>
        /// Fires when application loading is completed.
        /// </summary>
        public event EventHandler ApplicationInitialized;

        /// <summary>
        /// Fires when application's  date is changed in any calendar control.
        /// </summary>
        public event EventHandler CurrentDateChanged;

        /// <summary>
        /// Raises when tracker is initialized.
        /// </summary>
        public event EventHandler TrackerInitialized;

        #endregion // Public events

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current application instance.
        /// </summary>
        public static new App Current
        {
            get { return (App)Application.Current; }
        }

        /// <summary>
        /// Returns Main Window of the application.
        /// </summary>
        public new MainWindow MainWindow
        {
            get { return base.MainWindow as MainWindow; }
        }

        /// <summary>
        /// Current date that is used in Orders, Routes and Schedule pages.
        /// </summary>
        public DateTime CurrentDate
        {
            get { return _currentDate; }
            set
            {
                if (_currentDate != value)
                {
                    _currentDate = value;
                    _NotifyCurrentDateChanged();
                }
            }
        }

        /// <summary>
        /// Gets map object.
        /// </summary>
        public Map Map
        {
            get { return _services.Map; }
        }

        /// <summary>
        /// Gets geocoder object.
        /// </summary>
        public IGeocoder Geocoder
        {
            get { return _services.Geocoder; }
        }

        /// <summary>
        /// Gets reference to the streets geocoder object.
        /// </summary>
        public IGeocoder StreetsGeocoder
        {
            get { return _services.StreetsGeocoder; }
        }

        /// <summary>
        /// Name address storage object.
        /// </summary>
        internal NameAddressStorage NameAddressStorage
        {
            get
            {
                return _nameAddressStorage;
            }
        }

        /// <summary>
        /// Gets VRP solver.
        /// </summary>
        internal IVrpSolver Solver
        {
            get { return _services.Solver; }
        }


        /// <summary>
        /// Gets tracker.
        /// </summary>
        internal Tracker Tracker
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets collection of servers.
        /// </summary>
        internal ICollection<AgsServer> Servers
        {
            get { return _services.Servers; }
        }

        /// <summary>
        /// Gets the currently open project.
        /// </summary>
        public Project Project
        {
            get { return _project; }
        }

        /// <summary>
        /// Interface that facilitates adding messages to the message window.
        /// </summary>
        public IMessenger Messenger
        {
            get { return _messenger; }
        }

        /// <summary>
        /// Returns a collection of loaded extensions.
        /// </summary>
        /// <remarks>Collection is read-only.</remarks>
        public ICollection<IExtension> Extensions
        {
            get { return _extensions.AsReadOnly(); }
        }

        /// <summary>
        /// Returns application Command Manager.
        /// </summary>
        public AppCommands.CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        /// <summary>
        /// Gets reference to the routes workflow manager object.
        /// </summary>
        internal IRoutesWorkflowManager RoutesWorkflowManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets reference to the handler for tracking exceptions.
        /// </summary>
        internal IExceptionHandler WorkflowManagementExceptionHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets reference to the application container.
        /// </summary>
        public IUnityContainer Container
        {
            get
            {
                return _rootContainer;
            }
        }
        #endregion // Public properties

        #region Internal properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets geocoder object for internal use only.
        /// </summary>
        internal GeocoderBase InternalGeocoder
        {
            get { return _services.Geocoder; }
        }

        internal ProjectCatalog ProjectCatalog
        {
            get { return _projectCatalog; }
        }

        internal IUIManager UIManager
        {
            get { return _manager; }
        }

        //APIREV: make internal - A.Shipika
        public MapDisplay MapDisplay
        {
            get { return _mapDisplay; }
        }

        internal HistoryService HistoryService
        {
            get { return _historyService; }
        }

        internal HelpTopics HelpTopics
        {
            get { return _helpTopics; }
        }

        internal FuelTypesInfo DefaultFuelTypesInfo
        {
            get { return Defaults.Instance.FuelTypesInfo; }
        }

        internal CapacitiesInfo DefaultCapacitiesInfo
        {
            get { return Defaults.Instance.CapacitiesInfo; }
        }

        internal OrderCustomPropertiesInfo DefaultOrderCustomPropertiesInfo
        {
            get { return Defaults.Instance.OrderCustomPropertiesInfo; }
        }

        internal Exporter Exporter
        {
            get { return _exporter; }
        }

        internal ReportsGenerator ReportGenerator
        {
            get { return _reporter; }
        }

        internal ImportProfilesKeeper ImportProfilesKeeper
        {
            get { return _importProfilesKeeper; }
        }

        internal ICollection<ExtensionWidget> ExtensionWidgets
        {
            get { return _extWidgets.AsReadOnly(); }
        }

        internal PrinterSettingsStore PrinterSettingsStore
        {
            get { return _printerSettings; }
        }

        internal bool IsInitialized
        {
            get { return _isInitialized; }
        }


        /// <summary>
        /// Gets reference to the handler for licenser exceptions.
        /// </summary>
        internal IExceptionHandler LicenserExceptionHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a reference to the application license manager.
        /// </summary>
        internal ILicenseManager LicenseManager
        {
            get;
            private set;
        }
        #endregion // Internal properties

        #region Internal methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal void CloseCurProject()
        {
            if (_IsProjectOpen())
            {
                _project.Save();

                _OnProjectPreClose();

                _project.Close();
                _project = null;

                _OnProjectClosed();
            }
        }

        internal void NewProject(string name, string folderPath, string description)
        {
            // check if we can proceed
            _CheckCurProjectState();

            // save and close current project
            CloseCurProject();

            // create project
            _project = ProjectFactory.CreateProject(name, folderPath, description,
                this.DefaultCapacitiesInfo,
                this.DefaultOrderCustomPropertiesInfo,
                this.DefaultFuelTypesInfo,
                new AppSaveExceptionHandler());

            _projectCatalog.Refresh();
            _OnProjectLoaded();
        }

        internal void OpenProject(string projectConfigPath, bool doAutoArchive)
        {
            // check if we can proceed
            _CheckCurProjectState();

            // save and close current project
            CloseCurProject();

            // open project
            _project = _OpenProject(projectConfigPath, doAutoArchive);
            _OnProjectLoaded();
        }

        internal void SaveExportProfiles()
        {
            if (null != _exporter)
                _exporter.SaveProfiles(_ExportProfileFilePath());
        }

        /// <summary>
        /// Gets configuration of the currently opened project is such project exists.
        /// </summary>
        /// <returns>Configuration of the currently opened project - if project exists, otherwise - null.</returns>
        internal ProjectConfiguration GetCurrentProjectConfiguration()
        {
            // Get current project.
            Project currentProject = App.Current.Project;

            // Configuration of the current project.
            ProjectConfiguration currentProjectConfiguration = null;

            // If current project exists.
            if (currentProject != null)
            {
                currentProjectConfiguration = _GetProjectConfiguration(currentProject.Name);
            }
            // None of projects is opened.
            else
            {
                currentProjectConfiguration = null;
            }

            return currentProjectConfiguration;
        }

        /// <summary>
        /// Updates custom order properties info for given project in database and updates defaults.xml.
        /// </summary>
        /// <param name="propertiesInfo">Order custom properties info.</param>
        /// /// <param name="projectCfg">Project configuration.</param>
        internal void UpdateProjectCustomOrderPropertiesInfo(OrderCustomPropertiesInfo propertiesInfo,
                                                             ProjectConfiguration projectCfg)
        {
            Debug.Assert(propertiesInfo != null);
            Debug.Assert(projectCfg != null);

            // Check if project can be closed now.
            if (Solver.HasPendingOperations)
            {
                Messenger.AddWarning((string)FindResource("UpdateCustomOprderPropertiesMessageRoutingOperationsInProgress"));
            }
            else
            {
                // Update defaults.xml
                _UpdateDefaults(propertiesInfo);

                // Update custom order properties info for given project in database.
                _UpdateProjectCustomOrderPropertiesInfo(propertiesInfo, projectCfg);
            }
        }

        #endregion // Internal methods

        #region Internal Constants

        /// <summary>
        /// Name of the log folder.
        /// </summary>
        internal const string LOGS_FOLDER = "Logs";

        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits today date.
        /// </summary>
        private void _InitTodayDate()
        {
            // if application should be started from last saved page
            // set current date same last saved date else set as now date
            CurrentDate = (!Settings.Default.StartApplicationAtLastSavedPage || Settings.Default.LastSavedDate.Equals(default(DateTime)))?
                            DateTime.Now.Date : Settings.Default.LastSavedDate;
        }

        /// <summary>
        /// Raises when application date changed
        /// </summary>
        private void _NotifyCurrentDateChanged()
        {
            if (CurrentDateChanged != null)
                CurrentDateChanged(null, EventArgs.Empty);
        }

        private void App_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (Current.MainWindow != null)
            {
                if (!_mainWndEventsAttached)
                {
                    Current.MainWindow.ContentRendered += new EventHandler(MainWindow_ContentRendered);
                    Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
                    _mainWndEventsAttached = true;
                }
            }
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (_isInitialized)
            {
                // save project and related configuration
                _SaveCurProject();

                // save services configuration
                _SaveServicesConfig();
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Add resources from resx files.
            ResourcesLoader.LoadResourcesToDictionary
                (ESRI.ArcLogistics.App.Properties.Resources.ResourceManager, this.Resources);

            _CheckUserSettings();

            // Get logger settings
            Settings settings = Settings.Default;
            // calculate logs file path
            string logFileName = settings.LogFileName;
            int logFileSize = settings.LogFileSizeInKb;
            if (logFileName.Length == 0)
                throw new SettingsException((string)Application.Current.FindResource("LogFileCantBeEmpty"));

            string logPath = Path.Combine(DataFolder.Path, LOGS_FOLDER);
            string logFilePath = Path.Combine(logPath, logFileName);
            Logger.Initialize(logFilePath, logFileSize, settings.LogMinimalSeverity, settings.IsLogEnabled);
            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            _ShowSplahScreen();

            _LoadPlugIns();
            _LoadHelpSettings();

            // activating XCEED license
            //******** TODO: Insert XCEED Data Grid license below **************
            Xceed.Wpf.DataGrid.Licenser.LicenseKey = "###################";

            // uprgrade settings if necessary until main window begins loading
            // NOTE: we use custom settings provider without upgrade functionality
            //_UpgradeUserSettings();
        }

        /// <summary>
        /// Check user configuration settings file for corruptions.
        /// </summary>
        private void _CheckUserSettings()
        {
            try
            {
                // Try to initialize user config.
                AlSettingsProvider.SettingsStore store = new AlSettingsProvider.SettingsStore();
            }
            catch (SettingsException ex)
            {
                // If user config file is invalid log exception and remember warning string.
                if (ex.Source == AlSettingsProvider.SettingsStore.PathToConfig)
                {
                    // Log error.
                    Logger.Error(ex);
                    _initWarningMessages.Add((string)Application.Current.FindResource(
                        "UserConfigInvalidWarning"));

                    // Delete corrupted file.
                    File.Delete(AlSettingsProvider.SettingsStore.PathToConfig);
                }
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO: adjust the error message for relase version
            if (ESRI.ArcLogistics.App.Properties.Settings.Default.EnableCustomUnhandledExceptionHandling)
            {
                try
                {
                    Logger.Critical(e.Exception);
                }
                catch { }
                string failMessage = string.Format((string)Application.Current.FindResource("UnhandledExceptionMessage"), e.Exception.Source, e.Exception.TargetSite, e.Exception.Message);
                MessageBoxResult result = MessageBox.Show(MainWindow, failMessage, (string)Application.Current.FindResource("UnhandledExceptionMsgBoxTitle"), MessageBoxButton.OKCancel, MessageBoxImage.Error);

                e.Handled = true;

                if (result == MessageBoxResult.OK)
                    Shutdown();
            }
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e)
        {
            if (!_isInitialized && !_isInitializationFailed)
            {
                WorkingStatusHelper.SetReleased();

                // Reset Topmost flag so splash screen doesn't override other windows.
                Debug.Assert(_splash != null);
                _splash.Topmost = false;
                _splash.Owner = this.MainWindow;

                // Init application.
                _InitApplication(_progressReporter);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!_CanExitApp())
                e.Cancel = true;
        }

        /// <summary>
        /// Method shows splash screen.
        /// </summary>
        private void _ShowSplahScreen()
        {
            // Create progress indicator and progress reporter.
            var progressIndicator = new ProgressIndicator();
            _progressReporter = _CreateInitializationProgressReporter(
                progressIndicator);

            // Create and show splash screen.
            _splash = new InitializationProgressControl(progressIndicator);
            _splash.Show();
        }

        /// <summary>
        /// Show all app innitial warnings in Message Window.
        /// </summary>
        private void _ShowInitWarnings()
        {
            foreach (string str in _initWarningMessages)
                _messenger.AddWarning(str);
        }

        private void _InitApplication(IProgressReporter progressReporter)
        {
            Debug.Assert(progressReporter != null);

            try
            {
                var baseProxyStorage = new ApplicationSettingsGenericStorage<ProxySettings>(
                    _ => _.ProxySettings);
                var proxyStorage = new ApplicationProxySettingsStorage(baseProxyStorage);
                _rootContainer
                    .RegisterInstance<IGenericStorage<ProxySettings>>(
                        proxyStorage,
                        new ContainerControlledLifetimeManager())
                    .RegisterType<IHostNameValidator, HostNameValidator>()
                    .RegisterType<IProxyConfigurationService, ProxyConfigurationService>(
                        new ContainerControlledLifetimeManager());

                var proxyConfigurationService =
                    _rootContainer.Resolve<IProxyConfigurationService>();
                proxyConfigurationService.Update();

                _InitDefaults();

                _messenger = ((MainWindow)this.MainWindow).MessageWindow;
                _manager = (MainWindow)this.MainWindow;

                // Show warnings.
                _ShowInitWarnings();

                this.LicenserExceptionHandler = new LicenserExceptionHandler(this);

                this.WorkflowManagementExceptionHandler =
                    new WorkflowManagementExceptionHandler(this);

                // init common application printer settings
                _InitPrinterSettings();

                // activate ArcGIS license
                _ActivateLicense(proxyConfigurationService);

                progressReporter.Step();

                // init services
                _InitServices(progressReporter);

                progressReporter.Step();

                // load project configurations
                _LoadProjectCatalog();

                // If services hasnt been inited.
                if (!_services.ServicesInfoExist)
                {
                    MainWindow.MessageWindow.AddError
                        (ESRI.ArcLogistics.App.Properties.Resources.Error_RemoteConfigIsUnaccessible);

                    _splash.Close();

                    MainWindow.ShowMainPreferences();
                    return;
                }

                // init history service
                _InitHistoryService();

                // open/create project
                _OpenProjectOnStartup();

                // init current date
                _InitTodayDate();

                // init solver
                _InitSolver();

                // Map display config loading
                _mapDisplay = new MapDisplay();

                _InitRoutesWorkflowManager();

                progressReporter.Step();

                // load commands
                _LoadCommands();

                progressReporter.Step();

                // Create and init local geocoder.
                _InitNameAddressStorage();

                // init completed
                _NotifyApplicationInitialized();

                progressReporter.Step();

                _isInitialized = true;

                this.MainWindow.Start();
            }
            catch (Exception ex)
            {
                _HandleInitializationCriticalError(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;

                // Progress reporter and splash screen are no longer needed.
                _progressReporter = null;
                _splash = null;
            }
        }

        /// <summary>
        /// Init user's default settings.
        /// </summary>
        private void _InitDefaults()
        {
            string defaultsFilePath = Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_DEFAULTS);

            _initWarningMessages.Add("DEPRECATION NOTICE: Please Read. Official Esri contribution and support for this code project will cease on December 31st 2018. After this date, parts of this application may no longer function as the application is dependent on web services that will continue to evolve beyond that date.");

            try
            {
                // Try to init settings.
                Defaults.Initialize();
            }
            catch (SettingsException ex)
            {
                if (ex.Source == defaultsFilePath)
                {
                    // Log and show exception.
                    _initWarningMessages.Add((string)Application.Current.FindResource(
                        "DefaultsInvalidWarning"));
                    Logger.Error(ex);

                    // Delete file.
                    File.Delete(defaultsFilePath);

                    // Re-init settings.
                    Defaults.Initialize();
                }
                else
                    throw ex;
            }
        }

        private void _ActivateLicense(IProxyConfigurationService proxyConfigurationService)
        {
            Debug.Assert(proxyConfigurationService != null);

            var storage = new ApplicationSettingsLicenseCacheStorage();
            Licenser.Initialize(storage);

            this.LicenseManager = new LicenseManager(storage, proxyConfigurationService);

            try
            {
                this.LicenseManager.ActivateLicenseOnStartup();
            }
            catch (LicenseException e)
            {
                if (e.ErrorCode == LicenseError.LicenseComponentNotFound ||
                    e.ErrorCode == LicenseError.InvalidComponentSignature)
                {
                    // critical licensing error, cannot proceed
                    throw;
                }

                Logger.Warning(e);
            }
            catch (CommunicationException e)
            {
                Messenger.AddWarning(CommonHelpers.FormatCommunicationError(e.Message, e));
                Logger.Error(e);
            }
            catch (Exception e)
            {
                Messenger.AddWarning(
                    (string)Application.Current.FindResource("LicenseError"));

                Logger.Error(e);
            }
        }

        private void _InitPrinterSettings()
        {
            _printerSettings = new PrinterSettingsStore();
        }

        private void _InitServices(IProgressReporter progressReporter)
        {
            // Create Services catalog.
            _services = _CreateServicesCatalog(progressReporter);

            var proxyConfigurationService = this.Container.Resolve<IProxyConfigurationService>();
            _AuthenticateProxyServerIfNecessary(proxyConfigurationService, _services.Servers);

            var handler = Functional.MakeLambda(() =>
                ProxyServerAuthenticator.AskAndSetProxyCredentials(proxyConfigurationService));
            ProxyAuthenticationErrorHandler.SetupHandler(
                () => (bool)this.Dispatcher.Invoke(handler));

            _InitializeTracker();

            foreach (AgsServer server in _services.Servers)
            {
                string msg = null;
                if (server.State == AgsServerState.Unauthorized)
                {
                    if (server.AuthenticationType == AgsServerAuthenticationType.UseApplicationLicenseCredentials)
                    {
                        if (!this.LicenseManager.LicenseComponent.RequireAuthentication)
                        {
                            msg = string.Format((string)Application.Current.FindResource("ServerUseLicenseCredentialsError2"),
                                server.Title);
                        }
                        else if (this.LicenseManager.HaveStoredCredentials)
                        {
                            msg = string.Format((string)Application.Current.FindResource("ServerUseLicenseCredentialsError"),
                                server.Title);
                        }
                    }
                    else if (server.AuthenticationType == AgsServerAuthenticationType.No)
                    {
                        msg = string.Format((string)Application.Current.FindResource("ServerNeedCredentialsError"),
                            server.Title);
                    }
                    else if (server.HasCredentials)
                    {
                        msg = string.Format((string)Application.Current.FindResource("ServerAuthError"),
                            server.Title);
                    }
                }
                else if (server.State == AgsServerState.Unavailable)
                {
                    CommunicationException commEx = server.InitializationFailure as CommunicationException;
                    if (commEx != null)
                        msg = CommonHelpers.FormatServerCommunicationError(server.Title, commEx);
                    else
                    {
                        msg = string.Format((string)Application.Current.FindResource("ServerConnectionError"),
                            server.Title);
                    }
                }
                else if (server.State == AgsServerState.Authorized)
                {
                    if (server.AuthenticationType == AgsServerAuthenticationType.Yes &&
                        !(server.RequiresHttpAuthentication || server.RequiresTokens))
                    {
                        msg = string.Format((string)Application.Current.FindResource("ServerNotNeedCredentialsError"),
                            server.Title);
                    }
                }

                if (msg != null)
                    Messenger.AddWarning(msg);
            }
        }

        /// <summary>
        /// Initializes tracker component.
        /// </summary>
        /// <remarks>
        /// Expects VRP solver and geocoder to be already initialized.
        /// </remarks>
        private void _InitializeTracker()
        {
            const int maxRetryCount = 1;
            var retryCount = 0;
            var initializationError = default(Exception);
            while (true)
            {
                try
                {
                    if (_services.TrackerProvider != null)
                    {
                        this.Tracker = _services.TrackerProvider.GetTracker(
                            _services.Solver,
                            _services.Geocoder,
                            this.MainWindow.MessageWindow);

                        // If we have opened project - init tracker with it.
                        if (this.Project != null)
                        {
                            this.Tracker.Project = _project;
                            _NotifyTrackerInitialized();
                        }
                    }
                    break;
                }
                catch (Exception e)
                {
                    if (retryCount > maxRetryCount)
                    {
                        initializationError = e;
                        break;
                    }

                    var error = ServiceHelper.GetCommunicationError(e);
                    if (error != CommunicationError.ProxyAuthenticationRequired)
                    {
                        initializationError = e;
                        break;
                    }

                    ++retryCount;
                }
            }

            if (initializationError != null)
            {
                Logger.Warning(initializationError);
            }
        }

        /// <summary>
        /// Create services_user.xml.
        /// </summary>
        /// <param name="progressReporter">Progress reporter.</param>
        /// <returns>ServiceCatalog.</returns>
        private ServiceCatalog _CreateServicesCatalog(IProgressReporter progressReporter)
        {
            // Services config file path.
            string configPath = Path.Combine(DataFolder.Path,
                RELATIVE_FILEPATH_SERVICES);

            // Services user config file path.
            string userConfigPath = Path.Combine(DataFolder.Path,
                RELATIVE_FILEPATH_USER_SERVICES);

            var certificateValidator = new ConfigurableCertificateValidator();
            ServicePointManager.ServerCertificateValidationCallback =
                certificateValidator.ValidateRemoteCertificate;

             ServiceCatalog serviceCatalog = null;

            // Exceptions handler to catch exceptions in Service Catalog.
            var exceptionHandler =
                new ServiceConnectionExceptionHandler(this);

            // Try to load services.
            try
            {
                serviceCatalog = new ServiceCatalog(
                    progressReporter,
                    configPath,
                    userConfigPath,
                    certificateValidator,
                    exceptionHandler);
            }
            catch (SettingsException ex)
            {
                // Check that user services raises exception.
                if (ex.Source == userConfigPath)
                {
                    // Log and show exception.
                    _LogShowWarningMessage(ex, "ServicesUserInvalidWarning");

                    // Delete file.
                    File.Delete(userConfigPath);

                    // Recreate services.
                    serviceCatalog = new ServiceCatalog(
                        progressReporter,
                        configPath,
                        userConfigPath,
                        certificateValidator,
                        exceptionHandler);
                }
                else
                    throw;
            }
            catch (Exception ex)
            {
                _HandleInitializationCriticalError(ex);
            }

            // If we haven't authorized to feature server - subscribe to ags server state changed.
            if (serviceCatalog.TrackerProvider != null &&
                serviceCatalog.TrackerProvider.Server.State != AgsServerState.Authorized)
                serviceCatalog.TrackerProvider.Server.StateChanged += 
                    new EventHandler(_TrackerProviderAgsServerStateChanged);

            return serviceCatalog;
        }

        /// <summary>
        /// Occured when feature service server state has changed.
        /// </summary>
        /// <param name="sender">Ingored.</param>
        /// <param name="e">Ingored.</param>
        private void _TrackerProviderAgsServerStateChanged(object sender, EventArgs e)
        {
            // If we have authorized to server.
            if (_services.TrackerProvider.Server.State == AgsServerState.Authorized)
            {
                // We need to init tracker only once, so unsubscribe from server state changes.
                _services.TrackerProvider.Server.StateChanged -=
                    new EventHandler(_TrackerProviderAgsServerStateChanged);

                // Init tracker.
                _InitializeTracker();
            }
        }

        /// <summary>
        /// Checks servers status and tries to authenticate proxy server if necessary
        /// </summary>
        private void _AuthenticateProxyServerIfNecessary(
            IProxyConfigurationService proxyConfigurationService,
            ICollection<AgsServer> servers)
        {
            // check either some servers are not authenticated due to 407 "Proxy Requires Authentication" error
            bool isProxyError = _CheckServersOnProxyAuthError(_services.Servers);

            // repeat showing auth dialog until user either enters correct credentials or presses cancel
            while (isProxyError)
            {
                // ask user to enter credentials 
                if (!ProxyServerAuthenticator.AskAndSetProxyCredentials(proxyConfigurationService))
                    break; // break if user canceled dialog

                // if user entered credentials then reconnect servers with proxy error
                isProxyError = _ReconnectServersWithProxyError(servers);
            }
        }

        /// <summary>
        /// Method returns true if at least one of the input servers is unvailable due to 407 "Proxy Authentication Required" error
        /// </summary>
        private bool _CheckServersOnProxyAuthError(ICollection<AgsServer> servers)
        {
            bool isAuthError = false;
            foreach (AgsServer server in servers)
            {
                // if server has initialization failure and it is a communication exception
                if (server.InitializationFailure != null &&
                    server.InitializationFailure is CommunicationException)
                {
                    CommunicationException commError = (CommunicationException)server.InitializationFailure;

                    // if the exception has ProxyAuthenticationRequired code 
                    if (commError.ErrorCode == CommunicationError.ProxyAuthenticationRequired)
                    {
                        isAuthError = true;
                        break;
                    }
                }
            }

            return isAuthError;
        }

        /// <summary>
        /// Tries to reconnect servers with proxy error. If at least one server throws communication exception with the same error
        /// method return false, otherwise it returns true, that means that every such server is reconnected.
        /// </summary>
        /// <param name="servers"></param>
        /// <returns></returns>
        private bool _ReconnectServersWithProxyError(ICollection<AgsServer> servers)
        {
            bool isAuthError = false;

            foreach (AgsServer server in servers)
            {
                // if server state is unavailable, it has initialization failure and it is a communication exception
                if (server.State == AgsServerState.Unavailable &&
                    server.InitializationFailure != null &&
                    server.InitializationFailure is CommunicationException)
                {
                    CommunicationException commError = (CommunicationException)server.InitializationFailure;

                    // if the exception has ProxyAuthenticationRequired code 
                    if (commError.ErrorCode == CommunicationError.ProxyAuthenticationRequired)
                    {
                        try
                        {
                            // try to reconnect
                            server.Reconnect();
                        }
                        catch (CommunicationException ex)
                        {
                            if (ex.ErrorCode == CommunicationError.ProxyAuthenticationRequired)
                                isAuthError = true;
                        }
                        catch (Exception)
                        {
                            // we don't take care about other communication errors
                        }
                    }
                }

                // if we found at least one server with proxy error - break the cycle
                if (isAuthError)
                    break;
            }

            return isAuthError;
        }

        private void _NotifyApplicationInitialized()
        {
            if (ApplicationInitialized != null)
                ApplicationInitialized(this, EventArgs.Empty);
        }

        private void _NotifyProjectLoaded()
        {
            if (ProjectLoaded != null)
                ProjectLoaded(null, EventArgs.Empty);
        }

        private void _NotifyProjectClosed()
        {
            if (ProjectClosed != null)
                ProjectClosed(null, EventArgs.Empty);
        }

        private void _NotifyProjectPreClose()
        {
            if (ProjectClosing != null)
                ProjectClosing(null, EventArgs.Empty);
        }

        private void _NotifyTrackerInitialized()
        {
            if (TrackerInitialized != null)
                TrackerInitialized(this, EventArgs.Empty);
        }

        private void _OnProjectLoaded()
        {
            if (_services.Solver != null)
                _services.Solver.Project = _project;
            if (this.Tracker != null)
            {
                this.Tracker.Project = _project;
                if (this.Tracker.InitError != null)
                    _ShowTrackerError(this.Tracker.InitError);
                else
                    _NotifyTrackerInitialized();
            }

            // load import profiles - NOTE: call after _LoadDefaults() and _InitServices()
            _LoadImportProfiles();
            // init export system - NOTE: call after _LoadDefaults() and _InitServices()
            _InitExports();
            // init report system
            _InitReports();

            if (_isInitialized)
                CommonHelpers.StartFleetSetupWizard();

            _NotifyProjectLoaded();
        }

        private void _ShowTrackerError(Exception ex)
        {
            Debug.Assert(ex != null);

            Logger.Warning(ex);

            // TODO: other errors can be treated as internal, show some common text for them
            if (ex is ApplicationException ||
                ex is AuthenticationException ||
                ex is CommunicationException)
            {
                var message = (string)Application.Current.FindResource(
                    "TrackingServiceConnectionError");

                Messenger.AddWarning(message);
            }
        }

        private void _OnProjectPreClose()
        {
            _NotifyProjectPreClose();
        }

        private void _OnProjectClosed()
        {
            if (_services.Solver != null)
                _services.Solver.Project = null;

            _reporter = null;
            _exporter = null;
            _importProfilesKeeper = null;

            _NotifyProjectClosed();
        }

        /// <summary>
        /// Open project on startup
        /// </summary>
        private void _OpenProjectOnStartup()
        {
            // If there is no projectwcatalog - return.
            if (_projectCatalog == null)
                return;

            try
            {
                Project project = null;

                Settings settings = Settings.Default;

                // if there are no projects in the project catalog - create the default one and open it
                if (_projectCatalog.Projects.Count == 0)
                {
                    project = _CreateDefaultProject();
                }
                else if (!string.IsNullOrEmpty(settings.LastProjectName)) 
                {
                    project = _OpenLastOpenedProject(settings.LastProjectName);
                }
                else
                {
                    // Look for default project. If it is absent - create it.
                    bool defaultProjectExists = false;
                    string defaultName = (string)this.FindResource("DefaultProjectName");
                    foreach (ProjectConfiguration config in _projectCatalog.Projects)
                    {
                        if (config.Name.Equals(defaultName))
                        {
                            defaultProjectExists = true;
                            break;
                        }
                    }

                    if (defaultProjectExists)
                    {
                        // otherwise we have some project(s) in the catalog but the last project setting is empty
                        throw new ApplicationException((string)Application.Current.FindResource("SelectExistingProjectMessage"));
                    }
                    else
                    {
                        project = _CreateDefaultProject();
                    }
                }

                _project = project;
                if (_project != null)
                {
                    _projectCatalog.Refresh();
                    _OnProjectLoaded();
                }
            }
            catch (ApplicationException e)
            {
                Messenger.AddWarning(e.Message);
            }
        }

        /// <summary>
        /// Create default project
        /// </summary>
        /// <returns>Created project</returns>
        private Project _CreateDefaultProject()
        {
            Project project = null;

            try
            {
                // try to create default project
                project = ProjectFactory.CreateProject((string)this.FindResource("DefaultProjectName"),
                    ProjectCatalog.FolderPath,
                    (string)this.FindResource("DefaultProjectDescription"),
                    this.DefaultCapacitiesInfo,
                    this.DefaultOrderCustomPropertiesInfo,
                    this.DefaultFuelTypesInfo,
                    new AppSaveExceptionHandler());

                _projectCatalog.Refresh();
            }
            catch (ApplicationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new ApplicationException((string)Application.Current.FindResource("UnableCreateProjectMessage"));
            }

            return project;
        }

        private Project _OpenProject(string projectPath, bool doAutoArchive)
        {
            Debug.Assert(!string.IsNullOrEmpty(projectPath));

            if (doAutoArchive)
            {   // auto-archive project if necessary
                AutoArchiveProjectCmd cmd = new AutoArchiveProjectCmd();
                cmd.Initialize(this);
                cmd.Execute(projectPath);
            }

            return ProjectFactory.OpenProject(projectPath, new AppSaveExceptionHandler());
        }

        /// <summary>
        /// Open project, that was opened last time
        /// </summary>
        /// <param name="lastProjectName">Name of project, that was opened last time</param>
        /// <returns>Opened project</returns>
        private Project _OpenLastOpenedProject(string lastProjectName)
        {
            Project project = null;

            // else check either some project was opened before
            try
            {
                string projectPath = Path.Combine(ProjectCatalog.FolderPath, lastProjectName);
                // try to open last saved project
                project = _OpenProject(projectPath, true);
            }
            catch (DataException e)
            {
                if (e.ErrorCode == DataError.FileSharingViolation)
                    Messenger.AddWarning((string)App.Current.FindResource("UnableOpenAlreadyOpenedProject"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new ApplicationException(string.Format((string)Application.Current.FindResource("OpenLastProjectFailedMessage"), lastProjectName));
            }

            return project;
        }

        private void _SaveCurProject()
        {
            if (_IsProjectOpen())
            {
                try
                {
                    // save configuration
                    Settings settings = Settings.Default;
                    if (!settings.LastProjectName.Equals(Path.GetFileName(_project.Path)))
                    {
                        settings.LastProjectName = Path.GetFileName(_project.Path);
                        settings.Save();
                    }
                    // save and close project
                    CloseCurProject();
                }
                catch (Exception ex)
                {
                    Logger.Info(ex);
                }
            }
        }

        private void _CheckCurProjectState()
        {
            if (_IsProjectOpen())
            {
                // check if we have pending async. operations
                Debug.Assert(this.Solver != null);
                if (this.Solver.HasPendingOperations)
                {
                    throw new ApplicationException(
                        (string)Application.Current.FindResource("ProjectInProgressOperations"));
                }
            }
        }

        private bool _IsProjectOpen()
        {
            return (_project != null && _project.IsOpened);
        }

        private void _SaveServicesConfig()
        {
            if (_services != null)
            {
                try
                {
                    _services.Save();
                }
                catch (Exception ex)
                {
                    Logger.Info(ex);
                }
            }
        }

        #region Plug-Ins helpers

        private void _LoadPlugInByInterface<T>(string typeName, Type pluginType, ref List<T> collection)
        {
            Debug.Assert(!string.IsNullOrEmpty(typeName));

            Type typeInterface = pluginType.GetInterface(typeName, true);
            if (null != typeInterface)
            {   // make sure the interface we want to use actually exists
                try
                {
                    // create a new instance and store the instance in the collection for later use
                    T instance = (T)Activator.CreateInstance(pluginType);

                    // add the new plugin to our collection here
                    collection.Add(instance);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        private void _InitExtensionalWidgets(Assembly pluginAssembly, Type pluginType, ref List<ExtensionWidget> collection)
        {
            if (typeof(ESRI.ArcLogistics.App.Widgets.PageWidget) == pluginType.BaseType)
            {
                if (Attribute.IsDefined(pluginType, typeof(WidgetPlugInAttribute)))
                {
                    WidgetPlugInAttribute attribute = (WidgetPlugInAttribute)Attribute.GetCustomAttribute(pluginType, typeof(WidgetPlugInAttribute));
                    foreach (string pagePath in attribute.PagePaths)
                    {
                        ExtensionWidget widget = new ExtensionWidget();
                        widget.AssemblyPath = pluginAssembly.Location;
                        widget.ClassType = pluginType;
                        widget.PagePath = pagePath;
                        collection.Add(widget);
                    }
                }
            }
        }

        private void _LoadPlugIns()
        {
            Debug.Assert(null == _extensions);

            List<IExtension> extensionsList = new List<IExtension>();
            _extWidgets = new List<ExtensionWidget> ();

            ICollection<string> assemblyFiles = CommonHelpers.GetAssembliesFiles();
            foreach (string assemblyPath in assemblyFiles)
            {
                // create a new assembly from the plugin file we're adding...
                Assembly pluginAssembly = Assembly.LoadFrom(assemblyPath);

                try
                {
                    // next we'll loop through all the Types found in the assembly
                    foreach (Type pluginType in pluginAssembly.GetTypes())
                    {
                        if (!pluginType.IsPublic || pluginType.IsAbstract)
                            continue; // NOTE: only look at public and non-abstract types

                        _LoadPlugInByInterface(typeof(IExtension).ToString(), pluginType, ref extensionsList);
                        _InitExtensionalWidgets(pluginAssembly, pluginType, ref _extWidgets);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }

            // initialize of extensions
            _extensions = new List<IExtension> ();
            foreach (IExtension extension in extensionsList)
            {
                try
                {
                    extension.Initialize(this);
                    _extensions.Add(extension);
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
        }

        #endregion // Plug-Ins helpers

        private void _LoadHelpSettings()
        {
            string helpSettingsFilePath = Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_HELP);

            _helpTopics = HelpFile.Load(helpSettingsFilePath);
        }

        /// <summary>
        /// Load user import setting.
        /// </summary>
        private void _LoadImportProfiles()
        {
            // Import file path.
            string importFilePath = Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_IMPORT);

            // Try to load import settingss.
            try
            {
                
                _importProfilesKeeper = new ImportProfilesKeeper(importFilePath);
            }
            catch (SettingsException ex)
            {
                // If import file is corrupted.
                if (ex.Source == importFilePath)
                {
                    // Log error.
                    _LogShowWarningMessage(ex, "ImportsUserInvalidWarning");

                    // Delete file.
                    File.Delete(importFilePath);

                    _importProfilesKeeper = new ImportProfilesKeeper(importFilePath);
                }
                else
                    throw;
            }
        }

        /// <summary>
        /// Load user export setting.
        /// </summary>
        private void _InitExports()
        {
            _exporter = new Exporter(Project.CapacitiesInfo, Project.OrderCustomPropertiesInfo,
                Geocoder.AddressFields);

            string exportProfilesPath = _ExportProfileFilePath();

            // Try to load export file.
            try
            {
                _exporter.LoadProfiles(exportProfilesPath);
            }
            catch (SettingsException ex)
            {
                // If export file is corrupted.
                if (ex.Source == exportProfilesPath)
                {
                    // Log error.
                    _LogShowWarningMessage(ex, "ExportUserInvalidWarning");

                    // Delete file
                    File.Delete(exportProfilesPath);

                    _exporter.LoadProfiles(exportProfilesPath);
                }
                else
                    throw;
            }
        }

        /// <summary>
        /// Load user reports.
        /// </summary>
        private void _InitReports()
        {
            // Report config paths.
            string reportsFilePath = Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_REPORT);
            string reportsFilePathUser = Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_REPORT_USER);

            // Try to load reports.
            try
            {
                ReportsFile reports = new ReportsFile(reportsFilePath, reportsFilePathUser);
                _reporter = new ReportsGenerator(Exporter, reports);
            }
            catch (SettingsException ex)
            {
                // If user reports are corrupted.
                if (ex.Source == reportsFilePathUser)
                {
                    // Log error.
                    _LogShowWarningMessage(ex, "ReportsUserInvalidWarning");

                    // Delete file
                    File.Delete(reportsFilePathUser);

                    ReportsFile reports = new ReportsFile(reportsFilePath,
                        reportsFilePathUser);
                    _reporter = new ReportsGenerator(Exporter, reports);
                }
                else
                    throw;
            }
        }

        /// <summary>
        /// Log Exception, show message in MessageWindow
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        /// <param name="message">Message to show.</param>
        private void _LogShowWarningMessage(Exception ex, string message)
        {
            Logger.Error(ex);
            App.Current.Messenger.AddWarning((string)Application.Current.FindResource(message));
        }

        private void _LoadProjectCatalog()
        {
            string projectFolderPath = (string.IsNullOrEmpty(Settings.Default.UsersProjectsFolder))?
                                                Path.Combine(DataFolder.Path, PROJECTS_FOLDER) : Settings.Default.UsersProjectsFolder;
            //if (!Directory.Exists(projectCatalogFolderPath))
            //    Directory.CreateDirectory(projectCatalogFolderPath);

            //// currently we use default project folder
            //_projectCatalog = new ProjectCatalog(projectCatalogFolderPath);

            // Flag show - can we create project workspace.
            bool canCreateProjectWorkspace = true;

            try
            {
                // Check existing of the folder if it isnt exist - try to create it.
                if (!Directory.Exists(projectFolderPath))
                    Directory.CreateDirectory(projectFolderPath);
            }
            catch (Exception ex)
            {
                if (ex is IOException || ex is UnauthorizedAccessException)
                {
                    // Log exception.
                    Logger.Error(ex);

                    // We cannot create projectworkspace.
                    canCreateProjectWorkspace = false;
                }
            }

            // Check that we have access rights to the folder.
            if (canCreateProjectWorkspace && !FileHelpers.CheckWriteAccess(projectFolderPath))
            {
                // Log exception.
                Logger.Error((string)Application.Current.FindResource("WriteAccessDenied"));

                // We cannot create projectworkspace.
                canCreateProjectWorkspace = false;
            }

            if (canCreateProjectWorkspace)
                _projectCatalog = new ProjectCatalog(projectFolderPath);
            else
            {
                // Show error message.
                var link = new Link(App.Current.FindString("PreferencePanelLinkText"),
                    Pages.PagePaths.GeneralPreferencesPagePath, LinkType.Page);
                string messageFormatString = (string)Application.Current.
                    FindResource("ProjectFolderInaccessible");
                string message = string.Format(messageFormatString, projectFolderPath);
                message += (string)Application.Current.
                    FindResource("ProjectFolderInaccessibleDetail");

                App.Current.Messenger.AddMessage(MessageType.Error, message, link);
            }

        }

        private void _LoadCommands()
        {
            _commandManager = new AppCommands.CommandManager();
        }

        /// <summary>
        /// Create and init name address storage.
        /// </summary>
        private void _InitNameAddressStorage()
        {
            string nameAddressDBPath = Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_NAMEADDRESS_DB);

            _nameAddressStorage = new NameAddressStorage(nameAddressDBPath);
        }

        private void _InitHistoryService()
        {
            string historyDBPath = Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_HISTORY_DB);

            _historyService = new HistoryService(historyDBPath);
        }

        /// <summary>
        /// Upgrades user settings from previous version if necessary
        /// </summary>
        private void _UpgradeUserSettings()
        {
            if (Settings.Default.IsFirstTimeRun)
            {
                // first time run -> need to upgrade
                Settings.Default.Upgrade();
                Settings.Default.IsFirstTimeRun = false;
                Settings.Default.Save();
            }
        }

        private void _InitSolver()
        {
            Debug.Assert(_services != null);

            if (_project != null)
                _services.Solver.Project = _project;
        }

        /// <summary>
        /// Initializes routes workflow manager instance.
        /// </summary>
        private void _InitRoutesWorkflowManager()
        {
            var dateTimeProvider = new ApplicationCurrentDateProvider(this);
            var solveStateTracker = new SolveStateTrackingService(
                this.Solver,
                dateTimeProvider);
            var workflowManagementStateTracker = new TrackingCommandStateService(this);
            var optimizeAndEditPage = new DelayedOptimizeAndEditPage(this);

            var routesSender = new SendRoutesCommandHelper(this.Resources);
            var sendRoutesTask = new SendRoutesTask(
                workflowManagementStateTracker,
                solveStateTracker,
                optimizeAndEditPage,
                dateTimeProvider,
                routesSender);

            this.RoutesWorkflowManager = new RoutesWorkflowManager(
                sendRoutesTask
                );
        }

        private bool _CanExitApp()
        {
            bool canExit = true;
            if (_isInitialized)
            {
                // check if we have pending async. operations
                Debug.Assert(this.Solver != null);
                if (this.Solver.HasPendingOperations)
                {
                    MessageBoxResult res = MessageBox.Show(
                        (string)Application.Current.FindResource("InProgressOperations"),
                        (string)Application.Current.FindResource("WarningMessageBoxTitle"),
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Warning);

                    canExit = (res == MessageBoxResult.OK);
                }
            }

            return canExit;
        }

        private string _ExportProfileFilePath()
        {
            return Path.Combine(DataFolder.Path, RELATIVE_FILEPATH_EXPORT);
        }

        /// <summary>
        /// Handle critical excepitons, which happens during app initialization.
        /// Log error, show message box, and shutdown application.
        /// </summary>
        /// <param name="ex">The critical exception occured.</param>
        private void _HandleInitializationCriticalError(Exception ex)
        {
            // Check that this method was called during application initialization.
            // Otherwise - show assert message and do nothing.
            if (_isInitialized)
            {
                Debug.Assert(false, "Application was inited.");
                return;
            }


            // If splash screen is visible we have to close it, 
            // so user can see unexpected error message.
            if (_splash != null && _splash.IsVisible)
                _splash.Close();

            Logger.Critical(ex);

            // We need this flag since Shutdown doesn't immediatly close the application.
            _isInitializationFailed = true;

            // Show message box, with exception info.
            _ShowInitError(ex);

            Shutdown();
        }

        /// <summary>
        /// Displays information about exception occured during application
        /// initialization.
        /// </summary>
        /// <param name="ex">The exception to display information for.</param>
        private void _ShowInitError(Exception ex)
        {
            Debug.Assert(ex != null);

            if (ex is AggregateException)
            {
                var aggregateException = (AggregateException)ex;
                foreach (var inner in aggregateException.InnerExceptions)
                {
                    _ShowInitError(inner);
                }

                // if there are no inner exceptions then we should show
                // information about the aggregate exception itself at least.
                if (aggregateException.InnerExceptions.Any())
                {
                    return;
                }
            }

            string caption = null;
            string message = null;
            MessageBoxImage icon = MessageBoxImage.Warning;
            if (ex is LicenseException)
            {
                caption = (string)Application.Current.FindResource("LicenseErrorTitle");
                message = ex.Message;
                icon = MessageBoxImage.Warning;
            }
            else
            {
                // unrecoverable fault: one of the application-level components cannot be initialized
                caption = (string)Application.Current.FindResource("ApplicationFailDialogTitle");
                message = string.Format((string)Application.Current.FindResource("ApplicationInitFault"), ex.Message);
                icon = MessageBoxImage.Error;
            }

            if (null == MainWindow)
                MessageBox.Show(message, caption, MessageBoxButton.OK, icon);
            else
                MessageBox.Show(MainWindow, message, caption, MessageBoxButton.OK, icon);
        }

        /// <summary>
        /// Creates progress reporter instance to be used for reporting application
        /// initialization progress.
        /// </summary>
        /// <param name="progressIndicator">The instance of the progress
        /// indicator to be used for reporting progress.</param>
        /// <returns></returns>
        private IProgressReporter _CreateInitializationProgressReporter(
            IProgressIndicator progressIndicator)
        {
            Debug.Assert(progressIndicator != null);

            var statuses = new string[]
            {
                this.FindString(ACTIVATING_LICENSE_STEP_KEY),
                this.FindString(CONTACTING_SERVERS_STEP_KEY),
                this.FindString(INITIALIZING_SERVICES_STEP_KEY),
                this.FindString(LOADING_PROJECT_STEP_KEY),
                this.FindString(LOADING_COMMANDS_STEP_KEY),
                this.FindString(PREPARING_UI_STEP_KEY),
            };

            var reporter = new ProgressReporter(statuses, progressIndicator);

            return reporter;
        }

        /// <summary>
        /// Gets project configuration for project with given name.
        /// </summary>
        /// <param name="projectName">Name of a project.</param>
        /// <returns>Project configuration.</returns>
        private ProjectConfiguration _GetProjectConfiguration(string projectName)
        {
            ProjectConfiguration projectConfig = null;

            // Look for configuration data of project with given name in projects' catalog.
            foreach (ProjectConfiguration config in _projectCatalog.Projects)
            {
                if (config.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase))
                {
                    projectConfig = config;
                    break;
                }
            }

            return projectConfig;
        }

        /// <summary>
        /// Updates order custom properties info in defaults.xml.
        /// </summary>
        /// <param name="propertiesInfo">Order custom properties info.</param>
        private void _UpdateDefaults(OrderCustomPropertiesInfo propertiesInfo)
        {
            Debug.Assert(propertiesInfo != null);

            try
            {
                // Update defaults.
                Defaults.Instance.OrderCustomPropertiesInfo = propertiesInfo;
                Defaults.Instance.Save();
            }
            catch (SettingsException ex)
            {
                Messenger.AddWarning((string)FindResource("DefaultsUpdateError"));

                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Updates custom order properties info for given project.
        /// </summary>
        /// <param name="propertiesInfo">Custom order properties info.</param>
        /// <param name="projectCfg">Project configuration.</param>
        private void _UpdateProjectCustomOrderPropertiesInfo(OrderCustomPropertiesInfo propertiesInfo,
                                                             ProjectConfiguration projectCfg)
        {
            Debug.Assert(propertiesInfo != null);
            Debug.Assert(projectCfg != null);

            // Flag defines if current project exists.
            bool currentProjectNeedsUpdate =
                App.Current.Project != null && App.Current.Project.Path == projectCfg.FilePath;

            try
            {
                // Close current project if it is opened.
                if (currentProjectNeedsUpdate)
                    CloseCurProject();

                // Update database of project.
                ProjectFactory.UpdateProjectCustomOrderPropertiesInfo(projectCfg, propertiesInfo);
            }
            catch (DataException ex)
            {
                Messenger.AddWarning((string)FindResource("CustomOrderPropertiesUpdateError"));

                Logger.Error(ex);
            }
            finally
            {
                // Open project again (if it was opened before) using updated database.
                if (currentProjectNeedsUpdate)
                    OpenProject(projectCfg.FilePath, true);
            }
        }

        #endregion // Private methods

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Progress reported used in _InitApplication method to report about current progress.
        /// After initialization it is set to null.
        /// </summary>
        private IProgressReporter _progressReporter = null;

        /// <summary>
        /// Splash screen.
        /// </summary>
        InitializationProgressControl _splash = null;

        private ServiceCatalog _services;
        private ProjectCatalog _projectCatalog = null;
        private Project _project = null;
        private HistoryService _historyService = null;
        private AppCommands.CommandManager _commandManager = null;
        private DateTime _currentDate;
        private MapDisplay _mapDisplay;
        private Exporter _exporter = null;
        private ImportProfilesKeeper _importProfilesKeeper = null;
        private ReportsGenerator _reporter = null;
        private IMessenger _messenger = null;
        private IUIManager _manager = null;
        private HelpTopics _helpTopics = null;
        private List<IExtension> _extensions = null;
        private List<ExtensionWidget> _extWidgets = null;
        private PrinterSettingsStore _printerSettings = null;
        private NameAddressStorage _nameAddressStorage;

        /// <summary>
        /// List of warnings that occured before MessageWindow inited.
        /// </summary>
        private List<string> _initWarningMessages = new List<string>();
        private bool _isInitialized = false;
        private bool _isInitializationFailed = false;
        private bool _mainWndEventsAttached = false;

        /// <summary>
        /// The root application container.
        /// </summary>
        private UnityContainer _rootContainer = new UnityContainer();

        #endregion // Private fields

        #region Private Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string PROJECTS_FOLDER = "Projects";
        private const string RELATIVE_FILEPATH_HELP = "help.xml";
        private const string RELATIVE_FILEPATH_IMPORT = "import.xml";
        private const string RELATIVE_FILEPATH_DEFAULTS = "defaults.xml";
        private const string RELATIVE_FILEPATH_EXPORT = "export.xml";
        private const string RELATIVE_FILEPATH_REPORT = "reports.xml";
        private const string RELATIVE_FILEPATH_REPORT_USER = "reports_user.xml";
        private const string RELATIVE_FILEPATH_SERVICES = "services.xml";
        private const string RELATIVE_FILEPATH_USER_SERVICES = "services_user.xml";
        private const string RELATIVE_FILEPATH_HISTORY_DB = @"History\history.sdf";
        private const string RELATIVE_FILEPATH_NAMEADDRESS_DB = @"Addresses\addresses.sdf";

        private const double SPLASH_FADEOUT_DURATION = 0.3;
        private const string SPLASH_RESOURCE = @"Resources\splash.bmp";

        /// <summary>
        /// Resource key for the message about license activation step.
        /// </summary>
        private const string ACTIVATING_LICENSE_STEP_KEY = "ActivatingLicenseStep";

        /// <summary>
        /// Resource key for the message about servers contacting step.
        /// </summary>
        private const string CONTACTING_SERVERS_STEP_KEY = "ContactingServersStep";

        /// <summary>
        /// Resource key for the message about services initialization step.
        /// </summary>
        private const string INITIALIZING_SERVICES_STEP_KEY = "InitializingServicesStep";

        /// <summary>
        /// Resource key for the message about project loading step.
        /// </summary>
        private const string LOADING_PROJECT_STEP_KEY = "LoadingProjectStep";

        /// <summary>
        /// Resource key for the message about commands loading step.
        /// </summary>
        private const string LOADING_COMMANDS_STEP_KEY = "LoadingCommandsStep";

        /// <summary>
        /// Resource key for the message about UI preparation step.
        /// </summary>
        private const string PREPARING_UI_STEP_KEY = "PreparingUIStep";

        #endregion // Constants
    }
}
