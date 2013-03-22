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
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App
{
    internal class RoutesHelper
    {
        /// <summary>
        /// Method creates collection of already used vehicles in other routes
        /// </summary>
        public static Collection<ESRI.ArcLogistics.Data.DataObject> CreateUsedVehiclesCollection()
        {
            OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)((App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath));
            Schedule currentSchedule = schedulePage.CurrentSchedule;
            ICollection<Route> routeCollection = currentSchedule.Routes;

            Collection<ESRI.ArcLogistics.Data.DataObject> usedVehicles = new Collection<ESRI.ArcLogistics.Data.DataObject>();

            foreach (Route route in routeCollection)
                usedVehicles.Add(route.Vehicle);
            return usedVehicles;
        }

        /// <summary>
        /// Method creates collection of already used drivers in other routes
        /// </summary>
        public static Collection<ESRI.ArcLogistics.Data.DataObject> CreateUsedDriversCollection()
        {
            OptimizeAndEditPage schedulePage = (OptimizeAndEditPage)((App.Current.MainWindow).GetPage(PagePaths.SchedulePagePath));
            Schedule currentSchedule = schedulePage.CurrentSchedule;
            ICollection<Route> routeCollection = currentSchedule.Routes;

            Collection<ESRI.ArcLogistics.Data.DataObject> usedDrivers = new Collection<ESRI.ArcLogistics.Data.DataObject>();

            foreach (Route route in routeCollection)
                usedDrivers.Add(route.Driver);
            return usedDrivers;
        }
    }
}
