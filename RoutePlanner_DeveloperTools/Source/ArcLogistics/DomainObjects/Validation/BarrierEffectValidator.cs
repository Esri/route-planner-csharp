using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using System.Diagnostics;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// BarrierEffect validator implementation.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class BarrierEffectValidator : Validator<BarrierEffect>
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public BarrierEffectValidator()
            : base(null, null)
        { }
        #endregion // Constructors

        #region Protected methods

        /// <summary>
        /// Method does validation.
        /// </summary>
        /// <param name="objectToValidate">Object to validate.</param>
        /// <param name="currentTarget">Target barrier.</param>
        /// <param name="key">Key.</param>
        /// <param name="validationResults">Validation results.</param>
        protected override void DoValidate(BarrierEffect objectToValidate, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            // If BreakEffect is null we have nothing to validate.
            if (objectToValidate == null)
                return;

            // Barrier effect selected type is blocktravel - no need to validate.
            if (objectToValidate.BlockTravel == true)
                return;

            Barrier barrier = currentTarget as Barrier;
            Debug.Assert(barrier != null);

            // Delay time for point barrier must be in 0 - 1 year range.
            if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Point)
                if (objectToValidate.DelayTime < 0 || objectToValidate.DelayTime > SolverConst.MAX_TIME_MINS)
                    this.LogValidationResult(validationResults, Properties.Messages.Error_InvalidBarrierEffectDelayTime, currentTarget, key);

            if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Polygon)
                // Validate Speed Up SpeedFactor.
                if (objectToValidate.SpeedFactorInPercent > MAX_SPEEDUP_FACTOR)
                {
                    string warningMessage = string.Format(
                        Properties.Messages.Error_InvalidSpeedUp,
                        MIN_SPEED_FACTOR,
                        MAX_SPEEDUP_FACTOR);
                    this.LogValidationResult(validationResults, warningMessage, currentTarget, key);
                }
                // Validate Slow Down SpeedFactor.
                else if (objectToValidate.SpeedFactorInPercent < -MAX_SLOWDOWN_FACTOR)
                {
                    string warningMessage = string.Format(
                        Properties.Messages.Error_InvalidSlowdown,
                        MIN_SPEED_FACTOR,
                        MAX_SLOWDOWN_FACTOR + 1,
                        MAX_SLOWDOWN_FACTOR + 1);
                    this.LogValidationResult(validationResults, warningMessage, currentTarget, key);
                }
                else
                {
                    // Do nothing.
                }
        }

        protected override string DefaultMessageTemplate
        {
            get { return Properties.Messages.Error_StartDateLaterFinishDate; }
        }
        #endregion // Protected methods

        #region Private constants

        /// <summary>
        /// Max speed factor value for Speed Up.
        /// </summary>
        private int MAX_SPEEDUP_FACTOR = 999;

        /// <summary>
        /// Max speed factor value for Slow Down.
        /// </summary>
        private int MAX_SLOWDOWN_FACTOR = 99;

        /// <summary>
        /// Max speed factor value for Slow Down.
        /// </summary>
        private int MIN_SPEED_FACTOR = 0;

        #endregion
    }
}
