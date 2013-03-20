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
