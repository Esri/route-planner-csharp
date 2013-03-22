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
using Xceed.Wpf.DataGrid;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Converters
{
    [ValueConversion(typeof(Row), typeof(string))]
    internal class RowTooltipConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Row row = value as Row;

            if (row == null)
                return string.Empty;

            string errorMessage = string.Empty;

            Object obj = row.DataContext;

            if (obj is IDataErrorInfo)
                errorMessage = ((IDataErrorInfo)obj).Error;
            if (string.IsNullOrEmpty(errorMessage))
                errorMessage = "";

            return errorMessage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion

    }
}
