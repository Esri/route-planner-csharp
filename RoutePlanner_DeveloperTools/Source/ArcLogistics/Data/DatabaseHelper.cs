using System;
using System.Diagnostics;
using System.Data.SqlClient;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DatabaseHelper class.
    /// </summary>
    internal class DatabaseHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Builds SQL connection string by specified database path.
        /// </summary>
        /// <returns>Connection string.</returns>
        public static string BuildSqlConnString(string path, bool openExclusively)
        {
            Debug.Assert(path != null);

            SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder();
            sb.DataSource = path;

            string connStr = sb.ToString();
            if (openExclusively)
            {
                connStr = connStr.Replace("\"", "");
                connStr += ";File Mode=Exclusive;Persist Security Info=False;";
            }

            return connStr;
        }

        #endregion public methods
    }
}
