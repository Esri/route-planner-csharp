using System;
using System.IO;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Drawing.Imaging;
using System.Drawing;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Class that reperesent data exporter to Access format (Jet 4.0).
    /// </summary>
    internal sealed class AccessExporter
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>AccessExporter</c> class.
        /// </summary>
        /// <param name="structureKeeper">Export structure keeper.</param>
        public AccessExporter(IExportStructureKeeper structureKeeper)
        {
            Debug.Assert(null != structureKeeper);

            _listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            _structureKeeper = structureKeeper;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does export routine.
        /// </summary>
        /// <param name="filePath">Export file path.</param>
        /// <param name="tables">Table definitions.</param>
        /// <param name="schedules">Shedules to export.</param>
        /// <param name="routes">Routes to export (can be null).</param>
        /// <param name="imageExporter">Map's image exporter (can be null).</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <remarks>Routes must belong to the <c>schedule</c>. If <c>routes</c> collection
        /// is empty, Generator will use all the routes from the <c>schedules</c>.</remarks>
        public void DoExport(string filePath,
                             ICollection<ITableDefinition> tables,
                             ICollection<Schedule> schedules,
                             ICollection<Route> routes,
                             MapImageExporter imageExporter,
                             ICancelTracker tracker)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            Debug.Assert(null != tables);
            Debug.Assert(null != schedules);
            Debug.Assert((null == routes) || ((null != routes) && (1 == schedules.Count)));

            try
            {
                _CreateDatabase(filePath, tables);

                _CheckCancelState(tracker);

                _WriteContent(filePath, tables, schedules, routes, imageExporter, tracker);
            }
            catch (Exception ex)
            {
                if ( !(ex is UserBreakException) )
                    Logger.Error(ex);

                _DeleteFile(filePath);

                throw; // exception
            }
        }

        #endregion // Public methods

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Private creating helpers

        /// <summary>
        /// Converts <c>OleDbType</c> to <c>DataTypeEnum</c>.
        /// </summary>
        /// <param name="type">Type to converting.</param>
        /// <returns>Converted type.</returns>
        private ADOX.DataTypeEnum _ConvertType(OleDbType type)
        {
            ADOX.DataTypeEnum outType = ADOX.DataTypeEnum.adWChar;
            switch (type)
            {
                case OleDbType.SmallInt:
                    outType = ADOX.DataTypeEnum.adSmallInt;
                    break;
                case OleDbType.Integer:
                    outType = ADOX.DataTypeEnum.adInteger;
                    break;
                case OleDbType.Single:
                    outType = ADOX.DataTypeEnum.adSingle;
                    break;
                case OleDbType.Double:
                    outType = ADOX.DataTypeEnum.adDouble;
                    break;
                case OleDbType.Date:
                    outType = ADOX.DataTypeEnum.adDate;
                    break;
                case OleDbType.Guid:
                    outType = ADOX.DataTypeEnum.adGUID;
                    break;
                case OleDbType.WChar:
                    outType = ADOX.DataTypeEnum.adWChar;
                    break;
                case OleDbType.LongVarWChar:
                    outType = ADOX.DataTypeEnum.adLongVarWChar;
                    break;
                case OleDbType.LongVarBinary:
                    outType = ADOX.DataTypeEnum.adLongVarBinary;
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return outType;
        }

        /// <summary>
        /// Deletes file (safely).
        /// </summary>
        /// <param name="filePath">File path to deleting.</param>
        private void _DeleteFile(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {   // do nothing
            }
        }

        /// <summary>
        /// Gets connection string.
        /// </summary>
        /// <param name="filePath">Export file path.</param>
        /// <returns>Created connetion string.</returns>
        private string _GetConnectionString(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            // SQL specific: in connection string need replace quote to double-quote in file path.
            string formatedFilePath = filePath;
            if (-1 != filePath.IndexOf(QUOTE))
                formatedFilePath = filePath.Replace(QUOTE, DOUBLE_QUOTE);

            return string.Format(CONNECTION_STRING_FORMAT, formatedFilePath);
        }

        /// <summary>
        /// Creates database file.
        /// </summary>
        /// <param name="filePath">Export file path.</param>
        /// <returns>Created database.</returns>
        private ADOX.Catalog _CreateDatabase(string filePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));

            string connectionString = _GetConnectionString(filePath);

            // create empty database
            var catalog = new ADOX.CatalogClass();
            catalog.Create(connectionString);

            return catalog;
        }

        /// <summary>
        /// Adds fields to table.
        /// </summary>
        /// <param name="tableDefinition">Table definition.</param>
        /// <param name="tableDescription">Table Description.</param>
        /// <param name="columns">Database columns.</param>
        private void _AddFieldsToTable(ITableDefinition tableDefinition,
                                       TableDescription tableDescription,
                                       ADOX.Columns columns)
        {
            Debug.Assert(null != tableDefinition);
            Debug.Assert(null != tableDescription);
            Debug.Assert(null != columns);

            ICollection<string> fields = tableDefinition.Fields;
            foreach (string field in fields)
            {
                FieldInfo info = tableDescription.GetFieldInfo(field);
                Debug.Assert(null != info);
                columns.Append(info.Name, _ConvertType(info.Type), info.Size);

                // make field not required
                ADOX.Column column = columns[info.Name];
                column.Attributes = ADOX.ColumnAttributesEnum.adColNullable;
            }
        }

        /// <summary>
        /// Checks is index field selected.
        /// </summary>
        /// <param name="indexFieldNames">Index field names.</param>
        /// <param name="selectedFields">Selected fields.</param>
        /// <returns>TRUE if all index fields selected.</returns>
        private bool _IsIndexFieldSelected(StringCollection indexFieldNames,
                                           ICollection<string> selectedFields)
        {
            Debug.Assert(null != indexFieldNames);
            Debug.Assert(0 < indexFieldNames.Count);
            Debug.Assert(null != selectedFields);

            bool isSelected = true;
            foreach (string indexFieldName in indexFieldNames)
            {
                if (!selectedFields.Contains(indexFieldName))
                {
                    isSelected = false;
                    break; // found first not selected
                }
            }

            return isSelected;
        }

        /// <summary>
        /// Adds key to index of table.
        /// </summary>
        /// <param name="tableDescription">Table description.</param>
        /// <param name="indexDefinition">Index definition.</param>
        /// <param name="indexes">Database indexes.</param>
        private void _AddKeyToTableIndex(TableDescription tableDescription,
                                         TableIndex indexDefinition,
                                         ADOX.Indexes indexes)
        {
            Debug.Assert(null != tableDescription);
            Debug.Assert(null != indexDefinition);
            Debug.Assert(null != indexes);

            var index = new ADOX.Index();
            ADOX.Columns columns = index.Columns;
            switch (indexDefinition.Type)
            {
                case TableIndexType.Primary:
                case TableIndexType.Simple:
                {
                    string field = indexDefinition.FieldNames[0];
                    if (TableIndexType.Primary == indexDefinition.Type)
                    {
                        index.Name = INDEX_PRIMARY_KEY;
                        index.PrimaryKey = true;
                        index.Unique = true;
                    }
                    else // simple
                        index.Name = field;

                    FieldInfo info = tableDescription.GetFieldInfo(field);
                    Debug.Assert(null != info);
                    columns.Append(info.Name, _ConvertType(info.Type), info.Size);
                    break;
                }

                case TableIndexType.Multiple:
                {
                    var sbKeyName = new StringBuilder();
                    foreach (string field in indexDefinition.FieldNames)
                    {
                        FieldInfo info = tableDescription.GetFieldInfo(field);
                        Debug.Assert(null != info);
                        columns.Append(info.Name, _ConvertType(info.Type), info.Size);

                        if (!string.IsNullOrEmpty(sbKeyName.ToString()))
                            sbKeyName.Append(SQL_KEY_SYMBOL);
                        sbKeyName.Append(field);
                    }

                    index.Name = sbKeyName.ToString();
                    break;
                }

                default:
                {
                    Debug.Assert(false); // NOTE: not supported
                    break;
                }
            }

            index.IndexNulls = ADOX.AllowNullsEnum.adIndexNullsAllow;
            indexes.Append(index, null);
        }

        /// <summary>
        /// Adds keys to index of table.
        /// </summary>
        /// <param name="tableDescription">Table description.</param>
        /// <param name="tableDefinition">Table definition.</param>
        /// <param name="indexes">Database indexes.</param>
        private void _AddKeysToTableIndex(TableDescription tableDescription,
                                          ITableDefinition tableDefinition,
                                          ADOX.Indexes indexes)
        {
            ICollection<TableInfo> patternTables = _structureKeeper.GetPattern(ExportType.Access);
            foreach (TableInfo tableInfo in patternTables)
            {
                if (tableInfo.Type != tableDefinition.Type)
                    continue; // skip

                foreach(TableIndex indexDefinition in tableInfo.Indexes)
                {
                    if (_IsIndexFieldSelected(indexDefinition.FieldNames, tableDefinition.Fields))
                        _AddKeyToTableIndex(tableDescription, indexDefinition, indexes);
                }

                break; // process done
            }
        }

        /// <summary>
        /// Creates database.
        /// </summary>
        /// <param name="filePath">Database file path to creation.</param>
        /// <param name="tableDefinitions">Table definitions.</param>
        private void _CreateDatabase(string filePath,
                                     ICollection<ITableDefinition> tableDefinitions)
        {
            ADOX.Catalog catalog = _CreateDatabase(filePath);
            try
            {
                // add tables
                ADOX.Tables tables = catalog.Tables;
                foreach (ITableDefinition tableDefinition in tableDefinitions)
                {
                    TableDescription tableDescription =
                        _structureKeeper.GetTableDescription(tableDefinition.Type);

                    // create access table wiht name and empty fields
                    var table = new ADOX.TableClass();
                    table.ParentCatalog = catalog;
                    table.Name = tableDescription.Name;

                    _AddFieldsToTable(tableDefinition, tableDescription, table.Columns);
                    _AddKeysToTableIndex(tableDescription, tableDefinition, table.Indexes);

                    // add table to catalog
                    tables.Append(table);
                }
            }
            finally
            {
                if (null != catalog)
                {   // special routine to close connection
                    object adodbConnection = catalog.ActiveConnection;
                    adodbConnection.GetType().InvokeMember("Close",
                                                           BindingFlags.InvokeMethod,
                                                           null,
                                                           adodbConnection,
                                                           new object[0]);
                    catalog.ActiveConnection = null;
                }
            }
        }

        #endregion // Private creating helpers

        /// <summary>
        /// Checks cancel state.
        /// </summary>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <remarks>If cancelled throw UserBreakException.</remarks>
        private void _CheckCancelState(ICancelTracker tracker)
        {
            if (tracker != null)
            {
                if (tracker.IsCancelled)
                    throw new UserBreakException(); // exception
            }
        }

        /// <summary>
        /// Formats field name.
        /// </summary>
        /// <param name="fieldName">Field name.</param>
        /// <returns>Formated field name.</returns>
        private string _FormatFieldName(string fieldName)
        {
            // NOTE: if field name coincide with reserved words
            //       - it's necessary to write such names in brackets
            return (_structureKeeper.IsNameReserved(fieldName)) ?
                        string.Format(SQL_SYSTEM_NAME_FORMAT, fieldName) : fieldName;
        }

        /// <summary>
        /// Creates insert command.
        /// </summary>
        /// <param name="tableName">Table name.</param>
        /// <param name="tableDescription">Table Description.</param>
        /// <param name="fields">Fields to export.</param>
        /// <returns>Insert command.</returns>
        private OleDbCommand _CreateInsertCommand(string tableName,
                                                  TableDescription tableDescription,
                                                  ICollection<string> fields)
        {
            var command = new OleDbCommand();

            var valueNames = new StringBuilder();
            var values = new StringBuilder();
            OleDbParameterCollection parameters = command.Parameters;
            foreach (string field in fields)
            {
                if (!string.IsNullOrEmpty(valueNames.ToString()))
                {
                    valueNames.Append(SQL_PARAM_SEPARATOR);
                    values.Append(SQL_PARAM_SEPARATOR);
                }

                FieldInfo info = tableDescription.GetFieldInfo(field);
                Debug.Assert(null != info);

                string realName = _FormatFieldName(field);
                valueNames.Append(realName);
                values.Append(SQL_VALUE_SYMBOL);

                parameters.Add(field, info.Type, info.Size, field);
            }

            command.CommandText = string.Format(SQL_INSERT_COMMAND_FORMAT, tableName,
                                                valueNames.ToString(), values.ToString());
            return command;
        }

        /// <summary>
        /// Creates special field names list.
        /// </summary>
        /// <param name="propNames">Property names.</param>
        /// <returns>String with property name list.</returns>
        private string _CreateSpecialFieldNames(ICollection<string> propNames)
        {
            var sb = new StringBuilder();
            foreach (string name in propNames)
            {
                if (0 < sb.Length)
                    sb.Append(SCHEMA_TABLE_NAMELIST_SEPARATOR);

                sb.Append(name);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Adds schema row.
        /// </summary>
        /// <param name="fields">Fields to export.</param>
        /// <param name="relationType">Relation type.</param>
        /// <param name="propNames">Property names.</param>
        /// <param name="table">Table to data writing.</param>
        private void _AddSchemaRow(ICollection<string> fields,
                                   string relationType,
                                   ICollection<string> propNames,
                                   DataTable table)
        {
            // create row for special fields (OrderCustomProperies and Capacities)
            DataRow dr = table.NewRow();
            foreach (string tableField in fields)
            {
                string value = null;
                switch (tableField)
                {
                    case "Type":
                        value = relationType;
                        break;

                    case "FieldNames":
                        value = _CreateSpecialFieldNames(propNames);
                        break;

                    default:
                        Debug.Assert(false); // NOTE: not supported
                        break;
                }

                dr[tableField] = value;
            }

            table.Rows.Add(dr);
        }

        /// <summary>
        /// Formats field value.
        /// </summary>
        /// <param name="value">Value to write.</param>
        /// <returns>Formated value.</returns>
        private object _FormatFieldValue(DataWrapper value)
        {
            object val = DBNull.Value;
            if (null != value.Value)
                val = value.Value;
            else
            {   // value empty - do special routine
                switch (value.Type)
                {
                    case OleDbType.SmallInt:
                    case OleDbType.Integer:
                        val = -1;
                        break;

                    case OleDbType.Double:
                    case OleDbType.Single:
                        val = 0.0;
                        break;

                    // default: NOTE: do nothing - DBNull.Value
                }
            }

            return val;
        }

        /// <summary>
        /// Writes schedules.
        /// </summary>
        /// <param name="schedules">Schedules to export.</param>
        /// <param name="fields">Fields to export.</param>
        /// <param name="data">Data keeper.</param>
        /// <param name="table">Table to data writing.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        private void _WriteSchedules(ICollection<Schedule> schedules,
                                     ICollection<string> fields,
                                     DataKeeper data,
                                     DataTable table,
                                     ICancelTracker tracker)
        {
            foreach (Schedule schedule in schedules)
            {
                _CheckCancelState(tracker);

                DataRow dr = table.NewRow();
                foreach (string field in fields)
                    dr[field] = _FormatFieldValue(data.GetScheduleFieldValue(field, schedule));

                table.Rows.Add(dr);
            }
        }

        /// <summary>
        /// Converts image to blob.
        /// </summary>
        /// <param name="image">Image to converting.</param>
        /// <returns>Blob version of image data.</returns>
        private byte[] _ConvertImageToBlob(Image image)
        {
            byte[] imageContent = null;

            if (null != image)
            {
                // make a memory stream to work with the image bytes
                using(MemoryStream stream = new MemoryStream())
                {
                    // put the image into the memory stream
                    image.Save(stream, ImageFormat.Bmp);

                    // make byte array the same size as the image
                    imageContent = new Byte[stream.Length];
                    // rewind the memory stream
                    stream.Position = 0;
                    // load the byte array with the image
                    stream.Read(imageContent, 0, (int)stream.Length);
                }
            }

            return imageContent;
        }

        /// <summary>
        /// Writes routes.
        /// </summary>
        /// <param name="schedules">Schedules to export.</param>
        /// <param name="routes">Routes to export.</param>
        /// <param name="fields">Fields to export.</param>
        /// <param name="data">Data keeper.</param>
        /// <param name="imageExporter">Map image exporter.</param>
        /// <param name="table">Table to data writing.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        private void _WriteRoutes(ICollection<Schedule> schedules,
                                  ICollection<Route> routes,
                                  ICollection<string> fields,
                                  DataKeeper data,
                                  MapImageExporter imageExporter,
                                  DataTable table,
                                  ICancelTracker tracker)
        {
            foreach (Schedule schedule in schedules)
            {
                _CheckCancelState(tracker);

                Guid scheduleID = schedule.Id;

                foreach (Route route in schedule.Routes)
                {
                    _CheckCancelState(tracker);

                    if ((null != routes) && !routes.Contains(route))
                        continue; // NOTE: skeep not selected

                    if ((null == route.Stops) || (0 == route.Stops.Count))
                        continue; // NOTE: skeep empty routes

                    DataRow dr = table.NewRow();
                    foreach (string field in fields)
                    {
                        if ("OverviewMap" == field)
                        { // special routine
                            Debug.Assert(null != imageExporter);

                            _CheckCancelState(tracker);

                            Image image = imageExporter.GetRouteImage(route,
                                                                      ROUTE_MAP_IMAGE_SIZE_X,
                                                                      ROUTE_MAP_IMAGE_SIZE_Y,
                                                                      IMAGE_DPI);
                            dr[field] = _ConvertImageToBlob(image);
                            if (null != image)
                            {
                                image.Dispose();
                                image = null;
                            }
                        }
                        else
                        {
                            Debug.Assert("PlannedDate" != field); // NOTE: do not supported

                            dr[field] = _FormatFieldValue(data.GetRouteFieldValue(field,
                                                                                  scheduleID,
                                                                                  route));
                        }
                    }
                    table.Rows.Add(dr);
                }
            }
        }

        /// <summary>
        /// Gets formatted way text with length.
        /// </summary>
        /// <param name="lengthInMiles">Way length [miles].</param>
        /// <returns>Formatted length text.</returns>
        private string _GetLengthString(double lengthInMiles)
        {
            var sb = new StringBuilder(Properties.Resources.DirectionsWordDrive);

            double length = 0;
            string distanceMeasuringUnitName = null;
            if (RegionInfo.CurrentRegion.IsMetric)
            {
                length = lengthInMiles * SolverConst.KM_PER_MILE;
                distanceMeasuringUnitName = Properties.Resources.DistanceInKilometresText;
            }
            else
            {
                length = lengthInMiles;
                distanceMeasuringUnitName = Properties.Resources.DistanceInMilesText;
            }

            if (length < MIN_DISTANCE_TO_SHOW)
            {
                sb.Append(LENGTH_MIN_SYBOLS);
                length = MIN_DISTANCE_TO_SHOW;
            }

            sb.AppendFormat(LENGTH_TEXT_FORMAT, length.ToString("0.0"), distanceMeasuringUnitName);
            return sb.ToString();
        }

        /// <summary>
        /// Gets directions text.
        /// </summary>
        /// <param name="directions">Directions.</param>
        /// <returns>Created directions text.</returns>
        private string _GetDirectionsText(Direction[] directions)
        {
            StringBuilder sb = new StringBuilder();
            if (null != directions)
            {
                for (int i = 0; i < directions.Length; ++i)
                {
                    Direction direction = directions[i];

                    string text = null;
                    if ((StopManeuverType.Depart != direction.ManeuverType) &&
                        (StopManeuverType.Stop != direction.ManeuverType))
                    {
                        string lengthText = _GetLengthString(direction.Length); // exception
                        text = string.Format(DIRECTION_TEXT_FORMAT, direction.Text, lengthText);
                    }
                    else
                        text = direction.Text;

                    sb.AppendLine(text);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes stops.
        /// </summary>
        /// <param name="schedules">Schedules to export.</param>
        /// <param name="routes">Routes to export.</param>
        /// <param name="fields">Fields to export.</param>
        /// <param name="data">Data keeper.</param>
        /// <param name="imageExporter">Map image exporter.</param>
        /// <param name="table">Table to data writing.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        private void _WriteStops(ICollection<Schedule> schedules,
                                 ICollection<Route> routes,
                                 ICollection<string> fields,
                                 DataKeeper data,
                                 MapImageExporter imageExporter,
                                 DataTable table,
                                 ICancelTracker tracker)
        {
            foreach (Schedule schedule in schedules)
            {
                _CheckCancelState(tracker);

                // stops
                Guid scheduleId = schedule.Id;
                foreach (Route route in schedule.Routes)
                {
                    _CheckCancelState(tracker);

                    if ((null != routes) && !routes.Contains(route))
                        continue; // NOTE: skeep not selected

                    IDataObjectCollection<Stop> stops = route.Stops;
                    for (int index = 0; index < stops.Count; ++index)
                    {
                        _CheckCancelState(tracker);

                        // write stop
                        Stop stop = stops[index];

                        DataRow dr = table.NewRow();
                        foreach (string field in fields)
                        {
                            if (field == "StopVicinityMap")
                            { // NOTE: special routine
                                Debug.Assert(null != imageExporter);

                                _CheckCancelState(tracker);

                                Image image = imageExporter.GetStopImage(route,
                                                                         stop,
                                                                         STOP_MAP_RADIUS,
                                                                         STOP_MAP_IMAGE_SIZE_X,
                                                                         STOP_MAP_IMAGE_SIZE_Y,
                                                                         IMAGE_DPI);
                                dr[field] = _ConvertImageToBlob(image);
                                if (null != image)
                                {
                                    image.Dispose();
                                    image = null;
                                }
                            }
                            else if (field == "Directions")
                            { // NOTE: special routine
                                System.Text.UnicodeEncoding encoding =
                                    new System.Text.UnicodeEncoding();
                                dr[field] = encoding.GetBytes(_GetDirectionsText(stop.Directions));
                            }
                            else
                            {
                                dr[field] = _FormatFieldValue(data.GetStopFieldValue(field,
                                                                                     scheduleId,
                                                                                     stop));
                            }
                        }

                        table.Rows.Add(dr);
                    }
                }

                _CheckCancelState(tracker);

                // unassigned orders
                IDataObjectCollection<Order> orders = schedule.UnassignedOrders;
                if (null != orders)
                {
                    foreach (Order order in orders)
                    {
                        _CheckCancelState(tracker);

                        DataRow dr = table.NewRow();
                        foreach (string field in fields)
                        {
                            DataWrapper wrapper = data.GetStopFieldValue(field, scheduleId, order);
                            dr[field] = _FormatFieldValue(wrapper);
                        }

                        table.Rows.Add(dr);
                    }
                }
            }
        }

        /// <summary>
        /// Write schema.
        /// </summary>
        /// <param name="fields">Fields to export.</param>
        /// <param name="table">Table to data writing.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        private void _WriteSchema(ICollection<string> fields,
                                  DataTable table,
                                  ICancelTracker tracker)
        {
            TableDescription description = _structureKeeper.GetTableDescription(TableType.Schema);

            // Capacities
            var capacitiesSpecFields = new List<string> ();
            CapacitiesInfo capacitiesInfo = _structureKeeper.CapacitiesInfo;
            for (int index = 0; index < capacitiesInfo.Count; ++index)
            {
                string relativeName = description.ValidateRelativeName(capacitiesInfo[index].Name);
                capacitiesSpecFields.Add(relativeName);
            }

            _CheckCancelState(tracker);

            // add text Capacities
            if (0 < capacitiesSpecFields.Count)
                _AddSchemaRow(fields, SCHEMA_TABLE_TYPE_CAPACITIES,
                              capacitiesSpecFields.AsReadOnly(), table);

            // CustomOrderProperties
            var textCustomPropSpecFields = new List<string>();
            var numericCustomPropSpecFields = new List<string>();
            OrderCustomPropertiesInfo customOrderPropsInfo =
                _structureKeeper.OrderCustomPropertiesInfo;
            for (int index = 0; index < customOrderPropsInfo.Count; ++index)
            {
                var name = description.ValidateRelativeName(customOrderPropsInfo[index].Name);
                if (customOrderPropsInfo[index].Type == OrderCustomPropertyType.Text)
                    textCustomPropSpecFields.Add(name);
                else
                {   // numeric
                    Debug.Assert(customOrderPropsInfo[index].Type == OrderCustomPropertyType.Numeric);
                    numericCustomPropSpecFields.Add(name);
                }
            }

            _CheckCancelState(tracker);

            // add text CustomOrderProperties
            if (0 < textCustomPropSpecFields.Count)
                _AddSchemaRow(fields, SCHEMA_TABLE_TYPE_CUSTOMPROP_TEXT,
                              textCustomPropSpecFields.AsReadOnly(), table);

            _CheckCancelState(tracker);

            // add numeric CustomOrderProperties
            if (0 < numericCustomPropSpecFields.Count)
                _AddSchemaRow(fields, SCHEMA_TABLE_TYPE_CUSTOMPROP_NUMERIC,
                              numericCustomPropSpecFields.AsReadOnly(), table);
        }

        /// <summary>
        /// Write database content.
        /// </summary>
        /// <param name="filePath">Export file path.</param>
        /// <param name="tables">Table definitions.</param>
        /// <param name="schedules">Schedules to export.</param>
        /// <param name="routes">Routes to export.</param>
        /// <param name="imageExporter">Map image exporter.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        private void _WriteContent(string filePath,
                                   ICollection<ITableDefinition> tables,
                                   ICollection<Schedule> schedules,
                                   ICollection<Route> routes,
                                   MapImageExporter imageExporter,
                                   ICancelTracker tracker)
        {
            OleDbConnection connection = new OleDbConnection(_GetConnectionString(filePath));

            try
            {
                foreach (ITableDefinition tableDef in tables)
                {
                    _CheckCancelState(tracker);

                    TableDescription tableDescription =
                        _structureKeeper.GetTableDescription(tableDef.Type);
                    DataKeeper data = new DataKeeper(_listSeparator, tableDescription);
                    ICollection<string> fields = tableDef.Fields;

                    string tableName = tableDef.Name;

                    // obtain dataset
                    string selectCommand = string.Format(SQL_SELECT_COMMAND_FORMAT, tableName);
                    OleDbCommand accessCommand = new OleDbCommand(selectCommand, connection);

                    OleDbDataAdapter dataAdapter = new OleDbDataAdapter(accessCommand);
                    DataSet dataSet = new DataSet(tableName);
                    dataAdapter.Fill(dataSet, tableName);

                    // select table
                    DataTable table = dataSet.Tables[tableName];

                    // write data to table
                    switch (tableDef.Type)
                    {
                        case TableType.Schedules:
                            _WriteSchedules(schedules, fields, data, table, tracker);
                            break;

                        case TableType.Routes:
                            {
                                _WriteRoutes(schedules,
                                             routes,
                                             fields,
                                             data,
                                             imageExporter,
                                             table,
                                             tracker);
                                break;
                            }

                        case TableType.Stops:
                            {
                                _WriteStops(schedules,
                                            routes,
                                            fields,
                                            data,
                                            imageExporter,
                                            table,
                                            tracker);
                                break;
                            }

                        case TableType.Schema:
                            _WriteSchema(fields, table, tracker);
                            break;

                        default:
                            Debug.Assert(false); // NOTE: not supported
                            break;
                    }

                    _CheckCancelState(tracker);

                    // set insert command
                    OleDbCommand insertCommand = _CreateInsertCommand(tableName,
                                                                      tableDescription,
                                                                      fields);
                    insertCommand.Connection = connection;
                    dataAdapter.InsertCommand = insertCommand;

                    // store changes
                    dataAdapter.Update(table);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                connection.Close();
            }
        }

        #endregion // Private helpers

        #region Private constans
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string CONNECTION_STRING_FORMAT =
            "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='{0}'";
        private const string INDEX_PRIMARY_KEY = "PrimaryKey";

        private const string QUOTE = "'";
        private const string DOUBLE_QUOTE = "''";

        private const string SQL_SYSTEM_NAME_FORMAT = "[{0}]";
        private const string SQL_PARAM_SEPARATOR = ", ";
        private const string SQL_INSERT_COMMAND_FORMAT = "INSERT INTO [{0}] ({1}) VALUES ({2})";
        private const string SQL_SELECT_COMMAND_FORMAT = "SELECT * FROM [{0}]";
        private const char SQL_KEY_SYMBOL = '_';
        private const char SQL_VALUE_SYMBOL = '?';

        private const string SCHEMA_TABLE_TYPE_CAPACITIES = "Capacities";
        private const string SCHEMA_TABLE_TYPE_CUSTOMPROP_TEXT = "CustomOrderPropertiesText";
        private const string SCHEMA_TABLE_TYPE_CUSTOMPROP_NUMERIC = "CustomOrderPropertiesNumeric";
        private const char SCHEMA_TABLE_NAMELIST_SEPARATOR = ',';

        private const string DIRECTION_TEXT_FORMAT = "{0}, {1}";
        private const string LENGTH_TEXT_FORMAT = " {0} {1}";
        private const string LENGTH_MIN_SYBOLS = " <";

        private const int ROUTE_MAP_IMAGE_SIZE_X = 640; // [pixel]
        private const int ROUTE_MAP_IMAGE_SIZE_Y = 360; // [pixel]
        private const int STOP_MAP_IMAGE_SIZE_X = 230; // [pixel]
        private const int STOP_MAP_IMAGE_SIZE_Y = 180; // [pixel]
        private const double STOP_MAP_RADIUS = 1609 / 4; // [meters]
        private const int IMAGE_DPI = 96;
        private const double MIN_DISTANCE_TO_SHOW = 0.1;

        #endregion // Private constans

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Current locale's list separator.
        /// </summary>
        private string _listSeparator;
        /// <summary>
        /// Export structure keeper.
        /// </summary>
        private IExportStructureKeeper _structureKeeper;

        #endregion // Private members
    }
}
