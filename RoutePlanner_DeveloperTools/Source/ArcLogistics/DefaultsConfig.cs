using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace ESRI.ArcLogistics.Serialization
{
    [XmlRoot("DefaultConfiguration")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DefaultsConfig
    {
        [XmlElement("CapacitiesConfiguration")]
        public CapacitiesDefaults CapacitiesDefaults
        {
            get;
            set;
        }

        [XmlElement("FuelTypesConfiguration")]
        public FuelTypesDefaults FuelTypesDefaults
        {
            get;
            set;
        }

        [XmlElement("Locations")]
        public LocationsDefaultsConfig LocationsDefaults
        {
            get;
            set;
        }

        [XmlElement("Orders")]
        public OrdersDefaultsConfig OrdersDefaults
        {
            get;
            set;
        }

        [XmlElement("Routes")]
        public RoutesDefaultsConfig RoutesDefaults
        {
            get;
            set;
        }

        [XmlElement("Vehicles")]
        public VehiclesDefaultsConfig VehiclesDefaults
        {
            get;
            set;
        }


        [XmlElement("Drivers")]
        public DriversDefaultsConfig DriversDefaults
        {
            get;
            set;
        }

        [XmlElement("CustomOrderProperties")]
        public CustomOrderProperties CustomOrderProperties
        {
            get;
            set;
        }
    }

    /// <summary>
    /// CapacitiesConfiguration class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CapacitiesDefaults
    {
        [XmlElement("Capacity")]
        public CapacityConfig[] Capacity
        {
            get { return _capacity; }
            set { _capacity = value; }
        }

        CapacityConfig[] _capacity;
    }

    /// <summary>
    /// Capacity class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CapacityConfig
    {
        [XmlAttribute("Name")]
        public string Name
        { get; set; }

        [XmlAttribute("DisplayUnitUS")]
        public Unit DisplayUnitUS
        { get; set; }

        [XmlAttribute("DisplayUnitMetric")]
        public Unit DisplayUnitMetric
        { get; set; }
    }

    /// <summary>
    /// FuelTypesConfiguration class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FuelTypesDefaults
    {
        [XmlElement("FuelType")]
        public FuelTypeConfig[] FuelTypeConfig
        {
            get { return _fuelType; }
            set { _fuelType = value; }
        }

        FuelTypeConfig[] _fuelType;
    }

    /// <summary>
    /// FuelType class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FuelTypeConfig
    {
        [XmlAttribute("Name")]
        public string Name
        { get; set; }

        [XmlAttribute("Price")]
        public double Price
        { get; set; }

        [XmlAttribute("Co2Emission")]
        public double Co2Emission
        { get; set; }
    }

    /// <summary>
    /// Locations class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LocationsDefaultsConfig
    {
        [XmlElement("CurbApproach")]
        public CurbApproach? CurbApproach
        { get; set; }

        [XmlElement("TimeWindow")]
        public TimeWindowConfiguration TimeWindow
        { get; set; }
    }

    /// <summary>
    /// Orders class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class OrdersDefaultsConfig
    {
        [XmlElement("Type")]
        public OrderType? OrderType
        { get; set; }

        [XmlElement("Priority")]
        public OrderPriority? Priority
        { get; set; }

        [XmlElement("CurbApproach")]
        public CurbApproach? CurbApproach
        { get; set; }

        [XmlElement("ServiceTime")]
        public int? ServiceTime
        { get; set; }

        [XmlElement("TimeWindow")]
        public TimeWindowConfiguration TimeWindow
        { get; set; }

        [XmlElement("TimeWindow2")]
        public TimeWindowConfiguration TimeWindow2
        { get; set; }

        [XmlElement("MaxViolationTime")]
        public double? MaxViolationTime
        { get; set; }
    }

    /// <summary>
    /// Routes class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class RoutesDefaultsConfig
    {
        [XmlElement("StartTimeWindow")]
        public TimeWindowConfiguration StartTimeWindow
        { get; set; }

        [XmlElement("BreakTimeWindow")]
        public TimeWindowConfiguration BreakTimeWindow
        { get; set; }

        [XmlElement("BreakDuration")]
        public int? BreakDuration
        { get; set; }

        [XmlElement("TimeAtStart")]
        public int? TimeAtStart
        { get; set; }

        [XmlElement("TimeAtEnd")]
        public int? TimeAtEnd
        { get; set; }

        [XmlElement("TimeAtRenewal")]
        public int? TimeAtRenewal
        { get; set; }

        [XmlElement("MaxOrder")]
        public int? MaxOrder
        { get; set; }

        [XmlElement("MaxTravelDistance")]
        public int? MaxTravelDistance
        { get; set; }

        [XmlElement("MaxTravelDuration")]
        public int? MaxTravelDuration
        { get; set; }

        [XmlElement("MaxTotalDuration")]
        public int? MaxTotalDuration
        { get; set; }
    }

    /// <summary>
    /// CapacitiesDefaultValue class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CapacitiesDefaultValue
    {
        [XmlAttribute("Name")]
        public string Name
        { get; set; }

        [XmlAttribute("Value")]
        public int Value
        { get; set; }
    }

    /// <summary>
    /// CapacitiesDefaultValues class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CapacitiesDefaultValues
    {
        [XmlElement("Capacity")]
        public CapacitiesDefaultValue[] Capacity
        {
            get { return _capacity; }
            set { _capacity = value; }
        }

        CapacitiesDefaultValue[] _capacity;
    }

    /// <summary>
    /// Vehicles class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class VehiclesDefaultsConfig
    {
        [XmlElement("FuelEconomy")]
        public int? FuelEconomy
        { get; set; }

        [XmlElement("Capacities")]
        public CapacitiesDefaultValues CapacitiesDefaultValues
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Drivers class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DriversDefaultsConfig
    {
        [XmlElement("PerHour")]
        public int? PerHour
        { get; set; }

        [XmlElement("PerHourOT")]
        public int? PerHourOT
        { get; set; }

        [XmlElement("TimeBeforeOT")]
        public int? TimeBeforeOT
        { get; set; }
    }

    /// <summary>
    /// CustomOrderProperties
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomOrderProperties
    {
        [XmlElement("Property")]
        public CustomOrderProperty[] CustomOrderProperty
        {
            get { return _customOrderProperties; }
            set { _customOrderProperties = value; }
        }

        CustomOrderProperty[] _customOrderProperties;
    }

    /// <summary>
    /// CustomOrderProperty class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CustomOrderProperty
    {
        [XmlAttribute("Name")]
        public string Name
        { get; set; }

        [XmlAttribute("Type")]
        public OrderCustomPropertyType Type
        { get; set; }

        [XmlAttribute("MaxLength")]
        public int MaxLength
        { get; set; }

        [XmlAttribute("Description")]
        public string Description
        { get; set; }

        [XmlAttribute("OrderPairKey")]
        public bool OrderPairKey
        { get; set; }
    }

    /// <summary>
    /// TimeWindowConfiguration class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TimeWindowConfiguration
    {
        [XmlAttribute("IsWideopen")]
        public bool IsWideopen
        { get; set; }

        [XmlAttribute("From")]
        public string FromStr
        {
            get
            {
                return _fromStr;
            }
            set
            {
                _fromStr = value;
                _from = TimeSpan.Parse(_fromStr);
            }
        }

        [XmlAttribute("To")]
        public string ToStr
        {
            get
            {
                return _toStr;
            }
            set
            {
                _toStr = value;
                _to = TimeSpan.Parse(_toStr);
            }
        }

        public TimeSpan From
        {
            get { return _from; }
        }

        public TimeSpan To
        {
            get { return _to; }
        }

        private string _fromStr;
        private string _toStr;
        private TimeSpan _from;
        private TimeSpan _to;
    }
}
