using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Controls
{   
    /// <summary>
    /// Class CustomOrderPropertiesControl represents custom order properties control
    /// which contains Xceed DataGrid control keeping list of custom order properties and "Delete" button.
    /// List of custom order properties is serialized to XML and stored to database therefore
    /// it should not violate maximum length constraint equal to 2000 symbols.
    /// Interaction logic for CustomOrderPropertiesControl.xaml.
    /// </summary>
    internal partial class CustomOrderPropertiesControl : UserControl
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of CustomOrderPropertiesControl.
        /// </summary>
        public CustomOrderPropertiesControl()
        {
            InitializeComponent();

            _InitDataGridLayout();

            // Create custom order property name validator.
            _customOrderPropertyNameValidator = new CustomOrderPropertyNameValidator(_customOrderProperties);

            // Attach handler for the Xceed DataGrid SelectionChanged event.
            _customOrderPropertiesXceedGrid.SelectionChanged +=
                new DataGridSelectionChangedEventHandler(_CustomOrderPropertiesXceedGridSelectionChanged);

            // Attach handler for the Xceed DataGrid KeyDown event.
            _customOrderPropertiesXceedGrid.KeyDown += new KeyEventHandler(_CustomOrderPropertiesXceedGridKeyDown);

            // Attach handler for the Xceed IsVisibleChanged event.
            _customOrderPropertiesXceedGrid.IsVisibleChanged +=
                new DependencyPropertyChangedEventHandler(_CustomOrderPropertiesXceedGridIsVisibleChanged);

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController(_customOrderPropertiesXceedGrid);
        }

        #endregion Constructors

        #region Public properties

        /// <summary>
        /// Gets or sets availability of this control (Xceed DataGrid and Delete button).
        /// </summary>
        new public bool IsEnabled
        {
            get { return base.IsEnabled; }

            set
            {
                base.IsEnabled = value;

                // Set availability of Xceed DataGrid.
                _customOrderPropertiesXceedGrid.IsEnabled = value;

                // Delete button has additional condition of availability therefore
                // it's availability is not set to true together with availability of this control.
                if (!value)
                    _buttonDelete.IsEnabled = false;

                // To disable this control completely we show or hide special overlapping Locked Grid panel.
                _lockedGrid.Visibility = value ? Visibility.Hidden : Visibility.Visible;
            }
        }

        #endregion Public properties

        #region Public methods

        /// <summary>
        /// Loads custom order properties from collection OrderCustomPropertiesInfo.
        /// </summary>
        /// <param name="customPropertiesInfo">OrderCustomPropertiesInfo collection.</param>
        public void LoadCustomOrderProperties(OrderCustomPropertiesInfo customPropertiesInfo)
        {
            Debug.Assert(customPropertiesInfo != null);

            // Clear collection.
            _customOrderProperties.Clear();

            foreach (OrderCustomProperty orderCustomProperty in customPropertiesInfo)
            {
                // Create custom order property using data of item in collection.
                CustomOrderProperty newOrderProperty = 
                    new CustomOrderProperty(orderCustomProperty.Name, 
                                            orderCustomProperty.Description,
                                            orderCustomProperty.Length,
                                            orderCustomProperty.OrderPairKey);

                // Add new custom order property to collection.
                _customOrderProperties.Add(newOrderProperty);
            }

            // Backup collection of custom order properties.
            _BackupCustomOrderPropertiesCollection();
        }

        /// <summary>
        /// Converts collection of CustomOrderProperty objects to OrderCustomPropertiesInfo.
        /// </summary>
        /// <returns>OrderCustomPropertiesInfo object.</returns>
        public OrderCustomPropertiesInfo GetOrderCustomPropertiesInfo()
        {
            OrderCustomPropertiesInfo orderCustomPropertiesInfo = new OrderCustomPropertiesInfo();

            foreach (CustomOrderProperty customOrderProperty in _customOrderProperties)
            {
                // Create custom order property info using data of item in collection.
                OrderCustomProperty newOrderCustomProperty =
                    new OrderCustomProperty(customOrderProperty.Name,
                                            OrderCustomPropertyType.Text,
                                            customOrderProperty.MaximumLength,
                                            customOrderProperty.Description,
                                            customOrderProperty.OrderPairKey);

                // Add new custom order property to collection.
                orderCustomPropertiesInfo.Add(newOrderCustomProperty);
            }

            return orderCustomPropertiesInfo;
        }

        /// <summary>
        /// Checks if collection of custom order properties was modified since the last backup.
        /// </summary>
        /// <returns>True - if collection was modified, otherwise - false.</returns>
        public bool CustomOrderPropertiesModified()
        {
            return !_CompareCustomOrderPropertiesCollections(_customOrderProperties,
                                                             _customOrderPropertiesBackup);
        }

        /// <summary>
        /// Function cheks that collection of custom order properties contains valid data and
        /// this collection doesn't violate maximum length constraint.
        /// </summary>
        /// <returns></returns>
        public bool DataIsValid()
        {
            bool dataValidity =
                _CustomOrderPropertiesCollectionIsValid() &&
                _CheckMaximumLengthConstraint();

            return dataValidity;
        }

        /// <summary>
        /// Shows error messages (related to collection of custom order properties) im message window.
        /// These messages describe what is wrong with collection of custom order properties.
        /// </summary>
        public void ShowErrorMessagesInMessageWindow()
        {
            List<MessageDetail> details = new List<MessageDetail>();

            // Check if custom order properties have errors.
            foreach (CustomOrderProperty orderProperty in _customOrderProperties)
            {
                string errorString = orderProperty.Error;

                // If it has - add new MessageDetail.
                if (!string.IsNullOrEmpty(errorString))
                    details.Add(new MessageDetail(MessageType.Warning, errorString));
            }

            // Check maximum length constraint of custom order properties.
            if (!_CheckMaximumLengthConstraint())
            {
                string maxLengthViolationErrorMessage =
                    (string)App.Current.FindResource("CustomOrderPropertiesMaxLengthViolationMessage");

                details.Add(new MessageDetail(MessageType.Warning, maxLengthViolationErrorMessage));
            }

            // If we have MessageDetails add new Message to message window.
            if (details.Count > 0)
            {
                string errorMessage = ((string)App.Current.
                    FindResource("SetupPanelValidationError"));
                App.Current.Messenger.AddMessage(MessageType.Warning, errorMessage, details);
            }
        }

        #endregion Public methods

        #region Delegates

        /// <summary>
        /// Delegate ParamsDelegate.
        /// </summary>
        /// <param name="item"></param>
        private delegate void ParamsDelegate(DataGridItemEventArgs item);

        #endregion Delegates

        #region Event handlers

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.InsertionRow.Initialized event.
        /// </summary>
        /// <param name="sender">Insertion row.</param>
        /// <param name="e">Event arguments (ignored).</param>
        private void _InsertionRowInitialized(object sender, EventArgs e)
        {
            _insertionRow = sender as InsertionRow;
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.CreatingNewItem event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceCreatingNewItem(object sender, DataGridCreatingNewItemEventArgs e)
        {
            // Create new custom order property.
            e.NewItem = new CustomOrderProperty();

            // Set flag indicating that new object is being edited now.
            _isEdititngInProgress = true;

            // Make the "Delete" button enabled.
            _buttonDelete.IsEnabled = true;

            // Set flag indicating that event was handled.
            e.Handled = true;
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.NewItemCreated event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceNewItemCreated(object sender, DataGridItemEventArgs e)
        {
            // Invoking changing of the item's name. Those method must be invoked, otherwise 
            // grid will not understand that item in insertion ro was changed and grid wouldnt allow
            // to commit this item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeName),
                System.Windows.Threading.DispatcherPriority.Render, e);
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.EditCommitted event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceEditCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            _RevalidateCustomOrderPropertiesCollection();
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.CommittingNewItem event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceCommittingNewItem(object sender, DataGridCommittingNewItemEventArgs e)
        {
            // Get collection of custom order properties bound to the Xceed DataGrid control.
            ICollection<CustomOrderProperty> source =
                e.CollectionView.SourceCollection as ICollection<CustomOrderProperty>;

            // Get custom order property which should be committed to collection.
            CustomOrderProperty currentProperty = e.Item as CustomOrderProperty;

            // Add custom order property to collection.
            source.Add(currentProperty);

            // Set index of new item in collection.
            e.Index = source.Count - 1;

            // Set new count of items in collection.
            e.NewCount = source.Count;

            // Set flag indicating that event was handled.
            e.Handled = true;
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.CancelingNewItem event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceCancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            _isEdititngInProgress = false;

            // Make the "Delete" button disabled.
            _buttonDelete.IsEnabled = false;

            // Manually handling the insertion of new items requires that the CreatingNewItem,
            // CommitingNewItem, and CancelingNewItem events must all be handled even if nothing
            // is done in the event.
            // Set property to true otherwise an InvalidOperationException will be thrown 
            // (see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Inserting_Data.html).
            e.Handled = true;
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.NewItemCommitted event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceNewItemCommitted(object sender, Xceed.Wpf.DataGrid.DataGridItemEventArgs e)
        {
            // When new item is commited we should revalidate the whole collection
            // to check that items have unique names.
            _RevalidateCustomOrderPropertiesCollection();
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.BeginningEdit event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceBeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            _isEdititngInProgress = true;

            _buttonDelete.IsEnabled = true;
        }

        /// <summary>
        /// Handler for the Xceed.Wpf.DataGrid.DataGridCollectionViewSource.CommittingEdit event.
        /// </summary>
        /// <param name="sender">DataGridCollectionViewSource object.</param>
        /// <param name="e">Event arguments.</param>
        private void _DataGridCollectionViewSourceCommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            _isEdititngInProgress = false;
        }

        /// <summary>
        /// Handler for the SelectionChanged event of Xceed DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void _CustomOrderPropertiesXceedGridSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            // Bring selected item into view.
            if (e.SelectionInfos[0] != null && e.SelectionInfos[0].AddedItems.Count > 0)
                _customOrderPropertiesXceedGrid.BringItemIntoView(e.SelectionInfos[0].AddedItems[0]);

            // If selection is not empty make the Delete button available.
            _UpdateDeleteButtonAvailability();
        }

        /// <summary>
        /// Handler for the KeyDown event of Xceed DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void _CustomOrderPropertiesXceedGridKeyDown(object sender, KeyEventArgs e)
        {
            // If "Delete" key is down.
            if (e.Key == Key.Delete)
            {
                _ExecuteDeleteCommand();
            }
        }

        /// <summary>
        /// Handler for the IsVisibleChanged event of Xceed DataGrid.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void _CustomOrderPropertiesXceedGridIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_isEdititngInProgress)
            {
                _customOrderPropertiesXceedGrid.EndEdit();

                _isEdititngInProgress = false;
            }
        }

        /// <summary>
        /// Handler for event IsEnabledChanged of "Delete" button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void _ButtonDeleteIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string enabledTooltip = null;
            string disabledTooltip = null;

            Button button = (Button)sender;

            Debug.Assert(button == _buttonDelete);

            // Tool tip for the button when it is enabled.
            enabledTooltip = (string)App.Current.FindResource("DeleteCommandEnabledTooltip");

            // Tool tip for the button when it is disabled.
            disabledTooltip = (string)App.Current.FindResource("DeleteCommandDisabledTooltip");

            // Set appropriate tool tip for the button.
            if (button.IsEnabled)
                button.ToolTip = enabledTooltip;
            else
                button.ToolTip = disabledTooltip;
        }

        /// <summary>
        /// Handler for event Click of "Delete" button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void _ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            _ExecuteDeleteCommand();
        }

        #endregion Event handlers

        #region Private methods

        /// <summary>
        /// Method loads layout of Custom order properties data grid.
        /// </summary>
        private void _InitDataGridLayout()
        {
            // Get Data grid collection view source.
            DataGridCollectionViewSource gridCollectionViewSource =
                (DataGridCollectionViewSource)LayoutRoot.FindResource(CUSTOM_ORDER_PROPERTIES_COLLECTION_SOURCE_KEY);

            // Bind collection of custom order properties to Xceed DataGrid.
            gridCollectionViewSource.Source = _customOrderProperties;

            //Grid structure file name.
            string gridStructureFileName = GridSettingsProvider.CustomOrderPropertiesGridStructure;

            // Grid structure initializer.
            GridStructureInitializer gridStructureInitializer =
                new GridStructureInitializer(gridStructureFileName);

            // Build structure of Xceed DataGrid.
            gridStructureInitializer.BuildGridStructure(gridCollectionViewSource, _customOrderPropertiesXceedGrid);

            // Grid settings repository name.
            string gridSettingsRepositoryName =
                GridSettingsProvider.CustomOrderPropertiesSettingsRepositoryName;

            // Grid layout loader.
            GridLayoutLoader gridLayoutLoader =
                new GridLayoutLoader(gridSettingsRepositoryName, gridCollectionViewSource.ItemProperties);

            // Load grid layout.
            gridLayoutLoader.LoadLayout(_customOrderPropertiesXceedGrid);
        }

        /// <summary>
        /// Changes item's name.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        private void _ChangeName(DataGridItemEventArgs e)
        {
            // Get custom order property from event's data.
            CustomOrderProperty orderProperty = e.Item as CustomOrderProperty;

            Debug.Assert(orderProperty != null);

            // Check that item's name is null.
            if (!string.IsNullOrEmpty(orderProperty.Name))
                return;

            // Get new item's name.
            orderProperty.Name =
                DataObjectNamesConstructor.GetNewNameForCustomOrderProperty(_customOrderProperties, true);

            // Get current cell.
            Cell currentCell =
                _insertionRow.Cells[_customOrderPropertiesXceedGrid.CurrentContext.CurrentColumn];

            // Find TextBox inside the cell.
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);

             // Select contents of found text box.
            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Stores copy of _customOrderProperties collection: _customOrderPropertiesBackup.
        /// </summary>
        private void _BackupCustomOrderPropertiesCollection()
        {
            _customOrderPropertiesBackup.Clear();

            foreach (CustomOrderProperty item in _customOrderProperties)
            {
                // Create a clone of item.
                CustomOrderProperty itemClone = (CustomOrderProperty)item.Clone();

                // Add copy of item into backup collection.
                _customOrderPropertiesBackup.Add(itemClone);
            }
        }

        /// <summary>
        /// Compares to collections of custom order properties.
        /// </summary>
        /// <param name="collection1">First collection.</param>
        /// <param name="collection2">Second collection.</param>
        /// <returns>True - if collections contain identical custom order properties,
        /// otherwise - false.</returns>
        private bool _CompareCustomOrderPropertiesCollections(ObservableCollection<CustomOrderProperty> collection1,
                                                              ObservableCollection<CustomOrderProperty> collection2)
        {
            bool comparisonResult = false;

            // References are equal - so collections are equal.
            if (collection1 == collection2)
            {
                comparisonResult = true;
            }
            // If one reference to collection is null - the result is false.
            else if (collection1 == null || collection2 == null)
            {
                comparisonResult = false;
            }
            // If collections have different number of items - the result is false.
            else if (collection1.Count != collection2.Count)
            {
                comparisonResult = false;
            }
            // If collections have equal number of items.
            else if (collection1.Count == collection2.Count)
            {
                comparisonResult = true;

                int collectionSize = collection1.Count;

                // Compare items in collections.
                for (int i = 0; i < collectionSize && comparisonResult; i++)
                {
                    comparisonResult =
                        comparisonResult && collection1[i].CompareTo(collection2[i]);
                }
            }
            else
            {
                // This case should never happen.
                Debug.Assert(false);
                comparisonResult = false;
            }

            return comparisonResult;
        }

        /// <summary>
        /// Removes selected items from collection.
        /// </summary>
        private void _RemoveSelectedItems()
        {
            // List of selected items.
            IList selectedItems = _customOrderPropertiesXceedGrid.SelectedItems;

            // If selected items exist.
            if (selectedItems != null)
            {
                // Remove each selected item from collection of custom order properties.
                // Note that on deleting item from _customOrderProperties it is also deleted from 
                // _customOrderPropertiesXceedGrid.SelectedItems collection.
                bool deleteStatus = true;
                // Iterate while collection is not empty or delete operation is failed.
                while (selectedItems.Count > 0 && deleteStatus)
                {
                    IEnumerator enumerator = selectedItems.GetEnumerator();

                    // Position enumerator to the first item in collection.
                    enumerator.MoveNext();

                    // Get current item from collection.
                    CustomOrderProperty orderProperty = enumerator.Current as CustomOrderProperty;
                    Debug.Assert(orderProperty != null);

                    // Remove item from collection.
                    deleteStatus = _customOrderProperties.Remove(orderProperty);

                    // Actually each selected item should be deleted successfully.
                    Debug.Assert(deleteStatus);
                } // while

                selectedItems = null;
            } // if
        }

        /// <summary>
        /// Revalidate custom order properties collection.
        /// </summary>
        private void _RevalidateCustomOrderPropertiesCollection()
        {
            // Raise PropertyChanged event for "Name" property for each item in collection
            // to revalidate each custon order property name.
            foreach (CustomOrderProperty orderProperty in _customOrderProperties)
            {
                orderProperty.RaiseNamePropertyChangedEvent();
            }
        }

        /// <summary>
        /// Executes "Delete" command.
        /// </summary>
        private void _ExecuteDeleteCommand()
        {
            // If item is editing now - cancel editing.
            if (_isEdititngInProgress)
                _customOrderPropertiesXceedGrid.CancelEdit();

            // List of selected items.
            IList selectedItems = _customOrderPropertiesXceedGrid.SelectedItems;

            if (selectedItems != null && selectedItems.Count > 0)
            {
                bool processCommand = true;

                // Ask user if he or she really want's to delete selected item(s).
                if (Settings.Default.IsAllwaysAskBeforeDeletingEnabled)
                {
                    // Show warning dialog.
                    processCommand = DeletingWarningHelper.Execute(selectedItems);
                }

                // If delete operation should be performed.
                if (processCommand)
                {
                    // Remove selected items from collection.
                    _RemoveSelectedItems();

                    // Revalidate collection of custom order properties.
                    _RevalidateCustomOrderPropertiesCollection();
                }
            } // if (selectedItems != null && selectedItems.Count > 0)

            _UpdateDeleteButtonAvailability();
        }

        /// <summary>
        /// Updates availability of "Delete" button.
        /// </summary>
        private void _UpdateDeleteButtonAvailability()
        {
            _buttonDelete.IsEnabled =
                _isEdititngInProgress ||
                _customOrderPropertiesXceedGrid.SelectedItems.Count != 0;
        }

        /// <summary>
        /// Checks if collection of custom order properties is valid (each object in collection is valid).
        /// </summary>
        /// <returns>True - if collection is valid, otherwise - false.</returns>
        private bool _CustomOrderPropertiesCollectionIsValid()
        {
            bool validity = true;

            // Check each custom order property in collection for validity.
            for (int i = 0; i < _customOrderProperties.Count && validity; i++)
            {
                validity = validity && _customOrderProperties[i].IsValid();
            }

            return validity;
        }

        /// <summary>
        /// Checks maximum length constraint for list of custom order properties.
        /// </summary>
        /// <returns></returns>
        private bool _CheckMaximumLengthConstraint()
        {
            bool checkMaxLengthConstraint = false;

            // Get order custom properties info.
            OrderCustomPropertiesInfo propertiesInfo = GetOrderCustomPropertiesInfo();

            // Check constraint.
            checkMaxLengthConstraint = ProjectFactory.CheckMaximumLengthConstraint(propertiesInfo);

            return checkMaxLengthConstraint;
        }

        #endregion Private methods

        #region Private constants

        /// <summary>
        /// Xceed DataGrid's collection.
        /// </summary>
        private const string CUSTOM_ORDER_PROPERTIES_COLLECTION_SOURCE_KEY =
            "CustomOrderPropertiesDataGridColectionViewSource";

        /// <summary>
        /// Format of name for new object.
        /// </summary>
        private const string NEW_NAME_FORMAT = "{0} {1}";

        #endregion Private constants

        #region Private fields

        /// <summary>
        /// Collection of Custom order properties.
        /// </summary>
        private ObservableCollection<CustomOrderProperty> _customOrderProperties =
            new ObservableCollection<CustomOrderProperty>();

        /// <summary>
        /// Backup of collection _customOrderProperties got when
        /// function BackupCustomOrderPropertiesCollection() is called.
        /// </summary>
        private ObservableCollection<CustomOrderProperty> _customOrderPropertiesBackup =
            new ObservableCollection<CustomOrderProperty>();

        /// <summary>
        /// Custom order property name validator.
        /// </summary>
        private ICustomOrderPropertyNameValidator _customOrderPropertyNameValidator;

        /// <summary>
        /// Insertion row of Xceed DataGrid.
        /// </summary>
        private InsertionRow _insertionRow;

        /// <summary>
        /// Flag is set to true when editing of item in Xceed DataGrid is in progress.
        /// </summary>
        private bool _isEdititngInProgress;

        #endregion Private fields
    }
}
