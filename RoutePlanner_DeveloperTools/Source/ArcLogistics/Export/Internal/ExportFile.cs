using System;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics.Export;

namespace ESRI.ArcLogistics.Export
{
    /// <summary>
    /// Class that reperesent application export settings.
    /// </summary>
    internal sealed class ExportFile
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ExportFile</c>.
        /// </summary>
        public ExportFile()
        { }

        #endregion // Constructor

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads export profiles from file.
        /// </summary>
        /// <param name="fileName">Storage file path.</param>
        /// <param name="structureKeeper">Export structure reader.</param>
        /// <returns>Loaded list of <see cref="P:ESRI.ArcLogistics.Export.Profile" />.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Export file is invalid.
        /// In property "Source" there is path to invalid export file.</exception>
        public List<Profile> Load(string fileName, ExportStructureReader structureKeeper)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(); // exception

            var profiles = new List<Profile> ();
            if (File.Exists(fileName))
            {
                var doc = new XmlDocument();

                // Try to load file.
                try
                {
                    doc.Load(fileName);

                    // create navigation tree in memory
                    profiles = _LoadContent(doc.DocumentElement, structureKeeper);
                }
                catch (Exception ex )
                {
                    // If XML corrupted - wrap exception and save path to file.
                    if (ex is XmlException || ex is NotSupportedException)
                    {
                        SettingsException settingsException = new SettingsException(ex.Message, ex);
                        settingsException.Source = fileName;
                        throw settingsException;
                    }
                }
            }

            return profiles;
        }

        /// <summary>
        /// Stores export profiles to file.
        /// </summary>
        /// <param name="fileName">Storage file path.</param>
        /// <param name="profiles">Profiles to save.</param>
        public void Save(string fileName, ICollection<Profile> profiles)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(); // exception

            // rewrite imports file
            string defaultsFolderPath = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(defaultsFolderPath))
                Directory.CreateDirectory(defaultsFolderPath);

            if (File.Exists(fileName))
                File.Delete(fileName);

            _CreateFile(fileName, profiles);
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Load helpers

        /// <summary>
        /// Loads fields of table.
        /// </summary>
        /// <param name="nodeTable">Table node.</param>
        /// <param name="table">Table description.</param>
        private void _LoadFields(XmlNode nodeTable, ITableDefinition table)
        {
            // remove predefinited fields
            table.ClearFields();

            ICollection<string> supportedFields = table.SupportedFields;
            foreach (XmlNode node in nodeTable.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_FIELDS, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XmlNode nodeField in node.ChildNodes)
                    {
                        if (nodeField.NodeType != XmlNodeType.Element)
                            continue; // skip comments and other non element nodes

                        if (nodeField.Name.Equals(NODE_NAME_FIELD, StringComparison.OrdinalIgnoreCase))
                        {
                            string value = nodeField.Attributes[ATTRIBUTE_NAME_NAME].Value;
                            if (supportedFields.Contains(value))
                                table.AddField(value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads <c>Table descriptions</c>.
        /// </summary>
        /// <param name="node">Tables node.</param>
        /// <param name="tables">Table descriptions.</param>
        private void _LoadTables(XmlNode node, ICollection<ITableDefinition> tables)
        {
            foreach (XmlNode nodeTable in node.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                Debug.Assert(nodeTable.Name.Equals(NODE_NAME_TABLE, StringComparison.OrdinalIgnoreCase));

                string tableName = nodeTable.Attributes[ATTRIBUTE_NAME_NAME].Value;
                var tableType =
                    (TableType)Enum.Parse(typeof(TableType), nodeTable.Attributes[ATTRIBUTE_NAME_TYPE].Value);
                foreach (ITableDefinition table in tables)
                {
                    if (table.Type == tableType)
                    {
                        table.Name = tableName;
                        _LoadFields(nodeTable, table);
                        break; // process done
                    }
                }
            }
        }

        /// <summary>
        /// Loads <c>Profile</c>.
        /// </summary>
        /// <param name="nodeProfile">Profile node.</param>
        /// <param name="structureKeeper"><see cref="P:ESRI.ArcLogistics.Export.ExportStructureReader" />.</param>
        /// <returns>Loaded <see cref="P:ESRI.ArcLogistics.Export.Profile" />.</returns>
        private Profile _LoadProfile(XmlNode nodeProfile, ExportStructureReader structureKeeper)
        {
            var type =
                (ExportType)Enum.Parse(typeof(ExportType), nodeProfile.Attributes[ATTRIBUTE_NAME_TYPE].Value);
            string filePath = nodeProfile.Attributes[ATTRIBUTE_NAME_FILE].Value;

            bool isDefault = false;
            if (null != nodeProfile.Attributes[ATTRIBUTE_NAME_DEFAULT])
                isDefault = bool.Parse(nodeProfile.Attributes[ATTRIBUTE_NAME_DEFAULT].Value);

            var profile = new Profile(structureKeeper, type, filePath, isDefault);
            profile.Name = nodeProfile.Attributes[ATTRIBUTE_NAME_NAME].Value;
            ICollection<ITableDefinition> tables = profile.TableDefinitions;

            foreach (XmlNode node in nodeProfile.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_DESCRIPTION, StringComparison.OrdinalIgnoreCase))
                {
                    if (null != node.FirstChild)
                        profile.Description = node.FirstChild.Value;
                }
                else if (node.Name.Equals(NODE_NAME_TABLES, StringComparison.OrdinalIgnoreCase))
                    _LoadTables(node, tables);
            }

            return profile;
        }

        /// <summary>
        /// Loads <c>Profiles</c> from file.
        /// </summary>
        /// <param name="nodeProfiles">Profiles node.</param>
        /// <param name="structureKeeper"><see cref="P:ESRI.ArcLogistics.Export.ExportStructureReader" />.</param>
        /// <returns>Loaded list of <see cref="P:ESRI.ArcLogistics.Export.Profile" />.</returns>
        /// <exception cref="T:System.NotSupportedException.NotSupportedException">
        /// XML file is invalid.</exception>
        private List<Profile> _LoadContent(XmlElement nodeProfiles,
                                           ExportStructureReader structureKeeper)
        {
            var profiles = new List<Profile> ();
            foreach (XmlNode node in nodeProfiles.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_PROFILE, StringComparison.OrdinalIgnoreCase))
                    profiles.Add(_LoadProfile(node, structureKeeper));
                else
                    throw new NotSupportedException(); // exception
            }

            return profiles;
        }

        #endregion // Load helpers

        #region Save helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saves export <c>Profile</c>.
        /// </summary>
        /// <param name="profile"><see cref="P:ESRI.ArcLogistics.Export.Profile" /> to saving.</param>
        /// <param name="writer">XML writer.</param>
        private void _SaveProfile(Profile profile, XmlWriter writer)
        {
            writer.WriteStartElement(NODE_NAME_TABLES);

              foreach (ITableDefinition table in profile.TableDefinitions)
              {
                  writer.WriteStartElement(NODE_NAME_TABLE);
                  writer.WriteAttributeString(ATTRIBUTE_NAME_NAME, table.Name);
                  writer.WriteAttributeString(ATTRIBUTE_NAME_TYPE, table.Type.ToString());
                    writer.WriteStartElement(NODE_NAME_FIELDS);
                    foreach (string field in table.Fields)
                    {
                        writer.WriteStartElement(NODE_NAME_FIELD);
                        writer.WriteAttributeString(ATTRIBUTE_NAME_NAME, field);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                  writer.WriteEndElement();
              }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Saves <c>Profiles</c> collection.
        /// </summary>
        /// <param name="profiles">Collection of <see cref="P:ESRI.ArcLogistics.Export.Profile" />
        /// to saving.</param>
        /// <param name="writer">XML writer.</param>
        private void _SaveContent(ICollection<Profile> profiles, XmlWriter writer)
        {
            // write profiles
            writer.WriteStartElement(NODE_NAME_PROFILES);
              foreach (Profile profile in profiles)
              {   // write profile parameters
                  writer.WriteStartElement(NODE_NAME_PROFILE);
                  writer.WriteAttributeString(ATTRIBUTE_NAME_NAME, profile.Name);
                  writer.WriteAttributeString(ATTRIBUTE_NAME_TYPE, profile.Type.ToString());
                  writer.WriteAttributeString(ATTRIBUTE_NAME_FILE, profile.FilePath);
                  writer.WriteAttributeString(ATTRIBUTE_NAME_DEFAULT, profile.IsDefault.ToString());
                    writer.WriteStartElement(NODE_NAME_DESCRIPTION);
                    writer.WriteValue(profile.Description);
                    writer.WriteEndElement();

                    _SaveProfile(profile, writer);
                  writer.WriteEndElement();
              }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Creates storage file to saving <c>Profiles</c> collection.
        /// </summary>
        /// <param name="fileFullName">Storage file full name.</param>
        /// <param name="profiles">Collection of <see cref="P:ESRI.ArcLogistics.Export.Profile" />
        /// to saving.</param>
        /// <returns>TRUE if operation done successfully.</returns>
        public bool _CreateFile(string fileFullName, ICollection<Profile> profiles)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileFullName));

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = CommonHelpers.XML_SETTINGS_INDENT_CHARS;

            bool bOk = false;
            XmlWriter writer = null;
            try
            {
                writer = XmlWriter.Create(fileFullName, settings);
                _SaveContent(profiles, writer);
                writer.Flush();
                writer.Close();
                bOk = true;
            }
            catch
            {
                if (writer != null)
                    writer.Close();
                File.Delete(fileFullName);
            }

            return bOk;
        }

        #endregion // Save helpers

        #endregion // Private methods

        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // XML tags.
        private const string NODE_NAME_PROFILES = "Profiles";
        private const string NODE_NAME_PROFILE = "Profile";

        private const string NODE_NAME_DESCRIPTION = "Description";
        private const string NODE_NAME_TABLES = "Tables";
        private const string NODE_NAME_TABLE = "Table";
        private const string NODE_NAME_FIELDS = "Fields";
        private const string NODE_NAME_FIELD = "Field";

        private const string ATTRIBUTE_NAME_NAME = "Name";
        private const string ATTRIBUTE_NAME_TYPE = "Type";
        private const string ATTRIBUTE_NAME_FILE = "FilePath";
        private const string ATTRIBUTE_NAME_DEFAULT = "Default";

        #endregion // Constants
    }
}
