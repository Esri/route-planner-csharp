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
