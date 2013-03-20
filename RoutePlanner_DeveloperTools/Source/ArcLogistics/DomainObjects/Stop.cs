using System;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects.Attributes;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Utility;
using DataModel = ESRI.ArcLogistics.Data.DataModel;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Structure that represents a direction.
    /// </summary>
    public struct Direction
    {
        /// <summary>
        /// Direction length. 
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Direction time.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Direction text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Direction shape in compact format.
        /// </summary>
        /// <remarks>
        /// Use <c>CompactGeometryConverter</c> to get array of points from the string.
        /// </remarks>
        public string Geometry { get; set; }

        /// <summary>
        /// Type of direction maneuver.
        /// </summary>
        public StopManeuverType ManeuverType { get; set; }
    }

    /// <summary>
    /// Class that represents a stop. 
    /// </summary>
    /// <remarks>
    /// Stop can be either start/end/renewal location visit or order visit or break.
    /// </remarks>
    public class Stop : DataObject
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Stop</c> class.
        /// </summary>
        internal Stop()
            : base(DataModel.Stops.CreateStops(Guid.NewGuid()))
        {
        }

        internal Stop(DataModel.Stops entity)
            : base(entity)
        {
        }

        #endregion constructors

        #region public static properties
        /// <summary>
        /// Gets name of the IsLocked property.
        /// </summary>
        public static string PropertyNameIsLocked
        {
            get
            {
                return PROPERTY_NAME_ISLOCKED;
            }
        }

        /// <summary>
        /// Gets name of the MapLocation property.
        /// </summary>
        public static string PropertyNameMapLocation
        {
            get
            {
                return PROPERTY_NAME_MAPLOCATION;
            }
        }

        /// <summary>
        /// Gets name of the SequenceNumber property.
        /// </summary>
        public static string PropertyNameSequenceNumber
        {
            get
            {
                return PROPERTY_NAME_SEQUENCE_NUMBER;
            }
        }

        /// <summary>
        /// Gets name of the StopType property.
        /// </summary>
        public static string PropertyNameStopType
        {
            get
            {
                return PROPERTY_NAME_STOPTYPE;
            }
        }

        /// <summary>
        /// Gets TravelTime property name.
        /// </summary>
        public static string PropertyNameTravelTime
        {
            get
            {
                return PROPERTY_NAME_TRAVEL_TIME;
            }
        }

        /// <summary>
        /// Gets ArriveTime property name.
        /// </summary>
        public static string PropertyNameArriveTime
        {
            get
            {
                return PROPERTY_NAME_ARRIVE_TIME;
            }
        }

        /// <summary>
        /// Wait time property name.
        /// </summary>
        public static string PropertyNameWaitTime
        {
            get
            {
                return PROPERTY_NAME_WAIT_TIME;
            }
        }

        /// <summary>
        /// Gets TimeAtStop property name.
        /// </summary>
        public static string PropertyNameTimeAtStop
        {
            get
            {
                return PROPERTY_NAME_TIME_AT_STOP;
            }
        }

        /// <summary>
        /// Gets Status property name.
        /// </summary>
        public static string PropertyNameStatus
        {
            get
            {
                return PROPERTY_NAME_STATUS;
            }
        }

        #endregion

        #region public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets the object's type title.
        /// </summary>
        public override string TypeTitle
        {
            get { return Properties.Resources.Stop; }
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
        public override long? CreationTime
        {
            get
            {
                Debug.Assert(false); // NOTE: not supported
                return 0;
            }
            set { }
        }

        /// <summary>
        /// Gets either location or order stop name or "Lunch" for a stop with <c>Lunch</c> type.
        /// </summary>
        public override string Name
        {
            get
            {
                string name = null;
                switch (StopType)
                {
                    case StopType.Location:
                    case StopType.Order:
                        name = AssociatedObject.ToString();
                        break;

                    case StopType.Lunch:
                        name = Properties.Resources.Break;
                        break;

                    default:
                        Debug.Assert(false); // NOTE: not supported
                        break;
                }
                
                return name;
            }
            set
            {
                throw new NotSupportedException(Properties.Messages.Error_StopNameReadOnly);
            }
        }

        /// <summary>
        /// Stop arrive time.
        /// </summary>
        [DomainProperty("DomainPropertyNameArriveTime")]
        public DateTime? ArriveTime
        {
            get { return _Entity.ArriveTime; }
            internal set
            {
                _Entity.ArriveTime = value;
                NotifyPropertyChanged(PROPERTY_NAME_ARRIVE_TIME);
            }
        }

        /// <summary>
        /// Distance to the stop from previous stop.
        /// </summary>
        [DomainProperty("DomainPropertyNameDistance")]
        [UnitPropertyAttribute(Unit.Mile, Unit.Mile, Unit.Kilometer)]
        public double Distance
        {
            get { return _Entity.Distance; }
            internal set
            {
                _Entity.Distance = value;
                NotifyPropertyChanged(PROPERTY_NAME_DISTANCE);
            }
        }

        /// <summary>
        /// Stop's sequence number on the route.
        /// </summary>
        [DomainProperty("DomainPropertyNameSequenceNumber")]
        public int SequenceNumber
        {
            get { return _Entity.SequenceNumber; }
            internal set
            {
                _Entity.SequenceNumber = value;
                NotifyPropertyChanged(PROPERTY_NAME_SEQUENCE_NUMBER);
            }
        }

        /// <summary>
        /// Returns sequence number of order in case stop has <c>Order</c> type or <c>null</c> otherwise.
        /// </summary>
        [DomainProperty("DomainPropertyNameOrderSequenceNumber")]
        public int? OrderSequenceNumber
        {
            get { return _Entity.OrderSequenceNumber; }
            internal set
            {
                _Entity.OrderSequenceNumber = value;
                NotifyPropertyChanged(PROPERTY_NAME_ORDER_SEQUENCE_NUMBER);
            }
        }

        /// <summary>
        /// Type of the stop.
        /// </summary>
        [DomainProperty("DomainPropertyNameType")]
        public StopType StopType
        {
            get { return (StopType)_Entity.Type; }
            internal set
            {
                _Entity.Type = (int)value;
                NotifyPropertyChanged(PROPERTY_NAME_STOPTYPE);
            }
        }

        /// <summary>
        /// Time spent at stop in minutes.
        /// </summary>
        [DomainProperty("DomainPropertyNameTimeAtStop")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double TimeAtStop
        {
            get { return _Entity.TimeAtStop; }
            internal set
            {
                _Entity.TimeAtStop = value;
                NotifyPropertyChanged(PROPERTY_NAME_TIME_AT_STOP);
            }
        }

        /// <summary>
        /// Driving time to this stop from previous one in minutes. 
        /// </summary>
        /// <remarks>
        /// For the first stop this property value is 0. 
        /// </remarks>
        [DomainProperty("DomainPropertyNameTravelTime")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double TravelTime
        {
            get { return _Entity.TravelTime; }
            internal set
            {
                _Entity.TravelTime = value;
                NotifyPropertyChanged(PROPERTY_NAME_TRAVEL_TIME);
            }
        }

        /// <summary>
        /// Time that driver should spend waiting before servicing this stop.
        /// </summary>
        [DomainProperty("DomainPropertyNameWaitTime")]
        [UnitPropertyAttribute(Unit.Minute, Unit.Minute, Unit.Minute)]
        public double WaitTime
        {
            get { return _Entity.WaitTime; }
            internal set
            {
                _Entity.WaitTime = value;
                NotifyPropertyChanged(PROPERTY_NAME_WAIT_TIME);
            }
        }

        /// <summary>
        /// Indicates whether stop is locked. 
        /// </summary>
        /// <remarks>
        /// This property affects only stops with <c>Order</c> type. If order is locked it means that it cannot be unassigned or reassigned to another route.
        /// </remarks>
        [DomainProperty("DomainPropertyNameIsLocked")]
        public bool IsLocked
        {
            get { return _Entity.Locked; }
            set
            {
                _Entity.Locked = value;
                NotifyPropertyChanged(PROPERTY_NAME_ISLOCKED);
            }
        }

        /// <summary>
        /// Object associated with this stop.
        /// </summary>
        /// <remarks>
        /// If stop type is <c>Order</c> then this property returns corresponding <c>Order</c> object.
        /// If stop type is <c>Location</c> then this property returns corresponding <c>Location</c> object.
        /// If stop type is <c>Lunch</c> then this property returns null.
        /// </remarks>
        public DataObject AssociatedObject
        {
            get
            {
                DataObject obj = null;
                if (this.StopType == StopType.Order)
                    obj = _OrderWrap.Value;
                else if (this.StopType == StopType.Location)
                    obj = _LocationWrap.Value;

                return obj;
            }
            internal set
            {
                if (this.StopType == StopType.Order)
                    _OrderWrap.Value = value as Order;
                else if (this.StopType == StopType.Location)
                    _LocationWrap.Value = value as Location;
            }
        }

        /// <summary>
        /// Gets stop location on a map.
        /// </summary>
        [DomainProperty("DomainPropertyNameMapLocation")]
        public Point? MapLocation
        {
            get
            {
                Point? pt = null;
                if (this.StopType == StopType.Order &&
                    _OrderWrap.Value != null)
                {
                    pt = _OrderWrap.Value.GeoLocation;
                }
                else if (this.StopType == StopType.Location &&
                    _LocationWrap.Value != null)
                {
                    pt = _LocationWrap.Value.GeoLocation;
                }

                return pt;
            }
        }

        /// <summary>
        /// Gets stop parent route.
        /// </summary>
        public Route Route
        {
            get { return _RouteWrap.Value; }
        }

        /// <summary>
        /// Indicates whether stop's time window(s) are violated.
        /// </summary>
        public bool IsViolated
        {
            get
            {
                bool isViolated = false;

                // If stop isn't assigned to route it cannot be violated.
                if (Route != null)
                {
                    switch (StopType)
                    {
                        case StopType.Location:
                            Debug.Assert(AssociatedObject is Location);
                            isViolated = _IsLocationTimeWindowViolated((Location)AssociatedObject);
                        break;

                        case StopType.Order:
                            Debug.Assert(AssociatedObject is Order);
                            isViolated = _IsOrderTimeWindowViolated((Order)AssociatedObject);
                            break;

                        case StopType.Lunch:
                            isViolated = false;
                            break;

                        default:
                            Debug.Assert(false); // NOTE: not supported
                            break;
                    } // switch (StopType)
                } // if (Route != null)
                else
                {
                    isViolated = false;
                }
                
                return isViolated;
            }
        }

        /// <summary>
        /// Gets path to this stop from previous one.
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> for the first stop.
        /// </remarks>
        public Polyline Path
        {
            get
            {
                if (_path == null)
                {
                    if (_Entity.PathTo != null)
                        _path = new Polyline(_Entity.PathTo);
                }
                return _path;
            }
            internal set
            {
                _Entity.PathTo = (value != null ? value.ToByteArray() : null);
                _path = value;
            }
        }

        /// <summary>
        /// Gets array of driving directions to this stop from previous one.
        /// </summary>
        /// <remarks>
        /// Returns <c>null</c> or the first stop.
        /// </remarks>
        public Direction[] Directions
        {
            get
            {
                if (_directions == null)
                {
                    if (_Entity.Directions != null)
                    {
                        _directions = DirectionsHelper.ConvertFromBytes(
                            _Entity.Directions);
                    }
                }
                return _directions;
            }
            internal set
            {
                _Entity.Directions = (value != null ?
                    DirectionsHelper.ConvertToBytes(value) : null);

                _directions = value;
            }
        }

        #endregion public members

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns the name of the stop.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }

        internal static Stop CreateFromData(StopData stopData)
        {
            Debug.Assert(stopData != null);

            Stop stop = new Stop();
            stop.Distance = stopData.Distance;
            stop.SequenceNumber = stopData.SequenceNumber;
            stop.WaitTime = stopData.WaitTime;
            stop.TimeAtStop = stopData.TimeAtStop;
            stop.TravelTime = stopData.TravelTime;
            stop.ArriveTime = stopData.ArriveTime;
            stop.StopType = stopData.StopType;
            stop.AssociatedObject = stopData.AssociatedObject;
            stop.OrderSequenceNumber = stopData.OrderSequenceNumber;
            stop.Path = stopData.Path;
            stop.Directions = stopData.Directions;
            stop.IsLocked = stopData.IsLocked;

            return stop;
        }

        internal StopData GetData()
        {
            StopData stopData = new StopData();
            stopData.Distance = this.Distance;
            stopData.SequenceNumber = this.SequenceNumber;
            stopData.WaitTime = this.WaitTime;
            stopData.TimeAtStop = this.TimeAtStop;
            stopData.TravelTime = this.TravelTime;
            stopData.ArriveTime = this.ArriveTime != null ?
                (DateTime)this.ArriveTime : DateTime.MinValue;
            stopData.StopType = this.StopType;
            stopData.AssociatedObject = this.AssociatedObject;
            stopData.OrderSequenceNumber = this.OrderSequenceNumber;
            stopData.Path = this.Path;
            stopData.Directions = this.Directions;
            stopData.IsLocked = this.IsLocked;

            return stopData;
        }

        #endregion public methods

        #region ICloneable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            Stop obj = new Stop();
            obj.ArriveTime = this.ArriveTime;
            obj.Distance = this.Distance;
            obj.SequenceNumber = this.SequenceNumber;
            obj.OrderSequenceNumber = this.OrderSequenceNumber;
            obj.StopType = this.StopType;
            obj.TimeAtStop = this.TimeAtStop;
            obj.TravelTime = this.TravelTime;
            obj.WaitTime = this.WaitTime;
            obj.IsLocked = this.IsLocked;
            obj.AssociatedObject = this.AssociatedObject;

            if (this.Path != null)
                obj.Path = (Polyline)this.Path.Clone();

            if (this.Directions != null)
            {
                List<Direction> dirs = new List<Direction>();
                foreach (Direction dir in this.Directions)
                    dirs.Add(dir);

                obj.Directions = dirs.ToArray();
            }

            return obj;
        }

        #endregion ICloneable interface members

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks if stop's arrive date time violates given time windows.
        /// </summary>
        /// <param name="timeWindow1">First time window.</param>
        /// <param name="timeWindow2">Second time window.</param>
        /// <returns>True - if both time windows violated, false - otherwise.</returns>
        private bool _AreTimeWindowsViolated(TimeWindow timeWindow1, TimeWindow timeWindow2)
        {
            Debug.Assert(timeWindow1 != null);
            Debug.Assert(timeWindow2 != null);

            // Violation check result.
            bool isViolated = false;

            // Location's time window is violated when 1-st and 2-d time window is violated.
            if (ArriveTime.HasValue && Route.Schedule.PlannedDate.HasValue)
            {
                DateTime arriveDateTime = ArriveTime.Value;
                DateTime plannedDateTime = Route.Schedule.PlannedDate.Value;

                isViolated = !timeWindow1.DoesIncludeTime(arriveDateTime, plannedDateTime) &&
                             !timeWindow2.DoesIncludeTime(arriveDateTime, plannedDateTime);
            }
            else
            {
                isViolated = false;
            }

            return isViolated;
        }

        /// <summary>
        /// Checks if location's time window is violated.
        /// </summary>
        /// <param name="location">Location object.</param>
        /// <returns>True - if location's time window is violated, false - otherwise.</returns>
        private bool _IsLocationTimeWindowViolated(Location location)
        {
            Debug.Assert(location != null);

            // Violation check result.
            bool isViolated = _AreTimeWindowsViolated(location.TimeWindow, location.TimeWindow2);

            return isViolated;
        }

        /// <summary>
        /// Checks if order's time window is violated.
        /// </summary>
        /// <param name="order">Order object.</param>
        /// <returns>True - if order's time window is violated, false - otherwise.</returns>
        private bool _IsOrderTimeWindowViolated(Order order)
        {
            Debug.Assert(order != null);

            // Violation check result.
            bool isViolated = _AreTimeWindowsViolated(order.TimeWindow, order.TimeWindow2);

            return isViolated;
        }

        #endregion // private methods

        #region private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private DataModel.Stops _Entity
        {
            get
            {
                return (DataModel.Stops)base.RawEntity;
            }
        }

        private EntityRefWrapper<Location, DataModel.Locations> _LocationWrap
        {
            get
            {
                if (_locationRef == null)
                {
                    _locationRef = new EntityRefWrapper<Location,
                        DataModel.Locations>(_Entity.LocationsReference, this);
                }

                return _locationRef;
            }
        }

        private EntityRefWrapper<Order, DataModel.Orders> _OrderWrap
        {
            get
            {
                if (_orderRef == null)
                {
                    _orderRef = new EntityRefWrapper<Order,
                        DataModel.Orders>(_Entity.OrdersReference, this);
                }

                return _orderRef;
            }
        }

        private EntityRefWrapper<Route, DataModel.Routes> _RouteWrap
        {
            get
            {
                if (_routeRef == null)
                {
                    _routeRef = new EntityRefWrapper<Route,
                        DataModel.Routes>(_Entity.RoutesReference, this);
                }

                return _routeRef;
            }
        }

        #endregion private properties

        #region private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // property names
        private const string PROPERTY_NAME_ISLOCKED = "IsLocked";
        private const string PROPERTY_NAME_DISTANCE = "Distance";
        private const string PROPERTY_NAME_STATUS = "Status";
        private const string PROPERTY_NAME_WAIT_TIME = "WaitTime";
        private const string PROPERTY_NAME_TIME_AT_STOP = "TimeAtStop";
        private const string PROPERTY_NAME_TRAVEL_TIME = "TravelTime";
        private const string PROPERTY_NAME_MAPLOCATION = "MapLocation";
        private const string PROPERTY_NAME_SEQUENCE_NUMBER = "SequenceNumber";
        private const string PROPERTY_NAME_ORDER_SEQUENCE_NUMBER = "OrderSequenceNumber";
        private const string PROPERTY_NAME_STOPTYPE = "StopType";
        private const string PROPERTY_NAME_ARRIVE_TIME = "ArriveTime";
        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private EntityRefWrapper<Location, DataModel.Locations> _locationRef;
        private EntityRefWrapper<Order, DataModel.Orders> _orderRef;
        private EntityRefWrapper<Route, DataModel.Routes> _routeRef;

        private Direction[] _directions;
        private Polyline _path;

        #endregion private members
    }
}
