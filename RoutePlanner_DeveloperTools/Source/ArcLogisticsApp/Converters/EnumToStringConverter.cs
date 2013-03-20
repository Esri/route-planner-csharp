using System;
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts values of enumeration type to a localizeable string.
    /// </summary>
    [ValueConversion(typeof(Enum), typeof(String))]
    internal sealed class EnumToStringConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>
        /// Converts the specified value to a localizeable string.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <param name="targetType">The type to convert source value to.</param>
        /// <param name="parameter">The converter parameter.</param>
        /// <param name="culture">The culture to be used for conversion.</param>
        /// <returns>A localizeable string corresponding to the specified value.</returns>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            var enumValue = value as Enum;
            if (enumValue == null)
            {
                return null;
            }

            if (!Enum.IsDefined(value.GetType(), value))
            {
                return null;
            }

            var key = string.Format(
                "{0}.{1}",
                value.GetType().FullName,
                value.ToString());
            var resourceSelector = parameter as string;
            if (resourceSelector != null)
            {
                key = string.Format("{0},{1}", key, resourceSelector);
            }

            var result = App.Current.FindString(key);

            return result;
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
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            return null;
        }
        #endregion
    }
}
