using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class TimeWindow2Validator : Validator<TimeWindow>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public TimeWindow2Validator()
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

            if (objectToValidate.IsWideOpen)
                return;

            // Detect first time window.
            Order order = currentTarget as Order;
            Location location = currentTarget as Location;
            TimeWindow tw = null;
            if (location != null)
                tw = location.TimeWindow;
            if (order != null)
                tw = order.TimeWindow;

            System.Diagnostics.Debug.Assert(tw != null);

            if (null != tw)
            {
                if (tw.IsWideOpen)
                    return;

                // NOTE: Time Windows shouldn't overlap and TimeWindow1
                // should be earlier than TimeWindow2.
                string message = null;

                if (tw.Intersects(objectToValidate))
                    message = Properties.Messages.Error_OrderTWOverlapped;
                else if(tw.EffectiveFrom > objectToValidate.EffectiveFrom)
                    message = Properties.Messages.Error_OrderTW1LaterTW2;

                if (null != message)
					this.LogValidationResult(validationResults, message, currentTarget, key);
            }
        }

        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }
        #endregion // Protected methods
    }
}
