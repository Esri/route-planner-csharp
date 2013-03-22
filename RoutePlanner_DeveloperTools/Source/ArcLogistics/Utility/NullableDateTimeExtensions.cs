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
