using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.App.Widgets;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;
using AppData = ESRI.ArcLogistics.Data;
using AppGeometry = ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for BarriersPage.xaml.
    /// </summary>
    internal partial class BarriersPage :
        PageBase,
        ISupportDataObjectEditing,
        ISupportSelection,
        ICancelDataObjectEditing,
        ISupportSelectionChanged
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>BarriersPage</c> class.
        /// </summary>
        public BarriersPage()
        {
            InitializeComponent();

            _InitEventHandlers();
            _SetDefaults();

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController(XceedGrid);
        }

        #endregion // Constructors

        #region ICancelDataObjectEditing members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Cancels editing object.
        /// </summary>
        public void CancelObjectEditing()
        {
            XceedGrid.CancelEdit();
        }

        /// <summary>
        /// Cancels creating new Object (clears InsertionRow).
        /// </summary>
        public void CancelNewObject()
        {
            Debug.Assert(false); // Not implemented there.
        }

        #endregion // ICancelDataObjectEditing members

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
        /// Selected items from table.
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
        /// <returns>Always FALSE. Not supported there.</returns>
        public bool SaveEditedItem()
        {
            return false;
        }

        /// <summary>
        /// Selects items in table.
        /// </summary>
        /// <param name="items">Items to selection.</param>
        public void Select(IEnumerable items)
        {
            // Check that editing is not in progress.
            if (IsEditingInProgress)
            {
                string message = App.Current.FindString("EditingInProcessExceptionMessage");
                throw new NotSupportedException(message); // exception
            }

            // Check that all items are barriers.
            foreach (object item in items)
            {
                if (!(item is Barrier))
                {
                    string message = App.Current.FindString("BarriersTypeExceptionMessage");
                    throw new ArgumentException(message); // exception
                }
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
        /// Raises when selection changes.
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
            get { return App.Current.FindString("BarriersPageCaption"); }
        }

        /// <summary>
        /// Returns page icon as a TileBrush (ImageBrush).
        /// </summary>
        public override TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("BarriersBrush");
                return brush;
            }
        }

        public override bool CanBeLeft
        {
            get
            {
                if (_isInited)
                {
                    // If there are validation error in insertion row - we cannot leave page.
                    if (XceedGrid.IsInsertionRowInvalid)
                        return false;
                    // If there isnt - we must validate all barriers.
                    else
                    {
                        var barriers = (IDataObjectCollection<Barrier>)_collectionSource.Source;
                            return base.CanBeLeft && CanBeLeftValidator<Barrier>.IsValid(barriers);
                    }
                }
                else
                    // If page wasnt inited than we dont need to validate barriers.
                    return base.CanBeLeft;
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion

        #region Public PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saves layout of the page to some storage.
        /// </summary>
        internal override void SaveLayout()
        {
            if (null == Properties.Settings.Default.BarriersGridSettings)
                Properties.Settings.Default.BarriersGridSettings = new SettingsRepository();

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.BarriersGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Returns Help Topic reference.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.BarriersPagePath); }
        }

        /// <summary>
        /// Returns category name of commands that will be present in Tasks widget.
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion // PageBase overrided members

        #region Portected PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates widgets that are shown for all pages.
        /// </summary>
        protected override void CreateWidgets()
        {
            base.CreateWidgets();

            BarrierCalendarWidget calendarWidget = new BarrierCalendarWidget();
            calendarWidget.Initialize(this);

            this.EditableWidgetCollection.Insert(0, calendarWidget);
        }

        #endregion // Portected PageBase overrided members

        #region Private Map methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// React on application initialized. Init map.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ApplicationInitialized(object sender, EventArgs e)
        {
            this.mapCtrl.Map = App.Current.Map;
        }

        /// <summary>
        /// Init page.
        /// </summary>
        private void _Init()
        {
            _CreateBarriersLayer();
            XceedGrid.SelectedItems.Clear();
            _regionsPage = new RegionsPage(mapCtrl,
                                           XceedGrid,
                                           _barriersLayer,
                                           typeof(Barrier),
                                           LayoutRoot,
                                           MapBorder);

            mapCtrl.AddTool(new EditingTool(), null);
            _isInited = true;
        }

        #endregion // Map methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets sorted collection.
        /// </summary>
        /// <returns>Sorted barrier collection (sorted by creation time).</returns>
        private AppData.SortedDataObjectCollection<Barrier> _GetSortedCollection()
        {
            AppData.IDataObjectCollection<Barrier> collection =
                App.Current.Project.Barriers.SearchAll(true);

            var sortedCollection =
                new AppData.SortedDataObjectCollection<Barrier>(collection,
                                                                new CreationTimeComparer<Barrier>());

            return sortedCollection;
        }

        /// <summary>
        /// Method inits collection.
        /// </summary>
        private void _InitCollection()
        {
            try
            {
                _collectionSource =
                    (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);
                IList<Barrier> sortedBarriersCollection = _GetSortedCollection();
                _collectionSource.Source = sortedBarriersCollection;

                ((INotifyCollectionChanged)XceedGrid.Items).CollectionChanged +=
                    new NotifyCollectionChangedEventHandler(_BarriersPageCollectionChanged);
                _isDataGridCollectionInited = true;
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
            {
                _collectionSource =
                    (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);
            }

            var structureInitializer =
                new GridStructureInitializer(GridSettingsProvider.BarriersGridStructure);
            structureInitializer.BuildGridStructure(_collectionSource, XceedGrid);

            // load grid layout
            var layoutLoader =
                new GridLayoutLoader(GridSettingsProvider.BarriersSettingsRepositoryName,
                                     _collectionSource.ItemProperties);
            layoutLoader.LoadLayout(XceedGrid);

            // Find geometry field and...
            foreach (Column column in XceedGrid.Columns)
            {
                if (column.FieldName == GEOMETRY_FIELD_NAME)
                {
                    // ...set field width to default one,
                    // to let users know about this fields is exists.
                    ColumnBase geometryColumn =
                        XceedGrid.Columns[GEOMETRY_FIELD_NAME];

                    if (geometryColumn.Width == 0)
                        geometryColumn.Width = DEFAULT_COLUMN_WIDTH;

                    break; // Work done.
                }
            }

            _isLayoutLoaded = true;
        }

        /// <summary>
        /// Create layer to show barriers on map.
        /// </summary>
        private void _CreateBarriersLayer()
        {
            var coll = (IEnumerable) _collectionSource.Source;

            _barriersLayer = new ObjectLayer(coll, typeof(Barrier), false);
            _barriersLayer.EnableToolTip();
            mapCtrl.AddLayer(_barriersLayer);

            _barriersLayer.Selectable = true;
            _barriersLayer.LayerContext = App.Current.CurrentDate;

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
            commandButtonGroup.Initialize(CategoryNames.BarriersCommands, XceedGrid);
        }

        /// <summary>
        /// Inits event handlers.
        /// </summary>
        protected void _InitEventHandlers()
        {
            mapCtrl.KeyDown += new System.Windows.Input.KeyEventHandler(_KeyDown);
            App.Current.ApplicationInitialized += new EventHandler(_ApplicationInitialized);
            this.Loaded += new RoutedEventHandler(_BarrierPageLoaded);
            this.Unloaded += new RoutedEventHandler(_BarriersPageUnloaded);
            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);
            App.Current.Exit += new ExitEventHandler(_ApplicationExit);
            App.Current.CurrentDateChanged += new EventHandler(_BarriersPageCurrentDateChanged);
            XceedGrid.SelectionChanged +=
                new DataGridSelectionChangedEventHandler(_DataGridControlSelectionChanged);
        }

        /// <summary>
        /// When user press "ESC" - cancel all changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">System.Windows.Input.KeyEventArgs.</param>
        private void _KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                _regionsPage.CancelEdit();
        }
        
        #endregion // Protected methods

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _DataGridControlSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (e.SelectionInfos.Count > 0)
            {
                if (e.SelectionInfos[0] != null && e.SelectionInfos[0].AddedItems.Count > 0)
                    XceedGrid.BringItemIntoView(e.SelectionInfos[0].AddedItems[0]);

                if (_regionsPage != null)
                    _regionsPage.OnSelectionChanged(e);
            }

            _SetSelectionStatus(XceedGrid.Items.Count);

            // NOTE : event raises to notify all necessary object about selection was changed.
            // Added because DataGridControl.SelectedItems doesn't implement INotifyCollectionChanged
            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);
        }

        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _InitDataGridLayout();
            _InitCollection();

            if (!_isInited)
                _Init();

            _collectionBinding.UnregisterAllCollections();
            _barriersLayer.Collection = (IEnumerable)_collectionSource.Source;
            _CreateMultiCollectionBinding();

            _newProjectLoaded = true;
        }

        private void _BarriersPageUnloaded(object sender, RoutedEventArgs e)
        {
            if (null != App.Current)
                App.Current.MainWindow.NavigationCalled -= _BarriersPageNavigationCalled;

            this.CancelObjectEditing();

            SaveLayout();
        }

        /// <summary>
        /// React on date changed.
        /// </summary>
        private void _BarriersPageCurrentDateChanged(object sender, EventArgs e)
        {
            if (App.Current.Project != null && _barriersLayer != null)
            {
                // Refresh barriers view
                _barriersLayer.LayerContext = App.Current.CurrentDate;

                // Set extent to barriers, active on current day
                ICollection<Barrier> barriers =
                    App.Current.Project.Barriers.Search(App.Current.CurrentDate);

                // Add all geometry points to extent.
                var points = new List<AppGeometry.Point>();
                foreach (Barrier barrier in barriers)
                {
                    AppGeometry.Point? point = barrier.Geometry as AppGeometry.Point?;
                    if (point != null)
                    {
                        points.Add(point.Value);
                    }
                    else
                    {
                        AppGeometry.PolyCurve polyCurve = barrier.Geometry as AppGeometry.PolyCurve;
                        if (polyCurve != null)
                        {
                            AppGeometry.Point[] polyCurvePoints = polyCurve.GetPoints(0, polyCurve.TotalPointCount);
                            points.AddRange(polyCurvePoints);
                        }
                    }
                }

                MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
            }
        }

        /// <summary>
        /// React on insertion row initialized.
        /// </summary>
        /// <param name="sender">Data grid control insertion row.</param>
        /// <param name="e">Ignored.</param>
        private void _InsertionRowInitialized(object sender, EventArgs e)
        {
            _insertionRow = sender as InsertionRow;
        }

        /// <summary>
        /// Occurs when application close. Stores layout.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ApplicationExit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        /// <summary>
        /// Occurs when page loads. Inits page if need. Updates selection status.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BarrierPageLoaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled +=
                new EventHandler(_BarriersPageNavigationCalled);

            if (!_isLayoutLoaded)
                _InitDataGridLayout();
            if (!_isDataGridCollectionInited)
                _InitCollection();

            if (!_isInited)
                _Init();

            _needToUpdateStatus = true;

            // Set label as status bar content.
            _SetSelectionStatus(XceedGrid.Items.Count);

            // Set startup extent.
            IEnumerable collectionInExtent;

            var barriers = (AppData.IDataObjectCollection<Barrier>)_collectionSource.Source;
            // if barriers present - set extent on them. Otherwise on locations.
            if (0 < barriers.Count)
            {
                collectionInExtent = barriers;
            }
            else
            {
                collectionInExtent = App.Current.Project.Locations;
            }

            List<AppGeometry.Point> points = MapExtentHelpers.GetPointsInExtent(collectionInExtent);
            AppGeometry.Envelope? extent = MapExtentHelpers.GetCollectionExtent(points);
            mapCtrl.StartupExtent = extent;

            // if new project loaded update extent
            if (_newProjectLoaded)
            {
                MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
                _newProjectLoaded = false;
            }
        }

        /// <summary>
        /// Occurs when navigation called - stops editing in datagrid control.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BarriersPageNavigationCalled(object sender, EventArgs e)
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
            var barriers = (IDataObjectCollection<Barrier>)_collectionSource.Source;
            CanBeLeftValidator<Barrier>.ShowErrorMessagesInMessageWindow(barriers);
        }

        /// <summary>
        /// Occurs when data grid control collection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BarriersPageCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus(XceedGrid.Items.Count);
        }

        private void _BarriersPageSelectedCollectionChanged(object sender,
                                                            NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus(XceedGrid.Items.Count);
        }

        #endregion // Event handlers

        #region Data Object Editing Event Handlers

        /// <summary>
        /// Handler begins edit item (calls BeginEditItem method of appropriate ContextHandler).
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceBeginningEdit(object sender,
                                                                DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((AppData.DataObject)e.Item);
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

        /// <summary>
        /// Handler raises event about editing was started.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceEditBegun(object sender, DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((AppData.DataObject)e.Item);
            if (EditBegun != null)
                EditBegun(this, args);
        }

        /// <summary>
        /// Handler commits editing (calls CommitItem method of appropriate ContextHandler).
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCommittingEdit(object sender,
                                                                 DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((AppData.DataObject)e.Item);
            if (CommittingEdit != null)
                CommittingEdit(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                _regionsPage.OnCommittingEdit(e);

                App.Current.Project.Save();

                _SetSelectionStatus(((ICollection<Barrier>)e.CollectionView.SourceCollection).Count);
                IsEditingInProgress = false;
            }
            else
                e.Cancel = true;
        }

        /// <summary>
        /// Handler raise event about Editing was commited.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceEditCommitted(object sender,
                                                                DataGridItemEventArgs e)
        {
            BarriersDayStatusesManager.Instance.OnBarrierChanged();

            DataObjectEventArgs args = new DataObjectEventArgs((AppData.DataObject)e.Item);
            if (EditCommitted != null)
                EditCommitted(this, args);
        }

        /// <summary>
        /// Handler raises event about editing was cancelled.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceEditCanceled(object sender,
                                                               DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((AppData.DataObject)e.Item);
            if (EditCanceled != null)
                EditCanceled(this, args);

            _regionsPage.OnEditCanceled(e);
            _SetSelectionStatus(((ICollection<Barrier>)e.CollectionView.SourceCollection).Count);
        }

        /// <summary>
        /// Handler begin creating new item.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCreatingNewItem(object sender,
                                                                  DataGridCreatingNewItemEventArgs e)
        {
            // since barrier's start and end dates should be created in code we should also create
            // string fields there too.
            DateTime date = App.Current.CurrentDate.Date;
            Barrier barrier = CommonHelpers.CreateBarrier(date);

            e.NewItem = barrier;

            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((AppData.DataObject)e.NewItem);
            if (CreatingNewObject != null)
                CreatingNewObject(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                e.Handled = true;
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
            if (!string.IsNullOrEmpty((e.Item as Barrier).Name))
                return;

            // Get new item's name.
            var barriers = (IDataObjectCollection<Barrier>)_collectionSource.Source;
            (e.Item as Barrier).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                barriers, e.Item as Barrier, true);

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
                System.Windows.Threading.DispatcherPriority.Render, e);

            DataObjectEventArgs args = new DataObjectEventArgs((AppData.DataObject)e.Item);
            if (NewObjectCreated != null)
                NewObjectCreated(this, args);
        }

        /// <summary>
        /// Handler begin commiting new item (calls CommitNewItem method of appropriate ContextHandler).
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCommittingNewItem(object sender,
                                                                    DataGridCommittingNewItemEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((AppData.DataObject)e.Item);
            if (CommittingNewObject != null)
                CommittingNewObject(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
                var source = e.CollectionView.SourceCollection as ICollection<Barrier>;

                Barrier current = e.Item as Barrier;

                if (null == current.StartDate)
                    current.StartDate = App.Current.CurrentDate.Date;

                if (null == current.FinishDate)
                {
                    DateTime date = current.StartDate;
                    current.FinishDate = date.AddDays(1);
                }

                Project project = App.Current.Project;
                project.Barriers.Add(current);
                project.Save();

                e.Index = source.Count;
                e.NewCount = source.Count + 1;

                _regionsPage.OnNewItemCommitted(e);

                _SetSelectionStatus(((ICollection<Barrier>)e.CollectionView.SourceCollection).Count);
                IsEditingInProgress = false;
            }
            else
                e.Cancel = true;
        }

        /// <summary>
        /// Raises event about new item was commited.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceNewItemCommitted(object sender,
                                                                   DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((AppData.DataObject)e.Item);
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, args);
        }

        /// <summary>
        /// Raises event about new item was cancelled.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceNewItemCanceled(object sender,
                                                                  DataGridItemEventArgs e)
        {
            var args = new DataObjectEventArgs((AppData.DataObject)e.Item);
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, args);

            IsEditingInProgress = false;
            _SetSelectionStatus(((ICollection<Barrier>)e.CollectionView.SourceCollection).Count);
        }

        /// <summary>
        /// Raises event about new item is cancelling.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event args.</param>
        private void _DataGridCollectionViewSourceCancelingNewItem(object sender,
                                                                   DataGridItemHandledEventArgs e)
        {
            // set property to true if new item was created or to false if new item wasn't created
            // otherwise an InvalidOperationException will be thrown
            // (see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Inserting_Data.html)
            e.Handled = _isNewItemCreated;
            _regionsPage.OnNewItemCancelling(e);
            IsEditingInProgress = false;
            _SetSelectionStatus(((ICollection<Barrier>)e.CollectionView.SourceCollection).Count);
        }

        /// <summary>
        /// Handler cancels editing object.
        /// </summary>
        /// <param name="sender">Data grid control sender.</param>
        /// <param name="e">Item event erags.</param>
        private void _DataGridCollectionViewSourceCancelingEdit(object sender,
                                                                DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
            IsEditingInProgress = false;
            _SetSelectionStatus(((ICollection<Barrier>)e.CollectionView.SourceCollection).Count);
        }

        #endregion

        #region Private status helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets selection status.
        /// </summary>
        /// <param name="count">Selected elements count.</param>
        private void _SetSelectionStatus(int count)
        {
            if (_needToUpdateStatus)
            {
                _statusBuilder.FillSelectionStatus(count,
                                                   App.Current.FindString(OBJECT_TYPE_NAME_RSC),
                                                   XceedGrid.SelectedItems.Count,
                                                   this);
            }

            // TODO: CHECK THIS!!!
            _needToUpdateStatus = true;
        }

        /// <summary>
        /// Sets editing status.
        /// </summary>
        /// <param name="itemName">Current item name.</param>
        private void _SetEditingStatus(string itemName)
        {
            _statusBuilder.FillEditingStatus(itemName,
                                             App.Current.FindString(OBJECT_TYPE_NAME_RSC),
                                             this);
            _needToUpdateStatus = false;
        }

        /// <summary>
        /// Sets creating status.
        /// </summary>
        private void _SetCreatingStatus()
        {
            _statusBuilder.FillCreatingStatus(App.Current.FindString(OBJECT_TYPE_NAME_RSC), this);
            _needToUpdateStatus = false;
        }

        #endregion // Private status helpers

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Collection source key.
        /// </summary>
        private const string COLLECTION_SOURCE_KEY = "barriersSource";
        /// <summary>
        /// Column property name Name.
        /// </summary>
        private const string NAME_PROPERTY_STRING = "Name";
        /// <summary>
        /// Object type name resource.
        /// </summary>
        private const string OBJECT_TYPE_NAME_RSC = "Barrier";
        /// <summary>
        /// Page name.
        /// </summary>
        private const string PAGE_NAME = "Barriers";

        /// <summary>
        /// Geometry field name.
        /// </summary>
        private const string GEOMETRY_FIELD_NAME = "Geometry";

        /// <summary>
        /// Default column width.
        /// </summary>
        private const int DEFAULT_COLUMN_WIDTH = 150;

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page's insertion row
        /// </summary>
        private InsertionRow _insertionRow;

        /// <summary>
        /// Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem 
        /// </summary>
        private bool _isNewItemCreated;

        /// <summary>
        /// Flag shows whether layout loaded.
        /// </summary>
        private bool _isLayoutLoaded;

        /// <summary>
        /// Flag shows whether data grid collection inited.
        /// </summary>
        private bool _isDataGridCollectionInited;

        /// <summary>
        /// Is page inited.
        /// </summary>
        private bool _isInited;

        /// <summary>
        /// Layer to show zones on map.
        /// </summary>
        private ObjectLayer _barriersLayer;

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
