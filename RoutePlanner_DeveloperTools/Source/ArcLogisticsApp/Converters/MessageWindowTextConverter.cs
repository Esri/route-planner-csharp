using System;
using System.Windows.Data;
using System.Windows.Documents;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts MessageWindowDataWrapper to a set of Inlines.
    /// </summary>
    [ValueConversion(typeof(object), typeof(ICollection<Inline>))]
    internal class MessageWindowTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Inline> inlines = new List<Inline>();
            if (null != value)
            {
                MessageWindowTextDataWrapper description = (MessageWindowTextDataWrapper)value;
                Hyperlink hyperlink = MessageLinkHelper.CreateHiperlink(description.Link);
                if (null == hyperlink)
                {   // add simple text
                    inlines.Add(new Run(description.Message));
                }
                else
                {
                    MatchCollection mc = Regex.Matches(description.Message, @"({\d+})");
                    if (0 == mc.Count)
                    {
                        // add simple text
                        inlines.Add(new Run(description.Message));

                        // add link
                        inlines.Add(new Run(" "));
                        inlines.Add(hyperlink);
                    }
                    else
                    {
                        // add text before link
                        string stringLink = mc[0].Value;
                        int startIndex = description.Message.IndexOf(stringLink, 0);
                        if (0 < startIndex)
                            inlines.Add(new Run(description.Message.Substring(0, startIndex)));
                        int position = startIndex + stringLink.Length;

                        // add link
                        inlines.Add(hyperlink);

                        // add text after all links
                        if (position < description.Message.Length)
                            inlines.Add(new Run(description.Message.Substring(position, description.Message.Length - position)));
                    }
                }
            }

            return inlines;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
