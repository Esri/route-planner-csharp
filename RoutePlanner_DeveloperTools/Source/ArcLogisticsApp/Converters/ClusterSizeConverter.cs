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
using ESRI.ArcLogistics.App.Mapping;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts drawing color to SolidColorBrush
    /// </summary>
    [ValueConversion(typeof(IDictionary<string, object>), typeof(object))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ClusterSizeConverter : IValueConverter
    {
        private const int EXPANDANBLE_CLUSTER_SIZE = 22;
        private const int NONEXPANDANBLE_CLUSTER_SIZE = 28;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IDictionary<string, object> attributes = value as IDictionary<string, object>;

            if (attributes != null)
            {
                int count = (int)attributes[ALClusterer.COUNT_PROPERTY_NAME];
                if (count > ALClusterer.CLUSTER_COUNT_TO_EXPAND)
                    return NONEXPANDANBLE_CLUSTER_SIZE;
            }
            return EXPANDANBLE_CLUSTER_SIZE;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
