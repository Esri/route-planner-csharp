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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.Serialization;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Stores information about route.
    /// </summary>
    internal class Route : DataRecordBase
    {
        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Route() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="route">Route to send.</param>
        /// <param name="deviceID">Device ID route belong to.</param>
        /// <param name="plannedDate">Route planned date.</param>
        public Route(DomainObjects.Route route, long deviceID, DateTime plannedDate)
        {
            Name = route.Name;
            Color = _SerializeColor(route.Color);
            DeviceID = deviceID;
            Driver = route.Driver.Name;
            DriverSpecialty = _SerializeNames(route.Driver.Specialties);
            PlannedDate = plannedDate;
            Shape = route.Path;
            Vehicle = route.Vehicle.Name;
            VehicleSpecialty = _SerializeNames(route.Vehicle.Specialties);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets the route shape.
        /// </summary>
        public Polyline Shape { get; set; }

        /// <summary>
        /// Gets or sets user friendly name of the route.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets name of the route vehicle.
        /// </summary>
        public string Vehicle { get; set; }

        /// <summary>
        /// Gets or sets name of the route driver.
        /// </summary>
        public string Driver { get; set; }

        /// <summary>
        /// Gets or sets route color.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets speciality of the route vehicle.
        /// </summary>
        public string VehicleSpecialty { get; set; }

        /// <summary>
        /// Gets or sets speciality of the route driver.
        /// </summary>
        public string DriverSpecialty { get; set; }

        /// <summary>
        /// Gets or sets a foreign key indicating device the routes is assigned to.
        /// </summary>
        public long DeviceID { get; set; }

        /// <summary>
        /// Gets or sets date/time when the route should be serviced.
        /// </summary>
        public DateTime PlannedDate { get; set; }

        #endregion

        #region Private members

        /// <summary>
        /// Serialize color to string.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <returns>String representation of color.</returns>
        private string _SerializeColor(Color color)
        {
            string result = "{";
            result += string.Format(COLOR_SERIALIZING_FORMAT, color.R, color.G, color.B, color.A);
            return result + "}";
        }

        /// <summary>
        /// Serialize collection of objects with names to string.
        /// </summary>
        /// <param name="specialities">Collection of objects to serialize.</param>
        /// <returns>String with collection elements names.</returns>
        private string _SerializeNames(IEnumerable specialities)
        {
            if (specialities == null)
                return null;

            // Get names from objects collection.
            var names = new List<string>();
            foreach (ISupportName spec in specialities)
                names.Add(spec.Name);

            if (names.Count == 0)
                return null;

            // Serialize names list.
            var speciality = new Specialities();
            speciality.Names = names;
            return JsonSerializeHelper.Serialize(speciality);
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Color serialization format.
        /// </summary>
        private const string COLOR_SERIALIZING_FORMAT = "\"color\":[{0}, {1}, {2}, {3}]";

        #endregion
    }

    /// <summary>
    /// Stores settings affecting specialities.
    /// </summary>
    [DataContract]
    internal sealed class Specialities
    {
        /// <summary>
        /// Gets or sets a collection of restriction attributes to be used for routing.
        /// </summary>
        [DataMember]
        public IEnumerable<string> Names
        {
            get;
            set;
        }
    }
}
