using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility.CoreEx;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Solve statistics keeper class.
    /// </summary>
    internal class SolveStatistics
    {
        /// <summary>
        /// Request time.
        /// </summary>
        public TimeSpan RequestTime { get; internal set; }
    }

    /// <summary>
    /// VRP result keeper class.
    /// </summary>
    internal class VrpResult
    {
        /// <summary>
        /// Gets a reference to the collection of job messages received from
        /// VRP service.
        /// </summary>
        public JobMessage[] Messages { get; internal set; }
        /// <summary>
        /// Gets a reference to the route solve response.
        /// </summary>
        public BatchRouteSolveResponse RouteResponse { get; internal set; }

        /// <summary>
        /// Gets a reference to the VRP results. Could be null if there are no
        /// results for some reason (e.g. all submitted orders have violations).
        /// </summary>
        public GPRouteResult ResultObjects { get; internal set; }
        /// <summary>
        /// Gets a solve operation HResult from server.
        /// </summary>
        public int SolveHR { get; internal set; }
    }

    /// <summary>
    /// Base VRP operation implementation class.
    /// </summary>
    internal abstract class VrpOperation : ISolveOperation<SubmitVrpJobRequest>
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>VrpOperation</c> class.
        /// </summary>
        /// <param name="context">Solver context.</param>
        /// <param name="schedule">Current schedule.</param>
        /// <param name="options">Solve options.</param>
        public VrpOperation(SolverContext context, Schedule schedule, SolveOptions options)
        {
            _context = context;
            _schedule = schedule;
            _options = options;
        }

        #endregion constructors

        #region ISolveOperation<TSolveRequest> Members

        /// <summary>
        /// Related schedule.
        /// </summary>
        public Schedule Schedule
        {
            get { return _schedule; }
        }

        /// <summary>
        /// Solve operation type.
        /// </summary>
        public abstract SolveOperationType OperationType
        {
            get;
        }

        /// <summary>
        /// Input parameters.
        /// </summary>
        public abstract Object InputParams
        {
            get;
        }

        /// <summary>
        /// Checks - can get result without solve.
        /// </summary>
        public virtual bool CanGetResultWithoutSolve
        {
            get { return false; }
        }

        /// <summary>
        /// Creates result without solve.
        /// </summary>
        /// <returns>Always NULL.</returns>
        public virtual SolveResult CreateResultWithoutSolve()
        {
            return null;
        }

        /// <summary>
        /// Creates request.
        /// </summary>
        /// <returns>Created VRP request.</returns>
        public SubmitVrpJobRequest CreateRequest()
        {
            // check number of allowed routes
            _CheckLicensePermissions();

            // get orders and routes to solve
            SolveRequestData reqData = this.RequestData;

            // validate request data
            _ValidateReqData(reqData);

            // build request
            SubmitVrpJobRequest req = BuildRequest(reqData);

            // set operation info for logging
            req.OperationType = this.OperationType;
            req.OperationDate = (DateTime)_schedule.PlannedDate;

            if (req.PointBarriers != null)
                _pointBarriers = req.PointBarriers.Features;
            if (req.LineBarriers != null)
                _polylineBarriers = req.LineBarriers.Features;
            if (req.PolygonBarriers != null)
                _polygonBarriers = req.PolygonBarriers.Features;

            return req;
        }

        /// <summary>
        /// Does solve.
        /// </summary>
        /// <param name="jobRequest">VRP request.</param>
        /// <param name="cancelTracker">Cancel tracker (Can be NULL).</param>
        /// <returns>Function returning VRP solve operation result.</returns>
        public Func<SolveOperationResult<SubmitVrpJobRequest>> Solve(
            SubmitVrpJobRequest jobRequest,
            ICancelTracker cancelTracker)
        {
            Debug.Assert(null != jobRequest);

            var result = default(VrpResult);

            var resultProvider = Functional.MakeLambda(() =>
            {
                var operationResult = new SolveOperationResult<SubmitVrpJobRequest>();
                operationResult.SolveResult = _ProcessSolveResult(result, jobRequest);
                operationResult.NextStepOperation = _nextStep;

                return operationResult;
            });

            if (jobRequest.Orders.Features.Length == 0)
            {
                result = new VrpResult()
                {
                    SolveHR = 0,
                };

                return resultProvider;
            }

            var factory = _context.VrpServiceFactory;
            using (var client = factory.CreateService(VrpRequestBuilder.JsonTypes))
            {
                var requestTime = new Stopwatch();
                requestTime.Start();

                // send request
                var response = _SendRequest(
                    client,
                    jobRequest,
                    cancelTracker);

                // create VRP result
                result = new VrpResult()
                {
                    Messages = response.Messages,
                    SolveHR = response.SolveHR,
                    ResultObjects = response.RouteResult,
                };

                if (CanProcessResult(result.SolveHR))
                {
                    _ValidateVrpResults(response);

                    // calc. statistics
                    requestTime.Stop();

                    var stat = new SolveStatistics()
                    {
                        RequestTime = requestTime.Elapsed
                    };

                    _LogJobStatistics(response.JobID, stat);
                }

                if (!_options.GenerateDirections && result.ResultObjects != null)
                {
                    result.ResultObjects.Directions = null;
                }

                return resultProvider;
            }
        }
        #endregion

        #region protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Solver context.
        /// </summary>
        protected SolverContext SolverContext
        {
            get { return _context; }
        }

        /// <summary>
        /// Solve options.
        /// </summary>
        protected SolveOptions Options
        {
            get { return _options; }
        }

        #endregion protected properties

        #region protected abstract properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Request data.
        /// </summary>
        protected abstract SolveRequestData RequestData
        {
            get;
        }

        #endregion protected abstract properties

        #region protected abstract methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates operation.
        /// </summary>
        /// <param name="reqData">Solve request data.</param>
        /// <param name="violations">List of violations.</param>
        /// <returns>Created VRP opeartion.</returns>
        protected abstract VrpOperation CreateOperation(SolveRequestData reqData,
            List<Violation> violations);

        #endregion protected abstract methods

        #region protected overridable properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Request builder.
        /// </summary>
        protected virtual VrpRequestBuilder RequestBuilder
        {
            get { return new VrpRequestBuilder(_context); }
        }

        /// <summary>
        /// Request options.
        /// </summary>
        protected virtual SolveRequestOptions RequestOptions
        {
            get
            {
                var opt = new SolveRequestOptions();
                // TODO: configure proper parameter in model
                opt.PopulateRouteLines = _options.GenerateDirections;
                opt.ConvertUnassignedOrders = true;

                return opt;
            }
        }

        #endregion protected overridable properties

        #region protected overridable methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds request.
        /// </summary>
        /// <param name="reqData">Solve request data.</param>
        /// <returns>Submit VRP job request.</returns>
        protected virtual SubmitVrpJobRequest BuildRequest(SolveRequestData reqData)
        {
            return this.RequestBuilder.BuildRequest(
                _schedule,
                reqData,
                this.RequestOptions,
                this.Options);
        }

        /// <summary>
        /// Checks - can process result.
        /// </summary>
        /// <param name="solveHR">Solve operation HResult from server.</param>
        /// <returns>TRUE if can process result.</returns>
        protected virtual bool CanProcessResult(int solveHR)
        {
            return ComHelper.IsHRSucceeded(solveHR) ||
                   solveHR == (int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES ||
                   solveHR == (int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES ||
                   solveHR == (int)NAError.E_NA_VRP_SOLVER_NO_SOLUTION ||
                   solveHR == (int)NAError.E_NA_VRP_SOLVER_INVALID_INPUT;
        }

        /// <summary>
        /// Checks - can convert result.
        /// </summary>
        /// <param name="solveHR">Solve operation HResult from server.</param>
        /// <returns>TRUE if HResult is succeeded.</returns>
        protected virtual bool CanConvertResult(int solveHR)
        {
            return ComHelper.IsHRSucceeded(solveHR);
        }

        /// <summary>
        /// Checks - is solve succeeded.
        /// </summary>
        /// <param name="solveHR">Solve operation HResult from server.</param>
        /// <returns>TRUE if solve operation succeeded on server.</returns>
        protected virtual bool IsSolveSucceeded(int solveHR)
        {
            return CanConvertResult(solveHR);
        }

        /// <summary>
        /// Processes result.
        /// </summary>
        /// <param name="vrpResult">VRP solve operation result.</param>
        /// <param name="request">Request used for obtaining the response.</param>
        /// <returns>Founded violations.</returns>
        protected virtual List<Violation> ProcessResult(
            VrpResult vrpResult,
            SubmitVrpJobRequest request)
        {
            Debug.Assert(vrpResult != null);
            Debug.Assert(request != null);

            // get violations
            List<Violation> violations = GetViolations(vrpResult);

            VrpOperation nextStep = null;
            if (CanConvertResult(vrpResult.SolveHR))
            {
                if (vrpResult.ResultObjects != null)
                {
                    // convert VRP result
                    IList<RouteResult> routeResults = ConvertResult(vrpResult, request);

                    // check if we need next step
                    nextStep = GetNextStepOperation(routeResults, violations);
                    if (nextStep == null)
                        SetRouteResults(routeResults); // final step, set results to schedule
                }
            }
            else if (!_options.FailOnInvalidOrderGeoLocation)
            {
                if (!_HasRestrictedDepots(violations))
                {
                    List<Order> restrictedOrders = _GetRestrictedOrders(violations);
                    if (restrictedOrders.Count > 0)
                        nextStep = _GetRestrictedOrdersOperation(restrictedOrders, violations);
                }
            }

            _nextStep = nextStep;

            return violations;
        }

        /// <summary>
        /// Gets next step operation.
        /// </summary>
        /// <param name="routeResults">Route's result.</param>
        /// <param name="violations">Violations.</param>
        /// <returns>Next step operation otr NULL if not supported.</returns>
        protected virtual VrpOperation GetNextStepOperation(
            IList<RouteResult> routeResults,
            List<Violation> violations)
        {
            return null;
        }

        /// <summary>
        /// Gets violations.
        /// </summary>
        /// <param name="vrpResult">VRP solve operation result.</param>
        /// <returns>Founded violations.</returns>
        protected virtual List<Violation> GetViolations(VrpResult vrpResult)
        {
            Debug.Assert(null != vrpResult);

            var list = new List<Violation>();

            var conv = new VrpResultConverter(_context.Project, _schedule,
                _context.SolverSettings);

            var results = vrpResult.ResultObjects;

            int hr = vrpResult.SolveHR;
            if (ComHelper.IsHRSucceeded(hr) && vrpResult.ResultObjects != null)
                list.AddRange(conv.GetOrderViolations(results.ViolatedStops, hr));

            else if (hr == (int)NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES)
                list.AddRange(conv.GetRouteViolations(results.Routes, hr));

            else if (hr == (int)NAError.E_NA_VRP_SOLVER_NO_SOLUTION)
                list.AddRange(conv.GetOrderViolations(results.ViolatedStops, hr));

            else if (hr == (int)NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES)
            {
                list.AddRange(conv.GetRouteViolations(results.Routes, hr));
                list.AddRange(conv.GetOrderViolations(results.ViolatedStops, hr));
            }
            else if (hr == (int)NAError.E_NA_VRP_SOLVER_INVALID_INPUT)
            {
                list.AddRange(conv.GetDepotViolations(results.ViolatedStops));
                list.AddRange(conv.GetRestrictedOrderViolations(results.ViolatedStops));
            }

            return list;
        }

        /// <summary>
        /// Converts VRP operation result to route's result.
        /// </summary>
        /// <param name="vrpResult">VRP solve operation result.</param>
        /// <param name="request">Request used for obtaining the response.</param>
        /// <returns>Route results.</returns>
        protected virtual IList<RouteResult> ConvertResult(
            VrpResult vrpResult,
            SubmitVrpJobRequest request)
        {
            Debug.Assert(vrpResult != null);
            Debug.Assert(request != null);

            var conv = new VrpResultConverter(_context.Project, _schedule, _context.SolverSettings);
            return conv.Convert(vrpResult.ResultObjects, vrpResult.RouteResponse, request);
        }

        /// <summary>
        /// Sets route's results.
        /// </summary>
        /// <param name="routeResults">Route's results</param>
        protected virtual void SetRouteResults(IList<RouteResult> routeResults)
        {
            // get locked orders
            var lockedOrders = new List<Order>();
            foreach (Route route in _schedule.Routes)
            {
                foreach (Stop stop in route.Stops)
                {
                    if (stop.StopType == StopType.Order && stop.IsLocked)
                        lockedOrders.Add(stop.AssociatedObject as Order);
                }
            }

            // set locked status to new stops
            foreach (RouteResult rr in routeResults)
            {
                foreach (StopData stop in rr.Stops)
                {
                    if (stop.StopType == StopType.Order &&
                        lockedOrders.Contains(stop.AssociatedObject as Order))
                    {
                        stop.IsLocked = true;
                    }
                }
            }

            // set route results to schedule
            _context.Project.Schedules.SetRouteResults(_schedule, routeResults);
        }

        #endregion protected overridable methods

        #region private methods
        /// <summary>
        /// Processes (converts) solve operation result.
        /// </summary>
        /// <param name="vrpResult">VRP solve operation result.</param>
        /// <param name="request">Request used for obtaining the response.</param>
        /// <returns>Solve operation result.</returns>
        private SolveResult _ProcessSolveResult(VrpResult vrpResult, SubmitVrpJobRequest request)
        {
            Debug.Assert(vrpResult != null);
            Debug.Assert(request != null);

            List<Violation> violations = null;
            if (CanProcessResult(vrpResult.SolveHR))
                violations = ProcessResult(vrpResult, request);

            return _CreateSolveResult(vrpResult, violations);
        }

        /// <summary>
        /// Converts job submitting response into the result of synchronous
        /// job execution.
        /// </summary>
        /// <param name="response">The reference to the response object to
        /// be converted.</param>
        /// <param name="client">The reference to the REST service client object
        /// to retrieve job results with.</param>
        /// <param name="cancelTracker">The reference to the cancellation tracker
        /// object.</param>
        /// <returns><see cref="T:ESRI.ArcLogistics.Routing.Json.VrpResultsResponse"/>
        /// object corresponding to the specified job submitting response.</returns>
        private VrpResultsResponse _ConvertToSyncReponse(
            GetVrpJobResultResponse response,
            IVrpRestService client,
            ICancelTracker cancelTracker)
        {
            var syncResponse = new VrpResultsResponse()
            {
                SolveSucceeded = false,
                JobID = response.JobId,
            };

            syncResponse.SolveSucceeded = false;
            syncResponse.Messages = response.Messages;
            syncResponse.SolveHR = HResult.E_FAIL;

            if (response.JobStatus != NAJobStatus.esriJobSucceeded)
            {
                return syncResponse;
            }

            var solveSucceeded = _GetGPObject<GPBoolParam>(
                client,
                response.JobId,
                response.Outputs.SolveSucceeded.ParamUrl);
            syncResponse.SolveSucceeded = solveSucceeded.Value;
            syncResponse.SolveHR = _GetSolveHR(syncResponse);

            if (CanProcessResult(syncResponse.SolveHR))
            {
                _ValidateResponse(response);

                // get route results
                syncResponse.RouteResult = _LoadResultData(
                    client,
                    response,
                    cancelTracker);
            }

            return syncResponse;
        }

        /// <summary>
        /// Sends request to the VRP REST service using either syncronous or
        /// asynchronous service depending on the request.
        /// </summary>
        /// <param name="client">The reference to the REST service client object
        /// to send requests with.</param>
        /// <param name="request">The request to be sent to the VRP service.</param>
        /// <param name="tracker">The reference to the cancellation tracker
        /// object.</param>
        /// <returns>Result of the request processing by the VRP REST service.</returns>
        private VrpResultsResponse _SendRequest(
            IVrpRestService client,
            SubmitVrpJobRequest request,
            ICancelTracker tracker)
        {
            if (_context.VrpRequestAnalyzer.CanExecuteSyncronously(request))
            {
                var response = client.ExecuteJob(request);

                return _GetVrpResults(response);
            }

            var asyncResponse = _SendAsyncRequest(client, request, tracker);

            return _ConvertToSyncReponse(asyncResponse, client, tracker);
        }

        /// <summary>
        /// Sends async. request to the VRP REST service using either asynchronous service
        /// depending on the request.
        /// </summary>
        /// <param name="context">The reference to the VRP REST service context.</param>
        /// <param name="request">The request to be sent to the VRP service.</param>
        /// <param name="tracker">The reference to the cancellation tracker object.</param>
        /// <returns>Result of the request processing by the VRP REST service.</returns>
        private GetVrpJobResultResponse _SendAsyncRequest(
            IVrpRestService context,
            SubmitVrpJobRequest request,
            ICancelTracker tracker)
        {
            Debug.Assert(context != null);
            Debug.Assert(request != null);

            // check cancellation state
            _CheckCancelState(tracker);

            GetVrpJobResultResponse resp = null;
            try
            {
                // submit job
                resp = _SubmitJob(context, request);

                // poll job status
                bool jobCompleted = false;
                do
                {
                    // check cancellation state
                    _CheckCancelState(tracker);

                    // check job status
                    if (resp.JobStatus == NAJobStatus.esriJobFailed ||
                        resp.JobStatus == NAJobStatus.esriJobSucceeded ||
                        resp.JobStatus == NAJobStatus.esriJobCancelled)
                    {
                        // job completed
                        jobCompleted = true;
                        _LogResponse(resp);
                    }
                    else
                    {
                        // TODO: limit number of retries or total request time

                        // job is still executing, make timeout
                        Thread.Sleep(RETRY_DELAY);

                        // check cancellation state
                        _CheckCancelState(tracker);

                        string jobId = resp.JobId;
                        try
                        {
                            // try to get job result
                            resp = _GetJobResult(context, jobId);
                        }
                        catch (Exception e)
                        {
                            _LogJobError(jobId, e);
                            if (!ServiceHelper.IsCommunicationError(e))
                                throw;
                        }
                    }
                }
                while (!jobCompleted);
            }
            catch (UserBreakException)
            {
                if (resp != null)
                    _CancelJob(context, resp.JobId); // cancel job on server

                throw;
            }

            return resp;
        }

        /// <summary>
        /// Loads result data from response from the VRP REST service.
        /// </summary>
        /// <param name="client">The reference to the REST service client object
        /// to send requests with.</param>
        /// <param name="resp">The response from the VRP service.</param>
        /// <param name="tracker">The reference to the cancellation tracker
        /// object.</param>
        /// <returns>Result of the response GP route.</returns>
        private GPRouteResult _LoadResultData(
            IVrpRestService client,
            GetVrpJobResultResponse resp,
            ICancelTracker tracker)
        {
            Debug.Assert(client != null);
            Debug.Assert(resp != null);

            // get output routes
            _CheckCancelState(tracker);
            var routesParam = _GetGPObject<GPFeatureRecordSetLayerParam>(
                client,
                resp.JobId,
                resp.Outputs.Routes.ParamUrl);

            // get output stops
            _CheckCancelState(tracker);
            var stopsParam = _GetGPObject<GPRecordSetParam>(
                client,
                resp.JobId,
                resp.Outputs.Stops.ParamUrl);

            // get unassigned orders
            _CheckCancelState(tracker);
            var violatedStopsParam = _GetGPObject<GPRecordSetParam>(
                client,
                resp.JobId,
                resp.Outputs.ViolatedStops.ParamUrl);

            // Get directions.
            _CheckCancelState(tracker);
            var directionsParam = _GetGPObject<GPFeatureRecordSetLayerParam>(
                client,
                resp.JobId,
                resp.Outputs.Directions.ParamUrl);

            GPRouteResult result = new GPRouteResult();
            result.Routes = routesParam.Value;
            result.Stops = stopsParam.Value;
            result.ViolatedStops = violatedStopsParam.Value;
            result.Directions = directionsParam.Value;

            return result;
        }

        private GetVrpJobResultResponse _SubmitJob(
            IVrpRestService client,
            SubmitVrpJobRequest request)
        {
            Debug.Assert(client != null);
            Debug.Assert(request != null);

            GetVrpJobResultResponse response = null;
            try
            {
                response = client.SubmitJob(request);
            }
            catch (RestException e)
            {
                _LogServiceError(e);
                throw SolveHelper.ConvertServiceException(
                    Properties.Messages.Error_SubmitJobFailed, e);
            }

            return response;
        }

        private GetVrpJobResultResponse _GetJobResult(
            IVrpRestService client,
            string jobId)
        {
            Debug.Assert(client != null);
            Debug.Assert(jobId != null);

            GetVrpJobResultResponse response = null;
            try
            {
                response = client.GetJobResult(jobId);
            }
            catch (RestException e)
            {
                _LogServiceError(e);
                throw SolveHelper.ConvertServiceException(
                    String.Format(Properties.Messages.Erorr_GetJobResult, jobId), e);
            }

            return response;
        }

        private T _GetGPObject<T>(
            IVrpRestService client,
            string jobId,
            string objectUrl)
            where T : IFaultInfo
        {
            Debug.Assert(client != null);
            Debug.Assert(jobId != null);
            Debug.Assert(objectUrl != null);

            var result = default(T);
            try
            {
                result = client.GetGPObject<T>(jobId, objectUrl);
            }
            catch (RestException e)
            {
                _LogServiceError(e);
                throw SolveHelper.ConvertServiceException(
                    String.Format(Properties.Messages.Error_GetJobParameter, jobId), e);
            }

            return result;
        }

        private void _CheckLicensePermissions()
        {
            // check if license activated
            if (Licenser.ActivatedLicense == null)
            {
                throw new LicenseException(LicenseError.LicenseNotActivated,
                    Properties.Messages.Error_LicenseNotActivated);
            }

            // check license permissions
            if (Licenser.ActivatedLicense.IsRestricted)
            {
                // routes permission (max routes per schedule)
                _CheckRoutesLicPermission();

                // orders permission (max number of orders planned on schedule's date)
                _CheckOrdersLicPermission();
            }
        }

        private void _CheckRoutesLicPermission()
        {
            int routesCount = _schedule.Routes.Count;
            int? maxRoutesCount = Licenser.ActivatedLicense.PermittedRouteNumber;
            if (maxRoutesCount != null && routesCount > maxRoutesCount)
            {
                throw new LicenseException(LicenseError.MaxRoutesPermission,
                    String.Format(Properties.Messages.Error_MaxRoutesPermission,
                        maxRoutesCount,
                        routesCount));
            }
        }

        private void _CheckOrdersLicPermission()
        {
            int dayOrders = _context.Project.Orders.GetCount(
                (DateTime)_schedule.PlannedDate);

            int? maxOrdersCount = Licenser.ActivatedLicense.PermittedOrderNumber;
            if (maxOrdersCount != null && dayOrders > maxOrdersCount)
            {
                throw new LicenseException(LicenseError.MaxOrdersPermission,
                    String.Format(Properties.Messages.Error_MaxOrdersPermission,
                        maxOrdersCount,
                        dayOrders));
            }
        }

        private void _CancelJob(IVrpRestService client, string jobId)
        {
            Debug.Assert(client != null);
            Debug.Assert(jobId != null);

            try
            {
                client.CancelJob(jobId);
            }
            catch (Exception e)
            {
                Logger.Error(String.Format(LOG_JOB_CANCEL_ERROR, jobId, e.Message));
                Logger.Error(e);
            }
        }

        private static int _GetSolveHR(VrpResultsResponse resp)
        {
            Debug.Assert(resp != null);

            // check "succeeded" flag
            int hr = HResult.E_FAIL;
            if (!resp.SolveSucceeded)
            {
                // solve is not succeeded, try to determine exact HRESULT code
                // by response messages
                JobMessage[] messages = resp.Messages;
                if (messages != null)
                {
                    // process messages
                    if (!SolveErrorParser.ParseErrorCode(messages, out hr))
                    {
                        // cannot determine HRESULT
                        hr = HResult.E_FAIL;
                        Logger.Warning(LOG_UNDETERMINED_SOLVE_HR);
                    }
                }
            }
            else
            {
                // solve is succeeded, HRESULT = SO_OK
                hr = HResult.S_OK;
            }

            return hr;
        }

        private void _ValidateReqData(SolveRequestData reqData)
        {
            Debug.Assert(reqData != null);

            // check routes count
            if (reqData.Routes.Count < 1)
                throw new RouteException(Properties.Messages.Error_InvalidRoutesNum);

            // validate objects
            var invalidObjects = new List<DataObject>();
            invalidObjects.AddRange(_ValidateObjects<Order>(reqData.Orders));
            invalidObjects.AddRange(_ValidateObjects<Route>(reqData.Routes));

            if (invalidObjects.Count > 0)
            {
                throw new RouteException(Properties.Messages.Error_InvalidScheduleData,
                    invalidObjects.ToArray());
            }
        }

        private SolveResult _CreateSolveResult(VrpResult vrpResult, List<Violation> violations)
        {
            Debug.Assert(vrpResult != null);

            // convert job messages
            var msgList = new List<ServerMessage>();
            if (vrpResult.Messages != null)
            {
                foreach (JobMessage msg in vrpResult.Messages)
                    msgList.Add(_ConvertJobMessage(msg));
            }

            // violations
            List<Violation> vltList = violations;
            if (vltList == null)
                vltList = new List<Violation>();

            // status
            bool isSucceeded = IsSolveSucceeded(vrpResult.SolveHR);

            return new SolveResult(msgList.ToArray(), vltList.ToArray(),
                !isSucceeded);
        }

        private VrpOperation _GetRestrictedOrdersOperation(
            List<Order> restrictedOrders,
            List<Violation> violations)
        {
            Debug.Assert(restrictedOrders != null);

            SolveRequestData reqData = this.RequestData;

            var newOrders = new List<Order>();
            foreach (Order order in reqData.Orders)
            {
                if (!restrictedOrders.Contains(order))
                    newOrders.Add(order);
            }

            VrpOperation operation = null;
            if (newOrders.Count > 0)
            {
                reqData.Orders = newOrders;
                operation = CreateOperation(reqData, violations);
            }

            return operation;
        }

        private List<Order> _GetRestrictedOrders(List<Violation> violations)
        {
            Debug.Assert(violations != null);

            ICollection<Order> curOrders = this.RequestData.Orders;

            var list = new List<Order>();
            foreach (Violation violation in violations)
            {
                if (violation.ViolationType == ViolationType.TooFarFromRoad ||
                    violation.ViolationType == ViolationType.RestrictedStreet)
                {
                    if (violation.AssociatedObject != null &&
                        violation.AssociatedObject is Order)
                    {
                        Order restrictedOrder = violation.AssociatedObject as Order;
                        if (curOrders.Contains(restrictedOrder))
                            list.Add(restrictedOrder);
                    }
                }
            }

            return list;
        }

        private static bool _HasRestrictedDepots(List<Violation> violations)
        {
            Debug.Assert(violations != null);

            bool res = false;
            foreach (Violation violation in violations)
            {
                if (violation.ViolationType == ViolationType.TooFarFromRoad ||
                    violation.ViolationType == ViolationType.RestrictedStreet)
                {
                    if (violation.AssociatedObject != null &&
                        violation.AssociatedObject is Location)
                    {
                        res = true;
                        break;
                    }
                }
            }

            return res;
        }

        private static void _ValidateResponse(GetVrpJobResultResponse resp)
        {
            Debug.Assert(resp != null);

            if (resp.Outputs == null ||
                resp.Outputs.Routes == null ||
                resp.Outputs.Stops == null)
            {
                throw new RouteException(Properties.Messages.Error_InvalidResultJobResponse);
            }
        }

        private static VrpResultsResponse _GetVrpResults(
            SyncVrpResponse response)
        {
            Debug.Assert(response != null);

            var vrpResponse = new VrpResultsResponse()
            {
                RouteResult = new GPRouteResult(),
                JobID = SYNC_JOB_ID,
            };

            var results = vrpResponse.RouteResult;

            var layer = default(GPFeatureRecordSetLayer);
            _GetVrpObject<GPFeatureRecordSetLayer>(
                response,
                SyncVrpResponse.ParamRoutes,
                out layer);
            results.Routes = layer;

            var recordSet = default(GPRecordSet);
            _GetVrpObject<GPRecordSet>(response, SyncVrpResponse.ParamStops, out recordSet);
            results.Stops = recordSet;

            _GetVrpObject<GPRecordSet>(
                response,
                SyncVrpResponse.ParamUnassignedOrders,
                out recordSet);
            results.ViolatedStops = recordSet;

            _GetVrpObject<GPFeatureRecordSetLayer>(
                response,
                SyncVrpResponse.ParamDirections,
                out layer);
            results.Directions = layer;

            // solve result flag
            bool solveRes = false;
            if (_GetVrpObject<bool>(response, SyncVrpResponse.ParamSucceeded, out solveRes))
                vrpResponse.SolveSucceeded = solveRes;
            else
                vrpResponse.SolveSucceeded = false;

            // messages
            vrpResponse.Messages = response.Messages;

            vrpResponse.SolveHR = _GetSolveHR(vrpResponse);

            return vrpResponse;
        }

        private static bool _GetVrpObject<T>(SyncVrpResponse response, string paramName, out T obj)
        {
            Debug.Assert(response != null);
            Debug.Assert(paramName != null);

            obj = default(T);

            bool res = false;
            if (response.Objects != null)
            {
                foreach (GPParamObject gpParam in response.Objects)
                {
                    if (!String.IsNullOrEmpty(gpParam.paramName) &&
                        gpParam.paramName.Equals(paramName, StringComparison.OrdinalIgnoreCase))
                    {
                        obj = (T)gpParam.value;
                        res = true;
                        break;
                    }
                }
            }

            return res;
        }

        private static void _ValidateVrpResults(VrpResultsResponse resp)
        {
            Debug.Assert(resp != null);

            var results = resp.RouteResult;
            var invalidResults =
                results == null ||
                results.Routes == null ||
                results.Stops == null;
            if (invalidResults)
            {
                throw new RouteException(Properties.Messages.Error_InvalidResultJobResponse);
            }
        }

        private static ServerMessage _ConvertJobMessage(JobMessage msg)
        {
            Debug.Assert(msg != null);

            var type = ServerMessageType.Info;
            if (msg.Type == NAJobMessageType.esriJobMessageTypeError)
                type = ServerMessageType.Error;
            else if (msg.Type == NAJobMessageType.esriJobMessageTypeWarning)
                type = ServerMessageType.Warning;

            return new ServerMessage(type, msg.Description);
        }

        private static List<DataObject> _ValidateObjects<T>(ICollection<T> objects)
            where T : DataObject
        {
            var invalidObjects = new List<DataObject>();
            foreach (T obj in objects)
            {
                if (!String.IsNullOrEmpty(obj.PrimaryError))
                    invalidObjects.Add(obj);
            }

            return invalidObjects;
        }

        private static void _CheckCancelState(ICancelTracker tracker)
        {
            if (tracker != null && tracker.IsCancelled)
                throw new UserBreakException();
        }

        private static void _LogServiceError(RestException ex)
        {
            var error = new GPError();
            error.Message = ex.Message;
            error.Code = ex.ErrorCode;
            error.Details = ex.Details;

            Logger.Error(_BuildFaultResponseLog(error));
        }

        private static void _LogResponse(GetJobResultResponse resp)
        {
            try
            {
                string log = null;
                if (Logger.IsSeverityEnabled(TraceEventType.Information))
                {
                    if (resp.IsFault)
                    {
                        GPError error = resp.FaultInfo;
                        if (error != null)
                            log = _BuildFaultResponseLog(error);
                    }
                    else
                        log = _BuildResponseLog(resp);

                    if (log != null)
                        Logger.Info(log);
                }
            }
            catch { }
        }

        private static string _BuildResponseLog(GetJobResultResponse resp)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append("jobId: ");
            sb.AppendLine(resp.JobId);
            sb.Append("jobStatus: ");
            sb.AppendLine(resp.JobStatus);
            sb.Append(Environment.NewLine);

            if (resp.Messages != null)
            {
                foreach (JobMessage msg in resp.Messages)
                {
                    sb.Append(msg.Type);
                    sb.AppendLine(": ");
                    sb.AppendLine(msg.Description);
                    sb.AppendLine("");
                }
            }

            return sb.ToString();
        }

        private static string _BuildFaultResponseLog(GPError error)
        {
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append("code: ");
            sb.AppendLine(error.Code.ToString());
            sb.Append("message: ");
            sb.AppendLine(error.Message);
            sb.AppendLine("details:");

            if (error.Details != null)
            {
                foreach (string msg in error.Details)
                    sb.AppendLine(msg);
            }

            return sb.ToString();
        }

        private static void _LogJobError(string jobId, Exception e)
        {
            Logger.Error(String.Format(LOG_JOB_ERROR, jobId, e.Message));
            Logger.Error(e);
        }

        private void _LogJobStatistics(string jobId,
            SolveStatistics statistics)
        {
            Logger.Info(String.Format(LOG_JOB_STATISTICS, this.OperationType,
                jobId,
                statistics.RequestTime.TotalSeconds));
        }

        #endregion private methods

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // retry delay (milliseconds)
        private const int RETRY_DELAY = 3000;

        // log messages
        private const string LOG_JOB_ERROR = "Failed to send request for job id={0}: {1}";
        private const string LOG_JOB_STATISTICS = "Statistics for {0} (job id={1}):\nSolve and data transfer time: {2} seconds";
        private const string LOG_JOB_CANCEL_ERROR = "Failed to cancel job id={0}: {1}";
        private const string LOG_UNDETERMINED_SOLVE_HR = "Cannot determine solve HRESULT code by response messages.";

        /// <summary>
        /// The string to be used as job ID for the synchronous requests.
        /// </summary>
        private const string SYNC_JOB_ID = "sync-service";

        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SolverContext _context;
        private Schedule _schedule;
        private SolveOptions _options;

        private VrpOperation _nextStep;
        private GPFeature[] _pointBarriers;
        private GPFeature[] _polygonBarriers;
        private GPFeature[] _polylineBarriers;

        #endregion private fields
    }
}
