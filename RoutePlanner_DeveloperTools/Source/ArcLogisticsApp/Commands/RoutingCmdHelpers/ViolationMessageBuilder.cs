using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Routing;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Class builds messages dependent on violation.
    /// </summary>
    internal static class ViolationMessageBuilder
    {
        /// <summary>
        /// Method returns collection of violation message details.
        /// </summary>
        /// <param name="schedule">Schedule to be checked.</param>
        /// <param name="info">Solver's asyncrone operation info.</param>
        /// <param name="violations">Founded violations.</param>
        /// <returns>Message detail with description for violations.</returns>
        public static ICollection<MessageDetail> GetViolationDetails(Schedule schedule,
                                                                     AsyncOperationInfo info,
                                                                     ICollection<Violation> violations)
        {
            Debug.Assert(null != schedule);
            Debug.Assert(0 < violations.Count);

            // special case
            bool isSpecialCase = false;
            Order orderToAssign = null;
            if (SolveOperationType.AssignOrders == info.OperationType)
            {
                AssignOrdersParams param = (AssignOrdersParams)info.InputParams;
                if (null != param.TargetSequence)
                    isSpecialCase = param.TargetSequence.HasValue;

                if (isSpecialCase)
                {
                    Debug.Assert(1 == param.OrdersToAssign.Count);
                    orderToAssign = param.OrdersToAssign.First();
                }
            }

            ICollection<MessageDetail> details = null;
            if (!isSpecialCase && _IsRouteRelatedViolationPresent(violations))
                details = _GetRouteViolationDetails(violations);
            else
            {
                ICollection<Route> routes = ViolationsHelper.GetRoutingCommandRoutes(schedule, info);
                Debug.Assert((null != routes) && (0 < routes.Count));

                details = _GetViolationDetails(routes, isSpecialCase, orderToAssign, violations);
            }

            return details;
        }

        #region Private methods
        /// <summary>
        /// Creates violation message for Order or Route.
        /// </summary>
        /// <param name="obj">Violation's object (Order or Route).</param>
        /// <param name="resourceStringName">Description message format resource name.</param>
        /// <param name="type">Message type.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetViolationMessage(DataObject obj, string resourceStringName,
                                                          MessageType type)
        {
            Debug.Assert((obj is Order) || (obj is Route));
            string format = App.Current.FindString(resourceStringName);
            return new MessageDetail(type, format, obj);
        }

        /// <summary>
        /// Creates violation message for Order or Route.
        /// </summary>
        /// <param name="obj">Violation's object (Order or Route).</param>
        /// <param name="resourceStringName">Description message format resource name.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetViolationMessage(DataObject obj, string resourceStringName)
        {
            MessageType type = (obj is Route)? MessageType.Error : MessageType.Warning;
            return _GetViolationMessage(obj, resourceStringName, type);
        }

        /// <summary>
        /// Creates violation message for Order or Location.
        /// </summary>
        /// <param name="obj">Violation's object (Order or Location).</param>
        /// <param name="resourceStringNameOrder">Description message format resource name for order.</param>
        /// <param name="resourceStringNameLocation">Description message format resource name for location.</param>
        /// <param name="type">Message type.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetOrderOrLocationViolationMessage(DataObject obj,
                                                                         string resourceStringNameOrder,
                                                                         string resourceStringNameLocation,
                                                                         MessageType type)
        {
            Debug.Assert((obj is Order) || (obj is Location));
            string formatRsc = (obj is Order) ? resourceStringNameOrder : resourceStringNameLocation;
            return new MessageDetail(type, App.Current.FindString(formatRsc), obj);
        }

        /// <summary>
        /// Creates violation message for routes' related object.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="routes">Routes with violations.</param>
        /// <param name="resourceStringNameSingle">Description message format resource name for single route.</param>
        /// <param name="resourceStringNameMulty">Description message format resource name for multy route.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetViolationMessage(DataObject obj, ICollection<Route> routes,
                                                          string resourceStringNameSingle,
                                                          string resourceStringNameMulty)
        {
            Debug.Assert(!string.IsNullOrEmpty(resourceStringNameSingle));

            bool checkOneRoute = (1 == routes.Count);
            int paramCount = 1;
            if (checkOneRoute)
                ++paramCount;
            DataObject[] param = new DataObject[paramCount];

            int startIndex = 0;
            if (checkOneRoute)
                param[startIndex++] = routes.First();
            param[startIndex++] = obj;

            string formatResource = (checkOneRoute) ? resourceStringNameSingle : resourceStringNameMulty;
            Debug.Assert(!string.IsNullOrEmpty(formatResource));

            string format = App.Current.FindString(formatResource);
            return new MessageDetail(MessageType.Warning, format, param);
        }

        /// <summary>
        /// Creates violation message for route's related object.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="routes">Routes with violations (must be 1 route in collection).</param>
        /// <param name="resourceStringNameSingle">Description message format resource name.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetViolationMessage(DataObject obj, ICollection<Route> routes,
                                                          string resourceStringName)
        {
            Debug.Assert(1 == routes.Count);
            return _GetViolationMessage(obj, routes, resourceStringName, null);
        }

        /// <summary>
        /// Creates violation message for related object collection.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="inputCollectionCount">Collection's object count.</param>
        /// <param name="violatedObjCollection">Violated object collection.</param>
        /// <param name="resourceStringNameSingle">Description message format resource name for single object.</param>
        /// <param name="resourceStrNameList">Description message format resource name for object's name list.</param>
        /// <param name="resourceStrNameAll">Description message format resource name for all object's.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetViolationMessage<T>(DataObject obj, int inputCollectionCount,
                                                             ICollection<T> violatedObjCollection,
                                                             string resourceStrNameSingle,
                                                             string resourceStrNameList,
                                                             string resourceStrNameAll)
            where T : DataObject
        {
            return _GetViolationMessageStr(obj, inputCollectionCount, violatedObjCollection,
                                           App.Current.FindString(resourceStrNameSingle),
                                           App.Current.FindString(resourceStrNameList),
                                           App.Current.FindString(resourceStrNameAll));
        }

        /// <summary>
        /// Creates violation message for related object collection.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="inputCollectionCount">Collection's object count.</param>
        /// <param name="violatedObjCollection">Violated object collection.</param>
        /// <param name="formatSingle">Description message format for single object.</param>
        /// <param name="formatList">Description message format for object's name list.</param>
        /// <param name="formatAll">Description message format for all object's.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetViolationMessageStr<T>(DataObject obj, int inputCollectionCount,
                                                                ICollection<T> violatedObjCollection,
                                                                string formatSingle, string formatList,
                                                                string formatAll)
            where T : DataObject
        {
            bool checkOneRoute = (1 == violatedObjCollection.Count);
            bool checkNotAll = checkOneRoute ? false : (violatedObjCollection.Count < inputCollectionCount);

            int paramCount = 1;
            if (checkOneRoute)
                ++paramCount;
            else if (checkNotAll)
                paramCount += violatedObjCollection.Count;
            DataObject[] param = new DataObject[paramCount];

            string format = null;
            int index = 0;
            if (checkOneRoute)
            {
                param[index++] = violatedObjCollection.First();
                format = formatSingle;
            }
            else if (checkNotAll)
            {
                string listFormat = ViolationsHelper.GetObjectListFormat(violatedObjCollection, ref index, param);
                string substitution = "{" + index.ToString() + "}";
                format = string.Format(formatList, listFormat, substitution);
            }
            else
                format = formatAll;
            param[index++] = obj;

            return new MessageDetail(MessageType.Warning, format, param);
        }

        /// <summary>
        /// Creates MaxOrderCount's violation message.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="routes">Routes with violations.</param>
        /// <returns>Created MaxOrderCount's violation message.</returns>
        private static MessageDetail _GetMaxOrderCountViolationMessage(DataObject obj, ICollection<Route> routes)
        {
            var routeWithViolation = routes.Where(route => route.OrderCount >= route.MaxOrders);
            return _GetViolationMessage(obj, routes.Count, routeWithViolation.ToList(),
                                        "MaxOrdersCountSingleRouteViolationMessage",
                                        "MaxOrdersCountListRoutesViolationMessage",
                                        "MaxOrdersCountAnyRoutesViolationMessage");
        }

        /// <summary>
        /// Creates violation message for capacities.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="routes">Routes with violations.</param>
        /// <returns>Created violation messages for capacities.</returns>
        private static ICollection<MessageDetail> _GetCapacitiesViolationMessages(DataObject obj, ICollection<Route> routes)
        {
            Collection<MessageDetail> details = new Collection<MessageDetail>();
            Order order = obj as Order;
            if (null != order)
            {
                for (int cap = 0; cap < App.Current.Project.CapacitiesInfo.Count; ++cap)
                {
                    Collection<Vehicle> vehicles = new Collection<Vehicle>();
                    foreach (Route route in routes)
                    {
                        if ((null != route.RenewalLocations) && (0 < route.RenewalLocations.Count))
                        {
                            if (route.Vehicle.Capacities[cap] < order.Capacities[cap])
                                vehicles.Add(route.Vehicle);
                        }
                        else
                        {
                            if (route.Vehicle.Capacities[cap] - route.Capacities[cap] < order.Capacities[cap])
                                vehicles.Add(route.Vehicle);
                        }
                    }

                    if (0 < vehicles.Count)
                    {
                        Route route = routes.First();
                        string formatSingle = App.Current.GetString("CapacitySingleVehicleViolationMessage", route.CapacitiesInfo[cap].Name, "{0}", "{1}");
                        string formatList =App.Current.GetString("CapacityListVehicleViolationMessage", route.CapacitiesInfo[cap].Name, "{0}", "{1}");
                        string formatAll = App.Current.GetString("CapacityAnyVehiclesViolationMessage", route.CapacitiesInfo[cap].Name, "{0}");
                        details.Add(_GetViolationMessageStr(obj, routes.Count, vehicles, formatSingle, formatList, formatAll));
                    }
                }
            }

            return details;
        }

        /// <summary>
        /// Creates violation message for route's capacities.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <returns>Created violation messages for route's capacities.</returns>
        private static ICollection<MessageDetail> _GetCapacitiesRouteViolationMessages(DataObject obj)
        {
            Debug.Assert(obj is Route);

            Route route = obj as Route;

            CapacitiesInfo info = App.Current.Project.CapacitiesInfo;
            Capacities stopsCapacities = new Capacities(info);
            foreach (Stop stop in route.Stops)
            {
                if (StopType.Order == stop.StopType)
                {
                    Debug.Assert(null != stop.AssociatedObject);

                    Order order = stop.AssociatedObject as Order;
                    for (int cap = 0; cap < info.Count; ++cap)
                        stopsCapacities[cap] += order.Capacities[cap];
                }
            }

            Collection<MessageDetail> details = new Collection<MessageDetail>();
            for (int cap = 0; cap < App.Current.Project.CapacitiesInfo.Count; ++cap)
            {
                if (route.Vehicle.Capacities[cap] < stopsCapacities[cap])
                {
                    string format = App.Current.GetString("CapacityRouteViolationMessage", "{0}", route.CapacitiesInfo[cap].Name);
                    details.Add(new MessageDetail(MessageType.Error, format, route));
                }
            }

            return details;
        }

        /// <summary>
        /// Creates violation message for specialities.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="routes">Routes with violations.</param>
        /// <returns>Created violation messages for specialities.</returns>
        private static ICollection<MessageDetail> _GetSpecialtiesViolationMessages(DataObject obj,
                                                                                   ICollection<Route> routes)
        {
            Collection<MessageDetail> details = new Collection<MessageDetail>();

            Order order = obj as Order;
            if (null == order)
                return details; // NOTE: not supported values

            // get order specialities
            IDataObjectCollection<DriverSpecialty> driverSpecs = order.DriverSpecialties;
            IDataObjectCollection<VehicleSpecialty> vehicleSpecs = order.VehicleSpecialties;

            // create list of unique objects
            Collection<Driver> drivers = new Collection<Driver>();
            Collection<Vehicle> vehicles = new Collection<Vehicle>();
            foreach (Route route in routes)
            {
                Driver driver = route.Driver;
                if (!drivers.Contains(driver))
                    drivers.Add(driver);

                Vehicle vehicle = route.Vehicle;
                if (!vehicles.Contains(vehicle))
                    vehicles.Add(vehicle);
            }

            // get list of objects whiteout specialities
            var driversWhitoutSpecEnum =
                from driver in drivers
                let specialties = driver.Specialties
                from spec in driverSpecs
                where !specialties.Contains(spec)
                select driver;
            var driversWhitoutSpec = driversWhitoutSpecEnum.ToList();

            var vehiclesWhitoutSpecEnum =
                from vehicle in vehicles
                let specialties = vehicle.Specialties
                from spec in vehicleSpecs
                where !specialties.Contains(spec)
                select vehicle;
            var vehiclesWhitoutSpec = vehiclesWhitoutSpecEnum.ToList();

            if (0 < driversWhitoutSpec.Count)
            {
                details.Add(_GetViolationMessage(obj, drivers.Count, driversWhitoutSpec,
                                                 "DriverSpecialitiesSingleViolationMessage",
                                                 "DriverSpecialitiesListViolationMessage",
                                                 "DriverSpecialitiesAnyViolationMessage"));
            }

            if (0 < vehiclesWhitoutSpec.Count)
            {
                details.Add(_GetViolationMessage(obj, vehicles.Count, vehiclesWhitoutSpec,
                                                 "VehicleSpecialitiesSingleViolationMessage",
                                                 "VehicleSpecialitiesListViolationMessage",
                                                 "VehicleSpecialitiesAnyViolationMessage"));
            }

            return details;
        }

        /// <summary>
        /// Checks is route related violation present.
        /// </summary>
        /// <param name="violations">Detected violation's.</param>
        /// <returns>TRUE if route violation present.</returns>
        private static bool _IsRouteRelatedViolationPresent(ICollection<Violation> violations)
        {
            return (0 < _GetRouteViolations(violations).Count);
        }

        /// <summary>
        /// Filtr unreachable violations
        ///     special case {CR122194 - [JIRA] ARCLOGISTICS-900}
        ///         prefiltering messageges if present ViolationType.Unreachable other must be ignored.
        /// </summary>
        /// <param name="violations">All violations.</param>
        /// <returns>Filtred violations.</returns>
        private static ICollection<Violation> _FiltrUnreachable(ICollection<Violation> violations)
        {
            var unreachableViolations = violations.Where(violation => ViolationType.Unreachable == violation.ViolationType);

            Collection<Violation> filtratedViolations = new Collection<Violation>();
            foreach (Violation violation in violations)
            {
                if (ViolationType.Unreachable == violation.ViolationType)
                    filtratedViolations.Add(violation);
                else
                {
                    bool isSomeObject = false;
                    if (null != violation.AssociatedObject)
                    {
                        foreach (Violation unreachableViolation in unreachableViolations)
                        {
                            if ((null != unreachableViolation.AssociatedObject) &&
                                unreachableViolation.AssociatedObject.Equals(violation.AssociatedObject))
                            {
                                isSomeObject = true;
                                break;
                            }
                        }
                    }

                    if (!isSomeObject)
                        filtratedViolations.Add(violation);
                }
            }

            return filtratedViolations;
        }

        /// <summary>
        /// Gets violations for route.
        /// </summary>
        /// <param name="violations">All detected violations.</param>
        /// <returns>Violations for route.</returns>
        private static ICollection<Violation> _GetRouteViolations(ICollection<Violation> violations)
        {
            var routeViolations = violations.Where(violation => violation.AssociatedObject is Route);
            return routeViolations.ToList();
        }

        /// <summary>
        /// Gets related objects form violations for specified type.
        /// </summary>
        /// <param name="type">Violation's type.</param>
        /// <param name="violations">All detected violations.</param>
        /// <returns>Related objects form violations for specified type.</returns>
        private static ICollection<DataObject> _GetRelatedObjects(ViolationType type, ICollection<Violation> violations)
        {
            var objects =
                from violation in violations
                where !(violation.AssociatedObject is Route) && (type == violation.ViolationType)
                select violation.AssociatedObject;
            return objects.ToList();
        }

        /// <summary>
        /// Creates violation message for related object collection.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="relatedObjects">Related object's.</param>
        /// <param name="resourceStringName">Description message format resource name.</param>
        /// <returns>Created message detail.</returns>
        private static MessageDetail _GetRouteViolationMessageStr(DataObject obj, ICollection<DataObject> relatedObjects,
                                                                  string resourceStringName)
        {
            Debug.Assert(obj is Route);

            int paramCount = 1 + relatedObjects.Count; // route + all objects
            DataObject[] param = new DataObject[paramCount];

            int index = 0;
            string substitution = "{" + index.ToString() + "}";
            param[index++] = obj;
            string relatedObjFormat = ViolationsHelper.GetObjectListFormat(relatedObjects, ref index, param);
            string format = App.Current.GetString(resourceStringName, substitution, relatedObjFormat);

            return new MessageDetail(MessageType.Error, format, param);
        }

        /// <summary>
        /// Creates violation messages for route's specialities.
        /// </summary>
        /// <param name="obj">Violation's object (can be any DataObject: Vehicle, Driver and etc).</param>
        /// <param name="relatedObjects">Related object's.</param>
        /// <returns>Created violation messages for route's specialities.</returns>
        private static ICollection<MessageDetail> _GetSpecialitiesRouteViolationMessages(DataObject obj,
                                                                                         ICollection<DataObject> relatedObjects)
        {
            Debug.Assert(obj is Route);

            Route route = obj as Route;
            IDataObjectCollection<VehicleSpecialty> routeVehSpecialties = route.Vehicle.Specialties;
            IDataObjectCollection<DriverSpecialty> routeDrvSpecialties = route.Driver.Specialties;

            Collection<DataObject> ordersVehSpecNotSupported = new Collection<DataObject>();
            Collection<DataObject> ordersDrvSpecNotSupported = new Collection<DataObject>();
            foreach (DataObject relatedObj in relatedObjects)
            {
                Order order = relatedObj as Order;
                if (null != order)
                {
                    foreach (DriverSpecialty specialty in order.DriverSpecialties)
                    {
                        if (!routeDrvSpecialties.Contains(specialty))
                            ordersDrvSpecNotSupported.Add(specialty);
                    }

                    foreach (VehicleSpecialty specialty in order.VehicleSpecialties)
                    {
                        if (!routeVehSpecialties.Contains(specialty))
                            ordersVehSpecNotSupported.Add(specialty);
                    }
                }
            }

            Collection<MessageDetail> details = new Collection<MessageDetail>();
            if (0 < ordersDrvSpecNotSupported.Count)
                details.Add(_GetRouteViolationMessageStr(obj, ordersDrvSpecNotSupported, "DriverSpecialitiesRouteViolationMessage"));
            if (0 < ordersVehSpecNotSupported.Count)
                details.Add(_GetRouteViolationMessageStr(obj, ordersVehSpecNotSupported, "VehicleSpecialitiesRouteViolationMessage"));

            return details;
        }

        /// <summary>
        /// Creates violation's description messages.
        /// </summary>
        /// <param name="routes">Routes with violations.</param>
        /// <param name="isSpecialCase">Is special case flag.</param>
        /// <param name="orderToAssign">Order to assign (can be null).</param>
        /// <param name="violations">Detected violations.</param>
        /// <returns>Created violation's details.</returns>
        private static ICollection<MessageDetail> _GetViolationDetails(ICollection<Route> routes,
                                                                       bool isSpecialCase, Order orderToAssign,
                                                                       ICollection<Violation> violations)
        {
            ICollection<Violation> filtredViolations = _FiltrUnreachable(violations);

            if (isSpecialCase)
            {
                bool hasSpecialCaseViolations = ((null != orderToAssign) &&
                                                 _IsRouteRelatedViolationPresent(filtredViolations));
                if (!hasSpecialCaseViolations)
                    isSpecialCase = false;
            }

            List<MessageDetail> details = new List<MessageDetail>();
            foreach (Violation violation in filtredViolations)
            {
                DataObject obj = (isSpecialCase)? orderToAssign : violation.AssociatedObject;
                if (null == obj)
                    continue; // NOTE: ignore empty values
                if (isSpecialCase && !(violation.AssociatedObject is Route))
                    continue; // NOTE: skip solver duplicate message (simple filtration)

                switch (violation.ViolationType)
                {
                    case ViolationType.MaxOrderCount:
                        details.Add(_GetMaxOrderCountViolationMessage(obj, routes));
                        break;

                    case ViolationType.Capacities:
                        details.AddRange(_GetCapacitiesViolationMessages(obj, routes));
                        break;

                    case ViolationType.MaxTotalDuration:
                        details.Add(_GetViolationMessage(obj, routes, "MaxTotalTimeSingleRouteViolationMessage",
                                                         "MaxTotalTimeAnyRoutesViolationMessage"));
                        break;

                    case ViolationType.MaxTravelDuration:
                        details.Add(_GetViolationMessage(obj, routes, "MaxTotalTravelTimeSingleRouteViolationMessage",
                                                         "MaxTotalTravelTimeAnyRoutesViolationMessage"));
                        break;

                    case ViolationType.MaxTotalDistance:
                        details.Add(_GetViolationMessage(obj, routes, "MaxTotalDistanceSingleRouteViolationMessage",
                                                         "MaxTotalDistanceAnyRoutesViolationMessage"));
                        break;

                    case ViolationType.HardTimeWindow:
                        details.Add(_GetViolationMessage(obj, "HardTimeWindowOrderViolationMessage"));
                        break;

                    case ViolationType.Specialties:
                        details.AddRange(_GetSpecialtiesViolationMessages(obj, routes));
                        break;

                    case ViolationType.Zone:
                        details.Add(_GetViolationMessage(obj, routes, "ZonesSingleRouteViolationMessage",
                                                         "ZonesAnyRoutesViolationMessage"));
                        break;

                    case ViolationType.Unreachable:
                        details.Add(_GetViolationMessage(obj, "UnreachableViolationMessage"));
                        break;

                    case ViolationType.BreakRequired:
                        details.Add(_GetViolationMessage(obj, routes, "BreakRequiredRouteOrderViolationMessage"));
                        break;

                    case ViolationType.RenewalRequired:
                        details.Add(_GetViolationMessage(obj, routes, "RenewalRequiredRouteOrderViolationMessage"));
                        break;

                    case ViolationType.BreakMaxTravelTime:
                        details.Add(_GetViolationMessage(obj, routes,
                            "BreakMaxTravelTimeSingleRouteViolationMessage",
                            "BreakMaxTravelTimeAnyRoutesViolationMessage"));
                        break;

                    case ViolationType.BreakMaxCumulWorkTimeExceeded:
                        details.Add(_GetViolationMessage(obj, routes,
                            "BreakMaxCumulWorkTimeSingleRouteViolationMessage",
                            "BreakMaxCumulWorkTimeAnyRoutesViolationMessage"));
                        break;

                    case ViolationType.TooFarFromRoad:
                        details.Add(_GetOrderOrLocationViolationMessage(obj, "OrderNotFoundOnNetworkViolationMessage",
                                                                        "LocationNotFoundOnNetworkViolationMessage",
                                                                        MessageType.Warning));
                        break;

                    case ViolationType.RestrictedStreet:
                    {
                        MessageType type = (obj is Location)? MessageType.Error : MessageType.Warning;
                        details.Add(_GetOrderOrLocationViolationMessage(obj, "OrderRestrictedStreetViolationMessage",
                                                                        "LocationRestrictedStreetViolationMessage",
                                                                        type));
                        break;
                    }

                    case ViolationType.Ungeocoded:
                        details.Add(_GetViolationMessage(obj, "OrderIsNotGeocodedFormat", MessageType.Warning));
                        break;

                    default:
                        details.AddRange(_GetRouteViolationDetails(violation, violations));
                        break;
                }
            }

            return details;
        }

        /// <summary>
        /// Creates violation's description messages for route.
        /// </summary>
        /// <param name="violations">Detected violations.</param>
        /// <returns>Created violation's details for route.</returns>
        private static ICollection<MessageDetail> _GetRouteViolationDetails(ICollection<Violation> violations)
        {
            List<MessageDetail> details = new List<MessageDetail>();

            ICollection<Violation> routeViolations = _GetRouteViolations(violations);
            foreach (Violation violation in routeViolations)
                details.AddRange(_GetRouteViolationDetails(violation, violations));

            return details;
        }

        /// <summary>
        /// Creates description messages for route violation.
        /// </summary>
        /// <param name="violation">The route violation to create description
        /// message for.</param>
        /// <param name="violations">The collection of all violations containing
        /// related violations for the specified route one.</param>
        /// <returns>A collection of description messages for the specified
        /// violation.</returns>
        private static IEnumerable<MessageDetail> _GetRouteViolationDetails(Violation violation,
                                                                            ICollection<Violation> violations)
        {
            Debug.Assert(violation != null);
            Debug.Assert(violations != null);

            var details = new List<MessageDetail>();

            ICollection<DataObject> relatedObjects = null;
            if ((ViolationType.Specialties == violation.ViolationType) ||
                (ViolationType.Unreachable == violation.ViolationType) ||
                (ViolationType.Zone == violation.ViolationType))
                relatedObjects = _GetRelatedObjects(violation.ViolationType, violations);

            DataObject obj = violation.AssociatedObject;
            Debug.Assert(obj is Route);

            switch (violation.ViolationType)
            {
                case ViolationType.MaxOrderCount:
                    details.Add(_GetViolationMessage(obj, "MaxOrdersCountRouteViolationMessage"));
                    break;

                case ViolationType.Capacities:
                    details.AddRange(_GetCapacitiesRouteViolationMessages(obj));
                    break;

                case ViolationType.MaxTotalDuration:
                    details.Add(_GetViolationMessage(obj, "MaxTotalTimeRouteViolationMessage"));
                    break;

                case ViolationType.MaxTravelDuration:
                    details.Add(_GetViolationMessage(obj, "MaxTotalTravelTimeRouteViolationMessage"));
                    break;

                case ViolationType.MaxTotalDistance:
                    details.Add(_GetViolationMessage(obj, "MaxTotalDistanceRouteViolationMessage"));
                    break;

                case ViolationType.HardTimeWindow:
                    details.Add(_GetViolationMessage(obj, "HardTimeWindowRouteViolationMessage"));
                    break;

                case ViolationType.Specialties:
                    details.AddRange(_GetSpecialitiesRouteViolationMessages(obj, relatedObjects));
                    break;

                case ViolationType.Zone:
                    details.Add(_GetRouteViolationMessageStr(obj, relatedObjects, "ZonesRouteViolationMessage"));
                    break;

                case ViolationType.Unreachable:
                    details.Add(_GetRouteViolationMessageStr(obj, relatedObjects, "UnreachableRouteViolationMessage"));
                    break;

                case ViolationType.EmptyMaxTotalDuration:
                    details.Add(_GetViolationMessage(obj, "EmptyMaxTotalTimeViolationMessage"));
                    break;

                case ViolationType.EmptyMaxTravelDuration:
                    details.Add(_GetViolationMessage(obj, "EmptyMaxTotalTravelTimeViolationMessage"));
                    break;

                case ViolationType.EmptyMaxTotalDistance:
                    details.Add(_GetViolationMessage(obj, "EmptyMaxTotalDistanceViolationMessage"));
                    break;

                case ViolationType.EmptyHardTimeWindow:
                    details.Add(_GetViolationMessage(obj, "EmptyHardTimeWindowViolationMessage"));
                    break;

                case ViolationType.EmptyUnreachable:
                    details.Add(_GetViolationMessage(obj, "EmptyUnreachableViolationMessage"));
                    break;

                case ViolationType.BreakRequired:
                    details.Add(_GetViolationMessage(obj, "BreakRequiredRouteViolationMessage"));
                    break;

                case ViolationType.RenewalRequired:
                    details.Add(_GetViolationMessage(obj, "RenewalRequiredRouteViolationMessage"));
                    break;

                case ViolationType.EmptyBreakMaxTravelTime:
                    details.Add(_GetViolationMessage(obj, "EmptyBreaksMaxTravelTimeViolationMessage"));
                    break;

                case ViolationType.BreakMaxTravelTime:
                    details.Add(_GetViolationMessage(obj, "BreakMaxTravelTimeRouteViolationMessage"));
                    break;

                case ViolationType.BreakMaxCumulWorkTimeExceeded:
                    details.Add(_GetViolationMessage(obj, "BreakMaxCumulWorkTimeRouteViolationMessage"));
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return details;
        }

        #endregion // Private methods
    }
}
