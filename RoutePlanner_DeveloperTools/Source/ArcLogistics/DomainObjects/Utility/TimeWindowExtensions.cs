using System;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.DomainObjects.Utility
{
    /// <summary>
    /// Provides helper extensions for the <see cref="TimeWindow"/> objects.
    /// </summary>
    internal static class TimeWindowExtensions
    {
        /// <summary>
        /// Converts time window into a pair of starting and ending date/time values.
        /// </summary>
        /// <param name="timeWindow">The reference to the time window to be converted.</param>
        /// <param name="plannedDate">The date/time when time window should be applied.</param>
        /// <returns>A pair of starting and ending date/time values.</returns>
        public static Tuple<DateTime?, DateTime?> ToDateTime(
            this TimeWindow timeWindow,
            DateTime plannedDate)
        {
            // Start date time.
            DateTime? startDate = null;

            // End date time.
            DateTime? endDate = null;

            // Time window start/end date time tuple.
            Tuple<DateTime?, DateTime?> dateTimeTuple = null;

            // If time wondow is null or wide open.
            if (timeWindow == null || timeWindow.IsWideOpen)
            {
                dateTimeTuple = Tuple.Create(startDate, endDate);
            }
            // Time window is not wide open.
            else
            {
                startDate = plannedDate.Date.Add(timeWindow.EffectiveFrom);
                endDate = plannedDate.Date.Add(timeWindow.EffectiveTo);

                dateTimeTuple = Tuple.Create(startDate, endDate);
            }

            return dateTimeTuple;
        }
    }
}
