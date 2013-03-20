using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Convert restriction parameter value to its string representation.
    /// </summary>
    [ValueConversion(typeof(Parameter), typeof(string))]
    internal class RestrictionUsageLabelConverter : IValueConverter
    {
        /// <summary>
        /// Convert restriction parameter to corresponding string.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>RestrictionDataWrapper.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var usageParameter = value as Parameter;

            // Get wrapper which is corresponding to current parameter.
            return RestrictionUsageParameterValueWrapper.GetValueWrapper(usageParameter);
        }

        /// <summary>
        /// Always returned null.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
