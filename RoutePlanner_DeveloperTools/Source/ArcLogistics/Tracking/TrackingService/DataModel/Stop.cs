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
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Tracking.TrackingService.Json;

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Stores information about a single route stop.
    /// </summary>
    internal sealed class Stop : DataRecordBase
    {
        /// <summary>
        /// Gets or sets user friendly name of the stop.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets date/time when the stop should be serviced.
        /// </summary>
        public DateTime PlannedDate { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the order of stops in the route they belong to.
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets stop location.
        /// </summary>
        public Point? Location { get; set; }

        /// <summary>
        /// Gets or sets type of the order associated with the stop.
        /// </summary>
        public OrderType? OrderType { get; set; }

        /// <summary>
        /// Gets or sets priority of the order associated with the stop.
        /// </summary>
        public OrderPriority? Priority { get; set; }

        /// <summary>
        /// Gets or sets curb approach for the order associated with the stop.
        /// </summary>
        public CurbApproach? CurbApproach { get; set; }

        /// <summary>
        /// Gets or sets stop address information.
        /// </summary>
        public IEnumerable<NameValuePair> Address { get; set; }

        /// <summary>
        /// Gets or sets stop order capacities information.
        /// </summary>
        public IEnumerable<NameValuePair> Capacities { get; set; }

        /// <summary>
        /// Gets or sets custom properties for stop order.
        /// </summary>
        public IEnumerable<NameValuePair> CustomOrderProperties { get; set; }

        /// <summary>
        /// Gets or sets stop service time in minutes.
        /// </summary>
        public int ServiceTime { get; set; }

        /// <summary>
        /// Gets or sets the maximum time in minutes order time windows can be violated.
        /// </summary>
        public int MaxViolationTime { get; set; }

        /// <summary>
        /// Gets or sets a date/time of the arrival to the stop.
        /// </summary>
        public DateTime? ArriveTime { get; set; }

        /// <summary>
        /// Gets or sets date/time of the beginning of the first stop time window.
        /// </summary>
        public DateTime? TimeWindowStart1 { get; set; }

        /// <summary>
        /// Gets or sets date/time of the ending of the first stop time window.
        /// </summary>
        public DateTime? TimeWindowEnd1 { get; set; }

        /// <summary>
        /// Gets or sets date/time of the beginning of the second stop time window.
        /// </summary>
        public DateTime? TimeWindowStart2 { get; set; }

        /// <summary>
        /// Gets or sets date/time of the ending of the second stop time window.
        /// </summary>
        public DateTime? TimeWindowEnd2 { get; set; }

        /// <summary>
        /// Gets or sets a foreign key indicating device the stop is assigned to.
        /// </summary>
        public long DeviceID { get; set; }

        /// <summary>
        /// Gets or sets a type of the stop.
        /// </summary>
        public StopType? Type { get; set; }

        /// <summary>
        /// Gets or sets route version number.
        /// </summary>
        public int Version { get; set; }
    }
}
