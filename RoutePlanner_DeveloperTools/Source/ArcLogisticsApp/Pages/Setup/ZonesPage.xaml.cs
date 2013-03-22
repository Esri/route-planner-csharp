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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for ZonesPage.xaml
    /// </summary>
    internal partial class ZonesPage : PageBase, ISupportDataObjectEditing, ISupportSelection, ICancelDataObjectEditing, ISupportSelectionChanged
    {
        public const string PAGE_NAME = "Zones";

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ZonesPage</c> class.
        /// </summary>
        public ZonesPage()
        {
            InitializeComponent();
            _InitEventHandlers();
            _SetDefaults();

            if (App.Current.Project != null)
                _Init();

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController(XceedGrid);
        }

        #endregion // Constructors

        #region ICancelDataObjectEditing Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cancels object editing.
        /// </summary>
        public void CancelObjectEditing()
        {
            XceedGrid.CancelEdit();
        }

        #endregion

        #region ISupportDataObjectEditing Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// ISupportDataObjectEditing implementation.
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

        #region ISupportSelection Members
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
        /// Not implemented there
        /// </summary>
        /// <returns>False.</returns>
        public bool SaveEditedItem()
        {
            return false;
        }

        /// <summary>
        /// Method selects necessary items.
        /// </summary>
        /// <param name="items">Items that should be selected.</param>
        public void Select(IEnumerable items)
        {
            // Check that editing is not in progress.
            if (IsEditingInProgress)
                throw new NotSupportedException((string)App.Current.FindResource("EditingInProcessExceptionMessage"));

            // Check that all items are zones.
            foreach (object item in items)
            {
                if (!(item is Zone))
                    throw new ArgumentException("ZonesTypeExceptionMessage");
            }

            // Add items to selection.
            SelectedItems.Clear();
            foreach (object item in items)
                SelectedItems.Add(item);
        }

        #endregion // ISupportSelection members

        #region ISupportSelectionChanged Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when selection in page finish to change.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region Page Overrided Members
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
            get { return (string)App.Current.FindResource("ZonesPageCaption"); }
        }

        /// <summary>
        /// Returns page icon as a TileBrush (DrawingBrush or ImageBrush).
        /// </summary>
        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("ZonesBrush");
                return brush;
            }
        }

        #endregion

        #region PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saves current layout to user config file.
        /// </summary>
        internal override void SaveLayout()
        {
            if (null == Properties.Settings.Default.ZonesGridSettings)
                Properties.Settings.Default.ZonesGridSettings = new SettingsRepository();

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.ZonesGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Gets help text.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.ZonesPagePath); }
        }

        /// <summary>
        /// Gets commands category name.
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
                    return base.CanBeLeft && CanBeLeftValidator<Zone>.IsValid(App.Current.Project.Zones);
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion // PageBase overrided members

        #region Private Map methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        void App_ApplicationInitialized(object sender, EventArgs e)
        {
            this.mapCtrl.Map = App.Current.Map;
        }

        /// <summary>
        /// Init page.
        /// </summary>
        private void _Init()
        {
            _CreateZonesLayer();
            _regionsPage = new RegionsPage(mapCtrl, XceedGrid, _zonesLayer, typeof(Zone), LayoutRoot, MapBorder);

            mapCtrl.AddTool(new EditingTool(), null);

            _inited = true;
        }

        #endregion // Map methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method gets sorted collection.
        /// </summary>
        /// <returns>Sorted collection.</returns>
        private SortedDataObjectCollection<Zone> _GetSortedCollection()
        {
            IDataObjectCollection<Zone> zones = (IDataObjectCollection<Zone>)App.Current.Project.Zones;
            SortedDataObjectCollection<Zone> sortedCollection = new SortedDataObjectCollection<Zone>(zones, new CreationTimeComparer<Zone>());
            return sortedCollection;
        }

        /// <summary>
        /// Method inits collection.
        /// </summary>
        private void _InitDataGridCollection()
        {
            try
            {
                _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

                _collectionSource.Source = _GetSortedCollection();

                ((INotifyCollectionChanged)XceedGrid.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(ZonesPage_CollectionChanged);

                _isDataGridCollectionInited = true;
                XceedGrid.SelectedItems.Clear();
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }

        /// <summary>
        /// Loads grid layout.
        /// </summary>
        private void _InitDataGridLayout()
        {
            if (_collectionSource == null)
                _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.ZonesGridStructure);
            structureInitializer.BuildGridStructure(_collectionSource, XceedGrid);

            // Load grid layout.
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.ZonesSettingsRepositoryName, _collectionSource.ItemProperties);
            layoutLoader.LoadLayout(XceedGrid);

            _isDataGridLayoutLoaded = true;
        }

        /// <summary>
        /// Create layer to show zones on map.
        /// </summary>
        private void _CreateZonesLayer()
        {
            IEnumerable coll = (IEnumerable)App.Current.Project.Zones;

            _zonesLayer = new ObjectLayer(coll, typeof(Zone), false);
            _zonesLayer.EnableToolTip();
            mapCtrl.AddLayer(_zonesLayer);

            _zonesLayer.Selectable = true;
            _CreateMultiCollectionBinding();
        }

        /// <summary>
        /// Create multicollection binding for synchronizing seletion.
        /// </summary>
        private void _CreateMultiCollectionBinding()
        {
            _collectionBinding = new MultiCollectionBinding();
            _collectionBinding.RegisterCollection(XceedGrid);
            _collectionBinding.RegisterCollection(mapCtrl.SelectedItems);
        }

        /// <summary>
        /// Method fills page's properties by default values.
        /// </summary>
        protected void _SetDefaults()
        {
            IsRequired = false;
            IsAllowed = true;
            CanBeLeft = true;
            commandButtonGroup.Initialize(CategoryNames.ZonesCommands, XceedGrid);
        }

        /// <summary>
        /// Method init event handlers.
        /// </summary>
        protected void _InitEventHandlers()
        {
            mapCtrl.KeyDown += new System.Windows.Input.KeyEventHandler(_PageKeyDown);
            this.Loaded += new RoutedEventHandler(ZonePage_Loaded);
            this.Unloaded += new RoutedEventHandler(ZonesPage_Unloaded);
            App.Current.Exit += new ExitEventHandler(Current_Exit);
            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);
            XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(XceedGrid_SelectionChanged);
        }

        #endregion // Protected methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// When user press "ESC" - cancel all changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">System.Windows.Input.KeyEventArgs.</param>
        private void _PageKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                _regionsPage.CancelEdit();
        }

        private void XceedGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (e.SelectionInfos[0] != null && e.SelectionInfos[0].AddedItems.Count > 0)
                XceedGrid.BringItemIntoView(e.SelectionInfos[0].AddedItems[0]);

            _regionsPage.OnSelectionChanged(e);

            _SetSelectionStatus();

            // NOTE : event raises to notify all necessary object about selection was changed. Added because XceedGrid.SelectedItems doesn't implement INotifyCollectionChanged
            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);
        }

        private void App_ProjectLoaded(object sender, EventArgs e)
        {
            _InitDataGridLayout();
            _InitDataGridCollection();

            if (!_inited)
                _Init();

            _zonesLayer.Collection = App.Current.Project.Zones;

            _newProjectLoaded = true;
        }

        private void ZonesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled -= ZonesPage_NavigationCalled;
            this.CancelObjectEditing();

            SaveLayout();
        }

        private void InsertionRow_Initialized(object sender, EventArgs e)
        {
            _insertionRow = sender as InsertionRow;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        private void ZonePage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled += new EventHandler(ZonesPage_NavigationCalled);

            if (!_isDataGridLayoutLoaded)
                _InitDataGridLayout();
            if (!_isDataGridCollectionInited)
                _InitDataGridCollection();

            _needToUpdateStatus = true;

            // Set label as status bar content.
            _SetSelectionStatus();

            // Set startup extent.
            IEnumerable collectionInExtent;

            // If zones present - set extent on them. Otherwise on locations.
            if (App.Current.Project.Zones.Count > 0)
            {
                collectionInExtent = App.Current.Project.Zones;
            }
            else
            {
                collectionInExtent = App.Current.Project.Locations;
            }

            List<ESRI.ArcLogistics.Geometry.Point> points = MapExtentHelpers.GetPointsInExtent(collectionInExtent);
            ESRI.ArcLogistics.Geometry.Envelope? extent = MapExtentHelpers.GetCollectionExtent(points);
            mapCtrl.StartupExtent = extent;

            // if new project loaded update extent
            if (_newProjectLoaded)
            {
                MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
                _newProjectLoaded = false;
            }
        }

        private void ZonesPage_NavigationCalled(object sender, EventArgs e)
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

            // If there are validation errors - show them.
            CanBeLeftValidator<Zone>.ShowErrorMessagesInMessageWindow(App.Current.Project.Zones);
        }

        private void ZonesPage_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus();
        }

        #endregion // Event handlers

        #region Data Object Editing Event Handlers

        private void DataGridCollectionViewSource_BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (BeginningEdit != null)
                BeginningEdit(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                _regionsPage.OnBeginningEdit(e);
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
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingEdit != null)
                CommittingEdit(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                _regionsPage.OnCommittingEdit(e);
                if (App.Current.Project != null)
                    App.Current.Project.Save();

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

            _regionsPage.OnEditCanceled(e);
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CreatingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new Zone();
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.NewItem);
            if (CreatingNewObject != null)
                CreatingNewObject(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                _regionsPage.OnCreatingNewItem(e);

                _isNewItemCreated = true; // set flag to true because new object was created
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
            if (!string.IsNullOrEmpty((e.Item as Zone).Name))
                return;

            // Get new item's name.
            (e.Item as Zone).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                App.Current.Project.Zones, e.Item as Zone, true);

            // Find TextBox inside the cell and select new name.
            Cell currentCell = _insertionRow.Cells[XceedGrid.CurrentContext.CurrentColumn];
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Handler raises event about new item was created and invoking changing name of zone.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
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
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingNewObject != null)
                CommittingNewObject(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                ICollection<Zone> source = e.CollectionView.SourceCollection as ICollection<Zone>;

                Project project = App.Current.Project;
                Zone current = e.Item as Zone;
                project.Zones.Add(current);

                e.Index = source.Count;
                e.NewCount = source.Count + 1;

                project.Save();

                e.Handled = true;
                _regionsPage.OnNewItemCommitted(e);

                _SetSelectionStatus();
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
            _regionsPage.OnNewItemCancelling(e);
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        #endregion Data Object Editing Event Handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method sets selection status.
        /// </summary>
        private void _SetSelectionStatus()
        {
            if (_needToUpdateStatus)
                _statusBuilder.FillSelectionStatus(App.Current.Project.Zones.Count, (string)App.Current.FindResource(OBJECT_TYPE_NAME), XceedGrid.SelectedItems.Count, this);

            _needToUpdateStatus = true;
        }

        /// <summary>
        /// Method sets editing status.
        /// </summary>
        /// <param name="itemName">Current item name.</param>
        private void _SetEditingStatus(string itemName)
        {
            _statusBuilder.FillEditingStatus(itemName, (string)App.Current.FindResource(OBJECT_TYPE_NAME), this);
            _needToUpdateStatus = false;
        }

        /// <summary>
        /// Method sets creating status.
        /// </summary>
        private void _SetCreatingStatus()
        {
            _statusBuilder.FillCreatingStatus((string)App.Current.FindResource(OBJECT_TYPE_NAME), this);
            _needToUpdateStatus = false;
        }


        #endregion

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page's insertion row
        /// </summary>
        private InsertionRow _insertionRow;

        protected const string COLLECTION_SOURCE_KEY = "zonesSource";
        protected const string NAME_PROPERTY_STRING = "Name";
        protected const string OBJECT_TYPE_NAME = "Zone";

        /// <summary>
        /// Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem 
        /// </summary>
        private bool _isNewItemCreated;

        /// <summary>
        /// Flag shows whether layout loaded.
        /// </summary>
        private bool _isDataGridLayoutLoaded;

        /// <summary>
        /// Flag shows whether data grid collection inited.
        /// </summary>
        private bool _isDataGridCollectionInited;

        /// <summary>
        /// Is page inited.
        /// </summary>
        private bool _inited;

        /// <summary>
        /// Layer to show zones on map.
        /// </summary>
        private ObjectLayer _zonesLayer;

        /// <summary>
        /// Multi collection binding used for synchronizing selection on map and datagrid control.
        /// </summary>
        private MultiCollectionBinding _collectionBinding;

        /// <summary>
        /// Page helper for common regions functionality.
        /// </summary>
        private RegionsPage _regionsPage;
        
        /// <summary>
        /// Status builder.
        /// </summary>
        private StatusBuilder _statusBuilder = new StatusBuilder();

        /// <summary>
        /// Collection view source.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus;

        /// <summary>
        /// New project loaded flag.
        /// </summary>
        private bool _newProjectLoaded;

        #endregion // Private members
    }
}
