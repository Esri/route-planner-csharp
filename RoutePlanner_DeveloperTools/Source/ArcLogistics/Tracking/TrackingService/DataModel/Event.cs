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

namespace ESRI.ArcLogistics.Tracking.TrackingService.DataModel
{
    /// <summary>
    /// Contains stop status changing event information.
    /// </summary>
    internal sealed class Event : DataRecordBase
    {
        /// <summary>
        /// Gets or sets new stop status.
        /// </summary>
        public StopStatus Status
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets status change date/time in UTC.
        /// </summary>
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets object ID of the stop the status was changed for.
        /// </summary>
        public long StopID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets object ID of the mobile device the event originated from.
        /// </summary>
        public long DeviceID
        {
            get;
            set;
        }
    }
}
