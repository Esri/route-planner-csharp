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
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using System.Windows.Controls;


namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts drawing color to SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(System.Drawing.Color), typeof(Grid))]
    internal class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Control result = new Control();
            
            if (value != null)
            {
                try
                {
                    System.Drawing.Color color = (System.Drawing.Color)value;
                    Color mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                    result.Style = (System.Windows.Style)App.Current.FindResource("ColorControlStyle");

                    result.Background = new SolidColorBrush(mediaColor);
                }
                catch
                {
                    result.Style = null;
                }
            }
            else
                result.Style = null;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
