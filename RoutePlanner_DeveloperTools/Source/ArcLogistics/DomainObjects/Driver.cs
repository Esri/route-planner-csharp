using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Specialized;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Validation;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using System.Collections;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a driver.
    /// </summary>
    public class Driver : DataObject, IMarkableAsDeleted, ISupportOwnerCollection
    {
        #region public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // property names

        /// <summary>
        /// Name of Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_Name; }
        }

        /// <summary>
        /// Name of SpecialtiesCollection property.
        /// </summary>
        public static string PropertyNameSpecialtiesCollection
        {
            get { return PROP_NAME_SpecialtiesCollection; }
        }

        /// <summary>
        /// Name of Specialties property.
        /// </summary>
        public static string PropertyNameSpecialties
        {
            get { return PROP_NAME_Specialties; }
        }

        /// <summary>
        /// Name of MobileDevice property.
        /// </summary>
        public static string PropertyNameMobileDevice
        {
            get { return PROP_NAME_MobileDevice; }
        }

        /// <summary>
        /// Name of PerHourSalary property.
        /// </summary>
        public static string PropertyPerHourSalary
        {
            get { return PROP_NAME_PerHourSalary; }
        }

        /// <summary>
        /// Name of TimeBeforeOT property.
        /// </summary>
        public static string PropertyNameTimeBeforeOT
        {
            get { return PROP_NAME_TimeBeforeOT; }
        }

        /// <summary>
        /// Name of PerHourOTSalary property.
        /// </summary>
        public static string PropertyNamePerHourOTSalary
        {
            get { return PROP_NAME_PerHourOTSalary; }
        }

        /// <summary>
        /// Name of FixedCost property.
        /// </summary>
        public static string PropertyNameFixedCost
        {
            get { return PROP_NAME_FixedCost; }
        }

        /// <summary>
        /// Name of Comment property.
        /// </summary>
        public static string PropertyNameComment
        {
            get { return PROP_NAME_Comment; }
        }
        #endregion 

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Driver</c> class.
        /// </summary>
        public Driver()
            : base(DataModel.Drivers.CreateDrivers(Guid.NewGuid()))
        {
            _Entity.PerHourSalary = Defaults.Instance.DriversDefaults.PerHour;
            _Entity.PerHourOTSalary = Defaults.Instance.DriversDefaults.PerHourOT;
            _Entity.TimeBeforeOT = Defaults.Instance.DriversDefaults.TimeBeforeOT;

            _SpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(_Specialties_CollectionChanged);
            base.SetCreationTime();
        }

        internal Driver(DataModel.Drivers entity)
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
            get { return Properties.Resources.Driver; }
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
        /// Driver's name.
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
        /// Arbitrary text about the driver.
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
        /// Fixed amount of money(in currency units) that is paid to the driver for a day regardless of hours worked.
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
        /// Amount of money that is paid to the driver per hour of work.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Exclusive, SolverConst.MAX_SALARY,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidPerHourSalary",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNamePerHourSalary")]
        [UnitPropertyAttribute(Unit.Currency, Unit.Currency, Unit.Currency)]
        [AffectsRoutingProperty]
        public double PerHourSalary
        {
            get { return _Entity.PerHourSalary; }
            set 
            {
                _Entity.PerHourSalary = value;
                NotifyPropertyChanged(PROP_NAME_PerHourSalary);
            }
        }

        /// <summary>
        /// Amount of money that is paid to the driver per hour of overtime work.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_SALARY,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidPerHourOTSalary",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [PropertyComparisonValidator("PerHourSalary", ComparisonOperator.GreaterThanEqual,
            MessageTemplateResourceName = "Error_InvalidPerHourOTSalary2",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages))]
        [DomainProperty("DomainPropertyNamePerHourOTSalary")]
        [UnitPropertyAttribute(Unit.Currency, Unit.Currency, Unit.Currency)]
        [AffectsRoutingProperty]
        public double PerHourOTSalary
        {
            get { return _Entity.PerHourOTSalary; }
            set
            {
                _Entity.PerHourOTSalary = value;
                NotifyPropertyChanged(PROP_NAME_PerHourOTSalary);
            }
        }

        /// <summary>
        /// The number of working hours for each day after which overtime begins.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidTimeBeforeOT",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameTimeBeforeOT")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        [AffectsRoutingProperty]
        public double TimeBeforeOT
        {
            get { return _Entity.TimeBeforeOT; }
            set
            {
                _Entity.TimeBeforeOT = value;
                NotifyPropertyChanged(PROP_NAME_TimeBeforeOT);
            }
        }

        /// <summary>
        /// Mobile device used by the driver.
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
        /// Collection of driver's specialties.
        /// </summary>
        [DomainProperty("DomainPropertyNameSpecialties")]
        [AffectsRoutingProperty]
        public IDataObjectCollection<DriverSpecialty> Specialties
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

        #endregion public members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the Driver's name.
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
            Driver obj = new Driver();
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
            System.Diagnostics.Debug.Assert(obj is Driver);

            Driver driver = obj as Driver;
            driver.Name = this.Name;
            driver.Comment = this.Comment;
            driver.FixedCost = this.FixedCost;
            driver.PerHourSalary = this.PerHourSalary;
            driver.PerHourOTSalary = this.PerHourOTSalary;
            driver.TimeBeforeOT = this.TimeBeforeOT;

            foreach (DriverSpecialty spec in this.Specialties)
                driver.Specialties.Add(spec);

            driver.MobileDevice = this.MobileDevice;
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

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.Drivers _Entity
        {
            get
            {
                return (DataModel.Drivers)base.RawEntity;
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

        private EntityCollWrapper<DriverSpecialty,
            DataModel.DriverSpecialties> _SpecialtiesWrap
        {
            get
            {
                if (_specialtiesColl == null)
                {
                    _specialtiesColl = new EntityCollWrapper<DriverSpecialty,
                        DataModel.DriverSpecialties>(_Entity.DriverSpecialties, this, false);
                }

                return _specialtiesColl;
            }
        }

        #endregion private properties

        #region private constants

        /// <summary>
        /// Name of Name property.
        /// </summary>
        private const string PROP_NAME_Name = "Name";

        /// <summary>
        /// Name of Comment property.
        /// </summary>
        private const string PROP_NAME_Comment = "Comment";

        /// <summary>
        /// Name of FixedCost property.
        /// </summary>
        private const string PROP_NAME_FixedCost = "FixedCost";

        /// <summary>
        /// Name of PerHourSalary property.
        /// </summary>
        private const string PROP_NAME_PerHourSalary = "PerHourSalary";

        /// <summary>
        /// Name of PerHourOTSalary property.
        /// </summary>
        private const string PROP_NAME_PerHourOTSalary = "PerHourOTSalary";

        /// <summary>
        /// Name of TimeBeforeOT property.
        /// </summary>
        private const string PROP_NAME_TimeBeforeOT = "TimeBeforeOT";

        /// <summary>
        /// Name of MobileDevice property.
        /// </summary>
        private const string PROP_NAME_MobileDevice = "MobileDevice";

        /// <summary>
        /// Name of Specialties property.
        /// </summary>
        private const string PROP_NAME_Specialties = "Specialties";

        /// <summary>
        /// Name of SpecialtiesCollection property.
        /// </summary>
        private const string PROP_NAME_SpecialtiesCollection = "SpecialtiesCollection";

        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private EntityRefWrapper<MobileDevice,
            DataModel.MobileDevices> _mobileDeviceRef;

        private EntityCollWrapper<DriverSpecialty,
            DataModel.DriverSpecialties> _specialtiesColl;

        #endregion private members
    }
}
