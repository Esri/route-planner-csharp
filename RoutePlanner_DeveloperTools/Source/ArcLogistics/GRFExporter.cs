/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing;
using System.ComponentModel;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// GRF exporter class
    /// </summary>
    internal static class GrfExporter
    {
        #region constants

        private const string VERSION_ATTR_NAME = "version";
        private const string VERSION_VALUE = "1.3";

        private const string ENABLED_ATTR_NAME = "enabled";
        private const string VISIBLE_ATTR_NAME = "visible";
        private const string CLOSED_ATTR_NAME = "closed";
        private const string TRUE_VALUE = "True";
        private const string FALSE_VALUE = "False";

        private const string X_ATTR_NAME = "x";
        private const string Y_ATTR_NAME = "y";

        private const string GRF_DOC_NODE_NAME = "GRFDOC";

        private const string ROUTEINFO_NODE_NAME = "ROUTE_INFO";
        private const string STOPS_NODE_NAME = "STOPS";
        private const string STOP_NODE_NAME = "STOP";
        private const string LOCATION_NODE_NAME = "LOCATION";
        private const string POINT_NODE_NAME = "POINT";
        private const string TITLE_NODE_NAME = "TITLE";
        private const string COMMENTS_NODE_NAME = "COMMENTS";
        private const string POSITION_NODE_NAME = "POSITION";
        private const string DURATION_NODE_NAME = "DURATION";

        private const string BARRIERS_NODE_NAME = "BARRIERS";
        private const string BARRIER_NODE_NAME = "BARRIER";

        private const string ROUTE_RESULT_NODE_NAME = "ROUTERESULT";
        private const string TOTALS_TEXT_NODE_NAME = "TOTALSTEXT";
        private const string TEXT_NODE_NAME = "TEXT";
        private const string STRING_NODE_NAME = "STRING";
        private const string LENGTH_NODE_NAME = "LENGTH";
        private const string TRAVELTIME_NODE_NAME = "TRAVELTIME";
        private const string DRIVINGTIME_NODE_NAME = "DRIVINGTIME";
        private const string ITEMS_NODE_NAME = "ITEMS";
        private const string ITEM_NODE_NAME = "ITEM";
        private const string PARTID_NODE_NAME = "PARTID";
        private const string ITEMLENGTH_NODE_NAME = "ITEMLENGTH";
        private const string ITEMTIME_NODE_NAME = "ITEMTIME";
        private const string ITEMTYPE_NODE_NAME = "ITEMTYPE";
        private const string MANEUVER_NODE_NAME = "MANEUVERTYPE";
        private const string ITEMTEXT_NODE_NAME = "ITEMTEXT";
        private const string DRIVETEXT_NODE_NAME = "DRIVETEXT";
        private const string LENGTHUNIT_NODE_NAME = "LENGTHUNITS";
        private const string DIRECTIONS_CONTENT_TYPE_NODE_NAME = "DIRECTIONSCONTENTTYPE";
        private const string SHAPE_NODE_NAME = "SHAPE";
        private const string TYPE_NODE_NAME = "TYPE";
        private const string POLYLINE_NODE_NAME = "POLYLINE";
        private const string PATH_NODE_NAME = "PATH";
        private const string COORDS_NODE_NAME = "COORDS";
        private const string ROUTE_SETTINGS_NODE_NAME = "ROUTESETTINGS";
        private const string UTURNPOLICY_NODE_NAME = "BACKTRACKPOLICY";
        private const string IMPEDANCE_ATTR_NODE_NAME = "IMPEDANCEATTRIBUTE";
        private const string RESTRICTIONS_ATTR_NODE_NAME = "RESTRICTIONS";
        private const string RESTRICTION_NODE_NAME = "RESTRICTION";
        private const string ATTRIBUTE_PARAMS_NODE_NAME = "ATTRIBUTEPARAMS";
        private const string ATTRIBUTE_NODE_NAME = "ATTRIBUTE";
        private const string PARAM_NODE_NAME = "PARAM";
        private const string TRIPPLANSETTINGS_NODE_NAME = "TRIPPLANSETTINGS";
        private const string TRIP_START_NODE_NAME = "TRIPSTART";
        private const string DIRECTIONS_CONTENT_NODE_NAME = "DIRECTIONSCONTENT";
        private const string DIRECTIONS_LENGTH_UNITS_NODE_NAME = "DIRECTIONSLENGTHUNITS";

        private const string NAME_ATTR_NAME = "name";
        private const string VALUE_ATTR_NAME = "value";

        private const string UTurnEverywhere = "allow";
        private const string UTurnNowhere = "disable";
        private const string UTurnAtDeadEnds = "deadend";

        private const string TYPE_ATTR_NAME = "type";
        private const string STRICT_ATTR_NAME = "strict";
        private const string STATUS_ATTR_NAME = "status";
        private const string ON_ATTR = "on";
        private const string KILOMETERS = "kilometers";
        private const string MILES = "miles";

        private const int HOURS_PER_DAY = 24;
        private const int MINUTES_PER_HOUR = 60;
        private const double MINIMAL_VALUE = 0.1;
        private const string DOUBLE_FORMAT = "0.000000";

        #endregion

        internal enum esriSMDirectionType
        {
            esriSMDTNewRoad = 1,
            esriSMDTDepart = 7,
            esriSMDTArrive = 8
        };

        #region Public static methods
        
        /// <summary>
        /// Export route to GRF file.
        /// </summary>
        /// <param name="filePath">Path to grf file.</param>
        /// <param name="route">Route to export.</param>
        /// <param name="project">Project, which contains this route.</param>
        /// <param name="geocoder">Geocoder.</param>
        /// <param name="solver">Solver.</param>
        /// <param name="orderPropertiesToExport">Collection of order properties, which must be
        /// exported.</param>
        /// <param name="compress">Flag, shows compress result file or not.</param>
        /// <returns>GrfExportResult which contains messages about exporting.</returns>
        public static GrfExportResult ExportToGRF(string filePath, Route route, Project project,
            IGeocoder geocoder, IVrpSolver solver, ICollection<string> orderPropertiesToExport, bool compress)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = CommonHelpers.XML_SETTINGS_INDENT_CHARS;

            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                var result = ExportToGRF(writer, route, route.Stops, project, geocoder, solver, orderPropertiesToExport, compress);
                writer.Flush();
                return result;
            }
        }

        /// <summary>
        /// Export route to GRF file.
        /// </summary>
        /// <param name="writer">XMLWriter.</param>
        /// <param name="route">Route to export.</param>
        /// <param name="stops">Route's stops.</param>
        /// <param name="project">Project, which contains this route.</param>
        /// <param name="geocoder">Geocoder.</param>
        /// <param name="solver">Solver.</param>
        /// <param name="orderPropertiesToExport">Collection of order properties, which must be
        /// exported.</param>
        /// <param name="compress">Flag, shows compress result file or not.</param>
        /// <returns>GrfExportResult which contains messages about exporting.</returns>
        public static GrfExportResult ExportToGRF(XmlWriter writer, Route route, ICollection<Stop> stops, Project project,
            IGeocoder geocoder, IVrpSolver solver, ICollection<string> orderPropertiesToExport, bool compress)
        {
            GrfExportResult result = new GrfExportResult();

            writer.WriteStartElement(GRF_DOC_NODE_NAME);
            writer.WriteAttributeString(VERSION_ATTR_NAME, VERSION_VALUE);

            writer.WriteStartElement(ROUTEINFO_NODE_NAME);
            if (stops.Count > 0)
            {
                _SaveStops(
                    writer,
                    route,
                    stops,
                    orderPropertiesToExport,
                    project,
                    geocoder.AddressFields);
            }

            IDataObjectCollection<Barrier> barriers =
                (IDataObjectCollection<Barrier>)project.Barriers.Search(route.StartTime.Value.Date, true);
            _SaveBarriers(writer, barriers, result);
            _SaveRouteResults(writer, route, stops);
            writer.WriteEndElement();
            _SaveRouteSettings(writer, route, solver, result);
            writer.WriteEndElement();

            return result;
        }

        #endregion

        #region Private static methods

        private static void _SaveStops(
            XmlWriter writer,
            Route route,
            ICollection<Stop> stops,
            ICollection<string> orderPropertiesToExport,
            Project project,
            AddressField[] addressFields)
        {
            Debug.Assert(route != null);

            var sortedStops = new List<Stop>(stops);
            CommonHelpers.SortBySequence(sortedStops);

            var orderPropertiesTitlesToExport = new List<string>();
            var names = new List<string>(Order.GetPropertyNames(project.CapacitiesInfo,
                                         project.OrderCustomPropertiesInfo, addressFields));
            var titles = new List<string>(Order.GetPropertyTitles(project.CapacitiesInfo,
                                          project.OrderCustomPropertiesInfo, addressFields));

            foreach (string name in orderPropertiesToExport)
            {
                int index = names.IndexOf(name);
                string title = titles[index];
                orderPropertiesTitlesToExport.Add(title);
            }

            var routeStops = CommonHelpers.GetSortedStops(route);

            writer.WriteStartElement(STOPS_NODE_NAME);
            foreach (Stop stop in sortedStops)
            {
                string name = _GetStopName(stop, routeStops);

                var mapLocation = stop.MapLocation;
                if (!mapLocation.HasValue)
                {
                    if (stop.StopType != StopType.Lunch)
                    {
                        throw new InvalidOperationException(
                            Properties.Messages.Error_GrfExporterNoLocationForStop); // exception
                    }

                    var currentIndex = stop.SequenceNumber - 1;
                    var stopWithLocation = SolveHelper.GetActualLunchStop(routeStops, currentIndex);
                    if (!stopWithLocation.MapLocation.HasValue)
                    {
                        throw new InvalidOperationException(
                            Properties.Messages.Error_GrfExporterNoLocationForStop); // exception
                    }

                    mapLocation = stopWithLocation.MapLocation.Value;
                }

                writer.WriteStartElement(STOP_NODE_NAME);
                writer.WriteAttributeString(ENABLED_ATTR_NAME, TRUE_VALUE);

                string comments = _GetComments(stop, orderPropertiesToExport, orderPropertiesTitlesToExport);

                _SaveLocationNode(writer, mapLocation.Value, name, comments);
                writer.WriteStartElement(DURATION_NODE_NAME);
                double duration = stop.TimeAtStop * 60;
                writer.WriteValue(duration.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets name for the specified stop.
        /// </summary>
        /// <param name="stop">The reference to the stop object to get name for.</param>
        /// <param name="routeStops">The collection of route stops sorted by their
        /// sequence numbers for the route containing the <paramref name="stop"/>.</param>
        /// <returns>Name of the specified stop.</returns>
        private static string _GetStopName(Stop stop, IList<Stop> routeStops)
        {
            Debug.Assert(stop != null);
            Debug.Assert(routeStops != null);
            Debug.Assert(routeStops.Contains(stop));

            var order = stop.AssociatedObject as Order;
            if (order != null)
            {
                return order.Name;
            }

            var location = stop.AssociatedObject as Location;
            if (location != null)
            {
                var name = location.Name;
                if (stop.SequenceNumber == 1)
                    name = string.Format(Properties.Resources.StartLocationString, name);
                else if (stop == routeStops.Last())
                    name = string.Format(Properties.Resources.FinishLocationString, name);
                else
                    name = string.Format(Properties.Resources.RenewalLocationString, name);

                return name;
            }

            // in case of a break
            return stop.Name;
        }

        private static void _SaveLocationNode(XmlWriter writer, Point point, string name, string comments)
        {
            writer.WriteStartElement(LOCATION_NODE_NAME);

            writer.WriteAttributeString(VISIBLE_ATTR_NAME, TRUE_VALUE);
            writer.WriteAttributeString(CLOSED_ATTR_NAME, FALSE_VALUE);

            writer.WriteStartElement(POINT_NODE_NAME);
            writer.WriteAttributeString(X_ATTR_NAME, point.X.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
            writer.WriteAttributeString(Y_ATTR_NAME, point.Y.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
            writer.WriteEndElement();

            writer.WriteStartElement(TITLE_NODE_NAME);
            writer.WriteValue(name);
            writer.WriteEndElement();

            if (comments.Length > 0)
            {
                writer.WriteStartElement(COMMENTS_NODE_NAME);
                writer.WriteValue(comments);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static string _GetComments(Stop stop, ICollection<string> orderPropertiesToExport,
            IList<string> orderPropertiesTitlesToExport)
        {
            StringBuilder comments = new StringBuilder();

            Order order = stop.AssociatedObject as Order;
            if (order != null)
            {
                int index = 0;
                foreach (string propertyName in orderPropertiesToExport)
                {
                    string propertyTitle = orderPropertiesTitlesToExport[index];
                    index++;
                    object value = Order.GetPropertyValue(order, propertyName);
                    if (value != null && value.ToString().Length > 0)
                        comments.AppendLine(string.Format("{0}: {1}", propertyTitle, value));
                }
            }

            return comments.ToString();
        }

        /// <summary>
        /// Write point barriers with writer. If there are barriers with other geometry - add
        /// message to 'result'.
        /// </summary>
        /// <param name="writer">XmlWriter.</param>
        /// <param name="barriers">Collection of barriers to write.</param>
        /// <param name="result">GrfExportResult with messages.</param>
        private static void _SaveBarriers(XmlWriter writer, ICollection<Barrier> barriers, 
            GrfExportResult result)
        {

            if (barriers == null)
            {
                return;
            }

            // Detecting barriers with not point geometry and add warning message.
            var barriersWithNotPointGeometry = barriers.Where(barrier => (barrier.Geometry != null 
                && !(barrier.Geometry is ESRI.ArcLogistics.Geometry.Point)) );
            if (barriersWithNotPointGeometry.Any())
            {
                result.Warnings.Add(Properties.Messages.Warning_BarriersNotSupported);
            }

            // If there is no point barriers then finish function execution.
            var barriersWithPointGeometry = barriers.Where(barrier => 
                barrier.Geometry is ESRI.ArcLogistics.Geometry.Point);
            if (!barriersWithPointGeometry.Any())
            {
                return;
            }

            // Select block point barriers.
            // Detected delay point barriers - add warning message.
            // If there is no block point barriers then finish function execution.
            var blockBarriersWithPointGeometry = barriersWithPointGeometry.Where(barrier =>
                barrier.BarrierEffect.BlockTravel);
            if (!blockBarriersWithPointGeometry.Any())
            {
                // Add warning message
                result.Warnings.Add(Properties.Messages.Warning_DelayPointBarriersNotSupported);
                return;
            }

            // Detecting delay point barriers and add warning message.
            if (blockBarriersWithPointGeometry.Count() != barriersWithPointGeometry.Count())
            {
                // Add warning message
                result.Warnings.Add(Properties.Messages.Warning_DelayPointBarriersNotSupported);
            }

            writer.WriteStartElement(BARRIERS_NODE_NAME);
            foreach (Barrier barrier in blockBarriersWithPointGeometry)
            {
                writer.WriteStartElement(BARRIER_NODE_NAME);
                writer.WriteAttributeString(ENABLED_ATTR_NAME, TRUE_VALUE);
                _SaveLocationNode(writer, (Point)barrier.Geometry, barrier.Name, "");
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private static void _SaveRouteResults(XmlWriter writer, Route route, ICollection<Stop> stops)
        {
            bool hasDirections = stops.Any(s => s.Directions != null);
            if (hasDirections)
            {
                writer.WriteStartElement(ROUTE_RESULT_NODE_NAME);

                List<Stop> routeStops = new List<Stop>(stops);
                CommonHelpers.SortBySequence(routeStops);

                double totalTimeInDays = route.TravelTime / (MINUTES_PER_HOUR * HOURS_PER_DAY);
                double serviceTimeSum = 0;
                double drivingTimeInDays = route.TravelTime / (MINUTES_PER_HOUR * HOURS_PER_DAY);
                foreach (Stop stop in routeStops)
                {
                    Order order = stop.AssociatedObject as Order;
                    if (order != null)
                        serviceTimeSum += order.ServiceTime;
                }
                totalTimeInDays += serviceTimeSum / (MINUTES_PER_HOUR * HOURS_PER_DAY);

                string totalsText = _GetTotalsText(route.TotalDistance, drivingTimeInDays);
                writer.WriteStartElement(TOTALS_TEXT_NODE_NAME);
                writer.WriteStartElement(TEXT_NODE_NAME);
                writer.WriteStartElement(STRING_NODE_NAME);
                writer.WriteValue(totalsText);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement(LENGTH_NODE_NAME);
                writer.WriteValue(route.TotalDistance.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
                writer.WriteEndElement();

                writer.WriteStartElement(TRAVELTIME_NODE_NAME);
                writer.WriteValue((totalTimeInDays).ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
                writer.WriteEndElement();

                writer.WriteStartElement(DRIVINGTIME_NODE_NAME);
                writer.WriteValue((drivingTimeInDays).ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
                writer.WriteEndElement();

                if (stops.Count > 0)
                    _SaveItems(writer, stops);

                writer.WriteEndElement();
            }
        }

        private static Direction? _GetNextStopStartDirection(IList<Stop> sortedStops, int startIndex)
        {
            Direction? direction = null;
            int nextIndex = startIndex + 1;
            if (nextIndex < sortedStops.Count)
            {
                for (int index = nextIndex; index < sortedStops.Count; ++index)
                {
                    Stop currentStop = sortedStops[index];
                    Direction[] directions = currentStop.Directions;
                    if ((directions != null) &&
                        (0 < directions.Length))
                    {
                        direction = directions[0];
                        break; // result found
                    }
                }
            }

            return direction;
        }

        private static void _SaveFakeItems(XmlWriter writer, int partID, IList<Stop> sortedStops,
                                           Stop currentStop, bool previouslyStopBreak, int currentIndex)
        {
            Debug.Assert(currentStop.StopType == StopType.Lunch);

            // WORKAROUND: add fake direction for break
            var stopWithLocation = SolveHelper.GetActualLunchStop(sortedStops, currentIndex);
            Debug.Assert(0 < stopWithLocation.Directions.Length);

            int lastDirIndex = stopWithLocation.Directions.Length - 1;
            Direction lastDirection = stopWithLocation.Directions[lastDirIndex];
            string geometry = lastDirection.Geometry;

            // hardcoded text
            string text = Properties.Resources.DomainPropertyNameLunch;

            // add depart direction
            string departText = text;
            string departGeometry = geometry;
            StopManeuverType maneuver = StopManeuverType.Depart;

            // if previously stop not Break
            if (!previouslyStopBreak)
            {
                // if next stop have directions
                Direction? nextDirection = _GetNextStopStartDirection(sortedStops, currentIndex);
                if (nextDirection.HasValue)
                {
                    //  update values from start direction
                    Direction direction = nextDirection.Value;
                    maneuver = direction.ManeuverType;
                    departText = direction.Text;
                    departGeometry = direction.Geometry;
                }
            }

            _SaveStopDirection(writer, partID, 0.0, 0.0, maneuver, departText, departGeometry);

            // add arrive direction
            _SaveStopDirection(writer, partID, 0.0, 0.0, StopManeuverType.Stop, text, geometry);
        }

        private static void _SaveItems(XmlWriter writer, ICollection<Stop> stops)
        {
            int partID = 0;
            writer.WriteStartElement(ITEMS_NODE_NAME);

            List<Stop> routeStops = new List<Stop>(stops);
            CommonHelpers.SortBySequence(routeStops);

            for (int index = 0; index < routeStops.Count; index++)
            {
                Stop stop = routeStops[index];
                bool isEmptyDirections = (stop.Directions == null);
                if (isEmptyDirections &&
                    (0 == index))
                    continue;

                // check previously stop is break
                bool previouslyStopBreak = false;
                int previouslyIndex = index - 1;
                if (0 <= previouslyIndex)
                {
                    Stop previouslyStop = routeStops[previouslyIndex];
                    previouslyStopBreak = (previouslyStop.StopType == StopType.Lunch);
                }

                if (isEmptyDirections)
                {
                    _SaveFakeItems(writer, partID, routeStops, stop, previouslyStopBreak, index);
                }
                else
                {
                    _SaveStop(writer, partID, stop, previouslyStopBreak);
                }

                partID++;
            }
            writer.WriteEndElement();
        }

        private static void _SaveStopDirection(XmlWriter writer, int partID,
                                               double length, double time,
                                               StopManeuverType maneuverType,
                                               string text, string geometry)
        {
            writer.WriteStartElement(ITEM_NODE_NAME);

            writer.WriteStartElement(PARTID_NODE_NAME);
            writer.WriteValue(partID);
            writer.WriteEndElement();

            writer.WriteStartElement(ITEMLENGTH_NODE_NAME);
            writer.WriteValue(length.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
            writer.WriteEndElement();

            writer.WriteStartElement(ITEMTIME_NODE_NAME);
            writer.WriteValue(time.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
            writer.WriteEndElement();

            esriSMDirectionType directionType;
            switch (maneuverType)
            {
                case StopManeuverType.Depart:
                    directionType = esriSMDirectionType.esriSMDTDepart;
                    break;
                case StopManeuverType.Stop:
                    directionType = esriSMDirectionType.esriSMDTArrive;
                    break;
                default:
                    directionType = esriSMDirectionType.esriSMDTNewRoad;
                    break;
            };

            writer.WriteStartElement(ITEMTYPE_NODE_NAME);
            writer.WriteValue((int)directionType);
            writer.WriteEndElement();

            writer.WriteStartElement(ITEMTEXT_NODE_NAME);
            writer.WriteStartElement(TEXT_NODE_NAME);
            writer.WriteStartElement(STRING_NODE_NAME);
            writer.WriteValue(text);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();

            if (length > 0)
            {
                writer.WriteStartElement(DRIVETEXT_NODE_NAME);
                writer.WriteStartElement(STRING_NODE_NAME);

                string lengthText = _GetLengthString(length, true);
                writer.WriteValue(lengthText);

                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteStartElement(MANEUVER_NODE_NAME);
            // inc for compability with SM
            writer.WriteValue((int)maneuverType + 1);
            writer.WriteEndElement();

            _SaveShape(writer, geometry);

            writer.WriteEndElement();
        }

        private static void _SaveStopDirection(XmlWriter writer, int partID, Direction direction,
                                               bool useHardcodeddirection)
        {
            string text = useHardcodeddirection ? Properties.Resources.DomainPropertyNameLunch : direction.Text;
            _SaveStopDirection(writer, partID, direction.Length, direction.Time,
                               direction.ManeuverType, text, direction.Geometry);
        }

        private static void _SaveStop(XmlWriter writer, int partID, Stop stop, bool previouslyStopBreak)
        {
            bool first = true;
            foreach (Direction direction in stop.Directions)
            {
                _SaveStopDirection(writer, partID, direction, first && previouslyStopBreak);
                first = false;
            }
        }

        private static void _SaveShape(XmlWriter writer, string geometry)
        {
            writer.WriteStartElement(SHAPE_NODE_NAME);

            writer.WriteStartElement(TYPE_NODE_NAME);
            writer.WriteValue(1);
            writer.WriteEndElement();

            Point[] points;
            if (CompactGeometryConverter.Convert(geometry, out points))
            {
                writer.WriteStartElement(POLYLINE_NODE_NAME);
                writer.WriteStartElement(PATH_NODE_NAME);
                writer.WriteStartElement(COORDS_NODE_NAME);

                string coords = "";
                foreach (Point point in points)
                {
                    coords += string.Format("{0} {1};", point.X.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo),
                        point.Y.ToString(DOUBLE_FORMAT, NumberFormatInfo.InvariantInfo));
                }
                writer.WriteValue(coords);
                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private static void _SaveRouteSettings(XmlWriter writer, Route route, IVrpSolver solver,
            GrfExportResult result )
        {
            SolverSettings settings = solver.SolverSettings;
            writer.WriteStartElement(ROUTE_SETTINGS_NODE_NAME);

            writer.WriteStartElement(UTURNPOLICY_NODE_NAME);
            string uTurnPolicy = "";
            switch (settings.GetUTurnPolicy())
            {
                case UTurnPolicy.Nowhere:
                    uTurnPolicy = UTurnNowhere;
                    break;
                case UTurnPolicy.AtDeadEnds:
                    uTurnPolicy = UTurnAtDeadEnds;
                    break;
                case UTurnPolicy.AtDeadEndsAndIntersections:
                    // GRF doesnt support "U-Turns at Dead Ends and Intersections" UTurnPolicy,
                    // so replace it with "U-Turns at Dead Ends".
                    //fjk: updated so UTurnEverywhere respected
                    uTurnPolicy = UTurnEverywhere;

                    //fjk: commented out b/c of the above change
                    // Add warning message
                    //result.Warnings.Add(Properties.Messages.Warning_UTurnPolicyNotSupported);
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }
            writer.WriteAttributeString(VALUE_ATTR_NAME, uTurnPolicy);
            writer.WriteEndElement();

            writer.WriteStartElement(IMPEDANCE_ATTR_NODE_NAME);
            writer.WriteAttributeString(NAME_ATTR_NAME, "Time");
            writer.WriteEndElement();

            writer.WriteStartElement(RESTRICTIONS_ATTR_NODE_NAME);

            ICollection<string> restrictions = SolveHelper.GetEnabledRestrictionNames(
                solver.SolverSettings.Restrictions);

            foreach (string name in restrictions)
            {
                writer.WriteStartElement(RESTRICTION_NODE_NAME);
                writer.WriteAttributeString(NAME_ATTR_NAME, name);
                writer.WriteAttributeString(TYPE_ATTR_NAME, STRICT_ATTR_NAME);
                writer.WriteAttributeString(STATUS_ATTR_NAME, ON_ATTR);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            _SaveRouteAttributes(writer, route, solver);

            writer.WriteStartElement(TRIPPLANSETTINGS_NODE_NAME);
            writer.WriteStartElement(TRIP_START_NODE_NAME);
            string startTime = route.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ss");
            writer.WriteAttributeString(VALUE_ATTR_NAME, startTime);
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteStartElement(DIRECTIONS_LENGTH_UNITS_NODE_NAME);
            string lengthUnits;
            RegionInfo ri = new RegionInfo(System.Threading.Thread.CurrentThread.CurrentCulture.LCID);
            if (ri.IsMetric)
                lengthUnits = KILOMETERS;
            else
                lengthUnits = MILES;
            writer.WriteAttributeString(VALUE_ATTR_NAME, lengthUnits);
            writer.WriteEndElement();

            writer.WriteStartElement(DIRECTIONS_CONTENT_NODE_NAME);
            writer.WriteAttributeString(VALUE_ATTR_NAME, "all");
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        /// <summary>
        /// Convert value to string.
        /// </summary>
        /// <param name="value">Value to conversion.</param>
        /// <returns>Converted string value or empty string.</returns>
        private static string _ConvertValue2String(object value)
        {
            // NOTE: empty values null) - set as string.Empty
            return (null == value) ? string.Empty :
                                     (string)Convert.ChangeType(value, typeof(string));
        }

        private static void _SaveRouteAttributes(XmlWriter writer, Route route, IVrpSolver solver)
        {
            bool attributesContainsParams = false;
            Debug.Assert(solver.NetworkDescription != null);
            foreach (NetworkAttribute attribute in solver.NetworkDescription.NetworkAttributes)
            {
                foreach (NetworkAttributeParameter parameter in attribute.Parameters)
                {
                    attributesContainsParams = true;
                    break;
                }
                if (attributesContainsParams)
                    break;
            }

            if (attributesContainsParams)
            {
                SolverSettings solverSettings = solver.SolverSettings;

                writer.WriteStartElement(ATTRIBUTE_PARAMS_NODE_NAME);
                foreach (NetworkAttribute attribute in solver.NetworkDescription.NetworkAttributes)
                {
                    bool containsParams = false;
                    foreach (NetworkAttributeParameter parameter in attribute.Parameters)
                    {
                        containsParams = true;
                        break;
                    }

                    if (containsParams)
                    {
                        writer.WriteStartElement(ATTRIBUTE_NODE_NAME);
                        writer.WriteAttributeString(NAME_ATTR_NAME, attribute.Name);
                        foreach (NetworkAttributeParameter parameter in attribute.Parameters)
                        {
                            writer.WriteStartElement(PARAM_NODE_NAME);
                            writer.WriteAttributeString(NAME_ATTR_NAME, parameter.Name);

                            object valueObj = null;
                            if (!solverSettings.GetNetworkAttributeParameterValue(attribute.Name,
                                                                                  parameter.Name,
                                                                                  out valueObj))
                                valueObj = null;

                            string value = _ConvertValue2String(valueObj);

                            writer.WriteAttributeString(VALUE_ATTR_NAME, value);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndElement();
            }
        }

        private static string _GetLengthString(double lengthInMiles, bool addDriveWord)
        {
            StringBuilder sb = new StringBuilder();
            if (addDriveWord)
                sb.Append(Properties.Resources.DirectionsWordDrive);
            double length = 0;
            string distanceMeasuringUnitName = null;
            if (RegionInfo.CurrentRegion.IsMetric)
            {
                length = lengthInMiles * SolverConst.KM_PER_MILE;
                distanceMeasuringUnitName = Properties.Resources.DistanceInKilometresText;
            }
            else
            {
                length = lengthInMiles;
                distanceMeasuringUnitName = Properties.Resources.DistanceInMilesText;
            }

            if (length < MINIMAL_VALUE)
            {
                sb.Append(" <");
                length = MINIMAL_VALUE;
            }

            sb.AppendFormat(" {0} {1}", length.ToString("0.0", NumberFormatInfo.InvariantInfo), distanceMeasuringUnitName);
            return sb.ToString();
        }

        private static string _GetTotalsText(double totalDistance, double drivingTime)
        {
            string totalLengthStr = _GetLengthString(totalDistance, false);

            string totalDriveTime = _GetTimeString(drivingTime);

            string totals = string.Format(Properties.Resources.GRFTotalStringFormat, totalLengthStr, totalDriveTime);
            return totals;
        }

        private static string _GetTimeString(double dTime) // exception
        {
            string timeString;
            string hoursTimeString = "";
            int hours = (int)(dTime * HOURS_PER_DAY);
            int minutes = (int)(dTime * HOURS_PER_DAY * MINUTES_PER_HOUR - hours * HOURS_PER_DAY);
            if (minutes >= 1)
            {
                if (hours != 0)
                {
                    hoursTimeString = string.Format("{0} {1}(s)", hours, Properties.Resources.Hour);
                    if (minutes != 0)
                        hoursTimeString += " ";
                }

                if (minutes != 0)
                {
                    timeString = hoursTimeString + string.Format("{0} {1}(s)", minutes, Properties.Resources.Minute);
                }
                else
                    timeString = hoursTimeString;
            }
            else
                timeString = string.Format("< {0} {1}", MINIMAL_VALUE, Properties.Resources.Minute);

            return timeString;
        }
        #endregion
    }
}
