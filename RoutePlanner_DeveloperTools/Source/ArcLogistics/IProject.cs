using System;
using System.Collections.Generic;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Archiving;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.BreaksHelpers;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Interface represents project.
    /// </summary>
    public interface IProject
    {
        /// <summary>
        /// Name.
        /// </summary>
        string Name { get; }

        string Path { get; }

        /// <summary>
        /// Description.
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Properties.
        /// </summary>
        IProjectProperties ProjectProperties { get; }

        /// <summary>
        /// Archiving settings.
        /// </summary>
        /// <remarks>Can be null, if not supported.</remarks>
        ProjectArchivingSettings ProjectArchivingSettings { get; }

        /// <summary>
		/// Project breaks settings.
        /// </summary>
        /// <remarks>Cannot be null.</remarks>
        BreaksSettings BreaksSettings { get; }
        /// <summary>
		/// Default routes collection.
        /// </summary>
        IDataObjectCollection<Route> DefaultRoutes { get; }
        /// <summary>
        /// Drivers collection.
        /// </summary>
        IDataObjectCollection<Driver> Drivers { get; }
        /// <summary>
        /// Driver specialties collection.
        /// </summary>
        IDataObjectCollection<DriverSpecialty> DriverSpecialties { get; }
        /// <summary>
        /// Locations collection.
        /// </summary>
        IDataObjectCollection<Location> Locations { get; }
        /// <summary>
        /// Mobile devices collection.
        /// </summary>
        IDataObjectCollection<MobileDevice> MobileDevices { get; }
        /// <summary>
        /// Vehicles collection.
        /// </summary>
        IDataObjectCollection<Vehicle> Vehicles { get; }
        /// <summary>
        /// Vehicle specialties collection.
        /// </summary>
        IDataObjectCollection<VehicleSpecialty> VehicleSpecialties { get; }
        /// <summary>
        /// Zones collection.
        /// </summary>
        IDataObjectCollection<Zone> Zones { get; }
        /// <summary>
        /// Fuel types collection.
        /// </summary>
        IDataObjectCollection<FuelType> FuelTypes { get; }

        /// <summary>
        /// Barrires manager.
        /// </summary>
        BarrierManager Barriers { get; }
        /// <summary>
        /// Orders manager.
        /// </summary>
        OrderManager Orders { get; }
        /// <summary>
        /// Schedules manager.
        /// </summary>
        ScheduleManager Schedules { get; }

        /// <summary>
        /// Gets a reference to the deletion checking service.
        /// </summary>
        IDeletionCheckingService DeletionCheckingService
        {
            get;
        }

        /// <summary>
        /// Create new route with default breaks.
        /// </summary>
        /// <returns></returns>
        Route CreateRoute();

        /// <summary>
        /// Store changes.
        /// </summary>
        void Save();
    }
}
