using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Utility;
using Xceed.Wpf.DataGrid;
using ALGeometry = ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class for helping to create geocodable items such as locations and orders.
    /// </summary>
    class GeocodablePage
    {
        #region constructors

        /// <summary>
        /// Create GeocodablePage.
        /// </summary>
        /// <param name="geocodableType">Geocodable type.</param>
        /// <param name="mapCtrl">Map from parent page.</param>
        /// <param name="candidateSelect">Control for selecting candidates.</param>
        /// <param name="controlsGrid">Grid, that contains map and candidateselect control.</param>
        /// <param name="geocodableGrid">Grid with geocodable items from parent page.</param>
        /// <param name="splitter">Splitter between map and candidateSelectControl.</param>
        /// <param name="parentLayer">Layer, that contains regions.</param>
        public GeocodablePage(Type geocodableType, MapControl mapCtrl, CandidateSelectControl candidateSelect,
            Grid controlsGrid, DataGridControlEx geocodableGrid, GridSplitter splitter, ObjectLayer parentLayer)
        {
            Debug.Assert(mapCtrl != null);
            Debug.Assert(candidateSelect != null);
            Debug.Assert(controlsGrid != null);
            Debug.Assert(geocodableGrid != null);
            Debug.Assert(splitter != null);
            Debug.Assert(parentLayer != null);

            _mapCtrl = mapCtrl;

            _candidateSelect = candidateSelect;
            _candidateSelect.Initialize(geocodableType);
            _controlsGrid = controlsGrid;
            _geocodableGrid = geocodableGrid;
            _splitter = splitter;
            _parentLayer = parentLayer;

            _geocodableType = geocodableType;

            _collapsed = new GridLength(0, GridUnitType.Star);
            _expanded = controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].Width;
            _collapsedMinWidth = 0;
            _expandedMinWidth = controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].MinWidth;
            _CollapseCandidateSelect();

            // Subscribe to grid loaded event.
            _geocodableGrid.Loaded += new RoutedEventHandler(_GeocodableGridLoaded);
        }

        #endregion

        #region Public events

        /// <summary>
        /// Occurs when geocoding is started.
        /// </summary>
        public event EventHandler GeocodingStarted;

        /// <summary>
        /// Occurs when geocoding found match.
        /// </summary>
        public event EventHandler MatchFound;

        /// <summary>
        /// Occurs when geocoding found candidates.
        /// </summary>
        public event EventHandler CandidatesFound;

        /// <summary>
        /// Occurs when geocoding not found candidates.
        /// </summary>
        public event EventHandler CandidatesNotFound;

        #endregion

        #region Public members

        /// <summary>
        /// Is Page is in edited mode now.
        /// </summary>
        public bool IsInEditedMode
        {
            get;
            private set;
        }

        /// <summary>
        /// Is geocode in progress.
        /// </summary>
        public bool IsGeocodingInProcess
        {
            get;
            private set;
        }

        /// <summary>
        /// If geocoding is not successful - during geocoding process this property means best candidate to zoom.
        /// </summary>
        public AddressCandidate[] CandidatesToZoom
        {
            get;
            private set;
        }

        /// <summary>
        /// Is geocodable page used in fleet wizard, which means special logic.
        /// </summary>
        public bool ParentIsFleetWisard
        {
            get;
            set;
        }

        #endregion

        #region public Methods
        
        /// <summary>
        /// End geocoding.
        /// </summary>
        public void EndGeocoding()
        {
            _EndGeocoding(true);
        }

        /// <summary>
        /// React on creating new item.
        /// </summary>
        /// <param name="args">New item creating args.</param>
        public void OnCreatingNewItem(DataGridCreatingNewItemEventArgs args)
        {
            Debug.Assert(_parentLayer != null);
            Debug.Assert(args != null);

            _currentItem = (IGeocodable)args.NewItem;
            _currentItem.Address.PropertyChanged += new PropertyChangedEventHandler(_AddressPropertyChanged);

            Graphic graphic = _parentLayer.CreateGraphic(_currentItem);
            _parentLayer.MapLayer.Graphics.Add(graphic);

            _editStartedByGrid = true;
            _StartEdit(_currentItem);
            _editStartedByGrid = false;

            _SetAddressByPointToolEnabled(true);
        }

        /// <summary>
        /// React on commiting new item.
        /// </summary>
        /// <param name="args">New item commiting args.</param>
        /// <returns>Is commiting successful.</returns>
        public bool OnCommittingNewItem(DataGridCommittingNewItemEventArgs args)
        {
            Debug.Assert(_currentItem != null);
            Debug.Assert(args != null);

            bool commitingSuccessful = false;

            if (_isAddressChanged)
            {
                _StartGeocoding(_currentItem, true);

                if (!IsGeocodingInProcess)
                {
                    _currentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
                }
            }

            args.Handled = true;

            if (IsGeocodingInProcess)
            {
                args.Cancel = true;
            }
            else
            {
                commitingSuccessful = true;
            }

            _isNewItemJustCommited = true;

            return commitingSuccessful;
        }

        /// <summary>
        /// React on new item commited.
        /// </summary>
        public void OnNewItemCommitted()
        {
            Debug.Assert(_currentItem != null);

            _canceledByGrid = true;
            EditEnded(false);
            _currentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
            _canceledByGrid = false;

            _SetAddressByPointToolEnabled(false);

            _currentItem = null;
        }

        /// <summary>
        /// React on cancelling new item.
        /// </summary>
        public void OnNewItemCancelling()
        {
            Debug.Assert(_parentLayer != null);

            // Supporting API issues. Needed in case of external new item creating canceling.
            if (_currentItem == null)
                return;

            _canceledByGrid = true;

            ObjectLayer.DeleteObject(_currentItem, _parentLayer.MapLayer);
            EditEnded(false);
            _currentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);

            if (IsGeocodingInProcess)
            {
                _EndGeocoding(false);
            }
            else
            {
                _currentItem = null;
            }

            _canceledByGrid = false;

            _SetAddressByPointToolEnabled(false);
        }

        /// <summary>
        /// React on cancelling edit.
        /// </summary>
        /// <param name="args">Datagrid item args.</param>
        public void OnEditCanceled(DataGridItemEventArgs args)
        {
            Debug.Assert(args != null);

            _canceledByGrid = true;

            if (IsGeocodingInProcess)
            {
                _EndGeocoding(false);
            }

            IGeocodable geocodable = args.Item as IGeocodable;
            geocodable.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
            _currentItem = null;
            _isAddressChanged = false;
            EditEnded(false);

            _canceledByGrid = false;
        }

        /// <summary>
        /// React on beginning edit.
        /// </summary>
        /// <param name="args">Beginning edit args.</param>
        public void OnBeginningEdit(DataGridItemCancelEventArgs args)
        {
            Debug.Assert(args != null);

            // if geocoding in process and try to edit not geocoding geocodable object - than cancel it
            IGeocodable current = (IGeocodable)args.Item;

            // in case of deleting edited - cancel beginning edit
            if ((IsGeocodingInProcess && current != _currentItem))
            {
                args.Cancel = true;
            }
            else
            {
                _currentItem = args.Item as IGeocodable;
                _currentItem.Address.PropertyChanged += new PropertyChangedEventHandler(_AddressPropertyChanged);

                _editStartedByGrid = true;
                _StartEdit(_currentItem);
                _editStartedByGrid = false;
            }
        }

        /// <summary>
        /// React on commiting edit.
        /// </summary>
        /// <param name="args">Commiting event args.</param>
        /// <param name="canStartGeocoding">Flag for accepting geocoding start.</param>
        public void OnCommittingEdit(DataGridItemCancelEventArgs args, bool canStartGeocoding)
        {
            Debug.Assert(args != null);

            _canceledByGrid = true;
            IGeocodable geocodable = (IGeocodable)args.Item;
            if (geocodable != _currentItem)
            {
                args.Cancel = true;
            }

            if (_isAddressChanged && canStartGeocoding)
            {
                _StartGeocoding(_currentItem, true);
                if (!IsGeocodingInProcess)
                {
                    _currentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
                }
            }

            EditEnded(false);

            _canceledByGrid = false;

            if (!IsGeocodingInProcess)
            {
                _currentItem = null;
            }
        }

        /// <summary>
        /// React on project closed.
        /// </summary>
        public void OnProjectClosed()
        {
            if (IsGeocodingInProcess)
            {
                _EndGeocoding(true);
            }

            if (IsInEditedMode)
            {
                EditEnded(false);
            }
        }

        /// <summary>
        /// Do regeocoding.
        /// </summary>
        /// <param name="geocodable">Geocodable to regeocode.</param>
        public void StartGeocoding(IGeocodable geocodable)
        {
            Debug.Assert(geocodable != null);

            _currentItem = geocodable;
            _StartGeocoding(geocodable, false);
        }

        /// <summary>
        /// React on selection changed.
        /// </summary>
        /// <param name="selectedItems">Selected items list.</param>
        public void OnSelectionChanged(IList selectedItems)
        {
            Debug.Assert(selectedItems != null);

            // Do not hide hint in case of new item was committed and selection is not set yet.
            if (selectedItems.Count > 0 && !_isNewItemJustCommited)
                _mapCtrl.HideZoomToCandidatePopup();

            _isNewItemJustCommited = false;

            if (IsGeocodingInProcess)
            {
                _EndGeocoding(false);
            }

            // Address by point tool enabled in case of geocodable object selected or geocodable
            // object creating in progress.
            bool isAddressByPointToolEnabled = _currentItem != null ||
                (selectedItems.Count == 1 && selectedItems[0] is IGeocodable);
            _SetAddressByPointToolEnabled(isAddressByPointToolEnabled);

            if (_mapCtrl.CurrentTool == _addressByPointTool)
            {
                _mapCtrl.CurrentTool = null;
            }
        }

        /// <summary>
        /// End edit.
        /// </summary>
        /// <param name="commit">Is in case of editing in grid try to commit changes.</param>
        public void EditEnded(bool commit)
        {
            Debug.Assert(_mapCtrl != null);
            Debug.Assert(_parentLayer != null);

            if (_isReccurent)
                return;

            // NOTE : workaround - sometimes Xceed data grid returns incorrect value of property IsBeingEdited
            // and that case we have to use the same property of Insertion Row.
            bool isGridBeingEdited = (_geocodableGrid.IsBeingEdited ||
                (_geocodableGrid.InsertionRow != null &&
                _geocodableGrid.InsertionRow.IsVisible &&
                _geocodableGrid.InsertionRow.IsBeingEdited));

            bool setEditFinished = true;
            if (isGridBeingEdited && !_canceledByGrid)
            {
                _isReccurent = true;

                // If command from map to commit was come, than try to end edit and commit
                // otherwise cancel edit.
                if (commit)
                {
                    try
                    {
                        // If item is in insertion row - commit it.
                        if (_geocodableGrid.InsertionRow != null)
                        {
                            _geocodableGrid.InsertionRow.EndEdit();
                        }

                        _geocodableGrid.EndEdit();
                    }
                    catch
                    {
                        // Do not need to finish editing on map. Editing in grid was not finished.
                        setEditFinished = false;

                        // NOTE : Eat exception. Exception occurs if geocoding in progress.
                    }
                }
                else
                {
                    // If item is in Insertion row - clear it.
                    if (_geocodableGrid.InsertionRow != null)
                    {
                        _geocodableGrid.InsertionRow.CancelEdit();
                    }
                    else
                    {
                        _geocodableGrid.CancelEdit();
                    }
                }

                _isReccurent = false;
            }

            if (setEditFinished)
            {
                IsInEditedMode = false;

                if (_mapCtrl.IsInEditedMode)
                {
                    _isReccurent = true;
                    _mapCtrl.EditEnded();
                    _isReccurent = false;
                }
            }

            _parentLayer.Selectable = true;
            _isAddressChanged = false;
        }

        #endregion

        #region Private Static Methods
        /// <summary>
        /// Checks that distance between specified points is less than or equal to the specified
        /// maximum distance.
        /// </summary>
        /// <param name="first">The first point to be checked.</param>
        /// <param name="second">The second point to be checked.</param>
        /// <param name="maxDistance">The maximum distance in meters.</param>
        /// <returns>True if and only if the distance between two points does not exceed
        /// <paramref name="maxDistance"/>.</returns>
        private static bool _CheckDistance(
            ALGeometry.Point first,
            ALGeometry.Point second,
            double maxDistance)
        {
            var longitude1 = _Normalize(first.X);
            var latitude1 = _Normalize(first.Y);
            var longitude2 = _Normalize(second.X);
            var latitude2 = _Normalize(second.Y);

            var radius = DistCalc.GetExtentRadius(longitude1, latitude1, maxDistance);

            // Simple Euclidean geometry should be ok for small distances.
            var distance = Math.Sqrt(_Sqr(longitude2 - longitude1) + _Sqr(latitude2 - latitude1));

            return distance < radius;
        }

        /// <summary>
        /// Computes square of the specified value.
        /// </summary>
        /// <param name="x">The value to compute square for.</param>
        /// <returns>A square of the specified value.</returns>
        private static double _Sqr(double x)
        {
            return x * x;
        }

        /// <summary>
        /// Normalizes the specified WGS 84 coordinate value.
        /// </summary>
        /// <param name="coordinate">The coordinate value to be normalized.</param>
        /// <returns>A new coordinate value which is equal to the specified one to a period of
        /// 360 degrees and falls into [0, 360] range.</returns>
        private static double _Normalize(double coordinate)
        {
            const double period = 360.0;
            var result = Math.IEEERemainder(coordinate, period);
            if (result < 0.0)
            {
                result += period;
            }

            return result;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Occurs when parent grid is loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _GeocodableGridLoaded(object sender, RoutedEventArgs e)
        {
            // Do not initialize tools and candidates layers
            // if geocoder or map is not initialized.
            if (_mapCtrl.Map.IsInitialized() && App.Current.InternalGeocoder.IsInitialized())
            {
                _InitGeocodingTools();

                // When tools was activated - unsubscribe from grid loaded event.
                _geocodableGrid.Loaded -= new RoutedEventHandler(_GeocodableGridLoaded);
            }
        }

        /// <summary>
        /// Init geocoding tools.
        /// </summary>
        private void _InitGeocodingTools()
        {
            _CreateCandidatesLayer();

            // Init tool.
            _addressByPointTool = new AddressByPointTool();
            _addressByPointTool.SetGeocodableType(_geocodableType);
            _mapCtrl.AddTool(_addressByPointTool, _CanActivateTool);

            // If geocodable object is order then add handler, which disable selecting
            // objects from all map layers, when tool is active.
            if (_geocodableType == typeof(Order))
                _addressByPointTool.ActivatedChanged += new EventHandler
                    (_AddressByPointToolActivatedChanged);

            _InitEventHandlers();
        }

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            Debug.Assert(_mapCtrl != null);
            Debug.Assert(_candidateSelect != null);
            Debug.Assert(_addressByPointTool != null);

            _mapCtrl.CanSelectCallback = _CanSelect;
            _mapCtrl.StartEditGeocodableCallback = _StartEdit;
            _mapCtrl.EndEditGeocodableCallback = EditEnded;
            _mapCtrl.IsGeocodingInProgressCallback = _IsGeocodingInProgress;

            _candidateSelect.CandidateApplied += new EventHandler(_CandidateApplied);
            _candidateSelect.CandidateLeaved += new EventHandler(_CandidateLeaved);
            _candidateSelect.CandidatePositionApplied += new EventHandler(_CandidatePositionApplied);

            _addressByPointTool.OnComplete += new EventHandler(_AddressByPointToolOnComplete);
        }

        /// <summary>
        /// Can select callback.
        /// </summary>
        /// <param name="item">Item to check.</param>
        /// <returns>Is item can be selected.</returns>
        private bool _CanSelect(object item)
        {
            return true;
        }

        /// <summary>
        /// Can address by point tool be activated.
        /// </summary>
        /// <returns>Is tool can be activated.</returns>
        private bool _CanActivateTool()
        {
            return true;
        }


        /// <summary>
        /// Make request to geocode server and process result.
        /// </summary>
        /// <param name="systemPoint">Clicked point coords.</param>
        private void _DoReverseGeocode(System.Windows.Point systemPoint)
        {
            object[] args = new object[3];
            args[0] = systemPoint;
            IGeocodable geocodable;
            if (_currentItem != null)
            {
                geocodable = _currentItem;
            }
            else
            {
                geocodable = (IGeocodable)_geocodableGrid.SelectedItems[0];
            }

            args[1] = geocodable;
            AddressByPointCmd addressByPointCmd = new AddressByPointCmd();
            addressByPointCmd.Execute(args);

            if (MatchFound != null)
                MatchFound(this, EventArgs.Empty);

            _SaveToLocalAddressNameStorage(geocodable, null);

            // If responce was not received than message not need to be shown.
            bool isResponceReceived = (bool)args[addressByPointCmd.IsResponceReceivedIndex];
            if (_IsAddressFieldsEmpty(geocodable.Address) && isResponceReceived)
            {
                _ShowReverseGeocodingErrorMessage(geocodable);
            }

            App.Current.Project.Save();
        }

        /// <summary>
        /// If address is empty show message.
        /// </summary>
        /// <param name="geocodable">Geocodable item.</param>
        private void _ShowReverseGeocodingErrorMessage(IGeocodable geocodable)
        {
            string name = string.Empty;
            string formatStr = string.Empty;

            // Extract name and format string.
            Order order = geocodable as Order;
            Location location = geocodable as Location;
            if (order != null)
            {
                name = order.Name;
                formatStr = (string)App.Current.FindResource(ORDER_FAR_FROM_ROAD_RESOURCE_NAME);
            }
            else if (location != null)
            {
                name = location.Name;
                formatStr = (string)App.Current.FindResource(LOCATION_FAR_FROM_ROAD_RESOURCE_NAME);
            }
            else
            {
                // Geocodable object must be order or location.
                Debug.Assert(false);
            }

            if (!string.IsNullOrEmpty(formatStr))
            {
                // Show error.
                string message = string.Format(formatStr, name);
                App.Current.Messenger.AddError(message);
            }
        }

        /// <summary>
        /// Is all fields of address is empty.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>Is all fields of address is empty.</returns>
        private bool _IsAddressFieldsEmpty(Address address)
        {
            Debug.Assert(address != null);

            bool isAddressEmpty = true;
            foreach (var addressPart in EnumHelpers.GetValues<AddressPart>())
            {
                if (!string.IsNullOrEmpty(address[addressPart]))
                {
                    isAddressEmpty = false;
                    break;
                }
            }

            return isAddressEmpty;
        }

        /// <summary>
        /// Show candidate selection window.
        /// </summary>
        private void _ExpandCandidateSelect()
        {
            Debug.Assert(_controlsGrid != null);
            Debug.Assert(_candidateSelect != null);
            Debug.Assert(_splitter != null);

            _controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].MinWidth = _expandedMinWidth;
            _controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].Width = _expanded;
            _candidateSelect.Visibility = Visibility.Visible;
            _splitter.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hide candidate selection window.
        /// </summary>
        private void _CollapseCandidateSelect()
        {
            Debug.Assert(_controlsGrid != null);
            Debug.Assert(_candidateSelect != null);
            Debug.Assert(_splitter != null);

            // Do nothing if "Did you mean" already collapsed.
            if (_controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].MinWidth == _collapsedMinWidth)
                return;

            if (_controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].ActualWidth != 0)
            {
                _expanded = _controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].Width;
            }

            _controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].MinWidth = _collapsedMinWidth;
            _controlsGrid.ColumnDefinitions[SELECTORGRIDINDEX].Width = _collapsed;
            _candidateSelect.Visibility = Visibility.Collapsed;
            _splitter.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Start geocoding.
        /// </summary>
        /// <param name="geocodable">Item to geocode.</param>
        /// <param name="useLocalAsPrimary">Is item just created.</param>
        private void _StartGeocoding(IGeocodable geocodable, bool useLocalAsPrimary)
        {
            CandidatesToZoom = null;

            _mapCtrl.HideZoomToCandidatePopup();

            // Do not need to start geocoding if geocodable object is far from road. Geocode will be failed.
            string farFromRoadMatchMethod = (string)App.Current.FindResource(MANUALLY_EDITED_XY_FAR_FROM_NEAREST_ROAD_RESOURCE_NAME);
            if (geocodable.Address.MatchMethod != null &&
                geocodable.Address.MatchMethod.Equals(farFromRoadMatchMethod, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // If try to geocode with edited address fields during another geocoding process.
            if (IsGeocodingInProcess)
            {
                _candidateSelect.CandidateChanged -= new EventHandler(_CandidateChanged);
                _Clear();
                _candidateSelect.CandidateChanged += new EventHandler(_CandidateChanged);
            }

            IsGeocodingInProcess = true;

            if (GeocodingStarted != null)
            {
                GeocodingStarted(this, EventArgs.Empty);
            }

            List<AddressCandidate> candidates = GeocodeHelpers.DoGeocode(geocodable, useLocalAsPrimary, true);
            var minimumMatchScore = App.Current.Geocoder.MinimumMatchScore;

            _ProcessGeocodingResults(geocodable, candidates, minimumMatchScore);

            _isAddressChanged = false;
        }

        /// <summary>
        /// Filters out candidates with the same address and nearby locations.
        /// </summary>
        /// <param name="candidates">The collection of candidates to be filtered.</param>
        /// <param name="minimumMatchScore">Specifies the minimum score value at which candidate
        /// will be treated as matched.</param>
        /// <returns>A filtered collection of candidates.</returns>
        private IEnumerable<AddressCandidate> _FilterSameCandidates(
            IEnumerable<AddressCandidate> candidates,
            int minimumMatchScore)
        {
            Debug.Assert(candidates != null);
            Debug.Assert(candidates.All(candidate => candidate != null));

            var sameCandidates = new Dictionary<string, List<AddressCandidate>>(
                StringComparer.OrdinalIgnoreCase);
            var distinctCandidates = new List<AddressCandidate>();

            foreach (var candidate in candidates)
            {
                var fullAddress = candidate.Address.FullAddress ?? string.Empty;
                if (!sameCandidates.ContainsKey(fullAddress))
                {
                    sameCandidates[fullAddress] = new List<AddressCandidate>();
                }

                var existingCandidates = sameCandidates[fullAddress];
                var sameCandidate = existingCandidates.FirstOrDefault(
                    existingCandidate => _CheckDistance(
                        existingCandidate.GeoLocation,
                        candidate.GeoLocation,
                        MAX_CANDIDATES_DISTANCE));

                var isDuplicate =
                    sameCandidate != null &&
                    (sameCandidate.Score >= candidate.Score ||
                    sameCandidate.Score >= minimumMatchScore);
                if (isDuplicate)
                {
                    continue;
                }

                sameCandidates[fullAddress].Add(candidate);
                distinctCandidates.Add(candidate);
            }

            return distinctCandidates;
        }

        /// <summary>
        /// Validates and fixes candidates with incorrect locations.
        /// </summary>
        /// <param name="candidates">The reference to the collection of address candidates to
        /// be validated and fixed.</param>
        /// <param name="geocodable">The reference to the geocodable object used for retrieving
        /// candidates collection.</param>
        /// <returns>A reference to the collection of address candidates with fixed
        /// locations.</returns>
        private IEnumerable<AddressCandidate> _GetValidLocations(
            IEnumerable<AddressCandidate> candidates,
            IGeocodable geocodable)
        {
            Debug.Assert(candidates != null);
            Debug.Assert(candidates.All(candidate => candidate != null));
            Debug.Assert(geocodable != null);

            List<int> incorrectCandidates = new List<int>();

            try
            {
                var streetsGeocoder = App.Current.StreetsGeocoder;
                var locationValidator = new LocationValidator(streetsGeocoder);

                incorrectCandidates = locationValidator
                    .FindIncorrectLocations(candidates)
                    .ToList();
            }
            catch (Exception ex)
            {
                if (GeocodeHelpers.MustThrowException(ex))
                {
                    throw;
                }
            }

            if (!incorrectCandidates.Any())
            {
                return candidates;
            }

            // Get incorrect address candidates.
            List<AddressCandidate> allCandidates = candidates.ToList();
            var invalidAddressCandidates = incorrectCandidates
                .Select(index => allCandidates[index])
                .ToList();

            // Get all candidates which is not invalid.
            var result = candidates
                .Except(invalidAddressCandidates)
                .ToList();

            return result;
        }

        /// <summary>
        /// Processes geocoding candidates.
        /// </summary>
        /// <param name="allCandidates">The reference to a collection of all geocoding
        /// candidates.</param>
        /// <param name="geocodable">The reference to an object to be geocoded.</param>
        /// <param name="minimumMatchScore">Specifies the minimum score value at which candidate
        /// will be treated as matched.</param>
        private void _ProcessGeocodingCandidates(
            IEnumerable<AddressCandidate> allCandidates,
            IGeocodable geocodable,
            int minimumMatchScore)
        {
            Debug.Assert(allCandidates != null);
            Debug.Assert(allCandidates.All(candidate => candidate != null));
            Debug.Assert(geocodable != null);

            var candidates = GeocodeHelpers.SortCandidatesByPrimaryLocators(
                App.Current.Geocoder,
                allCandidates);

            if (!candidates.Any())
            {
                _ProcessCandidatesNotFound(App.Current.Geocoder, geocodable, allCandidates.ToList());

                return;
            }

            candidates = _GetValidLocations(candidates, geocodable).ToList();

            var distinctCandidates = _FilterSameCandidates(candidates, minimumMatchScore).ToList();

            var matchedCandidates = distinctCandidates
                .Where(candidate => candidate.Score >= minimumMatchScore)
                .ToList();

            if (matchedCandidates.Count == 1)
            {
                _ProcessMatchFound(geocodable, matchedCandidates.First());
            }
            else if (matchedCandidates.Count > 0)
            {
                // Find candidates with 100% score.
                var maxScoreCandidates = matchedCandidates
                    .Where(candidate => candidate.Score == 100)
                    .ToList();

                if (maxScoreCandidates.Count == 1)
                    // If found ONE candidate with 100% score, then choose it.
                    _ProcessMatchFound(geocodable, maxScoreCandidates.First());
                else
                    // If not found 100% candidates or found more than ONE, show candidates.
                    _ProcessCandidatesFound(matchedCandidates);
            }
            else
            {
                var topCandidates = distinctCandidates
                    .OrderByDescending(candidate => candidate.Score)
                    .Take(MAX_CANDIDATE_COUNT)
                    .ToList();

                if (topCandidates.Count > 0)
                    _ProcessCandidatesFound(topCandidates);
                else
                    _ProcessCandidatesNotFound(App.Current.Geocoder, geocodable, allCandidates);
            }
        }

        /// <summary>
        /// Process geocoding result.
        /// </summary>
        /// <param name="geocodable">Item to geocode.</param>
        /// <param name="allCandidates">Candidates list from all locators, include not primary.</param>
        /// <param name="minimumMatchScore">Specifies the minimum score value at which candidate
        /// will be treated as matched.</param>
        private void _ProcessGeocodingResults(
            IGeocodable geocodable,
            List<AddressCandidate> allCandidates,
            int minimumMatchScore)
        {
            _ProcessGeocodingCandidates(allCandidates, geocodable, minimumMatchScore);

            if (IsGeocodingInProcess)
            {
                _ExpandCandidateSelect();
                _SetMapviewToCandidates();
                _mapCtrl.SetEditingMapLayersOpacity(true);

                // React on map size changing only if map is initialized.
                if (_mapCtrl.Map.IsInitialized())
                    _mapCtrl.map.SizeChanged += new SizeChangedEventHandler(_MapSizeChanged);
            }
            else
            {
                // In case of we have geocode candidates at start, but with newly address data we haven't.
                _CollapseCandidateSelect();

                // Current item can be not null only in case of geocoding in progress or editing in progress.
                if (!_geocodableGrid.IsItemBeingEdited)
                    _currentItem = null;
            }
        }

        /// <summary>
        /// End geocoding with match found. Save candidate position to edited item.
        /// </summary>
        /// <param name="geocodable">Item to geocode.</param>
        /// <param name="candidate">Matched candidate.</param>
        private void _ProcessMatchFound(IGeocodable geocodable, AddressCandidate candidate)
        {
            string manuallyEditedXY = (string)Application.Current.FindResource("ManuallyEditedXY");

            if (string.Equals(candidate.Address.MatchMethod, manuallyEditedXY))
            {
                geocodable.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(candidate.GeoLocation.X, candidate.GeoLocation.Y); ;
                candidate.Address.CopyTo(geocodable.Address);
            }
            else
            {
                GeocodeHelpers.SetCandidate(geocodable, candidate);
            }

            if (MatchFound != null)
                MatchFound(this, EventArgs.Empty);

            IsGeocodingInProcess = false;
        }

        /// <summary>
        /// Show "Did you mean" dialog.
        /// </summary>
        /// <param name="candidates">Candidates for current geocoded items.</param>
        private void _ProcessCandidatesFound(List<AddressCandidate> candidates)
        {
            _candidates = candidates;

            // In case of candidate array.
            IsGeocodingInProcess = true;
            // Show candidates on map layer.
            _candidatesLayer.Collection = _candidates;
            _candidateSelect.CandidateChanged += new EventHandler(_CandidateChanged);
            _candidateSelect.AddCandidates(_candidates);
            _parentLayer.Selectable = false;

            if (CandidatesFound != null)
                CandidatesFound(this, EventArgs.Empty);
        }

        /// <summary>
        /// Finish geocoding and try to zoom near.
        /// </summary>
        /// <param name="geocoder">Geocoder which found candidates.</param>
        /// <param name="geocodable">Item to geocode.</param>
        /// <param name="allCandidates">All candidates, returned for current geocoded item.</param>
        private void _ProcessCandidatesNotFound(IGeocoder geocoder, IGeocodable geocodable,
            IEnumerable<AddressCandidate> allCandidates)
        {
            // Set candidates to zoom.
            var candidatesFromNotPrimaryLocators =
                GeocodeHelpers.GetBestCandidatesFromNotPrimaryLocators(geocoder, geocodable, allCandidates);
            CandidatesToZoom = candidatesFromNotPrimaryLocators.ToArray();

            if (CandidatesNotFound != null)
                CandidatesNotFound(this, EventArgs.Empty);

            AddressCandidate zoomedCandidate = null;
            // Zoom to candidates from not primary locators.
            if (CandidatesToZoom != null && CandidatesToZoom.Length > 0)
            {
                MapExtentHelpers.ZoomToCandidates(_mapCtrl, CandidatesToZoom);
                zoomedCandidate = CandidatesToZoom[0];
            }

            // Show popup on map if not in fleet wizard.
            if (!ParentIsFleetWisard)
                _mapCtrl.ShowZoomToCandidatePopup(geocodable, CandidatesToZoom);

            IsGeocodingInProcess = false;
        }

        /// <summary>
        /// Save manual order geocoding results to local storage.
        /// </summary>
        /// <param name="geocodable">Geocoding object.</param>
        /// <param name="oldAddress">Address before geocoding.</param>
        private void _SaveToLocalAddressNameStorage(IGeocodable geocodable, Address oldAddress)
        {
            // Do nothing in case of geocodable object is location.
            Order order = geocodable as Order;
            if (order != null)
            {
                NameAddressRecord nameAddressRecord = CommonHelpers.CreateNameAddressPair(order, oldAddress);

                // Do save in local storage.
                App.Current.NameAddressStorage.InsertOrUpdate(nameAddressRecord,
                    App.Current.Geocoder.AddressFormat);
            }
        }

        /// <summary>
        /// End geocoding.
        /// </summary>
        /// <param name="needToCancelEdit">Is need to cancel edit. Is used because
        /// on "cancelling new item" call "cancelling new item" again.</param>
        private void _EndGeocoding(bool needToCancelEdit)
        {
            Debug.Assert(_mapCtrl != null);

            Debug.Assert(IsGeocodingInProcess);

            _mapCtrl.IgnoreSizeChanged = true;

            // Workaround - see method comment.
            CommonHelpers.FillAddressWithSameValues(_currentItem.Address);

            _mapCtrl.SetEditingMapLayersOpacity(false);

            IsGeocodingInProcess = false;
            _isAddressChanged = false;
            _parentLayer.Selectable = true;
            _Clear();
            _CollapseCandidateSelect();
            if (_currentItem != null)
            {
                _currentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
            }

            if (App.Current.Project != null)
            {
                App.Current.Project.Save();
            }

            // End edit in data grid.
            if (needToCancelEdit &&
                !_canceledByGrid &&
                _geocodableGrid.IsItemBeingEdited)
            {
                _EndEditInGrid();
            }

            _currentItem = null;
        }

        /// <summary>
        /// Start edit.
        /// </summary>
        /// <param name="item">Item to edit.</param>
        private void _StartEdit(object item)
        {
            Debug.Assert(_mapCtrl != null);
            Debug.Assert(_parentLayer != null);

            if (_isReccurent)
                return;

            _isReccurent = true;

            if (!_editStartedByGrid)
            {
                try
                {
                    // Do not make bring item to view because datagrid control may be null in case of unassigned orders context
                    // not opened in visible views.
                    _geocodableGrid.BringItemIntoView(item);

                    Debug.Assert(_itemToEdit == null);

                    _itemToEdit = item;
                    _mapCtrl.Dispatcher.BeginInvoke(new Action(_StartEditInGrid), DispatcherPriority.Input, null);
                    _editStartedByGrid = true;
                }
                catch (Exception ex)
                {
                    Debug.Assert(false);
                    Logger.Warning(ex);
                }
            }

            if (!IsInEditedMode)
            {
                IsInEditedMode = true;

                _mapCtrl.StartEdit(item);

                _parentLayer.Selectable = false;
            }

            _isReccurent = false;
        }

        /// <summary>
        /// Start edit in grid.
        /// </summary>
        private void _StartEditInGrid()
        {
            Debug.Assert(_itemToEdit != null);
            
            try
            {
                _geocodableGrid.BeginEdit(_itemToEdit);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            _editStartedByGrid = false;
            _itemToEdit = null;
        }

        /// <summary>
        /// Method end edits in the data grid.
        /// </summary>
        private void _EndEditInGrid()
        {
            if (_geocodableGrid.IsBeingEdited)
            {
                try
                {
                    // End edit in the data grid.
                    _geocodableGrid.EndEdit();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            else
            {
                // End edit in the Insertion Row.
                if (_geocodableGrid.InsertionRow.IsBeingEdited)
                {
                    try
                    {
                        _geocodableGrid.InsertionRow.EndEdit();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                else
                {
                    // Do nothing: nothing was edited.
                }
            }
        }

        /// <summary>
        /// Create candidates layer.
        /// </summary>
        private void _CreateCandidatesLayer()
        {
            Debug.Assert(_mapCtrl != null);

            List<AddressCandidate> coll = new List<AddressCandidate>();

            _candidatesLayer = new ObjectLayer(coll, typeof(AddressCandidate), false);

            _mapCtrl.AddLayer(_candidatesLayer);

            _candidatesLayer.Selectable = true;
            _candidatesLayer.SelectedItems.CollectionChanged += 
                new NotifyCollectionChangedEventHandler(_SelectedItemsCollectionChanged);
            _candidatesLayer.SingleSelection = true;
            _candidatesLayer.ConstantOpacity = true;
        }

        /// <summary>
        /// React on candidate change in list
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidateChanged(object sender, EventArgs e)
        {
            Debug.Assert(_mapCtrl != null);
            Debug.Assert(_candidateSelect != null);
            Debug.Assert(_candidates != null);
            Debug.Assert(_candidatesLayer != null);

            if (_inCollectionChanged)
                return;

            AddressCandidate candidate = _candidateSelect.GetCandidate();
            Debug.Assert(candidate != null);

            List<ESRI.ArcLogistics.Geometry.Point> points = new List<ESRI.ArcLogistics.Geometry.Point>();
            points.Add(candidate.GeoLocation);
            MapExtentHelpers.SetExtentOnCollection(_mapCtrl, points);

            // Find selected candidate index.
            int candidateIndex = -1;
            for (int index = 0; index < _candidates.Count; index++)
            {
                if (_candidates[index] == candidate)
                {
                    candidateIndex = index;
                    break;
                }
            }

            // select candidate on map
            Debug.Assert(candidateIndex >= 0);
            ObservableCollection<object> selected = _candidatesLayer.SelectedItems;
            selected.CollectionChanged -= new NotifyCollectionChangedEventHandler(_SelectedItemsCollectionChanged);
            if (selected.Count > 0)
            {
                selected.Clear();
            }
            selected.Add(candidate);
            selected.CollectionChanged += new NotifyCollectionChangedEventHandler(_SelectedItemsCollectionChanged);
        }

        /// <summary>
        /// Set map view to candidates
        /// </summary>
        private void _SetMapviewToCandidates()
        {
            Debug.Assert(_mapCtrl != null);
            Debug.Assert(_candidates != null);

            List<ESRI.ArcLogistics.Geometry.Point> points = new List<ESRI.ArcLogistics.Geometry.Point>();

            // add location to extent
            foreach (AddressCandidate candidate in _candidates)
            {
                points.Add(candidate.GeoLocation);
            }

            _mapCtrl.IgnoreSizeChanged = true;
            MapExtentHelpers.SetExtentOnCollection(_mapCtrl, points);
        }

        /// <summary>
        /// End geocoding. Clear all data.
        /// </summary>
        private void _Clear()
        {
            _candidateSelect.CandidateChanged -= new EventHandler(_CandidateChanged);
            IsGeocodingInProcess = false;

            // Candidates layers are null if geocoder
            // or map is not initialized.
            if (_mapCtrl.Map.IsInitialized() &&
                App.Current.InternalGeocoder.IsInitialized())
            {
                _candidateSelect.ClearList();
                _candidatesLayer.Collection = null;
            }

            CandidatesToZoom = null;
        }

        /// <summary>
        /// Is geocoding in progress.
        /// </summary>
        private bool _IsGeocodingInProgress()
        {
            return IsGeocodingInProcess;
        }

        /// <summary>
        /// Method set value to AddressByPointTool if it is exists.
        /// </summary>
        /// <param name="value">Value to set.</param>
        private void _SetAddressByPointToolEnabled(bool value)
        {
            if (_addressByPointTool != null)
                _addressByPointTool.IsEnabled = value;
        }

        #endregion

        #region Private event handlers

        /// <summary>
        /// Changing routes, orders, stop layers selectable
        /// property depending on tool activation.
        /// </summary>
        /// <param name="sender">AddressByPointTool.</param>
        /// <param name="e">Ignored.</param>
        private void _AddressByPointToolActivatedChanged(object sender, EventArgs e)
        {
            // If tool was activated - make all layers non selectable.
            // If tool was disactivated - make layers selectable back.
            bool canSelectLayers = !(sender as AddressByPointTool).IsActivated;

            // Mark layers with stop,route and orders as selectable/non-selectable.
            foreach (var layer in _mapCtrl.ObjectLayers.Where(x => x.LayerType == typeof(Route)
                || x.LayerType == typeof(Stop) || x.LayerType == typeof(Order)))
                layer.Selectable = canSelectLayers;
        }

        /// <summary>
        /// React on reverse geocoding complete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AddressByPointToolOnComplete(object sender, EventArgs e)
        {
            Debug.Assert(_mapCtrl != null);
            Debug.Assert(_addressByPointTool != null);

            ESRI.ArcLogistics.Geometry.Point point = new ESRI.ArcLogistics.Geometry.Point(
                _addressByPointTool.X.Value, _addressByPointTool.Y.Value);

            // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator
            if (_mapCtrl.Map.SpatialReferenceID.HasValue)
            {
                point = WebMercatorUtil.ProjectPointFromWebMercator(point, _mapCtrl.Map.SpatialReferenceID.Value);
            }

            System.Windows.Point systemPoint = new System.Windows.Point(point.X, point.Y);

            if (_geocodableGrid.SelectedItems.Count == 1 || _currentItem != null)
            {
                if (IsGeocodingInProcess)
                {
                    _EndGeocoding(false);
                }

                _DoReverseGeocode(systemPoint);

                _mapCtrl.map.UpdateLayout();

                _isAddressChanged = false;
            }
            else
            {
                Debug.Assert(false);
            }

            _mapCtrl.HideZoomToCandidatePopup();
        }

        /// <summary>
        /// React on map size changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.Assert(_mapCtrl != null);

            _mapCtrl.map.SizeChanged -= new SizeChangedEventHandler(_MapSizeChanged);
            _SetMapviewToCandidates();
        }

        /// <summary>
        /// React on change selection on map control.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(_candidatesLayer != null);
            Debug.Assert(_candidateSelect != null);

            Debug.Assert(_candidatesLayer.SelectedItems.Count < 2);
            if (_candidatesLayer.SelectedItems.Count == 1)
            {
                AddressCandidate candidate = (AddressCandidate)_candidatesLayer.SelectedItems[0];

                // Select candidate at list.
                _inCollectionChanged = true;
                _candidateSelect.SelectCandidate(candidate);
                _inCollectionChanged = false;
            }
            else
            {
                _candidateSelect.SelectCandidate(null);
            }
        }

        /// <summary>
        /// React on address changed in grid.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Address property changed event args.</param>
        private void _AddressPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != Address.PropertyNameMatchMethod)
            {
                _isAddressChanged = true;
            }
        }

        /// <summary>
        /// React on candidate applied.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidateApplied(object sender, EventArgs e)
        {
            Debug.Assert(_currentItem != null);
            Debug.Assert(_candidateSelect != null);

            AddressCandidate candidate = _candidateSelect.GetCandidate();
            Debug.Assert(candidate != null);

            Address oldAddress = (Address)_currentItem.Address.Clone();

            string manuallyEditedXY = (string)Application.Current.FindResource("ManuallyEditedXY");
            if (string.Equals(candidate.Address.MatchMethod, manuallyEditedXY))
            {
                _currentItem.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(candidate.GeoLocation.X, candidate.GeoLocation.Y); ;
                candidate.Address.CopyTo(_currentItem.Address);
            }
            else
            {
                // Update address values from candidate.
                GeocodeHelpers.SetCandidate(_currentItem, candidate);

                candidate.Address.CopyTo(_currentItem.Address);

                if (!GeocodeHelpers.IsParsed(_currentItem.Address) &&
                    App.Current.Geocoder.AddressFormat != AddressFormat.SingleField)
                {
                    GeocodeHelpers.ParseAndFillAddress(_currentItem.Address);
                }
            }

            _SaveToLocalAddressNameStorage(_currentItem, oldAddress);

            _EndGeocoding(true);

            if (MatchFound != null)
                MatchFound(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on candidate position applied.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidatePositionApplied(object sender, EventArgs e)
        {
            Debug.Assert(_currentItem != null);
            Debug.Assert(_candidateSelect != null);

            AddressCandidate candidate = _candidateSelect.GetCandidate();

            // Candidate is empty in case of double click on scroll bar.
            if (candidate != null)
            {
                GeocodeHelpers.SetCandidate(_currentItem, candidate);
                _SaveToLocalAddressNameStorage(_currentItem, null);

                _EndGeocoding(true);

                if (MatchFound != null)
                    MatchFound(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// React on candidate not selected.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CandidateLeaved(object sender, EventArgs e)
        {
            _EndGeocoding(true);

            if (CandidatesNotFound != null)
                CandidatesNotFound(this, EventArgs.Empty);
        }

        #endregion

        #region constants

        /// <summary>
        /// Index of selector grid.
        /// </summary>
        private const int SELECTORGRIDINDEX = 1;

        /// <summary>
        /// Match method for not geocoded items resource name.
        /// </summary>
        private const string MANUALLY_EDITED_XY_FAR_FROM_NEAREST_ROAD_RESOURCE_NAME = "ManuallyEditedXYFarFromNearestRoad";

        /// <summary>
        /// Message about order far from road resource name.
        /// </summary>
        private const string ORDER_FAR_FROM_ROAD_RESOURCE_NAME = "OrderNotFoundOnNetworkViolationMessage";

        /// <summary>
        /// Message about order far from road resource name.
        /// </summary>
        private const string LOCATION_FAR_FROM_ROAD_RESOURCE_NAME = "LocationNotFoundOnNetworkViolationMessage";

        /// <summary>
        /// The maximum number of candidates to let user select from when no candidate has good
        /// enough match score.
        /// </summary>
        private const int MAX_CANDIDATE_COUNT = 10;

        /// <summary>
        /// The maximum distance in meters between two candidate points when candidates could be
        /// treated as a single candidate.
        /// </summary>
        /// <remarks>We use Euclidean metrics to check distance, which is adequate for relatively
        /// small distances only (like 200 meters). Change distance algorithm to a more precise
        /// one before increasing the maximum distance value.</remarks>
        private const double MAX_CANDIDATES_DISTANCE = 200.0;
        #endregion

        #region Private Fields
        
        /// <summary>
        /// Current geocodable item.
        /// </summary>
        private IGeocodable _currentItem;

        /// <summary>
        /// Grid length of collapsed candidate control.
        /// </summary>
        private GridLength _collapsed;

        /// <summary>
        /// Grid length of visible candidate control.
        /// </summary>
        private GridLength _expanded;

        /// <summary>
        /// Minimal width of candidate control in collapsed state.
        /// </summary>
        private double _collapsedMinWidth;

        /// <summary>
        /// Minimal width of candidate control in visible state.
        /// </summary>
        private double _expandedMinWidth;

        /// <summary>
        /// Map control.
        /// </summary>
        private MapControl _mapCtrl;

        /// <summary>
        /// Candidate select control.
        /// </summary>
        private CandidateSelectControl _candidateSelect;

        /// <summary>
        /// Parent grid of candidate select control.
        /// </summary>
        private Grid _controlsGrid;

        /// <summary>
        /// Splitter between map and candidate select control.
        /// </summary>
        private GridSplitter _splitter;

        /// <summary>
        /// Layer, which shows candidates.
        /// </summary>
        private ObjectLayer _candidatesLayer;

        /// <summary>
        /// Layer, which contains geocodable objects.
        /// </summary>
        private ObjectLayer _parentLayer;

        /// <summary>
        /// Candidate collection.
        /// </summary>
        private List<AddressCandidate> _candidates;

        /// <summary>
        /// Flag, which indicates about changes in address fields of current item.
        /// </summary>
        private bool _isAddressChanged;

        /// <summary>
        /// Flag, which indicates "in candidate collection changed" state.
        /// </summary>
        private bool _inCollectionChanged;

        /// <summary>
        /// Tool for reverse geocoding.
        /// </summary>
        private AddressByPointTool _addressByPointTool;

        /// <summary>
        /// Is in reccurent edit.
        /// </summary>
        private bool _isReccurent;

        /// <summary>
        /// Flag, which indicates that edit was started by grid.
        /// </summary>
        private bool _editStartedByGrid;

        /// <summary>
        /// Flag, which indicates cancelling by grid.
        /// </summary>
        private bool _canceledByGrid;

        /// <summary>
        /// Item for postponed editing.
        /// </summary>
        private object _itemToEdit;
        
        /// <summary>
        /// Datagrid control with geocodable
        /// </summary>
        private DataGridControlEx _geocodableGrid;

        /// <summary>
        /// Flag to store hint visibility, which need to be showed after selection changed.
        /// </summary>
        private bool _isNewItemJustCommited;


        /// <summary>
        /// Geocodable type.
        /// </summary>
        private Type _geocodableType;

        #endregion
    }
}
