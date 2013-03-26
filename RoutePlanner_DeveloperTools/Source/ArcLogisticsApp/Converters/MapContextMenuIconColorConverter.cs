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
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Class get stop and convert it to icon (circle filled by stop.Route color)
    /// </summary>
    [ValueConversion(typeof(Object), typeof(Object))]
    internal class MapContextMenuIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Object result = new Object();

            if (value != null)
            {
                try
                {
                    Debug.Assert(value is Stop); // only stops can be input value in this converter

                    Stop currentStop = (Stop)value;
                    StopType type = (StopType)currentStop.StopType;

                    ControlTemplate template = (ControlTemplate)App.Current.FindResource("OrderStopGlyph");
                    Grid grid = (Grid)template.LoadContent();

                    Path path = (Path)grid.Children[2];
                    Color color = Color.FromArgb(currentStop.Route.Color.A, currentStop.Route.Color.R,
                        currentStop.Route.Color.G, currentStop.Route.Color.B);
                    path.Fill = new SolidColorBrush(color);

                    result = grid;
                }
                catch
                {
                    result = null;
                }
            }
            else
                result = null;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
