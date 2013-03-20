using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(AddressCandidate), typeof(string))]
    internal class LocatorNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            if (value != null)
            {
                try
                {
                    AddressCandidate currentAddress = value as AddressCandidate;
                    result = currentAddress.Address.MatchMethod;
                }
                catch
                {
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
