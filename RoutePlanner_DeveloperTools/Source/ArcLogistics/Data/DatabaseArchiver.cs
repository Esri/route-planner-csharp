using System;
using System.Diagnostics;
using System.Data.SqlServerCe;
using System.IO;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// DbArchiveResult class.
    /// </summary>
    internal class DbArchiveResult
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public DbArchiveResult(string path, bool isCreated,
            DateTime? firstDate,
            DateTime? lastDate)
        {
            _path = path;
            _isCreated = isCreated;
            _firstDate = firstDate;
            _lastDate = lastDate;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets archive path.
        /// </summary>
        public string ArchivePath
        {
            get { return _path; }
        }

        /// <summary>
        /// Gets a value indicating if archive was created.
        /// False value means that original database does not contain data to archive.
        /// </summary>
        public bool IsArchiveCreated
        {
            get { return _isCreated; }
        }

        /// <summary>
        /// Gets oldest date with assigned schedule.
        /// </summary>
        public DateTime? FirstDateWithRoutes
        {
            get { return _firstDate; }
        }

        /// <summary>
        /// Gets newest date with assigned schedule.
        /// </summary>
        public DateTime? LastDateWithRoutes
        {
            get { return _lastDate; }
        }

        #endregion public properties

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _path;
        private bool _isCreated;
        private DateTime? _firstDate;
        private DateTime? _lastDate;

        #endregion private fields
    }

    /// <summary>
    /// DatabaseArchiver class.
    /// </summary>
    internal class DatabaseArchiver
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // date format for archive name
        private const string DATE_FORMAT = "yyyy-MM-dd";

        // archive file name format
        private const string ARCH_NAME_FORMAT = "{0} {1}{2}";
        private const string ARCH_NAME_EXT_FORMAT = "{0} {1} ({2}){3}";

        #endregion constants

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Archives database.
        /// This method creates archive database if the original one contains
        /// data to archive and cleans original database (removes archived data).
        /// If method succeeds, archive will contain schedules older than
        /// specified date.
        /// If original database does not contain data to archive, archive file
        /// will not be created and DbArchiveResult.IsArchiveCreated property
        /// will be set to "false".
        /// Method throws an exception if failure occures.
        /// </summary>
        /// <param name="path">
        /// File path of original database.
        /// </param>
        /// <param name="date">
        /// Schedules older than this date will be archived.
        /// </param>
        /// <returns>
        /// DbArchiveResult object.
        /// </returns>
        public static DbArchiveResult ArchiveDatabase(string path, DateTime date)
        {
            Debug.Assert(path != null);

            bool isCreated = false;
            DateTime? firstDate = null;
            DateTime? lastDate = null;

            string baseConnStr = DatabaseHelper.BuildSqlConnString(path, true);
            string archPath = null;

            // check if database has schedules to archive
            if (_HasDataToArchive(baseConnStr, date))
            {
                // make archive file path
                archPath = _BuildArchivePath(path);

                // copy original file
                File.Copy(path, archPath);

                try
                {
                    string archConnStr = DatabaseHelper.BuildSqlConnString(
                        archPath,
                        true);

                    // apply script to archive
                    _ApplyScript(archConnStr, ResourceLoader.ReadFileAsString(ARCHIVE_SCRIPT_FILE_NAME),
                        date);

                    // query archive dates
                    _QueryDates(archConnStr, out firstDate, out lastDate);

                    // compact archive file
                    SqlCeEngine engine = new SqlCeEngine(archConnStr);
                    engine.Shrink();

                    // apply script to original database
                    _ApplyScript(baseConnStr, ResourceLoader.ReadFileAsString(CLEAN_SCRIPT_FILE_NAME),
                        date);

                    isCreated = true;
                }
                catch
                {
                    DatabaseEngine.DeleteDatabase(archPath);
                    throw;
                }
            }

            return new DbArchiveResult(archPath, isCreated, firstDate, lastDate);
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static void _ApplyScript(string connStr, string script,
            DateTime date)
        {
            Debug.Assert(connStr != null);
            Debug.Assert(script != null);

            // upgrade database if necessary
            DatabaseOpener.CheckForDbUpgrade(connStr);

            // script parameters
            SqlCeParameter[] paramArray = new SqlCeParameter[]
            {
                new SqlCeParameter("@date", date)
            };

            // apply script
            DatabaseEngine.ExecuteScript(connStr, script, paramArray);
        }

        private static string _BuildArchivePath(string path)
        {
            Debug.Assert(path != null);

            string extension = Path.GetExtension(path);
            if (String.IsNullOrEmpty(extension))
                throw new DataException(Properties.Messages.Error_GenDBArchiveName);

            string baseName = Path.GetFileNameWithoutExtension(path);
            if (String.IsNullOrEmpty(baseName))
                throw new DataException(Properties.Messages.Error_GenDBArchiveName);

            string baseDir = Path.GetDirectoryName(path);
            if (String.IsNullOrEmpty(baseName))
                throw new DataException(Properties.Messages.Error_GenDBArchiveName);

            string dateSuffix = DateTime.Now.ToString(DATE_FORMAT);
            string archName = String.Format(ARCH_NAME_FORMAT, baseName,
                dateSuffix,
                extension);

            string archPath = Path.Combine(baseDir, archName);

            bool exists = File.Exists(archPath);
            if (exists)
            {
                for (int count = 1; count < int.MaxValue; count++)
                {
                    archName = String.Format(ARCH_NAME_EXT_FORMAT, baseName,
                        dateSuffix,
                        count,
                        extension);

                    archPath = Path.Combine(baseDir, archName);
                    exists = File.Exists(archPath);
                    if (!exists)
                        break;
                }

                if (exists)
                    throw new DataException(Properties.Messages.Error_GenDBArchiveName);
            }

            return archPath;
        }

        private static bool _HasDataToArchive(string connStr, DateTime date)
        {
            Debug.Assert(connStr != null);

            using (SqlCeConnection conn = new SqlCeConnection(connStr))
            {
                conn.Open();

                using (SqlCeCommand cmd = new SqlCeCommand( 
                    QUERY_STOPS_COUNT_BY_SCHEDULE_DATE, conn))
                {
                    cmd.Parameters.Add(new SqlCeParameter("@date", date));

                    int count = (int)cmd.ExecuteScalar();
                    return (count > 0);
                }
            }
        }

        private static void _QueryDates(string connStr,
            out DateTime? firstDate,
            out DateTime? lastDate)
        {
            Debug.Assert(connStr != null);

            using (SqlCeConnection conn = new SqlCeConnection(connStr))
            {
                conn.Open();
                firstDate = _QueryDate(conn, QUERY_OLDEST_SCHEDULE);
                lastDate = _QueryDate(conn, QUERY_OLDEST_SCHEDULE);
            }
        }

        private static DateTime? _QueryDate(SqlCeConnection conn, string query)
        {
            Debug.Assert(conn != null);
            Debug.Assert(query != null);

            using (SqlCeCommand cmd = new SqlCeCommand(query, conn))
            {
                using (SqlCeDataReader reader = cmd.ExecuteReader())
                {
                    DateTime? date = null;
                    if (reader.Read())
                        date = reader.GetDateTime(0);

                    return date;
                }
            }
        }

        #endregion private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of the file with archive script.
        /// </summary>
        private const string ARCHIVE_SCRIPT_FILE_NAME = "aldb_archive.sql";

        /// <summary>
        /// Name of the file with cleaning script.
        /// </summary>
        private const string CLEAN_SCRIPT_FILE_NAME = "aldb_clean.sql";

        /// <summary>
        /// Queries to DataBase.
        /// </summary>
        private const string QUERY_STOPS_COUNT_BY_SCHEDULE_DATE = @"select count(*) from [Stops] as st inner join [Routes] as rt on rt.[Id] = st.[RouteId] inner join [Schedules] as sch on sch.[Id] = rt.[ScheduleId] where rt.[Default] = 0 and sch.[PlannedDate] < @date";
        private const string QUERY_NEWEST_SCHEDULE = @"select top(1) sch.[PlannedDate] from [Schedules] as sch inner join [Routes] as rt on sch.[Id] = rt.[ScheduleId] inner join [Stops] as st on rt.[Id] = st.[RouteId] where rt.[Default] = 0 order by sch.[PlannedDate] desc";
        private const string QUERY_OLDEST_SCHEDULE = @"select top(1) sch.[PlannedDate] from [Schedules] as sch inner join [Routes] as rt on sch.[Id] = rt.[ScheduleId] inner join [Stops] as st on rt.[Id] = st.[RouteId] where rt.[Default] = 0 order by sch.[PlannedDate]";

        #endregion
    }
}
