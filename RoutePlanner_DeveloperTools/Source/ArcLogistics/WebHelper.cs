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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// HttpMethod enumeration.
    /// </summary>
    internal enum HttpMethod
    {
        Get,
        Post
    }

    /// <summary>
    /// HttpRequestOptions class.
    /// </summary>
    internal class HttpRequestOptions
    {
        // default request timeout (milliseconds)
        private const int DEFAULT_REQ_TIMEOUT = 2 * 60 * 1000;

        public HttpRequestOptions()
        {
            this.Method = HttpMethod.Get;
            this.UseGZipEncoding = false;
            this.Timeout = DEFAULT_REQ_TIMEOUT;
            this.CookieContainer = new CookieContainer();
        }

        public HttpMethod Method { get; set; }
        public bool UseGZipEncoding { get; set; }
        public int Timeout { get; set; }

        /// <summary>
        /// Gets or sets a container for HTTP request cookies.
        /// </summary>
        public CookieContainer CookieContainer
        {
            get;
            set;
        }
    }

    /// <summary>
    /// HttpResponseInfo class.
    /// </summary>
    internal class HttpResponseInfo
    {
        public Uri ResponseUri { get; set; }

        /// <summary>
        /// Gets or sets a collection of cookies received in the HTTP response.
        /// </summary>
        public CookieCollection Cookies
        {
            get;
            set;
        }
    }

    /// <summary>
    /// WebHelper class.
    /// </summary>
    internal class WebHelper
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string ENCODING_GZIP = "gzip";

        #endregion constants

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static string SendRequest(string url, string query,
            HttpRequestOptions opt)
        {
            HttpResponseInfo info = null;
            return SendRequest(url, query, opt, out info);
        }

        public static string SendRequest(string url, string query,
            HttpRequestOptions opt,
            out HttpResponseInfo info)
        {
            Debug.Assert(opt != null);

            string result = null;
            while (true)
            {
                try
                {
                    if (opt.Method == HttpMethod.Post)
                        result = _SendPostRequest(url, query, opt, out info);
                    else
                        result = _SendRequest(url, query, opt, out info);

                    break;
                }
                catch (Exception e)
                {
                    if (!ProxyAuthenticationErrorHandler.HandleError(e))
                    {
                        throw;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the specified web exception denotes transient error.
        /// </summary>
        /// <param name="exception">The exception object to be checked.</param>
        /// <returns>True if and only if the exception denotes transient error.</returns>
        public static bool IsTransientError(WebException exception)
        {
            var response = exception.Response as HttpWebResponse;
            if (response != null)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.InternalServerError:
                        return true;

                    case HttpStatusCode.ServiceUnavailable:
                        return true;

                    default:
                        return false;
                }
            }

            switch (exception.Status)
            {
                case WebExceptionStatus.ConnectionClosed:
                    return true;

                case WebExceptionStatus.KeepAliveFailure:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the reason of web exception is Bad service URL.
        /// </summary>
        /// <param name="exception">Web Exception to check out.</param>
        /// <returns>True if the reason of exception is Bad URL, otherwise false.</returns>
        public static bool IsBadURLError(WebException exception)
        {
            var response = exception.Response as HttpWebResponse;

            // Detect error for non-secure HTTP connection.
            if (response != null && response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                return true;
            }

            // Detect error for secure HTTP connection.
            if (exception.Status == WebExceptionStatus.TrustFailure)
            {
                return true;
            }

            return false;
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static string _SendRequest(string url, string query,
            HttpRequestOptions opt,
            out HttpResponseInfo info)
        {
            Debug.Assert(url != null);
            Debug.Assert(query != null);
            Debug.Assert(opt != null);

            var req = (HttpWebRequest)WebRequest.Create(_BuildUrl(url, query));

            _SetRequestOptions(req, opt);
            return _GetResponse(req, out info);
        }

        private static string _SendPostRequest(string url, string query,
            HttpRequestOptions opt,
            out HttpResponseInfo info)
        {
            Debug.Assert(url != null);
            Debug.Assert(query != null);
            Debug.Assert(opt != null);

            byte[] data = Encoding.UTF8.GetBytes(query);

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;

            _SetRequestOptions(req, opt);

            string result = null;

            Stream reqStream = req.GetRequestStream();
            try
            {
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
                reqStream = null;

                result = _GetResponse(req, out info);
            }
            finally
            {
                if (reqStream != null)
                    reqStream.Close();
            }

            return result;
        }

        private static string _ReadData(WebResponse resp)
        {
            return _IsGZipContent(resp) ? _ReadGZipContent(resp) :
                _ReadTextContent(resp);
        }

        private static bool _IsGZipContent(WebResponse resp)
        {
            string encoding = ((HttpWebResponse)resp).ContentEncoding;
            return (encoding != null &&
                encoding.Equals(ENCODING_GZIP, StringComparison.InvariantCultureIgnoreCase));
        }

        private static string _ReadGZipContent(WebResponse resp)
        {
            string data = null;
            using (GZipStream zip = new GZipStream(resp.GetResponseStream(),
                CompressionMode.Decompress))
            {
                data = _ReadBinaryData(zip);
            }

            return data;
        }

        private static string _ReadTextContent(WebResponse resp)
        {
            string data = null;
            using (StreamReader reader = new StreamReader(
                resp.GetResponseStream()))
            {
                data = reader.ReadToEnd();
            }

            return data;
        }

        private static string _ReadBinaryData(Stream stream)
        {
            string result = null;
            using (MemoryStream ms = new MemoryStream())
            {

                byte[] buffer = new byte[1024];
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    ms.Write(buffer, 0, bytesRead);
                }

                result = Encoding.UTF8.GetString(ms.ToArray());
            }

            return result;
        }

        private static string _BuildUrl(string url, string query)
        {
            UriBuilder uriBuilder = new UriBuilder(url);
            uriBuilder.Query = query;

            return uriBuilder.ToString();
        }

        /// <summary>
        /// Gets response for the specified HTTP request.
        /// </summary>
        /// <param name="request">Request instance to get response for.</param>
        /// <param name="info">Response info to filled with HTTP response data.</param>
        /// <returns>Data contained in the HTTP response.</returns>
        private static string _GetResponse(
            HttpWebRequest request,
            out HttpResponseInfo info)
        {
            string result = null;
            using (var response = request.GetResponse())
            {
                result = _ReadData(response);

                var httpResponse = (HttpWebResponse)response;
                info = new HttpResponseInfo
                {
                    ResponseUri = httpResponse.ResponseUri,
                    Cookies = httpResponse.Cookies,
                };
            }

            return result;
        }

        /// <summary>
        /// Sets common options for the HTTP request.
        /// </summary>
        /// <param name="request">Request to set options for.</param>
        /// <param name="opt">Options to be set for the request.</param>
        private static void _SetRequestOptions(
            HttpWebRequest request,
            HttpRequestOptions opt)
        {
            request.Timeout = opt.Timeout;
            request.CookieContainer = opt.CookieContainer;

            if (opt.UseGZipEncoding)
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, ENCODING_GZIP);

            request.Referer = AgsHelper.RefererValue;
        }
        #endregion private methods
    }
}
