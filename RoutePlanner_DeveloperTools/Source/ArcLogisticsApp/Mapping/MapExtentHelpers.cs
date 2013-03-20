using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Helper class for work with map extent.
    /// </summary>
    internal static class MapExtentHelpers
    {
        #region Public static methods

        /// <summary>
        /// Get extent for point collection.
        /// </summary>
        /// <param name="points">Points.</param>
        /// <returns>Envelope, which contains all points.</returns>
        public static ESRI.ArcLogistics.Geometry.Envelope? GetCollectionExtent(IList<ESRI.ArcLogistics.Geometry.Point> points)
        {
            ESRI.ArcLogistics.Geometry.Envelope? result = null;

            if (points.Count > 0)
            {
                ESRI.ArcLogistics.Geometry.Envelope rect = new ESRI.ArcLogistics.Geometry.Envelope();
                rect.SetEmpty();

                bool atLeastOnePointHavePosition = false;
                foreach (ESRI.ArcLogistics.Geometry.Point point in points)
                {
                    rect.Union(point);
                    atLeastOnePointHavePosition = true;
                }

                if (atLeastOnePointHavePosition)
                {
                    // Increase extent.
                    double heigthInc = EXTENT_INDENT * rect.Height;
                    double widthInc = EXTENT_INDENT * rect.Width;
                    if (heigthInc == 0)
                    {
                        heigthInc = ZOOM_ON_STREET;
                    }
                    if (widthInc == 0)
                    {
                        widthInc = ZOOM_ON_STREET;
                    }

                    rect.left -= widthInc;
                    rect.right += widthInc;
                    rect.top += heigthInc;
                    rect.bottom -= heigthInc;

                    result = rect;
                }
            }

            return result;
        }

        /// <summary>
        /// Set map extent on point collection.
        /// </summary>
        /// <param name="mapCtrl">Map control.</param>
        /// <param name="points">Points collection.</param>
        public static void SetExtentOnCollection(MapControl mapCtrl, IList<ESRI.ArcLogistics.Geometry.Point> points)
        {
            ESRI.ArcLogistics.Geometry.Envelope? rect = GetCollectionExtent(points);

            if (rect.HasValue)
            {
                ESRI.ArcGIS.Client.Geometry.Envelope extent = GeometryHelper.CreateExtent(rect.Value,
                    mapCtrl.Map.SpatialReferenceID);
                mapCtrl.ZoomTo(extent);
            }
        }

        /// <summary>
        /// Zoom to candidate depends on locator type.
        /// </summary>
        /// <param name="mapCtrl">Map control.</param>
        /// <param name="addressCandidate">Candidate to zoom.</param>
        public static void ZoomToCandidates(MapControl mapCtrl, AddressCandidate[] addressCandidates)
        {
            Debug.Assert(mapCtrl != null);
            Debug.Assert(addressCandidates != null);

            LocatorType? locatorType = null;
            if (addressCandidates.Length == 1)
            {
                locatorType = GeocodeHelpers.GetLocatorTypeOfCandidate(addressCandidates[0]);
            }

            if (locatorType == null)
            {
                // If not composite locator - zoom to candidate.
                List<ESRI.ArcLogistics.Geometry.Point> points = new List<ESRI.ArcLogistics.Geometry.Point>();
                foreach (AddressCandidate addressCandidate in addressCandidates)
                {
                    points.Add(addressCandidate.GeoLocation);
                }
                SetExtentOnCollection(mapCtrl, points);
            }
            else
            {
                _ZoomToCandidate(mapCtrl, addressCandidates[0], locatorType.Value);
            }
        }

        /// <summary>
        /// Get point of object collection geometries.
        /// </summary>
        /// <param name="coll">Object collection.</param>
        /// <returns></returns>
        public static List<Point> GetPointsInExtent(IEnumerable coll)
        {
            List<ESRI.ArcLogistics.Geometry.Point> points = new List<ESRI.ArcLogistics.Geometry.Point>();

            // Add location to extent.
            foreach (object obj in coll)
            {
                if (obj is Location)
                {
                    Location location = obj as Location;
                    if (location.GeoLocation != null)
                        points.Add(location.GeoLocation.Value);
                }
                else if (obj is Order)
                {
                    Order order = obj as Order;
                    if (order.GeoLocation != null && App.Current.MapDisplay.AutoZoom)
                        points.Add(order.GeoLocation.Value);
                }
                else if (obj is Stop)
                {
                    Stop stop = (Stop)obj;
                    if (stop.MapLocation != null && App.Current.MapDisplay.AutoZoom)
                        points.Add(stop.MapLocation.Value);
                }
                else if (obj is Route)
                {
                    if (App.Current.MapDisplay.AutoZoom)
                        _AddRoutePointsToExtent((Route)obj, points);
                }
                else if (obj is Zone)
                {
                    if (App.Current.MapDisplay.AutoZoom)
                        _AddRegionToExtent(obj, points);
                }
                else if (obj is Barrier)
                {
                    _AddRegionToExtent(obj, points);
                }
                else if (obj is AddressCandidate)
                {
                    AddressCandidate addressCandidate = (AddressCandidate)obj;
                    points.Add(addressCandidate.GeoLocation);
                }
                else
                    Debug.Assert(false);
            }

            return points;
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Zoom to candidate depends on locator type.
        /// </summary>
        /// <param name="mapCtrl">Map control.</param>
        /// <param name="addressCandidate">Candidate to zoom.</param>
        /// <param name="locatorType">Type of locator, which return this candidate.</param>
        private static void _ZoomToCandidate(MapControl mapCtrl, AddressCandidate addressCandidate, LocatorType locatorType)
        {
            Debug.Assert(mapCtrl != null);
            Debug.Assert(addressCandidate != null);

            double extentInc = 0;

            // Get extent size.
            switch (locatorType)
            {
                case LocatorType.CityState:
                    {
                        extentInc = ZOOM_ON_CITY_STATE_CANDIDATE;
                        break;
                    }
                case LocatorType.Zip:
                    {
                        extentInc = ZOOM_ON_ZIP_CANDIDATE;
                        break;
                    }
                case LocatorType.Street:
                    {
                        extentInc = ZOOM_ON_STREET_CANDIDATE;
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }

            // Make extent rectangle.
            ESRI.ArcLogistics.Geometry.Envelope rect = new ESRI.ArcLogistics.Geometry.Envelope();
            rect.SetEmpty();
            rect.Union(addressCandidate.GeoLocation);

            rect.left -= extentInc;
            rect.right += extentInc;
            rect.top += extentInc;
            rect.bottom -= extentInc;

            ESRI.ArcGIS.Client.Geometry.Envelope extent = GeometryHelper.CreateExtent(rect,
                mapCtrl.Map.SpatialReferenceID);
            mapCtrl.ZoomTo(extent);
        }

        /// <summary>
        /// Add all points, which need to be in extent of route, to point list.
        /// </summary>
        /// <param name="route">Route to get points.</param>
        /// <param name="points">Point list.</param>
        private static void _AddRoutePointsToExtent(Route route, IList<ESRI.ArcLogistics.Geometry.Point> points)
        {
            if (App.Current.MapDisplay.TrueRoute)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.Path != null && !stop.Path.IsEmpty)
                    {
                        for (int index = 0; index < stop.Path.Groups.Length; index++)
                        {
                            ESRI.ArcLogistics.Geometry.Point[] pointsArray = stop.Path.GetGroupPoints(index);
                            foreach (ESRI.ArcLogistics.Geometry.Point point in pointsArray)
                                points.Add(point);
                        }
                    }

                    if (stop.MapLocation.HasValue)
                        points.Add(stop.MapLocation.Value);
                }
            }
            else
            {
                foreach (Stop stop in route.Stops)
                    if (stop.MapLocation.HasValue)
                        points.Add(stop.MapLocation.Value);
            }
        }
        /// <summary>
        /// Add all points, which need to be in extent of region object, to point list.
        /// </summary>
        /// <param name="obj">Region object to get points.</param>
        /// <param name="points">Point list.</param>
        private static void _AddRegionToExtent(object obj, List<Point> points)
        {
            object geometry = (obj is Zone) ? (obj as Zone).Geometry : (obj as Barrier).Geometry;
            if (null != geometry)
            {
                ESRI.ArcLogistics.Geometry.Point? pointGeometry = geometry as ESRI.ArcLogistics.Geometry.Point?;
                if (pointGeometry != null)
                {
                    points.Add(pointGeometry.Value);
                }
                else
                {
                    Debug.Assert(geometry is ESRI.ArcLogistics.Geometry.PolyCurve);
                    ESRI.ArcLogistics.Geometry.PolyCurve polyCurve = geometry as ESRI.ArcLogistics.Geometry.PolyCurve;
                    for (int index = 0; index < polyCurve.Groups.Length; index++)
                    {
                        ESRI.ArcLogistics.Geometry.Point[] pointsArray = polyCurve.GetGroupPoints(index);
                        foreach (ESRI.ArcLogistics.Geometry.Point point in pointsArray)
                            points.Add(point);
                    }
                }
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Distance in degrees from object to extent restriction for one object.
        /// </summary>
        private const double ZOOM_ON_STREET = 0.006;

        /// <summary>
        /// Indent for extent of data objects.
        /// </summary>
        private const double EXTENT_INDENT = 0.05;

        /// <summary>
        /// Distance in degrees from object to extent restriction for street candidate.
        /// </summary>
        private const double ZOOM_ON_STREET_CANDIDATE = 0.02;

        /// <summary>
        /// Distance in degrees from object to extent restriction for zip candidate.
        /// </summary>
        private const double ZOOM_ON_ZIP_CANDIDATE = 0.03;

        /// <summary>
        /// Distance in degrees from object to extent restriction for citystate candidate.
        /// </summary>
        private const double ZOOM_ON_CITY_STATE_CANDIDATE = 0.1;

        #endregion
    }
}
