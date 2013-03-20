using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interaction logic for FleetSetupWizardLocationPage.xaml
    /// </summary>
    internal partial class FleetSetupWizardLocationPage : WizardPageBase,
        ISupportBack, ISupportNext, ISupportCancel
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public FleetSetupWizardLocationPage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(_FleetSetupWizardLocationPageLoaded);

            // Init validation callout controller.
            var vallidationCalloutController = new ValidationCalloutController(DataGridControl);
        }

        #endregion // Constructors

        #region ISupportBack members

        /// <summary>
        /// Occurs when "Back" button clicked.
        /// </summary>
        public event EventHandler BackRequired;

        #endregion

        #region ISupportNext members

        /// <summary>
        /// Occurs when "Next" button clicked.
        /// </summary>
        public event EventHandler NextRequired;

        #endregion

        #region ISupportCancel members

        /// <summary>
        /// Occurs when "Cancel" button clicked.
        /// </summary>
        public event EventHandler CancelRequired;

        #endregion // ISupportCancel members

        #region Private properties

        /// <summary>
        /// Current selected location.
        /// </summary>
        private Location CurrentItem
        {
            get
            {
                return DataGridControl.SelectedItem as Location;
            }
        }

        /// <summary>
        /// Specialized context.
        /// </summary>
        private FleetSetupWizardDataContext DataKeeper
        {
            get
            {
                return DataContext as FleetSetupWizardDataContext;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// React on page loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _FleetSetupWizardLocationPageLoaded(object sender, RoutedEventArgs e)
        {
            // Init page if not inited yet.
            if (!_inited)
            {
                _InitLocations();

                // Set row height.
                ContentGrid.RowDefinitions[DATA_GRID_ROW_DEFINITION_INDEX].Height =
                    new System.Windows.GridLength(DEFAULT_ROW_HEIGHT * ROW_COUNT);

                // Create subpages.
                string typeName = (string)App.Current.FindResource(LOCATION_RESOURCE_NAME);
                typeName = typeName.ToLower();
                _matchFoundSubPage = new MatchFoundSubPage(typeName);
                _candidatesNotFoundSubPage = new CandidatesNotFoundSubPage(typeName);
                _candidatesFoundSubPage = new CandidatesFoundSubPage(typeName);

                _SetSubPage(null);

                // Create layer of existed locations.
                _CreateLocationsLayer();
                // Create layer of current location.
                _CreateEditedLayer();

                _geocodablePage = new GeocodablePage(typeof(Location), mapCtrl, candidateSelect,
                    controlsGrid, DataGridControl, splitter, _locationsLayer);
                _geocodablePage.MatchFound += new EventHandler(_MatchFound);
                _geocodablePage.CandidatesFound += new EventHandler(_CandidatesFound);
                _geocodablePage.CandidatesNotFound += new EventHandler(_CandidatesNotFound);
                _geocodablePage.ParentIsFleetWisard = true;

                mapCtrl.AddTool(new EditingTool(), null);

                _InitDataGridControl();

                _inited = true;
            }

            // Create ungeocoded orders list.
            _collectionSource.Source = _locations;

            _selectionBinding.UnregisterAllCollections();
            _selectionBinding.RegisterCollection(DataGridControl);
            _selectionBinding.RegisterCollection(mapCtrl.SelectedItems);
        }

        /// <summary>
        /// Prepare data grid control to work.
        /// </summary>
        private void _InitDataGridControl()
        {
            _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

            // Initialize grid structure.
            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.FleetGeocodableGridStructure);
            structureInitializer.BuildGridStructure(_collectionSource, DataGridControl);

            // Find index for first non-Name field to determine it as Address Field.
            int columnIndex = 0;
            for (int i = 0; i < DataGridControl.Columns.Count; i++)
            {
                if (DataGridControl.Columns[i].FieldName != NAME_COLUMN)
                {
                    columnIndex = i;
                    break; // Work done: first non-Name field found.
                }
            }

            // Set special content template to show "Add location" string.
            ColumnBase addressColumn = DataGridControl.Columns[columnIndex];
            addressColumn.CellContentTemplate = (DataTemplate)App.Current.FindResource(ADDRESSLINE_CONTENT_TEMPLATE_NAME);

            CommonHelpers.HidePostalCode2Column(DataGridControl);

            // Load grid layout.
            GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.FleetLocationsSettingsRepositoryName,
                _collectionSource.ItemProperties);

            layoutLoader.LoadLayout(DataGridControl);
        }

        /// <summary>
        /// Init locations collection.
        /// </summary>
        private void _InitLocations()
        {
            // Add existed locations.
            foreach (Location location in DataKeeper.Locations)
            {
                _locations.Add(location);
            }

            if (_locations.Count == DEFAULT_LOCATION_COUNT)
            {
                // If all rows filled - add one more location.
                Location location = _CreateLocationWithFakeNameAndWithoutValidation();
                _locations.Add(location);
            }
            else
            {
                // If existed locations less then minimum - add.
                while (_locations.Count < DEFAULT_LOCATION_COUNT)
                {
                     Location location = _CreateLocationWithFakeNameAndWithoutValidation();
                    _locations.Add(location);
                }
            }

            // Set default name for first location if it is empty.
            if (string.IsNullOrWhiteSpace(_locations[0].Name))
            {
                _locations[0].Name = _GetDefaultLocationName();
                _locations[0].GeoLocation = null;
            }
        }

        /// <summary>
        /// Create location and switch of address validation.
        /// </summary>
        /// <returns></returns>
        private Location _CreateLocationWithFakeNameAndWithoutValidation()
        {
            // Create new location with whitespace name. Do so to cheat validation, so there would
            // be no yellow ! at location name.
            Location location = new Location();
            location.Name = DataObjectNamesConstructor.GetNewWhiteSpacesName(_locations);

            // Turn off address validation.
            location.IsAddressValidationEnabled = false;

            return location;
        }

        /// <summary>
        /// React on candidates not found.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidatesNotFound(object sender, EventArgs e)
        {
            _SetSubPage(_candidatesNotFoundSubPage);
        }

        /// <summary>
        /// React on candidates found.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidatesFound(object sender, EventArgs e)
        {
            _SetSubPage(_candidatesFoundSubPage);
        }

        /// <summary>
        /// React on match found.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MatchFound(object sender, EventArgs e)
        {
            _SetSubPage(_matchFoundSubPage);

            mapCtrl.map.UpdateLayout();

            // Get item container.
            DependencyObject itemContainer = DataGridControl.GetContainerFromItem(CurrentItem);
            DataRow row = itemContainer as DataRow;

            if (row != null)
            {
                // Invoke redraw.
                ControlTemplate template = row.Cells[ADDRESS_LINE_COLUMN_INDEX].Template;
                row.Cells[ADDRESS_LINE_COLUMN_INDEX].Template = null;
                row.Cells[ADDRESS_LINE_COLUMN_INDEX].Template = template;
            }
            else
            {
                // Incorrect functionality in Xceed grid. Sometimes item container is not returned.
            }

            // Zooming to current item suspended because zooming should go after suspended zoom
            // restoring in mapextentmanager. Suspended zoom restoring invoked in case of saved
            // old extent(zoom was changed by mouse) and because of map control size changed
            // after showing subpage.
            Dispatcher.BeginInvoke(new Action<Location>(delegate(Location item)
                {
                    _ZoomOnLocation(item);
                }
                ),
                DispatcherPriority.Background,CurrentItem);

            // Do not need to finish editing there if match was found by tool.
            if (_skipStartGeocoding)
            {
                // Do nothing.
            }
            else
            {
                _skipStartGeocoding = true;
                try
                {
                    DataGridControl.EndEdit();
                }
                catch
                {
                    // Exception in cycle end editing.
                }
                _skipStartGeocoding = false;
            }

            // If current location has no name - get next name for it.
            if (string.IsNullOrWhiteSpace(CurrentItem.Name))
            {
                CurrentItem.Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                    _locations, CurrentItem, true);
            }
        }

        /// <summary>
        /// Zoom on current selected location.
        /// </summary>
        /// <param name="location">Location to zoom at.</param>
        private void _ZoomOnLocation(Location location)
        {
            // Current item can be null because zooming suspended.
            if (location != null)
            {
                // Zoom on location.
                List<ESRI.ArcLogistics.Geometry.Point> points = new List<ESRI.ArcLogistics.Geometry.Point>();
                points.Add(location.GeoLocation.Value);
                MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
            }
        }

        /// <summary>
        /// Check that valid locations in collection are valid.
        /// </summary>
        /// <returns>'True' if locations are valid, 'false' otherwise.</returns>
        private List<Location> _GetValidLocation()
        {
            var validLocations = new List<Location>();

            // Fill location collection which will be validate with 
            // locations, which names are not whitespaces.
            List<Location> locationsToValidate = new List<Location>();
            foreach (var location in _locations)
                if (location.Name == "" || location.Name.Trim().Length > 0)
                    locationsToValidate.Add(location);

            // Check that there are locations to validate.
            if (locationsToValidate.Count == 0)
            {
                string message = ((string)App.Current.FindResource("NoLocationValidationError"));
                App.Current.Messenger.AddMessage(MessageType.Warning, message);

                //Return empty collection.
                return validLocations;
            }
            // Add valid locations to result collection.
            else
            {
                foreach (var location in locationsToValidate)
                {
                    // Turn on validation for address fields.
                    if (!location.IsAddressValidationEnabled)
                    {
                        location.IsAddressValidationEnabled = true;

                        // Start then end edit. This will push grid to realize,
                        // that Addres field become invalid.
                        // NOTE: we can have exception here if edit is already in progress.
                        DataGridControl.BeginEdit(location);
                        DataGridControl.EndEdit();
                    }
                       
                    if (location.IsValid)
                        validLocations.Add(location);
                }
            }

            // If there is no valid locations - show error message about invalid locations.
            if (validLocations.Count == 0)
                CanBeLeftValidator<Location>.ShowErrorMessagesInMessageWindow(locationsToValidate);

            return validLocations;
        } 

        /// <summary>
        /// Create layer for showing project locations.
        /// </summary>
        private void _CreateLocationsLayer()
        {
            _locationsLayer = new ObjectLayer(_locations, typeof(Location), false);
            _locationsLayer.EnableToolTip();
            _locationsLayer.Selectable = false;

            mapCtrl.AddLayer(_locationsLayer);
        }

        /// <summary>
        /// Create layer for showing newly created location.
        /// </summary>
        private void _CreateEditedLayer()
        {
            _editedCollection = new ObservableCollection<object>();

            _editedObjectLayer = new ObjectLayer(_editedCollection, typeof(Location), false);
            _editedObjectLayer.ConstantOpacity = true;
            _editedObjectLayer.Selectable = false;

            mapCtrl.AddLayer(_editedObjectLayer);
        }

        /// <summary>
        /// Set subpage.
        /// </summary>
        /// <param name="subPage">Subpage to set. Null if disable subpages.</param>
        private void _SetSubPage(IGeocodeSubPage subPage)
        {
            GeocodeHelperPanel.Children.Clear();

            if (subPage != null)
            {
                GeocodeHelperPanel.Children.Add(subPage as Grid);
                Grid.SetColumnSpan(controlsGrid, 1);

                // Set subpage text.
                GeocodeResultText.Text = subPage.GetGeocodeResultString(CurrentItem, _geocodablePage.CandidatesToZoom);
                GeocodeResultText.Visibility = Visibility.Visible;
            }
            else
            {
                Grid.SetColumnSpan(controlsGrid, 2);

                // Remove subpage text.
                GeocodeResultText.Text = string.Empty;
                GeocodeResultText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Back button click handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonBackClick(object sender, RoutedEventArgs e)
        {
            // Check can go back.
            bool canGoBack = false;
            try
            {
                DataGridControl.EndEdit();
                canGoBack = true;
            }
            catch
            { }

            if (canGoBack)
            {
                if (null != BackRequired)
                    BackRequired(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// React on button next click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonNextClick(object sender, RoutedEventArgs e)
        {
            // Try to apply changes.
            DataGridControl.EndEdit();

            var validLocations = _GetValidLocation();
            // If there are geocoded locations - save them and go to the next page.
            if(validLocations.Count > 0)
            {
                // Save valid locations newly created.
                foreach (Location location in validLocations)
                {
                    if (!DataKeeper.Locations.Contains(location) && location.IsGeocoded && !string.IsNullOrEmpty(location.Name))
                    {
                        DataKeeper.Locations.Add(location);
                    }
                }

                DataKeeper.Project.Save();

                if (null != NextRequired)
                    NextRequired(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// React on delete button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            // Cancel editing item.
            if (DataGridControl.IsBeingEdited)
            {
                DataGridControl.CancelEdit();
            }

            // Remove location.
            _locations.Remove(CurrentItem);

            // If locations count is less then minimum - add new location.
            if (_locations.Count < DEFAULT_LOCATION_COUNT)
            {
                Location location = _CreateLocationWithFakeNameAndWithoutValidation();
                _locations.Add(location);
            }
        }

        /// <summary>
        /// React on locate button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonLocateClick(object sender, RoutedEventArgs e)
        {
            // Remove history items.
            try
            {
                // Endedit can lead to exception if name is empty and not commited yet.
                DataGridControl.EndEdit();
            }
            catch { }

            if (CurrentItem.IsGeocoded)
            {
                _ZoomOnLocation(CurrentItem);
            }
            else
            {
                // Geocode location.
                _geocodablePage.StartGeocoding(CurrentItem);

                if (_geocodablePage.CandidatesToZoom != null)
                {
                    MapExtentHelpers.ZoomToCandidates(mapCtrl, _geocodablePage.CandidatesToZoom);
                }
            }
        }

        /// <summary>
        /// React on button cancel click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            if (null != CancelRequired)
                CancelRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Set new default location name to Text Box.
        /// </summary>
        private string _GetDefaultLocationName()
        {
            string defaultNameFmt = (string)App.Current.FindResource(DEFAULT_LOCATION_NAME_FMT);

            // Find last location formatted as default.
            string newLocationName = string.Format(defaultNameFmt, string.Empty);
            newLocationName.Trim();

            int index = 0;
            while (index <= _locations.Count)
            {
                bool isLocationNameUsed = false;
                
                if (index == 0)
                {
                    // Use "Depot" string without number if it is first item.
                    newLocationName = string.Format(defaultNameFmt, string.Empty);
                    newLocationName = newLocationName.Trim();
                }
                else
                {
                    newLocationName = string.Format(defaultNameFmt, index.ToString());
                }

                foreach (Location location in _locations)
                {
                    if (!string.IsNullOrEmpty(location.Name) && location.Name.Equals(newLocationName))
                    {
                        isLocationNameUsed = true;
                        break;
                    }
                }

                if (!isLocationNameUsed)
                {
                    break;
                }

                index++;
            }

            // Set location name as empty default location name.
            return newLocationName;
        }

        /// <summary>
        /// Set "Locate" and "Delete" buttons availability.
        /// </summary>
        private void _ProcessLocateState()
        {
            // If item is not selected or it has whitespaces name - hide buttons.
            if (CurrentItem == null || 
                (CurrentItem.Name != "" && String.IsNullOrWhiteSpace(CurrentItem.Name)))
            {
                // If item is not selected - hide button.
                ButtonsPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                // Show panel and enable "Locate" button.
                _ShowAndEnableButtons();

                // If address fields are empty - disable "Locate" button.
                if (GeocodeHelpers.IsActiveAddressFieldsEmpty(CurrentItem))
                {
                    ButtonLocate.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Show panel and enable "Locate" button.
        /// </summary>
        private void _ShowAndEnableButtons()
        {
            // Show panel and enable button.
            ButtonLocate.IsEnabled = true;
            ButtonsPanel.Visibility = Visibility.Visible;

            // Call margin setter suspended because container is not correct after scrolling.
            Dispatcher.BeginInvoke(
                new Action(delegate()
                    {
                        ButtonsPanel.Margin = CommonHelpers.GetItemContainerMargin
                            (CurrentItem, DataGridControl, DEFAULT_ROW_HEIGHT);
                    }), 
                DispatcherPriority.Background);
        }

        /// <summary>
        /// React on selection change.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            _geocodablePage.OnSelectionChanged(DataGridControl.SelectedItems);

            _SetSubPage(null);
            _ProcessLocateState();
        }

        /// <summary>
        /// React on beginning edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            // If location has no or whitespace name - get new name for it 
            // and turn on validation on address fields.
            Location location = e.Item as Location;
            if (string.IsNullOrWhiteSpace(location.Name))
            {
                location.Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                    _locations, location, true);
            }

            (e.Item as Location).IsAddressValidationEnabled = true;
            
            _geocodablePage.OnBeginningEdit(e);
            e.Handled = true;

            CurrentItem.Address.PropertyChanged += new PropertyChangedEventHandler(_AddressPropertyChanged);
            CurrentItem.PropertyChanged += new PropertyChangedEventHandler(_CurrentItemPropertyChanged);
        }

        /// <summary>
        /// React on committing edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _CommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            _geocodablePage.OnCommittingEdit(e, !_skipStartGeocoding);
            e.Handled = true;

            CurrentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
            CurrentItem.PropertyChanged -= new PropertyChangedEventHandler(_CurrentItemPropertyChanged);
        }

        /// <summary>
        /// React on cancelling edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            _geocodablePage.OnEditCanceled(e);
            e.Handled = true;

            // If item was cancelled - move location to fake position.
            Location canceledLocation = e.Item as Location;
            if (string.IsNullOrEmpty(canceledLocation.Name))
            {
                canceledLocation.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(0, 0);
                canceledLocation.Address.FullAddress = string.Empty;
            }

            if (CurrentItem != null)
            {
                CurrentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
                CurrentItem.PropertyChanged -= new PropertyChangedEventHandler(_CurrentItemPropertyChanged);
            }
            else
            {
                // Do nothing. Current item is null after cancelling.
            }
        }

        /// <summary>
        /// React on edit canceled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _EditCanceled(object sender, DataGridItemEventArgs e)
        {
            _geocodablePage.OnEditCanceled(e);
        }

        /// <summary>
        /// React on current item address property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AddressPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _ProcessLocateState();
        }

        /// <summary>
        /// React on selected item property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event args.</param>
        private void _CurrentItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(Location.PropertyNameName, StringComparison.OrdinalIgnoreCase))
            {
                _ProcessLocateState();
            }
        }

        /// <summary>
        /// React on edit committed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _EditCommitted(object sender, DataGridItemEventArgs e)
        {
            // If last item was edited - add new item.
            if (_locations.IndexOf((Location)e.Item) == _locations.Count - 1)
            {
                Location location = _CreateLocationWithFakeNameAndWithoutValidation();
                _locations.Add(location);

                // Make postponed bring item into view.
                Dispatcher.BeginInvoke(
                    new Action(delegate()
                    {
                        DataGridControl.BringItemIntoView(_locations[_locations.Count - 1]);

                        _ProcessLocateState();
                    }
                    ),
                    DispatcherPriority.Background);
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Default location name format string.
        /// </summary>
        private const string DEFAULT_LOCATION_NAME_FMT = "DefaultLocationNameFmt";

        /// <summary>
        /// Collection source resource key name.
        /// </summary>
        private const string COLLECTION_SOURCE_KEY = "DataGridSource";

        /// <summary>
        /// String format for invalid location warning message.
        /// </summary>
        private const string INVALID_STRING_FORMAT = "{0} {1}";

        /// <summary>
        /// Maximum location count, created by wizard.
        /// </summary>
        private const int DEFAULT_LOCATION_COUNT = 3;

        /// <summary>
        /// Datagrid control default row height.
        /// </summary>
        private readonly double DEFAULT_ROW_HEIGHT = (double)App.Current.FindResource("XceedRowDefaultHeight");

        /// <summary>
        /// Index of row in definions, which contains datagrid.
        /// </summary>
        private const int DATA_GRID_ROW_DEFINITION_INDEX = 3;

        /// <summary>
        /// Datagrid control default row count.
        /// </summary>
        private readonly int ROW_COUNT = DEFAULT_LOCATION_COUNT + 2;

        /// <summary>
        /// Collection source resource key name.
        /// </summary>
        private const string ADDRESSLINE_CONTENT_TEMPLATE_NAME = "AddLocationAddressLineContent";

        /// <summary>
        /// Location resource name
        /// </summary>
        private const string LOCATION_RESOURCE_NAME = "Location";

        /// <summary>
        /// Name string.
        /// </summary>
        private const string NAME_PROPERTY_STRING = "Name";

        /// <summary>
        /// Name column.
        /// </summary>
        private const string NAME_COLUMN = "Name";

        /// <summary>
        /// Datagrid control default row count.
        /// </summary>
        private readonly int ADDRESS_LINE_COLUMN_INDEX = 1;

        #endregion

        #region Private fields

        /// <summary>
        /// Project locations layer.
        /// </summary>
        private ObjectLayer _locationsLayer;

        /// <summary>
        /// Creating item layer.
        /// </summary>
        private ObjectLayer _editedObjectLayer;

        /// <summary>
        /// Creating item collection.
        /// </summary>
        private ObservableCollection<object> _editedCollection;

        /// <summary>
        /// Is page inited.
        /// </summary>
        private bool _inited;

        /// <summary>
        /// Match found subpage.
        /// </summary>
        private MatchFoundSubPage _matchFoundSubPage;

        /// <summary>
        /// Candidate not found subpage.
        /// </summary>
        private CandidatesNotFoundSubPage _candidatesNotFoundSubPage;

        /// <summary>
        /// Candidate found subpage.
        /// </summary>
        private CandidatesFoundSubPage _candidatesFoundSubPage;

        /// <summary>
        /// Geocodable page.
        /// </summary>
        private GeocodablePage _geocodablePage;

        /// <summary>
        /// Collection view source.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource = null;

        /// <summary>
        /// Locations collection.
        /// </summary>
        private ESRI.ArcLogistics.Data.OwnerCollection<Location> _locations =
            new ESRI.ArcLogistics.Data.OwnerCollection<Location>();

        /// <summary>
        /// Helper for synchronization selection between map layer and datagrid control.
        /// </summary>
        private MultiCollectionBinding _selectionBinding = new MultiCollectionBinding();

        /// <summary>
        /// Flag to prevent cycle calling start geocoding after match found.
        /// </summary>
        private bool _skipStartGeocoding;

        #endregion // Private fields
    }
}
