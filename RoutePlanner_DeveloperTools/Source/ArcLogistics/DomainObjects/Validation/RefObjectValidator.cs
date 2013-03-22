/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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
