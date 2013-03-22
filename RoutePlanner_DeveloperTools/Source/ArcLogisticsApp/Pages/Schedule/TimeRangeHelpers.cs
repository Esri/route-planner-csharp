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
using System.Globalization;

namespace ESRI.ArcLogistics.App.Pages
{
    enum TimeRanges
    {
        Today,
        Yesterday,
        ThisWeek,
        LastWeek,
        ThisMonth,
        LastMonth,
        SpecifiedTimeRange,
        Anyday
    };

    class TimeRange
    {
        public TimeRange(DateTime start, DateTime end)
        {
            _start = start;
            _end = end;
        }

        public DateTime Start
        {
            get {return _start;}
        }

        public DateTime End
        {
            get{return _end;}
        }

        private DateTime _start;
        private DateTime _end;
    }

    static class TimeRangeHelper
    {
        public static TimeRange GetRange(DateTime currentDate, TimeRanges rangeType)
        {
            TimeRange range = null;
            switch (rangeType)
            {
                case TimeRanges.Today:
                    range = new TimeRange(currentDate.Date, currentDate.Date);
                    break;
                case TimeRanges.Yesterday:
                    DateTime yesterday = currentDate.AddDays(-1);
                    range = new TimeRange(yesterday, yesterday);
                    break;
                case TimeRanges.ThisWeek:
                    range = _GetWeekRange(currentDate);
                    break;
                case TimeRanges.LastWeek:
                    TimeRange currentWeekRange = _GetWeekRange(currentDate);
                    range = _GetWeekRange(currentWeekRange.Start.AddDays(-1));
                    break;
                case TimeRanges.ThisMonth:
                    range = _GetMonthRange(currentDate);
                    break;
                case TimeRanges.LastMonth:
                    TimeRange currentMonthRange = _GetMonthRange(currentDate);
                    range = _GetMonthRange(currentMonthRange.Start.AddDays(-1));
                    break;
                case TimeRanges.Anyday:
                    range = new TimeRange(DateTime.MinValue.AddYears(1753), DateTime.MaxValue.AddYears(-1000));
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            return range;
        }

        /// <summary>
        /// Get name of the time rangeTypeName type.
        /// </summary>
        /// <param name="rangeType">TimeRanges.</param>
        /// <returns>Name of this time rangeTypeName type.</returns>
        /// <remarks>Name isn't localizable, do not use in UI.</remarks>
        public static string GetRangeSettingsName(TimeRanges rangeType)
        {
            string rangeTypeName = null;

            // Find corresponding name.
            switch (rangeType)
            {
                case TimeRanges.Today:
                    rangeTypeName = TODAY_SEARCH_RANGE_NAME;
                    break;
                case TimeRanges.Yesterday:
                    rangeTypeName = YESTERDAY_SEARCH_RANGE_NAME;
                    break;
                case TimeRanges.ThisWeek:
                    rangeTypeName = THIS_WEEK_SEARCH_RANGE_NAME;
                    break;
                case TimeRanges.LastWeek:
                    rangeTypeName = LAST_WEEK_SEARCH_RANGE_NAME;
                    break;
                case TimeRanges.ThisMonth:
                    rangeTypeName = THIS_MONTH_SEARCH_RANGE_NAME;
                    break;
                case TimeRanges.LastMonth:
                    rangeTypeName = LAST_MONTH_SEARCH_RANGE_NAME;
                    break;
                case TimeRanges.Anyday:
                    rangeTypeName = ANY_DAY_SEARCH_RANGE_NAME;
                    break;
                case TimeRanges.SpecifiedTimeRange:
                    rangeTypeName = SPECIFIED_SEARCH_RANGE_NAME;
                    break;
                // New enum was added so corresponding name must be added too.
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            return rangeTypeName;
        }

        /// <summary>
        /// Get rangeTypeName type corresponding for this name.
        /// </summary>
        /// <param name="rangeTypeName">Time rangeTypeName type name.</param>
        /// <returns>Corresponding value of TimeRanges enum.</returns>
        /// <remarks>Name isn't localizable, do not use in UI.</remarks>
        public static TimeRanges GetRangeType(string rangeTypeName)
        {
            TimeRanges rangeType = TimeRanges.Anyday;

            // Find enum value, corresponding to this name.
            switch (rangeTypeName)
            {
                case TODAY_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.Today;
                    break;
                case YESTERDAY_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.Yesterday;
                    break;
                case THIS_WEEK_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.ThisWeek;
                    break;
                case LAST_WEEK_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.LastWeek;
                    break;
                case THIS_MONTH_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.ThisMonth;
                    break;
                case LAST_MONTH_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.LastMonth;
                    break;
                case SPECIFIED_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.SpecifiedTimeRange;
                    break;
                case ANY_DAY_SEARCH_RANGE_NAME:
                    rangeType = TimeRanges.Anyday;
                    break;
                // There is no time rangeTypeName with such name.
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            return rangeType;
        }

        /// <summary>
        /// Get time rangeTypeName from start of start date of week to start of end date of week
        /// </summary>
        /// <param name="currentDate">Current date</param>
        /// <returns>Time rangeTypeName from start of start date of week to start of end date of week</returns>
        private static TimeRange _GetWeekRange(DateTime currentDate)
        {
            // Get first day of week for current locality
            DayOfWeek firstDayOfWeekLocality = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            DayOfWeek dayOfWeek = currentDate.DayOfWeek;

            int dayDiff = (int)firstDayOfWeekLocality - (int)dayOfWeek;
            // Get start date for this week
            DateTime firstDayOfWeek = currentDate.AddDays(dayDiff);
            // Get end date for this week
            DateTime lastDayOfWeek = currentDate.AddDays(6 + dayDiff);
            
            TimeRange result = new TimeRange(firstDayOfWeek, lastDayOfWeek);
            return result;
        }

        private static TimeRange _GetMonthRange(DateTime currentDate)
        {
            DateTime firstDayOfMonth = currentDate.AddDays(1 - currentDate.Day);
            
            int dayInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
            DateTime lastDayOfMonth = currentDate.AddDays(dayInMonth - currentDate.Day);

            TimeRange result = new TimeRange(firstDayOfMonth, lastDayOfMonth);
            return result;
        }


        #region private constants

        /// <summary>
        /// Searh ranges names. Used for saving current search rangeTypeName to settings file.
        /// </summary>
        private const string TODAY_SEARCH_RANGE_NAME = "Today";
        private const string YESTERDAY_SEARCH_RANGE_NAME = "Yesterday";
        private const string THIS_WEEK_SEARCH_RANGE_NAME = "This Week";
        private const string LAST_WEEK_SEARCH_RANGE_NAME = "Last Week";
        private const string THIS_MONTH_SEARCH_RANGE_NAME = "This Month";
        private const string LAST_MONTH_SEARCH_RANGE_NAME = "Last Month";
        private const string SPECIFIED_SEARCH_RANGE_NAME = "Specified Time Range";
        private const string ANY_DAY_SEARCH_RANGE_NAME = "Any day";

        #endregion
    }
}
