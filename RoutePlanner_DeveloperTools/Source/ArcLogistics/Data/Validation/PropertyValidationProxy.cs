using System;
using Microsoft.Practices.EnterpriseLibrary.Validation.Integration;
using Microsoft.Practices.EnterpriseLibrary.Validation;

namespace ESRI.ArcLogistics.Data.Validation
{
    internal class PropertyValidationProxy : IValidationIntegrationProxy
    {
        public MemberValueAccessBuilder GetMemberValueAccessBuilder()
        {
            return new ReflectionMemberValueAccessBuilder();
        }

        public void PerformCustomValueConversion(ValueConvertEventArgs e)
        {
        }

        public object GetRawValue()
        {
            return null;
        }

        public bool ProvidesCustomValueConversion
        {
            get { return _ProvidesValueConversion; }
            set { _ProvidesValueConversion = value; }
        }

        public string Ruleset
        {
            get { return _RulesetName; }
            set { _RulesetName = value; }
        }

        public ValidationSpecificationSource SpecificationSource
        {
            get { return _SpecificationSource; }
            set { _SpecificationSource = value; }
        }

        public string ValidatedPropertyName
        {
            get { return _PropertyName; }
            set { _PropertyName = value; }
        }

        public Type ValidatedType
        {
            get { return _validatedType; }
            set { _validatedType = value; }
        }

        private string _RulesetName;
        private string _PropertyName;
        private ValidationSpecificationSource _SpecificationSource;
        private Type _validatedType;
        private bool _ProvidesValueConversion;
    }
}