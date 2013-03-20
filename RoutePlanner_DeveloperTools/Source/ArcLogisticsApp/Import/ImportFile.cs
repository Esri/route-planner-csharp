using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App.Import
{
    /// <summary>
    /// Application import settings.
    /// </summary>
    internal class ImportFile
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">Path to import config file.</param>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Import file is invalid.
        /// In property "Source" there is path to invalid import file.</exception>
        public ImportFile(string fileName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName));

            _profiles.Clear();
            _fileName = fileName;

            if (!File.Exists(fileName))
                return;

            XmlDocument doc = new XmlDocument();
            // Try to load file.
            try
            {
                doc.Load(fileName);
                _LoadContent(doc.DocumentElement);
            }
            catch (Exception ex)
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

        #endregion // Constructor

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Store Import profile to file
        /// </summary>
        public bool Save()
        {
            Debug.Assert(!string.IsNullOrEmpty(_fileName)); // NOTE: load first
            Debug.Assert(null != Profiles); // NOTE: load first

            // rewrite imports file
            string defaultsFolderPath = Path.GetDirectoryName(_fileName);
            if (!Directory.Exists(defaultsFolderPath))
                Directory.CreateDirectory(defaultsFolderPath);

            if (File.Exists(_fileName))
                File.Delete(_fileName);

            return _CreateFile(_fileName);
        }

        /// <summary>
        /// Update defaults profile from project properties
        /// </summary>
        /// <remarks>Call after Load()</remarks>
        public string CreateDefaultsDescription()
        {
            Debug.Assert(null != Profiles); // NOTE: load first
            Debug.Assert(_IsDefaultOnePerType());

            StringBuilder sb = new StringBuilder();
            foreach (ImportProfile profile in Profiles)
            {
                if (profile.IsDefault)
                {
                    if (!string.IsNullOrEmpty(sb.ToString()))
                        sb.Append(SPLITTER_GROUPS);

                    sb.Append(profile.Type.ToString());
                    sb.Append(SPLITTER_GROUP);
                    sb.Append(profile.Name);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Update defaults profile from project properties
        /// </summary>
        /// <remarks>Call after Load()</remarks>
        public void UpdateDefaults(string projectDefaults)
        {
            Debug.Assert(null != Profiles); // NOTE: load first

            if ((Profiles.Count == 0) || string.IsNullOrEmpty(projectDefaults))
                return; // update not needed

            // split by groups (1 group for every type)
            char[] groupSplitters = new char[] {SPLITTER_GROUP};
            string[] groups = projectDefaults.Split(new char[]{SPLITTER_GROUPS}, StringSplitOptions.RemoveEmptyEntries);
            for (int groupNum = 0; groupNum < groups.Length; ++groupNum)
            {
                // split by group info
                string[] group = groups[groupNum].Split(groupSplitters);
                Debug.Assert(2 == group.Length);

                bool isSupportedType = true;
                ImportType type = ImportType.Orders;
                try
                {
                    type = (ImportType)Enum.Parse(typeof(ImportType), group[0]);
                }
                catch
                {
                    isSupportedType = false;
                }

                if (!isSupportedType)
                    continue; // NOTE: found not supported type - ignore this.

                string name = group[1];

                bool isUpdateNeeded = false;
                foreach (ImportProfile profile in Profiles)
                {
                    if (name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase) && (type == profile.Type))
                    {
                        if (!profile.IsDefault)
                            isUpdateNeeded = true;
                        break; // founded
                    }
                }

                if (!isUpdateNeeded)
                    continue; // update not needed

                foreach (ImportProfile profile in Profiles)
                {
                    if (type == profile.Type)
                        profile.IsDefault = name.Equals(profile.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
        }
        #endregion // Public methods

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Application import profiles
        /// </summary>
        /// <remarks>Call after Load()</remarks>
        public List<ImportProfile> Profiles
        {
            get { return _profiles; }
        }

        /// <summary>
        /// Application import auto field name
        /// </summary>
        /// <remarks>Call after Load()</remarks>
        public StringDictionary FieldAliases
        {
            get
            {
                if (null == _aliases)
                    _CreateAliases();

                return _aliases;
            }
        }

        #endregion // Public properties

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<FieldMap> _LoadFieldsMap(XmlNode nodeFieldsMap, ImportType type)
        {
            StringDictionary mapTitle2Name = PropertyHelpers.GetTitle2NameMap(type);

            List<FieldMap> fieldsMap = new List<FieldMap>();
            foreach (XmlNode node in nodeFieldsMap.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_FIELDSMAP, StringComparison.OrdinalIgnoreCase))
                {
                    FieldMap fieldMap = new FieldMap(node.Attributes[ATTRIBUTE_NAME_DSTFIELD].Value,
                                                     node.Attributes[ATTRIBUTE_NAME_SRCFIELD].Value);
                    if (mapTitle2Name.ContainsValue(fieldMap.ObjectFieldName))
                        fieldsMap.Add(fieldMap);
                }
            }

            return fieldsMap;
        }

        private ImportSettings _LoadSettings(XmlNode nodeSettings, ImportType type)
        {
            ImportSettings settings = new ImportSettings();
            foreach (XmlNode node in nodeSettings.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_SOURCE, StringComparison.OrdinalIgnoreCase))
                {
                    if (null != node.FirstChild)
                        settings.Source = node.FirstChild.Value;
                }
                else if (node.Name.Equals(NODE_NAME_TABLE, StringComparison.OrdinalIgnoreCase))
                    settings.TableName = node.Attributes[ATTRIBUTE_NAME_NAME].Value;
                else if (node.Name.Equals(NODE_NAME_FIELDSMAPPING, StringComparison.OrdinalIgnoreCase))
                   settings.FieldsMap = _LoadFieldsMap(node, type);
            }

            return settings;
        }

        private ImportProfile _LoadProfile(XmlNode nodeSettings)
        {
            ImportProfile profile = new ImportProfile();
            profile.Name = nodeSettings.Attributes[ATTRIBUTE_NAME_NAME].Value;

            bool isSupportedType = true;
            try
            {
                profile.Type = (ImportType)Enum.Parse(typeof(ImportType), nodeSettings.Attributes[ATTRIBUTE_NAME_TYPE].Value);
            }
            catch
            {
                isSupportedType = false;
            }

            if (!isSupportedType)
                return null;

            bool isDefault = bool.Parse(nodeSettings.Attributes[ATTRIBUTE_NAME_DEFAULT].Value);
            // NOTE: ignored in this version - it is left to backward compatibility

            profile.IsOnTime = false;
            if (null != nodeSettings.Attributes[ATTRIBUTE_NAME_ONTIME])
                profile.IsOnTime = bool.Parse(nodeSettings.Attributes[ATTRIBUTE_NAME_ONTIME].Value);
            else if (isDefault)
                profile.IsOnTime = true;
            profile.IsDefault = false;
            // else Do nothing

            foreach (XmlNode node in nodeSettings.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_DESCRIPTION, StringComparison.OrdinalIgnoreCase))
                {
                    if (null != node.FirstChild)
                        profile.Description = node.FirstChild.Value;
                }
                else if (node.Name.Equals(NODE_NAME_SETTINGS, StringComparison.OrdinalIgnoreCase))
                    profile.Settings = _LoadSettings(node, profile.Type);
            }

            return profile;
        }

        /// <summary>
        /// Loads <c>ImportProfile</c> from file.
        /// </summary>
        /// <param name="nodeProfiles">Profiles node.</param>
        /// <exception cref="T:System.NotSupportedException.NotSupportedException">
        /// XML file is invalid.</exception>
        private void _LoadContent(XmlElement nodeProfiles)
        {
            List<ImportProfile> profiles = Profiles;
            foreach (XmlNode node in nodeProfiles.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_IMPORTS_PROFILE, StringComparison.OrdinalIgnoreCase))
                {
                    ImportProfile profile = _LoadProfile(node);
                    if (null != profile)
                        profiles.Add(profile);
                }
                // else Do nothing
            }
        }

        private void _SaveFieldsMap(List<FieldMap> fieldsMap, XmlWriter writer)
        {
            writer.WriteStartElement(NODE_NAME_FIELDSMAPPING);
            foreach (FieldMap fieldMap in fieldsMap)
            {
                writer.WriteStartElement(NODE_NAME_FIELDSMAP);
                writer.WriteAttributeString(ATTRIBUTE_NAME_SRCFIELD, fieldMap.SourceFieldName);
                writer.WriteAttributeString(ATTRIBUTE_NAME_DSTFIELD, fieldMap.ObjectFieldName);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void _SaveProfileSettings(ImportSettings settings, XmlWriter writer)
        {
            writer.WriteStartElement(NODE_NAME_SETTINGS);
              writer.WriteStartElement(NODE_NAME_SOURCE);
              writer.WriteValue(settings.Source);
              writer.WriteEndElement();

              writer.WriteStartElement(NODE_NAME_TABLE);
              writer.WriteAttributeString(ATTRIBUTE_NAME_NAME, settings.TableName);
              writer.WriteEndElement();

              _SaveFieldsMap(settings.FieldsMap, writer);
            writer.WriteEndElement();
        }

        private void _SaveContent(XmlWriter writer)
        {
            // write profiles
            writer.WriteStartElement(NODE_NAME_IMPORTS_PROFILES);
              foreach (ImportProfile profile in Profiles)
              {   // write profile parameters
                  if (!profile.IsOnTime)
                      continue; // skip old version profiles

                  writer.WriteStartElement(NODE_NAME_IMPORTS_PROFILE);
                  writer.WriteAttributeString(ATTRIBUTE_NAME_NAME, profile.Name);
                  writer.WriteAttributeString(ATTRIBUTE_NAME_TYPE, profile.Type.ToString());
                  writer.WriteAttributeString(ATTRIBUTE_NAME_DEFAULT, false.ToString());
                  //writer.WriteAttributeString(ATTRIBUTE_NAME_DEFAULT, profile.IsDefault.ToString());
                    // NOTE: ignored in this version - it is left to backward compatibility
                  writer.WriteAttributeString(ATTRIBUTE_NAME_ONTIME, profile.IsOnTime.ToString());
                    writer.WriteStartElement(NODE_NAME_DESCRIPTION);
                    writer.WriteValue(profile.Description);
                    writer.WriteEndElement();

                    _SaveProfileSettings(profile.Settings, writer);
                  writer.WriteEndElement();
              }

            writer.WriteEndElement();
        }

        public bool _CreateFile(string fileFullName)
        {
            if (null == Profiles)
                return false;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = CommonHelpers.XML_SETTINGS_INDENT_CHARS;

            bool bOk = false;
            XmlWriter writer = null;
            try
            {
                writer = XmlWriter.Create(fileFullName, settings);
                _SaveContent(writer);
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

        private void _CreateAliases()
        {
            Debug.Assert(null == _aliases); // only once
            _aliases = new StringDictionary();

            App app = App.Current;
            _aliases.Add(app.FindString("ImportAliasesPropertyOrderNumber"),
                         app.FindString("ImportAliasesOrderNumber"));

            _aliases.Add(app.FindString("ImportAliasesPropertyPlannedDate"),
                         app.FindString("ImportAliasesPlannedDate"));

            _aliases.Add(app.FindString("ImportAliasesPropertyCustomerNumber"),
                         app.FindString("ImportAliasesCustomerNumber"));

            _aliases.Add(app.FindString("ImportAliasesPropertyName"),
                         app.FindString("ImportAliasesName"));

            _aliases.Add(app.FindString("ImportAliasesPropertyType"),
                         app.FindString("ImportAliasesType"));

            _aliases.Add(app.FindString("ImportAliasesPropertyOrderType"),
                         app.FindString("ImportAliasesOrderType"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeWindowStart"),
                         app.FindString("ImportAliasesTimeWindowStart"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeWindowFinish"),
                         app.FindString("ImportAliasesTimeWindowFinish"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeWindowStartDay"),
                         app.FindString("ImportAliasesTimeWindowStartDay"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeWindow2Start"),
                         app.FindString("ImportAliasesTimeWindow2Start"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeWindow2Finish"),
                         app.FindString("ImportAliasesTimeWindow2Finish"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeWindow2StartDay"),
                         app.FindString("ImportAliasesTimeWindow2StartDay"));

            _aliases.Add(app.FindString("ImportAliasesPropertyPriority"),
                         app.FindString("ImportAliasesPriority"));

            _aliases.Add(app.FindString("ImportAliasesPropertyServiceTime"),
                         app.FindString("ImportAliasesServiceTime"));

            _aliases.Add(app.FindString("ImportAliasesPropertyVehicleSpecialties"),
                         app.FindString("ImportAliasesVehicleSpecialties"));

            _aliases.Add(app.FindString("ImportAliasesPropertyDriverSpecialties"),
                         app.FindString("ImportAliasesDriverSpecialties"));

            _aliases.Add(app.FindString("ImportAliasesPropertyMaxViolationTime"),
                         app.FindString("ImportAliasesMaxViolationTime"));

            _aliases.Add(app.FindString("ImportAliasesPropertyMaxTimeOnVehicle"),
                         app.FindString("ImportAliasesMaxTimeOnVehicle"));

            _aliases.Add(app.FindString("ImportAliasesPropertyArriveTime"),
                         app.FindString("ImportAliasesArriveTime"));

            _aliases.Add(app.FindString("ImportAliasesPropertyX"),
                         app.FindString("ImportAliasesX"));

            _aliases.Add(app.FindString("ImportAliasesPropertyY"),
                         app.FindString("ImportAliasesY"));

            _aliases.Add(app.FindString("ImportAliasesPropertyComment"),
                         app.FindString("ImportAliasesComment"));

            _aliases.Add(app.FindString("ImportAliasesPropertyActiveSyncProfileName"),
                         app.FindString("ImportAliasesActiveSyncProfileName"));

            _aliases.Add(app.FindString("ImportAliasesPropertyEmailAddress"),
                         app.FindString("ImportAliasesEmailAddress"));

            _aliases.Add(app.FindString("ImportAliasesPropertySyncFolder"),
                         app.FindString("ImportAliasesSyncFolder"));

            _aliases.Add(app.FindString("ImportAliasesPropertySyncType"),
                         app.FindString("ImportAliasesSyncType"));

            _aliases.Add(app.FindString("ImportAliasesPropertyFixedCost"),
                         app.FindString("ImportAliasesFixedCost"));

            _aliases.Add(app.FindString("ImportAliasesPropertyPerHourSalary"),
                         app.FindString("ImportAliasesPerHourSalary"));

            _aliases.Add(app.FindString("ImportAliasesPropertyPerHourOTSalary"),
                         app.FindString("ImportAliasesPerHourOTSalary"));

            _aliases.Add(app.FindString("ImportAliasesPropertyMobileDevice"),
                         app.FindString("ImportAliasesMobileDevice"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeBeforeOT"),
                         app.FindString("ImportAliasesTimeBeforeOT"));

            _aliases.Add(app.FindString("ImportAliasesPropertySpecialties"),
                         app.FindString("ImportAliasesSpecialties"));

            _aliases.Add(app.FindString("ImportAliasesPropertyFuelEconomy"),
                         app.FindString("ImportAliasesFuelEconomy"));

            _aliases.Add(app.FindString("ImportAliasesPropertyFuelType"),
                         app.FindString("ImportAliasesFuelType"));

            _aliases.Add(app.FindString("ImportAliasesPropertyVehicle"),
                         app.FindString("ImportAliasesVehicle"));

            _aliases.Add(app.FindString("ImportAliasesPropertyDriver"),
                         app.FindString("ImportAliasesDriver"));

            _aliases.Add(app.FindString("ImportAliasesPropertyStartTimeWindowStart"),
                         app.FindString("ImportAliasesStartTimeWindowStart"));

            _aliases.Add(app.FindString("ImportAliasesPropertyStartTimeWindowFinish"),
                         app.FindString("ImportAliasesStartTimeWindowFinish"));

            _aliases.Add(app.FindString("ImportAliasesPropertyMaxOrders"),
                         app.FindString("ImportAliasesMaxOrders"));

            _aliases.Add(app.FindString("ImportAliasesPropertyMaxTravelDistance"),
                         app.FindString("ImportAliasesMaxTravelDistance"));

            _aliases.Add(app.FindString("ImportAliasesPropertyMaxTravelDuration"),
                         app.FindString("ImportAliasesMaxTravelDuration"));

            _aliases.Add(app.FindString("ImportAliasesPropertyMaxTotalDuration"),
                         app.FindString("ImportAliasesMaxTotalDuration"));

            _aliases.Add(app.FindString("ImportAliasesPropertyStartDepot"),
                         app.FindString("ImportAliasesStartDepot"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeAtStart"),
                         app.FindString("ImportAliasesTimeAtStart"));

            _aliases.Add(app.FindString("ImportAliasesPropertyEndDepot"),
                         app.FindString("ImportAliasesEndDepot"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeAtEnd"),
                         app.FindString("ImportAliasesTimeAtEnd"));

            _aliases.Add(app.FindString("ImportAliasesPropertyRenewalDepots"),
                         app.FindString("ImportAliasesRenewalDepots"));

            _aliases.Add(app.FindString("ImportAliasesPropertyTimeAtRenewal"),
                         app.FindString("ImportAliasesTimeAtRenewal"));

            _aliases.Add(app.FindString("ImportAliasesPropertyColor"),
                         app.FindString("ImportAliasesColor"));

            _aliases.Add(app.FindString("ImportAliasesPropertyZones"),
                         app.FindString("ImportAliasesZones"));
        }

        private bool _IsDefaultOnePerType()
        {
            foreach (ImportType type in EnumHelpers.GetValues<ImportType>())
            {
                bool isDefaultFounded = false;
                foreach (ImportProfile profile in Profiles)
                {
                    if (profile.IsDefault)
                    {
                        if (profile.Type == type)
                        {
                            if (isDefaultFounded)
                                return false; // founded secondary default profile for this type

                            isDefaultFounded = true;
                        }
                    }
                }
            }

            return true;
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string NODE_NAME_IMPORTS_PROFILES = "ImportProfiles";
        private const string NODE_NAME_IMPORTS_PROFILE = "ImportProfile";
        private const string NODE_NAME_DESCRIPTION = "Description";
        private const string NODE_NAME_SETTINGS = "Settings";
        private const string NODE_NAME_SOURCE = "Source";
        private const string NODE_NAME_TABLE = "Table";
        private const string NODE_NAME_FIELDSMAPPING = "FieldsMapping";
        private const string NODE_NAME_FIELDSMAP = "FieldsMap";

        private const string ATTRIBUTE_NAME_NAME = "Name";
        private const string ATTRIBUTE_NAME_TYPE = "Type";
        private const string ATTRIBUTE_NAME_DEFAULT = "Default";
        private const string ATTRIBUTE_NAME_ONTIME = "OnTime";
        private const string ATTRIBUTE_NAME_SRCFIELD = "SourceField";
        private const string ATTRIBUTE_NAME_DSTFIELD = "DestinationField";

        private const char SPLITTER_GROUPS = ';';
        private const char SPLITTER_GROUP = ':';

        #endregion // Private constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<ImportProfile> _profiles = new List<ImportProfile>();
        private StringDictionary _aliases = null;
        private string _fileName = null;

        #endregion // Private members
    }
}
