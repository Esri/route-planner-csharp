using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// AssignOrdersParams class.
    /// </summary>
    internal class AssignOrdersParams
    {
        internal AssignOrdersParams(ICollection<Order> ordersToAssign,
            ICollection<Route> targetRoutes,
            int? targetSequence,
            bool keepViolatedOrdersUnassigned)
        {
            _ordersToAssign = ordersToAssign;
            _targetRoutes = targetRoutes;
            _targetSequence = targetSequence;
            _keepViolatedOrdersUnassigned = keepViolatedOrdersUnassigned;

            if (targetSequence != null && ordersToAssign.Count > 0 && targetRoutes.Count > 0)
            {
                var targetIndex = targetSequence.Value - 1;
                var sourceOrder = ordersToAssign.First();
                var stops = CommonHelpers.GetSortedStops(_targetRoutes.First());
                this.TargetOrderSequence = stops
                    .Take(targetIndex)
                    .Where(stop =>
                        stop.StopType == StopType.Order &&
                        stop.AssociatedObject != sourceOrder)
                    .Count() + 1;
            }
        }

        public ICollection<Order> OrdersToAssign
        {
            get { return _ordersToAssign; }
        }

        public ICollection<Route> TargetRoutes
        {
            get { return _targetRoutes; }
        }

        public int? TargetSequence
        {
            get { return _targetSequence; }
        }

        /// <summary>
        /// Gets target order sequence number value for "assign order
        /// with sequence number within the same route" operation.
        /// </summary>
        public int? TargetOrderSequence
        {
            get;
            private set;
        }

        public bool KeepViolatedOrdersUnassigned
        {
            get { return _keepViolatedOrdersUnassigned; }
        }

        private ICollection<Order> _ordersToAssign;
        private ICollection<Route> _targetRoutes;
        private int? _targetSequence;
        private bool _keepViolatedOrdersUnassigned;
    }

    /// <summary>
    /// AssignOrdersOperation class.
    /// </summary>
    internal class AssignOrdersOperation : VrpOperation
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AssignOrdersOperation(SolverContext context,
            Schedule schedule,
            AssignOrdersParams inputParams,
            SolveOptions options)
            : base(context, schedule, options)
        {
            Debug.Assert(inputParams != null);
            _inputParams = inputParams;
        }

        public AssignOrdersOperation(SolverContext context,
            Schedule schedule,
            AssignOrdersParams inputParams,
            SolveOptions options,
            SolveRequestData reqData,
            AssignOrdersReqBuilder reqBuilder,
            List<Violation> violations)
            : base(context, schedule, options)
        {
            Debug.Assert(inputParams != null);
            Debug.Assert(reqData != null);
            Debug.Assert(reqBuilder != null);
            Debug.Assert(violations != null);

            _inputParams = inputParams;
            _reqData = reqData;
            _reqBuilder = reqBuilder;
            _violations = violations;
        }

        #endregion constructors

        #region public overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override SolveOperationType OperationType
        {
            get { return SolveOperationType.AssignOrders; }
        }

        public override Object InputParams
        {
            get { return _inputParams; }
        }

        public override bool CanGetResultWithoutSolve
        {
            get
            {
                return AssignOrdersOperationHelper.CanGetResultWithoutSolve(
                    SolverContext,
                    Schedule);
            }
        }

        public override SolveResult CreateResultWithoutSolve()
        {
            return AssignOrdersOperationHelper.CreateResultWithoutSolve(
                SolverContext,
                this.RequestData,
                _violations);
        }

        #endregion public overrides

        #region protected overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override SolveRequestData RequestData
        {
            get
            {
                if (_reqData == null)
                    _reqData = _BuildRequestData();

                return _reqData;
            }
        }

        protected override VrpRequestBuilder RequestBuilder
        {
            get
            {
                if (_reqBuilder == null)
                {
                    _reqBuilder = new AssignOrdersReqBuilder(
                        SolverContext,
                        _unlockedOrdersToAssign,
                        _unlockedTargetRoutes,
                        _hasSrcRoutesNotInTargetRoutes);
                }

                return _reqBuilder;
            }
        }

        protected override SubmitVrpJobRequest BuildRequest(
            SolveRequestData reqData)
        {
            SubmitVrpJobRequest req = base.BuildRequest(reqData);

            _AdjustRequestWithTargetSequence(req, _inputParams, _violations);

            _jobRequest = req;

            return req;
        }

        protected override VrpOperation CreateOperation(SolveRequestData reqData,
            List<Violation> violations)
        {
            return new AssignOrdersOperation(base.SolverContext, base.Schedule, _inputParams,
                base.Options,
                reqData,
                _reqBuilder,
                violations);
        }

        protected override SolveRequestOptions RequestOptions
        {
            get
            {
                SolveRequestOptions opt = base.RequestOptions;
                opt.ConvertUnassignedOrders = true;

                return opt;
            }
        }

        protected override bool IsSolveSucceeded(int solveHR)
        {
            // special cases
            if (solveHR == (int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES ||
                solveHR == (int)NAError.E_NA_VRP_SOLVER_NO_SOLUTION)
            {
                return true;
            }

            return base.IsSolveSucceeded(solveHR);
        }

        protected override VrpOperation GetNextStepOperation(
            IList<RouteResult> routeResults,
            List<Violation> violations)
        {
            bool isNextStepRequired = true;

            // check if we need next step
            if (!_hasSrcRoutesNotInTargetRoutes || // target routes include all source routes
                violations.Count == 0 ||           // there are no violations
                _inputParams.KeepViolatedOrdersUnassigned)
            {
                isNextStepRequired = false;
            }

            VrpOperation nextStep = null;
            if (isNextStepRequired)
                nextStep = _CreateNextStepOperation(routeResults, violations);

            return nextStep;
        }

        protected override List<Violation> GetViolations(VrpResult vrpResult)
        {
            List<Violation> violations = base.GetViolations(vrpResult);

            Order violatedOrder = null;
            Violation specViolation = null;
            foreach (Violation v in violations)
            {
                if (v.ViolationType == ViolationType.Specialties &&
                    v.ObjectType == ViolatedObjectType.Order)
                {
                    violatedOrder = v.AssociatedObject as Order;
                    specViolation = v;
                    break;
                }
            }

            if (violatedOrder != null)
            {
                List<Guid> specIds = new List<Guid>();
                foreach (DriverSpecialty spec in violatedOrder.DriverSpecialties)
                {
                    if (spec.Id != AssignOrdersReqBuilder.ASSIGNMENT_SPEC_ID)
                        specIds.Add(spec.Id);
                }

                foreach (VehicleSpecialty spec in violatedOrder.VehicleSpecialties)
                {
                    if (spec.Id != AssignOrdersReqBuilder.ASSIGNMENT_SPEC_ID)
                        specIds.Add(spec.Id);
                }

                bool removeSpecViolation = true;
                if (specIds.Count > 0)
                {
                    foreach (Guid specId in specIds)
                    {
                        foreach (GPFeature feature in _jobRequest.Routes.Features)
                        {
                            if (!_IsSpecBelongToRoute(specId, feature))
                            {
                                removeSpecViolation = false;
                                break;
                            }
                        }
                        if (!removeSpecViolation)
                            break;
                    }
                }

                if (removeSpecViolation)
                    violations.Remove(specViolation);
            }

            if (_violations != null)
                violations.AddRange(_violations);

            return violations;
        }

        #endregion protected overrides

        #region private static methods
        private static List<Order> _GetOrdersToConvert(
            ICollection<Route> affectedRoutes,
            ICollection<Order> unlockedOrders)
        {
            List<Order> orders = new List<Order>();
            foreach (Route route in affectedRoutes)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.StopType == StopType.Order)
                    {
                        Order order = stop.AssociatedObject as Order;
                        if (!unlockedOrders.Contains(order))
                            orders.Add(order); // TODO: (?) check duplicates
                    }
                }
            }

            return orders;
        }

        private static bool _IsSpecBelongToRoute(Guid specId, GPFeature rtFeature)
        {
            Guid routeSpecId;
            return _AttrToObjectId(NAAttribute.SPECIALTY_NAMES, rtFeature.Attributes,
                out routeSpecId) &&
                routeSpecId == specId;
        }

        private static bool _AttrToObjectId(string attrName, AttrDictionary attrs, out Guid id)
        {
            id = Guid.Empty;

            bool res = false;
            try
            {
                id = new Guid(attrs.Get<string>(attrName));
                res = true;
            }
            catch { }

            return res;
        }
        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SolveRequestData _BuildRequestData()
        {
            // validate target sequence conditions
            if (_inputParams.TargetSequence != null)
            {
                if (_inputParams.TargetSequence < 1)
                    throw new RouteException(Properties.Messages.Error_InvalidTargetSequence);

                if (_inputParams.OrdersToAssign.Count != 1)
                    throw new RouteException(Properties.Messages.Error_InvalidOrdersToAssignCount);

                if (_inputParams.TargetRoutes.Count != 1)
                    throw new RouteException(Properties.Messages.Error_InvalidTargetRoutesCount);
            }

            // get unlocked target routes
            List<Route> unlockedTargetRoutes = new List<Route>();
            foreach (Route route in _inputParams.TargetRoutes)
            {
                // TODO: (?) check if route belongs to processing schedule
                if (!route.IsLocked)
                    unlockedTargetRoutes.Add(route);
            }

            // check if we have at least one unlocked route
            if (unlockedTargetRoutes.Count == 0)
                throw new RouteException(Properties.Messages.Error_InvalidUnlockedTargetRoutesCount);

            // get unlocked orders
            List<Order> unlockedOrdersToAssign = new List<Order>();
            foreach (Order order in _inputParams.OrdersToAssign)
            {
                if (!SolveHelper.IsOrderLocked(order, Schedule))
                    unlockedOrdersToAssign.Add(order);
            }

            // check if we have at least one unlocked order
            if (unlockedOrdersToAssign.Count == 0)
                throw new RouteException(Properties.Messages.Error_InvalidUnlockedOrdersToAssignCount);

            // get source routes:
            // routes planned on schedule's day where each route has at
            // least one order from UnlockedOrdersToAssign assigned to it
            List<Route> sourceRoutes = _GetSourceRoutes(Schedule.Routes,
                unlockedOrdersToAssign);

            // routes to convert: TargetRoutes and SourceRoutes
            List<Route> routes = new List<Route>();
            routes.AddRange(_inputParams.TargetRoutes);
            routes.AddRange(sourceRoutes);

            // orders to convert:
            // orders assigned to converting routes and unassigned orders
            // from UnlockedOrdersToAssign
            List<Order> candidateOrders = _GetAssignedOrders(routes);
            candidateOrders.AddRange(unlockedOrdersToAssign);

            var orders = new List<Order>();
            foreach (var order in candidateOrders)
            {
                if (order.IsGeocoded)
                    orders.Add(order);
                else
                {
                    var violation = new Violation()
                    {
                        ViolationType = ViolationType.Ungeocoded,
                        AssociatedObject = order
                    };

                    _violations.Add(violation);
                }
            }

            // check if SourceRoutes contains routes that are not contained in
            // UnlockedTargetRoutes
            if (_inputParams.TargetSequence == null)
            {
                foreach (Route srcRoute in sourceRoutes)
                {
                    if (!unlockedTargetRoutes.Contains(srcRoute))
                    {
                        _hasSrcRoutesNotInTargetRoutes = true;
                        break;
                    }
                }
            }

            // get barriers planned on schedule's date
            ICollection<Barrier> barriers = SolverContext.Project.Barriers.Search(
                (DateTime)Schedule.PlannedDate);

            _sourceRoutes = sourceRoutes;
            _unlockedOrdersToAssign = unlockedOrdersToAssign;
            _unlockedTargetRoutes = unlockedTargetRoutes;
            _barriers = barriers;

            SolveRequestData reqData = new SolveRequestData();
            reqData.Routes = new List<Route>(routes.Distinct());
            reqData.Orders = new List<Order>(orders.Distinct());
            reqData.Barriers = barriers;

            return reqData;
        }

        private List<Order> _GetAssignedOrders(
            ICollection<Route> routes)
        {
            List<Order> orders = new List<Order>();
            foreach (Route route in routes)
                orders.AddRange(SolveHelper.GetAssignedOrders(route));

            return orders;
        }

        private List<Route> _GetSourceRoutes(
            ICollection<Route> dayRoutes,
            ICollection<Order> unlockedOrders)
        {
            List<Route> routes = new List<Route>();
            foreach (Route route in dayRoutes)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.StopType == StopType.Order)
                    {
                        Order order = stop.AssociatedObject as Order;
                        if (unlockedOrders.Contains(order))
                        {
                            routes.Add(route);
                            break;
                        }
                    }
                }
            }

            return routes;
        }

        private List<Route> _GetRoutesToUpdate(
            ICollection<Route> sourceRoutes,
            ICollection<Order> violatedOrders)
        {
            List<Route> routes = new List<Route>();
            foreach (Route route in sourceRoutes)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.StopType == StopType.Order)
                    {
                        Order order = stop.AssociatedObject as Order;
                        if (violatedOrders.Contains(order))
                        {
                            routes.Add(route);
                            break;
                        }
                    }
                }
            }

            return routes;
        }

        private VrpOperation _CreateNextStepOperation(
            IList<RouteResult> routeResults,
            IList<Violation> violations)
        {
            // get violated orders
            List<Order> violatedOrders = new List<Order>();
            foreach (Violation violation in violations)
            {
                if (violation.ObjectType == ViolatedObjectType.Order)
                    violatedOrders.Add(violation.AssociatedObject as Order);
            }

            // get routes to update:
            // routes subset from SourceRouts collection where each route
            // has at least one order from VilatedOrders assigned to it
            List<Route> routesToUpdate = _GetRoutesToUpdate(_sourceRoutes,
                violatedOrders);

            VrpOperation nextStep = null;
            if (routesToUpdate.Count > 0)
            {
                // get processed orders:
                // orders that were successfully assigned to their new target routes
                List<Order> processedOrders = new List<Order>();
                foreach (Order order in _unlockedOrdersToAssign)
                {
                    if (!violatedOrders.Contains(order))
                        processedOrders.Add(order);
                }

                // get orders assigned to RoutesToUpdate
                List<Order> assignedOrders = _GetAssignedOrders(routesToUpdate);

                // exclude processed orders
                List<Order> ordersToUpdate = new List<Order>();
                foreach (Order order in assignedOrders)
                {
                    if (!processedOrders.Contains(order))
                        ordersToUpdate.Add(order);
                }

                if (ordersToUpdate.Count > 0)
                {
                    SolveRequestData reqData = new SolveRequestData();
                    reqData.Routes = routesToUpdate;
                    reqData.Orders = ordersToUpdate;
                    reqData.Barriers = _barriers;

                    nextStep = new AssignOrdersStep2(SolverContext, Schedule, reqData,
                        _inputParams,
                        routeResults,
                        violations,
                        base.Options);
                }
            }

            return nextStep;
        }

        private void _ShiftSequence(GPFeatureRecordSetLayer orders,
            int targetSeq)
        {
            foreach (GPFeature feature in orders.Features)
            {
                int sequence;
                if (feature.Attributes.TryGet<int>(NAAttribute.SEQUENCE, out sequence) &&
                    sequence >= targetSeq)
                {
                    feature.Attributes.Set(NAAttribute.SEQUENCE, sequence + 1);
                }
            }
        }

        private bool _HasSequence(GPFeatureRecordSetLayer orders, int targetSeq)
        {
            bool found = false;
            foreach (GPFeature feature in orders.Features)
            {
                int sequence;
                if (feature.Attributes.TryGet<int>(NAAttribute.SEQUENCE, out sequence) &&
                    sequence == targetSeq)
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        private bool _FindTargetFeature(GPFeatureRecordSetLayer orders,
            Order order,
            out GPFeature orderFeature)
        {
            orderFeature = null;

            bool found = false;
            foreach (GPFeature feature in orders.Features)
            {
                string idStr = null;
                if (feature.Attributes.TryGet<string>(NAAttribute.NAME, out idStr))
                {
                    Guid id = new Guid(idStr);
                    if (id == order.Id)
                    {
                        orderFeature = feature;
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Adjusts VRP job request for cases when target sequence parameter is set.
        /// Does nothing if <paramref name="parameters"/> have no target sequence
        /// or order to be assigned is a violated one.
        /// </summary>
        /// <param name="request">Request to be adjusted.</param>
        /// <param name="parameters">Assign orders operation parameters to be
        /// used for adjusting the request.</param>
        /// <param name="violations">Collection of violations to be checked.</param>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RouteException">when
        /// <paramref name="request"/> does not contain order from
        /// <paramref name="parameters"/>.</exception>
        private void _AdjustRequestWithTargetSequence(
            SubmitVrpJobRequest request,
            AssignOrdersParams parameters,
            IEnumerable<Violation> violations)
        {
            Debug.Assert(request != null);
            Debug.Assert(parameters != null);
            Debug.Assert(violations != null);

            if (!parameters.TargetSequence.HasValue)
            {
                return;
            }

            Debug.Assert(parameters.OrdersToAssign.Count == 1); // must contain a single order
            Debug.Assert(parameters.TargetRoutes.Count == 1); // must contain a single route

            var orderToAssign = parameters.OrdersToAssign.First();
            Debug.Assert(orderToAssign != null);

            if (violations.Any(v => v.AssociatedObject == orderToAssign))
            {
                return;
            }

            GPFeature targetFeature;
            if (!_FindTargetFeature(request.Orders, orderToAssign, out targetFeature))
            {
                throw new RouteException(Properties.Messages.Error_InternalRouteError);
            }

            // free place for new order position
            var targetSequence = parameters.TargetSequence.Value;
            Debug.Assert(targetSequence > 0);

            if (_HasSequence(request.Orders, targetSequence))
                _ShiftSequence(request.Orders, targetSequence);

            // set assignment attributes
            var targetRoute = parameters.TargetRoutes.First();
            Debug.Assert(targetRoute != null);

            targetFeature.Attributes.Set(NAAttribute.SEQUENCE, targetSequence);
            targetFeature.Attributes.Set(NAAttribute.ROUTE_NAME, targetRoute.Id.ToString());
            targetFeature.Attributes.Set(NAAttribute.ASSIGNMENT_RULE,
                (int)NAOrderAssignmentRule.esriNAOrderPreserveRouteAndRelativeSequence);
        }
        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private AssignOrdersParams _inputParams;
        private ICollection<Order> _unlockedOrdersToAssign;
        private List<Route> _sourceRoutes;
        private List<Route> _unlockedTargetRoutes = new List<Route>();
        private ICollection<Barrier> _barriers;
        private bool _hasSrcRoutesNotInTargetRoutes = false;
        private SubmitVrpJobRequest _jobRequest;
        private SolveRequestData _reqData;
        private AssignOrdersReqBuilder _reqBuilder;
        private List<Violation> _violations = new List<Violation>();

        #endregion private fields
    }
}
