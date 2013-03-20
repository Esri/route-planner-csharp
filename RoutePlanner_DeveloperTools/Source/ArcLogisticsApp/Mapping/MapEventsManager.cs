using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class for managing events on map.
    /// </summary>
    internal class MapEventsManager
    {
        #region Constructors

        /// <summary>
        /// Parent map control.
        /// </summary>
        /// <param name="mapControl">Parent map control.</param>
        /// <param name="mapSelectionManager">Map selection manager.</param>
        /// <param name="clustering">Clustering manager.</param>
        /// <param name="tools">Tools manager.</param>
        /// <param name="mapTips">Maptips manager.</param>
        public MapEventsManager(MapControl mapControl, MapSelectionManager mapSelectionManager, Clustering clustering, 
            MapTools tools, MapTips mapTips)
        {
            Debug.Assert(mapControl != null);
            Debug.Assert(clustering != null);
            Debug.Assert(tools != null);
            Debug.Assert(mapTips != null);
            Debug.Assert(mapSelectionManager != null);

            _mapControl = mapControl;
            _clustering = clustering;
            _tools = tools;
            _mapTips = mapTips;
            _mapSelectionManager = mapSelectionManager;

            _InitEventHandlers();

            _openHandCursor = ((TextBlock)mapControl.LayoutRoot.Resources["OpenHand"]).Cursor;
            _grabbedHandCursor = ((TextBlock)mapControl.LayoutRoot.Resources["GrabbedHand"]).Cursor;

            _mapControl.map.Cursor = _openHandCursor;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Current pointed graphic.
        /// </summary>
        public Graphic PointedGraphic
        {
            get;
            private set;
        }

        /// <summary>
        /// Last clicked coords.
        /// </summary>
        public System.Windows.Point? ClickedCoords
        {
            get;
            set;
        }

        /// <summary>
        /// Last cursor move position on map.
        /// </summary>
        public System.Windows.Point? LastCursorPos
        {
            get;
            private set;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Subscribe to graphics layer events.
        /// </summary>
        /// <param name="graphicsLayer">Graphic layer to subscribe.</param>
        public void RegisterLayer(GraphicsLayer graphicsLayer)
        {
            Debug.Assert(graphicsLayer != null);

            graphicsLayer.MouseLeftButtonDown +=
                new GraphicsLayer.MouseButtonEventHandler(_GraphicsLayerMouseLeftButtonDown);
            graphicsLayer.MouseMove += new GraphicsLayer.MouseEventHandler(_GraphicsLayerMouseMove);
            graphicsLayer.MouseLeftButtonUp +=
                new GraphicsLayer.MouseButtonEventHandler(_GraphicsLayerMouseLeftButtonUp);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            _mapControl.map.MouseMove += new MouseEventHandler(_MapMouseMove);
            _mapControl.map.MouseDown += new MouseButtonEventHandler(_MapMouseDown);
            _mapControl.map.PreviewMouseDown += new MouseButtonEventHandler(_MapPreviewMouseDown);
            _mapControl.map.PreviewMouseMove += new MouseEventHandler(_MapPreviewMouseMove);
            _mapControl.map.MouseUp += new MouseButtonEventHandler(_MapMouseUp);
            _mapControl.map.MouseEnter += new MouseEventHandler(_MapMouseEnter);
            _mapControl.map.MouseLeave += new MouseEventHandler(_MapMouseLeave);
            _mapControl.map.MouseWheel += new MouseWheelEventHandler(_MapMouseWheel);
            _mapControl.map.MouseDoubleClick += new MouseButtonEventHandler(_MapMouseDoubleClick);
            _mapControl.map.KeyDown += new KeyEventHandler(_MapKeyDown);

            App.Current.MainWindow.MouseMove += new MouseEventHandler(_MainWindowMouseMove);
        }

        /// <summary>
        /// React on main window mouse move.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="args">Mouse move event args.</param>
        private void _MainWindowMouseMove(object sender, MouseEventArgs args)
        {
            // Clear clickedgraphic in case of mouse button released.
            if (args.LeftButton == MouseButtonState.Released && _clickedGraphic != null)
            {
                _clickedGraphic = null;
            }

            // Clicked graphic is not DataGraphicObject in case of moving from cluster layer.
            DataGraphicObject clickedDataGraphic = _clickedGraphic as DataGraphicObject;
            if (clickedDataGraphic != null)
            {
                bool isNewlyCreated = _mapControl.SelectedItems.Count == 0 && clickedDataGraphic.Selected;

                // If selection can be changed by mouse move.
                if (!isNewlyCreated && !_mapControl.IsInEditedMode)
                {
                    if (!(Keyboard.Modifiers != ModifierKeys.Control &&
                         _mapControl.SelectedItems.Contains(clickedDataGraphic.Data)) && !(clickedDataGraphic is EditMarkerGraphicObject))
                    {
                        _mapSelectionManager.ProcessGraphicMouseEvents(_clickedGraphic, _clickedGraphic);
                    }
                }
            }

            _clickedGraphic = null;
        }

        /// <summary>
        /// React on mouse left button down.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Graphic, which was clicked by left button.</param>
        private void _GraphicsLayerMouseLeftButtonDown(object sender, ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs e)
        {
            _clickedGraphic = e.Graphic;

            if (_tools.CurrentTool != null)
            {
                // Get the x and y coordinates of the mouse pointer.
                System.Windows.Point position = e.GetPosition(_mapControl.map);
                MapPoint pointOnMap = _mapControl.map.ScreenToMap(position);

                e.Handled = true;

                _tools.CurrentTool.OnMouseDown(MouseButton.Left,
                    Keyboard.Modifiers, pointOnMap.X, pointOnMap.Y);
            }

            // Unexpand cluster if needed.
            bool graphicIsInExpandedLayer = _clustering.ClusteringLayer.MapLayer.Graphics.Contains(e.Graphic);
            if (_clustering.ExpandedClusterGraphic != e.Graphic && !graphicIsInExpandedLayer)
            {
                _clustering.UnexpandIfExpanded();
            }
        }

        /// <summary>
        /// React on mouse move on graphic.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Graphic, which under mouse.</param>
        private void _GraphicsLayerMouseMove(object sender, ESRI.ArcGIS.Client.GraphicMouseEventArgs e)
        {
            PointedGraphic = e.Graphic;
        }

        /// <summary>
        /// React on mouse left button up.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Graphic, which was clicked by left button.</param>
        private void _GraphicsLayerMouseLeftButtonUp(object sender, ESRI.ArcGIS.Client.GraphicMouseButtonEventArgs e)
        {
            _mapSelectionManager.ProcessGraphicMouseEvents(e.Graphic, _clickedGraphic);
        }

        /// <summary>
        /// React on mouse down event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Mouse button event args.</param>
        private void _MapMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the x and y coordinates of the mouse pointer.
            System.Windows.Point position = e.GetPosition(_mapControl.map);
            MapPoint pointOnMap = _mapControl.map.ScreenToMap(position);

            if (e.LeftButton == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left)
            {
                ClickedCoords = new System.Windows.Point(position.X, position.Y);
            }

            if (_tools.CurrentTool != null)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    _tools.CurrentTool.OnMouseDown(MouseButton.Left,
                        Keyboard.Modifiers, pointOnMap.X, pointOnMap.Y);

                    // Enable pan on editing and pickpoint tools.
                    if (_tools.CurrentTool != _tools.EditingTool && !(_tools.CurrentTool is PickPointTool))
                    {
                        e.Handled = true;
                    }

                    if (Mouse.OverrideCursor == null)
                    {
                        _overridedCursor = _mapControl.map.Cursor;
                        _mapControl.map.Cursor = _grabbedHandCursor;
                    }
                }
            }
            else
            {
                if (e.RightButton == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Right && !_mapControl.IsInEditedMode)
                {
                    _mapSelectionManager.ShowSelectionFrame(pointOnMap);
                }
                else if (e.LeftButton == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && _clickedGraphic == null)
                {
                   _mapControl.map.Cursor = _grabbedHandCursor;
                }
            }
        }

        /// <summary>
        /// React on mouse move event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Mouse event args.</param>
        private void _MapMouseMove(object sender, MouseEventArgs e)
        {
            if (_tools.CurrentTool is PickPolygonTool || _tools.CurrentTool is PickPolylineTool)
            {
                return;
            }

            // Get the x and y coordinates of the mouse pointer.
            LastCursorPos = e.GetPosition(_mapControl.map);

            _clustering.UnexpandIfNeeded(LastCursorPos.Value);

            MapPoint pointOnMap = _mapControl.map.ScreenToMap(LastCursorPos.Value);

            if (_tools.CurrentTool != null)
            {
                _tools.CurrentTool.OnMouseMove(e.LeftButton, e.RightButton,
                    e.MiddleButton, Keyboard.Modifiers,
                    pointOnMap.X, pointOnMap.Y);

                // Enable pan on editing and pickpoint tools.
                if ((_tools.CurrentTool != _tools.EditingTool ||
                    PointedGraphic != null) &&
                    !(_tools.CurrentTool is PickPointTool))
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    _mapSelectionManager.MoveSelectionFrame(pointOnMap);
                }

                _SetMapMoveCursor();
            }
        }

        /// <summary>
        /// React on mouse up event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Mouse button event args.</param>
        private void _MapMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Get the x and y coordinates of the mouse pointer.
            System.Windows.Point position = e.GetPosition(_mapControl.map);
            MapPoint pointOnMap = _mapControl.map.ScreenToMap(position);

            if (_tools.CurrentTool != null)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    _tools.CurrentTool.OnMouseUp(MouseButton.Left,
                        Keyboard.Modifiers, pointOnMap.X, pointOnMap.Y);
                }

                if (position.Equals(ClickedCoords) && !_mapSelectionManager.SelectionWasMade && _mapControl.IsInEditedMode &&
                    _clickedGraphic == null &&
                    e.LeftButton == MouseButtonState.Released && e.ChangedButton == MouseButton.Left)
                {
                    // Do not react on mouse click in case of geocoding in progress.
                    if ((_mapControl.IsGeocodingInProgressCallback != null && !_mapControl.IsGeocodingInProgressCallback()) ||
                        (_mapControl.IsGeocodingInProgressCallback == null))
                    {
                        _CallEndEditDelegate(true);
                    }
                }
            }
            else
            {
                _OnMouseUpWithoutTool(e);
            }

            // Change cursor to open hand if need to change cursor from grabbed hand to open.
            if (_mapControl.map.Cursor == _grabbedHandCursor && e.LeftButton == MouseButtonState.Released && e.ChangedButton == MouseButton.Left)
            {
                Mouse.OverrideCursor = null;
                if (!(_mapControl.CurrentTool is EditingTool))
                {
                    // If tool is used - return tool cursor. Otherwise - open hand.
                    if (_mapControl.CurrentTool != null)
                        _mapControl.map.Cursor = _mapControl.CurrentTool.Cursor;
                    else
                        _mapControl.map.Cursor = _openHandCursor;
                }
                else
                {
                    _mapControl.map.Cursor = _openHandCursor;
                }
            }

            _clickedGraphic = null;
        }

        /// <summary>
        /// React on preview mouse down.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mapSelectionManager.SelectionWasMade = false;
            _clickedGraphic = null;
        }

        /// <summary>
        /// React on preview mouse move.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapPreviewMouseMove(object sender, MouseEventArgs e)
        {
            PointedGraphic = null;
        }

        /// <summary>
        /// React on map mouse enter.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapMouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = _overridedCursor;
            _overridedCursor = null;
        }

        /// <summary>
        /// React on mouse leave.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapMouseLeave(object sender, MouseEventArgs e)
        {
            _mapSelectionManager.HideSelectionFrame();

            if (_tools.CurrentTool != null)
            {
                App.Current.Project.Save();
            }

            _mapControl.UnexpandIfExpanded();

            _overridedCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// React on mouse wheel.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _clustering.UnexpandIfExpanded();
        }

        /// <summary>
        /// React on mouse double click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Mouse button event args.</param>
        private void _MapMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(_mapControl.map);

            bool isGraphicEdited = !(_clickedGraphic is EditMarkerGraphicObject)
                && !(_clickedGraphic is CandidateGraphicObject);

            // Workaround:
            // due notundersandable map messages(1 graphic mouse up on 3 graphic mouse down) in case of
            // mouse down on graphic, pan, mouse up, doubleclick, 
            // need to check selection. if selected element differ from doubleclicked - deny editing
            DataGraphicObject dataGraphic = _clickedGraphic as DataGraphicObject;
            bool isSelected = true;
            if (dataGraphic != null && !_mapSelectionManager.SelectedItems.Contains(dataGraphic.Data))
                isSelected = false;

            if (_clickedGraphic != null && isGraphicEdited && !_mapControl.IsInEditedMode &&
                position.Equals(ClickedCoords) && isSelected)
            {
                if (dataGraphic != null && dataGraphic.Data != _mapControl.EditedObject)
                {
                    if (dataGraphic.Data is IGeocodable)
                    {
                        _mapSelectionManager.SelectionChangedFromMap = true;
                        _mapControl.StartEditGeocodableCallback(dataGraphic.Data);
                        _mapSelectionManager.SelectionChangedFromMap = false;
                    }
                    else if (dataGraphic.Data is Zone || dataGraphic.Data is Barrier)
                    {
                        _mapSelectionManager.SelectionChangedFromMap = true;
                        _mapControl.StartEditRegionCallback(dataGraphic.Data);
                        _mapSelectionManager.SelectionChangedFromMap = false;
                    }

                    _mapSelectionManager.SelectionWasMade = true;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// React on key down event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Key event args.</param>
        private void _MapKeyDown(object sender, KeyEventArgs e)
        {
            if (_mapControl.IsInEditedMode && (e.Key == Key.Enter || e.Key == Key.Escape))
            {
                // If user pressed "Enter" we need to commit, otherwise no.
                bool needCommit = e.Key == Key.Enter;
                
                // For zones and barriers we have other "Esc" button down event handler,
                // so if user is editing zone or barrier and press "Esc" we do nothing.
                if (needCommit || !(_mapControl.EditedObject is Barrier || _mapControl.EditedObject is Zone))
                    _CallEndEditDelegate(needCommit);
            }
        }

        /// <summary>
        /// Set cursor during moving over map.
        /// </summary>
        private void _SetMapMoveCursor()
        {
            if (PointedGraphic != null)
            {
                if (PointedGraphic is ClusterGraphicObject)
                {
                    // Remove overrided cursor to show hand on cluster.
                    if (Mouse.OverrideCursor != null)
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
                else
                {
                    // Show arrow cursor on all graphic objects except cluster. 
                    if (Mouse.OverrideCursor == null)
                    {
                        Mouse.OverrideCursor = Cursors.Arrow;
                    }
                }
            }
            else
            {
                // If cursor not over graphic object - disable arrow cursor.
                if (Mouse.OverrideCursor == Cursors.Arrow)
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        /// <summary>
        /// Process mouse up without activated tool.
        /// </summary>
        /// <param name="e">Mouse button event args.</param>
        private void _OnMouseUpWithoutTool(MouseButtonEventArgs e)
        {
            // Get the x and y coordinates of the mouse pointer.
            System.Windows.Point position = e.GetPosition(_mapControl.map);
            MapPoint pointOnMap = _mapControl.map.ScreenToMap(position);

            if (e.RightButton == MouseButtonState.Released && e.ChangedButton == MouseButton.Right)
            {
                _mapSelectionManager.FinishSelectByFrame(pointOnMap);
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Released && e.ChangedButton == MouseButton.Left &&
                    position.Equals(ClickedCoords) && !_mapSelectionManager.SelectionWasMade &&
                    // do not clear selection in case of clicked item == edited item
                    _clickedGraphic == null)
                {
                    // Do not react on mouse click in case of geocoding in progress
                    if (_mapControl.IsGeocodingInProgressCallback != null && !_mapControl.IsGeocodingInProgressCallback())
                    {
                        _mapSelectionManager.SelectedItems.Clear();

                        if (position.Equals(ClickedCoords) && !_mapSelectionManager.SelectionWasMade && _mapControl.IsInEditedMode &&
                            e.LeftButton == MouseButtonState.Released && e.ChangedButton == MouseButton.Left)
                        {
                            _CallEndEditDelegate(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Call delegate to end edit.
        /// </summary>
        /// <param name="commit">Is need to commit changes.</param>
        private void _CallEndEditDelegate(bool commit)
        {
            // Do not react on End Editing if map or geocoder
            // are not initialized.
            if (!_mapControl.Map.IsInitialized() ||
                !App.Current.InternalGeocoder.IsInitialized())
            {
                return;
            }

            if (_mapControl.EditedObject is IGeocodable)
            {
                _mapControl.EndEditGeocodableCallback(commit);
            }
            else if (_mapControl.EditedObject is Route)
            {
                _mapControl.EndEditRouteCallback(commit);
            }
            else if (_mapControl.EditedObject is Barrier || _mapControl.EditedObject is Zone)
            {
                _mapControl.EndEditRegionCallback(commit);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// Parent map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Clustering manager.
        /// </summary>
        private Clustering _clustering;

        /// <summary>
        /// Tools manager.
        /// </summary>
        private MapTools _tools;

        /// <summary>
        /// Maptips manager.
        /// </summary>
        private MapTips _mapTips;

        /// <summary>
        /// Map selection manager.
        /// </summary>
        private MapSelectionManager _mapSelectionManager;

        /// <summary>
        /// Last clicked graphic.
        /// </summary>
        private Graphic _clickedGraphic;

        /// <summary>
        /// Open hand cursor.
        /// </summary>
        private Cursor _openHandCursor;

        /// <summary>
        /// Grabbed hand cursor.
        /// </summary>
        private Cursor _grabbedHandCursor;

        /// <summary>
        /// Previous overrided cursor.
        /// </summary>
        private Cursor _overridedCursor;

        #endregion
    }
}
