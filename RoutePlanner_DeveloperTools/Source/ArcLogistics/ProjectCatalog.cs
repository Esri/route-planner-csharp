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
using System.Linq;
using System.Text;
using System.IO;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class that exposes information about projects in the specified folder.
    /// </summary>
    internal class ProjectCatalog
    {
        #region constructors

        /// <summary>
        /// Creates catalog that shows projects in the specified folder.
        /// </summary>
        /// <param name="folderPath"></param>
        public ProjectCatalog(string folderPath)
        {
            if (folderPath == null)
                throw new ArgumentNullException("folderPath");

            _LoadProjects(folderPath);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Folder with the projects.
        /// </summary>
        public string FolderPath
        {
            get
            {
                return _folderPath;
            }
        }

        /// <summary>
        /// Returns list of available projects in the folder.
        /// </summary>
        /// <returns></returns>
        public ICollection<ProjectConfiguration> Projects
        {
            get
            {
                return _projects.AsReadOnly();
            }
        }

        /// <summary>
        /// Refreshes list of available projects.
        /// </summary>
        public void Refresh()
        {
            _LoadProjects(_folderPath);
        }

        #endregion

        #region private methods

        private void _LoadProjects(string folderPath)
        {
            // make project config file search pattern
            string searchPattern = "*" + ProjectConfiguration.FILE_EXTENSION;

            // find all project configuration files
            string[] files = Directory.GetFiles(folderPath, searchPattern, SearchOption.TopDirectoryOnly);

            _projects = new List<ProjectConfiguration>();
            foreach (string filePath in files)
            {
                ProjectConfiguration projectCfg;
                try
                {
                    projectCfg = ProjectConfiguration.Load(filePath);
                    _projects.Add(projectCfg);
                }
                catch { }// invalid projects are ignored
            }

            _folderPath = folderPath;
        }

        #endregion

        #region private members

        private string _folderPath = "";
        List<ProjectConfiguration> _projects = null;

        #endregion
    }
}
