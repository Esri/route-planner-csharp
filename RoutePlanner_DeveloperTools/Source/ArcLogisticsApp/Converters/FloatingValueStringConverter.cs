using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;


namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    internal class FloatingValueStringConverter : IValueConverter
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
                    result = ((double)currentValue).ToString("N", nfInfo);
                    result = ZeroNumbersCleaner.ClearNulls(result);
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
