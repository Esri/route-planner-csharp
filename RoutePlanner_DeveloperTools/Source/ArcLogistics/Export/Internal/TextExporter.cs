using System;
using System.IO;
using System.Text;
using System.Data.OleDb;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Class that implement text export.
    /// </summary>
    internal sealed class TextExporter
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>TextExporter</c> class.
        /// </summary>
        /// <param name="structureKeeper">Export structure keeper.</param>
        public TextExporter(IExportStructureKeeper structureKeeper)
        {
            Debug.Assert(null != structureKeeper);

            _separator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
            _listSeparator = (_separator == ",")? ";" : ",";

            _structureKeeper = structureKeeper;
        }

        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does export.
        /// </summary>
        /// <param name="filePath">Export file path.</param>
        /// <param name="table">Export table description.</param>
        /// <param name="schedules">Schedules to exporting.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        public void DoExport(string filePath,
                             ITableDefinition table,
                             ICollection<Schedule> schedules,
                             ICancelTracker tracker)
        {
            Debug.Assert(!string.IsNullOrEmpty(filePath));
            Debug.Assert(null != table);
            Debug.Assert(null != schedules);
            Debug.Assert((TableType.Routes == table.Type) ||
                         (TableType.Stops == table.Type) ||
                         (TableType.Orders == table.Type));

            try
            {
                // create file
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.AutoFlush = true;

                        ICollection<string> fields = table.Fields;
                        Debug.Assert(0 < table.Fields.Count);

                        // write header
                        sw.WriteLine(_AssemblyHeaderString(table, fields));

                        _CheckCancelState(tracker);

                        _WriteContent(table.Type, schedules, fields, tracker, sw);
                    }
                }
            }
            catch(Exception e)
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);

                throw (e);
            }
        }

        #endregion // Public methods

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

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
        /// Asseblyes header string.
        /// </summary>
        /// <param name="table">Export table description.</param>
        /// <param name="fields">Field names.</param>
        /// <returns>Header string.</returns>
        private string _AssemblyHeaderString(ITableDefinition table, ICollection<string> fields)
        {
            bool isSeparatorNeed = false;
            var sb = new StringBuilder();
            foreach (string field in fields)
            {
                if (isSeparatorNeed)
                    sb.Append(_separator);

                sb.AppendFormat(STRING_FORMAT, table.GetFieldTitleByName(field));
                isSeparatorNeed = true;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Fromat field value.
        /// </summary>
        /// <param name="value">Export value.</param>
        /// <returns>Formated field value string.</returns>
        private string _FormatFieldValue(DataWrapper value)
        {
            string result = (null == value.Value) ? string.Empty : value.Value.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                if ((OleDbType.WChar == value.Type) ||
                    (OleDbType.LongVarWChar == value.Type))
                    result = string.Format(STRING_FORMAT, result);

                else if (OleDbType.Date == value.Type)
                {
                    DateTime? date = (DateTime?)value.Value;
                    Debug.Assert(date.HasValue);
                    result = date.Value.ToShortDateString();
                }
            }
            else
            {
                switch (value.Type)
                {
                    case OleDbType.SmallInt:
                    case OleDbType.Integer:
                    {
                        int val = -1;
                        result = val.ToString();
                        break;
                    }

                    case OleDbType.Double:
                    case OleDbType.Single:
                    {
                        double val = 0;
                        result = val.ToString();
                        break;
                    }

                    case OleDbType.Date:
                    case OleDbType.Guid:
                        result = string.Empty;
                        break;

                    case OleDbType.WChar:
                    case OleDbType.LongVarWChar:
                        result = string.Format(STRING_FORMAT, string.Empty);
                        break;

                    default:
                        Debug.Assert(false); // NOTE: not supported
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Assemblyes route's values string.
        /// </summary>
        /// <param name="fields">Exported fields.</param>
        /// <param name="data">Data keeper.</param>
        /// <param name="scheduleId">Schedule ID.</param>
        /// <param name="route">Route to exporting.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <returns>Route's values string.</returns>
        private string _AssemblyRouteString(ICollection<string> fields,
                                            DataKeeper data,
                                            Guid scheduleId,
                                            Route route,
                                            ICancelTracker tracker)
        {
            bool isSeparatorNeed = false;
            var sb = new StringBuilder();
            foreach (string field in fields)
            {
                _CheckCancelState(tracker);

                if (isSeparatorNeed)
                    sb.Append(_separator);

                DataWrapper value = data.GetRouteFieldValue(field, scheduleId, route);
                sb.Append(_FormatFieldValue(value));
                isSeparatorNeed = true;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Assemblyes stop's values string.
        /// </summary>
        /// <param name="fields">Exported fields.</param>
        /// <param name="data">Data keeper.</param>
        /// <param name="scheduleId">Schedule ID.</param>
        /// <param name="obj">Stop to exporting.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <returns>Stop's values string.</returns>
        private string _AssemblyStopString(ICollection<string> fields,
                                           DataKeeper data,
                                           Guid scheduleId,
                                           DataObject obj,
                                           ICancelTracker tracker)
        {
            bool isSeparatorNeed = false;
            var sb = new StringBuilder();
            foreach (string field in fields)
            {
                _CheckCancelState(tracker);

                if (isSeparatorNeed)
                    sb.Append(_separator);

                DataWrapper value = data.GetStopFieldValue(field, scheduleId, obj);
                sb.Append(_FormatFieldValue(value));
                isSeparatorNeed = true;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Writes unassigned orders content.
        /// </summary>
        /// <param name="data">Data keeper.</param>
        /// <param name="schedule">Schedule to export.</param>
        /// <param name="fields">Exported fields.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <param name="sw">Export writer.</param>
        private void _WriteUnassignedOrdersContent(DataKeeper data,
                                                   Schedule schedule,
                                                   ICollection<string> fields,
                                                   ICancelTracker tracker,
                                                   StreamWriter sw)
        {
            Guid scheduleID = schedule.Id;

            IDataObjectCollection<Order> orders = schedule.UnassignedOrders;
            if (null != orders)
            {   // unassigned orders
                foreach (Order order in orders)
                {
                    _CheckCancelState(tracker);

                    string exportString = null;
                    exportString = _AssemblyStopString(fields, data, scheduleID, order, tracker);

                    Debug.Assert(!string.IsNullOrEmpty(exportString));
                    sw.WriteLine(exportString);
                }
            }
        }

        /// <summary>
        /// Writes routes content.
        /// </summary>
        /// <param name="data">Data keeper.</param>
        /// <param name="schedules">Schedules to export.</param>
        /// <param name="fields">Exported fields.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <param name="sw">Export writer.</param>
        private void _WriteRoutesContent(DataKeeper data,
                                         ICollection<Schedule> schedules,
                                         ICollection<string> fields,
                                         ICancelTracker tracker,
                                         StreamWriter sw)
        {
            foreach (Schedule schedule in schedules)
            {
                Guid scheduleID = schedule.Id;
                foreach (Route route in schedule.Routes)
                {
                    _CheckCancelState(tracker);

                    if ((null == route.Stops) || (0 == route.Stops.Count))
                        continue; // NOTE: skip empty routes

                   string exportString = _AssemblyRouteString(fields,
                                                              data,
                                                              scheduleID,
                                                              route,
                                                              tracker);

                    Debug.Assert(!string.IsNullOrEmpty(exportString));
                    sw.WriteLine(exportString);
                }
            }
        }

        /// <summary>
        /// Writes stops content.
        /// </summary>
        /// <param name="data">Data keeper.</param>
        /// <param name="schedules">Schedules to export.</param>
        /// <param name="fields">Exported fields.</param>
        /// <param name="writeOnlyOrders">Write only orders flag.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <param name="sw">Export writer.</param>
        private void _WriteStopsContent(DataKeeper data,
                                        ICollection<Schedule> schedules,
                                        ICollection<string> fields,
                                        bool writeOnlyOrders,
                                        ICancelTracker tracker,
                                        StreamWriter sw)
        {
            foreach (Schedule schedule in schedules)
            {
                Guid scheduleID = schedule.Id;
                foreach (Route route in schedule.Routes)
                {   // stops
                    IDataObjectCollection<Stop> stops = route.Stops;
                    foreach (Stop stop in stops)
                    {
                        _CheckCancelState(tracker);

                        if (!writeOnlyOrders ||
                            (writeOnlyOrders && (stop.StopType == StopType.Order)))
                        {
                            string exportString = _AssemblyStopString(fields,
                                                                      data,
                                                                      scheduleID,
                                                                      stop,
                                                                      tracker);
                            Debug.Assert(!string.IsNullOrEmpty(exportString));
                            sw.WriteLine(exportString);
                        }
                    }
                }

                _WriteUnassignedOrdersContent(data, schedule, fields, tracker, sw);
            }
        }

        /// <summary>
        /// Writes export data content.
        /// </summary>
        /// <param name="type">Table type.</param>
        /// <param name="schedules">Schedules to export.</param>
        /// <param name="fields">Exported fields.</param>
        /// <param name="tracker">Cancel tracker (can be null).</param>
        /// <param name="sw">Export writer.</param>
        private void _WriteContent(TableType type,
                                   ICollection<Schedule> schedules,
                                   ICollection<string> fields,
                                   ICancelTracker tracker,
                                   StreamWriter sw)
        {
            TableDescription tableDescription = _structureKeeper.GetTableDescription(type);
            var data = new DataKeeper(_listSeparator, tableDescription);

            switch (type)
            {
                case TableType.Routes:
                    _WriteRoutesContent(data, schedules, fields, tracker, sw);
                    break;

                case TableType.Stops:
                    _WriteStopsContent(data, schedules, fields, false, tracker, sw);
                    break;

                case TableType.Orders:
                    _WriteStopsContent(data, schedules, fields, true, tracker, sw);
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }
        }

        #endregion // Private helpers

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// String's format
        /// </summary>
        private const string STRING_FORMAT = "\"{0}\"";

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Data separator.
        /// </summary>
        private string _separator;
        /// <summary>
        /// Text list separator.
        /// </summary>
        private string _listSeparator;
        /// <summary>
        /// Export data structure.
        /// </summary>
        private IExportStructureKeeper _structureKeeper;

        #endregion // Private members
    }
}
