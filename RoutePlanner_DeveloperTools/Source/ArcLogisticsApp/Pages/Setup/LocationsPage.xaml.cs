using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for Locations.xaml
    /// </summary>
    internal partial class LocationsPage : PageBase, ISupportDataObjectEditing, ISupportSelection, ICancelDataObjectEditing, ISupportSelectionChanged
    {
        public const string PAGE_NAME = "Locations";

        #region Constructors

        public LocationsPage()
        {
            InitializeComponent();
            _InitEventHandlers();
            _SetDefaults();
            _CheckPageComplete();

            if (App.Current.Project != null)
            {
                _Init();
            }

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController(XceedGrid);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Do regeocoding
        /// </summary>
        /// <param name="location">Location to regeocode</param>
        public void StartGeocoding(IGeocodable location)
        {
            _geocodablePage.StartGeocoding(location);
        }

        #endregion

        #region ICancelDataObjectEditing Members

        public void CancelObjectEditing()
        {
            XceedGrid.CancelEdit();
        }

        #endregion

        #region ISupportDataObjectEditing Members

        public bool IsEditingInProgress
        {
            get;
            protected set;
        }

        public event DataObjectCanceledEventHandler BeginningEdit;

        public event DataObjectEventHandler EditBegun;

        public event DataObjectCanceledEventHandler CommittingEdit;

        public event DataObjectEventHandler EditCommitted;

        public event DataObjectEventHandler EditCanceled;

        public event DataObjectCanceledEventHandler CreatingNewObject;

        public event DataObjectEventHandler NewObjectCreated;

        public event DataObjectCanceledEventHandler CommittingNewObject;

        public event DataObjectEventHandler NewObjectCommitted;

        public event DataObjectEventHandler NewObjectCanceled;

        #endregion

        #region ISupportSelection Members

        public IList SelectedItems
        {
            get 
            {
                return XceedGrid.SelectedItems; 
            }
        }

        /// <summary>
        /// Not implemented there
        /// </summary>
        /// <returns></returns>
        public bool SaveEditedItem()
        {
            return false;
        }

        public void Select(System.Collections.IEnumerable items)
        {
            // check that editing is not in progress
            if (IsEditingInProgress)
                throw new NotSupportedException((string)App.Current.FindResource("EditingInProcessExceptionMessage"));

            // check that all items are locations
            foreach (object item in items)
            {
                if (!(item is Location))
                    throw new ArgumentException("LocationsTypeExceptionMessage");
            }

            // add items to selection
            SelectedItems.Clear();
            foreach (object item in items)
                SelectedItems.Add(item);
        }

        #endregion

        #region ISupportSelectionChanged Members

        public event EventHandler SelectionChanged;

        #endregion

        #region Public Porperties

        public IList Selection
        {
            get
            {
                return XceedGrid.SelectedItems;
            }
        }

        #endregion

        #region Public Page Overrided Members

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("LocationsPageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("LocationsBrush");
                return brush;
            }
        }

        public override bool CanBeLeft
        {
            get
            {
                // If there are validation error in insertion row - we cannot leave page.
                if (XceedGrid.InsertionRow != null && XceedGrid.InsertionRow.DataContext is Location &&
                    (XceedGrid.InsertionRow.HasValidationError 
                    || !(XceedGrid.InsertionRow.DataContext as Location).IsGeocoded))
                    return false;
                // If there isnt - we must validate all grid source items.
                else
                    return base.CanBeLeft && CanBeLeftValidator<Location>.IsValid(App.Current.Project.Locations);
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion

        #region Public PageBase overrided members

        internal override void SaveLayout()
        {
            if (Properties.Settings.Default.LocationsGridSettings == null)
                Properties.Settings.Default.LocationsGridSettings = new SettingsRepository();

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.LocationsGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.LocationsPagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Method inits event handlers
        /// </summary>
        protected void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(LocationsPage_Loaded);
            this.Unloaded += new RoutedEventHandler(LocationsPage_Unloaded);
            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(App_ProjectClosing);
            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);
            App.Current.Exit += new ExitEventHandler(Current_Exit);
            XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(XceedGrid_SelectionChanged);

            Project project = App.Current.Project;
            if (null != project)
            {
                project.Locations.CollectionChanged += new NotifyCollectionChangedEventHandler(Locations_CollectionChanged);
                _projectEventsAttached = true;
            }

            App.Current.MapDisplay.ShowBarriersChanged += new EventHandler(MapDisplay_ShowBarriersChanged);
            App.Current.MapDisplay.ShowZonesChanged += new EventHandler(MapDisplay_ShowZonesChanged);
        }

        /// <summary>
        /// Method fills page's properties by default values
        /// </summary>
        protected void _SetDefaults()
        {
            IsRequired = true;
            IsAllowed = true;
            CanBeLeft = true;
            DoesSupportCompleteStatus = true;
            IsComplete = false;
            commandButtonGroup.Initialize(CategoryNames.LocationsCommands, XceedGrid);
        }

        protected void _Init()
        {
            _CreateZonesLayer();
            _CreateBarriersLayer();

            double layerOpacity = (double)Application.Current.FindResource("BarriersAndZonesOpacity");
            if (layerOpacity > 1)
                layerOpacity = 1;
            if (layerOpacity < 0)
                layerOpacity = 0;
            _barriersLayer.MapLayer.Opacity = layerOpacity;
            _zonesLayer.MapLayer.Opacity = layerOpacity;
            mapCtrl.AddRegionsLayersToWidget = true;

            _CreateLocationsLayer();
            _geocodablePage = new GeocodablePage(typeof(Location), mapCtrl, candidateSelect, controlsGrid,
                XceedGrid, splitter, _locationsLayer);
            _CreateMultiCollectionBinding();
            mapCtrl.AddTool(new EditingTool(), null);

            _gridAutoFitHelper = new GridAutoFitHelper(XceedGrid, LayoutRoot, MapBorder);

            _inited = true;
        }

        /// <summary>
        /// Checks page complete status
        /// </summary>
        protected void _CheckPageComplete()
        {
            Project project = App.Current.Project;
            if (project != null && project.Locations.Count > 0)
                IsComplete = true;
            else
                IsComplete = false;
        }

        /// <summary>
        /// Method inits collection of locations.
        /// </summary>
        protected void _IniDataGridCollection()
        {
            Project project = App.Current.Project;
            if (project != null)
            {
                DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

                IDataObjectCollection<Location> collection = (IDataObjectCollection<Location>)project.Locations;
                SortedDataObjectCollection<Location> sortedLocationsCollection = new SortedDataObjectCollection<Location>(collection, new CreationTimeComparer<Location>());
                collectionSource.Source = sortedLocationsCollection;

                ((INotifyCollectionChanged)XceedGrid.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(LocationsPage_CollectionChanged);

                _isDataGridCollectionInited = true;
                XceedGrid.SelectedItems.Clear();
            }
            else
                _isDataGridCollectionInited = false;
        }

        protected void _InitDataGridLayout()
        {
            DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.LocationsGridStructure);
            structureInitializer.BuildGridStructure(collectionSource, XceedGrid);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.LoactionsSettingsRepositoryName, collectionSource.ItemProperties);
            layoutLoader.LoadLayout(XceedGrid);

            _isDataGridLayoutLoaded = true;
        }

        #endregion

        #region Private Status Helpers

        /// <summary>
        /// Method sets selection status
        /// </summary>
        private void _SetSelectionStatus()
        {
            if (App.Current.Project != null && _needToUpdateStatus)
                _statusBuilder.FillSelectionStatus(App.Current.Project.Locations.Count, (string)App.Current.FindResource(OBJECT_TYPE_NAME), XceedGrid.SelectedItems.Count, this);
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

        #region Map methods

        private void App_ApplicationInitialized(object sender, EventArgs e)
        {
            if (!_isServicesLoaded)
            {
                this.mapCtrl.Map = App.Current.Map;
                _isServicesLoaded = true;
            }
        }

        #endregion

        #region Data Object Editing Event Handlers

        private void DataGridCollectionViewSource_BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (BeginningEdit != null)
                BeginningEdit(this, args);
            
            e.Handled = true;

            if (!args.Cancel)
            {
                IsEditingInProgress = true;
                _geocodablePage.OnBeginningEdit(e);
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
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingEdit != null)
                CommittingEdit(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                _geocodablePage.OnCommittingEdit(e, true);

                _SetSelectionStatus();
                IsEditingInProgress = false;
            }
            else
                e.Cancel = true;
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

            _geocodablePage.OnEditCanceled(e);
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CreatingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new Location();
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.NewItem);
            if (CreatingNewObject != null)
                CreatingNewObject(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                if (!_geocodablePage.IsGeocodingInProcess)
                {
                    _geocodablePage.OnCreatingNewItem(e);
                    _isNewItemCreated = true; // set flag to true because new object was created
                }
                else
                {
                    e.Cancel = true;
                    _isNewItemCreated = false;  // set flag to false because new object wasn't created
                }

                _isNewItemCreated = true; // 
                _SetCreatingStatus();
                IsEditingInProgress = true;
            }
            else
            {
                e.Cancel = true;
                _isNewItemCreated = false; // set flag to false because new object wasn't created
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
            if (!string.IsNullOrEmpty((e.Item as Location).Name))
                return;

            // Get new item's name.
            (e.Item as Location).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                App.Current.Project.Locations, e.Item as Location, true);

            // Find TextBox inside the cell and select new name.
            Cell currentCell = _insertionRow.Cells[XceedGrid.CurrentContext.CurrentColumn];
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        private void DataGridCollectionViewSource_NewItemCreated(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            // Invoking changing of the item's name. Those method must be invoked, otherwise 
            // grid will not understand that item in insertion ro was changed and grid wouldnt allow
            // to commit this item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeName),
                System.Windows.Threading.DispatcherPriority.Render, e);

            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCreated != null)
                NewObjectCreated(this, args);
        }

        private void DataGridCollectionViewSource_CommittingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCommittingNewItemEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingNewObject != null)
                CommittingNewObject(this, args);
            
            e.Handled = true;

            if (!args.Cancel)
            {
                if (_geocodablePage.OnCommittingNewItem(e))
                {
                    ICollection<Location> source = e.CollectionView.SourceCollection as ICollection<Location>;

                    Location geocodable = e.Item as Location;
                    source.Add(geocodable);
                    
                    if (App.Current.Project != null)
                        App.Current.Project.Save();

                    e.Index = source.Count - 1;
                    e.NewCount = source.Count;
                    _SetSelectionStatus();
                }
                IsEditingInProgress = false;
            }
            else
                e.Cancel = true;
        }

        private void DataGridCollectionViewSource_NewItemCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, args);
            _geocodablePage.OnNewItemCommitted();
        }

        private void DataGridCollectionViewSource_NewItemCanceled(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, args);

            _geocodablePage.OnNewItemCancelling();
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            // set property to true if new item was created or to false if new item wasn't created
            // otherwise an InvalidOperationException will be thrown (see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Inserting_Data.html)
            e.Handled = _isNewItemCreated;
            _geocodablePage.OnNewItemCancelling();
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        #endregion

        #region Event Handlers

        private void XceedGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            // NOTE : commented in xceed v 3.6  - events order changed in this version of data grid
            //if (XceedGrid.IsBeingEdited)
            //    XceedGrid.CancelEdit();
            if (e.SelectionInfos[0] != null && e.SelectionInfos[0].AddedItems.Count > 0)
                XceedGrid.BringItemIntoView(e.SelectionInfos[0].AddedItems[0]);

            if (_geocodablePage != null)
                _geocodablePage.OnSelectionChanged(XceedGrid.SelectedItems);

            // NOTE : event raises to notify all necessary object about selection was changed. Added because XceedGrid.SelectedItems doesn't implement INotifyCollectionChanged
            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);

            _SetSelectionStatus();
        }

        /// <summary>
        /// Method initialize insertion row when it's focused
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertionRow_Initialized(object sender, EventArgs e)
        {
            _insertionRow = sender as InsertionRow;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        private void LocationsPage_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus();
        }

        /// <summary>
        /// Occurs when project loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_ProjectLoaded(object sender, EventArgs e)
        {
            if (!_projectEventsAttached)
            {
                App.Current.Project.Locations.CollectionChanged += new NotifyCollectionChangedEventHandler(Locations_CollectionChanged);
                _projectEventsAttached = true;
            }

            _CheckPageComplete();

            _InitDataGridLayout();
            _IniDataGridCollection();

            if (!_inited)
                _Init();

            // clear binding of old collections
            if (_collectionBinding != null)
                _collectionBinding.UnregisterAllCollections();

            _locationsLayer.Collection = App.Current.Project.Locations;
            _barriersLayer.Collection = App.Current.Project.Barriers.SearchAll(true);
            _zonesLayer.Collection = App.Current.Project.Zones;

            _CreateMultiCollectionBinding();

            Debug.Assert(_locationsLayer.SelectedItems.Count == 0);

            mapCtrl.HideZoomToCandidatePopup();

            _newProjectLoaded = true;
        }

        /// <summary>
        /// Occurs when project closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_ProjectClosing(object sender, EventArgs e)
        {
            if (_projectEventsAttached)
            {
                App.Current.Project.Locations.CollectionChanged -= Locations_CollectionChanged;
                _projectEventsAttached = false;
            }

            _geocodablePage.OnProjectClosed();

            SaveLayout();
        }

        /// <summary>
        /// Occurs when page loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationsPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled += new EventHandler(LocationsPage_NavigationCalled);

            if (!_isDataGridLayoutLoaded)
                _InitDataGridLayout();
            if (!_isDataGridCollectionInited)
                _IniDataGridCollection();

            _CheckPageComplete();

            _needToUpdateStatus = true;
            _SetSelectionStatus();

            // Set startup extent
            if (App.Current.Project.Locations.Count > 0)
            {
                List<ESRI.ArcLogistics.Geometry.Point> points = MapExtentHelpers.GetPointsInExtent(App.Current.Project.Locations);
                ESRI.ArcLogistics.Geometry.Envelope? extent = MapExtentHelpers.GetCollectionExtent(points);
                mapCtrl.StartupExtent = extent;

                // if new project loaded update extent
                if (_newProjectLoaded)
                {
                    MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
                    _newProjectLoaded = false;
                }
            }
        }

        private void Locations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _CheckPageComplete();
        }

        /// <summary>
        /// Occurs when user try navigate to other page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationsPage_NavigationCalled(object sender, EventArgs e)
        {
            if (_geocodablePage.IsGeocodingInProcess)
                CanBeLeft = false;
            else
            {
                CanBeLeft = true;
                try
                {
                    XceedGrid.EndEdit();
                    CanBeLeft = true;
                }
                catch
                {
                    CanBeLeft = false;
                }
            }

            // If there are validation errors - show them.
            CanBeLeftValidator<Location>.ShowErrorMessagesInMessageWindow(App.Current.Project.Locations);
        }

        /// <summary>
        /// Occurs when page unloads
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LocationsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled -= LocationsPage_NavigationCalled;

            this.CancelObjectEditing();
        }

        /// <summary>
        /// Occurs when starts navigation  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            CanBeLeft = false;
        }

        /// <summary>
        /// React on "Show Zones" option changed
        /// </summary>
        private void MapDisplay_ShowZonesChanged(object sender, EventArgs e)
        {
            if (_zonesLayer != null)
                _zonesLayer.MapLayer.Visible = App.Current.MapDisplay.ShowZones;
        }

        /// <summary>
        /// React on "Show Barriers" option changed
        /// </summary>
        private void MapDisplay_ShowBarriersChanged(object sender, EventArgs e)
        {
            if(_barriersLayer != null)
                _barriersLayer.MapLayer.Visible = App.Current.MapDisplay.ShowBarriers;
        }

        /// <summary>
        /// Create layer for showing project locations
        /// </summary>
        private void _CreateLocationsLayer()
        {
            IEnumerable coll = (IEnumerable)App.Current.Project.Locations;
            _locationsLayer = new ObjectLayer(coll, typeof(Location), false);
            _locationsLayer.EnableToolTip();
            mapCtrl.AddLayer(_locationsLayer);
            _locationsLayer.Selectable = true;
        }

        private void _CreateMultiCollectionBinding()
        {
            _collectionBinding = new MultiCollectionBinding();
            _collectionBinding.RegisterCollection(XceedGrid);
            _collectionBinding.RegisterCollection(mapCtrl.SelectedItems);
        }

        /// <summary>
        /// Create layer for showing project zones
        /// </summary>
        private void _CreateZonesLayer()
        {
            IEnumerable zonesColl = (IEnumerable)App.Current.Project.Zones;

            _zonesLayer = new ObjectLayer(zonesColl, typeof(Zone), false);
            _zonesLayer.EnableToolTip();
            _zonesLayer.MapLayer.Visible = App.Current.MapDisplay.ShowZones;
            _zonesLayer.ConstantOpacity = true;
            _zonesLayer.Name = App.Current.FindString("ZonesLayerName");
            _zonesLayer.IsBackgroundLayer = true;

            mapCtrl.AddLayer(_zonesLayer);
        }

        /// <summary>
        /// Create layer for showing project barriers for all dates
        /// </summary>
        private void _CreateBarriersLayer()
        {
            IEnumerable barriersColl = (IDataObjectCollection<Barrier>)App.Current.Project.Barriers.SearchAll(true);

            _barriersLayer = new ObjectLayer(barriersColl, typeof(Barrier), false);
            _barriersLayer.EnableToolTip();
            _barriersLayer.MapLayer.Visible = App.Current.MapDisplay.ShowBarriers;
            _barriersLayer.ConstantOpacity = true;
            _barriersLayer.Name = (string)App.Current.FindResource("BarriersLayerName");
            _barriersLayer.IsBackgroundLayer = true;

            mapCtrl.AddLayer(_barriersLayer);
        }

        #endregion

        #region Private Fields

        protected const string COLLECTION_SOURCE_KEY = "locationsSource";
        protected const string NAME_PROPERTY_STRING = "Name";
        protected const string OBJECT_TYPE_NAME = "Location";

        private bool _isDataGridCollectionInited;
        private bool _isDataGridLayoutLoaded;
        private bool _isServicesLoaded;
        private bool _inited;

        // Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem 
        private bool _isNewItemCreated;

        private GeocodablePage _geocodablePage;
        private ObjectLayer _locationsLayer;
        private InsertionRow _insertionRow;

        private MultiCollectionBinding _collectionBinding;
        private StatusBuilder _statusBuilder = new StatusBuilder();

        private ObjectLayer _zonesLayer;
        private ObjectLayer _barriersLayer;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus;

        /// <summary>
        /// Helper for autofit items on page.
        /// </summary>
        private GridAutoFitHelper _gridAutoFitHelper;

        /// <summary>
        /// Project events attached flag.
        /// </summary>
        private bool _projectEventsAttached;

        /// <summary>
        /// New project loaded flag.
        /// </summary>
        private bool _newProjectLoaded;

        #endregion
    }
}
