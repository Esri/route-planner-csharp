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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// JsonSerializeHelper class.
    /// </summary>
    internal class JsonSerializeHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Serializes JSON object.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns>
        /// String in JSON format.
        /// </returns>
        public static string Serialize(object obj)
        {
            Debug.Assert(obj != null);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(
                obj.GetType());

            return _Serialize(serializer, obj);
        }

        /// <summary>
        /// Serializes JSON object.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <param name="knownTypes">
        /// Types that may be present in the object graph.
        /// </param>
        /// <returns>
        /// String in JSON format.
        /// </returns>
        public static string Serialize(object obj, IEnumerable<Type> knownTypes)
        {
            Debug.Assert(obj != null);
            Debug.Assert(knownTypes != null);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(
                obj.GetType(), knownTypes);

            return _Serialize(serializer, obj);
        }

        /// <summary>
        /// Serializes JSON object.
        /// </summary>
        /// <param name="obj">
        /// Object to serialize.
        /// </param>
        /// <param name="doPostProcessing">
        /// A boolean value indicating whether post-processing should be
        /// performed.
        /// </param>
        /// <param name="knownTypes">
        /// Types that may be present in the object graph. Can be null.
        /// </param>
        /// <returns>
        /// String in JSON format.
        /// </returns>
        public static string Serialize(object obj, IEnumerable<Type> knownTypes,
            bool doPostProcessing)
        {
            Debug.Assert(obj != null);

            string json = null;
            if (knownTypes != null)
                json = Serialize(obj, knownTypes);
            else
                json = Serialize(obj);

            // post-processing
            if (doPostProcessing)
                json = JsonProcHelper.DoPostProcessing(json);

            return json;
        }

        /// <summary>
        /// Deserializes JSON object.
        /// </summary>
        /// <param name="json">String in JSON format.</param>
        /// <returns>
        /// Deserialized object.
        /// </returns>
        public static T Deserialize<T>(string json)
        {
            Debug.Assert(json != null);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(
                typeof(T));

            return _Deserialize<T>(serializer, json);
        }

        /// <summary>
        /// Deserializes JSON object.
        /// </summary>
        /// <param name="json">String in JSON format.</param>
        /// <param name="knownTypes">
        /// Types that may be present in the object graph.
        /// </param>
        /// <returns>
        /// Deserialized object.
        /// </returns>
        public static T Deserialize<T>(string json, IEnumerable<Type> knownTypes)
        {
            Debug.Assert(json != null);
            Debug.Assert(knownTypes != null);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(
                typeof(T), knownTypes);

            return _Deserialize<T>(serializer, json);
        }

        /// <summary>
        /// Deserializes JSON object.
        /// </summary>
        /// <param name="json">String in JSON format.</param>
        /// <param name="doPreProcessing">
        /// A boolean value indicating whether pre-processing should be
        /// performed.
        /// </param>
        /// <param name="knownTypes">
        /// Types that may be present in the object graph. Can be null.
        /// </param>
        /// <returns>
        /// Deserialized object.
        /// </returns>
        public static T Deserialize<T>(string json, IEnumerable<Type> knownTypes,
            bool doPreProcessing)
        {
            Debug.Assert(json != null);

            // pre-processing
            if (doPreProcessing)
                json = JsonProcHelper.DoPreProcessing(json);

            T obj;
            if (knownTypes != null)
                obj = Deserialize<T>(json, knownTypes);
            else
                obj = Deserialize<T>(json);

            return obj;
        }

        /// <summary>
        /// Deserializes response JSON object.
        /// </summary>
        /// <param name="json">String in JSON format.</param>
        /// <param name="knownTypes">
        /// Types that may be present in the object graph. Can be null.
        /// </param>
        /// <returns>
        /// Deserialized object.
        /// </returns>
        public static T DeserializeResponse<T>(string json,
            IEnumerable<Type> knownTypes)
        {
            // validate response string
            if (String.IsNullOrEmpty(json))
                throw new SerializationException(Properties.Messages.Error_InvalidJsonResponseString);

            T obj = default(T);

            // check if response string contains error data
            if (JsonProcHelper.IsFaultResponse(json))
            {
                // deserialize error object
                GPFaultResponse errorObj = Deserialize<GPFaultResponse>(json);

                // create response object
                obj = Activator.CreateInstance<T>();

                // set error data
                IFaultInfo fault = obj as IFaultInfo;
                if (fault != null)
                    fault.FaultInfo = errorObj.Error;
            }
            else
            {
                // deserialize response object
                obj = Deserialize<T>(json, knownTypes, true);
            }

            return obj;
        }

        /// <summary>
        /// Returns a boolean value indicating whether SerializationInfo
        /// object contains property with specified name.
        /// </summary>
        public static bool ContainsProperty(string name, SerializationInfo info)
        {
            bool found = false;

            SerializationInfoEnumerator en = info.GetEnumerator();
            while (en.MoveNext())
            {
                if (en.Name.Equals(name))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static string _Serialize(DataContractJsonSerializer serializer,
            object obj)
        {
            Debug.Assert(serializer != null);
            Debug.Assert(obj != null);

            string json = null;
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                json = Encoding.UTF8.GetString(ms.ToArray());
            }

            return json;
        }

        private static T _Deserialize<T>(DataContractJsonSerializer serializer,
            string json)
        {
            Debug.Assert(serializer != null);
            Debug.Assert(json != null);

            T obj = default(T);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                obj = (T)serializer.ReadObject(ms);
            }

            return obj;
        }

        #endregion private methods
    }

    internal class JS1
    {
        /// <summary>
        /// Deserializes object of the specified type from JSON string.
        /// </summary>
        /// <param name="type">The type of object to be deserialized.</param>
        /// <param name="json">The JSON string containing serialized object.</param>
        /// <param name="knownTypes">Collection of types that may be present in the object
        /// graph.</param>
        /// <param name="doPreProcessing">A value indicating whether pre-processing should be
        /// performed.</param>
        /// <returns>A reference to the deserialized object.</returns>
        public static object Deserialize(
            Type type,
            string json,
            IEnumerable<Type> knownTypes,
            bool doPreProcessing)
        {
            Debug.Assert(type != null);
            Debug.Assert(json != null);

            if (doPreProcessing)
            {
                json = JsonProcHelper.DoPreProcessing(json);
            }

            var serializer = new DataContractJsonSerializer(type, knownTypes);

            return _Deserialize(serializer, json);
        }

        private static object _Deserialize(
            DataContractJsonSerializer serializer,
            string json)
        {
            Debug.Assert(serializer != null);
            Debug.Assert(json != null);

            var obj = default(object);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                obj = serializer.ReadObject(ms);
            }

            return obj;
        }
    }
}
