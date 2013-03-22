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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.App.Widgets;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Utility.Reflection;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for OptimizeAndEditPage.xaml.
    /// </summary>
    internal partial class OptimizeAndEditPage :
        PageBase,
        IOptimizeAndEditPage,
        ISupportDataObjectEditing,
        ISupportSelection,
        ISupportSelectionChanged,
        ICancelDataObjectEditing
    {
        public const string PAGE_NAME = "OptimizeAndEdit";

        #region Constructors

        /// <summary>
        /// Constructor. Creates new instance of OptimizeAndEditPage, initializes multi collection binding, map control, 
        /// event handlers, PageBase properties and command to cancel routing.
        /// </summary>
        public OptimizeAndEditPage()
        {
            InitializeComponent();

            // Sets parent windoe for docking.
            DockManager1.ParentWindow = Application.Current.MainWindow;

            _InitEventHandlers();

            _InitGeocodablePage();

            // Define main PageBase properties.
            IsRequired = true;
            CanBeLeft = true;
            DoesSupportCompleteStatus = true;
            IsAllowed = true;

            // Add solver handlers.
            IVrpSolver solver = App.Current.Solver;
            if (solver != null)
            {
                solver.AsyncSolveStarted +=
                    new AsyncSolveStartedEventHandler(OptimizeAndEditPage_AsyncSolveStarted);
                solver.AsyncSolveCompleted +=
                    new AsyncSolveCompletedEventHandler(OptimizeAndEditPage_AsyncSolveCompleted);
            }

            _selectionManager = new SelectionManager(this);
            _selectionManager.SelectionChanged +=
                new EventHandler(_SelectionManagerSelectionChanged);

            _dateSelectionKeeper = new DateSelectionKeeper();

            // Create handlers to all editing events.
            _InitEditingEventHandlers();

            // Creates command to cancel routing operation.
            _InitCancelRoutingButtonCommand();

            _InitializeScheduleManager();
        }

        #endregion

        #region Public Static Properties

        /// <summary>
        /// Gets no-selection status format string.
        /// </summary>
        public static string NoSelectionStatusFormat
        {
            get
            {
                return NO_SELECTION_GRID_STATUS_FORMAT;
            }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when IsLocked property changed.
        /// </summary>
        public event EventHandler LockedPropertyChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns map panel.
        /// </summary>
        public MapView MapView
        {
            get { return _mapView; }
        }

        /// <summary>
        /// Returns time panel.
        /// </summary>
        public TimeView TimeView
        {
            get { return _timeView; }
        }

        /// <summary>
        /// Returns orders panel.
        /// </summary>
        public OrdersView OrdersView
        {
            get { return _ordersView; }
        }

        /// <summary>
        /// Returns routes panel.
        /// </summary>
        public RoutesView RoutesView
        {
            get { return _routesView; }
        }

        /// <summary>
        /// Editing manager.
        /// </summary>
        public ScheduleViewsEditingManager EditingManager
        {
            get
            {
                Debug.Assert(_scheduleViewsEditingManager != null);
                return _scheduleViewsEditingManager;
            }
        }

        /// <summary>
        /// Gets/sets lock/unlock status.
        /// </summary>
        public bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                _isLocked = value;

                if (CurrentSchedule != null)
                    _SetLockedStatus();
                NotifyLockedPropertyChanged();
            }
        }

        /// <summary>
        /// Geocodable page helper.
        /// </summary>
        public GeocodablePage GeocodablePage
        {
            get
            {
                return _geocodablePage;
            }
        }

        /// <summary>
        /// Selection manager.
        /// </summary>
        public SelectionManager SelectionManager
        {
            get
            {
                return _selectionManager;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method adds routing status to routing statuses collection.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="date"></param>
        public void SetRoutingStatus(String status, DateTime date)
        {
            if (!_lockedStatuses.ContainsKey(date))
                _lockedStatuses.Add(date, status);
            else
                _lockedStatuses[date] = status;
        }

        /// <summary>
        /// Method sets "Cancelling..." status.
        /// </summary>
        /// <param name="date"></param>
        public void SetCancellingStatus(DateTime date)
        {
            App app = App.Current;
            if (_lockedStatuses.ContainsKey(app.CurrentDate))
                _lockedStatuses[app.CurrentDate] = app.FindString("CancellingOperation");

            _SetLockedStatus();
        }

        /// <summary>
        /// Sets schedule when routing operation is completed.
        /// </summary>
        /// <param name="builtSchedule">Changed schedule.</param>
        public void OnScheduleChanged(Schedule builtSchedule)
        {
            _CacheSchedule(builtSchedule);
            if (builtSchedule.PlannedDate == App.Current.CurrentDate)
                _LoadSchedule(true);
        }

        /// <summary>
        /// Do regeocoding.
        /// </summary>
        /// <param name="order">Order to regeocode.</param>
        public void StartGeocoding(ESRI.ArcLogistics.Geocoding.IGeocodable order)
        {
            _geocodablePage.StartGeocoding(order);
        }

        /// <summary>
        /// Get last used schedule for date from hash.
        /// </summary>
        /// <param name="plannedDate">Date.</param>
        /// <returns>Last used schedule</returns>
        public Schedule GetLastUsedScheduleForDate(DateTime plannedDate)
        {
            var newSchedule = default(Schedule);
            if (_schedules.TryGetValue(plannedDate, out newSchedule))
            {
                return newSchedule;
            }

            // In case of selecting from "Find orders" order\stop, from schedule of
            // not opened date yet.
            Project project = App.Current.Project;

            IDataObjectCollection<Schedule> schedules = project.Schedules.Search(plannedDate);
            if (schedules.Count > 0)
            {
                newSchedule = OptimizeAndEditHelpers.FindScheduleToSelect(schedules);
                _CacheSchedule(newSchedule);
            }

            return newSchedule;
        }

        /// <summary>
        /// Method returns current visible data grid control with Unassigned orders context.
        /// Used in geocodable page constructor.
        /// </summary>
        /// <returns>Data grid control with Unassigned orders context.
        /// If such control was not found - returns null.</returns>
        public DataGridControlEx GetOrdersGrid()
        {
            return OrdersView.OrdersGrid;
        }

        /// <summary>
        /// Remove selection on date.
        /// </summary> 
        /// <param name="date">Date.</param>
        public void DeleteStoredSelection(DateTime date)
        {
            _dateSelectionKeeper.RestoreSelection(date);
        }

        #endregion

        #region Public Page Overrided Members

        /// <summary>
        /// Gets page name.
        /// </summary>
        public override string Name
        {
            get { return PAGE_NAME; }
        }

        /// <summary>
        /// Gets page title.
        /// </summary>
        public override string Title
        {
            get { return App.Current.FindString("OptimizeAndEditPageCaption"); }
        }

        /// <summary>
        /// Gets page icon.
        /// </summary>
        public override TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("OptimizeAndEditBrush");
                return brush;
            }
        }

        #endregion

        #region Public PageBase Overrided Members

        /// <summary>
        /// Saves current layout to user config file.
        /// </summary>
        internal override void SaveLayout()
        {
            Settings settings = Settings.Default;
            settings.DockingLayoutStateShedulePage = DockManager1.GetLayoutAsXml();
            settings.Save();
        }

        /// <summary>
        /// Gets help text.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.SchedulePagePath); }
        }

        /// <summary>
        /// Gets commands category name.
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return CategoryNames.ScheduleTaskWidgetCommands; }
        }

        #endregion

        #region IOptimizeAndEditPage Members
        /// <summary>
        /// Fired when current schedule was changed.
        /// </summary>
        public event EventHandler CurrentScheduleChanged;

        /// <summary>
        /// Gets or sets current schedule.
        /// </summary>
        public Schedule CurrentSchedule
        {
            get
            {
                if (_currentScheduleManager != null)
                    return _currentScheduleManager.ActiveSchedule;

                return null;
            }
            set
            {
                _SetActiveSchedule(value);
            }
        }
        #endregion

        #region ICancelDataObjectEditing Members

        /// <summary>
        /// Method cancels editing edited object.
        /// </summary>
        public void CancelObjectEditing()
        {
            _routesView.CancelObjectEditing();
            _ordersView.CancelObjectEditing();
        }

        #endregion

        #region ISupportDataObjectEditing Members

        /// <summary>
        /// Gets/sets bool value - "true" if any view in OptimizeAndEdit page is in editind state
        /// and "false" otherwise.
        /// </summary>
        public bool IsEditingInProgress
        {
            get;
            protected set;
        }

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

        #region ISupportSelection Members

        /// <summary>
        /// Current selection.
        /// </summary>
        public IList SelectedItems
        {
            get { return SelectionManager.SelectedItems; }
        }

        /// <summary>
        /// Try to save changes in edited item and return true if item was saved successfully.
        /// </summary>
        /// <returns>Is saved successfully.</returns>
        public bool SaveEditedItem()
        {
            bool result = true;
            if (_ordersView.OrdersGrid.IsBeingEdited)
                result = _ordersView.SaveEditedItem();
            if (_routesView.RoutesGrid.IsBeingEdited)
                result = _routesView.SaveEditedItem();

            return result;
        }

        /// <summary>
        /// Method selects necessary items.
        /// </summary>
        /// <param name="items">Items that should be selected.</param>
        public void Select(IEnumerable items)
        {
            // check that editing is not in progress
            if (IsEditingInProgress)
            {
                CancelObjectEditing();
            }

            SelectionManager.Select(items); // exception
        }

        #endregion

        #region ISupportSelectionChanged Members

        /// <summary>
        /// Occurs when selection in page finish to change.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region Internal methods

        internal void CancelNewObject()
        {
            _ordersView.CancelNewObject(false);
            _routesView.CancelNewObject();
        }

        /// <summary>
        /// Set extent on imported orders and set orders context.
        /// </summary>
        /// <param name="orders">Orders to set extent on.</param>
        internal void FinishImportOrders(IList<Order> orders)
        {
            // Set extent on orders.
            MapView.mapCtrl.SetExtentOnCollection((IList)orders);
        }

        #endregion

        #region Protected Page Base Overrided Methods

        /// <summary>
        /// Overrided. Creates Calendar widget and Views widget.
        /// </summary>
        protected override void CreateWidgets()
        {
            base.CreateWidgets();
            var calendarWidget = new CalendarWidget();
            calendarWidget.Initialize(this);
            _viewsWidget.Initialize(this);

            this.EditableWidgetCollection.Insert(0, calendarWidget);
            this.EditableWidgetCollection.Insert(VIEWS_WIDGET_POSITION, _viewsWidget);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Inits command for "Cancel" button.
        /// </summary>
        private void _InitCancelRoutingButtonCommand()
        {
            var status = LayoutRoot.FindResource("statusStack") as StackPanel;
            var commandButton = status.Children[2] as CommandButton;
            var command = new CancelRoutingOperationCmd();
            command.Initialize(App.Current);
            commandButton.ApplicationCommand = command;
            commandButton.Content = command.Title;
            commandButton.Style = (Style)App.Current.FindResource(STATUS_BAR_BUTTON_STYLE);
        }

        /// <summary>
        /// Creates all common event handlers (loaded/unloaded, collection changed etc.).
        /// </summary>
        private void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(_OptimizeAndEditPageLoaded);
            this.Unloaded += new RoutedEventHandler(OptimizeAndEditPage_Unloaded);
            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);
            App.Current.Exit += new ExitEventHandler(Current_Exit);

            _mapView.VisibileStateChanged += _MapViewVisibileStateChanged;
        }


        /// <summary>
        /// Initializes schedule manager.
        /// </summary>
        private void _InitializeScheduleManager()
        {
            if (App.Current.Project != null)
            {
                _currentScheduleManager = App.Current.Project.Schedules;

                _SubscribeToActiveSchedulePropertyChanged();
            }
        }

        /// <summary>
        /// Subscribe to Active Schedule PropertyChanged events.
        /// </summary>
        private void _SubscribeToActiveSchedulePropertyChanged()
        {
            _activeScheduleDescriptor.AddValueChanged(_currentScheduleManager,
                _ActiveSchedulePropertyChanged);
        }

        /// <summary>
        /// Unsubscribe from Active Schedule PropertyChanged events.
        /// </summary>
        private void _UnsubscribeFromActiveSchedulePropertyChanged()
        {
            _activeScheduleDescriptor.RemoveValueChanged(_currentScheduleManager,
                _ActiveSchedulePropertyChanged);
        }

        /// <summary>
        /// React on visibility of view changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapViewVisibileStateChanged(object sender, EventArgs e)
        {
            if (this.EditingManager.EditedObject != null &&
                !this.GeocodablePage.IsGeocodingInProcess)
            {
                this.CancelObjectEditing();
            }
        }

        /// <summary>
        /// Fires selection changes finished.
        /// </summary>
        private void _SelectionManagerSelectionChanged(object sender, EventArgs e)
        {
            // we should change status only if page isn't locked and no items are in editing state
            if (!IsLocked && CurrentSchedule != null && !IsEditingInProgress)
                _ChangeStatus();

            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);

            _geocodablePage.OnSelectionChanged(SelectedItems);
        }

        /// <summary>
        /// Method creates event handlers for all editing events from ScheduleViewsEditingManager.
        /// </summary>
        private void _InitEditingEventHandlers()
        {
            // Create instance of ScheduleViewsEditingManager - class which manage editing events.
            _scheduleViewsEditingManager = new ScheduleViewsEditingManager(this);

            // Add handlers for editing events.
            _scheduleViewsEditingManager.BeginningEdit +=
                new DataObjectCanceledEventHandler(_ScheduleViewsEditingManagerBeginningEdit);
            _scheduleViewsEditingManager.EditBegun +=
                new DataObjectEventHandler(_ScheduleViewsEditingManagerEditBegun);
            _scheduleViewsEditingManager.CommittingEdit +=
                new DataObjectCanceledEventHandler(_ScheduleViewsEditingManagerCommittingEdit);
            _scheduleViewsEditingManager.EditCommitted +=
                new DataObjectEventHandler(_ScheduleViewsEditingManagerEditCommitted);
            _scheduleViewsEditingManager.EditCanceled +=
                new DataObjectEventHandler(_ScheduleViewsEditingManagerEditCanceled);

            // Add handlers for creating new events.
            _scheduleViewsEditingManager.CreatingNewObject +=
                new DataObjectCanceledEventHandler(_ScheduleViewsEditingManagerCreatingNewObject);
            _scheduleViewsEditingManager.NewObjectCreated +=
                new DataObjectEventHandler(_ScheduleViewsEditingManagerNewObjectCreated);
            _scheduleViewsEditingManager.CommittingNewObject +=
                new DataObjectCanceledEventHandler(_ScheduleViewsEditingManagerCommittingNewObject);
            _scheduleViewsEditingManager.NewObjectCommitted +=
                new DataObjectEventHandler(_ScheduleViewsEditingManagerNewObjectCommitted);
            _scheduleViewsEditingManager.NewObjectCanceled +=
                new DataObjectEventHandler(_ScheduleViewsEditingManagerNewObjectCanceled);
        }

        /// <summary>
        /// Checks page locked/unlocked status.
        /// </summary>
        private void _CheckPageLocked()
        {
            // Page is locked when even one routing operation is running.
            IsLocked = (App.Current.Solver.GetAsyncOperations(App.Current.CurrentDate).Count > 0);
        }

        /// <summary>
        /// Checks page complete status.
        /// </summary>
        private void _CheckPageComplete()
        {
            // Page is complete when even one route is built.
            IsComplete = (CurrentSchedule != null &&
                          ScheduleHelper.DoesScheduleHaveBuiltRoutes(CurrentSchedule));
        }

        /// <summary>
        /// Raises event about schedule changed.
        /// </summary>
        private void _NotifyCurrentScheduleChanged()
        {
            if (CurrentScheduleChanged != null)
                CurrentScheduleChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Method inits geocodable page.
        /// </summary>
        private void _InitGeocodablePage()
        {
            Debug.Assert(_geocodablePage == null);

            _geocodablePage = new GeocodablePage(typeof(Order),
                                                 MapView.mapCtrl,
                                                 MapView.candidateSelect,
                                                 MapView.LayoutRoot,
                                                 _ordersView.OrdersGrid,
                                                 MapView.splitter,
                                                 MapView.UnassignedLayer);
            MapView.mapCtrl.AddTool(new EditingTool(), null);

            _geocodablePage.GeocodingStarted += new EventHandler(_GeocodablePageGeocodingStarted);
        }

        /// <summary>
        /// Method returns orders insertion row.
        /// </summary>
        /// <returns></returns>
        private InsertionRow _GetOrdersInsertionRow()
        {
            return OrdersView.InsertionRow;
        }

        /// <summary>
        /// Changes Map view visibility.
        /// </summary>
        /// <param name="sender">Geocodable page sender.</param>
        /// <param name="e">Event args.</param>
        private void _GeocodablePageGeocodingStarted(object sender, EventArgs e)
        {
            if (!MapView.IsVisible)
                MapView.Show();
        }

        /// <summary>
        /// Creates views and loads saved layout.
        /// </summary>
        private void _CreateViews()
        {
            DockManager1.ParentWindow = Application.Current.MainWindow;

            _timeView.ParentPage = this;
            _scheduleVersionsView.ParentPage = this;
            _findOrdersView.ParentPage = this;
            _ordersView.ParentPage = this;
            _routesView.ParentPage = this;

            // Load saved layout.
            _InitLayout();

            _mapView.ParentPage = this;
        }

        /// <summary>
        /// Validate docking layout settings.
        /// </summary>
        /// <returns>Current layout settings.</returns>
        private string _ValidateLayoutSetting()
        {
            string layoutSettings = null;

            Settings settings = Settings.Default;
            if (!_IsStoredLayoutEmpty() &&
                // If settings is old version - ignore settings.
                !settings.DockingLayoutStateShedulePage.Contains(LIST_VIEW_TYPE))
                layoutSettings = settings.DockingLayoutStateShedulePage;

            return layoutSettings;
        }

        /// <summary>
        /// Loads saved layout from settings.
        /// </summary>
        private void _LoadLayoutFromSettings()
        {
            bool isInited = false;

            string settings = _ValidateLayoutSetting();
            if (!string.IsNullOrEmpty(settings))
            {   // if settings is exist - load they.
                if (!settings.Contains(_mapView.GetType().ToString()))
                {   // NOTE: need init map control.
                    _mapView.Show();
                    _mapView.Close();
                }

                try
                {
                    DockManager1.RestoreLayoutFromXml(settings,
                        new GetContentFromTypeString(this._GetContentByTypeName));
                    isInited = true;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            if (!isInited)
            {
                _LoadDefaultLayout();
            }
        }

        /// <summary>
        /// Updates views widget state
        /// </summary>
        private void _UpdateViewsWidget()
        {
            var views = new List<DockableContent>();

            // Adds other views into views collection.
            views.Add(_ordersView);
            views.Add(_routesView);
            views.Add(_mapView);
            views.Add(_timeView);
            views.Add(_scheduleVersionsView);
            views.Add(_findOrdersView);

            // Set views of views widget.
            _viewsWidget.Views = views;
        }

        /// <summary>
        /// Loads saved layout or shows all wiews that must be shown by default.
        /// </summary>
        private void _InitLayout()
        {
            _mapView.DockManager = DockManager1;
            _timeView.DockManager = DockManager1;
            _scheduleVersionsView.DockManager = DockManager1;
            _findOrdersView.DockManager = DockManager1;
            _ordersView.DockManager = DockManager1;
            _routesView.DockManager = DockManager1;

            _LoadLayoutFromSettings();

            _UpdateViewsWidget();
        }

        /// <summary>
        /// Returns content by type name.
        /// </summary>
        /// <param name="type">Type name string.</param>
        /// <returns>View with necessary type.</returns>
        private DockableContent _GetContentByTypeName(string type)
        {
            Debug.Assert(!string.IsNullOrEmpty(type));

            DockableContent content = null;
            if (type == typeof(MapView).ToString())
                content = _mapView;
            else if (type == typeof(TimeView).ToString())
                content = _timeView;
            else if (type == typeof(ScheduleVersionsView).ToString())
                content = _scheduleVersionsView;
            else if (type == typeof(FindOrdersView).ToString())
                content = _findOrdersView;
            else if (type == typeof(OrdersView).ToString())
                content = _ordersView;
            else if (type == typeof(RoutesView).ToString())
                content = _routesView;
            else
            {
                Debug.Assert(false);
            }

            return content;
        }

        /// <summary>
        /// Loads new schedule and makes it Active.
        /// </summary>
        /// <param name="schedule">Schedule to load.</param>
        private void _SetActiveSchedule(Schedule schedule)
        {
            _CacheSchedule(schedule);

            _LoadSchedule(false);
        }

        /// <summary>
        /// Loads schedule from hashtable or create one if no schedule for current date.
        /// </summary>
        /// <param name="afterBuildRoutes">Bool value to define whether default routes
        /// should be loaded for schedule.</param>
        private void _LoadSchedule(bool afterBuildRoutes)
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, string.Empty);

            // Set status "Loading schedule" to status bar.
            var statusMessage = App.Current.FindString("LoadingScheduleStatus");
            using (WorkingStatusHelper.EnterBusyState(statusMessage))
            {
                _ReloadActiveSchedule();

                _CheckPageComplete();

                // Map view have to be notified about schedule changed before context manager.
                _mapView.OnScheduleLoad(afterBuildRoutes);

                // Load selection, if solve was on current date.
                var plannedDate = CurrentSchedule.PlannedDate;

                if (plannedDate.Equals(App.Current.CurrentDate))
                {
                    List<object> selection =
                        _dateSelectionKeeper.RestoreSelection(plannedDate.Value);

                    if (afterBuildRoutes)
                        SelectedItems.Clear();
                    else if (selection != null && selection.Count > 0)
                        Select(selection); // Exception.
                }

                // Raise event about schedule changed.
                _NotifyCurrentScheduleChanged();
            }
        }

        /// <summary>
        /// Loads active schedule for current schedule manager from cache or from project.
        /// </summary>
        private void _ReloadActiveSchedule()
        {
            Debug.Assert(_currentScheduleManager != null);

            // Unsubscribe from events to avoid duplicated handlers calls.
            _UnsubscribeFromActiveSchedulePropertyChanged();

            if (_activeSchedule != null)
                _activeSchedule.Routes.CollectionChanged -= _RoutesCollectionChanged;

            Schedule currentSchedule = _currentScheduleManager.ActiveSchedule;

            // Try to get schedule from cache, otherwise load it from the project.
            if (_schedules.TryGetValue(App.Current.CurrentDate, out currentSchedule))
            {
                OptimizeAndEditHelpers.FixSchedule(App.Current.Project, currentSchedule);
            }
            else
            {
                currentSchedule = OptimizeAndEditHelpers.LoadSchedule(
                    App.Current.Project,
                    App.Current.CurrentDate,
                    OptimizeAndEditHelpers.FindScheduleToSelect);
                _CacheSchedule(currentSchedule);
            }

            // Apply changes to current schedule manager.
            _currentScheduleManager.ActiveSchedule = currentSchedule;
            _activeSchedule = currentSchedule;

            // Add event handlers for new schedule.
            _SubscribeToActiveSchedulePropertyChanged();
            _activeSchedule.Routes.CollectionChanged += _RoutesCollectionChanged;
        }

        /// <summary>
        /// Raises event about page was locked or unlocked.
        /// </summary>
        private void NotifyLockedPropertyChanged()
        {
            if (LockedPropertyChanged != null)
                LockedPropertyChanged(this, new EventArgs());
        }

        /// <summary>
        /// Method sets action type into status if page is locked (action is running),
        /// or sets selection status if page is unlocked.
        /// </summary>
        private void _SetLockedStatus()
        {
            var status = LayoutRoot.FindResource("statusStack") as StackPanel;
            var statusLabel = status.Children[1] as Label;
            var commandButton = status.Children[2] as CommandButton;

            // If page is locked - set action status.
            if (IsLocked)
            {
                // If no operation is running at current date - return.
                if (!_lockedStatuses.ContainsKey(App.Current.CurrentDate) ||
                    string.IsNullOrEmpty(_lockedStatuses[App.Current.CurrentDate]))
                    return;

                statusLabel.Content = _lockedStatuses[App.Current.CurrentDate];

                if ((string)statusLabel.Content == App.Current.FindString("CancellingOperation"))
                    commandButton.Visibility = Visibility.Hidden; // If status contains text "Cancelling..." - hide "Cancel" button.
                else
                    commandButton.Visibility = Visibility.Visible; // Otherwise - show "Cancel" button.

                // Set status text to status bar.
                App.Current.MainWindow.StatusBar.SetStatus(this, status);
            }
            else // If page is unlocked - set selection status.
            {
                App.Current.MainWindow.StatusBar.SetStatus(this, "");
                _ChangeStatus();
                commandButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Method changes status in status bar according state of last focused List view.
        /// </summary>
        private void _ChangeStatus()
        {
            // Create new status builder.
            StatusBuilder statusBuilder = new StatusBuilder();

            // Init counters.
            int ordersCount = 0;
            int routesCount = 0;

            // 
            if (SelectedItems.Count == 0)
            {
                string status = App.Current.GetString(NO_SELECTION_GRID_STATUS_FORMAT,
                                                      CurrentSchedule.Routes.Count,
                                                      CurrentSchedule.UnassignedOrders.Count);
                App.Current.MainWindow.StatusBar.SetStatus(this, status);
                return;
            }

            // Get counts of selected items.
            foreach (Object obj in SelectedItems)
            {
                if (obj is Order || (obj is Stop && ((Stop)obj).StopType == StopType.Order))
                    ordersCount++; // If selection contains stops and orders.
                else if (obj is Route)
                    routesCount++;
            }

            if (ordersCount > 0)
            {
                // Fill selection status for orders selection.
                statusBuilder.FillSelectionStatusWithoutCollectionSize(CurrentSchedule.UnassignedOrders.Count,
                                                                       App.Current.FindString("Order"),
                                                                       ordersCount, this);
            }
            else if (routesCount > 0)
            {
                // Fill selection status for routes selection.
                statusBuilder.FillSelectionStatus(CurrentSchedule.Routes.Count,
                                                  App.Current.FindString("Route"),
                                                  routesCount, this);
            }
        }

        #endregion

        #region Private Editing Event Handlers

        /// <summary>
        /// Handler raises event about new object was cancelled.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerNewObjectCanceled(object sender, DataObjectEventArgs e)
        {
            IsEditingInProgress = false;

            // Raise event about new object was cancelled.
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, e);
        }

        /// <summary>
        /// Handler raises event about new object was commited.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerNewObjectCommitted(object sender, DataObjectEventArgs e)
        {
            // Raise event about new object was commited.
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, e);
        }

        /// <summary>
        /// Handler raises event about new object is commiting.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerCommittingNewObject(object sender, DataObjectCanceledEventArgs e)
        {
            // Raise event about new object is commiting.
            if (CommittingNewObject != null)
                CommittingNewObject(this, e);

            IsEditingInProgress = false;
        }

        /// <summary>
        /// Handler raises event about new object was created.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerNewObjectCreated(object sender, DataObjectEventArgs e)
        {
            // Raise event about new object was created.
            if (NewObjectCreated != null)
                NewObjectCreated(this, e);
        }

        /// <summary>
        /// Handler raises event about new object is creating.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerCreatingNewObject(object sender, DataObjectCanceledEventArgs e)
        {
            // Raise event about creating new is starting.
            if (CreatingNewObject != null)
                CreatingNewObject(this, e);

            IsEditingInProgress = true;
        }

        /// <summary>
        /// Handler raises event about edit was cancelled.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerEditCanceled(object sender, DataObjectEventArgs e)
        {
            IsEditingInProgress = false;

            // Raise event about edit was cancelled.
            if (EditCanceled != null)
                EditCanceled(this, new DataObjectEventArgs(e.Object));
        }

        /// <summary>
        /// Hndler raises event about editing was commited.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerEditCommitted(object sender, DataObjectEventArgs e)
        {
            // Raise event about editing was commited.
            if (EditCommitted != null)
                EditCommitted(this, e);
        }

        /// <summary>
        /// Handler raises event about edit is commiting.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args e.</param>
        private void _ScheduleViewsEditingManagerCommittingEdit(object sender, DataObjectCanceledEventArgs e)
        {
            // Raise event about commiting is starting.
            if (CommittingEdit != null)
                CommittingEdit(this, e);

            IsEditingInProgress = false;
        }

        /// <summary>
        /// Handler raises event about edit begun.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerEditBegun(object sender, DataObjectEventArgs e)
        {
            // Raise event about editing was started.
            if (EditBegun != null)
                EditBegun(this, e);
        }

        /// <summary>
        /// Handler raises event about editing is beginning.
        /// </summary>
        /// <param name="sender">View sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScheduleViewsEditingManagerBeginningEdit(object sender, DataObjectCanceledEventArgs e)
        {
            // Raise event about editing is starting.
            if (BeginningEdit != null)
                BeginningEdit(this, e);

            // Stop editing canceled inside StopListViewContextHandler, we dont need to set IsEditingInProgress to true
            if (!(e.Object is Stop))
                IsEditingInProgress = true;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Check page's complete status.
        /// </summary>
        /// <param name="sender">Collection of routes.</param>
        /// <param name="e">Event args.</param>
        private void _RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _CheckPageComplete();
        }

        /// <summary>
        /// Handler contains logic to update page's state when solve operation will be completed.
        /// </summary>
        /// <param name="sender">Solver.</param>
        /// <param name="e">Solve event args.</param>
        private void OptimizeAndEditPage_AsyncSolveCompleted(object sender,
                                                             AsyncSolveCompletedEventArgs e)
        {
            AsyncOperationInfo info = null;
            Schedule schedule = null;
            if (App.Current.Solver.GetAsyncOperationInfo(e.OperationId, out info))
                schedule = info.Schedule;

            // If operation was completed on other date - remove locked status for this date.
            if (schedule != null && schedule.PlannedDate.Equals(App.Current.CurrentDate))
            {
                if (_lockedStatuses.ContainsKey((DateTime)schedule.PlannedDate))
                    _lockedStatuses.Remove((DateTime)schedule.PlannedDate);
            }

            // If operation was completed on current date - unlock UI.
            if (schedule.PlannedDate.Equals(App.Current.CurrentDate))
                IsLocked = false;

            // Update page's "IsComplete" property if necessary.
            _CheckPageComplete();

            WorkingStatusHelper.SetReleased();
        }

        private void OptimizeAndEditPage_AsyncSolveStarted(object sender, AsyncSolveStartedEventArgs e)
        {
            AsyncOperationInfo info = null;
            Schedule schedule = null;
            if (App.Current.Solver.GetAsyncOperationInfo(e.OperationId, out info))
                schedule = info.Schedule;

            IsLocked = schedule.PlannedDate.Equals(App.Current.CurrentDate);

            // Save selection.
            _dateSelectionKeeper.StoreSelection(schedule.PlannedDate.Value, SelectedItems);

            // Finish geocoding after starting of solve operation.
            if (_geocodablePage.IsGeocodingInProcess)
            {
                _geocodablePage.EndGeocoding();
            }
        }

        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _schedules.Clear();
            _InitializeScheduleManager();
            _CheckPageComplete();
            _CheckPageLocked();
        }

        private void OptimizeAndEditPage_CurrentDateChanged(object sender, EventArgs e)
        {
            // Parameter is "true" because when current date was changed we need to update
            // collection of Routes if necessary.
            _LoadSchedule(false);
            _CheckPageComplete();
            _CheckPageLocked();

            _UpdateRouteShapeIfNecessary();
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            if (_isContentCreated && !_IsStoredLayoutEmpty())
            {
                SaveLayout();
            }
            DockManager1.Release();
        }

        /// <summary>
        /// Handler updates page's state (locked and complete properties),
        /// loads schedule and create views if necessary.
        /// </summary>
        /// <param name="sender">This page sender.</param>
        /// <param name="e">Event args.</param>
        private void _OptimizeAndEditPageLoaded(object sender, RoutedEventArgs e)
        {
            // Add handler to "CurrentDateChanged"
            App.Current.CurrentDateChanged +=
                new EventHandler(OptimizeAndEditPage_CurrentDateChanged);

            bool needToSetSpecialContext = (CurrentSchedule == null);

            _CheckPageLocked();

            // Load schedule when page's rendered.
            // Parameter can be "true" or "false":
            // when project was reloaded (page loads at first time with this project) - we need
            // to init routes collection, otherwise collection of routes shouldn't be updated.
            _LoadSchedule(false);

            // If views were not created before (page loads at first time) - create they there.
            if (!_isContentCreated)
            {
                _CreateViews();
                _isContentCreated = true;
            }
            else if (_IsStoredLayoutEmpty())
            {
                _LoadDefaultLayout();
                _viewsWidget.UpdateState();
            }
            // else - Do nothing.

            _CheckPageComplete();

            if (IsLocked)
            {
                var stackPanel = LayoutRoot.FindResource("statusStack") as StackPanel;
                App.Current.MainWindow.StatusBar.SetStatus(this, stackPanel);
            }

            App.Current.MainWindow.NavigationCalled +=
                new EventHandler(OptimizeAndEditPage_NavigationCalled);

            _UpdateRouteShapeIfNecessary();

            _SetLockedStatus();
        }

        private void OptimizeAndEditPage_NavigationCalled(object sender, EventArgs e)
        {
            CanBeLeft = SaveEditedItem();
        }

        private void OptimizeAndEditPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.CurrentDateChanged -= OptimizeAndEditPage_CurrentDateChanged;
            App.Current.MainWindow.NavigationCalled -= OptimizeAndEditPage_NavigationCalled;

            // Cancel editing.
            _scheduleViewsEditingManager.CancelObjectEditing();
        }

        /// <summary>
        /// Generate shapes for route of this schedule in case of
        /// "Follow streets" option is turned On, but shapes not exists.
        /// </summary>
        private void _UpdateRouteShapeIfNecessary()
        {
            App app = App.Current;
            if (app.MainWindow.CurrentPage.Equals(this) &&
                app.MapDisplay.TrueRoute &&
                app.Solver.GetAsyncOperations(app.CurrentDate).Count == 0)
            {
                List<Route> routesWithoutGeometry = _GetRoutesWithoutGeometry(CurrentSchedule);

                if (routesWithoutGeometry.Count > 0)
                {
                    object[] args = new object[1] { routesWithoutGeometry };

                    var routeShapeGenerationCmd = new RouteShapeGenerationCmd();
                    routeShapeGenerationCmd.Execute(args);
                }
            }
        }

        /// <summary>
        /// Get routes without geometry for schedule.
        /// </summary>
        /// <param name="schedule">Schedule to get routes.</param>
        /// <returns>Routes without geometry.</returns>
        private List<Route> _GetRoutesWithoutGeometry(Schedule schedule)
        {
            var routesWithoutGeometry = new List<Route>();

            // Go through routes in schedule and and find routes with stops, which havent directions.
            foreach (Route route in schedule.Routes)
            {
                IDataObjectCollection<Stop> stops = route.Stops;
                if (stops == null || stops.Count == 0)
                    continue;

                bool needDirectionsGenerate = true;

                // Check for stop without directions.
                foreach (Stop stop in stops)
                {
                    if (StopType.Location == stop.StopType || StopType.Order == stop.StopType)
                    {
                        if (stop.Directions != null && stop.Directions.Length > 0)
                        {
                            needDirectionsGenerate = false;
                            break; // Result founded.
                        }
                    }
                }

                // Add route to list, if directions for route is absent.
                if (needDirectionsGenerate)
                {
                    routesWithoutGeometry.Add(route);
                }
            }

            return routesWithoutGeometry;
        }

        /// <summary>
        /// Loads default docking layout.
        /// </summary>
        private void _LoadDefaultLayout()
        {
            // Use default layout (from local const) to init.
            var contentHandler = new GetContentFromTypeString(this._GetContentByTypeName);
            DockManager1.RestoreLayoutFromXml(DEFAULT_DOCKING_LAYOUT, contentHandler);

            // Store changes.
            SaveLayout();
        }

        /// <summary>
        /// Checks is docking layout stored setting is empty.
        /// </summary>
        /// <returns>TRUE if settings not have data.</returns>
        private bool _IsStoredLayoutEmpty()
        {
            return string.IsNullOrEmpty(Settings.Default.DockingLayoutStateShedulePage);
        }

        /// <summary>
        /// Adds the specified schedule to a cache of loaded schedules.
        /// </summary>
        /// <param name="schedule">The reference to the schedule object to be cached.</param>
        /// <remarks>The cache uses <see cref="Schdule.PlannedDate"/> property as a key, so for
        /// any give date there could only one schedule in the cache.</remarks>
        /// <exception cref="System.InvalidOperationException">when
        /// <see cref="Schdule.PlannedDate"/> property of the <paramref name="schedule"/> is
        /// null.</exception>
        private void _CacheSchedule(Schedule schedule)
        {
            Debug.Assert(schedule != null);

            _schedules[schedule.PlannedDate.Value] = schedule;
        }

        /// <summary>
        /// Loads active schedule when it was changed in Schedule Manager of current Project.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ActiveSchedulePropertyChanged(object sender, EventArgs e)
        {
            _SetActiveSchedule(App.Current.Project.Schedules.ActiveSchedule);
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Format string for status with empty selection.
        /// </summary>
        private const string NO_SELECTION_GRID_STATUS_FORMAT = "NoSelectedGridStatusFormat";

        /// <summary>
        /// Status bar button style.
        /// </summary>
        private const string STATUS_BAR_BUTTON_STYLE = "StatusBarButtonStyle";
        
        /// <summary>
        /// Position index of views widget.
        /// </summary>
        private const int VIEWS_WIDGET_POSITION = 2;

        /// <summary>
        /// List view full name type.
        /// </summary>
        private const string LIST_VIEW_TYPE = "ESRI.ArcLogistics.App.Pages.ListView";

        /// <summary>
        /// Docking layout default state.
        /// </summary>
        private const string DEFAULT_DOCKING_LAYOUT =
            "<DockingLibrary_Layout><_root><Child Type=\"Vertical\" SpaceFactor=\"0.5\">" +
            "<ChildGroups><Child Type=\"Horizontal\" SpaceFactor=\"0.480550670677205\">" +
            "<ChildGroups><Child Type=\"Terminal\" SpaceFactor=\"0.379907621247113\">" +
            "<DockablePane Size=\"493.044988114812,329\" Dock=\"Left\" State=\"Docked\" " +
            "ptFloatingWindow=\"192,496\" sizeFloatingWindow=\"300,300\">" +
            "<ESRI.ArcLogistics.App.Pages.OrdersView /></DockablePane></Child>" +
            "<Child Type=\"Terminal\" SpaceFactor=\"0.620092378752887\">" +
            "<DockablePane Size=\"493.044988114812,537\" Dock=\"Bottom\" State=\"Docked\" " +
            "ptFloatingWindow=\"448,513\" sizeFloatingWindow=\"300,300\">" +
            "<ESRI.ArcLogistics.App.Pages.MapView /></DockablePane></Child></ChildGroups></Child>" +
            "<Child Type=\"Terminal\" SpaceFactor=\"0.519449329322795\">" +
            "<DockablePane Size=\"532.955011885188,870\" Dock=\"Left\" State=\"Docked\" " +
            "ptFloatingWindow=\"0,0\" sizeFloatingWindow=\"300,300\">" +
            "<ESRI.ArcLogistics.App.Pages.RoutesView /></DockablePane></Child></ChildGroups>" +
            "</Child></_root><FloatingWindows /></DockingLibrary_Layout>";

        #endregion

        #region Private static fields

        /// <summary>
        /// Active schedule property descriptor.
        /// </summary>
        private static readonly PropertyDescriptor _activeScheduleDescriptor =
            TypeInfoProvider<ScheduleManager>.GetPropertyDescriptor(_ => _.ActiveSchedule);

        #endregion

        #region Private Fields

        /// <summary>
        /// Widet to manage views visibility.
        /// </summary>
        private ViewsWidget _viewsWidget = new ViewsWidget();

        /// <summary>
        /// Map view.
        /// </summary>
        private readonly MapView _mapView = new MapView();

        /// <summary>
        /// Time view.
        /// </summary>
        private readonly TimeView _timeView = new TimeView();

        /// <summary>
        /// Orders view.
        /// </summary>
        private readonly OrdersView _ordersView = new OrdersView();
        
        /// <summary>
        /// Routes view.
        /// </summary>
        private readonly RoutesView _routesView = new RoutesView();

        /// <summary>
        /// Schedule versions view.
        /// </summary>
        private readonly ScheduleVersionsView _scheduleVersionsView = new ScheduleVersionsView();

        /// <summary>
        /// Find orders view.
        /// </summary>
        private readonly FindOrdersView _findOrdersView = new FindOrdersView();

        /// <summary>
        /// Bool flag to define whether page's content was created.
        /// </summary>
        private bool _isContentCreated;

        /// <summary>
        /// Bool flag to define whether page was locked.
        /// </summary>
        private bool _isLocked;

        /// <summary>
        /// Collection of pairs : schedule + planned date.
        /// </summary>
        private IDictionary<DateTime, Schedule> _schedules = new Dictionary<DateTime, Schedule>();

        /// <summary>
        /// Collection of pairs : status + date where this status is actual.
        /// </summary>
        private Dictionary<DateTime, string> _lockedStatuses = new Dictionary<DateTime, string>();

        /// <summary>
        /// Geocodable page. Used for support synchronization with map control.
        /// </summary>
        private GeocodablePage _geocodablePage;

        /// <summary>
        /// Class instance for saving and restoring selection by days.
        /// </summary>
        private DateSelectionKeeper _dateSelectionKeeper;

        /// <summary>
        /// Selection manager.
        /// </summary>
        private SelectionManager _selectionManager;

        /// <summary>
        /// Editing manager.
        /// </summary>
        private ScheduleViewsEditingManager _scheduleViewsEditingManager;

        /// <summary>
        /// Reference to Schedule manager from current Project.
        /// </summary>
        private ScheduleManager _currentScheduleManager;

        /// <summary>
        /// Reference to Last used active schedule.
        /// </summary>
        private Schedule _activeSchedule;

        #endregion
    }
}
