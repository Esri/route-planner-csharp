namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Specifies the type of the stop in the stops record-set produced by VRP Solver.
    /// </summary>
    public enum NAStopType
    {
        /// <summary>
        /// The stop is an order stop.
        /// </summary>
        Order = 0,

        /// <summary>
        /// The stop is a starting, ending or renewal depot.
        /// </summary>
        Depot = 1,

        /// <summary>
        /// The stop is a lunch one.
        /// </summary>
        Break = 2,
    }
}
