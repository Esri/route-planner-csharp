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
using System.Collections.Specialized;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects.Validation;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Collections;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a vehicle.
    /// </summary>
    public class Vehicle : DataObject, IMarkableAsDeleted, ICapacitiesInit,
        ISupportOwnerCollection
    {
        #region public static properties
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
        /// Name of the Comment property.
        /// </summary>
        public static string PropertyNameComment
        {
            get { return PROP_NAME_Comment;}
        }

        /// <summary>
        /// Name of the FixedCost property.
        /// </summary>
        public static string PropertyNameFixedCost
        {
            get { return PROP_NAME_FixedCost;}
        }

        /// <summary>
        /// Name of the FuelEconomy property.
        /// </summary>
        public static string PropertyNameFuelEconomy
        {
            get { return PROP_NAME_FuelEconomy;}
        }

        /// <summary>
        /// Name of the FuelType property.
        /// </summary>
        public static string PropertyNameFuelType
        {
            get { return PROP_NAME_FuelType;}
        }

        /// <summary>
        /// Name of the MobileDevice property.
        /// </summary>
        public static string PropertyNameMobileDevice
        {
            get { return PROP_NAME_MobileDevice;}
        }

        /// <summary>
        /// Name of the Specialties property.
        /// </summary>
        public static string PropertyNameSpecialties
        {
            get { return PROP_NAME_Specialties;}
        }

        /// <summary>
        /// Name of the SpecialtiesCollection property.
        /// </summary>
        public static string PropertyNameSpecialtiesCollection
        {
            get { return PROP_NAME_SpecialtiesCollection;}
        }

        /// <summary>
        /// Name of the Capacities property.
        /// </summary>
        public static string PropertyNameCapacities
        {
            get { return PROP_NAME_Capacities;}
        }

        #endregion // Constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Vehicle</c> class.
        /// </summary>
        /// <param name="capacitiesInfo">Information about capacities. Get it from the project.</param>
        public Vehicle(CapacitiesInfo capacitiesInfo)
            : base(DataModel.Vehicles.CreateVehicles(Guid.NewGuid()))
        {
            _Entity.FuelConsumption = Defaults.Instance.VehiclesDefaults.FuelEconomy;
            _CreateCapacities(capacitiesInfo);
            _SpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(_Specialties_CollectionChanged);
            base.SetCreationTime();
        }

        internal Vehicle(DataModel.Vehicles entity)
            : base(entity)
        {
            Debug.Assert(0 < entity.CreationTime); // NOTE: must be inited

            _SpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(_Specialties_CollectionChanged);
            if (null != _MobileDeviceWrap.Value)
                _MobileDeviceWrap.Value.PropertyChanged += new PropertyChangedEventHandler(_MobileDevice_PropertyChanged);
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
            get { return Properties.Resources.Vehicle; }
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
        /// Vehicle name.
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
                    DataObjectValidationHelper.RaisePropertyChangedForDuplicate((this as ISupportOwnerCollection).OwnerCollection, name);

                NotifyPropertyChanged(PROP_NAME_Name);
            }
        }

        /// <summary>
        /// Arbitrary text about the vehicle.
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
        /// Amount of money that is paid for the vehicle per a day in currency units.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_COST / 2.0,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidFixedCost",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameFixedCost")]
        [UnitPropertyAttribute(Unit.Currency, Unit.Currency, Unit.Currency)]
        [AffectsRoutingProperty]
        public double FixedCost
        {
            // ToDo: need rename Entity property "FixedSalary" to "FixedCost"
            get { return _Entity.FixedSalary; }
            set
            {
                _Entity.FixedSalary = value;
                NotifyPropertyChanged(PROP_NAME_FixedCost);
            }
        }

        /// <summary>
        /// Vehicle's fuel economy in miles per gallon.
        /// </summary>
        [FuelEconomyValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameFuelEconomy")]
        [UnitPropertyAttribute(Unit.MilesPerGallon, Unit.MilesPerGallon, Unit.LitersPer100Kilometers)]
        [AffectsRoutingProperty]
        public double FuelEconomy
        {
            // ToDo: need rename Entity property "FuelConsumption" to "FuelEconomy"
            get { return _Entity.FuelConsumption; }
            set
            {
                _Entity.FuelConsumption = value;
                NotifyPropertyChanged(PROP_NAME_FuelEconomy);
            }
        }

        /// <summary>
        /// Fuel type used by vehicle.
        /// </summary>
        [NotNullValidator(MessageTemplateResourceName = "Error_InvalidFuelType",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [RefObjectValidator(MessageTemplateResourceName = "Error_InvalidRefObjFuelType",
                            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
                            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameFuelType")]
        [AffectsRoutingProperty]
        public FuelType FuelType
        {
            get { return _FuelTypeWrap.Value; }
            set
            {
                _FuelTypeWrap.Value = value;
                NotifyPropertyChanged(PROP_NAME_FuelType);
            }
        }

        /// <summary>
        /// Mobile device that belongs to the vehicle.
        /// </summary>
        [RefObjectValidator(MessageTemplateResourceName = "Error_InvalidRefObjMobileDevice",
                            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages))]
        [DomainProperty("DomainPropertyNameMobileDevice")]
        public MobileDevice MobileDevice
        {
            get { return _MobileDeviceWrap.Value; }
            set
            {
                if (null != _MobileDeviceWrap.Value)
                    _MobileDeviceWrap.Value.PropertyChanged -= _MobileDevice_PropertyChanged;
                _MobileDeviceWrap.Value = value;
                if (null != _MobileDeviceWrap.Value)
                    _MobileDeviceWrap.Value.PropertyChanged += new PropertyChangedEventHandler(_MobileDevice_PropertyChanged);

                NotifyPropertyChanged(PROP_NAME_MobileDevice);
            }
        }

        /// <summary>
        /// Collection of vehicle's specialties.
        /// </summary>
        [DomainProperty("DomainPropertyNameSpecialties")]
        [AffectsRoutingProperty]
        public IDataObjectCollection<VehicleSpecialty> Specialties
        {
            get { return _SpecialtiesWrap.DataObjects; }
            set
            {
                _SpecialtiesWrap.DataObjects.CollectionChanged -= _Specialties_CollectionChanged;
                _SpecialtiesWrap.DataObjects = value;
                _SpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(_Specialties_CollectionChanged);
                NotifyPropertyChanged(PROP_NAME_Specialties);
            }
        }

        /// <summary>
        /// Vehicle's capacities.
        /// </summary>
        [CapacityValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty]
        [AffectsRoutingProperty]
        public Capacities Capacities
        {
            get
            {
                if (_capacities == null)
                    throw new NotSupportedException(Properties.Resources.CapacityInfoIsNull);

                return _capacities; 
            }

            set
            {
                _capacities.PropertyChanged -= Capacities_PropertyChanged;

                if (value != null)
                {
                    _capacities = value;
                    _UpdateCapacitiesEntityData();
                }
                else
                {
                    _ClearCapacities();
                    _UpdateCapacitiesEntityData();
                }

                _capacities.PropertyChanged += new PropertyChangedEventHandler(
                    Capacities_PropertyChanged);

                NotifyPropertyChanged(PROP_NAME_Capacities);
            }
        }

        /// <summary>
        /// Information about capacities. The same as its project exposes.
        /// </summary>
        public CapacitiesInfo CapacitiesInfo
        {
            get { return _capacitiesInfo; }
            set
            {
                if (_capacitiesInfo != null)
                    throw new NotSupportedException(Properties.Resources.CapacitiesInfoAlreadySet);

                _capacitiesInfo = value;
                _InitCapacities(_Entity, _capacitiesInfo);
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        public override string this[string columnName]
        {
            get
            {
                int index = Capacities.GetCapacityPropertyIndex(columnName);
                if (-1 != index)
                    return _ValidateCapacity(index);
                else
                    return base[columnName];
            }
        }

        #endregion public members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// returns the name of the vehicle.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion public methods

        #region ICloneable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Vehicle obj = new Vehicle(this._capacitiesInfo);
            this.CopyTo(obj);

            return obj;
        }
        #endregion ICloneable interface members

        #region ICopyable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        public override void CopyTo(DataObject obj)
        {
            Debug.Assert(obj is Vehicle);

            Vehicle vehicle = obj as Vehicle;
            vehicle.Name = this.Name;
            vehicle.Comment = this.Comment;
            vehicle.FuelEconomy = this.FuelEconomy;
            vehicle.Capacities = (Capacities)this._capacities.Clone();
            vehicle.FixedCost = this.FixedCost;

            foreach (VehicleSpecialty spec in this.Specialties)
                vehicle.Specialties.Add(spec);

            vehicle.MobileDevice = this.MobileDevice;
            vehicle.FuelType = this.FuelType;

        }

        #endregion ICopyable interface members

        #region IMarkableAsDeleted interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets a value indicating whether data object is marked as deleted.
        /// </summary>
        bool IMarkableAsDeleted.IsMarkedAsDeleted
        {
            get { return _Entity.Deleted; }
            set { _Entity.Deleted = value; }
        }

        #endregion IMarkableAsDeleted interface members

        #region ICapacitiesInit members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets CapacitiesInfo.
        /// </summary>
        CapacitiesInfo ICapacitiesInit.CapacitiesInfo
        {
            set
            {
                if (this.CapacitiesInfo == null)
                    this.CapacitiesInfo = value;
            }
        }

        #endregion // ICapacitiesInit members

        #region ISupportOwnerCollection Members

        /// <summary>
        /// Collection in which this DataObject is placed.
        /// </summary>
        IEnumerable ISupportOwnerCollection.OwnerCollection
        {
            get;
            set;
        }

        #endregion

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.Vehicles _Entity
        {
            get
            {
                return (DataModel.Vehicles)base.RawEntity;
            }
        }


        private EntityRefWrapper<MobileDevice,
            DataModel.MobileDevices> _MobileDeviceWrap
        {
            get
            {
                if (_mobileDeviceRef == null)
                {
                    _mobileDeviceRef = new EntityRefWrapper<MobileDevice,
                        DataModel.MobileDevices>(_Entity.MobileDevicesReference, this);
                }

                return _mobileDeviceRef;
            }
        }

        private EntityCollWrapper<VehicleSpecialty,
            DataModel.VehicleSpecialties> _SpecialtiesWrap
        {
            get
            {
                if (_specialtiesColl == null)
                {
                    _specialtiesColl = new EntityCollWrapper<VehicleSpecialty,
                        DataModel.VehicleSpecialties>(_Entity.VehicleSpecialties, this, false);
                }

                return _specialtiesColl;
            }
        }

        private EntityRefWrapper<FuelType,
            DataModel.FuelTypes> _FuelTypeWrap
        {
            get
            {
                if (_fuelTypesRef == null)
                {
                    _fuelTypesRef = new EntityRefWrapper<FuelType,
                        DataModel.FuelTypes>(_Entity.FuelTypesReference, this);
                }

                return _fuelTypesRef;
            }
        }

        #endregion private properties

        #region private methods capacities
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize capacities from default values.
        /// </summary>
        /// <param name="capacitiesInfo">Capacity information.</param>
        private void _InitCapacitiesFromDefaults(CapacitiesInfo capacitiesInfo)
        {
            if (null == Defaults.Instance.VehiclesDefaults.CapacitiesDefaultValues)
                return; // defaults is empty

            var capacitiesDefaultValues = Defaults.Instance.VehiclesDefaults.CapacitiesDefaultValues.Capacity;
            for (int index = 0; index < capacitiesDefaultValues.Length; ++index)
            {
                try
                {   // init it as much as possible
                    var description = capacitiesDefaultValues[index];
                    for (int capacityIndex = 0; capacityIndex < capacitiesInfo.Count; ++capacityIndex)
                    {   // find capacity index by name
                        var capacityInfo = capacitiesInfo[capacityIndex];
                        if (capacityInfo.Name.Equals(description.Name, StringComparison.OrdinalIgnoreCase))
                        {   // set value by index
                            _capacities[capacityIndex] = description.Value;
                            break; // work done
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning(ex);
                }
            }

            _UpdateCapacitiesEntityData();
        }

        private void _CreateCapacities(CapacitiesInfo capacitiesInfo)
        {
            _capacitiesInfo = capacitiesInfo;
            _capacities = new Capacities(capacitiesInfo);
            _InitCapacitiesFromDefaults(capacitiesInfo);

            _capacities.PropertyChanged += new PropertyChangedEventHandler(Capacities_PropertyChanged);
        }

        private void Capacities_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateCapacitiesEntityData();

            NotifyPropertyChanged(e.PropertyName);

            NotifyPropertyChanged(PROP_NAME_Capacities);
        }

        private void _InitCapacities(DataModel.Vehicles entity, CapacitiesInfo capacitiesInfo)
        {
            _capacities = Capacities.CreateFromDBString(entity.Capacities, capacitiesInfo);
            _capacities.PropertyChanged += new PropertyChangedEventHandler(Capacities_PropertyChanged);
        }

        private void _ClearCapacities()
        {
            _capacities = new Capacities(_capacitiesInfo);
        }

        private void _UpdateCapacitiesEntityData()
        {
            _Entity.Capacities = Capacities.AssemblyDBString(_capacities);
        }

        private string _ValidateCapacity(int capIndex)
        {
            CapacityValidator capValidator = new CapacityValidator(capIndex);
            ValidationResults results = capValidator.Validate(_capacities);

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

        #endregion private methods capacities

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _MobileDevice_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_MobileDevice, e.PropertyName);
        }

        private void _Specialties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_Specialties, PROP_NAME_SpecialtiesCollection);
        }

        #endregion private methods

        #region private constants

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_Name = "Name";

        /// <summary>
        /// Name of the Comment property.
        /// </summary>
        private const string PROP_NAME_Comment = "Comment";

        /// <summary>
        /// Name of the FixedCost property.
        /// </summary>
        private const string PROP_NAME_FixedCost = "FixedCost";

        /// <summary>
        /// Name of the FuelEconomy property.
        /// </summary>
        private const string PROP_NAME_FuelEconomy = "FuelEconomy";

        /// <summary>
        /// Name of the FuelType property.
        /// </summary>
        private const string PROP_NAME_FuelType = "FuelType";

        /// <summary>
        /// Name of the MobileDevice property.
        /// </summary>
        private const string PROP_NAME_MobileDevice = "MobileDevice";

        /// <summary>
        /// Name of the Specialties property.
        /// </summary>
        private const string PROP_NAME_Specialties = "Specialties";

        /// <summary>
        /// Name of the SpecialtiesCollection property.
        /// </summary>
        private const string PROP_NAME_SpecialtiesCollection = "SpecialtiesCollection";

        /// <summary>
        /// Name of the Capacities property.
        /// </summary>
        private const string PROP_NAME_Capacities = "Capacities";

        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Capacities _capacities;
        private CapacitiesInfo _capacitiesInfo;

        private EntityRefWrapper<FuelType,
            DataModel.FuelTypes> _fuelTypesRef;

        private EntityRefWrapper<MobileDevice,
            DataModel.MobileDevices> _mobileDeviceRef;

        private EntityCollWrapper<VehicleSpecialty,
            DataModel.VehicleSpecialties> _specialtiesColl;

        #endregion private members
    }
}
