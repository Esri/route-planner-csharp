using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geometry;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class for helping to create regions such as point and polygons.
    /// </summary>
    class RegionsPage
    {
        #region Constructors

        /// <summary>
        /// Create RegionsPage.
        /// </summary>
        /// <param name="mapCtrl">Map from parent page.</param>
        /// <param name="dataGridControl">Grid from parent page.</param>
        /// <param name="parentLayer">Layer, that contains regions.</param>
        /// <param name="type">Semantic type of regions. Barrier or Zone.</param>
        /// <param name="layoutRoot">Parent page layout root.</param>
        /// <param name="mapBorder">Container element for map.</param>
        public RegionsPage(MapControl mapCtrl, DataGridControlEx dataGridControl, ObjectLayer parentLayer,
            Type type, Grid layoutRoot, Border mapBorder)
        {
            _mapCtrl = mapCtrl;
            _mapCtrl.CanSelectCallback = _CanSelect;
            _mapCtrl.StartEditRegionCallback = _EditStarted;
            _mapCtrl.EndEditRegionCallback = _EditEnded;

            _dataGridControl = dataGridControl;

            _parentLayer = parentLayer;

            _type = type;

            if (_type == typeof(Zone))
            {
                _polygonTool = new ZonePolygonTool();
                _polygonTool.OnComplete += new EventHandler(_PolygonToolOnComplete);
                _mapCtrl.AddTool(_polygonTool, _CanActivateZonePolygonTool);
            }
            else if (_type == typeof(Barrier))
            {
                _CreateBarrierTools();
            }
            else
                Debug.Assert(false);

            _gridAutoFitHelper = new GridAutoFitHelper(dataGridControl, layoutRoot, mapBorder);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// React on creating new item.
        /// </summary>
        /// <param name="e">Creating item event args.</param>
        public void OnCreatingNewItem(DataGridCreatingNewItemEventArgs e)
        {
            _mapCtrl.CurrentTool = null;

            _currentItem = (object)e.NewItem;

            // Graphic, which created to show not yet committed new item.
            Graphic graphic = _parentLayer.CreateGraphic(_currentItem);

            DataGraphicObject dataGraphicObject = graphic as DataGraphicObject;
            if (dataGraphicObject != null && _parentLayer.LayerContext != null)
                dataGraphicObject.ObjectContext = _parentLayer.LayerContext;

            _parentLayer.MapLayer.Graphics.Add(graphic);

            _EditStarted(_currentItem);
            _SetToolsEnabled(true);
        }

        /// <summary>
        /// React on cancelling new item.
        /// </summary>
        /// <param name="e">Cancelling item event args.</param>
        public void OnNewItemCancelling(DataGridItemHandledEventArgs e)
        {
            // Supporting API issues. Needed in case of external new item creating canceling.
            if (!_isInEditedMode)
                return;

            _canceledByGrid = true;
            ObjectLayer.DeleteObject(_currentItem, _parentLayer.MapLayer);
            _EditEnded(false);
            e.Handled = true;
            _canceledByGrid = false;

            _SetToolsEnabled(false);

            _currentItem = null;
        }

        /// <summary>
        /// React on new item commited.
        /// </summary>
        /// <param name="e">Committed item event args.</param>
        public void OnNewItemCommitted(DataGridItemEventArgs e)
        {
            _canceledByGrid = true;
            _EditEnded(false);
            _canceledByGrid = false;

            _SetToolsEnabled(false);

            _currentItem = null;
        }

        /// <summary>
        /// React on new item canceled.
        /// </summary>
        /// <param name="e">Cancelling item event args.</param>
        public void OnEditCanceled(DataGridItemEventArgs e)
        {
            _canceledByGrid = true;
            _EditEnded(false);
            _canceledByGrid = false;
        }

        /// <summary>
        /// React on commiting edited item.
        /// </summary>
        /// <param name="e">Committing item event args.</param>
        public void OnCommittingEdit(DataGridItemCancelEventArgs e)
        {
            _canceledByGrid = true;
            object obj = (object)e.Item;
            if (obj != _currentItem)
            {
                e.Cancel = true;
            }

            // If commiting canceled - don't end edit
            if (!e.Cancel)
                _EditEnded(false);
            _canceledByGrid = false;
        }

        /// <summary>
        /// React on beginning edit item.
        /// </summary>
        /// <param name="e">Beginning edit item event args.</param>
        public void OnBeginningEdit(DataGridItemCancelEventArgs e)
        {
            // If geocoding in process and try to edit not geocoding geocodable object - than cancel it.
            object current = (object)e.Item;

            // If new item is the same with currently editing item - cancel current editing.
            if (_currentItem == current && _IsAnyToolActivated())
                _CancelEdit();

            _currentItem = e.Item as object;
            _EditStarted(_currentItem);
        }

        /// <summary>
        /// React on project loaded.
        /// </summary>
        public void OnProjectLoaded()
        {
            if (_isInEditedMode)
            {
                _EditEnded(false);
            }

        }

        /// <summary>
        /// React on selection changed.
        /// </summary>
        /// <param name="e">Selection changed event args.</param>
        public void OnSelectionChanged(DataGridSelectionChangedEventArgs e)
        {
            bool isToolsEnabled = false;

            // If some tool is activated - cancel edit.
            if (_IsAnyToolActivated())
                _CancelEdit();

            if (e.SelectionInfos.Count > 0)
            {
                // Check is single selection.
                if (e.SelectionInfos[0].DataGridContext.SelectedItems.Count == 1)
                {
                    _currentItem = e.SelectionInfos[0].DataGridContext.SelectedItems[0];
                    isToolsEnabled = true;

                    // Save geometry.
                    _SaveGeometry();
                }
                else if (e.SelectionInfos[0].DataGridContext.SelectedItems.Count == 0 && _dataGridControl.IsBeingEdited)
                {
                    // In case of adding new item.
                    isToolsEnabled = true;
                    _initialGeometry = null;
                }
            }

            _SetToolsEnabled(isToolsEnabled);
        }

        /// <summary>
        /// Cancel edit and revert to initial geometry.
        /// </summary>
        public void CancelEdit()
        {
            // If there is no current item - do nothing.
            if (_currentItem == null)
                return;

			// If we are editing existing geometry, drawing new geometry
			// or barrier popup editor is opened we need to cancel changes.
            else if (_isInEditedMode || _IsAnyToolActivated() || _barrierPopupEditor != null)
            {
                // If current item is barrier or zone - cancel edit.
                if (_currentItem is Barrier || _currentItem is Zone)
                    _CancelEdit();
                // If current item is not barrier or zone - we cannot use this method.
                else
                    Debug.Assert(false);
            }
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Check that one of drawing tool is activated.
        /// </summary>
        /// <returns>'True' if one of drawing tools is activated, 'false' otherwise.</returns>
        private bool _IsAnyToolActivated()
        {
            if (_currentItem == null)
                return false;
            else if (_currentItem is Barrier)
                return _pointTool.IsActivated || _polygonTool.IsActivated || _polylineTool.IsActivated;
            else if (_currentItem is Zone)
                return _polygonTool.IsActivated;
            else
            {
                Debug.Assert(false);
                return false;
            }
        }

        /// <summary>
        /// Create map tools for barriers.
        /// </summary>
        private void _CreateBarrierTools()
        {
            _pointTool = new BarrierPointTool();
            _pointTool.OnComplete += new EventHandler(_PointToolOnComplete);
            _polygonTool = new BarrierPolygonTool();
            _polygonTool.OnComplete += new EventHandler(_PolygonToolOnComplete);
            _polylineTool = new BarrierPolylineTool();
            _polylineTool.OnComplete += new EventHandler(_PolylineToolOnComplete);

            // Add tools.
            List<IMapTool> tools = new List<IMapTool>();
            tools.Add(_pointTool);
            tools.Add(_polylineTool);
            tools.Add(_polygonTool);

            _mapCtrl.AddTools(tools.ToArray(), _CanActivateBarrierTool);
        }
        
        /// <summary>
        /// Can select on map handler.
        /// </summary>
        /// <param name="item">Item to select.</param>
        /// <returns>Is item can be selected.</returns>
        private bool _CanSelect(object item)
        {
            return true;
        }

        /// <summary>
        /// Can activate point tool handler.
        /// </summary>
        /// <returns>Is tool can be activated.</returns>
        private bool _CanActivateBarrierTool()
        {
            bool result = true;

            return result;
        }

        /// <summary>
        /// Can activate polygon tool handler.
        /// </summary>
        /// <returns>Is tool can be activated.</returns>
        private bool _CanActivateZonePolygonTool()
        {
            return true;
        }

        /// <summary>
        /// Can activate point tool handler.
        /// </summary>
        /// <returns>Is tool can be activated.</returns>
        private bool _CanActivateZonePointTool()
        {
            return true;
        }

        /// <summary>
        /// React on point tool complete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PointToolOnComplete(object sender, EventArgs e)
        {
            if (_dataGridControl.SelectedItems.Count == 1 || _currentItem != null)
            {
                if (_isInEditedMode)
                    _mapCtrl.ClearEditMarkers();

                Point point = new Point(_pointTool.X.Value, _pointTool.Y.Value);

                // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator.
                if (_mapCtrl.Map.SpatialReferenceID.HasValue)
                {
                    point = WebMercatorUtil.ProjectPointFromWebMercator(point, _mapCtrl.Map.SpatialReferenceID.Value);
                }

                if (_type == typeof(Zone))
                {
                    Zone zone = (Zone)_currentItem;
                    zone.Geometry = point;
                }
                else
                {
                    Barrier barrier = (Barrier)_currentItem;
                    barrier.Geometry = point;

                    _ShowBarrierEditor(barrier, point);
                }

                App.Current.Project.Save();
                _mapCtrl.map.UpdateLayout();

                if (_isInEditedMode)
                    _mapCtrl.FillEditMarkers(_currentItem);
            }
        }

        /// <summary>
        /// Show barrier editor on map
        /// </summary>
        /// <param name="barrier">Barrier to show.</param>
        /// <param name="point">Last added point.</param>
        private void _ShowBarrierEditor(Barrier barrier, Point point)
        {
            _barrierPopupEditor = new BarrierPopupEditor();

            //Subscribe to popupeditor events.
            _barrierPopupEditor.OnComplete += new EventHandler(_BarrierPopupEditorOnComplete);
            _barrierPopupEditor.OnCancel += new EventHandler(_BarrierPopupEditorOnCancel);

            Point projectedPoint = new Point(point.X, point.Y);
            if (_mapCtrl.Map.SpatialReferenceID.HasValue)
            {
                projectedPoint = WebMercatorUtil.ProjectPointToWebMercator(projectedPoint,
                    _mapCtrl.Map.SpatialReferenceID.Value);
            }

            // Get left down point.
            System.Windows.Point screenPoint =
                _mapCtrl.map.MapToScreen(new ESRI.ArcGIS.Client.Geometry.MapPoint(projectedPoint.X,
                    projectedPoint.Y));
            System.Windows.Rect rect = new System.Windows.Rect(
                screenPoint.X, screenPoint.Y - _barrierPopupEditor.MinHeight,
                _barrierPopupEditor.MinWidth, _barrierPopupEditor.MinHeight);

            Popup popup = _CreatePopup(_barrierPopupEditor, rect);
            _barrierPopupEditor.Initialize(barrier, popup);
        }

        /// <summary>
        /// If user pressed "cancel" in popup - then cancel edit and revert changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _BarrierPopupEditorOnCancel(object sender, EventArgs e)
        {
            _CancelEdit();
        }

        /// <summary>
        /// Reset tools and revert to initial geometry.
        /// </summary>
        private void _CancelEdit()
        {
            // Close popup if it is open.
            if (_barrierPopupEditor != null)
            {
                _barrierPopupEditor.Close();
                _barrierPopupEditor = null;
            }

            // Deactivate current tool.
            _SetToolsEnabled(false);

            // Enable tools.
            _SetToolsEnabled(true);

            // End edit.
            if (_isInEditedMode)
                _EditEnded(false);

            // Restore item's initial geometry.
            if (_currentItem is Barrier)
                (_currentItem as Barrier).Geometry = _initialGeometry;
            else if (_currentItem is Zone)
                (_currentItem as Zone).Geometry = _initialGeometry;
        }

        /// <summary>
        /// React on ok in barrier popup editor.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BarrierPopupEditorOnComplete(object sender, EventArgs e)
        {
            if (_isInEditedMode)
                _EditEnded(true);
            else
                _initialGeometry = (_currentItem as Barrier).Geometry;
        }

        /// <summary>
        /// Save current item's geometry to initial geometry.
        /// </summary>
        private void _SaveGeometry()
        {
            if (_currentItem is Barrier)
                _initialGeometry = (_currentItem as Barrier).Geometry;
            else if (_currentItem is Zone)
                _initialGeometry = (_currentItem as Zone).Geometry;
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// Create and init popup.
        /// </summary>
        /// <param name="child">Child of popup.</param>
        /// <param name="rect">Rect to show popup.</param>
        /// <returns>Created popup.</returns>
        private Popup _CreatePopup(object child, System.Windows.Rect rect)
        {
            Popup popup = new Popup();
            popup.AllowsTransparency = true;

            ContentControl control = new ContentControl();
            control.Content = child;

            popup.Child = control;
            popup.PlacementTarget = _mapCtrl;
            popup.PlacementRectangle = rect;
            popup.Visibility = System.Windows.Visibility.Visible;
            popup.IsOpen = true;

            return popup;
        }

        /// <summary>
        /// React on editing started.
        /// </summary>
        /// <param name="item">Item to edit.</param>
        private void _EditStarted(object item)
        {
            if (_isReccurent)
                return;

            _isInEditedMode = true;
            _currentItem = item;
            _isReccurent = true;
            _mapCtrl.StartEdit(item);
             _isReccurent = false;
            _parentLayer.Selectable = false;
            
            if (!_dataGridControl.IsBeingEdited)
            {
                _isReccurent = true;
                _dataGridControl.BeginEdit(item);
                _isReccurent = false;
            }
        }

        /// <summary>
        /// React on editing ended.
        /// </summary>
        /// <param name="commit">Is in case of editing in grid try to commit changes.</param>
        private void _EditEnded(bool commit)
        {
            if (_isReccurent)
                return;

            if (_barrierPopupEditor != null)
            {
                // If barrier popup editor is still showed - close it.
                _barrierPopupEditor.Close();
                _barrierPopupEditor.OnComplete -= new EventHandler(_BarrierPopupEditorOnComplete);
                _barrierPopupEditor = null;
            }

            _mapCtrl.EditEnded();
            if (_mapCtrl.IsInEditedMode)
            {
                _isReccurent = true;
                _mapCtrl.EditEnded();
                _isReccurent = false;
            }

            if (_dataGridControl.IsBeingEdited && !_canceledByGrid)
            {
                _isReccurent = true;

                // If command from map to commit was come, than try to end edit and commit
                // otherwise cancel edit.
                if (commit)
                {
                    _SaveGeometry();

                    try
                    {
                        _dataGridControl.EndEdit();
                    }
                    catch
                    {
                        _dataGridControl.CancelEdit();
                    }
                }
                else
                	_dataGridControl.CancelEdit();

                _isReccurent = false;
            }

            // Must be set false after commiting\cancelling in datagrid control.
            // Because of supporting API issues in cancelling.
            _isInEditedMode = false;

            if (_dataGridControl.SelectedItems.Count != 1)
                _SetToolsEnabled(false);

            _parentLayer.Selectable = true;
        }

        /// <summary>
        /// React on polygon tool complete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PolygonToolOnComplete(object sender, EventArgs e)
        {
            if (_dataGridControl.SelectedItems.Count == 1 || _currentItem != null)
            {
                Debug.Assert(_polygonTool.Geometry.Rings.Count == 1);
                ESRI.ArcGIS.Client.Geometry.PointCollection pointsCollection = _polygonTool.Geometry.Rings[0];

                ESRI.ArcLogistics.Geometry.Point[] points = new Point[pointsCollection.Count];

                for (int index = 0; index < pointsCollection.Count; index++)
                {
                    ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = pointsCollection[index];
                    Point point = new Point(mapPoint.X, mapPoint.Y);

                    // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator.
                    if (_mapCtrl.Map.SpatialReferenceID.HasValue)
                    {
                        point = WebMercatorUtil.ProjectPointFromWebMercator(point, _mapCtrl.Map.SpatialReferenceID.Value);
                    }

                    points[index] = point;
                }

                if (_isInEditedMode)
                    _mapCtrl.ClearEditMarkers();

                ESRI.ArcLogistics.Geometry.Polygon polygon = new Polygon(points);

                if (_type == typeof(Zone))
                {
                    Zone zone = (Zone)_currentItem;
                    zone.Geometry = polygon;
                }
                else
                {
                    Barrier barrier = (Barrier)_currentItem;
                    barrier.Geometry = polygon;

                    _ShowBarrierEditor(barrier, polygon.GetPoint(polygon.TotalPointCount - 1));
                }

                App.Current.Project.Save();

                if (_isInEditedMode)
                    _mapCtrl.FillEditMarkers(_currentItem);

                _mapCtrl.map.UpdateLayout();
            }
        }

        /// <summary>
        /// React on polyline tool complete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PolylineToolOnComplete(object sender, EventArgs e)
        {
            if (_dataGridControl.SelectedItems.Count == 1 || _currentItem != null)
            {
                Debug.Assert(_polylineTool.Geometry.Paths.Count == 1);
                ESRI.ArcGIS.Client.Geometry.PointCollection pointsCollection = _polylineTool.Geometry.Paths[0];

                ESRI.ArcLogistics.Geometry.Point[] points = new Point[pointsCollection.Count];

                for (int index = 0; index < pointsCollection.Count; index++)
                {
                    ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = pointsCollection[index];
                    Point point = new Point(mapPoint.X, mapPoint.Y);

                    // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator.
                    if (_mapCtrl.Map.SpatialReferenceID.HasValue)
                    {
                        point = WebMercatorUtil.ProjectPointFromWebMercator(point, _mapCtrl.Map.SpatialReferenceID.Value);
                    }

                    points[index] = point;
                }

                if (_isInEditedMode)
                    _mapCtrl.ClearEditMarkers();

                ESRI.ArcLogistics.Geometry.Polyline polyline = new Polyline(points);

                Barrier barrier = (Barrier)_currentItem;
                barrier.Geometry = polyline;
                barrier.BarrierEffect.BlockTravel = true;

                // Save this polyline as initial geometry.
                _initialGeometry = polyline;

                App.Current.Project.Save();

                if (_isInEditedMode)
                    _mapCtrl.FillEditMarkers(_currentItem);

                _mapCtrl.map.UpdateLayout();
            }
        }

        /// <summary>
        /// Set tools enabled.
        /// </summary>
        /// <param name="isEnabled">Is tool enabled.</param>
        private void _SetToolsEnabled(bool isEnabled)
        {
            if (_pointTool != null)
            {
                _pointTool.IsEnabled = isEnabled;
            }

            if (_polygonTool != null)
            {
                _polygonTool.IsEnabled = isEnabled;
            }

            if (_polylineTool != null)
            {
                _polylineTool.IsEnabled = isEnabled;
            }
        }


        #endregion

        #region Private Fields

        /// <summary>
        /// Current item.
        /// </summary>
        private object _currentItem;

        /// <summary>
        /// Is in edited mode.
        /// </summary>
        private bool _isInEditedMode;

        /// <summary>
        /// Map control.
        /// </summary>
        private MapControl _mapCtrl;

        /// <summary>
        /// Data grid control.
        /// </summary>
        private DataGridControlEx _dataGridControl;

        /// <summary>
        /// Tool for picking point geometry.
        /// </summary>
        private PickPointTool _pointTool;

        /// <summary>
        /// Tool for picking polygon geometry.
        /// </summary>
        private PickPolygonTool _polygonTool;

        /// <summary>
        /// Tool for picking polyline geometry.
        /// </summary>
        private PickPolylineTool _polylineTool;

        /// <summary>
        /// Regions type.
        /// </summary>
        private Type _type;

        /// <summary>
        /// Layer, which contains geocodable objects.
        /// </summary>
        private ObjectLayer _parentLayer;

        /// <summary>
        /// Is in reccurent edit.
        /// </summary>
        private bool _isReccurent;

        /// <summary>
        /// Flag, which indicates cancelling by grid.
        /// </summary>
        private bool _canceledByGrid;

        /// <summary>
        /// Helper for autofit items on page.
        /// </summary>
        private GridAutoFitHelper _gridAutoFitHelper;

        /// <summary>
        /// Popup editor for barriers.
        /// </summary>
        private BarrierPopupEditor _barrierPopupEditor;
        
        /// <summary>
        /// Initial geomtry, need for reverting changes if user cancel edit.
        /// </summary>
        private object _initialGeometry;

        #endregion
    }
}