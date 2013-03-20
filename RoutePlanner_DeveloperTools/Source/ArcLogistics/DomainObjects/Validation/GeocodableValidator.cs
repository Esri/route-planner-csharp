using System;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class GeocodableValidator : Validator<bool>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public GeocodableValidator()
            : this(null)
        { }

        public GeocodableValidator(string messageTemplate)
            : base(null, null)
        {
            this.MessageTemplate = messageTemplate;
        }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(bool objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            if (!objectToValidate)
                this.LogValidationResult(validationResults, this.MessageTemplate, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return base.MessageTemplate; }
        }
        #endregion // Protected methods
    }
}
