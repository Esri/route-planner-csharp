using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Commands
{
    static internal class RoutingCmdHelpers
    {
        #region public static methods

        /// <summary>
        /// Validate objects
        /// </summary>
        /// <returns>Object collection with Primary error.</returns>
        /// <remarks>Check only PRIMARY error.</remarks>
        public static ICollection<DataObject> ValidateObjects<T>(ICollection<T> objects)
            where T : DataObject
        {
            List<DataObject> invalidObjects = new List<DataObject>();
            foreach (T obj in objects)
            {
                if (!string.IsNullOrEmpty(obj.PrimaryError))
                    invalidObjects.Add(obj);
            }

            return invalidObjects;
        }

        /// <summary>
        /// Method returns routing exception message string
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string FormatRoutingExceptionMsg(RouteException e)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((string)App.Current.FindResource("ErrorExecutingRouteCommandString"));
            sb.Append(e.Message);

            if (e.Details != null)
            {
                foreach (string msg in e.Details)
                {
                    sb.Append(LINE_TRANSFER);
                    sb.Append(msg);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Method returns solver error message string
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static string FormatSolverErrorMsg(SolveResult res)
        {
            StringBuilder messageSb = new StringBuilder();
            foreach (ServerMessage msg in res.Messages)
            {
                if (msg.Type == ServerMessageType.Error || msg.Type == ServerMessageType.Warning)
                    messageSb.Append(_FixMessageText(msg.Text));
            }

            // Replace guids with names in message.
            var messageWithNames = GuidsReplacer.ReplaceGuids(messageSb.ToString(), App.Current.Project);

            return messageWithNames;
        }

        public static ICollection<Order> GetOrdersIncludingPairs(Schedule schedule, ICollection<Order> orders)
        {
            Collection<Order> resultOrders = new Collection<Order>();

            // anything to do?
            if (orders == null)
                return resultOrders;

            Dictionary<string, Order> pickupOrders = new Dictionary<string, Order>();
            Dictionary<string, Order> deliveryOrders = new Dictionary<string, Order>();

            // Add each selected order to the result collection.
            // If the order has a order key add these to the pickup or delivery
            // collections.

            foreach (Order order in orders)
            {
                resultOrders.Add(order);

                string orderKey = order.PairKey();
                if (string.IsNullOrEmpty(orderKey))
                    continue;

                if (order.Type == OrderType.Pickup)
                    pickupOrders.Add(orderKey, order);
                else if (order.Type == OrderType.Delivery && !deliveryOrders.ContainsKey(orderKey))
                    deliveryOrders.Add(orderKey, order);
                else
                    continue;   // not expected
            }

            // Check that each pickup is paired to a delivery if there is one in the schedule.

            foreach (KeyValuePair<string, Order> kvp in pickupOrders)
            {
                if (deliveryOrders.ContainsKey(kvp.Key))
                    continue;

                bool foundDeliveryOrder = false;

                if (schedule.UnassignedOrders != null)
                {
                    foreach (Order order in schedule.UnassignedOrders)
                    {
                        if (order.Type != OrderType.Delivery)
                            continue;

                        string orderKey = order.PairKey();
                        if (string.IsNullOrEmpty(orderKey))
                            continue;

                        if (kvp.Key == orderKey)
                        {
                            // found our delivery

                            resultOrders.Add(order);
                            foundDeliveryOrder = true;
                            break;
                        }
                    }
                }

                if (foundDeliveryOrder)
                    continue;

                foreach (Route route in schedule.Routes)
                {
                    foreach (Stop stop in route.Stops)
                    {
                        Order associatedOrder = stop.AssociatedObject as Order;
                        if (associatedOrder == null)
                            continue;

                        if (associatedOrder.Type != OrderType.Delivery)
                            continue;

                        string orderKey = associatedOrder.PairKey();
                        if (string.IsNullOrEmpty(orderKey))
                            continue;

                        if (kvp.Key == orderKey)
                        {
                            // found our delivery

                            resultOrders.Add(associatedOrder);
                            foundDeliveryOrder = true;
                            break;
                        }
                    }

                    if (foundDeliveryOrder)
                        break;
                }

            }

            // Check that each delivery is paired to a pickup if there is one in the schedule.

            foreach (KeyValuePair<string, Order> kvp in deliveryOrders)
            {
                if (pickupOrders.ContainsKey(kvp.Key))
                    continue;

                bool foundPickupOrder = false;

                if (schedule.UnassignedOrders != null)
                {
                    foreach (Order order in schedule.UnassignedOrders)
                    {
                        if (order.Type != OrderType.Pickup)
                            continue;

                        string orderKey = order.PairKey();
                        if (string.IsNullOrEmpty(orderKey))
                            continue;

                        if (kvp.Key == orderKey)
                        {
                            // found our delivery

                            resultOrders.Add(order);
                            foundPickupOrder = true;
                            break;
                        }
                    }
                }

                if (foundPickupOrder)
                    continue;

                foreach (Route route in schedule.Routes)
                {
                    foreach (Stop stop in route.Stops)
                    {
                        Order associatedOrder = stop.AssociatedObject as Order;
                        if (associatedOrder == null)
                            continue;

                        if (associatedOrder.Type != OrderType.Pickup)
                            continue;

                        string orderKey = associatedOrder.PairKey();
                        if (string.IsNullOrEmpty(orderKey))
                            continue;

                        if (kvp.Key == orderKey)
                        {
                            // found our delivery

                            resultOrders.Add(associatedOrder);
                            foundPickupOrder = true;
                            break;
                        }
                    }

                    if (foundPickupOrder)
                        break;
                }

            }

            return resultOrders;
        }

        #endregion

        #region private static methods

        private static string _FixMessageText(string message)
        {
            string fixedMsg = message;
            if (message.Contains(SERVER_OBJECT_MSG))
                fixedMsg = (string)App.Current.FindResource("ServerObjectTerminated");

            return fixedMsg;
        }

        #endregion

        #region Private fields

        private const string LINE_TRANSFER = "\n";
        private const string SERVER_OBJECT_MSG = "crash or termination of the server object";

        #endregion
    }
}
