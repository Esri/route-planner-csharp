using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Breaks"/> validator attribute implementation.
    /// </summary>
    class BreaksValidatorAttribute : ValidatorAttribute
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>BreaksValidatorAttribute</c> class.
        /// </summary>
        public BreaksValidatorAttribute()
        { }

        #endregion 

        #region Protected methods
       
        /// <summary>
        /// Does create validator.
        /// </summary>
        /// <param name="targetType">Ignored.</param>
        /// <returns>Created related validator
        /// (<see cref="T:ESRI.ArcLogistics.DomainObjects.Validation.BreaksValidator"/>).</returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new BreaksValidator();
        }

        #endregion 
    }
}
