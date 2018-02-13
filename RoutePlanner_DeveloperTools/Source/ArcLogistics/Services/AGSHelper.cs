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
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.Services
{
    /// <summary>
    /// Structure with token and time of its expiration.
    /// </summary>
    internal struct Token
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="value">Token string.</param>
        /// <param name="expires">Token expiration date.</param>
        internal Token(string value, DateTime expires)
        {
            _value = value;
            _expires = expires;
        }

        #region Properties

        /// <summary>
        /// Token string.
        /// </summary>
        public string Value { get { return _value; } }

        /// <summary>
        /// Token expiration date.
        /// </summary>
        public DateTime Expires { get { return _expires; } }

        #endregion

        #region Private fields 
        
        /// <summary>
        /// Token string.
        /// </summary>
        private string _value;

        /// <summary>
        /// Token expiration date.
        /// </summary>
        private DateTime _expires;

        #endregion
    }

    /// <summary>
    /// AgsHelper class.
    /// </summary>
    internal class AgsHelper
    {
        #region Public static properties

        #endregion

        /// <summary>
        /// Value of the "referer" parameter.
        /// </summary>
        public static string RefererValue
        {
            get
            {
                return REFERER_VALUE;
            }
        }

        /// <summary>
        /// Name of the "referer" parameter.
        /// </summary>
        public static string RefererParameterName
        {
            get
            {
                return REFERER_PARAMETER_NAME;
            }
        }

        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // query parameters
        private const string QUERY_TOKEN = "token";
        private const string QUERY_REQUEST = "request";
        private const string QUERY_GETTOKEN = "gettoken";
        private const string QUERY_USERNAME = "username";
        private const string QUERY_PASSWORD = "password";

        /// <summary>
        /// String to send as referer.
        /// </summary>
        private const string REFERER_VALUE = @"Route Planner";

        /// <summary>
        /// Strings for parsing server response.
        /// </summary>
        private const string TOKEN = @"token";
        private const string EXPIRES = @"expires";

        /// <summary>
        /// Key which must be in error response.
        /// </summary>
        private const string RESPONSE_ERROR = @"error";

        /// <summary>
        /// Key for getting response error details.
        /// </summary>
        private const string RESPONSE_DETAILS = @"details";

        /// <summary>
        /// Name for parameter responding for query format.
        /// </summary>
        private static string QUERY_FORMAT = @"f";

        /// <summary>
        /// JSON format name parameter value.
        /// </summary>
        private static string JSON_FORMAT = @"json";

        /// <summary>
        /// Name for parameter responding for token expiration.
        /// </summary>
        private const string QUERY_EXPIRATION = @"expiration";

        /// <summary>
        /// Name for parameter responding for client.
        /// </summary>
        private const string QUERY_CLIENT = @"client";

        /// <summary>
        /// Name for parameter responding for referer.
        /// </summary>
        public const string REFERER_PARAMETER_NAME = @"referer";

        /// <summary>
        /// Token expiration time in minutes.
        /// </summary>
        private const string EXPIRATION_IN_MINUTES = @"30";

        // token URL pattern
        private const string TOKEN_URL_FMT = "{0}?token={1}";

        // request timeout (milliseconds)
        private const int GET_TOKEN_TIMEOUT = 2 * 60 * 1000;

        /// <summary>
        /// Start date in the Unix time stamp.
        /// </summary>
        private static DateTime UNIX_START_DATE = new DateTime(1970, 1, 1, 0, 0, 0, 0, 
            DateTimeKind.Utc);

        #endregion constants

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Generates ArcGIS online server token.
        /// </summary>
        /// <param name="tokenServiceURL">
        /// Token service URL.
        /// </param>
        /// <param name="credentials">
        /// Credentials required to generate token.
        /// </param>
        /// <returns>
        /// Token string.
        /// </returns>
        public static Token GetServerToken(string tokenServiceURL,
            NetworkCredential credentials)
        {
            Debug.Assert(tokenServiceURL != null);
            Debug.Assert(credentials != null);


            HttpRequestOptions opt = new HttpRequestOptions();
            opt.Method = HttpMethod.Post;
            opt.UseGZipEncoding = false;
            opt.Timeout = GET_TOKEN_TIMEOUT;

            var response = WebHelper.SendRequest(tokenServiceURL, 
                _GetTokenCommonQueryString(credentials), opt);
            return _ParseTokenResponse(response);
        }

        /// <summary>
        /// Get server token using request where "referer" is specified.
        /// </summary>
        /// <param name="tokenServiceURL">Token service URL.</param>
        /// <param name="credentials">Credentials to get token.</param>
        /// <returns>Token from service.</returns>
        public static Token GetServerTokenUsingReferer(string tokenServiceURL,
            NetworkCredential credentials)
        {
            Debug.Assert(tokenServiceURL != null);
            Debug.Assert(credentials != null);

            StringBuilder query = new StringBuilder(_GetTokenCommonQueryString(credentials));
            RestHelper.AddQueryParam(QUERY_EXPIRATION, EXPIRATION_IN_MINUTES, query, true);
            RestHelper.AddQueryParam(QUERY_CLIENT, REFERER_PARAMETER_NAME, query, true);
            RestHelper.AddQueryParam(REFERER_PARAMETER_NAME, RefererValue, query, true);

            HttpRequestOptions opt = new HttpRequestOptions();
            opt.Method = HttpMethod.Post;
            opt.UseGZipEncoding = false;
            opt.Timeout = GET_TOKEN_TIMEOUT;

            var response = WebHelper.SendRequest(tokenServiceURL, query.ToString(), opt);
            return _ParseTokenResponse(response);
        }

        /// <summary>
        /// Creates tokenized service URL.
        /// </summary>
        /// <param name="serviceUrl">Service URL</param>
        /// <param name="token">Token value</param>
        /// <returns>
        /// Tokenized service URL.
        /// </returns>
        public static string FormatTokenUrl(string serviceUrl, string token)
        {
            Debug.Assert(serviceUrl != null);
            Debug.Assert(token != null);

            string query = FormatTokenQuery("", token);

            Uri uri = new Uri(serviceUrl);
            if (!String.IsNullOrEmpty(uri.Query))
                query = uri.Query + "&" + query;

            UriBuilder uriBuilder = new UriBuilder(serviceUrl);
            uriBuilder.Query = query;

            return uriBuilder.ToString();
        }

        /// <summary>
        /// Creates tokenized URL query.
        /// </summary>
        /// <param name="serviceUrl">Initial query</param>
        /// <param name="token">Token value</param>
        /// <returns>
        /// Tokenized query string.
        /// </returns>
        public static string FormatTokenQuery(string query, string token)
        {
            Debug.Assert(query != null);
            Debug.Assert(token != null);

            StringBuilder sb = new StringBuilder(query);
            RestHelper.AddQueryParam(QUERY_TOKEN, token, sb, true);

            return sb.ToString();
        }

        #endregion public methods

        #region Private members

        /// <summary>
        /// Get request for token.
        /// </summary>
        /// <param name="credentials">Credentials.</param>
        /// <returns>String with requests for token.</returns>
        private static string _GetTokenCommonQueryString(NetworkCredential credentials)
        {
            StringBuilder query = new StringBuilder();
            RestHelper.AddQueryParam(QUERY_REQUEST, QUERY_GETTOKEN, query, true);
            RestHelper.AddQueryParam(QUERY_USERNAME, credentials.UserName, query, true);
            RestHelper.AddQueryParam(QUERY_PASSWORD, credentials.Password, query, true);
            RestHelper.AddQueryParam(QUERY_FORMAT, JSON_FORMAT, query, true);

            return query.ToString();
        }

        /// <summary>
        /// Parse string with token and expiration date.
        /// </summary>
        /// <param name="response">String with responce from server.</param>
        /// <returns>Token struct.</returns>
        private static Token _ParseTokenResponse(string response)
        {
            var deserializer = new JavaScriptSerializer();
            var deserializedJSON = deserializer.Deserialize<Dictionary<string, object>>(response)
                as Dictionary<string, object>;

            // Check that we got auth error.
            _CheckForError(deserializedJSON);

            var token = deserializedJSON[TOKEN];
            var timestamp = (long)deserializedJSON[EXPIRES];

            var value = token as string;
            var expires = UNIX_START_DATE.AddMilliseconds(timestamp);

            return new Token(value, expires);
        }

        /// <summary>
        /// Check that response contains error. If so - throw exception.
        /// </summary>
        /// <param name="deserializedJSON">Dictionary with deserilized token response.</param>
        /// <exception cref="ESRI.ArcLogistics.AuthenticationException">
        /// Thrown if JSON contains error.</exception>
        private static void _CheckForError(Dictionary<string, object> deserializedJSON)
        {
            // Check that we have error.
            if (deserializedJSON.ContainsKey(RESPONSE_ERROR))
            {
                // Get details from response, throw exception.
                var error = deserializedJSON[RESPONSE_ERROR] as Dictionary<string, object>;
                throw new AuthenticationException(error[RESPONSE_DETAILS] as string);
            }
        }

        #endregion

    }
}
