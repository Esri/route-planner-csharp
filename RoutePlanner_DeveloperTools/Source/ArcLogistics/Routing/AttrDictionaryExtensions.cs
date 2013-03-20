using System;
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.DomainObjects.Utility;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides helper methods for <see cref="T:ESRI.ArcLogistics.Routing.AttrDictionary"/>
    /// class.
    /// </summary>
    internal static class AttrDictionaryExtensions
    {
        /// <summary>
        /// Tries to get value from the specified attribute dictionary for the
        /// specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to get from dictionary.</typeparam>
        /// <param name="dictionary">The reference to the attribute dictionary
        /// to get value from.</param>
        /// <param name="key">The key to get value for.</param>
        /// <returns>Value for the specified key or null if there is no such key
        /// in the dictionary.</returns>
        public static TValue? TryGet<TValue>(
            this AttrDictionary dictionary,
            string key)
            where TValue : struct
        {
            Debug.Assert(dictionary != null);

            TValue result;
            if (dictionary.TryGet(key, out result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Sets time window attributes with specified names in the specified attributes dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to set attributes for.</param>
        /// <param name="window">The time window to get attribute values from.</param>
        /// <param name="plannedDate">The date/time when time window should be applied.</param>
        /// <param name="timeWindowStartAttribute">The name of the time window start
        /// attribute.</param>
        /// <param name="timeWindowEndAttribute">The name of the time window end
        /// attribute.</param>
        public static void SetTimeWindow(
            this AttrDictionary dictionary,
            TimeWindow window,
            DateTime plannedDate,
            string timeWindowStartAttribute,
            string timeWindowEndAttribute)
        {
            Debug.Assert(dictionary != null);
            Debug.Assert(window != null);
            Debug.Assert(!string.IsNullOrEmpty(timeWindowStartAttribute));
            Debug.Assert(!string.IsNullOrEmpty(timeWindowEndAttribute));

            if (window.IsWideOpen)
            {
                dictionary.Set(timeWindowStartAttribute, null);
                dictionary.Set(timeWindowEndAttribute, null);

                return;
            }

            // Time window is not wide-open so we apply it to the specified planned date and
            // attribute values in the dictionary.
            var dates = window.ToDateTime(plannedDate);

            var from = dates.Item1.Value;
            dictionary.Add(
                timeWindowStartAttribute,
                GPObjectHelper.DateTimeToGPDateTime(from));

            var to = dates.Item2.Value;
            dictionary.Add(
                timeWindowEndAttribute,
                GPObjectHelper.DateTimeToGPDateTime(to));
            }
    }
}
