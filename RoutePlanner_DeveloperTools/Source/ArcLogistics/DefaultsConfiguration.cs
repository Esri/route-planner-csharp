using ESRI.ArcLogistics.Serialization;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Stores application defaults configuration.
    /// </summary>
    internal class DefaultsConfiguration
    {
        /// <summary>
        /// Gets or sets a reference to capacities default properties.
        /// </summary>
        public CapacitiesDefaults CapacitiesDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to fuel types default properties.
        /// </summary>
        public FuelTypesDefaults FuelTypesDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to locations default properties.
        /// </summary>
        public LocationsDefaults LocationsDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to capacities default properties.
        /// </summary>
        public OrdersDefaults OrdersDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to routes default properties.
        /// </summary>
        public RoutesDefaults RoutesDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to vehicles default properties.
        /// </summary>
        public VehiclesDefaults VehiclesDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to drivers default properties.
        /// </summary>
        public DriversDefaults DriversDefaults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a reference to custom orders default properties.
        /// </summary>
        public CustomOrderProperties CustomOrderProperties
        {
            get;
            set;
        }
    }
}
