using System;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Returns "true" if host name is valid, otherwise returns "false"
    /// </summary>
    [ValueConversion(typeof(string), typeof(bool))]
    class HostNameToBoolValidationConverter : IValueConverter
    {
        /// <summary>
        /// Converts the specified value to a boolean value indicating if the host name is valid.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <param name="targetType">The type to convert source value to.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture to be used for conversion.</param>
        /// <returns>A boolean value indicating if the host name is valid.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var validator = new HostNameValidator();

            return validator.Validate(value as string).IsValid;
        }

        /// <summary>
        /// Backward conversion is not supported, so this method does nothing
        /// and returns null always.
        /// </summary>
        /// <param name="value">The value obtained from the binding target.</param>
        /// <param name="targetType">The type to convert value to.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture to be used for conversion.</param>
        /// <returns>null reference.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
