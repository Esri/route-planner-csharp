using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// GPObjectHelper class.
    /// </summary>
    internal class GPObjectHelper
    {
        #region Public methods
        /// <summary>
        /// Method determines if Serialization info describes Polyline.
        /// </summary>
        /// <param name="info">Serialization Info.</param>
        /// <returns>True - if it is Polyline, otherwise - false.</returns>
        public static bool IsPolyline(SerializationInfo info)
        {
            return JsonSerializeHelper.ContainsProperty(
                GPAttribute.POLYLINE_PATHS,
                info);
        }

        /// <summary>
        /// Method determines if Serialization info describes Point.
        /// </summary>
        /// <param name="info">Serialization Info.</param>
        /// <returns>True - if it is Point, otherwise - false.</returns>
        public static bool IsPoint(SerializationInfo info)
        {
            return JsonSerializeHelper.ContainsProperty(GPAttribute.POINT_X, info) &&
                JsonSerializeHelper.ContainsProperty(GPAttribute.POINT_Y, info);
        }

        /// <summary>
        /// Checks if the serialization info stores Polygon geometry.
        /// </summary>
        /// <param name="info">The serialization info object to be checked.</param>
        /// <returns>true if and only if the specified serialization info object stores Polygon
        /// geometry object.</returns>
        public static bool IsPolygon(SerializationInfo info)
        {
            Debug.Assert(info != null);

            return JsonSerializeHelper.ContainsProperty(GPAttribute.POLYGON_RINGS, info);
        }

        /// <summary>
        /// Method converts Point object to GPPoint object.
        /// </summary>
        /// <param name="polygon">Point object to convert.</param>
        /// <returns>GPPoint object.</returns>
        public static GPPoint PointToGPPoint(Point pt)
        {
            GPPoint gppt = new GPPoint();
            gppt.X = pt.X;
            gppt.Y = pt.Y;
            gppt.SpatialReference = new GPSpatialReference(WKID);

            return gppt;
        }

        /// <summary>
        /// Converts <see cref="GPPoint"/> object into <see cref="Point"/> one.
        /// </summary>
        /// <param name="point">The reference to the <see cref="GPPoint"/> object
        /// to be converted.</param>
        /// <returns>A new <see cref="Point"/> instance converted from the <see cref="GPPoint"/>
        /// one.</returns>
        public static Point GPPointToPoint(GPPoint point)
        {
            Debug.Assert(point != null);

            var result = new Point(point.X, point.Y);

            return result;
        }

        /// <summary>
        /// Method converts Polygon object to GPPolygon object.
        /// </summary>
        /// <param name="polygon">Polygon object to convert.</param>
        /// <returns>GPPolygon object.</returns>
        public static GPPolygon PolygonToGPPolygon(Polygon polygon)
        {
            double[][][] rings = new double[polygon.Groups.Length][][];
            for (int nGroup = 0; nGroup < polygon.Groups.Length; nGroup++)
            {
                rings[nGroup] = new double[polygon.Groups[nGroup]][];

                Point[] points = polygon.GetGroupPoints(nGroup);
                for (int nPoint = 0; nPoint < points.Length; nPoint++)
                {
                    rings[nGroup][nPoint] = new double[2];
                    rings[nGroup][nPoint][0] = points[nPoint].X;
                    rings[nGroup][nPoint][1] = points[nPoint].Y;
                }
            }

            GPPolygon gppolygon = new GPPolygon();
            gppolygon.Rings = rings;
            gppolygon.SpatialReference = new GPSpatialReference(WKID);

            return gppolygon;
        }

        /// <summary>
        /// Converts the specified <see cref="GPPolygon"/> object to the <see cref="Polygon"/> one.
        /// </summary>
        /// <param name="polygon">The reference to the <see cref="GPPolygon"/> object to be
        /// converted.</param>
        /// <returns>A new <see cref="Polygon"/> object equivalent to the specified
        /// polygon.</returns>
        public static Polygon GPPolygonToPolygon(GPPolygon polygon)
        {
            return _MakePolyCurve(polygon.Rings, (groups, points) => new Polygon(groups, points));
        }

        /// <summary>
        /// Method converts Polyline object to GPPolyline object.
        /// </summary>
        /// <param name="polyline">Polyline object to convert.</param>
        /// <returns>GPPolyline object.</returns>
        public static GPPolyline PolylineToGPPolyline(Polyline polyline)
        {
            double[][][] paths = new double[polyline.Groups.Length][][];
            for (int nGroup = 0; nGroup < polyline.Groups.Length; nGroup++)
            {
                paths[nGroup] = new double[polyline.Groups[nGroup]][];

                Point[] points = polyline.GetGroupPoints(nGroup);
                for (int nPoint = 0; nPoint < points.Length; nPoint++)
                {
                    paths[nGroup][nPoint] = new double[3];
                    paths[nGroup][nPoint][0] = points[nPoint].X;
                    paths[nGroup][nPoint][1] = points[nPoint].Y;
                    paths[nGroup][nPoint][2] = points[nPoint].M;
                }
            }

            GPPolyline gppolyline = new GPPolyline();
            gppolyline.Paths = paths;
            gppolyline.SpatialReference = new GPSpatialReference(WKID);

            return gppolyline;
        }

        /// <summary>
        /// Converts the specified <see cref="GPPolyline"/> object to the <see cref="Polyline"/> one.
        /// </summary>
        /// <param name="polygon">The reference to the <see cref="GPPolyline"/> object to be
        /// converted.</param>
        /// <returns>A new <see cref="Polyline"/> object equivalent to the specified
        /// polyline.</returns>
        public static Polyline GPPolylineToPolyline(GPPolyline polyline)
        {
            return _MakePolyCurve(polyline.Paths, (groups, points) => new Polyline(groups, points));
        }

        /// <summary>
        /// Method gets type by its Json name.
        /// </summary>
        /// <param name="typeName">Type name.</param>
        /// <returns>Type object.</returns>
        public static Type GetTypeByJsonName(string typeName)
        {
            Debug.Assert(typeName != null);

            Type type = null;
            if (typeName.Equals(GPTYPE_GPFeatureRecordSetLayer,
                StringComparison.CurrentCultureIgnoreCase))
                type = typeof(GPFeatureRecordSetLayer);
            else if (typeName.Equals(GPTYPE_GPRecordSet,
                StringComparison.CurrentCultureIgnoreCase))
                type = typeof(GPRecordSet);

            return type;
        }

        /// <summary>
        /// Converts <see cref="System.DateTime"/> values into ArcGIS REST API JSON format.
        /// </summary>
        /// <param name="value">The <see cref="System.DateTime"/> instance
        /// to be converted.</param>
        /// <returns>Converted <see cref="System.DateTime"/> object.</returns>
        public static long DateTimeToGPDateTime(DateTime value)
        {
            var result = (value - START_DATE).Ticks / TICKS_IN_MILLISECOND;

            return result;
        }

        /// <summary>
        /// Converts <see cref="System.DateTime"/> values from ArcGIS REST API JSON format.
        /// </summary>
        /// <param name="value">The number of milliseconds to be converted to
        /// <see cref="System.DateTime"/> value.</param>
        /// <returns>Converted <see cref="System.DateTime"/> object.</returns>
        public static DateTime GPDateTimeToDateTime(long value)
        {
            var ticks = value * TICKS_IN_MILLISECOND;
            var result = START_DATE.AddTicks(ticks);

            return result;
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Creates <see cref="PolyCurve"/> geometry from the specified data using the specified
        /// constructor.
        /// </summary>
        /// <typeparam name="TGeometry">The type of the geometry to be created.</typeparam>
        /// <param name="data">The data of the geometry to be created.</param>
        /// <param name="constructor">The function creating new <typeparamref name="TGeometry"/>
        /// object from arrays of groups and points.</param>
        /// <returns>A new <typeparamref name="TGeometry"/> object with the specified
        /// data.</returns>
        private static TGeometry _MakePolyCurve<TGeometry>(
            double[][][] data,
            Func<int[], Point[], TGeometry> constructor)
            where TGeometry : PolyCurve
        {
            var groups = data.Select(group => group.Length).ToArray();
            var points = data.SelectMany(_ => _).Select(_MakePoint).ToArray();

            return constructor(groups, points);
        }

        /// <summary>
        /// Creates point from the specified array storing its coordinates.
        /// </summary>
        /// <param name="pointData">The reference to the array of point coordinates to
        /// create point from.</param>
        /// <returns>A new point with data from the specified array.</returns>
        private static Point _MakePoint(double[] pointData)
        {
            Debug.Assert(pointData != null);
            Debug.Assert(pointData.Length == 2);

            return new Point(pointData[0], pointData[1]);
        }
        #endregion

        #region Private constants

        /// <summary>
        /// Geometry WKID.
        /// </summary>
        private const int WKID = 4326;

        /// <summary>
        /// GPType of FeatureRecordSetLayer.
        /// </summary>
        private const string GPTYPE_GPFeatureRecordSetLayer = "GPFeatureRecordSetLayer";

        /// <summary>
        /// GPType of RecordSet.
        /// </summary>
        private const string GPTYPE_GPRecordSet = "GPRecordSet";

        /// <summary>
        /// DateTime values returned by the ArcGIS REST API are encoded as a number
        /// of milliseconds since this date.
        /// </summary>
        private static readonly DateTime START_DATE = new DateTime(1970, 1, 1);

        /// <summary>
        /// The number of ticks in one millisecond.
        /// </summary>
        private static readonly long TICKS_IN_MILLISECOND = new TimeSpan(0, 0, 0, 0, 1).Ticks;
        #endregion
    }
}
