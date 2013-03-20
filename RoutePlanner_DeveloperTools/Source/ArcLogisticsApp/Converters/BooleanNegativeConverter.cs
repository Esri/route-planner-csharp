using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts <see cref="T:System.Bool"/> values into <see cref="T:System.Bool"/> ones.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    internal sealed class BooleanNegativeConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Negation operation.
        /// </summary>
        /// <param name="value">The value of type <see cref="T:System.Bool"/> to
        /// be converted.</param>
        /// <param name="targetType">Ingored.</param>
        /// <param name="parameter">Ingored.</param>
        /// <param name="culture">Ingored.</param>
        /// <returns>Negation of "value".</returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return !(bool)value;
        }

        /// <summary>
        /// Negation operation.
        /// </summary>
        /// <param name="value">The value of type <see cref="T:System.Bool"/> to
        /// be converted.</param>
        /// <param name="targetType">Ingored.</param>
        /// <param name="parameter">Ingored.</param>
        /// <param name="culture">Ingored.</param>
        /// <returns>Negation of "value".</returns>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return !(bool)value;
        }
        #endregion
    }
}
