using System;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// BuildRoutesOperation class.
    /// </summary>
    internal class BuildRoutesOperation : VrpOperation
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public BuildRoutesOperation(SolverContext context,
            Schedule schedule,
            SolveOptions options,
            BuildRoutesParameters inputParams)
            : base(context, schedule, options)
        {
            Debug.Assert(inputParams != null);
            _inputParams = inputParams;
        }

        public BuildRoutesOperation(SolverContext context,
            Schedule schedule,
            SolveOptions options,
            SolveRequestData reqData,
            List<Violation> violations,
            BuildRoutesParameters inputParams)
            : base(context, schedule, options)
        {
            Debug.Assert(reqData != null);
            Debug.Assert(violations != null);
            Debug.Assert(inputParams != null);
            _reqData = reqData;
            _violations = violations;
            _inputParams = inputParams;
        }

        #endregion constructors

        #region public overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override SolveOperationType OperationType
        {
            get { return SolveOperationType.BuildRoutes; }
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

        protected override SolveRequestOptions RequestOptions
        {
            get
            {
                SolveRequestOptions opt = base.RequestOptions;
                opt.ConvertUnassignedOrders = true;

                return opt;
            }
        }

        protected override VrpRequestBuilder RequestBuilder
        {
            get
            {
                return new BuildRoutesReqBuilder(SolverContext);
            }
        }

        protected override List<Violation> GetViolations(VrpResult vrpResult)
        {
            List<Violation> violations = base.GetViolations(vrpResult);

            if (_violations != null)
                violations.AddRange(_violations);

            return violations;
        }

        protected override VrpOperation CreateOperation(SolveRequestData reqData,
            List<Violation> violations)
        {
            return new BuildRoutesOperation(base.SolverContext, base.Schedule,
                base.Options,
                reqData,
                violations,
                _inputParams);
        }

        protected override bool CanConvertResult(int solveHR)
        {
            return ComHelper.IsHRSucceeded(solveHR) ||
                solveHR == (int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES ||
                solveHR == (int)NAError.E_NA_VRP_SOLVER_NO_SOLUTION;
        }

        #endregion protected overrides

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SolveRequestData _BuildRequestData()
        {
            // exclude ungeocoded orders
            List<Order> orders = new List<Order>();
            foreach (Order order in _inputParams.OrdersToAssign)
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

            // get barriers planned on schedule's date
            DateTime day = (DateTime)Schedule.PlannedDate;
            ICollection<Barrier> barriers = SolverContext.Project.Barriers.Search(day);

            SolveRequestData reqData = new SolveRequestData();
            reqData.Routes = _inputParams.TargetRoutes;
            reqData.Orders = orders;
            reqData.Barriers = barriers;

            return reqData;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SolveRequestData _reqData;
        private List<Violation> _violations = new List<Violation>();

        /// <summary>
        /// Build routes operation parameters.
        /// </summary>
        BuildRoutesParameters _inputParams;

        #endregion private fields
    }
}
