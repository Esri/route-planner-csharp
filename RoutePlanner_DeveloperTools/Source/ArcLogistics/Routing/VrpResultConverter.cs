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
using System.Linq;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Utility;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// StopData class.
    /// </summary>
    internal sealed class StopData
    {
        public StopData()
        {
            this.IsLocked = false;
        }

        public int SequenceNumber { get; set; }
        public double Distance { get; set; }
        public double WaitTime { get; set; }
        public double TimeAtStop { get; set; }
        public double TravelTime { get; set; }
        public DateTime ArriveTime { get; set; }
        public DateTime? TimeWindowStart1 { get; set; }
        public DateTime? TimeWindowEnd1 { get; set; }
        public DateTime? TimeWindowStart2 { get; set; }
        public DateTime? TimeWindowEnd2 { get; set; }
        public DataObject AssociatedObject { get; set; }
        public Guid RouteId { get; set; }
        public GPGeometry Geometry { get; set; }
        public StopType StopType { get; set; }
        public NACurbApproachType NACurbApproach { get; set; }
        public int? OrderSequenceNumber { get; set; }
        public Polyline Path { get; set; }
        public Direction[] Directions { get; set; }
        public bool IsLocked { get; set; }
    }

    /// <summary>
    /// RouteResult class.
    /// </summary>
    internal sealed class RouteResult
    {
        public double Cost { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double Overtime { get; set; }
        public double TotalTime { get; set; }
        public double TotalDistance { get; set; }
        public double TravelTime { get; set; }
        public double ViolationTime { get; set; }
        public double WaitTime { get; set; }
        public bool IsLocked { get; set; }
        public Capacities Capacities { get; set; }
        public Route Route { get; set; }
        public IList<StopData> Stops { get; set; }
    }

    /// <summary>
    /// VrpResultConverter class.
    /// </summary>
    internal sealed class VrpResultConverter
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="project">project.</param>
        /// <param name="schedule">schedule.</param>
        /// <param name="settings">settings.</param>
        public VrpResultConverter(Project project, Schedule schedule,
            SolverSettings settings)
        {
            _project = project;
            _schedule = schedule;
            _settings = settings;
        }

        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public IList<RouteResult> Convert(GPRouteResult gpResult,
            BatchRouteSolveResponse routeResponse,
            SubmitVrpJobRequest request)
        {
            Debug.Assert(gpResult != null);
            Debug.Assert(request != null);

            var directionsFeatures = default(ILookup<Guid, GPFeature>);
            if (gpResult.Directions != null)
            {
                directionsFeatures = gpResult.Directions.Features.ToLookup(feature =>
                    _AttrToObjectId(NAAttribute.ROUTE_NAME, feature.Attributes));
            }

            var hasDirections = routeResponse != null || directionsFeatures != null;

            // route results
            var results = new List<RouteResult>();
            foreach (GPFeature feature in gpResult.Routes.Features)
            {
                // check violation status
                if (!_IsObjectViolated(feature))
                {
                    // route id
                    Guid routeId = _AttrToObjectId(NAAttribute.NAME, feature.Attributes);

                    // find route
                    Route route = DataObjectHelper.FindObjectById<Route>(routeId, _schedule.Routes);

                    if (route == null)
                    {
                        string message = Properties.Messages.Error_InvalidGPFeatureMapping;
                        throw new RouteException(message); // exception
                    }

                    List<StopData> stops = _GetRouteStops(gpResult, route, request);
                    // Get directions for route
                    if (stops.Count != 0 && hasDirections)
                    {
                        var directions = default(IEnumerable<DirectionEx>);

                        if (routeResponse != null)
                        {
                            // Use Routing service directions.
                            var routeDirs = _FindRouteDirections(routeResponse, routeId);
                            if (routeDirs == null || !_ContainsDirFeatures(routeDirs))
                            {   // route has stops, so there must be directions
                                throw _CreateNoDirectionsError(routeId);
                            }

                            directions = routeDirs.Features
                                .Select(DirectionsHelper.ConvertToDirection)
                                .ToList();
                        }
                        else if (directionsFeatures != null)
                        {
                            // Use VRP service directions
                            var directionFeatures = directionsFeatures[routeId];

                            // WORKAROUND: pass first stop geometry as geometry whihc will be used 
                            // in case if directions has no geometry.
                            _FixGeometries(directionFeatures, routeId, stops.First().Geometry as GPPoint);

                            directions = directionFeatures
                                .Select(DirectionsHelper.ConvertToDirection)
                                .ToList();
                        }

                        // We should have either Routing or VRP service directions here.
                        Debug.Assert(directions != null);
                        DirectionsHelper.SetDirections(stops, directions);
                    }

                    // set order sequence values
                    _SetOrderSequence(stops);

                    // add route result
                    results.Add(_CreateRouteResult(feature, route, stops));
                }
            }

            return results;
        }

        public IList<Violation> GetOrderViolations(GPRecordSet layer, int solveHR)
        {
            Debug.Assert(layer != null);

            var violations = new List<Violation>();
            foreach (GPFeature feature in layer.Features)
            {
                int vc;
                if (_GetViolatedConstraint(feature, out vc))
                {
                    // order id
                    Guid orderId = _AttrToObjectId(NAAttribute.NAME, feature.Attributes);

                    // find order
                    Order order = _project.Orders.SearchById(orderId);
                    if (order == null)
                    {
                        string message = Properties.Messages.Error_InvalidGPFeatureMapping;
                        throw new RouteException(message); // exception
                    }

                    // create violation objects
                    violations.AddRange(_CreateViolations(vc, solveHR, order, ORDER_VC));
                }
            }

            return violations;
        }

        public IList<Violation> GetRestrictedOrderViolations(GPRecordSet layer)
        {
            Debug.Assert(layer != null);

            var violations = new List<Violation>();
            foreach (GPFeature feature in layer.Features)
            {
                // status
                var status = (NAObjectStatus)feature.Attributes.Get<int>(NAAttribute.STATUS);
                if (_IsRestrictedOrNotLocated(status))
                {
                    // order id
                    Guid orderId = _AttrToObjectId(NAAttribute.NAME, feature.Attributes);

                    Order order = _project.Orders.SearchById(orderId);
                    if (order == null)
                    {
                        continue;
                    }

                    // create violation object
                    violations.Add(_CreateRestrictedObjViolation(status, order));
                }
            }

            return violations;
        }

        public IList<Violation> GetRouteViolations(GPFeatureRecordSetLayer layer, int solveHR)
        {
            Debug.Assert(layer != null);

            var violations = new List<Violation>();
            foreach (GPFeature feature in layer.Features)
            {
                int vc;
                if (_GetViolatedConstraint(feature, out vc))
                {
                    // route id
                    Guid routeId = _AttrToObjectId(NAAttribute.NAME, feature.Attributes);

                    // find route
                    Route route = DataObjectHelper.FindObjectById<Route>(routeId, _schedule.Routes);
                    if (route == null)
                    {
                        string message = Properties.Messages.Error_InvalidGPFeatureMapping;
                        throw new RouteException(message); // exception
                    }

                    // create violation objects
                    violations.AddRange(_CreateViolations(vc, solveHR, route, ROUTE_VC));
                }
            }

            return violations;
        }

        public IList<Violation> GetDepotViolations(GPRecordSet layer)
        {
            Debug.Assert(layer != null);

            var violations = new List<Violation>();
            foreach (GPFeature feature in layer.Features)
            {
                // status
                var status = (NAObjectStatus)feature.Attributes.Get<int>(NAAttribute.STATUS);

                if (_IsRestrictedOrNotLocated(status))
                {
                    // location id
                    Guid locId = _AttrToObjectId(NAAttribute.NAME, feature.Attributes);

                    // find location
                    Location loc = _project.LocationManager.SearchById(locId);
                    if (loc == null)
                    {
                        continue;
                    }

                    // create violation object
                    violations.Add(_CreateRestrictedObjViolation(status, loc));
                }
            }

            return violations;
        }

        #endregion public methods

        #region private data types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Violated constraint entry.
        /// </summary>
        private sealed class VCEntry
        {
            public NAViolatedConstraint NAType;
            public ViolationType type;
            public string defaultDesc;
            public VCDesc[] hrDesc;

            public VCEntry(NAViolatedConstraint naType,
                           ViolationType type,
                           string defaultDesc,
                           VCDesc[] hrDesc)
            {
                this.NAType = naType;
                this.type = type;
                this.defaultDesc = defaultDesc;
                this.hrDesc = hrDesc;
            }
        }

        /// <summary>
        /// Violated constraint extended info that depends on HRESULT.
        /// </summary>
        private class VCDesc
        {
            public int HR;
            public string desc;
            public ViolationType subType;

            public VCDesc(int hr, string desc, ViolationType subType)
            {
                this.HR = hr;
                this.desc = desc;
                this.subType = subType;
            }
        }

        #endregion private data types

        #region private static methods
        /// <summary>
        /// Creates exception for reporting about missing driving directions for route with
        /// the specified ID.
        /// </summary>
        /// <param name="routeID">The ID of the route without driving directions.</param>
        /// <returns>A new exception object to be used for reporting about missing driving
        /// directions.</returns>
        private static Exception _CreateNoDirectionsError(Guid routeID)
        {
            var message = string.Format(
                Properties.Messages.Error_NoDirectionsForRoute,
                routeID);

            return new RouteException(message);
        }

        /// <summary>
        /// Ensures that all driving directions have corresponding geometry.
        /// </summary>
        /// <param name="drivingDirections">Collection of driving directions to fix geometries
        /// for.</param>
        /// <param name="routeId">Route's id.</param>
        /// <param name="location">Location which will be used 
        /// in case if direction has no geometry.</param>
        private static void _FixGeometries(IEnumerable<GPFeature> drivingDirections, Guid routeId,
            GPPoint location)
        {
            // Check that we have directions.
            if (!drivingDirections.Any())
                throw _CreateNoDirectionsError(routeId);

            // Try to get item with geometry.
            var firstItemWithGeometry = drivingDirections.FirstOrDefault(
                gpFeature => gpFeature.Geometry != null && gpFeature.Geometry.Value != null);

            // Check that we get item with not null geometry.
            // NOTE: in some cases compact geometry is not set, for example when
            // route contains one order which point is the same as start and end
            // locations points. 
            // WORKAROUND: in such cases apply first stop point as each geometry direction.
            if (firstItemWithGeometry == null ||
                firstItemWithGeometry.Geometry == null ||
                firstItemWithGeometry.Geometry.Value == null)
            {
                // Set first direction item geometry, all other will be set in next cycle.
                var item = drivingDirections.First();
                _FillGeometry(item, new double[] { location.X, location.Y });
            }

            var directionsList = new List<GPFeature>(drivingDirections);
            for (int i = 0; i < directionsList.Count; i++)
            {
                var item = directionsList[i];

                // If current item has no geometry.
                if (item.Geometry == null || item.Geometry.Value == null)
                {
                    // If it is first geometry - use next geometry first point to 
                    // create line with zero length as current item geometry.
                    if (i == 0)
                    {
                        var geometry = (GPPolyline)firstItemWithGeometry.Geometry.Value;
                        _FillGeometry(item, geometry.Paths.Last().First());
                    }
                    // In all other case - use previous geometry last point to 
                    // create line with zero length as current item geometry.
                    else
                    {
                        var geometry = (GPPolyline)directionsList[i - 1].Geometry.Value;
                        _FillGeometry(item, geometry.Paths.Last().Last());
                    }
                }
            }
        }

        /// <summary>
        /// Create line which contain of two points with equal coordinates and set it as 
        /// items geometry.
        /// </summary>
        /// <param name="item">GPFeature wich geometry must be set.</param>
        /// <param name="point">Point which will be used for polyline.</param>
        private static void _FillGeometry(GPFeature item, double[] point)
        {
            item.Geometry = new GeometryHolder
            {
                Value = new GPPolyline
                {
                    Paths = new[] { new[] { point, point } },
                },
            };
        }

        private static bool _ContainsDirFeatures(RouteDirections dirs)
        {
            Debug.Assert(dirs != null);
            return (dirs.Features != null && dirs.Features.Length > 0);
        }
        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method gets stops collection from features collection.
        /// </summary>
        /// <param name="gpResult">GP Route result from server.</param>
        /// <param name="route">Route to get route info.</param>
        /// <param name="request">Vrp request.</param>
        /// <returns></returns>
        private List<StopData> _GetRouteStops(
            GPRouteResult gpResult,
            Route route,
            SubmitVrpJobRequest request)
        {
            Debug.Assert(gpResult != null);
            Debug.Assert(route != null);

            var stopsByType = gpResult.Stops.Features
                .ToLookup(feature => feature.Attributes.Get<NAStopType>(NAAttribute.StopType));

            var stops = new List<StopData>();

            // process orders
            stops.AddRange(_GetOrderStops(stopsByType[NAStopType.Order], route));

            // process breaks
            stops.AddRange(_GetBreakStops(stopsByType[NAStopType.Break], route));

            var renewals = request.Renewals == null ?
                Enumerable.Empty<GPFeature>() : request.Renewals.Features;

            // process depots
            stops.AddRange(_GetDepotStops(
                stopsByType[NAStopType.Depot],
                route,
                renewals));

            SolveHelper.SortBySequence(stops);
            if (!stops.Any())
            {
                return stops;
            }

            var isDepot = Functional.MakeLambda(
                (StopData stop) => stop.StopType == StopType.Location);

            var startingDepot = stops.FirstOrDefault(isDepot);
            if (startingDepot != null)
            {
                startingDepot.TimeAtStop = route.TimeAtStart;
            }

            var endingDepot = stops.LastOrDefault(isDepot);
            if (endingDepot != null)
            {
                endingDepot.TimeAtStop = route.TimeAtEnd;
            }

            return stops;
        }

        /// <summary>
        /// Method gets stop data collection from Order features collection.
        /// </summary>
        /// <param name="orderFeatures">Order features collection.</param>
        /// <param name="route">Route.</param>
        /// <returns>Collection of stop data.</returns>
        private List<StopData> _GetOrderStops(IEnumerable<GPFeature> orderFeatures, Route route)
        {
            var stops = new List<StopData>();

            foreach (GPFeature feature in orderFeatures)
            {
                // route id
                Guid id = _AttrToObjectId(NAAttribute.ROUTE_NAME, feature.Attributes);

                if (id == route.Id)
                    stops.Add(_CreateStopFromOrder(feature, route));
            }

            return stops;
        }

        /// <summary>
        /// Method gets stops collection from Breaks features collection for current route.
        /// </summary>
        /// <param name="breaks">Breaks feature collection.</param>
        /// <param name="route">Current route to get breaks.</param>
        /// <returns>Stops collection of Breaks for current route.</returns>
        private List<StopData> _GetBreakStops(IEnumerable<GPFeature> breaks, Route route)
        {
            // Sort breaks from route.
            Breaks breaksOnRoute = (Breaks)route.Breaks.Clone();
            breaksOnRoute.Sort();

            // Get all breaks from route.
            var routeBreaks = breaksOnRoute
                .OfType<Break>()
                .Where(item => item.Duration > 0.0)
                .ToList();

            // Find all break features for current route.
            var breakFeatures = (
                from feature in breaks
                where (route.Id == _AttrToObjectId(NAAttribute.ROUTE_NAME, feature.Attributes))
                select feature)
                .ToList();

            var stops = new List<StopData>();

            if (breakFeatures.Count <= routeBreaks.Count)
            {
                int breakIndex = 0;

                // Create stops from every break features.
                foreach (GPFeature feature in breakFeatures)
                {
                    // Get next break from route.
                    Break routeBreak = routeBreaks[breakIndex++];

                    StopData stopData = null;
                    if (_CreateStopFromBreak(feature, route, routeBreak, out stopData))
                        stops.Add(stopData);
                }
            }
            else
            {
                // Breaks count is incorrect.
                string message = Properties.Messages.Error_InvalidBreakGPFeatureCount;
                throw new RouteException(message);
            }

            return stops;
        }

        /// <summary>
        /// Method searches Depot GP feature by its ID in GP record set layer.
        /// </summary>
        /// <param name="depots">Depots GP record set layer.</param>
        /// <param name="id">Depot id to find.</param>
        /// <returns>Depot GP feature.</returns>
        private GPFeature _FindDepotById(GPFeatureRecordSetLayer depots, Guid id)
        {
            GPFeature res = null;
            foreach (GPFeature feature in depots.Features)
            {
                Guid locId = _AttrToObjectId(NAAttribute.NAME, feature.Attributes);

                if (locId == id)
                {
                    res = feature;
                    break;
                }
            }

            return res;
        }

        /// <summary>
        /// Method gets stops collection for route depots.
        /// </summary>
        /// <param name="depotVisits">Depots GP feature collection.</param>
        /// <param name="route">Route.</param>
        /// <param name="renewals">Renewals GP feature collection.</param>
        /// <returns>Collection of Stop data for depots.</returns>
        private List<StopData> _GetDepotStops(
              IEnumerable<GPFeature> depotVisits,
              Route route,
              IEnumerable<GPFeature> renewals)
        {
            Debug.Assert(depotVisits != null);
            Debug.Assert(route != null);
            Debug.Assert(renewals != null);

            var renewalsByID = renewals
                .GroupBy(feature => _AttrToObjectId(NAAttribute.DEPOT_NAME, feature.Attributes))
                .ToDictionary(group => group.Key, group => group.First());

            var stops = new List<StopData>();
            foreach (GPFeature depotVisitFeature in depotVisits)
            {
                // route id
                Guid depotRouteId = _AttrToObjectId(NAAttribute.ROUTE_NAME,
                    depotVisitFeature.Attributes);

                if (depotRouteId != route.Id)
                {
                    continue;
                }

                var depotID = default(string);
                // if not virtual depot
                if (!depotVisitFeature.Attributes.TryGet(NAAttribute.NAME, out depotID) ||
                    string.IsNullOrEmpty(depotID))
                {
                    continue;
                }

                // create/add stops
                stops.Add(_CreateStopFromDepot(depotVisitFeature, route, renewalsByID));
            }

            return stops;
        }

        /// <summary>
        /// Method creates stop from order GP feature.
        /// </summary>
        /// <param name="feature">Order GP feature</param>
        /// <param name="route">Route.</param>
        /// <returns>Stop data.</returns>
        private StopData _CreateStopFromOrder(GPFeature feature, Route route)
        {
            var stop = new StopData();

            // distance
            stop.Distance = feature.Attributes.Get<double>(NAAttribute.FROM_PREV_DISTANCE);

            // sequence number
            stop.SequenceNumber = feature.Attributes.Get<int>(NAAttribute.SEQUENCE);

            // wait time
            stop.WaitTime = feature.Attributes.Get<double>(NAAttribute.WAIT_TIME, 0.0);

            // service time
            stop.TimeAtStop = feature.Attributes.Get<double>(NAAttribute.SERVICE_TIME, 0.0);

            // travel time
            stop.TravelTime =
                feature.Attributes.Get<double>(NAAttribute.FROM_PREV_TRAVEL_TIME, 0.0);

            // arrive time
            var arriveTime = _Get(feature.Attributes, NAAttribute.ARRIVE_TIME);

            // set stop arrive time = real arrive time + wait time
            stop.ArriveTime = arriveTime.AddMinutes(stop.WaitTime);

            // TW1
            stop.TimeWindowStart1 = _TryGet(feature.Attributes, NAAttribute.TW_START1);
            stop.TimeWindowEnd1 = _TryGet(feature.Attributes, NAAttribute.TW_END1);

            // TW2
            stop.TimeWindowStart2 = _TryGet(feature.Attributes, NAAttribute.TW_START2);
            stop.TimeWindowEnd2 = _TryGet(feature.Attributes, NAAttribute.TW_END2);

            // curbapproach
            stop.NACurbApproach =
                (NACurbApproachType)feature.Attributes.Get<int>(NAAttribute.ArriveCurbApproach);

            // associated object
            Guid assocObjectId = _AttrToObjectId(NAAttribute.NAME, feature.Attributes);

            Order order = _project.Orders.SearchById(assocObjectId);
            if (order == null)
            {
                string message = Properties.Messages.Error_InvalidGPFeatureMapping;
                throw new RouteException(message); // exception
            }

            stop.AssociatedObject = order;

            stop.TimeAtStop = order.ServiceTime;

            var plannedDate = route.Schedule.PlannedDate.Value;
            var timeWindow = order.TimeWindow.ToDateTime(plannedDate);
            stop.TimeWindowStart1 = timeWindow.Item1;
            stop.TimeWindowEnd1 = timeWindow.Item2;

            timeWindow = order.TimeWindow2.ToDateTime(plannedDate);
            stop.TimeWindowStart2 = timeWindow.Item1;
            stop.TimeWindowEnd2 = timeWindow.Item2;

            // route id
            stop.RouteId = _AttrToObjectId(NAAttribute.ROUTE_NAME, feature.Attributes);

            // geometry
            if (order.GeoLocation != null)
            {
                stop.Geometry = GPObjectHelper.PointToGPPoint(order.GeoLocation.Value);
            }

            // stop type
            stop.StopType = StopType.Order;

            return stop;
        }

        /// <summary>
        /// Method creates stop from break GP feature.
        /// </summary>
        /// <param name="feature">Break GP feature</param>
        /// <param name="route">Route.</param>
        /// <param name="routeBreak">Break from route.</param>
        /// <param name="stop">Stop data to fill.</param>
        /// <returns>True - if successfully created, otherwise - False.</returns>
        private bool _CreateStopFromBreak(GPFeature feature, Route route,
            Break routeBreak, out StopData stop)
        {
            Debug.Assert(feature != null);
            Debug.Assert(route != null);
            Debug.Assert(routeBreak != null);

            // Check sequence number.
            int sequenceNumber = 0;

            if (!feature.Attributes.TryGet<int>(NAAttribute.SEQUENCE, out sequenceNumber)
                // Sequence always starts from 1. Otherwise: this is empty break.
                || sequenceNumber == 0)
            {
                stop = null;
                return false;
            }

            stop = new StopData();
            stop.SequenceNumber = sequenceNumber;

            // Distance.
            stop.Distance = feature.Attributes.Get<double>(NAAttribute.FROM_PREV_DISTANCE);

            // Wait time.
            stop.WaitTime = feature.Attributes.Get<double>(NAAttribute.WAIT_TIME, 0.0);

            // Service time.
            stop.TimeAtStop = routeBreak.Duration;

            // Travel time.
            stop.TravelTime =
                feature.Attributes.Get<double>(NAAttribute.FROM_PREV_TRAVEL_TIME, 0.0);

            // Time Windows for break stop.
            var breakTimeWindow = _GetBreakTimeWindow(routeBreak);
            var plannedDate = route.Schedule.PlannedDate.Value;

            var timeWindow = breakTimeWindow.ToDateTime(plannedDate);
            stop.TimeWindowStart1 = timeWindow.Item1;
            stop.TimeWindowEnd1 = timeWindow.Item2;

            // Arrive time.
            var arriveTime = _TryGet(feature.Attributes, NAAttribute.ARRIVE_TIME);

            if (!arriveTime.HasValue)
            {
                arriveTime = stop.TimeWindowStart1;
            }

            arriveTime = arriveTime.AddMinutes(stop.WaitTime);

            stop.ArriveTime = arriveTime.Value;

            // Route id.
            stop.RouteId = _AttrToObjectId(NAAttribute.ROUTE_NAME, feature.Attributes);

            // Geometry:
            // There is no geometry for breaks.

            // Curb approach.
            stop.NACurbApproach = NACurbApproachType.esriNAEitherSideOfVehicle;

            // Stop type.
            stop.StopType = StopType.Lunch;

            // Break stop does not have an associated object.
            stop.AssociatedObject = null;

            return true;
        }

        /// <summary>
        /// Method creates time windows from route break.
        /// </summary>
        /// <param name="routeBreak">Route break to get time information.</param>
        /// <returns>Time Window.</returns>
        private TimeWindow _GetBreakTimeWindow(Break routeBreak)
        {
            TimeWindow breakTimeWindow = new TimeWindow
            {
                IsWideOpen = true
            };

            if (routeBreak is TimeWindowBreak)
            {
                var twBreak = routeBreak as TimeWindowBreak;
                breakTimeWindow.From = twBreak.From;
                breakTimeWindow.To = twBreak.To;
                breakTimeWindow.IsWideOpen = false;
            }
            else
            {
                // Do nothing: any another break types have
                // wideopen time windows.
            }

            return breakTimeWindow;
        }

        /// <summary>
        /// Method creates stop from depot.
        /// </summary>
        /// <param name="depotVisitFeature">Depot GP feature.</param>
        /// <param name="route">Route.</param>
        /// <param name="renewalsByID">Renewals collection associated with its Id.</param>
        /// <returns>Stop data.</returns>
        private StopData _CreateStopFromDepot(
            GPFeature depotVisitFeature,
            Route route,
            IDictionary<Guid, GPFeature> renewalsByID)
        {
            Debug.Assert(depotVisitFeature != null);
            Debug.Assert(route != null);
            Debug.Assert(renewalsByID != null);

            StopData stop = new StopData();

            // Distance.
            stop.Distance = depotVisitFeature.Attributes.Get<double>(NAAttribute.FROM_PREV_DISTANCE);

            // Sequence number.
            stop.SequenceNumber = depotVisitFeature.Attributes.Get<int>(NAAttribute.SEQUENCE);

            // Wait time.
            stop.WaitTime = depotVisitFeature.Attributes.Get<double>(NAAttribute.WAIT_TIME, 0.0);

            // Travel time.
            stop.TravelTime =
                depotVisitFeature.Attributes.Get<double>(NAAttribute.FROM_PREV_TRAVEL_TIME, 0.0);

            // Arrive time.
            stop.ArriveTime = _Get(depotVisitFeature.Attributes, NAAttribute.ARRIVE_TIME);

            // Associated object.
            Guid assocObjectId =
                _AttrToObjectId(NAAttribute.NAME, depotVisitFeature.Attributes);

            Location loc = _GetRouteLocation(route, assocObjectId);
            if (loc == null)
            {
                string message = Properties.Messages.Error_InvalidGPFeatureMapping;
                throw new RouteException(message); // exception
            }

            stop.AssociatedObject = loc;

            var renewalFeature = default(GPFeature);
            if (renewalsByID.TryGetValue(assocObjectId, out renewalFeature))
            {
                stop.TimeAtStop = renewalFeature.Attributes.Get<double>(
                    NAAttribute.SERVICE_TIME,
                    0.0);
            }

            var plannedDate = route.Schedule.PlannedDate.Value;
            var timeWindow = loc.TimeWindow.ToDateTime(plannedDate);
            stop.TimeWindowStart1 = timeWindow.Item1;
            stop.TimeWindowEnd1 = timeWindow.Item2;

            // Curb approach.
            stop.NACurbApproach = CurbApproachConverter.ToNACurbApproach(
                _settings.GetDepotCurbApproach());

            // Route id.
            stop.RouteId = route.Id;

            // Geometry.
            if (loc.GeoLocation != null)
            {
                stop.Geometry = GPObjectHelper.PointToGPPoint(loc.GeoLocation.Value);
            }

            // Stop type.
            stop.StopType = StopType.Location;

            return stop;
        }

        /// <summary>
        /// Method route result.
        /// </summary>
        /// <param name="feature">GP feature.</param>
        /// <param name="route">Route.</param>
        /// <param name="stops">Stop data collection.</param>
        /// <returns>Route result.</returns>
        private RouteResult _CreateRouteResult(GPFeature feature,
                                               Route route,
                                               IList<StopData> stops)
        {
            RouteResult result = new RouteResult();

            // cost
            result.Cost = feature.Attributes.Get<double>(NAAttribute.TOTAL_COST, 0.0);

            // start time
            result.StartTime = _TryGet(feature.Attributes, NAAttribute.START_TIME);

            // end time
            result.EndTime = _TryGet(feature.Attributes, NAAttribute.END_TIME);

            // total time
            result.TotalTime = feature.Attributes.Get<double>(NAAttribute.TOTAL_TIME, 0.0);

            // total distance
            result.TotalDistance = feature.Attributes.Get<double>(NAAttribute.TOTAL_DISTANCE, 0.0);

            // total travel time
            result.TravelTime = feature.Attributes.Get<double>(NAAttribute.TOTAL_TRAVEL_TIME, 0.0);

            // total violation time
            result.ViolationTime =
                feature.Attributes.Get<double>(NAAttribute.TOTAL_VIOLATION_TIME, 0.0);

            // total wait time
            result.WaitTime = feature.Attributes.Get<double>(NAAttribute.TOTAL_WAIT_TIME, 0.0);

            // overtime
            double overtime = result.TotalTime - route.Driver.TimeBeforeOT;
            if (overtime > 0)
                result.Overtime = overtime;

            // capacities
            result.Capacities = _CreateCapacities(stops);

            // path
            // currently we use directions to get route geometry

            // route
            result.Route = route;

            // set stops
            var resStops = new List<StopData>();

            // When a break(s) has preassigned sequence, it could present alone in output
            // stops collection, so in this case there will be no stops at all.
            // Checks in stop sequence present at least one order
            if (stops.Any(stop => stop.StopType == StopType.Order))
                resStops.AddRange(stops);

            result.Stops = resStops;

            return result;
        }

        /// <summary>
        /// Method creates capacities for stop data collection.
        /// </summary>
        /// <param name="stops">Stop data collection.</param>
        /// <returns>Capacities.</returns>
        private Capacities _CreateCapacities(IList<StopData> stops)
        {
            var capacities = new Capacities(_project.CapacitiesInfo);
            foreach (StopData stop in stops)
            {
                if (stop.StopType == StopType.Order)
                {
                    Order order = stop.AssociatedObject as Order;
                    Debug.Assert(order != null);

                    if (order.Type == OrderType.Delivery ||
                        order.Type == OrderType.Pickup)
                    {
                        Debug.Assert(capacities.Count == order.Capacities.Count);
                        for (int cap = 0; cap < order.Capacities.Count; cap++)
                            capacities[cap] += order.Capacities[cap];
                    }
                }
            }

            return capacities;
        }

        /// <summary>
        /// Method gets location of route.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <param name="locId">Location id.</param>
        /// <returns>Start, End or renewal location - if found, otherwise - null.</returns>
         private static Location _GetRouteLocation(Route route, Guid locId)
        {
            Debug.Assert(route != null);

            Location loc = null;

            if (route.StartLocation != null &&
                route.StartLocation.Id == locId)
            {
                loc = route.StartLocation;
            }
            else if (route.EndLocation != null &&
                route.EndLocation.Id == locId)
            {
                loc = route.EndLocation;
            }
            else
            {
                loc = DataObjectHelper.FindObjectById<Location>(locId, route.RenewalLocations);
            }

            return loc;
        }

         /// <summary>
         /// Method set order sequence to stops in stop data collection.
         /// </summary>
         /// <param name="stops">Stop data collection.</param>
        private static void _SetOrderSequence(IList<StopData> stops)
        {
            Debug.Assert(null != stops);

            var sortedStops = new List<StopData>(stops);
            SolveHelper.SortBySequence(sortedStops);

            int orderSeq = 1;
            foreach (StopData stop in sortedStops)
            {
                if (StopType.Order == stop.StopType)
                {
                    stop.OrderSequenceNumber = orderSeq;
                    ++orderSeq;
                }
            }
        }

        /// <summary>
        /// Method gets route directions.
        /// </summary>
        /// <param name="batchRouteResponse">Batch route response.</param>
        /// <param name="routeId">Route id.</param>
        /// <returns>Route directions.</returns>
        private static RouteDirections _FindRouteDirections(
            BatchRouteSolveResponse batchRouteResponse,
            Guid routeId)
        {
            Debug.Assert(batchRouteResponse != null);
            Debug.Assert(batchRouteResponse.Responses != null);

            RouteDirections foundDirs = null;
            foreach (RouteSolveResponse resp in batchRouteResponse.Responses)
            {
                if (resp.Directions != null &&
                    resp.Directions.Length > 0)
                {
                    // we use one request per route, so we get first directions element
                    RouteDirections routeDirs = resp.Directions[0];

                    Guid dirsRouteId = _StringToObjectId(routeDirs.RouteName);
                    if (dirsRouteId == routeId)
                    {
                        foundDirs = routeDirs;
                        break;
                    }
                }
            }

            return foundDirs;
        }

        /// <summary>
        /// Method gets object Id for attribute by its name.
        /// </summary>
        /// <param name="attrName">Attribute name.</param>
        /// <param name="attrs">Collection of attributes.</param>
        /// <returns>Object id.</returns>
        private static Guid _AttrToObjectId(string attrName, AttrDictionary attrs)
        {
            Debug.Assert(attrName != null);
            Debug.Assert(attrs != null);

            return _StringToObjectId(attrs.Get<string>(attrName));
        }

        /// <summary>
        /// Method converts string value to Guid type.
        /// </summary>
        /// <param name="id">String id value.</param>
        /// <returns>Object id as Guid type.</returns>
        private static Guid _StringToObjectId(string id)
        {
            Debug.Assert(id != null);

            Guid res;
            try
            {
                res = new Guid(id);
            }
            catch
            {
                string message = Properties.Messages.Error_AttrToObjectIdConvertFailed;
                throw new RouteException(message); // exception
            }

            return res;
        }

        /// <summary>
        /// Method gets violated constraint from GP feature attributes.
        /// </summary>
        /// <param name="feature">GP feature.</param>
        /// <param name="constraint">Violated constraint as output parameter.</param>
        /// <returns>True if any violated constraint was found, otherwise - false.</returns>
        private static bool _GetViolatedConstraint(GPFeature feature, out int constraint)
        {
            return feature.Attributes.TryGet<int>(NAAttribute.VIOLATED_CONSTRAINTS, out constraint);
        }

        /// <summary>
        /// Method determines is GP feature object violated.
        /// </summary>
        /// <param name="feature">GP feature.</param>
        /// <returns>True if object violated, otherwise - false.</returns>
        private static bool _IsObjectViolated(GPFeature feature)
        {
            int vc;
            return _GetViolatedConstraint(feature, out vc);
        }

        /// <summary>
        /// Method determines is restricted or not located status.
        /// </summary>
        /// <param name="status">Status.</param>
        /// <returns>True if it is restricted or not located, otherwise - false.</returns>
        private static bool _IsRestrictedOrNotLocated(NAObjectStatus status)
        {
            return (status == NAObjectStatus.esriNAObjectStatusNotLocated ||
                    status == NAObjectStatus.esriNAObjectStatusElementNotLocated ||
                    status == NAObjectStatus.esriNAObjectStatusElementNotTraversable);
        }

        /// <summary>
        /// Method creates a collection of violations for collection of Violation Constraint entries.
        /// </summary>
        /// <param name="violatedConstraint">Violated constraint.</param>
        /// <param name="hresult">HResult.</param>
        /// <param name="assocObject">Associated object.</param>
        /// <param name="entries">Collection of Violation Constraint entries.</param>
        /// <returns>Collection of Violations.</returns>
        private static List<Violation> _CreateViolations(int violatedConstraint,
                                                         int hresult,
                                                         DataObject assocObject,
                                                         VCEntry[] entries)
        {
            var list = new List<Violation>();

            foreach (VCEntry entry in entries)
            {
                int naType = (int)entry.NAType;
                if ((violatedConstraint & naType) == naType)
                {
                    Violation violation = new Violation();
                    violation.ViolationType = entry.type;
                    violation.AssociatedObject = assocObject;

                    // Get extended info for HRESULT.
                    foreach (VCDesc hrDesc in entry.hrDesc)
                    {
                        if (hresult == hrDesc.HR)
                        {
                            // Override violation type.
                            violation.ViolationType = hrDesc.subType;
                            break;
                        }
                    }

                    list.Add(violation);
                }
            }

            return list;
        }

        /// <summary>
        /// Method creates violation for restricted object according to its status.
        /// </summary>
        /// <param name="status">Object status.</param>
        /// <param name="assocObject">Associated object.</param>
        /// <returns>Violation.</returns>
        private static Violation _CreateRestrictedObjViolation(NAObjectStatus status,
                                                               DataObject assocObject)
        {
            var violation = new Violation();
            violation.AssociatedObject = assocObject;

            if (status == NAObjectStatus.esriNAObjectStatusNotLocated ||
                status == NAObjectStatus.esriNAObjectStatusElementNotLocated)
            {
                violation.ViolationType = ViolationType.TooFarFromRoad;
            }
            else if (status == NAObjectStatus.esriNAObjectStatusElementNotTraversable)
            {
                violation.ViolationType = ViolationType.RestrictedStreet;
            }
            else
                Debug.Assert(false);

            return violation;
        }

        /// <summary>
        /// Tries to get <see cref="T:System.DateTime"/> value from the attribute
        /// dictionary for the specified key.
        /// </summary>
        /// <param name="dictionary">The reference to the attribute dictionary
        /// to get value from.</param>
        /// <param name="key">The key to get value for.</param>
        /// <returns><see cref="T:System.DateTime"/> value for the specified
        /// key or null if there is no value for the key.</returns>
        private DateTime? _TryGet(AttrDictionary dictionary, string key)
        {
            Debug.Assert(dictionary != null);
            Debug.Assert(key != null);

            var dateTime = dictionary.TryGet<DateTime>(key);
            if (dateTime.HasValue)
            {
                return dateTime;
            }

            // assume it's either ArcgGIS 10.0 server returning milliseconds or
            // there is no any value for this key.
            var milliseconds = dictionary.TryGet<long>(key);
            var ticks = milliseconds * TICKS_IN_MILLISECOND;

            return ticks.Select(value => START_DATE.AddTicks(value));
        }

        /// <summary>
        /// Gets <see cref="T:System.DateTime"/> value from the attribute
        /// dictionary for the specified key.
        /// </summary>
        /// <param name="dictionary">The reference to the attribute dictionary
        /// to get value from.</param>
        /// <param name="key">The key to get value for.</param>
        /// <returns>see cref="T:System.DateTime"/> value for the specified
        /// key.</returns>
        /// <exception cref="ESRI.ArcLogistics.Routing.RouteException">There
        /// is no value for the specified key in the dictionary.</exception>
        private DateTime _Get(AttrDictionary dictionary, string key)
        {
            Debug.Assert(dictionary != null);
            Debug.Assert(key != null);

            var result = _TryGet(dictionary, key);
            if (!result.HasValue)
            {
                string message = Properties.Messages.Error_GetAttributeByKeyFailed;
                throw new RouteException(message); // exception
            }

            return result.Value;
        }
        #endregion private methods

        #region private constants

        /// <summary>
        /// Order violation constraints.
        /// </summary>
        private static readonly VCEntry[] ORDER_VC = new VCEntry[]
        {
            new VCEntry(NAViolatedConstraint.esriNAViolationMaxOrderCount,
                ViolationType.MaxOrderCount,
                Properties.Resources.ViolationMaxOrderCount,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationCapacities,
                ViolationType.Capacities,
                Properties.Resources.ViolationCapacities,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationCapacities_Order_PIR,
                        ViolationType.Capacities)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationMaxTotalTime,
                ViolationType.MaxTotalDuration,
                Properties.Resources.ViolationMaxTotalTime,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationMaxTotalTravelTime,
                ViolationType.MaxTravelDuration,
                Properties.Resources.ViolationMaxTotalTravelTime,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationMaxTotalDistance,
                ViolationType.MaxTotalDistance,
                Properties.Resources.ViolationMaxTotalDistance,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationHardTimeWindow,
                ViolationType.HardTimeWindow,
                Properties.Resources.ViolationHardTimeWindow,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationSpecialties,
                ViolationType.Specialties,
                Properties.Resources.ViolationSpecialties,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationSpecialties_Order_PIR,
                        ViolationType.Specialties)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationZone,
                ViolationType.Zone,
                Properties.Resources.ViolationZone,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationZone_Order_PIR,
                        ViolationType.Zone)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationOrderPairMaxTransitTime,
                ViolationType.OrderPairMaxTransitTime,
                Properties.Resources.ViolationOrderPairMaxTransitTime,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationOrderPairOther,
                ViolationType.OrderPairOther,
                Properties.Resources.ViolationOrderPairOther,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationBreakMaxTravelTime,
                ViolationType.BreakMaxTravelTime,
                string.Empty,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationBreakMaxCumulWorkTime,
                ViolationType.BreakMaxCumulWorkTimeExceeded,
                string.Empty,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationUnreachable,
                ViolationType.Unreachable,
                Properties.Resources.ViolationUnreachable,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationUnreachable_Order_PIR,
                        ViolationType.Unreachable)
                })
        };

        /// <summary>
        /// Route violation constraints.
        /// </summary>
        private static readonly VCEntry[] ROUTE_VC = new VCEntry[]
        {
            new VCEntry(NAViolatedConstraint.esriNAViolationMaxTotalTime,
                ViolationType.MaxTotalDuration,
                Properties.Resources.ViolationMaxTotalTime,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationMaxTotalTime_Route_EIR,
                        ViolationType.EmptyMaxTotalDuration),
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationMaxTotalTime_Route_PIR,
                        ViolationType.MaxTotalDuration)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationMaxTotalTravelTime,
                ViolationType.MaxTravelDuration,
                Properties.Resources.ViolationMaxTotalTravelTime,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationMaxTotalTravelTime_Route_EIR,
                        ViolationType.EmptyMaxTravelDuration),
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationMaxTotalTravelTime_Route_PIR,
                        ViolationType.MaxTravelDuration)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationMaxTotalDistance,
                ViolationType.MaxTotalDistance,
                Properties.Resources.ViolationMaxTotalDistance,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationMaxTotalDistance_Route_EIR,
                        ViolationType.EmptyMaxTotalDistance),
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationMaxTotalDistance_Route_PIR,
                        ViolationType.MaxTotalDistance)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationHardTimeWindow,
                ViolationType.HardTimeWindow,
                Properties.Resources.ViolationHardTimeWindow,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationHardTimeWindow_Route_EIR,
                        ViolationType.EmptyHardTimeWindow),
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationHardTimeWindow_Route_PIR,
                        ViolationType.HardTimeWindow)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationUnreachable,
                ViolationType.Unreachable,
                Properties.Resources.ViolationUnreachable,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationUnreachable_Route_EIR,
                        ViolationType.EmptyUnreachable),
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationUnreachable_Route_PIR,
                        ViolationType.Unreachable)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationMaxOrderCount,
                ViolationType.MaxOrderCount,
                Properties.Resources.ViolationMaxOrderCount,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationMaxOrderCount_Route_PIR,
                        ViolationType.MaxOrderCount)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationCapacities,
                ViolationType.Capacities,
                Properties.Resources.ViolationCapacities,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationCapacities_Route_PIR,
                        ViolationType.Capacities)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationOrderPairMaxTransitTime,
                ViolationType.OrderPairMaxTransitTime,
                Properties.Resources.ViolationOrderPairMaxTransitTime,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationOrderPairMaxTransitTime_Route_PIR,
                        ViolationType.OrderPairMaxTransitTime)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationBreakRequired,
                ViolationType.BreakRequired,
                Properties.Resources.ViolationBreakRequired,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationBreakRequired_Route_PIR,
                        ViolationType.BreakRequired)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationRenewalRequired,
                ViolationType.RenewalRequired,
                Properties.Resources.ViolationRenewalRequired,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        Properties.Resources.ViolationRenewalRequired_Route_PIR,
                        ViolationType.RenewalRequired)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationBreakMaxTravelTime,
                ViolationType.BreakMaxTravelTime,
                string.Empty,
                new VCDesc[]
                {
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES,
                        string.Empty,
                        ViolationType.EmptyBreakMaxTravelTime),
                    new VCDesc((int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES,
                        string.Empty,
                        ViolationType.BreakMaxTravelTime)
                }),

            new VCEntry(NAViolatedConstraint.esriNAViolationBreakMaxCumulWorkTime,
                ViolationType.BreakMaxCumulWorkTimeExceeded,
                string.Empty,
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationZone,
                ViolationType.Zone,
                "",
                new VCDesc[0]),

            new VCEntry(NAViolatedConstraint.esriNAViolationSpecialties,
                ViolationType.Specialties,
                "",
                new VCDesc[0])
        };

        /// <summary>
        /// DateTime values returned by the VRP service are encoded as a number
        /// of milliseconds since this date.
        /// </summary>
        private readonly DateTime START_DATE = new DateTime(1970, 1, 1);

        /// <summary>
        /// The number of ticks in one millisecond.
        /// </summary>
        private readonly long TICKS_IN_MILLISECOND = new TimeSpan(0, 0, 0, 0, 1).Ticks;

        #endregion private constants

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Project.
        /// </summary>
        private Project _project;

        /// <summary>
        /// Schedule.
        /// </summary>
        private Schedule _schedule;

        /// <summary>
        /// Solver settings.
        /// </summary>
        private SolverSettings _settings;

        #endregion private fields
    }
}
