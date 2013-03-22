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
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Converts <c>SyncType</c> enum to it string representation.
    /// </summary>
    [ValueConversion(typeof(SyncType), typeof(string))]
    internal class SyncTypeConverter : IValueConverter
    {
        /// <summary>
        /// Convert Enum to string.
        /// </summary>
        /// <param name="value">Enum value, which must be converted.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns><c>String</c> representing enum's value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;

            // If value isnt null - convert.
            if (value != null)
            {
                SyncType syncType = (SyncType)value;

                switch (syncType)
                {
                    // Convert selected synctype to it's string representation.
                    case SyncType.None:
                        result = App.Current.GetString("SyncTypeNone");
                        break;
                    case SyncType.ActiveSync:
                        result = App.Current.GetString("SyncTypeActiveSync");
                        break;
                    case SyncType.EMail:
                        result = App.Current.GetString("SyncTypeEmail");
                        break;
                    case SyncType.Folder:
                        result = App.Current.GetString("SyncTypeFolder");
                        break;
                    case SyncType.WMServer:
                        result = Properties.Resources.WMServer;
                        break;
                    default:
                        // Not supported Enum value.
                        Debug.Assert(false);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Convert string to Enum.
        /// </summary>
        /// <param name="value">String, representing enum.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Value of <c>SyncType</c> enum.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = (string)value;
            SyncType result ;

            // Convert string to enum.
            if (name == App.Current.GetString("SyncTypeNone"))
                result = SyncType.None;
            else if (name == App.Current.GetString("SyncTypeEmail"))
                result = SyncType.EMail;
            else if (name == App.Current.GetString("SyncTypeActiveSync"))
                result = SyncType.ActiveSync;
            else if (name == App.Current.GetString("SyncTypeFolder"))
                result = SyncType.Folder;
            else if (name == Properties.Resources.WMServer)
                result = SyncType.WMServer;
            else
            {
                // Not supported Enum value.
                Debug.Assert(false);
                return null;
            }

            return result;
        }
    }
}
