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
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.Reports;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts object  to a description string.
    /// </summary>
    [ValueConversion(typeof(object), typeof(string))]
    internal class ObjectDescriptionConverter : IValueConverter
    {
        /// <summary>
        /// Converts report's template to description.
        /// </summary>
        /// <param name="value">Report's template wrapper.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Report's template description.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;
            if (null != value)
            {
                IDescripted desctiption = value as IDescripted;

                string desctiptionText = null;
                if (null != desctiption)
                    desctiptionText = desctiption.Description;

                if (!string.IsNullOrEmpty(desctiptionText))
                    result = desctiptionText;
            }

            return result;
        }

        /// <summary>
        /// Not used.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
