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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Direction route data keeper.
    /// </summary>
    internal sealed class DirRouteData
    {
        /// <summary>
        /// ID of route.
        /// </summary>
        public Guid RouteId { get; set; }
        /// <summary>
        /// Start time of route.
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Stops.
        /// </summary>
        public IList<StopData> Stops { get; set; }
    }

    /// <summary>
    /// Route solve request data keeper.
    /// </summary>
    internal sealed class RouteSolveRequestData
    {
        /// <summary>
        /// Direction's route data.
        /// </summary>
        public DirRouteData Route { get; set; }
        /// <summary>
        /// Point Barriers features.
        /// </summary>
        public GPFeature[] PointBarriers { get; set; }
        /// <summary>
        /// Polygon barriers features.
        /// </summary>
        public GPFeature[] PolygonBarriers { get; set; }
        /// <summary>
        /// Polyline barriers features.
        /// </summary>
        public GPFeature[] PolylineBarriers { get; set; }
    }

    /// <summary>
    /// Route request builder class.
    /// </summary>
    internal sealed class RouteRequestBuilder
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>RouteRequestBuilder</c> class.
        /// </summary>
        /// <param name="context">Solver context.</param>
        public RouteRequestBuilder(SolverContext context)
        {
            _context = context;
        }

        #endregion Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Json types.
        /// </summary>
        public static IEnumerable<Type> JsonTypes
        {
            get { return jsonTypes; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds request.
        /// </summary>
        /// <param name="reqData">Route solve request data.</param>
        /// <returns>Created route solve request.</returns>
        public RouteSolveRequest BuildRequest(RouteSolveRequestData reqData)
        {
            Debug.Assert(reqData != null);

            var req = new RouteSolveRequest();

            // stops
            req.Stops = _BuildStopFeatures(reqData.Route.Stops);

            // Point barriers.
            if (reqData.PointBarriers != null)
            {
                req.PointBarriers = new RouteRecordSet();
                req.PointBarriers.Features = reqData.PointBarriers;
            }
            // Polygon barriers.
            if (reqData.PolygonBarriers != null)
            {
                req.PolygonBarriers = new RouteRecordSet();
                req.PolygonBarriers.Features = reqData.PolygonBarriers;
            }
            // Polyline barriers.
            if (reqData.PolylineBarriers != null)
            {
                req.PolylineBarriers = new RouteRecordSet();
                req.PolylineBarriers.Features = reqData.PolylineBarriers;
            }

            // start time
            req.StartTime = (reqData.Route.StartTime == null) ?
                                NONE_TIME_VALUE : _FormatEpochTime(reqData.Route.StartTime.Value);

            req.ReturnDirections = true;
            req.ReturnRoutes = false;
            req.ReturnStops = false;
            req.ReturnBarriers = false;
            req.OutSR = GeometryConst.WKID_WGS84;
            req.IgnoreInvalidLocations = _context.SolverSettings.ExcludeRestrictedStreets;
            req.OutputLines = Enum.GetName(typeof(NAOutputLineType),
                NAOutputLineType.esriNAOutputLineTrueShapeWithMeasure);
            req.FindBestSequence = false;
            req.PreserveFirstStop = true;
            req.PreserveLastStop = true;
            req.UseTimeWindows = true;
            req.AccumulateAttributeNames = null;
            req.ImpedanceAttributeName = _context.NetworkDescription.ImpedanceAttributeName;
            req.RestrictionAttributeNames = _FormatRestrictions();
            req.AttributeParameters = _FormatAttrParameters();
            req.RestrictUTurns = _GetUTurnPolicy();
            req.UseHierarchy = true;
            req.DirectionsLanguage = _GetDirLanguage();
            req.OutputGeometryPrecision = null;
            req.OutputGeometryPrecisionUnits = null;
            NANetworkAttributeUnits unit = RequestBuildingHelper.GetDirectionsLengthUnits();
            req.DirectionsLengthUnits = unit.ToString();
            req.DirectionsTimeAttributeName = null;
            req.OutputFormat = NAOutputFormat.JSON;

            return req;
        }

        #endregion // Public methods

        #region Private static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets direction language name for sending to ArcGIS server.
        /// </summary>
        /// <returns>Current direction language name.</returns>
        /// <remarks>ArcGIS expects underscore as languagecode/regioncode delimiter.</remarks>
        private static string _GetDirLanguage()
        {
            return RequestBuildingHelper.GetDirectionsLanguage().Replace('-', '_');
        }

        /// <summary>
        /// Gets effective TimeWindow.
        /// </summary>
        /// <param name="stop">Stop data.</param>
        /// <param name="twStart">Start timewindow (can be null).</param>
        /// <param name="twEnd">End timewindow (can be null).</param>
        /// <returns>TRUE if twStart and twEnd not null.</returns>
        private static bool _GetEffectiveTW(StopData stop,
                                            out DateTime? twStart,
                                            out DateTime? twEnd)
        {
            Debug.Assert(stop != null);

            twStart = null;
            twEnd = null;

            bool hasTW1 = (stop.TimeWindowStart1 != null && stop.TimeWindowEnd1 != null);
            bool hasTW2 = (stop.TimeWindowStart2 != null && stop.TimeWindowEnd2 != null);

            DateTime? start = null;
            DateTime? end = null;

            bool result = false;
            if (stop.StopType == StopType.Order || stop.StopType == StopType.Location)
            {
                // Orders and locations have 2 time windows, need to choose appropriate one.
                if (hasTW1 && hasTW2)
                {
                    // Find and set time window, where time of arrival falls within the range.
                    if (stop.ArriveTime <= (DateTime)stop.TimeWindowEnd1)
                    {
                        start = stop.TimeWindowStart1;
                        end = stop.TimeWindowEnd1;
                    }
                    else
                    {
                        start = stop.TimeWindowStart2;
                        end = stop.TimeWindowEnd2;
                    }
                }
                else
                {
                    // Set one of existed time windows.
                    if (hasTW1)
                    {
                        start = stop.TimeWindowStart1;
                        end = stop.TimeWindowEnd1;
                    }
                    else if (hasTW2)
                    {
                        start = stop.TimeWindowStart2;
                        end = stop.TimeWindowEnd2;
                    }
                }
            }
            else if (stop.StopType == StopType.Lunch)
            {
                if (hasTW1)
                {
                    // Breaks have only one time window.
                    start = stop.TimeWindowStart1;
                    end = stop.TimeWindowEnd1;
                }
            }

            if (start != null && end != null)
            {
                twStart = start;
                twEnd = end;

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Converts the specified date/time into string suitable.
        /// </summary>
        /// <param name="date">Date/time to conversion</param>
        /// <returns></returns>
        /// <remarks>Should be specified as a numeric value representing the
        /// milliseconds since midnight January 1, 1970.</remarks>
        private static string _FormatEpochTime(DateTime date)
        {
            long epoch = (long)(date - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return Convert.ToString(epoch, fmtProvider);
        }

        /// <summary>
        /// Converts the specified date/time into string suitable for sending
        /// to ArcGIS server.
        /// </summary>
        /// <param name="date">The date/time value to be formatted.</param>
        /// <returns>The reference to the string representing <paramref name="date"/>
        /// or null reference if <paramref name="date"/> is null.</returns>
        private static string _FormatStopTime(DateTime? date)
        {
            return (date.HasValue) ? Convert.ToString(date.Value, fmtProvider) : null;
        }

        /// <summary>
        /// Converts network attributes to route attribute parameters.
        /// </summary>
        /// <param name="attrs">Network attributes.</param>
        /// <param name="settings">Solver settings.</param>
        /// <returns>Route attribute parameters.</returns>
        private static RouteAttrParameters _ConvertAttrParameters(
            ICollection<NetworkAttribute> attrs,
            SolverSettings settings)
        {
            Debug.Assert(attrs != null);
            Debug.Assert(settings != null);

            var list = new List<RouteAttrParameter>();
            foreach (NetworkAttribute attr in attrs)
            {
                foreach (NetworkAttributeParameter param in attr.Parameters)
                {
                    object value = null;
                    if (settings.GetNetworkAttributeParameterValue(attr.Name, param.Name, out value))
                    {
                        // skip null value overrides, let the service to use defaults
                        if (value != null)
                        {
                            var p = new RouteAttrParameter();
                            p.AttrName = attr.Name;
                            p.ParamName = param.Name;
                            p.Value = value;
                            list.Add(p);
                        }
                    }
                }
            }

            var res = new RouteAttrParameters();
            res.Parameters = list.ToArray();

            return res;
        }

        #endregion // Private static methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds stop features.
        /// </summary>
        /// <param name="stops">Stops.</param>
        /// <returns>Created route stops recordset for stops.</returns>
        private RouteStopsRecordSet _BuildStopFeatures(IList<StopData> stops)
        {
            Debug.Assert(stops != null);

            // Sort stops respecting sequence.
            var sortedStops = new List<StopData>(stops);
            SolveHelper.SortBySequence(sortedStops);

            Debug.Assert(_context != null);

            SolveHelper.ConsiderArrivalDelayInStops(
                _context.SolverSettings.ArriveDepartDelay, sortedStops);

            // Format impedance attribute name.
            string impedanceAttrName = null;
            if (!string.IsNullOrEmpty(_context.NetworkDescription.ImpedanceAttributeName))
            {
                impedanceAttrName = string.Format(IMPEDANCE_ATTR_FORMAT,
                    _context.NetworkDescription.ImpedanceAttributeName);
            }

            var features = new List<GPFeature>();
            for (int index = 0; index < sortedStops.Count; ++index)
            {
                var feature = new GPFeature();

                // Attributes.
                feature.Attributes = new AttrDictionary();

                StopData sd = sortedStops[index];

                Guid objectId = Guid.Empty;
                if (sd.AssociatedObject != null)
                    objectId = sd.AssociatedObject.Id;

                feature.Attributes.Add(NAAttribute.NAME, objectId.ToString());
                feature.Attributes.Add(NAAttribute.ROUTE_NAME, sd.RouteId.ToString());

                // Effective time window.
                DateTime? twStart = null;
                DateTime? twEnd = null;
                _GetEffectiveTW(sd, out twStart, out twEnd); // NOTE: ignore result
                feature.Attributes.Add(NAAttribute.TW_START, _FormatStopTime(twStart));
                feature.Attributes.Add(NAAttribute.TW_END, _FormatStopTime(twEnd));

                // Service time.
                if (impedanceAttrName != null)
                    feature.Attributes.Add(impedanceAttrName, sd.TimeAtStop);

                var geometry = new GeometryHolder();
                geometry.Value = sd.Geometry;

                if (sd.StopType == StopType.Lunch)
                {
                    var actualStop = SolveHelper.GetActualLunchStop(sortedStops, index);
                    geometry.Value = actualStop.Geometry;
                }

                // Set curb approach.
                var curbApproach = CurbApproachConverter.ToNACurbApproach(
                    _context.SolverSettings.GetOrderCurbApproach());
                if (sd.StopType == StopType.Location)
                {
                    curbApproach = CurbApproachConverter.ToNACurbApproach(
                        _context.SolverSettings.GetDepotCurbApproach());
                }

                feature.Attributes.Add(NAAttribute.CURB_APPROACH, (int)curbApproach);
                feature.Geometry = geometry;

                features.Add(feature);
            }

            var rs = new RouteStopsRecordSet();
            rs.Features = features.ToArray();

            // TODO: will be changed later when support custom AddLocations tool
            rs.DoNotLocateOnRestrictedElements = _context.SolverSettings.ExcludeRestrictedStreets;

            return rs;
        }

        /// <summary>
        /// Converts restrictions to text.
        /// </summary>
        /// <returns>Restrictions string.</returns>
        private string _FormatRestrictions()
        {
            ICollection<string> restrictions = SolveHelper.GetEnabledRestrictionNames(
                _context.SolverSettings.Restrictions);

            string resStr = string.Empty;
            if (restrictions != null && restrictions.Count > 0)
            {
                var list = new List<string>(restrictions);
                resStr = string.Join(RESTRICTIONS_DELIMITER, list.ToArray());
            }

            return resStr;
        }

        /// <summary>
        /// Converts attribute parameters to text.
        /// </summary>
        /// <returns>Attribute parameters string.</returns>
        private string _FormatAttrParameters()
        {
            string result = null;

            RouteAttrParameters parameters = _ConvertAttrParameters(
                _context.NetworkDescription.NetworkAttributes,
                _context.SolverSettings);

            if (parameters.Parameters.Length > 0)
            {
                string value = JsonSerializeHelper.Serialize(parameters);

                Match match = Regex.Match(value, "\\[.+\\]");
                if (!match.Success || string.IsNullOrEmpty(match.Value))
                    throw new RouteException(Properties.Messages.Error_AttrParametersFormat); // exception

                result = match.Value;
            }

            return result;
        }

        /// <summary>
        /// Gets NARoute U-Turn policy.
        /// </summary>
        /// <returns>NARoute U-Turn policy according by UTurn policy from SolverSettings.</returns>
        private string _GetUTurnPolicy()
        {
            UTurnPolicy policy = _context.SolverSettings.GetUTurnPolicy();

            string value = NARouteUTurnPolicy.NoUTurns;
            switch (policy)
            {
                case UTurnPolicy.Nowhere:
                    value = NARouteUTurnPolicy.NoUTurns;
                    break;
                case UTurnPolicy.AtDeadEnds:
                    value = NARouteUTurnPolicy.AllowDeadEndsOnly;
                    break;
                case UTurnPolicy.AtDeadEndsAndIntersections:
                    value = NARouteUTurnPolicy.AllowDeadEndsAndIntersectionsOnly;
                    break;
                default:
                    // Not supported.
                    Debug.Assert(false);
                    break;
            }

            return value;
        }

        #endregion Private methods

        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Restriction list delimiter.
        /// </summary>
        private const string RESTRICTIONS_DELIMITER = ",";

        /// <summary>
        /// Impedance attribute name format.
        /// </summary>
        private const string IMPEDANCE_ATTR_FORMAT = "Attr_{0}";

        /// <summary>
        /// A value to indicate that a start time should not be used,
        /// </summary>
        private const string NONE_TIME_VALUE = "none";

        /// <summary>
        /// Data contract custom types.
        /// </summary>
        private static readonly Type[] jsonTypes = new Type[]
        {
            typeof(GPDate),
            typeof(double[][][])
        };

        /// <summary>
        /// Predifined culture-specific formatting information.
        /// </summary>
        private static readonly CultureInfo fmtProvider = new CultureInfo("en-US");

        #endregion // Constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Solver context.
        /// </summary>
        private SolverContext _context;

        #endregion // Private fields
    }
}
