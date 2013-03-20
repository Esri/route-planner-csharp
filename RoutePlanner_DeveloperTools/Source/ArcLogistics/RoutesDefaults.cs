using ESRI.ArcLogistics.Serialization;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stores default properties for <see cref="T:ESRI.ArcLogistics.DomainObjects.Route"/>
    /// objects.
    /// </summary>
    internal class RoutesDefaults
    {
        /// <summary>
        /// Gets or sets a default value for the starting time window property.
        /// </summary>
        public TimeWindowConfiguration StartTimeWindow
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the time at start property.
        /// </summary>
        public int TimeAtStart
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the time at end property.
        /// </summary>
        public int TimeAtEnd
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the time at renewal property.
        /// </summary>
        public int TimeAtRenewal
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the maximum order count property.
        /// </summary>
        public int MaxOrder
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the maximum travel distance property.
        /// </summary>
        public int MaxTravelDistance
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the maximum travel duration property.
        /// </summary>
        public int MaxTravelDuration
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the maximum total duration property.
        /// </summary>
        public int MaxTotalDuration
        { get; set; }
    }
}
