using System;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.Common;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Database combponent factory class.
    /// Helpers function for Data Connection managment.
    /// </summary>
    internal sealed class DbFactory
    {
        #region Public helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates database connection.
        /// </summary>
        /// <param name="connectionString">Connetion string (can be null).</param>
        /// <param name="messageFailure">Description message failure
        /// (can be null if problem absent).</param>
        /// <returns>Connection object (can be null).</returns>
        public static DbConnection CreateConnection(string connectionString,
                                                    out string messageFailure)
        {
            messageFailure = null;
            if (string.IsNullOrEmpty(connectionString))
                return null;

            // try create OleDB connection
            DbConnection connection = _CreateConnection(OleDbFactory.Instance,
                                                        connectionString,
                                                        out messageFailure);
            // problem detected
            if (null == connection)
            {   // try create Odbc connection
                connection = _CreateConnection(OdbcFactory.Instance,
                                               connectionString,
                                               out messageFailure);
                // connection impossible
                if (null != connection)
                {   // release first problem
                    messageFailure = null;
                }
            }

            return connection;
        }

        /// <summary>
        /// Creates data adapter for specifided database connection.
        /// </summary>
        /// <param name="connection">Database connection for detection specialization.</param>
        /// <returns>Relatived data adapter.</returns>
        public static DbDataAdapter CreateDataAdapter(DbConnection connection)
        {
            Debug.Assert(null != connection); // created
            Debug.Assert((connection.GetType() == typeof(OleDbConnection)) ||
                         (connection.GetType() == typeof(OdbcConnection))); // supported type

            // select DB provider factory by connection type
            DbProviderFactory factory = (connection.GetType() == typeof(OleDbConnection)) ?
                                            (DbProviderFactory)OleDbFactory.Instance :
                                            (DbProviderFactory)OdbcFactory.Instance;
            return _CreateDataAdapter(factory);
        }

        #endregion // Public helpers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates database connection.
        /// </summary>
        /// <param name="factory">Database provider factory.</param>
        /// <param name="connectionString">Connetion string.</param>
        /// <param name="messageFailure">Description message failure
        /// (can be null if problem absent).</param>
        /// <returns>Connection object (can be null).</returns>
        private static DbConnection _CreateConnection(DbProviderFactory factory,
                                                      string connectionString,
                                                      out string messageFailure)
        {
            Debug.Assert(null != factory); // created
            Debug.Assert(!string.IsNullOrEmpty(connectionString));

            messageFailure = null;

            DbConnection connection = null;
            try
            {
                connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;
            }
            catch (Exception ex)
            {
                connection = null;
                messageFailure = App.Current.GetString("FailedDatabaseConnection", ex.Message);
                Logger.Error(ex);
            }

            return connection;
        }

        /// <summary>
        /// Creates data adapter for specifided database connection.
        /// </summary>
        /// <param name="factory">Database provider factory.</param>
        /// <returns>Created Relatived data adapter.</returns>
        private static DbDataAdapter _CreateDataAdapter(DbProviderFactory factory)
        {
            Debug.Assert(null != factory); // created

            DbDataAdapter adapter = factory.CreateDataAdapter();
            return adapter;
        }

        #endregion // Private helpers
    }
}
