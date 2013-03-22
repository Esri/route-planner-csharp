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
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converter for geometry type.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    internal class GeometryConverter : IValueConverter
    {
        /// <summary>
        /// Convert barrier attributes to barrier visibility.
        /// </summary>
        /// <param name="value">Barrier attributes.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Barrier visibility.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value != null)
            {
                object obj = value;
                if (value is Polygon)
                {
                    result = (string)App.Current.FindResource("ShapeBarrierLabel");
                }
                else if (value is Point)
                {
                    result = (string)App.Current.FindResource("PointBarrierLabel");
                }
                else if (value is Polyline)
                {
                    result = (string)App.Current.FindResource("LineBarrierLabel");
                }
                else
                {
                    Debug.Assert(false);
                }
            }
            return result;
        }

        /// <summary>
        /// Convert to source.
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
