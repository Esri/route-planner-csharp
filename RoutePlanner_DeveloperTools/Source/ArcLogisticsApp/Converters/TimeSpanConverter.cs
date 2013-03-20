using System;
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts TimeSpan to string, using DateTime.ToShortTimeString.
    /// </summary>
    [ValueConversion(typeof(DateTime), typeof(String))]
    internal class TimespanConverter : IValueConverter
    {
        /// <summary>
        /// Converts.
        /// </summary>
        /// <param name="value"><c>TimeSpan</c> to convert.</param>
        /// <param name="targetType">Type of <c>TimeSpan</c>.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                TimeSpan timeSpan = (TimeSpan)value;
                DateTime date = new DateTime(timeSpan.Ticks);
                return date.ToShortTimeString();
            }
            else
                return null; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
