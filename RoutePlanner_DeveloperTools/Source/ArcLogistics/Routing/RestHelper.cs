using System;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;
using ESRI.ArcLogistics.Routing.Json;
using System.Globalization;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// RestHelper class.
    /// </summary>
    internal class RestHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds query string.
        /// </summary>
        /// <param name="escape">
        /// A boolean value indicating whether parameter names and values must
        /// be escaped.
        /// </param>
        /// <returns>Query string</returns>
        public static string BuildQueryString(object request,
            IEnumerable<Type> knownTypes, bool escape)
        {
            StringDictionary queryData = _GetQueryParams(request, knownTypes);

            if (queryData.Count == 0)
                throw new RouteException(Properties.Messages.Error_InvalidVrpRequest);

            StringBuilder sb = new StringBuilder();
            foreach (DictionaryEntry entry in queryData)
            {
                AddQueryParam((string)entry.Key,
                    (string)entry.Value, sb, escape);
            }

            return sb.ToString();
        }

        public static void AddQueryParam(string name, string value,
            StringBuilder sb,
            bool escape)
        {
            if (sb.Length > 0)
                sb.Append('&');

            sb.Append(escape ? Uri.EscapeDataString(name) : name);
            sb.Append('=');

            // If value size is small or we don't need to 
            // convert escape symbols - append value to stringbuilder.
            if (value.Length < URI_ESCAPE_METHOD_INPUT_STRING_MAX_LENGTH || !escape)
                sb.Append(escape ? Uri.EscapeDataString(value) : value);
            // If we have big value - covnert each text element separately.
            else
            {
                var textElementEnumerator = StringInfo.GetTextElementEnumerator(value);

                while(textElementEnumerator.MoveNext())
                    sb.Append(Uri.EscapeDataString(textElementEnumerator.GetTextElement()));
            }
        }

        #endregion public methods

        /// <summary>
        /// Creates RestException object.
        /// </summary>
        public static RestException CreateRestException(IFaultInfo faultInfo)
        {
            Debug.Assert(faultInfo != null);
            Debug.Assert(faultInfo.IsFault);

            RestException ex = null;

            GPError error = faultInfo.FaultInfo;
            if (error != null)
            {
                ex = new RestException(error.Message, error.Code,
                    error.Details);
            }
            else
                ex = new RestException(
                    Properties.Messages.Error_InvalidArcgisRestResponse);

            return ex;
        }

        /// <summary>
        /// Validates ArcGIS REST service response.
        /// </summary>
        public static void ValidateResponse(object resp)
        {
            Debug.Assert(resp != null);

            IFaultInfo faultInfo = resp as IFaultInfo;
            Debug.Assert(faultInfo != null);

            if (faultInfo.IsFault)
                throw CreateRestException(faultInfo);
        }

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static StringDictionary _GetQueryParams(object request,
            IEnumerable<Type> knownTypes)
        {
            StringDictionary dict = new StringDictionary();

            _AddPropertyParams(request, dict, knownTypes);
            _AddFieldParams(request, dict, knownTypes);

            return dict;
        }

        private static void _AddPropertyParams(object obj, StringDictionary dict,
            IEnumerable<Type> knownTypes)
        {
            PropertyInfo[] propInfo = obj.GetType().GetProperties();
            foreach (PropertyInfo pi in propInfo)
            {
                QueryParameterAttribute queryAttr = null;
                if (_FindCustomAttr(pi.GetCustomAttributes(false), out queryAttr))
                {
                    dict.Add(queryAttr.Name,
                        _ParamValueToString(pi.GetValue(obj, null), knownTypes));
                }
            }
        }

        private static void _AddFieldParams(object obj, StringDictionary dict,
            IEnumerable<Type> knownTypes)
        {
            FieldInfo[] info = obj.GetType().GetFields();
            foreach (FieldInfo fi in info)
            {
                QueryParameterAttribute queryAttr = null;
                if (_FindCustomAttr(fi.GetCustomAttributes(false), out queryAttr))
                {
                    dict.Add(queryAttr.Name,
                        _ParamValueToString(fi.GetValue(obj), knownTypes));
                }
            }
        }

        private static string _ParamValueToString(object value,
            IEnumerable<Type> knownTypes)
        {
            string valueStr = ""; // reserve empty string for null values
            if (value != null)
            {
                if (_IsJsonObject(value))
                {
                    // serialize JSON object
                    valueStr = JsonSerializeHelper.Serialize(value, knownTypes, true);
                }
                else
                {
                    // serialize non-JSON object
                    valueStr = value.ToString();
                }
            }

            return valueStr;
        }

        private static bool _IsJsonObject(object obj)
        {
            return _HasCustomAttr<DataContractAttribute>(
                obj.GetType().GetCustomAttributes(false));
        }

        private static bool _HasCustomAttr<T>(object[] attrs)
        {
            T attr;
            return _FindCustomAttr(attrs, out attr);
        }

        private static bool _FindCustomAttr<T>(object[] attrs, out T resAttr)
        {
            resAttr = default(T);

            bool found = false;
            foreach (object attr in attrs)
            {
                if (attr.GetType() == typeof(T))
                {
                    resAttr = (T)attr;
                    found = true;
                    break;
                }
            }

            return found;
        }

        #endregion private methods

        #region private const

        /// <summary>
        /// Maximum size of input string for Uri.EscapeUriString method.
        /// </summary>
        private const int URI_ESCAPE_METHOD_INPUT_STRING_MAX_LENGTH = 32768;

        #endregion
    }
}
