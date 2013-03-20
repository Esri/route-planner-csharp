using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.DragAndDrop;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;
using Xceed.Wpf.DataGrid.Views;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for OrdersView.xaml.
    /// </summary>
    internal partial class OrdersView : DockableContent, ISupportDataObjectEditing, ICancelDataObjectEditing
    {
        #region Constructors

        /// <summary>
        /// Constructor. Creates new instance of List view and sets property ParentPage from constructor parameter.
        /// </summary>
        public OrdersView()
        {
            InitializeComponent();

            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(_ProjectClosing);
            App.Current.Exit += new ExitEventHandler(_AppExit);
            this.VisibileStateChanged += new EventHandler(_VisibileStateChanged);

            if (App.Current.Project != null)
                _InitGridControl();

            new ViewButtonsMarginUpdater(this, commandButtonsGroup);

            // Subscribe on mouse move for drag and drop.
            Application.Current.MainWindow.MouseMove += new MouseEventHandler(_MouseMove);

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController
                (OrdersGrid);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when data grid source items was set.
        /// </summary>
        public event EventHandler GridItemsSourceChanged;

        /// <summary>
        /// Occurs when data grid source items was reset.
        /// </summary>
        public event EventHandler GridItemsSourceChanging;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets optimize and edit page which contains View.
        /// </summary>
        public OptimizeAndEditPage ParentPage
        {
            get { return _schedulePage; }
            set
            {
                if (null != _schedulePage)
                    _UnsubscribeEvents();

                _schedulePage = value;

                _handler = null;
                _collectionSource.Source = null;

                if (null != _schedulePage)
                {
                    _UpdateState();
                    _SubscribeEvents();
                }
            }
        }
        /// <summary>
        /// Method returns orders insertion row or null.
        /// </summary>
        /// <returns></returns>
        public InsertionRow InsertionRow
        {
            get
            {
                return _ordersInsertionRow;
            }
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// Method cancels creation new object and clears insertion row.
        /// </summary>
        /// <param name="justClosed">Is view was just closed. Used because _ordersInsertionRow is not visible but needs to be cancelled.</param>
        public void CancelNewObject(bool justClosed) // REV: why do we have this method? Isn't it a part of CancelObjectEditing?
        {
            // Cancel editing in orders table.
            if (_ordersInsertionRow != null && (_ordersInsertionRow.IsVisible || justClosed) &&
                (_ordersInsertionRow.IsBeingEdited || _ordersInsertionRow.IsDirty))
            {
                OrdersGrid.CancelEdit();
                _ordersInsertionRow.CancelEdit();
                _isNewItemCreated = false;
            }
        }

        /// <summary>
        /// Method tries to save changes in edited grid and returns true if changes ware saved successfully.
        /// </summary>
        public bool SaveEditedItem()
        {
            bool isSuccessfull = false;
            try
            {
                OrdersGrid.EndEdit();
                isSuccessfull = true;
            }
            catch (Exception ex)
            {
                Logger.Warning(ex);
                OrdersGrid.CancelEdit();
            } // if unable to save changes (record has validation error).

            return isSuccessfull;
        }

        #endregion

        #region ICancelDataObjectEditing Members

        /// <summary>
        /// Cancels object editing on orders view.
        /// </summary>
        public void CancelObjectEditing()
        {
            Debug.Assert(OrdersGrid != null);
            OrdersGrid.CancelEdit();

            // Insertion row could be Null reference since it creates
            // only during first call (user click).
            if (OrdersGrid.InsertionRow != null)
                OrdersGrid.InsertionRow.CancelEdit();
        }

        #endregion

        #region ISupportDataObjectEditing Public Porperties

        /// <summary>
        /// ISupportDataObjectEditing implementation.
        /// </summary>
        public bool IsEditingInProgress
        {
            get { return OrdersGrid.IsBeingEdited; }
        }

        #endregion

        #region ISupportDataObjectEditing Events

        /// <summary>
        /// Occurs when editing is starting.
        /// </summary>
        public event DataObjectCanceledEventHandler BeginningEdit;

        /// <summary>
        /// Occurs when editing was started.
        /// </summary>
        public event DataObjectEventHandler EditBegun;

        /// <summary>
        /// Occurs when editing is commiting.
        /// </summary>
        public event DataObjectCanceledEventHandler CommittingEdit;

        /// <summary>
        /// Occurs when editing was commited.
        /// </summary>
        public event DataObjectEventHandler EditCommitted;

        /// <summary>
        /// Occurs when editing was cancelled.
        /// </summary>
        public event DataObjectEventHandler EditCanceled;

        /// <summary>
        /// Occurs when new object is creating.
        /// </summary>
        public event DataObjectCanceledEventHandler CreatingNewObject;

        /// <summary>
        /// Occurs when new object was created.
        /// </summary>
        public event DataObjectEventHandler NewObjectCreated;

        /// <summary>
        /// Occurs when new object is commiting.
        /// </summary>
        public event DataObjectCanceledEventHandler CommittingNewObject;

        /// <summary>
        /// Occurs when new object was commited.
        /// </summary>
        public event DataObjectEventHandler NewObjectCommitted;

        /// <summary>
        /// Occurs when new object was cancelled.
        /// </summary>
        public event DataObjectEventHandler NewObjectCanceled;

        #endregion

        #region Private Methods For Update UI

        /// <summary>
        /// Method changes command buttons set.
        /// </summary>
        private void _ChangeCommandButtonsSet()
        {
            commandButtonsGroup.Initialize(CategoryNames.UnassignedOrdersRoutingCommands, OrdersGrid);
        }

        /// <summary>
        /// Locks/unlocks UI when necessary.
        /// </summary>
        private void _CheckLocked()
        {
            if (_schedulePage.IsLocked)
            {
                lockedGrid.Visibility = Visibility.Visible;
                if (OrdersGrid != null)
                    OrdersGrid.ReadOnly = true;
            }
            else
            {
                lockedGrid.Visibility = Visibility.Hidden;
                if (OrdersGrid != null)
                    OrdersGrid.ReadOnly = false;
            }
        }

        #endregion

        #region Private Drag'n'Drop Helpers Methods

        /// <summary>
        /// Changes cursor during user drags over DataGridControl.
        /// </summary>
        /// <param name="sender">DataGridControl.</param>
        /// <param name="e">Event args.</param>
        private void _DragOverGrid(object sender, DragEventArgs e)
        {
            Row draggedOverRow = XceedVisualTreeHelper.GetRowByEventArgs(e);

            if (draggedOverRow is InsertionRow || draggedOverRow is ColumnManagerRow ||
                (!(e.OriginalSource is TableViewScrollViewer) && draggedOverRow == null))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }
        }

        /// <summary>
        /// Drops orders to grid.
        /// </summary>
        /// <param name="sender">DataGridControl.</param>
        /// <param name="e">Event args.</param>
        private void _OrdersGridDrop(object sender, DragEventArgs e)
        {
            // Get row where object was dropped.
            Row parentRow = XceedVisualTreeHelper.GetRowByEventArgs(e);

            object targetData = OrdersGrid;

            if (parentRow != null)
                targetData = _GetTargetData(parentRow); // Get data from dropped object.

            // Do necessary actions (move or reassign routes etc.).
            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();
            dragAndDropHelper.Drop(targetData, e.Data);
        }

        /// <summary>
        /// Method checks is dragging allowed and starts dragging if possible.
        /// </summary>
        private void _TryToStartDragging()
        {
            Collection<object> selection = _GetSelectedOrders();

            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();
            bool isDragAllowed = dragAndDropHelper.IsDragAllowed(selection);

            if (isDragAllowed && selection.Count > 0)
            {
                // We use deferred call to allow grid complete BringItemIntoView                
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    dragAndDropHelper.StartDragOrders(selection, DragSource.OrdersView);
                }));
            }
        }

        /// <summary>
        /// Method separates orders from items control selection
        /// </summary>
        /// <returns>Collection of selected orders</returns>
        private Collection<object> _GetSelectedOrders()
        {
            Collection<object> selectedOrders = new Collection<object>();
            foreach (object obj in OrdersGrid.SelectedItems)
            {
                Debug.Assert(obj is Order);
                selectedOrders.Add(obj);
            }

            return selectedOrders;
        }

        #endregion

        #region Private ISupportDataObjectEditing Raise Event Methods

        /// <summary>
        /// Raises Bedin Edit.
        /// </summary>
        /// <param name="obj">Edited object.</param>
        private void _OnEditBegun(ESRI.ArcLogistics.Data.DataObject obj)
        {
            if (EditBegun != null)
                EditBegun(this, new DataObjectEventArgs(obj));
        }

        /// <summary>
        /// Raises EditCommited.
        /// </summary>
        /// <param name="obj">Edited object.</param>
        private void _OnEditCommited(ESRI.ArcLogistics.Data.DataObject obj)
        {
            if (EditCommitted != null)
                EditCommitted(this, new DataObjectEventArgs(obj));
        }

        /// <summary>
        /// Raises New Object Commited.
        /// </summary>
        /// <param name="obj">New object.</param>
        private void _OnNewObjectCommited(ESRI.ArcLogistics.Data.DataObject obj)
        {
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, new DataObjectEventArgs(obj));
        }

        /// <summary>
        /// Raises Edit cancelled.
        /// </summary>
        /// <param name="obj">Edited object.</param>
        private void _OnEditCanceled(ESRI.ArcLogistics.Data.DataObject obj)
        {
            if (EditCanceled != null)
                EditCanceled(this, new ESRI.ArcLogistics.App.Pages.DataObjectEventArgs(obj));
        }

        /// <summary>
        /// Raises New Object Created.
        /// </summary>
        /// <param name="obj">New object.</param>
        private void _OnNewObjectCreated(ESRI.ArcLogistics.Data.DataObject obj)
        {
            if (NewObjectCreated != null)
                NewObjectCreated(this, new DataObjectEventArgs(obj));
        }

        /// <summary>
        /// Raises New Object Canceled.
        /// </summary>
        /// <param name="obj">Cancelled object.</param>
        private void _OnNewObjectCanceled(ESRI.ArcLogistics.Data.DataObject obj)
        {
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, new ESRI.ArcLogistics.App.Pages.DataObjectEventArgs(obj));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method removes all main event handlers used in visible view.
        /// </summary>
        private void _UnsubscribeEvents()
        {
            if (_isSubscribedToEvents)
            {
                Debug.Assert(null != _schedulePage);
                _schedulePage.CurrentScheduleChanged -= _schedulePage_CurrentScheduleChanged;
                _schedulePage.LockedPropertyChanged -= _schedulePage_LockedPropertyChanged;

                _isSubscribedToEvents = false;
            }
        }

        /// <summary>
        /// Method adds handlers to all events important for visible view.
        /// </summary>
        private void _SubscribeEvents()
        {
            if (!_isSubscribedToEvents)
            {
                Debug.Assert(null != _schedulePage);
                _schedulePage.CurrentScheduleChanged += new EventHandler(_schedulePage_CurrentScheduleChanged);
                _schedulePage.LockedPropertyChanged += new EventHandler(_schedulePage_LockedPropertyChanged);
                OrdersGrid.DragOver += new DragEventHandler(_DragOverGrid);

                _isSubscribedToEvents = true;
            }
        }

        /// <summary>
        /// Method inits all xceed data grid controls (sets collection source and columns structure foreach of them).
        /// </summary>
        private void _InitGridControl()
        {
            _isNewItemCreated = false;

            _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(ORDERS_COLLECTION_SOURCE_NAME);
            GridStructureInitializer initializer = new GridStructureInitializer(GridSettingsProvider.OrdersGridStructure);
            initializer.BuildGridStructure(_collectionSource, OrdersGrid);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.OrdersSettingsRepositoryName, _collectionSource.ItemProperties);
            layoutLoader.LoadLayout(OrdersGrid);
        }

        /// <summary>
        /// Updates state.
        /// </summary>
        private void _UpdateState()
        {
            _handler = null;
            _collectionSource.Source = null;

            if (GridItemsSourceChanging != null)
                GridItemsSourceChanging(this, EventArgs.Empty);

            if (null != _schedulePage.CurrentSchedule)
            {
                _handler = new OrdersViewContextHandler(_schedulePage);

                _collectionSource.Source =
                   new SortedDataObjectCollection<Order>(_schedulePage.CurrentSchedule.UnassignedOrders, new CreationTimeComparer<Order>());

                if (GridItemsSourceChanged != null)
                    GridItemsSourceChanged(this, EventArgs.Empty);
            }

            _ChangeCommandButtonsSet();
        }

        /// <summary>
        /// React on application close.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AppExit(object sender, ExitEventArgs e)
        {
            _SaveLayout();
        }

        /// <summary>
        /// Save grid layout.
        /// </summary>
        private void _SaveLayout()
        {
            if (Properties.Settings.Default.OrdersGridSettings == null)
                Properties.Settings.Default.OrdersGridSettings = new SettingsRepository();

            OrdersGrid.SaveUserSettings(Properties.Settings.Default.OrdersGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Method returns data from object.
        /// </summary>
        /// <param name="target">Object where from data should be returned.</param>
        /// <returns>Data object from dragged object.</returns>
        private object _GetTargetData(object target)
        {
            object data = new object();
            data = ((DataRow)target).DataContext;
            return data;
        }

        /// <summary>
        /// React on mouse up. Try to geocode order.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridControlPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Do not start geocoding in case of shift or control used or selected more than one order.
            if (_schedulePage.SelectedItems.Count == 1 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0
                && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                Order order = _schedulePage.SelectedItems[0] as Order;
                Row row = XceedVisualTreeHelper.GetRowByEventArgs(e);
                if (order != null && !order.IsGeocoded && !_schedulePage.GeocodablePage.IsGeocodingInProcess &&
                    !_schedulePage.IsEditingInProgress && row != null)
                {
                    // Geocode order.
                    _schedulePage.GeocodablePage.StartGeocoding(order);
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Updates objects collections.
        /// </summary>
        /// <param name="sender">Optimize and edit page sender.</param>
        /// <param name="e">Event args.</param>
        private void _schedulePage_CurrentScheduleChanged(object sender, EventArgs e)
        {
            _UpdateState();
        }

        /// <summary>
        /// Sets Locked state to View.
        /// </summary>
        /// <param name="sender">Optimize and edit page sender.</param>
        /// <param name="e">Event args.</param>
        private void _schedulePage_LockedPropertyChanged(object sender, EventArgs e)
        {
            // update UI locked/unlocked.
            _CheckLocked();
        }

        /// <summary>
        /// Initializes _ordersInsertionRow.
        /// </summary>
        /// <param name="sender">Orders insertion row sender.</param>
        /// <param name="e">Event args.</param>
        private void _OrdersInsertionRowInitialized(object sender, EventArgs e)
        {
            _ordersInsertionRow = (InsertionRow)sender;
        }

        /// <summary>
        /// Updates state when view changed visibility.
        /// </summary>
        private void _VisibileStateChanged(object sender, EventArgs e)
        {
            if (this.IsVisible)
            {
                if (null != _schedulePage)
                {
                    _UpdateState();
                    _SubscribeEvents();
                }

                // update Locked UI.
                _CheckLocked();
            }
            else
            {
                CancelNewObject(true);
                SaveEditedItem();

                _UnsubscribeEvents();

                _handler = null;
                _collectionSource.Source = null;
            }

            if (!_schedulePage.GeocodablePage.IsGeocodingInProcess && _schedulePage.EditingManager.EditedObject != null)
            {
                _schedulePage.CancelObjectEditing();
            }
        }

        /// <summary>
        /// Reinitialize controls when project loaded.
        /// </summary>
        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _InitGridControl();
        }

        /// <summary>
        /// Store layout when project start closing.
        /// </summary>
        private void _ProjectClosing(object sender, EventArgs e)
        {
            _SaveLayout();
        }

        #endregion

        #region Private Data Grid Control Event Handlers (Editing, Creating items etc)

        /// <summary>
        /// Handler begins edit item (calls BeginEditItem method of appropriate ContextHandler).
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceBeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            e.Handled = true;

            ESRI.ArcLogistics.Data.DataObject dataObject = e.Item as ESRI.ArcLogistics.Data.DataObject;
            if (dataObject != null)
            {
                // Define event args from editinf Item.
                DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs(dataObject);

                // Raise BeginningEdit event.
                if (BeginningEdit != null)
                    BeginningEdit(this, args);

                if (!args.Cancel)
                    _handler.BeginEditItem(e); // If action wasn't cancelled - begin editing in appropriate handler.
                else
                    e.Cancel = true;
            }
        }

        /// <summary>
        /// Handler raises event about editing was started.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceEditBegun(object sender, DataGridItemEventArgs e)
        {
            ESRI.ArcLogistics.Data.DataObject dataObject = e.Item as ESRI.ArcLogistics.Data.DataObject;

            if (dataObject != null)
            {
                // Raise event about item editing was started.
                _OnEditBegun(dataObject);
            }
        }

        /// <summary>
        /// Handler commits editing (calls CommitItem method of appropriate ContextHandler).
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            // Define event args from commited object. 
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);

            // Raise commiting edit event.
            if (CommittingEdit != null)
                CommittingEdit(this, args);

            e.Handled = true;

            if (!args.Cancel) // If action was not cancelled - commit changes.
            {
                _handler.CommitItem(e);
                App.Current.Project.Save();
            }
            else
                e.Cancel = true;
        }

        /// <summary>
        /// Handler raise event about Editing was commited.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceEditCommitted(object sender, DataGridItemEventArgs e)
        {
            _handler.CommitedEdit(e);

            // Raise event about item editing was commited.
            _OnEditCommited((ESRI.ArcLogistics.Data.DataObject)e.Item);
        }

        /// <summary>
        /// Handler cancels editing object.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event erags.</param>
        private void _DataGridCollectionViewSourceCancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;

            _handler.CancelEditItem(e);
        }

        /// <summary>
        /// Handler raises event about editing was cancelled.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceEditCanceled(object sender, DataGridItemEventArgs e)
        {
            // Raise event about item editing was cancelled.
            _OnEditCanceled((ESRI.ArcLogistics.Data.DataObject)e.Item);
        }

        /// <summary>
        /// Handler begin creating new item.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCreatingNewItem(object sender, DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new Order(App.Current.Project.CapacitiesInfo, App.Current.Project.OrderCustomPropertiesInfo);

            // NOTE : workaround - used to show drop-downs cell editors in orders insertion row.
            _ordersInsertionRow.CellEditorDisplayConditions = CellEditorDisplayConditions.CellIsCurrent;

            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.NewItem);

            // Raise event about creating new object was started.
            if (CreatingNewObject != null)
                CreatingNewObject(this, args);
            e.Handled = true;

            if (!args.Cancel) // If action was not cancelled - begin creating new item.
            {
                _handler.CreateNewItem(e);
                _isNewItemCreated = true; // Set flag to true because new object was created.
            }
            else
            {
                _isNewItemCreated = false; // Set flag to false because new object wasn't created.
                e.Cancel = true;
            }

            // Reset new item if adding of new item was canceled.
            if (e.Cancel)
                e.NewItem = null;
        }

        private delegate void ParamsDelegate(Xceed.Wpf.DataGrid.DataGridItemEventArgs item);

        /// <summary>
        /// Change item's name.
        /// </summary>
        /// <param name="e">DataGridItemEventArgs.</param>
        private void _ChangeName(Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            // Check that item's name is null.
            if (!string.IsNullOrEmpty((e.Item as Order).Name))
                return;
            
            (e.Item as Order).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
               (IDataObjectCollection<Order>)_collectionSource.Source, e.Item as Order, false);

            Cell currentCell = _ordersInsertionRow.Cells[OrdersGrid.CurrentContext.CurrentColumn];

            // Find TextBox inside the cell and select new name.
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Handler raises event about new item was created.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceNewItemCreated(object sender, DataGridItemEventArgs e)
        {
            // Invoking changing of the project's name. Those methode must be invoke, otherwise 
            // grid didnt understand that item in insertion was changed and grid wouldnt allow to 
            // commit this item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeName),
                DispatcherPriority.Render, e);
            
            // Raise event about new item was created.
            _OnNewObjectCreated((ESRI.ArcLogistics.Data.DataObject)e.Item);
        }

        /// <summary>
        /// Handler begin commiting new item (calls CommitNewItem method of appropriate ContextHandler).
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCommittingNewItem(object sender, DataGridCommittingNewItemEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);

            // Raise event about new item starts commiting.
            if (CommittingNewObject != null)
                CommittingNewObject(this, args);
            e.Handled = true;

            if (!args.Cancel) // If action was not cancelled - commit new item.
                _handler.CommitNewItem(e);
            else
                e.Cancel = true;

            // NOTE : workaround - hide cell editors when new item's commiting.
            if (_ordersInsertionRow != null)
                _ordersInsertionRow.CellEditorDisplayConditions = CellEditorDisplayConditions.None;
        }

        /// <summary>
        /// Raises event about new item was commited.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceNewItemCommitted(object sender, DataGridItemEventArgs e)
        {
            // Raise event about new item was commited.
            _handler.CommittedNewItem(e);

            // Raise event about new item was commited.
            _OnNewObjectCommited((ESRI.ArcLogistics.Data.DataObject)e.Item);
        }

        /// <summary>
        /// Raises event about new item is cancelling.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            // Set property to true if new item was created or to false if new item wasn't created.
            // Otherwise an InvalidOperationException will be thrown (see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Inserting_Data.html).
            e.Handled = _isNewItemCreated;
            _handler.CancellingNewItem(e);

            // NOTE : workaround - hide cell editors when new item's cancelling.
            if (_ordersInsertionRow != null)
                _ordersInsertionRow.CellEditorDisplayConditions = CellEditorDisplayConditions.None;

            _isNewItemCreated = false;
        }

        /// <summary>
        /// Raises event about new item was cancelled.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _NewItemCanceled(object sender, DataGridItemEventArgs e)
        {
            // Raise event about new item was cancelled.
            _OnNewObjectCanceled((ESRI.ArcLogistics.Data.DataObject)e.Item);
        }

        #endregion

        #region Private Drag'n'Drop Handlers

        /// <summary>
        /// Registers that drag and drop can potentially start.
        /// </summary>
        /// <param name="sender">Data grid.</param>
        /// <param name="e">Event args.</param>
        private void _DataGridControlPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check that user clicked on a row.
            Row row = XceedVisualTreeHelper.GetRowByEventArgs(e);
            if (row != null && row is DataRow && !(row is InsertionRow)) // Don't start drag of insertion row.
            {
                // We must start dragging on mouse move.
                _mustStartDraggingOnMouseMove = true;
            }
        }

        /// <summary>
        /// Starts drag object.
        /// </summary>
        /// <param name="sender">Either Main Window or data grid control.</param>
        /// <param name="e">Event args.</param>
        private void _MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && _mustStartDraggingOnMouseMove)
            {
                Row currentRow = OrdersGrid.GetContainerFromItem(OrdersGrid.CurrentItem) as Row;
                if (currentRow == null)
                    return;

                Cell currentCell = currentRow.Cells[OrdersGrid.CurrentContext.CurrentColumn];
                if (currentCell == null || currentCell.IsBeingEdited) // if user try to select text in cell we shouldn't begin drag'n'drop
                    return;

                if (OrdersGrid.SelectedItems.Count > 0)
                    _TryToStartDragging();
            }

            // Reset the flat on any mouse move without pressed left button.
            _mustStartDraggingOnMouseMove = false;
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Orders grid collection source resource name.
        /// </summary>
        private const string ORDERS_COLLECTION_SOURCE_NAME = "ordersGridItemsCollection";

        #endregion

        #region Private Members

        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _schedulePage;

        /// <summary>
        /// Orders table insertion row.
        /// </summary>
        private InsertionRow _ordersInsertionRow;

        /// <summary>
        /// Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem.
        /// </summary>
        private bool _isNewItemCreated;

        /// <summary>
        /// Flag shows that list view was subscribed to necessary events.
        /// </summary>
        private bool _isSubscribedToEvents;

        /// <summary>
        /// Grid source collection.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource;

        /// <summary>
        /// Context handler.
        /// </summary>
        private OrdersViewContextHandler _handler;

        /// <summary>
        /// Flag shows whether control must start dragging on mouse move.
        /// </summary>
        private bool _mustStartDraggingOnMouseMove = false;

        #endregion
    }
}
