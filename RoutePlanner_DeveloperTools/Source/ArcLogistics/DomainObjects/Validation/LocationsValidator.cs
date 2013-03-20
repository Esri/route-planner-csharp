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
    sealed class LocationsValidator : Validator<IDataObjectCollection<Location>>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public LocationsValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(IDataObjectCollection<Location> objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            if (null == objectToValidate)
                return;

            string format = this.MessageTemplate;
            string deletedFormat = Properties.Messages.Error_RenewalLocationsIsDeletedErrorFormat;
            StringBuilder sb = new StringBuilder();
            foreach (Location location in objectToValidate)
            {
                IMarkableAsDeleted mark = location as IMarkableAsDeleted;
                System.Diagnostics.Debug.Assert(null != mark);
                if (mark.IsMarkedAsDeleted) // deleted is not valid state
                    sb.AppendLine(string.Format(deletedFormat, location.Name));
                else
                {   // check location error
                    string error = location.Error;
                    if (!string.IsNullOrEmpty(error))
                        sb.AppendLine(string.Format(format, location.Name));
                }
            }

            string message = sb.ToString().Trim();
            if (!string.IsNullOrEmpty(message))
                this.LogValidationResult(validationResults, message, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_RenewalLocationsErrorFormat; }
        }
        #endregion // Protected methods
    }
}
