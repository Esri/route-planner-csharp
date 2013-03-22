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
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Geocode
{
    /// <summary>
    /// Class that manages name\address geocoded pairs.
    /// </summary>
    internal class NameAddressStorage
    {
        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="nameAddressDBPath">Path of DB file for local geocoder.</param>
        public NameAddressStorage(string nameAddressDBPath)
        {
            Debug.Assert(nameAddressDBPath != null);

            // Check DB file exists and create if not.
            if (!File.Exists(nameAddressDBPath))
            {
                string folderPath = Path.GetDirectoryName(nameAddressDBPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                _CreateDatabase(nameAddressDBPath);
                _OpenDatabase(nameAddressDBPath);
            }
            else
            {
                // Check if need to update database: add FullAddress,
                // MFullAddress, Unit, MUnit fields.
                if (_IsNeedToUpdateDatabase(nameAddressDBPath))
                    _UpdateDatabase(nameAddressDBPath);
                // If exists - open it.
                _OpenDatabase(nameAddressDBPath);
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Updates name/address pair.
        /// </summary>
        /// <param name="localGeocoderRecord">Local geocoder record.</param>
        /// <param name="format">Current Address Format.</param>
        public void InsertOrUpdate(NameAddressRecord localGeocoderRecord, AddressFormat format)
        {
            Debug.Assert(localGeocoderRecord != null);
            Debug.Assert(_IsInitialized());

            try
            {
                NameAddressRecord recordFromDB = _GetRecordFromDB(
                    localGeocoderRecord.NameAddress, format);

                string queryStr = string.Empty;
                if (recordFromDB != null)
                {
                    // Need to update record.
                    // Choose queries in dependence of used address format.
                    if (format == AddressFormat.SingleField)
                        queryStr = FULL_ADDRESS_UPDATE_QUERY;
                    else if (format == AddressFormat.MultipleFields)
                        queryStr = UPDATE_QUERY;
                    else
                    {
                        // Do nothing.
                    }
                }
                else
                {
                    // Need to insert record.
                    queryStr = INSERT_QUERY;
                }

                SqlCeCommand command = new SqlCeCommand(queryStr, _connection);
                _FillRecordCommandParameters(localGeocoderRecord, command);

                int count = command.ExecuteNonQuery();
                Debug.Assert(count == 1);
            }
            catch (Exception ex)
            {
                // Exception can be thrown if some other instance of the
                // application added the item before this one.
                Logger.Warning(ex);
            }
        }

        /// <summary>
        /// Searches in local storage by name/address pair.
        /// </summary>
        /// <param name="nameAddress">Name/address pair to search.</param>
        /// <param name="format">Current Address Format.</param>
        /// <returns>Name address record.</returns>
        public NameAddressRecord Search(NameAddress nameAddress, AddressFormat format)
        {
            Debug.Assert(nameAddress != null);
            Debug.Assert(_IsInitialized());

            NameAddressRecord recordFromDB = null;

            try
            {
                recordFromDB = _GetRecordFromDB(nameAddress, format);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex);
            }

            return recordFromDB;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Create DB file.
        /// </summary>
        /// <param name="nameAddressDBPath">Path of DB file for local geocoder.</param>
        private void _CreateDatabase(string nameAddressDBPath)
        {
            Debug.Assert(nameAddressDBPath != null);

            bool dbCreated = false;
            try
            {
                string connectionString = _BuildConnectionString(nameAddressDBPath);

                SqlCeEngine engine = new SqlCeEngine(connectionString);
                engine.CreateDatabase();
                dbCreated = true;

                // Get creation script.
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream scriptStream = assembly.GetManifestResourceStream(CREATE_SQL_SCRIPT_PATH);
                StreamReader streamReader = new StreamReader(scriptStream);
                string script = streamReader.ReadToEnd();

                _CreateDatabaseStructure(connectionString, script);
            }
            catch
            {
                if (dbCreated)
                {
                    File.Delete(nameAddressDBPath);
                }

                throw;
            }
        }

        /// <summary>
        /// Build connection string.
        /// </summary>
        /// <param name="dbPath">Path to DB file.</param>
        /// <returns>Connection string.</returns>
        private string _BuildConnectionString(string dbPath)
        {
            Debug.Assert(dbPath != null);

            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
            sb.DataSource = dbPath;

            return sb.ToString();
        }

        /// <summary>
        /// Create database structure.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="script">Script string.</param>
        private void _CreateDatabaseStructure(string connectionString, string script)
        {
            Debug.Assert(connectionString != null);
            Debug.Assert(script != null);

            SqlCeConnection conn = null;

            try
            {
                conn = new SqlCeConnection(connectionString);
                conn.Open();

                SqlCeCommand sqlCmd = new SqlCeCommand();
                sqlCmd.Connection = conn;

                _ExecuteScript(sqlCmd, script);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Execute database structure creating script.
        /// </summary>
        /// <param name="sqlCmd">SQL command.</param>
        /// <param name="script">Script text.</param>
        private void _ExecuteScript(SqlCeCommand sqlCmd, string script)
        {
            Debug.Assert(sqlCmd != null);
            Debug.Assert(script != null);

            SqlCeConnection conn = sqlCmd.Connection;

            string[] commands = Regex.Split(script, CMD_DELIMITER);

            SqlCeTransaction trans = conn.BeginTransaction();
            try
            {
                foreach (string cmdStr in commands)
                {
                    // If command exists - execute it.
                    if (cmdStr.Trim().Length > 0)
                    {
                        sqlCmd.CommandText = cmdStr;
                        sqlCmd.ExecuteNonQuery();
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

        /// <summary>
        /// Is connection to DB initialized.
        /// </summary>
        /// <returns>Is connection to DB initialized.</returns>
        private bool _IsInitialized()
        {
            return _connection != null;
        }

        /// <summary>
        /// Open DB.
        /// </summary>
        /// <param name="nameAddressDBPath">Path to DB file.</param>
        private void _OpenDatabase(string nameAddressDBPath)
        {
            string connectionString = _BuildConnectionString(nameAddressDBPath);
            _connection = new SqlCeConnection(connectionString);
            _connection.Open();
        }

        /// <summary>
        /// Determine is need to update DataBase: if FullAddress, MFullAddress, Unit and
        /// MUnit fields are not yet added.
        /// </summary>
        /// <param name="nameAddressDBPath">Path to DB file.</param>
        private bool _IsNeedToUpdateDatabase(string nameAddressDBPath)
        {
            Debug.Assert(nameAddressDBPath != null);

            string connectionString = _BuildConnectionString(nameAddressDBPath);
            bool result = true;

            // Make connection to DataBase.
            using (SqlCeConnection conn = new SqlCeConnection(connectionString))
            {
                conn.Open();

                SqlCeCommand command = new SqlCeCommand(SELECT_COLUMNS_QUERY, conn);
                List<string> columns = new List<string>();

                using (SqlCeDataReader reader = command.ExecuteReader())
                {
                    // Get list of table columns from DataBase.
                    while (reader.Read())
                    {
                        columns.Add((string)reader[0]);
                    }

                    // Try to find appropriate columns.
                    if (columns.IndexOf(FULL_ADDRESS_FIELD_NAME) > -1 &&
                        columns.IndexOf(MFULL_ADDRESS_FIELD_NAME) > -1 &&
                        columns.IndexOf(UNIT_FIELD_NAME) > -1 &&
                        columns.IndexOf(MUNIT_FIELD_NAME) > -1)
                        // If columns found, then don't need to update DataBase.
                        result = false;
                    else
                    {
                        // Do nothing: need to update DataBase.
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Update DataBase:add FullAddress, MFullAddress, Unit, MUnit fields.
        /// </summary>
        /// <param name="nameAddressDBPath">Path to DB file.</param>
        private void _UpdateDatabase(string nameAddressDBPath)
        {
            Debug.Assert(nameAddressDBPath != null);

            string connectionString = _BuildConnectionString(nameAddressDBPath);
            SqlCeConnection conn = null;
            try
            {
                // Make connection to DB.
                conn = new SqlCeConnection(connectionString);
                conn.Open();

                // Get script for update.
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream scriptStream = assembly.GetManifestResourceStream(UPDATE_SQL_SCRIPT_PATH);
                StreamReader streamReader = new StreamReader(scriptStream);
                string script = streamReader.ReadToEnd();

                // Update DB.
                SqlCeCommand sqlCmd = new SqlCeCommand();
                sqlCmd.Connection = conn;
                _ExecuteScript(sqlCmd, script);
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Get record from database.
        /// </summary>
        /// <param name="nameAddress">Name\address pair to find.</param>
        /// <param name="format">Current Address Format.</param>
        /// <returns>Extracted record.</returns>
        private NameAddressRecord _GetRecordFromDB(NameAddress nameAddress, AddressFormat format)
        {
            NameAddressRecord nameAddressRecord = null;

            string queryStr = string.Empty;

            // Choose queries in dependence of used address format.
            if (format == AddressFormat.SingleField)
                queryStr = FULL_ADDRESS_SELECT_QUERY;
            else if (format == AddressFormat.MultipleFields)
                queryStr = SELECT_QUERY;
            else
            {
                // Do nothing.
            }

            SqlCeCommand command = new SqlCeCommand(queryStr, _connection);
            _FillCommandParameters(nameAddress, command);

            SqlCeDataReader reader = null;
            try
            {
                reader = command.ExecuteReader();

                if (reader.Read())
                {
                    nameAddressRecord = _ReadRecord(reader);
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return nameAddressRecord;
        }


        /// <summary>
        /// Fill parameters of SQL command from local geocoder record.
        /// </summary>
        /// <param name="localGeocoderRecord">Source record.</param>
        /// <param name="command">Command to fill.</param>
        private void _FillRecordCommandParameters(NameAddressRecord localGeocoderRecord,
            SqlCeCommand command)
        {
            _FillCommandParameters(localGeocoderRecord.NameAddress, command);

            _AddParameter(command, X_PARAMETER_NAME, localGeocoderRecord.GeoLocation.X);
            _AddParameter(command, Y_PARAMETER_NAME, localGeocoderRecord.GeoLocation.Y);

            Address matchedAddress = localGeocoderRecord.MatchedAddress;
            _AddParameter(command, MUNIT_PARAMETER_NAME, matchedAddress.Unit);
            _AddParameter(command, MADDRESSLINE_PARAMETER_NAME, matchedAddress.AddressLine);
            _AddParameter(command, MLOCALITY1_PARAMETER_NAME, matchedAddress.Locality1);
            _AddParameter(command, MLOCALITY2_PARAMETER_NAME, matchedAddress.Locality2);
            _AddParameter(command, MLOCALITY3_PARAMETER_NAME, matchedAddress.Locality3);
            _AddParameter(command, MCOUNTYPREFECTURE_PARAMETER_NAME, matchedAddress.CountyPrefecture);
            _AddParameter(command, MPOSTALCODE1_PARAMETER_NAME, matchedAddress.PostalCode1);
            _AddParameter(command, MPOSTALCODE2_PARAMETER_NAME, matchedAddress.PostalCode2);
            _AddParameter(command, MSTATEPROVINCE_PARAMETER_NAME, matchedAddress.StateProvince);
            _AddParameter(command, MCOUNTRY_PARAMETER_NAME, matchedAddress.Country);
            _AddParameter(command, MFULL_ADDRESS_PARAMETER_NAME, matchedAddress.FullAddress);

            string matchMethod = localGeocoderRecord.MatchedAddress.MatchMethod;
            if (string.IsNullOrEmpty(matchMethod))
            {
                matchMethod = localGeocoderRecord.NameAddress.Address.MatchMethod;
            }

            _AddParameter(command, MATCHMETHOD_PARAMETER_NAME, matchMethod);
        }

        /// <summary>
        /// Fill parameters of SQL command from name\address pair.
        /// </summary>
        /// <param name="nameAddress">Source name\address record.</param>
        /// <param name="command">Command to fill.</param>
        private void _FillCommandParameters(NameAddress nameAddress, SqlCeCommand command)
        {
            _AddParameter(command, NAME_PARAMETER_NAME, nameAddress.Name);

            Address address = nameAddress.Address;

            _AddParameter(command, UNIT_PARAMETER_NAME, address.Unit);
            _AddParameter(command, ADDRESSLINE_PARAMETER_NAME, address.AddressLine);
            _AddParameter(command, LOCALITY1_PARAMETER_NAME, address.Locality1);
            _AddParameter(command, LOCALITY2_PARAMETER_NAME, address.Locality2);
            _AddParameter(command, LOCALITY3_PARAMETER_NAME, address.Locality3);
            _AddParameter(command, COUNTYPREFECTURE_PARAMETER_NAME, address.CountyPrefecture);
            _AddParameter(command, POSTALCODE1_PARAMETER_NAME, address.PostalCode1);
            _AddParameter(command, POSTALCODE2_PARAMETER_NAME, address.PostalCode2);
            _AddParameter(command, STATEPROVINCE_PARAMETER_NAME, address.StateProvince);
            _AddParameter(command, COUNTRY_PARAMETER_NAME, address.Country);
            _AddParameter(command, FULL_ADDRESS_PARAMETER_NAME, address.FullAddress);
        }

        /// <summary>
        /// Add paramater to command with replacing null values by empty strings.
        /// </summary>
        /// <param name="command">Command to add parameters.</param>
        /// <param name="name">Paramater name.</param>
        /// <param name="value">Parameter value.</param>
        private void _AddParameter(SqlCeCommand command, string name, object value)
        {
            object notNullValue = value;

            if (notNullValue == null)
            {
                notNullValue = string.Empty;
            }

            command.Parameters.Add(name, notNullValue);
        }

        /// <summary>
        /// Read record from database.
        /// </summary>
        /// <param name="reader">Database reader.</param>
        /// <returns>Readed record.</returns>
        private NameAddressRecord _ReadRecord(SqlCeDataReader reader)
        {
            NameAddressRecord nameAddressRecord = new NameAddressRecord();

            NameAddress nameAddress = new NameAddress();
            nameAddressRecord.NameAddress = nameAddress;

            nameAddress.Name = (string)reader[NAME_FIELD_NAME];

            Address address = new Address();
            nameAddress.Address = address;
            address.Unit = (string)reader[UNIT_FIELD_NAME];
            address.AddressLine = (string)reader[ADDRESSLINE_FIELD_NAME];
            address.Locality1 = (string)reader[LOCALITY1_FIELD_NAME];
            address.Locality2 = (string)reader[LOCALITY2_FIELD_NAME];
            address.Locality3 = (string)reader[LOCALITY3_FIELD_NAME];
            address.CountyPrefecture = (string)reader[COUNTYPREFECTURE_FIELD_NAME];
            address.PostalCode1 = (string)reader[POSTALCODE1_FIELD_NAME];
            address.PostalCode2 = (string)reader[POSTALCODE2_FIELD_NAME];
            address.StateProvince = (string)reader[STATEPROVINCE_FIELD_NAME];
            address.Country = (string)reader[COUNTRY_FIELD_NAME];
            address.FullAddress = (string)reader[FULL_ADDRESS_FIELD_NAME];
            float x = (float)reader[X_FIELD_NAME];
            float y = (float)reader[Y_FIELD_NAME];
            nameAddressRecord.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(x, y);

            Address matchedAddress = new Address();
            nameAddressRecord.MatchedAddress = matchedAddress;
            matchedAddress.Unit = (string)reader[MUNIT_FIELD_NAME];
            matchedAddress.AddressLine = (string)reader[MADDRESSLINE_FIELD_NAME];
            matchedAddress.Locality1 = (string)reader[MLOCALITY1_FIELD_NAME];
            matchedAddress.Locality2 = (string)reader[MLOCALITY2_FIELD_NAME];
            matchedAddress.Locality3 = (string)reader[MLOCALITY3_FIELD_NAME];
            matchedAddress.CountyPrefecture = (string)reader[MCOUNTYPREFECTURE_FIELD_NAME];
            matchedAddress.PostalCode1 = (string)reader[MPOSTALCODE1_FIELD_NAME];
            matchedAddress.PostalCode2 = (string)reader[MPOSTALCODE2_FIELD_NAME];
            matchedAddress.StateProvince = (string)reader[MSTATEPROVINCE_FIELD_NAME];
            matchedAddress.Country = (string)reader[MCOUNTRY_FIELD_NAME];
            matchedAddress.FullAddress = (string)reader[MFULL_ADDRESS_FIELD_NAME];

            string matchMethod = (string)reader[MATCHMETHOD_FIELD_NAME];
            if (CommonHelpers.IsAllAddressFieldsEmpty(matchedAddress) && string.IsNullOrEmpty(matchedAddress.MatchMethod))
            {
                address.MatchMethod = matchMethod;
            }
            else
            {
                matchedAddress.MatchMethod = matchMethod;
            }

            return nameAddressRecord;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Path to sql script for creation database.
        /// </summary>
        private const string CREATE_SQL_SCRIPT_PATH = "ESRI.ArcLogistics.App.Resources.AddressNamedb_create.sql";

        /// <summary>
        /// Path to sql script for database update.
        /// </summary>
        private const string UPDATE_SQL_SCRIPT_PATH = "ESRI.ArcLogistics.App.Resources.AddressNamedb_update.sql";

        /// <summary>
        /// Query for selecting columns.
        /// </summary>
        private const string SELECT_COLUMNS_QUERY = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.Columns";

        /// <summary>
        /// Select query for Multiple Fields Address Format.
        /// </summary>
        private const string SELECT_QUERY = "Select * From [AddressName] " +
            "Where [Name]=@name AND [Unit]=@unit AND [AddressLine]=@addressLine AND [Locality1]=@locality1 AND " +
            "[Locality2]=@locality2 AND [Locality3]=@locality3 AND [CountyPrefecture]=@countyPrefecture AND " +
            "[PostalCode1]=@postalCode1 AND [PostalCode2]=@postalCode2 AND [StateProvince]=@stateProvince AND " +
            "[Country]=@country";

        /// <summary>
        /// Select query for Single Field Address Format.
        /// </summary>
        private const string FULL_ADDRESS_SELECT_QUERY = "Select * From [AddressName] " +
            "Where [Name]=@name AND [FullAddress]=@fullAddress";

        /// <summary>
        /// Update query for Multiple Fields Address Format.
        /// </summary>
        private const string UPDATE_QUERY = "Update [AddressName] " +
            "Set [X]=@x, [Y]=@y, [MUnit]=@munit, [MAddressLine]=@maddressLine, [MLocality1]=@mlocality1, " +
            "[MLocality2]=@mlocality2, [MLocality3]=@mlocality3, [MCountyPrefecture]=@mcountyPrefecture, " +
            "[MPostalCode1]=@mpostalCode1, [MPostalCode2]=@mpostalCode2, [MStateProvince]=@mstateProvince, " +
            "[MCountry]=@mcountry , [MatchMethod]=@matchMethod " +
            "Where [Name]=@name AND [Unit]=@unit AND [AddressLine]=@addressLine AND [Locality1]=@locality1 AND " +
            "[Locality2]=@locality2 AND [Locality3]=@locality3 AND [CountyPrefecture]=@countyPrefecture AND " +
            "[PostalCode1]=@postalCode1 AND [PostalCode2]=@postalCode2 AND [StateProvince]=@stateProvince AND " +
            "[Country]=@country";

        /// <summary>
        /// Update query for Single Field Address Format.
        /// </summary>
        private const string FULL_ADDRESS_UPDATE_QUERY = "Update [AddressName] " +
            "Set [X]=@x, [Y]=@y, [MFullAddress]=@mfullAddress, [MatchMethod]=@matchMethod " +
            "Where [Name]=@name AND [FullAddress]=@fullAddress";

        /// <summary>
        /// Insert query.
        /// </summary>
        private const string INSERT_QUERY = "Insert into [AddressName] " +
            "([Name], [Unit], [AddressLine], [Locality1], [Locality2], [Locality3], [CountyPrefecture], " +
            "[PostalCode1], [PostalCode2], [StateProvince], [Country], [FullAddress], [X], [Y]," +
            "[MUnit], [MAddressLine], [MLocality1], [MLocality2], [MLocality3], [MCountyPrefecture]," +
            "[MPostalCode1], [MPostalCode2], [MStateProvince], [MCountry], [MFullAddress], [MatchMethod]" +
            ") values (" +
            "@name, @unit, @addressLine, @locality1, @locality2, @locality3, @countyPrefecture, " +
            "@postalCode1, @postalCode2, @stateProvince, @country, @fullAddress, " +
            "@x, @y, @munit, @maddressLine, @mlocality1, @mlocality2, @mlocality3, @mcountyPrefecture, " +
            "@mpostalCode1, @mpostalCode2, @mstateProvince, @mcountry, @mfullAddress, @matchMethod" +
            ")";

        /// <summary>
        /// Field names.
        /// </summary>
        private const string NAME_FIELD_NAME = "Name";

        private const string UNIT_FIELD_NAME = "Unit";
        private const string ADDRESSLINE_FIELD_NAME = "AddressLine";
        private const string LOCALITY1_FIELD_NAME = "Locality1";
        private const string LOCALITY2_FIELD_NAME = "Locality2";
        private const string LOCALITY3_FIELD_NAME = "Locality3";
        private const string COUNTYPREFECTURE_FIELD_NAME = "CountyPrefecture";
        private const string POSTALCODE1_FIELD_NAME = "PostalCode1";
        private const string POSTALCODE2_FIELD_NAME = "PostalCode2";
        private const string STATEPROVINCE_FIELD_NAME = "StateProvince";
        private const string COUNTRY_FIELD_NAME = "Country";
        private const string FULL_ADDRESS_FIELD_NAME = "FullAddress";

        private const string X_FIELD_NAME = "X";
        private const string Y_FIELD_NAME = "Y";

        private const string MUNIT_FIELD_NAME = "MUnit";
        private const string MADDRESSLINE_FIELD_NAME = "MAddressLine";
        private const string MLOCALITY1_FIELD_NAME = "MLocality1";
        private const string MLOCALITY2_FIELD_NAME = "MLocality2";
        private const string MLOCALITY3_FIELD_NAME = "MLocality3";
        private const string MCOUNTYPREFECTURE_FIELD_NAME = "MCountyPrefecture";
        private const string MPOSTALCODE1_FIELD_NAME = "MPostalCode1";
        private const string MPOSTALCODE2_FIELD_NAME = "MPostalCode2";
        private const string MSTATEPROVINCE_FIELD_NAME = "MStateProvince";
        private const string MCOUNTRY_FIELD_NAME = "MCountry";
        private const string MFULL_ADDRESS_FIELD_NAME = "MFullAddress";

        private const string MATCHMETHOD_FIELD_NAME = "matchMethod";

        /// <summary>
        /// Parameter names.
        /// </summary>
        private const string NAME_PARAMETER_NAME = "@name";

        private const string UNIT_PARAMETER_NAME = "@unit";
        private const string ADDRESSLINE_PARAMETER_NAME = "@addressLine";
        private const string LOCALITY1_PARAMETER_NAME = "@locality1";
        private const string LOCALITY2_PARAMETER_NAME = "@locality2";
        private const string LOCALITY3_PARAMETER_NAME = "@locality3";
        private const string COUNTYPREFECTURE_PARAMETER_NAME = "@countyPrefecture";
        private const string POSTALCODE1_PARAMETER_NAME = "@postalCode1";
        private const string POSTALCODE2_PARAMETER_NAME = "@postalCode2";
        private const string STATEPROVINCE_PARAMETER_NAME = "@stateProvince";
        private const string COUNTRY_PARAMETER_NAME = "@country";
        private const string FULL_ADDRESS_PARAMETER_NAME = "@fullAddress";

        private const string X_PARAMETER_NAME = "@x";
        private const string Y_PARAMETER_NAME = "@y";

        private const string MUNIT_PARAMETER_NAME = "@munit";
        private const string MADDRESSLINE_PARAMETER_NAME = "@maddressLine";
        private const string MLOCALITY1_PARAMETER_NAME = "@mlocality1";
        private const string MLOCALITY2_PARAMETER_NAME = "@mlocality2";
        private const string MLOCALITY3_PARAMETER_NAME = "@mlocality3";
        private const string MCOUNTYPREFECTURE_PARAMETER_NAME = "@mcountyPrefecture";
        private const string MPOSTALCODE1_PARAMETER_NAME = "@mpostalCode1";
        private const string MPOSTALCODE2_PARAMETER_NAME = "@mpostalCode2";
        private const string MSTATEPROVINCE_PARAMETER_NAME = "@mstateProvince";
        private const string MCOUNTRY_PARAMETER_NAME = "@mcountry";
        private const string MFULL_ADDRESS_PARAMETER_NAME = "@mfullAddress";

        private const string MATCHMETHOD_PARAMETER_NAME = "@matchMethod";

        /// <summary>
        /// Delimeter for parsing SQL script.
        /// </summary>
        private const string CMD_DELIMITER = "\\s+GO\\s+";

        #endregion

        #region Private fields

        /// <summary>
        /// Connection to database.
        /// </summary>
        private SqlCeConnection _connection;

        #endregion
    }
}