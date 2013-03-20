using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command sequences orders on a route trying to find optimal assignment.
    /// </summary>
    class SequenceRoutesCmd : RoutingCommandBase
    {
        public const string COMMAND_NAME = "ArcLogistics.Commands.SequenceRoutes";

        public override string Name
        {
            get { return COMMAND_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("SequenceRoutesCommandTitle"); }
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

                if (value)
                    TooltipText = (string)App.Current.FindResource("SequenceRoutesCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("SequenceRoutesCommandDisabledTooltip");
            }
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

        public override void Initialize(App app)
        {
            base.Initialize(app);
            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);
        }

        void App_ApplicationInitialized(object sender, EventArgs e)
        {
            App.Current.ProjectClosed += new EventHandler(Current_ProjectClosed);
            _schedulePage = (OptimizeAndEditPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            
            _schedulePage.SelectionChanged += new EventHandler(_schedulePage_SelectionChanged);

            _schedulePage.CurrentScheduleChanged += new EventHandler(_schedulePage_CurrentScheduleChanged);
            _schedulePage.EditCommitted += new DataObjectEventHandler(_schedulePage_EditCommitted);
            _schedulePage.EditBegun += new DataObjectEventHandler(_schedulePage_EditBegun); 
            _schedulePage.EditCanceled += new DataObjectEventHandler(_schedulePage_EditCanceled);

            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(Solver_AsyncSolveCompleted);
        }

        protected override string _FormatSuccessSolveCompletedMsg(ESRI.ArcLogistics.DomainObjects.Schedule schedule, AsyncOperationInfo info)
        {
            string result;
            
            int sequencedRoutesCount = ((SequenceRoutesParams)info.InputParams).RoutesToSequence.Count;

            result = string.Format((string)App.Current.FindResource("SequenceCommandCompletedText"), sequencedRoutesCount, schedule.PlannedDate.Value.ToShortDateString());

            return result;
        }

        /// <summary>
        /// Method checks is command enbled or not.
        /// </summary>
        /// <returns></returns>
        private void _CheckEnabled()
        {
            bool isEnabled = false;

            isEnabled = (_GetRoutesFromSelection(_schedulePage.SelectedItems).Count > 0 && !_HasLockedRoutes(_schedulePage.SelectedItems) && !_HasEmptyRoutes(_schedulePage.SelectedItems));

            if ((_schedulePage.IsLocked || _schedulePage.IsEditingInProgress) || (_schedulePage.CurrentSchedule == null || _schedulePage.CurrentSchedule.Routes.Count == 0))
                isEnabled = false;
            
            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Method checks is selection has unassigned orders
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        private bool _HasLockedRoutes(IList selection)
        {
            bool hasLockedRoutes = false;
            foreach (Object obj in selection)
            {
                if (obj is Route && ((Route)obj).IsLocked)
                {
                    hasLockedRoutes = true;
                    break;
                }
            }
            return hasLockedRoutes;
        }

        protected override void _Execute(params object[] args)
        {
            try
            {
                ICollection<Route> routes = _GetRoutesFromSelection(_schedulePage.SelectedItems);

                // get unlocked routes from selected routes
                // and all orders assigned to converting routes
                List<Order> orders = new List<Order>();
                foreach (Route route in routes)
                {
                    if (!route.IsLocked)
                     {
                         // get assigned orders
                         List<Order> routeOrders = new List<Order>();
                         foreach (Stop stop in route.Stops)
                         {
                             if (stop.StopType == StopType.Order)
                                 routeOrders.Add(stop.AssociatedObject as Order);
                         }
                         orders.AddRange(routeOrders);
                     }
                }

                // get current schedule
                Schedule schedule = _schedulePage.CurrentSchedule;

                if (_CheckRoutingParams(schedule, routes, orders))
                {
                    SolveOptions options = new SolveOptions();
                    options.GenerateDirections = App.Current.MapDisplay.TrueRoute;
                    options.FailOnInvalidOrderGeoLocation = false;

                    _SetOperationStartedStatus((string)App.Current.FindResource("SequenceRoutes"), (DateTime)schedule.PlannedDate);

                    OperationsIds.Add(App.Current.Solver.SequenceRoutesAsync(schedule, routes, options));

                    // set solve started message
                    string infoMessage = _FormatSuccesSolveStartedMsg(schedule, routes.Count);
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

        protected string _FormatSuccesSolveStartedMsg(Schedule schedule, int routesCount)
        {
            string message = string.Format((string)App.Current.FindResource("RoutesSubmittingForSequencingText"), routesCount, schedule.PlannedDate.Value.ToShortDateString());
            return message;
        }

        /// <summary>
        /// Method gets routes collection from selection collection
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        private ICollection<Route> _GetRoutesFromSelection(IList selection)
        {
            Collection<Route> routes = new Collection<Route>();
            foreach (Object obj in selection)
            {
                if (obj is Route)
                    routes.Add((Route)obj);
            }

            return routes;
        }

        /// <summary>
        /// Method checks is selection has empty routes
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        private bool _HasEmptyRoutes(IList selection)
        {
            bool hasEmptyRoutes = false;

            foreach (Object obj in selection)
            {
                if (obj is Route && ((Route)obj).Stops.Count == 0)
                    hasEmptyRoutes = true;
            }

            return hasEmptyRoutes;
        }

        #region Event Handlers

        private void Solver_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            if (e.Cancelled)
                _CheckEnabled();
        }

        private void Current_ProjectClosed(object sender, EventArgs e)
        {
            IsEnabled = false;
        }

        private void _schedulePage_EditBegun(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _schedulePage_EditCommitted(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _schedulePage_EditCanceled(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _schedulePage_CurrentScheduleChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
        }

        private void _schedulePage_SelectionChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
        }

        #endregion

        #region private members

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        private string _tooltipText = null;
        private OptimizeAndEditPage _schedulePage;

        #endregion
    }
}
