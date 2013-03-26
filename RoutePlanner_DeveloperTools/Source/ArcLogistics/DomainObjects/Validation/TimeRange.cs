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

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Class that represents a time range.
    /// </summary>
    internal class TimeRange : ICloneable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <c>TimeRange<c> class.
        /// </summary>
        public TimeRange()
        {
            From = TimeSpan.Zero;
            To = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of <c>TimeRange<c> class.
        /// </summary>
        /// <param name="from">Start time.</param>
        /// <param name="to">End time.</param>
        /// <exception cref="ArgumentNullException">'From' or 'to' is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">'From' is more then 'to'.</exception>
        public TimeRange(TimeSpan from, TimeSpan to)
        {
            if (From == null || To == null)
                throw new ArgumentNullException();

            if (From > To)
                throw new ArgumentOutOfRangeException();

            From = from;
            To = to;
        }

        /// <summary>
        /// Initializes a new instance of <c>TimeRange<c> class.
        /// </summary>
        /// <param name="timeWindow">TimeWindow, based on which time window will be created.</param>
        /// <exception cref="ArgumentNullException">TimeWindow is null.</exception>
        public TimeRange(TimeWindow timeWindow)
        {
            if (timeWindow == null)
                throw new ArgumentNullException();

            if (timeWindow.IsWideOpen)
            {
                From = TimeSpan.MinValue;
                To = new TimeSpan(TimeSpan.MaxValue.Ticks / 2);
            }
            else
            {
                From = timeWindow.EffectiveFrom;
                To = timeWindow.EffectiveTo;
            }
        }

        #endregion

        #region Static Method

        /// <summary>
        /// Get intersection of two TimeRanges.
        /// </summary>
        /// <param name="firstStart">First time range start.</param>
        /// <param name="firstFinish">First time range finish.</param>
        /// <param name="secondStart">Second time range start.</param>
        /// <param name="secondFinish">Second time range finish.</param>
        /// <returns>If they intersects - return intersection 
        /// time range, otherwise - return 'null'.</returns>
        public static TimeRange Intersection(TimeSpan firstStart,
            TimeSpan firstFinish, TimeSpan secondStart, TimeSpan secondFinish)
        {
            // Check that both time ranges are valid.
            if (firstStart == null || firstFinish == null || secondStart == null ||
                secondFinish == null)
                return null;

            var firstRange = new TimeRange(firstStart, firstFinish);
            return firstRange.Intersection(new TimeRange(firstStart, firstFinish));
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Length of the time range.
        /// </summary>
        public TimeSpan Length
        {
            get
            {
                return To - From;
            }
        }

        /// <summary>
        /// Start of the time range.
        /// </summary>
        public TimeSpan From
        {
            get;
            set;
        }

        /// <summary>
        /// End of the time range.
        /// </summary>
        public TimeSpan To
        {
            get;
            set;
        }

        #endregion
        
        #region Public Methods

        /// <summary>
        /// Shifts time range.
        /// This method is immutable.
        /// </summary>
        /// <param name="time">Delta time to shift this time range.</param>
        /// <returns>Shifted timerange.</returns>
        /// <exception cref="ArgumentNullException">Time is null.</exception>
        public TimeRange Shift(TimeSpan time)
        {
            if (time == null)
                throw new ArgumentNullException();

            return new TimeRange(From + time, To + time);
        }

        /// <summary>
        /// Get intersection of two TimeRanges.
        /// </summary>
        /// <param name="second">Second timerange.</param>
        /// <returns>If they intersects - return intersection 
        /// time range, otherwise - return 'null'.</returns>
        public TimeRange Intersection(TimeRange second)
        {
            // If second TimeRange is null - return null.
            if (second == null)
                return null;

            // Detect intersection time range.
            var start = this.From > second.From ? this.From : second.From;
            var finish = this.To < second.To ? this.To : second.To;
            if (start <= finish)
            {
                return new TimeRange(start, finish);
            }
            else
                return null;
        }

        /// <summary>
        /// Get intersection of two TimeRanges.
        /// </summary>
        /// <param name="secondStart">Second time range start.</param>
        /// <param name="secondFinish">Second time range finish.</param>
        /// <returns>If they intersects - return intersection 
        /// time range, otherwise - return 'null'.</returns>
        public TimeRange Intersection(TimeSpan secondStart, TimeSpan secondFinish)
        {
            // Check that second timerange isnt null.
            if (secondStart == null || secondFinish == null)
                return null;

            return Intersection(new TimeRange(secondStart, secondFinish));
        }

        /// <summary>
        /// Get intersection of two TimeRanges.
        /// </summary>
        /// <param name="second">Second TimeRange.</param>
        /// <returns>'True' if they intersects, otherwise no.</returns>
        public bool Intersects(TimeRange second)
        {
            return this.Intersection(second) != null;
        }

        /// <summary>
        /// Get intersection of two time ranges.
        /// </summary>
        /// <param name="secondStart">Second time range start.</param>
        /// <param name="secondFinish">Second time range finish.</param>
        /// <returns>If they intersects - return intersection 
        /// time range, otherwise - return 'null'.</returns>
        public bool Intersects(TimeSpan secondFrom, TimeSpan secondTo)
        {
            return Intersection(new TimeRange(secondFrom, secondTo)) != null;
        }

        #endregion

        #region ICloneable members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            TimeRange obj = new TimeRange();

            obj.From = From;
            obj.To = To;

            return obj;
        }

        #endregion ICloneable members
    }
}
