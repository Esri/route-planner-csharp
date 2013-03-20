using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class DataTimeNullableValidator : Validator<DateTime?>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public DataTimeNullableValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(DateTime? objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            if (!objectToValidate.HasValue)
                return;

            DateTime value = objectToValidate.Value;
            if ((value < DateTime.MinValue) && (DateTime.MaxValue < value))
                this.LogValidationResult(validationResults, this.MessageTemplate, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_InvalidDateTime; }
        }
        #endregion // Protected methods
    }
}
