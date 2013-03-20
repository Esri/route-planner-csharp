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
    [ValueConversion(typeof(Color), typeof(SolidColorBrush))]
    internal class LockedColorConverter : IValueConverter
    {
        public const double LOCKED_BRIGHTNESS_VALUE = 0.7;
        public const double MAX_ALPHA_VALUE = 255;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush result = new SolidColorBrush();

            if (value != null)
            {
                try
                {
                    Color inputColor = ((SolidColorBrush)value).Color;
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(inputColor.A,
                                                                               inputColor.R,
                                                                               inputColor.G,
                                                                               inputColor.B);

                    Color lockedColor = ConvertHSVtoRGB(color.GetHue(), color.GetSaturation(), LOCKED_BRIGHTNESS_VALUE);
                    result.Color = lockedColor;
                }
                catch
                {
                    result.Color = Colors.Transparent;
                }
            }
            else
                result.Color = Colors.Transparent;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        /// <summary>
        /// Method transform color from HSV to RGB opaque color
        /// </summary>
        /// <param name="H"></param>
        /// <param name="S"></param>
        /// <param name="V"></param>
        /// <returns></returns>
        public System.Windows.Media.Color ConvertHSVtoRGB(double H, double S, double V)
        {
            System.Windows.Media.Color result = new Color();

            int R, G, B = 0;

            if (S == 0)
            {
                R = System.Convert.ToInt32(V * MAX_ALPHA_VALUE);
                G = System.Convert.ToInt32(V * MAX_ALPHA_VALUE);
                B = System.Convert.ToInt32(V * MAX_ALPHA_VALUE);
            }
            else
            {
                int h = (Int32)(H / 60) % 6;

                double f = (H / 60) - h;

                double p = V * (1 - S);
                double q = V * (1 - f * S);
                double t = V * (1 - S * (1 - f));

                double r, g, b = 0;

                switch (h)
                {
                    case 0: r = V; g = t; b = p; break;
                    case 1: r = q; g = V; b = p; break;
                    case 2: r = p; g = V; b = t; break;
                    case 3: r = p; g = q; b = V; break;
                    case 4: r = t; g = p; b = V; break;
                    default: r = V; g = p; b = q; break;
                }

                R = (Int32)(r * MAX_ALPHA_VALUE);
                G = (Int32)(g * MAX_ALPHA_VALUE);
                B = (Int32)(b * MAX_ALPHA_VALUE);
            }

            result = Color.FromArgb((byte)MAX_ALPHA_VALUE, (byte)R, (byte)G, (byte)B);
            return result;
        }
    }
}
