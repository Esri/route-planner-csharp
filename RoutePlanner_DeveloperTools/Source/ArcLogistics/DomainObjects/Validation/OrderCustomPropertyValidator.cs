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
