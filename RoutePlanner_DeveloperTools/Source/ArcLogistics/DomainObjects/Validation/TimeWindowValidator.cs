using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using ESRI.ArcLogistics.BreaksHelpers;
using System.Diagnostics;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// TimeWindow validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class TimeWindowValidator : Validator<TimeSpan>
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>BreakValidator</c> class.
        /// </summary>
        public TimeWindowValidator()
            : base(null, null)
        { }

        #endregion 

        #region Protected methods
 
        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Object to validation.</param>
        /// <param name="currentTarget">Current target (expected only
        /// <see cref="T:ESRI.ArcLogistics.DomainObjects.TimeWindowBreak"/>).</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(TimeSpan objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            // Input parametrs check.
            Debug.Assert(currentTarget != null);
            if ((currentTarget as TimeWindowBreak).Breaks == null)
                return;

            // Detecting index of TimeWindowBreak in Breaks collection.
            var timeWindowBreak = currentTarget as TimeWindowBreak;
            int index = BreaksHelper.IndexOf(timeWindowBreak.Breaks, timeWindowBreak);

            // Detecting index of TimeWindowBreak in sorted collection.
            List<Break> sortedBreaks = BreaksHelper.GetSortedList(timeWindowBreak.Breaks);
            int sortedIndex = BreaksHelper.IndexOf(sortedBreaks, timeWindowBreak);

            // If it isn't last break in sorted collection, we can check it's
            // timewindow on intersection with next break.
            if (sortedIndex < sortedBreaks.Count - 1)
            {
                // Detecting index of next Break in input collection.
                TimeWindowBreak nextBreak = sortedBreaks[sortedIndex + 1] as TimeWindowBreak;
                int overlapIndex = BreaksHelper.IndexOf(timeWindowBreak.Breaks, nextBreak);

                // Checking next Break in collection on TimeWindow 
                // intersection with current TimeWindowBreak.
                if (nextBreak.EffectiveFrom <= timeWindowBreak.EffectiveTo)
                    // Breaks time windows overlap.
                    _AddString(Properties.Messages.Error_TimeWindowOverlap,
                        index, overlapIndex, currentTarget, key, validationResults);
                else
                {
                    // Checking for time window + break time overlap.
                    TimeSpan duration = TimeSpan.FromMinutes( timeWindowBreak.Duration);
                    if (nextBreak.EffectiveFrom <= timeWindowBreak.EffectiveTo + duration)
                        // Break time window + duration overlap other break time window.
                        _AddString(Properties.Messages.Error_TimeWindowPlusDurationOverlap,
                            index, overlapIndex, currentTarget, key, validationResults);
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

        #region Private methods.

        /// <summary>
        /// Adding string to ValidationResults.
        /// </summary>
        /// <param name="format">Message format.</param>
        /// <param name="index">Sequence number of first break.</param>
        /// <param name="overlapIndex">Sequence number of second break.</param>
        /// <param name="currentTarget">Ignored.</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Ignored.</param>
        private void _AddString(string format, int index, int overlapIndex,
            object currentTarget,
            string key,
            ValidationResults validationResults)
        {
            string message = string.Format(format,
                        BreaksHelper.GetOrdinalNumberName(index), BreaksHelper.GetOrdinalNumberName(overlapIndex));
            message = BreaksHelper.UppercaseFirstLetter(message);
            this.LogValidationResult(validationResults, message, currentTarget, key);
        }
        #endregion
    }
}
