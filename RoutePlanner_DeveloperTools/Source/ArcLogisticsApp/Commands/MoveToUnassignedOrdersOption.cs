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
    /// Class implements logic for "MoveToUnassignedOrders" option.
    /// </summary>
    class MoveToUnassignedOrdersOption : RoutingCommandBase, ICommandOption
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of  MoveToUnassignedOrdersOption. Inits group ID.
        /// </summary>
        /// <param name="groupId">Option group ID (to set in separate group in UI).</param>
        public MoveToUnassignedOrdersOption(int groupId)
        {
            GroupID = groupId;
            _schedulePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            Debug.Assert(_schedulePage != null);
            _schedulePage.SelectionChanged += new EventHandler(_SelectionChanged);

            Debug.Assert(App.Current.Solver != null);
            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(Solver_AsyncSolveCompleted);

            _UpdateTooltip();
        }

        #endregion

        #region Overrided Properties

        /// <summary>
        /// Gets option Name.
        /// </summary>
        public override string Name
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets/sets option's IsEnabled value.
        /// </summary>
        bool ICommandOption.IsEnabled
        {
            get
            {
                return base.IsEnabled;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        /// <summary>
        /// Returns option's title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource(TITLE_RESOURCE); }
        }

        /// <summary>
        /// Returns option' tooltip.
        /// </summary>
        public override string TooltipText
        {
            get;
            protected set;
        }

        #endregion

        #region Overrided Methods

        /// <summary>
        /// Gets message about Solve completed successfully.
        /// </summary>
        /// <param name="schedule">Edited schedule.</param>
        /// <param name="info">Operation info.</param>
        /// <returns>Message string.</returns>
        protected override string _FormatSuccessSolveCompletedMsg(Schedule schedule, AsyncOperationInfo info)
        {
            return RoutingMessagesHelper.GetUnassignOperationCompletedMessage(info);
        }

        /// <summary>
        /// Starts operation process.
        /// </summary>
        /// <param name="args">Operation args.</param>
        /// <exception cref="Exception">Throws if any unhandles exception occurs in method.</exception>
        protected override void _Execute(params object[] args)
        {
            try
            {
                // Get current schedule.
                if (_schedulePage == null)
                    _schedulePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
                
                Schedule schedule = _schedulePage.CurrentSchedule;

                ICollection<Order> selectedOrders = _GetOrdersWhichCanBeUnassignedFromSelection(_schedulePage.SelectedItems);
                ICollection<Order> orders = RoutingCmdHelpers.GetOrdersIncludingPairs(schedule, selectedOrders);
                ICollection<Route> routes = ViolationsHelper.GetRouteForUnassignOrders(schedule, orders);

                if (_CheckRoutingParams(schedule, routes, orders))
                {
                    SolveOptions options = new SolveOptions();
                    options.GenerateDirections = App.Current.MapDisplay.TrueRoute;
                    options.FailOnInvalidOrderGeoLocation = false;

                    _SetOperationStartedStatus((string)App.Current.FindResource(UNASSIGN_ORDERS), (DateTime)schedule.PlannedDate);

                    OperationsIds.Add(App.Current.Solver.UnassignOrdersAsync(schedule, orders, options));

                    // set solve started message
                    string infoMessage = RoutingMessagesHelper.GetUnassignOperationStartedMessage(orders);

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

        #endregion

        #region ICommandOption Members

        /// <summary>
        /// Option ID. 
        /// </summary>
        public int Id
        {
            get { return 0; }
        }

        /// <summary>
        /// Group ID.
        /// </summary>
        public int GroupID
        {
            get;
            private set;
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Updates IsEnabled and tooltip properties when selection changes.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _SelectionChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        /// <summary>
        /// Updates IsEnabled state and tooltip when solve completed.
        /// </summary>
        /// <param name="sender">Solver.</param>
        /// <param name="e">Event args.</param>
        private void Solver_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            _CheckEnabled();
            _UpdateTooltip();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method checks is command enbled or not.
        /// </summary>
        private void _CheckEnabled()
        {
            bool isEnabled;

            isEnabled = (_schedulePage.SelectedItems != null && _GetOrdersWhichCanBeUnassignedFromSelection(_schedulePage.SelectedItems).Count > 0);

            if (_schedulePage.IsLocked || _schedulePage.IsEditingInProgress)
                isEnabled = false;

            IsEnabled = isEnabled;
        }

        /// <summary>
        /// Updates option's tooltip.
        /// </summary>
        private void _UpdateTooltip()
        {
            if (IsEnabled)
                TooltipText = (string)App.Current.FindResource(ENABLED_TOOLTIP);
            else
                TooltipText = (string)App.Current.FindResource(DISABLED_TOOLTIP);
            _NotifyPropertyChanged(TOOLTIP_PROPERTY_NAME);
        }

        /// <summary>
        /// Method gets orders collection from selection collection
        /// </summary>
        /// <param name="selection">Selected objects.</param>
        /// <returns>Orders which can be unassigned.</returns>
        private ICollection<Order> _GetOrdersWhichCanBeUnassignedFromSelection(IList selection)
        {
            Collection<Order> orders = new Collection<Order>();
            foreach (Object obj in selection)
            {
                // If selected object is stop and not locked - add it to collection.
                if (obj is Stop && !((Stop)obj).IsLocked && ((Stop)obj).StopType.Equals(StopType.Order))
                    orders.Add((Order)((Stop)obj).AssociatedObject);
                else if (obj is Route && !((Route)obj).IsLocked) // If selected object is route - add to collection all it's unlocked stops.
                {
                    foreach (Stop stop in ((Route)obj).Stops)
                    {
                        if (!stop.IsLocked && stop.AssociatedObject is Order)
                            orders.Add((Order)stop.AssociatedObject);
                    }
                }
            }
            return orders;
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Enabled tooltip resource.
        /// </summary>
        private const string ENABLED_TOOLTIP = "UnassignOrdersCommandEnabledTooltip";

        /// <summary>
        /// Disabled tooltip resource.
        /// </summary>
        private const string DISABLED_TOOLTIP = "UnassignOrdersCommandDisabledTooltip";

        /// <summary>
        /// Title resource.
        /// </summary>
        private const string TITLE_RESOURCE = "UnassignedOrdersOption";

        /// <summary>
        /// Unassign orders string.
        /// </summary>
        private const string UNASSIGN_ORDERS = "UnassignOrders";

        /// <summary>
        /// Tooltip property name.
        /// </summary>
        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";

        #endregion

        #region Private Fields

        /// <summary>
        /// Tooltip.
        /// </summary>
        private string _tooltipText = null;

        /// <summary>
        /// Optimize and edit page.
        /// </summary>
        OptimizeAndEditPage _schedulePage;

        #endregion
    }
}
