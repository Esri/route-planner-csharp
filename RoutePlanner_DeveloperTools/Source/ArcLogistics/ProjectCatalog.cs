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
