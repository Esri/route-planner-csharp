using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class DaysValidator : Validator<Days>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public DaysValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(Days objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            // check value
            if (null == objectToValidate)
                return;

            string message = string.Empty;

            if (objectToValidate.From.HasValue && objectToValidate.To.HasValue)
            {
                DateTime valueStart = objectToValidate.From.Value;
                DateTime valueFinish = objectToValidate.To.Value;
                if (valueFinish < valueStart)
                    message = Properties.Messages.Error_FromDateLaterToDate;
            }

            bool isAnyDaySelected = false;
            DayOfWeek[] days = (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek));
            foreach (DayOfWeek day in days)
            {
                if (objectToValidate.IsDayEnabled(day))
                {
                    isAnyDaySelected = true;
                    break; // NOTE: exit
                }
            }

            if (!isAnyDaySelected)
            {
                if (!string.IsNullOrEmpty(message))
                    message += "\n";
                message += Properties.Messages.Error_NotSelectedDaysOfWeek;
            }

            if (!string.IsNullOrEmpty(message))
                this.LogValidationResult(validationResults, message, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }
        #endregion // Protected methods
    }
}
