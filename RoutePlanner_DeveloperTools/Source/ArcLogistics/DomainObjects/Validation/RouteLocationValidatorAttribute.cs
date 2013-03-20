using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Location"/> as part of route validator 
    /// attribute implementation.
    /// </summary>
    sealed class RouteLocationValidatorAttribute : ValidatorAttribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>RouteLocationValidatorAttribute</c> class.
        /// </summary>
        public RouteLocationValidatorAttribute()
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
            return new RouteLocationValidator();
        }

        #endregion // Protected methods
    }
}
