using System;
using System.Reflection;

namespace ESRI.ArcLogistics
{
    public class DataFolder
    {
        #region constants

        private const string APP_RELATIVE_FILEPATH = @"RoutePlanner";

        #endregion

        #region public static properties

        /// <summary>
        /// Path to folder with application data
        /// </summary>
        public static string Path
        {
            get
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                appDataFolder = System.IO.Path.Combine(appDataFolder, APP_RELATIVE_FILEPATH);
                return appDataFolder;
            }
        }

        #endregion
    }
}
