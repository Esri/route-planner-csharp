using ESRI.ArcLogistics.Serialization;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stores default properties for <see cref="T:ESRI.ArcLogistics.DomainObjects.Order"/>
    /// objects.
    /// </summary>
    internal class OrdersDefaults
    {
        /// <summary>
        /// Gets or sets a default value for the order type property.
        /// </summary>
        public OrderType OrderType
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the order priority property.
        /// </summary>
        public OrderPriority Priority
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the curb approach property.
        /// </summary>
        public CurbApproach CurbApproach
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the service time property.
        /// </summary>
        public int ServiceTime
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the first time window property.
        /// </summary>
        public TimeWindowConfiguration TimeWindow
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the second time window property.
        /// </summary>
        public TimeWindowConfiguration TimeWindow2
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the maximum violation time property.
        /// </summary>
        public double MaxViolationTime
        { get; set; }
    }
}
