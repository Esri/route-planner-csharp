using ESRI.ArcLogistics.Data;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Name not null validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class NameNotNullValidator : Validator<string>
    {
        #region Constructors
    
        /// <summary>
        /// Create a new instance of the <c>NameNotNullValidator</c> class.
        /// </summary>
        public NameNotNullValidator()
            : base(null, null)
        { }

        #endregion 

        #region Protected methods
        
        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Name to validate.</param>
        /// <param name="currentTarget">Ignored.</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(string objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            // Check that name is empty or null.
            if (objectToValidate == null || string.IsNullOrEmpty(objectToValidate.ToString()))
            {
                // If it is - add message to validation results.
                string format = Properties.Messages.Error_NullName;
                string message = string.Format(format,(currentTarget as DataObject).TypeTitle);
                this.LogValidationResult(validationResults, message, currentTarget, key);
            }
        }

        #endregion 

    
        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }
    }
}
