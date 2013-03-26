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
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Data source opener class.
    /// </summary>
    internal sealed class DataSourceOpener
    {
        #region Public enum
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Source files filter type.
        /// </summary>
        public enum FilterType
        {
            /// <summary>
            /// Show all supported source files.
            /// </summary>
            AllSupported,
            /// <summary>
            /// Show all supported source files without shape-files.
            /// </summary>
            WithoutShape,
            /// <summary>
            /// Show only shape-files.
            /// </summary>
            OnlyShape
        }

        #endregion // Public enum

        #region Static public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Queries data source (used system dialog).
        /// </summary>
        /// <param name="fromFile">Data source is file flag.</param>
        /// <param name="owner">Dialog owner (can be null).</param>
        /// <param name="type">Data source files filter type.</param>
        /// <returns>TRUE only if real DataSource selected.
        /// FALSE - error detected or cancel selected.</returns>
        /// <remarks>Recall supported.</remarks>
        static public bool QueryDataSource(bool fromFile, Window owner, FilterType type)
        {
            if (fromFile)
            {   // User select file
                // show Open File Dialog (NOTE: WPF not supported WinForms)
                var ofd = new Microsoft.Win32.OpenFileDialog();
                ofd.RestoreDirectory = true;

                switch (type)
                {
                    case FilterType.AllSupported:
                        ofd.Filter = OPENFILE_DIALOG_FILTER_ALLTYPES;
                        ofd.FilterIndex = 8;
                        break;

                    case FilterType.WithoutShape:
                        ofd.Filter = OPENFILE_DIALOG_FILTER_WITHSHAPE;
                        ofd.FilterIndex = 7;
                        break;

                    case FilterType.OnlyShape:
                        ofd.Filter = OPENFILE_DIALOG_FILTER_ONLYSHAPE;
                        ofd.FilterIndex = 2;
                        break;

                    default:
                        Debug.Assert(false); // NOTE: not supported
                        break;
                }

                if (true != ofd.ShowDialog(owner)) // Result could be true, false, or null
                    return false;

                FilePath = ofd.FileName;
            }
            else
            {   // User select data source/server

                // Reference DataLinks
                // NOTE: Reference 
                //    COM.Microsoft OLE DB Service Component 1.0 Type Library
                //    (Was MSDASC.dll)
                // SEE:
                //    http://support.microsoft.com:80/support/kb/articles/Q225/1/32.asp

                // show Data Links Properties dialog
                var dataLinks = new MSDASC.DataLinks();
                WindowInteropHelper helper = new WindowInteropHelper(owner);
                HwndSource _hwndSource = HwndSource.FromHwnd(helper.Handle);
                dataLinks.hWnd = (int)_hwndSource.Handle;
                var connection = dataLinks.PromptNew() as ADODB.Connection;
                if (null == connection)
                    return false;

                ConnectionString = connection.ConnectionString;
            }

            return true;
        }

        /// <summary>
        /// Gets ConnectionString for user's DataSource.
        /// </summary>
        static public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (value == _connectionString)
                    return;

                _connectionString = value;
                _filePath = _GetFilePath(_connectionString);
            }
        }

        /// <summary>
        /// Gets file full name for user's DataSource
        /// </summary>
        static public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value == _filePath)
                    return;

                _filePath = value;
                _connectionString = _AssemblyConnectionString(_filePath);
            }
        }

        /// <summary>
        /// Gets table name list from Data Source.
        /// </summary>
        /// <returns>Table name list in data source (Read-only).</returns>
        /// <remarks>Used ConnectionString (call after initialization).</remarks>
        static public IList<string> GetTableNameList(out string messageFailure)
        {
            Debug.Assert(!string.IsNullOrEmpty(_connectionString)); // init first

            var listTableNames = new List<string>();
            if (_IsSourceSHPFile(_filePath))
            {   // source is shape-file
                listTableNames.Add(SHP_SURROGATE_TABLE_NAME);
                messageFailure = null;
            }
            else
            {   // read table list
                StringDictionary dic = _GetTableNameDictionary(out messageFailure);
                foreach (string name in dic.Keys)
                    listTableNames.Add(name);
            }

            return listTableNames.AsReadOnly();
        }

        /// <summary>
        /// Creates DataProvider to Data Source.
        /// </summary>
        /// <param name="tableName">Table name in source.</param>
        /// <param name="messageFailure">Description message failure
        /// (can be null if problem absent).</param>
        /// <remarks>Used ConnectionString, call after SelectDataSource()</remarks>
        static public IDataProvider Open(string tableName, out string messageFailure)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName));
            return (_IsSourceSHPFile(_filePath))?
                        _OpenSHP(out messageFailure) :
                        _OpenDB(tableName, out messageFailure);
        }

        /// <summary>
        /// Checks connetion.
        /// </summary>
        /// <param name="dataSourceLink">Data source link.</param>
        /// <param name="messageFailure">Description message failure
        /// (can be null if problem absent).</param>
        /// <returns>Connetion passible status.</returns>
        static public bool IsConnectionPossible(string dataSourceLink, out string messageFailure)
        {
            bool isValid = false;
            if (_IsSourceSHPFile(dataSourceLink))
            {
                isValid = File.Exists(_filePath);
                messageFailure = isValid? null : App.Current.FindString("ImportProfileNotFound");
            }
            else
            {
                DbConnection connection = DbFactory.CreateConnection(dataSourceLink, out messageFailure);
                if (null != connection)
                {
                    try
                    {
                        connection.Open();
                        isValid = true;
                    }
                    catch (Exception ex)
                    {
                        messageFailure = App.Current.GetString("FailedDatabaseReqData", ex.Message);
                        Logger.Error(ex);
                    }
                    finally
                    {   // explicitly close - don't wait on garbage collection.
                        connection.Close();
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Checks is connection string.
        /// </summary>
        /// <param name="dataSourceLink">Data source link.</param>
        /// <returns>TRUE if dataLinkSource is connection string.</returns>
        static public bool IsConnectionString(string dataSourceLink)
        {
            bool result = (!string.IsNullOrEmpty(dataSourceLink) &&
                           dataSourceLink.StartsWith(PROVIDER_INDICATION));
            return result;
        }

        /// <summary>
        /// Resets state.
        /// </summary>
        static public void Reset()
        {
            _filePath = null;
            _connectionString = null;
        }

        #endregion // Static public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets table face name by system name.
        /// </summary>
        /// <param name="systemName">Data source table name.</param>
        /// <returns>Face name.</returns>
        static private string _GetTableFaceName(string systemName)
        {
            string faceName = systemName;
            // remove liding symbols
            if (faceName.StartsWith(SYSTEM_SYMBOL_QUOTA))
                faceName = faceName.Remove(0, 1);
            // remove trailing symbols
            if (faceName.EndsWith(SYSTEM_SYMBOL_QUOTA))
                faceName = faceName.Remove(faceName.Length - 1);
            if (faceName.EndsWith(SYSTEM_SYMBOL_DOLLAR))
                faceName = faceName.Remove(faceName.Length - 1);

            return faceName;
        }

        /// <summary>
        /// Gets table name dictionary.
        /// </summary>
        /// <param name="messageFailure">Description message failure
        /// (can be null if problem absent).</param>
        /// <returns>Table name dictionary (system name by face name).</returns>
        static private StringDictionary _GetTableNameDictionary(out string messageFailure)
        {
            Debug.Assert(!_IsSourceSHPFile(_filePath));
            var listTableNames = new StringDictionary();

            DbConnection connection = DbFactory.CreateConnection(_connectionString, out messageFailure);
            if (null != connection)
            {
                try
                {
                    connection.Open();

                    // Retrieve schema information about tables.
                    // Because tables include tables, views, and other objects,
                    // restrict to just TABLE in the Object array of restrictions.
                    DataTable schemaTable = connection.GetSchema(SCHEMA_TABLES_DESCR_NAME);

                    // list the table name from each row in the schema table.
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        string type = row[SCHEMA_ROW_TABLE_DESCRIPTION_TYPE].ToString();
                        if (SCHEMA_NON_SYSTEM_TABLE_TYPE.Equals(type,
                                    StringComparison.OrdinalIgnoreCase))
                        {
                            string systemName = row[SCHEMA_ROW_TABLE_DESCRIPTION_NAME].ToString();
                            string faceName = _GetTableFaceName(systemName);
                            listTableNames.Add(faceName, systemName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    messageFailure = App.Current.GetString("FailedDatabaseReqData", ex.Message);
                    Logger.Error(ex);
                }
                finally
                {   // explicitly close - don't wait on garbage collection.
                    connection.Close();
                }
            }

            return listTableNames;
        }

        /// <summary>
        /// Gets formatted file name.
        /// </summary>
        /// <param name="fullFileName">File full name.</param>
        /// <returns>Formatted file name.</returns>
        static private string _GetFormattedFileName(string fullFileName)
        {   // NOTE: some of the Providers use file directory in place of file path
            // (see ConnectionString, Schema.ini)
            string formatName = fullFileName;
            string ext = Path.GetExtension(fullFileName);
            if (!string.IsNullOrEmpty(ext))
            {
                for (int i = 0; i < DATASOURCE_SPEC_ELEMENT_EXT.Length; ++i)
                {
                    if (DATASOURCE_SPEC_ELEMENT_EXT[i] == ext)
                    {
                        formatName = Path.GetDirectoryName(fullFileName);
                        break;
                    }
                }
            }

            return formatName;
        }

        /// <summary>
        /// Create connection string by file path.
        /// </summary>
        /// <param name="fileFullPath">File full path.</param>
        /// <returns>Connection string (can be null).</returns>
        static private string _AssemblyConnectionString(string fileFullPath)
        {
            if (string.IsNullOrEmpty(fileFullPath) || !File.Exists(fileFullPath))
                return null;

            string connectionString = fileFullPath;
            string ext = Path.GetExtension(fileFullPath);
            if (!string.IsNullOrEmpty(ext))
            {
                for (int i = 0; i < PROVIDER_CONNECTION_STRING.Length; ++i)
                {
                    if (PROVIDER_CONNECTION_STRING[i].Contains(ext))
                    {
                        connectionString =
                            PROVIDER_CONNECTION_STRING[i].Replace(ext,
                                                                  _GetFormattedFileName(_filePath));
                        break;
                    }
                }
            }

            return connectionString;
        }

        /// <summary>
        /// Gets file path from connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to preparation.</param>
        /// <returns>File path (can be null).</returns>
        static private string _GetFilePath(string connectionString)
        {
            // NOTE: multy return function

            if (string.IsNullOrEmpty(connectionString))
                return null;

            // check: is real connection string
            if (!IsConnectionString(connectionString))
                return connectionString;

            string startSource = (connectionString.Contains(DATA_SOURCE))?
                                    DATA_SOURCE : DATA_SOURCE_DSN;

            int start = connectionString.IndexOf(startSource) + startSource.Length;
            int end = connectionString.IndexOf(DATA_SOURCE_END, start);

            if ((start < 0) || (end < 0) || (connectionString.Length < end))
                return connectionString;

            // substring path
            return connectionString.Substring(start, end - start);
        }

        /// <summary>
        /// Checks is data source is shape-file.
        /// </summary>
        /// <param name="dataSourceLink">Data source link.</param>
        /// <returns>TRUE if data source link select shape-file.</returns>
        private static bool _IsSourceSHPFile(string dataSourceLink)
        {
            return FileHelpers.IsShapeFile(dataSourceLink);
        }

        /// <summary>
        /// Opens shape-file.
        /// </summary>
        /// <param name="messageFailure">Description message failure
        /// (can be null if problem absent).</param>
        /// <returns>Data provider.</returns>
        static private IDataProvider _OpenSHP(out string messageFailure)
        {
            SHPProvider provider = new SHPProvider();
            bool successed = provider.Init(_filePath, out messageFailure);
            return successed? provider : null;
        }

        /// <summary>
        /// Opens Database.
        /// </summary>
        /// <param name="tableName">Slected table name in database.</param>
        /// <param name="messageFailure">Description message failure
        /// (can be null if problem absent).</param>
        /// <returns>Data provider (can be null).</returns>
        static private IDataProvider _OpenDB(string tableName, out string messageFailure)
        {
            Debug.Assert(!string.IsNullOrEmpty(tableName));

            // try convert table name to system table name
            string tableSystemName = tableName;
            StringDictionary tables = _GetTableNameDictionary(out messageFailure);
            if (tables.ContainsKey(tableName))
                tableSystemName = tables[tableName];

            DbConnection connection = DbFactory.CreateConnection(_connectionString, out messageFailure);
            if (null == connection)
                return null;

            // create the dataset and add select Table to it:
            bool bOK = false;
            var dataSet = new DataSet();
            try
            {
                connection.Open();
                using (var accessCommand = connection.CreateCommand())
                {
                    accessCommand.CommandText = tableSystemName;
                    accessCommand.CommandType = CommandType.TableDirect;

                    using (var dataAdapter = DbFactory.CreateDataAdapter(connection))
                    {
                        dataAdapter.SelectCommand = accessCommand;
                        dataAdapter.Fill(dataSet, tableName);
                        bOK = true;
                    }
                }
            }
            catch (Exception ex)
            {
                messageFailure = App.Current.GetString("FailedDatabaseReqData", ex.Message);
                Logger.Error(ex);
            }
            finally
            {   // explicitly close - don't wait on garbage collection.
                connection.Close();
            }
            if (!bOK)
                return null; // NOTE: proplem detected

            Debug.Assert(1 == dataSet.Tables.Count);
            // NOTE: do not supported dataset with multiple tables (relative base). Used only one.

            return new DBProvider(dataSet.Tables[tableName]);
        }

        #endregion // Private methods

        #region Private constans
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static private readonly string[] PROVIDER_CONNECTION_STRING = new string[]
        {
            @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=.mdb;User Id=admin;Password=;",
            @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=.dbf;Extended Properties=dBASE IV;User ID=Admin;Password=;",
            @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=.txt;Extended Properties=""text;HDR=Yes;FMT=Delimited"";",
            @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=.csv;Extended Properties=""text;HDR=Yes;FMT=Delimited"";",
            @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=.xls;Extended Properties=""Excel 8.0;HDR=Yes;IMEX=1"";",
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=.xlsx;Extended Properties=""Excel 12.0 Xml;HDR=YES"";",
            @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=.accdb;Persist Security Info=False;"
        };

        static private readonly string[] DATASOURCE_SPEC_ELEMENT_EXT = new string[]
        {
            ".txt",
            ".csv",
            ".dbf"
        };

        private const string OPENFILE_DIALOG_FILES_SEPARATOR = "|";
        private const string OPENFILE_DIALOG_FILES_ACCESS = "Access (*.mdb)|*.mdb";
        private const string OPENFILE_DIALOG_FILES_ACCESS2007 = "Access 2007 (*.accdb)|*.accdb";
        private const string OPENFILE_DIALOG_FILES_FOXPRO = "FoxPro (*.dbf)|*.dbf";
        private const string OPENFILE_DIALOG_FILES_TEXTFILE = "Text file (*.txt, *.csv)|*.txt;*.csv";
        private const string OPENFILE_DIALOG_FILES_EXCEL = "Excel (*.xls)|*.xls";
        private const string OPENFILE_DIALOG_FILES_EXCEL2007 = "Excel 2007 (*.xlsx)|*.xlsx";
        private const string OPENFILE_DIALOG_FILES_SHAPE = "Shape files (*.shp)|*.shp";

        private const string OPENFILE_DIALOG_FILTER_ALLTYPES =
            OPENFILE_DIALOG_FILES_ACCESS +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_ACCESS2007 +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_FOXPRO +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_TEXTFILE +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_EXCEL +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_EXCEL2007 +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_SHAPE +
            OPENFILE_DIALOG_FILES_SEPARATOR +
            "All Supported Files|*.dbf;*.txt;*.csv;*.xls;*.xlsx;*.mdb;*.accdb;*.shp";
        private const string OPENFILE_DIALOG_FILTER_WITHSHAPE =
            OPENFILE_DIALOG_FILES_ACCESS +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_ACCESS2007 +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_FOXPRO +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_TEXTFILE +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_EXCEL +
            OPENFILE_DIALOG_FILES_SEPARATOR + OPENFILE_DIALOG_FILES_EXCEL2007 +
            OPENFILE_DIALOG_FILES_SEPARATOR +
            "All Supported Files|*.dbf;*.txt;*.csv;*.xls;*.xlsx;*.mdb;*.accdb";
        private const string OPENFILE_DIALOG_FILTER_ONLYSHAPE =
            OPENFILE_DIALOG_FILES_SHAPE +
            OPENFILE_DIALOG_FILES_SEPARATOR +
            "All Supported Files|*.shp";
        private const string PROVIDER_INDICATION = "Provider=";
        private const string DATA_SOURCE = ";Data Source=";
        private const string DATA_SOURCE_DSN = ";DBQ=";
        private const char DATA_SOURCE_END = ';';

        private const string SCHEMA_TABLES_DESCR_NAME = "Tables";
        private const string SCHEMA_ROW_TABLE_DESCRIPTION_NAME = "TABLE_NAME";
        private const string SCHEMA_ROW_TABLE_DESCRIPTION_TYPE = "TABLE_TYPE";
        private const string SCHEMA_NON_SYSTEM_TABLE_TYPE = "TABLE";
        private const string SHP_SURROGATE_TABLE_NAME = "Table";
        private const string SYSTEM_SYMBOL_QUOTA = "'";
        private const string SYSTEM_SYMBOL_DOLLAR = "$";

        #endregion // Private constans

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Data source file path.
        /// </summary>
        static private string _filePath;
        /// <summary>
        /// Data source connection string.
        /// </summary>
        static private string _connectionString;

        #endregion // Private members
    }
}
