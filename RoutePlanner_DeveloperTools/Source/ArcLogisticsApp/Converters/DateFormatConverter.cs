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
    /// Converts DateTime to string with necessary format.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(String))]
    internal class DateFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = String.Empty;
            
            if (value != null && value is DateTime)
                result = ((DateTime)value).ToShortDateString(); // Convert DateTime to short format.
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
