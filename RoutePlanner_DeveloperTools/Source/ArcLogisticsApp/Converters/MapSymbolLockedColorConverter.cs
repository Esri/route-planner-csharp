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
using System.ComponentModel;


namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts drawing color to SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(IDictionary<string, object>), typeof(SolidColorBrush))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MapSymbolLockedColorConverter : IValueConverter
    {
        public const double LOCKED_BRIGHTNESS_VALUE = 0.7;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush result = new SolidColorBrush();

            if (value != null)
            {
                IDictionary<string, object> inputAttributes = value as IDictionary<string, object>;
                Color inputColor = ((SolidColorBrush)inputAttributes["Fill"]).Color;

                try
                {
                    if (inputAttributes != null &&
                        parameter.ToString() == "IsLocked" &&
                        inputAttributes.ContainsKey(parameter.ToString()))
                    {
                        LockedColorConverter colorConverter = new LockedColorConverter();

                        if ((bool)inputAttributes[parameter.ToString()])
                        {
                            var color = System.Drawing.Color.FromArgb(inputColor.A,
                                                                      inputColor.R,
                                                                      inputColor.G,
                                                                      inputColor.B);

                            Color lockedColor = colorConverter.ConvertHSVtoRGB(
                                color.GetHue(), color.GetSaturation(), LOCKED_BRIGHTNESS_VALUE);
                            result.Color = lockedColor;
                        }
                        else
                            result.Color = inputColor;
                    }
                    else
                        result.Color = inputColor;
                }
                catch
                {
                    result.Color = inputColor;
                }
            }
            else
                result.Color = Colors.Transparent;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
