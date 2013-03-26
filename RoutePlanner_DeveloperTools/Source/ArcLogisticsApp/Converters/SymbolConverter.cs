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
using System.Globalization;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.OrderSymbology;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(object), typeof(object))]
    class SymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result = null;

            if (value != null)
            {
                SymbologyRecord sr = value as SymbologyRecord;
                if (sr != null)
                    result = sr.Symbol;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string result = SymbologyManager.DEFAULT_TEMPLATE_NAME;
            SymbolBox symbolBox = value as SymbolBox;
            SymbologyRecord result = null;

            if (symbolBox != null)
            {
                result = symbolBox.ParentRecord;
                symbolBox.ParentRecord.SymbolFilename = symbolBox.TemplateFileName;
            }

            return result;
        }
    }
}
