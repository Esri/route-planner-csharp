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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.DomainObjects;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Diagnostics;
using System.Reflection;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using System.Windows;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(Object), typeof(TextBlock))]
    internal class GanttTooltipConverter : IValueConverter
    {
        #region Convertation Methods

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBlock result = new TextBlock();
            result.FontSize = (double)App.Current.FindResource("MiddleFontSize");

            if (value != null)
            {
                ESRI.ArcLogistics.App.Mapping.TipGenerator.FillTipText(result, value);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion
    }
}
