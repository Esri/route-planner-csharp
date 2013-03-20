using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Esri.ShapefileReader;
using ESRISDSGeometry = ESRI.ArcGIS.Client.Geometry;

namespace ESRI.ArcLogistics.ShapefileReader
{
    /// <summary>
    /// Shape.
    /// </summary>
    public class Shape
    {
        #region Constructors

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="shape">Shape.</param>
        internal Shape(Esri.ShapefileReader.Shape shape)
        {
            _shape = shape;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Extent.
        /// </summary>
        public ESRI.ArcLogistics.Geometry.Envelope Extent
        {
            get
            {
                return _ConvertToArcLogisticsEnvelope(
                    _shape.Extent as ESRISDSGeometry.Envelope);
            }
        }

        /// <summary>
        /// ArcLogistics Geometry object.
        /// Can be converted to one of ArcLogistics Geometry objects.
        /// </summary>
        public object Geometry
        {
            get
            {
                object result = null;

                // Convert ESRI ArcGIS geometry object to one of
                // ArcLogistics geometry objects.
                if (_shape.Geometry is ESRISDSGeometry.Polygon)
                {
                    result = _ConvertToArcLogisticsPolygon(
                        _shape.Geometry as ESRISDSGeometry.Polygon);
                }
                else if (_shape.Geometry is ESRISDSGeometry.Polyline)
                {
                    result = _ConvertToArcLogisticsPolyline(
                        _shape.Geometry as ESRISDSGeometry.Polyline);
                }
                else if (_shape.Geometry is ESRISDSGeometry.MapPoint)
                {
                    result = (object)_ConvertToArcLogisticsPoint
                        (_shape.Geometry as ESRISDSGeometry.MapPoint);
                }
                else if (_shape.Geometry is ESRISDSGeometry.Envelope)
                {
                    result = (object)_ConvertToArcLogisticsEnvelope
                        (_shape.Geometry as ESRISDSGeometry.Envelope);
                }
                else
                {
                    // Not supported type.
                    Debug.Assert(false);
                }

                return result;
            }
        }

        /// <summary>
        /// Shape type.
        /// </summary>
        public ShapeType ShapeType
        {
            get
            {
                ShapeType type = ShapeType.Null;

                switch (_shape.ShapeType)
                {
                    case Esri.ShapefileReader.ShapeType.Null:
                        type = ShapeType.Null;
                        break;
                    case Esri.ShapefileReader.ShapeType.Point:
                        type = ShapeType.Point;
                        break;
                    case Esri.ShapefileReader.ShapeType.PolyLine:
                        type = ShapeType.PolyLine;
                        break;
                    case Esri.ShapefileReader.ShapeType.Polygon:
                        type = ShapeType.Polygon;
                        break;
                    case Esri.ShapefileReader.ShapeType.MultiPoint:
                        type = ShapeType.MultiPoint;
                        break;
                    case Esri.ShapefileReader.ShapeType.PointZ:
                        type = ShapeType.PointZ;
                        break;
                    case Esri.ShapefileReader.ShapeType.PolyLineZ:
                        type = ShapeType.PolyLineZ;
                        break;
                    case Esri.ShapefileReader.ShapeType.PolygonZ:
                        type = ShapeType.PolygonZ;
                        break;
                    case Esri.ShapefileReader.ShapeType.MultiPointZ:
                        type = ShapeType.MultiPointZ;
                        break;
                    case Esri.ShapefileReader.ShapeType.PointM:
                        type = ShapeType.PointM;
                        break;
                    case Esri.ShapefileReader.ShapeType.PolyLineM:
                        type = ShapeType.PolyLineM;
                        break;
                    case Esri.ShapefileReader.ShapeType.PolygonM:
                        type = ShapeType.PolygonM;
                        break;
                    case Esri.ShapefileReader.ShapeType.MultiPointM:
                        type = ShapeType.MultiPointM;
                        break;
                    case Esri.ShapefileReader.ShapeType.MultiPatch:
                        type = ShapeType.MultiPatch;
                        break;
                    default:
                        break;
                }

                return type;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Method converts ESRI ArcGIS Polygon object to ESRI ArcLogistics Polygon object.
        /// </summary>
        /// <param name="polygon">Polygon to convert.</param>
        /// <returns>ESRI ArcLogistics Polygon object.</returns>
        private ESRI.ArcLogistics.Geometry.Polygon _ConvertToArcLogisticsPolygon(
            ESRISDSGeometry.Polygon polygon)
        {
            Debug.Assert(polygon != null);

            var groups = new List<int>();
            var points = new List<ESRI.ArcLogistics.Geometry.Point>();

            for (int i = 0; i < polygon.Rings.Count; i++)
            {
                // Fill current group by count of Points in it.
                groups.Add(polygon.Rings[i].Count);

                var collection = polygon.Rings[i];

                // Get all points.
                foreach (var point in collection)
                {
                    points.Add(_ConvertToArcLogisticsPoint(point));
                }
            }

            var newPolygon = new ESRI.ArcLogistics.Geometry.Polygon(
                groups.ToArray(), points.ToArray());

            return newPolygon;
        }

        /// <summary>
        /// Method converts ESRI ArcGIS Polyline object to ESRI ArcLogistics Polyline object.
        /// </summary>
        /// <param name="polyline">Polyline to convert.</param>
        /// <returns>ESRI ArcLogistics Polyline object.</returns>
        private ESRI.ArcLogistics.Geometry.Polyline _ConvertToArcLogisticsPolyline(
            ESRISDSGeometry.Polyline polyline)
        {
            Debug.Assert(polyline != null);

            var groups = new List<int>();
            var points = new List<ESRI.ArcLogistics.Geometry.Point>();

            for (int i = 0; i < polyline.Paths.Count; i++)
            {
                // Fill current group by count of Points in it.
                groups.Add(polyline.Paths[i].Count);

                var collection = polyline.Paths[i];

                // Get all points.
                foreach (var point in collection)
                {
                    points.Add(_ConvertToArcLogisticsPoint(point));
                }
            }

            var newPolyline = new ESRI.ArcLogistics.Geometry.Polyline(
                groups.ToArray(), points.ToArray());

            return newPolyline;
        }

        /// <summary>
        /// Method converts ESRI ArcGIS MapPoint object to ESRI ArcLogistics Point object.
        /// </summary>
        /// <param name="mapPoint">MapPoint to convert.</param>
        /// <returns>ESRI ArcLogistics Point object.</returns>
        private ArcLogistics.Geometry.Point _ConvertToArcLogisticsPoint(
            ESRISDSGeometry.MapPoint mapPoint)
        {
            Debug.Assert(mapPoint != null);

            var point = new ArcLogistics.Geometry.Point();

            if(_IsPointValid(mapPoint))
                point = new ArcLogistics.Geometry.Point(
                    mapPoint.X, mapPoint.Y);

            return point;
        }

        /// <summary>
        /// Checks is point has valid\inited values.
        /// </summary>
        /// <param name="point">Point object to check.</param>
        /// <returns>TRUE if all properties of point is valid.</returns>
        private bool _IsPointValid(ESRISDSGeometry.MapPoint point)
        {
            return (!double.IsNaN(point.X) && !double.IsNaN(point.Y) &&
                    !double.IsInfinity(point.X) && !double.IsInfinity(point.Y) &&
                    (double.MinValue != point.X) && (double.MinValue != point.Y) &&
                    (double.MaxValue != point.X) && (double.MaxValue != point.Y));
        }

        /// <summary>
        /// Method converts ESRI ArcGIS Envelope object to ESRI ArcLogistics Envelope object.
        /// </summary>
        /// <param name="envelope">Envelope to covert.</param>
        /// <returns>ESRI ArcLogistics Envelope object.</returns>
        private ESRI.ArcLogistics.Geometry.Envelope _ConvertToArcLogisticsEnvelope(
            ESRISDSGeometry.Envelope envelope)
        {
            Debug.Assert(envelope != null);

            var geometry = new ESRI.ArcLogistics.Geometry.Envelope(
                envelope.XMin,
                envelope.YMax,
                envelope.XMax,
                envelope.YMin);

            return geometry;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Shape.
        /// </summary>
        private Esri.ShapefileReader.Shape _shape = new Esri.ShapefileReader.Shape();

        #endregion
    }
}
