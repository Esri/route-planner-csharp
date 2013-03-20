using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Documents;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Base class for routing commands
    /// </summary>
    abstract class RoutingCommandBase : CommandBase
    {
        public RoutingCommandBase()
        {
        }

        public override void Initialize(ESRI.ArcLogistics.App.App app)
        {
            base.Initialize(app);
            App.Current.CurrentDateChanged += new EventHandler(RoutingCommandBase_CurrentDateChanged);
            App.Current.Solver.AsyncSolveStarted += new AsyncSolveStartedEventHandler(Cmd_AsyncSolveStarted);
            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(Cmd_AsyncSolveCompleted);
        }

        #region Public Properties

        public override bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set 
            {
                _isEnabled = value;
                _NotifyPropertyChanged("IsEnabled");
            }
        }

        #endregion

        #region Event handlers

        void RoutingCommandBase_CurrentDateChanged(object sender, EventArgs e)
        {
            if (App.Current.Solver.GetAsyncOperations(App.Current.CurrentDate).Count > 0)
                IsEnabled = false;
            else
                IsEnabled = true;
        }

        protected void Cmd_AsyncSolveStarted(object sender, AsyncSolveStartedEventArgs e)
        {
            IsEnabled = false;
        }

        protected void Cmd_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            // check is completed operation was started by this command
            if (OperationsIds.Contains(e.OperationId))
            {
                _operationsIds.Remove(e.OperationId);
                AsyncOperationInfo info = null;
                Schedule schedule = null;
                if (App.Current.Solver.GetAsyncOperationInfo(e.OperationId, out info))
                    schedule = info.Schedule;

                IsEnabled = true;

                if (e.Cancelled)
                    App.Current.Messenger.AddInfo(_FormatCancelMsg(schedule));
                else if (e.Error != null)
                {
                    _HandleSolveError(e.Error);
                }
                else
                {
                    SolveResult res = e.Result;
                    if (schedule != null)
                        _SaveSchedule(res, schedule, info);
                }
            }
        }
        #endregion

        #region Protected Methods

        /// <summary>
        /// Sets status to status bar in OptimizeAndEditPage
        /// </summary>
        /// <param name="statusString"></param>
        protected void _SetOperationStartedStatus(string statusString, DateTime date)
        {
            OptimizeAndEditPage optimizeAndEditPage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            optimizeAndEditPage.SetRoutingStatus(statusString, date);
        }

        /// <summary>
        /// Saves new schedule if it's build correctly or show error message.
        /// </summary>
        /// <param name="res">Solve result.</param>
        /// <param name="schedule">Schedule.</param>
        /// <param name="info">Operation info.</param>
        protected void _SaveSchedule(SolveResult res, Schedule schedule, AsyncOperationInfo info)
        {
            // if solver returns "failed"
            if (res.IsFailed)
            {
                // show result message(s)
                string message = string.Format(
                    (string)App.Current.FindResource(ROUTING_OPERATION_FAILED),
                    schedule.PlannedDate.Value.ToShortDateString());
                ICollection<MessageDetail> details = null;
                if (0 < res.Violations.Count)
                    details = ViolationMessageBuilder.GetViolationDetails(schedule, info, res.Violations);
                // Create details from solver error message.
                else
                {
                    details = new Collection<MessageDetail>();
                    var errorMessage = RoutingCmdHelpers.FormatSolverErrorMsg(res);

                    // If we have error message - add message detail.
                    if(errorMessage.Length != 0)
                        details.Add(new MessageDetail(MessageType.Error, errorMessage));
                }

                App.Current.Messenger.AddError(message, details);
            }
            else
            {
                // set unassigned orders
                if (null != schedule.UnassignedOrders)
                    schedule.UnassignedOrders.Dispose();

                // save route results
                Project project = App.Current.Project;
                project.Save();

                schedule.UnassignedOrders = project.Orders.SearchUnassignedOrders(schedule, true);

                // In case operation is Build Routes we should create "Original Schedule" if doesn't exist yet.
                // Schedule must be current.
                if (info.OperationType == SolveOperationType.BuildRoutes)
                {
                    if (schedule.Type == ScheduleType.BuildRoutesSnapshot)
                    {
                        // update current schedule (according to CR133526)
                        _UpdateSchedule(ScheduleType.Current, schedule,
                            (string)App.Current.FindResource("CurrentScheduleName"));

                        // update build routes snapshot name
                        schedule.Name = _FormatBuildRoutesSnapshotName();
                    }
                    else
                    {
                        // update build routes snapshot
                        _UpdateSchedule(ScheduleType.BuildRoutesSnapshot, schedule, _FormatBuildRoutesSnapshotName());
                    }

                    // save changes
                    project.Save();
                }

                // Show result message(s).
                if (0 < res.Violations.Count)
                {
                    _ShowPartialSuccessOperationResults(res.Violations, schedule, info);
                }
                else
                {
                    // If operation type is Assign To Routes
                    // show that Order was assigned to that Route
                    if (info.OperationType.Equals(SolveOperationType.AssignOrders))
                        _ShowSuccessfullyAssignedOrders(schedule, info);
                    else
                        App.Current.Messenger.AddInfo(
                            _FormatSuccessSolveCompletedMsg(schedule, info));
                }
            }

            OptimizeAndEditPage page = (OptimizeAndEditPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            if (null != page)
                page.OnScheduleChanged(schedule);
        }

        /// <summary>
        /// Method shows that Order was assigned to that Route
        /// in message window.
        /// </summary>
        /// <param name="schedule">Schedule.</param>
        /// <param name="info">Operation info.</param>
        protected void _ShowSuccessfullyAssignedOrders(Schedule schedule, AsyncOperationInfo info)
        {
            var assignOrdersDetails =
                _GetSuccessfullyAssignedOrdersDetails(schedule, info);

            // Add success details only in case of more than 1 orders assigned,
            // otherwise head message is good enough.
            if (assignOrdersDetails.Count > 1)
            {
                App.Current.Messenger.AddInfo(
                    _FormatSuccessSolveCompletedMsg(schedule, info),
                    assignOrdersDetails);
            }
            else
            {
                assignOrdersDetails.Clear();
                App.Current.Messenger.AddInfo(
                    _FormatSuccessSolveCompletedMsg(schedule, info),
                    assignOrdersDetails);
            }
        }

        /// <summary>
        /// Method returns cancel message string
        /// </summary>
        /// <returns></returns>
        protected string _FormatCancelMsg(Schedule schedule)
        {
            string result = string.Format(
                (string)App.Current.FindResource(ROUTES_OPERATION_CANCELLED),
                schedule.PlannedDate.Value.ToShortDateString());
            return result;
        }

        /// <summary>
        /// Method shows error message in message window
        /// </summary>
        /// <param name="message"></param>
        protected void _ShowErrorMsg(string message)
        {
            App.Current.Messenger.AddError(message);
        }

        /// <summary>
        /// Shows solve validation dialog
        /// </summary>
        /// <param name="invalidObjects"></param>
        protected void _ShowSolveValidationResult(ESRI.ArcLogistics.Data.DataObject[] invalidObjects)
        {
            RoutingSolveValidator validator = new RoutingSolveValidator();
            validator.Validate(invalidObjects);
        }

        /// <summary>
        /// Success solve completed message string is various for each command
        /// </summary>
        /// <returns></returns>
        protected abstract string _FormatSuccessSolveCompletedMsg(Schedule schedule, AsyncOperationInfo info);

        protected virtual Collection<Guid> OperationsIds
        {
            get { return _operationsIds; }
            set { _operationsIds = value; }
        }

        protected bool _CheckRoutingParams(Schedule schedule, ICollection<Route> routes, ICollection<Order> orders)
        {
            bool isValid = false;

            // check orders count
            if (orders.Count < 1)
                App.Current.Messenger.AddError((string)App.Current.FindResource("Error_InvalidOrdersNum"));
            // check routes count
            else if (routes.Count < 1)
                App.Current.Messenger.AddError((string)App.Current.FindResource("Error_InvalidRoutesNum"));
            // validate objects
            else
            {
                List<DataObject> invalidObjects = new List<DataObject>();
                invalidObjects.AddRange(RoutingCmdHelpers.ValidateObjects<Order>(orders));
                invalidObjects.AddRange(RoutingCmdHelpers.ValidateObjects<Route>(routes));
                if (invalidObjects.Count > 0)
                    _ShowSolveValidationResult(invalidObjects.ToArray());
                else
                    isValid = true;
            }

            return (isValid) ? ConstraintViolationsChecker.Check(schedule, routes, orders) : false;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the first schedule of specified type with a new value and name.
        /// </summary>
        private void _UpdateSchedule(ScheduleType scheduleTypeToUpdate, Schedule newValue, string newName)
        {
            // get all schedules
            Project project = App.Current.Project;
            IDataObjectCollection<Schedule> schedules = project.Schedules.Search(newValue.PlannedDate.Value, false);

            // and try to find schedule of specified type. 
            Schedule oldSchedule = null;
            foreach(Schedule schedule in schedules)
                if (schedule.Type == scheduleTypeToUpdate)
                {
                    oldSchedule = schedule;
                    break;
                }

            // delete old schedule if present
            if (oldSchedule != null)
                project.Schedules.Remove(oldSchedule);

            // create new schedule and add it to the project
            Schedule updatedSchedule = (Schedule)newValue.Clone();
            updatedSchedule.Type = scheduleTypeToUpdate;
            updatedSchedule.Name = newName;

            project.Schedules.Add(updatedSchedule);
        }

        /// <summary>
        /// Returns build routes snapshot name
        /// </summary>
        private string _FormatBuildRoutesSnapshotName()
        {
            string fmtString = (string)App.Current.FindResource("BuildRoutesSnapshotNameFormat");
            DateTime currentTime = DateTime.Now;

            return String.Format(fmtString, currentTime.ToShortDateString(), currentTime.ToShortTimeString());
        }

        /// <summary>
        /// Handles exception thrown from a solve operation.
        /// </summary>
        /// <param name="exception">The exception thrown from a solve operation.</param>
        private void _HandleSolveError(Exception exception)
        {
            Debug.Assert(exception != null);

            var routeException = exception as RouteException;
            if (routeException != null)
            {
                if (routeException.InvalidObjects != null)
                {
                    // exception was thrown because any Routes or Orders are invalid.
                    _ShowSolveValidationResult(routeException.InvalidObjects);
                }
                else
                    _ShowErrorMsg(RoutingCmdHelpers.FormatRoutingExceptionMsg(routeException));

                return;
            }

            // If we have error in sync response - show it.
            var restException = exception as RestException;
            if (restException != null)
            {
                var details = new List<MessageDetail>();

                // If exception has details- process them.
                if (restException.Details != null)
                {
                    foreach (var detail in restException.Details)
                    {
                        var detailMessage = GuidsReplacer.ReplaceGuids(detail, App.Current.Project);
                        details.Add(new MessageDetail(MessageType.Error, detailMessage));
                    }
                }

                // Process exception message.
                var message = GuidsReplacer.ReplaceGuids(exception.Message, App.Current.Project);
                
                // Show error in messenger.
                App.Current.Messenger.AddError(message, details);

                return;
            }

            Logger.Error(exception);
            CommonHelpers.AddRoutingErrorMessage(exception);
        }

        /// <summary>
        /// Method shows result messages for operation with Parial Success.
        /// </summary>
        /// <param name="violations">List of operations occured.</param>
        /// <param name="schedule">Schedule.</param>
        /// <param name="info">Operation info.</param>
        private void _ShowPartialSuccessOperationResults(IList<Violation> violations,
            Schedule schedule, AsyncOperationInfo info)
        {
            // Add violations results.
            ICollection<MessageDetail> details =
                ViolationMessageBuilder.GetViolationDetails(schedule,
                info, violations);

            // For Build Routes or Assign to Best Route operations
            // after violations list add successfully moved orders results.
            if (info.OperationType == SolveOperationType.BuildRoutes)
            {
                var routedOrdersDetails =
                    _GetSuccessfullyAssignedOrdersDetails(schedule, info);

                foreach (var detail in routedOrdersDetails)
                    details.Add(detail);
            }
            else if (info.OperationType == SolveOperationType.AssignOrders)
            {
                AssignOrdersParams parameters = (AssignOrdersParams)info.InputParams;

                // Is this operation is targeted to Best Route.
                if (parameters.TargetRoutes.Count > 1)
                {
                    var assignedOrdersDetails =
                        _GetSuccessfullyAssignedOrdersDetails(schedule, info);

                    foreach (var detail in assignedOrdersDetails)
                        details.Add(detail);
                }
            }

            App.Current.Messenger.AddMessage(
                _FormatSuccessSolveCompletedMsg(schedule, info), details);
        }

        
        /// <summary>
        /// Method gets details messages about successfully assigned orders during
        /// routing operation. Details message are expanded by route names.
        /// </summary>
        /// <param name="schedule">Schedule.</param>
        /// <param name="info">Operation info.</param>
        /// <returns>Collection of message details.</returns>
        private ICollection<MessageDetail> _GetSuccessfullyAssignedOrdersDetails(
            Schedule schedule, AsyncOperationInfo info)
        {
            var routedOrdersDetails = new Collection<MessageDetail>();
            ICollection<Order> ordersToAssign = _GetOrdersToAssign(info);

            if (ordersToAssign == null)
                return routedOrdersDetails;

            // Find all pairs Route & Order.
            var detailsPairs = new List<KeyValuePair<Route, Order>>();

            foreach (Order order in ordersToAssign)
                foreach (Route route in schedule.Routes)
                    foreach (Stop stop in route.Stops)
                        if (stop.AssociatedObject is Order &&
                            ((Order)stop.AssociatedObject).Equals(order))
                        {
                            var pair = new KeyValuePair<Route, Order>(
                                route, order);

                            detailsPairs.Add(pair);
                        }

            string formatString =
                (string)App.Current.FindResource(ORDERS_SUCCESSFULLY_ASSIGNED);

            // Add messages expanded by Routes.
            foreach (Route route in schedule.Routes)
                foreach (KeyValuePair<Route, Order> pair in detailsPairs)
                {
                    if (pair.Key.Name == route.Name)
                    {
                        DataObject[] parameters = new DataObject[] { pair.Value, pair.Key };
                        MessageDetail detail = new MessageDetail(MessageType.Information,
                            formatString, parameters);
                        routedOrdersDetails.Add(detail);
                    }
                }

            return routedOrdersDetails;
        }

        /// <summary>
        /// Method gets orders sent for routing operation, if appropriate operation
        /// support orders for assigning.
        /// </summary>
        /// <param name="info">Operation information.</param>
        /// <returns>Collection of orders to assign or null if operation
        /// doesn't support orders to assign.</returns>
        private ICollection<Order> _GetOrdersToAssign(AsyncOperationInfo info)
        {
            Debug.Assert(info != null);

            ICollection<Order> orders = null;

            if (info.OperationType == SolveOperationType.BuildRoutes)
            {
                BuildRoutesParameters parameters = (BuildRoutesParameters)info.InputParams;
                orders = parameters.OrdersToAssign;
            }
            else if (info.OperationType == SolveOperationType.AssignOrders)
            {
                AssignOrdersParams parameters = (AssignOrdersParams)info.InputParams;
                orders = parameters.OrdersToAssign;
            }
            else
            {
                // Do nothing: other operation doesn't support OrdersToAssign.
            }

            return orders;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Order successfully assigned to route.
        /// </summary>
        private const string ORDERS_SUCCESSFULLY_ASSIGNED = "OrderSuccessfullyAssignedToRoute";

        /// <summary>
        /// Routes operation cancelled.
        /// </summary>
        private const string ROUTES_OPERATION_CANCELLED = "RoutesOperationCancelledText";

        /// <summary>
        /// Routing operation failed.
        /// </summary>
        private const string ROUTING_OPERATION_FAILED = "RoutingOperationsFailedMessage";

        #endregion

        #region Private Fields

        private Collection<Guid> _operationsIds = new Collection<Guid>(); 
        private bool _isEnabled = true;

        #endregion
    }
}

