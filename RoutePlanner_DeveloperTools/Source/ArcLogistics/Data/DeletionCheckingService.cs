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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Data
{
    internal sealed class DeletionCheckingService : IDeletionCheckingService
    {
        /// <summary>
        /// Initializes a new instance of the DeletionCheckingService class.
        /// </summary>
        /// <param name="dataContext">The reference to the data context to be used for accessing
        /// project data.</param>
        internal DeletionCheckingService(DataObjectContext dataContext)
        {
            Debug.Assert(dataContext != null);
            _dataContext = dataContext;
        }

        #region IDeletionCheckingService Members
        /// <summary>
        /// Filters the specified collection of drivers by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="drivers">Collection of drivers to be filtered.</param>
        /// <returns>A collection of drivers associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="drivers"/> is a null
        /// reference.</exception>
        public IEnumerable<Driver> QueryDefaultRouteDrivers(IEnumerable<Driver> drivers)
        {
            if (drivers == null)
            {
                throw new ArgumentNullException("drivers");
            }

            return _QueryDefaultRouteObjects(drivers, route => route.Drivers);
        }

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
        public IEnumerable<Driver> QueryFutureRouteDrivers(
            IEnumerable<Driver> drivers,
            DateTime current)
        {
            if (drivers == null)
            {
                throw new ArgumentNullException("drivers");
            }

            return _QueryFutureRouteObjects(drivers, current, route => route.Drivers);
        }

        /// <summary>
        /// Filters the specified collection of vehicles by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="vehicles">Collection of vehicles to be filtered.</param>
        /// <returns>A collection of vehicles associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="vehicles"/> is a null
        /// reference.</exception>
        public IEnumerable<Vehicle> QueryDefaultRouteVehicles(IEnumerable<Vehicle> vehicles)
        {
            if (vehicles == null)
            {
                throw new ArgumentNullException("vehicles");
            }

            return _QueryDefaultRouteObjects(vehicles, route => route.Vehicles);
        }

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
        public IEnumerable<Vehicle> QueryFutureRouteVehicles(
            IEnumerable<Vehicle> vehicles,
            DateTime current)
        {
            if (vehicles == null)
            {
                throw new ArgumentNullException("vehicles");
            }

            return _QueryFutureRouteObjects(vehicles, current, route => route.Vehicles);
        }

        /// <summary>
        /// Filters the specified collection of zones by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="zones">Collection of zones to be filtered.</param>
        /// <returns>A collection of zones associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="zones"/> is a null
        /// reference.</exception>
        public IEnumerable<Zone> QueryDefaultRouteZones(IEnumerable<Zone> zones)
        {
            if (zones == null)
            {
                throw new ArgumentNullException("zones");
            }

            var result = _QueryObjects(
                zones,
                ids => _QueryRouteObjects(_QueryDefaultRoutes(), ids, route => route.Zones));

            return result;
        }

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
        public IEnumerable<Zone> QueryFutureRouteZones(
            IEnumerable<Zone> zones,
            DateTime current)
        {
            if (zones == null)
            {
                throw new ArgumentNullException("zones");
            }

            var result = _QueryObjects(
                zones,
                ids => _QueryRouteObjects(_QueryFutureRoutes(current), ids, route => route.Zones));

            return result;
        }

        /// <summary>
        /// Filters the specified collection of locations by returning ones associated with one or
        /// more default route.
        /// </summary>
        /// <param name="locations">Collection of vehicles to be filtered.</param>
        /// <returns>A collection of locations associated with one or more default route.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="locations"/> is a null
        /// reference.</exception>
        public IEnumerable<Location> QueryDefaultRouteLocations(IEnumerable<Location> locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }

            return _QueryRouteLocations(_QueryDefaultRoutes(), locations);
        }

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
        public IEnumerable<Location> QueryFutureRouteLocations(
            IEnumerable<Location> locations,
            DateTime current)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }

            return _QueryRouteLocations(_QueryFutureRoutes(current), locations);
        }

        /// <summary>
        /// Filters the specified collection of fuel types by returning ones associated with one or
        /// more vehicle.
        /// </summary>
        /// <param name="fuelTypes">Collection of fuel types to be filtered.</param>
        /// <returns>A collection of fuel types associated with one or more vehicles.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="fuelTypes"/> is a null
        /// reference.</exception>
        public IEnumerable<FuelType> QueryAssignedFuelTypes(IEnumerable<FuelType> fuelTypes)
        {
            if (fuelTypes == null)
            {
                throw new ArgumentNullException("fuelTypes");
            }

            var result = _QueryObjects(
                fuelTypes,
                ids =>
                    from vehicle in _dataContext.Vehicles
                    let fuelTypeID = vehicle.FuelTypes.Id
                    where !vehicle.Deleted && fuelTypeID != null && ids.Contains(fuelTypeID)
                    select fuelTypeID);

            return result;
        }

        /// <summary>
        /// Filters the specified collection of devices by returning ones associated with one or
        /// more driver.
        /// </summary>
        /// <param name="devices">Collection of devices to be filtered.</param>
        /// <returns>A collection of devices associated with one or more driver.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        public IEnumerable<MobileDevice> QueryAssignedToDriver(IEnumerable<MobileDevice> devices)
        {
            if (devices == null)
            {
                throw new ArgumentNullException("devices");
            }

            var result = _QueryObjects(
                devices,
                ids =>
                    from driver in _dataContext.Drivers
                    let device = driver.MobileDevices
                    where !driver.Deleted && device.Id != null && ids.Contains(device.Id)
                    select device.Id);

            return result;
        }

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more vehicle.
        /// </summary>
        /// <param name="devices">Collection of devices to be filtered.</param>
        /// <returns>A collection of devices associated with one or more vehicle.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="devices"/> is a null
        /// reference.</exception>
        public IEnumerable<MobileDevice> QueryAssignedToVehicle(IEnumerable<MobileDevice> devices)
        {
            if (devices == null)
            {
                throw new ArgumentNullException("devices");
            }

            var result = _QueryObjects(
                devices,
                ids =>
                    from vehicle in _dataContext.Vehicles
                    let device = vehicle.MobileDevices
                    where !vehicle.Deleted && device.Id != null && ids.Contains(device.Id)
                    select device.Id);

            return result;
        }

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more driver.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more driver.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        public IEnumerable<DriverSpecialty> QueryAssignedSpecialties(
            IEnumerable<DriverSpecialty> specialties)
        {
            if (specialties == null)
            {
                throw new ArgumentNullException("specialties");
            }

            var result = _QueryObjects(
                specialties,
                ids =>
                    from driver in _dataContext.Drivers
                    from specialty in driver.DriverSpecialties
                    where !driver.Deleted && specialty.Id != null && ids.Contains(specialty.Id)
                    select specialty.Id);

            return result;
        }

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more vehicle.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more vehicle.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        public IEnumerable<VehicleSpecialty> QueryAssignedSpecialties(
            IEnumerable<VehicleSpecialty> specialties)
        {
            if (specialties == null)
            {
                throw new ArgumentNullException("specialties");
            }

            var result = _QueryObjects(
                specialties,
                ids =>
                    from vehicle in _dataContext.Vehicles
                    from specialty in vehicle.VehicleSpecialties
                    where !vehicle.Deleted && specialty.Id != null && ids.Contains(specialty.Id)
                    select specialty.Id);

            return result;
        }

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more order.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more order.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        public IEnumerable<DriverSpecialty> QueryOrderSpecialties(
            IEnumerable<DriverSpecialty> specialties)
        {
            if (specialties == null)
            {
                throw new ArgumentNullException("specialties");
            }

            var result = _QueryObjects(
                specialties,
                ids =>
                    from order in _dataContext.Orders
                    from specialty in order.DriverSpecialties
                    where specialty.Id != null && ids.Contains(specialty.Id)
                    select specialty.Id);

            return result;
        }

        /// <summary>
        /// Filters the specified collection of specialties by returning ones associated with one or
        /// more order.
        /// </summary>
        /// <param name="specialties">Collection of specialties to be filtered.</param>
        /// <returns>A collection of specialties associated with one or more order.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="specialties"/> is a null
        /// reference.</exception>
        public IEnumerable<VehicleSpecialty> QueryOrderSpecialties(
            IEnumerable<VehicleSpecialty> specialties)
        {
            if (specialties == null)
            {
                throw new ArgumentNullException("specialties");
            }

            var result = _QueryObjects(
                specialties,
                ids =>
                    from order in _dataContext.Orders
                    from specialty in order.VehicleSpecialties
                    where specialty.Id != null && ids.Contains(specialty.Id)
                    select specialty.Id);

            return result;
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Queries objects associated with routes using the specified selector function filtering
        /// ones with the specified IDs.
        /// </summary>
        /// <param name="routes">Collection of routes to query objects for.</param>
        /// <param name="ids">Collection of IDs of route objects to be queried.</param>
        /// <param name="selector">A function projecting route to associated object.</param>
        /// <returns>Collection of IDs of objects associated with the specified routes.</returns>
        private static IQueryable<Guid> _QueryRouteObjects(
            IQueryable<DataModel.Routes> routes,
            IEnumerable<Guid> ids,
            Expression<Func<DataModel.Routes, DataModel.IDataObject>> selector)
        {
            Debug.Assert(routes != null);
            Debug.Assert(ids != null);
            Debug.Assert(selector != null);

            return routes
                .Select(selector)
                .Where(obj => obj.Id != null && ids.Contains(obj.Id))
                .Select(obj => obj.Id);
        }

        /// <summary>
        /// Queries objects associated with routes using the specified selector function filtering
        /// ones with the specified IDs.
        /// </summary>
        /// <param name="routes">Collection of routes to query objects for.</param>
        /// <param name="ids">Collection of IDs of route objects to be queried.</param>
        /// <param name="selector">A function projecting route to a collection of associated
        /// objects.</param>
        /// <returns>Collection of IDs of objects associated with the specified routes.</returns>
        private static IQueryable<Guid> _QueryRouteObjects(
            IQueryable<DataModel.Routes> routes,
            IEnumerable<Guid> ids,
            Expression<Func<DataModel.Routes, IEnumerable<DataModel.IDataObject>>> selector)
        {
            Debug.Assert(routes != null);
            Debug.Assert(ids != null);
            Debug.Assert(selector != null);

            return routes
                .SelectMany(selector)
                .Where(obj => obj.Id != null && ids.Contains(obj.Id))
                .Select(obj => obj.Id);
        }

        /// <summary>
        /// Queries locations associated with the specified routes.
        /// </summary>
        /// <param name="routes">Collection of routes to query locations for.</param>
        /// <param name="locations">Collection of locations to be used for filtering route
        /// locations.</param>
        /// <returns>A collection of locations associated with the specified routes and having ids
        /// same as of the specified locations.</returns>
        private static IEnumerable<Location> _QueryRouteLocations(
            IQueryable<DataModel.Routes> routes,
            IEnumerable<Location> locations)
        {
            Debug.Assert(routes != null);
            Debug.Assert(locations != null);

            var result = _QueryObjects(
                locations,
                ids =>
                {
                    var endLocations = _QueryRouteObjects(
                        routes,
                        ids,
                        route => route.Locations);
                    var startLocations = _QueryRouteObjects(
                        routes,
                        ids,
                        route => route.Locations1);
                    var renewalLocations = _QueryRouteObjects(
                        routes,
                        ids,
                        route => route.Locations2);

                    return startLocations.Union(endLocations).Union(renewalLocations);
                });

            return result;
        }

        /// <summary>
        /// Runs the specified query providing it a collection of IDs extracted from the specified
        /// data objects and converting received IDs back to data objects.
        /// </summary>
        /// <typeparam name="T">The type of data objects to run query for.</typeparam>
        /// <param name="objects">Collection of data objects to be used as a source for
        /// query.</param>
        /// <param name="query">The query to be run.</param>
        /// <returns>A collection of objects filtered by the query.</returns>
        private static IEnumerable<T> _QueryObjects<T>(
            IEnumerable<T> objects,
            Func<IEnumerable<Guid>, IQueryable<Guid>> query)
            where T : DataObject
        {
            Debug.Assert(objects != null);
            Debug.Assert(query != null);

            var objectsByIDs = objects.ToLookup(obj => obj.Id);
            var ids = objectsByIDs.Select(_ => _.Key).ToList();

            var result = query(ids);

            return result
                .AsEnumerable()
                .SelectMany(id => objectsByIDs[id]);
        }
        #endregion

        #region private methods
        /// <summary>
        /// Gets query for retrieving collection of default routes.
        /// </summary>
        /// <returns>A query for retrieving default routes.</returns>
        private IQueryable<DataModel.Routes> _QueryDefaultRoutes()
        {
            return _dataContext.Routes.Where(route => route.Default);
        }

        /// <summary>
        /// Gets query for retrieving collection of routes planned for the specified date or
        /// any date after it.
        /// </summary>
        /// <param name="current">The date to be used as a lower bound for searching for planned
        /// routes.</param>
        /// <returns>A query for retrieving routes planned for the specified date or
        /// any date after it.</returns>
        private IQueryable<DataModel.Routes> _QueryFutureRoutes(DateTime current)
        {
            return _dataContext.Routes.Where(route => route.Schedules.PlannedDate >= current.Date);
        }

        /// <summary>
        /// Filters the specified collection of objects associated with one or default routes.
        /// </summary>
        /// <typeparam name="T">The type of objects to be filtered.</typeparam>
        /// <param name="objects">Collection of objects to be filtered.</param>
        /// <param name="selector">The function for projecting route into associated object.</param>
        /// <returns>A collection of objects associated with one or more default route.</returns>
        private IEnumerable<T> _QueryDefaultRouteObjects<T>(
            IEnumerable<T> objects,
            Expression<Func<DataModel.Routes, DataModel.IDataObject>> selector)
            where T : DataObject
        {
            Debug.Assert(objects != null);
            Debug.Assert(selector != null);

            var result = _QueryObjects(
                objects,
                ids => _QueryRouteObjects(_QueryDefaultRoutes(), ids, selector));

            return result;
        }

        /// <summary>
        /// Filters the specified collection of objects associated with one or more route planned
        /// for the specified date or any date after it.
        /// </summary>
        /// <typeparam name="T">The type of objects to be filtered.</typeparam>
        /// <param name="objects">Collection of objects to be filtered.</param>
        /// <param name="current">The date to be used as a lower bound for searching for planned
        /// routes.</param>
        /// <param name="selector">The function for projecting route into associated object.</param>
        /// <returns>A collection of objects associated with one or more route planned for
        /// the specified date or any date after it.</returns>
        private IEnumerable<T> _QueryFutureRouteObjects<T>(
            IEnumerable<T> objects,
            DateTime current,
            Expression<Func<DataModel.Routes, DataModel.IDataObject>> selector)
            where T : DataObject
        {
            Debug.Assert(objects != null);
            Debug.Assert(selector != null);

            var result = _QueryObjects(
                objects,
                ids => _QueryRouteObjects(_QueryFutureRoutes(current), ids, selector));

            return result;
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the data context to be used for accessing project data.
        /// </summary>
        private DataObjectContext _dataContext;
        #endregion
    }
}
