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

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Stores information about mobile device.
    /// </summary>
    internal sealed class Device : DataRecordBase
    {
        /// <summary>
        /// Gets or sets user friendly mobile device name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets UTC date/time of last location change.
        /// </summary>
        public DateTime? Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets device location.
        /// </summary>
        public Point? Location
        {
            get;
            set;
        }
    }
}
