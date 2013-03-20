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
