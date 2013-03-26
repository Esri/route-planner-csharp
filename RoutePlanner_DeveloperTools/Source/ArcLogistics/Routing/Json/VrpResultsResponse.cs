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

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// VrpResult class.
    /// </summary>
    internal class VrpResultsResponse
    {
        /// <summary>
        /// Gets or sets reference to the route result object.
        /// </summary>
        public GPRouteResult RouteResult
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets value of the HRESULT code returned by the solve.
        /// </summary>
        public int SolveHR
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets ID of the VRP service job which produced this result.
        /// </summary>
        public string JobID
        {
            get;
            set;
        }

        public bool SolveSucceeded { get; set; }

        public JobMessage[] Messages { get; set; }
    }
}
