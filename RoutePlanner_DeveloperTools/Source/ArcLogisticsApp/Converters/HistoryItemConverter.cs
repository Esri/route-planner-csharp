using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Documents;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts HistoryItem to a set of Inlines where the substring is bolded and underscored.
    /// </summary>
    [ValueConversion(typeof(HistoryService.HistoryItem), typeof(ICollection<Inline>))]
    internal class HistoryItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value is HistoryService.HistoryItem);
            Debug.Assert(targetType.Equals(typeof(ICollection<Inline>)));
            Debug.Assert(value != null);

            HistoryService.HistoryItem item = (HistoryService.HistoryItem)value;

            List<Inline> inlines = new List<Inline>();

            int startIndex = 0;
            int nextIndex = 0;
            do
            {
                nextIndex = item.String.IndexOf(item.Substring, startIndex, StringComparison.InvariantCultureIgnoreCase);

                if (nextIndex == -1) // add remaining ordinary text
                    inlines.Add(new Run(item.String.Substring(startIndex)));
                else
                {
                    if (nextIndex != startIndex) // add ordinal text
                        inlines.Add(new Run(item.String.Substring(startIndex, nextIndex - startIndex)));

                    inlines.Add(new Bold(new Underline(new Run(item.String.Substring(nextIndex, item.Substring.Length)))));

                    startIndex = nextIndex + item.Substring.Length;
                }
            }
            while (nextIndex != -1);

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
