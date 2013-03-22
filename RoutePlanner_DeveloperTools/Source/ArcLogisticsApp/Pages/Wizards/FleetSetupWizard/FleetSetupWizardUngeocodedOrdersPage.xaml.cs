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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interaction logic for FleetSetupWizardUngeocodedOrdersPage.xaml
    /// </summary>
    internal partial class FleetSetupWizardUngeocodedOrdersPage : WizardPageBase,
        ISupportNext, ISupportCancel, ISupportBack
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public FleetSetupWizardUngeocodedOrdersPage()
        {
            InitializeComponent();

            Loaded += new RoutedEventHandler(_FleetSetupWizardUngeocodedOrdersPageLoaded);
            Unloaded += new RoutedEventHandler(_FleetSetupWizardUngeocodedOrdersPageUnloaded);
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public FleetSetupWizardUngeocodedOrdersPage(IList<Order> ungeocodedOrders)
            : this()
        {
            _FillUngeocodedOrders(ungeocodedOrders);
        }

        #endregion

        #region ISupportBack members

        /// <summary>
        /// Interface for event, which sygnalyze about "Back" button in page pressed.
        /// </summary>
        public event EventHandler BackRequired;

        #endregion

        #region ISupportNext members

        /// <summary>
        /// Interface for event, which sygnalyze about "Next" button in page pressed.
        /// </summary>
        public event EventHandler NextRequired;

        #endregion

        #region ISupportCancel members

        /// <summary>
        /// Interface for event, which sygnalyze about "Cancel" button in page pressed.
        /// </summary>
        public event EventHandler CancelRequired;

        #endregion

        #region Private properties

        /// <summary>
        /// Current selected order.
        /// </summary>
        private Order CurrentItem
        {
            get
            {
                return DataGridControl.SelectedItem as Order;
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
        private void _FleetSetupWizardUngeocodedOrdersPageLoaded(object sender, RoutedEventArgs e)
        {
            // Init page if not inited yet.
            if (!_inited)
            {
                _isParentFleetWizard = DataKeeper != null;

                // Set row height.
                ContentGrid.RowDefinitions[DATA_GRID_ROW_DEFINITION_INDEX].Height =
                    new System.Windows.GridLength(DEFAULT_ROW_HEIGHT * ROW_COUNT + DataGridControl.Margin.Top
                    + DataGridControl.Margin.Bottom + ROW_COUNT);

                // Create subpages.
                string typeName = (string)App.Current.FindResource(ORDER_RESOURCE_NAME);
                typeName = typeName.ToLower();
                _matchFoundSubPage = new MatchFoundSubPage(typeName);
                _candidatesNotFoundSubPage = new CandidatesNotFoundSubPage(typeName);
                _candidatesFoundSubPage = new CandidatesFoundSubPage(typeName);

                _SetSubPage(null);

                // Init orders collection.
                _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);
                _CreateOrdersLayer();

                // Create and init geocodable page.
                _geocodablePage = new GeocodablePage(typeof(Order), mapCtrl, candidateSelect,
                    controlsGrid, DataGridControl, splitter, _ordersLayer);
                _geocodablePage.MatchFound += new EventHandler(_MatchFound);
                _geocodablePage.CandidatesFound += new EventHandler(_CandidatesFound);
                _geocodablePage.CandidatesNotFound += new EventHandler(_CandidatesNotFound);
                // Datakeeper is not null in fleetwizard.
                _geocodablePage.ParentIsFleetWisard = _isParentFleetWizard;

                mapCtrl.AddTool(new EditingTool(), null);

                _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);

                GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.FleetGeocodableGridStructure);
                structureInitializer.BuildGridStructure(_collectionSource, DataGridControl);

                CommonHelpers.HidePostalCode2Column(DataGridControl);

                // Load grid layout.
                GridLayoutLoader layoutLoader = new GridLayoutLoader(GridSettingsProvider.FleetLocationsSettingsRepositoryName,
                    _collectionSource.ItemProperties);
                layoutLoader.LoadLayout(DataGridControl);

                if (!_isParentFleetWizard)
                    _RemoveRedundantElements();

                _inited = true;
            }

            // Get orders collection from datakeeper if it is not null. Otherwise - from current project.
            if (_isParentFleetWizard)
            {
                // Fill ungeocoded orders list.
                _FillUngeocodedOrders(DataKeeper.AddedOrders);
            }
            else
            {
                // Do nothing. Ungeocoded order already set by constructor.
            }

            _collectionSource.Source = _ungeocodedOrders;
            _ordersLayer.Collection = _ungeocodedOrders;

            _selectionBinding.UnregisterAllCollections();
            _selectionBinding.RegisterCollection(DataGridControl);
            _selectionBinding.RegisterCollection(mapCtrl.SelectedItems);

            ButtonFinish.IsEnabled = _IsFinishButtonEnabled(_ungeocodedOrders);
        }

        /// <summary>
        /// If orders page called by optimize and edit page - hide redundant. 
        /// </summary>
        private void _RemoveRedundantElements()
        {
            AllOrdersMustBeGeocodedText.Visibility = Visibility.Collapsed;
            ButtonBack.Visibility = Visibility.Collapsed;
            ButtonCancel.Visibility = Visibility.Collapsed;
            GeocodeResultText.Visibility = Visibility.Collapsed;
            GeocodeHelperPanel.Visibility = Visibility.Collapsed;

            // Hide column container for Geocode helper panel.
            ContentGrid.ColumnDefinitions[1].MaxWidth = 0;

            // Remove tooltip because we can finish with ungeocoded orders.
            ButtonFinish.ToolTip = null;
        }

        /// <summary>
        /// Create layer for showing ungeocoded orders.
        /// </summary>
        private void _CreateOrdersLayer()
        {
            _ordersLayer = new ObjectLayer(new List<Order>(), typeof(Order), false);
            _ordersLayer.EnableToolTip();
            _ordersLayer.Selectable = false;

            mapCtrl.AddLayer(_ordersLayer);
        }

        /// <summary>
        /// React on page unloaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _FleetSetupWizardUngeocodedOrdersPageUnloaded(object sender, RoutedEventArgs e)
        {
            foreach (Order order in _ungeocodedOrders)
            {
                order.PropertyChanged -= new PropertyChangedEventHandler(_OrderPropertyChanged);
            }
        }

        /// <summary>
        /// React on order property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OrderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var order = (Order)sender;
            _UpdateOrdersProperties(order);

            ButtonFinish.IsEnabled = _IsFinishButtonEnabled(_ungeocodedOrders);
        }

        /// <summary>
        /// Updates properties for all orders with the same name as the specified one.
        /// </summary>
        /// <param name="sourceOrder">The reference to an
        /// <see cref="ESRI.ArcLogistics.DomainObjects.Order"/> object to read property values
        /// from.</param>
        private void _UpdateOrdersProperties(Order sourceOrder)
        {
            Debug.Assert(sourceOrder != null);

            var orders = _orderGroups[sourceOrder].Where(order => order != sourceOrder);
            foreach (var order in orders)
            {
                sourceOrder.CopyTo(order);
            }
        }

        /// <summary>
        /// Button "Finish" click handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonFinishClick(object sender, RoutedEventArgs e)
        {
            if (null != NextRequired)
                NextRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fill orders collection.
        /// </summary>
        private void _FillUngeocodedOrders(IEnumerable<Order> unassignedOrders)
        {
            Debug.Assert(unassignedOrders != null);
            Debug.Assert(unassignedOrders.All(order => order != null));
            Debug.Assert(unassignedOrders.All(order => !string.IsNullOrEmpty(order.Name)));

            var orderGroups = unassignedOrders
                .Where(order => !order.IsGeocoded)
                .GroupBy(order => new OrderKey(order))
                .ToDictionary(group => group.First(), group => group.ToList());

            _ungeocodedOrders.Clear();
            foreach (var order in orderGroups.Keys)
            {
                _ungeocodedOrders.Add(order);
                order.PropertyChanged += new PropertyChangedEventHandler(_OrderPropertyChanged);
            }

            _orderGroups = orderGroups;
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

            // Current item must have geolocation.
            Debug.Assert(CurrentItem.GeoLocation != null);

            // Remeber the current oreder, it can changed before invoke executes.
            Geometry.Point geoLocatedPoint = CurrentItem.GeoLocation.Value;

            // Zooming to current item suspended because zooming should go after suspended zoom
            // restoring in mapextentmanager. Suspended zoom restoring invoked in case of saved
            // old extent(zoom was changed by mouse) and because of map control size changed
            // after showing subpage.
            Dispatcher.BeginInvoke(new Action(delegate()
                {
                    _ZoomOnCurrentItem(geoLocatedPoint);
                }
                ),
                DispatcherPriority.Background);

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
        }

        /// <summary>
        /// Check is finish button enabled: parent page is optimize and edit or ungeocoded orders is absent.
        /// </summary>
        /// <param name="orders">Orders collection.</param>
        /// <returns>Is finish button enabled.</returns>
        private bool _IsFinishButtonEnabled(IList<Order> orders)
        {
            bool isFinishButtonEnabled = true;

            if (DataKeeper != null)
            {
                foreach (Order order in orders)
                {
                    if (!order.IsGeocoded)
                    {
                        isFinishButtonEnabled = false;
                        break;
                    }
                }
            }

            return isFinishButtonEnabled;
        }

        /// <summary>
        /// Set subpage.
        /// </summary>
        /// <param name="subPage">Subpage to set. Null if disable subpages.</param>
        private void _SetSubPage(IGeocodeSubPage subPage)
        {
            // Do not need to set subpages if page was not called from fleet wizard.
            if (!_isParentFleetWizard)
                return;

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
        /// React on delete button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonDeleteClick(object sender, RoutedEventArgs e)
        {
            // Cancel to prevent crash in case of item is not valid.
            if (DataGridControl.IsBeingEdited)
            {
                DataGridControl.CancelEdit();
            }

            Order orderToDelete = CurrentItem;
            orderToDelete.PropertyChanged -= _OrderPropertyChanged;

            IList<Order> orders = (IList<Order>)_collectionSource.Source;
            orders.Remove(orderToDelete);

            Debug.Assert(_orderGroups.ContainsKey(orderToDelete));
            foreach (var order in _orderGroups[orderToDelete])
            {
                // If datakeeper is not null - remove order from it.
                if (DataKeeper != null)
                {
                    DataKeeper.AddedOrders.Remove(order);
                }

                // Remove order from current project.
                App.Current.Project.Orders.Remove(order);
            }

            App.Current.Project.Save();

            // If all remaining orders geocoded - make finish button enabled.
            ButtonFinish.IsEnabled = _IsFinishButtonEnabled(_ungeocodedOrders);
        }

        /// <summary>
        /// React on locate button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonLocateClick(object sender, RoutedEventArgs e)
        {
            bool editingWasEndedSuccessfully = false;
            // Remove history items.
            try
            {
                // Endedit can lead to exception if name is empty and not commited yet.
                DataGridControl.EndEdit();
                editingWasEndedSuccessfully = true;
            }
            catch { }

            _ProcessLocateOrder(editingWasEndedSuccessfully);
        }

        /// <summary>
        /// React on cancel button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            if (null != CancelRequired)
                CancelRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on back button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonBackClick(object sender, RoutedEventArgs e)
        {
            DataGridControl.CancelEdit();

            if (null != CancelRequired)
                CancelRequired(this, EventArgs.Empty);

            if (null != BackRequired)
                BackRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Zoom on order if geocoded or start geocoding otherwise.
        /// </summary>
        /// <param name="editingWasEndedSuccessfully">Is Locate button was pressed and endedit was successful.</param>
        private void _ProcessLocateOrder(bool editingWasEndedSuccessfully)
        {
            if (CurrentItem.IsGeocoded)
            {
                _ZoomOnCurrentItem(CurrentItem.GeoLocation.Value);
            }
            else
            {
                // Geocoding possibly already started by endedit.
                if (!editingWasEndedSuccessfully)
                {
                    // Geocode order.
                    _geocodablePage.StartGeocoding(CurrentItem);
                }

                if (_geocodablePage.CandidatesToZoom != null)
                {
                    MapExtentHelpers.ZoomToCandidates(mapCtrl, _geocodablePage.CandidatesToZoom);
                }
            }
        }

        /// <summary>
        /// Zoom on point.
        /// </summary>
        /// <param name="point">Point to zoom at</param>
        private void _ZoomOnCurrentItem(Geometry.Point point)
        {
            // Current item can be null because zooming suspended.
            //if (CurrentItem != null)
            //{
                // Zoom on order.
                List<ESRI.ArcLogistics.Geometry.Point> points = new List<ESRI.ArcLogistics.Geometry.Point>();
                points.Add(point);
                MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
            //}
        }

        /// <summary>
        /// Set "Locate" button availability.
        /// </summary>
        private void _ProcessLocateState()
        {
            ButtonsPanel.IsEnabled = false;

            if (CurrentItem == null)
            {
                // If item is not selected - hide button.
                ButtonsPanel.Visibility = Visibility.Hidden;
            }
            else
            {
                if (GeocodeHelpers.IsActiveAddressFieldsEmpty(CurrentItem))
                {
                    // If address fields are empty - disable button.
                    ButtonsPanel.IsEnabled = false;
                }
                else
                {
                    _ShowLocateButton();
                    ButtonsPanel.IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// React on selection change.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (_inited)
            {
                if (_isSelectedByInternalGridLogic)
                {
                    DataGridControl.SelectedItems.Clear();
                    _isSelectedByInternalGridLogic = false;
                }
                else
                {
                    _geocodablePage.OnSelectionChanged(DataGridControl.SelectedItems);

                    _SetSubPage(null);
                    _ProcessLocateState();

                    if (CurrentItem != null)
                    {
                        _ProcessLocateOrder(false);
                    }
                }
            }
        }

        /// <summary>
        /// React on beginning edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            _ShowLocateButton();

            if (GeocodeHelpers.IsActiveAddressFieldsEmpty(CurrentItem))
            {
                ButtonsPanel.IsEnabled = false;
            }
            else
            {
                ButtonsPanel.IsEnabled = true;
            }

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

            CurrentItem.Address.PropertyChanged -= new PropertyChangedEventHandler(_AddressPropertyChanged);
            CurrentItem.PropertyChanged -= new PropertyChangedEventHandler(_CurrentItemPropertyChanged);
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
        /// Show locate button with corrected margin.
        /// </summary>
        private void _ShowLocateButton()
        {
            ButtonsPanel.Visibility = Visibility.Visible;

            // Call margin setter suspended because container is not correct after scrolling.
            Dispatcher.BeginInvoke(
                new Action(delegate()
                {
                    ButtonsPanel.Margin = CommonHelpers.GetItemContainerMargin(CurrentItem, DataGridControl, DEFAULT_ROW_HEIGHT);
                }
                ),
                DispatcherPriority.Background);
        }

        #endregion // Event handlers

        #region Private constants

        /// <summary>
        /// Collection source resource key name.
        /// </summary>
        private const string COLLECTION_SOURCE_KEY = "DataGridSource";

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
        private readonly int ROW_COUNT = 5;

        /// <summary>
        /// Order resource name.
        /// </summary>
        private const string ORDER_RESOURCE_NAME = "Order";

        /// <summary>
        /// Name string.
        /// </summary>
        private const string NAME_PROPERTY_STRING = "Name";

        #endregion

        #region Private fields

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
        private DataGridCollectionViewSource _collectionSource;

        /// <summary>
        /// Layer for showing orders.
        /// </summary>
        private ObjectLayer _ordersLayer;

        /// <summary>
        /// Flag for clearing selection after selecting first element by internal datagrid control internal logic.
        /// </summary>
        private bool _isSelectedByInternalGridLogic = true;

        /// <summary>
        /// Helper for synchronization selection between map layer and datagrid control.
        /// </summary>
        private MultiCollectionBinding _selectionBinding = new MultiCollectionBinding();

        /// <summary>
        /// Collection of ungeocoded orders.
        /// </summary>
        private ObservableCollection<Order> _ungeocodedOrders = new ObservableCollection<Order>();

        /// <summary>
        /// Flag to prevent cycle calling start geocoding after match found.
        /// </summary>
        private bool _skipStartGeocoding;

        /// <summary>
        /// Is parent fleet wizard. It depends on is data keeper not null.
        /// </summary>
        private bool _isParentFleetWizard;

        /// <summary>
        /// Groups orders with the same name but with different planned dates.
        /// </summary>
        private Dictionary<Order, List<Order>> _orderGroups;

        #endregion
    }
}
