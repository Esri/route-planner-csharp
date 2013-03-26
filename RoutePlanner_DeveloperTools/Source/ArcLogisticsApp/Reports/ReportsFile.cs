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
using System.Diagnostics;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Reports
{
    /// <summary>
    /// Context type.
    /// </summary>
    internal enum ContextType
    {
        Schedule, // for schedule
        Route     // for route
    }

    /// <summary>
    /// Class that represents report description.
    /// </summary>
    internal class ReportDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new report template description.
        /// </summary>
        /// <param name="name">Template name.</param>
        /// <param name="templatePath">Template relative path.</param>
        /// <param name="description">Template description text.</param>
        public ReportDescription(string name, string templatePath, string description)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(!string.IsNullOrEmpty(templatePath));

            _name = name;
            _templatePath = templatePath;
            _description = (string.IsNullOrEmpty(description)) ? null : description.Trim();
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of report.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Relative path of template.
        /// </summary>
        public string TemplatePath
        {
            get { return _templatePath; }
            set { _templatePath = value; }
        }

        /// <summary>
        /// Description of template.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name = null;
        private string _templatePath = null;
        private string _description = null;

        #endregion // Private members
    }

    /// <summary>
    /// Class that represents subreport information.
    /// </summary>
    internal class SubReportInfo : ReportDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of SubReportInfo class.
        /// </summary>
        public SubReportInfo(string name, string templatePath, string description,
                             bool isDefault, string groupId, bool isVisible)
            : base(name, templatePath, description)
        {
            _isVisible = isVisible;
            _isDefault = isDefault;
            _groupId = groupId;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is default in group.
        /// </summary>
        public bool IsDefault
        {
            get { return _isDefault; }
        }

        /// <summary>
        /// Group identificator.
        /// </summary>
        public string GroupId
        {
            get { return _groupId; }
        }

        /// <summary>
        /// Is visible in GUI flag.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is default flag (possible one default per group).
        /// </summary>
        private bool _isDefault = false;
        /// <summary>
        /// Group identificator.
        /// </summary>
        private string _groupId = null;
        /// <summary>
        /// Is visible flag (show in GUI).
        /// </summary>
        private bool _isVisible = true;

        #endregion // Private members
    }

    /// <summary>
    /// Class that represents report information.
    /// </summary>
    internal class ReportInfo : ReportDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of ReportInfo class.
        /// </summary>
        public ReportInfo(string name, ContextType context, string templatePath,
                          string description, bool isPredefined, ICollection<SubReportInfo> subReports)
            : base(name, templatePath, description)
        {
            _context = context;
            _isPredefined = isPredefined;
            _subReports = subReports;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Context of report.
        /// </summary>
        public ContextType Context
        {
            get { return _context; }
        }

        /// <summary>
        /// Collection of SubReports.
        /// </summary>
        /// <remarks>Collection is read-only. Can be empty.</remarks>
        public ICollection<SubReportInfo> SubReports
        {
            get { return _subReports; }
        }

        /// <summary>
        /// Is predefined template
        /// </summary>
        public bool IsPredefined
        {
            get { return _isPredefined; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ContextType _context = ContextType.Route;
        private readonly bool _isPredefined = true;
        private ICollection<SubReportInfo> _subReports = null;

        #endregion // Private members
    }

    /// <summary>
    /// Application reports settings.
    /// </summary>
    internal sealed class ReportsFile
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

// REV: comment must specify that exception can be thrown from here.
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileNamePredefined">Report config filename.</param>
        /// <param name="fileNameUser">Users report config filename.</param>
        public ReportsFile(string fileNamePredefined, string fileNameUser)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileNamePredefined));
            Debug.Assert(!string.IsNullOrEmpty(fileNameUser));

            _Load(fileNamePredefined, true);
            _userFileName = fileNameUser;
            _Load(fileNameUser, false);
        }

        #endregion // Constructor

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Stores ReportsFile to file.
        /// </summary>
        public bool Save()
        {
            Debug.Assert(!string.IsNullOrEmpty(_userFileName)); // NOTE: load first
            Debug.Assert(null != ReportInfos); // NOTE: load first

            // rewrite imports file
            string defaultsFolderPath = Path.GetDirectoryName(_userFileName);
            if (!Directory.Exists(defaultsFolderPath))
                Directory.CreateDirectory(defaultsFolderPath);

            if (File.Exists(_userFileName))
                File.Delete(_userFileName);

            return _CreateFile(_userFileName);
        }

        #endregion // Public methods

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Application report template descriptions.
        /// </summary>
        /// <remarks>Call after Load()</remarks>
        public List<ReportInfo> ReportInfos
        {
            get { return _infos; }
        }

        #endregion // Public properties

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads sub reports.
        /// </summary>
        /// <param name="nodeReport">Report node.</param>
        /// <returns>Readed subreports collection.</returns>
        private ICollection<SubReportInfo> _LoadSubReports(XmlNode nodeReport)
        {
            List<SubReportInfo> subReports = new List<SubReportInfo> ();
            foreach (XmlNode node in nodeReport.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (!node.Name.Equals(NODE_NAME_SUBREPORTS, StringComparison.OrdinalIgnoreCase))
                    continue; // NOTE: skip not supported

                foreach (XmlNode subReportNode in node.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                        continue; // skip comments and other non element nodes

                    if (subReportNode.Name.Equals(NODE_NAME_SUBREPORT, StringComparison.OrdinalIgnoreCase))
                    {
                        string template = subReportNode.Attributes[ATTRIBUTE_NAME_FILEPATH].Value;

                        if (File.Exists(ReportsGenerator.GetTemplateAbsolutelyPath(template)))
                        {
                            string name = subReportNode.Attributes[ATTRIBUTE_NAME_NAME].Value;
                            bool isDefault = false;
                            if (null != subReportNode.Attributes[ATTRIBUTE_NAME_DEFAULT])
                                isDefault = bool.Parse(subReportNode.Attributes[ATTRIBUTE_NAME_DEFAULT].Value);
                            string groupId = null;
                            if (null != subReportNode.Attributes[ATTRIBUTE_NAME_GROUPID])
                                groupId = subReportNode.Attributes[ATTRIBUTE_NAME_GROUPID].Value;
                            bool isVisible = true; // default is visible state
                            if (null != subReportNode.Attributes[ATTRIBUTE_NAME_VISIBLE])
                                isVisible = bool.Parse(subReportNode.Attributes[ATTRIBUTE_NAME_VISIBLE].Value);

                            string description = _LoadDescription(subReportNode);
                            SubReportInfo subInfo = new SubReportInfo(name, template, description,
                                                                      isDefault, groupId, isVisible);
                            subReports.Add(subInfo);
                        }
                    }
                    else
                    {
                        Debug.Assert(false); // NOTE: not supported
                    }
                }
            }

            return subReports.AsReadOnly();
        }

        /// <summary>
        /// Load report description.
        /// </summary>
        /// <param name="nodeTemplate">Report node.</param>
        /// <returns>Description text or null if absent.</returns>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Reports file is invalid.
        /// In property "Source" there is path to invalid reports file.</exception>
        private string _LoadDescription(XmlNode nodeTemplate)
        {
            string result = null;
            foreach (XmlNode node in nodeTemplate.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (!node.Name.Equals(NODE_NAME_DESCRIPTION, StringComparison.OrdinalIgnoreCase))
                    continue; // skip other nodes

                result = node.InnerText;
                break; // work done
            }

            return result;
        }

        /// <summary>
        /// Loads report template.
        /// </summary>
        /// <param name="nodeReport">Report template node.</param>
        /// <param name="isPredefined">Is predefined flag.</param>
        /// <returns>Report template info.</returns>
        private ReportInfo _LoadTemplate(XmlNode nodeReport, bool isPredefined)
        {
            ReportInfo info = null;
            try
            {
                string template = nodeReport.Attributes[ATTRIBUTE_NAME_FILEPATH].Value;
                if (File.Exists(ReportsGenerator.GetTemplateAbsolutelyPath(template)))
                {
                    string name = nodeReport.Attributes[ATTRIBUTE_NAME_NAME].Value;
                    ContextType context = (ContextType)Enum.Parse(typeof(ContextType), nodeReport.Attributes[ATTRIBUTE_NAME_CONTEXT].Value, true);
                    string description = _LoadDescription(nodeReport);
                    info = new ReportInfo(name, context, template, description, isPredefined,
                                          _LoadSubReports(nodeReport));
                }
            }
            catch
            {
                info = null;
            }

            return info;
        }

        /// <summary>
        /// Loads report templates.
        /// </summary>
        /// <param name="nodeTemplates">Templates node.</param>
        /// <param name="isPredefined">Is predefined flag.</param>
        private void _LoadTemplates(XmlNode nodeTemplates, bool isPredefined)
        {
            foreach (XmlNode node in nodeTemplates.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                if (node.Name.Equals(NODE_NAME_REPORT, StringComparison.OrdinalIgnoreCase))
                {
                    ReportInfo info = _LoadTemplate(node, isPredefined);
                    if (null != info)
                        _infos.Add(info);
                }
                else
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Loads settings file content.
        /// </summary>
        /// <param name="nodeReports">Report templates node.</param>
        /// <param name="isPredefined">Is predefined flag.</param>
        /// <exception cref="T:System.NotSupportedException.NotSupportedException">
        /// XML file is invalid.</exception>
        private void _LoadContent(XmlNode nodeReports, bool isPredefined)
        {
            foreach (XmlNode node in nodeReports.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue; // skip comments and other non element nodes

                else if (node.Name.Equals(NODE_NAME_TEMPLATES, StringComparison.OrdinalIgnoreCase))
                    _LoadTemplates(node, isPredefined);

                else if (node.Name.Equals(NODE_NAME_SETTINGS, StringComparison.OrdinalIgnoreCase))
                {   // Do nothing - don't touch - support old version
                }
                else
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Loads settings file.
        /// </summary>
        /// <param name="fileName">File full name to load.</param>
        /// <param name="isPredefined">Is predefined flag.</param>
        /// <exception cref="T:ESRI.ArcLogistics.SettingsException"> Reports file is invalid.
        /// In property "Source" there is path to invalid reports file.</exception>
        private void _Load(string fileName, bool isPredefined)
        {
            if (!File.Exists(fileName))
                return;

            XmlDocument doc = new XmlDocument();
            // Try to load file.
            try
            {
                doc.Load(fileName);
                _LoadContent(doc.DocumentElement, isPredefined);
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

        /// <summary>
        /// Creates settings file.
        /// </summary>
        /// <param name="fileFullName">Full file name to store.</param>
        /// <returns>TRUE if operation successed.</returns>
        public bool _CreateFile(string fileFullName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileFullName));

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

        /// <summary>
        /// Saves report templates settings.
        /// </summary>
        /// <param name="writer">Writer object.</param>
        private void _SaveContent(XmlWriter writer)
        {
            writer.WriteStartElement(NODE_NAME_REPORTS);
              // write templates
              writer.WriteStartElement(NODE_NAME_TEMPLATES);
                foreach (ReportInfo info in ReportInfos)
                { // write info
                    if (!info.IsPredefined)
                    {   // NOTE: save only user's repors
                        writer.WriteStartElement(NODE_NAME_REPORT);
                          writer.WriteAttributeString(ATTRIBUTE_NAME_NAME, info.Name);
                          writer.WriteAttributeString(ATTRIBUTE_NAME_CONTEXT, info.Context.ToString());
                          writer.WriteAttributeString(ATTRIBUTE_NAME_FILEPATH, info.TemplatePath);

                          if (!string.IsNullOrEmpty(info.Description))
                          {
                              writer.WriteStartElement(NODE_NAME_DESCRIPTION);
                                writer.WriteString(info.Description);
                              writer.WriteEndElement();
                          }

                          // write subreports
                          if (0 < info.SubReports.Count)
                          {
                              writer.WriteStartElement(NODE_NAME_SUBREPORTS);
                              foreach (SubReportInfo subInfo in info.SubReports)
                              {
                                  writer.WriteStartElement(NODE_NAME_SUBREPORT);
                                    writer.WriteAttributeString(ATTRIBUTE_NAME_NAME, subInfo.Name);
                                    writer.WriteAttributeString(ATTRIBUTE_NAME_FILEPATH, subInfo.TemplatePath);
                                    writer.WriteAttributeString(ATTRIBUTE_NAME_DEFAULT, subInfo.IsDefault.ToString());

                                    if (!string.IsNullOrEmpty(subInfo.GroupId))
                                        writer.WriteAttributeString(ATTRIBUTE_NAME_GROUPID, subInfo.GroupId);

                                    writer.WriteAttributeString(ATTRIBUTE_NAME_VISIBLE, subInfo.IsVisible.ToString());

                                    if (!string.IsNullOrEmpty(subInfo.Description))
                                    {
                                        writer.WriteStartElement(NODE_NAME_DESCRIPTION);
                                        writer.WriteString(subInfo.Description);
                                        writer.WriteEndElement();
                                    }
                                  writer.WriteEndElement();
                              }
                              writer.WriteEndElement();
                          }
                        writer.WriteEndElement();
                    }
                }
              writer.WriteEndElement();
            writer.WriteEndElement();
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private List<ReportInfo> _infos = new List<ReportInfo> ();
        private string _userFileName = null; // NOTE: update only users report info

        #endregion // Private members

        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // XML tags.
        private const string NODE_NAME_REPORTS = "Reports";
        private const string NODE_NAME_TEMPLATES = "Templates";
        private const string NODE_NAME_REPORT = "Report";
        private const string NODE_NAME_SUBREPORTS = "SubReports";
        private const string NODE_NAME_SUBREPORT = "SubReport";
        private const string NODE_NAME_SETTINGS = "Settings";
        private const string NODE_NAME_DESCRIPTION = "Description";

        private const string ATTRIBUTE_NAME_NAME = "Name";
        private const string ATTRIBUTE_NAME_CONTEXT = "Context";
        private const string ATTRIBUTE_NAME_FILEPATH = "RPXFilePath";
        private const string ATTRIBUTE_NAME_LEVEL = "Level";
        private const string ATTRIBUTE_NAME_FILE = "File";
        private const string ATTRIBUTE_NAME_DEFAULT = "Default";
        private const string ATTRIBUTE_NAME_GROUPID = "GroupID";
        private const string ATTRIBUTE_NAME_VISIBLE = "Visible";

        #endregion // Constants
    }
}
