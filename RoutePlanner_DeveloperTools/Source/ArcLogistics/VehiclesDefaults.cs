using ESRI.ArcLogistics.Serialization;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stores default properties for <see cref="T:ESRI.ArcLogistics.DomainObjects.Vehicle"/>
    /// objects.
    /// </summary>
    internal class VehiclesDefaults
    {
        /// <summary>
        /// Gets or sets a default value for the fuel economy property.
        /// </summary>
        public int FuelEconomy
        { get; set; }

        /// <summary>
        /// Gets or sets a default value for capacities values.
        /// </summary>
        public CapacitiesDefaultValues CapacitiesDefaultValues
        {
            get;
            set;
        }
    }
}
