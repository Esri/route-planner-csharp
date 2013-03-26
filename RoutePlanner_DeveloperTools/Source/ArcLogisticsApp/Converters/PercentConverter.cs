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


namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    internal class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result;

            if (value != null)
            {
                try
                {
                    double? currentValue = value as double?;
                    NumberFormatInfo nfInfo = CultureInfo.CurrentCulture.NumberFormat;

                    result = ZeroNumbersCleaner.ClearNulls(((double)currentValue).ToString("N", nfInfo));

                    result = string.Format("{0} {1}", result, CultureInfo.CurrentCulture.NumberFormat.PercentSymbol);
                }
                catch
                {
                    result = "";
                }
            }
            else
                result = "";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
