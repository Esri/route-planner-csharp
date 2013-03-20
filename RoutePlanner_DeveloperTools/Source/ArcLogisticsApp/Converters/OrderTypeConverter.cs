using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts <c>OrderType</c> enum to it string representation.
    /// </summary>
    [ValueConversion(typeof(OrderType), typeof(string))]
    internal class OrderTypeConverter : IValueConverter
    {
        /// <summary>
        /// Convert Enum to string.
        /// </summary>
        /// <param name="value">Enum value, which must be converted.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns><c>String</c> representing enum's value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            // If value isnt null - convert.
            if (value != null)
            {
                OrderType syncType = (OrderType)value;

                switch (syncType)
                {
                    // Convert selected synctype to it's string representation.
                    case OrderType.Delivery:
                        result = App.Current.GetString("OrderTypeDelivery");
                        break;
                    case OrderType.Pickup:
                        result = App.Current.GetString("OrderTypePickup");
                        break;
                    default:
                        // Not supported Enum value.
                        Debug.Assert(false);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Convert string to Enum.
        /// </summary>
        /// <param name="value">String, representing enum.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Value of <c>OrderType</c> enum.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            OrderType result;

            // Convert string to enum.
            if (name == App.Current.GetString("OrderTypeDelivery"))
                result = OrderType.Delivery;
            else if (name == App.Current.GetString("OrderTypePickup"))
                result = OrderType.Pickup;
            else
            {
                // Not supported Enum value.
                Debug.Assert(false);
                return null;
            }

            return result;
        }
    }
}
