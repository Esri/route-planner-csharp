using System;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Provides extension methods for nullable date/time.
    /// </summary>
    internal static class NullableDateTimeExtensions
    {
        /// <summary>
        /// Adds the specified number of days to the <paramref name="source"/>
        /// value.
        /// </summary>
        /// <param name="source">The date/time to add days for.</param>
        /// <param name="days">The amount of days to be added.</param>
        /// <returns>A new value which is the sum of the <paramref name="source"/>
        /// and <paramref name="days"/> or null if the <paramref name="source"/>
        /// is null.</returns>
        public static DateTime? AddDays(
            this DateTime? source,
            double days)
        {
            return source.Select(value => value.AddDays(days));
        }

        /// <summary>
        /// Adds the specified number of minutes to the <paramref name="source"/>
        /// value.
        /// </summary>
        /// <param name="source">The date/time to add minutes for.</param>
        /// <param name="minutes">The amount of minutes to be added.</param>
        /// <returns>A new value which is the sum of the <paramref name="source"/>
        /// and <paramref name="minutes"/> or null if the <paramref name="source"/>
        /// is null.</returns>
        public static DateTime? AddMinutes(
            this DateTime? source,
            double minutes)
        {
            return source.Select(value => value.AddMinutes(minutes));
        }

        /// <summary>
        /// Gets the date component for the specified date/time.
        /// </summary>
        /// <param name="source">The date/time to get date component for.</param>
        /// <returns>A new value representing the date component of the <paramref name="source"/>
        /// or null if the <paramref name="source"/> is null.</returns>
        public static DateTime? Date(this DateTime? source)
        {
            return source.Select(value => value.Date);
        }

        /// <summary>
        /// Converts the specified date/time to UTC.
        /// </summary>
        /// <param name="source">The date/time to be converted.</param>
        /// <returns>A new date/time value representing the <paramref name="source"/> in UTC
        /// or null if the <paramref name="source"/> is null.</returns>
        public static DateTime? ToUniversalTime(this DateTime? source)
        {
            return source.Select(value => value.ToUniversalTime());
        }
    }
}
