using System;
using System.Globalization;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    [ConfigurationElementType(typeof(CustomValidatorData))]
    class FuelEconomyValidator : Validator<double>
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public FuelEconomyValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        protected override void DoValidate(double objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            string message = null;
            double fuelEconomy = objectToValidate;
            Vehicle vehicle = currentTarget as Vehicle;
            if ((0 < fuelEconomy) && (null != vehicle.FuelType) && (SolverConst.MAX_SALARY < (vehicle.FuelType.Price / fuelEconomy)))
            {
                string format = null;
                if (RegionInfo.CurrentRegion.IsMetric)
                    format = Properties.Messages.Error_InvalidFuelEconomyMetric;
                else
                    format = Properties.Messages.Error_InvalidFuelEconomyUS;
                message = string.Format(format, SolverConst.MAX_SALARY);
            }
            else
            {
                if (fuelEconomy <= 0)
                    message = Properties.Messages.Error_InvalidFuelEconomyValue;
            }

            if (!string.IsNullOrEmpty(message))
                this.LogValidationResult(validationResults, message, currentTarget, key);
        }

        protected override string DefaultMessageTemplate
        {
            get { return null; }
        }
        #endregion // Protected methods
    }
}
