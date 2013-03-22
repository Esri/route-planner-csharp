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
using System.Reflection;
using System.Diagnostics;
using System.Data;
using System.Data.SqlServerCe;
using System.Data.EntityClient;
using System.Text.RegularExpressions;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DatabaseOpener class.
    /// </summary>
    internal class DatabaseOpener
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // TODO: (?) store connection string or parts of the string in app.config

        // database provider name
        private const string DB_PROVIDER = "System.Data.SqlServerCe.3.5";

        // database metadata resources
        private const string DB_METADATA =
            @"res://*/ESRI.ArcLogistics.Data.DataModel.DataModel.csdl|" +
            "res://*/ESRI.ArcLogistics.Data.DataModel.DataModel.ssdl|" +
            "res://*/ESRI.ArcLogistics.Data.DataModel.DataModel.msl";

        // SQL Server CE error codes
        private const int SSCE_M_FILESHAREVIOLATION = 25035;

        #endregion constants

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Opens SQL Server CE database using specified file path.
        /// Upgrades database if scheme version less than the current one.
        /// </summary>
        /// <param name="path">
        /// Database file path.
        /// </param>
        /// <returns>
        /// DataObjectContext object.
        /// </returns>
        public static DataObjectContext OpenDatabase(string path)
        {
            Debug.Assert(path != null);

            try
            {
                string sqlConnStr = DatabaseHelper.BuildSqlConnString(path, true);

                // upgrade database if necessary
                CheckForDbUpgrade(sqlConnStr);

                // create object context
                DataObjectContext ctx = new DataObjectContext(
                    _BuildEntityConnStr(sqlConnStr));

                // keep connection alive to ensure exclusive mode
                ctx.Connection.Open();

                return ctx;
            }
            catch (Exception e)
            {
                Logger.Error(e);

                SqlCeException ceEx = _GetCEException(e);
                if (ceEx != null &&
                    ceEx.NativeError == SSCE_M_FILESHAREVIOLATION)
                {
                    throw new DataException(Properties.Messages.Error_DbFileSharingViolation,
                        DataError.FileSharingViolation);
                }
                else
                    throw;
            }
        }

        public static void CheckForDbUpgrade(string sqlConnStr)
        {
            Debug.Assert(sqlConnStr != null);

            // get project version
            double projectVer = _GetProjectVersion(sqlConnStr);
            if (projectVer > SchemeVersion.CurrentVersion)
            {
                // unsupported scheme version
                throw new DataException(
                    Properties.Messages.Error_UnsupportedSchemeVersion,
                    DataError.NotSupportedFileVersion);
            }
            else if (projectVer < SchemeVersion.CurrentVersion)
            {
                // early version, trying to upgrade database
                _UpgradeDB(sqlConnStr, projectVer);
            }
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static double _GetProjectVersion(string sqlConnStr)
        {
            Debug.Assert(sqlConnStr != null);

            using (SqlCeConnection conn = new SqlCeConnection(sqlConnStr))
            {
                conn.Open();

                using (SqlCeCommand cmd = new SqlCeCommand(
                    QUERY_PROJECT_VERSION, conn))
                {
                    using (SqlCeDataReader reader = cmd.ExecuteReader())
                    {
                        double version;
                        if (reader.Read())
                            version = reader.GetDouble(0);
                        else
                            throw new DataException(Properties.Messages.Error_GetProjectVersionFailed);

                        return version;
                    }
                }
            }
        }

        /// <summary>
        /// Method calls database engine for execution upgade scripts.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="projectVer"></param>
        private static void _UpgradeDB(string connectionString, double projectVersion)
        {
            Debug.Assert(!string.IsNullOrEmpty(connectionString));
            Debug.Assert(SchemeVersion.StartVersion <= projectVersion);

            foreach (var upgradeScript in SchemeVersion.GetUpgradeScripts(projectVersion))
            {
                if (upgradeScript == null)
                    throw new DataException(Properties.Messages.Error_GetUpgradeScriptFailed);

                DatabaseEngine.ExecuteScript(connectionString, upgradeScript, null);
            }
        }

        private static string _BuildEntityConnStr(string sqlConnStr)
        {
            Debug.Assert(sqlConnStr != null);

            Assembly assembly = Assembly.GetExecutingAssembly();

            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();
            entityBuilder.Provider = DB_PROVIDER;
            entityBuilder.ProviderConnectionString = sqlConnStr;
            entityBuilder.Metadata = DB_METADATA.Replace("*", assembly.FullName);

            return entityBuilder.ToString();
        }

        private static SqlCeException _GetCEException(Exception e)
        {
            SqlCeException ceEx = null;

            if (e is SqlCeException)
                ceEx = e as SqlCeException;
            else if (e.InnerException != null && e.InnerException is SqlCeException)
                ceEx = e.InnerException as SqlCeException;

            return ceEx;
        }

        #endregion private methods

        #region private constants

        /// <summary>
        /// Query to DataBase.
        /// </summary>
        private const string QUERY_PROJECT_VERSION = @"select [Version] from [Project]";

        #endregion
    }
}
