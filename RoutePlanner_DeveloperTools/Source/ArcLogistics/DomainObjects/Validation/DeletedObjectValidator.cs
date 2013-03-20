using System.Diagnostics;
using ESRI.ArcLogistics.Data;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Validates that this object isn't Marked as deleted.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class DeletedObjectValidator : Validator<DataObject>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create a new instance of the <c>DeletedObjectValidator</c> class.
        /// </summary>
        public DeletedObjectValidator()
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
            // Check input parametrs.
            IMarkableAsDeleted mark = objectToValidate as IMarkableAsDeleted;
            Debug.Assert(currentTarget is Route);
            Debug.Assert(this.MessageTemplate != null);
            if (objectToValidate == null || mark != null)
                return;

            // If item is mark as deleted - log validation message.
            if (mark.IsMarkedAsDeleted)
            {
                string error = this.MessageTemplate;
                this.LogValidationResult(validationResults, error, currentTarget, key);
            }
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
