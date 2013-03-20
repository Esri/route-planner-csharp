using System;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// <see cref="T:ESRI.ArcLogistics.Data.DataObject"/> as part of route
    /// validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class RouteRefObjectValidator : Validator<DataObject>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>RouteRefObjectValidator</c> class.
        /// </summary>
        public RouteRefObjectValidator()
            : base(null, null)
        {}

        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Object to validation
        /// (<see cref="T:ESRI.ArcLogistics.Data.DataObject"/>).</param>
        /// <param name="currentTarget">Current target (expected only
        /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Route"/>).</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(DataObject objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            Debug.Assert(currentTarget is Route);

            if (objectToValidate == null)
            {
                return;
            }

            var route = currentTarget as Route;
            if (route.Schedule == null)
            {
                return;
            }

            if (!_IsDeleted(objectToValidate))
            {
                return;
            }

            string error = null;
            if (objectToValidate is Driver)
            {
                error = Properties.Messages.Error_InvalidRefObjDriverIsDeleted;
            }
            else if (objectToValidate is Vehicle)
            {
                error = Properties.Messages.Error_InvalidRefObjVehicleIsDeleted;
            }
            else if (objectToValidate is Location)
            {
                error = this.MessageTemplate;
            }
            else
            {
                Debug.Assert(false); // NOTE: not supported
            }

            if (!string.IsNullOrEmpty(error))
                this.LogValidationResult(validationResults, error, currentTarget, key);
        }

        /// <summary>
        /// Default message template for validation.
        /// </summary>
        protected override string DefaultMessageTemplate
        {
            get { return base.MessageTemplate; }
        }

        #endregion // Protected methods

        #region Private methods
        /// <summary>
        /// Checks reference object marked as deleted.
        /// </summary>
        /// <param name="obj">Object to check.</param>
        /// <returns>TRUE if object marked as deleted.</returns>
        private bool _IsDeleted(DataObject obj)
        {
            Debug.Assert(null != obj);

            IMarkableAsDeleted mark = obj as IMarkableAsDeleted;
            bool isDeleted = ((null != mark) && mark.IsMarkedAsDeleted);
            return isDeleted;
        }

        #endregion // Private methods
    }
}
