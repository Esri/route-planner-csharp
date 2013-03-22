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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using System.Diagnostics;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command assigns orders to a drop position on any view in Optimize and Edit page.
    /// </summary>
    class DropOrdersCmd : RoutingCommandBase
    {
        #region ICommand Override Memebers

        public override string Name
        {
            get { return ""; }
        }

        public override string Title
        {
            get { return ""; }
        }

        public override string TooltipText
        {
            get { return null; }
            protected set { }
        }

        #endregion

        #region Override Members

        /// <summary>
        /// Starts command executing.
        /// </summary>
        /// <param name="args">Command args.</param>
        protected override void _Execute(params object[] args)
        {
            // Get OptimizeAndEdit page.
            OptimizeAndEditPage schedulePage = ((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath) as OptimizeAndEditPage;
            Debug.Assert(schedulePage != null);

            // If editing is in progress - try to save editing.
            if (schedulePage.IsEditingInProgress)
            {
                // If editing cannot be saved - cancel editing.
                if (!schedulePage.SaveEditedItem())
                    schedulePage.CancelObjectEditing();
            }

            // Orders to assign.
            Collection<Order> ordersToAssign = (Collection<Order>)args[0];
            Debug.Assert(ordersToAssign != null);

            // Target object.
            Object target = (Object)args[1];

            // Include paired orders unless we are moving one order within the same route
            bool movingSingleOrderInPair = false;
            if (ordersToAssign.Count == 1 && (target is Stop))
            {
                Route targetRoute = (target as Stop).Route;
                Order orderToAssign = ordersToAssign.ElementAt(0);
                foreach (Stop stop in targetRoute.Stops)
                {
                    Order associatedOrder = stop.AssociatedObject as Order;
                    if (associatedOrder == null)
                        continue;

                    if (associatedOrder == orderToAssign)
                    {
                        movingSingleOrderInPair = true;
                        break;
                    }
                }
            }

            if (!movingSingleOrderInPair)
                ordersToAssign = RoutingCmdHelpers.GetOrdersIncludingPairs(schedulePage.CurrentSchedule, ordersToAssign) as Collection<Order>;

            int? sequence = null;
            if (target is Stop)
            {
                // Set sequence only if there is a single order to assign.
                if (ordersToAssign.Count == 1)
                    sequence = (target as Stop).SequenceNumber;
                else // Otherwise change target to route. For more details see CR161083.
                    target = (target as Stop).Route;
            }
            
            // Get target routes.
            ICollection<Route> targetRoutes = _CreateTargetRoutes(target);

            try
            {
                // If target routes count is 0 - orders must be unassigned.
                if (targetRoutes.Count == 0)
                {
                    // Start unassigning orders.
                    _UnassignOrders(ordersToAssign);
                }
                else
                {
                    // Start moving orders.
                    _MoveOrders(ordersToAssign, targetRoutes, sequence);
                }
            }
            catch (RouteException e)
            {
                if (e.InvalidObjects != null) // If exception throw because any Routes or Orders are invalid.
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

        /// <summary>
        /// Method formats success message.
        /// </summary>
        /// <param name="schedule">Edited schedule.</param>
        /// <param name="info">Operation info.</param>
        /// <returns>Info string.</returns>
        protected override string _FormatSuccessSolveCompletedMsg(Schedule schedule, AsyncOperationInfo info)
        {
            string message = string.Empty;

            // If oreders was assigned to any route.
            if (info.OperationType == SolveOperationType.AssignOrders)
                message = RoutingMessagesHelper.GetAssignOperationCompletedMessage(info);

            // If orders was unassigned.
            else if (info.OperationType == SolveOperationType.UnassignOrders)
                message = RoutingMessagesHelper.GetUnassignOperationCompletedMessage(info);

            else
            {
                // Not supported yet.
                Debug.Assert(false);
            }

            return message;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method removes all redundant orders from collection to assign.
        /// </summary>
        /// <param name="targetRoutes">Routes where orders should be assigned.</param>
        /// <param name="sequence">Sequence number.</param>
        private void _RemoveRedundantOrders(ref Collection<Order> ordersToAssign, ICollection<Route> targetRoutes, int? sequence)
        {
            Collection<Order> orders = new Collection<Order>();

            // if we try to drop more than one order we should remove all orders which contains in routes
            if (ordersToAssign.Count > 1)
            {
                foreach (Route route in targetRoutes)
                {
                    foreach (Order order in ordersToAssign)
                    {
                        if (CommonHelpers.GetBoundStop(route, order) == null)
                            orders.Add(order);
                    }
                }
                ordersToAssign.Clear();
                ordersToAssign = orders;
            }
            // if we try to drop only one order we should check It's sequence number
            // if after routing operation sequence number won't change - remove order from collection
            else
            {
                foreach (Route route in targetRoutes)
                {
                    Stop stop = CommonHelpers.GetBoundStop(route, ordersToAssign[0]);

                    if (stop != null)
                    {
                        if (sequence == stop.SequenceNumber || stop.SequenceNumber + NEXT_SEQUENCE_INCREMENT == sequence)
                            ordersToAssign.RemoveAt(0);
                    }
                }
            }
        }

        /// <summary>
        /// Removes unassigned orders from collection.
        /// </summary>
        /// <param name="ordersToAssign">Dragged orders collection.</param>
        /// <param name="currentSchedule">Current schedule.</param>
        private void _RemoveUnassignedOrders(ref Collection<Order> ordersToAssign, Schedule currentSchedule)
        {
            Collection<Order> orders = new Collection<Order>();
            foreach (Order order in ordersToAssign)
            {
                if (currentSchedule.UnassignedOrders.Contains(order))
                    orders.Add(order);
            }

            foreach (Order order in orders)
                ordersToAssign.Remove(order);
        }

        /// <summary>
        /// Creates target Routes collection and define sequence parameter
        /// </summary>
        /// <param name="target">Target object.</param>
        /// <returns>Collection of target routes.</returns>
        private Collection<Route> _CreateTargetRoutes(Object target)
        {
            Collection<Route> targetRoutes = new Collection<Route>();

            if (target is Route)
                targetRoutes.Add(((Route)target));
            else if (target is Stop)
                targetRoutes.Add(((Stop)target).Route);
            else
            {
                // Target is not Stop or Route that means we need to unassign orders.
            }

            return targetRoutes;
        }

        /// <summary>
        /// Method starts moving orders.
        /// </summary>
        /// <param name="ordersToAssign">Collection of orders to move.</param>
        /// <param name="targetRoutes">Target routes.</param>
        /// <param name="sequence">Order sequence. Can be null.</param>
        private void _MoveOrders(Collection<Order> ordersToAssign, ICollection<Route> targetRoutes, int? sequence)
        {
            _RemoveRedundantOrders(ref ordersToAssign, targetRoutes, sequence);

            // if collection of dropping orders is empty - return
            if (ordersToAssign.Count == 0)
                return;

            // get current schedule
            OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)(App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            Schedule schedule = schedulePage.CurrentSchedule;

            if (_CheckRoutingParams(schedule, targetRoutes, ordersToAssign))
            {
                SolveOptions options = new SolveOptions();
                options.GenerateDirections = App.Current.MapDisplay.TrueRoute;
                options.FailOnInvalidOrderGeoLocation = false;

                // Update status.
                _SetOperationStartedStatus((string)App.Current.FindResource("AssignOrders"), (DateTime)schedule.PlannedDate);

                // Start solve.
                OperationsIds.Add(App.Current.Solver.AssignOrdersAsync(schedule, ordersToAssign,
                                                                       targetRoutes, sequence,
                                                                       false, options));

                IEnumerator<Route> rtEnum = targetRoutes.GetEnumerator();
                rtEnum.MoveNext();

                // Set solve started message.
                string infoMessage = RoutingMessagesHelper.GetAssignOperationStartedMessage(
                    ordersToAssign, rtEnum.Current.Name);

                if (!string.IsNullOrEmpty(infoMessage))
                    App.Current.Messenger.AddInfo(infoMessage);
            }
        }

        /// <summary>
        /// Method starts unassigning orders.
        /// </summary>
        /// <param name="ordersToAssign">Collection of orders to unassign.</param>
        private void _UnassignOrders(Collection<Order> ordersToUnassign)
        {
            // Get current schedule.
            OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)(App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath);
            Schedule schedule = schedulePage.CurrentSchedule;

            // Remove unassigned orders from collection of orders to unassign.
            _RemoveUnassignedOrders(ref ordersToUnassign, schedule);

            // If all orders in selection are unassigned - just return.
            if (ordersToUnassign.Count == 0)
                return;

            ICollection<Route> routes = ViolationsHelper.GetRouteForUnassignOrders(schedule, ordersToUnassign);

            if (_CheckRoutingParams(schedule, routes, ordersToUnassign))
            {
                SolveOptions options = new SolveOptions();
                options.GenerateDirections = App.Current.MapDisplay.TrueRoute;
                options.FailOnInvalidOrderGeoLocation = false;

                // Update status.
                _SetOperationStartedStatus((string)App.Current.FindResource(UNASSIGN_ORDERS), (DateTime)schedule.PlannedDate);

                // Start solve.
                OperationsIds.Add(App.Current.Solver.UnassignOrdersAsync(schedule, ordersToUnassign, options));

                // Set solve started message
                string infoMessage = RoutingMessagesHelper.GetUnassignOperationStartedMessage(ordersToUnassign);

                if (!string.IsNullOrEmpty(infoMessage))
                    App.Current.Messenger.AddInfo(infoMessage);
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Unassign orders string.
        /// </summary>
        private const string UNASSIGN_ORDERS = "UnassignOrders";

        /// <summary>
        /// Next sequence increment.
        /// </summary>
        private static int NEXT_SEQUENCE_INCREMENT = 1;

        /// <summary>
        /// Last sequence increment.
        /// </summary>
        private static int LAST_SEQUENCE_INCREMENT = 2;

        #endregion
    }
}
