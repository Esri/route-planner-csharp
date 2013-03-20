using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Time interval validator attribute implementation.
    /// </summary>
    class TimeIntervalValidatorAttribute : ValidatorAttribute
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>BreaksValidatorAttribute</c> class.
        /// </summary>
        public TimeIntervalValidatorAttribute()
        { }

        #endregion 

        #region Protected methods

        /// <summary>
        /// Does create validator.
        /// </summary>
        /// <param name="targetType">Ignored.</param>
        /// <returns>Created related validator
        /// (<see cref="T:ESRI.ArcLogistics.DomainObjects.Validation.TimeIntervalValidator"/>).</returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new TimeIntervalValidator();
        }

        #endregion 
    }
}
