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
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics.DomainObjects.Validation;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents route recurrence settings.
    /// </summary>
    public class Days : ICloneable, INotifyPropertyChanged
    {
        #region Public Static Propeties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // property names

        /// <summary>
        /// Name of the Days property.
        /// </summary>
        public static string PropertyNameDays
        {
            get { return PROP_NAME_Days; }
        }

        /// <summary>
        /// Name of the DatesFrom property.
        /// </summary>
        public static string PropertyNameDatesFrom
        {
            get { return PROP_NAME_DatesFrom; }
        }

        /// <summary>
        /// Name of the DatesTo property.
        /// </summary>
        public static string PropertyNameDatesTo
        {
            get { return PROP_NAME_DatesTo; }
        }


        #endregion // public static methods

        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Days</c> class.
        /// </summary>
        public Days()
        {
            _startDate = _endDate = null;
            _InitWeekDays(true);
        }

        #endregion // Constructor

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Indicates whether route is available for the specified day of week. 
        /// </summary>
        /// <param name="day">Day of week to check.</param>
        /// <returns>Returns <c>true</c> if route is available for the <c>day</c>.</returns>
        public bool IsDayEnabled(DayOfWeek day)
        {
            return _enabledDays.Contains(day);
        }

        /// <summary>
        /// Makes route available or not available for the specified day of week.
        /// </summary>
        /// <param name="day">Day of week.</param>
        /// <param name="isEnabled">Flag that indicates either <c>day</c> should be included or excluded from route availability days.</param>
        public void EnableDay(DayOfWeek day, bool isEnabled)
        {
            bool isNeedUpdate = (isEnabled)? !_enabledDays.Contains(day) : _enabledDays.Contains(day);
            if (isNeedUpdate)
            {
                if (isEnabled)
                    _enabledDays.Add(day);
                else
                    _enabledDays.Remove(day);
                _NotifyPropertyChanged(PROP_NAME_Days);
            }
        }

        /// <summary>
        /// Start date when the route becomes available.
        /// </summary>
        /// <remarks>
        /// <c>null</c> means that there is no start date.
        /// </remarks>
        public DateTime? From
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                _NotifyPropertyChanged(PROP_NAME_DatesFrom);
            }
        }

        /// <summary>
        /// Finish date when the route stops to be available.
        /// </summary>
        /// <remarks>
        /// <c>null</c> means that there is no finish date.
        /// </remarks>
        public DateTime? To
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                _NotifyPropertyChanged(PROP_NAME_DatesTo);
            }
        }

        #endregion // Public members

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Event which is invoked when any of the object's properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion // Public events

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            string content = null;
            string days = _DaysToString();
            if (days == Properties.Resources.NoEffectiveDays)
                content = days;
            else
            {
                string dates = _DatesToString();
                content = ((days == Properties.Resources.EverydayDays) && (dates.Contains(Properties.Resources.Everyday))) ?
                            days : string.Format("{0} {1}", days, dates);
            }

            return content;
        }

        /// <summary>
        /// Checks either specific day satisfies current recurrence settings.
        /// </summary>
        /// <param name="day">Date to check.</param>
        /// <returns>Returns <c>true</c> if route is available for the <c>day</c>.</returns>
        public bool DoesDaySatisfy(DateTime day)
        {
            bool doesDaySatisfy = true;
            if (From.HasValue)
            {
                doesDaySatisfy = (From.Value <= day);
                if (To.HasValue)
                    doesDaySatisfy &= (day <= To.Value);
            }

            if (doesDaySatisfy)
                doesDaySatisfy = IsDayEnabled(day.DayOfWeek);

            return doesDaySatisfy;
        }

        #endregion // Public methods

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Clones the Days object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            Days obj = new Days();

            // days of week
            obj._enabledDays = new List<DayOfWeek>();
            foreach (DayOfWeek day in this._enabledDays)
                obj._enabledDays.Add(day);

            obj.From = this.From;
            obj.To = this.To;

            return obj;
        }

        #endregion // ICloneable members

        #region Public static methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        internal static string AssemblyDBString(Days days)
        {
            // NOTE: format
            //  version\_weeklyPattern\_dates
            string result = string.Empty;
            result += VERSION_CURR;
            result += CommonHelpers.SEPARATOR_OF_PART + _AssemblyDaysDBString(days._enabledDays);
            result += CommonHelpers.SEPARATOR_OF_PART + _AssemblyDatesDBString(days._startDate, days._endDate);
            return result;
        }

        internal static Days CreateFromDBString(string value)
        {
            Days days = new Days();
            if (null != value)
            {
                string[] values = value.Split(new char[] { CommonHelpers.SEPARATOR_OF_PART }, StringSplitOptions.None);

                Debug.Assert(3 == values.Length);
                int version = (int)TypeDescriptor.GetConverter(typeof(int)).ConvertFrom(values[0]);
                    // NOTE: now not used

                days._enabledDays = _CreateDaysFromDBString(values[1]);

                // create dates
                string datesStting = values[2].Replace(CommonHelpers.SEPARATOR_OLD, CommonHelpers.SEPARATOR); // NOTE: for support old projects

                string[] valuesDates = datesStting.Split(new char[] { CommonHelpers.SEPARATOR }, StringSplitOptions.None);
                if (0 < valuesDates.Length)
                {
                    if (!string.IsNullOrEmpty(valuesDates[0]))
                        days._startDate = _CreateDateFromDBString(valuesDates[0]);

                    if (1 < valuesDates.Length)
                    {
                        Debug.Assert(2 == valuesDates.Length);
                        if (!string.IsNullOrEmpty(valuesDates[1]))
                            days._endDate = _CreateDateFromDBString(valuesDates[1]);
                    }
                }
            }

            return days;
        }

        #endregion // Public static methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

        private void _InitWeekDays(bool isAllEnabled)
        {
            _enabledDays = new List<DayOfWeek>();

            if (isAllEnabled)
            {
                DayOfWeek[] days = (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek));
                for (int index = 0; index < days.Length; ++index)
                    _enabledDays.Add(days[index]);
            }
        }

        private string _DaysToString()
        {
            string content = string.Empty;
            if (0 == _enabledDays.Count)
                content = Properties.Resources.NoEffectiveDays;
            else
            {
                // NOTE: use format Mon, Tue and Fri (honor days order. e.g. for US Sun is the first day
                DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

                DayOfWeek[] days = (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek));
                if (days.Length == _enabledDays.Count)
                    content = Properties.Resources.EverydayDays;
                else
                {
                    StringCollection selectedDays = new StringCollection();
                    _MakeDaysList((int)firstDayOfWeek, days.Length, days, ref selectedDays);
                    _MakeDaysList(0, (int)firstDayOfWeek, days, ref selectedDays);

                    int count = selectedDays.Count;
                    if (1 == count)
                        content = selectedDays[0];
                    else if (2 == count)
                        content = selectedDays[0] + Properties.Resources.SplitterAnd + selectedDays[1];
                    else if (2 < count)
                    {   // Count - 1  for last added element need use special splitter
                        for (int index = 0; index < selectedDays.Count - 1; ++index)
                        {
                            if (0 < content.Length)
                                content += ", ";

                            content += selectedDays[index];
                        }

                        content += Properties.Resources.SplitterAnd + selectedDays[selectedDays.Count - 1];
                    }
                }
            }

            return content;
        }

        private void _MakeDaysList(int startIndex, int count, DayOfWeek[] days, ref StringCollection selectedDays)
        {
            for (int index = startIndex; index < count; ++index)
            {
                if (IsDayEnabled(days[index]))
                    selectedDays.Add(CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[index]);
            }
        }

        private static string _AssemblyDaysDBString(List<DayOfWeek> days)
        {
            string result = string.Empty;
            foreach (DayOfWeek day in days)
            {
                if (0 < result.Length)
                    result += CommonHelpers.SEPARATOR;

                result += ((int)day).ToString();
            }

            return result;
        }

        private static List<DayOfWeek> _CreateDaysFromDBString(string value)
        {
            List<DayOfWeek> days = new List<DayOfWeek>();
            if (null != value)
            {
                value = value.Replace(CommonHelpers.SEPARATOR_OLD, CommonHelpers.SEPARATOR); // NOTE: for support old projects

                string[] values = value.Split(new char[] { CommonHelpers.SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);

                for (int index = 0; index < values.Length; ++index)
                    days.Add((DayOfWeek)Enum.Parse(typeof(DayOfWeek), values[index]));
            }

            return days;
        }

        private string _DatesToString()
        {
            // NOTE:
            //   effective everyday
            //   effective 11.05.2009 // in case EndDate is null
            //   effective 11.05.2009 until 01.06.2009 // in case both dates are specified.

            string range = string.Empty;
            if (!From.HasValue && !To.HasValue)
                range = Properties.Resources.Everyday;
            else
            {
                string start = (From.HasValue)? From.Value.ToString("d", CultureInfo.CurrentCulture) : string.Empty;
                range = (To.HasValue) ? string.Format(Properties.Resources.DaysRangeSegmentFormat, start, To.Value.ToString("d", CultureInfo.CurrentCulture)) : start;
            }

            return string.Format(Properties.Resources.DaysRangeFormat, range);
        }

        private static string _AssemblyDatesDBString(DateTime? from, DateTime? to)
        {
            return _GetDateDBString(from) + CommonHelpers.SEPARATOR + _GetDateDBString(to);
        }

        private static string _GetDateDBString(DateTime? date)
        {
            CultureInfo culture = new CultureInfo(CommonHelpers.STORAGE_CULTURE);
            return (date.HasValue)? date.Value.ToString(culture.DateTimeFormat.ShortDatePattern, culture) : string.Empty;
        }

        private static DateTime _CreateDateFromDBString(string date)
        {
            CultureInfo culture = new CultureInfo(CommonHelpers.STORAGE_CULTURE);
            return  DateTime.Parse(date, culture);
        }
        #endregion // Private methods

        #region private constants

        /// <summary>
        /// Name of the Days property.
        /// </summary>
        private const string PROP_NAME_Days = "Days";

        /// <summary>
        /// Name of the DatesFrom property.
        /// </summary>
        private const string PROP_NAME_DatesFrom = "DatesFrom";

        /// <summary>
        /// Name of the DatesTo property.
        /// </summary>
        private const string PROP_NAME_DatesTo = "DatesTo";

        #endregion

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<DayOfWeek> _enabledDays;
        private DateTime? _startDate;
        private DateTime? _endDate;

        private const int VERSION_1 = 0x00000001;
        private const int VERSION_CURR = VERSION_1;
        #endregion // Private members
    }
}
