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
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// SolveRequestData class.
    /// </summary>
    internal class SolveRequestData
    {
        /// <summary>
        /// Collection of orders for solve request.
        /// </summary>
        public ICollection<Order> Orders { get; set; }

        /// <summary>
        /// Collection of routes for solve request.
        /// </summary>
        public ICollection<Route> Routes { get; set; }

        /// <summary>
        /// Collection of barriers for solve request.
        /// </summary>
        public ICollection<Barrier> Barriers { get; set; }
    }

    /// <summary>
    /// RequestOptions class.
    /// </summary>
    internal class SolveRequestOptions
    {
        /// <summary>
        /// Unassigned orders for solve request.
        /// </summary>
        public bool ConvertUnassignedOrders { get; set; }

        /// <summary>
        /// Shape for solve request.
        /// </summary>
        public bool PopulateRouteLines { get; set; }
    }

    /// <summary>
    /// VrpRequestBuilder class.
    /// </summary>
    internal class VrpRequestBuilder
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The reference to the solver context object.</param>
        public VrpRequestBuilder(SolverContext context)
        {
            _context = context;
        }

        #endregion

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Property contain available types for JSON.
        /// </summary>
        public static IEnumerable<Type> JsonTypes
        {
            get { return jsonTypes; }
        }

        #endregion

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method collect information from solve request data for planned date and build request
        /// object.
        /// </summary>
        /// <param name="schedule">Current schedule.</param>
        /// <param name="reqData">Request data to get information.</param>
        /// <param name="options">Request options.</param>
        /// <param name="solveOptions">The reference to to the solve options object.</param>
        /// <returns>Request object with filled information for request.</returns>
        /// <exception cref="RouteException">If Fuel Economy in some of routes is 0.0,
        /// If unassigned order or location is not geocoded.</exception>
        public SubmitVrpJobRequest BuildRequest(Schedule schedule, SolveRequestData reqData,
            SolveRequestOptions options, SolveOptions solveOptions)
        {
            Debug.Assert(schedule != null);
            Debug.Assert(reqData != null);
            Debug.Assert(options != null);
            Debug.Assert(solveOptions != null);

            _plannedDate = (DateTime)schedule.PlannedDate;
            _orderRevenue = Math.Min(_CalcMaxRoutesCost(reqData.Routes),
                MAX_REVENUE);

            _BuildDepotsColl(reqData.Routes);

            SubmitVrpJobRequest req = new SubmitVrpJobRequest();

            // Get depots.
            req.Depots = _ConvertDepots();

            // Get orders.
            req.Orders = _ConvertOrders(reqData.Orders, reqData.Routes,
                options.ConvertUnassignedOrders);

            // Get order pairs.
            req.OrderPairs = _ConvertOrderPairs(reqData.Orders);

            // Get routes.
            req.Routes = _ConvertRoutes(reqData.Routes);

            // Get route zones.
            var zoneInfo = _ConvertZones(reqData.Routes);
            req.RouteZones = zoneInfo.Zones;
            req.SpatiallyClusterRoutes = zoneInfo.UseSpatialClustering;

            // Get renewals.
            req.Renewals = _ConvertRenewals(reqData.Routes);

            // Get breaks.
            req.Breaks = ConvertBreaks(reqData.Routes);

            // Get barriers of all types.
            var typedBarriers = reqData.Barriers.ToLookup(BarriersConverter.DetermineBarrierType);
            req.PointBarriers = _ConvertBarriers(typedBarriers, BarrierGeometryType.Point,
                _context.NetworkDescription.NetworkAttributes);
            req.PolygonBarriers = _ConvertBarriers(typedBarriers, BarrierGeometryType.Polygon,
                _context.NetworkDescription.NetworkAttributes);
            req.LineBarriers = _ConvertBarriers(typedBarriers, BarrierGeometryType.Polyline,
                _context.NetworkDescription.NetworkAttributes);

            // Get network attribute parameters.
            req.NetworkParams = _ConvertNetworkParams();

            // Get analysis region.
            req.AnalysisRegion = _context.RegionName;

            // Get restrictions.
            req.Restrictions = _FormatRestrictions();

            // Get u-turn policy.
            req.UTurnPolicy = _GetUTurnPolicy();

            req.Date = GPObjectHelper.DateTimeToGPDateTime(_plannedDate);
            req.UseHierarchyInAnalysis = true;
            req.PopulateRouteLines = false;
            req.EnvOutSR = solverSR.WKID;
            req.EnvProcessSR = solverSR.WKID;
            req.TWPreference = _context.SolverSettings.TWPreference;
            req.ExcludeRestrictedStreets = _context.SolverSettings.ExcludeRestrictedStreets;
            req.OutputFormat = NAOutputFormat.JSON;

            req.DirectionsLanguage = RequestBuildingHelper.GetDirectionsLanguage();
            req.PopulateDirections = options.PopulateRouteLines;
            req.SaveOutputLayer = _context.SolverSettings.SaveOutputLayer;

            req.ReturnM = true;

            return req;
        }

        #endregion

        #region Protected data types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Class describe information about assigned order.
        /// </summary>
        protected class AssignedOrder
        {
            /// <summary>
            /// Assigned order.
            /// </summary>
            public Order Order { get; set; }

            /// <summary>
            /// Route on which the order assigned.
            /// </summary>
            public Route Route { get; set; }

            /// <summary>
            /// Stop, appropriate for order.
            /// </summary>
            public Stop Stop { get; set; }

            /// <summary>
            /// Stop sequence number on route.
            /// </summary>
            public int SequenceNumber { get; set; }
        }

        #endregion

        #region Protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current solver context property.
        /// </summary>
        protected SolverContext SolverContext
        {
            get { return _context; }
        }

        #endregion

        #region Protected overridable methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method set assigned order attributes.
        /// </summary>
        /// <param name="attrs">Attributes to set.</param>
        /// <param name="assignedOrder">Assigned order to get attribute values.</param>
        protected virtual void SetOrderAssignment(AttrDictionary attrs, AssignedOrder assignedOrder)
        {
            Debug.Assert(attrs != null);

            // Route name.
            attrs.Set(NAAttribute.ROUTE_NAME, assignedOrder.Route.Id.ToString());
            // Sequence.
            attrs.Set(NAAttribute.SEQUENCE, assignedOrder.SequenceNumber);
            // Assignment rule.
            attrs.Set(NAAttribute.ASSIGNMENT_RULE,
                (int)NAOrderAssignmentRule.esriNAOrderPreserveRouteAndRelativeSequence);
        }

        /// <summary>
        /// Method set unassigned order attributes.
        /// </summary>
        /// <param name="attrs">Attributes to set.</param>
        /// <param name="unassignedOrder">Unassigned order to get attribute values.</param>
        protected virtual void SetOrderAssignment(AttrDictionary attrs, Order unassignedOrder)
        {
            Debug.Assert(attrs != null);

            // Route name.
            attrs.Set(NAAttribute.ROUTE_NAME, "");
            // Sequence.
            attrs.Set(NAAttribute.SEQUENCE, null);
            // Assignment rule.
            attrs.Set(NAAttribute.ASSIGNMENT_RULE,
                (int)NAOrderAssignmentRule.esriNAOrderOverride);
        }

        /// <summary>
        /// Method gets specialties Ids from route.
        /// </summary>
        /// <param name="route">Route to get specialties.</param>
        /// <returns>Collection of specialties Ids from route.</returns>
        protected virtual List<Guid> GetRouteSpecIds(Route route)
        {
            Debug.Assert(route != null);

            List<Guid> specs = new List<Guid>();

            foreach (DriverSpecialty spec in route.Driver.Specialties)
                specs.Add(spec.Id);

            foreach (VehicleSpecialty spec in route.Vehicle.Specialties)
                specs.Add(spec.Id);

            return specs;
        }

        /// <summary>
        /// Method gets specialties Ids from order.
        /// </summary>
        /// <param name="order">Order to get specialties.</param>
        /// <returns>Collection of specialties Ids from order.</returns>
        protected virtual List<Guid> GetOrderSpecIds(Order order)
        {
            Debug.Assert(order != null);

            List<Guid> specs = new List<Guid>();

            foreach (DriverSpecialty spec in order.DriverSpecialties)
                specs.Add(spec.Id);

            foreach (VehicleSpecialty spec in order.VehicleSpecialties)
                specs.Add(spec.Id);

            return specs;
        }

        /// <summary>
        /// Method converts breaks from routes to GPRecordSet.
        /// </summary>
        /// <param name="routes">Routes collection to get breaks.</param>
        /// <returns>Breaks GPRecordSet.</returns>
        protected virtual GPRecordSet ConvertBreaks(ICollection<Route> routes)
        {
            var converter = CreateBreaksConverter();

            // While routing we don't need breaks "sequence" attribute.
            return converter.ConvertBreaks(routes, false);
        }

        #endregion

        #region Protected

        /// <summary>
        /// Create class for converting breaks into GPRecordSet.
        /// </summary>
        /// <returns>BreaksConverter.</returns>
        protected BreaksConverter CreateBreaksConverter()
        {
            return new BreaksConverter(_plannedDate, _context.Project.BreaksSettings);
        }

        #endregion

        #region Private data types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Zones types available for assign to routes.
        /// </summary>
        private enum RouteZoneType
        {
            /// <summary>
            /// None.
            /// </summary>
            None,
            /// <summary>
            /// Point zone.
            /// </summary>
            Point,
            /// <summary>
            /// Polygon zone.
            /// </summary>
            Polygon,
            /// <summary>
            /// Mixed case.
            /// </summary>
            Mixed
        }

        /// <summary>
        /// Provides access to route zone usage settings.
        /// </summary>
        private class ZoneInfo
        {
            /// <summary>
            /// Initializes a new instance of the ZoneInfo class.
            /// </summary>
            /// <param name="useSpatialClustering">The value indicating if spatial clustering
            /// should be used.</param>
            public ZoneInfo(bool useSpatialClustering)
                : this(useSpatialClustering, null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the ZoneInfo class.
            /// </summary>
            /// <param name="zones">The reference to route zones.</param>
            public ZoneInfo(GPFeatureRecordSetLayer zones)
                : this(false, zones)
            {
            }

            /// <summary>
            /// Initializes a new instance of the ZoneInfo class.
            /// </summary>
            /// <param name="useSpatialClustering">The value indicating if spatial clustering
            /// should be used.</param>
            /// <param name="zones">The reference to route zones.</param>
            private ZoneInfo(bool useSpatialClustering, GPFeatureRecordSetLayer zones)
            {
                this.UseSpatialClustering = useSpatialClustering;
                this.Zones = zones;
            }

            /// <summary>
            /// Gets or sets a value indicating if spatial clustering should be used instead of
            /// route zones.
            /// </summary>
            public bool UseSpatialClustering { get; private set; }

            /// <summary>
            /// Gets or sets a reference to route zones.
            /// </summary>
            public GPFeatureRecordSetLayer Zones { get; private set; }
        }

        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method converts renewal locations from routes collection into GPRecordSet.
        /// </summary>
        /// <param name="routes">Routes collection to get renewals.</param>
        /// <returns>Renewals GPRecordSet.</returns>
        private GPRecordSet _ConvertRenewals(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            List<GPFeature> features = new List<GPFeature>();
            foreach (Route route in routes)
            {
                foreach (Location loc in route.RenewalLocations)
                    features.Add(_ConvertRenewal(loc, route));
            }

            GPRecordSet rs = null;
            if (features.Count > 0)
            {
                rs = new GPRecordSet();
                rs.Features = features.ToArray();
            }

            return rs;
        }

        /// <summary>
        /// Method converts renewal location from route into GPFeature.
        /// </summary>
        /// <param name="loc">Location to convert.</param>
        /// <param name="route">Route on which location assigned.</param>
        /// <returns>Renewals GPFeature.</returns>
        private GPFeature _ConvertRenewal(Location loc, Route route)
        {
            Debug.Assert(loc != null);
            Debug.Assert(route != null);

            GPFeature feature = new GPFeature();

            // Attributes.
            AttrDictionary attrs = new AttrDictionary();

            // Route name.
            attrs.Add(NAAttribute.ROUTE_NAME, route.Id.ToString());
            // Depot name.
            attrs.Add(NAAttribute.DEPOT_NAME, loc.Id.ToString());
            // Service time.
            attrs.Add(NAAttribute.SERVICE_TIME, route.TimeAtRenewal);

            // Sequences: currently sequences are not used.

            feature.Attributes = attrs;

            return feature;
        }

        /// <summary>
        /// Method converts routes collection into GPRecordSet.
        /// </summary>
        /// <param name="routes">Routes collection.</param>
        /// <returns>Routes GPRecordSet.</returns>
        /// <exception cref="RouteException">If Fuel Economy in routes is 0.0.</exception>
        private GPRecordSet _ConvertRoutes(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            List<GPFeature> features = new List<GPFeature>();
            foreach (Route route in routes)
                features.Add(_ConvertRoute(route));

            GPRecordSet rs = new GPRecordSet();
            rs.Features = features.ToArray();

            return rs;
        }

        /// <summary>
        /// Method converts route into GPFeature.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <returns>Routes GPFeature.</returns>
        /// <exception cref="RouteException">If Fuel Economy in routes is 0.0.</exception>
        private GPFeature _ConvertRoute(Route route)
        {
            Debug.Assert(route != null);

            // Attributes.
            AttrDictionary attrs = new AttrDictionary();

            // Name.
            attrs.Add(NAAttribute.NAME, route.Id.ToString());

            // Start depot.
            attrs.Add(NAAttribute.START_DEPOT_NAME,
                route.StartLocation == null ? null : route.StartLocation.Id.ToString());

            // End depot.
            attrs.Add(NAAttribute.END_DEPOT_NAME,
                route.EndLocation == null ? null : route.EndLocation.Id.ToString());

            // Depot service times.
            attrs.Add(NAAttribute.START_DEPOT_SERVICE_TIME,
                route.StartLocation == null ? null : (object)route.TimeAtStart);
            attrs.Add(NAAttribute.END_DEPOT_SERVICE_TIME,
                route.EndLocation == null ? null : (object)route.TimeAtEnd);

            // Grace period.
            _SetTimeWindowAttribute(route.StartTimeWindow,
                NAAttribute.EARLIEST_START_TIME,
                NAAttribute.LATEST_START_TIME,
                attrs);

            // Capacities.
            attrs.Add(NAAttribute.CAPACITIES,
                _FormatCapacities(route.Vehicle.Capacities));

            // Costs.
            attrs.Add(NAAttribute.FIXED_COST, route.Driver.FixedCost + route.Vehicle.FixedCost);
            attrs.Add(NAAttribute.COST_PER_UNIT_TIME, route.Driver.PerHourSalary / 60);
            attrs.Add(NAAttribute.COST_PER_UNIT_OVERTIME, route.Driver.PerHourOTSalary / 60);
            attrs.Add(NAAttribute.OVERTIME_START_TIME, route.Driver.TimeBeforeOT);

            if (route.Vehicle.FuelEconomy == 0.0)
                throw new RouteException(Properties.Messages.Error_InvalidFuelConsumptionValue);

            attrs.Add(NAAttribute.COST_PER_UNIT_DISTANCE,
                route.Vehicle.FuelType.Price / route.Vehicle.FuelEconomy);

            // Order constraints.
            attrs.Add(NAAttribute.MAX_ORDERS, route.MaxOrders);

            // Max drive duration.
            attrs.Add(NAAttribute.MAX_TOTAL_TRAVEL_TIME,
                route.MaxTravelDuration == 0.0 ? null : (object)route.MaxTravelDuration);

            // Max total duration.
            attrs.Add(NAAttribute.MAX_TOTAL_TIME,
                route.MaxTotalDuration == 0.0 ? null : (object)route.MaxTotalDuration);

            // Max drive distance.
            attrs.Add(NAAttribute.MAX_TOTAL_DISTANCE,
                route.MaxTravelDistance == 0.0 ? null : (object)route.MaxTravelDistance);

            // Arrive and depart delay.
            attrs.Add(NAAttribute.ARRIVE_DEPART_DELAY,
                _context.SolverSettings.ArriveDepartDelay);

            // Specialties.
            List<Guid> specIds = GetRouteSpecIds(route);
            if (specIds.Count > 0)
                attrs.Add(NAAttribute.SPECIALTY_NAMES, _FormatSpecList(specIds));

            // assignment rule.
            attrs.Add(NAAttribute.ASSIGNMENT_RULE,
                (int)NARouteAssignmentRule.esriNARouteIncludeInSolve);

            // TODO: dynamic point zones.

            GPFeature feature = new GPFeature();
            feature.Attributes = attrs;

            return feature;
        }

        /// <summary>
        /// Method converts zones from routes collection into GPFeatureRecordSetLayer.
        /// </summary>
        /// <param name="routes">Routes collection to get zones.</param>
        /// <returns>Zones information object with route zones settings.</returns>
        private ZoneInfo _ConvertZones(ICollection<Route> routes)
        {
            RouteZoneType type = _GetRoutesZonesType(routes);
            if (type == RouteZoneType.None && _context.SolverSettings.UseDynamicPoints)
            {
                return new ZoneInfo(useSpatialClustering: true);
            }

            List<GPFeature> zoneFeatures = new List<GPFeature>();
            List<GPFeature> seedPointFeatures = new List<GPFeature>();

            foreach (Route route in routes)
            {
                _ConvertRouteZones(route, type, zoneFeatures,
                    seedPointFeatures);
            }

            // We should have either route zones or seed points, but not both.
            Debug.Assert(zoneFeatures.Count == 0 || seedPointFeatures.Count == 0);

            var zones = default(GPFeatureRecordSetLayer);
            if (zoneFeatures.Count > 0)
            {
                zones = new GPFeatureRecordSetLayer
                {
                    SpatialReference = solverSR,
                    Features = zoneFeatures.ToArray(),
                    GeometryType = type == RouteZoneType.Point ?
                        NAGeometryType.esriGeometryPoint : NAGeometryType.esriGeometryPolygon,
                };
            }
            else if (seedPointFeatures.Count > 0)
            {
                zones = new GPFeatureRecordSetLayer
                {
                    GeometryType = NAGeometryType.esriGeometryPoint,
                    SpatialReference = solverSR,
                    Features = seedPointFeatures.ToArray(),
                };
            }

            return new ZoneInfo(zones);
        }

        /// <summary>
        /// Method converts zones and seed points from route to GPFeatures and fill collections 
        /// in parameters.
        /// </summary>
        /// <param name="route">Route.</param>
        /// <param name="type">Zone type.</param>
        /// <param name="zoneFeatures">Zones GPFeatures collection to fill.</param>
        /// <param name="seedPointFeatures">Seed points GPFeatures collection to fill.</param>
        private void _ConvertRouteZones(Route route, RouteZoneType type,
            List<GPFeature> zoneFeatures, List<GPFeature> seedPointFeatures)
        {
            Debug.Assert(type != RouteZoneType.None);

            if (_HasValidZones(route))
            {
                if (type == RouteZoneType.Point)
                {
                    seedPointFeatures.Add(_CreateSeedPointFeature(route,
                        _GetZonesCentroid(route.Zones)));
                }
                else if (type == RouteZoneType.Polygon)
                {
                    GPFeature feature = new GPFeature();

                    feature.Attributes = new AttrDictionary();
                    feature.Attributes.Add(NAAttribute.ROUTE_NAME, route.Id.ToString());
                    feature.Attributes.Add(NAAttribute.IS_HARD_ZONE, route.HardZones);

                    // Geometry.
                    feature.Geometry = new GeometryHolder();
                    feature.Geometry.Value = GPObjectHelper.PolygonToGPPolygon(
                        _GetZonesPolygon(route.Zones));

                    zoneFeatures.Add(feature);
                }
                else
                    Debug.Assert(false);
            }
            else
            {
                // Zones count = 0 and some another routes contain point then
                // just set midpoint between the start and end location.
                if (type == RouteZoneType.Point)
                {
                    seedPointFeatures.Add(_CreateSeedPointFeature(route,
                        _GetRouteLocationsMidpoint(route)));
                }
            }
        }

        /// <summary>
        /// Method converts orders collection into GPFeatureRecordSetLayer.
        /// </summary>
        /// <param name="orders">Orders collection to convert.</param>
        /// <param name="routes">Routes to determine which orders are assigned.</param>
        /// <param name="convertUnassigned">Is need to convert unassigned orders.</param>
        /// <returns>Orders GPFeatureRecordSetLayer.</returns>
        /// <exception cref="RouteException">If unassigned order is not geocoded.</exception>
        private GPFeatureRecordSetLayer _ConvertOrders(ICollection<Order> orders,
            ICollection<Route> routes, bool convertUnassigned)
        {
            Debug.Assert(orders != null);

            GPFeatureRecordSetLayer layer = new GPFeatureRecordSetLayer();
            layer.GeometryType = NAGeometryType.esriGeometryPoint;
            layer.SpatialReference = solverSR;

            List<AssignedOrder> assignedOrders = _GetAssignedOrders(
                routes);

            List<GPFeature> features = new List<GPFeature>();
            foreach (Order order in orders)
            {
                GPFeature feature = null;

                AssignedOrder assignedOrder = null;
                if (_FindAssignedOrder(order, assignedOrders,
                    out assignedOrder))
                {
                    // Convert assigned order.
                    feature = _ConvertOrder(order, assignedOrder);
                }
                else if (convertUnassigned)
                {
                    // Convert unassigned order.
                    feature = _ConvertOrder(order, null);
                }

                if (feature != null)
                    features.Add(feature);
            }

            layer.Features = features.ToArray();

            return layer;
        }

        /// <summary>
        /// Method converts order into GPFeature.
        /// </summary>
        /// <param name="unassignedOrder">Unassigned order to convert.</param>
        /// <param name="assignedOrder">Assigned order to convert.</param>
        /// <returns>Orders GPFeature.</returns>
        /// <exception cref="RouteException">If unassigned order is not geocoded.</exception>
        private GPFeature _ConvertOrder(Order unassignedOrder, AssignedOrder assignedOrder)
        {
            Debug.Assert(unassignedOrder != null);

            GPFeature feature = new GPFeature();

            // Geometry.
            IGeocodable gc = unassignedOrder as IGeocodable;
            Debug.Assert(gc != null);

            if (!gc.IsGeocoded)
            {
                throw new RouteException(String.Format(
                    Properties.Messages.Error_OrderIsUngeocoded, unassignedOrder.Id));
            }

            Debug.Assert(gc.GeoLocation != null);
            feature.Geometry = new GeometryHolder();
            feature.Geometry.Value = GPObjectHelper.PointToGPPoint((Point)gc.GeoLocation);

            // Attributes.
            AttrDictionary attrs = new AttrDictionary();

            // Name.
            attrs.Add(NAAttribute.NAME, unassignedOrder.Id.ToString());

            // Curb approach.
            attrs.Add(NAAttribute.CURB_APPROACH,
                (int)CurbApproachConverter.ToNACurbApproach(
                _context.SolverSettings.GetOrderCurbApproach()));

            // Service time.
            attrs.Add(NAAttribute.SERVICE_TIME, unassignedOrder.ServiceTime);

            // Time windows.
            TimeWindow timeWindow1 = unassignedOrder.TimeWindow;
            TimeWindow timeWindow2 = unassignedOrder.TimeWindow2;

            _SetTimeWindowsAttributes(attrs, ref timeWindow1, ref timeWindow2);

            // Max Violation Time 1.
            attrs.Add(NAAttribute.MAX_VIOLATION_TIME1,
                timeWindow1.IsWideOpen ? (double?)null : unassignedOrder.MaxViolationTime);

            // Max Violation Time 2.
            attrs.Add(NAAttribute.MAX_VIOLATION_TIME2,
                timeWindow2.IsWideOpen ? (double?)null : unassignedOrder.MaxViolationTime);

            // PickUp or DropOff quantities.
            string capacities = _FormatCapacities(unassignedOrder.Capacities);

            if (unassignedOrder.Type == OrderType.Delivery)
            {
                attrs.Add(NAAttribute.DELIVERY, capacities);
                attrs.Add(NAAttribute.PICKUP, null);
            }
            else
            {
                attrs.Add(NAAttribute.PICKUP, capacities);
                attrs.Add(NAAttribute.DELIVERY, null);
            }

            // Revenue.            
			attrs.Add(NAAttribute.REVENUE, (int)unassignedOrder.Priority);

            // Specialties.
            List<Guid> specIds = GetOrderSpecIds(unassignedOrder);
            if (specIds.Count > 0)
                attrs.Add(NAAttribute.SPECIALTY_NAMES, _FormatSpecList(specIds));

            if (assignedOrder != null)
                SetOrderAssignment(attrs, assignedOrder);
            else
                SetOrderAssignment(attrs, unassignedOrder);

            // Status.
            attrs.Add(NAAttribute.STATUS, (int)NAObjectStatus.esriNAObjectStatusOK);

            feature.Attributes = attrs;

            return feature;
        }

        /// <summary>
        /// Method converts orders collection into GPRecordSet containing order pairs.
        /// The rule is when a pickup order has a matching delivery order by order number
        /// we have an order pair.
        /// </summary>
        /// <param name="orders">Orders collection.</param>
        /// <returns>Orders GPRecordSet.</returns>
        private GPRecordSet _ConvertOrderPairs(ICollection<Order> orders)
        {
            Debug.Assert(orders != null);

            List<GPFeature> features = new List<GPFeature>();    
            List<Order> pickupOrders = new List<Order>();
            Dictionary<string, Order> deliveryOrders = new Dictionary<string, Order>();

            foreach (Order orderItem in orders)
            {
                string orderKey = orderItem.PairKey();
                if (string.IsNullOrEmpty(orderKey))
                    continue;

                if (orderItem.Type == OrderType.Pickup)
                    pickupOrders.Add(orderItem);
                else if (orderItem.Type == OrderType.Delivery && !deliveryOrders.ContainsKey(orderKey))
                    deliveryOrders.Add(orderKey, orderItem);
                else
                    continue;
                }

            foreach (Order pickupOrder in pickupOrders)
            {
                string pickupOrderKey = pickupOrder.PairKey();
                    
                // do we have a matching delivery order?

                Order deliveryOrder = null;
                if (!deliveryOrders.TryGetValue(pickupOrderKey, out deliveryOrder))
                    continue;
                  
                features.Add(_ConvertOrderPair(pickupOrder, deliveryOrder));
            }
            

            GPRecordSet rs = new GPRecordSet();
            rs.Features = features.ToArray();

            return rs;
        }

         /// <summary>
        /// Method converts pickup and delivery orders into GPFeature.
        /// </summary>
        /// <param name="pickupOrder">Order.</param>
        /// <param name="deliveryOrder">Order.</param>
        /// <returns>OrderPair GPFeature.</returns>
        private GPFeature _ConvertOrderPair(Order pickupOrder, Order deliveryOrder)
        {
            Debug.Assert(pickupOrder != null && deliveryOrder != null);

            GPFeature feature = new GPFeature();

            // Attributes.
            AttrDictionary attrs = new AttrDictionary();

            // Pickup Name.
            attrs.Add(NAAttribute.FIRST_ORDER_NAME, pickupOrder.Id.ToString());

            // Delivery Name.
            attrs.Add(NAAttribute.SECOND_ORDER_NAME, deliveryOrder.Id.ToString());

            // not using MaxTransitTime

            feature.Attributes = attrs;

            return feature;
        }

        /// <summary>
        /// Method converts depots into GPFeatureRecordSetLayer.
        /// </summary>
        /// <returns>Depots GPFeatureRecordSetLayer.</returns>
        /// <exception cref="RouteException">If location is not geocoded.</exception>
        private GPFeatureRecordSetLayer _ConvertDepots()
        {
            GPFeatureRecordSetLayer layer = new GPFeatureRecordSetLayer();
            layer.GeometryType = NAGeometryType.esriGeometryPoint;
            layer.SpatialReference = solverSR;

            List<GPFeature> features = new List<GPFeature>();
            foreach (Location loc in _depotsAll)
                features.Add(_ConvertDepot(loc));

            layer.Features = features.ToArray();

            return layer;
        }

        /// <summary>
        /// Method converts location into GPFeature.
        /// </summary>
        /// <param name="loc">Location to convert.</param>
        /// <returns>Locations GPFeature.</returns>
        /// <exception cref="RouteException">If location is not geocoded.</exception>
        private GPFeature _ConvertDepot(Location loc)
        {
            Debug.Assert(loc != null);

            GPFeature feature = new GPFeature();

            // Geometry.
            feature.Geometry = new GeometryHolder();
            feature.Geometry.Value = GPObjectHelper.PointToGPPoint(_GetLocationPoint(loc));

            // Attributes.
            AttrDictionary attrs = new AttrDictionary();

            // Name.
            attrs.Add(NAAttribute.NAME, loc.Id.ToString());

            // Curb approach.
            attrs.Add(NAAttribute.CURB_APPROACH,
                (int)CurbApproachConverter.ToNACurbApproach(
                _context.SolverSettings.GetDepotCurbApproach()));

            // Set Time Windows attributes.
            TimeWindow timeWindow1 = loc.TimeWindow;
            TimeWindow timeWindow2 = loc.TimeWindow2;

            _SetTimeWindowsAttributes(attrs, ref timeWindow1, ref timeWindow2);

            feature.Attributes = attrs;

            return feature;
        }

        /// <summary>
        /// Method converts collection of Barriers with its Geometry Types into GPFeatureRecordSetLayer of
        /// BarrierGeometryType type.
        /// </summary>
        /// <param name="barriersByTypes">Typed Barriers collection.</param>
        /// <param name="type">Type to convert.</param>
        /// <param name="attributes">Collection of network attribute units.</param>
        /// <returns>GPFeatureRecordSetLayer of specified barriers type.</returns>
        private static GPFeatureRecordSetLayer _ConvertBarriers(ILookup<BarrierGeometryType, Barrier> barriersByTypes,
            BarrierGeometryType type, IEnumerable<NetworkAttribute> attributes)
        {
            Debug.Assert(barriersByTypes != null);

            // Convert Lookup Table of barriers to enumerable collection of Barriers of specified type.
            var barriers = barriersByTypes[type];

            GPFeatureRecordSetLayer result = null;

            if (barriers.Any())
            {
                // Convert barriers by its types.
                BarriersConverter converter = new BarriersConverter(attributes);
                if (type == BarrierGeometryType.Point)
                    result = converter.ConvertToPointBarriersLayer(barriers, solverSR);
                else if (type == BarrierGeometryType.Polygon)
                    result = converter.ConvertToPolygonBarriersLayer(barriers, solverSR);
                else if (type == BarrierGeometryType.Polyline)
                    result = converter.ConvertToLineBarriersLayer(barriers, solverSR);
                else
                    // Not supported type.
                    Debug.Assert(false);
            }

            return result;
        }

        /// <summary>
        /// Method fills private collection of depots.
        /// </summary>
        /// <param name="routes">Routes collection to get information from.</param>
        private void _BuildDepotsColl(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            _depotsAll.Clear();
            foreach (Route route in routes)
            {
                // Start location.
                Location loc = route.StartLocation;
                if (loc != null && !_depotsAll.Contains(loc))
                    _depotsAll.Add(loc);

                // End location.
                loc = route.EndLocation;
                if (loc != null && !_depotsAll.Contains(loc))
                    _depotsAll.Add(loc);

                // Renewals.
                foreach (Location renewal in route.RenewalLocations)
                {
                    if (!_depotsAll.Contains(renewal))
                        _depotsAll.Add(renewal);
                }
            }
        }

        /// <summary>
        /// Method gets collection of assigned orders from routes collection.
        /// </summary>
        /// <param name="routes">Routes collection to get information from.</param>
        /// <returns>Collection of assigned orders.</returns>
        private List<AssignedOrder> _GetAssignedOrders(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            List<AssignedOrder> orders = new List<AssignedOrder>();
            foreach (Route route in routes)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.StopType == StopType.Order)
                    {
                        AssignedOrder order = new AssignedOrder();
                        order.Order = stop.AssociatedObject as Order;
                        order.Route = route;
                        order.Stop = stop;
                        order.SequenceNumber = stop.SequenceNumber;

                        orders.Add(order);
                    }
                }
            }

            return orders;
        }

        /// <summary>
        /// Method converts network parameters.into GPRecordSet.
        /// </summary>
        /// <returns>Network parameters GPRecordSet.</returns>
        private GPRecordSet _ConvertNetworkParams()
        {
            List<GPFeature> features = new List<GPFeature>();
            foreach (NetworkAttribute attr in _context.NetworkDescription.NetworkAttributes)
            {
                foreach (NetworkAttributeParameter param in attr.Parameters)
                {
                    object value = null;
                    if (_context.SolverSettings.GetNetworkAttributeParameterValue(
                        attr.Name,
                        param.Name,
                        out value))
                    {
                        // Skip null value overrides, let the service to use defaults.
                        if (value != null)
                        {
                            GPFeature feature = new GPFeature();
                            feature.Attributes = new AttrDictionary();
                            feature.Attributes.Add(NAAttribute.NETWORK_ATTR_NAME, attr.Name);
                            feature.Attributes.Add(NAAttribute.NETWORK_ATTR_PARAM_NAME, param.Name);
                            feature.Attributes.Add(NAAttribute.NETWORK_ATTR_PARAM_VALUE, value);
                            features.Add(feature);
                        }
                    }
                }
            }

            GPRecordSet rs = null;
            if (features.Count > 0)
            {
                rs = new GPRecordSet();
                rs.Features = features.ToArray();
            }

            return rs;
        }

        /// <summary>
        /// Method formats restrictions collections from private Solver Context 
        /// field into one string.
        /// </summary>
        /// <returns>String of restrictions.</returns>
        private string _FormatRestrictions()
        {
            ICollection<string> restrictions = SolveHelper.GetEnabledRestrictionNames(
                _context.SolverSettings.Restrictions);

            var restrictionNames = (restrictions ?? Enumerable.Empty<string>())
                .Select(name => string.Format(RESTRICTION_NAME_FORMAT, name))
                .ToList();
            var resStr = string.Join(RESTRICTIONS_DELIMITER, restrictionNames);

            return string.Format(
                RESTRICTIONS_FORMAT,
                resStr);
        }

        /// <summary>
        /// Method get UTurn policy from Solver Context settings and returns it
        /// in correct string format.
        /// </summary>
        /// <returns>String of UTurn policy.</returns>
        private string _GetUTurnPolicy()
        {
            UTurnPolicy policy = _context.SolverSettings.GetUTurnPolicy();
            string uturnPolicy = ModelUTurnPolicy.NoUTurns;

            switch (policy)
            {
                case UTurnPolicy.Nowhere:
                    uturnPolicy = ModelUTurnPolicy.NoUTurns;
                    break;
                case UTurnPolicy.AtDeadEnds:
                    uturnPolicy = ModelUTurnPolicy.AllowDeadEndsOnly;
                    break;
                case UTurnPolicy.AtDeadEndsAndIntersections:
                    uturnPolicy = ModelUTurnPolicy.AllowDeadEndsAndIntersectionsOnly;
                    break;
                default:
                    Debug.Assert(false); // not supported
                    break;
            }

            return uturnPolicy;
        }

        /// <summary>
        /// Method determine if assigned order exists and returns it in output parameter.
        /// </summary>
        /// <param name="order">Order to find.</param>
        /// <param name="assignedOrders">Collection of assigned orders to search in.</param>
        /// <param name="assignedOrder">Output assigned order.</param>
        /// <returns>True - if assigned order found, otherwise - false.</returns>
        private bool _FindAssignedOrder(Order order, ICollection<AssignedOrder> assignedOrders,
            out AssignedOrder assignedOrder)
        {
            assignedOrder = null;

            bool found = false;
            foreach (AssignedOrder ao in assignedOrders)
            {
                if (ao.Order.Equals(order))
                {
                    assignedOrder = ao;
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Method set attributes for both time windows.
        /// </summary>
        /// <param name="attributes">Attributes.</param>
        /// <param name="timeWindow1">Time window 1.</param>
        /// <param name="timeWindow2">Time window 2.</param>
        private void _SetTimeWindowsAttributes(AttrDictionary attributes,
            ref TimeWindow timeWindow1, ref TimeWindow timeWindow2)
        {
            Debug.Assert(attributes != null);
            Debug.Assert(timeWindow1 != null);
            Debug.Assert(timeWindow2 != null);

            // Swap time windows in case first is Wideopen but second isn't.
            if (timeWindow1.IsWideOpen && !timeWindow2.IsWideOpen)
            {
                var temp = timeWindow1;
                timeWindow1 = timeWindow2;
                timeWindow2 = temp;
            }

            // Time window 1.
            _SetTimeWindowAttribute(timeWindow1, NAAttribute.TW_START1, NAAttribute.TW_END1,
                attributes);

            // Time window 2.
            _SetTimeWindowAttribute(timeWindow2, NAAttribute.TW_START2, NAAttribute.TW_END2,
                attributes);
        }

        /// <summary>
        /// Method fills TimeWindow attributes in correct format.
        /// </summary>
        /// <param name="timeWindow">TimeWindow value.</param>
        /// <param name="attrNameStart">Format string for Start TimeWindow.</param>
        /// <param name="attrNameEnd">Format string for End TimeWindow</param>
        /// <param name="attributes">Attributes to fill in.</param>
        private void _SetTimeWindowAttribute(TimeWindow timeWindow,
            string attrNameStart,
            string attrNameEnd,
            AttrDictionary attributes)
        {
            attributes.SetTimeWindow(timeWindow, _plannedDate, attrNameStart, attrNameEnd);
        }

        /// <summary>
        /// Method returns formatted string of specialties Ids collection.
        /// </summary>
        /// <param name="specIds">Collection of specialties Ids.</param>
        /// <returns>Formatted string of specialties Ids.</returns>
        private static string _FormatSpecList(List<Guid> specIds)
        {
            Debug.Assert(specIds != null);

            List<String> list = new List<String>();
            foreach (Guid id in specIds)
                list.Add(id.ToString());

            return String.Join(ATTR_LIST_DELIMITER, list.ToArray());
        }

        /// <summary>
        /// Method returns formatted string of capacities collection.
        /// </summary>
        /// <param name="capacities">Collection of capacities.</param>
        /// <returns>Formatted string of capacities.</returns>
        private static string _FormatCapacities(Capacities capacities)
        {
            Debug.Assert(capacities != null);

            List<string> caps = new List<string>();
            for (int cap = 0; cap < capacities.Count; cap++)
            {
                var capacity = string.Format(
                    CultureInfo.InvariantCulture,
                    CAPACITY_FORMAT,
                    capacities[cap]);
                caps.Add(capacity);
            }

            return String.Join(ATTR_LIST_DELIMITER, caps.ToArray());
        }

        /// <summary>
        /// Method determine a type of zones, assigned to collection of routes.
        /// </summary>
        /// <param name="routes">Collection of routes to get zones from.</param>
        /// <returns>Routes zone type.</returns>
        private static RouteZoneType _GetRoutesZonesType(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            RouteZoneType type = RouteZoneType.None;
            foreach (Route route in routes)
            {
                RouteZoneType zonesType = _GetZonesType(route.Zones);
                if (zonesType != RouteZoneType.None)
                {
                    type = zonesType;
                    break;
                }
            }

            return type;
        }

        /// <summary>
        /// Method determine a common type of zones from collection of zones.
        /// </summary>
        /// <param name="zones">Collection of zones</param>
        /// <returns>Zones type.</returns>
        private static RouteZoneType _GetZonesType(ICollection<Zone> zones)
        {
            Debug.Assert(zones != null);

            RouteZoneType type = RouteZoneType.None;
            foreach (Zone zone in zones)
            {
                if (zone.Geometry != null)
                {
                    if (zone.Geometry is Point)
                    {
                        if (type != RouteZoneType.None && type != RouteZoneType.Point)
                            type = RouteZoneType.Mixed;
                        else
                            type = RouteZoneType.Point;
                    }
                    else if (zone.Geometry is Polygon)
                    {
                        if (type != RouteZoneType.None && type != RouteZoneType.Polygon)
                            type = RouteZoneType.Mixed;
                        else
                            type = RouteZoneType.Polygon;
                    }
                    else
                        // Unsupported geometry type.
                        Debug.Assert(false);

                    if (type == RouteZoneType.Mixed)
                        break;
                }
            }

            return type;
        }

        /// <summary>
        /// Method creates GPFeature for seed point.
        /// </summary>
        /// <param name="route">Route to get information.</param>
        /// <param name="pt">Point location of seed point.</param>
        /// <returns>Seed point GPFeature.</returns>
        private static GPFeature _CreateSeedPointFeature(Route route, Point? pt)
        {
            Debug.Assert(route != null);
            Debug.Assert(pt != null);

            GPFeature feature = new GPFeature();

            feature.Attributes = new AttrDictionary();
            feature.Attributes.Add(NAAttribute.ROUTE_NAME, route.Id.ToString());
            feature.Attributes.Add(NAAttribute.SEED_POINT_TYPE,
                (int)NARouteSeedPointType.esriNARouteSeedPointStatic);

            Point seedPoint = (Point)pt;

            feature.Geometry = new GeometryHolder();
            feature.Geometry.Value = GPObjectHelper.PointToGPPoint(seedPoint);

            return feature;
        }

        /// <summary>
        /// Method gets zones centroid.
        /// </summary>
        /// <param name="zones">Zones to find centroid.</param>
        /// <returns>Zones centroid point.</returns>
        private static Point _GetZonesCentroid(ICollection<Zone> zones)
        {
            Debug.Assert(zones != null);

            double sumX = 0.0;
            double sumY = 0.0;

            foreach (Zone zone in zones)
            {
                Debug.Assert(zone.Geometry is Point);
                Point pt = (Point)zone.Geometry;
                sumX += pt.X;
                sumY += pt.Y;
            }

            Debug.Assert(zones.Count > 0);
            return new Point(
                sumX / (double)zones.Count,
                sumY / (double)zones.Count);
        }

        /// <summary>
        /// Method gets middle point of locations from route.
        /// </summary>
        /// <param name="route">Route to get information.</param>
        /// <returns>Middle point.</returns>
        /// <exception cref="RouteException">If location is not geocoded.</exception>
        private static Point _GetRouteLocationsMidpoint(Route route)
        {
            Debug.Assert(route != null);

            Point pt1 = _GetLocationPoint(route.StartLocation);
            Point pt2 = _GetLocationPoint(route.EndLocation);

            return new Point((pt1.X + pt2.X) / 2, (pt1.Y + pt2.Y) / 2);
        }

        /// <summary>
        /// Method gets union polygon of zones polygons.
        /// </summary>
        /// <param name="zones">Zones to find union.</param>
        /// <returns>Union polygon.</returns>
        private static Polygon _GetZonesPolygon(ICollection<Zone> zones)
        {
            Debug.Assert(zones != null);
            Debug.Assert(zones.All(zone => zone != null));

            var polygons =
                from zone in zones
                where zone.Geometry != null
                select (Polygon)zone.Geometry;

            return _Union(polygons);
        }

        /// <summary>
        /// Unions all polygons in the specified collection into a single polygon.
        /// </summary>
        /// <param name="polygons">The reference to the collection of polygons to union.</param>
        /// <returns>A new polygon containing points from all the specified polygons.</returns>
        private static Polygon _Union(IEnumerable<Polygon> polygons)
        {
            Debug.Assert(polygons != null);
            Debug.Assert(polygons.All(polygon => polygon != null));

            var groups = polygons.SelectMany(polygon => polygon.Groups).ToArray();
            var points = polygons
                .SelectMany(polygon => polygon.GetPoints(0, polygon.TotalPointCount))
                .ToArray();

            return new Polygon(groups, points);
        }

        /// <summary>
        /// Method determines if route has valid zones.
        /// </summary>
        /// <param name="route">Route to get information.</param>
        /// <returns>True - if route has valid zones assigned, otherwise - false.</returns>
        private static bool _HasValidZones(Route route)
        {
            Debug.Assert(route != null);

            bool hasZones = false;
            foreach (Zone zone in route.Zones)
            {
                if (zone.Geometry != null)
                {
                    hasZones = true;
                    break;
                }
            }

            return hasZones;
        }

        /// <summary>
        /// Method gets location point of Location.
        /// </summary>
        /// <param name="loc">Location to get information.</param>
        /// <returns>Point location.</returns>
        /// <exception cref="RouteException">If location is not geocoded.</exception>
        private static Point _GetLocationPoint(Location loc)
        {
            Debug.Assert(loc != null);

            IGeocodable gc = loc as IGeocodable;
            Debug.Assert(gc != null);

            if (!gc.IsGeocoded)
            {
                throw new RouteException(String.Format(
                    Properties.Messages.Error_LocationIsUngeocoded, loc.Id));
            }

            Debug.Assert(gc.GeoLocation != null);
            return (Point)gc.GeoLocation;
        }

        /// <summary>
        /// Method calculates maximum routes cost.
        /// </summary>
        /// <param name="routes">Collection of routes to calculate.</param>
        /// <returns>Max routes costs value.</returns>
        private static double _CalcMaxRoutesCost(ICollection<Route> routes)
        {
            Debug.Assert(routes != null);

            double cost = 0.0;
            foreach (Route route in routes)
                cost += _CalcMaxRouteCost(route);

            return cost;
        }

        /// <summary>
        /// Method calculates maximum route cost.
        /// </summary>
        /// <param name="route">Routes to calculate.</param>
        /// <returns>Max route cost value.</returns>
        /// <exception cref="RouteException">If Fuel Economy of route is 0.0.</exception>
        private static double _CalcMaxRouteCost(Route route)
        {
            Debug.Assert(route != null);

            Driver driver = route.Driver;
            Debug.Assert(driver != null);

            Vehicle vehicle = route.Vehicle;
            Debug.Assert(vehicle != null);

            if (vehicle.FuelEconomy == 0.0)
                throw new RouteException(Properties.Messages.Error_InvalidFuelConsumptionValue);

            double costPerDistance = vehicle.FuelType.Price / vehicle.FuelEconomy;

            double totalCost = vehicle.FixedCost +
                driver.FixedCost +
                driver.TimeBeforeOT * driver.PerHourSalary +
                (route.MaxTotalDuration - driver.TimeBeforeOT) * driver.PerHourOTSalary +
                Math.Max(route.MaxTravelDuration * MAX_SPEED, route.MaxTravelDistance) * costPerDistance;

            return totalCost;
        }

        #endregion

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Attributes list delimiter.
        /// </summary>
        private const string ATTR_LIST_DELIMITER = " ";

        /// <summary>
        /// String format to be used for restrictions parameter creation.
        /// </summary>
        private const string RESTRICTIONS_FORMAT = "[{0}]";

        /// <summary>
        /// String format to be used for restriction names passed to the server.
        /// </summary>
        private const string RESTRICTION_NAME_FORMAT = "\"{0}\"";

        /// <summary>
        /// Restriction list delimiter.
        /// </summary>
        private const string RESTRICTIONS_DELIMITER = ",";

        /// <summary>
        /// Default spatial reference.
        /// </summary>
        private static readonly GPSpatialReference solverSR = new GPSpatialReference(
            GeometryConst.WKID_WGS84);

        /// <summary>
        /// Data contract custom types.
        /// </summary>
        private static readonly Type[] jsonTypes = new Type[]
        {
            typeof(GPFeatureRecordSetLayer),
            typeof(GPRecordSet),
            typeof(GPBoolean),
            typeof(GPDate),
            typeof(double[][][])
        };

        /// <summary>
        /// Max revenue.
        /// </summary>
        private const double MAX_REVENUE = 1000000000.0;

        /// <summary>
        /// Miles per hour.
        /// </summary>
        private const double MAX_SPEED = 60.0;

        /// <summary>
        /// Formatting string for capacities.
        /// </summary>
        private const string CAPACITY_FORMAT = "{0:0.###############}";

        #endregion

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Collection of all depots.
        /// </summary>
        private List<Location> _depotsAll = new List<Location>();

        /// <summary>
        /// Order revenue value.
        /// </summary>
        private double _orderRevenue;

        /// <summary>
        /// Planned date.
        /// </summary>
        private DateTime _plannedDate;

        /// <summary>
        /// Current solver context.
        /// </summary>
        private SolverContext _context;

        #endregion
    }
}
