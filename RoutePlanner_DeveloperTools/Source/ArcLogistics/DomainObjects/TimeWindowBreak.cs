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
using System.Globalization;
using System.Text;

using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Validation;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a break with specific time window.
    /// </summary>
    public class TimeWindowBreak : Break
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>TimeWindowBreak</c> class.
        /// </summary>
        public TimeWindowBreak()
        {
            // Set defaults.
            var from = new TimeSpan (DEFAULT_FROM_HOURS, 0, 0);
            var to = new TimeSpan(DEFAULT_TO_HOURS, 0, 0);
            _timeWindow = new TimeWindow(from, to);
            Duration = DefautDuration;
        }

        #endregion // Constructors

        #region Public static properties

        /// <summary>
        /// Gets name of the To property.
        /// </summary>
        public static string PropertyNameTo
        {
            get { return PROP_NAME_TO; }
        }

        /// <summary>
        /// Gets name of the Day property.
        /// </summary>
        public static string PropertyNameDay
        {
            get { return PROP_NAME_DAY; }
        }

        /// <summary>
        /// Gets name of the From property.
        /// </summary>
        public static string PropertyNameFrom
        {
            get { return PROP_NAME_FROM; }
        }

        #endregion // Public static properties

        #region Public members

        /// <summary>
        /// Time of day when the time window opens.
        /// </summary>
        public TimeSpan From
        {
            get { return _timeWindow.From; }
            set
            {
                if (value != _timeWindow.From)
                {
                    _timeWindow.From = value;
                    _NotifyPropertyChanged(PROP_NAME_FROM);
                }
            }
        }

        /// <summary>
        /// Time of day when the time window closes.
        /// </summary>
        /// <remarks>
        /// If <c>To</c> value is less than <c>From</c> value then this means that the time window closes at the specified time but on the next day.
        /// </remarks>
        [TimeWindowValidator]
        public TimeSpan To
        {
            get { return _timeWindow.To; }
            set
            {
                if (value != _timeWindow.To)
                {
                    _timeWindow.To = value;
                    _NotifyPropertyChanged(PROP_NAME_TO);
                }
            }
        }

        /// <summary>
        /// Day of the break.
        /// </summary>
        public uint Day
        {
            get { return _timeWindow.Day; }
            set
            {
                if (value != _timeWindow.Day)
                {
                    _timeWindow.Day = value;
                    _NotifyPropertyChanged(PROP_NAME_DAY);
                }
            }
        }

        /// <summary>
        /// Gets time passed since planned date midnight till the start of break.
        /// </summary>
        public TimeSpan EffectiveFrom
        {
            get { return _timeWindow.EffectiveFrom; }
        }

        /// <summary>
        /// Gets time passed since planned date midnight till the end of break.
        /// </summary>
        public TimeSpan EffectiveTo
        {
            get { return _timeWindow.EffectiveTo; }
        }

        /// <summary>
        /// Returns a string representation of the break information.
        /// </summary>
        /// <returns>Break's representation string.</returns>
        public override string ToString()
        {
            string timeWindow = _timeWindow.ToString();
            return string.Format(Properties.Resources.TimeWindowBrakeFormat, base.ToString(), timeWindow);
        }

        #endregion

        #region ICloneable members

        /// <summary>
        /// Clones the <c>TimeWindowBreak</c> object.
        /// </summary>
        /// <returns>Cloned object.</returns>
        public override object Clone()
        {
            var obj = new TimeWindowBreak();

            obj.To = this.To;
            obj.From = this.From;
            obj.Duration = this.Duration;
            obj.Day = this.Day;

            return obj;
        }

        #endregion 

        #region Internal overrided methods
        
        /// <summary>
        /// Check that both breaks have same types and same Duration, To and From values.
        /// </summary>
        /// <param name="breakObject">Brake to compare with this.</param>
        /// <returns>'True' if breaks types and  Duration, To and From
        /// values are the same, 'false' otherwise.</returns>
        internal override bool EqualsByValue(Break breakObject)
        {
            TimeWindowBreak breakToCompare = breakObject as TimeWindowBreak;
            return breakToCompare != null && base.EqualsByValue(breakObject) &&
                breakToCompare.From == this.From && breakToCompare.To == this.To;
        }

        /// <summary>
        /// Converts state of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        internal override string ConvertToString()
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE);

            var result = new StringBuilder();
            // 0. Current version
            result.Append(VERSION_CURR.ToString());
            // 1. Duration
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(Duration.ToString(cultureInfo));
            // 2. TW.From
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(From.ToString());
            // 3. TW.To
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(To.ToString());
            // 4. Day
            result.Append(CommonHelpers.SEPARATOR);
            result.Append(Day.ToString());

            return result.ToString();
        }

        /// <summary>
        /// Converts the string representation of a break to break internal state equivalent.
        /// </summary>
        /// <param name="context">String representation of a break.</param>
        internal override void InitFromString(string context)
        {
            if (null != context)
            {
                char[] valuesSeparator = new char[1] { CommonHelpers.SEPARATOR };
                string[] values = context.Split(valuesSeparator, StringSplitOptions.None);
                Debug.Assert((3 <= values.Length) && (values.Length <= 5));

                CultureInfo cultureInfo = CultureInfo.GetCultureInfo(CommonHelpers.STORAGE_CULTURE);
                int index = 0;
                int version = 0;

                // If we have 4 or more parts, this break is versioned.
                if (values.Length >= 4)
                {
                    version = int.Parse(values[0]);
                    ++index; // 0 - index is version
                    // first version has 3 elements
                }

                // 1. Duration
                this.Duration = double.Parse(values[index], cultureInfo);
                // 2. TW.From
                From = TimeSpan.Parse(values[++index]);
                // 3. TW.To
                To = TimeSpan.Parse(values[++index]);
                // 4. Day
                if (version >= VERSION_2)
                    Day = uint.Parse(values[++index]);
                else
                    Day = 0;
            }
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
            // changed for "To" property for all breaks in this collection.
            _NotifyPropertyChanged(PropertyNameTo);
        }

        #endregion // Internal overrided methods

        #region Private methods

        /// <summary>
        /// Notifies about change of the From property.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _FromPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _NotifyPropertyChanged(e.PropertyName);
            _NotifyPropertyChanged(PropertyNameFrom);
        }

        /// <summary>
        /// Notifies about change of the To property.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _ToPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _NotifyPropertyChanged(e.PropertyName);
            _NotifyPropertyChanged(PropertyNameTo);
        }

        #endregion 

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
        /// Name of the To property.
        /// </summary>
        private const string PROP_NAME_DAY = "Day";

        /// <summary>
        /// Default start hour.
        /// </summary>
        private const int DEFAULT_FROM_HOURS = 11;

        /// <summary>
        /// Default end hour.
        /// </summary>
        private const int DEFAULT_TO_HOURS = 13;

        /// <summary>
        /// Storage schema version 1.
        /// </summary>
        private const int VERSION_1 = 0x00000001;

        /// <summary>
        /// Storage schema version 2.
        /// </summary>
        private const int VERSION_2 = 0x00000002;

        /// <summary>
        /// Storage schema current version.
        /// </summary>
        private const int VERSION_CURR = VERSION_2;

        #endregion 

        #region Private members

        private TimeWindow _timeWindow;

        #endregion 
    }
}
