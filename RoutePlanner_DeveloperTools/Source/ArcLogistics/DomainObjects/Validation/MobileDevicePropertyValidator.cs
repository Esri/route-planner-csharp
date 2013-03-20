using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class MobileDevicePropertyValidator : Validator<string>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public MobileDevicePropertyValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(string objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            Debug.Assert(currentTarget is MobileDevice);

            string message = null;

            SyncType syncType = (currentTarget as MobileDevice).SyncType;
            switch (syncType)
            {
                case SyncType.EMail:
                {
                    if ("EmailAddress" == key)
                        message = _ValidateSpecString(objectToValidate, "^[a-z0-9!#$%&'*+/=?^_`{|}\\~\\-]+(\\.[a-z0-9!#$%&'*+/=?^_`{|}\\~\\-]+)*@([a-z0-9]([a-z0-9\\-]*[a-z0-9])?\\.)+[a-z0-9]([a-z0-9\\-]*[a-z0-9])$",
                                                      ArcLogistics.Properties.Messages.Error_InvalidEMail);
                    break;
                }

                case SyncType.Folder:
                {
                    if ("SyncFolder" == key)
                        message = _ValidateSpecString(objectToValidate, @"^(((\\\\([^\\/:\*\?""\|<>\. ]+))|([a-zA-Z]:\\))(([^\\/:\*\?""\|<>]*)([\\]*))*)$",
                                                      ArcLogistics.Properties.Messages.Error_InvalidFolderPath);
                    break;
                }

                case SyncType.ActiveSync:
                    if ("ActiveSyncProfileName" == key)
                        message = (string.IsNullOrEmpty(objectToValidate))? ArcLogistics.Properties.Messages.Error_InvalidActiveSyncProfileName : null;
                    break;

                case SyncType.None:
                    break; // NOTE: do nothing

                case SyncType.WMServer:
                    // Dont validate.
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            if (null != message)
                this.LogValidationResult(validationResults, message, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }
        #endregion // Protected methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private string _ValidateSpecString(string stringToValidate, string pattern, string errorMessage)
        {
            bool isValid = false;
            if (!string.IsNullOrEmpty(stringToValidate))
            {
                RegexValidator validator = new RegexValidator(pattern, RegexOptions.IgnoreCase);
                ValidationResults results = new ValidationResults();
                validator.Validate(stringToValidate, results);
                isValid = results.IsValid;
            }

            return (isValid) ? null : errorMessage;
        }
        #endregion // Private methods
    }
}
