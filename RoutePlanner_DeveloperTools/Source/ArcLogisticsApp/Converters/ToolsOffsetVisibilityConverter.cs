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
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Class for conversion from status type to status visibility.
    /// </summary>
    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class ToolsOffsetVisibilityConverter : IValueConverter
    {
        #region IValueConverter members

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">Status.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Offset Visibility.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PaneState? paneState = value as PaneState?;

            Visibility result = Visibility.Visible;
            if (paneState != null && paneState.Value == PaneState.DockableWindow)
            {
                result = Visibility.Collapsed;
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

        #endregion
    }
}
