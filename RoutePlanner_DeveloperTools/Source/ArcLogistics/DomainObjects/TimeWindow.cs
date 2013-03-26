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
using System.ComponentModel;
using System.Diagnostics;

using ESRI.ArcLogistics.DomainObjects.Attributes;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a time window.
    /// </summary>
    public class TimeWindow : ICloneable, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <c>TimeWindow<c> class.
        /// </summary>
        public TimeWindow()
        {
            _isWideOpen = true;
            _from = TimeSpan.Zero;
            _to = TimeSpan.Zero;
            _day = 0;
        }

        /// <summary>
        /// Initializes a new instance of <c>TimeWindow<c> class.
        /// </summary>
        /// <param name="from">Start time.</param>
        /// <param name="to">End time.</param>
        /// <exception cref="ArgumentException">Length of 'from' or 'to' time span 
        /// is more than or equal to 24 hours.</exception>
        public TimeWindow(TimeSpan from, TimeSpan to)
        {
            // Check time window boundaries and throw exception if either of them is invalid.
            _CheckTimeWindowBoundariesTime(from, to);

            _from = from;
            _to = to;
            _day = 0;
            _isWideOpen = false;
        }

        /// <summary>
        /// Initializes a new instance of <c>TimeWindow<c> class.
        /// </summary>
        /// <param name="from">Start time.</param>
        /// <param name="to">End time.</param>
        /// <param name="day">Day.</param>
        /// <exception cref="ArgumentException">Length of 'from' or 'to' time span 
        /// is more than or equal to 24 hours.</exception>
        public TimeWindow(TimeSpan from, TimeSpan to, uint day)
        {
            // Check time window boundaries and throw exception if either of them is invalid.
            _CheckTimeWindowBoundariesTime(from, to);

            _from = from;
            _to = to;
            _day = day;
            _isWideOpen = false;
        }

        #endregion Constructors

        #region Public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the From property
        /// </summary>
        public static string PropertyNameFrom
        {
            get { return PROP_NAME_FROM; }
        }

        /// <summary>
        /// Name of the To property
        /// </summary>
        public static string PropertyNameTo
        {
            get { return PROP_NAME_TO;}
        }

        /// <summary>
        /// Name of the Day property.
        /// </summary>
        public static string PropertyNameDay
        {
            get { return PROP_NAME_DAY; }
        }

        /// <summary>
        /// Name of the IsWideOpen property
        /// </summary>
        public static string PropertyNameIsWideOpen
        {
            get { return PROP_NAME_ISWIDEOPEN;}
        }

        #endregion Public static properties

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Fired when property value is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public events

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Time of day when the time window opens.
        /// <exception cref="ArgumentException">Length of time span to set is more than or equal to 24 hours.</exception>
        /// </summary>
        [DomainProperty("DomainPropertyNameTimeWindowFrom")]
        public TimeSpan From
        {
            get { return _from; }
            set
            {
                if (value != _from)
                {
                    if (value.TotalHours >= HOURS_PER_DAY)
                        throw new ArgumentException(Properties.Messages.Error_TimeWindowFromParameter);

                    _from = value;
                    _NotifyPropertyChanged(PROP_NAME_FROM);
                }
            }
        }

        /// <summary>
        /// Time of day when the time window closes.
        /// </summary>
        /// <exception cref="ArgumentException">Length of time span to set is more than or equal to 24 hours.</exception>
        /// <remarks>
        /// If <c>To</c> value is less than <c>From</c> value then this means that the time window closes at the
        /// specified time but on the next day.
        /// </remarks>
        [DomainProperty("DomainPropertyNameTimeWindowTo")]
        public TimeSpan To
        {
            get { return _to; }
            set
            {
                if (value != _to)
                {
                    if (value.TotalHours >= HOURS_PER_DAY)
                        throw new ArgumentException(Properties.Messages.Error_TimeWindowToParameter);

                    _to = value;
                    _NotifyPropertyChanged(PROP_NAME_TO);
                }
            }
        }

        /// <summary>
        /// Day from PlannedDate when this time window is active.
        /// Day = 0 (default) means that time window relates to the order/route planned date. 
        /// Day = 1, means that time window relates to the next day after planned and so on.
        /// </summary>
        [DomainProperty("DomainPropertyNameTimeWindowDay")]
        public uint Day
        {
            get { return _day; }
            set
            {
                if (value != _day)
                {
                    _day = value;
                    _NotifyPropertyChanged(PROP_NAME_DAY);
                }
            }
        }

        /// <summary>
        /// Indicates whether the time window is wideopen. This property must be set explicitly if the TimeWindow is modified.
        /// </summary>
        public bool IsWideOpen
        {
            get { return _isWideOpen; }
            set
            {
                if (value != _isWideOpen)
                {
                    _isWideOpen = value;
                    _NotifyPropertyChanged(PROP_NAME_ISWIDEOPEN);
                }
            }
        }

        /// <summary>
        /// Gets time passed since planned date midnight till the start of time window.
        /// </summary>
        public TimeSpan EffectiveFrom
        {
            get
            {
                TimeSpan effectiveFrom = new TimeSpan((int)(_day * HOURS_PER_DAY), 0, 0);
                effectiveFrom += _from;

                return effectiveFrom;
            }
        }

        /// <summary>
        /// Gets time passed since planned date midnight till the end of time window.
        /// </summary>
        public TimeSpan EffectiveTo
        {
            get
            {
                // Day of time window end time.
                uint endDay = (_from > _to) ? _day + 1 : _day;

                TimeSpan effectiveTo = new TimeSpan((int)(endDay * HOURS_PER_DAY), 0, 0);
                effectiveTo += _to;

                return effectiveTo;
            }
        }

        /// <summary>
        /// Gets time window's length.
        /// If timewindow is wideopen - return maximum timespan ticks.
        /// </summary>
        public TimeSpan Length
        {
            get
            {
                TimeSpan timeWindowLength;

                // If timewindow isn't wideopen - calculate its duration.
                if (!IsWideOpen)
                    timeWindowLength = EffectiveTo - EffectiveFrom;
                // There is no duration.
                else
                    timeWindowLength = new TimeSpan(TimeSpan.MaxValue.Ticks);

                return timeWindowLength;
            }
        }

        #endregion Public properties

        #region Public methods
        
        /// <summary>
        /// Returns a string represtentation of the time window.
        /// If time window is wideopen function returns "Wideopen" string.
        /// </summary>
        /// <returns>String representation of time window.</returns>
        public override string ToString()
        {
            string timeWindowString = string.Empty;

            // If time window is wide open.
            if (_isWideOpen)
            {
                timeWindowString = Properties.Resources.Wideopen;
            }
            // Else format time window string.
            else
            {
                // Date time from.
                DateTime dateTimeFrom = new DateTime();
                dateTimeFrom += _from;

                // Date time to.
                DateTime dateTimeTo = new DateTime();
                dateTimeTo += _to;

                // Flag defines if time window crosses midnight.
                bool midnightIsCrossed = _from > _to;

                // Day of time window start time.
                uint startDay = _day;

                // Day of time window end time.
                uint endDay = midnightIsCrossed ? _day + 1 : _day;

                // String representation of start time.
                string startTimeString = dateTimeFrom.ToShortTimeString() + _GetDaySuffix(startDay);

                // String representation of end time.
                string endTimeString = dateTimeTo.ToShortTimeString() + _GetDaySuffix(endDay);

                timeWindowString =
                    string.Format(Properties.Resources.TimeWindowFormat, startTimeString, endTimeString);
            }

            return timeWindowString;
        }

        /// <summary>
        /// Checks if input time is inside the time window.
        /// </summary>
        /// <param name="time">Time to check.</param>
        /// <returns>True - if current time is inside the time window, false - otherwise.</returns>
        [Obsolete("Don't use this method. Use this one instead: DoesIncludeTime(DateTime currentTime, DateTime plannedDate).", true)]
        public bool DoesIncludeTime(DateTime time)
        {
            bool result = false;

            // If window is wide open.
            if (_isWideOpen)
            {
                result = true;
            }
            // Check if current time is inside time window.
            else
            {
                result = (time.TimeOfDay >= EffectiveFrom) &&
                         (time.TimeOfDay <= EffectiveTo);
            }

            return result;
        }

        /// <summary>
        /// Checks if input time is inside the time window.
        /// </summary>
        /// <param name="currentTime">Time to check.</param>
        /// <param name="plannedDate">Planned date.</param>
        /// <returns>True - if current time is inside the time window, false - otherwise.</returns>
        public bool DoesIncludeTime(DateTime currentTime, DateTime plannedDate)
        {
            bool result = false;

            // If window is wide open.
            if (_isWideOpen)
            {
                result = true;
            }
            // Check if current time is inside time window.
            else
            {
                result = (currentTime >= plannedDate + EffectiveFrom) && 
                         (currentTime <= plannedDate + EffectiveTo);
            }

            return result;
        }

        /// <summary>
        /// Get intersection of two TimeWindow.
        /// </summary>
        /// <param name="first">First timewindow.</param>
        /// <param name="second">Second timewindow.</param>
        /// <returns>If they intersects - return intersection 
        /// timewindow, otherwise - return 'null'.</returns>
        public TimeWindow Intersection(TimeWindow second)
        {
            // If one of timewindow is null - return null.
            if (second == null)
                return null;

            // If one of timewindows is wideopen - return other.
            if (this.IsWideOpen)
                return second.Clone() as TimeWindow;
            else if (second.IsWideOpen)
                return this.Clone() as TimeWindow;

            // Detect intersection of two timewindows.
            var start = this.EffectiveFrom > second.EffectiveFrom ? 
                this.EffectiveFrom : second.EffectiveFrom;
            var finish = this.EffectiveTo < second.EffectiveTo ? 
                this.EffectiveTo : second.EffectiveTo;
            if (start <= finish)
            {
                return CreateFromEffectiveTimes(start, finish);
            }
            else
                return null;
        }

        /// <summary>
        /// Checks is time windows intersects or not.
        /// </summary>
        /// <param name="timeWindow">Second time window.</param>
        /// <returns>'True' if they are intersect, 'false' otherwise.</returns>
        public bool Intersects(TimeWindow timeWindow)
        {
            return (Intersection(timeWindow) != null);
        }

        #endregion Public methods

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            TimeWindow obj = new TimeWindow();

            obj._from = _from;
            obj._to = _to;
            obj._day = _day;
            obj._isWideOpen = _isWideOpen;

            return obj;
        }

        #endregion ICloneable members

        #region Internal methods

        /// <summary>
        /// Compares this time window with another one using values of their properties.
        /// </summary>
        /// <param name="secondTimeWindow">TimeWindow to compare with this.</param>
        /// <returns>True - if either both time windows are wide open or have equal 
        /// values of properties From, To and Day, otherwise - false.</returns>
        internal bool EqualsByValue(TimeWindow secondTimeWindow)
        {
            bool result = false;

            // If input parameter is invalid.
            if (secondTimeWindow == null || this.GetType() != secondTimeWindow.GetType())
            {
                result = false;
            }
            // If one of TimeWindow is WideOpen they both must be wide open.
            else if (IsWideOpen || secondTimeWindow.IsWideOpen)
            {
                result = IsWideOpen == secondTimeWindow.IsWideOpen;
            }
            // Compare From, To and Day properties.
            else
            {
                result = (From == secondTimeWindow.From &&
                          To == secondTimeWindow.To &&
                          Day == secondTimeWindow.Day);
            }

            return result;
        }

        #endregion Internal methods

        #region Internal static methods

        /// <summary>
        /// Creates a new instance of <c>TimeWindow<c> class using effective times.
        /// </summary>
        /// <param name="effectiveFrom">Time offset of time window start from planned date.</param>
        /// <param name="effectiveTo">Time offset of time window end from planned date.</param>
        /// <exception cref="ArgumentException">Invalid time window parameter(s).</exception>
        /// <returns>Created time window.</returns>
        internal static TimeWindow CreateFromEffectiveTimes(TimeSpan effectiveFrom, TimeSpan effectiveTo)
        {
            if (!_ValidateTimeWindowEffectiveTime(effectiveFrom, effectiveTo))
                throw new ArgumentException(Properties.Messages.Error_TimeWindowParameters);

            TimeSpan fromTime = _GetTimeWithoutDays(effectiveFrom);
            TimeSpan toTime = _GetTimeWithoutDays(effectiveTo);

            TimeWindow newTimeWindow = new TimeWindow(fromTime, toTime, (uint)effectiveFrom.Days);

            return newTimeWindow;
        }

        #endregion Internal static methods

        #region Private static methods

        /// <summary>
        /// Gets time without days (nulls days property of TimeSpan).
        /// </summary>
        /// <param name="timeSpan">Time span value.</param>
        /// <returns>Time without days.</returns>
        private static TimeSpan _GetTimeWithoutDays(TimeSpan timeSpan)
        {
            TimeSpan timeWithoutDays =
                new TimeSpan(0, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

            return timeWithoutDays;
        }

        /// <summary>
        /// Validates time window using it's effective from and effective to time.
        /// </summary>
        /// <param name="effectiveFrom">Time offset of time window start from planned date.</param>
        /// <param name="effectiveTo">Time offset of time window end from planned date.</param>
        /// <returns>True - if time window is valid, otrherwise - false.</returns>
        private static bool _ValidateTimeWindowEffectiveTime(TimeSpan effectiveFrom, TimeSpan effectiveTo)
        {
            bool validationResult = false;

            // Effective from time should be less then effective to time.
            if (effectiveFrom <= effectiveTo)
            {
                // If effectiveFrom and effectiveTo have equal value of days - time window is valid.
                if (effectiveFrom.Days == effectiveTo.Days)
                {
                    validationResult = true;
                }
                else
                {
                    validationResult = (effectiveTo.Days - effectiveFrom.Days == 1);
                }
            } // if (effectiveFrom <= effectiveTo)
            // Time window is invalid.
            else 
            {
                validationResult = false;
            }

            return validationResult;
        }

        #endregion Private static methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies that property is changed.
        /// </summary>
        /// <param name="propertyName">Name of property.</param>
        private void _NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets day suffix added to time on conversion of time window to string.
        /// For example if day index = 0 then day suffix is empty string, 
        /// if day index = 1 the day suffix is "(2)".
        /// </summary>
        /// <param name="dayIndex">Index of day.</param>
        /// <returns>Day suffix.</returns>
        private string _GetDaySuffix(uint dayIndex)
        {
            string daySuffix = string.Empty;

            if (dayIndex == 0)
                daySuffix = string.Empty;
            else
                daySuffix = string.Format(DAY_SUFFIX_FORMAT, dayIndex + 1);

            return daySuffix;
        }

        /// <summary>
        /// Checks from / to parameters of time window and throws exception if either of these parameters is invalid.
        /// </summary>
        /// <param name="from">Start time of time window.</param>
        /// <param name="to">End time of time window.</param>
        /// <exception cref="ArgumentException">Length of 'from' or 'to' time span 
        /// is more than or equal to 24 hours.</exception>
        private void _CheckTimeWindowBoundariesTime(TimeSpan from, TimeSpan to)
        {
            if (from.TotalHours >= HOURS_PER_DAY)
                throw new ArgumentException(Properties.Messages.Error_TimeWindowFromParameter);

            if (to.TotalHours >= HOURS_PER_DAY)
                throw new ArgumentException(Properties.Messages.Error_TimeWindowToParameter);
        }

        #endregion Private methods

        #region Private constants

        /// <summary>
        /// Name of the From property.
        /// </summary>
        private const string PROP_NAME_FROM = "From";

        /// <summary>
        /// Name of the To property.
        /// </summary>
        private const string PROP_NAME_TO = "To";

        /// <summary>
        /// Name of the Day property.
        /// </summary>
        private const string PROP_NAME_DAY = "Day";

        /// <summary>
        /// Name of the IsWideOpen property.
        /// </summary>
        private const string PROP_NAME_ISWIDEOPEN = "IsWideOpen";

        /// <summary>
        /// String format of day suffix used on conversion time window to string.
        /// </summary>
        private const string DAY_SUFFIX_FORMAT = " ({0})";

        /// <summary>
        /// Defines count of hours per day.
        /// </summary>
        private const uint HOURS_PER_DAY = 24;

        #endregion Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Low boundary time of time window.
        /// </summary>
        private TimeSpan _from;

        /// <summary>
        /// High boundary time of time window.
        /// </summary>
        private TimeSpan _to;

        /// <summary>
        /// Day from PlannedDate when this time window is active.
        /// </summary>
        private uint _day;

        /// <summary>
        /// Defines if time window is wide open.
        /// </summary>
        private bool _isWideOpen = true;

        #endregion Private members
    }
}
