using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Class provide helper functions for directions process.
    /// </summary>
    internal sealed class DirectionsHelper
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts directions to bytes array.
        /// </summary>
        /// <param name="directions">Directions to convert.</param>
        /// <returns>Direction data in byte array.</returns>
        public static byte[] ConvertToBytes(Direction[] directions)
        {
            Debug.Assert(directions != null);

            byte[] bytes = null;
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    // directions number
                    writer.Write(directions.Length);

                    // directions
                    foreach (Direction dir in directions)
                    {
                        writer.Write(dir.Length);
                        writer.Write(dir.Time);
                        writer.Write(dir.Text);
                        writer.Write(dir.Geometry ?? string.Empty);
                        writer.Write((int)dir.ManeuverType);
                    }

                    bytes = ms.ToArray();
                }
            }

            return bytes;
        }

        /// <summary>
        /// Converts bytes array to directions.
        /// </summary>
        /// <param name="bytes">Data in bytes.</param>
        /// <returns>Directions.</returns>
        public static Direction[] ConvertFromBytes(byte[] bytes)
        {
            Debug.Assert(bytes != null);

            Direction[] dirs = null;
            using (var ms = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(ms))
                {
                    // directions number
                    int dirCount = reader.ReadInt32();

                    // directions
                    dirs = new Direction[dirCount];
                    for (int nDir = 0; nDir < dirs.Length; nDir++)
                    {
                        var dir = new Direction();
                        dir.Length = reader.ReadDouble();
                        dir.Time = reader.ReadDouble();
                        dir.Text = reader.ReadString();
                        dir.Geometry = reader.ReadString();
                        dir.ManeuverType = (StopManeuverType)reader.ReadInt32();
                        dirs[nDir] = dir;
                    }
                }
            }

            return dirs;
        }

        /// <summary>
        /// Converts the specified feature with compact geometry to the
        /// <see cref="Direction"/> value.
        /// </summary>
        /// <param name="feature">The reference to the feature to be converted.</param>
        /// <returns>A new <see cref="Direction"/> value corresponding to the
        /// <paramref name="feature"/>.</returns>
        public static DirectionEx ConvertToDirection(GPCompactGeomFeature feature)
        {
            Debug.Assert(feature != null);

            var dir = new DirectionEx();

            // Length.
            dir.Length = feature.Attributes.Get<double>(NAAttribute.DIR_LENGTH);
            // Time.
            dir.Time = feature.Attributes.Get<double>(NAAttribute.DIR_TIME);
            // Text.
            dir.Text = feature.Attributes.Get<string>(NAAttribute.DIR_TEXT);
            // Geometry.
            dir.Geometry = feature.CompressedGeometry;
            // Direction type.
            dir.DirectionType = DirectionType.ManeuverDirection;

            // Maneuver type.
            var maneuverType =
                feature.Attributes.Get<NADirectionsManeuverType>(NAAttribute.DIR_MANEUVER_TYPE);

            dir.ManeuverType = _ConvertManeuverType(maneuverType);

            return dir;
        }

        public static T Identity<T>(T value)
        {
            return value;
        }

        /// <summary>
        /// Converts the specified feature to the <see cref="Direction"/> value.
        /// </summary>
        /// <param name="feature">The reference to the feature to be converted.</param>
        /// <returns>A new <see cref="Direction"/> value corresponding to the
        /// <paramref name="feature"/>.</returns>
        public static DirectionEx ConvertToDirection(GPFeature feature)
        {
            Debug.Assert(feature != null);

            var direction = new DirectionEx
            {
                DirectionType = ArcLogistics.DirectionType.ManeuverDirection,
                Length = feature.Attributes.Get<double>(NAAttribute.DirectionsDriveDistance),
                Text = feature.Attributes.Get<string>(NAAttribute.DirectionsText),
            };

            var geometry = default(IEnumerable<Point>);
            Debug.Assert(feature.Geometry != null && feature.Geometry.Value != null);

            var points = (GPPolyline)feature.Geometry.Value;
            var allPoints = points.Paths.SelectMany(Identity);
            var haveMCoordinate = allPoints.All(point => point.Length == 3);
            if (haveMCoordinate)
                geometry = allPoints.Select(point => new Point(point[0], point[1], point[2]));
            else
                geometry = allPoints.Select(point => new Point(point[0], point[1]));

            direction.Geometry = CompactGeometryConverter.Convert(geometry);

            // Read maneuver type and drive time.
            var type = NADirectionsManeuverType.esriDMTUnknown;
            var subItemType = feature.Attributes.Get<NADirectionsSubItemType>(
                NAAttribute.DirectionsSubItemType);
            if (subItemType == NADirectionsSubItemType.ManeuverItem)
            {
                type = feature.Attributes.Get<NADirectionsManeuverType>(
                    NAAttribute.DirectionsStringType);

                // Elapsed time is equal to drive time for maneuver direction items which are
                // not arrive/depart ones.
                if (type != NADirectionsManeuverType.esriDMTStop &&
                    type != NADirectionsManeuverType.esriDMTDepart)
                {
                    direction.Time = feature.Attributes.Get<double>(
                        NAAttribute.DirectionsElapsedTime);
                }
            }
            else
                direction.DirectionType = DirectionType.Other;

            direction.ManeuverType = _ConvertManeuverType(type);

            return direction;
        }

        /// <summary>
        /// Sets directions and path geometries to stops.
        /// </summary>
        /// <param name="stops">Stops data.</param>
        /// <param name="directions">Direction features.</param>
        public static void SetDirections(IEnumerable<StopData> stops,
                                         IEnumerable<DirectionEx> directions)
        {
            Debug.Assert(directions != null);
            Debug.Assert(stops != null);

            // Create directions.
            IList<DirectionEx[]> dirs = _BuildDirections(directions);

            // Merge every Lunch directions with Next stop directions.
            IList<Direction[]> mergedDirs = _MergeBreaksDirections(dirs, stops);

            // Fix direction texts.
            _FixDirStrings(stops, mergedDirs);

            // Set directions and paths to stops.
            _SetDirections(mergedDirs, stops);
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds directions.
        /// </summary>
        /// <param name="directions">Direction features.</param>
        /// <returns>Created directions.</returns>
        private static IList<DirectionEx[]> _BuildDirections(
            IEnumerable<DirectionEx> directions)
        {
            Debug.Assert(directions != null);

            var dirs = new List<DirectionEx[]>();

            List<DirectionEx> dirItems = null;
            foreach (var direction in directions)
            {
                if (direction.ManeuverType == StopManeuverType.Depart)
                {
                    // Starting direction.
                    if (dirItems == null)
                    {
                        dirItems = new List<DirectionEx>();
                        dirItems.Add(direction);
                    }
                }
                else if (direction.ManeuverType == StopManeuverType.Stop)
                {
                    // Ending direction.
                    if (dirItems != null)
                    {
                        dirItems.Add(direction);
                        dirs.Add(dirItems.ToArray());
                        dirItems = null;
                    }
                }
                else
                {
                    // Intermediate direction.
                    if (dirItems != null)
                        dirItems.Add(direction);
                }
            }

            return dirs;
        }

        /// <summary>
        /// Method do merge of every Lunch directions with Next stop directions.
        /// Not maneuver, Break Arrive and Break Depart directions are passed.
        /// </summary>
        /// <param name="dirs">Directions.</param>
        /// <param name="stops">Stops.</param>
        /// <returns>Merged directions.</returns>
        private static IList<Direction[]> _MergeBreaksDirections(
            IList<DirectionEx[]> dirs, IEnumerable<StopData> stops)
        {
            Debug.Assert(dirs != null);
            Debug.Assert(stops != null);

            // Sort stops respecting sequence.
            var sortedStops = new List<StopData>(stops);
            SolveHelper.SortBySequence(sortedStops);

            int stopIndex = 0;
            var lunchDirections = new List<DirectionEx>();

            // Flag is needed for cases when breaks goes one by one.
            bool isLunchFoundBefore = false;

            // Flag means that route started from Lunch (route with Virtual Depot).
            bool lunchAtStart = true;
            if (sortedStops[0].StopType != StopType.Lunch)
                lunchAtStart = false;

            // Start from first element since zero element doesn't have directions.
            for (int index = 1; index < sortedStops.Count; ++index)
            {
                if (stopIndex >= dirs.Count)
                {
                    throw new RouteException(
                        Properties.Messages.Error_InvalidDirectionsNumber);
                }

                DirectionEx[] stopDirections = dirs[stopIndex];

                // Find all Lunch Stops.
                if (sortedStops[index].StopType == StopType.Lunch)
                {
                    if (isLunchFoundBefore)
                    {   // This lunch goes after another one: merge lunch directions.
                        lunchDirections.AddRange(
                            _FilterManeuverDirections(stopDirections,
                            StopManeuverType.Stop, StopManeuverType.Depart));

                        // Remove current lunch directions (if it is), to stay with merged ones.
                        if (_IsBreakDirection(dirs[stopIndex]))
                        {
                            dirs.RemoveAt(stopIndex);
                        }
                    }
                    else
                    {
                        if (lunchAtStart)
                            // First lunch is first on route at all: get only Maneuver directions.
                            lunchDirections = _FilterManeuverDirections(stopDirections,
                                StopManeuverType.Stop, StopManeuverType.Depart);
                        else
                            // First lunch in stops queue: get only Maneuver and Depart directions.
                            lunchDirections = _FilterManeuverDirections(stopDirections,
                                StopManeuverType.Stop);

                        // Remove current lunch directions, to stay with merged ones.
                        dirs.RemoveAt(stopIndex);
                    }

                    // Set flag to consider case when breaks goes one by one.
                    isLunchFoundBefore = true;
                }
                // Remove Depart of last Break and merge all found Lunch directions
                // with stop directions.
                else
                {
                    lunchAtStart = false;

                    if (isLunchFoundBefore)
                    {
                        // Final lunch direction: merge lunch and stops directions.
                        var stopsWithoutDepart = _FilterManeuverDirections(stopDirections,
                            StopManeuverType.Depart);
                        lunchDirections.AddRange(stopsWithoutDepart);

                        dirs[stopIndex] = lunchDirections.ToArray();
                        lunchDirections.Clear();
                        isLunchFoundBefore = false;
                    }

                    ++stopIndex;
                }
            }

            return _ConvertToDirectionsWithoutType(dirs);
        }

        /// <summary>
        /// Method does filtering in directions collection and returns
        /// collection of only Maneuver directions with optionally filtered
        /// stop maneuver types.
        /// </summary>
        /// <param name="toFilter">Collection to filter.</param>
        /// <param name="filters">Collection of maneuver types to be filtered.</param>
        /// <returns>Filtered directions collection.</returns>
        private static List<DirectionEx> _FilterManeuverDirections(DirectionEx[] toFilter,
            params StopManeuverType[] filters)
        {
            // Return only maneuver type directions.
            var result = toFilter
                .Where(obj => obj.DirectionType == DirectionType.ManeuverDirection);

            // Filter out all specified stop maneuver types.
            foreach (var filter in filters)
            {
                var filterValue = filter;
                result = result.Where(obj => obj.ManeuverType != filterValue);
            }

            return result.ToList();
        }

        /// <summary>
        /// Check if given direction is for Break.
        /// </summary>
        /// <param name="dirs">Directions list.</param>
        /// <returns>True - if it is Break Direction, otherwise - false.</returns>
        private static bool _IsBreakDirection(DirectionEx[] dirs)
        {
            Debug.Assert(dirs != null);

            bool result = false;

            if (dirs.Length > 0)
            {
                // Check arrive direction.
                DirectionEx dir = dirs.Last();

                string breakPropertyName =
                    (string)Properties.Resources.BreakDirectionName;

                result = dir.Text.Contains(breakPropertyName);
            }

            return result;
        }

        /// <summary>
        /// Method do conversion from extended directions with type to simple Directions.
        /// </summary>
        /// <param name="directionsToConvert">Direction to convert.</param>
        private static IList<Direction[]> _ConvertToDirectionsWithoutType(
            IList<DirectionEx[]> directionsToConvert)
        {
            var resultDirections = new List<Direction[]>();

            foreach (DirectionEx[] directionsWithType in directionsToConvert)
            {
                var directions = new List<Direction>();

                foreach (DirectionEx currentDirection in directionsWithType)
                {
                    directions.Add(new Direction
                    {
                        Geometry = currentDirection.Geometry,
                        Length = currentDirection.Length,
                        ManeuverType = currentDirection.ManeuverType,
                        Text = currentDirection.Text,
                        Time = currentDirection.Time
                    });
                }

                resultDirections.Add(directions.ToArray());
            }

            return resultDirections;
        }

        /// <summary>
        /// Fixes direction strings.
        /// </summary>
        /// <param name="stops">Stop datas.</param>
        /// <param name="dirs">Directions to text update.</param>
        private static void _FixDirStrings(IEnumerable<StopData> stops, IList<Direction[]> dirs)
        {
            Debug.Assert(stops != null);
            Debug.Assert(dirs != null);

            foreach (StopData stop in stops)
            {
                if (stop.StopType != StopType.Lunch)
                {
                    object assocObject = stop.AssociatedObject;
                    if (assocObject != null)
                    {
                        Guid objId;
                        string objName = null;
                        if (_GetStopObjName(assocObject, out objId, out objName))
                            _ReplaceDirText(objId.ToString(), objName, dirs);
                    }
                }
            }
        }

        /// <summary>
        /// Sets directions.
        /// </summary>
        /// <param name="dirs">Directions.</param>
        /// <param name="stops">Stop datas as direction keeper (for update).</param>
        private static void _SetDirections(IList<Direction[]> dirs, IEnumerable<StopData> stops)
        {
            Debug.Assert(stops != null);
            Debug.Assert(dirs != null);

            // sort stops respecting sequence
            var sortedStops = new List<StopData>(stops);
            SolveHelper.SortBySequence(sortedStops);

            int nDir = 0;
            // starts from fisrt, 0 element do not have directions
            for (int index = 1; index < sortedStops.Count; ++index)
            {
                StopData stop = sortedStops[index];
                if (stop.StopType != StopType.Lunch)
                {
                    if (nDir >= dirs.Count)
                        throw new RouteException(Properties.Messages.Error_InvalidDirectionsNumber); // exception

                    Direction[] stopDirs = dirs[nDir];

                    // set path
                    if (_IsDirGeometryValid(stopDirs))
                    {
                        // build path from compact geometry
                        stop.Path = _BuildPath(stopDirs);
                    }
                    else
                    {
                        // NOTE: in some cases compact geometry is not set, for example when
                        // route contains one order which point is the same as start and end
                        // locations points

                        // set geometry of associated object
                        var gc = stop.AssociatedObject as IGeocodable;
                        if (gc != null && gc.IsGeocoded)
                            stop.Path = new Polyline(new Point[] { (Point)gc.GeoLocation });
                        else
                            throw new RouteException(Properties.Messages.Error_BuildStopPathFailed); // exception
                    }

                    // set directions
                    stop.Directions = stopDirs;
                    ++nDir;
                }
            }
        }

        /// <summary>
        /// Gets stop object name.
        /// </summary>
        /// <param name="obj">Associated object (can be null).</param>
        /// <param name="objId">Object's id (readed object id or empty).</param>
        /// <param name="objName">Object's name (readed object name or null).</param>
        /// <returns>TRUE if real read object name.</returns>
        private static bool _GetStopObjName(object obj, out Guid objId, out string objName)
        {
            objId = Guid.Empty;
            objName = null;

            bool res = true;
            if (obj is Order)
            {
                var order = obj as Order;
                if (order.Address != null &&
                    !string.IsNullOrEmpty(order.Address.FullAddress))
                {
                    objName = order.Address.FullAddress;
                }
                else
                    objName = order.Name;

                objId = order.Id;
            }
            else if (obj is Location)
            {
                var loc = obj as Location;
                objName = loc.Name;
                objId = loc.Id;
            }
            else
                res = false;

            return res;
        }

        /// <summary>
        /// Replaces all occurrences of old text in direction's, with new text.
        /// </summary>
        /// <param name="oldText">Text to be replaced.</param>
        /// <param name="newText">Text to replace all occurrences of oldText.</param>
        /// <param name="dirs">Directions.</param>
        private static void _ReplaceDirText(string oldText, string newText, IList<Direction[]> dirs)
        {
            Debug.Assert(oldText != null);
            Debug.Assert(newText != null);
            Debug.Assert(dirs != null);

            foreach (Direction[] dirsItem in dirs)
            {
                for (int nDir = 0; nDir < dirsItem.Length; nDir++)
                {
                    string text = dirsItem[nDir].Text;
                    if (text != null)
                        dirsItem[nDir].Text = text.Replace(oldText, newText);
                }
            }
        }

        /// <summary>
        /// Builds path.
        /// </summary>
        /// <param name="dirs">Directions.</param>
        /// <returns>Created directions geometry.</returns>
        private static Polyline _BuildPath(Direction[] dirs)
        {
            Debug.Assert(dirs != null);

            var points = new List<Point>();
            foreach (Direction dir in dirs)
            {
                Point[] dirPoints = null;
                if (!CompactGeometryConverter.Convert(dir.Geometry, out dirPoints))
                    throw new RouteException(Properties.Messages.Error_CompactGeometryConversion); // exception

                points.AddRange(dirPoints);
            }

            return new Polyline(points.ToArray());
        }

        /// <summary>
        /// Checks is geometry valid.
        /// </summary>
        /// <param name="stopDirs">Stop directions.</param>
        /// <returns>TRUE if directions has valid geometry.</returns>
        private static bool _IsDirGeometryValid(Direction[] stopDirs)
        {
            Debug.Assert(stopDirs != null);

            bool isValid = true;
            foreach (Direction dir in stopDirs)
            {
                if (string.IsNullOrEmpty(dir.Geometry))
                {
                    isValid = false;
                    break; // result founded
                }
            }

            return isValid;
        }

        /// <summary>
        /// Converts directions maneuver type to according stop maneuver type.
        /// </summary>
        /// <param name="naType">Directions maneuver type to convert.</param>
        /// <returns>Stop maneuver type according with naType.</returns>
        private static StopManeuverType _ConvertManeuverType(NADirectionsManeuverType naType)
        {
            var type = StopManeuverType.Unknown;
            switch (naType)
            {
                case NADirectionsManeuverType.esriDMTBearLeft:
                    type = StopManeuverType.BearLeft;
                    break;
                case NADirectionsManeuverType.esriDMTBearRight:
                    type = StopManeuverType.BearRight;
                    break;
                case NADirectionsManeuverType.esriDMTDepart:
                    type = StopManeuverType.Depart;
                    break;
                case NADirectionsManeuverType.esriDMTEndOfFerry:
                    type = StopManeuverType.EndOfFerry;
                    break;
                case NADirectionsManeuverType.esriDMTFerry:
                    type = StopManeuverType.Ferry;
                    break;
                case NADirectionsManeuverType.esriDMTForkCenter:
                    type = StopManeuverType.ForkCenter;
                    break;
                case NADirectionsManeuverType.esriDMTForkLeft:
                    type = StopManeuverType.ForkLeft;
                    break;
                case NADirectionsManeuverType.esriDMTForkRight:
                    type = StopManeuverType.ForkRight;
                    break;
                case NADirectionsManeuverType.esriDMTHighwayChange:
                    type = StopManeuverType.HighwayChange;
                    break;
                case NADirectionsManeuverType.esriDMTHighwayExit:
                    type = StopManeuverType.HighwayExit;
                    break;
                case NADirectionsManeuverType.esriDMTHighwayMerge:
                    type = StopManeuverType.HighwayMerge;
                    break;
                case NADirectionsManeuverType.esriDMTRoundabout:
                    type = StopManeuverType.Roundabout;
                    break;
                case NADirectionsManeuverType.esriDMTSharpLeft:
                    type = StopManeuverType.SharpLeft;
                    break;
                case NADirectionsManeuverType.esriDMTSharpRight:
                    type = StopManeuverType.SharpRight;
                    break;
                case NADirectionsManeuverType.esriDMTStop:
                    type = StopManeuverType.Stop;
                    break;
                case NADirectionsManeuverType.esriDMTStraight:
                    type = StopManeuverType.Straight;
                    break;
                case NADirectionsManeuverType.esriDMTTripItem:
                    type = StopManeuverType.TripItem;
                    break;
                case NADirectionsManeuverType.esriDMTTurnLeft:
                    type = StopManeuverType.TurnLeft;
                    break;
                case NADirectionsManeuverType.esriDMTTurnRight:
                    type = StopManeuverType.TurnRight;
                    break;
                case NADirectionsManeuverType.esriDMTUTurn:
                    type = StopManeuverType.UTurn;
                    break;
                case NADirectionsManeuverType.esriDMTRampLeft:
                    type = StopManeuverType.RampLeft;
                    break;
                case NADirectionsManeuverType.esriDMTRampRight:
                    type = StopManeuverType.RampRight;
                    break;
            }

            return type;
        }

        #endregion // Private methods
    }
}
