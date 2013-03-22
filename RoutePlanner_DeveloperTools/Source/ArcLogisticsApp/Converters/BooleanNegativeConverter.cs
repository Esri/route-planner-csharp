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
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts <see cref="T:System.Bool"/> values into <see cref="T:System.Bool"/> ones.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    internal sealed class BooleanNegativeConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Negation operation.
        /// </summary>
        /// <param name="value">The value of type <see cref="T:System.Bool"/> to
        /// be converted.</param>
        /// <param name="targetType">Ingored.</param>
        /// <param name="parameter">Ingored.</param>
        /// <param name="culture">Ingored.</param>
        /// <returns>Negation of "value".</returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return !(bool)value;
        }

        /// <summary>
        /// Negation operation.
        /// </summary>
        /// <param name="value">The value of type <see cref="T:System.Bool"/> to
        /// be converted.</param>
        /// <param name="targetType">Ingored.</param>
        /// <param name="parameter">Ingored.</param>
        /// <param name="culture">Ingored.</param>
        /// <returns>Negation of "value".</returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return !(bool)value;
        }
        #endregion
    }
}
