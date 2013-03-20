using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ESRI.ArcLogistics.App.OrderSymbology;
using System.Globalization;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(double?), typeof(object))]
    internal class QuantityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "";

            OrderQuantity orderQuantity = value as OrderQuantity;
            if (orderQuantity != null)
            {
                if (orderQuantity.DefaultValue)
                    result = SymbologyManager.DefaultValueString;
                else
                {
                    if (OrderQuantity.PROP_NAME_MinValue.Equals((string)parameter))
                        result = orderQuantity.MinValue.ToString();
                    else if (OrderQuantity.PROP_NAME_MaxValue.Equals((string)parameter))
                        result = orderQuantity.MaxValue.ToString();
                    else
                        System.Diagnostics.Debug.Assert(false);
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
