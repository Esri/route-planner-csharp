using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class contains paths to all application pages.
    /// </summary>
    /// <remarcs>Format page path is "CategoryName"\"PageName"</remarcs>
    public static class PagePaths
    {
        // Home tab pages
        public static string GettingStartedPagePath
        {
            get
            {
                return @"Home\GettingStarted";
            }
        }

        public static string ProjectsPagePath
        {
            get
            {
                return @"Home\Projects";
            }
        }

        public static string LicensePagePath
        {
            get
            {
                return @"Home\License";
            }
        }

        // Setup tab pages
        public static string FuelTypesPagePath
        {
            get
            {
                return @"Setup\FuelTypes";
            }
        }

        public static string LocationsPagePath
        {
            get
            {
                return @"Setup\Locations";
            }
        }

        public static string VehiclesPagePath
        {
            get
            {
                return @"Setup\Vehicles";
            }
        }

        public static string DriversPagePath
        {
            get
            {
                return @"Setup\Drivers";
            }
        }

        public static string BreaksPagePath
        {
            get
            {
                return @"Setup\Breaks";
            }
        }

        public static string MobileDevicesPagePath
        {
            get
            {
                return @"Setup\MobileDevices";
            }
        }

        public static string SpecialtiesPagePath
        {
            get
            {
                return @"Setup\Specialties";
            }
        }

        public static string ZonesPagePath
        {
            get
            {
                return @"Setup\Zones";
            }
        }

        public static string BarriersPagePath
        {
            get
            {
                return @"Setup\Barriers";
            }
        }

        public static string DefaultRoutesPagePath
        {
            get
            {
                return @"Setup\DefaultRoutes";
            }
        }

        // Schedule tab pages
        public static string SchedulePagePath
        {
            get
            {
                return @"Schedule\OptimizeAndEdit";
            }
        }

        // Deployment tab pages
        public static string ReportsPagePath
        {
            get
            {
                return @"Deployment\Reports";
            }
        }

        public static string ExportPagePath
        {
            get
            {
                return @"Deployment\Export";
            }
        }

        // Preferences tab pages
        public static string GeneralPreferencesPagePath
        {
            get
            {
                return @"Preferences\General";
            }
        }

        public static string MapDisplayPreferencesPagePath
        {
            get
            {
                return @"Preferences\MapDisplay";
            }
        }

        public static string ReportsPreferencesPagePath
        {
            get
            {
                return @"Preferences\Reports";
            }
        }

        public static string RoutingPreferencesPagePath
        {
            get
            {
                return @"Preferences\Routing";
            }
        }

        // NOTE: special cases - don't use in Navigation
        internal static string MainPage
        {
            get
            {
                return @"Main";
            }
        }
    }
}
