/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// Helps in checking validity of data entities deletion attempts.
    /// </summary>
    public interface IDeletionCheckingService
    {
        /// <summary>
        /// Filters the specified collection of drivers by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="drivers">Collection of drivers to be filtered.</param>
        /// <returns>A collection of drivers associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="drivers"/> is a null
        /// reference.</exception>
        IEnumerable<Driver> QueryDefaultRouteDrivers(IEnumerable<Driver> drivers);

        /// <summary>
        /// Filters the specified collection of drivers by returning ones associated with one or
        /// more route planned for the specified date or any date after it.
        /// </summary>
        /// <param name="drivers">Collection of drivers to be filtered.</param>
        /// <param name="current">The date to be used as a lower bound for searching for planned
        /// routes.</param>
        /// <returns>A collection of drivers associated with one or more route planned for
        /// the specified date or any date after it.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="drivers"/> is a null
        /// reference.</exception>
        IEnumerable<Driver> QueryFutureRouteDrivers(
            IEnumerable<Driver> drivers,
            DateTime current);

        /// <summary>
        /// Filters the specified collection of vehicles by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="vehicles">Collection of vehicles to be filtered.</param>
        /// <returns>A collection of vehicles associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="vehicles"/> is a null
        /// reference.</exception>
        IEnumerable<Vehicle> QueryDefaultRouteVehicles(IEnumerable<Vehicle> vehicles);

        /// <summary>
        /// Filters the specified collection of vehicles by returning ones associated with one or
        /// more route planned for the specified date or any date after it.
        /// </summary>
        /// <param name="vehicles">Collection of vehicles to be filtered.</param>
        /// <param name="current">The date to be used as a lower bound for searching for planned
        /// routes.</param>
        /// <returns>A collection of vehicles associated with one or more route planned for
        /// the specified date or any date after it.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="vehicles"/> is a null
        /// reference.</exception>
        IEnumerable<Vehicle> QueryFutureRouteVehicles(
            IEnumerable<Vehicle> vehicles,
            DateTime current);

        /// <summary>
        /// Filters the specified collection of zones by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="zones">Collection of zones to be filtered.</param>
        /// <returns>A collection of zones associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="zones"/> is a null
        /// reference.</exception>
        IEnumerable<Zone> QueryDefaultRouteZones(IEnumerable<Zone> zones);

        /// <summary>
        /// Filters the specified collection of zones by returning ones associated with one or
        /// more route planned for the specified date or any date after it.
        /// </summary>
        /// <param name="zones">Collection of zones to be filtered.</param>
        /// <param name="current">The date to be used as a lower bound for searching for planned
        /// routes.</param>
        /// <returns>A collection of zones associated with one or more route planned for
        /// the specified date or any date after it.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="zones"/> is a null
        /// reference.</exception>
        IEnumerable<Zone> QueryFutureRouteZones(
            IEnumerable<Zone> zones,
            DateTime current);

        /// <summary>
        /// Filters the specified collection of locations by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="locations">Collection of vehicles to be filtered.</param>
        /// <returns>A collection of locations associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="locations"/> is a null
        /// reference.</exception>
        IEnumerable<Location> QueryDefaultRouteLocations(IEnumerable<Location> locations);

        /// <summary>
        /// Filters the specified collection of locations by returning ones associated with one or
        /// more route planned for the specified date or any date after it.
        /// </summary>
        /// <param name="locations">Collection of locations to be filtered.</param>
        /// <param name="current">The date to be used as a lower bound for searching for planned
        /// routes.</param>
        /// <returns>A collection of locations associated with one or more route planned for
        /// the specified date or any date after it.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="locations"/> is a null
        /// reference.</exception>
        IEnumerable<Location> QueryFutureRouteLocations(
            IEnumerable<Location> locations,
            DateTime current);

        /// <summary>
        /// Filters the specified collection of fuel types by returning ones associated with one or
        /// more vehicle.
        /// </summary>
        /// <param name="fuelTypes">Collection of fuel types to be filtered.</param>
        /// <returns>A collection of fuel types associated with one or more vehicles.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="fuelTypes"/> is a null
        /// reference.</exception>
        IEnumerable<FuelType> QueryAssignedFuelTypes(
            IEnumerable<FuelType> fuelTypes);

        /// <summary>
        /// Filters the specified collection of devices by returning ones associated with one or
        /// more driver.
        /// </summary>
        /// <param name="devices">Collection of devices to be filtered.</param>
        /// <returns>A collection of devices associated with one or more driver.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        IEnumerable<MobileDevice> QueryAssignedToDriver(IEnumerable<MobileDevice> devices);

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more vehicle.
        /// </summary>
        /// <param name="devices">Collection of devices to be filtered.</param>
        /// <returns>A collection of devices associated with one or more vehicle.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        IEnumerable<MobileDevice> QueryAssignedToVehicle(IEnumerable<MobileDevice> devices);

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more driver.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more driver.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        IEnumerable<DriverSpecialty> QueryAssignedSpecialties(
            IEnumerable<DriverSpecialty> specialties);

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more vehicle.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more vehicle.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        IEnumerable<VehicleSpecialty> QueryAssignedSpecialties(
            IEnumerable<VehicleSpecialty> specialties);

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more order.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more order.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        IEnumerable<DriverSpecialty> QueryOrderSpecialties(
            IEnumerable<DriverSpecialty> specialties);

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more order.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more order.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        IEnumerable<VehicleSpecialty> QueryOrderSpecialties(
            IEnumerable<VehicleSpecialty> specialties);
    }
}
