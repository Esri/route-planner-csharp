using System;
using System.Diagnostics;

using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Location"/> as part of route
    /// validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class RouteLocationValidator : Validator<Location>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>RouteLocationValidator</c> class.
        /// </summary>
        public RouteLocationValidator()
            : base(null, null)
        { }

        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Object to validation
        /// (<see cref="T:ESRI.ArcLogistics.DomainObjects.Location"/>).</param>
        /// <param name="currentTarget">Current target (expected only
        /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Route"/>).</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(Location objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            Route route = currentTarget as Route;
            Debug.Assert(null != route);

            if ((null == route.StartLocation) && (null == route.EndLocation))
                this.LogValidationResult(validationResults,
                                         this.MessageTemplate,
                                         currentTarget,
                                         key);
        }

        /// <summary>
        /// Default message template for validation.
        /// </summary>
        protected override string DefaultMessageTemplate
        {
            get { return base.MessageTemplate; }
        }

        #endregion // Protected methods
    }
}
