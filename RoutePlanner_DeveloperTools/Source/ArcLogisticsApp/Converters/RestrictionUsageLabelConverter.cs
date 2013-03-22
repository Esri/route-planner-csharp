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
using System.Linq;
using System.Windows.Data;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Convert restriction parameter value to its string representation.
    /// </summary>
    [ValueConversion(typeof(Parameter), typeof(string))]
    internal class RestrictionUsageLabelConverter : IValueConverter
    {
        /// <summary>
        /// Convert restriction parameter to corresponding string.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>RestrictionDataWrapper.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var usageParameter = value as Parameter;

            // Get wrapper which is corresponding to current parameter.
            return RestrictionUsageParameterValueWrapper.GetValueWrapper(usageParameter);
        }

        /// <summary>
        /// Always returned null.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
