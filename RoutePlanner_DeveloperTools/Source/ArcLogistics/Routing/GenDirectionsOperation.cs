using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// GenDirectionsParams class.
    /// </summary>
    internal class GenDirectionsParams
    {
        /// <summary>
        /// Collection of routes.
        /// </summary>
        public ICollection<Route> Routes { get; internal set; }
    }

    /// <summary>
    /// GenDirectionsOperation class.
    /// Class provides generation of directions operation.
    /// </summary>
    internal sealed class GenDirectionsOperation : ISolveOperation<BatchRouteSolveRequest>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="context">Solver context.</param>
        /// <param name="inputParams">Input parameters.</param>
        public GenDirectionsOperation(SolverContext context, GenDirectionsParams inputParams)
        {
            Debug.Assert(inputParams != null);
            Debug.Assert(inputParams.Routes != null);

            if (inputParams.Routes.Count > 0)
                _schedule = inputParams.Routes.First().Schedule;

            _context = context;
            _inputParams = inputParams;
        }

        #endregion

        #region ISolveOperation<TSolveRequest> Members

        /// <summary>
        /// Schedule.
        /// </summary>
        public Schedule Schedule
        {
            get { return _schedule; }
        }

        /// <summary>
        /// Operation type.
        /// </summary>
        public SolveOperationType OperationType
        {
            get { return SolveOperationType.GenerateDirections; }
        }

        /// <summary>
        /// Input parameters.
        /// </summary>
        public Object InputParams
        {
            get { return _inputParams; }
        }

        /// <summary>
        /// Flag says if can get results without solve operation.
        /// </summary>
        public bool CanGetResultWithoutSolve
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Creates result without solve operation.
        /// </summary>
        /// <returns>Null - not supported.</returns>
        public SolveResult CreateResultWithoutSolve()
        {
            return null;
        }

        /// <summary>
        /// Creates request for solve operation.
        /// </summary>
        /// <returns>BatchRouteSolveRequest.</returns>
        /// <exception cref="RouteException">In case of Invalid routes number, 
        /// routes belong to different Schedules, not set Schedule or Schedule date.
        /// </exception>
        public BatchRouteSolveRequest CreateRequest()
        {
            if (_inputParams.Routes.Count == 0)
                throw new RouteException(Properties.Messages.Error_InvalidRoutesNumber);

            if (_schedule == null)
                throw new RouteException(Properties.Messages.Error_ScheduleNotSet);

            if (_schedule.PlannedDate == null)
                throw new RouteException(Properties.Messages.Error_ScheduleDateNotSet);

            // Check that all routes belong to the same schedule.
            foreach (Route route in _inputParams.Routes)
            {
                if (!route.Schedule.Equals(_schedule))
                    throw new RouteException(Properties.Messages.Error_RoutesBelongToDifSchedules);
            }

            // Get barriers for planned date.
            ICollection<Barrier> barriers = _context.Project.Barriers.Search(
                (DateTime)_schedule.PlannedDate);

            // Convert barriers its features collections.
            GPFeature[] pointBarrierFeats = null;
            GPFeature[] polygonBarrierFeats = null;
            GPFeature[] polylineBarrierFeats = null;
            if (barriers != null && barriers.Count > 0)
            {
                // Get barriers with its types.
                var gpBarriers = barriers.ToLookup(BarriersConverter.DetermineBarrierType);

                // Convert barriers by type.
                pointBarrierFeats = _ConvertBarriers(gpBarriers, BarrierGeometryType.Point,
                    _context.NetworkDescription.NetworkAttributes);
                polygonBarrierFeats = _ConvertBarriers(gpBarriers, BarrierGeometryType.Polygon,
                    _context.NetworkDescription.NetworkAttributes);
                polylineBarrierFeats = _ConvertBarriers(gpBarriers, BarrierGeometryType.Polyline,
                    _context.NetworkDescription.NetworkAttributes);
            }

            // Create request data.
            List<DirRouteData> dirRoutes = _GetDirRoutes(_inputParams.Routes);

            // Build request.
            RouteRequestBuilder builder = new RouteRequestBuilder(
                _context);

            List<RouteSolveRequest> requests = new List<RouteSolveRequest>();
            foreach (DirRouteData dirRoute in dirRoutes)
            {
                RouteSolveRequestData reqData = new RouteSolveRequestData();
                reqData.Route = dirRoute;
                reqData.PointBarriers = pointBarrierFeats;
                reqData.PolygonBarriers = polygonBarrierFeats;
                reqData.PolylineBarriers = polylineBarrierFeats;

                RouteSolveRequest req = builder.BuildRequest(reqData);

                // Check if we have at least 2 stops.
                if (req.Stops.Features.Length > 1)
                    requests.Add(req);
            }

            return new BatchRouteSolveRequest(requests.ToArray());
        }

        /// <summary>
        /// Provides solve operation.
        /// </summary>
        /// <param name="request">Request to solver.</param>
        /// <param name="cancelTracker">Cancel tracker.</param>
        /// <returns>Function returning BatchRouteSolveResponse.</returns>
        /// <exception cref="ESRI.ArcLogistics.Routing.RouteException">If get 
        /// directions operation error occurs.</exception>
        public Func<SolveOperationResult<BatchRouteSolveRequest>> Solve(
            BatchRouteSolveRequest request,
            ICancelTracker cancelTracker)
        {
            Debug.Assert(request != null);
            Debug.Assert(request.Requests != null);

            try
            {
                List<RouteSolveResponse> responses = new List<RouteSolveResponse>();
                foreach (RouteSolveRequest req in request.Requests)
                {
                    responses.Add(_context.RouteService.Solve(req,
                        RouteRequestBuilder.JsonTypes));
                }

                var response = new BatchRouteSolveResponse(responses.ToArray());

                return () => new SolveOperationResult<BatchRouteSolveRequest>
                {
                    SolveResult = _ProcessSolveResult(response),
                    NextStepOperation = null,
                };
            }
            catch (RestException e)
            {
                throw SolveHelper.ConvertServiceException(
                    Properties.Messages.Error_GetDirections, e);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Processing results of solve operation.
        /// </summary>
        /// <param name="batchResponse">Response from solver.</param>
        /// <returns>SolveResult.</returns>
        /// <exception cref="RouteException">In case of invalid GPFeature mapping.
        /// </exception>
        private SolveResult _ProcessSolveResult(BatchRouteSolveResponse batchResponse)
        {
            Debug.Assert(batchResponse != null);
            Debug.Assert(batchResponse.Responses != null);

            List<RouteMessage> messages = new List<RouteMessage>();
            foreach (RouteSolveResponse resp in batchResponse.Responses)
            {
                if (resp.Directions != null &&
                    resp.Directions.Length > 0)
                {
                    // We use one request per route, so we get first directions element.
                    RouteDirections routeDirs = resp.Directions[0];

                    // Route id.
                    Guid routeId = new Guid(routeDirs.RouteName);

                    // Find route.
                    Route route = _FindRoute(routeId);
                    if (route == null)
                        throw new RouteException(Properties.Messages.Error_InvalidGPFeatureMapping);

                    // Set directions.
                    _SetDirections(route.Stops, routeDirs.Features);
                }

                if (resp.Messages != null)
                    messages.AddRange(resp.Messages);
            }

            return _CreateSolveResult(messages.ToArray());
        }

        /// <summary>
        /// Method gets directions from routes collection.
        /// </summary>
        /// <param name="routes">Routes to get directions.</param>
        /// <returns>Collection of directions data.</returns>
        private List<DirRouteData> _GetDirRoutes(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            List<DirRouteData> rtList = new List<DirRouteData>();
            foreach (Route route in routes)
            {
                DirRouteData rt = new DirRouteData();
                rt.RouteId = route.Id;
                rt.StartTime = route.StartTime;
                rt.Stops = _GetStops(route);
                rtList.Add(rt);
            }

            return rtList;
        }

        /// <summary>
        /// Method gets stop point.
        /// </summary>
        /// <param name="stop">Stop.</param>
        /// <returns>Stop point location.</returns>
        private Point _GetStopPoint(Stop stop)
        {
            Debug.Assert(null != stop);
            Debug.Assert(StopType.Lunch != stop.StopType);
            Debug.Assert(null != stop.AssociatedObject);

            var geocodable = stop.AssociatedObject as IGeocodable;
            Debug.Assert(null != geocodable);

            Point? pt = geocodable.GeoLocation;
            Debug.Assert(pt != null);

            return pt.Value;
        }

        /// <summary>
        /// Method gets stops date from route.
        /// </summary>
        /// <param name="route">Route to get information.</param>
        /// <returns>Collection of stops data.</returns>
        private List<StopData> _GetStops(Route route)
        {
            Debug.Assert(route != null);

            IDataObjectCollection<Stop> stops = route.Stops;
            int stopsCount = stops.Count;

            var stopDatas = new List<StopData>(stopsCount);
            for (int index = 0; index < stopsCount; ++index)
            {
                Stop stop = stops[index];

                StopData sd = new StopData();
                sd.SequenceNumber = stop.SequenceNumber;
                sd.Distance = stop.Distance;
                sd.WaitTime = stop.WaitTime;
                sd.TimeAtStop = stop.TimeAtStop;
                sd.TravelTime = stop.TravelTime;
                sd.ArriveTime = (DateTime)stop.ArriveTime;

                if (stop.StopType == StopType.Lunch)
                {
                    // Break.

                    // ToDo - need update logic - now find only first TWBreak.
                    var twBreaks =
                        from currBreak in route.Breaks
                        where
                            currBreak is TimeWindowBreak
                        select currBreak;

                    TimeWindowBreak rtBreak = twBreaks.FirstOrDefault() as TimeWindowBreak;
                    if (rtBreak != null && rtBreak.Duration > 0.0)
                    {
                        // TODO Break

                        //// Time window.
                        //DateTime? twStart = null;
                        //DateTime? twEnd = null;
                        //if (rtBreak.TimeWindow != null)
                        //    _ConvertTW(rtBreak.TimeWindow, out twStart, out twEnd);
                        sd.TimeWindowStart1 = _TSToDate(rtBreak.From);
                        sd.TimeWindowEnd1 = _TSToDate(rtBreak.To);
                    }
                }
                else
                {
                    Debug.Assert(stop.AssociatedObject != null);

                    // Associated object id.
                    sd.AssociatedObject = stop.AssociatedObject;

                    // Geometry.
                    Point pt = _GetStopPoint(stop);
                    sd.Geometry = GPObjectHelper.PointToGPPoint(pt);

                    // Time windows.
                    DateTime? twStart1 = null;
                    DateTime? twStart2 = null;
                    DateTime? twEnd1 = null;
                    DateTime? twEnd2 = null;

                    // Type-specific data.
                    if (stop.StopType == StopType.Order)
                    {
                        Order order = stop.AssociatedObject as Order;
                        Debug.Assert(order != null);

                        // Curbapproach.
                        sd.NACurbApproach = 
                            CurbApproachConverter.ToNACurbApproach(
                            _context.SolverSettings.GetDepotCurbApproach());

                        // Time window 1.
                        if (order.TimeWindow != null)
                            _ConvertTW(order.TimeWindow, out twStart1, out twEnd1);

                        // Time window 2.
                        if (order.TimeWindow2 != null)
                            _ConvertTW(order.TimeWindow2, out twStart2, out twEnd2);
                    }
                    else if (stop.StopType == StopType.Location)
                    {
                        Location loc = stop.AssociatedObject as Location;
                        Debug.Assert(loc != null);

                        // Time window.
                        if (loc.TimeWindow != null)
                            _ConvertTW(loc.TimeWindow, out twStart1, out twEnd1);
                    }

                    sd.TimeWindowStart1 = twStart1;
                    sd.TimeWindowStart2 = twStart2;
                    sd.TimeWindowEnd1 = twEnd1;
                    sd.TimeWindowEnd2 = twEnd2;
                }

                sd.RouteId = route.Id;
                sd.StopType = stop.StopType;
                stopDatas.Add(sd);
            }

            SolveHelper.ConsiderArrivalDelayInStops(
                _context.SolverSettings.ArriveDepartDelay, stopDatas);

            return stopDatas;
        }

        /// <summary>
        /// Method set directions to GP direction features.
        /// </summary>
        /// <param name="stops">Stops collection to get data.</param>
        /// <param name="dirFeatures">Direction feature to set.</param>
        private void _SetDirections(ICollection<Stop> stops, GPCompactGeomFeature[] dirFeatures)
        {
            Debug.Assert(stops != null);

            Dictionary<Stop, StopData> dict = new Dictionary<Stop, StopData>();
            foreach (Stop stop in stops)
                dict.Add(stop, stop.GetData());

            var directions = dirFeatures.Select(DirectionsHelper.ConvertToDirection);
            DirectionsHelper.SetDirections(dict.Values, directions);

            foreach (var entry in dict)
            {
                entry.Key.Path = entry.Value.Path;
                entry.Key.Directions = entry.Value.Directions;
            }
        }

        /// <summary>
        /// Method converts TimeWindow to separate DataTimes for Start and End time ranges.
        /// </summary>
        /// <param name="tw">TimeWindow to convert.</param>
        /// <param name="twStart">Output parameter for Start DateTime.</param>
        /// <param name="twEnd">Output parameter for End DateTime.</param>
        private void _ConvertTW(TimeWindow tw, out DateTime? twStart,
            out DateTime? twEnd)
        {
            Debug.Assert(tw != null);

            twStart = null;
            twEnd = null;

            if (!tw.IsWideOpen)
            {
                twStart = _TSToDate(tw.From);
                twEnd = _TSToDate(tw.To);
                if (twEnd < twStart)
                    // Set next day.
                    twEnd = ((DateTime)twEnd).AddDays(1);
            }
        }

        /// <summary>
        /// Method converts collection of Barriers into GPFeature collection of
        /// BarrierGeometryType type.
        /// </summary>
        /// <param name="barriersByTypes">Typed Barriers collection.</param>
        /// <param name="type">Type to convert.</param>
        /// <param name="attributes">Network attributes used to get barrier attributes.</param>
        /// <returns>GPFeature collection of specified barriers type.</returns>
        private static GPFeature[] _ConvertBarriers(ILookup<BarrierGeometryType, Barrier> barriersByTypes,
            BarrierGeometryType type, IEnumerable<NetworkAttribute> attributes)
        {
            Debug.Assert(barriersByTypes != null);

            // Convert Lookup Table of barriers to enumerable  
            // collection of Barriers of specified type.
            var barriers = barriersByTypes[type];

            GPFeature[] result = null;

            // Convert barriers to GPFeatures.
            BarriersConverter converter = new BarriersConverter(attributes);

            // Convert barrier by its type.
            if (type == BarrierGeometryType.Point)
                result = converter.ConvertToPointBarriersFeatures(barriers);
            else if (type == BarrierGeometryType.Polygon)
                result = converter.ConvertToPolygonBarriersFeatures(barriers);
            else if (type == BarrierGeometryType.Polyline)
                result = converter.ConvertToLineBarriersFeatures(barriers);
            else
                // Not supported type.
                Debug.Assert(false);

            return result;
        }

        /// <summary>
        /// Method converts TimeSpan into DateTime.
        /// </summary>
        /// <param name="ts">TimeSpan to convert.</param>
        /// <returns>DateTime converted object.</returns>
        private DateTime _TSToDate(TimeSpan ts)
        {
            DateTime date = (DateTime)_schedule.PlannedDate;
            return new DateTime(date.Year, date.Month, date.Day,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }

        /// <summary>
        /// Method searches for a route with Route Id in a collection of Input Routes.
        /// </summary>
        /// <param name="routeId">Route Id to find.</param>
        /// <returns>Route object - if found, otherwise - null.</returns>
        private Route _FindRoute(Guid routeId)
        {
            foreach (Route route in _inputParams.Routes)
            {
                if (route.Id == routeId)
                    return route;
            }

            return null;
        }

        /// <summary>
        /// Method creates Solve Result from solver messages.
        /// </summary>
        /// <param name="messages">Route messages.</param>
        /// <returns>Solve result.</returns>
        private static SolveResult _CreateSolveResult(RouteMessage[] messages)
        {
            List<ServerMessage> msgList = new List<ServerMessage>();

            if (messages != null)
            {
                // Convert messages.
                foreach (RouteMessage msg in messages)
                    msgList.Add(_ConvertRouteMessage(msg));
            }

            return new SolveResult(msgList.ToArray(), null, false);
        }

        /// <summary>
        /// Method converts route message from solver to server message.
        /// </summary>
        /// <param name="msg">Route message.</param>
        /// <returns>Server message.</returns>
        private static ServerMessage _ConvertRouteMessage(RouteMessage msg)
        {
            Debug.Assert(msg != null);

            return new ServerMessage(ServerMessageType.Info,
                String.Format(MSG_FORMAT, msg.Type, msg.Description));
        }

        #endregion

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Solver context.
        /// </summary>
        private SolverContext _context;

        /// <summary>
        /// Input parameters.
        /// </summary>
        private GenDirectionsParams _inputParams;

        /// <summary>
        /// Current schedule.
        /// </summary>
        private Schedule _schedule;

        #endregion

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Message format string.
        /// </summary>
        private const string MSG_FORMAT = "code: {0}. {1}";

        #endregion

    }
}
