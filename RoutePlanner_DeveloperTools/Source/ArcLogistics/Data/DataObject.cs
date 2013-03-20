using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using ESRI.ArcLogistics.Data.Validation;
using ESRI.ArcLogistics.Utility.ComponentModel;
using ESRI.ArcLogistics.Utility.Reflection;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Integration;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DataObject class represents a base abstract class for other domain objects
    /// that can be serialized to the database.
    /// </summary>
    public abstract class DataObject :
        NotifyPropertyChangedBase,
        IModifyState,
        IValidatable,
        IDataErrorInfo,
        ICloneable,
        IRawDataAccess,
        ICopyable,
        INotifyPropertyChanged,
        IForceNotifyPropertyChanged,
        ISupportName
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>DataObject</c> class.
        /// </summary>
        protected DataObject(EntityObject entity)
        {
            Debug.Assert(entity != null);

            _InitValidation();

            // set entity's data object reference 
            IWrapDataAccess wda = entity as IWrapDataAccess;
            if (wda == null)
                Debug.Assert(false, "Entity object does not implement IWrapDataAccess.");

            wda.DataObject = this;

            _entity = entity;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public abstract string TypeTitle
        {
            get;
        }

        /// <summary>
        /// Gets the object's globally unique identifier.
        /// </summary>
        public abstract Guid Id
        {
            get;
        }

        /// <summary>
        /// Gets/sets object creation time.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">Although property can get null value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Although property can get 0 or less value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        public abstract long? CreationTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether data object was stored in database.
        /// </summary>
        public bool IsStored
        {
            get
            {
                return (_entity.EntityState == EntityState.Unchanged ||
                        _entity.EntityState == EntityState.Modified);
            }
        }

        /// <summary>
        /// Gets a value indicating whether object can be stored in database.
        /// </summary>
        /// <remarks>
        /// The value is false by default.
        /// </remarks>
        internal virtual bool CanSave
        {
            get { return _canSave; }
            set { _canSave = value; }
        }

        #endregion public properties

        #region IModifyState interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public virtual bool IsModified
        {
            get
            {
                // TODO: the value is not affected on relation changes, is it ok for us?
                return (_entity.EntityState & EntityState.Modified) == EntityState.Modified;
            }
        }

        #endregion IModifyState interface members

        #region IValidatable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets a value indicating whether the object is valid.
        /// </summary>
        public virtual bool IsValid
        {
            get
            {
                ValidationResults res = _ValidateObject();
                return res.IsValid;
            }
        }
        /// <summary>
        /// Gets the descriptions of all the errors associated with the object.
        /// </summary>
        public virtual string FullError
        {
            get
            {
                return this.Error;
            }
        }

        /// <summary>
        /// Gets the list of errors which affect routing.
        /// </summary>
        /// <remarks>
        /// Each time a routing operation is executed, ArcLogistics checks whether there are
        /// primary errors in routes and orders. If so, routing operation fails.
        /// </remarks>
        public virtual string PrimaryError
        {
            get
            {
                string error = null;
                ValidationResults res = _ValidateObject();
                if (!res.IsValid)
                {
                    ValidationResults filtredRes =
                        res.FindAll(TagFilter.Include, PRIMARY_VALIDATOR_TAG);

                    if (!filtredRes.IsValid)
                        error = FormatErrorString(filtredRes);
                }

                return error;
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        public virtual string GetPropertyError(string propName)
        {
            return this[propName];
        }

        #endregion IValidatable interface members

        #region IDataErrorInfo interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public virtual string Error
        {
            get
            {
                ValidationResults res = _ValidateObject();
                return res.IsValid ? null : FormatErrorString(res);
            }
        }
        
        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        public virtual string this[string columnName]
        {
            get
            {
                // TODO: add support of several strings in result delimited with \n
                return _ValidateProperty(columnName);
            }
        }

        #endregion IDataErrorInfo interface members

        #region ICloneable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        #endregion ICloneable interface members

        #region IRawDataAccess interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        EntityObject IRawDataAccess.RawEntity
        {
            get { return RawEntity; }
        }

        #endregion IRawDataAccess interface members

        #region ICopyable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        public virtual void CopyTo(DataObject obj)
        {
            Debug.Assert(false); // NOTE: need replace in child classes
        }

        #endregion ICopyable interface members

        #region IForceNotifyPropertyChanged Members

        /// <summary>
        /// Call property changed event for selected property.
        /// </summary>
        /// <param name="propertyName">Property for which event is called.</param>
        void IForceNotifyPropertyChanged.RaisePropertyChangedEvent(string propertyName)
        {
            if (propertyName != null && this.GetType().GetProperty(propertyName) != null)
                NotifyPropertyChanged(propertyName);
        }

        #endregion

        #region ISupportName Members

        /// <summary>
        /// Data object Name property. Not implemented here.
        /// </summary>
        public abstract string Name
        {
            get;
            set;
        }

        #endregion

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            DataObject dataObject = (DataObject)obj;
            return _entity.Equals(dataObject.RawEntity);
        }
        /// <summary>
        /// Serves as a hash function.
        /// </summary>
        /// <returns>
        /// A hash code for the current Object.
        /// </returns>
        public override int GetHashCode()
        {
            return _entity.GetHashCode();
        }

        #endregion public methods

        #region protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets underlying entity object of <c>DataObject</c> instance.
        /// </summary>
        protected EntityObject RawEntity
        {
            get { return _entity; }
        }

        #endregion protected properties

        #region protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // APIREV: we don't want to expose these methods to public API

        /// <summary>
        /// Formats error string that will be returned by IDataErrorInfo.Error property.
        /// </summary>
        /// <param name="results"><c>ValidationResults</c> object.</param>
        /// <returns>An error string.</returns>
        internal protected virtual string FormatErrorString(ValidationResults results)
        {
            Debug.Assert(results != null);

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
        /// Sets object creation time to current time.
        /// </summary>
        protected virtual void SetCreationTime()
        {
            this.CreationTime = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Fire when object subproperty changed.
        /// </summary>
        protected void NotifySubPropertyChanged(string property, string subproperty)
        {
            this.NotifyPropertyChanged(subproperty);
            this.NotifyPropertyChanged(property);
        }
        #endregion protected methods

        #region private classes
        /// <summary>
        /// Provides access to validators for data object instances and their properties.
        /// </summary>
        private sealed class ValidatorsContainer
        {
            /// <summary>
            /// Gets or sets a reference to the data object validator.
            /// </summary>
            public Validator ObjectValidator
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a reference to the dictionary mapping property names into their
            /// validators.
            /// </summary>
            public IDictionary<string, Validator> PropertyValidators
            {
                get;
                set;
            }
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Creates validators for objects of the specified type and it's properties.
        /// </summary>
        /// <param name="objectType">The type of objects to create validators for.</param>
        /// <returns>An instance of the validators container with validators for the specified
        /// type.</returns>
        private static ValidatorsContainer _CreateValidators(Type objectType)
        {
            Debug.Assert(objectType != null);

            var propertyFlags =
                BindingFlags.Instance |
                BindingFlags.Public;
            var validateableProperties =
                from propertyInfo in objectType.GetProperties(propertyFlags)
                where propertyInfo.GetCustomAttributes<BaseValidationAttribute>().Any()
                select propertyInfo;

            var propertyValidators = new Dictionary<string, Validator>();
            foreach (var propertyInfo in validateableProperties)
            {
                var validationProxy = new PropertyValidationProxy
                {
                    ProvidesCustomValueConversion = false,
                    SpecificationSource = ValidationSpecificationSource.Attributes,
                    ValidatedType = objectType,
                    Ruleset = string.Empty,
                    ValidatedPropertyName = propertyInfo.Name,
                };

                var validationHelper = new ValidationIntegrationHelper(validationProxy);
                var validator = validationHelper.GetValidator();

                propertyValidators.Add(propertyInfo.Name, validator);
            }

            var objectValidator = ValidationFactory.CreateValidator(objectType);

            return new ValidatorsContainer
            {
                ObjectValidator = objectValidator,
                PropertyValidators = propertyValidators,
            };
        }
        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // validation helper methods
        private void _InitValidation()
        {
            var validators = default(ValidatorsContainer);
            var objectType = this.GetType();
            lock (_dataObjectValidatorsGuard)
            {
                if (!_dataObjectValidators.TryGetValue(objectType, out validators))
                {
                    validators = _CreateValidators(objectType);
                    _dataObjectValidators[objectType] = validators;
                }
            }

            _objectValidator = validators.ObjectValidator;
            _propertyValidators = validators.PropertyValidators;
        }

        private string _ValidateProperty(string propName)
        {
            var error = default(string);

            var validator = default(Validator);
            if (!_propertyValidators.TryGetValue(propName, out validator))
            {
                // Can't validate the property without corresponding validator.
                return error;
            }

            var result = validator.Validate(this);
            if (!result.IsValid)
            {
                error = _FindError(result, propName);
            }

            return error;
        }

        private ValidationResults _ValidateObject()
        {
            return _objectValidator.Validate(this);
        }

        private string _FindError(ValidationResults results, string propName)
        {
            string error = null;
            StringBuilder str = new StringBuilder();
            foreach (ValidationResult res in results)
            {
                if (res.Key.Equals(propName))
                {
                    error = res.Message;
                    if (error != null )
                    {   
                        if(str.Length != 0) 
                            //If error not empty, and new line to previous errors.
                            str.Append(Environment.NewLine);
                        str.Append(error);
                    }
                }
            }
            return str.ToString();
        }
        #endregion private methods

        #region internal constants

        internal const string PRIMARY_VALIDATOR_TAG = "Primary";

        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // underlying entity object
        private EntityObject _entity;

        // CanSave flag
        private bool _canSave = false;

        // Maps property names into their validators.
        private IDictionary<string, Validator> _propertyValidators;

        // Validator for this data object instance.
        private Validator _objectValidator;

        // Maps specific data object type into it's validators.
        private static IDictionary<Type, ValidatorsContainer> _dataObjectValidators =
            new Dictionary<Type, ValidatorsContainer>();

        // An object for serializing access to the _dataObjectValidators field.
        private static object _dataObjectValidatorsGuard = new object();
        #endregion private fields
    }
}
