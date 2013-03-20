using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts HistoryItem to a set of Inlines where the substring is bolded and underscored.
    /// </summary>
    [ValueConversion(typeof(string), typeof(ICollection<Inline>))]
    internal class RouteNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Inline> inlines = new List<Inline>();

            if (value == null)
            {
                return inlines;
            }

            var currentRoute = value as Route;
            if (currentRoute == null)
            {
                return inlines;
            }

            Italic noOrders = new Italic(new Run((string)App.Current.FindResource("NoOrdersCellText")));
            noOrders.Foreground = new SolidColorBrush(Colors.Gray);
            noOrders.FontSize = (double)App.Current.FindResource("StandartHelpFontSize");

            inlines.Add(new Run(currentRoute.Name + " "));

            if (currentRoute.Stops.Count == 0)
                inlines.Add(noOrders);

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
