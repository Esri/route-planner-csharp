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
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;

using AppData = ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.DragAndDrop;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for RoutesView.xaml.
    /// </summary>
    internal partial class RoutesView : DockableContent, ISupportDataObjectEditing, ICancelDataObjectEditing
    {
        #region Constructors

        /// <summary>
        /// Constructor. Creates new instance of List view and sets property ParentPage from constructor parameter.
        /// </summary>
        public RoutesView()
        {
            InitializeComponent();

            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(_ProjectClosing);
            this.VisibileStateChanged += new EventHandler(_VisibileStateChanged);
            App.Current.Exit += new ExitEventHandler(_AppExit);

            if (App.Current.Project != null)
                _InitGridControl();

            App.Current.Solver.AsyncSolveCompleted += new ESRI.ArcLogistics.Routing.AsyncSolveCompletedEventHandler(_AsyncSolveCompleted);

            new ViewButtonsMarginUpdater(this, commandButtonsGroup);

            // Subscribe on mouse move for drag and drop.
            Application.Current.MainWindow.MouseMove += new MouseEventHandler(_MouseMove);

            // Init timer for expanding Route Details.
            _expandTimer = new Timer(EXPAND_ROUTE_DETAILS_TIME_INTERVAL);
            _expandTimer.Elapsed += new ElapsedEventHandler(_TimeElapsed);

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController
                (RoutesGrid);
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
        /// Method returns routes insertion row or null.
        /// </summary>
        /// <returns></returns>
        public InsertionRow InsertionRow
        {
            get { return _routesInsertionRow; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method cancels creation new object and clears insertion row.
        /// </summary>
        public void CancelNewObject() 
        {
            // Cancel editing in routes table.
            if (_routesInsertionRow != null && _routesInsertionRow.IsVisible &&
                (_routesInsertionRow.IsBeingEdited || _routesInsertionRow.IsDirty))
            {
                RoutesGrid.CancelEdit();
                _routesInsertionRow.CancelEdit();
                _isNewItemCreated = false;
            }
        }

        /// <summary>
        /// Method tries to save changes in edited grid and returns true if changes were saved successfully.
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        public bool SaveEditedItem()
        {
            bool isSuccessfull = false;
            try
            {
                // Therefore we have master-detail structure we need to end editing in current context.
                RoutesGrid.CurrentContext.EndEdit();
                isSuccessfull = true;
            }
            catch
            {
                // If unable to save changes (record has validation error) we need to cancel editing in current context.
                RoutesGrid.CurrentContext.CancelEdit();
            } 

            return isSuccessfull;
        }

        /// <summary>
        /// Is item can be selected.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Is item can be selected.</returns>
        public bool SelectionFilterCallback(object item)
        {
            bool result = true;

            Stop stop = item as Stop;
            if (stop != null)
            {
                result = RoutesGrid.Items.Contains(stop.Route);

                if (result && !_IsRouteStopExpanded(stop))
                {
                    RoutesGrid.ExpandDetails(stop.Route);
                }
            }

            return result;
        }

        #endregion

        #region ICancelDataObjectEditing Members

        /// <summary>
        /// Cancels object editing on routes view.
        /// </summary>
        public void CancelObjectEditing()
        {
            Debug.Assert(RoutesGrid != null);
            RoutesGrid.CancelEdit();

            // Insertion row could be Null reference since it creates
            // only during first call (user click).
            if (RoutesGrid.InsertionRow != null)
                RoutesGrid.InsertionRow.CancelEdit();
        }

        #endregion

        #region ISupportDataObjectEditing Public Porperties

        /// <summary>
        /// ISupportDataObjectEditing implementation.
        /// </summary>
        public bool IsEditingInProgress
        {
            get { return RoutesGrid.IsBeingEdited; }
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
            string currentCommands = CategoryNames.RoutesRoutingCommands;

            // If selection contains at least one stop - show stops commands set. Otherwise show route commands set.
            if (_schedulePage != null && _schedulePage.SelectedItems != null)
            {
                foreach (Object item in _schedulePage.SelectedItems)
                {
                    if (item is Stop)
                    {
                        currentCommands = CategoryNames.RouteRoutingCommands;
                        break;
                    }
                }
            }

            commandButtonsGroup.Initialize(currentCommands, RoutesGrid);
        }

        /// <summary>
        /// Locks/unlocks UI when necessary.
        /// </summary>
        private void _CheckLocked()
        {
            if (_schedulePage.IsLocked)
            {
                lockedGrid.Visibility = Visibility.Visible;
                if (RoutesGrid != null)
                    RoutesGrid.ReadOnly = true;
            }
            else
            {
                lockedGrid.Visibility = Visibility.Hidden;
                if (RoutesGrid != null)
                    RoutesGrid.ReadOnly = false;
            }
        }

        #endregion

        #region Private Drag'n'Drop Helpers Methods

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
                _schedulePage.CurrentScheduleChanged -= _SchedulePageCurrentScheduleChanged;
                _schedulePage.LockedPropertyChanged -= _SchedulePageLockedPropertyChanged;
                _schedulePage.SelectionManager.NotifyMultiCollectionChanged -=
                    new NotifyMultiCollectionChangedEventHandler(_SelectionManagerNotifyMultiCollectionChanged);

                _schedulePage.SelectionChanged -= _SchedulePageSelectionChanged;
                RoutesGrid.DragOver -= _DragOverGrid;
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
                _schedulePage.CurrentScheduleChanged += new EventHandler(_SchedulePageCurrentScheduleChanged);
                _schedulePage.LockedPropertyChanged += new EventHandler(_SchedulePageLockedPropertyChanged);
                _schedulePage.SelectionManager.NotifyMultiCollectionChanged +=
                    new NotifyMultiCollectionChangedEventHandler(_SelectionManagerNotifyMultiCollectionChanged);

                _schedulePage.SelectionChanged += new EventHandler(_SchedulePageSelectionChanged);
                RoutesGrid.DragOver += new DragEventHandler(_DragOverGrid);

                _isSubscribedToEvents = true;
            }
        }

        /// <summary>
        /// Changes command set dependent on selection.
        /// </summary>
        /// <param name="sender">OptimizeAndEdit page.</param>
        /// <param name="e">Event args.</param>
        private void _SchedulePageSelectionChanged(object sender, EventArgs e)
        {
            _ChangeCommandButtonsSet();
        }

        /// <summary>
        /// Method inits all xceed data grid controls (sets collection source and columns structure foreach of them).
        /// </summary>
        private void _InitGridControl()
        {
            _isNewItemCreated = false;

            // init grid structure
            if (_collectionSource == null)
                _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(ROUTES_COLLECTION_SOURCE_NAME);

            // Set collection source to null to refresh items collection.
            _collectionSource.Source = null;

            GridStructureInitializer initializer = new GridStructureInitializer(GridSettingsProvider.RoutesGridStructure);
            initializer.BuildGridStructure(_collectionSource, RoutesGrid);

            // init detail structure
            DetailConfiguration orders = (DetailConfiguration)LayoutRoot.FindResource(ROUTES_DETAIL_CONFIGURATION_NAME);
            GridStructureInitializer detailInitializer = new GridStructureInitializer(GridSettingsProvider.StopsGridStructure);
            detailInitializer.BuildDetailStructure(_collectionSource, RoutesGrid, orders);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.RoutesSettingsRepositoryName, _collectionSource.ItemProperties,
                 _collectionSource.DetailDescriptions[0].ItemProperties);
            layoutLoader.LoadLayout(RoutesGrid);
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
                _collectionSource.Source =
                    new AppData.SortedDataObjectCollection<Route>(_schedulePage.CurrentSchedule.Routes, new RoutesComparer());
                _handler = new RoutesListViewContextHandler(_schedulePage);

                if (GridItemsSourceChanged != null)
                    GridItemsSourceChanged(this, EventArgs.Empty);
            }

            _ChangeCommandButtonsSet();
        }

        /// <summary>
        /// Add stop to selection.
        /// </summary>
        /// <param name="stop">Stop to select.</param>
        private void _AddStopToSelection(Stop stop)
        {
            if (!RoutesGrid.Items.Contains(stop.Route))
                return;

            // If stop route is not expanded - expand and select it.
            bool isStopParentExpanded = _IsRouteStopExpanded(stop);
            if (!isStopParentExpanded)
            {
                RoutesGrid.ExpandDetails(stop.Route);
            }

            RoutesGrid.SelectInChildContext(stop);
        }

        /// <summary>
        /// Check is route of stop expanded.
        /// </summary>
        /// <param name="stop">Stop to check.</param>
        /// <returns>Is route of stop expanded.</returns>
        private bool _IsRouteStopExpanded(Stop stop)
        {
            bool isStopParentExpanded = false;

            IEnumerable<DataGridContext> childContexts = RoutesGrid.GetChildContexts();
            foreach (DataGridContext dataGridContext in childContexts)
            {
                if (dataGridContext.Items.Contains(stop))
                {
                    isStopParentExpanded = true;
                    break;
                }
            }

            return isStopParentExpanded;
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
            if (Properties.Settings.Default.RoutesGridSettings == null)
                Properties.Settings.Default.RoutesGridSettings = new SettingsRepository();

            RoutesGrid.SaveUserSettings(Properties.Settings.Default.RoutesGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles PropertyChanged event to support editing bool fields by single click.
        /// </summary>
        /// <param name="sender">List view sender.</param>
        /// <param name="e">Property changed event args.</param>
        private void _RoutesGridObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(IS_LOCKED_PROPERTY_NAME) || e.PropertyName.Equals(IS_VISIBLE_PROPERTY_NAME))
            {
                // Call EndEdit to save changes and cancel editing mode in RoutesGrid.
                if (RoutesGrid.CurrentContext != null)
                    RoutesGrid.CurrentContext.EndEdit();
            }
        }

        /// <summary>
        /// Updates objects collections.
        /// </summary>
        /// <param name="sender">Optimize and edit page sender.</param>
        /// <param name="e">Event args.</param>
        private void _SchedulePageCurrentScheduleChanged(object sender, EventArgs e)
        {
            _UpdateState();
        }

        /// <summary>
        /// Sets Locked state to View.
        /// </summary>
        /// <param name="sender">Optimize and edit page sender.</param>
        /// <param name="e">Event args.</param>
        private void _SchedulePageLockedPropertyChanged(object sender, EventArgs e)
        {
            // Update UI locked/unlocked.
            _CheckLocked();
        }

        /// <summary>
        /// React on changes in multi collection binding.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Collection changed event args.</param>
        private void _SelectionManagerNotifyMultiCollectionChanged(object sender, NotifyMultiCollectionChangedEventArgs e)
        {
            if (e.Initiator == RoutesGrid.SelectedItems)
                return;

            // If collection reset - reset also selected items in all child contexts.
            if (e.EventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                IEnumerable<DataGridContext> childContexts = RoutesGrid.GetChildContexts();
                foreach (DataGridContext dataGridContext in childContexts)
                    dataGridContext.SelectedItems.Clear();
            }
            else if (e.EventArgs.NewItems != null && e.EventArgs.NewItems.Count > 0)
            {
                // If stop added - bring it to view.
                Stop stopToBringIntoView = null;
                foreach (object obj in e.EventArgs.NewItems)
                {
                    Stop stop = obj as Stop;
                    if (stop != null)
                    {
                        _AddStopToSelection(stop);

                        if (stopToBringIntoView == null)
                            stopToBringIntoView = stop; // store first
                    }
                }

                if (stopToBringIntoView != null)
                {
                    RoutesGrid.BringItemIntoView(stopToBringIntoView);
                }
            }
        }

        /// <summary>
        /// Collapses details id details table is empty.
        /// </summary>
        /// <param name="sender">Solver.</param>
        /// <param name="e">Event args.</param>
        private void _AsyncSolveCompleted(object sender, ESRI.ArcLogistics.Routing.AsyncSolveCompletedEventArgs e)
        {
            // Collapse details if details table is empty.
            List<DataGridContext> dataGridContexts = new List<DataGridContext>(RoutesGrid.GetChildContexts());
            foreach (DataGridContext dataGridContext in dataGridContexts)
            {
                if (dataGridContext.Items.Count == 0)
                {
                    dataGridContext.ParentDataGridContext.CollapseDetails(dataGridContext.ParentItem);
                }
            }
        }

        /// <summary>
        /// Initializes _routesInsertionRow.
        /// </summary>
        /// <param name="sender">Routes insertion row sender.</param>
        /// <param name="e">Event args.</param>
        private void _RoutesInsertionRowInitialized(object sender, EventArgs e)
        {
            _routesInsertionRow = (InsertionRow)sender;
        }

        /// <summary>
        /// Changes visibility af necessary cells when Insertion row loaded.
        /// </summary>
        /// <param name="sender">Insertion Row.</param>
        /// <param name="e">Event args.</param>
        private void _RouteInsertionRowLoaded(object sender, RoutedEventArgs e)
        {
            if (_routesInsertionRow.Cells[IS_LOCKED_PROPERTY_NAME] != null)
                _routesInsertionRow.Cells[IS_LOCKED_PROPERTY_NAME].Visibility = Visibility.Collapsed;

            if (_routesInsertionRow.Cells[IS_VISIBLE_PROPERTY_NAME] != null)
                _routesInsertionRow.Cells[IS_VISIBLE_PROPERTY_NAME].Visibility = Visibility.Collapsed;

            if (_routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME] != null)
                _routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME].Visibility = Visibility.Collapsed;

            if (_routesInsertionRow.Cells[STATUS_PROPERTY_NAME] != null)
                _routesInsertionRow.Cells[STATUS_PROPERTY_NAME].Visibility = Visibility.Collapsed;
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

                // Update UI locked/unlocked.
                _CheckLocked();
            }
            else
            {
                CancelNewObject();
                SaveEditedItem();

                _UnsubscribeEvents();

                _handler = null;
                _collectionSource.Source = null;
            }
        }

        /// <summary>
        /// Itialize controls when project loaded if it was not initialized before.
        /// </summary>
        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _InitGridControl();
        }

        /// <summary>
        /// Handles ProjectClosing event.
        /// </summary>
        private void _ProjectClosing(object sender, EventArgs e)
        {
            _SaveLayout();
        }

        /// <summary>
        /// Expand route details on time interval elapsed.
        /// Timer not stopped and leave enabled to do not auto-collapse route details if user 
        /// keep dragging over row. Timer will be stopped on DragLeaveRow event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _TimeElapsed(object sender, ElapsedEventArgs e)
        {
            // Make a deferred function call to Expand or Collapse route details for Current Row.
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                if (_currentRowToExpand != null)
                {
                    // Get grid context to know if details expaned for current row.
                    DataGridControl gridControl = (DataGridControl)RoutesGrid;
                    DataGridContext gridContext = DataGridControl.GetDataGridContext(gridControl);

                    if (gridContext.AreDetailsExpanded(_currentRowToExpand))
                        RoutesGrid.CollapseDetails(_currentRowToExpand);
                    else
                        RoutesGrid.ExpandDetails(_currentRowToExpand);

                    _currentRowToExpand = null;
                }
            }));
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
            if (e.Item is AppData.DataObject)
            {
                // Define event args from editinf Item.
                var args = _CreateDataObjectCanceledEventArgs(e.Item);

                // Raise BeginningEdit event.
                if (BeginningEdit != null)
                    BeginningEdit(this, args);

                // Add handler to PropertyChanged event - need to support editing bool fields
                // by single click and save changes.
                ((INotifyPropertyChanged)e.Item).PropertyChanged +=
                    new PropertyChangedEventHandler(_RoutesGridObjectPropertyChanged);

                if (!args.Cancel)
                    _handler.BeginEditItem(e); // If action wasn't cancelled - begin editing
                // in appropriate handler.
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
            if (CommonHelpers.IgnoreVirtualLocations(e.Item))
            {
                // Remove handler to PropertyChanged (was added before for support 
                // editing bool fields by single click and save changes).
                ((INotifyPropertyChanged)e.Item).PropertyChanged -= _RoutesGridObjectPropertyChanged;

                // Define event args from commited object.
                var args = _CreateDataObjectCanceledEventArgs(e.Item);

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
            else
            {
                e.Handled = true;
                e.Cancel = true;
            }
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
            // Remove handler to PropertyChanged (was added before for support editing bool fields by single click and save changes).
            ((INotifyPropertyChanged)e.Item).PropertyChanged -= _RoutesGridObjectPropertyChanged;

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
            // If current visible collection is routes.
            e.NewItem = App.Current.Project.CreateRoute();

            // NOTE : workaround - used to show drop-downs cell editors in routes insertion row.
            _routesInsertionRow.CellEditorDisplayConditions = CellEditorDisplayConditions.CellIsCurrent;

            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.NewItem);

            // Raise event about creating new object was started.
            if (CreatingNewObject != null)
                CreatingNewObject(this, args);
            e.Handled = true;

            if (!args.Cancel) // If action was not cancelled - begin creating new item.
            {
                _handler.CreateNewItem(e);
                _isNewItemCreated = true; // Set flag to true because new object was created.

                if (_routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME] != null)
                    _routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME].Visibility = Visibility.Visible;
            }
            else
            {
                _isNewItemCreated = false; // Set flag to false because new object wasn't created.
                e.Cancel = true;
            }
        }

        private delegate void ParamsDelegate(Xceed.Wpf.DataGrid.DataGridItemEventArgs item);

        /// <summary>
        /// Change item's name.
        /// </summary>
        /// <param name="e">DataGridItemEventArgs.</param>
        private void _ChangeName(Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            // Check that item's name is null.
            if (!string.IsNullOrEmpty((e.Item as Route).Name))
                return;

            // Get new route name.
            var routes = e.CollectionView.SourceCollection as IEnumerable<Route>;
            (e.Item as Route).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                routes, e.Item as Route, true);

            var routeNameCell = _routesInsertionRow.Cells[ROUTE_NAME_COLUMN];
            // The cell is not marked dirty by itself because we never set it's content to the
            // new route, but changed route object itself. So we need to mark it dirty to make
            // insertion row commit new items instead of cancelling them.
            routeNameCell.MarkDirty();

            // Find TextBox inside the cell and select new name.
            Cell currentCell = _routesInsertionRow.Cells[RoutesGrid.CurrentContext.CurrentColumn];

            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Handler raises event about new item was created and invoking changing name of zone.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceNewItemCreated(object sender,
                                                                 DataGridItemEventArgs e)
        {
            // Invoking changing of the item's name. Those method must be invoked, otherwise 
            // grid will not understand that item in insertion ro was changed and grid wouldnt allow
            // to commit this item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeName),
                System.Windows.Threading.DispatcherPriority.Render, e);

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
            if (CommonHelpers.IgnoreVirtualLocations(e.Item))
            {
                var args = _CreateDataObjectCanceledEventArgs(e.Item);

                // Raise event about new item starts commiting.
                if (CommittingNewObject != null)
                    CommittingNewObject(this, args);

                e.Handled = true;

                if (!args.Cancel) // If action was not cancelled - commit new item.
                    _handler.CommitNewItem(e);
                else
                    e.Cancel = true;

                // NOTE : workaround - hide cell editors when new item's commiting.
                if (_routesInsertionRow != null)
                    _routesInsertionRow.CellEditorDisplayConditions =
                        CellEditorDisplayConditions.None;
            }
            else
            {
                e.Handled =
                    e.Cancel = true;

                RoutesGrid.BeginEditCurrentCell(_routesInsertionRow);
            }
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

            if (_routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME] != null)
                _routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME].Visibility = Visibility.Collapsed;

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

            if (_routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME] != null)
                _routesInsertionRow.Cells[HARD_ZONES_PROPERTY_NAME].Visibility = Visibility.Collapsed;

            // NOTE : workaround - hide cell editors when new item's cancelling.
            if (_routesInsertionRow != null)
                _routesInsertionRow.CellEditorDisplayConditions = CellEditorDisplayConditions.None;

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
        /// Handler changes drag cursor and starts timer to expand route's details
        /// when user drags any order over row.
        /// </summary>
        /// <param name="sender">Row.</param>
        /// <param name="e">Drag event args.</param>
        private void _DragOverRow(object sender, DragEventArgs e)
        {
            var draggedOverRow = sender as Row;

            Object dragOveredObject = draggedOverRow.DataContext;

            var dragAndDropHelper = new DragAndDropHelper();

            // Check if Drag&Drop operation allowed.
            ICollection<Order> orders = dragAndDropHelper.GetDraggingOrders(e.Data);
            bool isDropAllowed = dragAndDropHelper.DoesDropAllowed(dragOveredObject, orders);

            if (!isDropAllowed)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
            else
                e.Effects = DragDropEffects.Move;

            // If dragged over route has stops and timer disabled (not turned on yet or already
            // turned off) - start timer and remember context.
            var route = dragOveredObject as Route;
            if ((null != route) &&
                route.Stops != null &&
                route.Stops.Count > 0 &&
                _expandTimer.Enabled == false)
            {
                _expandTimer.Enabled = true;
                _expandTimer.Start();
                _currentRowToExpand = ((Row)sender).DataContext;
            }
        }

        /// <summary>
        /// Handler disables timer when user leave dragged row.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DragLeaveRow(object sender, DragEventArgs e)
        {
            // Disable timer since user leave from current row and no need to expand row details.
            _expandTimer.Stop();
            _expandTimer.Enabled = false;
        }

        /// <summary>
        /// Changes cursor to "locked" when user drags over insertion row, column manager row or empty space.
        /// </summary>
        /// <param name="sender">DataGridControl.</param>
        /// <param name="e">Event args.</param>
        private void _DragOverGrid(object sender, DragEventArgs e)
        {
            Row draggedOverRow = XceedVisualTreeHelper.GetRowByEventArgs(e);

            if (draggedOverRow == null || draggedOverRow is InsertionRow || draggedOverRow is ColumnManagerRow)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Method checks is dragging allowed and starts dragging if possible.
        /// </summary>
        private void _TryToStartDragging()
        {
            Collection<object> selection = _GetSelectedStops();

            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();
            bool isDragAllowed = dragAndDropHelper.IsDragAllowed(selection);

            if (isDragAllowed && selection.Count > 0)
            {
                // we use deferred call to allow grid complete BringItemIntoView
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    dragAndDropHelper.StartDragOrders(selection, DragSource.RoutesView);
                }));
            }
        }

        /// <summary>
        /// Method separates stop from items control selection.
        /// </summary>
        /// <returns>Collection of selected stops.</returns>
        private Collection<object> _GetSelectedStops()
        {
            Collection<object> selectedOrders = new Collection<object>();
            foreach (object obj in _schedulePage.SelectedItems)
            {
                if (obj is Stop)
                    selectedOrders.Add(obj);
            }

            return selectedOrders;
        }

        /// <summary>
        /// Drops objects to grid.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Event args.</param>
        private void _DataGridControlDrop(object sender, DragEventArgs e)
        {
            // Get row where object was dropped.
            Row parentRow = XceedVisualTreeHelper.GetRowByEventArgs(e);
            if (parentRow == null)
                return;

            // Get data from dropped object.
            object targetData = _GetTargetData(parentRow);

            // Do necessary actions (move or reassign routes etc.).
            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();
            dragAndDropHelper.Drop(targetData, e.Data);
        }

        /// <summary>
        /// Registers that drag and drop can potentially start.
        /// </summary>
        /// <param name="sender">Data grid.</param>
        /// <param name="e">Event args.</param>
        private void _DataGridControlPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check that user clicked on a row.
            Row row = XceedVisualTreeHelper.GetRowByEventArgs(e);
            if (row != null && row is DataRow && !(row is InsertionRow)) // Don't start drag of insertion row or routes.
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
                Row currentRow = null;
                IEnumerable<DataGridContext> childContexts = RoutesGrid.GetChildContexts();
                foreach (DataGridContext dataGridContext in childContexts)
                {
                    if (dataGridContext.CurrentItem != null)
                    {
                        currentRow = dataGridContext.GetContainerFromItem(dataGridContext.CurrentItem) as Row;
                        break;
                    }
                }

                if (currentRow == null)
                    return;

                // If user try to select text in cell we shouldn't begin drag'n'drop.
                Cell currentCell = currentRow.Cells[RoutesGrid.CurrentContext.CurrentColumn];
                if (currentCell == null || currentCell.IsBeingEdited) 
                    return;

                if (_schedulePage.SelectedItems.Count > 0)
                    _TryToStartDragging();
            }

            // Reset the flat on any mouse move without pressed left button.
            _mustStartDraggingOnMouseMove = false;
        }

        /// <summary>
        /// Creates data object canceled event arguments.
        /// </summary>
        /// <param name="obj">Data objcte (Route).</param>
        /// <returns>Created data object canceled event arguments.</returns>
        private DataObjectCanceledEventArgs _CreateDataObjectCanceledEventArgs(object obj)
        {
            Debug.Assert(null != obj);

            var dataObject = obj as AppData.DataObject;
            Debug.Assert(null != dataObject);

            var args = new DataObjectCanceledEventArgs(dataObject);
            return args;
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Routes grid collection source resource name.
        /// </summary>
        private const string ROUTES_COLLECTION_SOURCE_NAME = "routesGridItemsCollection";

        /// <summary>
        /// Routes grid detail configuration resource name.
        /// </summary>
        private const string ROUTES_DETAIL_CONFIGURATION_NAME = "routeDetailConfiguration";

        /// <summary>
        /// "IsLocked" string.
        /// </summary>
        private const string IS_LOCKED_PROPERTY_NAME = "IsLocked";

        /// <summary>
        /// "IsVisible" string.
        /// </summary>
        private const string IS_VISIBLE_PROPERTY_NAME = "IsVisible";

        /// <summary>
        /// "Status" string.
        /// </summary>
        private const string STATUS_PROPERTY_NAME = "Status";

        /// <summary>
        /// "HardZones" string.
        /// </summary>
        private const string HARD_ZONES_PROPERTY_NAME = "HardZones";

        /// <summary>
        /// Time interval before expand Route Details on Drag and Drop.
        /// </summary>
        private const int EXPAND_ROUTE_DETAILS_TIME_INTERVAL = 750;

        /// <summary>
        /// The name of the column containing route names.
        /// </summary>
        private const string ROUTE_NAME_COLUMN = "Name";
        #endregion

        #region Private Members

        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _schedulePage;

        /// <summary>
        /// Routes table insertion row.
        /// </summary>
        private InsertionRow _routesInsertionRow;

        /// <summary>
        /// Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem.
        /// </summary>
        private bool _isNewItemCreated = false;

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
        private RoutesListViewContextHandler _handler;

        /// <summary>
        /// Flag shows whether control must start dragging on mouse move.
        /// </summary>
        private bool _mustStartDraggingOnMouseMove = false;

        /// <summary>
        /// Timer to expand route details on Drag and Drop.
        /// </summary>
        private Timer _expandTimer;

        /// <summary>
        /// Context of Row to expand during Drag and Drop.
        /// </summary>
        private object _currentRowToExpand;

        #endregion
    }
}
