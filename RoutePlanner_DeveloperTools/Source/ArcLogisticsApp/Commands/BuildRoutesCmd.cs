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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Properties;
using System.IO;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command that builds routes.
    /// </summary>
    class BuildRoutesCmd : RoutingCommandBase, ISupportDisabledExecution
    {
        #region RoutingCommandBase Members

        public const string COMMAND_NAME = "ArcLogistics.Commands.BuildRoutes";

        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource(COMMAND_TITLE); }
        }

        public override string TooltipText
        {
            get
            {
                return _tooltipText;
            }
            protected set
            {
                _tooltipText = value;
                _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
            }
        }

        public override bool IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            protected set
            {
                base.IsEnabled = value;
            }
        }

        public override void Initialize(App app)
        {
            base.Initialize(app);
            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);

            // Define whether "key" file to allow run several BuildRoutes operations exist. 
            // Done there one time to avoid multiple call _DoesKeyToAllowMultipleBuildRoutesExists method.
            _isKeyToAllowMultipleBuildRoutesExist = _DoesKeyToAllowMultipleBuildRoutesExist();
        }

        #endregion

        #region ISupportDisabledExecution Members

        /// <summary>
        /// Gets bool value which define wheher command can be executed in disabled state.
        /// </summary>
        public bool AllowDisabledExecution
        {
            get
            {
                return _allowDisabledExecution;
            }
            private set
            {
                _allowDisabledExecution = value;
                _NotifyPropertyChanged(ALLOW_DISABLED_EXECUTION_PROPETY_NAME);
            }
        }

        #endregion

        #region Protected Override methods

        protected override void _Execute(params object[] args)
        {
            AsyncOperationInfo info = null;

            // Check whether operation can be started.
            if (!_CanBuildRoutesBeStarted(out info)) // If BuildRoutes operation cannot be started - show warning and return.
            {
                Debug.Assert(info != null); // Must be defined in _CanBuildRoutesBeStarted method.
                App.Current.MainWindow.MessageWindow.AddWarning(
                    string.Format((string)App.Current.FindResource(ALREADY_BUILDING_ROUTES_MESSAGE_RESOURCE), info.Schedule.PlannedDate.Value.ToShortDateString()));
                return;
            }

            try
            {
                Schedule schedule = _optimizeAndEditPage.CurrentSchedule;

                var inputParams = _GetBuildRoutesParameters(schedule);
                var routes = inputParams.TargetRoutes;
                var orders = inputParams.OrdersToAssign;

                if (_CheckRoutingParams(schedule, routes, orders))
                {
                    SolveOptions options = new SolveOptions();
                    options.GenerateDirections = App.Current.MapDisplay.TrueRoute;
                    options.FailOnInvalidOrderGeoLocation = false;

                    _SetOperationStartedStatus((string)App.Current.FindResource(BUILD_ROUTES_STRING), (DateTime)schedule.PlannedDate);

                    OperationsIds.Add(App.Current.Solver.BuildRoutesAsync(
                        schedule,
                        options,
                        inputParams));

                    // set solve started message
                    string infoMessage = _FormatSuccesSolveStartedMsg(schedule, inputParams);
                    if (!string.IsNullOrEmpty(infoMessage))
                        App.Current.Messenger.AddInfo(infoMessage);
                }
            }
            catch (RouteException e)
            {
                if (e.InvalidObjects != null) // if exception throw because any Routes or Orders are invalid
                    _ShowSolveValidationResult(e.InvalidObjects);
                else
                    _ShowErrorMsg(RoutingCmdHelpers.FormatRoutingExceptionMsg(e));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                if ((e is LicenseException) || (e is AuthenticationException) || (e is CommunicationException))
                    CommonHelpers.AddRoutingErrorMessage(e);
                else
                    throw;
            }
        }

        protected override string _FormatSuccessSolveCompletedMsg(
            Schedule schedule,
            AsyncOperationInfo info)
        {
            var inputParams = (BuildRoutesParameters)info.InputParams;

            string result;
            int totalOrdersCount = inputParams.OrdersToAssign.Count;
            Debug.Assert(schedule.UnassignedOrders.Count <= totalOrdersCount);

            if (schedule.UnassignedOrders.Count == 0)
                result = string.Format((string)App.Current.FindResource(ALL_ORDERS_ROUTED_TEXT), schedule.PlannedDate.Value.ToShortDateString());
            else
            {
                int ordersCount = totalOrdersCount - schedule.UnassignedOrders.Count;
                result = string.Format((string)App.Current.FindResource(NOT_ALL_ORDERS_ROUTED_TEXT), ordersCount, totalOrdersCount, schedule.PlannedDate.Value.ToShortDateString());
            }
            return result;
        }

        protected string _FormatSuccesSolveStartedMsg(
            Schedule schedule,
            BuildRoutesParameters inputParams)
        {
            string message = string.Format(
                (string)App.Current.FindResource(ORDERS_SUBMITTING_FOR_ROUTING_RESOURCE),
                inputParams.OrdersToAssign.Count,
                schedule.PlannedDate.Value.ToShortDateString());
            return message;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method checks whether "key" file exist in data folder directory.
        /// </summary>
        /// <returns>True if file was found and false otherwise.</returns>
        private bool _DoesKeyToAllowMultipleBuildRoutesExist()
        {
            // Define valid "key" file path.
            string keyPath = Path.Combine(DataFolder.Path, MULTIPLE_BUILD_ROUTES_KEY_FILE_NAME);

            Debug.Assert(!string.IsNullOrEmpty(keyPath));

            return File.Exists(keyPath);
        }

        /// <summary>
        /// Method checks is any BuildRoutes operation was started in other dates.
        /// </summary>
        /// <returns>True if no any BuildRoutes already started or application settings allows multiple BuildRoutes.</returns>
        /// <param name="info">Out param where method save AsyncOperationInfo with date where BuildRoutes is running.</param>
        private bool _CanBuildRoutesBeStarted(out AsyncOperationInfo outInfo)
        {
            bool canBuildRoutesBeStarted = true;

            // If "key" file allows multiple build routes - return "true".
            if (_isKeyToAllowMultipleBuildRoutesExist)
            {
                outInfo = null;
                return true;
            }

            bool allowDisabledExecution = false;
            AsyncOperationInfo info = null;

            foreach (Guid id in OperationsIds) // Check all running operations.
            {
                App.Current.Solver.GetAsyncOperationInfo(id, out info);
                if (info.OperationType == SolveOperationType.BuildRoutes && info.Schedule.PlannedDate != App.Current.CurrentDate)
                {
                    canBuildRoutesBeStarted = false; // "info" contains date where BuildRoutes is running.
                    allowDisabledExecution = true;
                    break;
                }
                info = null; // Set out parameter to null if BuildRoutes operation was not found.
            }

            outInfo = info;

            AllowDisabledExecution = allowDisabledExecution;

            return canBuildRoutesBeStarted;
        }

        /// <summary>
        /// Method check is command enabled.
        /// </summary>
        private void _CheckEnabled()
        {
            Schedule schedule = _optimizeAndEditPage.CurrentSchedule;
            bool hasOrders = ((schedule != null) && (schedule.UnassignedOrders != null) &&
                              ((schedule.UnassignedOrders.Count > 0) || ScheduleHelper.DoesScheduleHaveBuiltRoutes(schedule)));

            AsyncOperationInfo info = null;

            bool canBuildRoutesBeStarted = _CanBuildRoutesBeStarted(out info);

            bool isPageStateAllowRouting = (hasOrders && (schedule.Routes.Count > 0) &&
                         !_optimizeAndEditPage.IsEditingInProgress && !_optimizeAndEditPage.IsLocked && !_DoesAllRoutesLocked());

            // AllowDisabledExecution should be "false" if editing in progress.
            AllowDisabledExecution = isPageStateAllowRouting;

            IsEnabled = (isPageStateAllowRouting && canBuildRoutesBeStarted);
        }

        /// <summary>
        /// Method updates tooltip dependent on IsEnabled property and Routing operations state.
        /// </summary>
        /// <param name="isEnabled">IsEnabled value.</param>
        private void _UpdateTooltip()
        {
            if (!IsEnabled && AllowDisabledExecution) // If any BuildRoutes operation is started in other date and command is enabled - show special tooltip.
                TooltipText = (string)App.Current.FindResource(ALREADY_BUILDING_ROUTES_MESSAGE_TOOLTIP);
            else if (IsEnabled)
                TooltipText = (string)App.Current.FindResource(ENABLED_TOOLTIP);
            else
                TooltipText = (string)App.Current.FindResource(DISABLED_TOOLTIP);
        }

        /// <summary>
        /// Checks is schedule has locked routes.
        /// </summary>
        /// <returns></returns>
        private bool _DoesAllRoutesLocked()
        {
            bool _allRoutesLocked = true;

            foreach (Route route in _optimizeAndEditPage.CurrentSchedule.Routes)
            {
                if (!route.IsLocked)
                {
                    _allRoutesLocked = false;
                    break;
                }
            }

            return _allRoutesLocked;
        }

        /// <summary>
        /// Gets build routes operation parameters for the specified schedule.
        /// </summary>
        /// <param name="schedule">The schedule to get build routes operation
        /// parameters for.</param>
        /// <returns>Build routes operation parameters.</returns>
        private BuildRoutesParameters _GetBuildRoutesParameters(Schedule schedule)
        {
            var routes = ViolationsHelper.GetBuildRoutes(schedule);

            // get orders planned on schedule's date
            var day = (DateTime)schedule.PlannedDate;

            var orders =
                from order in App.Current.Project.Orders.Search(day)
                where !ConstraintViolationsChecker.IsOrderRouteLocked(order, schedule)
                select order;

            var parameters = new BuildRoutesParameters()
            {
                TargetRoutes = routes,
                OrdersToAssign = orders.ToList()
            };

            return parameters;
        }

        #endregion

        #region Private Event Handlers

        private void App_ApplicationInitialized(object sender, EventArgs e)
        {
            if (App.Current.Project != null)
            {
                App.Current.Project.SaveChangesCompleted += new SaveChangesCompletedEventHandler(Project_SaveChangesCompleted);
                App.Current.ProjectClosing += new EventHandler(Current_ProjectClosing);
            }

            App.Current.ProjectLoaded += new EventHandler(Current_ProjectLoaded);
            _optimizeAndEditPage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            _optimizeAndEditPage.CurrentScheduleChanged += new EventHandler(_schedulePage_CurrentScheduleChanged);
            _optimizeAndEditPage.LockedPropertyChanged += new EventHandler(_schedulePage_LockedPropertyChanged);
            _optimizeAndEditPage.EditBegun += new DataObjectEventHandler(_schedulePage_EditBegun);
            _optimizeAndEditPage.EditCommitted += new DataObjectEventHandler(_schedulePage_EditCommitted);
            _optimizeAndEditPage.EditCanceled += new DataObjectEventHandler(_schedulePage_EditCanceled);
            _optimizeAndEditPage.NewObjectCreated += new DataObjectEventHandler(_schedulePage_NewObjectCreated);
            _optimizeAndEditPage.NewObjectCanceled += new DataObjectEventHandler(_schedulePage_NewObjectCanceled);

            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(Solver_AsyncSolveCompleted);
        }

        private void Current_ProjectClosing(object sender, EventArgs e)
        {
            App.Current.Project.SaveChangesCompleted -= Project_SaveChangesCompleted;
        }

        private void Solver_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                _CheckEnabled();
                _UpdateTooltip();
            }
        }

        private void _schedulePage_NewObjectCanceled(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void _schedulePage_NewObjectCreated(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void _schedulePage_EditCommitted(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void _schedulePage_EditBegun(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void Current_ProjectLoaded(object sender, EventArgs e)
        {
            App.Current.Project.SaveChangesCompleted += new SaveChangesCompletedEventHandler(Project_SaveChangesCompleted);
        }

        private void _schedulePage_EditCanceled(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void _schedulePage_LockedPropertyChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void Project_SaveChangesCompleted(object sender, SaveChangesCompletedEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void _orders_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _CheckEnabled();
        }

        private void _routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        private void _schedulePage_CurrentScheduleChanged(object sender, EventArgs e)
        {
            if (null != _optimizeAndEditPage.CurrentSchedule.UnassignedOrders)
                ((INotifyCollectionChanged)_optimizeAndEditPage.CurrentSchedule.UnassignedOrders).CollectionChanged += new NotifyCollectionChangedEventHandler(_orders_CollectionChanged);

            if (null != _optimizeAndEditPage.CurrentSchedule.Routes)
                ((INotifyCollectionChanged)_optimizeAndEditPage.CurrentSchedule.Routes).CollectionChanged += new NotifyCollectionChangedEventHandler(_routes_CollectionChanged);

            _CheckEnabled();
            _UpdateTooltip();
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// "TooltipText" string.
        /// </summary>
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        /// <summary>
        /// "AlreadyBuildingRoutesMessage" resource name.
        /// </summary>
        private const string ALREADY_BUILDING_ROUTES_MESSAGE_RESOURCE = "AlreadyBuildingRoutesMessage";

        /// <summary>
        /// "AlreadyBuildingRoutesTooltip" resource name.
        /// </summary>
        private const string ALREADY_BUILDING_ROUTES_MESSAGE_TOOLTIP = "AlreadyBuildingRoutesTooltip";

        /// <summary>
        /// "OrdersSubmittingForRoutingText" resource name.
        /// </summary>
        private const string ORDERS_SUBMITTING_FOR_ROUTING_RESOURCE = "OrdersSubmittingForRoutingText";

        /// <summary>
        /// "AllOrdersRoutedText" resource name.
        /// </summary>
        private const string ALL_ORDERS_ROUTED_TEXT = "AllOrdersRoutedText";

        /// <summary>
        /// "NotAllOrdersRoutedText" resource name.
        /// </summary>
        private const string NOT_ALL_ORDERS_ROUTED_TEXT = "NotAllOrdersRoutedText";

        /// <summary>
        /// "BuildRoutes" resource name.
        /// </summary>
        private const string BUILD_ROUTES_STRING = "BuildRoutes";

        /// <summary>
        /// Enabled tooltip resource.
        /// </summary>
        private const string ENABLED_TOOLTIP = "BuildRoutesCommandEnabledTooltip";

        /// <summary>
        /// Disabled tooltip resource.
        /// </summary>
        private const string DISABLED_TOOLTIP = "BuildRoutesCommandDisabledTooltip";

        /// <summary>
        /// Command title resource.
        /// </summary>
        private const string COMMAND_TITLE = "BuildRoutesCommandTitle";

        /// <summary>
        /// "AllowDisabledExecution" string.
        /// </summary>
        private const string ALLOW_DISABLED_EXECUTION_PROPETY_NAME = "AllowDisabledExecution";

        /// <summary>
        /// Name of "key" file to allow several BuildRoutes operations.
        /// </summary>
        private const string MULTIPLE_BUILD_ROUTES_KEY_FILE_NAME = @"60A6142D-F6B2-4495-83BC-C004DBBBD49F.key";

        /// <summary>
        /// File extension "key".
        /// </summary>
        private const string FILE_KEY_EXTENSION = @"*.key";

        #endregion

        #region Private Fields

        /// <summary>
        /// Tooltip string.
        /// </summary>
        private string _tooltipText = null;

        /// <summary>
        /// Parent optimize and edit page.
        /// </summary>
        private OptimizeAndEditPage _optimizeAndEditPage;

        /// <summary>
        /// Defines whether command execution is allowed in disabled state.
        /// </summary>
        private bool _allowDisabledExecution = false;

        /// <summary>
        /// Flag shows that key to allow user run several BuildRoutes operations exists.
        /// </summary>
        private bool _isKeyToAllowMultipleBuildRoutesExist = false;

        #endregion
    }
}
