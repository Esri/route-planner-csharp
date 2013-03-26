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
    class OrderCustomPropertyValidator : Validator<OrderCustomProperties>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public OrderCustomPropertyValidator(int? index)
            : base(null, null)
        {
            _index = index;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(OrderCustomProperties objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            if (_index == null)
            {   // validate whole object
                for (int i = 0; i < objectToValidate.Count; i++)
                    _Validate(objectToValidate, i, currentTarget, key, validationResults);
            }
            else
                _Validate(objectToValidate, _index.Value, currentTarget, key, validationResults);
        }

        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_InvalidCustomProperty; }
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void _Validate(OrderCustomProperties properties, int index, object currentTarget, string key, ValidationResults validationResults)
        {
            if (properties.Info[index].Type == OrderCustomPropertyType.Numeric)
            {
                if (null != properties[index])
                {
                    double value = 0.0;
                    try
                    {
                        if (properties[index] is double)
                            value = (double)properties[index];
                        else if (properties[index] is string)
                            value = double.Parse(properties[index].ToString());
                    }
                    catch
                    {
                    }

                    if (value < 0.0)
                    {
                        string message = string.Format(this.MessageTemplate, properties.Info[index].Name);
                        this.LogValidationResult(validationResults, message, currentTarget, key);
                    }
                }
            }
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private int? _index = null;

        #endregion // Private members
    }
}
