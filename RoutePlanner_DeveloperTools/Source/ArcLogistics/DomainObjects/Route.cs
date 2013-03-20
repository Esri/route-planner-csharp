using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.DomainObjects.Utility;
using ESRI.ArcLogistics.DomainObjects.Validation;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Utility.ComponentModel;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using DataModel = ESRI.ArcLogistics.Data.DataModel;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a route.
    /// </summary>
    public class Route : DataObject, ICapacitiesInit, ISupportOwnerCollection
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Route</c> class.
        /// </summary>
        /// <param name="capacitiesInfo">Information about capacities.</param>
        public Route(CapacitiesInfo capacitiesInfo)
            : base(DataModel.Routes.CreateRoutes(Guid.NewGuid()))
        {
            Color = Color.Empty;

            Defaults defaults = Defaults.Instance;

            _timeWindow.IsWideOpen = defaults.RoutesDefaults.StartTimeWindow.IsWideopen;
            if (!_timeWindow.IsWideOpen)
            {
                _timeWindow.From = defaults.RoutesDefaults.StartTimeWindow.From;
                _timeWindow.To = defaults.RoutesDefaults.StartTimeWindow.To;
                _timeWindow.Day = 0;
            }
            _UpdateTimeWindowEntityData();

            _Entity.TimeAtStart = defaults.RoutesDefaults.TimeAtStart;
            _Entity.TimeAtEnd = defaults.RoutesDefaults.TimeAtEnd;
            _Entity.TimeAtRenewal = defaults.RoutesDefaults.TimeAtRenewal;
            _Entity.MaxOrders = defaults.RoutesDefaults.MaxOrder;
            _Entity.MaxTravelDistance = defaults.RoutesDefaults.MaxTravelDistance;
            _Entity.MaxTravelDuration = defaults.RoutesDefaults.MaxTravelDuration;
            _Entity.MaxTotalDuration = defaults.RoutesDefaults.MaxTotalDuration;

            _UpdateBreaksEntityData();
            _UpdateDaysEntityData();

            _CreateCapacities(capacitiesInfo);
            _InitPropertiesEvents();

            base.SetCreationTime();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Route</c> class.
        /// </summary>
        /// <param name="entity">Entity initialize datas.</param>
        internal Route(DataModel.Routes entity)
            : base(entity)
        {
            Debug.Assert(0 < entity.CreationTime); // NOTE: must be inited

            _InitTimeWindow(entity);
            _breaks = Breaks.CreateFromDBString(entity.Breaks);
            _days = Days.CreateFromDBString(entity.Days);

            // If collection is in schedule - remember it schedule.routes and
            // subscribe to collection changed event.
            if (this.Schedule != null)
            {
                this.Schedule.RoutesCollectionInitialized += _ScheduleRoutesCollectionInitialized;
            }

            _InitPropertiesEvents();
        }

        #endregion // Constructors

        #region Public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets name of the Name property.
        /// </summary>
        public static string PropertyNameName
        {
            get { return PROP_NAME_NAME; }
        }

        /// <summary>
        /// Gets the name of the Driver property.
        /// </summary>
        public static string PropertyNameDriver
        {
            get { return PROP_NAME_DRIVER; }
        }

        /// <summary>
        /// Gets name of the Vehicle property.
        /// </summary>
        public static string PropertyNameVehicle
        {
            get { return PROP_NAME_VEHICLE; }
        }

        /// <summary>
        /// Gets name of the Color property.
        /// </summary>
        public static string PropertyNameColor
        {
            get { return PROP_NAME_COLOR; }
        }

        /// <summary>
        /// Gets name of the IsVisible property.
        /// </summary>
        public static string PropertyNameIsVisible
        {
            get { return PROP_NAME_ISVISIBLE; }
        }

        /// <summary>
        /// Gets name of the IsLocked property.
        /// </summary>
        public static string PropertyNameIsLocked
        {
            get { return PROP_NAME_ISLOCKED; }
        }

        /// <summary>
        /// Gets name of the ZonesCollection property.
        /// </summary>
        public static string PropertyNameZonesCollection
        {
            get { return PROP_NAME_ZONESCOLLECTION; }
        }

        /// <summary>
        /// Gets name of the Cost property.
        /// </summary>
        public static string PropertyNameCost
        {
            get { return PROP_NAME_COST; }
        }

        /// <summary>
        /// Gets name of the Overtime property.
        /// </summary>
        public static string PropertyNameOvertime
        {
            get { return PROP_NAME_OVERTIME; }
        }

        /// <summary>
        /// Gets name of the TotalTime property.
        /// </summary>
        public static string PropertyNameTotalTime
        {
            get { return PROP_NAME_TOTAL_TIME; }
        }

        /// <summary>
        /// Gets name of the TotalDistance property.
        /// </summary>
        public static string PropertyNameTotalDistance
        {
            get { return PROP_NAME_TOTAL_DISTANCE; }
        }

        /// <summary>
        /// Gets name of the ViolationTime property.
        /// </summary>
        public static string PropertyNameViolationTime
        {
            get { return PROP_NAME_VIOLATION_TIME; }
        }

        /// <summary>
        /// Gets name of the WaitTime property.
        /// </summary>
        public static string PropertyNameWaitTime
        {
            get { return PROP_NAME_WAIT_TIME; }
        }

        /// <summary>
        /// Gets name of the Breaks property.
        /// </summary>
        public static string PropertyNameBreaks
        {
            get { return PROP_NAME_BREAKS; }
        }

        #endregion // Public static properties

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.Route; }
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
        /// Route name.
        /// </summary>
        [DomainProperty("DomainPropertyNameName", true)]
        [DuplicateNameValidator]
        [NameNotNullValidator]
        [RouteEmptyStateValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [PropertyDependsOn(PROP_NAME_STARTTIMEWINDOW)]
        [PropertyDependsOn(PROP_NAME_BREAKS)]
        [PropertyDependsOn(PROP_NAME_ENDLOCATION)]
        [PropertyDependsOn(PROP_NAME_MAXTOTALDURATION)]
        [PropertyDependsOn(PROP_NAME_STARTLOCATION)]
        public override string Name
        {
            get { return _Entity.Name; }
            set
            {
                if (value == _Entity.Name)
                    return;

                // Save current name.
                var name = _Entity.Name;

                // Set new name.
                _Entity.Name = value;

                // Raise Property changed event for all items which 
                // has the same name, as item's old name.
                if ((this as ISupportOwnerCollection).OwnerCollection != null)
                    DataObjectValidationHelper.RaisePropertyChangedForDuplicate((this as ISupportOwnerCollection).OwnerCollection, name);

                NotifyPropertyChanged(PROP_NAME_NAME);
            }
        }

        /// <summary>
        /// Vehicle used on the route.
        /// </summary>
        [NotNullValidator(
            MessageTemplateResourceName = "Error_NotSetReferenceObjectVehicle",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [RefObjectValidator(
            MessageTemplateResourceName = "Error_InvalidRefObjVehicle",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [RouteRefObjectValidator(
            MessageTemplateResourceName = "Error_RouteContainsInvalidVehicle",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages))]
        [DomainProperty("DomainPropertyNameVehicle")]
        [AffectsRoutingProperty]
        [FreeRouteAssetValidator]
        public Vehicle Vehicle
        {
            get { return _VehicleWrap.Value; }
            set
            {
                if (_VehicleWrap.Value == value)
                {
                    return;
                }

                if (null != _VehicleWrap.Value)
                    _VehicleWrap.Value.PropertyChanged -= _Vehicle_PropertyChanged;

                // Save current value.
                var oldValue = _VehicleWrap.Value;

                _VehicleWrap.Value = value;
                if (this.RoutesCollectionOwner != null)
                {
                    this.RoutesCollectionOwner.UpdateRouteAssociation(this, PROP_NAME_VEHICLE);
                }

                // Raise Property changed event for all items which 
                // has the same value, as item's old value.
                var ownedObject = (ISupportOwnerCollection)this;
                if (ownedObject.OwnerCollection != null)
                {
                    DataObjectValidationHelper.RaisePropertyChangedForRoutesVehicles(
                        ownedObject.OwnerCollection,
                        oldValue,
                        value);
                }

                if (null != _VehicleWrap.Value)
                {
                    _VehicleWrap.Value.PropertyChanged +=
                        new PropertyChangedEventHandler(_Vehicle_PropertyChanged);
                }

                NotifyPropertyChanged(PROP_NAME_VEHICLE);
            }
        }

        /// <summary>
        /// Driver used on the route.
        /// </summary>
        [NotNullValidator(
            MessageTemplateResourceName = "Error_NotSetReferenceObjectDriver",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [RefObjectValidator(
            MessageTemplateResourceName = "Error_InvalidRefObjDriver",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [RouteRefObjectValidator(
            MessageTemplateResourceName = "Error_RouteContainsInvalidDriver",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages))]
        [DomainProperty("DomainPropertyNameDriver")]
        [AffectsRoutingProperty]
        [FreeRouteAssetValidator]
        public Driver Driver
        {
            get { return _DriverWrap.Value; }
            set
            {
                if (_DriverWrap.Value == value)
                {
                    return;
                }

                if (null != _DriverWrap.Value)
                    _DriverWrap.Value.PropertyChanged -= _Driver_PropertyChanged;

                // Save current value.
                var oldValue = _DriverWrap.Value;
                _DriverWrap.Value = value;

                if (this.RoutesCollectionOwner != null)
                {
                    this.RoutesCollectionOwner.UpdateRouteAssociation(this, PROP_NAME_DRIVER);
                }
                
                // Raise Property changed event for all items which 
                // has the same value, as item's old value.
                var ownedObject = (ISupportOwnerCollection)this;
                if (ownedObject.OwnerCollection != null)
                {
                    DataObjectValidationHelper.RaisePropertyChangedForRoutesDrivers(
                        ownedObject.OwnerCollection,
                        oldValue,
                        value);
                }

                if (null != _DriverWrap.Value)
                {
                    _DriverWrap.Value.PropertyChanged +=
                        new PropertyChangedEventHandler(_Driver_PropertyChanged);
                }

                NotifyPropertyChanged(PROP_NAME_DRIVER);
            }
        }

        /// <summary>
        /// Maximum number of orders that the route can accommodate.
        /// </summary>
        [RangeValidator(0, RangeBoundaryType.Exclusive, 1000, RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidMaxOrder",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameMaxOrders")]
        [AffectsRoutingProperty]
        public int MaxOrders
        {
            get { return (int)_Entity.MaxOrders; }
            set
            {
                if (value == _Entity.MaxOrders)
                    return;

                _Entity.MaxOrders = value;
                NotifyPropertyChanged(PROP_NAME_MAXORDERS);
            }
        }

        /// <summary>
        /// Time window when route can start.
        /// </summary>
        [RouteTimeWindowValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameStartTimeWindow")]
        [AffectsRoutingProperty]
        public TimeWindow StartTimeWindow
        {
            get { return _timeWindow; }
            set
            {
                if (_timeWindow == value)
                    return;

                _timeWindow.PropertyChanged -= _TimeWindow_PropertyChanged;

                if (null != value)
                {
                    _timeWindow = value;
                    _UpdateTimeWindowEntityData();

                    _timeWindow.PropertyChanged +=
                        new PropertyChangedEventHandler(_TimeWindow_PropertyChanged);
                }
                else
                {
                    _ClearTimeWindow();
                    _UpdateTimeWindowEntityData();
                }

                NotifyPropertyChanged(PROP_NAME_STARTTIMEWINDOW);
            }
        }

        /// <summary>
        /// Break preference for the route.
        /// </summary>
        [BreaksValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [BreakTypesValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [AffectsRoutingProperty]
        public Breaks Breaks
        {
            get
            {
                return _breaks;
            }
            set
            {
                if (_breaks == value)
                    return;

                _breaks.PropertyChanged -= _Breaks_PropertyChanged;

                if (value != null)
                {
                    _breaks = value.Clone() as Breaks;
                    _UpdateBreaksEntityData();

                    // Check that all breaks have same type on all routes.
                    if ((this as ISupportOwnerCollection).OwnerCollection != null)
                        DataObjectValidationHelper.RaisePropertyChangedForRoutesBreaks((this as ISupportOwnerCollection).OwnerCollection, Breaks);

                    _breaks.PropertyChanged += new PropertyChangedEventHandler(_Breaks_PropertyChanged);
                    _breaks.CollectionChanged += new NotifyCollectionChangedEventHandler(_Breaks_CollectionChanged);
                }
                else
                {
                    _breaks = null;
                    _UpdateBreaksEntityData();
                }

                NotifyPropertyChanged(PROP_NAME_BREAKS);
            }
        }

        /// <summary>
        /// This property used for backward compatibility with existent 
        /// plugins. Without it plugin will crash the application. 
        /// Property doesn't affect anything. Don't use it in new source code.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Don't use this property.", true)]
        public Break Break
        {
            get { return new TimeWindowBreak(); }
            set { var breakObj = value; }
        }

        /// <summary>
        /// Maximum route distance in miles.
        /// </summary>
        [RangeValidatorExt(0.0, SolverConst.MAX_TRAVEL_DISTANCE_MILES,
            MessageTemplateResourceName = "Error_InvalidMaxTravelDistance",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameMaxTravelDistance")]
        [UnitPropertyAttribute(Unit.Mile, Unit.Mile, Unit.Kilometer)]
        [AffectsRoutingProperty]
        public double MaxTravelDistance
        {
            get { return _Entity.MaxTravelDistance; }
            set
            {
                if ((float)value == _Entity.MaxTravelDistance)
                    return;

                _Entity.MaxTravelDistance = (float)value;
                NotifyPropertyChanged(PROP_NAME_MAXTRAVELDISTANCE);
            }
        }

        /// <summary>
        /// Maximum amount of time in minutes that driver can spend driving.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidMaxTravelDuration",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [PropertyComparisonValidator("MaxTotalDuration", ComparisonOperator.LessThanEqual,
            MessageTemplateResourceName = "Error_InvalidMaxTravelDuration2",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameMaxTravelDuration")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        [AffectsRoutingProperty]
        public double MaxTravelDuration
        {
            get { return _Entity.MaxTravelDuration; }
            set
            {
                if ((float)value == _Entity.MaxTravelDuration)
                    return;

                _Entity.MaxTravelDuration = (float)value;
                NotifyPropertyChanged(PROP_NAME_MAXTRAVELDURATION);
            }
        }

        /// <summary>
        /// Maximum total route duration in minutes.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidMaxTotalDuration",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameMaxTotalDuration")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        [AffectsRoutingProperty]
        public double MaxTotalDuration
        {
            get { return _Entity.MaxTotalDuration; }
            set
            {
                if ((float)value == _Entity.MaxTotalDuration)
                    return;

                _Entity.MaxTotalDuration = (float)value;
                NotifyPropertyChanged(PROP_NAME_MAXTOTALDURATION);
            }
        }

        /// <summary>
        /// Location where route starts.
        /// </summary>
        [RouteLocationValidator(
            MessageTemplateResourceName = "Error_NotSetReferenceObjectLocations",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DeletedObjectValidator(MessageTemplateResourceName = "Error_InvalidRefObjStartLocationIsDeleted",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [RefObjectValidator(
            MessageTemplateResourceName = "Error_InvalidRefObjStartLocation",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameStartLocation")]
        [AffectsRoutingProperty]
        public Location StartLocation
        {
            get { return _LocationStartWrap.Value; }
            set
            {
                if (_LocationStartWrap.Value == value)
                    return;

                if (null != _LocationStartWrap.Value)
                    _LocationStartWrap.Value.PropertyChanged -= _LocationStart_PropertyChanged;

                _LocationStartWrap.Value = value;

                if (null != _LocationStartWrap.Value)
                {
                    _LocationStartWrap.Value.PropertyChanged +=
                        new PropertyChangedEventHandler(_LocationStart_PropertyChanged);
                }

                NotifyPropertyChanged(PROP_NAME_STARTLOCATION);
            }
        }

        /// <summary>
        /// Time in minutes that must be spent in start location.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidTimeAtStart",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameTimeAtStart")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        [AffectsRoutingProperty]
        public double TimeAtStart
        {
            get { return _Entity.TimeAtStart; }
            set
            {
                if ((float)value == _Entity.TimeAtStart)
                    return;

                _Entity.TimeAtStart = (float)value;
                NotifyPropertyChanged(PROP_NAME_TIMEATSTART);
            }
        }

        /// <summary>
        /// Location where route ends.
        /// </summary>
        [RouteLocationValidator(
            MessageTemplateResourceName = "Error_NotSetReferenceObjectLocations",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DeletedObjectValidator(MessageTemplateResourceName = "Error_InvalidRefObjEndLocationIsDeleted",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [RefObjectValidator(
            MessageTemplateResourceName = "Error_InvalidRefObjEndLocation",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameEndLocation")]
        [AffectsRoutingProperty]
        public Location EndLocation
        {
            get { return _LocationEndWrap.Value; }
            set
            {
                if (_LocationEndWrap.Value == value)
                    return;

                if (null != _LocationEndWrap.Value)
                    _LocationEndWrap.Value.PropertyChanged -= _LocationEnd_PropertyChanged;

                _LocationEndWrap.Value = value;

                if (null != _LocationEndWrap.Value)
                {
                    _LocationEndWrap.Value.PropertyChanged +=
                        new PropertyChangedEventHandler(_LocationEnd_PropertyChanged);
                }

                NotifyPropertyChanged(PROP_NAME_ENDLOCATION);
            }
        }

        /// <summary>
        /// Time in minutes that must be spent in end location.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidTimeAtEnd",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameTimeAtEnd")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        [AffectsRoutingProperty]
        public double TimeAtEnd
        {
            get { return _Entity.TimeAtEnd; }
            set
            {
                if ((float)value == _Entity.TimeAtEnd)
                    return;

                _Entity.TimeAtEnd = (float)value;
                NotifyPropertyChanged(PROP_NAME_TIMEATEND);
            }
        }

        /// <summary>
        /// Location where driver can renew vehicle's capacities.
        /// </summary>
        [LocationsValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameRenewalLocations")]
        [AffectsRoutingProperty]
        public IDataObjectCollection<Location> RenewalLocations
        {
            get { return _LocationsRenewalWrap.DataObjects; }
            set
            {
                if (_LocationsRenewalWrap.DataObjects == value)
                {
                    return;
                }

                if (value == null)
                {
                    return;
                }

                _LocationsRenewalWrap.DataObjects.CollectionChanged -=
                    _LocationsRenewal_CollectionChanged;
                _LocationsRenewalWrap.DataObjects = value;
                _LocationsRenewalWrap.DataObjects.CollectionChanged +=
                    _LocationsRenewal_CollectionChanged;

                NotifyPropertyChanged(PROP_NAME_RENEWALLOCATIONS);
            }
        }

        /// <summary>
        /// Time in minutes that driver needs to renew vehicle's capacities.
        /// </summary>
        [RangeValidator(0.0, RangeBoundaryType.Inclusive, SolverConst.MAX_TIME_MINS,
            RangeBoundaryType.Inclusive,
            MessageTemplateResourceName = "Error_InvalidTimeAtRenewal",
            MessageTemplateResourceType = typeof(ArcLogistics.Properties.Messages),
            Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameTimeAtRenewal")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        [AffectsRoutingProperty]
        public double TimeAtRenewal
        {
            get { return _Entity.TimeAtRenewal; }
            set
            {
                if ((float)value == _Entity.TimeAtRenewal)
                    return;

                _Entity.TimeAtRenewal = (float)value;
                NotifyPropertyChanged(PROP_NAME_TIMEATRENEWAL);
            }
        }

        /// <summary>
        /// Route color for drawing on the map.
        /// </summary>
        [DomainProperty("DomainPropertyNameColor")]
        public Color Color
        {
            get { return (0 == _Entity.Color) ? Color.Empty : Color.FromArgb(_Entity.Color); }
            set
            {
                _Entity.Color = value.ToArgb();
                NotifyPropertyChanged(PROP_NAME_COLOR);
            }
        }

        /// <summary>
        /// Collection of route zones which orders should be serviced.
        /// </summary>
        [ZonesValidator(Tag = PRIMARY_VALIDATOR_TAG)]
        [DomainProperty("DomainPropertyNameZones")]
        [AffectsRoutingProperty]
        public IDataObjectCollection<Zone> Zones
        {
            get { return _ZonesWrap.DataObjects; }
            set
            {
                if (_ZonesWrap.DataObjects == value)
                {
                    return;
                }

                if (value == null)
                {
                    return;
                }

                _ZonesWrap.DataObjects.CollectionChanged -= _Zones_CollectionChanged;
                _ZonesWrap.DataObjects = value;
                _ZonesWrap.DataObjects.CollectionChanged += _Zones_CollectionChanged;

                NotifyPropertyChanged(PROP_NAME_ZONES);
            }
        }

        /// <summary>
        /// Arbitrary text about the route.
        /// </summary>
        [DomainProperty("DomainPropertyNameComment")]
        public string Comment
        {
            get { return _Entity.Comment; }
            set
            {
                _Entity.Comment = value;
                NotifyPropertyChanged(PROP_NAME_COMMENT);
            }
        }

        /// <summary>
        /// Recurrence settings for the route. Applied only to default routes.
        /// </summary>
        [DaysValidator]
        public Days Days
        {
            get { return _days; }
            set
            {
                _days.PropertyChanged -= _Days_PropertyChanged;

                _days = value;
                if (null == value)
                    _Entity.Days = null;
                else
                {
                    _UpdateDaysEntityData();

                    _days.PropertyChanged +=
                        new PropertyChangedEventHandler(_Days_PropertyChanged);
                }

                NotifyPropertyChanged(PROP_NAME_DAYS);
            }
        }

        /// <summary>
        /// Gets ID of the default route from which this route was created.
        /// </summary>
        public Guid? DefaultRouteID
        {
            get { return _Entity.DefaultRouteID; }
            set { _Entity.DefaultRouteID = value; }
        }

        /// <summary>
        /// Indicates either route should be drawn on the map.
        /// </summary>
        public bool IsVisible
        {
            get { return _Entity.Visible; }
            set
            {
                _Entity.Visible = value;
                NotifyPropertyChanged(PROP_NAME_ISVISIBLE);
            }
        }

        /// <summary>
        /// Gets total cost of the route in currency units.
        /// </summary>
        [UnitPropertyAttribute(Unit.Currency, Unit.Currency, Unit.Currency)]
        public double Cost
        {
            get { return _Entity.Cost; }
            internal set
            {
                if (_Entity.Cost != value)
                {
                    _Entity.Cost = value;
                    NotifyPropertyChanged(PROP_NAME_COST);
                }
            }
        }

        /// <summary>
        /// Gets start time of the route.
        /// </summary>
        public DateTime? StartTime
        {
            get { return _Entity.StartTime; }
            internal set
            {
                if (_Entity.StartTime != value)
                {
                    _Entity.StartTime = value;
                    NotifyPropertyChanged(PROP_NAME_START_TIME);
                }
            }
        }

        /// <summary>
        /// Gets end time of the route.
        /// </summary>
        public DateTime? EndTime
        {
            get { return _Entity.EndTime; }
            internal set
            {
                if (_Entity.EndTime != value)
                {
                    _Entity.EndTime = value;
                    NotifyPropertyChanged(PROP_NAME_END_TIME);
                }
            }
        }

        /// <summary>
        /// Gets amount of route overtime in minutes.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        public double Overtime
        {
            get { return _Entity.Overtime; }
            internal set
            {
                if (_Entity.Overtime != value)
                {
                    _Entity.Overtime = value;
                    NotifyPropertyChanged(PROP_NAME_OVERTIME);
                }
            }
        }

        /// <summary>
        /// Gets total route time in minutes.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        public double TotalTime
        {
            get { return _Entity.TotalTime; }
            internal set
            {
                if (_Entity.TotalTime != value)
                {
                    _Entity.TotalTime = value;
                    NotifyPropertyChanged(PROP_NAME_TOTAL_TIME);
                }
            }
        }

        /// <summary>
        /// Gets total route distance in miles.
        /// </summary>
        [UnitPropertyAttribute(Unit.Mile, Unit.Mile, Unit.Kilometer)]
        public double TotalDistance
        {
            get { return _Entity.TotalDistance; }
            internal set
            {
                if (_Entity.TotalDistance != value)
                {
                    _Entity.TotalDistance = value;
                    NotifyPropertyChanged(PROP_NAME_TOTAL_DISTANCE);
                }
            }
        }

        /// <summary>
        /// Gets total amount of time in minutes that driver spent on the way.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Hour, Unit.Hour)]
        public double TravelTime
        {
            get { return _Entity.TravelTime; }
            internal set
            {
                if (_Entity.Cost != value)
                {
                    _Entity.TravelTime = value;
                    NotifyPropertyChanged(PROP_NAME_TRAVEL_TIME);
                }
            }
        }

        /// <summary>
        /// Gets total amount of violation time in minutes that is the sum of violation times
        /// for all the route orders.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double ViolationTime
        {
            get { return _Entity.ViolationTime; }
            internal set
            {
                if (_Entity.ViolationTime != value)
                {
                    _Entity.ViolationTime = value;
                    NotifyPropertyChanged(PROP_NAME_VIOLATION_TIME);
                }
            }
        }

        /// <summary>
        /// Gets total amount of time that driver spent waiting for the orders.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double WaitTime
        {
            get { return _Entity.WaitTime; }
            internal set
            {
                if (_Entity.WaitTime != value)
                {
                    _Entity.WaitTime = value;
                    NotifyPropertyChanged(PROP_NAME_WAIT_TIME);
                }
            }
        }

        /// <summary>
        /// Indicates whether the route is locked.
        /// </summary>
        /// <remarks>
        /// Locked route cannot take part in routing operations. Orders cannot be assigned
        /// or unassigned from this route.
        /// </remarks>
        public bool IsLocked
        {
            get { return _Entity.Locked; }
            set
            {
                if (_Entity.Locked != value)
                {
                    _Entity.Locked = value;
                    NotifyPropertyChanged(PROP_NAME_ISLOCKED);
                }
            }
        }

        /// <summary>
        /// Indicates whether the route can accommodate orders located outside its assigned zones.
        /// </summary>
        /// <remarks>If <c>HardZones</c> is <c>true</c> then route can only accommodate orders
        /// located inside of its zones.</remarks>
        [AffectsRoutingProperty]
        public bool HardZones
        {
            get { return _Entity.HardZones; }
            set
            {
                _Entity.HardZones = value;
                NotifyPropertyChanged(PROP_NAME_HARDZONES);
            }
        }

        /// <summary>
        /// Total capacities used on the route.
        /// </summary>
        public Capacities Capacities
        {
            get { return _capacities; }
            internal set
            {
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
            }
        }

        /// <summary>
        /// Information about capacities. The same as its project exposes.
        /// </summary>
        public CapacitiesInfo CapacitiesInfo
        {
            get { return _capacitiesInfo; }
            internal set
            {
                if (_capacitiesInfo != null)
                {
                    string message = Properties.Resources.CapacitiesInfoAlreadySet;
                    throw new InvalidOperationException(message); // exception
                }

                _capacities = Capacities.CreateFromDBString(_Entity.Capacities, value);
                _capacitiesInfo = value;
            }
        }

        /// <summary>
        /// Gets the route's true path.
        /// </summary>
        public Polyline Path
        {
            get { return _BuildPath(); }
        }

        /// <summary>
        /// Gets the route's total wait time in percentage.
        /// </summary>
        public double WaitTimeInPercentage
        {
            get
            {
                double value = 0.0;
                if (this.TotalTime != 0.0)
                    value = (this.WaitTime / this.TotalTime) * 100;

                return value;
            }
        }

        /// <summary>
        /// Gets number of orders assigned to the route.
        /// </summary>
        public int OrderCount
        {
            get
            {
                int orderCount = 0;
                foreach (Stop stop in Stops)
                {
                    if (StopType.Order == stop.StopType)
                        ++orderCount;
                }

                return orderCount;
            }
        }

        /// <summary>
        /// Gets total planned amount of time in minutes for servicing all orders.
        /// </summary>
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double TotalServiceTime
        {
            get
            {
                double totalServiceTime = 0.0;
                foreach (Stop stop in Stops)
                {
                    if (StopType.Order != stop.StopType)
                        continue;

                    Debug.Assert(stop.AssociatedObject is Order);
                    Order order = stop.AssociatedObject as Order;
                    totalServiceTime += order.ServiceTime;
                }

                return totalServiceTime;
            }
        }

        /// <summary>
        /// Gets number of location visits excluding the end location.
        /// </summary>
        public int RunCount
        {
            get
            {
                int runCount = 0;
                foreach (Stop stop in Stops)
                {
                    if (StopType.Location == stop.StopType)
                        ++runCount;
                }

                if (0 < runCount)
                {   // NOTE: excluding the end location
                    --runCount;
                }

                return runCount;
            }
        }

        /// <summary>
        /// Gets number of violated stops.
        /// </summary>
        public int ViolatedStopCount
        {
            get
            {
                int violatedStopCount = 0;
                foreach (Stop stop in Stops)
                {
                    if (stop.IsViolated)
                        ++violatedStopCount;
                }

                return violatedStopCount;
            }
        }

        /// <summary>
        /// Gets parent schedule of the route.
        /// </summary>
        public Schedule Schedule
        {
            get { return _ScheduleWrap.Value; }
        }

        /// <summary>
        /// Gets collection of route stops.
        /// </summary>
        public IDataObjectCollection<Stop> Stops
        {
            get { return _StopsWrap.DataObjects; }
            internal set { _StopsWrap.DataObjects = value; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the route.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion // Public members

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

        #endregion ICapacitiesInit members

        #region ICloneable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>Clone of this object.</returns>
        public override object Clone()
        {
            Route obj = new Route(_capacitiesInfo);
            this.CopyTo(obj);
            return obj;
        }

        /// <summary>
        /// Method clones all route properties except results.
        /// </summary>
        /// <returns>Clone of this object without build results.</returns>
        public object CloneNoResults()
        {
            Route obj = new Route(_capacitiesInfo);
            _CopyNoResults(obj);
            return obj;
        }

        #endregion // ICloneable members

        #region ICopyable members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Copies all the object's data to the target data object.
        /// </summary>
        /// <param name="obj">Target data object.</param>
        public override void CopyTo(DataObject obj)
        {
            Debug.Assert(obj is Route);

            _CopyNoResults(obj);

            Route route = obj as Route;

            // resulting data
            route.Cost = this.Cost;
            route.StartTime = this.StartTime;
            route.EndTime = this.EndTime;
            route.Overtime = this.Overtime;
            route.TotalTime = this.TotalTime;
            route.TotalDistance = this.TotalDistance;
            route.TravelTime = this.TravelTime;
            route.ViolationTime = this.ViolationTime;
            route.WaitTime = this.WaitTime;
            route.IsLocked = this.IsLocked;
            route.IsVisible = this.IsVisible;
            route.Capacities = (Capacities)this._capacities.Clone();

            // stops
            foreach (Stop stop in this.Stops)
                route.Stops.Add((Stop)stop.Clone());
        }
        #endregion // ICopyable members

        #region ISupportOwnerCollection Members

        /// <summary>
        /// Collection in which this DataObject is placed.
        /// </summary>
        IEnumerable ISupportOwnerCollection.OwnerCollection
        {
            get
            {
                // If route is scheduled then return scheduled collection, otherwise - default 
                // routes collection.
                if (Schedule != null)
                    return Schedule.Routes;
                else
                    return _DefaultRoutesCollection;
            }
            set
            {
                // If new collection isnt null and route is scheduled - throw exception.
                if (value != null && Schedule != null)
                        throw new InvalidOperationException(Properties.Messages.Error_RouteIsScheduled);

                // If we delete object from collection - raise property
                // changed events for driver, vehicle and breaks for all routes in owner collection.
                if (value == null && (this as ISupportOwnerCollection).OwnerCollection != null)
                {
                    foreach (Route route in (this as ISupportOwnerCollection).OwnerCollection)
                    {
                        (route as IForceNotifyPropertyChanged).RaisePropertyChangedEvent(PROP_NAME_DRIVER);
                        (route as IForceNotifyPropertyChanged).RaisePropertyChangedEvent(PROP_NAME_VEHICLE);
                        (route as IForceNotifyPropertyChanged).RaisePropertyChangedEvent(PROP_NAME_BREAKS);
                    }
                }

                // Set owner collection.
                _DefaultRoutesCollection = value;
            }
        }

        #endregion

        #region internal properties
        /// <summary>
        /// Can save flag.
        /// </summary>
        internal override bool CanSave
        {
            get { return base.CanSave; }
            set
            {
                foreach (Stop stop in this.Stops)
                    stop.CanSave = value;

                base.CanSave = value;
            }
        }

        internal bool Default
        {
            get { return _Entity.Default; }
            set { _Entity.Default = value; }
        }

        /// <summary>
        /// Gets or sets a reference to the <see cref="IRoutesCollectionOwner"/> object this Route
        /// belongs to.
        /// </summary>
        internal IRoutesCollectionOwner RoutesCollectionOwner
        {
            get;
            set;
        }
        #endregion

        #region Internal method

        /// <summary>
        /// Compare two routes.
        /// </summary>
        /// <param name="route">Route to compare with this.</param>
        /// <returns>"True" if they are equal and false otherwise.</returns>
        internal bool EqualsByValue(Route route)
        {
            // If route to compare with is null - they are not equal.
            if (route == null)
                return false;

            // Compare properties of both routes.
            return Breaks.EqualsByValue(route.Breaks) && Name == route.Name &&
                Color == route.Color && Comment == route.Comment && route.HardZones == HardZones &&
                MaxOrders == route.MaxOrders && MaxTotalDuration == route.MaxTotalDuration &&
                MaxTravelDistance == route.MaxTravelDistance && TimeAtEnd == route.TimeAtEnd &&
                MaxTravelDuration == route.MaxTravelDuration &&
                StartTimeWindow.EqualsByValue(route.StartTimeWindow) &&
                TimeAtStart == route.TimeAtStart && TimeAtRenewal == route.TimeAtRenewal ;
        }

        #endregion

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Reference to entity.
        /// </summary>
        private DataModel.Routes _Entity
        {
            get { return (base.RawEntity as DataModel.Routes); }
        }

        /// <summary>
        /// Wrapper for the vehicle entity data.
        /// </summary>
        private EntityRefWrapper<Vehicle, DataModel.Vehicles> _VehicleWrap
        {
            get
            {
                if (null == _refVehicle)
                {
                    _refVehicle = new EntityRefWrapper<Vehicle, DataModel.Vehicles>
                                                      (_Entity.VehiclesReference, this);
                }

                return _refVehicle;
            }
        }

        /// <summary>
        /// Wrapper for the driver entity data.
        /// </summary>
        private EntityRefWrapper<Driver, DataModel.Drivers> _DriverWrap
        {
            get
            {
                if (null == _refDriver)
                {
                    _refDriver = new EntityRefWrapper<Driver, DataModel.Drivers>
                                                     (_Entity.DriversReference, this);
                }

                return _refDriver;
            }
        }

        /// <summary>
        /// Wrapper for the start location entity data.
        /// </summary>
        private EntityRefWrapper<Location, DataModel.Locations> _LocationStartWrap
        {
            get
            {
                if (null == _refLocationStart)
                {
                    _refLocationStart = new EntityRefWrapper<Location, DataModel.Locations>
                                                            (_Entity.LocationsReference, this);
                }

                return _refLocationStart;
            }
        }

        /// <summary>
        /// Wrapper for the end location entity data.
        /// </summary>
        private EntityRefWrapper<Location, DataModel.Locations> _LocationEndWrap
        {
            get
            {
                if (null == _refLocationEnd)
                {
                    _refLocationEnd = new EntityRefWrapper<Location, DataModel.Locations>
                                                          (_Entity.Locations1Reference, this);
                }

                return _refLocationEnd;
            }
        }

        /// <summary>
        /// Wrapper for the renewal locations entity data.
        /// </summary>
        private EntityCollWrapper<Location, DataModel.Locations> _LocationsRenewalWrap
        {
            get
            {
                if (null == _refLocationsRenewal)
                {
                    _refLocationsRenewal = new EntityCollWrapper<Location, DataModel.Locations>
                                                                (_Entity.Locations2, this, false);
                }

                return _refLocationsRenewal;
            }
        }

        /// <summary>
        /// Wrapper for the zones entity data.
        /// </summary>
        private EntityCollWrapper<Zone, DataModel.Zones> _ZonesWrap
        {
            get
            {
                if (null == _refZones)
                {
                    _refZones = new EntityCollWrapper<Zone, DataModel.Zones>
                                                     (_Entity.Zones, this, false);
                }

                return _refZones;
            }
        }

        /// <summary>
        /// Wrapper for the parent schedule entity data.
        /// </summary>
        private EntityRefWrapper<Schedule, DataModel.Schedules> _ScheduleWrap
        {
            get
            {
                if (_refSchedule == null)
                {
                    _refSchedule = new EntityRefWrapper<Schedule, DataModel.Schedules>
                                                       (_Entity.SchedulesReference, this);
                }

                return _refSchedule;
            }
        }

        /// <summary>
        /// Wrapper for the stops entity data.
        /// </summary>
        private EntityCollWrapper<Stop, DataModel.Stops> _StopsWrap
        {
            get
            {
                if (_refStops == null)
                {
                    _refStops = new EntityCollWrapper<Stop, DataModel.Stops>
                                                      (_Entity.Stops, this, false);
                }

                return _refStops;
            }
        }

        #endregion // Private properties

        #region Private methods - TimeWindow
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies about change of the time window.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _TimeWindow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateTimeWindowEntityData();
            NotifySubPropertyChanged(PROP_NAME_STARTTIMEWINDOW, e.PropertyName);
        }

        /// <summary>
        /// inits time window.
        /// </summary>
        /// <param name="entity">Entity data.</param>
        private void _InitTimeWindow(DataModel.Routes entity)
        {
            if ((null != entity.WorkFrom) && (null != entity.WorkTo))
                _SetTimeWindow(entity);
            else
                _ClearTimeWindow();
        }

        /// <summary>
        /// Sets time window using entity data.
        /// </summary>
        /// <param name="entity"></param>
        private void _SetTimeWindow(DataModel.Routes entity)
        {
            _timeWindow =
                TimeWindow.CreateFromEffectiveTimes(new TimeSpan((long)entity.WorkFrom),
                                                    new TimeSpan((long)entity.WorkTo));
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
        /// Updates time window in database.
        /// </summary>
        private void _UpdateTimeWindowEntityData()
        {
            if (!_timeWindow.IsWideOpen)
            {
                _Entity.WorkFrom = _timeWindow.EffectiveFrom.Ticks;
                _Entity.WorkTo = _timeWindow.EffectiveTo.Ticks;
            }
            else
            {
                _Entity.WorkFrom = null;
                _Entity.WorkTo = null;
            }
        }
        #endregion // Private methods - TimeWindow

        #region Private methods - Breaks
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies about change of the breaks.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _Breaks_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateBreaksEntityData();
            NotifySubPropertyChanged(PROP_NAME_BREAKS, e.PropertyName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _Breaks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((this as ISupportOwnerCollection).OwnerCollection != null)
                DataObjectValidationHelper.RaisePropertyChangedForRoutesBreaks
                    ((this as ISupportOwnerCollection).OwnerCollection, Breaks);
        }

        /// <summary>
        /// Updates breaks entity data.
        /// </summary>
        private void _UpdateBreaksEntityData()
        {
            if (_breaks != null)
                _Entity.Breaks = Breaks.AssemblyDBString(_breaks);
            else
                _Entity.Breaks = null;
        }
        #endregion // Private methods - Break

        #region Private methods - Days
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies about change of the days.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _Days_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _UpdateDaysEntityData();
            NotifyPropertyChanged(PROP_NAME_DAYS);
        }

        /// <summary>
        /// Updates days entity data.
        /// </summary>
        private void _UpdateDaysEntityData()
        {
            _Entity.Days = Days.AssemblyDBString(_days);
        }

        #endregion // Private methods - Days

        #region Private methods - Capacities
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates capacities by capacities info.
        /// </summary>
        /// <param name="capacitiesInfo">Capacities info.</param>
        private void _CreateCapacities(CapacitiesInfo capacitiesInfo)
        {
            _capacities = new Capacities(capacitiesInfo);
            _capacitiesInfo = capacitiesInfo;
        }

        /// <summary>
        /// Clears capacities.
        /// </summary>
        private void _ClearCapacities()
        {
            _capacities = new Capacities(_capacitiesInfo);
        }

        /// <summary>
        /// Updates capacities entity data.
        /// </summary>
        private void _UpdateCapacitiesEntityData()
        {
            _Entity.Capacities = Capacities.AssemblyDBString(_capacities);
        }

        #endregion // Private methods - Capacities

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
       
        /// <summary>
        /// When collection changed clear owner collection if this item was deleted.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ScheduleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // If item was added - raise Name changed for items with the same name.
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    if ((newItem as ISupportName) != null && (newItem as ISupportName).Name == Name)
                    {
                        NotifyPropertyChanged(PROP_NAME_NAME);
                        break;
                    }
                }
            }
            // If item was removed from collection - raise property changed
            // events for items with the same name, driver, vehicle.
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Route route in e.OldItems)
                {
                    if (route.Name == Name)
                        NotifyPropertyChanged(PROP_NAME_NAME);
                    if (route.Driver == Driver)
                        NotifyPropertyChanged(PROP_NAME_DRIVER);
                    if (route.Vehicle == Vehicle)
                        NotifyPropertyChanged(PROP_NAME_VEHICLE);
                }
            }
            // If collection was reseted or some items in it was replaced - 
            // then raise name, driver, vehicle, breaks property changed for all its elements.
            else if (e.Action == NotifyCollectionChangedAction.Reset ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                var ownerCollection = (this as ISupportOwnerCollection).OwnerCollection;
                DataObjectValidationHelper.RaisePropertyChangedForDuplicate(
                    ownerCollection,
                    Name);
                DataObjectValidationHelper.RaisePropertyChangedForRoutesDrivers(
                    ownerCollection,
                    Driver);
                DataObjectValidationHelper.RaisePropertyChangedForRoutesVehicles(
                    ownerCollection,
                    Vehicle);
                DataObjectValidationHelper.RaisePropertyChangedForRoutesBreaks(
                    ownerCollection,
                    Breaks);
            }
        }

        /// <summary>
        /// Notifies about change of the vehicle.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _Vehicle_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_VEHICLE, e.PropertyName);
        }

        /// <summary>
        /// Notifies about change of the driver.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _Driver_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_DRIVER, e.PropertyName);
        }

        /// <summary>
        /// Notifies about change of the start location.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _LocationStart_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_STARTLOCATION, e.PropertyName);
        }

        /// <summary>
        /// Notifies about change of the end location.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _LocationEnd_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_ENDLOCATION, e.PropertyName);
        }

        /// <summary>
        /// Notifies about change of the renewal locations.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _LocationsRenewal_CollectionChanged(object sender,
                                                         NotifyCollectionChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_RENEWALLOCATIONS,
                                     PROP_NAME_RENEWALLOCATIONSCOLLECTION);
        }

        /// <summary>
        /// Notifies about change of the zones.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Zones_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifySubPropertyChanged(PROP_NAME_ZONES, PROP_NAME_ZONESCOLLECTION);
        }

        /// <summary>
        /// Does deep copy without build results.
        /// </summary>
        /// <param name="obj">Object to copy values.</param>
        private void _CopyNoResults(DataObject obj)
        {
            Route route = obj as Route;
            Debug.Assert(null != route);

            route.Name = this.Name;
            route.Comment = this.Comment;
            route.Color = this.Color;
            route.MaxOrders = this.MaxOrders;
            route.MaxTotalDuration = this.MaxTotalDuration;
            route.MaxTravelDistance = this.MaxTravelDistance;
            route.MaxTravelDuration = this.MaxTravelDuration;
            route.TimeAtEnd = this.TimeAtEnd;
            route.TimeAtRenewal = this.TimeAtRenewal;
            route.TimeAtStart = this.TimeAtStart;
            route.HardZones = this.HardZones;

            route.StartLocation = this.StartLocation;
            route.EndLocation = this.EndLocation;
            route.Vehicle = this.Vehicle;
            route.Driver = this.Driver;

            if (null != this.StartTimeWindow)
                route.StartTimeWindow = this.StartTimeWindow.Clone() as TimeWindow;

            if (null != this.Breaks)
                route.Breaks = this.Breaks.Clone() as Breaks;

            route.RenewalLocations = this.RenewalLocations;

            route.Zones = this.Zones;

            route.Days = this.Days.Clone() as Days;

            if (_Entity.Default)
                route._Entity.DefaultRouteID = this.Id;
            else if (null != this.DefaultRouteID)
                route._Entity.DefaultRouteID = this.DefaultRouteID;
        }

        /// <summary>
        /// Builds route's path.
        /// </summary>
        /// <returns>Route's path geometry.</returns>
        private Polyline _BuildPath()
        {
            Polyline polyline = null;

            if (this.Stops.Count > 0)
            {
                var stops = CommonHelpers.GetSortedStops(this);

                var points = new List<ESRI.ArcLogistics.Geometry.Point>();
                foreach (Stop stop in stops)
                {
                    if (stop.Path != null && stop.Path.TotalPointCount > 0)
                        points.AddRange(stop.Path.GetGroupPoints(0));
                }

                if (points.Count > 0)
                    polyline = new Polyline(points.ToArray());
            }

            return polyline;
        }

        /// <summary>
        /// Attaches properties events.
        /// </summary>
        private void _InitPropertiesEvents()
        {
            _Entity.SchedulesReference.AssociationChanged +=  SchedulesReference_AssociationChanged;

            _timeWindow.PropertyChanged += _TimeWindow_PropertyChanged;

            _breaks.PropertyChanged += _Breaks_PropertyChanged;
            _breaks.CollectionChanged += _Breaks_CollectionChanged;

            _days.PropertyChanged += _Days_PropertyChanged;

            _LocationsRenewalWrap.DataObjects.CollectionChanged += _LocationsRenewal_CollectionChanged;
            _ZonesWrap.DataObjects.CollectionChanged += _Zones_CollectionChanged;

            if (null != _VehicleWrap.Value)
                _VehicleWrap.Value.PropertyChanged += _Vehicle_PropertyChanged;

            if (null != _DriverWrap.Value)
                _DriverWrap.Value.PropertyChanged += _Driver_PropertyChanged;

            if (null != _LocationStartWrap.Value)
                _LocationStartWrap.Value.PropertyChanged += _LocationStart_PropertyChanged;

            if (null != _LocationEndWrap.Value)
                _LocationEndWrap.Value.PropertyChanged += _LocationEnd_PropertyChanged;
        }

        /// <summary>
        /// Remember ref to current scheduled routes, subscribe to scheduled routes 
        /// collection changed event and clear default routes collection.
        /// </summary>
        private void _AddScheduledRoutesCollectionChangedHandler()
        {
            this.Schedule.RoutesCollectionInitialized -= _ScheduleRoutesCollectionInitialized;

            _ScheduledRoutesCollection = this.Schedule.Routes;
            _ScheduledRoutesCollection.CollectionChanged += _ScheduleCollectionChanged;
            if (_DefaultRoutesCollection != null)
                (this as ISupportOwnerCollection).OwnerCollection = null;
        }

        /// <summary>
        /// When route added to or removed from schedule we need to subscribe(unsubscribe) from event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">CollectionChangeEventArgs.</param>
        void SchedulesReference_AssociationChanged(object sender, CollectionChangeEventArgs e)
        {
            // If we added this route to schedule, remember it schedule.routes, subscribe to 
            // collection changed event and clear _DefaultsRoutes if we haven't done this before.
            if (e.Action == CollectionChangeAction.Add)
            {
                if (_ScheduledRoutesCollection == null ||
                    _ScheduledRoutesCollection != Schedule.Routes)
                {
                    _AddScheduledRoutesCollectionChangedHandler();
                }
            }
            // If we removed route from collection - then unsubscribe from this
            // collection changed event and clear reference to this collection.
            if (e.Action == CollectionChangeAction.Remove)
            {
                _ScheduledRoutesCollection.CollectionChanged -= _ScheduleCollectionChanged;
                _ScheduledRoutesCollection = null;
            }
        }

        /// <summary>
        /// Handles completion of owner schedule routes collection initialization.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event data object.</param>
        private void _ScheduleRoutesCollectionInitialized(object sender, EventArgs e)
        {
            _AddScheduledRoutesCollectionChangedHandler();
        }
        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the Name property.
        /// </summary>
        private const string PROP_NAME_NAME = "Name";
        /// <summary>
        /// Name of the Vehicle property.
        /// </summary>
        private const string PROP_NAME_VEHICLE = "Vehicle";
        /// <summary>
        /// Name of the Driver property.
        /// </summary>
        private const string PROP_NAME_DRIVER = "Driver";
        /// <summary>
        /// Name of the MaxOrders property.
        /// </summary>
        private const string PROP_NAME_MAXORDERS = "MaxOrders";
        /// <summary>
        /// Name of the MaxTotalDuration property.
        /// </summary>
        private const string PROP_NAME_MAXTOTALDURATION = "MaxTotalDuration";
        /// <summary>
        /// Name of the MaxTravelDistance property.
        /// </summary>
        private const string PROP_NAME_MAXTRAVELDISTANCE = "MaxTravelDistance";
        /// <summary>
        /// Name of the MaxTravelDuration property.
        /// </summary>
        private const string PROP_NAME_MAXTRAVELDURATION = "MaxTravelDuration";
        /// <summary>
        /// Name of the StartTimeWindow property.
        /// </summary>
        private const string PROP_NAME_STARTTIMEWINDOW = "StartTimeWindow";
        /// <summary>
        /// Name of the Breaks property.
        /// </summary>
        private const string PROP_NAME_BREAKS = "Breaks";
        /// <summary>
        /// Name of the TimeAtStart property.
        /// </summary>
        private const string PROP_NAME_TIMEATSTART = "TimeAtStart";
        /// <summary>
        /// Name of the StartLocation property.
        /// </summary>
        private const string PROP_NAME_STARTLOCATION = "StartLocation";
        /// <summary>
        /// Name of the TimeAtEnd property.
        /// </summary>
        private const string PROP_NAME_TIMEATEND = "TimeAtEnd";
        /// <summary>
        /// Name of the EndLocation property.
        /// </summary>
        private const string PROP_NAME_ENDLOCATION = "EndLocation";
        /// <summary>
        /// Name of the TimeAtRenewal property.
        /// </summary>
        private const string PROP_NAME_TIMEATRENEWAL = "TimeAtRenewal";
        /// <summary>
        /// Name of the RenewalLocations property.
        /// </summary>
        private const string PROP_NAME_RENEWALLOCATIONS = "RenewalLocations";
        /// <summary>
        /// Name of the RenewalLocationsCollection property.
        /// </summary>
        private const string PROP_NAME_RENEWALLOCATIONSCOLLECTION = "RenewalLocationsCollection";
        /// <summary>
        /// Name of the Zones property.
        /// </summary>
        private const string PROP_NAME_ZONES = "Zones";
        /// <summary>
        /// Name of the ZonesCollection property.
        /// </summary>
        private const string PROP_NAME_ZONESCOLLECTION = "ZonesCollection";
        /// <summary>
        /// Name of the Color property.
        /// </summary>
        private const string PROP_NAME_COLOR = "Color";
        /// <summary>
        /// Name of the Comment property.
        /// </summary>
        private const string PROP_NAME_COMMENT = "Comment";
        /// <summary>
        /// Name of the Days property.
        /// </summary>
        private const string PROP_NAME_DAYS = "Days";
        /// <summary>
        /// Name of the IsLocked property.
        /// </summary>
        private const string PROP_NAME_ISLOCKED = "IsLocked";
        /// <summary>
        /// Name of the IsVisible property.
        /// </summary>
        private const string PROP_NAME_ISVISIBLE = "IsVisible";
        /// <summary>
        /// Name of the HardZones property.
        /// </summary>
        private const string PROP_NAME_HARDZONES = "HardZones";
        /// <summary>
        /// Name of the Cost property.
        /// </summary>
        private const string PROP_NAME_COST = "Cost";
        /// <summary>
        /// Name of the StartTime property.
        /// </summary>
        private const string PROP_NAME_START_TIME = "StartTime";
        /// <summary>
        /// Name of the EndTime property.
        /// </summary>
        private const string PROP_NAME_END_TIME = "EndTime";
        /// <summary>
        /// Name of the Overtime property.
        /// </summary>
        private const string PROP_NAME_OVERTIME = "Overtime";
        /// <summary>
        /// Name of the TotalTime property.
        /// </summary>
        private const string PROP_NAME_TOTAL_TIME = "TotalTime";
        /// <summary>
        /// Name of the TotalDistance property.
        /// </summary>
        private const string PROP_NAME_TOTAL_DISTANCE = "TotalDistance";
        /// <summary>
        /// Name of the TravelTime property.
        /// </summary>
        private const string PROP_NAME_TRAVEL_TIME = "TravelTime";
        /// <summary>
        /// Name of the ViolationTime property.
        /// </summary>
        private const string PROP_NAME_VIOLATION_TIME = "ViolationTime";
        /// <summary>
        /// Name of the WaitTime property.
        /// </summary>
        private const string PROP_NAME_WAIT_TIME = "WaitTime";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Collection in which stored this item.
        /// </summary>
        private IEnumerable _DefaultRoutesCollection;
        /// <summary>
        /// Schedule routes collection in which stored this route.
        /// </summary>
        public IDataObjectCollection<Route> _ScheduledRoutesCollection;

        /// <summary>
        /// Reference to the start location.
        /// </summary>
        private EntityRefWrapper<Location, DataModel.Locations> _refLocationStart;
        /// <summary>
        /// Reference to the end location.
        /// </summary>
        private EntityRefWrapper<Location, DataModel.Locations> _refLocationEnd;
        /// <summary>
        /// Reference to the renewal locations.
        /// </summary>
        private EntityCollWrapper<Location, DataModel.Locations> _refLocationsRenewal;

        /// <summary>
        /// Reference to the vehicle.
        /// </summary>
        private EntityRefWrapper<Vehicle, DataModel.Vehicles> _refVehicle;
        /// <summary>
        /// Reference to the driver.
        /// </summary>
        private EntityRefWrapper<Driver, DataModel.Drivers> _refDriver;

        /// <summary>
        /// Reference to the zones.
        /// </summary>
        private EntityCollWrapper<Zone, DataModel.Zones> _refZones;

        /// <summary>
        /// Reference to the parent schedule.
        /// </summary>
        private EntityRefWrapper<Schedule, DataModel.Schedules> _refSchedule;
        /// <summary>
        /// Reference to the stops.
        /// </summary>
        private EntityCollWrapper<Stop, DataModel.Stops> _refStops;

        /// <summary>
        /// Work time window.
        /// </summary>
        private TimeWindow _timeWindow = new TimeWindow();
        /// <summary>
        /// Worked days.
        /// </summary>
        private Days _days = new Days();
        /// <summary>
        /// Breaks.
        /// </summary>
        private Breaks _breaks = new Breaks();

        /// <summary>
        /// Capacities.
        /// </summary>
        private Capacities _capacities;
        /// <summary>
        /// Capacities info.
        /// </summary>
        private CapacitiesInfo _capacitiesInfo;

        #endregion // Private members
    }
}
