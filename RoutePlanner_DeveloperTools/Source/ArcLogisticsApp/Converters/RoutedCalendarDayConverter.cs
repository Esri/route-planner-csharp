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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converter to define routed days in calendar.
    /// </summary>
    internal class RoutedCalendarDayConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, 
                              object parameter, CultureInfo culture)
        {
            string result = "";

            if (value != null)
            {
                if (DayStatusesManager.Instance.DayStatuses.Count > 0 &&
                    DayStatusesManager.Instance.DayStatuses.ContainsKey((DateTime)value))
                    result = DayStatusesManager.Instance.DayStatuses[(DateTime)value].Status.ToString();

            }
            return result;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
