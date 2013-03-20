using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.DragAndDrop;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class for supporting Drag and Drop in map view.
    /// </summary>
    internal class MapViewDragAndDropHelper
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mapView">Map view.</param>
        public MapViewDragAndDropHelper(MapView mapView)
        {
            Debug.Assert(mapView != null);

            _mapView = mapView;

            _InitEventHandlers();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            Debug.Assert(_mapView != null);
            Debug.Assert(_dragAndDropHelper != null);

            _mapView.mapCtrl.MouseMove += new MouseEventHandler(_MapCtrlMouseMove);
            _mapView.mapCtrl.MouseLeftButtonDown += new MouseButtonEventHandler(_MapCtrlMouseLeftButtonDown);

            _mapView.mapCtrl.map.Drop += new DragEventHandler(_MapDrop);
            _mapView.mapCtrl.map.DragOver += new DragEventHandler(_MapDragOverChanged);
            _mapView.mapCtrl.map.DragEnter += new DragEventHandler(_MapDragOverChanged);
            _mapView.mapCtrl.map.DragLeave += new DragEventHandler(_MapDragOverChanged);

            _dragAndDropHelper.DragStarted += new EventHandler(_DragAndDropHelperDragStarted);
            _dragAndDropHelper.DragEnded += new EventHandler(_DragAndDropHelperDragEnded);
        }

        /// <summary>
        /// React on map mouse leftbutton down.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _MapCtrlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Store item under mouse cursor.
            _clickedItem = _GetPointedItemByMouseArgs(e);
        }

        /// <summary>
        /// React on mouse move.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Mouse event args.</param>
        private void _MapCtrlMouseMove(object sender, MouseEventArgs e)
        {
            Debug.Assert(_mapView != null);

            // Check is drag need to be started.
            bool canDragSelectedItem = false;
            if (_mapView.SelectedItems.Count > 0 && _clickedItem != null)
            {
                canDragSelectedItem = _mapView.SelectedItems.Contains(_clickedItem);
            }

            // Start drag if needed.
            if (canDragSelectedItem && Mouse.LeftButton == MouseButtonState.Pressed && !_mapView.mapCtrl.IsInEditedMode &&
                (Keyboard.Modifiers & ModifierKeys.Shift) == 0 && (Keyboard.Modifiers & ModifierKeys.Control) == 0 &&
                !_isDragging)
            {
                _TryToStartDragging();
            }
        }

        /// <summary>
        /// Get pointed item on map control by original source.
        /// </summary>
        /// <param name="e">Mouse event args.</param>
        /// <returns>Pointed item.</returns>
        private object _GetPointedItemByMouseArgs(MouseEventArgs e)
        {
            Debug.Assert(_mapView != null);

            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element == null)
                return null;

            DataBinding dataBinding = element.DataContext as DataBinding;
            if (dataBinding == null)
                return null;

            if (!dataBinding.Attributes.ContainsKey(DataGraphicObject.DataKeyName))
            {
                // Check this is cluster.
                if (dataBinding.Attributes.ContainsKey(ALClusterer.COUNT_PROPERTY_NAME))
                {
                    int count = (int)dataBinding.Attributes[ALClusterer.COUNT_PROPERTY_NAME];
                    for (int index = 0; index < count; index++)
                    {
                        string attributeName = ALClusterer.GRAPHIC_PROPERTY_NAME + index.ToString();
                        DataGraphicObject dataGraphic = (DataGraphicObject)dataBinding.Attributes[attributeName];
                        if (_mapView.SelectedItems.Contains(dataGraphic.Data))
                            return dataGraphic.Data;
                    }
                }

                return null;
            }

            object item = dataBinding.Attributes[DataGraphicObject.DataKeyName];
            return item;
        }

        /// <summary>
        /// React on drag started.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DragAndDropHelperDragStarted(object sender, EventArgs e)
        {
            _isDragging = true;
        }

        /// <summary>
        /// React on drag ended.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DragAndDropHelperDragEnded(object sender, EventArgs e)
        {
            Debug.Assert(_mapView != null);

            _isDragging = false;
            _mapView.mapCtrl.UnexpandIfExpanded();
        }

        /// <summary>
        /// Method separates stops and orders from items control selection.
        /// </summary>
        /// <returns>Selected orders and stops.</returns>
        private Collection<Object> _GetSelectedStopsAndOrders()
        {
            Debug.Assert(_mapView != null);

            Collection<Object> selectedStopsAndOrders = new Collection<Object>();

            foreach (Object obj in _mapView.SelectedItems)
            {
                if (obj is Stop || obj is Order)
                    selectedStopsAndOrders.Add(obj);
            }

            return selectedStopsAndOrders;
        }

        /// <summary>
        /// Method checks is dragging allowed and starts dragging if possible.
        /// </summary>
        private void _TryToStartDragging()
        {
            Debug.Assert(_dragAndDropHelper != null);

            Collection<Object> selection = _GetSelectedStopsAndOrders();

            bool isDragAllowed = _dragAndDropHelper.IsDragAllowed(selection);

            if (isDragAllowed && selection.Count > 0)
            {
                _dragAndDropHelper.StartDragOrders(selection, DragSource.MapView);
            }
        }

        /// <summary>
        /// React on changed object under dragging object.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">DragOver event args.</param>
        private void _MapDragOverChanged(object sender, DragEventArgs e)
        {
            Debug.Assert(_dragAndDropHelper != null);

            bool allowDrop = false;
            FrameworkElement frameworkElement = (FrameworkElement)e.OriginalSource;
            if (frameworkElement.DataContext != null)
            {
                DataBinding dataBinding = (DataBinding)frameworkElement.DataContext;
                if (!dataBinding.Attributes.ContainsKey(DataGraphicObject.DataKeyName))
                {
                    return;
                }

                Object target = dataBinding.Attributes[DataGraphicObject.DataKeyName];
                ICollection<Order> draggingOrders = _dragAndDropHelper.GetDraggingOrders(e.Data);
                allowDrop = _dragAndDropHelper.DoesDropAllowed(target, draggingOrders);
            }

            if (!allowDrop)
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }

            e.Handled = true;
        }

        /// <summary>
        /// React on dropping on map.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Dropping event args.</param>
        private void _MapDrop(object sender, DragEventArgs e)
        {
            Debug.Assert(_dragAndDropHelper != null);
            Debug.Assert(_mapView != null);

            FrameworkElement frameworkElement = (FrameworkElement)e.OriginalSource;
            DataBinding dataBinding = (DataBinding)frameworkElement.DataContext;

            Debug.Assert(dataBinding != null);
            if (dataBinding != null)
            {
                System.Windows.Point position = e.GetPosition(_mapView.mapCtrl.map);

                Object targetData = dataBinding.Attributes[DataGraphicObject.DataKeyName];

                Route route = targetData as Route;
                IList<Stop> stops = null;
                if (route != null)
                {
                    Envelope extent = _GetExtentNearDroppedPoint(position);
                    stops = MapHelpers.FindNearestPreviousStops(route.Schedule, extent);
                }

                if (stops != null && stops.Count > 1)
                {
                    _DoPrepareDropOnMultipleStops(e.Data, stops);
                }
                else
                {
                    Stop nextStop = targetData as Stop;
                    _dragAndDropHelper.Drop(targetData, e.Data);
                }
            }
        }

        /// <summary>
        /// Show context menu or do drop in case of dropping ordesr on unique route.
        /// </summary>
        /// <param name="draggingData">Dragging orders.</param>
        /// <param name="nextStops">List of stops, that can be next in sequence.</param>
        private void _DoPrepareDropOnMultipleStops(IDataObject draggingData, IList<Stop> nextStops)
        {
            Debug.Assert(_dragAndDropHelper != null);

            IList<Stop> stops;
            Collection<Order> draggingOrders = _dragAndDropHelper.GetDraggingOrders(draggingData);

            string formatStr;
            if (draggingOrders.Count == 1)
            {
                formatStr = (string)App.Current.FindResource("RouteDropBeforeStopMenuItemHeaderFormatStr");
                stops = nextStops;
            }
            else
            {
                formatStr = (string)App.Current.FindResource("RouteDropMenuItemHeaderFormatStr");
                stops = new List<Stop>();

                // Make list of stops with unique routes.
                foreach (Stop stop in nextStops)
                {
                    bool isUnique = true;
                    foreach (Stop uniqueRouteStop in stops)
                    {
                        if (uniqueRouteStop.Route == stop.Route)
                        {
                            isUnique = false;
                            break;
                        }
                    }

                    if (isUnique)
                    {
                        stops.Add(stop);
                    }
                }
            }

            // In case of multiple dragged orders and dropping to one route no need to show context menu.
            if (stops.Count == 1)
            {
                if (draggingOrders.Count == 1)
                {
                    _dragAndDropHelper.Drop(stops[0], draggingData);
                }
                else
                {
                    _dragAndDropHelper.Drop(stops[0].Route, draggingData);
                }
            }
            else
            {
                _CreateContextMenuForDrop(draggingData, stops, formatStr);
            }
        }

        /// <summary>
        /// Create and show context menu.
        /// </summary>
        /// <param name="draggingData">Dragging orders.</param>
        /// <param name="stops">List of stops, that can be next in sequence.</param>
        /// <param name="formatStr">Format string fot creating menuitems header.</param>
        private void _CreateContextMenuForDrop(IDataObject draggingData, IList<Stop> stops, string formatStr)
        {
            Debug.Assert(_dragAndDropHelper != null);
            Debug.Assert(_mapView != null);

            Collection<Order> draggingOrders = _dragAndDropHelper.GetDraggingOrders(draggingData);
            List<MenuItem> dropTargets = new List<MenuItem>();
            foreach (Stop stop in stops)
            {
                MenuItem menuItem = new MenuItem();

                if (draggingOrders.Count == 1)
                {
                    menuItem.Header = string.Format(formatStr, stop.Route.Name, stop.Name);
                }
                else
                {
                    menuItem.Header = string.Format(formatStr, stop.Route.Name);
                }

                menuItem.Click += new RoutedEventHandler(_MenuItemClick);
                menuItem.DataContext = stop;
                menuItem.Template = (ControlTemplate)App.Current.FindResource("MapContextMenuItemTemplate");
                menuItem.IsEnabled = !stop.Route.IsLocked;
                dropTargets.Add(menuItem);
            }

            dropTargets.Sort(delegate(MenuItem menuItem1, MenuItem menuItem2)
            {
                return ((string)menuItem1.Header).CompareTo((string)menuItem2.Header);
            });

            ContextMenu menu = (ContextMenu)_mapView.LayoutRoot.FindResource("DropMenu");
            menu.ItemsSource = dropTargets;
            _mapView.mapCtrl.ContextMenu = menu;
            menu.Closed += new RoutedEventHandler(_MenuClosed);

            menu.PlacementTarget = _mapView.mapCtrl;
            menu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            menu.IsOpen = true;

            Debug.Assert(_droppingData == null);
            _droppingData = draggingData;
        }

        /// <summary>
        /// Do drop.
        /// </summary>
        /// <param name="sender">Clicked menu item.</param>
        /// <param name="e">Ignored.</param>
        private void _MenuItemClick(object sender, RoutedEventArgs e)
        {
            Debug.Assert(_dragAndDropHelper != null);
            Debug.Assert(_droppingData != null);

            MenuItem menuItem = (MenuItem)sender;
            Stop nextStop = (Stop)menuItem.DataContext;
            Collection<Order> draggingOrders = _dragAndDropHelper.GetDraggingOrders(_droppingData);

            if (draggingOrders.Count == 1)
            {
                _dragAndDropHelper.Drop(nextStop, _droppingData);
            }
            else
            {
                _dragAndDropHelper.Drop(nextStop.Route, _droppingData);
            }
        }

        /// <summary>
        /// React on menu closing.
        /// </summary>
        /// <param name="sender">Closed menu.</param>
        /// <param name="e">Ignored.</param>
        private void _MenuClosed(object sender, RoutedEventArgs e)
        {
            Debug.Assert(_mapView != null);

            ContextMenu menu = (ContextMenu)sender;
            foreach (MenuItem menuItem in menu.ItemsSource)
                menuItem.Click -= _MenuItemClick;

            _mapView.mapCtrl.ContextMenu = null;
            _droppingData = null;
        }

        /// <summary>
        /// Get extent near point.
        /// </summary>
        /// <param name="position">Dropping position.</param>
        /// <returns>Extent near point.</returns>
        private Envelope _GetExtentNearDroppedPoint(System.Windows.Point position)
        {
            Debug.Assert(_mapView != null);

            System.Windows.Point leftTopPoint = new System.Windows.Point(position.X - ROUTE_WIDTH, position.Y + ROUTE_WIDTH);
            System.Windows.Point rightBottomPoint = new System.Windows.Point(position.X + ROUTE_WIDTH, position.Y - ROUTE_WIDTH);
            ESRI.ArcGIS.Client.Geometry.MapPoint leftTopMapPoint = _mapView.mapCtrl.map.ScreenToMap(leftTopPoint);
            ESRI.ArcGIS.Client.Geometry.MapPoint rightBottomMapPoint = _mapView.mapCtrl.map.ScreenToMap(rightBottomPoint);

            ESRI.ArcLogistics.Geometry.Point leftTopPointOnMap = new ESRI.ArcLogistics.Geometry.Point(
                leftTopMapPoint.X, leftTopMapPoint.Y);
            ESRI.ArcLogistics.Geometry.Point rightBottomPointOnMap = new ESRI.ArcLogistics.Geometry.Point(
                rightBottomMapPoint.X, rightBottomMapPoint.Y);

            // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator.
            if (_mapView.mapCtrl.Map.SpatialReferenceID.HasValue)
            {
                leftTopPointOnMap = WebMercatorUtil.ProjectPointFromWebMercator(leftTopPointOnMap,
                    _mapView.mapCtrl.Map.SpatialReferenceID.Value);
                rightBottomPointOnMap = WebMercatorUtil.ProjectPointFromWebMercator(rightBottomPointOnMap,
                    _mapView.mapCtrl.Map.SpatialReferenceID.Value);
            }

            Envelope extent = new Envelope(leftTopPointOnMap.X, leftTopPointOnMap.Y,
                rightBottomPointOnMap.X, rightBottomPointOnMap.Y);
            return extent;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Route width on map.
        /// </summary>
        private static readonly byte ROUTE_WIDTH = (byte)App.Current.FindResource("RouteWidth");

        #endregion

        #region Private fields

        /// <summary>
        /// Parent map view.
        /// </summary>
        private MapView _mapView;

        /// <summary>
        /// Helper for drag and drop.
        /// </summary>
        private DragAndDropHelper _dragAndDropHelper = new DragAndDropHelper();

        /// <summary>
        /// Is dragging in progress.
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// Dropping data.
        /// </summary>
        private IDataObject _droppingData;

        /// <summary>
        /// Item, which was clicked by left mouse button.
        /// </summary>
        private object _clickedItem;

        #endregion
    }
}
