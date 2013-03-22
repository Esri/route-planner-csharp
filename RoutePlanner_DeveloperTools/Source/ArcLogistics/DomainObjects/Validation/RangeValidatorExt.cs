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
using System.Reflection;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

using ESRI.ArcLogistics.DomainObjects.Attributes;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class RangeValidatorExt : Validator<double>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public RangeValidatorExt(double minValue, double maxValue)
            : base(null, null)
        {
            _minValue = minValue;
            _maxValue = maxValue;
        }
        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(double objectToValidate, object currentTarget, string key, ValidationResults validationResults)
        {
            Type type = currentTarget.GetType();
            PropertyInfo property = type.GetProperty(key);

            UnitPropertyAttribute unitAttribute = (UnitPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(UnitPropertyAttribute));

            Unit displayUnits = (RegionInfo.CurrentRegion.IsMetric)? unitAttribute.DisplayUnitMetric : unitAttribute.DisplayUnitUS;
            Unit valueUnits = unitAttribute.ValueUnits;

            if ((objectToValidate < _minValue) || (_maxValue < objectToValidate))
            {
                string format = this.MessageTemplate;

                double maxValue = _maxValue;
                if (valueUnits != displayUnits)
                    maxValue = UnitConvertor.Convert(maxValue, valueUnits, displayUnits);

                string valueToDisplay = UnitFormatter.Format(maxValue, displayUnits);

                string message = string.Format(format, valueToDisplay);
                this.LogValidationResult(validationResults, message, currentTarget, key);
            }
        }

        protected override string DefaultMessageTemplate
        {
            get { return base.MessageTemplate; }
        }
        #endregion // Protected methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private double _minValue = 0;
        private double _maxValue = Double.MaxValue;
        #endregion // Private members
    }
}
