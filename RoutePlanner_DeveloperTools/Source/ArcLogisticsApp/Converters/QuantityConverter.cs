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
using ESRI.ArcLogistics.App.OrderSymbology;
using System.Globalization;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(double?), typeof(object))]
    internal class QuantityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "";

            OrderQuantity orderQuantity = value as OrderQuantity;
            if (orderQuantity != null)
            {
                if (orderQuantity.DefaultValue)
                    result = SymbologyManager.DefaultValueString;
                else
                {
                    if (OrderQuantity.PROP_NAME_MinValue.Equals((string)parameter))
                        result = orderQuantity.MinValue.ToString();
                    else if (OrderQuantity.PROP_NAME_MaxValue.Equals((string)parameter))
                        result = orderQuantity.MaxValue.ToString();
                    else
                        System.Diagnostics.Debug.Assert(false);
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
