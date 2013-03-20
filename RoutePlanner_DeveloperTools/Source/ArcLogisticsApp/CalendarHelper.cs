using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class contains helper method for custom calendar controls
    /// </summary>
    internal class CalendarHelper
    {
        #region Public Methods

        /// <summary>
        /// Returns start date of calendar range (the 1st day of last week in previous month)
        /// </summary>
        /// <returns></returns>
        public static DateTime GetStartRangeDate(DateTime currentDate)
        {
            DateTime displayDate = currentDate;

            // previous month number is current month number - 1 or 12(if current month is January)
            int month = (displayDate.Month > FIRST_MONTH_NUMBER) ? displayDate.Month - 1 : LAST_MONTH_NUMBER;

            // year number is current year number - 1 (if current month is December) or equals current year number
            int year = (month == LAST_MONTH_NUMBER && displayDate.Year > 1) ? displayDate.Year - 1 : displayDate.Year;
            int dayInMonth = DateTime.DaysInMonth(year, month) - DAYS_IN_WEEK;

            Debug.Assert(year > 1753 && year < 9999);

            return new DateTime(year, month, dayInMonth);;
        }

        /// <summary>
        /// Returns end date of calendar range (the 14th of next month)
        /// </summary>
        /// <returns></returns>
        public static DateTime GetEndRangeDate(DateTime currentDate)
        {
            DateTime displayDate = (DateTime)currentDate;

            // next month number is current month number + 1 or 1(if current month is December)
            int month = (displayDate.Month < LAST_MONTH_NUMBER) ? displayDate.Month + 1 : FIRST_MONTH_NUMBER;

            // year number is current year number + 1 (if current month is January) or equals current year number
            int year = (month == FIRST_MONTH_NUMBER) ? displayDate.Year + 1 : displayDate.Year;
            int dayInMonth = DAYS_IN_WEEK * 2; // get 14th day of month

            Debug.Assert(year > 1753 && year < 9999);

            return new DateTime(year, month, dayInMonth);
        }

        #endregion

        #region Private Fields

        private const int LAST_MONTH_NUMBER = 12;
        private const int FIRST_MONTH_NUMBER = 1;
        private const int DAYS_IN_WEEK = 7;

        #endregion
    }
}
