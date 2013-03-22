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
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Provides data for asynchronous solve starting event.
    /// </summary>
    public class AsyncSolveStartingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the AsyncSolveStartingEventArgs class.
        /// </summary>
        /// <param name="schedule">The reference to the schedule object to
        /// solve routes for.</param>
        public AsyncSolveStartingEventArgs(Schedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException("schedule");
            }

            this.Schedule = schedule;
        }

        /// <summary>
        /// Gets reference to the schedule object the solve will be started for.
        /// </summary>
        public Schedule Schedule
        {
            get;
            private set;
        }
    }
}
