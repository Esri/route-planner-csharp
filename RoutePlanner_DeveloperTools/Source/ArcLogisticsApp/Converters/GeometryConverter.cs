using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converter for geometry type.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    internal class GeometryConverter : IValueConverter
    {
        /// <summary>
        /// Convert barrier attributes to barrier visibility.
        /// </summary>
        /// <param name="value">Barrier attributes.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Barrier visibility.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value != null)
            {
                object obj = value;
                if (value is Polygon)
                {
                    result = (string)App.Current.FindResource("ShapeBarrierLabel");
                }
                else if (value is Point)
                {
                    result = (string)App.Current.FindResource("PointBarrierLabel");
                }
                else if (value is Polyline)
                {
                    result = (string)App.Current.FindResource("LineBarrierLabel");
                }
                else
                {
                    Debug.Assert(false);
                }
            }
            return result;
        }

        /// <summary>
        /// Convert to source.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
