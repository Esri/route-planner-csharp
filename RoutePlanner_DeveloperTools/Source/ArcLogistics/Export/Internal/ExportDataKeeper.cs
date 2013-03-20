using System;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Data wrapper structure.
    /// </summary>
    internal struct DataWrapper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>DataWrapper</c>.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="type">Type.</param>
        public DataWrapper(object value, OleDbType type)
        {
            _value = value;
            _type = type;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Value.
        /// </summary>
        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// OleDb type.
        /// </summary>
        public OleDbType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Value.
        /// </summary>
        private object _value;
        /// <summary>
        /// Type.
        /// </summary>
        private OleDbType _type;

        #endregion // Private members
    }

    /// <summary>
    /// Data keeper class represents methods to getting value object field.
    /// </summary>
    internal sealed class DataKeeper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>DataKeeper</c> class.
        /// </summary>
        /// <param name="listSeparator">List separator.</param>
        /// <param name="tableDescription">Export table description.</param>
        public DataKeeper(string listSeparator, TableDescription tableDescription)
        {
            Debug.Assert(null != listSeparator);
            Debug.Assert(null != tableDescription);

            _listSeparator = listSeparator;
            _tableDescription = tableDescription;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets schedule field value.
        /// </summary>
        /// <param name="field">Field name.</param>
        /// <param name="schedule">Schedule object for initialization data.</param>
        /// <returns>Readed data wrapper.</returns>
        public DataWrapper GetScheduleFieldValue(string field, Schedule schedule)
        {
            Debug.Assert(TableType.Schedules == _tableDescription.Type);

            Debug.Assert(null != schedule);
            FieldInfo info = _GetFieldInfo(field);

            DataWrapper value = new DataWrapper(null, info.Type);
            switch (field)
            {
                case "ID":
                    value.Value = schedule.Id;
                    break;

                case "PlannedDate":
                    value.Value = _DateTimeToDate(schedule.PlannedDate);
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return value;
        }

        /// <summary>
        /// Gets route field value.
        /// </summary>
        /// <param name="field">Field name.</param>
        /// <param name="scheduleId">Schedule ID.</param>
        /// <param name="route">Route object for initialization data.</param>
        /// <returns>Readed data wrapper.</returns>
        public DataWrapper GetRouteFieldValue(string field, Guid scheduleId, Route route)
        {
            Debug.Assert(TableType.Routes == _tableDescription.Type);

            Debug.Assert(null != route);
            Debug.Assert(!string.IsNullOrEmpty(field));

            FieldInfo info = _GetFieldInfo(field);

            var value = new DataWrapper(null, info.Type);
            if (!string.IsNullOrEmpty(info.RelationType))
                value.Value = _GetRouteSpecialFieldValue(info, route);

            else
            {
                switch (field)
                {
                    case "OverviewMap":
                        Debug.Assert(false);
                            // NOTE: not supported - special routine in AccessExporter
                        break;

                    case "ID":
                        value.Value = route.Id;
                        break;

                    case "ScheduleID":
                        value.Value = scheduleId;
                        break;

                    case "Name":
                        value.Value = route.Name;
                        break;

                    case "VehicleName":
                        value.Value = route.Vehicle.Name;
                        break;

                    case "DriverName":
                        value.Value = route.Driver.Name;
                        break;

                    case "StartLocationName":
                        value.Value = (null == route.StartLocation) ? string.Empty : route.StartLocation.Name;
                        break;

                    case "EndLocationName":
                        value.Value = (null == route.EndLocation) ? string.Empty : route.EndLocation.Name;
                        break;

                    case "RenewalLocations":
                        value.Value = _CreateNameList(route.RenewalLocations, _listSeparator);
                        break;

                    case "{0}Capacity":
                        Debug.Assert(false);
                        break;

                    case "StartTWDay":
                        value.Value = route.StartTimeWindow.Day;
                        break;

                    case "StartTWFrom":
                        value.Value = _ConvertTimeToMinutes(route.StartTimeWindow.From);
                        break;

                    case "StartTWFromString":
                        value.Value = _TimeToString(route.StartTimeWindow.From);
                        break;

                    case "StartTWTo":
                        value.Value = _ConvertTimeToMinutes(route.StartTimeWindow.To);
                        break;

                    case "StartTWToString":
                        value.Value = _TimeToString(route.StartTimeWindow.To);
                        break;

                    case "FixedCost":
                        value.Value = route.Vehicle.FixedCost + route.Driver.FixedCost;
                        break;

                    case "CostPerMile":
                        value.Value = _CalculateCostPerMile(route.Vehicle);
                        break;

                    case "CostPerKM":
                        value.Value =
                            _CalculateCostPerMile(route.Vehicle) / SolverConst.KM_PER_MILE;
                        break;

                    case "CostPerHour":
                        value.Value = route.Driver.PerHourSalary;
                        break;

                    case "CostPerHourOT":
                        value.Value = route.Driver.PerHourOTSalary;
                        break;

                    case "TimeBeforeOT":
                        value.Value = route.Driver.TimeBeforeOT;
                        break;

                    case "FuelType":
                        value.Value = route.Vehicle.FuelType.Name;
                        break;

                    case "FuelEconomy":
                        value.Value = route.Vehicle.FuelEconomy;
                        break;

                    case "CO2Emission":
                        value.Value = route.Vehicle.FuelType.Co2Emission;
                        break;

                    case "Breaks":
                        {
                            value.Value = route.Breaks.AssemblyExportString();
                            break;
                        }

                    case "MaxOrders":
                        value.Value = route.MaxOrders;
                        break;

                    case "MaxTotalDuration":
                        value.Value = route.MaxTotalDuration;
                        break;

                    case "MaxTravelDuration":
                        value.Value = route.MaxTravelDuration;
                        break;

                    case "MaxTravelMiles":
                        value.Value = route.MaxTravelDistance;
                        break;

                    case "MaxTravelKM":
                        value.Value = route.MaxTravelDistance * SolverConst.KM_PER_MILE;
                        break;

                    case "VehicleSpecialties":
                        value.Value = _CreateNameList(route.Vehicle.Specialties, _listSeparator);
                        break;

                    case "DriverSpecialties":
                        value.Value = _CreateNameList(route.Driver.Specialties, _listSeparator);
                        break;

                    case "Zones":
                        value.Value = _CreateNameList(route.Zones, _listSeparator);
                        break;

                    case "Comments":
                        value.Value = route.Comment;
                        break;

                    case "StartDate":
                        value.Value = _DateTimeToDate(route.StartTime);
                        break;

                    case "StartTime":
                        value.Value = _ConvertDateToMinutes(route.StartTime);
                        break;

                    case "StartTimeString":
                        value.Value = _DateTimeToTimeString(route.StartTime);
                        break;

                    case "EndDate":
                        value.Value = _DateTimeToDate(route.EndTime);
                        break;

                    case "EndTime":
                        value.Value = _ConvertDateToMinutes(route.EndTime);
                        break;

                    case "EndTimeString":
                        value.Value = _DateTimeToTimeString(route.EndTime);
                        break;

                    case "RouteTimeString":
                        value.Value = string.Format(ROUTE_TIME_STRING_FORMAT,
                            _DateTimeToTimeString(route.StartTime),
                            _DateTimeToTimeString(route.EndTime));
                        break;

                    case "TotalStops":
                        value.Value = route.Stops.Count;
                        break;

                    case "TotalOrders":
                        value.Value = route.OrderCount;
                        break;

                    case "TotalServiceTime":
                        value.Value = route.TotalServiceTime;
                        break;

                    case "TotalTravelTime":
                        value.Value = route.TravelTime;
                        break;

                    case "TotalWaitTime":
                        value.Value = route.WaitTime;
                        break;

                    case "TotalTime":
                        value.Value = route.TotalTime;
                        break;

                    case "TotalOT":
                        value.Value = route.Overtime;
                        break;

                    case "TotalCost":
                        value.Value = route.Cost;
                        break;

                    case "TotalMiles":
                        value.Value = route.TotalDistance;
                        break;

                    case "TotalKM":
                        value.Value = route.TotalDistance * SolverConst.KM_PER_MILE;
                        break;

                    case "TotalMilesPerStop":
                        value.Value =
                            _CalculateDistancePerStop(route.TotalDistance, route.Stops.Count);
                        break;

                    case "TotalKMPerStop":
                        {
                            double totalDistanceKM = route.TotalDistance * SolverConst.KM_PER_MILE;
                            value.Value =
                                _CalculateDistancePerStop(totalDistanceKM, route.Stops.Count);
                            break;
                        }

                    case "TotalCO2Emission":
                        {
                            double totalEmission =
                                route.Vehicle.FuelType.Co2Emission * route.TotalDistance;
                            value.Value = (0 == route.Vehicle.FuelEconomy) ? 0 :
                                            totalEmission / route.Vehicle.FuelEconomy;
                            break;
                        }

                    case "TotalViolations":
                        value.Value = route.ViolatedStopCount;
                        break;

                    case "TotalViolationTime":
                        value.Value = route.ViolationTime;
                        break;

                    case "TotalRuns":
                        value.Value = route.RunCount;
                        break;

                    case "Total{0}":
                        Debug.Assert(false);
                        break;

                    case "TimeUtilization":
                        value.Value = _CalculateTimeUtilization(route);
                        break;

                    case "{0}Utilization":
                        Debug.Assert(false);
                        break;

                    default:
                        Debug.Assert(false); // NOTE: not supported
                        break;
                }
            }

            return value;
        }

        /// <summary>
        /// Gets stop field value.
        /// </summary>
        /// <param name="field">Field name.</param>
        /// <param name="scheduleId">Schedule ID.</param>
        /// <param name="obj">Stop/order object for initialization data.</param>
        /// <returns>Readed data wrapper.</returns>
        public DataWrapper GetStopFieldValue(string field, Guid scheduleId, DataObject obj)
        {
            Debug.Assert((TableType.Stops == _tableDescription.Type) ||
                         (TableType.Orders == _tableDescription.Type));

            Debug.Assert(!string.IsNullOrEmpty(field));
            Debug.Assert(null != obj);

            FieldInfo info = _GetFieldInfo(field);
            var stop = obj as Stop;
            DataObject workObject = (null == stop)? obj : stop.AssociatedObject;

            var value = new DataWrapper(null, info.Type);
            if (!string.IsNullOrEmpty(info.RelationType))
                value.Value = _GetStopSpecialFieldValue(info, workObject);

            else
            {
                switch (field)
                {
                    case "StopVicinityMap":
                    case "Directions":
                        // NOTE: not supported - special routine in AccessExporter
                        break;

                    case "{0}":
                        Debug.Assert(false);
                        break;

                    case "ScheduleID":
                        value.Value = scheduleId;
                        break;

                    case "Name":
                        value.Value = _GetName(stop, workObject as Order);
                        break;

                    case "ServiceTime":
                        value.Value = _GetServiceTime(stop, workObject as Order);
                        break;

                    case "StopID":
                        value.Value = (null != stop)? stop.Id : workObject.Id;
                        break;

                    case "OrderID":
                        Debug.Assert(workObject is Order);
                        value.Value = workObject.Id;
                        break;

                    case "FullAddress":
                    case "FullAddressShort":
                    case "Confidence":
                    case "X":
                    case "Y":
                    case "PlannedDate":
                    case "OrderType":
                    case "OrderTypeString":
                    case "Priority":
                    case "PriorityString":
                    case "TWDay":
                    case "TWFrom":
                    case "TWTo":
                    case "TW2Day":
                    case "TW2From":
                    case "TW2To":
                    case "TWFromString":
                    case "TWToString":
                    case "TWFrom2String":
                    case "TWTo2String":
                    case "TWString":
                    case "TW2String":
                    case "MaxViolationTime":
                    case "DriverSpecialties":
                    case "VehicleSpecialties":
                        value.Value = _GetObjectValue(field, workObject);
                        break;

                    case "RouteID":
                    case "RouteName":
                    case "DriverName":
                    case "VehicleName":
                    case "StopType":
                    case "StopTypeString":
                    case "StopTypeExString":
                    case "Sequence":
                    case "OrderSequence":
                    case "TravelTime":
                    case "WaitTime":
                    case "ArriveTime":
                    case "ArriveTimeString":
                    case "ArriveDate":
                    case "DistanceFromPrevious":
                    case "LoadAtID":
                    case "StopNamePrefix":
                    case "StopNamePostfix":
                        value.Value = _GetStopValue(field, stop);
                        break;

                    default:
                        Debug.Assert(false); // NOTE: not supported
                        break;
                }
            }

            return value;
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates name list.
        /// </summary>
        /// <typeparam name="T">DataObject type.</typeparam>
        /// <param name="collection">Application collection of object.</param>
        /// <param name="listSeparator">List separator.</param>
        /// <returns>String with objects name list.</returns>
        private static string _CreateNameList<T>(IDataObjectCollection<T> collection,
                                                 string listSeparator)
            where T : DataObject
        {
            var sb = new StringBuilder();
            foreach (T obj in collection)
            {
                if (!string.IsNullOrEmpty(sb.ToString()))
                    sb.Append(listSeparator);

                sb.Append(obj.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts time to minutes.
        /// </summary>
        /// <param name="time">Time to conversion.</param>
        /// <returns>Time in minutes.</returns>
        private double _ConvertTimeToMinutes(TimeSpan time)
        {
            return 60.0 * time.Hours + time.Minutes + (double)time.Seconds / 60.0;
        }

        /// <summary>
        /// Converts date to minutes.
        /// </summary>
        /// <param name="date">Date to conversions.</param>
        /// <returns>Date in minutes.</returns>
        private double _ConvertDateToMinutes(DateTime date)
        {
            // NOTE: from the beginning of the day
            return (null == date)? 0 : _ConvertTimeToMinutes(date.TimeOfDay);
        }

        /// <summary>
        /// Converts date to minutes.
        /// </summary>
        /// <param name="date">Date to conversions.</param>
        /// <returns>Date in minutes.</returns>
        private double _ConvertDateToMinutes(DateTime? date)
        {
            return (date.HasValue) ? _ConvertDateToMinutes(date.Value) : 0;
        }

        /// <summary>
        /// Calculate cost per mile.
        /// </summary>
        /// <param name="vehicle">Vehicle object.</param>
        /// <returns>Cost per mile.</returns>
        private double _CalculateCostPerMile(Vehicle vehicle)
        {
            return (0 == vehicle.FuelEconomy) ? 0 : vehicle.FuelType.Price / vehicle.FuelEconomy;
        }

        /// <summary>
        /// Converts time to string.
        /// </summary>
        /// <param name="time">Time to conversion.</param>
        /// <returns>Time formated string.</returns>
        private string _TimeToString(TimeSpan time)
        {
            var dateTime = new DateTime(time.Ticks);
            return _DateTimeToTimeString(dateTime);
        }

        /// <summary>
        /// Converts DateTime to string.
        /// </summary>
        /// <param name="dateTime">DateTime to conversion.</param>
        /// <returns>Time formated string.</returns>
        private string _DateTimeToTimeString(DateTime? dateTime)
        {
            string time = string.Empty;
            if (dateTime.HasValue)
            {
                DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
                time = dateTime.Value.ToString(dateTimeFormat.ShortTimePattern);
            }

            return time;
        }

        /// <summary>
        /// Converts DateTime to date.
        /// </summary>
        /// <param name="dateTime">DateTime to conversion.</param>
        /// <returns>Date formated string.</returns>
        private DateTime? _DateTimeToDate(DateTime? dateTime)
        {
            DateTime? date = null;
            if (dateTime.HasValue)
                date = dateTime.Value.Date;

            return date;
        }

        /// <summary>
        /// Calculates time utilization.
        /// </summary>
        /// <param name="route">Route objct to calculation.</param>
        /// <returns>Time utilization [in percent].</returns>
        private double _CalculateTimeUtilization(Route route)
        {
            double timeUtilization = 0;
            if (0 < route.TotalTime)
                timeUtilization = (route.TotalTime - route.WaitTime) / route.TotalTime * 100;
            return timeUtilization;
        }

        /// <summary>
        /// Calculates distance per stop.
        /// </summary>
        /// <param name="distance">Total route distance.</param>
        /// <param name="stopCount">Route stop count.</param>
        /// <returns>Route distance per stop.</returns>
        private double _CalculateDistancePerStop(double distance, int stopCount)
        {
            double distancePerStop = distance;
            if (0 < stopCount)
                distancePerStop /= stopCount;

            return distancePerStop;
        }

        /// <summary>
        /// Gets service time.
        /// </summary>
        /// <param name="stop">Stop object (can be null).</param>
        /// <param name="order">Order object (can be null).</param>
        /// <returns>Service time.</returns>
        /// <remarks>Needed one object - stop or order.</remarks>
        private double _GetServiceTime(Stop stop, Order order)
        {
            Debug.Assert((null != stop) || (null != order));

            double result = 0;
            if (null != stop)
                result = stop.TimeAtStop;
            else
            {
                Debug.Assert(null != order);
                result = order.ServiceTime;
            }

            return result;
        }

        /// <summary>
        /// Gets name.
        /// </summary>
        /// <param name="stop">Stop object (can be null).</param>
        /// <param name="order">Order object (can be null).</param>
        /// <returns>Name.</returns>
        /// <remarks>Needed one object - stop or order.</remarks>
        private string _GetName(Stop stop, Order order)
        {
            Debug.Assert((null != stop) || (null != order));

            string name = string.Empty;
            if (null != stop)
                name = _GetStopName(stop);
            else
            {
                Debug.Assert(null != order);
                name = order.Name;
            }

            return name;
        }

        /// <summary>
        /// Gets stop name.
        /// </summary>
        /// <param name="stop">Stop object.</param>
        /// <returns>Name.</returns>
        private string _GetStopName(Stop stop)
        {
            string name = string.Empty;
            if (StopType.Lunch == stop.StopType)
                name = Properties.Resources.Break;
            else
            {
                Debug.Assert(null != stop.AssociatedObject);
                name = stop.AssociatedObject.ToString();
            }

            return name;
        }

        /// <summary>
        /// Gets geocodable interface.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Interface for geocoding or null if not supported.</returns>
        private IGeocodable _GetGeocodable(DataObject obj)
        {
            IGeocodable geocodable = null;
            if (null != obj)
            {
                geocodable = obj as IGeocodable;
                Debug.Assert(null != geocodable);
            }

            return geocodable;
        }

        /// <summary>
        /// Gets address.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Address object or null if not supported.</returns>
        private Address _GetAddress(DataObject obj)
        {
            Address address = null;

            IGeocodable geocodable = _GetGeocodable(obj);
            if (null != geocodable)
                address = geocodable.Address;

            return address;
        }

        /// <summary>
        /// Gets address fields.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="partName">Address part name.</param>
        /// <returns>Address field name.</returns>
        private string _GetAddressFields(DataObject obj, string partName)
        {
            string addressPart = string.Empty;
            Address address = _GetAddress(obj);
            if (null != address)
            {
                AddressPart part = (AddressPart)Enum.Parse(typeof(AddressPart), partName);
                addressPart = address[part];
            }

            return addressPart;
        }

        /// <summary>
        /// Gets full address.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Full address or empty string if not supported.</returns>
        private string _GetFullAddress(DataObject obj)
        {
            string fullAddress = string.Empty;
            Address address = _GetAddress(obj);
            if (null != address)
                fullAddress = address.FullAddress;

            return fullAddress;
        }

        /// <summary>
        /// Gets confidence.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Confidence string or empty string if not supported.</returns>
        private string _GetConfidence(DataObject obj)
        {
            string matchMethod = string.Empty;
            Address address = _GetAddress(obj);
            if (null != address)
                matchMethod = address.MatchMethod;

            return matchMethod;
        }

        /// <summary>
        /// Gets geolocation.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <returns>Point object or empty point if not supported.</returns>
        private Point _GetGeoLocation(DataObject obj)
        {
            var pt = new Point(0, 0);
            IGeocodable geocodable = _GetGeocodable(obj);
            if ((null != geocodable) && (geocodable.GeoLocation.HasValue))
                pt = geocodable.GeoLocation.Value;

            return pt;
        }

        /// <summary>
        /// Adds address part to full address.
        /// </summary>
        /// <param name="addressPart">Address part.</param>
        /// <param name="address">Address.</param>
        /// <returns>Address with added value.</returns>
        private string _AddAddressPart(string addressPart, string address)
        {
            var sb = new StringBuilder(address);
            if (!string.IsNullOrEmpty(addressPart))
            {
                if (!string.IsNullOrEmpty(address))
                    sb.Append(DELIMETER);

                sb.Append(addressPart);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets shorted full address from address fields (without PostalCode1,PostalCode2,Country).
        /// </summary>
        /// <param name="obj">Object.</param>
        private string _GetShorterFullAddress(DataObject obj)
        {
            string result = string.Empty;
            Address address = _GetAddress(obj);
            if (null != address)
            {
                result = _AddAddressPart(address[AddressPart.Unit], result);
                result = _AddAddressPart(address[AddressPart.AddressLine], result);
                result = _AddAddressPart(address[AddressPart.Locality1], result);
                result = _AddAddressPart(address[AddressPart.Locality2], result);
                result = _AddAddressPart(address[AddressPart.Locality3], result);
                result = _AddAddressPart(address[AddressPart.CountyPrefecture], result);
                result = _AddAddressPart(address[AddressPart.StateProvince], result);
            }

            return result;
        }

        /// <summary>
        /// Adaptes value to export format.
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <returns>Empty string or real value string.</returns>
        private string _AdapterNullableObjectToString(object value)
        {
            return (null == value) ? string.Empty : value.ToString();
        }

        /// <summary>
        /// Converts to order.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>Order object or null.</returns>
        private Order _ConvertToOrder(DataObject obj)
        {
            return obj as Order;
        }

        /// <summary>
        /// Converts to location.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>Location object or null.</returns>
        private Location _ConvertToLocation(DataObject obj)
        {
            return obj as Location;
        }

        /// <summary>
        /// Gets planned date.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>DateTime or null if not supported.</returns>
        private DateTime? _GetPlannedDate(DataObject obj)
        {
            DateTime? plannedDate = null;
            if (null != obj)
            {
                Order order = _ConvertToOrder(obj);
                if (null != order)
                    plannedDate = order.PlannedDate;
            }

            return plannedDate;
        }

        /// <summary>
        /// Gets order type.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>OrderType or null if not supported.</returns>
        private OrderType? _GetOrderType(DataObject obj)
        {
            OrderType? type = null;
            Order order = _ConvertToOrder(obj);
            if (null != order)
                type = order.Type;

            return type;
        }

        /// <summary>
        /// Gets order priority.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>OrderPriority or null if not supported.</returns>
        private OrderPriority? _GetOrderPriority(DataObject obj)
        {
            OrderPriority? priority = null;
            Order order = _ConvertToOrder(obj);
            if (null != order)
                priority = order.Priority;

            return priority;
        }

        /// <summary>
        /// Gets Order's / Location's time window.
        /// </summary>
        /// <param name="obj">Input object (expected Order or Location).</param>
        /// <param name="isFirstTimeWindow">If true - 1-st time window, otherwise - 2-nd time window.</param>
        /// <returns>Time window object (for Order.TW or TW2, for Location TW).</returns>
        private TimeWindow _GetOrderTimeWindow(DataObject obj, bool isFirstTimeWindow)
        {
            TimeWindow timeWindow = null;

            // Try to convert object to Order.
            Order order = _ConvertToOrder(obj);

            // Try to convert object to Location.
            Location location = _ConvertToLocation(obj);

            if (null != order)
            {
                timeWindow = (isFirstTimeWindow) ? order.TimeWindow : order.TimeWindow2;
            }
            else if (location != null)
            {
                timeWindow = (isFirstTimeWindow) ? location.TimeWindow : location.TimeWindow2;
            }
            else
            {
                // Object should be Order or Location.
                Debug.Assert(false);
            }

            return timeWindow;
        }

        /// <summary>
        /// Gets order time window value.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <param name="isFirstTimeWindow">If true - 1-st time window, otherwise - 2-nd time window.</param>
        /// <param name="isFrom">Flag - is part of TW is From</param>
        /// <returns>From or To value for selected TW.</returns>
        private TimeSpan? _GetOrderTimeWindowValue(DataObject obj, bool isFirstTimeWindow, bool isFrom)
        {
            TimeSpan? time = null;
            TimeWindow tw = _GetOrderTimeWindow(obj, isFirstTimeWindow);
            if (null != tw)
                time = (isFrom) ? tw.From : tw.To;

            return time;
        }

        /// <summary>
        /// Gets max violation time.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <returns>Max violation time.</returns>
        private double _GetMaxViolationTime(DataObject obj)
        {
            double maxViolationTime = 0;
            Order order = _ConvertToOrder(obj);
            if (null != order)
                maxViolationTime = order.MaxViolationTime;

            return maxViolationTime;
        }

        /// <summary>
        /// Get specialities as string.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <param name="needDriverSpecialities">Need driver specialities flag.</param>
        /// <returns>Specialities list string.</returns>
        private string _GetSpecialities(DataObject obj, bool needDriverSpecialities)
        {
            string specialities = null;
            Order order = _ConvertToOrder(obj);
            if (null != order)
            {
                specialities = (needDriverSpecialities) ?
                                    _CreateNameList(order.DriverSpecialties, _listSeparator) :
                                    _CreateNameList(order.VehicleSpecialties, _listSeparator);
            }

            return specialities;
        }

        /// <summary>
        /// Gets load at ID object.
        /// </summary>
        /// <param name="stop">Stop object.</param>
        /// <returns>Load at ID object.</returns>
        private Guid? _GetLoadAtId(Stop stop)
        {
            Guid? id = null;

            // found location previously for this stop
            Route route = stop.Route;
            Order order = _ConvertToOrder(stop.AssociatedObject);
            if ((null != order) && (null != route))
            {
                foreach (Stop renevalStop in route.Stops)
                {
                    if (StopType.Location == renevalStop.StopType)
                        id = _ConvertToLocation(renevalStop.AssociatedObject).Id;

                    if (stop == renevalStop)
                        break;
                }
            }

            return id;
        }

        /// <summary>
        /// Gets stop name prefix.
        /// </summary>
        /// <param name="stop">Stop object.</param>
        /// <returns>Stop name prefix.</returns>
        private string _GetStopTypeEx(Stop stop)
        {
            string result = string.Empty;
            switch (stop.StopType)
            {
                case StopType.Location:
                    {
                        if (stop.SequenceNumber == 1)
                            result = STOP_TYPE_EX_START_LOCATION;
                        else if (stop.SequenceNumber == stop.Route.Stops.Count)
                            result = STOP_TYPE_EX_FINISH_LOCATION;
                        else
                            result = STOP_TYPE_EX_RENEWAL_LOCATION;
                        break;
                    }

                case StopType.Lunch:
                    result = STOP_TYPE_EX_LUNCH;
                    break;

                case StopType.Order:
                    result = STOP_TYPE_EX_STOP;
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets stop name prefix.
        /// </summary>
        /// <param name="stop">Stop object.</param>
        /// <returns>Stop name prefix.</returns>
        private string _GetStopNamePrefix(Stop stop)
        {
            string result = string.Empty;
            switch (stop.StopType)
            {
                case StopType.Location:
                    {
                        if (stop.SequenceNumber == 1)
                            result = Properties.Resources.StopNamePrefixStartLocation;
                        else if (stop.SequenceNumber < stop.Route.Stops.Count)
                            result = Properties.Resources.StopNamePrefixRenewalLocation;
                        else
                            result = Properties.Resources.StopNamePrefixFinishLocation;
                        break;
                    }

                case StopType.Lunch:
                    result = Properties.Resources.StopNamePrefixLunch;
                    break;

                case StopType.Order:
                    {
                        string format = Properties.Resources.StopNamePrefixFormat;
                        result = string.Format(format, stop.OrderSequenceNumber.ToString());
                        break;
                    }

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets stop name postfix.
        /// </summary>
        /// <param name="stop">Stop object.</param>
        /// <returns>Stop name postfix.</returns>
        private string _GetStopNamePostfix(Stop stop)
        {
            string result = string.Empty;
            switch (stop.StopType)
            {
                case StopType.Location:
                    {
                        if (stop.SequenceNumber == 1)
                            result = Properties.Resources.StopNamePostfixStartLocation;
                        else if (stop.SequenceNumber == stop.Route.Stops.Count)
                            result = Properties.Resources.StopNamePostfixFinishLocation;
                        else
                            result = Properties.Resources.StopNamePostfixRenewalLocation;
                        break;
                    }

                case StopType.Lunch:
                    result = Properties.Resources.StopNamePostfixLunch;
                    break;

                case StopType.Order:
                    result = Properties.Resources.StopNamePostfix;
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets route special field value.
        /// </summary>
        /// <param name="info">Export field info</param>
        /// <param name="route">Route object.</param>
        /// <returns>Getted value.</returns>
        private object _GetRouteSpecialFieldValue(FieldInfo info, Route route)
        {
            Debug.Assert(!string.IsNullOrEmpty(info.NameFormat));
            Debug.Assert(!string.IsNullOrEmpty(info.RelationType));

            object value = null;
            switch (info.RelationType)
            {
                case "Capacities":
                    {
                        int index = _tableDescription.GetCapacityIndex(info.Name, info.NameFormat);

                        double capacityVal = 0;
                        if (info.NameFormat.Contains("Total"))
                            capacityVal = route.Capacities[index];

                        else if (info.NameFormat.Contains("Utilization"))
                        {
                            capacityVal = 0;
                            if (0 != (route.Vehicle.Capacities[index] * route.RunCount))
                            {
                                double vehCapacities =
                                    route.Vehicle.Capacities[index] * route.RunCount;
                                capacityVal = (route.Capacities[index] / vehCapacities) * 100;
                            }
                        }

                        else
                            capacityVal = route.Vehicle.Capacities[index];

                        value = capacityVal;
                        break;
                    }

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return value;
        }

        /// <summary>
        /// Gets stop special field value.
        /// </summary>
        /// <param name="info">Export field info.</param>
        /// <param name="obj">Input object.</param>
        /// <returns>Getted value.</returns>
        private object _GetStopSpecialFieldValue(FieldInfo info, DataObject obj)
        {
            Debug.Assert(!string.IsNullOrEmpty(info.NameFormat));
            Debug.Assert(!string.IsNullOrEmpty(info.RelationType));

            object value = null;
            if (null != obj)
            {
                if ("Address" == info.RelationType)
                    value = _GetAddressFields(obj, info.NameFormat);
                else
                {
                    Order order = obj as Order;
                    if (null != order)
                    {
                        switch (info.RelationType)
                        {
                            case "Capacities":
                                {
                                    int index =
                                        _tableDescription.GetCapacityIndex(info.Name,
                                                                           info.NameFormat);
                                    value = order.Capacities[index];
                                    break;
                                }

                            case "CustomOrderProperties":
                                {
                                   int index =
                                       _tableDescription.GetOrderCustomPropertyIndex(info.Name,
                                                                                     info.NameFormat);
                                   value = order.CustomProperties[index];
                                   break;
                                }

                            default:
                                Debug.Assert(false); // NOTE: not supported
                                break;
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets route time string.
        /// </summary>
        /// <param name="route">Route object.</param>
        /// <returns>Formated route time string.</returns>
        private string _GetRouteTimeString(Route route)
        {
            return string.Format(ROUTE_TIME_STRING_FORMAT,
                                 _DateTimeToTimeString(route.StartTime),
                                 _DateTimeToTimeString(route.EndTime));
        }

        /// <summary>
        /// Gets field info by name.
        /// </summary>
        /// <param name="field">Field name.</param>
        /// <returns>Field info.</returns>
        private FieldInfo _GetFieldInfo(string field)
        {
            FieldInfo fieldInfo = _tableDescription.GetFieldInfo(field);
            Debug.Assert(null != fieldInfo);
            return fieldInfo;
        }

        /// <summary>
        /// Gets object value.
        /// </summary>
        /// <param name="field">Field name.</param>
        /// <param name="workObject">Input object.</param>
        /// <returns>Getted field value.</returns>
        private object _GetObjectValue(string field, DataObject workObject)
        {
            object value = null;
            if (null != workObject)
            {
                switch (field)
                {
                    case "FullAddress":
                        value = _GetFullAddress(workObject);
                        break;

                    case "FullAddressShort":
                        value = _GetShorterFullAddress(workObject);
                        break;

                    case "Confidence":
                        value = _GetConfidence(workObject);
                        break;

                    case "X":
                        value = _GetGeoLocation(workObject).X;
                        break;

                    case "Y":
                        value = _GetGeoLocation(workObject).Y;
                        break;

                    case "PlannedDate":
                        value = _GetPlannedDate(workObject);
                        break;

                    case "OrderType":
                        {
                            OrderType? val = _GetOrderType(workObject);
                            if (val.HasValue)
                                value = (int)val.Value;
                            else
                                value = null;
                            break;
                        }

                    case "OrderTypeString":
                        value = _AdapterNullableObjectToString(_GetOrderType(workObject));
                        break;

                    case "Priority":
                        {
                            OrderPriority? val = _GetOrderPriority(workObject);
                            if (val.HasValue)
                                value = (int)val.Value;
                            else
                                value = null;
                            break;
                        }

                    case "PriorityString":
                        value = _AdapterNullableObjectToString(_GetOrderPriority(workObject));
                        break;

                    case "TWDay":
                        {
                            TimeWindow timeWindow1 = _GetOrderTimeWindow(workObject, true);
                            if (timeWindow1 != null)
                                value = timeWindow1.Day;
                            else
                                value = null;
                            break;
                        }

                    case "TWFrom":
                        {
                            TimeSpan? val = _GetOrderTimeWindowValue(workObject, true, true);
                            if (val.HasValue)
                                value = _ConvertTimeToMinutes(val.Value);
                            else
                                value = null;
                            break;
                        }

                    case "TWTo":
                        {
                            TimeSpan? val = _GetOrderTimeWindowValue(workObject, true, false);
                            if (val.HasValue)
                                value = _ConvertTimeToMinutes(val.Value);
                            else
                                value = null;
                            break;
                        }

                    case "TW2Day":
                        {
                            TimeWindow timeWindow2 = _GetOrderTimeWindow(workObject, false);
                            if (timeWindow2 != null)
                                value = timeWindow2.Day;
                            else
                                value = null;
                            break;
                        }

                    case "TW2From":
                        {
                            TimeSpan? val = _GetOrderTimeWindowValue(workObject, false, true);
                            if (val.HasValue)
                                value = _ConvertTimeToMinutes(val.Value);
                            else
                                value = null;
                            break;
                        }

                    case "TW2To":
                        {
                            TimeSpan? val = _GetOrderTimeWindowValue(workObject, false, false);
                            if (val.HasValue)
                                value = _ConvertTimeToMinutes(val.Value);
                            else
                                value = null;
                            break;
                        }

                    case "TWFromString":
                        {
                            TimeSpan? time = _GetOrderTimeWindowValue(workObject, true, true);
                            value = _AdapterNullableObjectToString(time);
                            break;
                        }

                    case "TWToString":
                        {
                            TimeSpan? time = _GetOrderTimeWindowValue(workObject, true, false);
                            value = _AdapterNullableObjectToString(time);
                            break;
                        }

                    case "TWFrom2String":
                        {
                            TimeSpan? time = _GetOrderTimeWindowValue(workObject, false, true);
                            value = _AdapterNullableObjectToString(time);
                            break;
                        }

                    case "TWTo2String":
                        {
                            TimeSpan? time =_GetOrderTimeWindowValue(workObject, false, false);
                            value = _AdapterNullableObjectToString(time);
                            break;
                        }

                    case "TWString":
                        {
                            TimeWindow tw = _GetOrderTimeWindow(workObject, true);
                            value = _AdapterNullableObjectToString(tw);
                            break;
                        }

                    case "TW2String":
                        {
                            TimeWindow tw = _GetOrderTimeWindow(workObject, false);
                            value = _AdapterNullableObjectToString(tw);
                            break;
                        }

                    case "MaxViolationTime":
                        value = _GetMaxViolationTime(workObject);
                        break;

                    case "DriverSpecialties":
                        value = _GetSpecialities(workObject, true);
                        break;
                    case "VehicleSpecialties":
                        value = _GetSpecialities(workObject, false);
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            return value;
        }

        /// <summary>
        /// Gets stop's route value.
        /// </summary>
        /// <param name="field">Field name.</param>
        /// <param name="route">Route object.</param>
        /// <returns>Getted stop's route value.</returns>
        private object _GetStopRouteValue(string field, Route route)
        {
            object value = null;
            if (null != route)
            {
                switch (field)
                {
                    case "RouteID":
                        value = route.Id;
                        break;

                    case "RouteName":
                        value = route.Name;
                        break;

                    case "DriverName":
                        value = route.Driver.Name;
                        break;

                    case "VehicleName":
                        value = route.Vehicle.Name;
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            return value;

        }

        /// <summary>
        /// Gets stop value.
        /// </summary>
        /// <param name="field">Field name.</param>
        /// <param name="stop">Stop object.</param>
        /// <returns>Getted stop value.</returns>
        private object _GetStopValue(string field, Stop stop)
        {
            object value = null;
            if (null != stop)
            {
                switch (field)
                {
                    case "RouteID":
                    case "RouteName":
                    case "DriverName":
                    case "VehicleName":
                        value = _GetStopRouteValue(field, stop.Route);
                        break;

                    case "StopType":
                        value = (int)stop.StopType;
                        break;

                    case "StopTypeString":
                        value = stop.StopType.ToString();
                        break;

                    case "StopTypeExString":
                        value = _GetStopTypeEx(stop);
                        break;

                    case "StopNamePrefix":
                        value = _GetStopNamePrefix(stop);
                        break;

                    case "StopNamePostfix":
                        value = _GetStopNamePostfix(stop);
                        break;

                    case "Sequence":
                        value = stop.SequenceNumber;
                        break;

                    case "OrderSequence":
                        value = stop.OrderSequenceNumber;
                        break;

                    case "TravelTime":
                        value = stop.TravelTime;
                        break;

                    case "WaitTime":
                        value = stop.WaitTime;
                        break;

                    case "ArriveDate":
                        value = _DateTimeToDate(stop.ArriveTime);
                        break;

                    case "ArriveTime":
                        value = _ConvertDateToMinutes(stop.ArriveTime);
                        break;

                    case "ArriveTimeString":
                        value = _DateTimeToTimeString(stop.ArriveTime);
                        break;

                    case "DistanceFromPrevious":
                        value = stop.Distance;
                        break;

                    case "LoadAtID":
                        value = _GetLoadAtId(stop);
                        break;

                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            return value;
        }

        #endregion // Private methods

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Route time string format.
        /// </summary>
        private const string ROUTE_TIME_STRING_FORMAT = "{0} - {1}";

        // Predifined stop type ex consts
        private const string STOP_TYPE_EX_START_LOCATION = "StartLocation";
        private const string STOP_TYPE_EX_FINISH_LOCATION = "FinishLocation";
        private const string STOP_TYPE_EX_RENEWAL_LOCATION = "RenewalLocation";
        private const string STOP_TYPE_EX_LUNCH = "Lunch";
        private const string STOP_TYPE_EX_STOP = "Stop";

        /// <summary>
        /// Delimeter for combining full address property.
        /// </summary>
        private const string DELIMETER = ", ";

        #endregion // Private consts

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current list separator.
        /// </summary>
        private string _listSeparator;
        /// <summary>
        /// Export table description.
        /// </summary>
        private TableDescription _tableDescription;

        #endregion // Private members
    }
}
