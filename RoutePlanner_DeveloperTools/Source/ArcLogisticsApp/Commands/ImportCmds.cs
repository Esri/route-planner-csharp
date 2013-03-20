using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.Import;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using AppPages = ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Pages.Wizards;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Import Base command.
    /// </summary>
    abstract class ImportBaseCmd : CommandBase
    {
        #region Abstract properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected abstract ImportType Type
        {
            get;
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected abstract AppPages.Page ParentPage
        {
            get;
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected abstract string TitleResource
        {
            get;
        }

        #endregion // Abstract properties

        #region CommandBase methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize routine.
        /// </summary>
        /// <param name="app">Application instance</param>
        public override void Initialize(App app)
        {
            base.Initialize(app);
            app.ApplicationInitialized += new EventHandler(_App_ApplicationInitialized);
        }

        #endregion // CommandBase methods

        #region CommandBase properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Title of the command that can be shown in UI.
        /// </summary>
        public override string Title
        {
            get { return App.Current.FindString(TitleResource); }
        }

        /// <summary>
        /// Tooltip text.
        /// </summary>
        public override string TooltipText
        {
            get { return _tooltipText; }
            protected set
            {
                _tooltipText = value;
                _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
            }
        }

        /// <summary>
        /// Is enabled flag.
        /// </summary>
        public override bool IsEnabled
        {
            get { return _isEnabled; }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(ENABLED_PROPERTY_NAME);

                if (value)
                    TooltipText = App.Current.FindString("ImportCommandEnabledTooltip");
                else
                {
                    string objectsName = CommonHelpers.GetImportObjectsName(Type);
                    TooltipText =
                        App.Current.GetString("ImportCommandsTooltipTextFormat", objectsName);
                }
            }
        }
        #endregion // CommandBase properties

        #region CommandBase methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Internal execute.
        /// </summary>
        /// <param name="args">Commnad arguments.</param>
        protected override void _Execute(params object[] args)
        {
            _ClearGridSelectionOnParentPage();

            ImportProfilesKeeper importProfileKeeper = _Application.ImportProfilesKeeper;
            ImportProfile profile = importProfileKeeper.GetDefaultProfile(Type);
            if (null != profile)
                // start import with default profile
                _DoImport(profile);
            else
            {   // default profile absent - use on fly profile

                // create page to on fly creating profile
                Type type = typeof(FleetSetupWizardImportObjectsPage);
                _onFlyCreatingProfilePage =
                    (FleetSetupWizardImportObjectsPage)Activator.CreateInstance(type);
                _onFlyCreatingProfilePage.Loaded +=
                    new RoutedEventHandler(_onFlyCreatingProfilePage_Loaded);

                // create empty profile
                _doesNewProfile = (null == importProfileKeeper.GetOneTimeProfile(Type));
                ImportProfile onFlyProfile = CommonHelpers.GetOneTimeProfile(Type);

                // init page state
                _onFlyCreatingProfilePage.PostInit(onFlyProfile);

                // show page
                App.Current.MainWindow.PageFrame.Navigate(_onFlyCreatingProfilePage);
            }
        }

        #endregion // CommandBase methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _App_ApplicationInitialized(object sender, EventArgs e)
        {
            if (null != ParentPage)
                ParentPage.Loaded += new RoutedEventHandler(_ParentPage_Loaded);
        }

        private void _ParentPage_Loaded(object sender, RoutedEventArgs e)
        {
            IsEnabled = _isEnabled; // update tooltip

            // do import
            if (null != _onFlyProfile)
            {
                // store profile for using
                _Application.ImportProfilesKeeper.AddOrUpdateProfile(_onFlyProfile);

                _DoImport(_onFlyProfile);

                _onFlyProfile = null;
                _doesNewProfile = false;
            }
        }

        private void _onFlyCreatingProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(null != _onFlyCreatingProfilePage);
            Debug.Assert(null != ParentPage);

            App.Current.MainWindow.ToggleWidgetsState(ParentPage, IGNORED_WIDGETS, false);

            // unsubscribe this event
            _onFlyCreatingProfilePage.Loaded -= _onFlyCreatingProfilePage_Loaded;

            // subscribe events
            _onFlyCreatingProfilePage.EditOK +=
                new EventHandler(_onFlyCreatingProfilePage_EditOK);
            _onFlyCreatingProfilePage.EditCancel +=
                new EventHandler(_onFlyCreatingProfilePage_EditCancel);
            _onFlyCreatingProfilePage.Unloaded +=
                new RoutedEventHandler(_onFlyCreatingProfilePage_Unloaded);
        }

        private void _onFlyCreatingProfilePage_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(null != _onFlyCreatingProfilePage);

            // unsubscribe events
            _onFlyCreatingProfilePage.EditOK -= _onFlyCreatingProfilePage_EditOK;
            _onFlyCreatingProfilePage.EditCancel -= _onFlyCreatingProfilePage_EditCancel;
            _onFlyCreatingProfilePage.Unloaded -= _onFlyCreatingProfilePage_Unloaded;
            _onFlyCreatingProfilePage = null;

            App.Current.MainWindow.ToggleWidgetsState(ParentPage, IGNORED_WIDGETS, true);
        }

        private void _onFlyCreatingProfilePage_EditOK(object sender, EventArgs e)
        {
            Debug.Assert(null != _onFlyCreatingProfilePage);

            // store created profile
            _onFlyProfile = (sender as FleetSetupWizardImportObjectsPage).Profile;

            _GoBackToParentPage();
        }

        private void _onFlyCreatingProfilePage_EditCancel(object sender, EventArgs e)
        {
            _GoBackToParentPage();
        }

        /// <summary>
        /// Disables Navigation Pane Widgets on Ungecoded orders page load.
        /// </summary>
        /// <param name="sender">Ungeocoded orders fleet wizard page.</param>
        /// <param name="e">Not used.</param>
        private void _UngeocodedOrdersPageLoaded(object sender, EventArgs e)
        {
            App.Current.MainWindow.ToggleWidgetsState(ParentPage, IGNORED_WIDGETS, false);

            var ordersPage = sender as FleetSetupWizardUngeocodedOrdersPage;

            if (ordersPage != null)
                ordersPage.Loaded -= _UngeocodedOrdersPageLoaded;
        }

        /// <summary>
        /// Enables Navigation Pane Widgets on Ungecoded orders page unload.
        /// </summary>
        /// <param name="sender">Ungeocoded orders fleet wizard page.</param>
        /// <param name="e">Not used.</param>
        private void _UngeocodedOrdersPageUnloaded(object sender, EventArgs e)
        {
            App.Current.MainWindow.ToggleWidgetsState(ParentPage, IGNORED_WIDGETS, true);

            var ordersPage = sender as FleetSetupWizardUngeocodedOrdersPage;

            if (ordersPage != null)
                ordersPage.Unloaded -= _UngeocodedOrdersPageUnloaded;
        }

        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clears grid selection on Parent Page before importing and inserting new elements.
        /// </summary>
        private void _ClearGridSelectionOnParentPage()
        {
            if (ParentPage == null)
                return;

            var selection = ParentPage as AppPages.ISupportSelection;

            if (selection != null)
            {
                selection.SelectedItems.Clear();
            }
        }

        /// <summary>
        /// Do import process.
        /// </summary>
        /// <param name="profile">Profile to importing.</param>
        private void _DoImport(ImportProfile profile)
        {
            var manager = new ImportManager();

            // Subscribe to order completed. If parent page is OptimizeAndEditPage -
            // call Ungeocoded orders page.
            manager.ImportCompleted += new ImportCompletedEventHandler(_ImportCompleted);

            manager.ImportAsync(ParentPage, profile, App.Current.CurrentDate);
        }

        /// <summary>
        /// React on import completed.
        /// </summary>
        /// <param name="sender">Importer.</param>
        /// <param name="e">Ignored.</param>
        private void _ImportCompleted(object sender, ImportCompletedEventArgs e)
        {
            var manager = sender as ImportManager;
            Debug.Assert(manager != null);
            manager.ImportCompleted -= _ImportCompleted;
            manager.Dispose();

            // Special logic for Optimize and edit page.
            if (ParentPage is AppPages.OptimizeAndEditPage)
            {
                // Get ungeocoded orders collection.
                var ungeocodedOrders = new List<Order>();
                foreach (Order order in e.ImportedObjects)
                {
                    if (!order.IsGeocoded)
                    {
                        Debug.Assert(!ungeocodedOrders.Contains(order));
                        ungeocodedOrders.Add(order);
                    }
                }

                // If at least one ungeocoded order present on current day -
                // show ungeocoded orders page.
                if (ungeocodedOrders.Count > 0)
                {
                    var ordersPage =
                        new FleetSetupWizardUngeocodedOrdersPage(ungeocodedOrders);

                    // React on page load and unload.
                    ordersPage.Loaded +=
                        new RoutedEventHandler(_UngeocodedOrdersPageLoaded);
                    ordersPage.Unloaded +=
                        new RoutedEventHandler(_UngeocodedOrdersPageUnloaded);

                    ordersPage.NextRequired += new EventHandler(_OrdersPageNextRequired);
                    // Show orders geocoding page.
                    App.Current.MainWindow.PageFrame.Navigate(ordersPage);
                }
            }
        }

        /// <summary>
        /// Return to optimize and edit page on next button clicked.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OrdersPageNextRequired(object sender, EventArgs e)
        {
            _GoBackToParentPage();
        }

        /// <summary>
        /// Go back to parent page.
        /// </summary>
        private void _GoBackToParentPage()
        {
            App.Current.MainWindow.PageFrame.NavigationService.GoBack();
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Properties names.
        /// </summary>
        private const string ENABLED_PROPERTY_NAME = "IsEnabled";
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        /// <summary>
        /// Predifined ignored widget collection.
        /// </summary>
        static private readonly Type[] IGNORED_WIDGETS = new Type[]
        {
            typeof(ESRI.ArcLogistics.App.Widgets.QuickHelpWidget)
        };

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Enabled flag.
        /// </summary>
        private bool _isEnabled = true;
        /// <summary>
        /// Tooltip text.
        /// </summary>
        private string _tooltipText;

        /// <summary>
        /// On fly created profile.
        /// </summary>
        private ImportProfile _onFlyProfile;
        /// <summary>
        /// On fly creating profile page.
        /// </summary>
        private FleetSetupWizardImportObjectsPage _onFlyCreatingProfilePage;

        /// <summary>
        /// New import profile cerated flag.
        /// </summary>
        private bool _doesNewProfile;

        #endregion // Private fields
    }

    /// <summary>
    /// Import Orders command.
    /// </summary>
    class ImportOrdersCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportOrders";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        /// <summary>
        /// Enabled flag.
        /// </summary>
        public override bool IsEnabled
        {
            get { return (base.IsEnabled && _isEnabled); }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(ENABLED_PROPERTY_NAME);

                string resourceName = (value)? "ImportOrdersCommandEnabledTooltip" :
                                               "ImportOrdersCommandDisabledTooltip";
                TooltipText = App.Current.FindString(resourceName);
            }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.Orders; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.SchedulePagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportOrdersCommandTitle"; }
        }

        #endregion // CommandBase methods

        #region CommandBase methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize routine.
        /// </summary>
        /// <param name="app">Application instance.</param>
        public override void Initialize(App app)
        {
            base.Initialize(app);
            App.Current.ApplicationInitialized +=
                new EventHandler(_App_ApplicationInitialized);
            App.Current.Solver.AsyncSolveStarted +=
                new AsyncSolveStartedEventHandler(_AsyncSolveStarted);
            App.Current.Solver.AsyncSolveCompleted +=
                new AsyncSolveCompletedEventHandler(_AsyncSolveCompleted);
        }

        #endregion // CommandBase methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _ParentPage_Loaded(object sender, RoutedEventArgs e)
        {
            App currentApp = App.Current;
            IsEnabled = (currentApp.Solver.GetAsyncOperations(currentApp.CurrentDate).Count == 0);
        }

        private void _AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            Schedule schedule = null;
            AsyncOperationInfo info = null;
            if (App.Current.Solver.GetAsyncOperationInfo(e.OperationId, out info))
                schedule = info.Schedule;

            if (!IsEnabled)
            {
                IsEnabled = ((null == _schedulePage.CurrentSchedule) ||
                             (schedule.PlannedDate == _schedulePage.CurrentSchedule.PlannedDate));
            }
        }

        private void _AsyncSolveStarted(object sender, AsyncSolveStartedEventArgs e)
        {
            IsEnabled = false;
        }

        private void _App_ApplicationInitialized(object sender, EventArgs e)
        {
            App.Current.CurrentDateChanged +=
                new EventHandler(_ImportCmd_CurrentDateChanged);

            if (null != ParentPage)
                ParentPage.Loaded += new RoutedEventHandler(_ParentPage_Loaded);

            _schedulePage =
                (AppPages.OptimizeAndEditPage)App.Current.MainWindow.GetPage(AppPages.PagePaths.SchedulePagePath);
            _schedulePage.EditBegun +=
                new AppPages.DataObjectEventHandler(_SchedulePage_EditBegun);
            _schedulePage.EditCommitted +=
                new AppPages.DataObjectEventHandler(_SchedulePage_EditCommitted);
            _schedulePage.EditCanceled +=
                new AppPages.DataObjectEventHandler(_SchedulePage_EditCanceled);
        }

        private void _SchedulePage_EditCommitted(object sender, AppPages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _SchedulePage_EditBegun(object sender, AppPages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _SchedulePage_EditCanceled(object sender, AppPages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _ImportCmd_CurrentDateChanged(object sender, EventArgs e)
        {
            App currentApp = App.Current;
            IsEnabled = (currentApp.Solver.GetAsyncOperations(currentApp.CurrentDate).Count == 0);
        }

        #endregion // Event handlers

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method checks is command enabled.
        /// </summary>
        protected void _CheckEnabled()
        {
            IsEnabled = !_schedulePage.IsEditingInProgress;
        }

        #endregion // Protected methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string ENABLED_PROPERTY_NAME = "IsEnabled";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Schedule page.
        /// </summary>
        private AppPages.OptimizeAndEditPage _schedulePage;

        /// <summary>
        /// Enabled flag.
        /// </summary>
        private bool _isEnabled = true;

        #endregion // Private members
    }

    /// <summary>
    /// Import Locations command.
    /// </summary>
    class ImportLocationsCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportLocations";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.Locations; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.LocationsPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportLocationsCommandTitle"; }
        }

        #endregion // CommandBase methods
    }

    /// <summary>
    /// Import MobileDevices command.
    /// </summary>
    class ImportMobileDevicesCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportMobileDevices";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.MobileDevices; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.MobileDevicesPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportMobileDevicesCommandTitle"; }
        }

        #endregion // CommandBase methods
    }

    /// <summary>
    /// Import Drivers command.
    /// </summary>
    class ImportDriversCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportDrivers";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.Drivers; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.DriversPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportDriversCommandTitle"; }
        }

        #endregion // CommandBase methods
    }

    /// <summary>
    /// Import Vehicles command.
    /// </summary>
    class ImportVehiclesCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportVehicles";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.Vehicles; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.VehiclesPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportVehiclesCommandTitle"; }
        }

        #endregion // CommandBase methods
    }

    /// <summary>
    /// Import DefaultRoutes command.
    /// </summary>
    class ImportDefaultRoutesCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportDefaultRoutes";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.DefaultRoutes; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.DefaultRoutesPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportDefaultRoutesCommandTitle"; }
        }

        #endregion // CommandBase methods
    }

    /// <summary>
    /// Import DriverSpecialties command.
    /// </summary>
    class ImportDriverSpecialtiesCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportDriverSpecialties";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.DriverSpecialties; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.SpecialtiesPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportDriverSpecialtiesCommandTitle"; }
        }

        #endregion // CommandBase methods
    }
    
    /// <summary>
    /// Import VehicleSpecialties command.
    /// </summary>
    class ImportVehicleSpecialtiesCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportVehicleSpecialties";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.VehicleSpecialties; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.SpecialtiesPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportVehicleSpecialtiesCommandTitle"; }
        }

        #endregion // CommandBase methods
    }

    /// <summary>
    /// Import Barriers command.
    /// </summary>
    class ImportBarriersCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportBarriers";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.Barriers; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.BarriersPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportBarriersCommandTitle"; }
        }

        #endregion // CommandBase methods
    }

    /// <summary>
    /// Import Zones command.
    /// </summary>
    class ImportZonesCmd : ImportBaseCmd
    {
        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string COMMAND_NAME = "ArcLogistics.Commands.ImportZones";

        #endregion // Constants

        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the command. Must be unique and unchanging.
        /// </summary>
        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        #endregion // Properties

        #region ImportBaseCmd methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets import type.
        /// </summary>
        protected override ImportType Type
        {
            get { return ImportType.Zones; }
        }

        /// <summary>
        /// Gets parent page.
        /// </summary>
        protected override AppPages.Page ParentPage
        {
            get { return App.Current.MainWindow.GetPage(AppPages.PagePaths.ZonesPagePath); }
        }

        /// <summary>
        /// Gets title string resource name.
        /// </summary>
        protected override string TitleResource
        {
            get { return "ImportZonesCommandTitle"; }
        }

        #endregion // CommandBase methods
    }
}
