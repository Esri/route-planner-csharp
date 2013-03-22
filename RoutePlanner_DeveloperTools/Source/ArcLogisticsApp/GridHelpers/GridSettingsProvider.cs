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

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Class provide setting for loading grid structure and grid layout.
    /// </summary>
    internal sealed class GridSettingsProvider
    {
        #region Public static properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Message Window grid structure file.
        /// </summary>
        public static string MessageWindowGridStructure
        {
            get { return GRID_STRUCTURE_MESSAGE_WINDOW; }
        }

        /// <summary>
        /// Export Fields grid structure file.
        /// </summary>
        public static string ExportFieldsGridStructure
        {
            get { return GRID_STRUCTURE_EXPORT_FIELDS; }
        }

        /// <summary>
        /// Report Routes grid structure file.
        /// </summary>
        public static string ReportRoutesGridStructure
        {
            get { return GRID_STRUCTURE_REPORT_ROUTES; }
        }

        /// <summary>
        /// Report Templates grid structure file.
        /// </summary>
        public static string ReportTemplatesGridStructure
        {
            get { return GRID_STRUCTURE_REPORT_TEMPLATES; }
        }

        /// <summary>
        /// Report Reports grid structure file.
        /// </summary>
        public static string ReportReportsGridStructure
        {
            get { return GRID_STRUCTURE_REPORT_REPORTS; }
        }

        /// <summary>
        /// Projects grid structure file.
        /// </summary>
        public static string ProjectsGridStructure
        {
            get { return GRID_STRUCTURE_PROJECTS; }
        }

        /// <summary>
        /// Export Edit Profiles grid structure file.
        /// </summary>
        public static string ExportEditProfilesGridStructure
        {
            get { return GRID_STRUCTURE_EXPORT_EDIT_PROFILES; }
        }

        /// <summary>
        /// Export Show Profiles grid structure file.
        /// </summary>
        public static string ExportShowProfilesGridStructure
        {
            get { return GRID_STRUCTURE_EXPORT_SHOW_PROFILES; }
        }

        /// <summary>
        /// Import Show Profiles grid structure file.
        /// </summary>
        public static string ImportShowProfilesGridStructure
        {
            get { return GRID_STRUCTURE_IMPORT_SHOW_PROFILES; }
        }

        /// <summary>
        /// Category Symbology Preferences grid structure file.
        /// </summary>
        public static string CategorySymbologyPreferencesGridStructure
        {
            get { return GRID_STRUCTURE_CATEGORY_SYMBOLOGY_PREFERENCES; }
        }

        /// <summary>
        /// Quantity Symbology Preferences grid structure file.
        /// </summary>
        public static string QuantitySymbologyPreferencesGridStructure
        {
            get { return GRID_STRUCTURE_QUANTITY_SYMBOLOGY_PREFERENCES; }
        }

        /// <summary>
        /// Report Preferences grid structure file.
        /// </summary>
        public static string ReportPreferencesGridStructure
        {
            get { return GRID_STRUCTURE_REPORT_PREFERENCES; }
        }

        /// <summary>
        /// Routing Preferences grid structure file.
        /// </summary>
        public static string RoutingPreferencesGridStructure
        {
            get { return GRID_STRUCTURE_ROUTING_PREFERENCES; }
        }

        /// <summary>
        /// Find Orders grid structure file.
        /// </summary>
        public static string FindOrdersGridStructure
        {
            get { return GRID_STRUCTURE_FIND_ORDERS; }
        }

        /// <summary>
        /// Orders grid structure file.
        /// </summary>
        public static string OrdersGridStructure
        {
            get { return GRID_STRUCTURE_ORDERS; }
        }

        /// <summary>
        /// Routes grid structure file.
        /// </summary>
        public static string RoutesGridStructure
        {
            get { return GRID_STRUCTURE_ROUTES; }
        }

        /// <summary>
        /// Stops grid structure file.
        /// </summary>
        public static string StopsGridStructure
        {
            get { return GRID_STRUCTURE_STOPS; }
        }

        /// <summary>
        /// Schedules grid structure file.
        /// </summary>
        public static string SchedulesGridStructure
        {
            get { return GRID_STRUCTURE_SCHEDULES; }
        }

        /// <summary>
        /// Barriers grid structure file.
        /// </summary>
        public static string BarriersGridStructure
        {
            get { return GRID_STRUCTURE_BARRIERS; }
        }

        /// <summary>
        /// Default Routes grid structure file.
        /// </summary>
        public static string DefaultRoutesGridStructure
        {
            get { return GRID_STRUCTURE_DEFAULT_ROUTES; }
        }

        /// <summary>
        /// Drivers grid structure file.
        /// </summary>
        public static string DriversGridStructure
        {
            get { return GRID_STRUCTURE_DRIVERS; }
        }

        /// <summary>
        /// Fuel Types grid structure file.
        /// </summary>
        public static string FuelTypesGridStructure
        {
            get { return GRID_STRUCTURE_FUEL_TYPES; }
        }

        /// <summary>
        /// Locations grid structure file.
        /// </summary>
        public static string LocationsGridStructure
        {
            get { return GRID_STRUCTURE_LOCATIONS; }
        }

        /// <summary>
        /// Mobile Devices grid structure file.
        /// </summary>
        public static string MobileDevicesGridStructure
        {
            get { return GRID_STRUCTURE_MOBILE_DEVICES; }
        }

        /// <summary>
        /// Driver Specialties grid structure file.
        /// </summary>
        public static string DriverSpecialtiesGridStructure
        {
            get { return GRID_STRUCTURE_DRIVER_SPECIALTIES; }
        }

        /// <summary>
        /// Vehicle Specialties grid structure file.
        /// </summary>
        public static string VehicleSpecialtiesGridStructure
        {
            get { return GRID_STRUCTURE_VEHICLE_SPECIALTIES; }
        }

        /// <summary>
        /// Vehicles grid structure file.
        /// </summary>
        public static string VehiclesGridStructure
        {
            get { return GRID_STRUCTURE_VEHICLES; }
        }

        /// <summary>
        /// Zones grid structure file.
        /// </summary>
        public static string ZonesGridStructure
        {
            get { return GRID_STRUCTURE_ZONES; }
        }

        /// <summary>
        /// TimeWindowBrakes grid structure file.
        /// </summary>
        public static string TimeWindowBrakesGridStructure
        {
            get { return GRID_STRUCTURE_TIMEWINDOW_BRAKES; }
        }

        /// <summary>
        /// TimeIntervalBrakes grid structure file.
        /// </summary>
        public static string TimeIntervalBrakesGridStructure
        {
            get { return GRID_STRUCTURE_TIMEINTERVAL_BRAKES; }
        }

        /// <summary>
        /// Fleet Routes grid structure file.
        /// </summary>
        public static string FleetRoutesGridStructure
        {
            get { return GRID_STRUCTURE_FLEET_ROUTES; }
        }

        /// <summary>
        /// Fleet Orders grid structure file.
        /// </summary>
        public static string FleetGeocodableGridStructure
        {
            get { return GRID_STRUCTURE_FLEET_GEOCODABLE; }
        }

        /// <summary>
        /// Custom order properties grid structure file.
        /// </summary>
        public static string CustomOrderPropertiesGridStructure
        {
            get { return GRID_STRUCTURE_CUSTOM_ORDER_PROPERTIES; }
        }

        /// <summary>
        /// Category Symbology Preferences grid settings repository name.
        /// </summary>
        public static string CategorySymbologyPreferencesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_CATEGORY_SYMBOLOGY_PREFERENCES; }
        }

        /// <summary>
        /// Quantities Symbology Preferences grid settings repository name.
        /// </summary>
        public static string QuantitiesSymbologyPreferencesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_QUANTITY_SYMBOLOGY_PREFERENCES; }
        }

        /// <summary>
        /// Orders grid settings repository name.
        /// </summary>
        public static string OrdersSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_ORDERS; }
        }

        /// <summary>
        /// Routes grid settings repository name.
        /// </summary>
        public static string RoutesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_ROUTES; }
        }

        /// <summary>
        /// Barriers grid settings repository name.
        /// </summary>
        public static string BarriersSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_BARRIERS; }
        }

        /// <summary>
        /// Default Routes grid settings repository name.
        /// </summary>
        public static string DefaultRoutesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_DEFAULTROUTES; }
        }

        /// <summary>
        /// Drivers grid settings repository name.
        /// </summary>
        public static string DriversSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_DRIVERS; }
        }

        /// <summary>
        /// TimeWindowBreaks grid settings repository name.
        /// </summary>
        public static string TimeWindowBreaksSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_TIMEWINDOWBREAKS; }
        }

        /// <summary>
        /// WorkTime Breaks grid settings repository name.
        /// </summary>
        public static string WorkTimeBreaksSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_WORKTIMEBREAKS; }
        }

        /// <summary>
        /// DriveTime Breaks grid settings repository name.
        /// </summary>
        public static string DriveTimeBreaksSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_DRIVETIMEBREAKS; }
        }

        /// <summary>
        /// Fuel Types grid settings repository name.
        /// </summary>
        public static string FuelTypesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_FUELTYPES; }
        }

        /// <summary>
        /// Locations grid settings repository name.
        /// </summary>
        public static string LoactionsSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_LOCATIONS; }
        }

        /// <summary>
        /// Mobile Devices grid settings repository name.
        /// </summary>
        public static string MobileDevicesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_MOBILE_DEVICES; }
        }

        /// <summary>
        /// Driver Specialties grid settings repository name.
        /// </summary>
        public static string DriverSpecialtiesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_DRIVER_SPECIALITIES; }
        }

        /// <summary>
        /// Vehicle Specialties grid settings repository name.
        /// </summary>
        public static string VehicleSpecialtiesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_VEHICLE_SPECIALITIES; }
        }

        /// <summary>
        /// Vehicles grid settings repository name.
        /// </summary>
        public static string VehiclesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_VEHICLES; }
        }

        /// <summary>
        /// Zones grid settings repository name.
        /// </summary>
        public static string ZonesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_ZONES; }
        }

        /// <summary>
        /// Custom order properties grid settings repository name.
        /// </summary>
        public static string CustomOrderPropertiesSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_NAME_CUSTOM_ORDER_PROPERTIES; }
        }

        /// <summary>
        /// Fleet locations grid settings repository name.
        /// </summary>
        public static string FleetLocationsSettingsRepositoryName
        {
            get { return SETTINGS_REPOSITORY_FLEET_LOCATIONS; }
        }

        #endregion // Public static properties

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string GRID_STRUCTURE_MESSAGE_WINDOW = "ESRI.ArcLogistics.App.GridHelpers.MessageWindowGridStructure.xaml";
        private const string GRID_STRUCTURE_EXPORT_FIELDS = "ESRI.ArcLogistics.App.GridHelpers.SelectPropertiesGridStructure.xaml";
        private const string GRID_STRUCTURE_REPORT_ROUTES = "ESRI.ArcLogistics.App.GridHelpers.SelectPropertiesGridStructure.xaml";
        private const string GRID_STRUCTURE_REPORT_TEMPLATES = "ESRI.ArcLogistics.App.GridHelpers.SelectReportGridStructure.xaml";
        private const string GRID_STRUCTURE_REPORT_REPORTS = "ESRI.ArcLogistics.App.GridHelpers.ReportGridStructure.xaml";
        private const string GRID_STRUCTURE_PROJECTS = "ESRI.ArcLogistics.App.GridHelpers.ProjectsGridStructure.xaml";
        private const string GRID_STRUCTURE_EXPORT_EDIT_PROFILES = "ESRI.ArcLogistics.App.GridHelpers.SelectPropertiesGridStructure.xaml";
        private const string GRID_STRUCTURE_EXPORT_SHOW_PROFILES = "ESRI.ArcLogistics.App.GridHelpers.NamedPropertyGridStructure.xaml";
        private const string GRID_STRUCTURE_IMPORT_SHOW_PROFILES = "ESRI.ArcLogistics.App.GridHelpers.ImportProfileShowGridStructure.xaml";
        private const string GRID_STRUCTURE_CATEGORY_SYMBOLOGY_PREFERENCES = "ESRI.ArcLogistics.App.GridHelpers.CategorySymbologyGridStructure.xaml";
        private const string GRID_STRUCTURE_QUANTITY_SYMBOLOGY_PREFERENCES = "ESRI.ArcLogistics.App.GridHelpers.QuantitySymbologyGridStructure.xaml";
        private const string GRID_STRUCTURE_REPORT_PREFERENCES = "ESRI.ArcLogistics.App.GridHelpers.ReportGridStructure.xaml";
        private const string GRID_STRUCTURE_ROUTING_PREFERENCES = "ESRI.ArcLogistics.App.GridHelpers.RestrictionsGridStructure.xaml";
        private const string GRID_STRUCTURE_FIND_ORDERS = "ESRI.ArcLogistics.App.GridHelpers.FindOrdersGridStructure.xaml";
        private const string GRID_STRUCTURE_ORDERS = "ESRI.ArcLogistics.App.GridHelpers.UnassignedOrdersGridStructure.xaml";
        private const string GRID_STRUCTURE_ROUTES = "ESRI.ArcLogistics.App.GridHelpers.RouteResultsGridStructure.xaml";
        private const string GRID_STRUCTURE_STOPS = "ESRI.ArcLogistics.App.GridHelpers.StopGridStructure.xaml";
        private const string GRID_STRUCTURE_SCHEDULES = "ESRI.ArcLogistics.App.GridHelpers.ScheduleVersionsGridStructure.xaml";
        private const string GRID_STRUCTURE_BARRIERS = "ESRI.ArcLogistics.App.GridHelpers.BarriersGridStructure.xaml";
        private const string GRID_STRUCTURE_DEFAULT_ROUTES = "ESRI.ArcLogistics.App.GridHelpers.DefaultRoutesGridStructure.xaml";
        private const string GRID_STRUCTURE_DRIVERS = "ESRI.ArcLogistics.App.GridHelpers.DriversGridStructure.xaml";
        private const string GRID_STRUCTURE_TIMEWINDOW_BRAKES = "ESRI.ArcLogistics.App.GridHelpers.TimeWindowBrakeGridStructure.xaml";
        private const string GRID_STRUCTURE_TIMEINTERVAL_BRAKES = "ESRI.ArcLogistics.App.GridHelpers.TimeIntervalBrakeGridStructure.xaml";
        private const string GRID_STRUCTURE_FUEL_TYPES = "ESRI.ArcLogistics.App.GridHelpers.FuelGridStructure.xaml";
        private const string GRID_STRUCTURE_LOCATIONS = "ESRI.ArcLogistics.App.GridHelpers.LocationsGridStructure.xaml";
        private const string GRID_STRUCTURE_MOBILE_DEVICES = "ESRI.ArcLogistics.App.GridHelpers.MobileDevicesGridStructure.xaml";
        private const string GRID_STRUCTURE_DRIVER_SPECIALTIES = "ESRI.ArcLogistics.App.GridHelpers.DriverSpecialtiesGridStructure.xaml";
        private const string GRID_STRUCTURE_VEHICLE_SPECIALTIES = "ESRI.ArcLogistics.App.GridHelpers.VehicleSpecialtiesGridStructure.xaml";
        private const string GRID_STRUCTURE_VEHICLES = "ESRI.ArcLogistics.App.GridHelpers.VehiclesGridStructure.xaml";
        private const string GRID_STRUCTURE_ZONES = "ESRI.ArcLogistics.App.GridHelpers.ZonesGridStructure.xaml";
        private const string GRID_STRUCTURE_FLEET_ROUTES = "ESRI.ArcLogistics.App.GridHelpers.FleetRoutesGridStructure.xaml";
        private const string GRID_STRUCTURE_FLEET_GEOCODABLE = "ESRI.ArcLogistics.App.GridHelpers.GeocodableGridStructure.xaml";
        private const string GRID_STRUCTURE_CUSTOM_ORDER_PROPERTIES = "ESRI.ArcLogistics.App.GridHelpers.CustomOrderPropertiesGridStructure.xaml";

        private const string SETTINGS_REPOSITORY_NAME_CATEGORY_SYMBOLOGY_PREFERENCES = "CategoriesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_QUANTITY_SYMBOLOGY_PREFERENCES = "QuantitiesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_ORDERS = "OrdersGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_ROUTES = "RoutesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_BARRIERS = "BarriersGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_DEFAULTROUTES = "DefaultRoutesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_DRIVERS = "DriversGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_TIMEWINDOWBREAKS = "TimeWindowBreaksGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_DRIVETIMEBREAKS = "DriveTimeBreaksGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_WORKTIMEBREAKS = "WorkTimeBreaksGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_FUELTYPES = "FuelGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_LOCATIONS = "LocationsGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_MOBILE_DEVICES = "MobileDevicesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_DRIVER_SPECIALITIES = "DriverSpecialtiesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_VEHICLE_SPECIALITIES = "VehicleSpecialtiesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_VEHICLES = "VehiclesGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_ZONES = "ZonesGridSettings";
        private const string SETTINGS_REPOSITORY_FLEET_LOCATIONS = "FleetLocationsGridSettings";
        private const string SETTINGS_REPOSITORY_NAME_CUSTOM_ORDER_PROPERTIES = "CustomOrderPropertiesGridSettings";

        #endregion // Private consts
    }
}
