using System;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections;
using System.IO;
using System.Xml;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Custom settings provider.
    /// </summary>
    internal class AlSettingsProvider : SettingsProvider
    {
        #region SettingsProvider overrides
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string ApplicationName
        {
            get { return "ArcLogistics"; }
            set { }
        }

        public override void Initialize(string name, NameValueCollection col)
        {
            base.Initialize(this.ApplicationName, col);
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection propvals)
        {
            lock (_locker)
            {
                // iterate through the settings to be stored
                foreach (SettingsPropertyValue propval in propvals)
                {
                    if (propval.IsDirty)
                    {
                        // set only user-scoped settings
                        if (_IsUserScoped(propval.Property))
                            _SetValue(propval);
                    }
                }

                _Save();
            }
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection props)
        {
            lock (_locker)
            {
                // create new collection of values
                SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

                // iterate through the settings to be retrieved
                foreach (SettingsProperty prop in props)
                {
                    SettingsPropertyValue value = new SettingsPropertyValue(prop);
                    value.SerializedValue = _GetValue(prop);
                    value.IsDirty = false;
                    values.Add(value);
                }

                return values;
            }
        }

        #endregion SettingsProvider overrides

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _SetValue(SettingsPropertyValue propval)
        {
            try
            {
                _store.SetValue(propval);
            }
            catch (Exception e)
            {
                // MSDN: If the provider cannot fulfill the request, it should ignore it silently.
                Logger.Error(e);
            }
        }

        private object _GetValue(SettingsProperty prop)
        {
            object value = null;
            try
            {
                value = _store.GetValue(prop);
            }
            catch (Exception e)
            {
                // MSDN: If the provider cannot fulfill the request, it should ignore it silently.
                Logger.Error(e);
            }

            return value;
        }

        private void _Save()
        {
            try
            {
                _store.Save();
            }
            catch (Exception e)
            {
                // MSDN: If the provider cannot fulfill the request, it should ignore it silently.
                Logger.Error(e);
            }
        }

        private static bool _IsUserScoped(SettingsProperty prop)
        {
            foreach (DictionaryEntry entry in prop.Attributes)
            {
                Attribute attr = (Attribute)entry.Value;
                if (attr.GetType() == typeof(UserScopedSettingAttribute))
                    return true;
            }

            return false;
        }

        #endregion private methods

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private SettingsStore _store = new SettingsStore();
        private object _locker = new object(); // mt

        #endregion private fields

        #region private classes
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Settings storage.
        /// </summary>
        internal class SettingsStore
        {
            #region constants
            ///////////////////////////////////////////////////////////////////////////////////////////

            private const string CONF_FILE_NAME = "user.config";

            private const string NODE_ROOT = "configuration";
            private const string NODE_USER_SETTINGS = "userSettings";
            private const string NODE_SETTINGS = "ESRI.ArcLogistics.App.Properties.Settings";
            private const string NODE_SETTING = "setting";
            private const string NODE_VALUE = "value";

            private const string ATTR_NAME = "name";
            private const string ATTR_SERIALIZE = "serializeAs";

            private static readonly string XPATH_SETTINGS = NODE_ROOT + '/' +
                NODE_USER_SETTINGS + '/' +
                NODE_SETTINGS;

            private static readonly string XPATH_SETTING_BY_NAME = "descendant::" +
                NODE_ROOT + '/' +
                NODE_USER_SETTINGS + '/' +
                NODE_SETTINGS + '/' +
                NODE_SETTING +
                "[@name='{0}']";

            #endregion constants
            
            #region Constructor

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Settings file is invalid.
            /// In property "Source" there is path to invalid settings file.</exception>
            public SettingsStore()
            {
                _Init();
            }

            #endregion

            #region Public Static Properties

            /// <summary>
            /// Path to user.config file.
            /// </summary>
            public static string PathToConfig   
            {
                get
                {
                    return Path.Combine(DataFolder.Path, CONF_FILE_NAME);
                }
            }

            #endregion

            #region public methods
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            public object GetValue(SettingsProperty prop)
            {
                Debug.Assert(prop != null);

                object value = null;

                string xmlValue = _FindValue(prop.Name);
                if (xmlValue != null)
                {
                    // get stored value
                    value = _GetSerializedValue(prop, xmlValue);
                }
                else
                {
                    // get default value
                    value = prop.DefaultValue;
                }

                return value;
            }

            public void SetValue(SettingsPropertyValue propval)
            {
                Debug.Assert(propval != null);

                bool addSetting = false;

                XmlNode settingNode = _FindSettingNode(propval.Name);
                if (settingNode == null)
                {
                    settingNode = _CreateSettingNode(propval);
                    addSetting = true;
                }

                XmlNode valueNode = settingNode.SelectSingleNode(
                    NODE_VALUE);

                if (valueNode == null)
                {
                    valueNode = _Document.CreateNode(XmlNodeType.Element,
                        NODE_VALUE, "");

                    settingNode.AppendChild(valueNode);
                }

                valueNode.InnerXml = _Serialize(propval);
                _FixValueNode(valueNode);

                if (addSetting)
                    _AddSettingNode(settingNode);

                _isDirty = true;
            }

            public void Save()
            {
                if (_isDirty)
                {
                    _Document.Save(_docPath);
                    _isDirty = false;
                }
            }

            #endregion public methods

            #region private methods
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            private XmlDocument _Document
            {
                get
                {
                    if (_doc == null)
                        _Init();

                    return _doc;
                }
            }

            private XmlNode _FindSettingNode(string settingName)
            {
                return _Document.SelectSingleNode(
                    String.Format(XPATH_SETTING_BY_NAME, settingName));
            }

            private XmlNode _CreateSettingNode(SettingsPropertyValue propval)
            {
                XmlNode node = _Document.CreateNode(XmlNodeType.Element,
                    NODE_SETTING, "");

                XmlAttribute attrName = _Document.CreateAttribute(ATTR_NAME);
                attrName.Value = propval.Name;
                node.Attributes.Append(attrName);

                XmlAttribute attrSerialize = _Document.CreateAttribute(ATTR_SERIALIZE);
                attrSerialize.Value = propval.Property.SerializeAs.ToString();
                node.Attributes.Append(attrSerialize);

                return node;
            }

            private void _AddSettingNode(XmlNode node)
            {
                XmlNode settingsNode = _Document.SelectSingleNode(XPATH_SETTINGS);
                if (settingsNode == null)
                    throw new ConfigurationErrorsException(); // wrong config file scheme

                settingsNode.AppendChild(node);
            }

            private string _FindValue(string settingName)
            {
                string value = null;

                XmlNode settingsNode = _FindSettingNode(settingName);
                if (settingsNode != null)
                {
                    XmlNode valueNode = settingsNode.SelectSingleNode(NODE_VALUE);
                    if (valueNode != null)
                        value = valueNode.InnerXml;
                }

                return value;
            }
            
            /// <summary>
            /// Initialize user config.
            /// </summary>
            /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Settings file is invalid.
            /// In property "Source" there is path to invalid settings file.</exception>
            private void _Init()
            {
                _doc = _LoadDoc(PathToConfig);
                _docPath = PathToConfig;
            }

            private string _Serialize(SettingsPropertyValue propval)
            {
                Debug.Assert(propval != null);

                string value = propval.SerializedValue as string;
                if (value == null)
                    value = String.Empty;

                if (propval.Property.SerializeAs == SettingsSerializeAs.String)
                {
                    value = _escaper.Escape(value);
                }
                else if (propval.Property.SerializeAs == SettingsSerializeAs.Binary)
                {
                    byte[] buf = propval.SerializedValue as byte[];
                    if (buf != null)
                        value = Convert.ToBase64String(buf);
                }

                return value;
            }

            private string _GetSerializedValue(SettingsProperty prop, string xmlValue)
            {
                Debug.Assert(prop != null);
                Debug.Assert(xmlValue != null);

                string value = xmlValue;
                if (prop.SerializeAs == SettingsSerializeAs.String)
                    value = _escaper.Unescape(xmlValue);

                return value;
            }

            /// <summary>
            /// Load user settings from file.
            /// </summary>
            /// <param name="configPath">Path to settings file.</param>
            /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Settings file is invalid.
            /// In property "Source" there is path to invalid settings file.</exception>
            private static XmlDocument _LoadDoc(string path)
            {
                XmlDocument doc = new XmlDocument();

                if (File.Exists(path))
                {
                    // Try to load file.
                    try
                    {
                        doc.Load(path);
                    }
                    catch (XmlException ex)
                    {
                        // If XML corrupted - wrap exception and save path to file.
                        SettingsException settingsException = new SettingsException(ex.Message, ex);
                        settingsException.Source = path;
                        throw settingsException;
                    }
                }
                else
                {
                    _InitDocStructure(doc);
                    doc.Save(path);
                }

                return doc;
            }

            private static void _InitDocStructure(XmlDocument doc)
            {
                XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "utf-8", "");
                doc.AppendChild(dec);

                XmlNode rootNode = doc.CreateNode(XmlNodeType.Element, NODE_ROOT, "");
                doc.AppendChild(rootNode);

                XmlNode setTypeNode = doc.CreateNode(XmlNodeType.Element, NODE_USER_SETTINGS, "");
                rootNode.AppendChild(setTypeNode);

                XmlNode settingsNode = doc.CreateNode(XmlNodeType.Element, NODE_SETTINGS, "");
                setTypeNode.AppendChild(settingsNode);
            }

            private static void _FixValueNode(XmlNode valueNode)
            {
                // remove declaration node if exists
                XmlNode nodeToRemove = null;
                foreach (XmlNode child in valueNode.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        nodeToRemove = child;
                        break;
                    }
                }

                if (nodeToRemove != null)
                    valueNode.RemoveChild(nodeToRemove);
            }

            #endregion private methods

            #region private fields
            ///////////////////////////////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////////

            private XmlDocument _doc;
            private string _docPath;
            private XmlEscaper _escaper = new XmlEscaper();
            private bool _isDirty = false;

            #endregion private fields

            /// <summary>
            /// XmlEscaper class.
            /// </summary>
            private class XmlEscaper
            {
                public XmlEscaper()
                {
                    doc = new XmlDocument();
                    temp = doc.CreateElement("temp");
                }

                public string Escape(string xmlString)
                {
                    if (String.IsNullOrEmpty(xmlString))
                        return xmlString;

                    temp.InnerText = xmlString;
                    return temp.InnerXml;
                }

                public string Unescape(string escapedString)
                {
                    if (String.IsNullOrEmpty(escapedString))
                        return escapedString;

                    temp.InnerXml = escapedString;
                    return temp.InnerText;
                }

                private XmlDocument doc;
                private XmlElement temp;
            }

        }

        #endregion private classes
    }
}
