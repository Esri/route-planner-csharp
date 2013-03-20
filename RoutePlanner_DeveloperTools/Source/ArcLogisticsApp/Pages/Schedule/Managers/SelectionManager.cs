using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class to manage selection in optimize and edit page.
    /// </summary>
    internal class SelectionManager
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="optimizeAndEditPage">Parent page.</param>
        /// <param name="timeView">Time view.</param>
        public SelectionManager(OptimizeAndEditPage optimizeAndEditPage)
        {
            _optimizeAndEditPage = optimizeAndEditPage;
            _timeView = optimizeAndEditPage.TimeView;
            _mapView = optimizeAndEditPage.MapView;
            _ordersView = optimizeAndEditPage.OrdersView;
            _routesView = optimizeAndEditPage.RoutesView;

            // Set callback for checking selection possibility.
            _mapView.mapCtrl.CanSelectCallback = _CanSelect;

            _CreateCollectionsInMultiCollectionBinding();

            _InitEventHandlers();

            _selectionChanger = new SelectionChanger(_optimizeAndEditPage);
        }

        #endregion

        #region Public events

        /// <summary>
        /// Occurs when selection in page finish to change.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Occurs on each change in selection.
        /// </summary>
        public event NotifyMultiCollectionChangedEventHandler NotifyMultiCollectionChanged;

        #endregion

        #region Public properties

        /// <summary>
        /// Current selection.
        /// </summary>
        public IList SelectedItems
        {
            get { return _mapView.SelectedItems; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set selection.
        /// </summary>
        /// <param name="items">Items to select.</param>
        public void Select(IEnumerable items)
        {
            Debug.Assert(_selectionChanger != null);

            _selectionChanger.ItemForSettingContextAfterDayChanged = null;

            IEnumerable itemsToSelect = _selectionChanger.GetItemsToSelect(items); // exception

            if (SelectedItems.Count > 0)
            {
                SelectedItems.Clear();
            }

            foreach (object item in itemsToSelect)
            {
                if (_IsItemUsedInCurrentSchedule(item))
                {
                    SelectedItems.Add(item);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            Debug.Assert(_optimizeAndEditPage != null);

            INotifyCollectionChanged selectedItemsNotifier = (INotifyCollectionChanged)SelectedItems;
            selectedItemsNotifier.CollectionChanged += new NotifyCollectionChangedEventHandler(_OnSelectionChanged);
        }

        /// <summary>
        /// Creates multicollection binding.
        /// </summary>
        private void _CreateCollectionsInMultiCollectionBinding()
        {
            Debug.Assert(_mapView != null);
            Debug.Assert(_timeView != null);
            Debug.Assert(_ordersView != null);
            Debug.Assert(_routesView != null);

            _collectionBinding = new MultiCollectionBindingEx(_CanSelect);
            _collectionBinding.NotifyMultiCollectionChanged +=
                new NotifyMultiCollectionChangedEventHandler(_CollectionBindingNotifyMultiCollectionChanged);

            _collectionBinding.RegisterCollection((IList)_mapView.mapCtrl.SelectedItems, _MapViewSelectionFilterCallback);
            _collectionBinding.RegisterCollection(_timeView.SelectedItems, _AlwaysTrueSelectionFilterCallback);

            _ordersView.GridItemsSourceChanging += new EventHandler(_GridItemsSourceChanging);
            _ordersView.GridItemsSourceChanged += new EventHandler(_GridItemsSourceChanged);

            _routesView.GridItemsSourceChanging += new EventHandler(_GridItemsSourceChanging);
            _routesView.GridItemsSourceChanged += new EventHandler(_GridItemsSourceChanged);
        }

        /// <summary>
        /// React on items source changed.
        /// </summary>
        /// <param name="sender">Sender view.</param>
        /// <param name="e">Ignored.</param>
        private void _GridItemsSourceChanged(object sender, EventArgs e)
        {
            _optimizeAndEditPage.Dispatcher.BeginInvoke(new RegisterDataGridDelegate(_ListViewContextChangedHandler),
                DispatcherPriority.Input, sender);
        }

        /// <summary>
        /// React on items source changing.
        /// </summary>
        /// <param name="sender">Sender view.</param>
        /// <param name="e">Ignored.</param>
        private void _GridItemsSourceChanging(object sender, EventArgs e)
        {
            DataGridControlEx dataGridControl = _GetDataGridControlByView(sender);
            if (_collectionBinding.IsCollectionRegistered(dataGridControl.SelectedItems))
            {
                _ListViewContextChangingHandler(sender);
            }
            else
            {
                _optimizeAndEditPage.Dispatcher.BeginInvoke(new RegisterDataGridDelegate(_ListViewContextChangingHandler),
                    DispatcherPriority.Input, sender);
            }
        }

        /// <summary>
        /// Unregister datagrid.
        /// </summary>
        /// <param name="sender">View-parent of datagrid control to unregister.</param>
        private void _ListViewContextChangingHandler(object sender)
        {
            Debug.Assert(_collectionBinding != null);

            if (sender == _ordersView)
            {
                if (_collectionBinding.IsCollectionRegistered(_ordersView.OrdersGrid.SelectedItems))
                {
                    _collectionBinding.UnregisterCollection(_ordersView.OrdersGrid.SelectedItems);
                }
            }
            else if (sender == _routesView)
            {
                if (_collectionBinding.IsCollectionRegistered(_routesView.RoutesGrid.SelectedItems))
                {
                    _collectionBinding.UnregisterCollection(_routesView.RoutesGrid.SelectedItems);
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Register datagrid and brings first selected item to view.
        /// </summary>
        /// <param name="sender">View-parent of datagrid control to register.</param>
        private void _ListViewContextChangedHandler(object sender)
        {
            Debug.Assert(_collectionBinding != null);

            if (sender == _ordersView)
            {
                if (!_collectionBinding.IsCollectionRegistered(_ordersView.OrdersGrid.SelectedItems))
                {
                    _collectionBinding.RegisterCollection(_ordersView.OrdersGrid, _AlwaysTrueSelectionFilterCallback);
                }
            }
            else if (sender == _routesView)
            {
                if (!_collectionBinding.IsCollectionRegistered(_routesView.RoutesGrid.SelectedItems))
                {
                    _collectionBinding.RegisterCollection(_routesView.RoutesGrid, _routesView.SelectionFilterCallback);
                }
            }
            else
            {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Is item used in current schedule.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Is item used in current schedule.</returns>
        private bool _IsItemUsedInCurrentSchedule(object item)
        {
            Schedule currentSchedule = _optimizeAndEditPage.CurrentSchedule;

            bool result = false;

            Order order = item as Order;
            if (order != null)
            {
                result = currentSchedule.UnassignedOrders.Contains(order);
            }
            else
            {
                Route route = item as Route;
                if (route != null)
                {
                    result = currentSchedule.Routes.Contains(route);
                }
                else
                {
                    Stop stop = item as Stop;
                    if (stop != null)
                    {
                        Route stopRoute = stop.Route;
                        result = stopRoute != null && currentSchedule.Routes.Contains(stopRoute);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Is item can be selected always.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Is item can be selected.</returns>
        private bool _AlwaysTrueSelectionFilterCallback(object item)
        {
            return true;
        }

        /// <summary>
        /// Is item can be selected on map view.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Is item can be selected.</returns>
        private bool _MapViewSelectionFilterCallback(object item)
        {
            Debug.Assert(item != null);

            // If stop is not break, than check selecting possibility depending on previous selection.
            bool accept = _CanSelect(item);

            return accept;
        }

        /// <summary>
        /// Gets bool value defines whether selection can be changed.
        /// </summary>
        /// <param name="item">Item which should be selected.</param>
        /// <returns>Defined bool value.</returns>
        private bool _CanSelect(object item)
        {
            Debug.Assert(item != null);

            bool canSelect = true;

            if (SelectedItems.Count > 0)
            {
                // Item can be selected in 2 cases:
                // 1. Selected item is Route and item is Route.
                // 2. Selected item and item is order or location or stop.
                bool typesIsDifferent = (item is Route) ^ (SelectedItems[0] is Route);
                canSelect = !typesIsDifferent;
            }

            return canSelect;
        }

        /// <summary>
        /// React on each selection change.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OnSelectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // If method sygnalizer was not invoked - invoke it.
            if (!_selectionChangedInvoked)
            {
                _optimizeAndEditPage.Dispatcher.BeginInvoke(new DelegateSelectionFinalChanged(_OnFinalSelectionChanged), DispatcherPriority.Input);

                _selectionChangedInvoked = true;
            }
        }

        /// <summary>
        /// Fires selection changes finished.
        /// </summary>
        private void _OnFinalSelectionChanged()
        {
            _selectionChangedInvoked = false;

            // Notify selection changed
            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// React on changes in multicollection binding.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Multi collection changed event args.</param>
        private void _CollectionBindingNotifyMultiCollectionChanged(object sender, NotifyMultiCollectionChangedEventArgs e)
        {
            IList initiator = e.Initiator;

            DataGridControlEx initiatorGrid = null;
            if (initiator == _routesView.RoutesGrid.SelectedItems)
            {
                initiatorGrid = _routesView.RoutesGrid;
            }
            else if (initiator == _ordersView.OrdersGrid.SelectedItems)
            {
                initiatorGrid = _ordersView.OrdersGrid;
            }

            // Check that need to clear selection - if initiator is selected items of datagridcontrol and item was add.
            if ((e.EventArgs.Action == NotifyCollectionChangedAction.Add || e.EventArgs.Action == NotifyCollectionChangedAction.Replace) &&
                initiatorGrid != null && initiatorGrid.SelectedItemsFromAllContexts.Count > 0)
            {
                object firstSelectedItem = e.EventArgs.NewItems[0];

                // In case of shift or control adding element need to check type of new element.
                // if type of new element can't be selected with old selected.
                bool needToClearCollections = true;
                if (Keyboard.Modifiers == ModifierKeys.Control || Keyboard.Modifiers == ModifierKeys.Shift)
                    needToClearCollections = !_CanSelect(firstSelectedItem);

                if (needToClearCollections && !_postponedSelectionClearInvoked)
                {
                    _optimizeAndEditPage.Dispatcher.BeginInvoke(new DelegateWithItemParam(_PostponedClearingInvoked),
                        DispatcherPriority.Send, firstSelectedItem);

                    _postponedSelectionClearInvoked = true;
                }
            }

            _CallBringItemIntoViewIfNeeded(e);

            if (NotifyMultiCollectionChanged != null)
                NotifyMultiCollectionChanged(sender, e);
        }

        /// <summary>
        /// Make postponed call bring item into view, if item was add to selection by click in mapview or timeview.
        /// </summary>
        /// <param name="args">Collection changed args.</param>
        private void _CallBringItemIntoViewIfNeeded(NotifyMultiCollectionChangedEventArgs args)
        {
            Debug.Assert(_optimizeAndEditPage != null);
            Debug.Assert(_mapView != null);
            Debug.Assert(_timeView != null);
            Debug.Assert(args != null);

            if (!_postponedBringItemIntoViewInvoked && args.EventArgs.Action == NotifyCollectionChangedAction.Add &&
                (args.Initiator == _mapView.SelectedItems || args.Initiator == _timeView.SelectedItems))
            {
                _optimizeAndEditPage.Dispatcher.BeginInvoke(new BringToViewDelegate(_BringItemIntoView),
                        DispatcherPriority.Input, args.EventArgs.NewItems[0]);

                _postponedBringItemIntoViewInvoked = true;
            }
        }

        /// <summary>
        /// Do bring item into view.
        /// </summary>
        /// <param name="item">Item to bring into view.</param>
        private void _BringItemIntoView(object item)
        {
            Debug.Assert(item != null);

            DataGridControlEx dataGridControl = _GetParentDataGridControl(item);
            if (dataGridControl != null)
                dataGridControl.BringItemIntoView(item);

            _postponedBringItemIntoViewInvoked = false;
        }

        /// <summary>
        /// Get data grid which contains item.
        /// </summary>
        /// <param name="item">Item to find.</param>
        /// <returns>Data grid which contains item.</returns>
        private DataGridControlEx _GetParentDataGridControl(object item)
        {
            Debug.Assert(item != null);

            DataGridControlEx parentDataGridControl = null;

            if (item is Order)
                parentDataGridControl = _ordersView.OrdersGrid;
            else if ((item is Route) || (item is Stop))
                parentDataGridControl = _routesView.RoutesGrid;
            else
            {
                Debug.Assert(false); // not supported
            }

            return parentDataGridControl;
        }

        /// <summary>
        /// Updates multi collection binding.
        /// </summary>
        /// <param name="firstSelectedItem">First selected item.</param>
        private void _PostponedClearingInvoked(object firstSelectedItem)
        {
            // NOTE: Do not need to process postponed call in case of deleting route.
            // Schedule will be loaded again.

            if (_postponedSelectionClearInvoked)
            {
                Debug.Assert(_timeView != null);
                Debug.Assert(_mapView != null);
                Debug.Assert(_collectionBinding != null);

                // Make timeview and mapview selected items collections empty.
                _collectionBinding.UnregisterCollection((IList)_mapView.mapCtrl.SelectedItems);
                _collectionBinding.UnregisterCollection(_timeView.SelectedItems);
                if (firstSelectedItem is Order)
                {
                    _collectionBinding.UnregisterCollection(_routesView.RoutesGrid.SelectedItems);
                    _routesView.RoutesGrid.ClearSelectionInAllContexts();
                }
                else if (firstSelectedItem is Route || firstSelectedItem is Stop)
                {
                    _collectionBinding.UnregisterCollection(_ordersView.OrdersGrid.SelectedItems);
                    _ordersView.OrdersGrid.ClearSelectionInAllContexts();
                }
                else
                {
                    Debug.Assert(false);
                }

                _timeView.SelectedItems.Clear();
                _mapView.mapCtrl.SelectedItems.Clear();
                
                // Register mapview and timeview back.
                _collectionBinding.RegisterCollection((IList)_mapView.mapCtrl.SelectedItems, _MapViewSelectionFilterCallback);
                _collectionBinding.RegisterCollection(_timeView.SelectedItems, _AlwaysTrueSelectionFilterCallback);

                if (firstSelectedItem is Order)
                {
                    _collectionBinding.RegisterCollection(_routesView.RoutesGrid, _routesView.SelectionFilterCallback);
                }
                else if (firstSelectedItem is Route || firstSelectedItem is Stop)
                {
                    _collectionBinding.RegisterCollection(_ordersView.OrdersGrid, _AlwaysTrueSelectionFilterCallback);
                }
                else
                {
                    Debug.Assert(false);
                }

                _postponedSelectionClearInvoked = false;
            }
        }

        /// <summary>
        /// Get grid from view.
        /// </summary>
        /// <param name="view">View.</param>
        /// <returns>Grid.</returns>
        private DataGridControlEx _GetDataGridControlByView(object view)
        {
            Debug.Assert(view != null);

            DataGridControlEx dataGridControl = null;

            if (view == _ordersView)
            {
                dataGridControl = _ordersView.OrdersGrid;
            }
            else if (view == _routesView)
            {
                dataGridControl = _routesView.RoutesGrid;
            }

            return dataGridControl;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Multi collection binding.
        /// </summary>
        private MultiCollectionBindingEx _collectionBinding;

        /// <summary>
        /// TimeView from parent page.
        /// </summary>
        private TimeView _timeView;

        /// <summary>
        /// MapView from parent page.
        /// </summary>
        private MapView _mapView;

        /// <summary>
        /// Orders view from parent page.
        /// </summary>
        private OrdersView _ordersView;

        /// <summary>
        /// Routes view from parent page.
        /// </summary>
        private RoutesView _routesView;

        /// <summary>
        /// Selection changer.
        /// </summary>
        private SelectionChanger _selectionChanger;

        /// <summary>
        /// Flag, which indicates that selection clearing invoked.
        /// </summary>
        private bool _postponedSelectionClearInvoked;

        /// <summary>
        /// Flag, which indicates is method sygnalizer of selection changes invoked.
        /// </summary>
        private bool _selectionChangedInvoked;

        /// <summary>
        /// Flag, which indicates that bringing to view invoked.
        /// </summary>
        private bool _postponedBringItemIntoViewInvoked;

        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _optimizeAndEditPage;

        /// <summary>
        /// Delegate for invoke selection changed notifier.
        /// </summary>
        private delegate void DelegateSelectionFinalChanged();

        /// <summary>
        /// Delegate with item param.
        /// </summary>
        private delegate void DelegateWithItemParam(object item);

        /// <summary>
        /// Delegate with parameter. Used in code for Bring item into view.
        /// </summary>
        /// <param name="item">Item to bring to view.</param>
        private delegate void BringToViewDelegate(object item);

        /// <summary>
        /// Delegate with item param.
        /// </summary>
        private delegate void RegisterDataGridDelegate(object sender);

        #endregion
    }
}