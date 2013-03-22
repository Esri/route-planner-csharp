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
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;

using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.DomainObjects;
using AppData = ESRI.ArcLogistics.Data;
using AppPages = ESRI.ArcLogistics.App.Pages;
using AppGeometry = ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Class that manages store objects to current project.
    /// </summary>
    internal sealed class Storage
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>Storage</c> class.
        /// </summary>
        public Storage()
        {}

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Count of new created objects.
        /// Valid after AddToProject call.
        /// </summary>
        public int CreatedCount
        {
            get { return _createdCount; }
        }

        /// <summary>
        /// Count of valid objects.
        /// Valid after AddToProject call.
        /// </summary>
        public int ValidCount
        {
            get { return _validCount; }
        }

        /// <summary>
        /// Store procedure detail list.
        /// Valid after AddToProject call.
        /// </summary>
        public IList<MessageDetail> Details
        {
            get { return _details; }
        }

        /// <summary>
        /// Updated objects in project.
        /// Valid after AddToProject call.
        /// </summary>
        public IList<AppData.DataObject> UpdatedObjects
        {
            get { return _updatedObjects; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds objects to project.
        /// </summary>
        /// <param name="objects">Objects to adding.</param>
        /// <param name="objectName">Object name to message generation.</param>
        /// <param name="objectsName">Objects name to message generation.</param>
        /// <returns></returns>
        public bool AddToProject(IList<AppData.DataObject> objects,
                                 string objectName,
                                 string objectsName)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(!string.IsNullOrEmpty(objectName)); // not empty
            Debug.Assert(!string.IsNullOrEmpty(objectsName)); // not empty

            // reset internal state first
            _validCount = 0;
            _createdCount = 0;
            _details.Clear();
            _updatedObjects.Clear();

            // reinit values
            _objectName = objectName;
            _objectsName = objectsName;

            // do process
            bool result = _AddToProject(objects);
            return result;
        }

        #endregion Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates list of unique dates from orders.
        /// </summary>
        /// <param name="objects">Object collection (orders).</param>
        /// <returns>Collection of unique dates for input orders.</returns>
        private IList<DateTime> _CreateOrdersUniqueDateList(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(typeof(Order) == objects[0].GetType()); // check supported type

            var uniqueDates = new List<DateTime>();
            foreach (Order order in objects)
            {
                Debug.Assert(order.PlannedDate.HasValue); // valid state
                if (!uniqueDates.Contains(order.PlannedDate.Value))
                    uniqueDates.Add(order.PlannedDate.Value);
            }

            return uniqueDates;
        }

        /// <summary>
        /// Gets orders for predefined date.
        /// </summary>
        /// <param name="orders">Object collection (orders).</param>
        /// <param name="date">Date to filtring.</param>
        /// <returns>Filtred by date input collection.</returns>
        IEnumerable _CreateOrderListByDate(IList<Order> orders, DateTime date)
        {
            Debug.Assert(null != orders); // created
            Debug.Assert(0 < orders.Count); // not empty
            Debug.Assert(typeof(Order) == orders[0].GetType()); // supported type

            var orderToDate = 
                from Order order in orders
                where order.PlannedDate == date
                select order;

            return orderToDate;
        }

        /// <summary>
        /// Stores validate result.
        /// </summary>
        /// <param name="obj">Object to validation.</param>
        private void _StoreValidateResult(AppData.DataObject obj)
        {
            Debug.Assert(null != obj); // created

            string error = obj.Error;
            if (string.IsNullOrEmpty(error))
            {
                ++_validCount;
            }
            else
            {
                string errorTextFormat =
                    _objectsName + FORMAT_VALIDATION_MESSAGE_PART + Environment.NewLine + error;
                _details.Add(new MessageDetail(MessageType.Warning, errorTextFormat, obj));
            }
        }

        /// <summary>
        /// Does validate object.
        /// </summary>
        /// <param name="obj">Object to validation.</param>
        private void _ValidateObject(AppData.DataObject obj)
        {
            Debug.Assert(null != obj); // created

            // check geocoded point in map extent
            var geocodable = obj as IGeocodable;
            if ((null != geocodable) &&
                geocodable.IsGeocoded)
            {
                AppGeometry.Envelope extent = App.Current.Map.ImportCheckExtent;
                // geolocation is not valide
                if (!extent.IsPointIn(geocodable.GeoLocation.Value))
                {
                    // reset geolocation
                    geocodable.GeoLocation = null;
                    geocodable.Address.MatchMethod = string.Empty;
                    // store problem description
                    string extendedFormat =
                        App.Current.FindString("ImportProcessStatusRecordOutMapExtendFormat");
                    string errorTextFormat =
                        string.Format(GEOCODE_MESSAGE_FORMAT, _objectsName, extendedFormat);
                    _details.Add(new MessageDetail(MessageType.Warning, errorTextFormat, obj));
                }
            }

            _StoreValidateResult(obj);
        }

        /// <summary>
        /// Populate update object warning.
        /// </summary>
        /// <param name="obj">Updated object.</param>
        private void _AddUpdateObjectWarning(AppData.DataObject obj)
        {
            Debug.Assert(null != obj); // created

            IGeocodable geocodable = obj as IGeocodable;
            if (null != geocodable)
            {   // NOTE: workaround - see method comment
                CommonHelpers.FillAddressWithSameValues(geocodable.Address);
            }

            string statusText = App.Current.FindString("ImportProcessStatusUpdated");
            string textFormat = _objectsName + FORMAT_UPDATE_MESSAGE_PART + statusText;
            _details.Add(new MessageDetail(MessageType.Warning, textFormat, obj));
        }

        /// <summary>
        /// Checks is orders equals
        /// </summary>
        /// <param name="order1">First order to check.</param>
        /// <param name="order2">Second order to check.</param>
        /// <returns>TRUE if order's name and address equals.</returns>
        private bool _IsOrdersEquals(Order order1, Order order2)
        {
            Debug.Assert(null != order1); // created
            Debug.Assert(null != order2); // created

            bool result = false;
            if (order1.Name.Equals(order2.Name, StringComparison.OrdinalIgnoreCase))
                result = order1.Address.Equals(order2.Address);

            return result;
        }

        /// <summary>
        /// Creates schedule if not presented.
        /// </summary>
        /// <param name="plannedDate">Schedule's planned date.</param>
        private void _CreateSchedule(DateTime plannedDate)
        {
            // create new schedule if early not created
            Schedule schedule = ScheduleHelper.GetCurrentScheduleByDay(plannedDate);
            if (null == schedule)
            {
                App currentApp = App.Current;

                // workaround: don't touch - need do real update
                currentApp.Project.Save();

                schedule = new Schedule();
                schedule.PlannedDate = plannedDate;
                schedule.Name = currentApp.FindString("CurrentScheduleName");
                schedule.UnassignedOrders =
                    currentApp.Project.Orders.SearchUnassignedOrders(schedule, true);
                currentApp.Project.Schedules.Add(schedule);

                // workaround: don't touch - need do real update
                currentApp.Project.Save();
            }
        }

        /// <summary>
        /// Updates GUI.
        /// </summary>
        /// <param name="orders">Object collection (orders).</param>
        /// <remarks>Workaround for GUI directly update.</remarks>
        private void _UpdateGui(IList<Order> orders)
        {
            Debug.Assert(null != orders); // created
            Debug.Assert(0 < orders.Count); // not empty
            Debug.Assert(orders[0] is Order); // supported type

            // get Optimize and edit page
            MainWindow mainWindow = App.Current.MainWindow;
            string pagePath = AppPages.PagePaths.SchedulePagePath;
            var optimizeAndEditPage = mainWindow.GetPage(pagePath) as AppPages.OptimizeAndEditPage;
            Debug.Assert(null != optimizeAndEditPage); // result conversion

            // notificate it of changes
            optimizeAndEditPage.FinishImportOrders(orders);
        }

        /// <summary>
        /// Adds orders to project.
        /// </summary>
        /// <param name="ordersByDate">Order collection for date.</param>
        /// <param name="appOrdersByDate">Application order collection for date.</param>
        /// <param name="manager">Application order manager.</param>
        private void _AddOrdersToProject(IEnumerable ordersByDate,
                                         AppData.IDataObjectCollection<Order> appOrdersByDate,
                                         AppData.OrderManager manager)
        {
            Debug.Assert(null != ordersByDate); // created
            Debug.Assert(null != appOrdersByDate); // created
            Debug.Assert(null != manager); // created

            // check and replace data for orders with same name
            int appOrdersCount = appOrdersByDate.Count;
            foreach (Order order in ordersByDate)
            {
                bool isReplace = false;
                for (int index = 0; index < appOrdersCount; ++index)
                {
                    Order currOrder = appOrdersByDate[index];
                    // update order data
                    if (_IsOrdersEquals(currOrder, order))
                    {
                        order.CopyTo(currOrder);

                        _AddUpdateObjectWarning(currOrder);
                        _ValidateObject(currOrder);

                        if (!_updatedObjects.Contains(currOrder))
                            _updatedObjects.Add(currOrder); // only unique

                        isReplace = true;
                        break; // NOTE: process done
                    }
                }

                // order with this name not found - add new order
                if (!isReplace)
                {
                    _updatedObjects.Add(order);
                    manager.Add(order);
                    _ValidateObject(order);
                    ++_createdCount;
                }
            }
        }

        /// <summary>
        /// Converts objects to orders.
        /// </summary>
        /// <param name="objects">Object collection (orders).</param>
        /// <returns>Order collection.</returns>
        private IList<Order> _GetOrders(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(objects[0] is Order); // supported type

            // convert objects to orders
            var orders = new List<Order>(objects.Count);
            foreach (Order order in objects)
            {
                orders.Add(order);
            }

            return orders;
        }

        /// <summary>
        /// Adds orders to project.
        /// </summary>
        /// <param name="objects">Object collection (orders).</param>
        /// <param name="project">Project to adding objects.</param>
        private bool _AddOrdersToProject(IList<AppData.DataObject> objects, Project project)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(objects[0] is Order); // supported type
            Debug.Assert(null != project);

            bool result = false;
            try
            {
                // NOTE: if order with this name present in application for same date -
                //       need replace data

                // convert objects to orders
                IList<Order> orders = _GetOrders(objects);

                // create unique date list
                IList<DateTime> uniqueDate = _CreateOrdersUniqueDateList(objects);

                // foreach unique date do
                AppData.OrderManager manager = project.Orders;
                for (int index = 0; index < uniqueDate.Count; ++index)
                {
                    DateTime date = uniqueDate[index];

                    IEnumerable ordersByDate = _CreateOrderListByDate(orders, date);
                    AppData.IDataObjectCollection<Order> collection = manager.Search(date);

                    // search application orders with this date
                    if (0 < collection.Count)
                    {
                        // in application present orders to this date - add orders with update
                        _AddOrdersToProject(ordersByDate, collection, manager);
                    }
                    else
                    {   // in application no orders to this date - add all orders
                        foreach (Order order in ordersByDate)
                        {
                            manager.Add(order);
                            _ValidateObject(order);
                            ++_createdCount;

                            _updatedObjects.Add(order);
                        }
                    }

                    // create new schedule if early not created
                    _CreateSchedule(date);
                }

                // store changes
                App.Current.Project.Save();
                result = true;

                // Workaround
                _UpdateGui(orders);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                _updatedObjects.Clear();
            }

            return result;
        }

        /// <summary>
        /// Adds barriers to project.
        /// </summary>
        /// <param name="objects">Barrier collection.</param>
        /// <param name="project">Project to adding objects.</param>
        private bool _AddBarriersToProject(IList<AppData.DataObject> objects, Project project)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(objects[0] is Barrier); // supported type
            Debug.Assert(null != project);

            bool result = false;
            try
            {
                // split collection by required action
                AppData.BarrierManager manager = project.Barriers;
                foreach (Barrier barrier in objects)
                {
                    // find equals barrier in application
                    bool isUpdated = false;
                    AppData.IDataObjectCollection<Barrier> barriers =
                        manager.Search(barrier.StartDate);

                    foreach (Barrier appBarrier in barriers)
                    {
                        // update barrier data
                        if ((barrier.Name == appBarrier.Name) &&
                             barrier.FinishDate.Equals(appBarrier.FinishDate))
                        {
                            barrier.CopyTo(appBarrier);
                            _AddUpdateObjectWarning(appBarrier);
                            _ValidateObject(appBarrier);

                            // store in updated
                            if (!_updatedObjects.Contains(appBarrier))
                                _updatedObjects.Add(appBarrier); // only unique

                            isUpdated = true;
                            break; // NOTE: process done
                        }
                    }

                    // add new barrier
                    if (!isUpdated)
                    {
                        manager.Add(barrier);
                        _ValidateObject(barrier);
                        ++_createdCount;

                        _updatedObjects.Add(barrier);
                    }
                }

                // store changes
                App.Current.Project.Save();
                result = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                _updatedObjects.Clear();
            }

            return result;
        }

        /// <summary>
        /// Updates destination object by data from source object.
        /// </summary>
        /// <typeparam name="T">The type of data objects to updating.</typeparam>
        /// <param name="sourceObj">Source object.</param>
        /// <param name="destinationObj">Destination object (object to update).</param>
        private void _UpdateObject<T>(AppData.DataObject sourceObj,
                                      AppData.DataObject destinationObj)
            where T : AppData.DataObject
        {
            Debug.Assert(null != sourceObj); // created
            Debug.Assert(null != destinationObj); // created

            // special issue for routes: color not updated by empty value
            if (typeof(Route) == typeof(T))
            {
                var route = destinationObj as Route;
                Color storedColor = route.Color;
                sourceObj.CopyTo(route);
                if (route.Color.IsEmpty)
                    route.Color = storedColor;
            }

            // simple update data
            else
            {
                sourceObj.CopyTo(destinationObj);
            }
        }

        /// <summary>
        /// Adds objects to project.
        /// </summary>
        /// <typeparam name="T">The type of data objects to collection adding.</typeparam>
        /// <param name="objects">Object collection.</param>
        /// <param name="appObjects">Application's object collection.</param>
        private bool _AddObjectsToProject<T>(IList<AppData.DataObject> objects, IList<T> appObjects)
            where T : AppData.DataObject
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty
            Debug.Assert(null != appObjects); // created
            Debug.Assert(typeof(T) == objects[0].GetType()); // some types

            bool result = false;
            try
            {
                // NOTE: if object with this name present in application - need replace data
                for (int index = 0; index < objects.Count; ++index)
                {
                    AppData.DataObject obj = objects[index];

                    // find equals object in application
                    bool isUpdated = false;
                    for (int appObjIndex = 0; appObjIndex < appObjects.Count; ++appObjIndex)
                    {
                        // update application object by object data
                        if (obj.ToString() == appObjects[appObjIndex].ToString())
                        {
                            AppData.DataObject updatedObject = appObjects[appObjIndex];
                            _UpdateObject<T>(obj, updatedObject);
                            _AddUpdateObjectWarning(updatedObject);
                            _ValidateObject(updatedObject);

                            // store in updated
                            if (!_updatedObjects.Contains(updatedObject))
                                _updatedObjects.Add(updatedObject); // only unique

                            isUpdated = true;
                            break; // NOTE: process done
                        }
                    }

                    // add new object
                    if (!isUpdated)
                    {
                        CreateHelpers.SpecialInit(appObjects, obj);
                        appObjects.Add((T)obj);
                        _ValidateObject(obj);
                        ++_createdCount;

                        _updatedObjects.Add(obj);
                    }
                }

                // store changes
                App.Current.Project.Save();
                result = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                _updatedObjects.Clear();
            }

            return result;
        }

        /// <summary>
        /// Adds objects to project.
        /// </summary>
        /// <param name="objects">Object collection to adding.</param>
        /// <returns>TRUE if operation ended successed.</returns>
        private bool _AddToProject(IList<AppData.DataObject> objects)
        {
            Debug.Assert(null != objects); // created
            Debug.Assert(0 < objects.Count); // not empty

            App currentApp = App.Current;
            Project project = currentApp.Project;

            // store in project
            bool result = false;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // get objects type
                AppData.DataObject firstObject = objects.FirstOrDefault();

                if (firstObject == null)
                    return result;

                Type type = firstObject.GetType();

                if (type == typeof(Order))
                {   // case for Orders
                    result = _AddOrdersToProject(objects, project);
                }
                else if (type == typeof(Barrier))
                {   // case for Barriers
                    result = _AddBarriersToProject(objects, project);
                }
                else if (type == typeof(Location))
                {   // case for Locations
                    result = _AddObjectsToProject(objects, project.Locations);
                }
                else if (type == typeof(Driver))
                {   // case for Drivers
                    result = _AddObjectsToProject(objects, project.Drivers);
                }
                else if (type == typeof(Vehicle))
                {   // case for Vehicles
                    result = _AddObjectsToProject(objects, project.Vehicles);
                }
                else if (type == typeof(MobileDevice))
                {   // case for MobileDevices
                    result = _AddObjectsToProject(objects, project.MobileDevices);
                }
                else if (type == typeof(Route))
                {   // case for Routes
                    // NOTE: support only for Default Routes
                    result = _AddObjectsToProject(objects, project.DefaultRoutes);
                }
                else if (type == typeof(DriverSpecialty))
                {   // case for DriverSpecialties
                    result = _AddObjectsToProject(objects, project.DriverSpecialties);
                }
                else if (type == typeof(VehicleSpecialty))
                {   // case for VehicleSpecialties
                    result = _AddObjectsToProject(objects, project.VehicleSpecialties);
                }
                else if (type == typeof(Zone))
                {   // case for Zones
                    result = _AddObjectsToProject(objects, project.Zones);
                }
                else
                {
                    Debug.Assert(false); // not supported
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            return result;
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Extension geocode format.
        /// </summary>
        private string GEOCODE_MESSAGE_FORMAT = "{0} {1}";
        /// <summary>
        /// Format part of validation message.
        /// </summary>
        private string FORMAT_VALIDATION_MESSAGE_PART = " {0} :";
        /// <summary>
        /// Format part of update message.
        /// </summary>
        private string FORMAT_UPDATE_MESSAGE_PART = " {0} - ";

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Object name to message generation.
        /// </summary>
        private string _objectName;
        /// <summary>
        /// Objects name to message generation.
        /// </summary>
        private string _objectsName;

        /// <summary>
        /// Counter of new added objects.
        /// </summary>
        private int _createdCount;
        /// <summary>
        /// Counter of valid objects.
        /// </summary>
        private int _validCount;

        /// <summary>
        /// Validation detail list.
        /// </summary>
        private List<MessageDetail> _details = new List<MessageDetail> ();
        /// <summary>
        /// Updated objects.
        /// </summary>
        private List<AppData.DataObject> _updatedObjects = new List<AppData.DataObject> ();

        #endregion // Private fields
    }
}
