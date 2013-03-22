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
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Stores stop information suitable for exporting to GRF or other formats.
    /// </summary>
    internal sealed class StopInfo
    {
        /// <summary>
        /// Gets or sets a name of the stop.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets stop location.
        /// </summary>
        public Point Location { get; set; }

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
        /// Gets or sets address for an order associated with the stop.
        /// </summary>
        public AttrDictionary Address { get; set; }

        /// <summary>
        /// Gets or sets capacities for an order associated with the stop.
        /// </summary>
        public AttrDictionary Capacities { get; set; }

        /// <summary>
        /// Gets or sets custom properties for an order associated with the stop.
        /// </summary>
        public AttrDictionary CustomOrderProperties { get; set; }

        /// <summary>
        /// Gets or sets the maximum time in minutes order time windows can be violated.
        /// </summary>
        public int MaxViolationTime { get; set; }

        /// <summary>
        /// Gets or sets a date/time of the arrival to the stop.
        /// </summary>
        public DateTime? ArriveTime { get; set; }

        /// <summary>
        /// Gets or sets comments for an order associated with the stop.
        /// </summary>
        public string OrderComments { get; set; }
    }
}
