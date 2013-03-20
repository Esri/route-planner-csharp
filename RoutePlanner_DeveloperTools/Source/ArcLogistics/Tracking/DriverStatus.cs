namespace ESRI.ArcLogistics.Tracking
{
    /// <summary>
    /// Defines statuses for the driver.
    /// </summary>
    public enum DriverStatus
    {
        /// <summary>
        /// Driver is on the way to some stop.
        /// </summary>
        InRoute,

        /// <summary>
        /// Driver arrived and didn’t start service yet (probably he is waiting
        /// for a customer).
        /// </summary>
        Waiting,

        /// <summary>
        /// Driver services some order.
        /// </summary>
        Servicing,

        /// <summary>
        /// Driver has this status when its current stop is break.
        /// </summary>
        OnBreak,

        /// <summary>
        /// Driver has this status when he renews his capacities at renewal stop.
        /// </summary>
        Renewing,

        /// <summary>
        /// Driver is servicing in start location.
        /// </summary>
        InStartLocation,

        /// <summary>
        /// Driver is servicing in end location.
        /// </summary>
        InEndLocation,

        /// <summary>
        /// Driver has “Off Duty” status when he didn’t start route or completed it.
        /// </summary>
        OffDuty,
    }
}
