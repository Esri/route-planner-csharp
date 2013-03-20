using System;
using System.Windows;
using System.Windows.Media;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.App.GridHelpers;

using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;
using ESRI.ArcLogistics.App.Help;
using System.Windows.Threading;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for Drivers.xaml
    /// </summary>
    internal partial class DriversPage : PageBase, ISupportSelection, ISupportDataObjectEditing, ICancelDataObjectEditing, ISupportSelectionChanged
    {
        public const string PAGE_NAME = "Drivers";

        #region Constructors

        public DriversPage()
        {
            InitializeComponent();
            _InitEventHandlers();
            _SetDefaults();
            _CheckPageComplete();
            
            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController(XceedGrid);
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

        public System.Collections.IList SelectedItems
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
                if (!(item is Driver))
                    throw new ArgumentException("DriversTypeExceptionMessage");
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

        #region Page Overrided Members

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("DriversPageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("DriversBrush");
                return brush;
            }
        }

        #endregion

        #region PageBase overrided members

        /// <summary>
        /// Saves page layout.
        /// </summary>
        internal override void SaveLayout()
        {
            if (Properties.Settings.Default.DriversGridSettings == null)
            {
                Properties.Settings.Default.DriversGridSettings = new SettingsRepository();
            }

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.DriversGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.DriversPagePath); }
        }

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
                    return base.CanBeLeft && CanBeLeftValidator<Driver>.IsValid(App.Current.Project.Drivers);
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Checks page complete status
        /// </summary>
        protected void _CheckPageComplete()
        {
            Project project = App.Current.Project;
            IsComplete = (project != null && project.Drivers.Count > 0);
        }

        /// <summary>
        /// Method inits collection of drivers.
        /// </summary>
        protected void _InitDataGridCollection()
        {
            try
            {
                DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);


                IDataObjectCollection<Driver> collection = (IDataObjectCollection<Driver>)App.Current.Project.Drivers;
                SortedDataObjectCollection<Driver> sortedDriversCollection = new SortedDataObjectCollection<Driver>(collection, new CreationTimeComparer<Driver>());
                collectionSource.Source = sortedDriversCollection;

                _isDataGridCollectionInited = true;
            }
            catch (Exception ex)
            {
                _isDataGridCollectionInited = false;
                Logger.Info(ex);
            }
        }

        protected void _InitDataGridLayout()
        {
            DataGridCollectionViewSource collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.DriversGridStructure);

            structureInitializer.BuildGridStructure(collectionSource, XceedGrid);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.DriversSettingsRepositoryName, collectionSource.ItemProperties);
            layoutLoader.LoadLayout(XceedGrid);

            _isDataGridLayoutLoaded = true;
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
            commandButtonGroup.Initialize(CategoryNames.DriversCommands, XceedGrid);
        }

        /// <summary>
        /// Method init event handlers
        /// </summary>
        protected void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(DriversPage_Loaded);
            this.Unloaded += new RoutedEventHandler(DriversPage_Unloaded);
            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(App_ProjectClosing);
            App.Current.Exit += new ExitEventHandler(Current_Exit);

            Project project = App.Current.Project;
            if (null != project)
            {
                project.Drivers.CollectionChanged += new NotifyCollectionChangedEventHandler(Drivers_CollectionChanged);
                _projectEventsAttached = true;
            }

            XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(XceedGrid_SelectionChanged);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when project loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_ProjectLoaded(object sender, EventArgs e)
        {
            _InitDataGridLayout();
            _InitDataGridCollection();

            if (!_projectEventsAttached)
            {
                App.Current.Project.Drivers.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Drivers_CollectionChanged);
                _projectEventsAttached = true;
            }
            _CheckPageComplete();
        }

        /// <summary>
        /// Occurs when project start closing.
        /// </summary>
        private void App_ProjectClosing(object sender, EventArgs e)
        {
            if (_projectEventsAttached)
            {
                App.Current.Project.Drivers.CollectionChanged -= Drivers_CollectionChanged;
                _projectEventsAttached = false;
            }

            SaveLayout();
        }

        /// <summary>
        /// Occurs when page loads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriversPage_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).NavigationCalled += new EventHandler(DriversPage_NavigationCalled);

            if (!_isDataGridLayoutLoaded)
                _InitDataGridLayout();
            if (!_isDataGridCollectionInited)
                _InitDataGridCollection();
            _CheckPageComplete();

            _needToUpdateStatus = true;
            _SetSelectionStatus();
        }

        private void Drivers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _CheckPageComplete();
            _SetSelectionStatus();
        }

        private void DriversPage_NavigationCalled(object sender, EventArgs e)
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
            CanBeLeftValidator<Driver>.ShowErrorMessagesInMessageWindow(App.Current.Project.Drivers);
        }

        private void DriversPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).NavigationCalled -= DriversPage_NavigationCalled;

            this.CancelObjectEditing();
        }

        private void InsertionRow_Initialized(object sender, EventArgs e)
        {
            _InsertionRow = sender as InsertionRow;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        private void ImportDriver_Click(object sender, RoutedEventArgs e)
        {
            ImportDriversCmd cmd = new ImportDriversCmd();
            cmd.Execute();
        }

        private void XceedGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            _SetSelectionStatus();

            // NOTE : event raises to notify all necessary object about selection was changed. Added because XceedGrid.SelectedItems doesn't implement INotifyCollectionChanged
            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);
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

            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CreatingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new Driver();
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.NewItem);
            if (CreatingNewObject != null)
                CreatingNewObject(this, args);

            e.Handled = true;

            if (!args.Cancel)
            {
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
            if (!string.IsNullOrEmpty((e.Item as Driver).Name))
                return;

            // Get new item's name.
            (e.Item as Driver).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                App.Current.Project.Drivers, e.Item as Driver, true);

            // Find TextBox inside the cell and select new name.
            Cell currentCell = _InsertionRow.Cells[XceedGrid.CurrentContext.CurrentColumn];
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        private void DataGridCollectionViewSource_NewItemCreated(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
           // Invoking changing of the item's name. Those method must be invoked, otherwise 
           // grid will not understand that item in insertion ro was changed and grid wouldnt allow
           // to commit this item.
           Dispatcher.BeginInvoke(new Action<Xceed.Wpf.DataGrid.DataGridItemEventArgs>(_ChangeName),
               DispatcherPriority.DataBind, e);

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
                ICollection<Driver> source = e.CollectionView.SourceCollection as ICollection<Driver>;

                Driver currentDriver = e.Item as Driver;
                source.Add(currentDriver);

                e.Index = source.Count - 1;
                e.NewCount = source.Count;

                App.Current.Project.Save();

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

        #region Private Status Helpers

        /// <summary>
        /// Method sets selection status
        /// </summary>
        private void _SetSelectionStatus()
        {
            if (_needToUpdateStatus)
                _statusBuilder.FillSelectionStatus(App.Current.Project.Drivers.Count, (string)App.Current.FindResource(OBJECT_TYPE_NAME), XceedGrid.SelectedItems.Count, this);

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

        #region Private Fields

        protected const string COLLECTION_SOURCE_KEY = "driversCollection";
        protected const string NAME_PROPERTY_STRING = "Name";
        protected const string OBJECT_TYPE_NAME = "Driver";

        InsertionRow _InsertionRow;

        private StatusBuilder _statusBuilder = new StatusBuilder();

        // Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem 
        bool _isNewItemCreated = false;
        private bool _isDataGridCollectionInited = false;
        private bool _isDataGridLayoutLoaded = false;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus = false;

        /// <summary>
        /// Project events attached flag.
        /// </summary>
        private bool _projectEventsAttached = false;

        #endregion
    }
}
