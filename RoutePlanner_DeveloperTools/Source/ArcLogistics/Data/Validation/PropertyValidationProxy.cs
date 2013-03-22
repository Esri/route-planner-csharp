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