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
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.DragAndDrop.Adornments;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop
{
    /// <summary>
    /// Soruce where orders can be dragged from.
    /// </summary>
    enum DragSource
    {
        OrdersView,
        RoutesView,
        FindView,
        MapView,
        TimeView
    }

    /// <summary>
    /// Class that contains several helpers functions for drag and drop.
    /// </summary>
    internal class DragAndDropHelper
    {
        private struct DraggingOrdersObject
        {
            public DateTime OrdersPlannedDate { get; set; }
            public Collection<Guid> IDsCollection { get; set; }
        }

        #region Constants

        private const string DRAGGING_DATA_FORMAT_STRING = "GUID";

        #endregion

        #region public events

        public event EventHandler DragStarted;
        public event EventHandler DragEnded;

        #endregion

        #region public methods

        /// <summary>
        /// Method checks is dragging of selected objects allowed
        /// </summary>
        /// <param name="selectedElements"></param>
        /// <returns></returns>
        public bool IsDragAllowed(ICollection<Object> selectedElements)
        {
            bool isDragAllowed = false;

            foreach (Object obj in selectedElements)
            {
                if (obj is Order)
                    isDragAllowed = true;
                else if (obj is Stop && !((Stop)obj).IsLocked && !((Stop)obj).Route.IsLocked)
                    isDragAllowed = true;
                else // if any one object is locked - return false
                {
                    isDragAllowed = false;
                    break;
                }
            }

            return isDragAllowed;
        }

        /// <summary>
        /// Method initialize drag operation
        /// </summary>
        /// <param name="draggingOrders"></param>
        public void StartDragOrders(Collection<object> draggingOrdersAndStops, DragSource dragSource)
        {
            // Get orders from stops and orders.
            Collection<Order> draggingOrders = GetOrdersFromObjectsCollection(draggingOrdersAndStops);
            if (draggingOrders.Count == 0)
                return;

            // Decline dragging if orders and stops not belongs to the same date.
            bool isSameDate = _CheckOrdersAndStopsIsFromSameDate(draggingOrdersAndStops);
            if (!isSameDate)
            {
                string message = (string)App.Current.FindResource(DRAGGING_ORDERS_WITH_DIFFERENT_DAYS_PROHIBITED_RESOURCE_NAME);
                App.Current.Messenger.AddWarning(message);
                return;
            }

            // Create data object for drag and drop.
            IDataObject draggingObject = _CreateDraggingObject(draggingOrders);

            if (draggingObject == null)
                return;
                
            try
            {
                // Create adornment.
                IAdornment adornment = AdornmentFactory.CreateAdornment(draggingOrdersAndStops, dragSource);

                // Create feedbacker.
                using (DragDropFeedbacker feedbacker = new DragDropFeedbacker(adornment))
                {
                    if (DragStarted != null)
                        DragStarted(this, EventArgs.Empty);
                    DragDropEffects effect = DragDrop.DoDragDrop(feedbacker, draggingObject, DragDropEffects.None | DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                if (DragEnded != null)
                    DragEnded(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Method changes mouse cursor
        /// </summary>
        /// <param name="isAssignAllowed"></param>
        public void ChangeCursor(bool isAssignAllowed)
        {
            if (isAssignAllowed)
                Mouse.OverrideCursor = Cursors.ArrowCD;
            else
                Mouse.OverrideCursor = Cursors.Help;
        }

        /// <summary>
        /// Method extracts orders from collection of objects
        /// </summary>
        /// <param name="objectsCollection"></param>
        /// <returns></returns>
        public Collection<Order> GetOrdersFromObjectsCollection(Collection<Object> objectsCollection)
        {
            Collection<Order> ordersCollection = new Collection<Order>();

            foreach (Object obj in objectsCollection)
            {
                if (obj is Order)
                    ordersCollection.Add((Order)obj);
                else if (obj is Stop && ((Stop)obj).AssociatedObject is Order)
                    ordersCollection.Add((Order)((Stop)obj).AssociatedObject);
            }

            return ordersCollection;
        }

        /// <summary>
        /// Method checks is drop allowed on current object
        /// (is it route or stop and not locked)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool DoesDropAllowed(object target, ICollection<Order> draggingObjects)
        {
            bool isAllowed = false;

            if (draggingObjects.Count == 0)
                return false;

            // shows does target associated object is order
            bool isTargetStopOrder = ((target is Stop) && ((Stop)target).AssociatedObject is Order);

            // shows does target locked
            bool isTargetLocked = ((target is Route && ((Route)target).IsLocked) ||
                ((target is Stop) && ((Stop)target).Route.IsLocked));

            // shows does dragging objects coolection contain target (user try to drop order to itself)
            bool isDraggingObjectsContainsTarget = (isTargetStopOrder && draggingObjects.Contains((Order)((Stop)target).AssociatedObject));

            // drop not allowed when dragging object is one order in a pair and stop target would violate
            // the rule that the pickup must be before delivery
            bool isOrderPairSequenceViolation = false;
            if (draggingObjects.Count == 1 && (target is Stop))
            {
                Order draggingOrder = draggingObjects.First();

                string draggingOrderPairKey = draggingOrder.PairKey();
                bool hasOrderPairKey = !string.IsNullOrEmpty(draggingOrderPairKey);

                if (hasOrderPairKey)
                {
                    Stop pairedOrderStop = null;
                    Route targetRoute = ((Stop)target).Route;

                    // does the target route already contain the order being dragged?

                    bool dragOrderInTargetRoute = false;
                    foreach (Stop stop in targetRoute.Stops)
                    {
                        Order associatedOrder = stop.AssociatedObject as Order;
                        if (associatedOrder == null)
                            continue;

                        if (associatedOrder == draggingOrder)
                            dragOrderInTargetRoute = true;
                         
                        // also check if we have found our matching order

                        if ((draggingOrder.Type == OrderType.Pickup && associatedOrder.Type == OrderType.Delivery) ||
                            (draggingOrder.Type == OrderType.Delivery && associatedOrder.Type == OrderType.Pickup))
                        {
                            if (draggingOrderPairKey == associatedOrder.PairKey())
                                pairedOrderStop = stop;
                        }
                    }

                    if (dragOrderInTargetRoute && pairedOrderStop != null)
                    {
                        // yes, we can check our sequencing rule

                        if (draggingOrder.Type == OrderType.Pickup &&
                            ((target as Stop).SequenceNumber >= pairedOrderStop.SequenceNumber))
                            isOrderPairSequenceViolation = true;

                        else if (draggingOrder.Type == OrderType.Delivery &&
                            ((target as Stop).SequenceNumber <= pairedOrderStop.SequenceNumber))
                            isOrderPairSequenceViolation = true;
                    }
                }
            }

            // Drop allowed when target objet isn't locked and if target is order it not contains in dragging collection 
            isAllowed = (!(target is DataTemplate) && target != null && 
                !isTargetLocked && !isDraggingObjectsContainsTarget &&
                !isOrderPairSequenceViolation);

            // NOTE: it is possible to drop multiple orders on a specific position on a route. In this case
            // ArcLogistics will assign the orders to the best position. For more details see CR161083.

            return isAllowed;
        }

        /// <summary>
        /// React on Drop
        /// </summary>
        /// <param name="targetData">Dropped target</param>
        /// <param name="draggingData">Dropped objects</param>
        /// <param name="previousStop"></param>
        public void Drop(object targetData, IDataObject draggingData)
        {
            Collection<Order> draggingOrders = GetDraggingOrders(draggingData);
            bool isDropAllowed = DoesDropAllowed(targetData, draggingOrders);

            if (isDropAllowed)
            {
                DropOrdersCmd dropCmd = new DropOrdersCmd();
                dropCmd.Initialize((ESRI.ArcLogistics.App.App)Application.Current);
                dropCmd.Execute(draggingOrders, targetData);
            }
        }

        /// <summary>
        /// React on Drop on calendar
        /// </summary>
        /// <param name="targetData">Dropped target</param>
        /// <param name="draggingData">Dropped objects</param>
        /// <param name="previousStop"></param>
        public void DropOnDate(object targetDate, IDataObject draggingData)
        {
            Collection<Order> draggingOrders = GetDraggingOrders(draggingData);

            MoveOrdersToOtherDateCmd moveOrdersCmd = new MoveOrdersToOtherDateCmd();
            moveOrdersCmd.Initialize((ESRI.ArcLogistics.App.App)Application.Current);
            moveOrdersCmd.Execute(draggingOrders, targetDate);
        }

        /// <summary>
        /// Method unpacks dragging objects from dragging data
        /// </summary>
        /// <param name="draggingData"></param>
        /// <returns></returns>
        public Collection<Order> GetDraggingOrders(IDataObject draggingData)
        {
            Collection<Order> draggingObjects = new Collection<Order>();

            // get collection of dragging elements
            if (draggingData.GetDataPresent(DRAGGING_DATA_FORMAT_STRING))
            {
                // get data from  data.GetData("format");
                DraggingOrdersObject draggingObject = (DraggingOrdersObject)draggingData.GetData(DRAGGING_DATA_FORMAT_STRING);

                IDataObjectCollection<Order> ordersForDate = App.Current.Project.Orders.Search(draggingObject.OrdersPlannedDate);

                foreach (Order order in ordersForDate)
                {
                    foreach (Guid id in draggingObject.IDsCollection)
                    {
                        if (order.Id.Equals(id))
                            draggingObjects.Add(order);
                    }
                }
            }

            return draggingObjects;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Method packs selected orders to dragging data object
        /// </summary>
        /// <param name="selectedOrers"></param>
        /// <returns></returns>
        private IDataObject _CreateDraggingObject(Collection<Order> selectedOrers)
        {
            DraggingOrdersObject draggingObject = new DraggingOrdersObject();
            draggingObject.OrdersPlannedDate = (DateTime)selectedOrers[0].PlannedDate;

            draggingObject.IDsCollection = new Collection<Guid>();

            foreach (Order order in selectedOrers)
                draggingObject.IDsCollection.Add(order.Id);

            System.Windows.DataObject newObject = new System.Windows.DataObject(DRAGGING_DATA_FORMAT_STRING, draggingObject);

            return newObject;
        }

        /// <summary>
        /// Check dates equals to current in all items in collection of orders and stops.
        /// </summary>
        /// <param name="draggingOrdersAndStops">Collection of dragging orders and stops.</param>
        /// <returns>True if dates equals in all items in collection of orders and stops. False otherwise.</returns>
        private bool _CheckOrdersAndStopsIsFromSameDate(Collection<object> draggingOrdersAndStops)
        {
            Debug.Assert(draggingOrdersAndStops != null);

            bool result = true;

            // Compare dates of all other items with date of first item.
            foreach (object item in draggingOrdersAndStops)
            {
                DateTime? itemDate = _GetItemDate(item);

                if (itemDate.HasValue && App.Current.CurrentDate != itemDate.Value.Date)
                {
                    result = false;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Get date of order or stop.
        /// </summary>
        /// <param name="obj">Order or stop.</param>
        /// <returns>Date of order or stop.</returns>
        private DateTime? _GetItemDate(object obj)
        {
            Debug.Assert(obj != null);

            DateTime? itemDate = null;

            Stop stop = obj as Stop;
            if (stop != null)
            {
                // If item is stop - get date of parent schedule.
                itemDate = stop.Route.Schedule.PlannedDate.Value;
            }
            else
            {
                Order order = obj as Order;
                if (order != null)
                {
                    // If item is order - get date of order.
                    itemDate = order.PlannedDate;
                }
                else
                {
                    Debug.Assert(false);
                }
            }

            return itemDate;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Resource name for warning string to decline dragging orders from different days.
        /// </summary>
        private const string DRAGGING_ORDERS_WITH_DIFFERENT_DAYS_PROHIBITED_RESOURCE_NAME = "DraggingOrdersWithDifferentDaysProhibited";

        #endregion
    }
}
