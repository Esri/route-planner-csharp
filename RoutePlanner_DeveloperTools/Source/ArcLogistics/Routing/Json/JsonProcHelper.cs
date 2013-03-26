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
using System.Text.RegularExpressions;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// JsonProcHelper class.
    /// </summary>
    internal class JsonProcHelper
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // error response pattern (lead fragment)
        private const string RPAT_ERROR_RESPONSE = "^\\s*?{\\s*?\"error\"\\s*?:";

        // JSON type information patterns
        private const string RPAT_JSON_TYPE_INFO = "\"__type\":\".+?\",";
        private const string RPAT_OBJECT_TYPE = "\"dataType\"\\s*?:\\s*?\"(?<type>.+?)\"";
        private const string RPAT_OBJECT_VALUE = "\"value\"\\s*?:\\s*?{";
        private const string TYPE_HINT = "\"__type\":\"{0}:#{1}\",";

        // JSON pre-processing patterns
        private const string RPAT_OPENING_CURLY_BRACE_DELIM = "{\\s*?,";
        private const string RPAT_CLOSING_CURLY_BRACE_DELIM = ",\\s*?}";

        /// <summary>
        /// Space symbol.
        /// </summary>
        private const string SPACE = " ";

        /// <summary>
        /// Underscore symbol.
        /// </summary>
        private const string UNDERSCORE = "_";

        #endregion constants

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static string DoPreProcessing(string json)
        {
            Debug.Assert(json != null);

            // remove waste attribute delimiters
            json = Regex.Replace(json, RPAT_OPENING_CURLY_BRACE_DELIM, "{");
            json = Regex.Replace(json, RPAT_CLOSING_CURLY_BRACE_DELIM, "}");

            return json;
        }

        public static string DoPostProcessing(string json)
        {
            Debug.Assert(json != null);

            // remove type information
            json = Regex.Replace(json, RPAT_JSON_TYPE_INFO, "");

            return json;
        }

        public static bool IsFaultResponse(string json)
        {
            Debug.Assert(json != null);

            return Regex.IsMatch(json, RPAT_ERROR_RESPONSE,
                RegexOptions.IgnoreCase);
        }

        public static string AddJsonTypeInfo(string json)
        {
            Debug.Assert(json != null);

            string res = json;

            List<string> types = _GetJsonTypeNames(json);
            if (types.Count > 0)
                res = _AddTypeHints(json, types);

            return res;
        }

        /// <summary>
        /// In input json string replaces spaces to underscores.
        /// </summary>
        /// <param name="json">Input json string.</param>
        /// <returns>Json string where spaces replaced to underscores.</returns>
        public static string ReplaceSpacesToUnderscores(string json)
        {
            Debug.Assert(json != null);

            return json.Replace(SPACE, UNDERSCORE);
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static List<string> _GetJsonTypeNames(string json)
        {
            Debug.Assert(json != null);

            Regex regex = new Regex(RPAT_OBJECT_TYPE);
            List<string> types = new List<string>();

            Match match = regex.Match(json);
            while (match.Success)
            {
                Group group = match.Groups["type"];
                if (group != null && group.Success)
                {
                    if (String.IsNullOrEmpty(group.Value))
                        throw new Exception(); // TODO:

                    types.Add(group.Value);
                }

                match = match.NextMatch();
            }

            return types;
        }

        private static string _AddTypeHints(string json, List<string> types)
        {
            Debug.Assert(json != null);
            Debug.Assert(types != null);

            string res = json;

            Regex regex = new Regex(RPAT_OBJECT_VALUE);
            int jsonIndex = 0;

            foreach (var jsonType in types)
            {
                var type = GPObjectHelper.GetTypeByJsonName(jsonType);
                if (type == null)
                {
                    continue;
                }

                var match = regex.Match(res, jsonIndex);
                if (!match.Success)
                {
                    break;
                }

                var typeHint = string.Format(TYPE_HINT, type.Name, type.Namespace);

                jsonIndex = match.Index + match.Length;
                res = res.Insert(jsonIndex, typeHint);
            }

            return res;
        }

        #endregion private methods
    }
}
