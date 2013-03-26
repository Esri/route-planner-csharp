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
using System.Windows.Documents;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;
using System.Windows.Media;
using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts HistoryItem to a set of Inlines where the substring is bolded and underscored.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    internal class ArriveTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value != null)
            {
                DateTime inputDateTime = (DateTime)value;

                if (inputDateTime.Date == App.Current.CurrentDate)
                    result = inputDateTime.ToShortTimeString();
                else
                    result = string.Format((string)App.Current.FindResource("FullArriveTimeCellText"), inputDateTime.ToShortDateString(), inputDateTime.ToShortTimeString());
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
