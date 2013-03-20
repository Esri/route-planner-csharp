namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Curb approach specifies the direction a vehicle may arrive at and depart from a location.
    /// </summary>
    public enum CurbApproach
    {
        /// <summary>
        /// When the vehicle approaches and departs the stop, the stop must be on the left side
        /// of the vehicle. A U-turn is prohibited.
        /// </summary>
        Left,

        /// <summary>
        /// When the vehicle approaches and departs the stop, the stop must be on the right side
        /// of the vehicle. A U-turn is prohibited.
        /// </summary>
        Right,

        /// <summary>
        /// The vehicle can approach and depart the stop in either direction, so a U-turn is allowed.
        /// This setting can be chosen if it is possible and desirable for your vehicle
        /// to turn around at the stop.
        /// </summary>
        Both,

        /// <summary>
        /// When the vehicle approaches the stop, the stop can be on either side of the vehicle;
        /// however, when it departs, the vehicle must continue in the same direction it arrived in.
        /// A U-turn is prohibited.
        /// </summary>
        NoUTurns
    }

    /// <summary>
    /// Synchronization type with the mobile device.
    /// </summary>
    public enum SyncType
    {
        /// <summary>
        /// Synchronization type none specified.
        /// </summary>
        None,
        /// <summary>
        /// Synchronize by email.
        /// </summary>
        EMail,
        /// <summary>
        /// Used ActiveSync to synchronize.
        /// </summary>
        ActiveSync,
        /// <summary>
        /// Synchronize from common folder.
        /// </summary>
        Folder,
        /// <summary>
        /// WMServer ActiveSync to synchronize.
        /// </summary>
        WMServer
    }

    public enum AddressPart
    {
        Unit,
        FullAddress,
        AddressLine,
        Locality1,
        Locality2,
        Locality3,
        CountyPrefecture,
        PostalCode1,
        PostalCode2,
        StateProvince,
        Country
    }

    /// <summary>
    /// Represents possible address formats.
    /// </summary>
    public enum AddressFormat
    {
        /// <summary>
        /// Multiple fields format.
        /// </summary>
        MultipleFields,

        /// <summary>
        /// Single field format.
        /// </summary>
        SingleField
    };

    /// <summary>
    /// Order service type.
    /// </summary>
    public enum OrderType
    {
        /// <summary>
        /// Pickup service type.
        /// </summary>
        Pickup,
        /// <summary>
        /// Delivery service type.
        /// </summary>
        Delivery
    }

    /// <summary>
    /// Order priority specifies the level of servicing.
    /// </summary>
    public enum OrderPriority
    {
        /// <summary>
        /// High priorited servicing order.
        /// </summary>
        High,
        /// <summary>
        /// Normal priorited servicing order.
        /// </summary>
        Normal
    }

    public enum StopType
    {
        Order,
        Location,
        Lunch
    }

    public enum StopManeuverType
    {
        Unknown,
        Stop,
        Straight,
        BearLeft,
        BearRight,
        TurnLeft,
        TurnRight,
        SharpLeft,
        SharpRight,
        UTurn,
        Ferry,
        Roundabout,
        HighwayMerge,
        HighwayExit,
        HighwayChange,
        ForkCenter,
        ForkLeft,
        ForkRight,
        Depart,
        TripItem,
        EndOfFerry,
        RampLeft,
        RampRight
    }

    public enum ScheduleType
    {
        BuildRoutesSnapshot,
        Current,
        Edited
    }

    /// <summary>
    /// Direction type is used to determine type of information
    /// in the direction.
    /// </summary>
    internal enum DirectionType
    {
        /// <summary>
        /// Maneuver direction.
        /// </summary>
        ManeuverDirection,

        /// <summary>
        /// Other type direction.
        /// </summary>
        Other
    }
}