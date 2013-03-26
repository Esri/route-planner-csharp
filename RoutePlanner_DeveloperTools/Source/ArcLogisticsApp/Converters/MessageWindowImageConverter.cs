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
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;

using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(MessageType), typeof(StackPanel))]
    internal class MessageWindowImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StackPanel result = new StackPanel();
            result.Orientation = Orientation.Horizontal;

            if (null == value)
                result.Children.Clear();
            else
            {
                try
                {
                    MessageType type = (MessageType)value;

                    Image img = new Image();
                    switch (type)
                    {
                    case MessageType.Error :
                        img.Source = new BitmapImage(new Uri(@"\Resources\PNG_Icons\Error16.png", UriKind.Relative));
                        break;
                    case MessageType.Information :
                        img.Source = new BitmapImage(new Uri(@"\Resources\PNG_Icons\Info16.png", UriKind.Relative));
                        break;
                    case MessageType.Warning:
                        Debug.Assert(MessageType.Warning == type);
                        img.Source = new BitmapImage(new Uri(@"\Resources\PNG_Icons\Warning16.png", UriKind.Relative));
                        break;
                    default:
                        Debug.Assert(false); // NOTE: not supported
                        break;
                   }

                   Viewbox vb = new Viewbox();
                   vb.Child = img;
                   vb.SnapsToDevicePixels = true;
                   vb.Margin = new Thickness(0);

                   result.Children.Add(vb);
                }
                catch
                {
                   result.Children.Clear();
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
