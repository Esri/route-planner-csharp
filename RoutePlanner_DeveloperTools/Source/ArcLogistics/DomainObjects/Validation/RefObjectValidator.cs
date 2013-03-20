using System;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class RefObjectValidator : Validator<DataObject>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public RefObjectValidator()
            : base(null, null)
        {}
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(DataObject objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            if (null != objectToValidate)
            {
                string messageError = (!string.IsNullOrEmpty(Tag) && Tag.Equals(DataObject.PRIMARY_VALIDATOR_TAG)) ?
                                            objectToValidate.PrimaryError : objectToValidate.Error;
                if (!string.IsNullOrEmpty(messageError))
                    this.LogValidationResult(validationResults, this.MessageTemplate, currentTarget, key);
            }
        }

        protected override string DefaultMessageTemplate
        {
            get { return base.MessageTemplate; }
        }
        #endregion // Protected methods
    }
}
