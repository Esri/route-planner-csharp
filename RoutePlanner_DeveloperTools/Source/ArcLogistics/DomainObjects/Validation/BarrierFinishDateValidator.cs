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
