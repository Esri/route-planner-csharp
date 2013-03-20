using System;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Helper class for work with Web Mercator spatial references.
    /// </summary>
    public sealed class WebMercatorUtil
    {
        #region constants

        private const double DEGREES_PER_RADIANS = 57.295779513082320;
        private const double RADIANS_PER_DEGREES = 0.017453292519943;

        // Earth radius
        private const double RADIUS = 6378137; // Using Equatorial radius, meters: http://en.wikipedia.org/wiki/Earth_radius

        // WGS84 spatial reference ID
        private const int GEO_WKID = 4326;
        // Web Mercator spatial reference ID
        private const int MER_WKID = 102100;

        // Delegate for function, which convert values
        private delegate double ConverterDelegate(double value);

        #endregion

        #region public members

        /// <summary>
        /// Project point to Web Mercator spatial reference in case of map spatial reference is Web Mercator.
        /// </summary>
        /// <param name="point">Point to project.</param>
        /// <param name="spatialReferenceID">Map spatial reference ID.</param>
        /// <returns>Point, projected in map spatial reference.</returns>
        public static Point ProjectPointToWebMercator(Point point, int spatialReferenceID)
        {
            if (spatialReferenceID == MER_WKID)
            {
                return _ProjectPoint(point, _LongitudeToX, _LatitudeToY, MER_WKID);
            }
            else
            {
                return point;
            }
        }

        /// <summary>
        /// Project polygon to Web Mercator spatial reference in case of map spatial reference is Web Mercator.
        /// </summary>
        /// <param name="polygon">Polygon to project.</param>
        /// <param name="spatialReferenceID">Map spatial reference ID.</param>
        /// <returns>Polygon, projected in map spatial reference.</returns>
        public static Polygon ProjectPolygonToWebMercator(Polygon polygon, int spatialReferenceID)
        {
            if (spatialReferenceID == MER_WKID)
            {
                return _ProjectPolygon(polygon, _LongitudeToX, _LatitudeToY, MER_WKID);
            }
            else
            {
                return polygon;
            }
        }

        /// <summary>
        /// Project polyline to Web Mercator spatial reference in case of map spatial reference is Web Mercator.
        /// </summary>
        /// <param name="polyline">Polyline to project.</param>
        /// <param name="spatialReferenceID">Map spatial reference ID.</param>
        /// <returns>Polyline, projected in map spatial reference.</returns>
        public static Polyline ProjectPolylineToWebMercator(Polyline polyline, int spatialReferenceID)
        {
            if (spatialReferenceID == MER_WKID)
            {
                return _ProjectPolyline(polyline, _LongitudeToX, _LatitudeToY, MER_WKID);
            }
            else
            {
                return polyline;
            }
        }

        /// <summary>
        /// Project point from Web Mercator spatial reference in case of map spatial reference is Web Mercator.
        /// </summary>
        /// <param name="point">Point to project.</param>
        /// <param name="spatialReferenceID">Map spatial reference ID.</param>
        /// <returns>Point, projected in map spatial reference.</returns>
        public static Point ProjectPointFromWebMercator(Point point, int spatialReferenceID)
        {
            if (spatialReferenceID != GEO_WKID)
            {
                return _ProjectPoint(point, _XToLongitude, _YToLatitude, GEO_WKID);
            }
            else
            {
                return point;
            }
        }

        /// <summary>
        /// Project polygon from Web Mercator spatial reference in case of map spatial reference is Web Mercator.
        /// </summary>
        /// <param name="polygon">Polygon to project.</param>
        /// <param name="spatialReferenceID">Map spatial reference ID.</param>
        /// <returns>Polygon, projected in map spatial reference.</returns>
        public static Polygon ProjectPolygonFromWebMercator(Polygon polygon, int spatialReferenceID)
        {
            if (spatialReferenceID == GEO_WKID)
            {
                return _ProjectPolygon(polygon, _XToLongitude, _YToLatitude, GEO_WKID);
            }
            else
            {
                return polygon;
            }
        }

        /// <summary>
        /// Project polyline from Web Mercator spatial reference in case of map spatial reference is Web Mercator.
        /// </summary>
        /// <param name="polyline">Polyline to project.</param>
        /// <param name="spatialReferenceID">Map spatial reference ID.</param>
        /// <returns>Polyline, projected in map spatial reference.</returns>
        public static Polyline ProjectPolylineFromWebMercator(Polyline polyline, int spatialReferenceID)
        {
            if (spatialReferenceID == GEO_WKID)
            {
                return _ProjectPolyline(polyline, _XToLongitude, _YToLatitude, GEO_WKID);
            }
            else
            {
                return polyline;
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Project point to map spatial reference.
        /// </summary>
        /// <param name="point">Point to project</param>
        /// <param name="xConv">Delegate for convert x coordinate.</param>
        /// <param name="yConv">Delegate for convert y coordinate.</param>
        /// <param name="wkid">Spatial reference ID.</param>
        /// <returns>Projected point.</returns>
        private static Point _ProjectPoint(Point point, ConverterDelegate xConv, ConverterDelegate yConv, int? wkid)
        {
            double x = xConv(point.X);
            double y = yConv(point.Y);

            Point result = new Point(x, y);

            return result;
        }

        /// <summary>
        /// Project polygon to map spatial reference.
        /// </summary>
        /// <param name="polygon">Polygon to project.</param>
        /// <param name="xConv">Delegate for convert x coordinate.</param>
        /// <param name="yConv">Delegate for convert y coordinate.</param>
        /// <param name="wkid">Spatial reference ID.</param>
        /// <returns>Projected polygon.</returns>
        private static Polygon _ProjectPolygon(Polygon polygon, ConverterDelegate xConvFunc, ConverterDelegate yConvFunc, int wkid)
        {
            Point[] points = polygon.GetPoints(0, polygon.TotalPointCount);
            Point[] projectedPoints = new Point[polygon.TotalPointCount];

            for (int pointIndex = 0; pointIndex< points.Length; pointIndex++ )
            {
                Point point = points[pointIndex];
                projectedPoints[pointIndex] = _ProjectPoint(point, xConvFunc, yConvFunc, wkid);
            }

            Polygon projectedPolygon = new Polygon(polygon.Groups, projectedPoints);

            return projectedPolygon;
        }

        /// <summary>
        /// Project polyline to map spatial reference.
        /// </summary>
        /// <param name="polyline">Polyline to project.</param>
        /// <param name="xConv">Delegate for convert x coordinate.</param>
        /// <param name="yConv">Delegate for convert y coordinate.</param>
        /// <param name="wkid">Spatial reference ID.</param>
        /// <returns>Projected polyline.</returns>
        private static Polyline _ProjectPolyline(Polyline polyline, ConverterDelegate xConvFunc, ConverterDelegate yConvFunc, int wkid)
        {
            Point[] points = polyline.GetPoints(0, polyline.TotalPointCount);
            Point[] projectedPoints = new Point[polyline.TotalPointCount];

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                Point point = points[pointIndex];
                projectedPoints[pointIndex] = _ProjectPoint(point, xConvFunc, yConvFunc, wkid);
            }

            Polyline projectedPolyline = new Polyline(polyline.Groups, projectedPoints);

            return projectedPolyline;
        }

        /// <summary>
        /// Convert latitude to y coordinate.
        /// </summary>
        /// <param name="latitude">Latitude.</param>
        /// <returns>Y coordinate.</returns>
        private static double _LatitudeToY(double latitude)
        {
            double lat_rad = _DegreesToRadians(latitude);
            double y = RADIUS/2.0 * Math.Log( (1.0 + Math.Sin(lat_rad)) / (1.0 - Math.Sin(lat_rad)) );

            return y;
        }

        /// <summary>
        /// Convert longitude to x coordinate.
        /// </summary>
        /// <param name="longitude">Longitude.</param>
        /// <returns>X coordinate.</returns>
        private static double _LongitudeToX(double longitude)
        {
            double x = _DegreesToRadians(longitude) * RADIUS;

            return x;
        }

        /// <summary>
        /// Convert x coordinate to longitude.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <returns>Longitude.</returns>
        private static double _XToLongitude(double x)
        {
            double lng_rad = x / RADIUS;
            double lng_deg = _RadiansToDegrees(lng_rad);
            double rotations = Math.Floor((lng_deg + 180)/360);
            double lng = lng_deg - (rotations * 360);
            return lng;
        }

        /// <summary>
        /// Convert y coordinate to latitude.
        /// </summary>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Latitude.</returns>
        private static double _YToLatitude(double y)
        {
            double lat_rad = Math.PI / 2/* ProjUtils.PI_OVER_2*/ - (2 * Math.Atan(Math.Exp(-1.0 * y / RADIUS)));
            double lat_deg = _RadiansToDegrees(lat_rad);
            return lat_deg;
        }

        /// <summary>
        /// Convert degrees to radians.
        /// </summary>
        /// <param name="degrees">Degrees value.</param>
        /// <returns>Radians value.</returns>
        private static double _DegreesToRadians(double degrees)
        {
            return degrees * RADIANS_PER_DEGREES;
        }

        /// <summary>
        /// Convert radians to degrees.
        /// </summary>
        /// <param name="radians">Radians value.</param>
        /// <returns>Degrees value.</returns>
        private static double _RadiansToDegrees(double radians)
        {
            return radians * DEGREES_PER_RADIANS;
        }

        #endregion
    }
}
