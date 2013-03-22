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
using System.ComponentModel;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Class represents custom order property which has properties name and description.
    /// </summary>
    internal class CustomOrderProperty :
        INotifyPropertyChanged,
        IDataErrorInfo,
        ICloneable
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of CustomOrderProperty.
        /// </summary>
        public CustomOrderProperty()
        {
            _maximumLength = DEFAULT_MAXIMUM_LENGTH;
            _orderPairKey = false;
        }

        /// <summary>
        /// Initializes a new instance of CustomOrderProperty.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="description">Description.</param>
        public CustomOrderProperty(string name, string description)
        {
            Debug.Assert(name != null);
            Debug.Assert(description != null);

            _name = name;
            _description = description;

            _maximumLength = DEFAULT_MAXIMUM_LENGTH;
            _orderPairKey = false;
        }

        /// <summary>
        /// Initializes a new instance of CustomOrderProperty.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="description">Description.</param>
        /// <param name="maxLength">Maximum length.</param>
        public CustomOrderProperty(string name, string description, int maxLength, bool orderPairKey)
        {
            Debug.Assert(name != null);
            Debug.Assert(maxLength > 0);

            _name = name;
            _description = description != null ? description : string.Empty;
            _maximumLength = maxLength;
            _orderPairKey = orderPairKey;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Gets or sets name of custom order property.
        /// </summary>
        public string Name
        {
            get { return _name; }

            set 
            {
                if (_name != value)
                {
                    _name = value;
                    _NotifyPropertyChanged(PROPERTY_NAME);
                }
            }
        }

        /// <summary>
        /// Gets or sets description of custom order property.
        /// </summary>
        public string Description
        {
            get { return _description; }

            set
            {
                if (_description != value)
                {
                    _description = value;
                    _NotifyPropertyChanged(PROPERTY_DESCRIPTION);
                }
            }
        }

        /// <summary>
        /// Gets maximum length of the property.
        /// </summary>
        public int MaximumLength
        {
            get { return _maximumLength; }
        }

        /// <summary>
        /// Gets whether or not property can be used as a key pairing two orders..
        /// </summary>
        public bool OrderPairKey
        {
            get { return _orderPairKey; }
        }

        /// <summary>
        /// Gets or sets custom order property name validator.
        /// </summary>
        public ICustomOrderPropertyNameValidator NameValidator
        {
            get { return _propertyNameValidator; }

            set { _propertyNameValidator = value; }
        }

        #endregion Public Properties

        #region Public methods

        /// <summary>
        /// Returns the name of custom order property.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _name;
        }

        /// <summary>
        /// Compares this custom order property with given one.
        /// </summary>
        /// <param name="customOrderProperty">Custom order property to compare.</param>
        /// <returns>True - if this custom order property has the same name and description
        /// as onother one, otherwise - false.</returns>
        public bool CompareTo(CustomOrderProperty customOrderProperty)
        {
            bool comparisonResult =
                customOrderProperty != null &&
                Name.Equals(customOrderProperty.Name, StringComparison.CurrentCulture) &&
                Description.Equals(customOrderProperty.Description, StringComparison.CurrentCulture);

            return comparisonResult;
        }

        /// <summary>
        /// Function checks if this object is valid.
        /// </summary>
        /// <returns>True - if object is valid, otherwise - false.</returns>
        public bool IsValid()
        {
            bool validity = false;

            // If object has validator.
            if (_propertyNameValidator != null)
            {
                string errorMessage = string.Empty;

                validity = _propertyNameValidator.Validate(_name, out errorMessage);
            }
            // Object has no validator so we assume it is valid.
            else
            {
                validity = true;
            }

            return validity;
        }

        /// <summary>
        /// Raises PropertyChanged event for "Name" property.
        /// </summary>
        public void RaiseNamePropertyChangedEvent()
        {
            _NotifyPropertyChanged(PROPERTY_NAME);
        }

        #endregion Public methods

        #region INotifyPropertyChanged members

        /// <summary>
        /// Property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        #endregion INotifyPropertyChanged Members

        #region IDataErrorInfo members

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error
        {
            get
            {
                // Only "Name" property needs to be validated.
                return _ValidateProperty(PROPERTY_NAME);
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        public string this[string propertyName]
        {
            get
            {
                return _ValidateProperty(propertyName);
            }
        }

        #endregion IDataErrorInfo members

        #region IClonable members

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            // Create a clone of this object.
            CustomOrderProperty clone =
                new CustomOrderProperty(_name, _description, _maximumLength, _orderPairKey);

            return clone;
        }

        #endregion IClonable members

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

        #endregion Ptotected methods

        #region Private methods

        /// <summary>
        /// Validates property with given name.
        /// </summary>
        /// <param name="propertyName">Name of a property.</param>
        /// <returns>Validation error message which is an empty string if property is valid.</returns>
        private string _ValidateProperty(string propertyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(propertyName));

            // Result error message.
            string errorMessage = string.Empty;

            switch (propertyName)
            {
                // Validate "Name" property.
                case PROPERTY_NAME:
                    // Get error messsage which is an empty string if property's name is valid.
                    if (_propertyNameValidator != null)
                        _propertyNameValidator.Validate(_name, out errorMessage);
                    else
                        errorMessage = string.Empty;
                    break;

                // Description property is always valid.
                case PROPERTY_DESCRIPTION:
                    errorMessage = string.Empty;
                    break;

                default:
                    // Unknown property, this case should never happen.
                    Debug.Assert(false);
                    break;
            }

            return errorMessage;
        }

        #endregion Private methods

        #region Private constants

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROPERTY_NAME = "Name";

        /// <summary>
        /// Name of the Description property.
        /// </summary>
        private const string PROPERTY_DESCRIPTION = "Description";

        /// <summary>
        /// Default maximum length of property.
        /// </summary>
        private const int DEFAULT_MAXIMUM_LENGTH = 50;

        #endregion Private constants

        #region Private Fields

        /// <summary>
        /// Name of custom order property.
        /// </summary>
        private string _name = string.Empty;

        /// <summary>
        /// Description of custom order property.
        /// </summary>
        private string _description = string.Empty;

        /// <summary>
        /// Maximum length of property.
        /// </summary>
        private int _maximumLength;

        /// <summary>
        /// Property is used as a key for pairing orders.
        /// </summary>
        private bool _orderPairKey;

        /// <summary>
        /// Custom order property name validator.
        /// </summary>
        private ICustomOrderPropertyNameValidator _propertyNameValidator;

        #endregion Private Fields
    }
}
