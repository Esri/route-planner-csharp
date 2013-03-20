using System.ComponentModel;
using System.Diagnostics;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing edit markers.
    /// </summary>
    class EditMarkerGraphicObject : DataGraphicObject
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="editingMarker">Editing marker to show.</param>
        private EditMarkerGraphicObject(EditingMarker editingMarker)
            : base(editingMarker)
        {
            _editingMarker = editingMarker;
            _SubscribeOnChange();

            Geometry = _CreatePoint(_editingMarker.EditingObject);
        }

        #endregion constructors

        #region public static methods

        /// <summary>
        /// Create graphic object for editing marker.
        /// </summary>
        /// <param name="editingMarker">Source editing marker.</param>
        /// <returns>Graphic object for editing marker.</returns>
        public static EditMarkerGraphicObject Create(EditingMarker editingMarker)
        {
            EditMarkerGraphicObject graphic = null;            

            Symbol editMarkerSymbol;
            if (editingMarker.MultipleIndex > -1)
                editMarkerSymbol = new EditingMarkerSymbol();
            else
                editMarkerSymbol = new PencilSymbol();

            graphic = new EditMarkerGraphicObject(editingMarker)
            {
                Symbol = editMarkerSymbol
            };

            graphic.SetZIndex(ObjectLayer.FRONTZINDEX);

            return graphic;
        }

        /// <summary>
        /// Unsubscribe from all events.
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            if (_editingMarker.EditingObject is Location)
            {
                Location location = (Location)_editingMarker.EditingObject;
                location.PropertyChanged -= new PropertyChangedEventHandler(_LocationPropertyChanged);
            }
            else if (_editingMarker.EditingObject is Order)
            {
                Order order = (Order)_editingMarker.EditingObject;
                order.PropertyChanged -= new PropertyChangedEventHandler(_OrderPropertyChanged);
            }
            else if (_editingMarker.EditingObject is Zone)
            {
                Zone zone = (Zone)_editingMarker.EditingObject;
                zone.PropertyChanged -= new PropertyChangedEventHandler(_ZonePropertyChanged);
            }
            else if (_editingMarker.EditingObject is Barrier)
            {
                Barrier barrier = (Barrier)_editingMarker.EditingObject;
                barrier.PropertyChanged -= new PropertyChangedEventHandler(_BarrierPropertyChanged);
            }
            else
                System.Diagnostics.Debug.Assert(false);
        }

        /// <summary>
        /// Project geometry to map spatial reference.
        /// </summary>
        public override void ProjectGeometry()
        {
            Geometry = _CreatePoint(_editingMarker.EditingObject);
        }

        #endregion public static methods

        #region Public members

        /// <summary>
        /// Source editing marker for this graphic object.
        /// </summary>
        public EditingMarker EditingMarker
        {
            get
            {
                return _editingMarker;
            }
        }

        #endregion public members

        #region Private methods

        /// <summary>
        /// Subscribe to changes in editing object.
        /// </summary>
        private void _SubscribeOnChange()
        {
            if (_editingMarker.EditingObject is Location)
            {
                Location location = (Location)_editingMarker.EditingObject;
                location.PropertyChanged += new PropertyChangedEventHandler(_LocationPropertyChanged);
            }
            else if (_editingMarker.EditingObject is Order)
            {
                Order order = (Order)_editingMarker.EditingObject;
                order.PropertyChanged += new PropertyChangedEventHandler(_OrderPropertyChanged);
            }
            else if (_editingMarker.EditingObject is Zone)
            {
                Zone zone = (Zone)_editingMarker.EditingObject;
                zone.PropertyChanged += new PropertyChangedEventHandler(_ZonePropertyChanged);
            }
            else if (_editingMarker.EditingObject is Barrier)
            {
                Barrier barrier = (Barrier)_editingMarker.EditingObject;
                barrier.PropertyChanged += new PropertyChangedEventHandler(_BarrierPropertyChanged);
            }
            else
                System.Diagnostics.Debug.Assert(false);
        }

        /// <summary>
        /// React on barrier property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _BarrierPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Barrier.PropertyNameGeometry)
            {
                Barrier barrier = _editingMarker.EditingObject as Barrier;
                Geometry = _CreatePoint(barrier);

                if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Point)
                    Symbol = new PencilSymbol();
                else
                    Symbol = new EditingMarkerSymbol();
            }
        }

        /// <summary>
        /// React on zone property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _ZonePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Zone.PropertyNameGeometry)
            {
                ESRI.ArcGIS.Client.Geometry.MapPoint newPoint = _CreatePoint(_editingMarker.EditingObject);
                ESRI.ArcGIS.Client.Geometry.MapPoint oldPoint = (ESRI.ArcGIS.Client.Geometry.MapPoint)Geometry;
                if (newPoint != null && oldPoint != null)
                {
                    if (newPoint.X != oldPoint.X || newPoint.Y != oldPoint.Y)
                        Geometry = newPoint;
                }
                else
                    Geometry = newPoint;
            }
        }

        /// <summary>
        /// React on order property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _OrderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Order.PropertyNameGeoLocation)
            {
                Geometry = _CreatePoint(_editingMarker.EditingObject);
            }
        }

        /// <summary>
        /// React on location property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _LocationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Location.PropertyNameGeoLocation)
            {
                Geometry = _CreatePoint(_editingMarker.EditingObject);
            }
        }

        /// <summary>
        /// Create position for editing marker.
        /// </summary>
        /// <param name="obj">Edited object.</param>
        /// <returns>Position for editing marker.</returns>
        private ESRI.ArcGIS.Client.Geometry.MapPoint _CreatePoint(object obj)
        {
            ESRI.ArcLogistics.Geometry.Point? point = null;

            if (_editingMarker.EditingObject is Location)
            {
                Location location = (Location)obj;
                if (location.GeoLocation.HasValue)
                    point = new ESRI.ArcLogistics.Geometry.Point(location.GeoLocation.Value.X, location.GeoLocation.Value.Y);
                else
                    point = null;
            }
            else if (_editingMarker.EditingObject is Order)
            {
                Order order = (Order)_editingMarker.EditingObject;
                if (order.GeoLocation.HasValue)
                    point = new ESRI.ArcLogistics.Geometry.Point(order.GeoLocation.Value.X, order.GeoLocation.Value.Y);
                else
                    point = null;
            }
            else if ((_editingMarker.EditingObject is Zone) || (_editingMarker.EditingObject is Barrier))
            {
                point = _CreatePointForMultiPointObject(obj);
            }

            ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = null;

            if (point.HasValue)
            {
                // Project point from WGS84 to Web Mercator if spatial reference of map is Web Mercator
                if (ParentLayer != null && ParentLayer.SpatialReferenceID != null)
                {
                    point = WebMercatorUtil.ProjectPointToWebMercator(point.Value, ParentLayer.SpatialReferenceID.Value);
                }

                mapPoint = new ESRI.ArcGIS.Client.Geometry.MapPoint(point.Value.X, point.Value.Y);
            }

            return mapPoint;
        }

        /// <summary>
        /// Create position for editing marker for edit zone or barrier.
        /// </summary>
        /// <param name="obj">Edited object.</param>
        /// <returns>Position for editing marker for edit zone or barrier.</returns>
        private ESRI.ArcLogistics.Geometry.Point? _CreatePointForMultiPointObject(object obj)
        {
            ESRI.ArcLogistics.Geometry.Point? point = null;

            object editObj = _editingMarker.EditingObject;
            object geometry = (editObj is Zone) ? (editObj as Zone).Geometry : (editObj as Barrier).Geometry;
            if (null != geometry)
            {
                if (geometry is ESRI.ArcLogistics.Geometry.Point)
                {
                    ESRI.ArcLogistics.Geometry.Point? pt = geometry as ESRI.ArcLogistics.Geometry.Point?;
                    point = new ESRI.ArcLogistics.Geometry.Point(pt.Value.X, pt.Value.Y);
                }
                else if (geometry is ESRI.ArcLogistics.Geometry.PolyCurve)
                {
                    System.Diagnostics.Debug.Assert(geometry is ESRI.ArcLogistics.Geometry.Polygon ||
                        geometry is ESRI.ArcLogistics.Geometry.Polyline);

                    int index = _editingMarker.MultipleIndex;
                    if (index == -1)
                        index = 0;

                    ESRI.ArcLogistics.Geometry.PolyCurve polyCurve = geometry as ESRI.ArcLogistics.Geometry.PolyCurve;
                    ESRI.ArcLogistics.Geometry.Point polyCurvePoint = polyCurve.GetPoint(index);

                    point = new ESRI.ArcLogistics.Geometry.Point(polyCurvePoint.X, polyCurvePoint.Y);
                }
                else
                    Debug.Assert(false);
            }
            else
                point = null;

            return point;
        }
        
        #endregion

        #region private members

        /// <summary>
        /// Editing marker to show.
        /// </summary>
        private EditingMarker _editingMarker;

        #endregion private members
    }
}
