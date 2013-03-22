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
using System.Windows;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts <see cref="T:System.Bool"/> values into
    /// <see cref="T:System.Windows.Visibility"/> ones.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    internal sealed class BooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Converts the specified value of type <see cref="T:System.Bool"/> into
        /// corresponding <see cref="T:System.Windows.Visibility"/> value.
        /// </summary>
        /// <param name="value">The value of type <see cref="T:System.Bool"/> to
        /// be converted.</param>
        /// <param name="targetType">The type to convert source value to.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture to be used for conversion.</param>
        /// <returns><see cref="System.Windows.Visibility.Visible"/> if and only
        /// if the <paramref name="value"/> is of type <see cref="T:System.Bool"/>
        /// and is True, otherwise returns
        /// <see cref="System.Windows.Visibility.Collapsed"/>.</returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Backward conversion is not supported, so this method does nothing
        /// and returns null always.
        /// </summary>
        /// <param name="value">The value obtained from the binding target.</param>
        /// <param name="targetType">The type to convert value to.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture to be used for conversion.</param>
        /// <returns>null reference.</returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}
