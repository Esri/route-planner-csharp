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
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Archiving;
using ESRI.ArcLogistics.BreaksHelpers;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class contains project configuration settings.
    /// </summary>
    internal class ProjectConfiguration
    {
        #region constants

        public const string FILE_EXTENSION = ".xml";
        private const string ROOT_NODE_NAME = "ProjectConfiguration";
        private const string DATABASEPATH_NODE_NAME = "DatabasePath";
        private const string DESCRIPTION_NODE_NAME = "Description";
        private const string PROPERTIES_NODE_NAME = "Properties";
        private const string PROPERTY_NODE_NAME = "Property";
        private const string NAME_ATTRIBUTE_NAME = "Name";
        private const string VALUE_ATTRIBUTE_NAME = "Value";
        private const string CREATION_TIME_NODE_NAME = "CreationTime";

        #endregion

        #region constructors

        internal ProjectConfiguration(string name, string folderPath, string description,
            string databasePath, Dictionary<string, string> propertiesMap,
            DateTime creationTime)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException();

            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException();

            if (string.IsNullOrEmpty(databasePath))
                throw new ArgumentException();

            _name = name;
            _folderPath = folderPath;
            _description = description;
            _creationTime = creationTime;

            // make project database path. By default it has common path with project xml file
            _databasePath = databasePath;
            _projectProperties = new ProjectProperties(propertiesMap);
            _projectArchivingSettings = new ProjectArchivingSettings(_projectProperties);
            _breaksSettings = new BreaksSettings(_projectProperties);
        }

        #endregion

        #region public static methods

        /// <summary>
        /// Loads project configuration from file.
        /// </summary>
        /// <param name="filePath">Project configuration file path.</param>
        static internal ProjectConfiguration Load(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");

            if (!FileHelpers.ValidateFilepath(filePath))
                throw new ArgumentException(Properties.Resources.ProjectCfgFilePathInvalid);

            // check if file with project configuration doesn't exist
            if (!System.IO.File.Exists(filePath))
                throw new FileNotFoundException(Properties.Resources.ProjectCfgNotFound, filePath);

            ProjectConfiguration projectCfg = null;
            try
            {
                string name = Path.GetFileNameWithoutExtension(filePath);
                string folderPath = Path.GetDirectoryName(filePath);
                string databasePath = string.Empty;
                string description = null;
                Dictionary<string, string> propertiesMap = new Dictionary<string,string>();
                DateTime? creationTime = null;

                XmlDocument defaultsDoc = new XmlDocument();
                defaultsDoc.Load(filePath);

                // try to parse xml file
                XmlElement rootElement = defaultsDoc.DocumentElement;
                Debug.Assert(rootElement.Name == ROOT_NODE_NAME);
                foreach (XmlNode node in rootElement.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue; // skip comments and other non element nodes

                    if (node.Name.Equals(DATABASEPATH_NODE_NAME, StringComparison.OrdinalIgnoreCase))
                        databasePath = node.FirstChild.Value;

                    else if (node.Name.Equals(CREATION_TIME_NODE_NAME, StringComparison.OrdinalIgnoreCase))
                        creationTime = _ParseTime(node.FirstChild.Value);

                    else if (node.Name.Equals(DESCRIPTION_NODE_NAME, StringComparison.OrdinalIgnoreCase))
                    {
                        if (node.FirstChild != null)
                            description = node.FirstChild.Value;
                    }

                    else if (node.Name.Equals(PROPERTIES_NODE_NAME, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (XmlNode nodePropery in node.ChildNodes)
                        {
                            if (nodePropery.NodeType != XmlNodeType.Element)
                                continue; // skip comments and other non element nodes

                            if (nodePropery.Name.Equals(PROPERTY_NODE_NAME, StringComparison.OrdinalIgnoreCase))
                            {
                                string propertyName = nodePropery.Attributes[NAME_ATTRIBUTE_NAME].Value;
                                string propertyValue = nodePropery.Attributes[VALUE_ATTRIBUTE_NAME].Value;
                                propertiesMap.Add(propertyName, propertyValue);
                            }

                            else
                                throw new NotSupportedException();
                        }
                    }

                    else
                        throw new NotSupportedException();
                }

                if (creationTime == null)
                    creationTime = File.GetCreationTime(filePath);

                projectCfg = new ProjectConfiguration(name, folderPath, description, databasePath, propertiesMap, (DateTime)creationTime);
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
                throw new Exception(Properties.Resources.ProjectCfgIsInvalid, ex);
            }
           
            return projectCfg;
        }

        static internal string GetDatabaseAbsolutPath(string projectFolder, string filepath)
        {
            return (FileHelpers.IsAbsolutPath(filepath))? filepath : Path.Combine(projectFolder, filepath);
        }

        static internal string GetDatabaseFileName(string name)
        {
            return (name + DatabaseEngine.DATABASE_EXTENSION);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Save project to default path
        /// </summary>
        public void Save()
        {
            Save(FilePath);
        }

        internal void Validate()
        {
            // todo Move to resources
            // check if project name is empty
            if (_name.Length == 0)
                throw new NotSupportedException(Properties.Resources.ProjectNameCannotBeEmpty);

            // check that project name is correct
            if (!FileHelpers.IsFileNameCorrect(_name))
                throw new NotSupportedException(Properties.Resources.ProjectNameIsNotCorrect);

            // check that project settings location path is valid
            if (!FileHelpers.ValidateFilepath(FilePath)) // path to file have to be correct
                throw new NotSupportedException(Properties.Resources.ProjectFilepathIsNotCorrect);

            // check that we have access rights to the folder
            string path = Path.GetDirectoryName(FilePath);
            if (!FileHelpers.CheckWriteAccess(path))
                throw new NotSupportedException(Properties.Resources.WriteAccessDenied);

            // filename equals
        }

        /// <summary>
        /// Saves project configuration file by specified location.
        /// </summary>
        /// <param name="filePath"></param>
        public void Save(string filePath)
        {
            XmlWriter writer = null;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = CommonHelpers.XML_SETTINGS_INDENT_CHARS;

                writer = XmlWriter.Create(filePath, settings);
                writer.WriteStartElement(ROOT_NODE_NAME);

                writer.WriteStartElement(DATABASEPATH_NODE_NAME);
                writer.WriteValue(_databasePath);
                writer.WriteEndElement();

                writer.WriteStartElement(CREATION_TIME_NODE_NAME);
                writer.WriteValue(_FormatTime(_creationTime));
                writer.WriteEndElement();

                  if (_description != null)
                  {
                    writer.WriteStartElement(DESCRIPTION_NODE_NAME);
                      writer.WriteValue(_description);
                    writer.WriteEndElement();
                  }

                  // write properies
                  if (null != _projectProperties)
                  {
                    writer.WriteStartElement(PROPERTIES_NODE_NAME);
                        ICollection<string> properties = _projectProperties.GetPropertiesName();
                        foreach (string property in properties)
                        {
                            string value = _projectProperties.GetPropertyByName(property);
                            writer.WriteStartElement(PROPERTY_NODE_NAME);
                            writer.WriteAttributeString(NAME_ATTRIBUTE_NAME, property);
                              writer.WriteAttributeString(VALUE_ATTRIBUTE_NAME, value);
                            writer.WriteEndElement();
                        }
                    writer.WriteEndElement();
                  }

                writer.WriteEndElement();

                writer.Flush();
                writer.Close();
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Project configuration file name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Project configuration file path.
        /// </summary>
        public string FilePath
        {
            get
            {
                string filepath = Path.Combine(_folderPath, _name);
                
                filepath += FILE_EXTENSION;
                return filepath;
            }
        }

        /// <summary>
        /// Path to project directory
        /// </summary>
        public string FolderPath
        { 
            get { return _folderPath; }
        }

        /// <summary>
        /// Project database path
        /// </summary>
        public string DatabasePath
        {
            get
            {
                string databaseFullPath = GetDatabaseAbsolutPath(_folderPath, _databasePath);
                return databaseFullPath;
            }
            set
            {
                _databasePath = Path.GetFileName(value);
            }
        }

        /// <summary>
        /// Project description.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Project creation time.
        /// </summary>
        public DateTime CreationTime
        {
            get { return _creationTime; }
        }

        public IProjectProperties ProjectProperties
        {
            get { return _projectProperties; }
        }

        public ProjectArchivingSettings ProjectArchivingSettings
        {
            get { return _projectArchivingSettings; }
        }

        /// <summary>
        /// Project breaks settings.
        /// </summary>
        /// <remarks>Cannot be null.</remarks>
        public BreaksSettings BreaksSettings
        {
            get { return _breaksSettings; }
        }

        #endregion

        #region private methods

        private static string _FormatTime(DateTime date)
        {
            return date.ToString("G", DateTimeFormatInfo.InvariantInfo);
        }

        private static DateTime? _ParseTime(string date)
        {
            DateTime? res = null;
            try
            {
                if (!String.IsNullOrEmpty(date))
                    res = DateTime.Parse(date, DateTimeFormatInfo.InvariantInfo);
            }
            catch { }

            return res;
        }

        #endregion

        #region private members

        private string _name = string.Empty;
        private string _folderPath = string.Empty;
        private string _description = string.Empty;
        private string _databasePath = string.Empty;
        private DateTime _creationTime;
        private IProjectProperties _projectProperties = null;
        private ProjectArchivingSettings _projectArchivingSettings = null;
        private BreaksSettings _breaksSettings = null;

        #endregion

        // TODO: add support of versions.
        // TODO: add support of relative database path.
    }
}