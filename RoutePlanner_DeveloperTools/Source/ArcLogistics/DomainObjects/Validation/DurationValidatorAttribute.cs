using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Duration validator attribute implementation.
    /// </summary>
    class DurationValidatorAttribute : ValidatorAttribute
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>DurationValidatorAttribute</c> class.
        /// </summary>
        public DurationValidatorAttribute()
        { }

        #endregion 

        #region Protected methods

        /// <summary>
        /// Does create validator.
        /// </summary>
        /// <param name="targetType">Ignored.</param>
        /// <returns>Created related validator.</returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new DurationValidator();
        }

        #endregion 
    }
}
