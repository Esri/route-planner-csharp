/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Windows.Data;
using System.Windows.Documents;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts MessageDescription to a set of Inlines.
    /// </summary>
    [ValueConversion(typeof(object), typeof(ICollection<Inline>))]
    internal class MessageDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Inline> inlines = new List<Inline>();
            if (null != value)
            {
                MessageDescription description = (MessageDescription)value;

                if (description.IsSimpleMessage)
                    // add simple text
                    inlines.Add(new Run(description.Text));
                else
                {   // add text with link
                    string format = description.Format;
                    try
                    {
                        MatchCollection mc = Regex.Matches(format, @"({\d+})");
                        if (0 == mc.Count)
                            inlines.Add(new Run(format));
                        else
                        {
                            int index = 0;
                            for (int i = 0; i < mc.Count; ++i)
                            {
                                // add text before link
                                string stringObj = mc[i].Value;
                                int startIndex = format.IndexOf(stringObj, index);
                                if (0 < startIndex)
                                    inlines.Add(new Run(format.Substring(index, startIndex - index)));
                                index = startIndex + stringObj.Length;

                                // add link
                                MatchCollection mcNum = Regex.Matches(stringObj, @"(\d+)");
                                if (1 == mcNum.Count)
                                {
                                    int objNum = Int32.Parse(mcNum[0].Value);
                                    if (objNum < description.Objects.Count)
                                    {
                                        MessageObjectContext context = description.Objects[objNum];
                                        string objectName = string.Format("'{0}'", context.Name);
                                        if (string.IsNullOrEmpty(context.Hyperlink))
                                            inlines.Add(new Run(objectName));
                                        else
                                        {
                                            Hyperlink hlink = new Hyperlink(new Run(objectName));
                                            hlink.Command = new NavigationCommand(context.Hyperlink, context.Type, context.Id);
                                            inlines.Add(hlink);
                                        }
                                    }
                                }
                            }

                            // add text after all links
                            if (index < format.Length)
                                inlines.Add(new Run(format.Substring(index, format.Length - index)));
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        App.Current.Messenger.AddError(e.Message);
                        inlines.Add(new Run(format));
                    }
                }

                // add link
                Hyperlink hyperlink = MessageLinkHelper.CreateHiperlink(description.Link);
                if (null != hyperlink)
                {
                    inlines.Add(new Run(" "));
                    inlines.Add(hyperlink);
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
