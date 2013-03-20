using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class RouteTimeWindowValidator : Validator<TimeWindow>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public RouteTimeWindowValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(TimeWindow objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            System.Diagnostics.Debug.Assert(null != objectToValidate);

            // Route's time window cannot be wideopen.
            if (objectToValidate.IsWideOpen)
                this.LogValidationResult(validationResults, 
                    Properties.Messages.Error_RouteStartTWCannotWideopen, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }
        #endregion // Protected methods
    }
}
