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
using System.Globalization;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Class removes redundant nulls from input string
    /// </summary>
    class ZeroNumbersCleaner
    {
        /// <summary>
        /// Removes all final nulls
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string ClearNulls(string inputString)
        {
            string clearString = inputString;

            int decimalIndex = inputString.IndexOf(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            if (decimalIndex > 0)
            {
                for (int i = clearString.Length - 1; i > decimalIndex; i--)
                {
                    if (clearString[i] == '0')
                        clearString = clearString.Remove(i, 1);
                    else
                        break;
                }
            }

            if (clearString.Length - 1 == decimalIndex)
                clearString = clearString.Remove(decimalIndex, 1);

            return clearString;
        }
    }
}
