using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Dispatcher;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Services.Geocoding
{
    /// <summary>
    /// Converts parameters in query strings of geocoding service to objects of the appropriate
    /// type.
    /// </summary>
    internal sealed class GeocodingQueryStringConverter : QueryStringConverter
    {
        /// <summary>
        /// Checks if the specified type can be converted to and from a string representation.
        /// </summary>
        /// <param name="type">The type object to be checked.</param>
        /// <returns>True if and only if objects of the specified type can be converted
        /// to and from a string representation.</returns>
        public override bool CanConvert(Type type)
        {
            if (base.CanConvert(type))
            {
                return true;
            }

            if (typeof(Point) == type)
            {
                return true;
            }

            // For nullable types like 'T?' we extract underlying type 'T' and perform conversion
            // for it if possible.
            if (_IsNullableType(type))
            {
                return this.CanConvert(type.GetGenericArguments().FirstOrDefault());
            }

            return false;
        }

        /// <summary>
        /// Converts a query string parameter to an object of the specified type.
        /// </summary>
        /// <param name="parameter">The string value to be converted to an object of the
        /// specified type.</param>
        /// <param name="parameterType">The type of the object represented by the parameter
        /// string.</param>
        /// <returns>The converted parameter object.</returns>
        /// <exception cref="FormatException">The provided parameter string does not have
        /// correct format.</exception>
        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            if (parameterType == typeof(Point) && parameter != null)
            {
                // We got string-encoded Point value which is expected to be formatted like
                // "x, y" where 'x' and 'y' are corresponding Point coordinates.

                var values = parameter.Split(POINT_ELEMENTS_SEPARATOR);
                if (values.Length != POINT_ELEMENT_COUNT)
                {
                    var message = string.Format(
                        CultureInfo.CurrentUICulture,
                        Properties.Messages.Error_InvalidPointParameterFormat,
                        parameter);
                    throw new FormatException(message);
                }

                var x = double.Parse(values[0], NumberStyles.Float, NUMBER_FORMAT);
                var y = double.Parse(values[1], NumberStyles.Float, NUMBER_FORMAT);

                return new Point
                {
                    X = x,
                    Y = y,
                };
            }

            if (_IsNullableType(parameterType))
            {
                // We got a string-encoded nullable type value, so we need to restore
                // the value of the underlying type if string is non empty.

                if (string.IsNullOrEmpty(parameter))
                {
                    return null;
                }

                var baseType = parameterType.GetGenericArguments().FirstOrDefault();

                return this.ConvertStringToValue(parameter, baseType);
            }

            // No custom logic needed, just redirect call to the base converter implementation.

            return base.ConvertStringToValue(parameter, parameterType);
        }

        /// <summary>
        /// Converts the specified parameter object to a query string representation.
        /// </summary>
        /// <param name="parameter">The parameter object to be converted.</param>
        /// <param name="parameterType">The type of the parameter to be converted.</param>
        /// <returns>A string representation of the specified parameter.</returns>
        public override string ConvertValueToString(object parameter, Type parameterType)
        {
            if (parameterType == typeof(Point) && (parameter is Point))
            {
                // Convert point to the string in "x, y" format where 'x' and 'y' are corresponding
                // point coordinates.

                var point = (Point)parameter;
                var result = string.Format(
                    NUMBER_FORMAT,
                    POINT_TO_STRING_CONVERSION_FORMAT,
                    point.X,
                    point.Y);

                return result;
            }

            if (_IsNullableType(parameterType))
            {
                // Convert nullable value by taking the value of underlying type (if any) and
                // applying conversion to it.

                if (parameter == null)
                {
                    return string.Empty;
                }

                var baseType = parameterType.GetGenericArguments().FirstOrDefault();

                return this.ConvertValueToString(parameter, baseType);
            }

            // No custom logic needed, just redirect call to the base converter implementation.

            return base.ConvertValueToString(parameter, parameterType);
        }

        #region private methods
        /// <summary>
        /// Checks if the specified type is a <see cref="Nullable&lt;T&gt;"/> one.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns>True if and only if the specified type is a <see cref="Nullable&lt;T&gt;"/>
        /// one.</returns>
        private static bool _IsNullableType(Type type)
        {
            return
                type != null &&
                type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        #endregion

        #region private constants
        /// <summary>
        /// Default number format to be used from converting numbers into strings and vice-versa.
        /// </summary>
        private static readonly IFormatProvider NUMBER_FORMAT =
            CultureInfo.InvariantCulture.NumberFormat;

        /// <summary>
        /// The number of <see cref="Point"/> elements in the string representation of the point.
        /// </summary>
        private const int POINT_ELEMENT_COUNT = 2;

        /// <summary>
        /// The separator character to be used for finding <see cref="Point"/> elements in its
        /// string representation.
        /// </summary>
        private const char POINT_ELEMENTS_SEPARATOR = ',';

        /// <summary>
        /// The format string to be used for converting <see cref="Point"/> instances into strings.
        /// </summary>
        private const string POINT_TO_STRING_CONVERSION_FORMAT = "{0},{1}";
        #endregion
    }
}