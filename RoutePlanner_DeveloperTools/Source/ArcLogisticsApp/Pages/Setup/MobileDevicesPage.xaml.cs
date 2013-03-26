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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Services;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Microsoft.Win32;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for MobileDevices.xaml
    /// </summary>
    internal partial class MobileDevicesPage : PageBase, ISupportDataObjectEditing, ISupportSelection, ICancelDataObjectEditing, ISupportSelectionChanged
    {
        public const string PAGE_NAME = "MobileDevices";

        #region Constructors

        public MobileDevicesPage()
        {
            InitializeComponent();
            _InitEventHandlers();
            _SetDefaults();
            _FillDevicesList();
            commandButtonGroup.Initialize(CategoryNames.MobileDevicesCommands, XceedGrid);

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
                if (!(item is MobileDevice))
                    throw new ArgumentException("MobileDevicesTypeExceptionMessage");
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
            get { return (string)App.Current.FindResource("MobileDevicesPageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("MobileDevicesBrush");
                return brush;
            }
        }

        #endregion

        #region PageBase overrided members

        internal override void SaveLayout()
        {
            if (Properties.Settings.Default.MobileDevicesGridSettings == null)
                Properties.Settings.Default.MobileDevicesGridSettings = new SettingsRepository();

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.MobileDevicesGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.MobileDevicesPagePath); }
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
                    return base.CanBeLeft && CanBeLeftValidator<MobileDevice>.
                    	IsValid(App.Current.Project.MobileDevices);
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion

        #region Protected methods

        public IList<string> Devices
        {
            get { return _devices; }
        }

        protected void _InitDataGridCollection()
        {
            Project project = App.Current.Project;
            if (project == null)
                _isDataGridCollectionInited = false;
            else
            {
                DataGridCollectionViewSource mobileDevicesCollection = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

                IDataObjectCollection<MobileDevice> collection = (IDataObjectCollection<MobileDevice>)project.MobileDevices;
                SortedDataObjectCollection<MobileDevice> sortedMobileDevicesCollection = new SortedDataObjectCollection<MobileDevice>(collection, new CreationTimeComparer<MobileDevice>());
                mobileDevicesCollection.Source = sortedMobileDevicesCollection;

                ((INotifyCollectionChanged)XceedGrid.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(MobileDevicesPage_CollectionChanged);

                _isDataGridCollectionInited = true;
            }
        }

        private void _InitDataGridLayout()
        {
            DataGridCollectionViewSource mobileDevicesCollection = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);
            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.MobileDevicesGridStructure);

            structureInitializer.BuildGridStructure(mobileDevicesCollection, XceedGrid);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.MobileDevicesSettingsRepositoryName, mobileDevicesCollection.ItemProperties);
            layoutLoader.LoadLayout(XceedGrid);

            _isDataGridLayoutLoaded = true;
        }

        protected void _FillDevicesList()
        {
            RegistryKey regKey = null;
            try
            {
                regKey = Registry.CurrentUser.OpenSubKey(MobileDevice.PARTNERS_KEY_PATH);
                if (regKey != null)
                {
                    string[] partnerNames = regKey.GetSubKeyNames();
                    foreach (string partnerName in partnerNames)
                    {
                        string partnerKey = System.IO.Path.Combine(MobileDevice.PARTNERS_KEY_PATH, partnerName);
                        RegistryKey partnerRegKey = Registry.CurrentUser.OpenSubKey(partnerKey);
                        if (partnerRegKey != null)
                        {
                            string partnerDisplayName = (string)partnerRegKey.GetValue(MobileDevice.PARTNERS_DISPLAYNAME);
                            if (partnerDisplayName != null)
                                _devices.Add(partnerDisplayName);
                        }
                        partnerRegKey.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                if (regKey != null)
                    regKey.Close();
            }
        }

        /// <summary>
        /// Method fills page's properties by default values
        /// </summary>
        protected void _SetDefaults()
        {
            IsRequired = false;
            IsAllowed = true;
            CanBeLeft = true;
            commandButtonGroup.Initialize(CategoryNames.MobileDevicesCommands, XceedGrid);
        }

        /// <summary>
        /// Method init event handlers
        /// </summary>
        protected void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(MobileDevicesPage_Loaded);
            this.Unloaded += new RoutedEventHandler(MobileDevicesPage_Unloaded);
            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.ProjectClosing += new EventHandler(App_ProjectClosing);
            App.Current.Exit += new ExitEventHandler(Current_Exit);
            XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(XceedGrid_SelectionChanged);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Occurs when user changes any property in mobile device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void device_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // if any property except "SyncType" was changed - return
            if (e.PropertyName != MobileDevice.PropertyNameSyncType)
                return;

            DataRow row = null; // edited row

            if (_InsertionRow.IsBeingEdited)
                row = _InsertionRow; // if insertion row is in editing - use insertion row as current row
            else
                row = XceedGrid.GetContainerFromItem(XceedGrid.CurrentItem) as DataRow; // otherwise - get current row by current item

            Debug.Assert(row != null);

            MobileDevice device = (MobileDevice)sender; // edited mobile device
            Debug.Assert(device != null);

            // update value in field corresponding to sync type for show validation
            switch (device.SyncType)
            {
                case SyncType.None:
                    break;
                case SyncType.EMail:
                    device.EmailAddress = (string)row.Cells[MobileDevice.PropertyNameEmailAddress].Content;
                    break;
                case SyncType.ActiveSync:
                    device.ActiveSyncProfileName = (string)row.Cells[MobileDevice.PropertyNameActiveSyncProfileName].Content;
                    break;
                case SyncType.Folder:
                    device.SyncFolder = (string)row.Cells[MobileDevice.PropertyNameSyncFolder].Content;
                    break;
                case SyncType.WMServer:
                    device.TrackingId = (string)row.Cells[MobileDevice.PropertyNameName].Content;
                    break;
                default:
                    break;
            }
        }

        private void App_ProjectClosing(object sender, EventArgs e)
        {
            SaveLayout();
        }

        private void App_ProjectLoaded(object sender, EventArgs e)
        {
            _InitDataGridLayout();
            _InitDataGridCollection();
        }

        private void MobileDevicesPage_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).NavigationCalled += new EventHandler(MobileDevicesPage_NavigationCalled);

            if (!_isDataGridLayoutLoaded)
                _InitDataGridLayout();
            if (!_isDataGridCollectionInited)
                _InitDataGridCollection();

            var tracker = App.Current.Tracker;
            if (tracker != null && tracker.InitError == null)
            {
                var synchronizationService = tracker.SynchronizationService;
                var workingStatusController = new ApplicationWorkingStatusController();
                var exceptionHandler = new TrackingServiceExceptionHandler();

                _devicesEditor = new MobileDevicesEditor(
                    synchronizationService,
                    workingStatusController,
                    exceptionHandler,
                    _app.Project);
            }

            _needToUpdateStatus = true;
            _SetSelectionStatus();
        }

        private void MobileDevicesPage_NavigationCalled(object sender, EventArgs e)
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
            CanBeLeftValidator<MobileDevice>.ShowErrorMessagesInMessageWindow(App.Current.Project.MobileDevices);
        }

        private void InsertionRow_Initialized(object sender, EventArgs e)
        {
            _InsertionRow = sender as InsertionRow;
        }

        private void MobileDevicesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ((MainWindow)App.Current.MainWindow).NavigationCalled -= MobileDevicesPage_NavigationCalled;
            this.CancelObjectEditing();
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        private void MobileDevicesPage_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus();
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

            var currentDevice = (MobileDevice)e.Item;

            if (_devicesEditor != null)
            {
                _devicesEditor.BeginEditing(currentDevice);
            }

            // add handler to property changed event when user starts editing
            _SubscribeToMobileDevicePropertyChanged((MobileDevice)e.Item);
        }

        private void DataGridCollectionViewSource_CommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingEdit != null)
                CommittingEdit(this, args);

            e.Handled = true;

            if (args.Cancel)
            {
                e.Cancel = true;

                return;
            }

            if (_devicesEditor != null)
            {
                _devicesEditor.FinishEditing();
            }

            if (App.Current.Project != null)
                App.Current.Project.Save();

            _SetSelectionStatus();
            IsEditingInProgress = false;
        }

        private void DataGridCollectionViewSource_EditCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditCommitted != null)
                EditCommitted(this, args);

            // remove handler to property changed event when editing is commited
            _UnsubscribeFromMobileDevicePropertyChanged((MobileDevice)e.Item);
        }

        private void DataGridCollectionViewSource_EditCanceled(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (EditCanceled != null)
                EditCanceled(this, args);

            // remove handler to property changed event when editing is cancelled
            _UnsubscribeFromMobileDevicePropertyChanged((MobileDevice)e.Item);
            _SetSelectionStatus();
        }

        private void DataGridCollectionViewSource_CreatingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new MobileDevice();
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
            if (!string.IsNullOrEmpty((e.Item as MobileDevice).Name))
                return;

            // Get new item's name.
            (e.Item as MobileDevice).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                App.Current.Project.MobileDevices, e.Item as MobileDevice, true);

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
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeName),
                System.Windows.Threading.DispatcherPriority.Render, e);

            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCreated != null)
                NewObjectCreated(this, args);

            // when user creates new item add handler to property changed event for show validation
            _SubscribeToMobileDevicePropertyChanged((MobileDevice)e.Item);
        }

        private void DataGridCollectionViewSource_CommittingNewItem(object sender, Xceed.Wpf.DataGrid.DataGridCommittingNewItemEventArgs e)
        {
            DataObjectCanceledEventArgs args = new DataObjectCanceledEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (CommittingNewObject != null)
                CommittingNewObject(this, args);

            e.Handled = true;

            if (args.Cancel)
            {
                e.Cancel = true;

                return;
            }

            ICollection<MobileDevice> source = e.CollectionView.SourceCollection as ICollection<MobileDevice>;

            MobileDevice currentMobileDevice = e.Item as MobileDevice;
            source.Add(currentMobileDevice);

            if (_devicesEditor != null)
            {
                _devicesEditor.AddDevice(currentMobileDevice);
            }

            e.Index = source.Count - 1;
            e.NewCount = source.Count;

            App.Current.Project.Save();

            _SetSelectionStatus();
            IsEditingInProgress = false;
        }

        private void DataGridCollectionViewSource_NewItemCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCommitted != null)
                NewObjectCommitted(this, args);

            // remove handler to property changed event when new item is commited
            _UnsubscribeFromMobileDevicePropertyChanged((MobileDevice)e.Item);
        }

        private void DataGridCollectionViewSource_NewItemCanceled(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            DataObjectEventArgs args = new DataObjectEventArgs((ESRI.ArcLogistics.Data.DataObject)e.Item);
            if (NewObjectCanceled != null)
                NewObjectCanceled(this, args);

            // remove handler to property changed event when new item is cancelled
            _UnsubscribeFromMobileDevicePropertyChanged((MobileDevice)e.Item);
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
                _statusBuilder.FillSelectionStatus(App.Current.Project.MobileDevices.Count, (string)App.Current.FindResource(OBJECT_TYPE_NAME), XceedGrid.SelectedItems.Count, this);

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

        #region Private Subscribe Helpers

        /// <summary>
        /// Adds handler for PropertyChanged event from Mobile Device
        /// </summary>
        /// <param name="device"></param>
        private void _SubscribeToMobileDevicePropertyChanged(MobileDevice device)
        {
            device.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(device_PropertyChanged);
        }

        /// <summary>
        /// Removes handler for PropertyChanged event from Mobile Device
        /// </summary>
        /// <param name="device"></param>
        private void _UnsubscribeFromMobileDevicePropertyChanged(MobileDevice device)
        {
            device.PropertyChanged -= device_PropertyChanged;
        }

        #endregion

        #region Private fields

        protected const string COLLECTION_SOURCE_KEY = "mdCollection";
        protected const string NAME_PROPERTY_STRING = "Name";
        protected const string OBJECT_TYPE_NAME = "MobileDevice";

        private InsertionRow _InsertionRow;

        private StatusBuilder _statusBuilder = new StatusBuilder();

        private List<string> _devices = new List<string>();

        // Flag shows is new item was created or not to set correct value of Handled property in DataGridCollectionViewSource_CancelingNewItem 
        bool _isNewItemCreated = false;
        private bool _isDataGridCollectionInited = false;
        private bool _isDataGridLayoutLoaded = false;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus = false;

        /// <summary>
        /// Handles mobile devices editing performing necessary synchronizations with tracking
        /// service.
        /// </summary>
        private MobileDevicesEditor _devicesEditor;
        #endregion
    }
}
