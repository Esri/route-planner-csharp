using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    sealed class RouteEmptyStateValidatorAttribute : ValidatorAttribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>RouteLocationValidatorAttribute</c> class.
        /// </summary>
        public RouteEmptyStateValidatorAttribute()
        { }

        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does create validator.
        /// </summary>
        /// <param name="targetType">Ignored.</param>
        /// <returns>Created related validator
        /// (<see cref="T:ESRI.ArcLogistics.DomainObjects.Validation.RouteLocationValidator"/>).</returns>
        protected override Validator DoCreateValidator(Type targetType)
        {
            return new RouteEmptyStateValidator();
        }

        #endregion // Protected methods
    }
}