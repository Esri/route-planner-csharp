using System;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using ESRI.ArcLogistics.Data;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class ZonesValidator : Validator<IDataObjectCollection<Zone>>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public ZonesValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(IDataObjectCollection<Zone> objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            if (null == objectToValidate)
                return;

            string format = this.MessageTemplate;
            StringBuilder sb = new StringBuilder();
            foreach (Zone zone in objectToValidate)
            {
                string error = zone.Error;
                if (!string.IsNullOrEmpty(error))
                    sb.AppendLine(string.Format(format, zone.Name));
            }

            string message = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(message))
                this.LogValidationResult(validationResults, message, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_ZonesErrorFormat; }
        }
        #endregion // Protected methods
    }
}
