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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// RoutingSolveValidator class
    /// </summary>
    internal class RoutingSolveValidator
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RoutingSolveValidator()
        {
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create validation message detail list
        /// </summary>
        /// <param name="invalidObjects"></param>
        /// <returns></returns>
        public ICollection<MessageDetail> GetValidateMessageDetail(Data.DataObject[] invalidObjects)
        {
            _orders.Clear();
            _routes.Clear();
            _relativeLocations.Clear();
            _relativeVehicles.Clear();
            _relativeDrivers.Clear();
            _relativeZones.Clear();

            _SplitObjectsByType(invalidObjects);

            List<MessageDetail> details = new List<MessageDetail>();
            string invalidPropertyFormat = ((string)App.Current.FindResource("SolveValidationPropertyInvalidFormat"));

            // Check - All locations that are referenced in routes.
            _Validate(_relativeLocations, invalidPropertyFormat, ref details);
            // Check - All vehicles that are references in routes.
            _Validate(_relativeVehicles, invalidPropertyFormat, ref details);
            // Check - All drivers that are referenced in routes.
            _Validate(_relativeDrivers, invalidPropertyFormat, ref details);
            // Check - All zones that referenced in routes.
            _Validate(_relativeZones, invalidPropertyFormat, ref details);
            // Check - Routes that take part in the routing operation.
            _Validate(_routes, invalidPropertyFormat, ref details);
            // Check - Orders that take part in the routing operation.
            _Validate(_orders, invalidPropertyFormat, ref details);

            return details.AsReadOnly();
        }

        /// <summary>
        /// Do validate
        /// </summary>
        /// <returns>True only if problems not founded</returns>
        /// <remarks>Show solve validation operation problems</remarks>
        public bool Validate(Data.DataObject[] invalidObjects)
        {
            ICollection<MessageDetail> details = GetValidateMessageDetail(invalidObjects);

            bool isValid = (0 == details.Count);
            if (!isValid)
            {
                string invalidOperationTitle = ((string)App.Current.FindResource("SolveValidationOperationInvalid"));
                App.Current.Messenger.AddMessage(MessageType.Error, invalidOperationTitle, details);
            }

            return isValid;
        }

        #endregion // Public methods

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _ConvertType2ObjectName(Type type)
        {
            string resourceName = null;
            if (typeof(Driver) == type)
                resourceName = "Driver";
            else if (typeof(Location) == type)
                resourceName = "Location";
            else if (typeof(Order) == type)
                resourceName = "Order";
            else if (typeof(Route) == type)
                resourceName = "Route";
            else if (typeof(Vehicle) == type)
                resourceName = "Vehicle";
            else if (typeof(Zone) == type)
                resourceName = "Zone";
            else
            {
                Debug.Assert(false); // NOTE: not supported
            }

            return (string)App.Current.FindResource(resourceName);
        }

        private void _ValidateObject(Data.DataObject dataObject, string invalidPropertyFormat, ref List<MessageDetail> details)
        {
            string error = dataObject.PrimaryError;
            if (!string.IsNullOrEmpty(error))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0} ", _ConvertType2ObjectName(dataObject.GetType()));
                sb.AppendFormat("{0}\n", invalidPropertyFormat);
                sb.Append(error);
                details.Add(new MessageDetail(MessageType.Warning, sb.ToString(), dataObject));
            }
        }

        private void _SplitObjectsByType(Data.DataObject[] invalidObjects)
        {
            List<MessageDetail> details = new List<MessageDetail>();
            string invalidPropertyFormat = ((string)App.Current.FindResource("SolveValidationPropertyInvalidFormat"));

            // split invalid object to supported object collection - only unique object select to check
            foreach (Data.DataObject dataObject in invalidObjects)
            {
                if (dataObject is Order)
                {
                    Order order = dataObject as Order;
                    if (!_orders.Contains(order))
                        _orders.Add(order);
                }
                else if (dataObject is Route)
                {
                    Route route = dataObject as Route;
                    if (!_routes.Contains(route))
                        _routes.Add(route);

                    // split to relative object collections
                    if (null != route.StartLocation)
                    {
                        if (!_relativeLocations.Contains(route.StartLocation))
                            _relativeLocations.Add(route.StartLocation);
                    }

                    if (null != route.EndLocation)
                    {
                        if (!_relativeLocations.Contains(route.EndLocation))
                            _relativeLocations.Add(route.EndLocation);
                    }

                    if (null != route.RenewalLocations)
                    {
                        foreach (Location location in route.RenewalLocations)
                        {
                            if (!_relativeLocations.Contains(location))
                                _relativeLocations.Add(location);
                        }
                    }

                    if (null != route.Vehicle)
                    {
                        if (!_relativeVehicles.Contains(route.Vehicle))
                            _relativeVehicles.Add(route.Vehicle);
                    }

                    // Driver that are referenced in routes.
                    if (null != route.Driver)
                    {
                        if (!_relativeDrivers.Contains(route.Driver))
                            _relativeDrivers.Add(route.Driver);
                    }

                    // All zones that referenced in routes.
                    if (null != route.Zones)
                    {
                        foreach (Zone zone in route.Zones)
                        {
                            if (!_relativeZones.Contains(zone))
                                _relativeZones.Add(zone);
                        }
                    }
                }
                else
                {
                    Debug.Assert(false); // NOTE: not supported
                    // NOTE: invalid object must be Order or Route
                }
            }
        }

        private void _Validate<T>(ICollection<T> objects, string invalidPropertyFormat, ref List<MessageDetail> details)
            where T : Data.DataObject
        {
            foreach (T dataObject in objects)
                _ValidateObject(dataObject, invalidPropertyFormat, ref details);
        }

        #endregion // Private helpers

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<Data.DataObject> _orders = new List<Data.DataObject>();
        private List<Data.DataObject> _routes = new List<Data.DataObject>();
        private List<Data.DataObject> _relativeLocations = new List<Data.DataObject>();
        private List<Data.DataObject> _relativeVehicles = new List<Data.DataObject>();
        private List<Data.DataObject> _relativeDrivers = new List<Data.DataObject>();
        private List<Data.DataObject> _relativeZones = new List<Data.DataObject>();

        #endregion // Private members
    }
}

