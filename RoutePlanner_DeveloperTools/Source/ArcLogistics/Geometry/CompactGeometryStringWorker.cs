using System;
using System.Diagnostics;
using System.Text;

namespace ESRI.ArcLogistics.Geometry
{
    /// <summary>
    /// Class for working with compact geometry string.
    /// </summary>
    internal class CompactGeometryStringWorker
    {
        #region Public property

        /// <summary>
        /// Separator between XY and additional info.
        /// </summary>
        public static char AdditionalPartsSeparator
        {
            get
            {
                return ADDITIONAL_PART_SEPARATOR;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Read one double from compressed geometry string by using passed position.
        /// Re-writes startPos for the next integer.
        /// </summary>
        /// <param name="geometryString">String with compact geometry.</param>
        /// <param name="startPos">Start position from reading should be begin.</param>
        /// <param name="result)">Double, extracted from the string.</param>
        /// <returns>Extracted double or null if there is nothing to extract.</returns>
        public static bool ReadDouble(string geometry, ref int startPos, ref double result)
        {
            var temp = CompactGeometryStringWorker.ReadInt(geometry, ref startPos);
            if (temp == null)
                return false;

            result = (double)temp;
            return true;
        }

        /// <summary>
        /// Read one integer from compressed geometry string by using passed position.
        /// Re-writes startPos for the next integer.
        /// </summary>
        /// <param name="geometryString">String with compact geometry.</param>
        /// <param name="startPos">Start position from reading should be begin.</param>
        /// <returns>Extracted integer or null if there is nothing to extract.</returns>
        internal static int? ReadInt(string geometry, ref int startPos)
        {
            if (startPos == geometry.Length)
            {
                return null;
            }

            bool isCompleted = false;
            bool isSubZero = false;

            int result = 0;
            int currentPos = startPos;

            while (!isCompleted)
            {
                int currentChar = geometry[currentPos];
                if (currentChar == '+' || currentChar == '-' || currentChar == ADDITIONAL_PART_SEPARATOR)
                {
                    if (currentPos != startPos)
                    {
                        isCompleted = true;
                        continue;
                    }
                    else if (currentChar == '-')
                        isSubZero = true;
                }
                else
                {
                    if (currentChar >= '0' && currentChar <= '9')
                    {
                        result = result << 5;
                        result += currentChar - (int)'0';
                    }
                    else if (currentChar >= 'a' && currentChar <= 'v')
                    {
                        result = result << 5;
                        result += currentChar - (int)'a' + 10;
                    }
                    else
                        return null;
                }

                currentPos++;
                if (currentPos == geometry.Length)
                    isCompleted = true;
            }

            startPos = currentPos;
            if (isSubZero)
                result = -result;

            return result;
        }

        /// <summary>
        /// Appends the specified point coordinate value the specified string builder converting it
        /// into compact geometry representation.
        /// </summary>
        /// <param name="value">Either X or Y point coordinate to be appended.</param>
        /// <param name="previous">The previous difference between point coordinates.</param>
        /// <param name="multBy">The multiplication factor to be used for converting coordinate
        /// into the compact geometry format.</param>
        /// <param name="result">The string builder object to append value to.</param>
        /// <returns>A new difference between point coordinates.</returns>
        internal static int AppendValue(
            double value,
            int previous,
            int multBy,
            StringBuilder result)
        {
            Debug.Assert(result != null);

            var newValue = (int)Math.Round(value * multBy);
            AppendValue(newValue - previous, result);

            return newValue;
        }

        /// <summary>
        /// Appends the specified value to the specified string builder converting it into
        /// compact geometry representation.
        /// </summary>
        /// <param name="value">The value to be appended.</param>
        /// <param name="result">The string builder object to append value to.</param>
        internal static void AppendValue(int value, StringBuilder result)
        {
            Debug.Assert(result != null);

            _ToString(value, COMPACT_GEOMETRY_VALUE_RADIX, result);
        }
        
        #endregion

        #region Private members

        /// <summary>
        /// Converts the specified value to a string using the specified radix and appends
        /// the string to the specified string builder.
        /// </summary>
        /// <param name="value">The value to be converted to string.</param>
        /// <param name="radix">The radix to be used for string representation of the value.</param>
        /// <param name="result">The string builder object to append resulting string to.</param>
        private static void _ToString(int value, int radix, StringBuilder result)
        {
            Debug.Assert(0 < radix && radix <= 32);
            Debug.Assert(result != null);

            result.Append(value > 0 ? '+' : '-');

            var digits = new char[MAX_DIGITS];
            var index = 0;
            var rest = Math.Abs(value);
            do
            {
                var digit = rest % radix;
                rest = rest / radix;

                var c = default(char);
                if (digit < 10)
                {
                    c = (char)('0' + digit);
                }
                else
                {
                    c = (char)('a' + digit - 10);
                }

                digits[index++] = c;
            } while (rest > 0);

            Array.Reverse(digits, 0, index);
            result.Append(digits, 0, index);
        }

        #endregion

        #region private constants

        /// <summary>
        /// Separator between XY and additional info.
        /// </summary>
        private const char ADDITIONAL_PART_SEPARATOR = '|';

        /// <summary>
        /// The radix used for storing compact geometry values.
        /// </summary>
        private const int COMPACT_GEOMETRY_VALUE_RADIX = 32;

        /// <summary>
        /// The maximum number of digits in the radix used for encoding values for compact geometry
        /// format.
        /// </summary>
        private static readonly int MAX_DIGITS = (int)Math.Ceiling(
            Math.Log(-(double)Int32.MinValue, COMPACT_GEOMETRY_VALUE_RADIX));

        #endregion
    }
}
