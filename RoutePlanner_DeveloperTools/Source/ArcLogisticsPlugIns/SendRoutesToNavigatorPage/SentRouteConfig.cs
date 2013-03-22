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
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.DomainObjects;

namespace ArcLogisticsPlugIns.SendRoutesToNavigatorPage
{
    /// <summary>
    /// Class to select is route need to be send in grid
    /// </summary>
    class SentRouteConfig
    {
        #region constructors

        public SentRouteConfig(Route route)
        {
            Route = route;
        }

        #endregion

        #region public members

        /// <summary>
        /// Is route need to be sent
        /// </summary>
        public bool IsChecked
        {
            get;
            set;
        }

        /// <summary>
        /// Route name
        /// </summary>
        public string RouteName
        {
            get;
            set;
        }

        /// <summary>
        /// Route send method
        /// </summary>
        public string SendMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Route
        /// </summary>
        public Route Route
        {
            get;
            private set;
        }

        #endregion
    }
}
