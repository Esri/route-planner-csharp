using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    class BreakTypesValidatorAttribute : ValidatorAttribute
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>BreakTypesValidatorAttribute</c> class.
        /// </summary>
        public BreakTypesValidatorAttribute()
        { }

        #endregion

        #region Protected methods

        /// <summary>
        /// Does create validator.
        /// </summary>
        /// <param name="targetType">Ignored.</param>
        /// <returns>Created related validator
        /// (<see cref="T:ESRI.ArcLogistics.DomainObjects.Validation.BreakTypesValidator"/>).</returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new BreakTypesValidator();
        }

        #endregion
    }
}
