using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Tools
{
    /// <summary>
    /// Editing on map tool.
    /// </summary>
    class EditingTool : IMapTool
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditingTool()
        {
            _popupAddress = new AddressPopupHelper();
        }

        #endregion

        #region public members

        /// <summary>
        /// Current edited object.
        /// </summary>
        public object EditingObject
        {
            get
            {
                return _editingObject;
            }
            set
            {
                _editingObject = value;
                _popupAddress.EditingObject = value;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Set markers layer.
        /// </summary>
        /// <param name="markersLayer">Parent map control markers layer.</param>
        public void SetLayer(ObjectLayer markersLayer)
        {
            Debug.Assert(_markersLayer == null);
            Debug.Assert(markersLayer != null);

            _markersLayer = markersLayer;
        }

        #endregion

        #region ITool members

        /// <summary>
        /// Initializes tool with map control.
        /// </summary>
        /// <param name="mapControl">Map control.</param>
        public void Initialize(MapControl mapControl)
        {
            Debug.Assert(mapControl != null);

            _cursor = mapControl.map.Cursor;

            _mapControl = mapControl;
            _moveShapeCursor = ((TextBlock)mapControl.LayoutRoot.Resources[MOVE_SHAPE_CURSOR_RESOURCE_NAME]).Cursor;
            _moveVertexCursor = ((TextBlock)mapControl.LayoutRoot.Resources[MOVE_VERTEX_CURSOR_RESOURCE_NAME]).Cursor;

            _popupAddress.Initialize(_mapControl);
        }

        /// <summary>
        /// Tool's cursor.
        /// </summary>
        public Cursor Cursor
        {
            get
            {
                return _cursor;
            }
        }

        // <summary>
        // Is tool enabled.
        // </summary>
        public bool IsEnabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Is tool activated.
        /// </summary>
        public bool IsActivated
        {
            get
            {
                return _isActivated;
            }
            private set
            {
                _isActivated = value;
                _NotifyActivatedChanged();
            }
        }

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        public string TooltipText
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Tool's title text.
        /// </summary>
        public string Title
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Icon's URI source.
        /// </summary>
        public string IconSource 
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Called when tool is activated on toolbar.
        /// </summary>
        public void Activate()
        {
            IsActivated = true;
        }

        /// <summary>
        /// Called when tool is deactivated on toolbar.
        /// </summary>
        public void Deactivate()
        {
            IsActivated = false;
            EditingObject = null;
            _EndPan();
            _editingInProcess = false;
        }
        
        public bool OnContextMenu(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void OnDblClick(ModifierKeys modifierKeys, double x, double y)
        {
            throw new NotImplementedException();
        }

        public void OnKeyDown(int keyCode, int shift)
        {
        }

        public void OnKeyUp(int keyCode, int shift)
        {
        }

        /// <summary>
        /// React on mouse down.
        /// </summary>
        /// <param name="pressedButton">Pressed mouse button.</param>
        /// <param name="modifierKeys">Modifier keys state.</param>
        /// <param name="x">X coord.</param>
        /// <param name="y">Y coord.</param>
        public void OnMouseDown(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y)
        {
            Debug.Assert(_mapControl != null);

            if (_mapControl.PointedGraphic != null)
            {
                Point point = new Point(x, y);

                // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator.
                if (_mapControl.Map.SpatialReferenceID.HasValue)
                {
                    point = WebMercatorUtil.ProjectPointFromWebMercator(point, _mapControl.Map.SpatialReferenceID.Value);
                }

                // Save current position.
                _previousX = point.X;
                _previousY = point.Y;

                // Start dragging.
                DataGraphicObject pointedGraphicObject = _mapControl.PointedGraphic as DataGraphicObject;
                if (pointedGraphicObject != null && 
                    (pointedGraphicObject.Data == EditingObject || pointedGraphicObject is EditMarkerGraphicObject))
                {
                    _editingInProcess = true;
                    _editedGraphic = pointedGraphicObject;
                    _StartPan();
                }
            }
            else
            {
                _editingInProcess = false;
            }
        }

        /// <summary>
        /// React on mouse move.
        /// </summary>
        /// <param name="left">Left button state.</param>
        /// <param name="right">Right button state.</param>
        /// <param name="middle">Middle button state.</param>
        /// <param name="modifierKeys">Modifier keys state.</param>
        /// <param name="x">X coord.</param>
        /// <param name="y">Y coord.</param>
        public void OnMouseMove(MouseButtonState left, MouseButtonState right, MouseButtonState middle,
            ModifierKeys modifierKeys, double x, double y)
        {
            Debug.Assert(_mapControl != null);

            if (_editingInProcess && left == MouseButtonState.Pressed)
            {
                Debug.Assert(_editedGraphic != null);

                Point point = new Point(x, y);

                // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator.
                if (_mapControl.Map.SpatialReferenceID.HasValue)
                {
                    point = WebMercatorUtil.ProjectPointFromWebMercator(point, _mapControl.Map.SpatialReferenceID.Value);
                }

                double dx = point.X - _previousX;
                double dy = point.Y - _previousY;

                // Save current position.
                _previousX = point.X;
                _previousY = point.Y;

                // Do pan object.
                object obj = _editedGraphic.Data;
                if (obj is EditingMarker || obj is IGeocodable || obj is Zone || obj is Barrier)
                {
                    _PanObject(EditingObject, dx, dy);
                }
                else
                    Debug.Assert(false);
            }
            else
            {
                // If can start drag - change cursor.
                Cursor newCursor = null;
                DataGraphicObject pointedGraphicObject = _mapControl.PointedGraphic as DataGraphicObject;
                if (pointedGraphicObject != null)
                {
                    if (pointedGraphicObject.Data == EditingObject || pointedGraphicObject is EditMarkerGraphicObject)
                    {
                        bool multiple = false;
                        if (pointedGraphicObject is EditMarkerGraphicObject)
                        {
                            EditMarkerGraphicObject editMarkerGraphicObject = (EditMarkerGraphicObject)pointedGraphicObject;
                            multiple = editMarkerGraphicObject.EditingMarker.MultipleIndex > -1;
                        }
                        if (multiple)
                            newCursor = _moveVertexCursor;
                        else
                            newCursor = _moveShapeCursor;
                    }
                }
                _ChangeCursor(newCursor);
            }

            _popupAddress.OnMouseMove(_previousX, _previousY);
        }

        /// <summary>
        /// React on mouse up.
        /// </summary>
        /// <param name="modifierKeys">Modifier keys state.</param>
        /// <param name="x">X coord.</param>
        /// <param name="y">Y coord.</param>
        public void OnMouseUp(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y)
        {
            Debug.Assert(_mapControl != null);

            _EndPan();

            if (_editingInProcess)
            {
                App.Current.Project.Save();

                // Save to local geocoder database.
                Order order = _mapControl.EditedObject as Order;
                if (order != null)
                {
                    NameAddressRecord nameAddressRecord = CommonHelpers.CreateNameAddressPair(order, null);

                    // Do save in local storage.
                    App.Current.NameAddressStorage.InsertOrUpdate(nameAddressRecord,
                        App.Current.Geocoder.AddressFormat);
                }

                // Check new position is near to road. Set match method if not.
                IGeocodable geocodable = _mapControl.EditedObject as IGeocodable;
                if (geocodable != null)
                {
                    Address addressUnderPoint = _ReverseGeocode(geocodable.GeoLocation.Value);
                    if (addressUnderPoint == null)
                    {
                        geocodable.Address.MatchMethod = (string)App.Current.FindResource(MANUALLY_EDITED_XY_FAR_FROM_NEAREST_ROAD_RESOURCE_NAME);
                    }
                }

                if (OnComplete != null)
                    OnComplete(this, EventArgs.Empty);
                _editingInProcess = false;
                _editedGraphic = null;
            }
        }

        public void Refresh(int hdc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event is raised when tool finished its job.
        /// </summary>
        public event EventHandler OnComplete;

        /// <summary>
        /// Event is raised when tool enability changed.
        /// </summary>
        public event EventHandler EnabledChanged;

        public event EventHandler CursorChanged;

        /// <summary>
        /// Event is raised when tool activated.
        /// </summary>
        public event EventHandler ActivatedChanged;

        #endregion

        #region protected methods

        /// <summary>
        /// Raises <see cref="EnabledChanged"/> event with the specified
        /// arguments.
        /// </summary>
        /// <param name="e">The arguments of the EnabledChanged event.</param>
        protected virtual void OnEnabledChanged(EventArgs e)
        {
            var temp = this.EnabledChanged;
            if (temp != null)
            {
                temp(this, e);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Change cursor.
        /// </summary>
        /// <param name="cursor">Cursor to set.</param>
        private void _ChangeCursor(Cursor cursor)
        {
            if (_cursor != cursor)
            {
                _cursor = cursor;
                CursorChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Pan Geometry.
        /// </summary>
        /// <param name="geometry">Geometry to pan.</param>
        /// <param name="dx">Pan dx.</param>
        /// <param name="dy">Pan dy.</param>
        /// <returns>New geometry.</returns>
        private object _PanGeometry(object geometry, double dx, double dy)
        {
            Debug.Assert(geometry != null);

            Point[] points = null;

            if (geometry is Point)
            {
                Point? pt = geometry as Point?;
                points = new Point[] { pt.Value };
            }
            else if (geometry is PolyCurve)
            {
                PolyCurve polyCurve = geometry as PolyCurve;
                points = polyCurve.GetPoints(0, polyCurve.TotalPointCount);
            }
            else
                throw new NotSupportedException();

            if (points.Length == 1 || !(_editedGraphic is EditMarkerGraphicObject))
            {
                for (int index = 0; index < points.Length; index++)
                {
                    double newX = points[index].X + dx;
                    double newY = points[index].Y + dy;
                    Point point = new Point(newX, newY);
                    points[index] = point;
                }
            }
            else
            {
                _PanByEditMarker(points, dx, dy, geometry as PolyCurve);
            }

            // Create new geometry.
            object newGeometry = null;

            if (geometry is Point)
            {
                newGeometry = points[0];
            }
            else if (geometry is Polygon)
            {
                newGeometry = new Polygon((geometry as Polygon).Groups, points);
            }
            else if (geometry is Polyline)
            {
                newGeometry = new Polyline((geometry as Polyline).Groups, points);
            }
            else
                throw new NotSupportedException();

            
            return newGeometry;
        }

        /// <summary>
        /// Pan Geometry by edit marker.
        /// </summary>
        /// <param name="points">Geometry points.</param>
        /// <param name="dx">Pan dx.</param>
        /// <param name="dy">Pan dy.</param>
        /// <param name="polyCurve">Poly curve.</param>
        private void _PanByEditMarker(Point[] points, double dx, double dy, PolyCurve polyCurve)
        {
            Debug.Assert(points != null);
            Debug.Assert(_editedGraphic is EditMarkerGraphicObject);

            if (_editedGraphic is EditMarkerGraphicObject)
            {
                int index = _markersLayer.MapLayer.Graphics.IndexOf(_editedGraphic);
                
                // In case of pan marker for start or end point in ring(which has equal positions)
                // we need to pan both points.
                int groupStartIndex = 0;
                int groupEndIndex = 0;

                int maxIndexInGroup = 0;
                foreach (int groupSize in polyCurve.Groups)
                {
                    if (maxIndexInGroup + groupSize > index)
                    {
                        groupStartIndex = maxIndexInGroup;
                        groupEndIndex = groupStartIndex + groupSize - 1;
                        break;
                    }

                    maxIndexInGroup += groupSize;
                }

                // In case of last point in ring in polygon.
                if (index == groupEndIndex && polyCurve is Polygon)
                    index = groupStartIndex;

                double newX = points[index].X + dx;
                double newY = points[index].Y + dy;
                Point point = new Point(newX, newY);
                points[index] = point;

                // Move last point of polygon.
                if (index == groupStartIndex && polyCurve is Polygon)
                {
                    index = groupEndIndex;
                    double newLastX = points[index].X + dx;
                    double newLastY = points[index].Y + dy;
                    Point pointLast = new Point(newLastX, newLastY);
                    points[index] = pointLast;
                }
            }
        }

        /// <summary>
        /// Pan object geometry.
        /// </summary>
        /// <param name="editingObject">Editing object.</param>
        /// <param name="dx">Pan dx.</param>
        /// <param name="dy">Pan dy.</param>
        private void _PanObject(object editingObject, double dx, double dy)
        {
            Debug.Assert(editingObject != null);
            Debug.Assert(_markersLayer != null);

            IList<EditingMarker> list = (IList<EditingMarker>)_markersLayer.Collection;
            if (editingObject is IGeocodable)
            {
                IGeocodable geocodable = (IGeocodable)editingObject;

                if (geocodable.GeoLocation.HasValue)
                {
                    geocodable.GeoLocation = new Point(geocodable.GeoLocation.Value.X + dx, geocodable.GeoLocation.Value.Y + dy);
                    string manuallyEditedXY = (string)System.Windows.Application.Current.FindResource(MANUALLY_EDITED_XY_RESOURCE_NAME);
                    if (geocodable.Address.MatchMethod == null ||
                        !geocodable.Address.MatchMethod.Equals(manuallyEditedXY, StringComparison.OrdinalIgnoreCase))
                        geocodable.Address.MatchMethod = manuallyEditedXY;
                }
            }
            else if (editingObject is Zone)
            {
                Zone zone = (Zone)editingObject;
                zone.Geometry = _PanGeometry(zone.Geometry, dx, dy);
            }
            else if (editingObject is Barrier)
            {
                Barrier barrier = (Barrier)editingObject;
                barrier.Geometry = _PanGeometry(barrier.Geometry, dx, dy);
            }
        }

        /// <summary>
        /// Start pan.
        /// </summary>
        private void _StartPan()
        {
            _popupAddress.Enable();
        }

        /// <summary>
        /// End pan.
        /// </summary>
        private void _EndPan()
        {
            _popupAddress.Disable();
        }

        /// <summary>
        /// Finds address by geographical location.
        /// </summary>
        /// <param name="location">The point to find address for.</param>
        /// <returns>An address corresponding to the specified point.</returns>
        private Address _ReverseGeocode(Point location)
        {
            var result = default(Address);

            try
            {
                result = App.Current.Geocoder.ReverseGeocode(location);
            }
            catch (Exception e)
            {
                var canHandle =
                    e is AuthenticationException ||
                    e is CommunicationException;
                if (!canHandle)
                {
                    throw;
                }

                var serviceName = App.Current.FindString(GEOCODING_SERVICE_NAME_KEY);
                CommonHelpers.AddServiceMessage(serviceName, e);

                Logger.Error(e);
            }

            return result;
        }


        /// <summary>
        /// Raises on tool activated.
        /// </summary>
        private void _NotifyActivatedChanged()
        {
            if (ActivatedChanged != null)
                ActivatedChanged(this, EventArgs.Empty);
        }
        #endregion

        #region Private constants

        /// <summary>
        /// Match method manually edited items resource name.
        /// </summary>
        private const string MANUALLY_EDITED_XY_RESOURCE_NAME = "ManuallyEditedXY";

        /// <summary>
        /// Match method for not geocoded items resource name.
        /// </summary>
        private const string MANUALLY_EDITED_XY_FAR_FROM_NEAREST_ROAD_RESOURCE_NAME = "ManuallyEditedXYFarFromNearestRoad";
        
        /// <summary>
        /// Path to cursor resource.
        /// </summary>
        private const string MOVE_VERTEX_CURSOR_PATH = @"..\..\Resources\MoveVertex.cur";

        /// <summary>
        /// Move shape cursor resource name.
        /// </summary>
        private const string MOVE_SHAPE_CURSOR_RESOURCE_NAME = "MoveShape";
        
        /// <summary>
        /// Move vertex cursor resource name.
        /// </summary>
        private const string MOVE_VERTEX_CURSOR_RESOURCE_NAME = "MoveVertex";

        /// <summary>
        /// Resource key for the geocoding service name.
        /// </summary>
        private const string GEOCODING_SERVICE_NAME_KEY = "ServiceNameGeocoding";

        #endregion

        #region Private members

        /// <summary>
        /// Map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Edited graphic object.
        /// </summary>
        private DataGraphicObject _editedGraphic;

        /// <summary>
        /// Current cursor.
        /// </summary>
        private Cursor _cursor;

        /// <summary>
        /// Is tool activated.
        /// </summary>
        private bool _isActivated;

        /// <summary>
        /// Is editing in progress.
        /// </summary>
        private bool _editingInProcess;

        /// <summary>
        /// Previous x coord.
        /// </summary>
        private double _previousX;

        /// <summary>
        /// Previous y coord.
        /// </summary>
        private double _previousY;

        /// <summary>
        /// Editing markers layer.
        /// </summary>
        private ObjectLayer _markersLayer;

        /// <summary>
        /// Cursor, which shows on shape moving.
        /// </summary>
        private Cursor _moveShapeCursor;

        /// <summary>
        /// Cursor, which shows on vertex moving.
        /// </summary>
        private Cursor _moveVertexCursor;

        /// <summary>
        /// Popup address helper.
        /// </summary>
        private AddressPopupHelper _popupAddress;

        /// <summary>
        /// Current edited object.
        /// </summary>
        private object _editingObject;

        #endregion
    }
}
