using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;
using System.Windows.Controls;
using System.Collections;
using ESRI.ArcLogistics.App.Dialogs;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for DefaultRoutesPage.xaml
    /// </summary>
    internal partial class DefaultRoutesPage : PageBase, ISupportDataObjectEditing, ISupportSelection, ICancelDataObjectEditing, ISupportSelectionChanged
    {
        public const string PAGE_NAME = "DefaultRoutes";

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>DefaultRoutesPage</c> class.
        /// </summary>
        public DefaultRoutesPage()
        {
            InitializeComponent();
            _InitEventHandlers();
            _SetDefaults();

            _CheckPageComplete();
            _CheckPageAllowed();

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController(XceedGrid);
        }

        #endregion // Constructors

        #region ICancelDataObjectEditing members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cancels Object editing.
        /// </summary>
        public void CancelObjectEditing()
        {
            XceedGrid.CancelEdit();
        }

        #endregion

        #region ISupportDataObjectEditing members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets/sets bool value - TRUE if any view in OptimizeAndEdit page is in editind state
        /// and FALSE otherwise.
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

        #endregion // ISupportDataObjectEditing members

        #region ISupportSelection members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current selection.
        /// </summary>
        public IList SelectedItems
        {
            get
            {
                return XceedGrid.SelectedItems;
            }
        }

        /// <summary>
        /// Saves edited item.
        /// </summary>
        /// <returns>Always FALSE. Not implemented there.</returns>
        public bool SaveEditedItem()
        {
            return false;
        }

        /// <summary>
        /// Set selection.
        /// </summary>
        /// <param name="items">Items to select.</param>
        public void Select(IEnumerable items)
        {
            // check that editing is not in progress
            if (IsEditingInProgress)
                throw new NotSupportedException((string)App.Current.FindResource("EditingInProcessExceptionMessage"));

            // check that all items are locations
            foreach (object item in items)
            {
                if (!(item is Route))
                    throw new ArgumentException("RoutesTypeExceptionMessage");
            }

            // add items to selection
            SelectedItems.Clear();
            foreach (object item in items)
                SelectedItems.Add(item);
        }

        #endregion // ISupportSelection members

        #region ISupportSelectionChanged members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when selection changes.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion // ISupportSelectionChanged members

        #region Page overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns unique page name.
        /// </summary>
        public override string Name
        {
            get { return PAGE_NAME; }
        }

        /// <summary>
        /// Returns page title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource("DefaultRoutesPageCaption"); }
        }

        /// <summary>
        /// Returns page icon as a TileBrush (DrawingBrush or ImageBrush).
        /// </summary>
        public override TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("DefaultRoutesBrush");
                return brush;
            }
        }

        /// <summary>
        /// Saves page layout.
        /// </summary>
        internal override void SaveLayout()
        {
            if (null == Properties.Settings.Default.DefaultRoutesGridSettings)
                Properties.Settings.Default.DefaultRoutesGridSettings = new SettingsRepository();

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.DefaultRoutesGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.DefaultRoutesPagePath); }
        }

        /// <summary>
        /// Returns category name of commands that will be present in Tasks widget.
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        public override bool CanBeLeft
        {
            get
            {
                // If there are validation error in insertion row - we cannot leave page.
                if (XceedGrid.IsInsertionRowInvalid)
                    return false;
                // If there isnt - we must validate all grid source items.
                else
                    return base.CanBeLeft &&
                        CanBeLeftValidator<Route>.IsValid(App.Current.Project.DefaultRoutes);
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }
        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fills route's fields by default values if possible.
        /// </summary>
        /// <returns>Created route with default settings.</returns>
        private Route _CreateRouteWithDefaultValues()
        {
            Project appCurrentProject = App.Current.Project;
            Route route = appCurrentProject.CreateRoute();

            // If we have one location and it is valid - select this location as route's 
            // StartLocation and EndLocation.
            if (appCurrentProject.Locations.Count == 1 && appCurrentProject.Locations[0].IsValid)
            {
                route.StartLocation = appCurrentProject.Locations[0];
                route.EndLocation = appCurrentProject.Locations[0];
            }

            // If we have one vehicle and it is valid - select it as route's Vehicle.
            if (appCurrentProject.Vehicles.Count == 1 && appCurrentProject.Vehicles[0].IsValid)
                route.Vehicle = appCurrentProject.Vehicles[0];

            // If we have one driver and it is valid - select it as route's Driver.
            if (appCurrentProject.Drivers.Count == 1 && appCurrentProject.Drivers[0].IsValid)
                route.Driver = appCurrentProject.Drivers[0];

            route.Color = RouteColorManager.Instance.NextRouteColor(appCurrentProject.DefaultRoutes);

            return route;
        }

        /// <summary>
        /// Checks page complete status.
        /// </summary>
        private void _CheckPageComplete()
        {
            Project project = App.Current.Project;
            IsComplete = (project != null && project.DefaultRoutes.Count > 0);
        }

        /// <summary>
        /// Checks is page allowed or not.
        /// </summary>
        private void _CheckPageAllowed()
        {
            Project project = App.Current.Project;
            IsAllowed = (project != null &&
                         project.Drivers.Count > 0 &&
                         project.Vehicles.Count > 0 &&
                         project.Locations.Count > 0 &&
                         project.BreaksSettings.BreaksType != null);
        }

        /// <summary>
        /// Inits collection of drivers.
        /// </summary>
        private void _InitDataGridCollection()
        {
            Project project = App.Current.Project;
            if (project == null)
                _isGridCollectionLoaded = false;
            else
            {
                DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

                IDataObjectCollection<Route> collection = (IDataObjectCollection<Route>)project.DefaultRoutes;
                SortedDataObjectCollection<Route> sortedRoutesCollection = new SortedDataObjectCollection<Route>(collection, new CreationTimeComparer<Route>());

                collectionSource.Source = sortedRoutesCollection;

                ((INotifyCollectionChanged)XceedGrid.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(DefaultRoutesPage_CollectionChanged);

                _isGridCollectionLoaded = true;
            }
        }

        /// <summary>
        /// Loads grid layout.
        /// </summary>
        private void _InitDataGridLayout()
        {
            DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.DefaultRoutesGridStructure);

            structureInitializer.BuildGridStructure(collectionSource, XceedGrid);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.DefaultRoutesSettingsRepositoryName, collectionSource.ItemProperties);
            layoutLoader.LoadLayout(XceedGrid);

            _isGridLayoutLoaded = true;
        }

        /// <summary>
        /// Fills page's properties by default values.
        /// </summary>
        protected void _SetDefaults()
        {
            IsRequired = true;
            CanBeLeft = true;
            DoesSupportCompleteStatus = true;
            commandButtonGroup.Initialize(CategoryNames.DefaultRoutesCommands, XceedGrid);
        }

        /// <summary>
        /// Method init event handlers
        /// </summary>
        protected void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(DefaultRoutesPage_Loaded);
            this.Unloaded += new RoutedEventHandler(DefaultRoutesPage_Unloaded);

            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(App_ProjectClosing);
            App.Current.Exit += new ExitEventHandler(Current_Exit);

            // When application is closing we need to check for updates.
            App.Current.MainWindow.Closing += (sender, args) =>
            {   if (_defaultRoutesController != null)
                    _defaultRoutesController.CheckDefaultRoutesForUpdates(); };

            Project project = App.Current.Project;
            if (null != project)
            {
                project.DefaultRoutes.CollectionChanged += 
					new System.Collections.Specialized.NotifyCollectionChangedEventHandler(DefaultRoutes_CollectionChanged);
                project.Locations.CollectionChanged += 
					new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_LocationsCollectionChanged);
                project.Vehicles.CollectionChanged += 
					new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_VehiclesCollectionChanged);
                project.Drivers.CollectionChanged += 
					new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_DriversCollectionChanged);
				project.BreaksSettings.PropertyChanged +=
                    new System.ComponentModel.PropertyChangedEventHandler(_BreaksSettingsPropertyChanged);
                _projectEventsAttached = true;
            }

            XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(XceedGrid_SelectionChanged);
        }

        #endregion // Private methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Drivers collection changed handler - Ckecks page allowed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DriversCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _CheckPageAllowed();
        }        

        /// <summary>
        /// Breaks type changed - check page allowed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreaksSettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _CheckPageAllowed();
        }

        /// <summary>
        /// Vehicles collection changed handler - Ckecks page allowed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _VehiclesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _CheckPageAllowed();
        }

        /// <summary>
        /// Locations collection changed handler - Ckecks page allowed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _LocationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _CheckPageAllowed();
        }

        /// <summary>
        /// Occurs when application close. Stores layout.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Current_Exit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        /// <summary>
        /// Occurs when project loads. Inits page and subscribes messages.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void App_ProjectLoaded(object sender, EventArgs e)
        {
            if (!_projectEventsAttached)
            {
                Project project = App.Current.Project;
                project.DefaultRoutes.CollectionChanged += new System.Collections.Specialized.
                    NotifyCollectionChangedEventHandler(DefaultRoutes_CollectionChanged);
                project.Locations.CollectionChanged += new System.Collections.Specialized.
                    NotifyCollectionChangedEventHandler(_LocationsCollectionChanged);
                project.Vehicles.CollectionChanged += new System.Collections.Specialized.
                    NotifyCollectionChangedEventHandler(_VehiclesCollectionChanged);
                project.Drivers.CollectionChanged += new System.Collections.Specialized.
                    NotifyCollectionChangedEventHandler(_DriversCollectionChanged);
                project.BreaksSettings.PropertyChanged += new System.ComponentModel.
                    PropertyChangedEventHandler(_BreaksSettingsPropertyChanged);
                _projectEventsAttached = true;
            }

            _InitDataGridLayout();
            _InitDataGridCollection();
            _CheckPageAllowed();
            _CheckPageComplete();
        }

        /// <summary>
        /// Occurs when project closings. Stores layout and unsubscribes messages/
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void App_ProjectClosing(object sender, EventArgs e)
        {
            if (_projectEventsAttached)
            {
                Project project = App.Current.Project;
                project.DefaultRoutes.CollectionChanged -= DefaultRoutes_CollectionChanged;
                project.Locations.CollectionChanged -= _LocationsCollectionChanged;
                project.Vehicles.CollectionChanged -= _VehiclesCollectionChanged;
                project.Drivers.CollectionChanged -= _DriversCollectionChanged; 
                project.BreaksSettings.PropertyChanged -= _BreaksSettingsPropertyChanged;
                _projectEventsAttached = false;
            }

            SaveLayout();
        }

        /// <summary>
        /// Occurs when page loads. Inits page if need. Updates selection status.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DefaultRoutesPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Init routes controller.
            _defaultRoutesController = new DefaultRoutesController(App.Current.Project.DefaultRoutes);

            App.Current.MainWindow.NavigationCalled += new EventHandler(DefaultRoutesPage_NavigationCalled);

            if (!_isGridLayoutLoaded)
                _InitDataGridLayout();
            if (!_isGridCollectionLoaded)
                _InitDataGridCollection();

            _needToUpdateStatus = true;
            _SetSelectionStatus();
        }

        /// <summary>
        /// Occurs when default routes collection changed. Checks page complete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DefaultRoutes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _CheckPageComplete();
        }

        /// <summary>
        /// Occurs when navigation called - stops xceed edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DefaultRoutesPage_NavigationCalled(object sender, EventArgs e)
        {
            try
            {
                XceedGrid.EndEdit();
                CanBeLeft = true;
            }
            catch
            {
                CanBeLeft = false;
            }

            // If we will navigate to other page - check routes for updates.
            if (CanBeLeft)
                _defaultRoutesController.CheckDefaultRoutesForUpdates();
            // Else - show validation errors.
            else
                CanBeLeftValidator<Route>.ShowErrorMessagesInMessageWindow
                    (App.Current.Project.DefaultRoutes);
        }

        /// <summary>
        /// Occurs when page unloaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void DefaultRoutesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).NavigationCalled -= DefaultRoutesPage_NavigationCalled;

            this.CancelObjectEditing();
        }

        /// <summary>
        /// Occurs when insertion row initialized.
        /// </summary>
        /// <param name="sender">Insertion row.</param>
        /// <param name="e">Ignored.</param>
        private void InsertionRow_Initialized(object sender, EventArgs e)
        {
            _InsertionRow = sender as InsertionRow;
            if (_InsertionRow.Cells["HardZones"] != null)
                _InsertionRow.Cells["HardZones"].Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Occurs when import button click. Does import.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            ImportDefaultRoutesCmd cmd = new ImportDefaultRoutesCmd();
            cmd.Execute();
        }

        private void XceedGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            _SetSelectionStatus();

            // NOTE : event raises to notify all necessary object about selection was changed. Added because XceedGrid.SelectedItems doesn't implement INotifyCollectionChanged
            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);
        }

        private void DefaultRoutesPage_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus();
        }

        #endregion // Event handlers

        #region Data Object Editing Event Handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void DataGridCollectionViewSource_BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (BeginningEdit != null)
                BeginningEdit(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                IsEditingInProgress = true;
                _SetEditingStatus(e.Item.ToString());
            }
            else
                e.Cancel = true;
        }

        private void DataGridCollectionViewSource_EditBegun(object sender, DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditBegun != null)
                EditBegun(this, args);
        }

        private void DataGridCollectionViewSource_CommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            if (CommonHelpers.IgnoreVirtualLocations(e.Item))
            {
                var args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
                if (CommittingEdit != null)
                    CommittingEdit(this, args);

                e.Handled = true;

                if (args.Cancel)
                    e.Cancel = true;
                else
                {
                    var project = App.Current.Project;
                    if (project != null)
                        project.Save();

                    _SetSelectionStatus();
                    IsEditingInProgress = false;
                }
            }
            else
            {
                e.Handled = true;
                e.Cancel = true;
            }
        }

        private void DataGridCollectionViewSource_EditCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditCommitted != null)
                EditCommitted(this, args);
        }

        private void DataGridCollectionViewSource_EditCanceled(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditCanceled != null)
                EditCanceled(this, args);

            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CreatingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = _CreateRouteWithDefaultValues();
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.NewItem);
            if (CreatingNewObject != null)
                CreatingNewObject(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                _isNewItemCreated = true; // set flag to true because new object was created
                _SetCreatingStatus();
                IsEditingInProgress = true;

                if (_InsertionRow.Cells["HardZones"] != null)
                    _InsertionRow.Cells["HardZones"].Visibility = Visibility.Visible;
            }
            else
            {
                e.Cancel = true;
                _isNewItemCreated = false; // set flag to false because new object wasn't created
                
                if (_InsertionRow.Cells["HardZones"] != null)
                    _InsertionRow.Cells["HardZones"].Visibility = Visibility.Hidden;
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

            // Get new item's name.
            (e.Item as Route).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                App.Current.Project.DefaultRoutes, e.Item as Route, true);

            // Find TextBox inside the cell and select new name.
            Cell currentCell = _InsertionRow.Cells[XceedGrid.CurrentContext.CurrentColumn];
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// View source new item created handler and invoking changing name of zone.
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Data grid item event arguments.</param>
        private void _DataGridCollectionViewSourceNewItemCreated(object sender,
                                                                 DataGridItemEventArgs e)
        {
            // Invoking changing of the item's name. Those method must be invoked, otherwise 
            // grid will not understand that item in insertion ro was changed and grid wouldnt allow
            // to commit this item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeName),
                System.Windows.Threading.DispatcherPriority.DataBind, e);

            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCreated != null)
                NewObjectCreated(this, args);
        }

        private void DataGridCollectionViewSource_CommittingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCommittingNewItemEventArgs e)
        {
            if (CommonHelpers.IgnoreVirtualLocations(e.Item))
            {
                DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
                if (CommittingNewObject != null)
                    CommittingNewObject(this, args);

                e.Handled = true;

                if (!args.Cancel)
                {
                    ICollection<Route> source = e.CollectionView.SourceCollection as ICollection<Route>;

                    Route currentRoute = e.Item as Route;
                    source.Add(currentRoute);

                    e.Index = source.Count - 1;
                    e.NewCount = source.Count;

                    App.Current.Project.Save();

                    _SetSelectionStatus();
                    IsEditingInProgress = false;

                    if (_InsertionRow.Cells["HardZones"] != null)
                        _InsertionRow.Cells["HardZones"].Visibility = Visibility.Hidden;
                }
                else
                {
                    e.Cancel = true;

                    if (_InsertionRow.Cells["HardZones"] != null)
                        _InsertionRow.Cells["HardZones"].Visibility = Visibility.Visible;
                }
            }
            else
            {
                e.Handled = true;
                e.Cancel = true;

                if (_InsertionRow.Cells["HardZones"] != null)
                    _InsertionRow.Cells["HardZones"].Visibility = Visibility.Visible;
            }
        }

        private void DataGridCollectionViewSource_NewItemCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, args);
        }

        private void DataGridCollectionViewSource_NewItemCanceled(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, args);

            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            // set property to true if new item was created or to false if new item wasn't created
            // otherwise an InvalidOperationException will be thrown (see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Inserting_Data.html)
            e.Handled = _isNewItemCreated;
            IsEditingInProgress = false;

            if (_InsertionRow.Cells["HardZones"] != null)
                _InsertionRow.Cells["HardZones"].Visibility = Visibility.Hidden;
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        #endregion

        #region Private Status Helpers

        /// <summary>
        /// Method sets selection status
        /// </summary>
        private void _SetSelectionStatus()
        {
            if (_needToUpdateStatus)
                _statusBuilder.FillSelectionStatus(App.Current.Project.DefaultRoutes.Count, (string)App.Current.FindResource(OBJECT_TYPE_NAME), XceedGrid.SelectedItems.Count, this);

            _needToUpdateStatus = true;
        }

        /// <summary>
        /// Method sets editing status
        /// </summary>
        /// <param name="itemName"></param>
        private void _SetEditingStatus(string itemName)
        {
            _statusBuilder.FillEditingStatus(itemName, (string)App.Current.FindResource(OBJECT_TYPE_NAME), this);
            _needToUpdateStatus = false;
        }

        /// <summary>
        /// Method sets creating status
        /// </summary>
        private void _SetCreatingStatus()
        {
            _statusBuilder.FillCreatingStatus((string)App.Current.FindResource(OBJECT_TYPE_NAME), this);
            _needToUpdateStatus = false;
        }

        #endregion

        #region Private members

        protected const string COLLECTION_SOURCE_KEY = "routesCollection";
        protected const string NAME_PROPERTY_STRING = "Name";
        protected const string OBJECT_TYPE_NAME = "DefaultRoute";

        private InsertionRow _InsertionRow;
        private StatusBuilder _statusBuilder = new StatusBuilder();

        // Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem 
        bool _isNewItemCreated = false;
        private bool _isGridCollectionLoaded = false;
        private bool _isGridLayoutLoaded = false;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus = false;

        /// <summary>
        /// Project events attached flag.
        /// </summary>
        private bool _projectEventsAttached = false;

        /// <summary>
        /// When default routes changing - updates scheduled routes if necessary.
        /// </summary>
        private DefaultRoutesController _defaultRoutesController;

        #endregion // Private members
    }
}
