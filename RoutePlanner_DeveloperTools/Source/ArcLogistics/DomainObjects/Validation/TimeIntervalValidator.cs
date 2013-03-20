using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using ESRI.ArcLogistics.BreaksHelpers;
using System.Diagnostics;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// TimeInterval validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class TimeIntervalValidator : Validator<double>
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>BreakValidator</c> class.
        /// </summary>
        public TimeIntervalValidator()
            : base(null, null)
        { }

        #endregion 

        #region Protected methods
 
        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Ignored.</param>
        /// <param name="currentTarget">Current target (expected only
        /// <see cref="T:ESRI.ArcLogistics.DomainObjects.TimeIntervalBreak"/>).</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(double objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            // Input parametrs check.
            Debug.Assert(currentTarget != null);
            if ((currentTarget as TimeIntervalBreak).Breaks == null)
                return;

            // Detecting index of timeIntervalBreak in Breaks collection.
            var timeIntervalBreak = currentTarget as TimeIntervalBreak;
            int index = BreaksHelper.IndexOf(timeIntervalBreak.Breaks, timeIntervalBreak);

            // Validate time interval.
            _ValidatingTimeInterval(timeIntervalBreak.TimeInterval, index, currentTarget, key, validationResults);

            // Detecting index of timeIntervalBreak in sorted breaks collection.
            List<Break> sortedBreaks = BreaksHelper.GetSortedList(timeIntervalBreak.Breaks);
            int sortedIndex = BreaksHelper.IndexOf(sortedBreaks, timeIntervalBreak);
                
            // If it isn't last break in sorted collection, we can check it`s timeinterval on intersection 
            // with next break.
            if (sortedIndex < sortedBreaks.Count - 1)
            {
                // Checking next Break in collection on equal TimeIntervals.
                TimeIntervalBreak nextBreak = sortedBreaks[sortedIndex + 1] as TimeIntervalBreak;
                if (timeIntervalBreak.TimeInterval == nextBreak.TimeInterval)
                {
                    // Detecting index of nextBreak in initial collection.
                    int indexOfNextBreak = BreaksHelper.IndexOf(timeIntervalBreak.Breaks, nextBreak);
                    _AddString(Properties.Messages.Error_TimeIntervalsAreEqual, index,
                        indexOfNextBreak, currentTarget, key, validationResults);
                }

                // For WorkTimeBreak Break After plus Duration of one break cannot exceed 
                // Break After of next break in a sequence.
                var workTimeBreak = currentTarget as WorkTimeBreak;
                if (workTimeBreak != null)
                {
                    var nextWorkTimeBreak = sortedBreaks[sortedIndex + 1] as WorkTimeBreak;
                    double timeIntervalPlusDuration = workTimeBreak.Duration / MINUTES_IN_HOUR + workTimeBreak.TimeInterval;
                    int indexOfNextBreak = BreaksHelper.IndexOf(timeIntervalBreak.Breaks, nextWorkTimeBreak);
                    if (nextWorkTimeBreak != null && timeIntervalPlusDuration >= nextWorkTimeBreak.TimeInterval)
                        _AddString(Properties.Messages.Error_TimeIntervalsPlusDurationExceed, index,
                            indexOfNextBreak, currentTarget, key, validationResults);
                }
            }
        }

        /// <summary>
        /// Default message template for validation.
        /// </summary>
        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_InvalidBreakDuration; }
        }

        #endregion 

        #region Private constants

        private int MINUTES_IN_HOUR = 60;

        #endregion

        #region Private methods

        /// <summary>
        /// Adding string to ValidationResults.
        /// </summary>
        /// <param name="format">Message format.</param>
        /// <param name="index">Sequence number of first break.</param>
        /// <param name="overlapIndex">Sequence number of second break.</param>
        /// <param name="currentTarget">Ignored.</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Ignored.</param>
        private void _AddString(string format, int index, int secondIndex,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            string message = string.Format(format,
                        BreaksHelper.GetOrdinalNumberName(index), BreaksHelper.GetOrdinalNumberName(secondIndex));
            message = BreaksHelper.UppercaseFirstLetter(message);
            this.LogValidationResult(validationResults, message, currentTarget, key);
        }


        /// <summary>
        /// Validating that TimeInterval is not less then 0 and not more then 1 year.
        /// </summary>
        /// <param name="timeInterval">TimeInterval in hours.</param>
        /// <param name="index">Sequence number of break.</param>
        /// <param name="currentTarget">Ignored.</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Ignored.</param>
        private void _ValidatingTimeInterval(double timeInterval,
            int index,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            if (timeInterval <= 0 || timeInterval > SolverConst.MAX_TIME_HOURS)
            {
                string message = string.Format(Properties.Messages.Error_InvalidBreakInterval, BreaksHelper.GetOrdinalNumberName(index));
                message = BreaksHelper.UppercaseFirstLetter(message);
                this.LogValidationResult(validationResults, message, currentTarget, key);
                return;
            }

        }
        #endregion
    }
}
