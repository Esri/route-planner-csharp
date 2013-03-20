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
