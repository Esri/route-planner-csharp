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
using System.Text;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Validation;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// TimeIntervalBreak class represents a base abstract class for other breaks
    /// with specific interval.
    /// </summary>
    public abstract class TimeIntervalBreak : Break
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>TimeIntervalBreak</c> class.
        /// </summary>
        protected TimeIntervalBreak()
        { }

        #endregion // Constructors

        #region Public static properties

        /// <summary>
        /// Gets name of the TimeInterval property.
        /// </summary>
        public static string PropertyNameTimeInterval
        {
            get { return PROP_NAME_TIMEINTERVAL; }
        }

        #endregion // Public static properties

        #region Public members

        /// <summary>
        /// Time interval in hours.
        /// </summary>
        /// <remarks>It is a time interval (in hours) from previous break (or route start if a
        /// break is the first) that must be spent before this break takes place on a route.</remarks>
        [TimeIntervalValidator]
        [DomainProperty("DomainPropertyNameInterval")]
        [UnitPropertyAttribute(Unit.Hour, Unit.Hour, Unit.Hour)]
        public double TimeInterval
        {
            get { return _timeInterval; }
            set
            {
                if (_timeInterval == value)
                    return;

                _timeInterval = value;

                _NotifyPropertyChanged(PROP_NAME_TIMEINTERVAL);
            }
        }

        /// <summary>
        /// Returns a string representation of the break information.
        /// </summary>
        /// <returns>Break's string.</returns>
        public override string ToString()
        {
            return string.Format(Properties.Resources.BreakIntervalFormat,
                                 base.ToString(),
                                 TimeInterval.ToString());
        }

        #endregion // Public members

        #region Internal Properties

        internal double DefaultTimeInterval
        {
            get
            {
                return DEFAULT_TIME_INTERVAL;
            }
        }

        #endregion

        #region Internal overrided methods

        /// <summary>
        /// Check that both breaks have same types and same Duration and TimeInterval values.
        /// </summary>
        /// <param name="breakObject">Brake to compare with this.</param>
        /// <returns>'True' if breaks types and  Duration and TimeInterval
        /// values are the same, 'false' otherwise.</returns>
        internal override bool EqualsByValue(Break breakObject)
        {
            TimeIntervalBreak breakToCompare = breakObject as TimeIntervalBreak;
            return breakToCompare != null && base.EqualsByValue(breakObject) &&
                breakToCompare.TimeInterval == this.TimeInterval;
        }

        /// <summary>
        /// Converts state of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        internal override string ConvertToString()
        {
            var result = new StringBuilder();

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE);

            // 0. Current version
            result.Append(VERSION_CURR.ToString());
            // 1. Duration
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(Duration.ToString(cultureInfo));
            // 2. Interval
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(TimeInterval.ToString(cultureInfo));

            return result.ToString();
        }

        /// <summary>
        /// Method occured, when breaks collection changed. Need for validation.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        protected override void BreaksCollectionChanged(object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // If breaks collection changed - raise notify property 
            // changed for "TimeInterval" property for all breaks in this collection.
            _NotifyPropertyChanged(PropertyNameTimeInterval);
        }
        #endregion // Internal overrided methods

        #region Protected methods

        /// <summary>
        /// Converts the string representation of a break to break's values.
        /// </summary>
        /// <param name="context">String representation of a break.</param>
        /// <param name="intervalBreak">Parsed break's.</param>
        protected static void _Parse(string context, TimeIntervalBreak intervalBreak)
        {
            Debug.Assert(!string.IsNullOrEmpty(context));

            char[] valuesSeparator = new char[1] { CommonHelpers.SEPARATOR };
            string[] values = context.Split(valuesSeparator, StringSplitOptions.None);
            Debug.Assert(3 == values.Length);

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE);
            int index = 0;
            // 0. Current version, now not used
            // 1. Duration
            intervalBreak.Duration = double.Parse(values[++index], cultureInfo);
            // 2. Interval
            intervalBreak.TimeInterval = double.Parse(values[++index], cultureInfo);
        }

        /// <summary>
        /// Copies all the breaks's data to the target interval break.
        /// </summary>
        /// <param name="intervalBreak">Target interval break.</param>
        protected void _CopyTo(TimeIntervalBreak intervalBreak)
        {
            intervalBreak.Duration = this.Duration;
            intervalBreak.TimeInterval = this.TimeInterval;
        }

        #endregion // Protected methods

        #region Private constants

        /// <summary>
        /// Name of the TimeInterval property.
        /// </summary>
        private const string PROP_NAME_TIMEINTERVAL = "TimeInterval";

        /// <summary>
        /// Storage schema version 1.
        /// </summary>
        private const int VERSION_1 = 0x00000001;
        /// <summary>
        /// Storage schema current version.
        /// </summary>
        private const int VERSION_CURR = VERSION_1;

        /// <summary>
        /// Default break's time interval.
        /// </summary>
        private const double DEFAULT_TIME_INTERVAL = 4;

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Time interval.
        /// </summary>
        private double _timeInterval;

        #endregion // Private members
    }

    /// <summary>
    /// Class that represents a drive time break.
    /// Specify how long a person can drive before the break is required.
    /// (Note that only travel time is limited, not other times like wait and service times).
    /// </summary>
    public class DriveTimeBreak : TimeIntervalBreak
    {
        #region Constructors
        
        /// <summary>
        /// Initializes a new instance of the <c>DriveTimeBreak</c> class.
        /// </summary>
        public DriveTimeBreak()
        {
            Duration = DefautDuration;
            TimeInterval = DefaultTimeInterval;
        }

        #endregion 

        #region ICloneable members

        /// <summary>
        /// Clones the <c>TimeWindowBreak</c> object.
        /// </summary>
        /// <returns>Cloned object.</returns>
        public override object Clone()
        {
            var obj = new DriveTimeBreak();
            _CopyTo(obj);

            return obj;
        }

        #endregion // ICloneable members

        #region Internal overrided methods
        
        /// <summary>
        /// Converts the string representation of a break to break internal state equivalent.
        /// </summary>
        /// <param name="context">String representation of a break.</param>
        internal override void InitFromString(string context)
        {
            if (null != context)
                _Parse(context, this);
        }

        #endregion // Internal overrided methods
    }

    /// <summary>
    /// Class that represents a work time break.
    /// Specifies how long a person can work before a break is required.
    /// This breaks always accumulate work time from the beginning of the route, including any
    /// service time at the start depot (which includes travel time and all service times;
    /// it excludes wait time, however).
    /// </summary>
    public class WorkTimeBreak : TimeIntervalBreak
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>WorkTimeBreak</c> class.
        /// </summary>
        public WorkTimeBreak()
        {
            Duration = DefautDuration;
            TimeInterval = DefaultTimeInterval;
        }

        #endregion

        #region ICloneable members

        /// <summary>
        /// Clones the <c>TimeWindowBreak</c> object.
        /// </summary>
        /// <returns>Cloned object.</returns>
        public override object Clone()
        {
            var obj = new WorkTimeBreak();
            _CopyTo(obj);

            return obj;
        }

        #endregion

        #region Internal overrided methods

        /// <summary>
        /// Converts the string representation of a break to break internal state equivalent.
        /// </summary>
        /// <param name="context">String representation of a break.</param>
        internal override void InitFromString(string context)
        {
            if (null != context)
                _Parse(context, this);
        }

        #endregion 
    }
}
