using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using ESRI.ArcLogistics.App.Mapping;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts drawing color to SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(IDictionary<string, object>), typeof(object))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ClusterSizeConverter : IValueConverter
    {
        private const int EXPANDANBLE_CLUSTER_SIZE = 22;
        private const int NONEXPANDANBLE_CLUSTER_SIZE = 28;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IDictionary<string, object> attributes = value as IDictionary<string, object>;

            if (attributes != null)
            {
                int count = (int)attributes[ALClusterer.COUNT_PROPERTY_NAME];
                if (count > ALClusterer.CLUSTER_COUNT_TO_EXPAND)
                    return NONEXPANDANBLE_CLUSTER_SIZE;
            }
            return EXPANDANBLE_CLUSTER_SIZE;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
