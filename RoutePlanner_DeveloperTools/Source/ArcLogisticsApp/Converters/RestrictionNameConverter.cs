using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Documents;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts resctriction's descritption and name to a string with special postfix if needed
    /// </summary>
    [ValueConversion(typeof(object), typeof(ICollection<Inline>))]
    internal class RestrictionNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Inline> inlines = new List<Inline>();

            if (null != value)
            {
                System.Diagnostics.Debug.Assert(value is RestrictionName);
                RestrictionName name = value as RestrictionName;

                inlines.Add(new Run(name.Name));
                if (!string.IsNullOrEmpty(name.Description))
                {
                    inlines.Add(new Run(NEW_LINE));
                    Inline description = new Run(name.Description);
                    description.Foreground = new SolidColorBrush(Colors.Gray);
                    description.FontSize = (double)App.Current.FindResource("StandartHelpFontSize");
                    inlines.Add(description);
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
