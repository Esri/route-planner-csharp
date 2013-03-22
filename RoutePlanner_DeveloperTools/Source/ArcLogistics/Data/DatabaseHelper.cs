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
