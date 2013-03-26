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
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts TimeSpan to string, using DateTime.ToShortTimeString.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(String))]
    internal class TimespanConverter : IValueConverter
    {
        /// <summary>
        /// Converts.
        /// </summary>
        /// <param name="value"><c>TimeSpan</c> to convert.</param>
        /// <param name="targetType">Type of <c>TimeSpan</c>.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                TimeSpan timeSpan = (TimeSpan)value;
                DateTime date = new DateTime(timeSpan.Ticks);
                return date.ToShortTimeString();
            }
            else
                return null; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
