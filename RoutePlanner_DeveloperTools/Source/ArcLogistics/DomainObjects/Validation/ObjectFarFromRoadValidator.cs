using System;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace ESRI.ArcLogistics.DomainObjects.Validation
{
    /// <summary>
    /// Validation class to validate geocodable objects, which lies far from nearest road.
    /// </summary>
    [ConfigurationElementType(typeof(CustomValidatorData))]
    sealed class ObjectFarFromRoadValidator : Validator<Address>
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectFarFromRoadValidator()
            : this(null)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="messageTemplate">Validator message template.</param>
        public ObjectFarFromRoadValidator(string messageTemplate)
            : base(null, null)
        {
            this.MessageTemplate = messageTemplate;
        }
        #endregion

        #region Protected methods

        /// <summary>
        /// Do validation.
        /// </summary>
        /// <param name="objectToValidate"></param>
        /// <param name="currentTarget"></param>
        /// <param name="key"></param>
        /// <param name="validationResults"></param>
        protected override void DoValidate(Address address, object currentTarget, string key,
                                           ValidationResults validationResults)
        {
            if ((address.MatchMethod != null) &&
                address.MatchMethod.Equals(Properties.Resources.ManuallyEditedXYFarFromNearestRoadResourceName,
                                           StringComparison.OrdinalIgnoreCase))
                this.LogValidationResult(validationResults, this.MessageTemplate, currentTarget, key);
        }

        /// <summary>
        /// Validator message template.
        /// </summary>
        protected override string DefaultMessageTemplate
        {
            get { return base.MessageTemplate; }
        }

        #endregion
    }
}
