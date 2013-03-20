using System;
using System.Windows.Data;
using System.Globalization;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts resctriction's parameter value to a string in special form
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    internal class RestrictionParameterValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If we have no default value - return special value from resource.
            return (string)value == string.Empty ? 
                Properties.Resources.RestrictionParameterNull : (string)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Converts resctriction's parameter value to a boolean flag
    /// </summary>
    [ValueConversion(typeof(string), typeof(bool))]
    internal class RestrictionParameterPresentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (null != value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
