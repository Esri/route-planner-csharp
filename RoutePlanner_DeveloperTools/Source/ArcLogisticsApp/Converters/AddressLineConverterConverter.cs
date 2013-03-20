using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Convert to extract addressline.
    /// </summary>
    [ValueConversion(typeof(AddressCandidate), typeof(string))]
    internal class AddressLineConverter : IValueConverter
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
                // Show address line.
                int index = addressCandidate.Address.FullAddress.IndexOf(',');

                // Address line is before first separator.
                if (index != -1)
                {
                    result = addressCandidate.Address.FullAddress.Substring(0, index);
                }                
                else
                {
                    // If separator is absent - show full address.
                    result = addressCandidate.Address.FullAddress;
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
