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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.Data;
using System.IO;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Archiving;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// ArchiveResult class.
    /// </summary>
    internal class ArchiveResult
    {
        #region constructors

        public ArchiveResult(string path, bool isCreated)
        {
            _path = path;
            _isCreated = isCreated;
        }

        #endregion constructors

        #region public properties

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

        #endregion public properties

        #region private fields

        private string _path;
        private bool _isCreated;

        #endregion private fields
    }

    // APIREV: all method should become static and belong to Project class.
    /// <summary>
    /// Class that allows to create, open, rename and delete project.
    /// </summary>
    internal static class ProjectFactory
    {
        #region public static methods

        /// <summary>
        /// Creates project.
        /// </summary>
        /// <param name="name">Project's name.</param>
        /// <param name="folderPath">Project's folder path.</param>
        /// <param name="description">Proejct's description.</param>
        /// <returns>Created project</returns>
        static public Project CreateProject(string name, string folderPath, string description, CapacitiesInfo capacitiesInfo,
            OrderCustomPropertiesInfo orderCustomPropertiesInfo, FuelTypesInfo fuelTypesInfo /*serivces*/,
                                     IProjectSaveExceptionHandler logHandler)
        {
            WorkspaceHandler workspaceHandler = new WorkspaceHandler(logHandler);

            if (name == null)
                throw new ArgumentNullException("name");
            if (folderPath == null)
                throw new ArgumentNullException("folderPath");
            if (description == null)
                throw new ArgumentNullException("description");

            if (!CheckMaximumLengthConstraint(orderCustomPropertiesInfo))
                throw new ApplicationException(Properties.Messages.Error_OrderCustomPropTooLong);

            bool isDBCreated = false;
            string dbPath = "";

            try
            {
                name = name.Trim();

                // make project configuration path
                string projCfgPath = System.IO.Path.Combine(folderPath, name);
                projCfgPath += ProjectConfiguration.FILE_EXTENSION;

                string databasePath = ProjectConfiguration.GetDatabaseFileName(name);
                // create project configuration
                ProjectConfiguration projConfig = new ProjectConfiguration(name, folderPath, description,
                    databasePath, null, DateTime.Now);

                projConfig.Validate();

                projConfig.Save();

                dbPath = projConfig.DatabasePath;

                DatabaseEngine.DeleteDatabase(dbPath);

                // create database
                DatabaseEngine.CreateDatabase(dbPath, SchemeVersion.CreationScript);
                isDBCreated = true;

                Project project = new Project(projCfgPath, capacitiesInfo,
                    orderCustomPropertiesInfo, workspaceHandler);

                foreach (FuelTypeInfo fuelTypeInfo in fuelTypesInfo)
                {
                    FuelType projectFuelType = new FuelType();
                    projectFuelType.Name = fuelTypeInfo.Name;
                    projectFuelType.Price = fuelTypeInfo.Price;
                    projectFuelType.Co2Emission = fuelTypeInfo.Co2Emission;
                    project.FuelTypes.Add(projectFuelType);
                }

                project.Save();

                workspaceHandler.Handled = true;

                return project;
            }
            catch(Exception ex)
            {
                Logger.Info(ex);
                if (isDBCreated)
                    DatabaseEngine.DeleteDatabase(dbPath);

                throw;
            }
        }

        static public Project CreateProjectFromTemplate(/*string name, string folderPath, string templateProjectConfigPath*/)
        {
            // TODO
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Open project
        /// </summary>
        /// <param name="projectConfigPath">Project path</param>
        /// <returns>Opened project</returns>
        static public Project OpenProject(string projectConfigPath,
            IProjectSaveExceptionHandler logHandler)
        {
            WorkspaceHandler workspaceHandler = new WorkspaceHandler(logHandler);
            Project project = new Project(projectConfigPath, logHandler);
            workspaceHandler.Handled = true;

            return project;
        }

        /// <summary>
        /// Rename project
        /// </summary>
        /// <param name="projectConfig">Project configuration</param>
        /// <param name="newName">New project name</param>
        static public void RenameProject(ProjectConfiguration projectConfig, string newName, ProjectCatalog catalog)
        {
            // check if project name is empty
            if (newName.Length == 0)
                throw new NotSupportedException(Properties.Resources.ProjectNameCannotBeEmpty);

            // check that project name is correct
            if (!FileHelpers.IsFileNameCorrect(newName))
                throw new NotSupportedException(Properties.Resources.ProjectNameIsNotCorrect);

            // check if project with such name already exists
            bool nameExisted = false;
            foreach (ProjectConfiguration cfg in catalog.Projects)
            {
                if (newName.Equals(cfg.Name, StringComparison.OrdinalIgnoreCase))
                {
                    nameExisted = true;
                }
            }
            if (nameExisted)
                throw new NotSupportedException(Properties.Resources.ProjectNameIsAlreadyExists);

            // todo: check that you have write rename/delete access to the files
            string newProjectPath = Path.Combine(projectConfig.FolderPath, newName);
            //if (!FileHelpers.CheckWriteAccess(newProjectPath))
            //    throw new NotSupportedException(Properties.Resources.WriteAccessDenied);

            string oldDatabasePath = projectConfig.DatabasePath;
            string newDBFileName = ProjectConfiguration.GetDatabaseFileName(newName);
            string oldDBFileName = Path.GetFileName(oldDatabasePath);
            int indexStart = oldDatabasePath.Length - oldDBFileName.Length;
            string newDBFilePath = oldDatabasePath.Remove(indexStart);
            newDBFilePath = newDBFilePath + newDBFileName;

            string newDBAbsolutePath = ProjectConfiguration.GetDatabaseAbsolutPath(projectConfig.FolderPath, newDBFilePath);
            File.Move(oldDatabasePath, newDBAbsolutePath);

            string oldProjectFilePath = projectConfig.FilePath;
            projectConfig.Name = newName;
            projectConfig.DatabasePath = newDBFilePath;
            projectConfig.Save(projectConfig.FilePath);
            File.Delete(oldProjectFilePath);
        }

        /// <summary>
        /// Delete Project
        /// </summary>
        /// <param name="projectConfigPath">Project path</param>
        static public void DeleteProject(string projectConfigPath)
        {
            if (projectConfigPath == null)
                throw new ArgumentNullException("projectConfigPath");

            // load project configuration
            ProjectConfiguration projectCfg = ProjectConfiguration.Load(projectConfigPath);

            // delete database
            DatabaseEngine.DeleteDatabase(projectCfg.DatabasePath);

            // delete project configuration file
            File.Delete(projectConfigPath);
        }

        /// <summary>
        /// Archives project.
        /// If original database does not contain data to archive, archive file
        /// will not be created and ArchiveResult.IsArchiveCreated property
        /// will be set to "false".
        /// Method throws an exception if failure occures.
        /// </summary>
        /// <param name="projectConfig">
        /// Configuration of project to archive.
        /// </param>
        /// <param name="date">
        /// Schedules older than this date will be archived.
        /// </param>
        /// <returns>
        /// ArchiveResult object.
        /// </returns>
        public static ArchiveResult ArchiveProject(ProjectConfiguration projectConfig,
            DateTime date)
        {
            Debug.Assert(projectConfig != null);

            // archive database
            DbArchiveResult dbRes = DatabaseArchiver.ArchiveDatabase(
                projectConfig.DatabasePath,
                date);

            string archConfigPath = null;

            if (dbRes.IsArchiveCreated)
            {
                bool archConfigSaved = false;
                try
                {
                    // create archive configuration
                    string archConfigName = Path.GetFileNameWithoutExtension(
                        dbRes.ArchivePath);

                    // format description
                    string firstDate = String.Empty;
                    if (dbRes.FirstDateWithRoutes != null)
                        firstDate = ((DateTime)dbRes.FirstDateWithRoutes).ToString("d");

                    string lastDate = String.Empty;
                    if (dbRes.LastDateWithRoutes != null)
                        lastDate = ((DateTime)dbRes.LastDateWithRoutes).ToString("d");

                    string archConfigDesc = String.Format(
                        Properties.Resources.ArchiveDescription,
                        firstDate,
                        lastDate);

                    // clone project properties
                    Dictionary<string, string> archProps = new Dictionary<string, string>();
                    ICollection<string> propNames = projectConfig.ProjectProperties.GetPropertiesName();
                    foreach (string propName in propNames)
                        archProps.Add(propName, projectConfig.ProjectProperties[propName]);

                    ProjectConfiguration archConfig = new ProjectConfiguration(
                        archConfigName,
                        projectConfig.FolderPath,
                        archConfigDesc,
                        dbRes.ArchivePath,
                        archProps,
                        DateTime.Now);

                    // update archive settings
                    ProjectArchivingSettings arSet = archConfig.ProjectArchivingSettings;
                    Debug.Assert(arSet != null);

                    arSet.IsArchive = true;
                    arSet.IsAutoArchivingEnabled = false;
                    arSet.LastArchivingDate = null;

                    // save archive configuration
                    archConfigPath = Path.ChangeExtension(dbRes.ArchivePath,
                        ProjectConfiguration.FILE_EXTENSION);

                    archConfig.Save(archConfigPath);
                    archConfigSaved = true;

                    // update configuration of archived project
                    projectConfig.ProjectArchivingSettings.LastArchivingDate = DateTime.Now.Date;
                    projectConfig.Save();
                }
                catch
                {
                    FileHelpers.DeleteFileSilently(dbRes.ArchivePath);

                    if (archConfigSaved)
                        FileHelpers.DeleteFileSilently(archConfigPath);

                    throw;
                }
            }

            return new ArchiveResult(archConfigPath, dbRes.IsArchiveCreated);
        }

        /// <summary>
        /// Updates project's custom order properties info.
        /// </summary>
        /// <param name="projectConfig">Project configuration.</param>
        /// <param name="propertiesInfo">Order custom properties info.</param>
        /// <exception cref="DataException">Failed to update database.</exception>
        public static void UpdateProjectCustomOrderPropertiesInfo(ProjectConfiguration projectConfig,
                                                                  OrderCustomPropertiesInfo propertiesInfo)
        {
            Debug.Assert(projectConfig != null);
            Debug.Assert(propertiesInfo != null);

            DataObjectContext dataContext = null;

            try
            {
                // Open database.
                dataContext = DatabaseOpener.OpenDatabase(projectConfig.DatabasePath);

                // Update custom order properties info in database.
                dataContext.UpdateCustomOrderPropertiesInfo(propertiesInfo);
            }
            catch (Exception ex)
            {
                // Failed to update database.
                throw new DataException(Properties.Messages.Error_DatabaseUpdate, ex, DataError.DatabaseUpdateError);
            }
            finally
            {
                if (dataContext != null)
                    dataContext.Dispose();
            }
        }

        /// <summary>
        /// Checks maximum length constraint for list of custom order properties.
        /// </summary>
        /// <param name="propertiesInfo">Order custom properties info.</param>
        /// <returns>Maximum length constraint.</returns>
        public static bool CheckMaximumLengthConstraint(OrderCustomPropertiesInfo propertiesInfo)
        {
            Debug.Assert(propertiesInfo != null);

            bool checkMaxLengthConstraint = false;

            // Serialize order custom properties info to XML.
            string customOrderPropertiesSerialized =
                ConfigDataSerializer.SerializeOrderCustomPropertiesInfo(propertiesInfo);

            // Maximum length needed to store values of cusom order properties.
            int maxLengthOfPropertiesValuesString =
                OrderCustomProperties.CalculateMaximumLengthOfDBString(propertiesInfo);

            // Check constraint.
            checkMaxLengthConstraint =
                customOrderPropertiesSerialized.Length <= MAX_DB_FIELD_LENGTH &&
                maxLengthOfPropertiesValuesString <= MAX_DB_FIELD_LENGTH;

            return checkMaxLengthConstraint;
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Maximum lentgh constraint for field in database.
        /// </summary>
        private const long MAX_DB_FIELD_LENGTH = 4000;

        #endregion Private constants
    }
}
