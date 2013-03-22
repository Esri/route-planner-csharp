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
