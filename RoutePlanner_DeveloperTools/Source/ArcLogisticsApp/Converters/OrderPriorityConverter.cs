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
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts <c>OrderPriority</c> enum to it string representation.
    /// </summary>
    [ValueConversion(typeof(OrderPriority), typeof(string))]
    internal class OrderPriorityConverter : IValueConverter
    {
        /// <summary>
        /// Convert Enum value to string.
        /// </summary>
        /// <param name="value">Enum value, which must be converted.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns><c>String</c> representing enum's value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            // If value isnt null - convert.
            if (value != null)
            {
                OrderPriority syncType = (OrderPriority)value;

                switch (syncType)
                {
                    // Convert selected synctype to it's string representation.
					case OrderPriority.Urgent:                        
                        result = App.Current.GetString("OrderPriorityUrgent");
                        break;     
                    case OrderPriority.High:
                        result = App.Current.GetString("OrderPriorityHigh");
                        break;
                    case OrderPriority.Normal:
                        result = App.Current.GetString("OrderPriorityNormal");
                        break;
					case OrderPriority.Low:
                        result = App.Current.GetString("OrderPriorityLow");
                        break;
                    default:
                        // Not supported Enum value.
                        Debug.Assert(false);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Convert string to Enum.
        /// </summary>
        /// <param name="value">String, representing enum.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Value of <c>OrderPriority</c> enum.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            OrderPriority result;

            // Convert string to enum.
            if (name == App.Current.GetString("OrderPriorityUrgent"))
                result = OrderPriority.Urgent;
            else if (name == App.Current.GetString("OrderPriorityHigh"))
                result = OrderPriority.High;
            else if (name == App.Current.GetString("OrderPriorityNormal"))
                result = OrderPriority.Normal;
            else if (name == App.Current.GetString("OrderPriorityLow"))
                result = OrderPriority.Low;
            else
            {
                // Not supported Enum value.
                Debug.Assert(false);
                return null;
            }

            return result;
        }
    }
}
