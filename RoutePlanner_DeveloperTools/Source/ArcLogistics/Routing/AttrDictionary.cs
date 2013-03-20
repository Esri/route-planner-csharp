using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// AttrDictionary class.
    /// </summary>
    [Serializable]
    internal class AttrDictionary : ISerializable, IEnumerable<KeyValuePair<string, object>>
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AttrDictionary()
        {
        }

        protected AttrDictionary(SerializationInfo info, StreamingContext context)
        {
            foreach (var entry in info)
                _dict.Add(entry.Name, entry.Value);
        }

        #endregion constructors

        #region IEnumerable<KeyValuePair<string, object>> Members
        /// <summary>
        /// Returns an enumerator that iterates over <see cref="AttrDictionary"/> keys and values.
        /// </summary>
        /// <returns>A reference to an enumerator that iterates over <see cref="AttrDictionary"/>
        /// keys and values.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates over <see cref="AttrDictionary"/> keys and values.
        /// </summary>
        /// <returns>A reference to an enumerator that iterates over <see cref="AttrDictionary"/>
        /// keys and values.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public object Get(string key)
        {
            return _dict[key];
        }
        
        public T Get<T>(string key)
        {
            object obj = Get(key);
            if (obj == null)
                throw new RouteException(Properties.Messages.Error_GetAttributeByKeyFailed);

            return _ConvertObject<T>(obj);
        }

        public T Get<T>(string key, T defaultValue)
        {
            T value;
            if (!TryGet<T>(key, out value))
                value = defaultValue;

            return value;
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default(T);

            var obj = default(object);
            if (!_dict.TryGetValue(key, out obj) || obj == null)
            {
                return false;
            }

            var conversionExceptions = new[]
            {
                typeof(ArgumentException),
                typeof(FormatException),
                typeof(InvalidCastException),
                typeof(OverflowException),
            };

            bool res = false;
            try
            {
                value = _ConvertObject<T>(obj);
                res = true;
            }
            catch (Exception e)
            {
                if (!conversionExceptions.Any(type => type.IsAssignableFrom(e.GetType())))
                {
                    throw;
                }
            }

            return res;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (string key in _dict.Keys)
                info.AddValue(key, _dict[key]);
        }

        public void Add(string key, object value)
        {
            _dict.Add(key, value);
        }

        public void Set(string key, object value)
        {
            if (!_dict.ContainsKey(key))
                _dict.Add(key, value);
            else
                _dict[key] = value;
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static T _ConvertObject<T>(object obj)
        {
            T value;

            Type type = typeof(T);
            if (type.IsEnum)
                value = (T)_ConvertEnum(obj, type);
            else
                value = (T)Convert.ChangeType(obj, typeof(T), _fmtProvider);

            return value;
        }

        private static object _ConvertEnum(object obj, Type enumType)
        {
            object value;
            if (obj is String)
                value = Enum.Parse(enumType, (string)obj);
            else
            {
                var underlyingType = Enum.GetUnderlyingType(enumType);
                obj = Convert.ChangeType(obj, underlyingType);
                value = Enum.ToObject(enumType, obj);
            }

            return value;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // expect results from ArcGIS server in en-US locale 
        private static readonly CultureInfo _fmtProvider = new CultureInfo("en-US");

        private Dictionary<string, object> _dict = new Dictionary<
            string, object>();

        #endregion private fields
    }
}