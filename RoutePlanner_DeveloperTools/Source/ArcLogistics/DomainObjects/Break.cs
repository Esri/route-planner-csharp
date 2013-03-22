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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using ESRI.ArcLogistics.Data.Validation;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Integration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Break class represents a base abstract class for other breaks.
    /// </summary>
    public abstract class Break :
        INotifyPropertyChanged,
        ICloneable,
        IDataErrorInfo
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>Break</c> class.
        /// </summary>
        protected Break()
        { }

        #endregion // Constructors

        #region Public static properties
   
        /// <summary>
        /// Gets name of the Duration property.
        /// </summary>
        public static string PropertyNameDuration
        {
            get { return PROP_NAME_DURATION; }
        }

        #endregion // Public static properties

        #region Public members

        /// <summary>
        /// This property used for backward compatibility with existent 
        /// plugins. Without it plugin will crash the application. 
        /// Property doesn't affect anything. Don't use it in new source code.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Don't use this property.", true)]
        public TimeWindow TimeWindow
        {
            get { return new TimeWindow(); }
            set { var timeWindow = value; }
        }

        /// <summary>
        /// Break's duration in minutes.
        /// </summary>
        [DurationValidator]
        [DomainProperty("DomainPropertyNameDuration")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double Duration
        {
            get { return _duration; }
            set
            {
                if (value == _duration)
                    return;

                _duration = value;
                _NotifyPropertyChanged(PROP_NAME_DURATION);
            }
        }

        /// <summary>
        /// Returns a string representation of the break information.
        /// </summary>
        /// <returns>Break's string.</returns>
        public override string ToString()
        {
            return string.Format(Properties.Resources.BreakFormat, _duration);
        }

        #endregion 

        #region INotifyPropertyChanged members

        /// <summary>
        /// Event which is invoked when any of the object's properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion 

        #region ICloneable interface members
     
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        #endregion // ICloneable interface members

        #region IDataErrorInfo Members

        /// <summary>
        /// Validate Break, returns error string or null.
        /// </summary>
        public string Error
        {
            get
            {
                ValidationResults res = _ValidateObject();
                return res.IsValid ? null : FormatErrorString(res);
            }
        }

        /// <summary>
        /// Validate property.
        /// </summary>
        /// <param name="columnName">Name of the property to validate.</param>
        /// <returns>Error string or null.</returns>
        public string this[string columnName]
        {
            get
            {
                _InitValidation();
                return _ValidateProperty(columnName);
            }
        }

        #endregion

        #region Internal Method

        /// <summary>
        /// Check that both breaks have same types and same duration.
        /// </summary>
        /// <param name="breakObject">Brake to compare with this.</param>
        /// <returns>'True' if second break isnt null and breaks 
        /// types and durations are the same, 'false' otherwise.</returns>
        internal virtual bool EqualsByValue(Break breakObject)
        {
            // If second break is null - breaks are not equal.
            if (breakObject == null)
                return false;

            // Check type and duration.
            return this.GetType() == breakObject.GetType() && this.Duration == breakObject.Duration;
        }

        #endregion

        #region Internal abstract methods

        /// <summary>
        /// Converts state of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of the value of this instance.</returns>
        internal abstract string ConvertToString();
        /// <summary>
        /// Converts the string representation of a break to break internal state equivalent.
        /// </summary>
        /// <param name="context">String representation of a break.</param>
        internal abstract void InitFromString(string context);

        #endregion // Internal abstract methods

        #region Internal properties

        /// <summary>
        /// Breaks collection with this break.
        /// </summary>
        internal Breaks Breaks
        {
            set
            {
                // Unsubscribe from old collection's collection changed event.
                if (value == null)
                    _breaks.CollectionChanged -= BreaksCollectionChanged;

                _breaks = value;

                // Subscribe to new collection's collection changed event.
                if (value != null)
                    _breaks.CollectionChanged += new NotifyCollectionChangedEventHandler
                        (BreaksCollectionChanged);

                _NotifyPropertyChanged("Breaks");
            }
            get
            {
                return _breaks;
            }
        }

        internal double DefautDuration 
        {
            get
            {
                return DEFAULT_DURATION;
            }
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Notifies about change of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void _NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Formats error string that will be returned by IDataErrorInfo.Error property.
        /// </summary>
        /// <param name="results"><c>ValidationResults</c> object.</param>
        /// <returns>An error string.</returns>
        private string FormatErrorString(ValidationResults results)
        {
            Debug.Assert(results != null);

            // Clear result string from identical messages.
            var uniqueMessages = new List<string>(results.Count);
            var sb = new StringBuilder();
            foreach (ValidationResult res in results)
            {
                string message = res.Message;
                if (!uniqueMessages.Contains(message))
                {   // show only unique
                    sb.AppendLine(message);
                    uniqueMessages.Add(message);
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Method occured, when breaks collection changed. Need for validation.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        protected virtual void BreaksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        #endregion

        #region private methods

        /// <summary>
        /// Init validationproxy.
        /// </summary>
        private void _InitValidation()
        {
            _validationProxy = new PropertyValidationProxy();
            _validationProxy.ProvidesCustomValueConversion = false;
            _validationProxy.SpecificationSource = ValidationSpecificationSource.Attributes;
            _validationProxy.ValidatedType = GetType();
            _validationProxy.Ruleset = "";
        }

        /// <summary>
        /// Validate property.
        /// </summary>
        /// <param name="propName">Property name.</param>
        /// <returns>Error string or null.</returns>
        private string _ValidateProperty(string propName)
        {
            string error = null;

            if (_CanValidate(propName))
            {
                _validationProxy.ValidatedPropertyName = propName;

                ValidationIntegrationHelper hlp = new ValidationIntegrationHelper(
                    _validationProxy);

                Validator validator = hlp.GetValidator();
                ValidationResults res = validator.Validate(this);
                if (!res.IsValid)
                    error = _FindError(res, propName);
            }

            return error;
        }

        /// <summary>
        /// Validate this Break.
        /// </summary>
        /// <returns><c>ValidationResults</c></returns>
        private ValidationResults _ValidateObject()
        {
            Validator validator = ValidationFactory.CreateValidator(
                GetType());

            return validator.Validate(this);
        }

        /// <summary>
        /// Checking can validate property or not.
        /// </summary>
        /// <param name="propName">Property name.</param>
        /// <returns><returns>
        private bool _CanValidate(string propName)
        {
            bool canValidate = false;

            PropertyInfo pi = GetType().GetProperty(propName);
            if (pi != null)
            {
                object[] attrs = pi.GetCustomAttributes(false);
                if (attrs != null)
                {
                    foreach (object obj in attrs)
                    {
                        if (obj is BaseValidationAttribute)
                        {
                            canValidate = true;
                            break;
                        }
                    }
                }
            }

            return canValidate;
        }

        /// <summary>
        /// Look for string in validation results.
        /// </summary>
        /// <param name="results"><c>ValidationResults</c>.</param>
        /// <param name="propName">Name of property.</param>
        /// <returns>Error message or null.</returns>
        private string _FindError(ValidationResults results, string propName)
        {
            string error = null;
            foreach (ValidationResult res in results)
            {
                if (res.Key.Equals(propName))
                {
                    error = res.Message;
                    break;
                }
            }

            return error;
        }

        #endregion private methods

        #region Private constants

        /// <summary>
        /// Name of the Duration property.
        /// </summary>
        private const string PROP_NAME_DURATION = "Duration";

        /// <summary>
        /// Name of the primary validation tag.
        /// </summary>
        internal const string PRIMARY_VALIDATOR_TAG = "Primary";
            
        /// <summary>
        /// Default break's duration.
        /// </summary>
        internal const double DEFAULT_DURATION = 30;

        #endregion // Private constants

        #region Private members
        
        /// <summary>
        /// Used for validation.
        /// </summary>
        private PropertyValidationProxy _validationProxy;

        /// <summary>
        /// Duration in minutes.
        /// </summary>
        private double _duration { get; set; }

        /// <summary>
        /// Breaks collection with this break.
        /// </summary>
        private Breaks _breaks;

        #endregion
    }
}
