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
using ESRI.ArcLogistics.Data;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// <see cref="T:ESRI.ArcLogistics.DomainObjects.Breaks"/> validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class BreakTypesValidator : Validator<Breaks>
    {
        #region Constructors

        /// <summary>
        /// Create a new instance of the <c>BreakTypesValidator</c> class.
        /// </summary>
        public BreakTypesValidator() : base(null, null)
        { }

        #endregion

        #region Protected methods

        /// <summary>
        /// Does validation.
        /// </summary>
        /// <param name="objectToValidate">Object to validation
        /// (<see cref="T:ESRI.ArcLogistics.DomainObjects.Breaks"/>).</param>
        /// <param name="currentTarget">Ignored.</param>
        /// <param name="key">Ignored.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(Breaks objectToValidate,
                                           object currentTarget,
                                           string key,
                                           ValidationResults validationResults)
        {
            Route route = currentTarget as Route;

            if (objectToValidate == null || route == null ||
                (route as ISupportOwnerCollection).OwnerCollection == null)
                return;

            if (objectToValidate.Count > 0)
            {
                // Get only first break.
                Break currentBreak = objectToValidate[0];

                // Check that all first breaks on All Routes have same type.
                foreach (var obj in (route as ISupportOwnerCollection).OwnerCollection)
                {
                    Breaks breaks = (obj as Route).Breaks;

                    if (breaks.Count > 0)
                    {
                        Break br = breaks[0];

                        if (!(br is TimeWindowBreak && currentBreak is TimeWindowBreak) &&
                            !(br is DriveTimeBreak && currentBreak is DriveTimeBreak) &&
                            !(br is WorkTimeBreak && currentBreak is WorkTimeBreak))
                        {
                            // If it is - add message to validation results.
                            string message = Properties.Messages.Error_InvalidBreaksTypes;
                            this.LogValidationResult(validationResults, message, currentTarget, key);

                            break;
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Default message template for validation.
        /// </summary>
        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_InvalidBreaksTypes; }
        }

        #endregion
    }
}
