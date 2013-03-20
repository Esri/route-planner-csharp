using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converter to define days with barriers in calendar.
    /// </summary>
    internal class BarriersCalendarDayConverter : IValueConverter
    {

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            bool hasBarriers = false;

            if (value != null)
            {
                if (BarriersDayStatusesManager.Instance.DayStatuses.Contains((DateTime)value))
                    hasBarriers = true;
            }

            return hasBarriers;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
