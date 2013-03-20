using System.Diagnostics;
using ESRI.ArcLogistics.BreaksHelpers;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// <c>Break</c> validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class DurationValidator : Validator<double>
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>DurationValidator</c> class.
        /// </summary>
        public DurationValidator()
            : base(null, null)
        { }

        #endregion 

        #region Protected methods
 
        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Ignored.</param>
        /// <param name="currentTarget">Current target (expected only
        /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Break"/>).</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(double objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            // Input parametrs check.
            Debug.Assert(currentTarget != null);

            if ((currentTarget as Break).Breaks != null)
            {
                // Detecting index of break in Breaks collection.
                var breakObject = currentTarget as Break;
                int index = BreaksHelper.IndexOf(breakObject.Breaks, breakObject);

                // Validating Break TimeInterval.
                if (breakObject.Duration <= 0 || breakObject.Duration > SolverConst.MAX_TIME_MINS)
                {
                    string message = string.Format(Properties.Messages.Error_InvalidBreakDuration, BreaksHelper.GetOrdinalNumberName(index));
                    message = BreaksHelper.UppercaseFirstLetter(message);
                    this.LogValidationResult(validationResults, message, currentTarget, key);
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
    }
}
