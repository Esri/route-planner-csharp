using System.Collections.Generic;
using System.Linq;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.Data
{
    /// <summary>
    /// SchemeVersion class.
    /// </summary>
    internal sealed class SchemeVersion
    {
        #region Public properties

        /// <summary>
        /// Gets start supported database scheme version.
        /// </summary>
        public static double StartVersion
        {
            get { return START_SCHEME_VERSION; }
        }

        /// <summary>
        /// Gets current database scheme version.
        /// </summary>
        public static double CurrentVersion
        {
            get { return CUR_SCHEME_VERSION; }
        }

        /// <summary>
        /// Gets database creation SQL script.
        /// </summary>
        public static string CreationScript
        {
            get { return ResourceLoader.ReadFileAsString(CREATE_SCRIPT.scriptName); }
        }

        #endregion Public properties

        #region Public methods

        /// <summary>
        /// Gets collection of SQL scripts to upgrade database from the 
        /// specified version to the <see cref="CUR_SCHEME_VERSION"/>.
        /// </summary>
        /// <param name="version">Current database scheme version.</param>
        /// <returns>Collection of SQL upgrade scripts.</returns>
        public static IEnumerable<string> GetUpgradeScripts(double version)
        {
            var scripts =
                from script in UPGRADE_SCRIPTS
                where script.version >= version
                orderby script.version ascending
                select ResourceLoader.ReadFileAsString(script.scriptName);

            return scripts;
        }

        #endregion Public methods

        #region Private data types

        /// <summary>
        /// Databse script entry.
        /// </summary>
        private class DbScript
        {
            /// <summary>
            /// Script version.
            /// In case of upgrade script, the script upgrades DB from this
            /// version to the current one.
            /// </summary>
            public double version;

            /// <summary>
            /// script name in the application resources
            /// </summary>
            public string scriptName;

            public DbScript(double version, string scriptName)
            {
                this.version = version;
                this.scriptName = scriptName + SCRIPT_EXTENSION;
            }
        }

        #endregion Private data types

        #region Private constants

        /// <summary>
        /// Min supported database scheme version.
        /// </summary>
        private const double START_SCHEME_VERSION = 1.0;

        // current database scheme version
        private const double CUR_SCHEME_VERSION = 1.2;

        /// <summary>
        /// Extionsion for script files.
        /// </summary>
        private const string SCRIPT_EXTENSION = ".sql";

        // creation script
        private static readonly DbScript CREATE_SCRIPT = new DbScript(
            CUR_SCHEME_VERSION,
            "aldb_create");

        /// <summary>
        /// Collection of upgrade scripts.
        /// </summary>
        /// <remarks>
        /// Scripts must contain commands to update project version to the next one.
        /// Use underscores instead of dots in script resource names.
        /// </remarks>
        /// <example>
        /// ...
        /// new DbScript(0.9,  "aldb_upgrade_0_9"), // upgrades from 0.9 to 1.0
        /// new DbScript(1.0,  "aldb_upgrade_1_0"), // upgrades from 1.0 to 1.1
        /// new DbScript(1.1,  "aldb_upgrade_1_1"), // upgrades from 1.1 to 1.2
        /// ...
        /// </example>
        private static readonly DbScript[] UPGRADE_SCRIPTS =
        {
            // TODO: add upgrade scripts here as necessary
            new DbScript(1.0, "aldb_upgrade_1_0"),
            new DbScript(1.01, "aldb_upgrade_1_01"),
            new DbScript(1.1, "aldb_upgrade_1_1"),
        };

        #endregion Private constants
    }
}
