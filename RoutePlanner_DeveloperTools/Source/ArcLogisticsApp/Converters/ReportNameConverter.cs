using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Documents;
using System.Globalization;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts report's name to a string with special postfix if needed
    /// </summary>
    [ValueConversion(typeof(object), typeof(ICollection<Inline>))]
    internal class ReportNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Inline> inlines = new List<Inline>();

            if (null != value)
            {
                System.Diagnostics.Debug.Assert(value is SpecialName);
                SpecialName name = value as SpecialName;

                inlines.Add(new Run(name.Name));
                if (name.IsSpecial)
                {
                    inlines.Add(new Run(NEW_LINE));
                    Italic noOrders = new Italic(new Run((string)App.Current.FindResource("ReportEnforceSplittedText")));
                    noOrders.Foreground = new SolidColorBrush(Colors.Gray);
                    noOrders.FontSize = (double)App.Current.FindResource("StandartHelpFontSize");
                    inlines.Add(noOrders);
                }
            }

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private const string NEW_LINE = "\n";
    }
}
