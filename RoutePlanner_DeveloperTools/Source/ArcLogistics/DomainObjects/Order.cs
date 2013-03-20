using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;

using Microsoft.Practices.EnterpriseLibrary.Validation;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.DomainObjects.Validation;
using DataModel = ESRI.ArcLogistics.Data.DataModel;
using ESRI.ArcLogistics.DomainObjects.Attributes;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents an order.
    /// </summary>
    public class Order : DataObject, IGeocodable,
        ICapacitiesInit,
        IOrderPropertiesInit
    {
        #region public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_Name;}
        }

        /// <summary>
        /// Name of the PlannedDate property.
        /// </summary>
        public static string PropertyNamePlannedDate
        {
            get { return PROP_NAME_PlannedDate; }
        }

        /// <summary>
        /// Name of the Address property.
        /// </summary>
        public static string PropertyNameAddress
        {
            get { return PROP_NAME_Address;}
        }

        /// <summary>
        /// Name of the GeoLocation property.
        /// </summary>
        public static string PropertyNameGeoLocation
        {
            get { return PROP_NAME_GeoLocation;}
        }

        /// <summary>
        /// Name of the Type property.
        /// </summary>
        public static string PropertyNameType
        {
            get { return PROP_NAME_Type;}
        }

        /// <summary>
        /// Name of the Priority property.
        /// </summary>
        public static string PropertyNamePriority
        {
            get { return PROP_NAME_Priority;}
        }

        /// <summary>
        /// Name of the ServiceTime property.
        /// </summary>
        public static string PropertyNameServiceTime
        {
            get { return PROP_NAME_ServiceTime;}
        }

        /// <summary>
        /// Name of the TimeWindow property.
        /// </summary>
        public static string PropertyNameTimeWindow
        {
            get { return PROP_NAME_TimeWindow;}
        }

        /// <summary>
        /// Name of the TimeWindow2 property.
        /// </summary>
        public static string PropertyNameTimeWindow2
        {
            get { return PROP_NAME_TimeWindow2;}
        }

        /// <summary>
        /// Name of the Capacities property.
        /// </summary>
        public static string PropertyNameCapacities
        {
            get { return PROP_NAME_Capacities;}
        }

        /// <summary>
        /// Name of the CustomProperties property.
        /// </summary>
        public static string PropertyNameCustomProperties
        {
            get { return PROP_NAME_CustomProperties;}
        }

        /// <summary>
        /// Name of the VehicleSpecialties property.
        /// </summary>
        public static string PropertyNameVehicleSpecialties
        {
            get { return PROP_NAME_VehicleSpecialties;}
        }

        /// <summary>
        /// Name of the DriverSpecialties property.
        /// </summary>
        public static string PropertyNameDriverSpecialties
        {
            get { return PROP_NAME_DriverSpecialties;}
        }

        /// <summary>
        /// Name of the VehicleSpecialtiesCollection property.
        /// </summary>
        public static string PropertyNameVehicleSpecialtiesCollection
        {
            get { return PROP_NAME_VehicleSpecialtiesCollection;}
        }

        /// <summary>
        /// Name of the DriverSpecialtiesCollection property.
        /// </summary>
        public static string PropertyNameDriverSpecialtiesCollection
        {
            get { return PROP_NAME_DriverSpecialtiesCollection;}
        }

        /// <summary>
        /// Name of the MaxViolationTime property.
        /// </summary>
        public static string PropertyNameMaxViolationTime
        {
            get { return PROP_NAME_MaxViolationTime;}
        }

        /// <summary>
        /// Name of the X property.
        /// </summary>
        public static string PropertyNameX
        {
            get { return PROP_NAME_X ;}
        }

        /// <summary>
        /// Name of the Y property.
        /// </summary>
        public static string PropertyNameY
        {
            get { return PROP_NAME_Y;}
        }

        #endregion 

        #region public static members
        /// <summary>
        /// Gets order property info collection for the specified capacities info.
        /// </summary>
        /// <param name="capacities">The reference to the collection of capacity info
        /// objects to get property information for.</param>
        /// <returns>A reference to the collection of order property info objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="capacities"/> is a null
        /// reference.</exception>
        public static IEnumerable<OrderPropertyInfo> GetPropertiesInfo(
            IEnumerable<CapacityInfo> capacities)
        {
            if (capacities == null)
            {
                throw new ArgumentNullException("capacities");
            }

            return capacities
                .Select((info, index) => OrderPropertyInfo.Create(
                    Capacities.GetCapacityPropertyName(index),
                    info.Name));
        }

        /// <summary>
        /// Gets order property info collection for the specified custom properties info.
        /// </summary>
        /// <param name="orderCustomProperties">The reference to the collection of order custom
        /// property info objects to get property information for.</param>
        /// <returns>A reference to the collection of order property info objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="orderCustomProperties"/> is a
        /// null reference.</exception>
        public static IEnumerable<OrderPropertyInfo> GetPropertiesInfo(
            IEnumerable<OrderCustomProperty> orderCustomProperties)
        {
            if (orderCustomProperties == null)
            {
                throw new ArgumentNullException("orderCustomProperties");
            }

            return orderCustomProperties
                .Select((info, index) => OrderPropertyInfo.Create(
                    OrderCustomProperties.GetCustomPropertyName(index),
                    info.Name));
        }

        /// <summary>
        /// Gets order property info collection for the specified address fields.
        /// </summary>
        /// <param name="addressFields">The reference to the collection of address field objects
        /// to get property information for.</param>
        /// <returns>A reference to the collection of order property info objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="addressFields"/> is a null
        /// reference.</exception>
        public static IEnumerable<OrderPropertyInfo> GetPropertiesInfo(
            IEnumerable<AddressField> addressFields)
        {
            if (addressFields == null)
            {
                throw new ArgumentNullException("addressFields");
            }

            return addressFields
                .Select(field => OrderPropertyInfo.Create(field.Type.ToString(), field.Title));
        }

        /// <summary>
        /// Gets order property names.
        /// </summary>
        /// <param name="capacitiesInfo">Information about capacities.</param>
        /// <param name="orderCustomPropertiesInfo">Information about custom order properties.</param>
        /// <param name="addressFields">Set of geocoder address fields.</param>
        /// <returns>Returns full collection of order property names.</returns>
        public static string[] GetPropertyNames(CapacitiesInfo capacitiesInfo,
            OrderCustomPropertiesInfo orderCustomPropertiesInfo, AddressField[] addressFields)
        {
            Type type = typeof(Order);

            List<string> propertyNames = new List<string>();

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(DomainPropertyAttribute)))
                {
                    DomainPropertyAttribute attribute = (DomainPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(DomainPropertyAttribute));
                    Debug.Assert(null != attribute);
                    Type typeProperty = _GetEffectiveType(property.PropertyType);

                    if (typeof(OrderCustomProperties) == typeProperty)
                    {   // specials type: order custom property
                        OrderCustomPropertiesInfo info = orderCustomPropertiesInfo;
                        for (int i = 0; i < info.Count; ++i)
                            propertyNames.Add(OrderCustomProperties.GetCustomPropertyName(i));
                    }
                    else if (typeof(Capacities) == typeProperty)
                    {   // specials type: capacities
                        CapacitiesInfo info = capacitiesInfo;
                        for (int i = 0; i < info.Count; ++i)
                            propertyNames.Add(Capacities.GetCapacityPropertyName(i));
                    }
                    else if (typeof(Address) == typeProperty)
                    {   // specials type: address
                        ESRI.ArcLogistics.Geocoding.AddressField[] fields = addressFields;
                        for (int i = 0; i < fields.Length; ++i)
                            propertyNames.Add(fields[i].Type.ToString());
                    }
                    else if (typeof(Point) == typeProperty)
                    {
                        propertyNames.Add(PROP_NAME_X);
                        propertyNames.Add(PROP_NAME_Y);
                    }
                    else
                        propertyNames.Add(property.Name);
                }
            }

            return propertyNames.ToArray();
        }

        /// <summary>
        /// Get order property titles.
        /// </summary>
        /// <param name="capacitiesInfo">Information about capacities.</param>
        /// <param name="orderCustomPropertiesInfo">Information about custom order properties.</param>
        /// <param name="addressFields">Set of geocoder address fields.</param>
        /// <returns>Returns full collection of order property title to show in UI.</returns>
        public static string[] GetPropertyTitles(CapacitiesInfo capacitiesInfo,
                    OrderCustomPropertiesInfo orderCustomPropertiesInfo, AddressField[] addressFields)
        {
            Type type = typeof(Order);

            List<string> propertyTitles = new List<string>();

            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(DomainPropertyAttribute)))
                {
                    DomainPropertyAttribute attribute = (DomainPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(DomainPropertyAttribute));
                    Debug.Assert(null != attribute);
                    Type typeProperty = _GetEffectiveType(property.PropertyType);

                    if (typeof(OrderCustomProperties) == typeProperty)
                    {   // specials type: order custom property
                        OrderCustomPropertiesInfo info = orderCustomPropertiesInfo;
                        for (int i = 0; i < info.Count; ++i)
                            propertyTitles.Add(info[i].Name);
                    }
                    else if (typeof(Capacities) == typeProperty)
                    {   // specials type: capacities
                        CapacitiesInfo info = capacitiesInfo;
                        for (int i = 0; i < info.Count; ++i)
                            propertyTitles.Add(info[i].Name);
                    }
                    else if (typeof(Address) == typeProperty)
                    {   // specials type: address
                        ESRI.ArcLogistics.Geocoding.AddressField[] fields = addressFields;
                        for (int i = 0; i < fields.Length; ++i)
                            propertyTitles.Add(fields[i].Title);
                    }
                    else if (typeof(Point) == typeProperty)
                    {
                        propertyTitles.Add(Properties.Resources.DomainPropertyNameX);
                        propertyTitles.Add(Properties.Resources.DomainPropertyNameY);
                    }
                    else
                        propertyTitles.Add(attribute.Title);
                }
            }

            return propertyTitles.ToArray();
        }

        /// <summary>
        /// Gets order property value by property name.
        /// </summary>
        /// <param name="order">Order instance.</param>
        /// <param name="name">Property name.</param>
        /// <returns>Returns property value.</returns>
        public static object GetPropertyValue(Order order, string name)
        {
            object value = null;

            int orderPropertyIndex = OrderCustomProperties.GetCustomPropertyIndex(name);
            int capacityIndex = Capacities.GetCapacityPropertyIndex(name);

            AddressPart? addressPart = null;
            try
            {
                addressPart = (AddressPart)Enum.Parse(typeof(AddressPart), name);
            }
            catch { }

            if (orderPropertyIndex != -1)
            {   // specials type: order custom property
                if ((0 <= orderPropertyIndex) && (orderPropertyIndex < order.CustomProperties.Count))
                    value = order.CustomProperties[orderPropertyIndex];
                else
                {
                    string mes = string.Format(Properties.Resources.PropertyNameNotExists, name);
                    throw new ArgumentException(mes);
                }
            }
            else if (capacityIndex != -1)
            {   // specials type: capacities
                if ((0 <= capacityIndex) && (capacityIndex < order.Capacities.Count))
                    value = order.Capacities[capacityIndex];
                else
                {
                    string mes = string.Format(Properties.Resources.PropertyNameNotExists, name);
                    throw new ArgumentException(mes);
                }
            }
            else if (addressPart.HasValue)
            {   // specials type: address
                value = order.Address[addressPart.Value];
            }
            else if (name.Equals("X", StringComparison.OrdinalIgnoreCase) || name.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                if (order.GeoLocation.HasValue)
                {
                    if (name.Equals("X", StringComparison.OrdinalIgnoreCase))
                        value = order.GeoLocation.Value.X;
                    if (name.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        value = order.GeoLocation.Value.Y;
                }
            }
            else
            {
                PropertyInfo propInfo = _GetPropertyByName(name);
                value = propInfo.GetValue(order, null);
            }

            return value;
        }

        #endregion

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Order</c> class.
        /// </summary>
        /// <param name="capacitiesInfo">Information about capacities.</param>
        /// <param name="customPropertiesInfo">Information about custom properties.</param>
        public Order(CapacitiesInfo capacitiesInfo, OrderCustomPropertiesInfo customPropertiesInfo)
            : base(DataModel.Orders.CreateOrders(Guid.NewGuid()))
        {
            // Enable address validation.
            IsAddressValidationEnabled = true;

            _Entity.OrderType = (int)Defaults.Instance.OrdersDefaults.OrderType;
            _Entity.OrderPriority = (int)Defaults.Instance.OrdersDefaults.Priority;
            _Entity.ServiceTime = Defaults.Instance.OrdersDefaults.ServiceTime;
            _Entity.CurbApproach = (int)Defaults.Instance.OrdersDefaults.CurbApproach;
            _Entity.MaxViolationTime = Defaults.Instance.OrdersDefaults.MaxViolationTime;

            _timeWindow1.IsWideOpen = Defaults.Instance.OrdersDefaults.TimeWindow.IsWideopen;
            if (!_timeWindow1.IsWideOpen)
            {
                _timeWindow1.From = Defaults.Instance.OrdersDefaults.TimeWindow.From;
                _timeWindow1.To = Defaults.Instance.OrdersDefaults.TimeWindow.To;
                _timeWindow1.Day = 0;
            }

            _timeWindow2.IsWideOpen = Defaults.Instance.OrdersDefaults.TimeWindow2.IsWideopen;
            if (!_timeWindow2.IsWideOpen)
            {
                _timeWindow2.From = Defaults.Instance.OrdersDefaults.TimeWindow2.From;
                _timeWindow2.To = Defaults.Instance.OrdersDefaults.TimeWindow2.To;
                _timeWindow2.Day = 0;
            }

            _SubscribeToAddressEvent();
            _CreateCapacities(capacitiesInfo);
            _CreateCustomProperties(customPropertiesInfo);

            _timeWindow1.PropertyChanged += new PropertyChangedEventHandler(TimeWindow_PropertyChanged1);
            _timeWindow2.PropertyChanged += new PropertyChangedEventHandler(TimeWindow_PropertyChanged2);
            _VehicleSpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(VehicleSpecialties_CollectionChanged);
            _DriverSpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(DriverSpecialties_CollectionChanged);

            base.SetCreationTime();
        }

        internal Order(DataModel.Orders entity)
            : base(entity)
        {
            Debug.Assert(0 < entity.CreationTime); // NOTE: must be inited

            // Enable address validation.
            IsAddressValidationEnabled = true;

            _InitTimeWindow(_Entity.TW1From, _Entity.TW1To, ref _timeWindow1);
            _InitTimeWindow(_Entity.TW2From, _Entity.TW2To, ref _timeWindow2);

            _timeWindow1.PropertyChanged += new PropertyChangedEventHandler(TimeWindow_PropertyChanged1);
            _timeWindow2.PropertyChanged += new PropertyChangedEventHandler(TimeWindow_PropertyChanged2);
            _VehicleSpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(VehicleSpecialties_CollectionChanged);
            _DriverSpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(DriverSpecialties_CollectionChanged);

            _InitAddress(entity);
            _InitGeoLocation(entity);
        }

        #endregion // Constructors

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.Order; }
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
        /// Order name.
        /// </summary>
        [DomainProperty("DomainPropertyNameName", true)]
        [NameNotNullValidator]
        public override string Name
        {
            get { return _Entity.Name; }
            set
            {
                _Entity.Name = value;
                NotifyPropertyChanged(PROP_NAME_Name);
            }
        }

        /// <summary>
        /// Planned date when this order should be serviced.
        /// </summary>
        [DataTimeNullableValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNamePlannedDate")]
        public DateTime? PlannedDate
        {
            get
            {
                // We need to discard time component of order planned date if it was set
                // to the value other than "12:00 AM".
                DateTime? plannedDate = _Entity.PlannedDate.HasValue ?
                                        (DateTime?)_Entity.PlannedDate.Value.Date :
                                        null;

                return plannedDate;
            }

            set
            {
                _Entity.PlannedDate = value;
                NotifyPropertyChanged(PROP_NAME_PlannedDate);
            }
        }

        /// <summary>
        /// Order service type.
        /// </summary>
        [DomainProperty("DomainPropertyNameOrderType")]
        [AffectsRoutingProperty]
        public OrderType Type
        {
            get { return (OrderType)_Entity.OrderType; }
            set
            {
                _Entity.OrderType = (int)value;
                NotifyPropertyChanged(PROP_NAME_Type);
            }
        }

        /// <summary>
        /// Order priority.
        /// </summary>
        [DomainProperty("DomainPropertyNameOrderPriority")]
        [AffectsRoutingProperty]
        public OrderPriority Priority
        {
            get { return (OrderPriority)_Entity.OrderPriority; }
            set
            {
                _Entity.OrderPriority = (int)value;
                NotifyPropertyChanged(PROP_NAME_Priority);
            }
        }

        /// <summary>
        /// Service time in minutes necessary to complete this order.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidServiceTime",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameServiceTime")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        [AffectsRoutingProperty]
        public double ServiceTime
        {
            get { return _Entity.ServiceTime; }
            set
            {
                _Entity.ServiceTime = (float)value;
                NotifyPropertyChanged(PROP_NAME_ServiceTime);
            }
        }

        /// <summary>
        /// Curb approach for the order's location.
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
        /// First time window when this order can be serviced. 
        /// </summary>
        /// <remarks>
        /// If this time window is not wideopen but the second time window is then the second time window isn't taken into account.
        /// </remarks>
        [DomainProperty("DomainPropertyNameTimeWindow")]
        [AffectsRoutingProperty]
        public TimeWindow TimeWindow
        {
            get { return _timeWindow1; }
            set
            {
                _timeWindow1.PropertyChanged -= TimeWindow_PropertyChanged1;

                if (null != value)
                {
                    _timeWindow1 = value;
                    _UpdateTimeWindowEntityData(_timeWindow1, true);

                    _timeWindow1.PropertyChanged += new PropertyChangedEventHandler(TimeWindow_PropertyChanged1);
                }
                else
                {
                    _ClearTimeWindow(ref _timeWindow1);
                    _UpdateTimeWindowEntityData(_timeWindow1, true);
                }

                NotifyPropertyChanged(PROP_NAME_TimeWindow);
            }
        }

        /// <summary>
        /// Second time window when this order can be serviced. 
        /// </summary>
        /// <remarks>
        /// If this time window is not wideopen but the first time window is then the first time window isn't taken into account.
        /// </remarks>
        [TimeWindow2Validator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameTimeWindow2")]
        [AffectsRoutingProperty]
        public TimeWindow TimeWindow2
        {
            get { return _timeWindow2; }
            set
            {
                _timeWindow2.PropertyChanged -= TimeWindow_PropertyChanged2;

                if (null != value)
                {
                    _timeWindow2 = value;
                    _UpdateTimeWindowEntityData(_timeWindow2, false);

                    _timeWindow2.PropertyChanged += new PropertyChangedEventHandler(TimeWindow_PropertyChanged2);
                }
                else
                {
                    _ClearTimeWindow(ref _timeWindow2);
                    _UpdateTimeWindowEntityData(_timeWindow2, false);
                }

                NotifyPropertyChanged(PROP_NAME_TimeWindow2);
            }
        }

        /// <summary>
        /// Order capacities.
        /// </summary>
        [CapacityValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty]
        [AffectsRoutingProperty]
        public Capacities Capacities
        {
            get
            {
                if (null == _capacities)
                    throw new NotSupportedException(Properties.Resources.CapacityInfoIsNull);

                return _capacities;
            }
            set
            {
                _capacities.PropertyChanged -= Capacities_PropertyChanged;

                if (null != value)
                {
                    _capacities = value;
                    _UpdateCapacitiesEntityData();
                }
                else
                {
                    _ClearCapacities();
                    _UpdateCapacitiesEntityData();
                }

                _capacities.PropertyChanged += new PropertyChangedEventHandler(Capacities_PropertyChanged);

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
                if (null != _capacitiesInfo)
                    throw new NotSupportedException(Properties.Resources.CapacitiesInfoAlreadySet);

                _capacitiesInfo = value;
                _InitCapacities(_Entity, _capacitiesInfo);
            }
        }

        /// <summary>
        /// Order custom properties.
        /// </summary>
        [DomainProperty]
        [OrderCustomPropertyValidator]
        public OrderCustomProperties CustomProperties
        {
            get
            {
                if (null == _customProperties)
                    throw new NotSupportedException(Properties.Resources.OrderCustomPropertiesInfoIsNull);

                return _customProperties;
            }

            set
            {
                _customProperties.PropertyChanged -= CustomProperties_PropertyChanged;

                if (null != value)
                {
                    _customProperties = value;
                    _UpdateCustomPropertiesEntityData();
                }
                else
                {
                    _ClearCustomProperties();
                    _UpdateCustomPropertiesEntityData();
                }

                _customProperties.PropertyChanged += new PropertyChangedEventHandler(CustomProperties_PropertyChanged);

                NotifyPropertyChanged(PROP_NAME_CustomProperties);
            }
        }

        /// <summary>
        /// Information about custom properties. The same as project exposes.
        /// </summary>
        public OrderCustomPropertiesInfo CustomPropertiesInfo
        {
            get { return _customPropertiesInfo; }
            set
            {
                if (_customPropertiesInfo != null)
                    throw new NotSupportedException(Properties.Resources.OrderCustomPropertiesInfoAlreadySet);

                _customPropertiesInfo = value;
                _InitCustomProperties(_Entity, _customPropertiesInfo);
            }
        }
        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public override string this[string columnName]
        {
            get
            {
                int index = Capacities.GetCapacityPropertyIndex(columnName);
                if (-1 != index)
                    return _ValidateCapacity(index);
                {
                    index = OrderCustomProperties.GetCustomPropertyIndex(columnName);
                    if (-1 != index)
                        return _ValidateCustomProperty(index);
                    else if (Address.IsAddressPropertyName(columnName))
                        return _ValidateAddress();
                    else
                        return base[columnName];
                }
            }
        }

        /// <summary>
        /// Collection of vehicle specialties necessary to service this order.
        /// </summary>
        [DomainProperty("DomainPropertyNameVehicleSpecialties")]
        [AffectsRoutingProperty]
        public IDataObjectCollection<VehicleSpecialty> VehicleSpecialties
        {
            get { return _VehicleSpecialtiesWrap.DataObjects; }
            set
            {
                _VehicleSpecialtiesWrap.DataObjects.CollectionChanged -= VehicleSpecialties_CollectionChanged;
                _VehicleSpecialtiesWrap.DataObjects = value;
                _VehicleSpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(VehicleSpecialties_CollectionChanged);
                NotifyPropertyChanged(PROP_NAME_VehicleSpecialties);
            }
        }

        /// <summary>
        ///  Collection of driver specialties necessary to service this order.
        /// </summary>
        [DomainProperty("DomainPropertyNameDriverSpecialties")]
        [AffectsRoutingProperty]
        public IDataObjectCollection<DriverSpecialty> DriverSpecialties
        {
            get { return _DriverSpecialtiesWrap.DataObjects; }
            set
            {
                _DriverSpecialtiesWrap.DataObjects.CollectionChanged -= DriverSpecialties_CollectionChanged;
                _DriverSpecialtiesWrap.DataObjects = value;
                _DriverSpecialtiesWrap.DataObjects.CollectionChanged += new NotifyCollectionChangedEventHandler(DriverSpecialties_CollectionChanged);
                NotifyPropertyChanged(PROP_NAME_DriverSpecialties);
            }
        }

        /// <summary>
        /// Order time windows can be violated by the value(in minutes) specified by this property.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidMaxViolationTime",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameMaxViolationTime")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        [AffectsRoutingProperty]
        public double MaxViolationTime
        {
            get
            {
                double result = 0;

                if (_Entity.MaxViolationTime.HasValue)
                {
                    result = _Entity.MaxViolationTime.Value;
                }

                return result;
            }
            set
            {
                _Entity.MaxViolationTime = value;
                NotifyPropertyChanged(PROP_NAME_MaxViolationTime);
            }
        }

        /// <summary>
        /// Collection of stops associated with this order.
        /// </summary>
        public IDataObjectCollection<Stop> Stops
        {
            get { return _StopsWrap.DataObjects; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the order.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Returns the custom property that is used as an order pair key.
        /// </summary>
        /// <returns></returns>
        public string PairKey()
        {
            // Look for custom order property used as the key for
            // pairing orders.
    
            for (int i = 0; i < _customPropertiesInfo.Count; i++)
            {
                OrderCustomProperty orderPropertyInfoItem = _customPropertiesInfo[i];
                if (orderPropertyInfoItem.OrderPairKey)
                    return _customProperties[i] as string;
            }

            return null;
        }

        #endregion // Public members

        #region Public static methods

        /// <summary>
        /// Gets order property info by property name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>Returns property info.</returns>
        /// <exception cref="System.ArgumentException">
        /// Property is absent.</exception>
        public static PropertyInfo GetPropertyByName(string name)
        {
            return _GetPropertyByName(name);
        }

        #endregion

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Order obj = new Order(this._capacitiesInfo, this._customPropertiesInfo);
            this.CopyTo(obj);

            return obj;
        }
        #endregion // ICloneable members

        #region ICopyable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        public override void CopyTo(DataObject obj)
        {
            Debug.Assert(obj is Order);

            Order order = obj as Order;

            order.Name = this.Name;
            order.PlannedDate = this.PlannedDate;

            if (null != this.Address)
                order.Address = (Address)this.Address.Clone();

            if (GeoLocation.HasValue)
                order.GeoLocation = new Point(this.GeoLocation.Value.X, this.GeoLocation.Value.Y);
            else
                order.GeoLocation = null;

            order.Type = this.Type;
            order.Priority = this.Priority;
            order.ServiceTime = this.ServiceTime;
            order.CurbApproach = this.CurbApproach;

            if (null != this.TimeWindow)
                order.TimeWindow = (TimeWindow)this.TimeWindow.Clone();

            if (null != this.TimeWindow2)
                order.TimeWindow2 = (TimeWindow)this.TimeWindow2.Clone();

            order.Capacities = (Capacities)this._capacities.Clone();
            order.CustomProperties = (OrderCustomProperties)this._customProperties.Clone();

            order.VehicleSpecialties.Clear();
            foreach (VehicleSpecialty spec in this.VehicleSpecialties)
                order.VehicleSpecialties.Add(spec);

            order.DriverSpecialties.Clear();
            foreach (DriverSpecialty spec in this.DriverSpecialties)
                order.DriverSpecialties.Add(spec);
        }
        #endregion ICopyable members

        #region IGeocodable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The address associated with the order.
        /// </summary>
        [ObjectFarFromRoadValidator(MessageTemplateResourceName = "Error_OrderNotFoundOnNetworkViolationMessage",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages))]
        [DomainProperty]
        public Address Address
        {
            get { return _address; }
            set
            {
                _address.PropertyChanged -= Address_PropertyChanged;

                if (null != value)
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
        /// The geolocation of the order address.
        /// </summary>
        [DomainProperty]
        public Point? GeoLocation
        {
            get
            {
                return _geoLocation;
            }
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
        [GeocodableValidator(MessageTemplateResourceName = "Error_OrderNotGeocoded",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages))]
        public bool IsGeocoded
        {
            get { return _geoLocation.HasValue; }
        }

        /// <summary>
        /// Property which tirn on/off address validation.
        /// </summary>
        public bool IsAddressValidationEnabled
        {
            get;
            set;
        }

        #endregion // IGeocodable members

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

        #region IOrderPropertiesInit members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets OrderCustomPropertiesInfo.
        /// </summary>
        OrderCustomPropertiesInfo IOrderPropertiesInit.OrderCustomPropertiesInfo
        {
            set
            {
                if (this.CustomPropertiesInfo == null)
                    this.CustomPropertiesInfo = value;
            }
        }

        #endregion // IOrderPropertiesInit members

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private EntityCollWrapper<VehicleSpecialty, DataModel.VehicleSpecialties> _VehicleSpecialtiesWrap
        {
            get
            {
                if (null == _collVehicleSpecialties)
                {
                    _collVehicleSpecialties = new EntityCollWrapper<VehicleSpecialty,
                        DataModel.VehicleSpecialties>(_Entity.VehicleSpecialties, this, false);
                }

                return _collVehicleSpecialties;
            }
        }

        private EntityCollWrapper<DriverSpecialty, DataModel.DriverSpecialties> _DriverSpecialtiesWrap
        {
            get
            {
                if (null == _collDriverSpecialties)
                {
                    _collDriverSpecialties = new EntityCollWrapper<DriverSpecialty,
                        DataModel.DriverSpecialties>(_Entity.DriverSpecialties, this, false);
                }

                return _collDriverSpecialties;
            }
        }

        private EntityCollWrapper<Stop, DataModel.Stops> _StopsWrap
        {
            get
            {
                if (_collStops == null)
                {
                    _collStops = new EntityCollWrapper<Stop, DataModel.Stops>(
                        _Entity.Stops, this, true);
                }

                return _collStops;
            }
        }

        private DataModel.Orders _Entity
        {
            get { return (base.RawEntity as DataModel.Orders); }
        }
        #endregion // Private properties

        #region Private static methods

        private static PropertyInfo _GetPropertyByName(string name)
        {
            Type type = typeof(Order);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (Attribute.IsDefined(property, typeof(DomainPropertyAttribute)))
                {
                    DomainPropertyAttribute attribute = (DomainPropertyAttribute)Attribute.GetCustomAttribute(property, typeof(DomainPropertyAttribute));
                    Debug.Assert(null != attribute);

                    if (name.Equals(property.Name))
                        return property;
                }
            }

            string mes = string.Format(Properties.Resources.PropertyNameNotExists, name);
            throw new ArgumentException(mes);
        }

        private static Type _GetEffectiveType(Type type)
        {
            // NOTE: type can be nullabled
            Type effectiveType = type;

            Type typeReal = Nullable.GetUnderlyingType(type);
            if (null != typeReal)
                effectiveType = typeReal;

            return effectiveType;
        }

        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void VehicleSpecialties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_VehicleSpecialties, PROP_NAME_VehicleSpecialtiesCollection);
        }

        private void DriverSpecialties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_DriverSpecialties, PROP_NAME_DriverSpecialtiesCollection);
        }
        #endregion // Private methods

        #region Private methods - CustomProperty
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void _CreateCustomProperties(OrderCustomPropertiesInfo info)
        {
            _customPropertiesInfo = info;
            _customProperties = new OrderCustomProperties(info);
            _customProperties.PropertyChanged += new PropertyChangedEventHandler(CustomProperties_PropertyChanged);
        }

        private void CustomProperties_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateCustomPropertiesEntityData();

            NotifyPropertyChanged(e.PropertyName);

            NotifyPropertyChanged(PROP_NAME_CustomProperties);
        }

        private void _InitCustomProperties(DataModel.Orders entity, OrderCustomPropertiesInfo info)
        {
            _customProperties = OrderCustomProperties.CreateFromDBString(entity.CustomProperties, info);
            _customProperties.PropertyChanged += new PropertyChangedEventHandler(CustomProperties_PropertyChanged);
        }

        private void _ClearCustomProperties()
        {
            _customProperties = new OrderCustomProperties(_customPropertiesInfo);
        }

        private void _UpdateCustomPropertiesEntityData()
        {
            _Entity.CustomProperties = OrderCustomProperties.AssemblyDBString(_customProperties, _customPropertiesInfo);
        }

        private string _ValidateCustomProperty(int index)
        {
            OrderCustomPropertyValidator validator = new OrderCustomPropertyValidator(index);
            ValidationResults results = validator.Validate(_customProperties);

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
        #endregion // Private methods - CustomProperty

        #region Private methods - capacities
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void _CreateCapacities(CapacitiesInfo capacitiesInfo)
        {
            _capacitiesInfo = capacitiesInfo;
            _capacities = new Capacities(capacitiesInfo);
            _capacities.PropertyChanged += new PropertyChangedEventHandler(Capacities_PropertyChanged);
        }

        private void Capacities_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateCapacitiesEntityData();

            NotifyPropertyChanged(e.PropertyName);

            NotifyPropertyChanged(PROP_NAME_Capacities);
        }

        private void _InitCapacities(DataModel.Orders entity, CapacitiesInfo capacitiesInfo)
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
        #endregion // Private methods - capacities

        #region Private methods - TimeWindow
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void TimeWindow_PropertyChanged1(object sender, PropertyChangedEventArgs e)
        {
            _UpdateTimeWindowEntityData(_timeWindow1, true);
            NotifySubPropertyChanged(PROP_NAME_TimeWindow, e.PropertyName);
        }

        private void TimeWindow_PropertyChanged2(object sender, PropertyChangedEventArgs e)
        {
            _UpdateTimeWindowEntityData(_timeWindow2, false);
            NotifySubPropertyChanged(PROP_NAME_TimeWindow2, e.PropertyName);
        }

        /// <summary>
        /// Initializes time window using effective to and effective from ticks.
        /// </summary>
        /// <param name="ticksFrom">Low boundary of time window in ticks.</param>
        /// <param name="ticksTo">High boundary of time window in ticks.</param>
        /// <param name="timeWindow">Time window to initialize.</param>
        private void _InitTimeWindow(long? ticksFrom, long? ticksTo, ref TimeWindow timeWindow)
        {
            if ((null != ticksFrom) && (null != ticksTo))
                _SetTimeWindow((long)ticksFrom, (long)ticksTo, ref timeWindow);
            else
                _ClearTimeWindow(ref timeWindow);
        }

        /// <summary>
        /// Creates time window using effective to and effective from ticks and stores result to
        /// the output parameter.
        /// </summary>
        /// <param name="ticksFrom">Low boundary of time window in ticks.</param>
        /// <param name="ticksTo">High boundary of time window in ticks.</param>
        /// <param name="timeWindow">Output parameter to store time window.</param>
        private void _SetTimeWindow(long ticksFrom, long ticksTo, ref TimeWindow timeWindow)
        {
            // Create time window using effective times.
            TimeWindow newTimeWindow =
                TimeWindow.CreateFromEffectiveTimes(new TimeSpan(ticksFrom), new TimeSpan(ticksTo));

            timeWindow.From = newTimeWindow.From;
            timeWindow.To = newTimeWindow.To;
            timeWindow.Day = newTimeWindow.Day;
            timeWindow.IsWideOpen = false;
        }

        /// <summary>
        /// Clears given time window.
        /// </summary>
        /// <param name="timeWindow">Time window to clear.</param>
        private void _ClearTimeWindow(ref TimeWindow timeWindow)
        {
            timeWindow.From = new TimeSpan();
            timeWindow.To = new TimeSpan();
            timeWindow.Day = 0;
            timeWindow.IsWideOpen = true;
        }

        /// <summary>
        /// Updates time window data in database.
        /// </summary>
        /// <param name="timeWindow">Time window data.</param>
        /// <param name="isFirst">Defines which time window should be updated: 
        /// true - 1-st time window, otherwise 2-nd time window.</param>
        private void _UpdateTimeWindowEntityData(TimeWindow timeWindow, bool isFirst)
        {
            if (isFirst)
            {
                if (!timeWindow.IsWideOpen)
                {
                    _Entity.TW1From = timeWindow.EffectiveFrom.Ticks;
                    _Entity.TW1To = timeWindow.EffectiveTo.Ticks;
                }
                else
                {
                    _Entity.TW1From = null;
                    _Entity.TW1To = null;
                }
            }
            else
            {
                if (!timeWindow.IsWideOpen)
                {
                    _Entity.TW2From = timeWindow.EffectiveFrom.Ticks;
                    _Entity.TW2To = timeWindow.EffectiveTo.Ticks;
                }
                else
                {
                    _Entity.TW2From = null;
                    _Entity.TW2To = null;
                }
            }
        }
        #endregion // Private methods - TimeWindow

        #region Private methods - address
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void Address_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateAddressEntityData();

            NotifyPropertyChanged(e.PropertyName);

            NotifyPropertyChanged(PROP_NAME_Address);
        }

        private void _InitAddress(DataModel.Orders entity)
        {
            Address.FullAddress = _Entity.FullAddress;
            Address.Unit = _Entity.Unit;
            Address.AddressLine = _Entity.AddressLine;
            Address.Locality1 = _Entity.Locality1;
            Address.Locality2 = _Entity.Locality2;
            Address.Locality3 = _Entity.Locality3;
            Address.CountyPrefecture = _Entity.CountyPrefecture;
            Address.PostalCode1 = _Entity.PostalCode1;
            Address.PostalCode2 = _Entity.PostalCode2;
            Address.StateProvince = _Entity.StateProvince;
            Address.Country = _Entity.Country;
            Address.MatchMethod = _Entity.Locator; // ToDo rename MatchMethod
            _SubscribeToAddressEvent();
        }

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

        private void _InitGeoLocation(DataModel.Orders entity)
        {
            if (null != entity.X && null != entity.Y)
                _geoLocation = new Point(entity.X.Value, entity.Y.Value);
        }

        private void _SubscribeToAddressEvent()
        {
            _address.PropertyChanged += new PropertyChangedEventHandler(Address_PropertyChanged);
        }

        private string _ValidateAddress()
        {
            // If we turned off validation - do nothing.
            if (!IsAddressValidationEnabled)
                return null;

            GeocodableValidator validator = new GeocodableValidator(Properties.Messages.Error_OrderNotGeocoded);
            ValidationResults results = validator.Validate(IsGeocoded);

            // If validation result is valid - check match method. If it is "Edited X/Y far from road" - address is not valid.
            if (results.IsValid)
            {
                ObjectFarFromRoadValidator objectFarFromRoadValidator =
                    new ObjectFarFromRoadValidator(Properties.Messages.Error_OrderNotFoundOnNetworkViolationMessage);
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
        #endregion // Private methods - address

        #region private constants

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_Name = "Name";

        /// <summary>
        /// Name of the PlannedDate property.
        /// </summary>
        private const string PROP_NAME_PlannedDate = "PlannedDate";

        /// <summary>
        /// Name of the CurbApproach property.
        /// </summary>
        public const string PROP_NAME_CurbApproach = "CurbApproach";

        /// <summary>
        /// Name of the Address property.
        /// </summary>
        private const string PROP_NAME_Address = "Address";

        /// <summary>
        /// Name of the GeoLocation property.
        /// </summary>
        private const string PROP_NAME_GeoLocation = "GeoLocation";

        /// <summary>
        /// Name of the Type property.
        /// </summary>
        private const string PROP_NAME_Type = "Type";

        /// <summary>
        /// Name of the Priority property.
        /// </summary>
        private const string PROP_NAME_Priority = "Priority";

        /// <summary>
        /// Name of the ServiceTime property.
        /// </summary>
        private const string PROP_NAME_ServiceTime = "ServiceTime";

        /// <summary>
        /// Name of the TimeWindow property.
        /// </summary>
        private const string PROP_NAME_TimeWindow = "TimeWindow";

        /// <summary>
        /// Name of the TimeWindow2 property.
        /// </summary>
        private const string PROP_NAME_TimeWindow2 = "TimeWindow2";

        /// <summary>
        /// Name of the Capacities property.
        /// </summary>
        private const string PROP_NAME_Capacities = "Capacities";

        /// <summary>
        /// Name of the CustomProperties property.
        /// </summary>
        private const string PROP_NAME_CustomProperties = "CustomProperties";

        /// <summary>
        /// Name of the VehicleSpecialties property.
        /// </summary>
        private const string PROP_NAME_VehicleSpecialties = "VehicleSpecialties";

        /// <summary>
        /// Name of the DriverSpecialties property.
        /// </summary>
        private const string PROP_NAME_DriverSpecialties = "DriverSpecialties";

        /// <summary>
        /// Name of the VehicleSpecialtiesCollection property.
        /// </summary>
        private const string PROP_NAME_VehicleSpecialtiesCollection = "VehicleSpecialtiesCollection";

        /// <summary>
        /// Name of the DriverSpecialtiesCollection property.
        /// </summary>
        private const string PROP_NAME_DriverSpecialtiesCollection = "DriverSpecialtiesCollection";

        /// <summary>
        /// Name of the MaxViolationTime property.
        /// </summary>
        private const string PROP_NAME_MaxViolationTime = "MaxViolationTime";

        /// <summary>
        /// Name of the X property.
        /// </summary>
        private const string PROP_NAME_X = "X";

        /// <summary>
        /// Name of the Y property.
        /// </summary>
        private const string PROP_NAME_Y = "Y";

        #endregion

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        private Capacities _capacities = null;
        private CapacitiesInfo _capacitiesInfo = null;

        private OrderCustomProperties _customProperties = null;
        private OrderCustomPropertiesInfo _customPropertiesInfo = null;

        private TimeWindow _timeWindow1 = new TimeWindow();
        private TimeWindow _timeWindow2 = new TimeWindow();

        private Address _address = new Address();
        private Point? _geoLocation = null;

        private EntityCollWrapper<VehicleSpecialty, DataModel.VehicleSpecialties> _collVehicleSpecialties = null;
        private EntityCollWrapper<DriverSpecialty, DataModel.DriverSpecialties> _collDriverSpecialties = null;
        private EntityCollWrapper<Stop, DataModel.Stops> _collStops = null;
        #endregion // Private members
    }
}
