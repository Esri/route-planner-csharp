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
    /// Class helps in creation of solve operations result messages.
    /// </summary>
    internal static class RoutingMessagesHelper
    {
        #region Public static methods

        /// <summary>
        /// Method gets status message: assign operation started.
        /// </summary>
        /// <param name="ordersToUnassign">Orders to unassign.</param>
        /// <param name="routeName">Route name for assigning to.</param>
        /// <returns>Success solve started message.</returns>
        public static string GetAssignOperationStartedMessage(
            ICollection<Order> ordersToUnassign, string routeName)
        {
            Debug.Assert(!string.IsNullOrEmpty(routeName));
            Debug.Assert(ordersToUnassign != null);

            int count = ordersToUnassign.Count;

            if (routeName != (string)App.Current.FindResource(BEST_ROUTES_RESOURCE_NAME) &&
                routeName != (string)App.Current.FindResource(BEST_OTHER_ROUTE_RESOURCE_NAME))
            {
                routeName = string.Format(
                    (string)App.Current.FindResource(OPTION_WITH_QUOTES),
                    routeName);
            }

            // Set solve started message
            string infoMessage = string.Empty;

            if (count == 1)
            {
                Order order = ordersToUnassign.First();

                if (order == null)
                    return infoMessage;

                infoMessage = string.Format(
                    (string)App.Current.FindResource(ORDER_SUBMITTING_TO),
                    order.Name, routeName);
            }
            else
            {
                infoMessage = string.Format(
                    (string)App.Current.FindResource(ORDERS_SUBMITTING_TO),
                    count, routeName);
            }

            return infoMessage;
        }

        /// <summary>
        /// Method gets status message: unassign operation started.
        /// </summary>
        /// <param name="ordersToUnassign">Orders to unassign.</param>
        /// <returns>Success unassign started message.</returns>
        public static string GetUnassignOperationStartedMessage(ICollection<Order> ordersToUnassign)
        {
            Debug.Assert(ordersToUnassign != null);

            int count = ordersToUnassign.Count;

            // Set solve started message
            string infoMessage = string.Empty;
            string unassignedOrdersName =
                (string)App.Current.FindResource(UNASSIGNED_ORDERS_RESOURCE_NAME);

            if (count == 1)
            {
                Order order = ordersToUnassign.First();

                if (order != null)
                {
                    infoMessage = string.Format(
                        (string)App.Current.FindResource(ORDER_SUBMITTING_TO),
                        order.Name, unassignedOrdersName);
                }
            }
            else
            {
                infoMessage = string.Format(
                    (string)App.Current.FindResource(ORDERS_SUBMITTING_TO),
                    count, unassignedOrdersName);
            }

            return infoMessage;
        }


        /// <summary>
        /// Method gets status message: assign operation completed.
        /// </summary>
        /// <param name="info">Operation information.</param>
        /// <returns>Message.</returns>
        public static string GetAssignOperationCompletedMessage(AsyncOperationInfo info)
        {
            Debug.Assert(info != null);

            var inputParameters = (AssignOrdersParams)info.InputParams;
            ICollection<Order> ordersToAssign = inputParameters.OrdersToAssign;

            int numberOfReassignedOrders = _GetNumberOfReassignedOrders(inputParameters);
            int numberOfOrdersToAssign = ordersToAssign.Count;
            string routeName = _GetAssignedRouteName(info);
            string message = string.Empty;

            if (numberOfReassignedOrders == 0)
            {   // No orders moved.
                if (numberOfOrdersToAssign == 1)
                {   // Add message for one not moved order.
                    Order order = ordersToAssign.First();

                    if (order != null)
                    {
                        message = string.Format((string)App.Current.FindResource(ORDER_FAILED_TO_MOVE),
                            order.Name, routeName);
                    }
                }
                else
                {   // Add message for all not moved orders.
                    message = string.Format((string)App.Current.FindResource(ALL_ORDERS_FAILED_TO_MOVE),
                        numberOfOrdersToAssign, routeName);
                }
            }
            else if (numberOfReassignedOrders == 1 &&
                     numberOfReassignedOrders == numberOfOrdersToAssign)
            {   // Add message for one moved order.
                Order order = ordersToAssign.First();

                if (order != null)
                {
                    // We can determine exact route name for one order,
                    // if it was assigned to any Best Route.
                    if (routeName == (string)App.Current.FindResource(BEST_ROUTES_RESOURCE_NAME))
                    {
                        routeName = _GetRouteNameForAssignedOrder(info, order);

                        Debug.Assert(!string.IsNullOrEmpty(routeName));

                        routeName = string.Format((string)App.Current.FindResource(OPTION_WITH_QUOTES),
                            routeName);
                    }

                    // Create final message.
                    message = string.Format((string)App.Current.FindResource(ORDER_MOVED),
                        order.Name, routeName);
                }
            }
            else if (numberOfReassignedOrders > 1 &&
                     numberOfReassignedOrders == numberOfOrdersToAssign)
            {   // Add message for All moved orders.
                message = string.Format((string)App.Current.FindResource(ALL_ORDERS_MOVED),
                    numberOfReassignedOrders, routeName);
            }
            else
            {   // Add message for some of orders moved.
                message = string.Format((string)App.Current.FindResource(SOME_ORDERS_MOVED),
                    numberOfReassignedOrders, numberOfOrdersToAssign, routeName);
            }

            return message;
        }

        /// <summary>
        /// Method gets status message: unassign operation completed.
        /// </summary>
        /// <param name="info">Operation information.</param>
        /// <returns>Message.</returns>
        public static string GetUnassignOperationCompletedMessage(AsyncOperationInfo info)
        {
            Debug.Assert(info != null);

            string result = string.Empty;

            var orders = ((UnassignOrdersParams)info.InputParams).OrdersToUnassign;
            int unassignedOrdersCount = orders.Count;
            string unassignedOrdersName =
                (string)App.Current.FindResource(UNASSIGNED_ORDERS_RESOURCE_NAME);

            if (unassignedOrdersCount == 1)
            {
                // Create message for only one unassigned order.
                Order order = orders.First();

                if (order != null)
                {
                    result = string.Format(
                        (string)App.Current.FindResource(ORDER_MOVED),
                        order.Name, unassignedOrdersName);
                }
            }
            else
            {
                // Create message for all unassigned orders.
                result = string.Format(
                    (string)App.Current.FindResource(ALL_ORDERS_MOVED),
                    unassignedOrdersCount, unassignedOrdersName);
            }

            return result;
        }

        /// <summary>
        /// Method gets status message: unassign operation completed,
        /// for multiple unassign operation.
        /// </summary>
        /// <param name="schedule">Schedule where operation proceeded.</param>
        /// <param name="info">Operation information.</param>
        /// <returns>Message.</returns>
        public static string GetMultipleUnassignOperationCompletedMessage(
            Schedule schedule, AsyncOperationInfo info)
        {
            Debug.Assert(schedule != null);
            Debug.Assert(info != null);

            string result = string.Empty;

            var orders = ((UnassignOrdersParams)info.InputParams).OrdersToUnassign;
            int unassignedOrdersCount = orders.Count;

            if (unassignedOrdersCount == 1)
            {
                // Create message for only one unassigned order.
                Order order = orders.First();

                if (order != null)
                {
                    result = string.Format(
                        (string)App.Current.FindResource(ORDER_UNASSIGNED_FROM_SCHEDULES),
                        order.Name, schedule.Name);
                }
            }
            else
            {
                // Create message for all unassigned orders.
                result = string.Format(
                    (string)App.Current.FindResource(ORDERS_UNASSIGNED_FROM_SCHEDULES),
                    unassignedOrdersCount, schedule.Name);
            }

            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Method gets number of orders, which was reassigned in result of operation.
        /// </summary>
        /// <param name="parameters">Operation parameters.</param>
        /// <returns>Number of reassigned orders.</returns>
        private static int _GetNumberOfReassignedOrders(AssignOrdersParams parameters)
        {
            Debug.Assert(parameters != null);

            ICollection<Order> ordersToAssign = parameters.OrdersToAssign;
            ICollection<Route> targetRoutes = parameters.TargetRoutes;
            var targetOrderSequence = parameters.TargetOrderSequence;

            int numberOfReassignedOrders = 0;
            foreach (Order order in ordersToAssign)
            {
                if (_IsOrderAssignedToRoute(order, targetRoutes, targetOrderSequence))
                {
                    numberOfReassignedOrders++;
                }
            }

            return numberOfReassignedOrders;
        }

        /// <summary>
        /// Method determines is order assigned to one of selected routes.
        /// </summary>
        /// </summary>
        /// <param name="order">Order.</param>
        /// <param name="routes">Collection of routes.</param>
        /// <param name="targetOrderSequence">Target orders sequence.</param>
        /// <returns>True - if order assigned to one of selected routes, otherwise - false.</returns>
        private static bool _IsOrderAssignedToRoute(Order order,
            ICollection<Route> routes, int? targetOrderSequence)
        {
            Debug.Assert(routes != null);
            Debug.Assert(order != null);

            var assignedStops =
                from route in routes
                where route.Stops != null
                from stop in route.Stops
                where
                    stop.AssociatedObject == order &&
                    (targetOrderSequence == null ||
                    stop.OrderSequenceNumber == targetOrderSequence)
                select stop;

            return assignedStops.Any();
        }

        /// <summary>
        /// Method gets name of route which contains assigned orders.
        /// </summary>
        /// <param name="info">Operation information.</param>
        /// <returns>Route name.</returns>
        private static string _GetAssignedRouteName(AsyncOperationInfo info)
        {
            Debug.Assert(info != null);

            var inputParameters = (AssignOrdersParams)info.InputParams;
            ICollection<Route> targetRoutes = inputParameters.TargetRoutes;

            string routeName = null;

            if (targetRoutes.Count == 1)
            {
                // In most cases only one route is targeted.
                routeName = string.Format(
                    (string)App.Current.FindResource(OPTION_WITH_QUOTES),
                    targetRoutes.First().Name);
            }
            else
            {
                // Orders assigned to Best Route.
                routeName =
                    (string)App.Current.FindResource(BEST_ROUTES_RESOURCE_NAME);
            }

            return routeName;
        }

        /// <summary>
        /// Method gets route name for assigned order.
        /// </summary>
        /// <param name="info">Operation info.</param>
        /// <param name="order">Assigned order.</param>
        /// <returns>Route name or empty string, in case order is unassigned.</returns>
        private static string _GetRouteNameForAssignedOrder(
            AsyncOperationInfo info, Order order)
        {
            Debug.Assert(info != null);
            Debug.Assert(order != null);

            var inputParameters = (AssignOrdersParams)info.InputParams;
            ICollection<Route> targetRoutes = inputParameters.TargetRoutes;

            // Search for stop associated with order, which was assigned to route.
            var assignedStops =
                from route in targetRoutes
                where route.Stops != null
                from stop in route.Stops
                where (stop.AssociatedObject == order)
                select stop;

            string routeName = string.Empty;

            if (assignedStops.Any())
            {
                Stop stop = assignedStops.First();
                routeName = stop.Route.Name;
            }

            return routeName;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Orders submitted for moving to Route string resource.
        /// </summary>
        private const string ORDERS_SUBMITTING_TO = "OrdersSubmittingForMovingOperation";

        /// <summary>
        /// One order submitted for moving to Route string resource.
        /// </summary>
        private const string ORDER_SUBMITTING_TO = "OrderSubmittingForMovingOperation";

        /// <summary>
        /// Moving completed for all orders string resource.
        /// </summary>
        private const string ALL_ORDERS_MOVED = "AllOrdersMovingOperationCompletedText";

        /// <summary>
        /// Moving completed for some of orders string resource.
        /// </summary>
        private const string SOME_ORDERS_MOVED = "OrdersMovingOperationCompletedText";

        /// <summary>
        /// Moving completed for one order string resource.
        /// </summary>
        private const string ORDER_MOVED = "OrderMovingOperationCompletedText";

        /// <summary>
        /// All orders failed to move string resource.
        /// </summary>
        private const string ALL_ORDERS_FAILED_TO_MOVE = "AllOrdersMovingOperationFailedText";

        /// <summary>
        /// Order failed to move string resource.
        /// </summary>
        private const string ORDER_FAILED_TO_MOVE = "OrderMovingOperationFailedText";

        /// <summary>
        /// Unassigned orders option resource string.
        /// </summary>
        private const string UNASSIGNED_ORDERS_RESOURCE_NAME = "UnassignedOrdersOption";

        /// <summary>
        /// Best route option resource string.
        /// </summary>
        private const string BEST_ROUTES_RESOURCE_NAME = "BestRouteOption";

        /// <summary>
        /// Best other route option resource string.
        /// </summary>
        private const string BEST_OTHER_ROUTE_RESOURCE_NAME = "BestOtherRouteOption";

        /// <summary>
        /// Orders unassigned from schedules resource string.
        /// </summary>
        private const string ORDERS_UNASSIGNED_FROM_SCHEDULES = "OrdersUnassigningCompleteSuccessfulMessage";

        /// <summary>
        /// Order unassigned from schedules resource string.
        /// </summary>
        private const string ORDER_UNASSIGNED_FROM_SCHEDULES = "OrderUnassigningCompleteSuccessfulMessage";

        /// <summary>
        /// Options with quotes for formatting route names.
        /// </summary>
        private const string OPTION_WITH_QUOTES = "OptionWithQuotes";

        #endregion
    }
}
