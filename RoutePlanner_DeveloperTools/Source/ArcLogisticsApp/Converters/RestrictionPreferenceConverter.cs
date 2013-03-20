using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts resctriction's parameter to RestrictionDataWrapper colection.
    /// </summary>
    [ValueConversion(typeof(Parameter), typeof(IEnumerable<RestrictionUsageParameterValueWrapper>))]
    internal class RestrictionPreferenceAllValuesConverter : IValueConverter
    {
        /// <summary>
        /// Get source collection.
        /// </summary>
        /// <param name="value">Restriction parameter.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>RestrictionDataWrapper.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedParameter = value as Parameter;

            // Get default wrappers for restriction parameters.
            return RestrictionUsageParameterValueWrapper.GetDefaultWrappers(selectedParameter);
        }

        /// <summary>
        /// Always returns null.
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
    
    /// <summary>
    /// Converts resctriction's parameter to a combobox item.
    /// </summary>
    [ValueConversion(typeof(Parameter), typeof(RestrictionUsageParameterValueWrapper))]
    internal class RestrictionPreferenceSelectedValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert restriction parameter to RestrictionDataWrapper item.
        /// </summary>
        /// <param name="value">Restriction parameter.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>RestrictionDataWrapper.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var usageParameter = value as Parameter;
            
            // Get wrapper for parameter which is selected in combobox.
            return RestrictionUsageParameterValueWrapper.GetSelectedItemWrapper(usageParameter);
        }

        /// <summary>
        /// Convert RestrictionDataWrapper to restriction parameter.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>RestrictionDataWrapper.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var pair = (RestrictionUsageParameterValueWrapper)value;
            return pair.Parameter;
        }
    }
       
}
