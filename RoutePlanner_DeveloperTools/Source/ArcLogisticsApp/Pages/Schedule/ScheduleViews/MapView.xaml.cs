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
using System.Windows;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for MapView.
    /// </summary>
    internal partial class MapView : DockableContent
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public MapView()
        {
            InitializeComponent();

            _InitEventHandlers();

            _CreateMapViewLayers();

            mapCtrl.AddRegionsLayersToWidget = true;

            _mapViewDragAndDropHelper = new MapViewDragAndDropHelper(this);
        }

        #endregion

        #region public members

        /// <summary>
        /// Current selection.
        /// </summary>
        public IList SelectedItems
        {
            get
            {
                return mapCtrl.SelectedItems;
            }
        }

        /// <summary>
        /// Layer, which shows unassigned orders.
        /// </summary>
        public ObjectLayer UnassignedLayer
        {
            get
            {
                return _unassignedLayer;
            }
        }

        /// <summary>
        /// Optimize and edit page.
        /// </summary>
        public OptimizeAndEditPage ParentPage
        {
            get
            {
                return _schedulePage;
            }
            set
            {
                if (_schedulePage != null)
                {
                    _schedulePage.LockedPropertyChanged -= _SchedulePageLockedPropertyChanged;
                    _schedulePage.Loaded -= new RoutedEventHandler(_SchedulePageLoaded);

                    ContainerPane.PropertyChanged -= new PropertyChangedEventHandler(_ContainerPanePropertyChanged);
                }

                _schedulePage = value;

                _schedulePage.LockedPropertyChanged += new EventHandler(_SchedulePageLockedPropertyChanged);
                _schedulePage.Loaded += new RoutedEventHandler(_SchedulePageLoaded);
                
                // Support tools margin setting.
                ContainerPane.PropertyChanged += new PropertyChangedEventHandler(_ContainerPanePropertyChanged);
                _SetToolsMargin();

                OnScheduleLoad(false);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set collections from current schedule.
        /// </summary>
        /// <param name="afterRoutesBuilding">Bool value to define whether extent should be changed to builded routes.</param>
        public void OnScheduleLoad(bool afterRoutesBuilding)
        {
            if (_schedulePage != null)
            {
                Debug.Assert(_zonesLayer != null);

                bool dateChanged = false;

                if ((_currentSchedule != null && _schedulePage.CurrentSchedule != null &&
                    _currentSchedule.PlannedDate != _schedulePage.CurrentSchedule.PlannedDate)
                    || (_currentSchedule == null && _schedulePage.CurrentSchedule != null))
                {
                    dateChanged = true;
                }

                _currentSchedule = _schedulePage.CurrentSchedule;
                if (_currentSchedule != null)
                {
                    _InitCollections();

                    _zonesLayer.Collection = App.Current.Project.Zones;
                    _SetBarriersCollection();
                }
                else
                {
                    if (_stopOrders != null)
                    {
                        _stopOrders.Clear();
                        _stopLocations.Clear();
                    }

                    if (_routesLayer != null)
                    {
                        List<Route> empty = new List<Route>();
                        _routesLayer.Collection = empty;
                    }
                }

                if (dateChanged)
                {
                    if (mapCtrl.SelectedItems.Count > 0)
                    {
                        mapCtrl.SelectedItems.Clear();
                    }

                    List<object> scheduleExtentCollection = _GetScheduleExtentCollection();
                    List<ESRI.ArcLogistics.Geometry.Point> points = MapExtentHelpers.GetPointsInExtent(scheduleExtentCollection);

                    if (mapCtrl.StartupExtent != null)
                    {
                        MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
                    }
                    else
                    {
                        ESRI.ArcLogistics.Geometry.Envelope? extent = MapExtentHelpers.GetCollectionExtent(points);
                        mapCtrl.StartupExtent = extent;
                    }
                }

                // If schedule loaded after build routes finished.
                if (afterRoutesBuilding)
                {
                    List<object> routesExtentCollection = new List<object>();

                    // Add routes to extent.
                    foreach (Route route in _routesColl)
                    {
                        routesExtentCollection.Add(route);
                    }

                    mapCtrl.SetExtentOnCollection(routesExtentCollection);
                }

                _zonesLayer.LayerContext = _currentSchedule;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            App.Current.ApplicationInitialized += new EventHandler(_MapViewApplicationInitialized);
            App.Current.ProjectLoaded += new EventHandler(_MapViewProjectLoaded);

            App.Current.CurrentDateChanged += new EventHandler(_MapViewCurrentDateChanged);

            App.Current.MapDisplay.ShowBarriersChanged += new EventHandler(_MapDisplayShowBarriersChanged);
            App.Current.MapDisplay.ShowZonesChanged += new EventHandler(_MapDisplayShowZonesChanged);

            VisibileStateChanged += new EventHandler(_MapViewVisibileStateChanged);
        }

        /// <summary>
        /// Create layers.
        /// </summary>
        private void _CreateMapViewLayers()
        {
            _CreateZonesLayer();
            _CreateBarriersLayer();

            double layerOpacity = (double)Application.Current.FindResource("BarriersAndZonesOpacity");
            if (layerOpacity > MapControl.FullOpacity)
            {
                layerOpacity = MapControl.FullOpacity;
            }

            if (layerOpacity < 0)
            {
                layerOpacity = 0;
            }

            _barriersLayer.MapLayer.Opacity = layerOpacity;
            _zonesLayer.MapLayer.Opacity = layerOpacity;

            _InitCollections();

            _CreateRoutesLayer();

            _CreateStopsCollections();

            _CreateStopLocationsLayer();
            _CreateStopOrdersLayer();

            _CreateUnassignedLayer();
        }

        /// <summary>
        /// Create layer for showing project zones
        /// </summary>
        private void _CreateZonesLayer()
        {
            _zonesLayer = new ObjectLayer(null, typeof(Zone), false);

            _zonesLayer.EnableToolTip();
            _zonesLayer.MapLayer.Visible = App.Current.MapDisplay.ShowZones;
            _zonesLayer.ConstantOpacity = true;
            _zonesLayer.Name = (string)App.Current.FindResource("ZonesLayerName");
            _zonesLayer.IsBackgroundLayer = true;

            mapCtrl.AddLayer(_zonesLayer);
        }

        /// <summary>
        /// Create layer for showing project barriers for current date
        /// </summary>
        private void _CreateBarriersLayer()
        {
            _barriersLayer = new ObjectLayer(null, typeof(Barrier), false);

            _barriersLayer.EnableToolTip(); // REV: can we expose EnableToolTip as a property rather than a method
            _barriersLayer.MapLayer.Visible = App.Current.MapDisplay.ShowBarriers;
            _barriersLayer.ConstantOpacity = true;
            _barriersLayer.Name = (string)App.Current.FindResource("BarriersLayerName");
            _barriersLayer.IsBackgroundLayer = true;

            mapCtrl.AddLayer(_barriersLayer);
        }

        /// <summary>
        /// Init layers collections.
        /// </summary>
        private void _InitCollections()
        {
            if (_currentSchedule == null)
            {
                _routesColl = null;
                _unassignedOrdersColl = null;
            }
            else
            {
                Debug.Assert(_routesLayer != null);
                Debug.Assert(_unassignedLayer != null);

                _routesColl = (IEnumerable)_currentSchedule.Routes;
                _routesLayer.Collection = _routesColl;

                _unassignedOrdersColl = (IEnumerable)_currentSchedule.UnassignedOrders;
                _unassignedLayer.Collection = _unassignedOrdersColl;

                _CreateStopsCollections();
            }
        }

        /// <summary>
        /// Create layer for showing routes of current schedule.
        /// </summary>
        private void _CreateRoutesLayer()
        {
            _routesLayer = new ObjectLayer(_routesColl, typeof(Route), false);
            _routesLayer.EnableToolTip();
            mapCtrl.AddLayer(_routesLayer);
            _routesLayer.Selectable = true;
        }

        /// <summary>
        /// Create layer for showing stops associated with orders of current schedule.
        /// </summary>
        private void _CreateStopOrdersLayer()
        {
            _stopOrdersLayer = new ObjectLayer(_stopOrders, typeof(Stop), true);
            _stopOrdersLayer.EnableToolTip();
            _stopOrdersLayer.Selectable = true;
            mapCtrl.AddLayer(_stopOrdersLayer);
        }

        /// <summary>
        /// Create layer for showing stops associated with locations of current schedule.
        /// </summary>
        private void _CreateStopLocationsLayer()
        {
            _stopLocationsLayer = new ObjectLayer(_stopLocations, typeof(Stop), false);
            _stopLocationsLayer.EnableToolTip();
            _stopLocationsLayer.Selectable = true;
            mapCtrl.AddLayer(_stopLocationsLayer);
        }

        /// <summary>
        /// Create layer for showing unassigned orders of current schedule.
        /// </summary>
        private void _CreateUnassignedLayer()
        {
            _unassignedLayer = new ObjectLayer(_unassignedOrdersColl, typeof(Order), true, _stopOrdersLayer.MapLayer);
            _unassignedLayer.EnableToolTip();
            mapCtrl.AddLayer(_unassignedLayer);
            _unassignedLayer.Selectable = true;
        }

        /// <summary>
        /// Create collections of stop orders and stop locations from all stops in schedule.
        /// </summary>
        private void _CreateStopsCollections()
        {
            _stopOrders = new ObservableCollection<Stop>();
            _stopLocations = new ObservableCollection<Stop>();

            if (_stopOrdersLayer != null)
            {
                _stopOrdersLayer.Collection = _stopOrders;
                _stopLocationsLayer.Collection = _stopLocations;
            }

            if (_routesColl != null)
            {
                foreach (Route route in _routesColl)
                {
                    foreach (Stop stop in route.Stops)
                    {
                        if (stop.AssociatedObject is Order)
                        {
                            _stopOrders.Add(stop);
                        }
                        else if (stop.AssociatedObject is Location)
                        {
                            _stopLocations.Add(stop);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// React on schedule page locked property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _SchedulePageLockedPropertyChanged(object sender, EventArgs e)
        {
            if (!_schedulePage.IsLocked)
            {
                lockedGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                lockedGrid.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// React on schedule page loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _SchedulePageLoaded(object sender, RoutedEventArgs e)
        {
            // Set startup extent.
            List<object> scheduleExtentCollection = _GetScheduleExtentCollection();
            List<ESRI.ArcLogistics.Geometry.Point> points = MapExtentHelpers.GetPointsInExtent(scheduleExtentCollection);
            ESRI.ArcLogistics.Geometry.Envelope? extent = MapExtentHelpers.GetCollectionExtent(points);
            mapCtrl.StartupExtent = extent;

            // if new project loaded update extent
            if (_newProjectLoaded)
            {
                MapExtentHelpers.SetExtentOnCollection(mapCtrl, points);
                _newProjectLoaded = false;
            }
        }

        /// <summary>
        /// React on application initialized.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapViewApplicationInitialized(object sender, EventArgs e)
        {
            mapCtrl.Map = App.Current.Map;
        }

        /// <summary>
        /// React on project loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapViewProjectLoaded(object sender, EventArgs e)
        {
            Debug.Assert(_zonesLayer != null);

            _SetBarriersCollection();

            _zonesLayer.Collection = (IEnumerable)App.Current.Project.Zones;

            _newProjectLoaded = true;
        }

        /// <summary>
        /// React on current date changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapViewCurrentDateChanged(object sender, EventArgs e)
        {
            _SetBarriersCollection();
        }

        /// <summary>
        /// React on "Show Zones" option changed
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapDisplayShowZonesChanged(object sender, EventArgs e)
        {
            Debug.Assert(_zonesLayer != null);

            _zonesLayer.MapLayer.Visible = App.Current.MapDisplay.ShowZones;
        }

        /// <summary>
        /// React on "Show Barriers" option changed
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapDisplayShowBarriersChanged(object sender, EventArgs e)
        {
            Debug.Assert(_barriersLayer != null);

            _barriersLayer.MapLayer.Visible = App.Current.MapDisplay.ShowBarriers;
        }

        /// <summary>
        /// React on visibility of view changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapViewVisibileStateChanged(object sender, EventArgs e)
        {
            if (_schedulePage != null && _schedulePage.EditingManager.EditedObject != null
                && !_schedulePage.GeocodablePage.IsGeocodingInProcess)
            {
                _schedulePage.CancelObjectEditing();
            }
        }

        /// <summary>
        /// Set barriers collection for current date
        /// </summary>
        private void _SetBarriersCollection()
        {
            Debug.Assert(_barriersLayer != null);

            if (_barriersLayer.Collection != null)
            {
                IDisposable disposable = (IDisposable)_barriersLayer.Collection;
                disposable.Dispose();
            }

            _barriersLayer.Collection = (IDataObjectCollection<Barrier>)
                App.Current.Project.Barriers.Search(App.Current.CurrentDate, true);
        }

        /// <summary>
        /// Get objects, which must be in startup extent.
        /// </summary>
        /// <returns>Objects, which must be in startup extent.</returns>
        private  List<object> _GetScheduleExtentCollection()
        {
            List<object> scheduleList = new List<object>();

            IDataObjectCollection<Order> unassignedOrders = (IDataObjectCollection<Order>)_unassignedOrdersColl;
            if (_stopOrders.Count + unassignedOrders.Count > 0)
            {
                if (_routesColl != null)
                {
                    // Add stops to extent.
                    foreach (Stop stop in _stopOrders)
                    {
                        scheduleList.Add(stop);
                    }

                    // Add routes to extent.
                    foreach (Route route in _routesColl)
                    {
                        scheduleList.Add(route);
                    }
                }

                if (_unassignedOrdersColl != null)
                {
                    // Add orders to extent.
                    foreach (Order order in _unassignedOrdersColl)
                    {
                        scheduleList.Add(order);
                    }
                }
            }
            else
            {
                // Add zones to extent.
                foreach (Zone zone in App.Current.Project.Zones)
                {
                    scheduleList.Add(zone);
                }

                // Add locations to extent.
                foreach (Location location in App.Current.Project.Locations)
                {
                    scheduleList.Add(location);
                }
            }

            return scheduleList;
        }

        /// <summary>
        /// React on container pane property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event args.</param>
        private void _ContainerPanePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(DockablePane.PROP_NAME_State, StringComparison.OrdinalIgnoreCase))
            {
                _SetToolsMargin();
            }
        }

        /// <summary>
        /// Set margins for tools panel.
        /// </summary>
        private void _SetToolsMargin()
        {
            if (ContainerPane.State == PaneState.Docked)
            {
                double topOffset = (double)Application.Current.FindResource("DockedWindowHeaderHeight");
                mapCtrl.ToolsMargin = new Thickness(0, topOffset, 0, 0);
            }
            else
            {
                mapCtrl.ToolsMargin = new Thickness(0, 0, 0, 0);
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Layer, which shows unassigned orders.
        /// </summary>
        private ObjectLayer _unassignedLayer;

        /// <summary>
        /// Collection of unassigned orders of current schedule.
        /// </summary>
        private IEnumerable _unassignedOrdersColl;

        /// <summary>
        /// Layer, which shows routes.
        /// </summary>
        private ObjectLayer _routesLayer;

        /// <summary>
        /// Collection of routes of current schedule.
        /// </summary>
        private IEnumerable _routesColl;

        /// <summary>
        /// Layer, which shows stops, associated with orders.
        /// </summary>
        private ObjectLayer _stopOrdersLayer;

        /// <summary>
        /// Collection of stops, associated with orders, of current schedule.
        /// </summary>
        private ObservableCollection<Stop> _stopOrders;

        /// <summary>
        /// Collection of stops, associated with locations, of current schedule.
        /// </summary>
        private ObservableCollection<Stop> _stopLocations;

        /// <summary>
        /// Layer, which shows stops, associated with locations.
        /// </summary>
        private ObjectLayer _stopLocationsLayer;

        /// <summary>
        /// Layer, which shows zones.
        /// </summary>
        private ObjectLayer _zonesLayer;

        /// <summary>
        /// Layer, which shows barriers.
        /// </summary>
        private ObjectLayer _barriersLayer;

        /// <summary>
        /// Current schedule.
        /// </summary>
        private Schedule _currentSchedule;

        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _schedulePage;

        /// <summary>
        /// Drag & Drop helper.
        /// </summary>
        private MapViewDragAndDropHelper _mapViewDragAndDropHelper;

        /// <summary>
        /// New project loaded flag.
        /// </summary>
        private bool _newProjectLoaded;

        #endregion
    }
}
