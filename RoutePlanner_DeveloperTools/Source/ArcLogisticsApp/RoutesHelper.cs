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
