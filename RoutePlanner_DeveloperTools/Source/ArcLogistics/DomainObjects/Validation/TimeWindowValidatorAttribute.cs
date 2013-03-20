using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// TimeWindow validator attribute implementation.
    /// </summary>
    class TimeWindowValidatorAttribute : ValidatorAttribute
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>TimeWindowValidatorAttribute</c> class.
        /// </summary>
        public TimeWindowValidatorAttribute()
        { }

        #endregion 

        #region Protected methods

        /// <summary>
        /// Does create validator.
        /// </summary>
        /// <param name="targetType">Ignored.</param>
        /// <returns>Created related validator
        /// (<see cref="T:ESRI.ArcLogistics.DomainObjects.Validation.TimeWindowValidator"/>).</returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new TimeWindowValidator();
        }

        #endregion 
    }
}
