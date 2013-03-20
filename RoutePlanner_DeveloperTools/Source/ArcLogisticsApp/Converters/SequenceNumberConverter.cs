using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Diagnostics;


namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Method gets stop and return OrederSequenceNumber of this stop
    /// </summary>
    [ValueConversion(typeof(Object), typeof(String))]
    internal class SequenceNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = default(string);

            if (value != null)
            {
                Debug.Assert(value is Stop);

                //cast input object to the stop
                Stop currentStop = (Stop)value;
                result = currentStop.OrderSequenceNumber.ToString();
            }
            else
                result = null;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
