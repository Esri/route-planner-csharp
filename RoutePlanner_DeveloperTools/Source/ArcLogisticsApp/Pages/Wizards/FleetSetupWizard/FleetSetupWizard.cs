using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.App.Import;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Fleet setup wizard.
    /// </summary>
    internal partial class FleetSetupWizard : WizardBase
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public FleetSetupWizard()
            :base(_pageTypes, new FleetSetupWizardDataContext())
        {
            DataKeeper.AddedOrders = new List<Order>();
        }

        #endregion // Constructors

        #region Public methods

        /// <summary>
        /// Starts wizard.
        /// </summary>
        public override void Start()
        {
            base.Start();

            MainWindow mainWindow = App.Current.MainWindow;

            // Store application state.
            DataKeeper[FleetSetupWizardDataContext.ParentPageFieldName] = mainWindow.CurrentPage;
            DataKeeper[FleetSetupWizardDataContext.ProjectFieldName] = App.Current.Project;

            // Start wizard.
            mainWindow.Lock(false);
            _NavigateToPage(START_PAGE_INDEX);
        }

        #endregion // Public methods

        #region Private properties

        /// <summary>
        /// Specialized context.
        /// </summary>
        private FleetSetupWizardDataContext DataKeeper
        {
            get
            {
                return DataContext as FleetSetupWizardDataContext;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Special initializing of wizard pages.
        /// </summary>
        protected override void PostInit()
        {
            base.PostInit();

            foreach (WizardPageBase page in Pages)
            {
                FleetSetupWizardImportObjectsPage importObjectsPage = page as FleetSetupWizardImportObjectsPage;
                if (importObjectsPage != null)
                {
                    ImportProfile profile = CommonHelpers.GetOneTimeProfile(ImportType.Orders);
                    importObjectsPage.PostInit(profile);
                }
            }
        }
        /// <summary>
        /// "Next" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Event arguments.</param>
        protected override void OnNextRequired(object sender, EventArgs e)
        {
            int nextPageIndex = -1;
            if (sender is FleetSetupWizardImportOrdersDecisionPage)
            {   // special next page selection routine - select one from variantes
                var page = sender as FleetSetupWizardImportOrdersDecisionPage;
                Type nextPageType = (page.IsImportSelected) ?
                    typeof(FleetSetupWizardImportObjectsPage) : typeof(FleetSetupWizardFinishPage);
                nextPageIndex = _GetPageIndex(nextPageType);
            }
            else if (sender is FleetSetupWizardImportObjectsPage)
            {
                // Special next page selection routine.
                Type nextPageType = typeof(FleetSetupWizardFinishPage);

                var page = sender as FleetSetupWizardImportObjectsPage;
                if (!page.IsProcessCanceled)
                {
                    // Go to ungeocoded orders page.
                    if (_IsUngeocodedPresent())
                        nextPageType = typeof(FleetSetupWizardUngeocodedOrdersPage);
                }
                nextPageIndex = _GetPageIndex(nextPageType);
            }
            else if (sender is FleetSetupWizardUngeocodedOrdersPage)
            {
                // Special next page selection routine. Ungeocoded orders page leads to finish page.
                Type nextPageType = typeof(FleetSetupWizardFinishPage);
                nextPageIndex = _GetPageIndex(nextPageType);
            }
            else
            {
                // Next by index.
                int currentPageIndex = _GetPageIndex(sender as WizardPageBase);
                nextPageIndex = currentPageIndex + 1;
            }

            // Go to next page.
            if (-1 != nextPageIndex)
                _NavigateToPage(nextPageIndex);
        }

        /// <summary>
        /// "Cancel" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Ignored.</param>
        protected override void _OnCancelRequired(object sender, EventArgs e)
        {
            _Close(PagePaths.ProjectsPagePath);
        }

        /// <summary>
        /// "Finish" button cliked in page.
        /// </summary>
        /// <param name="sender">Page when button clicked.</param>
        /// <param name="e">Ignored.</param>
        protected override void _OnFinishRequired(object sender, EventArgs e)
        {
            OptimizeAndEditPage optimizePage = _GetOptimizePage();
            if (optimizePage.IsAllowed)
                optimizePage.Loaded += new System.Windows.RoutedEventHandler(_OptimizePageLoaded);

            // Close wizard.
            _Close(optimizePage.IsAllowed ? PagePaths.SchedulePagePath : PagePaths.ProjectsPagePath);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Optimize abd edit page loaded handler.
        /// </summary>
        private void _OptimizePageLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _GetOptimizePage().Loaded -= _OptimizePageLoaded;

            if ((0 < DataKeeper.AddedOrders.Count) && (0 < DataKeeper.Routes.Count))
            { 
                // Build routes. Update build date.
                if (0 < DataKeeper.AddedOrders.Count)
                    App.Current.CurrentDate = _CalculateBuildDate(DataKeeper.AddedOrders);

                _UpdateCurrentScheduleRoutes();

                // Call build routes command.
                string widgetName = AppCommands.CategoryNames.ScheduleTaskWidgetCommands;
                ICollection<AppCommands.ICommand> commands =
                    App.Current.CommandManager.GetCategoryCommands(widgetName);
                foreach (AppCommands.ICommand command in commands)
                {
                    if (command.Name == AppCommands.BuildRoutesCmd.COMMAND_NAME)
                    {
                        command.Execute(null);
                        break;
                    }
                }
            }
        }

        #endregion // Event handlers

        #region Private methods

        /// <summary>
        /// Close wizard routine.
        /// </summary>
        /// <param name="showPagePath">Page to showing after close wizard.</param>
        private void _Close(string showPagePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(showPagePath));

            App.Current.MainWindow.Unlock();
            App.Current.MainWindow.Navigate(showPagePath);
        }

        /// <summary>
        /// Finds date interval of imported orders.
        /// </summary>
        /// <param name="orders">Added orders.</param>
        /// <param name="minDate">Min date.</param>
        /// <param name="maxDate">Max date.</param>
        private void _FindDateIntervalOfImportedOrders(IList<Order> orders,
                                                       out DateTime minDate, out DateTime maxDate)
        {
            Debug.Assert(null != orders);

            DateTime currentDate = App.Current.CurrentDate;
            minDate = DateTime.MaxValue;
            maxDate = DateTime.MinValue;
            foreach (Order order in orders)
            {
                Debug.Assert(order.PlannedDate.HasValue);
                DateTime plannedDate = order.PlannedDate.Value;

                if (plannedDate < minDate)
                    minDate = plannedDate;
                if (maxDate < plannedDate)
                    maxDate = plannedDate;
            }
        }

        /// <summary>
        /// Calculates build date.
        /// </summary>
        /// <param name="orders">Added orders.</param>
        /// <returns>Calculated build date.</returns>
        private DateTime _CalculateBuildDate(IList<Order> orders)
        {
            Debug.Assert(null != orders);

            DateTime minDate;
            DateTime maxDate;
            _FindDateIntervalOfImportedOrders(orders, out minDate, out maxDate);

            DateTime buildDate = minDate;
            if (minDate != maxDate)
            {
                DateTime currentDate = App.Current.CurrentDate;

                if (currentDate <= minDate)
                    buildDate = minDate; // if planned date field, build for 1st day with orders
                else if (maxDate < currentDate)
                {
                    buildDate = maxDate; // if orders are imported into the past,
                    // build for most recent day.
                }
                else
                {   // search first date with orders from current date
                    DateTime date =
                        buildDate = currentDate;
                    while (date <= maxDate)
                    {
                        if (orders.Any(order => order.PlannedDate == date))
                        {
                            buildDate = date;
                            break; // result found
                        }

                        date = date.AddDays(1);
                    }
                }
            }

            return buildDate;
        }

        /// <summary>
        /// Checks in unassigned orders present ungeocoded.
        /// </summary>
        /// <returns>TRUE if in project present ungeocoded unassigned orders.</returns>
        private bool _IsUngeocodedPresent()
        {
            return DataKeeper.AddedOrders.Any(order => !order.IsGeocoded);
        }

        /// <summary>
        /// Gets current schedule for date.
        /// </summary>
        /// <returns>Current schedule for date.</returns>
        private Schedule _GetCurrentScheduleByDate()
        {
            return ScheduleHelper.GetCurrentScheduleByDay(App.Current.CurrentDate);
        }

        /// <summary>
        /// Optimize and Edit page accessor.
        /// </summary>
        /// <returns>Optimize and Edit page.</returns>
        private OptimizeAndEditPage _GetOptimizePage()
        {
            return (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
        }

        /// <summary>
        /// Checks is object present in object collection.
        /// </summary>
        /// <param name="currentObj">Object to check.</param>
        /// <param name="collection">Object's collection.</param>
        /// <returns>TRUE if cheked object present in collection.</returns>
        private bool _IsObjectPresentInCollection<T>(T currentObj,
                                                     IDataObjectCollection<T> collection)
            where T : DataObject
        {
            string objName = currentObj.ToString();
            return collection.Any(obj =>
                                  obj.ToString().Equals(objName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds new routes to current schedule routes.
        /// </summary>
        private void _UpdateCurrentScheduleRoutes()
        {
            OptimizeAndEditPage optimizePage = _GetOptimizePage();
            Schedule schedule = optimizePage.CurrentSchedule;
            if (null != schedule)
            {
                IDataObjectCollection<Route> wizardRoutes = DataKeeper.Routes;
                IDataObjectCollection<Route> scheduleRoutes = schedule.Routes;
                for (int index = 0; index < wizardRoutes.Count; ++index)
                {
                    Route route = wizardRoutes[index];
                    if (!_IsObjectPresentInCollection(route, scheduleRoutes))
                        scheduleRoutes.Add(route);
                }
            }
        }

        #endregion // Private methods

        #region Private consts

        /// <summary>
        /// Start page index.
        /// </summary>
        private const int START_PAGE_INDEX = 0;

        /// <summary>
        /// Predifined wizards pages.
        /// </summary>
        /// <remarks>Wizard show pages in same order.</remarks>
        private static Type[] _pageTypes = new Type[]
        {
            typeof(FleetSetupWizardIntroductionPage),
            typeof(FleetSetupWizardLocationPage),
            typeof(FleetSetupWizardRoutesPage),
            typeof(FleetSetupWizardImportOrdersDecisionPage),
            typeof(FleetSetupWizardImportObjectsPage),
            typeof(FleetSetupWizardUngeocodedOrdersPage),
            typeof(FleetSetupWizardFinishPage)
        };

        #endregion // Private consts
    }
}
