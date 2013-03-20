using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using System.Windows.Controls;


namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts drawing color to SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(System.Drawing.Color), typeof(Grid))]
    internal class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Control result = new Control();
            
            if (value != null)
            {
                try
                {
                    System.Drawing.Color color = (System.Drawing.Color)value;
                    Color mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                    result.Style = (System.Windows.Style)App.Current.FindResource("ColorControlStyle");

                    result.Background = new SolidColorBrush(mediaColor);
                }
                catch
                {
                    result.Style = null;
                }
            }
            else
                result.Style = null;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
