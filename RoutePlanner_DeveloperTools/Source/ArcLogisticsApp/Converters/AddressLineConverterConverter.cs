/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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
