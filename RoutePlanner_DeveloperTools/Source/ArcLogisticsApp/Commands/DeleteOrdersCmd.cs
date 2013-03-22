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
using System.Diagnostics;
using System.Collections.ObjectModel;

using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Properties;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command for delete orders - inherits from MoveOrdersCmdBase where included common logic to unassign orders
    /// </summary>
    internal class DeleteOrdersCmd : MultiUnassignCmdBase
    {
        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteOrders";

        #region Overrided Members

        public override void Initialize(App app)
        {
            base.Initialize(app);
            IsEnabled = false;
            App.Current.ApplicationInitialized += new EventHandler(Current_ApplicationInitialized);

            KeyGesture = new KeyGesture(INVOKE_KEY);
        }

        public override bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            protected set
            {
                _isEnabled = value;
                _NotifyPropertyChanged(IS_ENABLED_PROPERTY_NAME);

                if (value)
                    TooltipText = (string)App.Current.FindResource("DeleteCommandEnabledTooltip");
                else
                    TooltipText = (string)App.Current.FindResource("DeleteCommandDisabledTooltip");
            }
        }

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        public override string Title
        {
            get
            {
                return (string)App.Current.FindResource("DeleteOrdersCommandTitle");
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

        protected override string OperationSuccessStartedMessage
        {
            get { return (string)App.Current.FindResource("DeletingOrdersStartSuccessfulMessage"); }
        }

        protected override string OperationIsFailedMessage
        {
            get { return (string)App.Current.FindResource("DeletingFailedCommonMessage"); }
        }

        protected override string OrdersAreLockedMessage
        {
            get { return (string)App.Current.FindResource("LockedOrdersDuringDeleteMessage"); }
        }

        protected override void _Execute(params object[] args)
        {
            // if editing is process in optimize and edit page - cancel editing
            if (_optimizePage.IsEditingInProgress)
                ((ICancelDataObjectEditing)_optimizePage).CancelObjectEditing();

            // create collection of orders to delete
            Collection<Order> ordersToDelete = _GetOrdersFromSelection();

            // Include any paired orders
            if (App.Current.Project.Schedules.ActiveSchedule != null)
            {
                Schedule schedule = App.Current.Project.Schedules.ActiveSchedule;
                ordersToDelete = RoutingCmdHelpers.GetOrdersIncludingPairs(schedule, ordersToDelete) as Collection<Order>;
            }

            if (0 < ordersToDelete.Count)
            {
                bool doProcess = true;
                if (Settings.Default.IsAllwaysAskBeforeDeletingEnabled)
                    // show warning dialog
                    doProcess = DeletingWarningHelper.Execute(ordersToDelete);

                if (doProcess)
                    // call base Execute() to unassign orders if necessary
                    base._Execute(ordersToDelete);
            }
        }

        /// <summary>
        /// Overrided method for move orders. It will be called in parent class
        /// </summary>
        /// <param name="args"></param>
        protected override void _ProcessOrders(params object[] args)
        {
            Collection<Order> ordersToDelete = (Collection<Order>)args[0];
            Debug.Assert(ordersToDelete is Collection<Order>);

            foreach (Order order in ordersToDelete)
                App.Current.Project.Orders.Remove(order);

            App.Current.Project.Save();
            App.Current.Messenger.AddInfo(string.Format((string)App.Current.FindResource("OrdersDeletedSuccessfullyString"), ordersToDelete.Count, App.Current.CurrentDate.ToShortDateString()));

            _optimizePage.OnScheduleChanged(_optimizePage.CurrentSchedule);
        }

        #endregion

        #region Event handlers

        private void Current_ApplicationInitialized(object sender, EventArgs e)
        {
            _optimizePage = (OptimizeAndEditPage)App.Current.MainWindow.GetPage(PagePaths.SchedulePagePath);

            _optimizePage.CurrentScheduleChanged += new EventHandler(optimizePage_CurrentScheduleChanged);
            _optimizePage.SelectionChanged += new EventHandler(_schedulePage_SelectionChanged);
            _optimizePage.EditBegun += new DataObjectEventHandler(_optimizePage_EditBegun);
            _optimizePage.EditCommitted += new DataObjectEventHandler(_optimizePage_EditCommitted);
            _optimizePage.EditCanceled += new ESRI.ArcLogistics.App.Pages.DataObjectEventHandler(_optimizePage_EditCanceled);
            _optimizePage.NewObjectCreated += new DataObjectEventHandler(_optimizePage_NewObjectCreated);
            _optimizePage.NewObjectCommitted += new DataObjectEventHandler(_optimizePage_NewObjectCommitted);
            _optimizePage.NewObjectCanceled += new DataObjectEventHandler(_optimizePage_NewObjectCanceled);
        }

        private void _optimizePage_NewObjectCanceled(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _optimizePage_NewObjectCommitted(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _optimizePage_NewObjectCreated(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _optimizePage_EditCommitted(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _optimizePage_EditBegun(object sender, DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        private void _schedulePage_SelectionChanged(object sender, EventArgs e)
        {
            _CheckEnabled();
        }
        
        protected void _optimizePage_EditCanceled(object sender, ESRI.ArcLogistics.App.Pages.DataObjectEventArgs e)
        {
            _CheckEnabled();
        }

        protected void optimizePage_CurrentScheduleChanged(object sender, EventArgs e)
        {
            _currentSchedule = _optimizePage.CurrentSchedule;
            _CheckEnabled();
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///  Method checks is command enabled
        /// </summary>
        private void _CheckEnabled()
        {
            bool isOrderEditing = (_optimizePage.IsEditingInProgress && _optimizePage.EditingManager.EditedObject != null && _optimizePage.EditingManager.EditedObject is Order);
            IsEnabled = ((_GetOrdersFromSelection().Count > 0) || isOrderEditing);
        }

        /// <summary>
        /// Methods returns orders from selection 
        /// </summary>
        /// <returns></returns>
        protected Collection<Order> _GetOrdersFromSelection()
        {
            Collection<Order> orders = new Collection<Order>();

            foreach (Object obj in _optimizePage.SelectedItems)
            {
                var order = _ExtractOrder(obj);
                if (order != null)
                    orders.Add(order);
            }

            return orders;
        }

        #endregion

        #region private static methods
        /// <summary>
        /// Extracts reference to an order from the specified selected item.
        /// </summary>
        /// <param name="selectedItem">The reference to the object selected
        /// in the optimize and edit view.</param>
        /// <returns>The reference to an order associated with the specified
        /// seleted item or null reference if there is no such order.</returns>
        private static Order _ExtractOrder(object selectedItem)
        {
            var selectedOrder = selectedItem as Order;
            if (selectedOrder != null)
            {
                return selectedOrder;
            }

            var selectedStop = selectedItem as Stop;
            if (selectedStop != null)
            {
                var order = selectedStop.AssociatedObject as Order;

                return order;
            }

            return null;
        }
        #endregion

        #region Private constants

        /// <summary>
        /// Key to invoke command.
        /// </summary>
        private const Key INVOKE_KEY = Key.Delete;

        #endregion

        #region Private fields

        private const string TOOLTIP_PROPERTY_NAME = "TooltipText";
        private const string IS_ENABLED_PROPERTY_NAME = "IsEnabled";
        private bool _isEnabled;

        // current schedule on Optimize and edit
        private Schedule _currentSchedule;

        // reference to Optimize and edit page
        private OptimizeAndEditPage _optimizePage;
        private string _tooltipText = null;

        #endregion
    }
}
