using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.App.Pages;
using System.Diagnostics;
using System.Collections;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Commands
{
     /// <summary>
    /// Base class for Move/Delete orders command
    /// </summary>
    internal abstract class MultiUnassignCmdBase : CommandBase
    {
        /// <summary>
        /// Struct present information about schedule where orders should be unassigned
        /// </summary>
        private class UnassignScheduleInfo
        {
            // Creates new instance of UnassignScheduleInfo and inits it's fields
            public UnassignScheduleInfo(Schedule schedule, ICollection<Order> ordersToUnassign, bool isProcessed)
            {
                _schedule = schedule;
                _ordersToUnassign = ordersToUnassign;
                _isProcessed = isProcessed;
            }

            /// <summary>
            /// Schedule where orders should be unassigned
            /// </summary>
            public Schedule Schedule
            {
                get { return _schedule; }
            }

            /// <summary>
            /// Collection of orders to unassign
            /// </summary>
            public ICollection<Order> OrdersToUnassign
            {
                get { return _ordersToUnassign; }
            }

            /// <summary>
            /// True if all necessary orders already unassigned from schedule
            /// </summary>
            public bool IsProcessed
            {
                get { return _isProcessed; }
                set { _isProcessed = value; }
            }

            private Schedule _schedule;
            private ICollection<Order> _ordersToUnassign;
            private bool _isProcessed;
        }


        #region Command Base members

        public override string Name
        {
            get { return null; }
        }

        public override string Title
        {
            get { return null; }
        }

        public override string TooltipText
        {
            get { return null; }
            protected set { }
        }

        protected override void _Execute(params object[] args)
        {
            Debug.Assert((Collection<Order>)args[0] != null);
            _initialOrdersCollection = (Collection<Order>)args[0];
            _args = args;

            // if any schedule version has assigned orders from _initialOrdersCollection collection - unassign them at first
            if (ScheduleHelper.IsAnyOrderAssignedToSchedule((IList<Order>)_initialOrdersCollection, App.Current.CurrentDate))
                _UnassignOrders(_initialOrdersCollection, App.Current.CurrentDate);
            // otherwise - start processing operation
            else
                _ProcessOrders(_args);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Abstarct method to move/remove orders (shoul be overrided in child classes)
        /// </summary>
        /// <param name="args"></param>
        protected abstract void _ProcessOrders(params object[] args);

        #endregion

        #region Protected Abstarct Properties

        /// <summary>
        /// Sets operation success started format string (should be defined in child class)
        /// </summary>
        protected abstract string OperationSuccessStartedMessage { get; }

        /// <summary>
        /// Sets operation in process format string (should be defined in child class)
        /// </summary>
        protected abstract string OperationIsFailedMessage { get; }

        /// <summary>
        /// Sets locked orders format string (should be defined in child class)
        /// </summary>
        protected abstract string OrdersAreLockedMessage { get; }

        #endregion

        #region Private Event Handlers
        /// <summary>
        /// Handle contains logic to save changes, start new operation or process errors when solve conpleted.
        /// </summary>
        /// <param name="sender">Solver.</param>
        /// <param name="e">Solve completed event args.</param>
        private void Solver_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            // If event came from any else solve operation - exit.
            if (e.OperationId != _currentOperationId)
                return;

            AsyncOperationInfo info = null;
            App.Current.Solver.GetAsyncOperationInfo(e.OperationId, out info); // Get operation info.

            // If operation complete successful.
            if (e.Error == null && !e.Cancelled && !e.Result.IsFailed)
            {
                Schedule changedSchedule = info.Schedule;
                _ProcessSaveSchedule(changedSchedule, info); // Save edited schedule.
                _SetScheduleProcessed(changedSchedule); // Set schedule Processed to "true".
                App.Current.Messenger.AddInfo(_FormatSuccessSolveCompletedMsg(changedSchedule, info)); // Add info message.
                _NotifyScheduleChanged(changedSchedule);

                UnassignScheduleInfo nextInfo = null;

                if (_GetNextScheduleToUnassign(out nextInfo))
                {
                    _StartNextScheduleUnassigning(nextInfo);
                    return;
                }

                _ProcessOrders(_args); // Call abstract method _ProcessOrders overrided in child command (to move orders to other date or delete them).
            }
            else if (e.Error != null) // If Error occured during operation.
            {
                Logger.Error(e.Error);
                CommonHelpers.AddRoutingErrorMessage(e.Error);

                if (e.Result != null) // Result is "null" when connection failed.
                {
                    // Create violations collection.
                    ICollection<MessageDetail> details = _GetErrorFailedDetails(info, e.Result);
                    _ShowFailedMessage(e.Error, info.Schedule, details); // Show failed message.
                }
            }
            else if (e.Cancelled) // If operation was cancelled.
            {
                App.Current.Messenger.AddInfo(_FormatCancelMsg(info.Schedule));
            }
            else if (e.Result.IsFailed) // If operation's failed.
            {
                // Create violations collection.
                ICollection<MessageDetail> details = _GetErrorFailedDetails(info, e.Result);
                _ShowFailedMessage(e.Error, info.Schedule, details);
            }

            _UpdateOptimizeAndEditPageSchedules();  // Update optimize and edit page content.
            _CleanUp();
            _UnlockUI();
        }

        private void Solver_AsyncSolveStarted(object sender, AsyncSolveStartedEventArgs e)
        {
            // Remember last operation ID.
            _currentOperationId = e.OperationId;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Unassign orders form all schedules
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="targetDate"></param>
        private void _UnassignOrders(ICollection<Order> orders, DateTime targetDate)
        {
            // If any orders are locked - return (messages will be added in _CheckLocked method).
            if (_CheckLocked(orders))
                return;

            // Add handlers for solver events.
            App.Current.Solver.AsyncSolveStarted += new AsyncSolveStartedEventHandler(Solver_AsyncSolveStarted);
            App.Current.Solver.AsyncSolveCompleted += new AsyncSolveCompletedEventHandler(Solver_AsyncSolveCompleted);

            ICollection<Schedule> schedules = App.Current.Project.Schedules.Search(targetDate);

            // Init collection of pairs "schedule" - "orders to unassign".
            foreach (Schedule schedule in schedules)
            {
                ICollection<Order> ordersToUnassign = _CreateOrdersToUnassignCollection(_initialOrdersCollection, schedule);
                if (ordersToUnassign.Count > 0)
                    _schedulesToUnassign.Add(new UnassignScheduleInfo(schedule, ordersToUnassign, false));
            }

            // Lock UI and call UnassignOrders for first schedule.
            App.Current.MainWindow.Lock(true);
            _UnassignOrdersFromSchedule(_schedulesToUnassign[0].Schedule, _schedulesToUnassign[0].OrdersToUnassign);
        }

        /// <summary>
        /// Method checks is any order in collection locked and shows corresponding messages in message window 
        /// </summary>
        /// <returns></returns>
        private bool _CheckLocked(ICollection<Order> orders)
        {
            bool result = false;

            // Get collection of locked stops and appropriate orders.
            Dictionary<Schedule, Collection<Order>> schedulesWithLockedOrders = ScheduleHelper.GetLockedOrdersSchedules(orders, App.Current.CurrentDate);
            if (schedulesWithLockedOrders.Count == 0)
                return false;

            Collection<MessageDetail> details = new Collection<MessageDetail>();

            // Create MessageDetails objects with necessary links to locked orders.
            foreach (KeyValuePair<Schedule, Collection<Order>> pair in schedulesWithLockedOrders)
            {
                StringBuilder sb = new StringBuilder();

                foreach (Order order in (Collection<Order>)pair.Value)
                {
                    if (0 < sb.Length)
                        sb.Append(", ");
                    sb.Append(order.Name);
                }

                // Create message.
                string message = string.Format((string)App.Current.FindResource("ListOfLockedOrdersMessage"), sb.ToString(), pair.Key.Name);
                details.Add(new MessageDetail(MessageType.Error, message));
            }
            App.Current.Messenger.AddError(OrdersAreLockedMessage, details);
            result = true;

            return result;
        }

        /// <summary>
        /// Method select orders, assigned on current schedule from all orders for unassign.
        /// </summary>
        /// <param name="initialOrders">Input orders collection.</param>
        /// <param name="schedule">Schedule where from orders should be unassigned.</param>
        /// <returns>Collection of orders to unassign.</returns>
        private ICollection<Order> _CreateOrdersToUnassignCollection(ICollection<Order> initialOrders, Schedule schedule)
        {
            Debug.Assert(schedule.Routes != null);

            Collection<Order> ordersToUnassign = new Collection<Order>();

            foreach (Route route in schedule.Routes)
            {
                if (route.Stops != null)
                {
                    foreach (Stop stop in route.Stops)
                    {
                        if (stop.AssociatedObject is Order && initialOrders.Contains((Order)stop.AssociatedObject))
                        {
                            ordersToUnassign.Add((Order)stop.AssociatedObject);
                        }
                    }
                }
            }

            return ordersToUnassign;
        }

        /// <summary>
        /// Method unassignes orders from schedule
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="schedule"></param>
        private void _UnassignOrdersFromSchedule(Schedule schedule, ICollection<Order> orders)
        {
            try
            {
                ICollection<Order> ordersWithPairs = RoutingCmdHelpers.GetOrdersIncludingPairs(schedule, orders);

                // Create routes collection.
                ICollection<Route> routes = ViolationsHelper.GetRouteForUnassignOrders(schedule, ordersWithPairs);

                if (_CheckRoutingParams(schedule, ordersWithPairs, routes))
                {
                    SolveOptions options = new SolveOptions();
                    options.GenerateDirections = App.Current.MapDisplay.TrueRoute;
                    options.FailOnInvalidOrderGeoLocation = false;

                    string infoMessage = _FormatSuccessUnassigningStartedMessage(schedule, _schedulesToUnassign);

                    // Set operation info status.
                    _SetOperationStartedStatus(infoMessage, (DateTime)schedule.PlannedDate);

                    // Start solve operation.
                    App.Current.Solver.UnassignOrdersAsync(schedule, ordersWithPairs, options);
                }
                else // If routing operation was not started - clean collections and unlock UI.
                {
                    _CleanUp();
                    _UnlockUI();
                }
            }
            catch (RouteException e)
            {
                App.Current.Messenger.AddError(RoutingCmdHelpers.FormatRoutingExceptionMsg(e));
                App.Current.Messenger.AddError(string.Format(OperationIsFailedMessage, schedule));

                // Save already edited schedules.
                _UpdateOptimizeAndEditPageSchedules();
                _CleanUp();
                _UnlockUI();
            }
            catch (Exception ex)
            {
                // Save already edited schedules.
                _UpdateOptimizeAndEditPageSchedules();
                _CleanUp();
                _UnlockUI();

                if ((ex is LicenseException) || (ex is AuthenticationException) || (ex is CommunicationException))
                    CommonHelpers.AddRoutingErrorMessage(ex);
                else
                    throw;                
            }
        }

        /// <summary>
        /// Method checks params of routing operation and show warning message with details if parameters are invalid
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="orders"></param>
        /// <param name="routes"></param>
        /// <returns></returns>
        private bool _CheckRoutingParams(Schedule schedule, ICollection<Order> orders, ICollection<Route> routes)
        {
            bool isValid = false;
            List<DataObject> invalidObjects = new List<DataObject>();
            invalidObjects.AddRange(RoutingCmdHelpers.ValidateObjects<Order>(orders));
            invalidObjects.AddRange(RoutingCmdHelpers.ValidateObjects<Route>(routes));
            if (invalidObjects.Count > 0)
            {
                RoutingSolveValidator validator = new RoutingSolveValidator();
                ICollection<MessageDetail> details = validator.GetValidateMessageDetail(invalidObjects.ToArray());
                string invalidOperationTitle = ((string)App.Current.FindResource("SolveValidationOperationInvalid"));
                App.Current.Messenger.AddMessage(MessageType.Error, invalidOperationTitle, details);
            }
            else
                isValid = true;

            return isValid;
        }

        /// <summary>
        /// Method saves schedule 
        /// </summary>
        private void _ProcessSaveSchedule(Schedule schedule, AsyncOperationInfo info)
        {
            // set unassigned orders
            if (null != schedule.UnassignedOrders)
                schedule.UnassignedOrders.Dispose();
            schedule.UnassignedOrders = App.Current.Project.Orders.SearchUnassignedOrders(schedule, true);

            // save results
            App.Current.Project.Save();
        }

        /// <summary>
        /// Method sets schedule Processed status to true
        /// </summary>
        private void _SetScheduleProcessed(Schedule schedule)
        {
            int scheduleInfoIndex = _GetInfoIndexBySchedule(schedule);
            if (scheduleInfoIndex != -1)
            {
                UnassignScheduleInfo processedInfo = _schedulesToUnassign[scheduleInfoIndex];
                processedInfo.IsProcessed = true;
                _schedulesToUnassign[scheduleInfoIndex] = processedInfo;
            }
        }

        /// <summary>
        /// Method search UnassignScheduleInfo by schedule in _schedulesToUnassign and return it's index
        /// if no info found - return -1;
        /// </summary>
        /// <param name="schedule">Schedule to find.</param>
        /// <returns>Index of schedule to unassign or -1 if schedule not found.</returns>
        private int _GetInfoIndexBySchedule(Schedule schedule)
        {
            foreach (UnassignScheduleInfo info in _schedulesToUnassign)
            {
                if (info.Schedule == schedule)
                    return _schedulesToUnassign.IndexOf(info);
            }
            return -1;
        }

        /// <summary>
        /// Returns true if next UnassignScheduleInfo is found and sets its into out object.
        /// Otherwise returns false and sets out object to null.
        /// </summary>
        /// <param name="info">Schedule info.</param>
        /// <returns>True - if next schedule found, otherwise - false.</returns>
        private bool _GetNextScheduleToUnassign(out UnassignScheduleInfo info)
        {
            foreach (UnassignScheduleInfo scheduleInfo in _schedulesToUnassign)
            {
                if (!scheduleInfo.IsProcessed)
                {
                    info = scheduleInfo;
                    return true;
                }
            }

            info = null;
            return false;
        }

        /// <summary>
        /// Method start unassign orders from next schedule from list.
        /// </summary>
        /// <param name="schedule"></param>
        /// <returns></returns>
        private void _StartNextScheduleUnassigning(UnassignScheduleInfo info)
        {
            _UnassignOrdersFromSchedule(info.Schedule, info.OrdersToUnassign);
        }

        /// <summary>
        /// Method reloads all schedules in optimize and edit page
        /// </summary>
        private void _UpdateOptimizeAndEditPageSchedules()
        {
            OptimizeAndEditPage page = (OptimizeAndEditPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            page.SetRoutingStatus(string.Empty, App.Current.CurrentDate);

            Debug.Assert(page != null);
            foreach (UnassignScheduleInfo info in _schedulesToUnassign)
            {
                // Refresh unassigned orders collection if schedule was changed
                if (info.IsProcessed)
                {
                    if (info.Schedule.UnassignedOrders == null)
                    {
                        info.Schedule.UnassignedOrders = App.Current.Project.Orders.SearchUnassignedOrders(info.Schedule, true);
                    }
                }
            }
        }

        /// <summary>
        /// Remove redundant event handlers and unlock UI
        /// </summary>
        private void _UnlockUI()
        {
            App.Current.MainWindow.Unlock(); // unlock UI
        }

        /// <summary>
        /// Creates collection of message detail when solve operation failed
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="info"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private ICollection<MessageDetail> _GetErrorFailedDetails(AsyncOperationInfo info, SolveResult result)
        {
            Debug.Assert(info != null);
            Debug.Assert(result != null);

            // Create details.
            ICollection<MessageDetail> details = null;

            if (0 < result.Violations.Count)
                details = ViolationMessageBuilder.GetViolationDetails(info.Schedule, info, result.Violations);
            else
            {
                // Create details from solver error message.
                details = new Collection<MessageDetail>();
                details.Add(new MessageDetail(MessageType.Error, RoutingCmdHelpers.FormatSolverErrorMsg(result)));
            }

            return details;
        }

        /// <summary>
        /// Shows error and failed messages with details
        /// </summary>
        private void _ShowFailedMessage(Exception error,
            Schedule schedule, ICollection<MessageDetail> details)
        {
            string message = string.Format(
                (string)App.Current.FindResource(UNASSIGN_OPERATION_FAILED),
                ((DateTime)schedule.PlannedDate).ToShortDateString(),
                schedule.Name);
            App.Current.Messenger.AddError(message, details);
            App.Current.Messenger.AddError(string.Format(OperationIsFailedMessage, schedule));
        }

        /// <summary>
        /// Method cleans all collections
        /// </summary>
        private void _CleanUp()
        {
            _initialOrdersCollection.Clear();
            _schedulesToUnassign.Clear();

            // remove handlers to cancel react to solve events
            App.Current.Solver.AsyncSolveStarted -= Solver_AsyncSolveStarted;
            App.Current.Solver.AsyncSolveCompleted -= Solver_AsyncSolveCompleted;
        }

        /// <summary>
        /// Notifies application about specified schedule changes.
        /// </summary>
        /// <param name="changedSchedule">The reference to the changed schedule object.</param>
        private void _NotifyScheduleChanged(Schedule changedSchedule)
        {
            Debug.Assert(changedSchedule != null);
            var optimizePage = (OptimizeAndEditPage)_Application.MainWindow.GetPage(
                PagePaths.SchedulePagePath);
            if (changedSchedule == optimizePage.CurrentSchedule)
            {
                optimizePage.OnScheduleChanged(optimizePage.CurrentSchedule);
            }
        }

        #endregion

        #region Private Messages Methods

        /// <summary>
        /// Creates success unassigned complete message
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private string _FormatSuccessSolveCompletedMsg(
            Schedule schedule, AsyncOperationInfo info)
        {
            return RoutingMessagesHelper.GetMultipleUnassignOperationCompletedMessage(schedule, info);
        }

        /// <summary>
        /// Method returns cancel message string.
        /// </summary>
        /// <returns>Message for route operation cancel.</returns>
        private string _FormatCancelMsg(Schedule schedule)
        {
            return string.Format(
                (string)App.Current.FindResource(ROUTES_OPERATION_CANCELLED),
                schedule.PlannedDate.Value.ToShortDateString());
        }

        /// <summary>
        /// Creates start unassigning status message
        /// </summary>
        /// <param name="schedule"></param>
        /// <param name="schedules"></param>
        /// <returns></returns>
        private string _FormatSuccessUnassigningStartedMessage(Schedule schedule, ICollection<UnassignScheduleInfo> schedules)
        {
            return string.Format(OperationSuccessStartedMessage, _GetInfoIndexBySchedule(schedule) + 1, schedules.Count, schedule.Name);
        }

        /// <summary>
        /// Sets status to status bar in OptimizeAndEditPage
        /// </summary>
        /// <param name="statusString">Status string.</param>
        private void _SetOperationStartedStatus(string statusString, DateTime date)
        {
            OptimizeAndEditPage optimizeAndEditPage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);
            optimizeAndEditPage.SetRoutingStatus(statusString, date);
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Routes operation cancelled.
        /// </summary>
        private const string ROUTES_OPERATION_CANCELLED = "RoutesOperationCancelledText";

        /// <summary>
        /// Unassign operation failed.
        /// </summary>
        private const string UNASSIGN_OPERATION_FAILED = "UnassigningFailedMessage";

        #endregion

        #region Private Fields

        // saved last operatio ID
        private Guid _currentOperationId = default(Guid);

        // list of all schedules for current date which contains necessary orders in stops
        private Collection<UnassignScheduleInfo> _schedulesToUnassign = new Collection<UnassignScheduleInfo>();

        // input orders collection
        private Collection<Order> _initialOrdersCollection = new Collection<Order>();

        // array of input args
        private object[] _args;

        #endregion
    }
}
