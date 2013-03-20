using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(IDictionary<string,object>), typeof(System.Windows.Visibility))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class OrderViolatedPropertyConverter : IValueConverter
    {
        System.Windows.Visibility result = System.Windows.Visibility.Hidden;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            result = System.Windows.Visibility.Collapsed;

            if (value != null && parameter.ToString() == "IsViolated")
            {
                try
                {
                    IDictionary<string, object> inputAttributes = value as IDictionary<string, object>;

                    if (inputAttributes != null &&
                        inputAttributes.ContainsKey(parameter.ToString()))
                    {
                         bool isViolated = (bool)inputAttributes[parameter.ToString()];

                         if (isViolated)
                             result = System.Windows.Visibility.Visible;
                         else
                             result = System.Windows.Visibility.Collapsed;
                    }
                }
                catch
                {
                    result = System.Windows.Visibility.Collapsed;
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
