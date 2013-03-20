using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class BarrierFinishDateValidator : Validator<DateTime?>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public BarrierFinishDateValidator()
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
            // check value
            Validator validator = new DataTimeNullableValidator();
            ValidationResults valueResults = validator.Validate(objectToValidate);
            foreach (ValidationResult res in valueResults)
            {
                if (res.Key.Equals(key))
                {
                    this.LogValidationResult(validationResults, res.Message, currentTarget, key);
                    break;
                }
            }

            // check finish date must be after start
            if (!objectToValidate.HasValue)
                return;

            Barrier barrier = currentTarget as Barrier;
            System.Diagnostics.Debug.Assert(null != barrier);

            DateTime? startDate = barrier.StartDate;
            if (!startDate.HasValue)
                return;

            DateTime valueStart = startDate.Value;
            DateTime valueFinish = objectToValidate.Value;
            if (valueFinish < valueStart)
                this.LogValidationResult(validationResults, this.MessageTemplate, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_StartDateLaterFinishDate; }
        }
        #endregion // Protected methods
    }
}
