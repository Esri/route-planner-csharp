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
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using ESRI.ArcLogistics.Routing.Json;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides common facilities for using ArcGIS REST API.
    /// </summary>
    internal class RestService
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the RestService class.
        /// </summary>
        public RestService()
            : this(s => s)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RestService class with the specified
        /// result preprocessor.
        /// </summary>
        /// <param name="resultPreprocessor">The delegate to be used for
        /// preprocessing results of all REST requests.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="resultPreprocessor"/> argument is a null reference.</exception>
        public RestService(Func<string, string> resultPreprocessor)
        {
            if (resultPreprocessor == null)
            {
                throw new ArgumentNullException("resultPreprocessor");
            }

            _resultPreprocessor = resultPreprocessor;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Sends REST request to the specified url.
        /// </summary>
        /// <typeparam name="T">The type of the request result.</typeparam>
        /// <param name="context">The reference to the rest request context object.</param>
        /// <param name="url">The url to send request to.</param>
        /// <param name="query">The query to be sent.</param>
        /// <param name="opt">The reference to the request sending options.</param>
        /// <returns>Result of the specified REST request.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.Routing.RestException">error was
        /// returned by the REST API.</exception>
        /// <exception cref="T:ESRI.ArcLogistics.CommunicationException">failed
        /// to communicate with the REST service at the specified url.</exception>
        /// <exception cref="T:System.ArgumentNullException">Any of
        /// <paramref name="context"/>, <paramref name="url"/>,
        /// <paramref name="query"/> or <paramref name="opt"/> arguments is a null
        /// reference.</exception>
        public T SendRequest<T>(
            IRestRequestContext context,
            string url,
            string query,
            HttpRequestOptions opt)
        {
            Debug.Assert(context != null);
            Debug.Assert(!string.IsNullOrEmpty(url));
            Debug.Assert(!string.IsNullOrEmpty(query));
            Debug.Assert(opt != null);

            var sendRequestWrapper = new RetriableInvocationWrapper(
                MAX_RETRY_COUNT,
                (e) => _PrepareRetry(context, e),
                (e) => _TranslateException(context, e));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            return sendRequestWrapper.Invoke(() =>
            {
                var timeout = opt.Timeout - stopwatch.ElapsedMilliseconds;
                timeout = Math.Max(timeout, 0);
                opt.Timeout = (int)timeout;

                var response = _SendRequest<T>(
                    context,
                    url,
                    query,
                    opt);
                RestHelper.ValidateResponse(response);

                return response;
            });
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Checks if the specified exception denotes a transient error so
        /// request sending could be retried.
        /// </summary>
        /// <param name="exception">The reference to the exception object
        /// to be checked.</param>
        /// <returns>True if and only if the exception was considered transient.</returns>
        private static bool _IsTransientError(Exception exception)
        {
            var webException = exception as WebException;
            if (webException != null)
            {
                if (!WebHelper.IsTransientError(webException))
                {
                    return false;
                }

                return true;
            }

            var serializationException = exception as SerializationException;
            if (serializationException != null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to prepare for retrying request sending upon catching the
        /// specified exception.
        /// </summary>
        /// <param name="context">Request context to be used for token generation.</param>
        /// <param name="exception">The exception caused failure of the previous
        /// request sending.</param>
        /// <returns>True if and only if the request sending could be repeated.</returns>
        private static bool _PrepareRetry(
            IRestRequestContext context,
            Exception exception)
        {
            var restException = exception as RestException;
            if (restException != null)
            {
                if (restException.ErrorCode == AgsConst.ARCGIS_EXPIRED_TOKEN)
                {
                    context.Connection.GenerateToken();

                    return true;
                }

                return false;
            }

            if (_IsTransientError(exception))
            {
                Thread.Sleep(RETRY_WAIT_TIME);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Translates the specified exception into the more application-specific
        /// one.
        /// </summary>
        /// <param name="context">Context of the failed request.</param>
        /// <param name="exception">The exception to be translated.</param>
        /// <returns>Translated exception or null reference if the exception
        /// cannot be translated.</returns>
        private static Exception _TranslateException(
            IRestRequestContext context,
            Exception exception)
        {
            if (ServiceHelper.IsCommunicationError(exception) ||
                exception is SerializationException)
            {
                var serverName = context.Connection.Title;
                return ServiceHelper.CreateCommException(serverName, exception);
            }

            return null;
        }
        #endregion

        #region private methods
        private T _SendRequest<T>(
            IRestRequestContext context,
            string url,
            string query,
            HttpRequestOptions opt)
        {
            String queryWithToken = "";

            // Add authentication token to the request.
            if (context.Connection.RequiresTokens)
            {
                var requestUri = new Uri(url);
                var tokenCookie = new Cookie(
                    TOKEN_COOKIE_NAME,
                    context.Connection.LastToken,
                    TOKEN_COOKIE_PATH,
                    requestUri.Host);

                // Adding a cookie will replace existing one (if any) with the same name, path
                // and domain using case-insensitive comparison.
                opt.CookieContainer.Add(requestUri, tokenCookie);

                // 10.1 routing/VRP services need the "token=..." in the query string
                queryWithToken = AgsHelper.FormatTokenQuery(query, context.Connection.LastToken);
            }
            else
                queryWithToken = query;

            // Add session cookie to the request so all requests in the session would be processed
            // by the same server.
            if (context.SessionCookie != null)
            {
                opt.CookieContainer.Add(context.SessionCookie);
            }

            var responseInfo = default(HttpResponseInfo);
            string json = WebHelper.SendRequest(url, queryWithToken, opt, out responseInfo);
            json = _resultPreprocessor(json);

            // Get session cookie from the response. When this cookie is sent in the request
            // we might get no cookie in the response, so we update session cookie only when
            // it's not null.
            var sessionCookie = responseInfo.Cookies[ELB_SESSION_COOKIE_NAME];
            if (sessionCookie != null)
            {
                context.SessionCookie = sessionCookie;
            }

            return JsonSerializeHelper.DeserializeResponse<T>(json, context.KnownTypes);
        }
        #endregion private methods

        #region private constants
        /// <summary>
        /// Specifies the maximum number of request sending retries to be attempted.
        /// </summary>
        private const int MAX_RETRY_COUNT = 4;

        /// <summary>
        /// Time in milliseconds to wait before retrying request upon connection
        /// failure.
        /// </summary>
        private const int RETRY_WAIT_TIME = 500;

        /// <summary>
        /// Name of the cookie storing authentication token.
        /// </summary>
        private const string TOKEN_COOKIE_NAME = "agstoken";

        /// <summary>
        /// Name of the cookie storing ELB session information.
        /// </summary>
        private const string ELB_SESSION_COOKIE_NAME = "AWSELB";

        /// <summary>
        /// Path of the cookie storing authentication token.
        /// </summary>
        private const string TOKEN_COOKIE_PATH = "/";
        #endregion

        #region private fields
        /// <summary>
        /// The delegate to be used for preprocessing results of REST requests.
        /// </summary>
        private Func<string, string> _resultPreprocessor;
        #endregion
    }
}
