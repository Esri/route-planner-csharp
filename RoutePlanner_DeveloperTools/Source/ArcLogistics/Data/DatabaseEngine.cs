using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlServerCe;
using System.Data;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DatabaseEngine class.
    /// </summary>
    internal class DatabaseEngine
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates SQL Server CE database using specified file path.
        /// Executes SQL script using specified script value if not null.
        /// </summary>
        /// <param name="path">
        /// Database file path.
        /// </param>
        /// <param name="createScript">
        /// Transact SQL script to execute. Optional (can be null).
        /// Provided script must conform to SQL Server CE.
        /// </param>
        public static void CreateDatabase(string path, string createScript)
        {
            Debug.Assert(path != null);

            bool dbCreated = false;
            try
            {
                // check if specified file exists
                if (File.Exists(path))
                    throw new DataException(Properties.Messages.Error_DbFileExists);

                // build connection string
                string connStr = DatabaseHelper.BuildSqlConnString(path, false);
                
                // create database
                SqlCeEngine engine = new SqlCeEngine(connStr);
                engine.CreateDatabase();
                dbCreated = true;

                // execute SQL script
                if (createScript != null)
                    ExecuteScript(connStr, createScript, null);
            }
            catch (Exception e)
            {
                Logger.Error(e);

                if (dbCreated)
                    DeleteDatabase(path);

                throw;
            }
        }

        /// <summary>
        /// Executes SQL script using specified connection string and script
        /// value.
        /// </summary>
        /// <param name="sqlConnStr">
        /// SQL connection string.
        /// </param>
        /// <param name="script">
        /// Transact SQL script to execute. Provided script must conform to SQL
        /// Server CE.
        /// </param>
        public static void ExecuteScript(string sqlConnStr, string script,
            SqlCeParameter[] parameters)
        {
            Debug.Assert(sqlConnStr != null);
            Debug.Assert(script != null);

            using (SqlCeConnection conn = new SqlCeConnection(sqlConnStr))
            {
                conn.Open();

                SqlCeCommand cmd = new SqlCeCommand();
                cmd.Connection = conn;

                // set parameters if any
                if (parameters != null)
                {
                    foreach (SqlCeParameter param in parameters)
                        cmd.Parameters.Add(param);
                }

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
        }

        /// <summary>
        /// Deletes specified database silently.
        /// </summary>
        /// <param name="path">Database path.</param>
        public static void DeleteDatabase(string path)
        {
            Debug.Assert(path != null);

            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// SQL Server CE database extension.
        /// </summary>
        public const string DATABASE_EXTENSION = ".sdf";

        /// <summary>
        /// Script commands delimiter.
        /// </summary>
        private const string CMD_DELIMITER = "\\s+GO\\s+";

        #endregion
    }
}
