using ESRI.ArcLogistics.Serialization;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stores default properties for <see cref="T:ESRI.ArcLogistics.DomainObjects.Location"/>
    /// objects.
    /// </summary>
    internal class LocationsDefaults
    {
        /// <summary>
        /// Gets or sets a default value for the curb approach property.
        /// </summary>
        public CurbApproach CurbApproach
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for the time window property.
        /// </summary>
        public TimeWindowConfiguration TimeWindow
        { get; set; }
    }
}
