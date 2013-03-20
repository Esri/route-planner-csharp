using System.Diagnostics;
using ESRI.ArcLogistics.Data;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Name validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class DuplicateNameValidator : Validator<string>
    {
        #region Constructors
    
        /// <summary>
        /// Create a new instance of the <c>DuplicateNameValidator</c> class.
        /// </summary>
        public DuplicateNameValidator()
            : base(null, null)
        { }

        #endregion 

        #region Protected methods
        
        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">String to validate.</param></param>
        /// <param name="currentTarget">>Object to validation. It must implement 
        /// ISupportName and ISupportParentCollection.</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(string objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            // Cast currentTarget to ISupportParent collection.
            ISupportOwnerCollection hasParent = currentTarget as
                ISupportOwnerCollection;

            // Input Parametrs check.
            Debug.Assert(currentTarget as ISupportName != null);

            if (objectToValidate == null || hasParent == null || hasParent.OwnerCollection == null)
                return;

            // Get the name to validate.
            string name = objectToValidate.ToString();

            // Check that name has no duplicates.
            foreach (var obj in hasParent.OwnerCollection)
                // If it has then add validation result.
                if ((obj as ISupportName).Name == name && obj != currentTarget)
                {
                    // If it is - add message to validation results.
                    string format = Properties.Messages.Error_DuplicateName;
                    string typeName = (obj as DataObject).TypeTitle;
                    string message = string.Format(format, typeName, name);
                    this.LogValidationResult(validationResults, message, currentTarget, key);
                    break;
                }
        }

        #endregion 
    
        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }
    }
}
