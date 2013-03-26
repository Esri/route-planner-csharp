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
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Represents the method that handles <c>AsyncExportCompleted</c> event.
    /// </summary>
    public delegate void AsyncExportCompletedEventHandler(Object sender,
                                                          RunWorkerCompletedEventArgs e);

    /// <summary>
    /// Class that reperesents a data exporter.
    /// </summary>
    public partial class Exporter
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>Exporter</c> class.
        /// </summary>
        /// <param name="capacitiesInfo">Capacities information.</param>
        /// <param name="orderCustomPropertiesInfo">Order custom properties infromation.</param>
        /// <param name="addressFields">Geocoder address fields.</param>
        public Exporter(CapacitiesInfo capacitiesInfo,
                        OrderCustomPropertiesInfo orderCustomPropertiesInfo,
                        AddressField[] addressFields)
        {
            _structureKeeper = GetReader(capacitiesInfo, orderCustomPropertiesInfo, addressFields);

            _InitExtendedFields();
        }

        #endregion // Constructors

        #region Public interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads export profiles from configuration file.
        /// </summary>
        /// <param name="filePath">Full path to export configuration file.</param>
        public void LoadProfiles(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            var file = new ExportFile();
            _profiles = file.Load(filePath, _structureKeeper);
        }

        /// <summary>
        /// Stores export profiles to a configuration file.
        /// </summary>
        /// <param name="filePath">Full path to export configuration file where it is necessary
        /// to store the profiles.</param>
        public void SaveProfiles(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            var file = new ExportFile();
            file.Save(filePath, _profiles);
        }

        /// <summary>
        /// Gets read-only profiles collection.
        /// </summary>
        public ICollection<Profile> Profiles
        {
            get { return _profiles.AsReadOnly(); }
        }

        /// <summary>
        /// Creates a profile.
        /// </summary>
        /// <param name="type">Export type.</param>
        /// <param name="filePath">Full path to a file where data will be exported.</param>
        /// <returns>New <c>Profile</c> class instance of specified type.</returns>
        public Profile CreateProfile(ExportType type, string filePath)
        {
            Debug.Assert(null != _structureKeeper);

            return new Profile(_structureKeeper, type, filePath, true);
        }

        /// <summary>
        /// Adds the profile to the Exporter's <c>Profiles</c> collection.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> to add.</param>
        public void AddProfile(Profile profile)
        {
            _ValidateProfile(profile);

            // check name - must be unique
            foreach (Profile profileApp in _profiles)
            {
                if (profile.Name.Equals(profileApp.Name))
                {
                    string text = Properties.Resources.ProfileNameIsAlreadyExists;
                    throw new ArgumentException(text); // exception
                }
            }

            if (!_profiles.Contains(profile))
                _profiles.Add(profile);
        }

        /// <summary>
        /// Removes specified profile from Exporter's <c>Profiles</c> collection.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> to remove.</param>
        public void RemoveProfile(Profile profile)
        {
            Debug.Assert(null != profile);

            if (_profiles.Contains(profile))
                _profiles.Remove(profile);
        }

        /// <summary>
        /// Exports specified schedules.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> that has to be
        /// used for export.</param>
        /// <param name="schedules">Schedule to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        public void DoExport(Profile profile, ICollection<Schedule> schedules, MapLayer mapLayer)
        {
            DoExport(profile, schedules, mapLayer, new ExportOptions());
        }

        /// <summary>
        /// Exports specified schedules.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> that has to be
        /// used for export.</param>
        /// <param name="schedules">Schedule to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        public void DoExport(Profile profile,
                             ICollection<Schedule> schedules,
                             MapLayer mapLayer,
                             ExportOptions options)
        {
            _DoExport(profile, schedules, mapLayer, options, false);
        }

        #region Public interface. Report Source Part
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Collection of export field names, generating whose values is time consuming.
        /// </summary>
        /// <remarks>This list includes fields that contain images or driving directions.</remarks>
        public string[] ExtendedFields
        {
            get { return _extendedFields.ToArray(); }
        }

        /// <summary>
        /// Collection of export field names, generating whose values is memory-intensive.
        /// </summary>
        public string[] HardFields
        {
            get { return _structureKeeper.HardFields; }
        }

        /// <summary>
        /// Creates a database that can be used as a report source.
        /// </summary>
        /// <param name="filePath">Path to Access database file where data will be exported.</param>
        /// <param name="schedules">Collection of schedules to export.</param>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        public void DoReportSource(string filePath,
                                   ICollection<Schedule> schedules,
                                   ICollection<string> extendedFields,
                                   MapLayer mapLayer)
        {
            DoReportSource(filePath, schedules, extendedFields, mapLayer, new ExportOptions());
        }

        /// <summary>
        /// Creates a database that can be used as a report source.
        /// </summary>
        /// <param name="filePath">Path to Access database file where data will be exported.</param>
        /// <param name="schedules">Collection of schedules to export.</param>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        public void DoReportSource(string filePath,
                                   ICollection<Schedule> schedules,
                                   ICollection<string> extendedFields,
                                   MapLayer mapLayer,
                                   ExportOptions options)
        {
            _DoReportSource(filePath, extendedFields, schedules, null, mapLayer, options, false);
        }

        /// <summary>
        /// Creates a database that can be used as a report source.
        /// </summary>
        /// <param name="filePath">Path to Access database file where data will be exported.</param>
        /// <param name="schedule">Collection of schedules to export.</param>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="routes">Collection of routes from the <c>schedule</c> to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        public void DoReportSource(string filePath,
                                   Schedule schedule,
                                   ICollection<string> extendedFields,
                                   ICollection<Route> routes,
                                   MapLayer mapLayer)
        {
            DoReportSource(filePath,
                           schedule,
                           extendedFields,
                           routes,
                           mapLayer,
                           new ExportOptions());
        }

        /// <summary>
        /// Creates a database that can be used as a report source.
        /// </summary>
        /// <param name="filePath">Path to Access database file where data will be exported.</param>
        /// <param name="schedule">Collection of schedules to export.</param>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="routes">Collection of routes from the <c>schedule</c> to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        public void DoReportSource(string filePath,
                                   Schedule schedule,
                                   ICollection<string> extendedFields,
                                   ICollection<Route> routes,
                                   MapLayer mapLayer,
                                   ExportOptions options)
        {
            var schedules = (new List<Schedule>()
                                {
                                    schedule
                                }
                            ).AsReadOnly();

            _DoReportSource(filePath, extendedFields, schedules, routes, mapLayer, options, false);
        }

        #endregion // Public interface. Report Source Part

        #region Public interface. Asynchronous
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when asynchronous export operation completed.
        /// </summary>
        public event AsyncExportCompletedEventHandler AsyncExportCompleted;

        /// <summary>
        /// Exports specified schedules (asynchronous).
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> that has to be
        /// used for export.</param>
        /// <param name="schedules">Schedule to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        public void DoExportAsync(Profile profile,
                                  ICollection<Schedule> schedules,
                                  MapLayer mapLayer)
        {
            DoExportAsync(profile, schedules, mapLayer, new ExportOptions());
        }

        /// <summary>
        /// Exports specified schedules (asynchronous).
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> that has to be
        /// used for export.</param>
        /// <param name="schedules">Schedule to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        public void DoExportAsync(Profile profile,
                                  ICollection<Schedule> schedules,
                                  MapLayer mapLayer,
                                  ExportOptions options)
        {
            ICollection<Schedule> schedulesCopy = _DoCopy(schedules);
            _DoExport(profile, schedulesCopy, mapLayer, options, true);
        }

        /// <summary>
        /// Creates a database that can be used as a report source (asynchronous).
        /// </summary>
        /// <param name="filePath">Path to Access database file where data will be exported.</param>
        /// <param name="schedules">Collection of schedules to export.</param>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="routes">Collection of routes from the <c>schedule</c> to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        public void DoReportSourceAsync(string filePath,
                                        ICollection<Schedule> schedules,
                                        ICollection<string> extendedFields,
                                        ICollection<Route> routes,
                                        MapLayer mapLayer)
        {
            DoReportSourceAsync(filePath, schedules, extendedFields, routes, mapLayer, new ExportOptions());
        }

        /// <summary>
        /// Creates a database that can be used as a report source (asynchronous).
        /// </summary>
        /// <param name="filePath">Path to Access database file where data will be exported.</param>
        /// <param name="schedules">Collection of schedules to export.</param>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="routes">Collection of routes from the <c>schedule</c> to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        public void DoReportSourceAsync(string filePath,
                                        ICollection<Schedule> schedules,
                                        ICollection<string> extendedFields,
                                        ICollection<Route> routes,
                                        MapLayer mapLayer,
                                        ExportOptions options)
        {
            // create copy all elements (schedules, routes)
            ICollection<Schedule> schedulesCopy = _DoCopy(schedules);

            ICollection<Route> routesCopy = null;
            if (null != routes)
            {   // need select routes from selected schedule
                Debug.Assert(1 == schedulesCopy.Count);
                Schedule schedule = schedulesCopy.First();

                routesCopy = _GetScheduleRoutes(schedule, routes);
            }

            // start export
            _DoReportSource(filePath, extendedFields, schedulesCopy, routesCopy, mapLayer, options, true);
        }

        /// <summary>
        /// Is asynchronous export work.
        /// </summary>
        /// <returns>TRUE if asynchronous export work.</returns>
        public bool IsExportOn()
        {
            return (_exportWorker != null);
        }

        /// <summary>
        /// Stops asynchronous export.
        /// </summary>
        public void AbortExport()
        {
            if (_exportWorker != null)
            {
                _exportWorker.CancelAsync();
                _exportWorker = null;
            }
        }

        #endregion // Public interface. Asynchronous

        #endregion // Public interface

        #region Internal static helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates and inits a new instance of the <c>ExportStructureReader</c> class.
        /// </summary>
        /// <param name="capacitiesInfo">Capacities information.</param>
        /// <param name="orderCustomPropertiesInfo">Order custom properties infromation.</param>
        /// <param name="addressFields">Geocoder address fields.</param>
        /// <returns>Created and inited Export Structure Reader.</returns>
        internal static ExportStructureReader GetReader(CapacitiesInfo capacitiesInfo,
                                                        OrderCustomPropertiesInfo orderCustomPropertiesInfo,
                                                        AddressField[] addressFields)
        {
            Debug.Assert(null != capacitiesInfo);
            Debug.Assert(null != orderCustomPropertiesInfo);
            Debug.Assert(null != addressFields);

            // load export structure
            var structure = ResourceLoader.ReadFileAsString(EXPORT_STRUCTURE_FILE_NAME);
            var exportStructureDoc = new System.Xml.XmlDocument();
            exportStructureDoc.LoadXml(structure);

            ExportStructureReader reader = new ExportStructureReader();
            reader.Load(exportStructureDoc,
                        capacitiesInfo,
                        orderCustomPropertiesInfo,
                        addressFields);

            return reader;
        }

        #endregion // Internal static helpers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks is image fields present in seleceted.
        /// </summary>
        /// <param name="tables">Export table definitions.</param>
        /// <returns>TRUE if image fields present in seleceted.</returns>
        private bool _IsImageFieldsPresent(ICollection<ITableDefinition> tables)
        {
            Debug.Assert(null != tables);

            // check is image field select
            bool isImageFieldsPresent = false;
            foreach (ITableDefinition table in tables)
            {
                if ((TableType.Schedules == table.Type) || (TableType.Schema == table.Type))
                    continue; // skip this table

                TableDescription descr = _structureKeeper.GetTableDescription(table.Type);
                foreach (string field in table.Fields)
                {
                    FieldInfo info = descr.GetFieldInfo(field);
                    if (info.IsImage)
                    {
                        isImageFieldsPresent = true;
                        break; // result founded
                    }
                }

                if (isImageFieldsPresent)
                    break; // result founded
            }

            return isImageFieldsPresent;
        }

        /// <summary>
        /// Validates <c>Profile</c>.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> to validation.</param>
        private void _ValidateProfile(Profile profile)
        {
            if (null == profile)
                throw new ArgumentNullException(); // exception

            if (string.IsNullOrEmpty(profile.Name))
                throw new ArgumentException(Properties.Resources.ProfileNameCannotBeEmpty); // exception

            if (string.IsNullOrEmpty(profile.FilePath))
                throw new ArgumentException(Properties.Resources.ProfileFileNameCannotBeEmpty); // exception

            foreach (ITableDefinition table in profile.TableDefinitions)
            {
                if (0 == table.Fields.Count)
                {
                    var error = string.Format(Properties.Messages.Error_AbsentFieldsInTable,
                                              table.Name);
                    throw new ArgumentException(error); // exception
                }
            }
        }

        /// <summary>
        /// Prepares procedure to creating storage file.
        /// </summary>
        /// <param name="filePath">Storage full file name.</param>
        private void _PrepareDirToCreation(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            // delete file if already exists
            if (File.Exists(filePath))
                File.Delete(filePath);
            else
            {   // create directory if need
                string path = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Inits extended fields.
        /// </summary>
        private void _InitExtendedFields()
        {
            ICollection<TableInfo> tableInfos = _structureKeeper.GetPattern(ExportType.Access);
            foreach (TableInfo tableInfo in tableInfos)
            {
                if ((TableType.Schedules == tableInfo.Type) || (TableType.Schema == tableInfo.Type))
                    continue; // skip this table

                TableDescription descr = _structureKeeper.GetTableDescription(tableInfo.Type);
                foreach (string name in descr.GetFieldNames())
                {
                    FieldInfo info = descr.GetFieldInfo(name);
                    if (!info.IsDefault)
                        _extendedFields.Add(info.Name);
                }
            }
        }

        /// <summary>
        /// Adds exteneded fields (for reporting need full data generation).
        /// </summary>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="tables">Table definitions to updating.</param>
        private void _AddExtendedFields(ICollection<string> extendedFields,
                                        ref ICollection<ITableDefinition> tables)
        {
            if (0 < extendedFields.Count)
            {   // only if present
                foreach (ITableDefinition table in tables)
                {
                    if ((TableType.Schedules == table.Type) || (TableType.Schema == table.Type))
                        continue; // skip this table

                    foreach (string field in extendedFields)
                    {
                        if (table.SupportedFields.Contains(field))
                            table.AddField(field);
                    }
                }
            }
        }

        /// <summary>
        /// Adds hidden fields (for reporting need full data generation).
        /// </summary>
        /// <param name="tables">Table definitions to updating.</param>
        private void _AddHiddenFields(ref ICollection<ITableDefinition> tables)
        {
            // NOTE: add hidden fields - to full data generation
            ICollection<TableInfo> tableInfos = _structureKeeper.GetPattern(ExportType.Access);
            foreach (ITableDefinition table in tables)
            {
                if ((TableType.Schedules == table.Type) || (TableType.Schema == table.Type))
                    continue; // skip this table

                // add hidden fields to table definition
                TableDescription descr = _structureKeeper.GetTableDescription(table.Type);
                foreach (string name in descr.GetFieldNames())
                {
                    FieldInfo info = descr.GetFieldInfo(name);
                    if (info.IsHidden)
                        table.AddField(info.Name);
                }
            }
        }

        /// <summary>
        /// Creates image exporter.
        /// </summary>
        /// <param name="tables">Table definitions.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be
        /// used for export.</param>
        /// <returns>Created map image exporter or null if not need</returns>
        private MapImageExporter _CreateImageExporter(ICollection<ITableDefinition> tables,
                                                      MapLayer mapLayer,
                                                      ExportOptions options)
        {
            Debug.Assert(null != tables);

            // check is image field select
            bool isImageFieldPresent = _IsImageFieldsPresent(tables);

            MapImageExporter mapImageExporter = null;
            if (isImageFieldPresent)
            {
                if (null == mapLayer)
                {
                    throw new NotSupportedException(); // exception
                }

                mapImageExporter =
                    new MapImageExporter(options.ShowLeadingStemTime, options.ShowTrailingStemTime);
                mapImageExporter.Init(mapLayer);
            }

            return mapImageExporter;
        }

        /// <summary>
        /// Exports specified schedules.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> that has to be
        /// used for export.</param>
        /// <param name="schedules">Schedule to export.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        /// <param name="needAsyncStart">Start operation as asynchronous flag.</param>
        public void _DoExport(Profile profile,
                              ICollection<Schedule> schedules,
                              MapLayer mapLayer,
                              ExportOptions options,
                              bool needAsyncStart)
        {
            _ValidateProfile(profile);

            ICollection<ITableDefinition> tables = profile.TableDefinitions;
            _DoExport(profile, tables, schedules, null, mapLayer, options, needAsyncStart);
        }


        #region Private helpers. Report Source Part
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does report source.
        /// </summary>
        /// <param name="filePath">Full path to creatind data source.</param>
        /// <param name="schedules">Schedules to exporting datas.</param>
        /// <param name="routes">Routes to exporting datas (can be null).</param>
        /// <param name="extendedFields">Collection of extended fields that have to be generated.</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        /// <param name="needAsyncStart">Start operation as asynchronous flag.</param>
        /// <remarks>Routes must belong to the <c>schedule</c>. If <c>routes</c> collection is empty,
        /// Exporter will export all the routes from the <c>schedule</c>.</remarks>
        private void _DoReportSource(string filePath,
                                     ICollection<string> extendedFields,
                                     ICollection<Schedule> schedules,
                                     ICollection<Route> routes,
                                     MapLayer mapLayer,
                                     ExportOptions options,
                                     bool needAsyncStart)
        {
            // get export table descriptions
            Profile profile = CreateProfile(ExportType.Access, filePath);
            // add special fields for reporting
            ICollection<ITableDefinition> tables = profile.TableDefinitions;
            _AddExtendedFields(extendedFields, ref tables);
            _AddHiddenFields(ref tables);

            _DoExport(profile, tables, schedules, routes, mapLayer, options, needAsyncStart);
        }

        #endregion // Private helpers. Report Source Part

        #region Private helpers. Asynchronous
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates copy all elements.
        /// </summary>
        /// <param name="source">Source collection.</param>
        /// <returns>Collection with copy elements for source.</returns>
        private ICollection<T> _DoCopy<T>(ICollection<T> source)
            where T : ICloneable
        {
            var copy = new List<T>(source.Count);
            foreach (T obj in source)
            {
                T objCopy = (T)obj.Clone();

                // WORKAROUND: special routine for schedule - need init Unassigned Orders
                Schedule scheduleCopy = objCopy as Schedule;
                if (null != scheduleCopy)
                {
                    Schedule schedule = obj as Schedule;
                    Debug.Assert(null != obj);

                    IDataObjectCollection<Order> unassignedOrders = schedule.UnassignedOrders;
                    if (null != unassignedOrders)
                        scheduleCopy.UnassignedOrders = unassignedOrders;
                }

                copy.Add(objCopy);
            }

            return copy;
        }

        /// <summary>
        /// Gets routes from schedule by name.
        /// </summary>
        /// <param name="schedule">Source schedule.</param>
        /// <param name="routes">Routes for name getting.</param>
        /// <returns>Route from schedule with same name as in routes.</returns>
        private ICollection<Route> _GetScheduleRoutes(Schedule schedule, ICollection<Route> routes)
        {
            var relatedRoutes = new List<Route>(routes.Count);
            foreach (Route route in routes)
            {
                var scheduleRoutes =
                    from scheduleRoute in schedule.Routes
                    where
                        scheduleRoute.Name.Equals(route.Name, StringComparison.OrdinalIgnoreCase)
                    select scheduleRoute;
                Debug.Assert(null != scheduleRoutes);

                relatedRoutes.Add((Route)scheduleRoutes.First());
            }

            return relatedRoutes;
        }

        /// <summary>
        /// Does report source.
        /// </summary>
        /// <param name="settings">Export settings.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        private void _DoExport(ExportSettings settings, ICancelTracker tracker)
        {
            Debug.Assert(null != settings);

            // prepare direction for file
            _PrepareDirToCreation(settings.FilePath);

            if (ExportType.Access == settings.Type)
            {   // export to access
                // create image exporter
                MapImageExporter imageExporter = _CreateImageExporter(settings.Tables,
                                                                      settings.MapLayer,
                                                                      settings.Options);
                // do export to access format
                var writer = new AccessExporter(_structureKeeper);
                writer.DoExport(settings.FilePath,
                                settings.Tables,
                                settings.Schedules,
                                settings.Routes,
                                imageExporter,
                                tracker);
            }
            else if ((ExportType.TextRoutes == settings.Type) ||
                     (ExportType.TextStops == settings.Type) ||
                     (ExportType.TextOrders == settings.Type))
            {   // export as text
                Debug.Assert(1 == settings.Tables.Count);

                var writer = new TextExporter(_structureKeeper);
                writer.DoExport(settings.FilePath,
                                settings.Tables.First(),
                                settings.Schedules,
                                tracker);
            }
            else
            {
                Debug.Assert(false); // NOTE: not supported
            }
        }

        /// <summary>
        /// Export worker do work handler.
        /// </summary>
        /// <param name="sender">Export background worker.</param>
        /// <param name="e">Report source settings.</param>
        private void ExportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            Debug.Assert(null != bw);

            try
            {
                BackgroundWorkCancelTracker tracker = new BackgroundWorkCancelTracker(bw);

                var settings = e.Argument as ExportSettings;
                Debug.Assert(null != settings);

                _DoExport(settings, tracker);

                if (tracker.IsCancelled)
                    e.Cancel = true;
            }
            catch (UserBreakException)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Export worker completed event handler.
        /// </summary>
        /// <param name="sender">Export background worker.</param>
        /// <param name="e">Run worker completed event arguments.</param>
        private void ExportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            Debug.Assert(null != bw);

            // stop events
            bw.DoWork -= ExportWorker_DoWork;
            bw.RunWorkerCompleted -= ExportWorker_RunWorkerCompleted;

            // free resources
            bw.Dispose();
            _exportWorker = null;

            // fire - source created
            if (null != AsyncExportCompleted)
                AsyncExportCompleted(this, e);
        }

        /// <summary>
        /// Starts export background worker.
        /// </summary>
        /// <param name="settings">Report source settings.</param>
        private void _RunExportWorker(ExportSettings settings)
        {
            Debug.Assert(null != settings);

            // create background worker
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(ExportWorker_DoWork);
            bw.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(ExportWorker_RunWorkerCompleted);

            // run worker
            bw.RunWorkerAsync(settings);
            _exportWorker = bw;
        }

        /// <summary>
        /// Does export.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> to export.</param>
        /// <param name="tables">Export table definitions.</param>
        /// <param name="schedules">Schedules to exporting datas.</param>
        /// <param name="routes">Routes to exporting datas (can be null).</param>
        /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be used
        /// for images generation.</param>
        /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be used
        /// for export.</param>
        /// <param name="needAsyncStart">Start operation as asynchronous flag.</param>
        /// <remarks>Routes must belong to the <c>schedule</c>. If <c>routes</c> collection is empty,
        /// Exporter will export all the routes from the <c>schedule</c>.</remarks>
        private void _DoExport(Profile profile,
                               ICollection<ITableDefinition> tables,
                               ICollection<Schedule> schedules,
                               ICollection<Route> routes,
                               MapLayer mapLayer,
                               ExportOptions options,
                               bool needAsyncStart)
        {
            if (_IsImageFieldsPresent(tables))
            {
                if (null == mapLayer)
                    throw new ArgumentNullException("mapLayer"); // exception
            }

            // init parameters for operation
            var settings = new ExportSettings(profile.FilePath,
                                              profile.Type,
                                              tables,
                                              schedules,
                                              routes,
                                              mapLayer,
                                              options);
            // do start
            if (needAsyncStart)
            {   // start operation asynchronous
                if (IsExportOn())
                    throw new InvalidOperationException(Properties.Messages.Error_StartExportTwice); // exception

                _RunExportWorker(settings);
            }
            else
            {   // start operation synchronous
                _DoExport(settings, null);

                GC.Collect();
                GC.WaitForFullGCApproach();
            }
        }

        #endregion // Private helpers. Asynchronous

        /// <summary>
        /// Export report source operation settings.
        /// </summary>
        private sealed class ExportSettings
        {
            /// <summary>
            /// Creates and init a new instance of the <c>ExportDescriptionProvider</c> class.
            /// </summary>
            /// <param name="filePath">Path to Access database file where data will be exported.</param>
            /// <param name="type">Export type.</param>
            /// <param name="tables">Table definitions.</param>
            /// <param name="schedules">Collection of schedules to export.</param>
            /// <param name="routes">Collection of routes from <c>schedules</c> to export
            /// (can be null).</param>
            /// <param name="mapLayer"><see cref="P:ESRI.ArcLogistics.MapLayer" /> that has to be
            /// used for images generation (can be null if not needed).</param>
            /// <param name="options"><see cref="P:ESRI.ArcLogistics.Export.ExportOptions" /> that has to be
            /// used for export.</param>
            public ExportSettings(string filePath,
                                  ExportType type,
                                  ICollection<ITableDefinition> tables,
                                  ICollection<Schedule> schedules,
                                  ICollection<Route> routes,
                                  MapLayer mapLayer,
                                  ExportOptions options)
            {
                // check input values
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException("filePath"); // exception

                if (null == tables)
                    throw new ArgumentNullException("tables"); // exception

                if (null == schedules)
                    throw new ArgumentNullException("schedules"); // exception

                if (null == options)
                    throw new ArgumentNullException("options"); // exception

                // routes and mapLayer - can be null

                // init internal state
                FilePath = filePath;
                Type = type;
                Tables = tables;
                Schedules = schedules;
                Routes = routes;
                MapLayer = mapLayer;
                Options = options;
            }

            /// <summary>
            /// Report source file path.
            /// </summary>
            public readonly string FilePath;
            /// <summary>
            /// Export type.
            /// </summary>
            public readonly ExportType Type;
            /// <summary>
            /// Exported tables definitions.
            /// </summary>
            public readonly ICollection<ITableDefinition> Tables;
            /// <summary>
            /// Schedules to export.
            /// </summary>
            public readonly ICollection<Schedule> Schedules;
            /// <summary>
            /// Routes to export (can be null).
            /// </summary>
            public readonly ICollection<Route> Routes;
            /// <summary>
            /// Map layer that has to be used for images generation (can be null).
            /// </summary>
            public readonly MapLayer MapLayer;
            /// <summary>
            /// Export options.
            /// </summary>
            public readonly ExportOptions Options;
        }

        #endregion // Private helpers

        #region Private constants

        /// <summary>
        /// Name of the file with export structure.
        /// </summary>
        private const string EXPORT_STRUCTURE_FILE_NAME = "ExportStructure.xml";

        #endregion

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Presented profiles.
        /// </summary>
        private List<Profile> _profiles = new List<Profile>();
        /// <summary>
        /// Export structure reader.
        /// </summary>
        private ExportStructureReader _structureKeeper;
        /// <summary>
        /// Full collection of extended fields.
        /// </summary>
        private Collection<string> _extendedFields = new Collection<string>();

        /// <summary>
        /// Export operation background worker.
        /// </summary>
        private BackgroundWorker _exportWorker;

        #endregion // Private members
    }
}
