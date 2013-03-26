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
using System.Diagnostics;
using System.ComponentModel;

using Microsoft.Practices.EnterpriseLibrary.Validation;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects.Validation;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Collections;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a location.
    /// </summary>
    /// <remarks>Typically location is a depot or warehouse.</remarks>
    public class Location : DataObject, IMarkableAsDeleted, IGeocodable,
        ISupportOwnerCollection
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_Name; }
        }

        /// <summary>
        /// Name of the Comment property
        /// </summary>
        public static string PropertyNameComment
        {
            get { return PROP_NAME_Comment; }
        }

        /// <summary>
        /// Name of the CurbApproach property
        /// </summary>
        public static string PropertyNameCurbApproach
        {
            get { return PROP_NAME_CurbApproach; }
        }

        /// <summary>
        /// Name of the TimeWindow property
        /// </summary>
        public static string PropertyNameTimeWindow
        {
            get { return PROP_NAME_TimeWindow; }
        }

        /// <summary>
        /// Name of the TimeWindow2 property
        /// </summary>
        public static string PropertyNameTimeWindow2
        {
            get { return PROP_NAME_TimeWindow2; }
        }

        /// <summary>
        /// Name of the Address property.
        /// </summary>
        public static string PropertyNameAddress
        {
            get { return PROP_NAME_Address; }
        }

        /// <summary>
        /// Name of the GeoLocation property
        /// </summary>
        public static string PropertyNameGeoLocation
        {
            get { return PROP_NAME_GeoLocation; }
        }

        #endregion constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Location</c> class.
        /// </summary>
        public Location()
            : base(DataModel.Locations.CreateLocations(Guid.NewGuid()))
        {
            // Enable address validation.
            IsAddressValidationEnabled = true;

            _Entity.CurbApproach = (int)Defaults.Instance.LocationsDefaults.CurbApproach;
            _timeWindow.IsWideOpen = Defaults.Instance.LocationsDefaults.TimeWindow.IsWideopen;
            if (!_timeWindow.IsWideOpen)
            {
                _timeWindow.From = Defaults.Instance.LocationsDefaults.TimeWindow.From;
                _timeWindow.To = Defaults.Instance.LocationsDefaults.TimeWindow.To;
                _timeWindow.Day = 0;
            }

            _UpdateTimeWindowEntityData();
            _UpdateTimeWindow2EntityData();

            _timeWindow.PropertyChanged += new PropertyChangedEventHandler(_TimeWindowPropertyChanged);
            _SubscribeToAddressEvent();

            base.SetCreationTime();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Location</c> class.
        /// </summary>
        /// <param name="entity">Entity data.</param>
        internal Location(DataModel.Locations entity)
            : base(entity)
        {
            Debug.Assert(0 < entity.CreationTime); // NOTE: must be inited

            // Enable address validation.
            IsAddressValidationEnabled = true;

            // init holder objects
            _InitTimeWindow(entity);
            _InitTimeWindow2(entity);
            _InitAddress(entity);
            _InitGeoLocation(entity);
        }

        #endregion constructors

        #region public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.Location; }
        }
        
        /// <summary>
        /// Gets the object's globally unique identifier.
        /// </summary>
        public override Guid Id
        {
            get { return _Entity.Id; }
        }
        /// <summary>
        /// Gets\sets object creation time.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">Although property can get null value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Although property can get 0 or less value
        /// (for backward compatibility with existent plug-ins) it is not actually supported.</exception>
        public override long? CreationTime
        {
            get
            {
                Debug.Assert(0 < _Entity.CreationTime); // NOTE: must be inited
                return _Entity.CreationTime;
            }
            set
            {
                if (!value.HasValue)
                    throw new ArgumentNullException(); // exception
                if (value.Value <= 0)
                    throw new ArgumentOutOfRangeException(); // exception

                _Entity.CreationTime = value.Value;
            }
        }

        /// <summary>
        /// Location name.
        /// </summary>
        [DomainProperty("DomainPropertyNameName", true)]
        [DuplicateNameValidator]
        [NameNotNullValidator]
        public override string Name
        {
            get { return _Entity.Name; }
            set
            {
                // Save current name.
                var name = _Entity.Name;

                // Set new name.
                _Entity.Name = value;

                // Raise Property changed event for all items which 
                // has the same name, as item's old name.
                if ((this as ISupportOwnerCollection).OwnerCollection != null)
                    DataObjectValidationHelper.RaisePropertyChangedForDuplicate
                        ((this as ISupportOwnerCollection).OwnerCollection, name);

                NotifyPropertyChanged(PROP_NAME_Name);
            }
        }

        /// <summary>
        /// Arbitrary text about the location.
        /// </summary>
        [DomainProperty("DomainPropertyNameComment")]
        public string Comment
        {
            get { return _Entity.Comment; }
            set 
            { 
                _Entity.Comment = value;
                NotifyPropertyChanged(PROP_NAME_Comment);
            }
        }

        /// <summary>
        /// Curb approach for the location.
        /// </summary>
        [DomainProperty("DomainPropertyNameCurbApproach")]
        [AffectsRoutingProperty]
        public CurbApproach CurbApproach
        {
            get { return (CurbApproach)_Entity.CurbApproach; }
            set
            {
                _Entity.CurbApproach = (int)value;
                NotifyPropertyChanged(PROP_NAME_CurbApproach);
            }
        }

        /// <summary>
        /// Time window when the location is opened.
        /// </summary>
        [DomainProperty("DomainPropertyNameTimeWindow")]
        [AffectsRoutingProperty]
        public TimeWindow TimeWindow
        {
            get { return _timeWindow; }
            set
            {
                _timeWindow.PropertyChanged -= _TimeWindowPropertyChanged;

                if (value != null)
                {
                    _timeWindow = value;
                    _UpdateTimeWindowEntityData();

                    _timeWindow.PropertyChanged += new PropertyChangedEventHandler(
                        _TimeWindowPropertyChanged);
                }
                else
                {
                    _ClearTimeWindow();
                    _UpdateTimeWindowEntityData();
                }

                NotifyPropertyChanged(PROP_NAME_TimeWindow);
            }
        }

        /// <summary>
        /// Time window 2 when the location is opened.
        /// </summary>
        [TimeWindow2Validator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameTimeWindow2")]
        [AffectsRoutingProperty]
        public TimeWindow TimeWindow2
        {
            get { return _timeWindow2; }
            set
            {
                _timeWindow2.PropertyChanged -= _TimeWindow2PropertyChanged;

                if (value != null)
                {
                    _timeWindow2 = value;
                    _UpdateTimeWindow2EntityData();

                    _timeWindow2.PropertyChanged += new PropertyChangedEventHandler(
                        _TimeWindow2PropertyChanged);
                }
                else
                {
                    _ClearTimeWindow2();
                    _UpdateTimeWindow2EntityData();
                }

                NotifyPropertyChanged(PROP_NAME_TimeWindow2);
            }
        }

        #endregion public members

        #region IGeocodable members

        /// <summary>
        /// Address of the Location.
        /// </summary>
        [ObjectFarFromRoadValidator(MessageTemplateResourceName = "Error_LocationNotFoundOnNetworkViolationMessage",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty]
        public Address Address
        {
            get { return _address; }
            set
            {
                _address.PropertyChanged -= Address_PropertyChanged;

                if (value != null)
                {
                    _address = value;
                    _UpdateAddressEntityData();
                    _SubscribeToAddressEvent();
                }
                else
                {
                    _ClearAddress();
                    _UpdateAddressEntityData();
                }

                NotifyPropertyChanged(PROP_NAME_Address);
            }
        }
        /// <summary>
        /// Geolocation of the Location
        /// </summary>
        [DomainProperty]
        public Point? GeoLocation
        {
            get { return _geoLocation; }
            set
            {
                _geoLocation = value;
                if (value == null)
                {
                    _Entity.X = null;
                    _Entity.Y = null;
                }
                else
                {
                    _Entity.X = _geoLocation.Value.X;
                    _Entity.Y = _geoLocation.Value.Y;
                }
                NotifyPropertyChanged(PROP_NAME_GeoLocation);
            }
        }

        /// <summary>
        /// Returns a value based on whether or not the address has been geocoded.
        /// </summary>
        [GeocodableValidator(MessageTemplateResourceName = "Error_LocationNotGeocoded",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        public bool IsGeocoded
        {
            get { return _geoLocation.HasValue; }
        }

        /// <summary>
        /// Property which turn on/off address validation.
        /// </summary>
        public bool IsAddressValidationEnabled
        {
            get;
            set;
        }

        #endregion IGeocodable members

        #region IDataErrorInfo members
        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public override string this[string columnName]
        {
            get
            {
                if (Address.IsAddressPropertyName(columnName))
                    return _ValidateAddress();
                else
                    return base[columnName];
            }
        }

        #endregion IDataErrorInfo members

        #region public methods

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the location.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion public methods

        #region ICloneable members

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Location obj = new Location();
            this.CopyTo(obj);

            return obj;
        }

        #endregion ICloneable members

        #region ICopyable members

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        public override void CopyTo(DataObject obj)
        {
            Location location = obj as Location;
            location.Name = this.Name;
            location.Comment = this.Comment;
            location.CurbApproach = this.CurbApproach;

            if (null != this.TimeWindow)
                location.TimeWindow = (TimeWindow)this.TimeWindow.Clone();

            if (null != this.TimeWindow2)
                location.TimeWindow2 = (TimeWindow)this.TimeWindow2.Clone();

            if (null != this.Address)
                location.Address = (Address)this.Address.Clone();

            if (GeoLocation.HasValue)
                location.GeoLocation = new Point(this.GeoLocation.Value.X, this.GeoLocation.Value.Y);
        }
        #endregion ICopyable members

        #region IMarkableAsDeleted interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        bool IMarkableAsDeleted.IsMarkedAsDeleted
        {
            get { return _Entity.Deleted; }
            set { _Entity.Deleted = value; }
        }

        #endregion IMarkableAsDeleted interface members

        #region ISupportOwnerCollection members

        /// <summary>
        /// Collection in which this DataObject is placed.
        /// </summary>
        IEnumerable ISupportOwnerCollection.OwnerCollection
        {
            get;
            set;
        }

        #endregion ISupportOwnerCollection members

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets entity data.
        /// </summary>
        private DataModel.Locations _Entity
        {
            get { return (DataModel.Locations)base.RawEntity; }
        }

        #endregion private properties

        #region private methods

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Handler for TimeWindow PropertyChanged event.
        /// </summary>
        /// <param name="sender">Source of an event.</param>
        /// <param name="e">Event data.</param>
        private void _TimeWindowPropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            _UpdateTimeWindowEntityData();
            NotifySubPropertyChanged(PROP_NAME_TimeWindow, e.PropertyName);
        }

        /// <summary>
        /// Handler for TimeWindow2 PropertyChanged event.
        /// </summary>
        /// <param name="sender">Source of an event.</param>
        /// <param name="e">Event data.</param>
        private void _TimeWindow2PropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            _UpdateTimeWindow2EntityData();
            NotifySubPropertyChanged(PROP_NAME_TimeWindow2, e.PropertyName);
        }

        /// <summary>
        /// Initializes TimeWindow usind entity data.
        /// </summary>
        /// <param name="entity">Entity data.</param>
        private void _InitTimeWindow(DataModel.Locations entity)
        {
            if (entity.OpenFrom != null && entity.OpenTo != null)
                _SetTimeWindow(entity);
            else
                _ClearTimeWindow();

            _timeWindow.PropertyChanged += new PropertyChangedEventHandler(_TimeWindowPropertyChanged);
        }

        /// <summary>
        /// Initializes TimeWindow2 usind entity data.
        /// </summary>
        /// <param name="entity">Entity data.</param>
        private void _InitTimeWindow2(DataModel.Locations entity)
        {
            if (entity.OpenFrom2 != null && entity.OpenTo2 != null)
                _SetTimeWindow2(entity);
            else
                _ClearTimeWindow2();

            _timeWindow2.PropertyChanged += new PropertyChangedEventHandler(_TimeWindow2PropertyChanged);
        }

        /// <summary>
        /// Sets time window using data from database.
        /// </summary>
        /// <param name="entity">Entity object Locations.</param>
        private void _SetTimeWindow(DataModel.Locations entity)
        {
            _timeWindow =
                TimeWindow.CreateFromEffectiveTimes(new TimeSpan((long)entity.OpenFrom),
                                                    new TimeSpan((long)entity.OpenTo));
        }

        /// <summary>
        /// Sets time window 2 using data from database.
        /// </summary>
        /// <param name="entity">Entity object Locations.</param>
        private void _SetTimeWindow2(DataModel.Locations entity)
        {
            _timeWindow2 =
                TimeWindow.CreateFromEffectiveTimes(new TimeSpan((long)entity.OpenFrom2),
                                                    new TimeSpan((long)entity.OpenTo2));
        }

        /// <summary>
        /// Clears time window.
        /// </summary>
        private void _ClearTimeWindow()
        {
            _timeWindow.From = new TimeSpan();
            _timeWindow.To = new TimeSpan();
            _timeWindow.Day = 0;
            _timeWindow.IsWideOpen = true;
        }

        /// <summary>
        /// Clears time window 2.
        /// </summary>
        private void _ClearTimeWindow2()
        {
            _timeWindow2.From = new TimeSpan();
            _timeWindow2.To = new TimeSpan();
            _timeWindow2.Day = 0;
            _timeWindow2.IsWideOpen = true;
        }

        /// <summary>
        /// Updates data of time window in database.
        /// </summary>
        private void _UpdateTimeWindowEntityData()
        {
            if (!_timeWindow.IsWideOpen)
            {
                _Entity.OpenFrom = _timeWindow.EffectiveFrom.Ticks;
                _Entity.OpenTo = _timeWindow.EffectiveTo.Ticks;
            }
            else
            {
                _Entity.OpenFrom = null;
                _Entity.OpenTo = null;
            }
        }

        /// <summary>
        /// Updates data of time window 2 in database.
        /// </summary>
        private void _UpdateTimeWindow2EntityData()
        {
            if (!_timeWindow2.IsWideOpen)
            {
                _Entity.OpenFrom2 = _timeWindow2.EffectiveFrom.Ticks;
                _Entity.OpenTo2 = _timeWindow2.EffectiveTo.Ticks;
            }
            else
            {
                _Entity.OpenFrom2 = null;
                _Entity.OpenTo2 = null;
            }
        }

        /// <summary>
        /// Handler for Address PropertyChanged event.
        /// </summary>
        /// <param name="sender">Source of an event.</param>
        /// <param name="e">Event data.</param>
        private void Address_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateAddressEntityData();

            NotifySubPropertyChanged(PROP_NAME_Address, e.PropertyName);
        }

        /// <summary>
        /// Initializes Address property using entity data.
        /// </summary>
        /// <param name="entity">Entity data.</param>
        private void _InitAddress(DataModel.Locations entity)
        {
            Address.FullAddress = entity.FullAddress;
            Address.Unit = entity.Unit;
            Address.AddressLine = entity.AddressLine;
            Address.Locality1 = entity.Locality1;
            Address.Locality2 = entity.Locality2;
            Address.Locality3 = entity.Locality3;
            Address.CountyPrefecture = entity.CountyPrefecture;
            Address.PostalCode1 = entity.PostalCode1;
            Address.PostalCode2 = entity.PostalCode2;
            Address.StateProvince = entity.StateProvince;
            Address.Country = entity.Country;
            Address.MatchMethod = entity.Locator; // ToDo rename MatchMethod
            _SubscribeToAddressEvent();
        }

        /// <summary>
        /// Clears address property.
        /// </summary>
        private void _ClearAddress()
        {
            Address.FullAddress = string.Empty;
            Address.Unit = string.Empty;
            Address.AddressLine = string.Empty;
            Address.Locality1 = string.Empty;
            Address.Locality2 = string.Empty;
            Address.Locality3 = string.Empty;
            Address.CountyPrefecture = string.Empty;
            Address.PostalCode1 = string.Empty;
            Address.PostalCode2 = string.Empty;
            Address.StateProvince = string.Empty;
            Address.Country = string.Empty;
            Address.MatchMethod = string.Empty;
        }

        /// <summary>
        /// Updates Address entity data.
        /// </summary>
        private void _UpdateAddressEntityData()
        {
            _Entity.FullAddress = Address.FullAddress;
            _Entity.Unit = Address.Unit;
            _Entity.AddressLine = Address.AddressLine;
            _Entity.Locality1 = Address.Locality1;
            _Entity.Locality2 = Address.Locality2;
            _Entity.Locality3 = Address.Locality3;
            _Entity.CountyPrefecture = Address.CountyPrefecture;
            _Entity.PostalCode1 = Address.PostalCode1;
            _Entity.PostalCode2 = Address.PostalCode2;
            _Entity.StateProvince = Address.StateProvince;
            _Entity.Country = Address.Country;
            _Entity.Locator = Address.MatchMethod; // ToDo rename MatchMethod
        }

        /// <summary>
        /// Initializes geo location using entity data.
        /// </summary>
        /// <param name="entity">Entity data.</param>
        private void _InitGeoLocation(DataModel.Locations entity)
        {
            if (entity.X != null && entity.Y != null)
                _geoLocation = new Point(entity.X.Value, entity.Y.Value);
        }

        /// <summary>
        /// Sets event handler for the PropertyChanged event of Address.
        /// </summary>
        private void _SubscribeToAddressEvent()
        {
            _address.PropertyChanged += new PropertyChangedEventHandler(Address_PropertyChanged);
        }

        /// <summary>
        /// Validates address.
        /// </summary>
        /// <returns></returns>
        private string _ValidateAddress()
        {
            // If we turned off validation - do nothing.
            if (!IsAddressValidationEnabled)
                return string.Empty;

            GeocodableValidator validator = new GeocodableValidator(Properties.Messages.Error_LocationNotGeocoded);
            ValidationResults results = validator.Validate(IsGeocoded);

            // If validation result is valid - check match method. If it is "Edited X/Y far from road" - address is not valid.
            if (results.IsValid)
            {
                ObjectFarFromRoadValidator objectFarFromRoadValidator
                    = new ObjectFarFromRoadValidator(Properties.Messages.Error_LocationNotFoundOnNetworkViolationMessage);
                results = objectFarFromRoadValidator.Validate(Address);
            }

            string message = string.Empty;
            if (!results.IsValid)
            {
                foreach (ValidationResult result in results)
                {
                    message = result.Message;
                    break;
                }
            }

            return message;
        }

        #endregion private methods
        
        #region private constants

        /// <summary>
        /// Name of Name Property.
        /// </summary>
        private const string PROP_NAME_Name = "Name";

        /// <summary>
        /// Name of Comment Property.
        /// </summary>
        private const string PROP_NAME_Comment = "Comment";

        /// <summary>
        /// Name of CurbApproach Property.
        /// </summary>
        public const string PROP_NAME_CurbApproach = "CurbApproach";

        /// <summary>
        /// Name of TimeWindow Property.
        /// </summary>
        private const string PROP_NAME_TimeWindow = "TimeWindow";

        /// <summary>
        /// Name of TimeWindow2 Property.
        /// </summary>
        private const string PROP_NAME_TimeWindow2 = "TimeWindow2";

        /// <summary>
        /// Name of Address Property.
        /// </summary>
        private const string PROP_NAME_Address = "Address";

        /// <summary>
        /// Name of GeoLocation Property.
        /// </summary>
        private const string PROP_NAME_GeoLocation = "GeoLocation";

        #endregion private constants

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// First time window.
        /// </summary>
        private TimeWindow _timeWindow = new TimeWindow();

        /// <summary>
        /// Second time window.
        /// </summary>
        private TimeWindow _timeWindow2 = new TimeWindow();

        /// <summary>
        /// Address.
        /// </summary>
        private Address _address = new Address();

        /// <summary>
        /// Geo location point.
        /// </summary>
        private Point? _geoLocation;

        #endregion private members
    }
}
