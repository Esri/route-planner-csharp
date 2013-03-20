namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stores default properties for <see cref="T:ESRI.ArcLogistics.DomainObjects.Driver"/>
    /// objects.
    /// </summary>
    internal class DriversDefaults
    {
        /// <summary>
        /// Gets or sets a default value for the per hour salary property.
        /// </summary>
        public int PerHour
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the per overtime hour salary property.
        /// </summary>
        public int PerHourOT
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the time before overtime property.
        /// </summary>
        public int TimeBeforeOT
        { get; set; }
    }
}
