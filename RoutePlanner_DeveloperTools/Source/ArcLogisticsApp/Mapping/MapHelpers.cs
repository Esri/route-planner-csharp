using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Helper class for work with map.
    /// </summary>
    internal static class MapHelpers
    {
        #region Public static methods

        /// <summary>
        /// Check rectangle intersects with geometry.
        /// </summary>
        /// <param name="frame">Rectangle.</param>
        /// <param name="geometry">Geometry to check.</param>
        /// <returns>True if intersects</returns>
        public static bool IsIntersects(ESRI.ArcGIS.Client.Geometry.Envelope frame, ESRI.ArcGIS.Client.Geometry.Geometry geometry)
        {
            bool isIntersects = false;

            if (geometry is ESRI.ArcGIS.Client.Geometry.MapPoint)
            {
                ESRI.ArcGIS.Client.Geometry.MapPoint point = (ESRI.ArcGIS.Client.Geometry.MapPoint)geometry;
                if (frame.Extent.Intersects(point.Extent))
                    isIntersects = true;
            }
            else if (geometry is ESRI.ArcGIS.Client.Geometry.Polyline)
            {
                ESRI.ArcGIS.Client.Geometry.Polyline polyline = (ESRI.ArcGIS.Client.Geometry.Polyline)geometry;
                if (frame.Extent.Intersects(polyline.Extent))
                {
                    foreach (ESRI.ArcGIS.Client.Geometry.PointCollection points in polyline.Paths)
                    {
                        ESRI.ArcGIS.Client.Geometry.MapPoint prevPoint = null;
                        foreach (ESRI.ArcGIS.Client.Geometry.MapPoint point in points)
                        {
                            if (prevPoint != null)
                            {
                                if (_IsSegmentIntersectRectangle(frame.XMin, frame.YMin, frame.XMax, frame.YMax,
                                    point.X, point.Y, prevPoint.X, prevPoint.Y))
                                {
                                    isIntersects = true;
                                    break;
                                }
                            }

                            prevPoint = point;
                        }

                        if (isIntersects)
                            break;
                    }
                }
            }
            else if (geometry is ESRI.ArcGIS.Client.Geometry.Polygon)
            {
                ESRI.ArcGIS.Client.Geometry.Polygon polygon = (ESRI.ArcGIS.Client.Geometry.Polygon)geometry;
                if (frame.Extent.Intersects(polygon.Extent))
                {
                    foreach (ESRI.ArcGIS.Client.Geometry.PointCollection points in polygon.Rings)
                    {
                        ESRI.ArcGIS.Client.Geometry.MapPoint prevPoint = null;
                        foreach (ESRI.ArcGIS.Client.Geometry.MapPoint point in points)
                        {
                            if (prevPoint != null)
                            {
                                if (_IsSegmentIntersectRectangle(frame.XMin, frame.YMin, frame.XMax, frame.YMax,
                                    point.X, point.Y, prevPoint.X, prevPoint.Y))
                                {
                                    isIntersects = true;
                                    break;
                                }
                            }

                            prevPoint = point;
                        }

                        if (isIntersects)
                            break;
                    }
                }
            }
            else
                Debug.Assert(false);

            return isIntersects;
        }

        /// <summary>
        /// Find stops, that goes after intersection of schedule routes and rect.
        /// </summary>
        /// <param name="schedule">Schedule with routes array.</param>
        /// <param name="rect">Extent.</param>
        /// <returns>Nearest stops.</returns>
        public static IList<Stop> FindNearestPreviousStops(Schedule schedule, Envelope rect)
        {
            List<Stop> stops = new List<Stop>();

            foreach (Route route in schedule.Routes)
            {
                List<Stop> routeStops = CommonHelpers.GetSortedStops(route);

                if (routeStops.Count > 0)
                {
                    bool trueRouteExists = false;
                    if (App.Current.MapDisplay.TrueRoute)
                    {
                        trueRouteExists = _AddStopsInExtentOnTrueRoute(rect, routeStops, stops);
                    }

                    if (!App.Current.MapDisplay.TrueRoute || !trueRouteExists)
                    {
                        _AddStopsInExtentOnStraightRoute(rect, routeStops, stops);
                    }
                }
            }

            return stops;
        }

        /// <summary>
        /// Is line intersects with extent.
        /// Implementation of Cohen-Sutherland algorithm.
        /// </summary>
        /// <param name="extent">Extent.</param>
        /// <param name="startPoint">Line start.</param>
        /// <param name="endPoint">Line end.</param>
        /// <returns>Is line intersects with extent.</returns>
        public static bool _IsLineIntersectsWithRect(Envelope extent, Point startPoint, Point endPoint)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint start = new ESRI.ArcGIS.Client.Geometry.MapPoint(startPoint.X, startPoint.Y);
            ESRI.ArcGIS.Client.Geometry.MapPoint end = new ESRI.ArcGIS.Client.Geometry.MapPoint(endPoint.X, endPoint.Y);

            ESRI.ArcGIS.Client.Geometry.Envelope rect = new ESRI.ArcGIS.Client.Geometry.Envelope(
                extent.left, extent.top, extent.right, extent.bottom);

            int code_a, code_b, code;
            ESRI.ArcGIS.Client.Geometry.MapPoint temp;

            code_a = _GetPointCode(rect, start);
            code_b = _GetPointCode(rect, end);

            while (code_a > 0 || code_b > 0)
            {
                // If both points on one side, than line does not intersects extent.
                if ((code_a & code_b) > 0)
                    return false;

                if (code_a > 0)
                {
                    code = code_a;
                    temp = start;
                }
                else
                {
                    code = code_b;
                    temp = end;
                }

                if ((code & LEFT_CODE) > 0)
                {
                    temp.Y = temp.Y + (start.Y - end.Y) * (rect.XMin - temp.X) / (start.X - end.X);
                    temp.X = rect.XMin;
                }
                else if ((code & RIGHT_CODE) > 0)
                {
                    temp.Y += (start.Y - end.Y) * (rect.XMax - temp.X) / (start.X - end.X);
                    temp.X = rect.XMax;
                }

                if ((code & BOTTOM_CODE) > 0)
                {
                    temp.X += (start.X - end.X) * (rect.YMin - temp.Y) / (start.Y - end.Y);
                    temp.Y = rect.YMin;
                }
                else if ((code & TOP_CODE) > 0)
                {
                    temp.X += (start.X - end.X) * (rect.YMax - temp.Y) / (start.Y - end.Y);
                    temp.Y = rect.YMax;
                }

                if (code == code_a)
                    code_a = _GetPointCode(rect, start);
                else
                    code_b = _GetPointCode(rect, end);
            }

            return true;
        }

        /// <summary>
        /// Find graphic in layers, associated to item.
        /// </summary>
        /// <param name="item">Source item.</param>
        /// <param name="objectLayers">Layers collection.</param>
        /// <returns>Graphic.</returns>
        public static ESRI.ArcGIS.Client.Graphic GetGraphicByDataItem(object item, IList<ObjectLayer> objectLayers)
        {
            Debug.Assert(item != null);
            Debug.Assert(objectLayers != null);

            // Find graphic, which represents edited item.
            ESRI.ArcGIS.Client.Graphic graphic = null;

            foreach (ObjectLayer layer in objectLayers)
            {
                graphic = layer.FindGraphicByData(item);

                if (graphic != null)
                {
                    break;
                }
            }

            return graphic;
        }

        /// <summary>
        /// Find layer which contains data.
        /// </summary>
        /// <param name="data">Object to find.</param>
        /// <param name="objectLayers">Object layers for searching in.</param>
        /// <returns>Layer, that contains graphic, associated to object.</returns>
        public static ObjectLayer GetLayerWithData(object data, IList<ObjectLayer> objectLayers)
        {
            ObjectLayer layerWithData = null;

            foreach (ObjectLayer objectLayer in objectLayers)
            {
                if (objectLayer.Collection != null)
                {
                    bool isContains = CollectionContainsData(objectLayer.Collection, data);
                    if (isContains)
                    {
                        layerWithData = objectLayer;
                        break;
                    }
                }
            }

            return layerWithData;
        }

        /// <summary>
        /// Function, that implements "Contains" functionality.
        /// </summary>
        /// <param name="iEnumerable">Collection to search.</param>
        /// <param name="data">Object to search.</param>
        /// <returns>True, if object contains in collections. False otherwise.</returns>
        public static bool CollectionContainsData(IEnumerable iEnumerable, object data)
        {
            bool isContains = false;

            if (iEnumerable is IList)
            {
                IList list = (IList)iEnumerable;
                isContains = list != null && list.Contains(data);
            }
            else
            // todo: replace when all collection will implements IList
            if (iEnumerable is ICollection<Route>)
            {
                if (data is Route)
                {
                    ICollection<Route> list = (ICollection<Route>)iEnumerable;
                    isContains = list != null && list.Contains((Route)data);
                }
            }
            else if (iEnumerable is ICollection<Location>)
            {
                if (data is Location)
                {
                    ICollection<Location> list = (ICollection<Location>)iEnumerable;
                    isContains = list != null && list.Contains((Location)data);
                }
            }
            else if (iEnumerable is ICollection<Order>)
            {
                if (data is Order)
                {
                    ICollection<Order> list = (ICollection<Order>)iEnumerable;
                    isContains = list != null && list.Contains((Order)data);
                }
            }
            else if (iEnumerable is ICollection<Zone>)
            {
                if (data is Zone)
                {
                    ICollection<Zone> list = (ICollection<Zone>)iEnumerable;
                    isContains = list != null && list.Contains((Zone)data);
                }
            }
            else if (iEnumerable is ICollection<Barrier>)
            {
                Barrier barrier = data as Barrier;
                if (barrier != null)
                {
                    ICollection<Barrier> list = (ICollection<Barrier>)iEnumerable;
                    isContains = list != null && list.Contains(barrier);
                }
            }
            else
                Debug.Assert(false);

            return isContains;
        }

        /// <summary>
        /// Convert scalebar units to map scalebar units.
        /// </summary>
        /// <param name="scaleBarUnits">Scalebar units.</param>
        /// <returns>Scalebar map units.</returns>
        public static ESRI.ArcGIS.Client.ScaleBarUnit ConvertScalebarUnits(ScaleBarUnit scaleBarUnits)
        {
            ESRI.ArcGIS.Client.ScaleBarUnit mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Undefined;

            switch (scaleBarUnits)
            {
                case ScaleBarUnit.Undefined:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Undefined;
                        break;
                    }
                case ScaleBarUnit.Inches:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Inches;
                        break;
                    }
                case ScaleBarUnit.Feet:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Feet;
                        break;
                    }
                case ScaleBarUnit.Yards:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Yards;
                        break;
                    }
                case ScaleBarUnit.Miles:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Miles;
                        break;
                    }
                case ScaleBarUnit.NauticalMiles:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.NauticalMiles;
                        break;
                    }
                case ScaleBarUnit.Millimeters:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Millimeters;
                        break;
                    }
                case ScaleBarUnit.Centimeters:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Centimeters;
                        break;
                    }
                case ScaleBarUnit.Meters:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Meters;
                        break;
                    }
                case ScaleBarUnit.Kilometers:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Kilometers;
                        break;
                    }
                case ScaleBarUnit.DecimalDegrees:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.DecimalDegrees;
                        break;
                    }
                case ScaleBarUnit.Decimeters:
                    {
                        mapScalebarUnits = ESRI.ArcGIS.Client.ScaleBarUnit.Decimeters;
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }

            return mapScalebarUnits;
        }

        /// <summary>
        /// Convert from ArcLogistics polygon to ArcGIS polygon.
        /// </summary>
        /// <param name="sourcePolygon">ArcLogistics polygon</param>
        /// <param name="spatialReferenceID">Map spatial reference.</param>
        /// <returns>ArcGIS polygon.</returns>
        internal static ESRI.ArcGIS.Client.Geometry.Polygon ConvertToArcGISPolygon(Polygon sourcePolygon,
            int? spatialReferenceID)
        {
            ESRI.ArcGIS.Client.Geometry.Polygon resultPolygon = new ESRI.ArcGIS.Client.Geometry.Polygon();

            // Project polygon from WGS84 to Web Mercator if spatial reference of map is Web Mercator.
            if (spatialReferenceID != null)
            {
                sourcePolygon = WebMercatorUtil.ProjectPolygonToWebMercator(sourcePolygon, spatialReferenceID.Value);
            }

            int[] groups = sourcePolygon.Groups;
            for (int groupIndex = 0; groupIndex < groups.Length; ++groupIndex)
            {
                ESRI.ArcLogistics.Geometry.Point[] points = sourcePolygon.GetGroupPoints(groupIndex);

                ESRI.ArcGIS.Client.Geometry.PointCollection pointsCollection = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                for (int index = 0; index < points.Length; index++)
                {
                    ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = new ESRI.ArcGIS.Client.Geometry.MapPoint(points[index].X, points[index].Y);
                    pointsCollection.Add(mapPoint);
                }

                resultPolygon.Rings.Add(pointsCollection);
            }

            return resultPolygon;
        }

        /// <summary>
        /// Convert from ArcLogistics polyline to ArcGIS polyline.
        /// </summary>
        /// <param name="sourcePolyline">ArcLogistics polyline</param>
        /// <param name="spatialReferenceID">Map spatial reference.</param>
        /// <returns>ArcGIS polyline.</returns>
        internal static ESRI.ArcGIS.Client.Geometry.Geometry ConvertToArcGISPolyline(Polyline sourcePolyline,
            int? spatialReferenceID)
        {
            ESRI.ArcGIS.Client.Geometry.Polyline resultPolyline = new ESRI.ArcGIS.Client.Geometry.Polyline();

            // Project polyline from WGS84 to Web Mercator if spatial reference of map is Web Mercator.
            if (spatialReferenceID != null) // REV: comapre with specific Web Mercator WKID, instead of null.
            {
                sourcePolyline = WebMercatorUtil.ProjectPolylineToWebMercator(sourcePolyline, spatialReferenceID.Value);
            }

            int[] groups = sourcePolyline.Groups;
            for (int groupIndex = 0; groupIndex < groups.Length; ++groupIndex)
            {
                ESRI.ArcLogistics.Geometry.Point[] points = sourcePolyline.GetGroupPoints(groupIndex);

                ESRI.ArcGIS.Client.Geometry.PointCollection pointsCollection = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                for (int index = 0; index < points.Length; index++)
                {
                    ESRI.ArcGIS.Client.Geometry.MapPoint mapPoint = new ESRI.ArcGIS.Client.Geometry.MapPoint(
                        points[index].X, points[index].Y);
                    pointsCollection.Add(mapPoint);
                }

                resultPolyline.Paths.Add(pointsCollection);
            }

            return resultPolyline;
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Find stops, that goes after intersection of true route schedule routes and rect. 
        /// </summary>
        /// <param name="rect">Extent.</param>
        /// <param name="routeStops">Route stops array, sorted by sequence number.</param>
        /// <param name="stops">Finded nearest stops.</param>
        private static bool _AddStopsInExtentOnTrueRoute(Envelope rect, List<Stop> routeStops, List<Stop> stops)
        {
            bool trueRouteExists = false;

            foreach (Stop stop in routeStops)
            {
                if (stop.Path != null && !stop.Path.IsEmpty)
                {
                    trueRouteExists = true;

                    if (_IsStopInRect(stop, rect))
                    {
                        stops.Add(stop);
                    }
                }
            }

            return trueRouteExists;
        }

        /// <summary>
        /// Is stop in rect.
        /// </summary>
        /// <param name="stop">Stop, path to which is try to intersect.</param>
        /// <param name="rect">Extent.</param>
        /// <returns>Is stop in rect.</returns>
        private static bool _IsStopInRect(Stop stop, Envelope rect)
        {
            for (int index = 0; index < stop.Path.Groups.Length; index++)
            {
                Point[] points = stop.Path.GetGroupPoints(index);
                for (int pointIndex = 0; pointIndex < points.Length - 1; pointIndex++)
                {
                    if (_IsLineIntersectsWithRect(rect, points[pointIndex], points[pointIndex + 1]))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find stops, that goes after intersection of straight schedule routes and rect.
        /// </summary>
        /// <param name="rect">Extent.</param>
        /// <param name="routeStops">Route stops array, sorted by sequence number.</param>
        /// <param name="stops">Finded nearest stops.</param>
        private static void _AddStopsInExtentOnStraightRoute(Envelope rect, List<Stop> routeStops, List<Stop> stops)
        {
            Point startPoint = routeStops[0].MapLocation.Value;
            for (int stopIndex = 1; stopIndex < routeStops.Count; stopIndex++)
            {
                Stop stop = routeStops[stopIndex];
                if (stop.MapLocation != null)
                {
                    Point endPoint = stop.MapLocation.Value;

                    if (_IsLineIntersectsWithRect(rect, startPoint, endPoint))
                    {
                        stops.Add(stop);
                    }

                    startPoint = endPoint;
                }
            }
        }

        /// <summary>
        /// Get point-rectangle relative code by cohen-sutherland algorithm.
        /// </summary>
        /// <param name="rect">Rectangle.</param>
        /// <param name="point">Point.</param>
        /// <returns>Point-rectangle relative code.</returns>
        private static int _GetPointCode(ESRI.ArcGIS.Client.Geometry.Envelope rect, ESRI.ArcGIS.Client.Geometry.MapPoint point)
        {
            int code = 0;

            code += point.X < rect.XMin ? LEFT_CODE : 0;
            code += point.X > rect.XMax ? RIGHT_CODE : 0;
            code += point.Y < rect.YMin ? BOTTOM_CODE : 0;
            code += point.Y > rect.YMax ? TOP_CODE : 0;

            return code;
        }

        /// <summary>
        /// Check segment intersects rectangle.
        /// </summary>
        /// <param name="rectMinX">Rectangle left coord.</param>
        /// <param name="rectMinY">Rectangle right coord.</param>
        /// <param name="rectMaxX">Rectangle top coord.</param>
        /// <param name="rectMaxY">Rectangle bottom coord.</param>
        /// <param name="x1">Segment start x coord.</param>
        /// <param name="y1">Segment start y coord.</param>
        /// <param name="x2">Segment end x coord.</param>
        /// <param name="y2">Segment end y coord.</param>
        /// <returns>Is segment intersects rectangle.</returns>
        private static bool _IsSegmentIntersectRectangle(double rectMinX, double rectMinY, double rectMaxX, double rectMaxY,
            double x1, double y1, double x2, double y2)
        {
            double minX = x1;
            double maxX = x2;
            if (x1 > x2)
            {
                minX = x2;
                maxX = x1;
            }

            // Find the intersection of the line's and rectangle's x-projections.
            if (maxX > rectMaxX)
                maxX = rectMaxX;

            if (minX < rectMinX)
                minX = rectMinX;

            // If their projections do not intersect - intersection absent.
            if (minX > maxX)
                return false;

            // Find corresponding min and max Y for min and max X we found before.
            double minY = y1;
            double maxY = y2;
            double dx = x2 - x1;
            if (Math.Abs(dx) > 0.0000001)
            {
                double k = (y2 - y1) / dx;
                double b = y1 - k * x1;
                minY = k * minX + b;
                maxY = k * maxX + b;
            }

            if (minY > maxY)
            {
                double tmp = maxY;
                maxY = minY;
                minY = tmp;
            }

            // Find the intersection of the line's and rectangle's y-projections.   
            if (maxY > rectMaxY)
                maxY = rectMaxY;

            if (minY < rectMinY)
                minY = rectMinY;

            // If Y-projections do not intersect - intersection absent.
            if (minY > maxY)
                return false;

            return true;
        }

        #endregion

        #region Constants

        // Cohen-Sutherland algorithm constants.
        private const int LEFT_CODE = 1;
        private const int RIGHT_CODE = 2;
        private const int BOTTOM_CODE = 4;
        private const int TOP_CODE = 8;

        #endregion
    }
}
