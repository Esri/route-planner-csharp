using System;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Convert to extract address, except addressline.
    /// </summary>
    [ValueConversion(typeof(AddressCandidate), typeof(string))]
    internal class AddressConverter : IValueConverter
    {
        /// <summary>
        /// Convert cell context to cell content.
        /// </summary>
        /// <param name="value">Cell context.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Cell content.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            AddressCandidate addressCandidate = value as AddressCandidate;
            if (addressCandidate != null)
            {
                // Show all address, except address line.
                string fullAddress = addressCandidate.Address.FullAddress;

                // Address line is before first separator.
                int index = fullAddress.IndexOf(',');

                if (index != -1)
                {
                    result = fullAddress.Substring(index + 1, fullAddress.Length - index - 1);
                    result = result.Trim();
                }
            }

            return result;
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
