using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// RestVrpService class.
    /// </summary>
    internal class RestVrpService : IVrpRestService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RestVrpService class.
        /// </summary>
        /// <param name="syncContext">REST request context instance to be used for
        /// communicating with the syncronous VRP rest service.</param>
        /// <param name="asyncContext">REST request context instance to be used for
        /// communicating with the asyncronous VRP rest service.</param>
        /// <param name="soapUrl">SOAP GP service url.</param>
        public RestVrpService(
            RestServiceContext syncContext,
            RestServiceContext asyncContext,
            string soapUrl)
        {
            Debug.Assert(asyncContext != null);
            Debug.Assert(!string.IsNullOrEmpty(soapUrl));

            // synchronous context could be null if synchronous VRP support
            // was disabled.
            _syncContext = syncContext;

            // SyncVrpResponse contains array of objects of different types.
            // WCF DataContractJsonSerializer cannot deserialize such objects
            // without special type hints, so we do some pre-processing of
            // original JSON string.
            _syncRestService = new RestService(JsonProcHelper.AddJsonTypeInfo);

            _context = asyncContext.Context;
            _soapGPService = new GPServiceClient(
                soapUrl,
                _context.Connection);
            _baseUrl = asyncContext.Url;
            _restService = new RestService();
        }
        #endregion constructors

        #region IVrpRestService Members
        /// <summary>
        /// Executes VRP job synchronously.
        /// </summary>
        /// <param name="request">The reference to the request object to be
        /// send to the server.</param>
        /// <returns>Result of the synchronous job execution.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        public SyncVrpResponse ExecuteJob(SubmitVrpJobRequest request)
        {
            Debug.Assert(request != null);
            Debug.Assert(_syncContext != null);

            var url = UriHelper.Concat(_syncContext.Url, QUERY_OBJ_EXECUTE_TASK);
            var query = RestHelper.BuildQueryString(
                request,
                _syncContext.Context.KnownTypes,
                false);

            var options = new HttpRequestOptions()
            {
                Method = HttpMethod.Post,
                UseGZipEncoding = true,
                Timeout = EXEC_TASK_TIMEOUT,
            };

            return _syncRestService.SendRequest<SyncVrpResponse>(
                _syncContext.Context,
                url,
                query,
                options);
        }

        /// <summary>
        /// Submits job to the VRP service.
        /// </summary>
        /// <param name="request">Request describing job to be submitted.</param>
        /// <returns>Result of the job submitting.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        public GetVrpJobResultResponse SubmitJob(SubmitVrpJobRequest request)
        {
            Debug.Assert(request != null);

            var url = UriHelper.Concat(_baseUrl, QUERY_OBJ_SUBMIT_JOB);
            var query = RestHelper.BuildQueryString(
                request,
                _context.KnownTypes,
                false);

            // log request
            Logger.Info(String.Format(MSG_SUBMIT_JOB, request.OperationType,
                request.OperationDate.ToString("d"),
                query));

            HttpRequestOptions opt = new HttpRequestOptions();
            opt.Method = HttpMethod.Post;
            opt.UseGZipEncoding = true;
            opt.Timeout = DEFAULT_REQ_TIMEOUT;

            return _restService.SendRequest<GetVrpJobResultResponse>(
                _context,
                url,
                query,
                opt);
        }

        public GetVrpJobResultResponse GetJobResult(string jobId)
        {
            Debug.Assert(jobId != null);

            var getJobResult = string.Format(
                CultureInfo.InvariantCulture,
                URL_JOB_STATUS,
                jobId);
            var url = UriHelper.Concat(_baseUrl, getJobResult);

            StringBuilder sb = new StringBuilder();
            RestHelper.AddQueryParam(QUERY_FORMAT, NAOutputFormat.JSON, sb, true);

            string query = sb.ToString();

            HttpRequestOptions opt = new HttpRequestOptions();
            opt.Method = HttpMethod.Get;
            opt.UseGZipEncoding = true;
            opt.Timeout = DEFAULT_REQ_TIMEOUT;

            return _restService.SendRequest<GetVrpJobResultResponse>(
                _context,
                url,
                query,
                opt);
        }

        public T GetGPObject<T>(
            string jobId,
            string objectUrl)
        {
            Debug.Assert(jobId != null);
            Debug.Assert(objectUrl != null);

            var getObject = string.Format(
                CultureInfo.InvariantCulture,
                URL_GP_OBJECT,
                jobId,
                objectUrl);
            var url = UriHelper.Concat(_baseUrl, getObject);

            StringBuilder sb = new StringBuilder();
            RestHelper.AddQueryParam(QUERY_FORMAT, NAOutputFormat.JSON, sb, true);

            string query = sb.ToString();

            HttpRequestOptions opt = new HttpRequestOptions();
            opt.Method = HttpMethod.Get;
            opt.UseGZipEncoding = true;
            opt.Timeout = DEFAULT_REQ_TIMEOUT;

            return _restService.SendRequest<T>(_context, url, query, opt);
        }

        /// <summary>
        /// Cancels VRP job with the specified ID.
        /// </summary>
        /// <param name="jobId">ID of the job to be cancelled.</param>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST VRP service.</exception>
        public void CancelJob(string jobId)
        {
            _soapGPService.CancelJob(jobId);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Closes VRP service client.
        /// </summary>
        public void Dispose()
        {
            _soapGPService.Close();
        }
        #endregion

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // URL query objects
        private const string QUERY_OBJ_SUBMIT_JOB = "submitJob";

        /// <summary>
        /// Name of the synchronous task.
        /// </summary>
        private const string QUERY_OBJ_EXECUTE_TASK = "execute";

        // URL templates
        private const string URL_JOB_STATUS = "jobs/{0}";
        private const string URL_GP_OBJECT = "jobs/{0}/{1}";

        // URL query parameters
        private const string QUERY_FORMAT = "f";

        // log messages
        private const string MSG_SUBMIT_JOB = "Request to the VRP service for {0} operation on {1}:\n{2}";

        // timeouts (milliseconds)
        private const int DEFAULT_REQ_TIMEOUT = 10 * 60 * 1000;

        /// <summary>
        /// Timeout in milliseconds for the "execute" task.
        /// </summary>
        private const int EXEC_TASK_TIMEOUT = 10 * 60 * 1000;
        #endregion

        #region private constants
        /// <summary>
        /// Request context to be used for communicating with the VRP service.
        /// </summary>
        private IRestRequestContext _context;

        /// <summary>
        /// SOAP GP service instance to be used for jobs cancellation.
        /// </summary>
        private GPServiceClient _soapGPService;

        /// <summary>
        /// Url to the VRP REST service.
        /// </summary>
        private string _baseUrl;

        /// <summary>
        /// The reference to the rest service object to be used for sending requests.
        /// </summary>
        private RestService _restService;

        /// <summary>
        /// The reference to the context object for synchronous VRP service.
        /// </summary>
        private RestServiceContext _syncContext;

        /// <summary>
        /// The reference to the rest service object to be used for sending requests
        /// to the synchronous GP service.
        /// </summary>
        private RestService _syncRestService;
        #endregion
    }
}
