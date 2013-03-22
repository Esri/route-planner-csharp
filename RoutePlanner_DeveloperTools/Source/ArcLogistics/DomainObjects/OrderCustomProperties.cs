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
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Order custom properties.
    /// </summary>
    public class OrderCustomProperties : IEnumerable<object>, ICloneable, INotifyPropertyChanged
    {
        #region Static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get custom property name by index.
        /// </summary>
        /// <remarks>
        /// First property's name is "OrderCustomProperty0", second property's name is "OrderCustomProperty1", and so on.
        /// </remarks>
        public static string GetCustomPropertyName(int propIndex)
        {
            if (propIndex < 0)
                throw new ArgumentException(Properties.Resources.InvalidCustomPropertyIndex);

            return string.Format(CUSTOM_PROPERTY_NAME_FORMAT, propIndex);
        }

        /// <summary>
        /// Gets custom property index by property name.
        /// </summary>
        /// <returns>Property index or -1 if <c>propName</c> is not correct custom property name.</returns>
        public static int GetCustomPropertyIndex(string propName)
        {
            int index = -1;
            if (propName.StartsWith(CUSTOM_PROPERTY_NAME_BASE))
            {
                string indexStr = propName.Substring(CUSTOM_PROPERTY_NAME_BASE.Length);

                int parsedIndex;
                if (int.TryParse(indexStr, out parsedIndex))
                    index = parsedIndex;
            }

            return index;
        }

        #endregion // Static methods

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>OrderCustomProperties</c> class.
        /// </summary>
        public OrderCustomProperties(OrderCustomPropertiesInfo info)
        {
            _propertiesInfo = info;
            _values = new object[_propertiesInfo.Count];

            // post init all numeric must set
            for (int index = 0; index < info.Count; ++index)
            {
                if (OrderCustomPropertyType.Numeric == info[index].Type)
                    _values[index] = 0.0;
            }
        }

        #endregion // Constructors

        #region Static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Put all properties to string.
        /// </summary>
        /// <param name="properties">Order custom properties values</param>
        internal static string AssemblyDBString(OrderCustomProperties properties, OrderCustomPropertiesInfo info)
        {
            Debug.Assert(properties.Count == info.Count);

            string strSeparator = new string(new char[1] { CommonHelpers.SEPARATOR });

            StringBuilder result = new StringBuilder();
            for (int index = 0; index < info.Count; ++index)
            {
                OrderCustomProperty propertyInfo = info[index];

                string value = null;
                if (OrderCustomPropertyType.Text == propertyInfo.Type)
                {
                    if (null != properties[index])
                    {
                        Debug.Assert(properties[index] is string);
                        value = (string)properties[index];
                    }
                }
                else if (OrderCustomPropertyType.Numeric == propertyInfo.Type)
                {
                    double tmp = 0.0;
                    if (null != properties[index])
                    {
                        if (properties[index] is double)
                            tmp = (double)properties[index];
                        else if (properties[index] is string)
                            tmp = double.Parse(properties[index].ToString());
                    }

                    value = tmp.ToString(CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE));
                }
                else
                {
                    Debug.Assert(false); // NOTE: not supported
                }

                if (!string.IsNullOrEmpty(value))
                {
                    string prop = value.Replace(strSeparator, CommonHelpers.SEPARATOR_ALIAS);
                    result.Append(prop);
                }

                if (index < properties.Count - 1)
                    result.Append(CommonHelpers.SEPARATOR); // NOTE: after last not neded
            }

            return result.ToString();
        }

        /// <summary>
        /// Parse string and split it to properties values
        /// </summary>
        /// <param name="propertiesValuesString">DB order custom properties string</param>
        /// <param name="orderCustomPropertiesInfo">Project order custom properties info</param>
        /// <returns>Parsed order custom properties</returns>
        internal static OrderCustomProperties CreateFromDBString(string propertiesValuesString,
                                                                 OrderCustomPropertiesInfo orderCustomPropertiesInfo)
        {
            Debug.Assert(orderCustomPropertiesInfo != null);

            // Create order custom properties object using data of orderCustomPropertiesinfo.
            OrderCustomProperties orderCustomProperties = new OrderCustomProperties(orderCustomPropertiesInfo);

            if (null == propertiesValuesString)
            {   // special initialization - of numeric values
                for (int index = 0; index < orderCustomPropertiesInfo.Count; ++index)
                {
                    if (OrderCustomPropertyType.Numeric == orderCustomPropertiesInfo[index].Type)
                        orderCustomProperties[index] = 0.0;
                }
            }
            else
            {
                // Values separator.
                char[] valuesSeparator = new char[1] { CommonHelpers.SEPARATOR };

                // Get array of values splitted by separator.
                string[] propertiesValues = propertiesValuesString.Split(valuesSeparator, StringSplitOptions.None);

                // This condition is not always true.
                //Debug.Assert(propertiesValues.Length == orderCustomPropertiesinfo.Count);

                string strSeparator = new string(valuesSeparator);

                // Iterate through list of order custom properties.
                for (int index = 0; index < orderCustomPropertiesInfo.Count; ++index)
                {
                    // Get current order custom property.
                    OrderCustomProperty orderCustomProperty = orderCustomPropertiesInfo[index];

                    // If index of item in list of order custom properties info is less than 
                    // length of array with values.
                    if (index < propertiesValues.Length)
                    {
                        // Get current value for appropriate custom order property.
                        string currentStrValue = propertiesValues[index];
                        if (!string.IsNullOrEmpty(currentStrValue))
                            currentStrValue = currentStrValue.Replace(CommonHelpers.SEPARATOR_ALIAS, strSeparator);

                        // Value of current property.
                        object currentPropertyValue = null;

                        // If type of order custom property is Text.
                        if (OrderCustomPropertyType.Text == orderCustomProperty.Type)
                        {
                            // Assign property valus as it is.
                            currentPropertyValue = currentStrValue;
                        }
                        // Type of custom order property is Numeric.
                        else if (OrderCustomPropertyType.Numeric == orderCustomProperty.Type)
                        {
                            // Convert string value to double.
                            double tmp = 0.0;
                            if (!string.IsNullOrEmpty(currentStrValue))
                                tmp = double.Parse(currentStrValue, CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE));
                            currentPropertyValue = tmp;
                        }
                        else
                        {
                            Debug.Assert(false); // NOTE: not supported
                        }

                        // Assign value of current custom order property.
                        orderCustomProperties[index] = currentPropertyValue;

                    } // if (index < values.Length)
                }// for (int index = 0; index < info.Count; ++index)
            } // else of if (null == value)

            return orderCustomProperties;
        }

        /// <summary>
        /// Calculates maximum length of string needed to serialize values of custom order properties
        /// to string taking into account types of properties (numeric / text), delimiter
        /// characters used to separate values of properties are accounted too.
        /// </summary>
        /// <param name="orderCustomPropertiesInfo">Order custom properties info.</param>
        /// <returns>Maximum length of string with values of properties.</returns>
        internal static int CalculateMaximumLengthOfDBString(OrderCustomPropertiesInfo orderCustomPropertiesInfo)
        {
            Debug.Assert(orderCustomPropertiesInfo != null);

            // Maximum string length of numeric property value.
            int numericPropertyMaxStringLength = _GetNumericPropertyMaxStringLength();

            // Result max length.
            int maxLength = 0;

            // Iterate through custom order properties info and calculate maximum length
            // needed to serialize values of properties to string taking into account
            // type of properties (numeric / text).
            foreach (OrderCustomProperty orderCustomProperty in orderCustomPropertiesInfo)
            {
                // If property is numeric.
                if (orderCustomProperty.Type == OrderCustomPropertyType.Numeric)
                {
                    maxLength += numericPropertyMaxStringLength;
                }
                // If property is text.
                else if (orderCustomProperty.Type == OrderCustomPropertyType.Text)
                {
                    maxLength += orderCustomProperty.Length;
                }
                // Unknown property type.
                else
                {
                    // NOTE: not supported.
                    Debug.Assert(false);
                }
            }

            // Take into account that after each value (except the last) delimiter character is added.
            if (orderCustomPropertiesInfo.Count != 0)
                maxLength += orderCustomPropertiesInfo.Count - 1;

            return maxLength;
        }

        /// <summary>
        /// Gets maximum length of string representation of numeric property value.
        /// </summary>
        /// <returns></returns>
        private static int _GetNumericPropertyMaxStringLength()
        {
            int maxLength = 0;

            // Minimum double value converted to string has the longest string representation.
            string minDoubleValueString = double.MinValue.ToString(
                CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE));

            maxLength = minDoubleValueString.Length;

            return maxLength;
        }

        #endregion // Static methods

        #region IEnumerable<object> members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            yield return _values;
        }

        #endregion // IEnumerable<object> members

        #region IEnumerable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return _values;
        }

        #endregion // IEnumerable members

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            OrderCustomProperties obj = new OrderCustomProperties(this._propertiesInfo);
            for (int index = 0; index < _values.Length; ++index)
                obj._values[index] = this._values[index];

            return obj;
        }

        #endregion // ICloneable members

        #region INotifyPropertyChanged members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // INotifyPropertyChanged members

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the number of properties.
        /// </summary>
        public int Count
        {
            get { return _values.Length; }
        }

        /// <summary>
        /// Gets property value by index.
        /// </summary>
        public object this[int index]
        {
            get { return _values[index]; }
            set
            {
                object actualeValue = _NormalizeValue(index, value);
                if (actualeValue != _values[index])
                {
                    _values[index] = actualeValue;
                    _NotifyPropertyChanged(GetCustomPropertyName(index));
                }
            }
        }

        /// <summary>
        /// Gets property value by its name.
        /// </summary>
        /// <remarks>
        /// Use names ("OrderCustomProperty0", "OrderCustomProperty1", etc) not titles from <c>OrderCustomPropertiesInfo</c>.
        /// </remarks>
        public object this[string propName]
        {
            get
            {
                int index = _GetIndex(propName);
                return _values[index];
            }
            set
            {
                int index = _GetIndex(propName);
                object actualeValue = _NormalizeValue(index, value);
                if (actualeValue != _values[index])
                {
                    _values[index] = actualeValue;
                    _NotifyPropertyChanged(propName);
                }
            }
        }

        /// <summary>
        /// Gets information about custom properies.
        /// </summary>
        public OrderCustomPropertiesInfo Info
        {
            get { return _propertiesInfo; }
        }

        #endregion // Public properties

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _NotifyPropertyChanged(String info)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private int _GetIndex(string propName)
        {
            int index = GetCustomPropertyIndex(propName);
            if ((index < 0) || (_values.Length < index))
            {
                string mes = string.Format(Properties.Resources.PropertyNameNotExists, propName);
                throw new ArgumentException(mes);
            }

            return index;
        }

        private object _NormalizeValue(int index, object value)
        {
            object actualeValue = value;
            if (_propertiesInfo[index].Type == OrderCustomPropertyType.Numeric)
            {
                if (value is double)
                    actualeValue = value;
                else
                {
                    Debug.Assert(value is string);

                    double tmp = 0.0;
                    try
                    {
                        tmp = double.Parse(value as string);
                    }
                    catch
                    {
                        tmp = 0.0;
                    }
                    actualeValue = tmp;
                }
            }
            else
            {
                Debug.Assert((null == value) || (value is string));
                actualeValue = value;
            }

            return actualeValue;
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private object[] _values = null;
        private OrderCustomPropertiesInfo _propertiesInfo = null;

        private const string CUSTOM_PROPERTY_NAME_BASE = "OrderCustomProperty";
        private const string CUSTOM_PROPERTY_NAME_FORMAT = "OrderCustomProperty{0}";

        #endregion // Private members
    }
}
