using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Symbols;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing barriers.
    /// </summary>
    class BarrierGraphicObject : DataGraphicObject
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="barrier">Barrier to show.</param>
        private BarrierGraphicObject(Barrier barrier)
            : base(barrier)
        {
            _barrier = barrier;
            _barrier.PropertyChanged += new PropertyChangedEventHandler(_BarrierPropertyChanged);

            Geometry = _CreateGeometry(barrier);

            Attributes.Add(START_ATTRIBUTE_NAME, null);
            Attributes.Add(FINISH_ATTRIBUTE_NAME, null);

            Attributes.Add(ESRI.ArcLogistics.App.OrderSymbology.SymbologyContext.FILL_ATTRIBUTE_NAME,
                new SolidColorBrush(Colors.Red));
        }

        #endregion constructors

        #region Public static properties

        /// <summary>
        /// Start attribute name.
        /// </summary>
        public static string StartAttributeName
        {
            get
            {
                return START_ATTRIBUTE_NAME;
            }
        }

        /// <summary>
        /// Finish attribute name.
        /// </summary>
        public static string FinishAttributeName
        {
            get
            {
                return FINISH_ATTRIBUTE_NAME;
            }
        }

        #endregion Public static properties

        #region Public static methods

        /// <summary>
        /// Create graphic object for barrier.
        /// </summary>
        /// <param name="barrier">Source barrier.</param>
        /// <returns>Graphic object for barrier.</returns>
        public static BarrierGraphicObject Create(Barrier barrier)
        {
            BarrierGraphicObject graphic = new BarrierGraphicObject(barrier);

            return graphic;
        }

        #endregion Public static methods

        #region Public methods

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        public override void UnsubscribeOnChange()
        {
            _barrier.PropertyChanged -= new PropertyChangedEventHandler(_BarrierPropertyChanged);
        }

        /// <summary>
        /// Project geometry to map spatial reference.
        /// </summary>
        public override void ProjectGeometry()
        {
            Geometry = _CreateGeometry(_barrier);
        }

        #endregion Public methods

        #region Private methods

        /// <summary>
        /// Create barrier geometry.
        /// </summary>
        /// <param name="barrier">Barrier.</param>
        /// <returns>Barrier geometry.</returns>
        private ESRI.ArcGIS.Client.Geometry.Geometry _CreateGeometry(Barrier barrier)
        {
            ESRI.ArcGIS.Client.Geometry.Geometry geometry = null;

            if (barrier.Geometry != null)
            {
                int? spatialReference = null;
                if (ParentLayer != null)
                {
                    spatialReference = ParentLayer.SpatialReferenceID;
                }

                if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Point)
                {
                    ESRI.ArcLogistics.Geometry.Point point = (ESRI.ArcLogistics.Geometry.Point)barrier.Geometry;

                    // Project point from WGS84 to Web Mercator if spatial reference of map is Web Mercator
                    if (ParentLayer != null && ParentLayer.SpatialReferenceID != null)
                    {
                        point = WebMercatorUtil.ProjectPointToWebMercator(point, spatialReference.Value);
                    }

                    geometry = new ESRI.ArcGIS.Client.Geometry.MapPoint(point.X, point.Y);
                }
                else if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Polygon)
                {
                    geometry = MapHelpers.ConvertToArcGISPolygon(
                        barrier.Geometry as ESRI.ArcLogistics.Geometry.Polygon, spatialReference);
                }
                else if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Polyline)
                {
                    geometry = MapHelpers.ConvertToArcGISPolyline(
                        barrier.Geometry as ESRI.ArcLogistics.Geometry.Polyline, spatialReference);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                }

                _SetSymbol();
            }
            else
            {
                geometry = null;
            }

            return geometry;
        }

        /// <summary>
        /// Set symbol if not yet set or geometry type changed.
        /// </summary>
        private void _SetSymbol()
        {
            if (_geometryType == null || _geometryType != _barrier.Geometry.GetType())
            {
                _geometryType = _barrier.Geometry.GetType();
                if (_geometryType == typeof(ESRI.ArcLogistics.Geometry.Point))
                {
                    Symbol = new BarrierSymbol();
                }
                else if (_geometryType == typeof(ESRI.ArcLogistics.Geometry.Polygon))
                {
                    Symbol = new BarrierPolygonSymbol();
                }
                else if (_geometryType == typeof(ESRI.ArcLogistics.Geometry.Polyline))
                {
                    Symbol = new BarrierPolylineSymbol();
                }
                else
                    Debug.Assert(false);
            }
        }

        /// <summary>
        /// React on barrier property changes.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _BarrierPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Barrier.PropertyNameGeometry)
            {
                // if barrier position changed - show barrier in new place
                ESRI.ArcGIS.Client.Geometry.Geometry geometry = _CreateGeometry(_barrier);

                Geometry = geometry;

                _Refresh();
            }
            else if (e.PropertyName.Equals(Barrier.PropertyNameStartDate) ||
                     e.PropertyName.Equals(Barrier.PropertyNameFinishDate))
            {
                // if layer has context, then refresh view.
                if (_objectContext != null)
                {
                    _Refresh();
                }
            }
        }

        /// <summary>
        /// Refresh graphic visibility.
        /// </summary>
        private void _Refresh()
        {
            Attributes[START_ATTRIBUTE_NAME] = _barrier.StartDate;
            Attributes[FINISH_ATTRIBUTE_NAME] = _barrier.FinishDate;

            // Refresh symbol.
            Symbol symbol = Symbol;
            Symbol = null;
            Symbol = symbol;
        }

        #endregion private methods

        #region Public members

        /// <summary>
        /// Object, depending on whose properties graphics changes their view.
        /// </summary>
        public override object ObjectContext
        {
            get
            {
                return _objectContext;
            }
            set
            {
                _Refresh();
                _objectContext = value;
            }
        }

        #endregion public members

        #region Private constants

        /// <summary>
        /// Start attribute name.
        /// </summary>
        private const string START_ATTRIBUTE_NAME = "Start";

        /// <summary>
        /// Finish attribute name.
        /// </summary>
        private const string FINISH_ATTRIBUTE_NAME = "Finish";

        #endregion

        #region Private members

        /// <summary>
        /// Barrier to show.
        /// </summary>
        private Barrier _barrier;

        /// <summary>
        /// Context.
        /// </summary>
        private object _objectContext;

        /// <summary>
        /// Barrier geometry type.
        /// </summary>
        private Type _geometryType;

        #endregion Private members
    }
}
