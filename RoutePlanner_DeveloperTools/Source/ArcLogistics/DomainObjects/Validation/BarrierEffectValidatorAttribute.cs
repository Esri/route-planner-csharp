using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// BarrierEffect validator attribute implementation.
    /// </summary>
    sealed class BarrierEffectValidatorAttribute : ValidatorAttribute
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public BarrierEffectValidatorAttribute() { }
        #endregion // Constructors

        #region Protected methods
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new BarrierEffectValidator();
        }
        #endregion // Protected methods
    }
}
