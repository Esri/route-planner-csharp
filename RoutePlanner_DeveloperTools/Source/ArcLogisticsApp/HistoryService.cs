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
using System.IO;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class that manages history items by categories.
    /// </summary>
    internal class HistoryService
    {
        #region public structures

        public struct HistoryItem
        {
            public string String { get; set; }
            public string Category { get; set; }
            public string Substring { get; set; }

            public override string ToString()
            {
                return String + ", " + Category;
            }
        }

        #endregion

        #region constructors 

        public HistoryService(string historyDBPath)
        {
            if (!File.Exists(historyDBPath))
            {
                string folderPath = Path.GetDirectoryName(historyDBPath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                _CreateDatabase(historyDBPath);
                _OpenDatabase(historyDBPath);
            }
            else
            {
                _OpenDatabase(historyDBPath);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Updates history item Last Modified Date
        /// </summary>
        /// <param name="item"></param>
        public void UpdateItem(HistoryItem item)
        {
            Debug.Assert(_IsInitialized());

            SqlCeDataReader reader = null;
            try
            {
                SqlCeCommand command = new SqlCeCommand("Select * From [History] Where [String]=@string AND [Category]=@category", _connection);
                command.Parameters.Add("@string", item.String);
                command.Parameters.Add("@category", item.Category);

                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    // need to update the record
                    command = new SqlCeCommand("Update [History] Set [LastModifiedDate] = @lastModifiedDate Where [String]=@string AND [Category]=@category", _connection);
                }
                else
                {
                    // need to insert new record
                    command = new SqlCeCommand("Insert into [History] ([String], [Category], [LastModifiedDate]) values (@string, @category, @lastModifiedDate)", _connection);
                }

                command.Parameters.Add("@string", item.String);
                command.Parameters.Add("@category", item.Category);
                command.Parameters.Add("@lastModifiedDate", DateTime.Now);

                int count = command.ExecuteNonQuery();
                Debug.Assert(count == 1);
            }
            catch
            {
                // exception can be thrown if some other instance of the application
                // added the item before this one. Silently ignore.
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }

        /// <summary>
        /// Searches history items by substring. 
        /// </summary>
        /// <param name="substring"></param>
        /// <param name="category"></param>
        /// <returns>Returns maximum 10 last used items or null.</returns>
        public HistoryItem[] SearchItems(string substring, string category)
        {
            Debug.Assert(_IsInitialized());

            List<HistoryItem> items = new List<HistoryItem>();

            SqlCeDataReader reader = null;
            try
            {
                SqlCeCommand command = new SqlCeCommand("Select TOP (10) * From [History] Where [String] Like @substring AND [Category]=@category Order by [LastModifiedDate] Desc", _connection);
                command.Parameters.Add("@substring", string.Format("%{0}%", substring));
                command.Parameters.Add("@category", category);

                reader = command.ExecuteReader();

                while (reader.Read())
                {
                    HistoryItem item = new HistoryItem();
                    item.String = (string)reader["String"];
                    item.Category = (string)reader["Category"];
                    item.Substring = substring;

                    items.Add(item);
                }
            }
            catch
            {
                // ignore exception
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            if (items.Count == 0)
                return null;
            else
                return items.ToArray();
        }

        #endregion

        #region private methods

        private void _OpenDatabase(string historyDBPath)
        {
            _connection = new SqlCeConnection(_BuildConnectionString(historyDBPath));
            _connection.Open();
        }

        private void _CreateDatabase(string historyDBPath)
        {
            bool dbCreated = false;
            try
            {
                string connectionString = _BuildConnectionString(historyDBPath);

                SqlCeEngine engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
                dbCreated = true;

                // get creation script
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream txtStream = assembly.GetManifestResourceStream("ESRI.ArcLogistics.App.Resources.Historydb_create.sql");
                StreamReader streamReader = new StreamReader(txtStream);
                string script = streamReader.ReadToEnd();

                _CreateDatabaseStructure(connectionString, script);
            }
            catch
            {
                if (dbCreated)
                    File.Delete(historyDBPath);

                throw;
            }
        }

        private string _BuildConnectionString(string dbPath)
        {
            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
            sb.DataSource = dbPath;

            return sb.ToString();
        }

        private void _CreateDatabaseStructure(string connectionString, string script)
        {
            SqlCeConnection conn = null;
            try
            {
                conn = new SqlCeConnection(connectionString);
                conn.Open();

                SqlCeCommand cmd = new SqlCeCommand();
                cmd.Connection = conn;

                string[] commands = Regex.Split(script, CMD_DELIMITER);

                SqlCeTransaction trans = conn.BeginTransaction();
                try
                {
                    foreach (string cmdStr in commands)
                    {
                        if (cmdStr.Trim().Length > 0)
                        {
                            cmd.CommandText = cmdStr;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private bool _IsInitialized()
        {
            return _connection != null;
        }

        #endregion

        #region private members

        private SqlCeConnection _connection;

        private const string CMD_DELIMITER = "\\s+GO\\s+";

        #endregion
    }
}
