using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;

using ESRI.ArcLogistics;
using ESRI.ArcLogistics.Export;
using ESRI.ArcLogistics.DomainObjects;

using DataDynamics.ActiveReports;

namespace ESRI.ArcLogistics.App.Reports
{
    /// <summary>
    /// Application report generator class.
    /// </summary>
    internal sealed class ReportsGenerator
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>ReportsGenerator</c> class.
        /// </summary>
        /// <param name="exporter">Data exporter.</param>
        /// <param name="reports">Reports template settings keeper.</param>
        public ReportsGenerator(Exporter exporter, ReportsFile reports)
        {
            Debug.Assert(null != exporter);
            Debug.Assert(null != reports);

            _exporter = exporter;
            _reports = reports;
        }

        #endregion // Constructors

        #region Static public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets new template path.
        /// </summary>
        /// <param name="newReportName">New report name.</param>
        /// <param name="previousTemplatePath">Previous template path (can be null).</param>
        /// <returns>New report template path.</returns>
        static public string GetNewTemplatePath(string newReportName, string previousTemplatePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(newReportName));

            string templatePath = newReportName;
            if (!string.IsNullOrEmpty(previousTemplatePath))
            {
                try
                {
                    string templateName = newReportName + Path.GetExtension(previousTemplatePath);
                    string dir = Path.GetDirectoryName(previousTemplatePath);
                    templatePath = Path.Combine(dir, templateName);
                }
                catch
                {
                }
            }

            return templatePath;
        }

        /// <summary>
        /// Gets template absolutely path.
        /// </summary>
        /// <param name="relativeTemplatPath">Relative template path.</param>
        /// <returns>Template absolutely path.</returns>
        static public string GetTemplateAbsolutelyPath(string relativeTemplatPath)
        {
            Debug.Assert(!string.IsNullOrEmpty(relativeTemplatPath));
            return Path.Combine(DataFolder.Path, relativeTemplatPath);
        }

        /// <summary>
        /// Gets temp directory name.
        /// </summary>
        /// <returns>Temp directory name.</returns>
        static public string GetTempDirectoryName()
        {
            return Path.Combine(Path.GetTempPath(), RELATIVE_TEMP_DIRECTORY);
        }

        /// <summary>
        /// Gets temp full file name.
        /// </summary>
        /// <returns>Temp full file name.</returns>
        static public string GetTempFullFileName()
        {
            return Path.Combine(GetTempDirectoryName(), Path.GetRandomFileName());
        }

        #endregion // Static public methods

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        #region Public methods. Internal state
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets presented report template names.
        /// </summary>
        /// <param name="withSubReports">With sub reports name flag. If True generated collection
        /// with all report template names in application.</param>
        /// <returns>Presented report template names (read-only).</returns>
        public ICollection<string> GetPresentedNames(bool withSubReports)
        {
            var names = new List<string>();
            foreach (ReportInfo info in _reports.ReportInfos)
            {
                names.Add(info.Name);

                if (withSubReports)
                {
                    foreach (SubReportInfo subInfo in info.SubReports)
                        names.Add(subInfo.Name);
                }
            }

            return names.AsReadOnly();
        }

        /// <summary>
        /// Gets report template info by name.
        /// </summary>
        /// <param name="name">Report template name.</param>
        /// <returns>Selected report info or null.</returns>
        public ReportInfo GetReportInfo(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            ReportInfo info = null;
            foreach (ReportInfo currentInfo in _reports.ReportInfos)
            {
                if (name == currentInfo.Name)
                {
                    info = currentInfo;
                    break; // result founded
                }
            }

            return info;
        }

        /// <summary>
        /// Adds report template info.
        /// </summary>
        /// <param name="info">New info to adding.</param>
        public void AddReportInfo(ReportInfo info)
        {
            Debug.Assert(null != info);

            // check name
            foreach (ReportInfo currentInfo in _reports.ReportInfos)
            {
                if (info.Name == currentInfo.Name)
                {
                    string text = App.Current.FindString("ReportTemplateNotUniqueName");
                    throw new ArgumentException(text); // exception
                }
            }

            // check file template present
            string reportTemplatePath = GetTemplateAbsolutelyPath(info.TemplatePath);
            if (!File.Exists(reportTemplatePath))
                throw new ArgumentException(); // exception

            _reports.ReportInfos.Add(info);
        }

        /// <summary>
        /// Deletes report template info by name.
        /// </summary>
        /// <param name="name">Report name.</param>
        public void DeleteReportInfo(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            ReportInfo info = GetReportInfo(name);
            if (null != info)
                _reports.ReportInfos.Remove(info);
        }

        /// <summary>
        /// Stores changes in report template collection.
        /// </summary>
        public void StoreChanges()
        {
            _reports.Save();
        }

        /// <summary>
        /// Checks report enforce splitted.
        /// </summary>
        /// <param name="reportInfo">Report template info to check.</param>
        /// <returns>TRUE if splitted needed.</returns>
        public bool IsReportEnforceSplitted(ReportInfo reportInfo)
        {
            Debug.Assert(null != reportInfo);

            bool isHardFieldPresent = false;
            ICollection<ReportInfo> reports =
                CommonHelpers.CreateCollectionWithOneObject(reportInfo);
            ICollection<string> extendedFields = GetReportsExtendedFields(reports);
            foreach (string hardField in _exporter.HardFields)
            {
                isHardFieldPresent = extendedFields.Contains(hardField);
                if (isHardFieldPresent)
                    break; // result founded
            }

            return isHardFieldPresent;
        }

        /// <summary>
        /// Gets extended fiels names.
        /// </summary>
        /// <param name="reportInfos">Report template infos to selection.</param>
        /// <returns>Read-only collection of exteneded field names.</returns>
        public ICollection<string> GetReportsExtendedFields(ICollection<ReportInfo> reportInfos)
        {
            Debug.Assert(null != reportInfos);

            var extendedFields = new List<string>();
            foreach (ReportInfo info in reportInfos)
                _GetReportExtendedFields(info, ref extendedFields);

            return extendedFields.AsReadOnly();
        }

        #endregion // Public methods. Internal state

        #region Public methods. Reports processing procedures
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Raises when asynchronous create route operation completed.
        /// </summary>
        public event CreateReportsCompletedEventHandler CreateReportsCompleted;

        /// <summary>
        /// Creates reports for all routes in selected scdedules (Asynchronous).
        /// </summary>
        /// <param name="reportInfos">Report template info to creation.</param>
        /// <param name="schedules">Schedules for data reports.</param>
        /// <param name="routes">Selected routes from schedule (can be null).</param>
        /// <returns>List of created reports.</returns>
        /// <remarks>Routes must belong to the <c>schedule</c>. If <c>routes</c> collection
        /// is empty, Generator will use all the routes from the <c>schedules</c>.</remarks>
        public void CreateReportsAsync(IDictionary<string, ReportInfo> reportInfos,
                                       ICollection<Schedule> schedules,
                                       ICollection<Route> routes)
        {
            Debug.Assert(null != reportInfos);
            Debug.Assert(null != schedules);

            Debug.Assert(null == _reportInfos);
            Debug.Assert(string.IsNullOrEmpty(_sourceFileName));

            Debug.Assert(null != _exporter);
            Debug.Assert(null != reportInfos);
            Debug.Assert(null != schedules);

            ICollection<string> extendedFields = GetReportsExtendedFields(reportInfos.Values);

            // store state
            _reportInfos = reportInfos;
            _sourceFileName = _PrepareSourceFilePath();

            _exporter.AsyncExportCompleted +=
                new AsyncExportCompletedEventHandler(_exporter_AsyncExportCompleted);

            try
            {
                ExportOptions options = new ExportOptions();
                options.ShowLeadingStemTime = App.Current.MapDisplay.ShowLeadingStemTime;
                options.ShowTrailingStemTime = App.Current.MapDisplay.ShowTrailingStemTime;

                // start process
                _exporter.DoReportSourceAsync(_sourceFileName,
                                              schedules,
                                              extendedFields,
                                              routes,
                                              CommonHelpers.GetCurrentLayer(),
                                              options);
            }
            catch
            {
                _ClearState();

                throw;
            }
        }

        /// <summary>
        /// Stops asynchronous report source creating.
        /// </summary>
        public void AbortCreateReports()
        {
            Debug.Assert(null != _exporter);

            _exporter.AbortExport();
        }

        /// <summary>
        /// Runs report core to real report creation (call after <c>CreateReportsAsync</c>).
        /// </summary>
        /// <param name="description">Created report description.</param>
        public void RunReport(ReportStateDescription description)
        {
            Debug.Assert(null != description);
            Debug.Assert(null == description.Report);

            using (WorkingStatusHelper.EnterBusyState(null))
            {
                try
                {
                    // load report structure
                    var rpt = _LoadReportStructure(description.ReportInfo.TemplatePath);

                    // update connection string
                    var ds = (DataDynamics.ActiveReports.DataSources.OleDBDataSource)rpt.DataSource;
                    ds.ConnectionString = _UpdateConnectionString(description.SourceFilePath,
                                                                  ds.ConnectionString);
                    string connectionString = ds.ConnectionString;

                    // set sub reports
                    Section section = rpt.Sections[REPORT_SECTION_NAME_DETAIL];
                    _InitSubReports(description.ReportInfo.SubReports, connectionString, section);

                    // run report building
                    rpt.Document.Name = description.ReportName;
                    rpt.Run();

                    description.Report = rpt;

                    GC.Collect();
                    GC.WaitForFullGCApproach();
                }
                catch
                {
                    DisposeReport(description);

                    throw; // exception
                }
            }
        }

        /// <summary>
        /// Disposes all report core resources.
        /// </summary>
        /// <param name="description">Created report description.</param>
        public void DisposeReport(ReportStateDescription description)
        {
            Debug.Assert(null != description);

            ActiveReport3 rpt = description.Report;
            if (null != rpt)
            {
                description.Report = null;
                _DisposeReport(ref rpt);

                GC.Collect();
                GC.WaitForFullGCApproach();
            }
        }

        #endregion // Public methods. Reports processing procedures

        #endregion // Public methods

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Clears internal state.
        /// </summary>
        private void _ClearState()
        {
            if (null != _reportInfos)
            {
                _reportInfos.Clear();
                _reportInfos = null;
            }
            _sourceFileName = null;
        }

        /// <summary>
        /// Gets report data source file path.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <returns>Report data source file full path.</returns>
        private string _GetSourceFilePath(string fileName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileName));

            string tmpFileName = Path.Combine(GetTempDirectoryName(), fileName);
            tmpFileName += DATASOURCE_FILE_EXT;
            return tmpFileName;
        }

        /// <summary>
        /// Updates connection string to real source.
        /// </summary>
        /// <param name="sourceFilePath">Current source file full name.</param>
        /// <param name="connectionString">Defined connection string.</param>
        /// <returns>Real connection string.</returns>
        private string _UpdateConnectionString(string sourceFilePath, string connectionString)
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceFilePath));
            Debug.Assert(!string.IsNullOrEmpty(connectionString));

            int start = connectionString.IndexOf(DATA_SOURCE) + DATA_SOURCE.Length;
            int end = connectionString.IndexOf(DATA_SOURCE_END_SYMBOL, start);
            Debug.Assert((0 <= start) && (start < end));

            string defaultDataSource = connectionString.Substring(start, end - start);
            string realConnectionString = connectionString.Replace(defaultDataSource,
                                                                   sourceFilePath);
            return realConnectionString;
        }

        /// <summary>
        /// Loads report structure.
        /// </summary>
        /// <param name="templatePath">Report template path.</param>
        /// <returns>Created and inited report core.</returns>
        private ActiveReport3 _LoadReportStructure(string templatePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(templatePath));

            // load report structure
            var rpt = new ActiveReport3();
            rpt.Document.CacheToDisk = true;
            rpt.Document.CacheToDiskLocation = GetTempFullFileName();

            string filePath =
                ReportsGenerator.GetTemplateAbsolutelyPath(templatePath);
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                rpt.LoadLayout(fs);

            return rpt;
        }

        /// <summary>
        /// Initiates sub.reports.
        /// </summary>
        /// <param name="subReports">Sub report descriptions.</param>
        /// <param name="connectionString">Source connection string.</param>
        /// <param name="subSection">Master report section for subreports.</param>
        private void _InitSubReports(ICollection<SubReportInfo> subReports,
                                     string connectionString,
                                     Section subSection)
        {
            Debug.Assert(null != subReports);
            Debug.Assert(!string.IsNullOrEmpty(connectionString));
            Debug.Assert(null != subSection);

            int controlStartIndex = 0;
            foreach (SubReportInfo info in subReports)
            {
                // load report
                var report = _LoadReportStructure(info.TemplatePath);
                var subDS =
                    (DataDynamics.ActiveReports.DataSources.OleDBDataSource)report.DataSource;

                // update connection string
                subDS.ConnectionString = connectionString;

                // set subreport to free control in to master report
                Debug.Assert(controlStartIndex < subSection.Controls.Count);
                for (int controlIndex = controlStartIndex;
                     controlIndex < subSection.Controls.Count;
                     ++controlIndex)
                {
                    bool isReportSet = false;
                    var subReportCtrl = subSection.Controls[controlIndex] as SubReport;
                    if (null != subReportCtrl)
                    {
                        subReportCtrl.Report = report;
                        isReportSet = true;
                    }

                    ++controlStartIndex;
                    if (isReportSet)
                        break; // do next sub report
                }
            }
        }

        /// <summary>
        /// Freeses all report core resources.
        /// </summary>
        /// <param name="report">Active report object.</param>
        private void _FreeReport(ref ActiveReport3 report)
        {
            Debug.Assert(null != report);

            report.Document.Dispose();
            report.Dispose();
            report = null;
        }

        /// <summary>
        /// Disposes all report core resources.
        /// </summary>
        /// <param name="report">Active report object.</param>
        private void _DisposeReport(ref ActiveReport3 report)
        {
            Debug.Assert(null != report);

            // dispose subreports
            for (int sectionIndex = 0; sectionIndex < report.Sections.Count; ++sectionIndex)
            {
                // find and dispose all subReports - one by one (safely)
                try
                {
                    Section section = report.Sections[sectionIndex];
                    for (int controlIndex = 0; controlIndex < section.Controls.Count; ++controlIndex)
                    {
                        var subReport = section.Controls[controlIndex] as SubReport;
                        if (null != subReport)
                        {
                            ActiveReport3 subReportObj = subReport.Report;
                            if (null != subReportObj)
                                _FreeReport(ref subReportObj);
                        }
                    }
                }
                catch
                {} // do nothing
            }

            // dispose master report
            _FreeReport(ref report);
        }

        /// <summary>
        /// Gets extended fields from controls (checks in related DataFields).
        /// </summary>
        /// <param name="declareExtendedFields">Supported extended fields.</param>
        /// <param name="sections">Report's control sections.</param>
        /// <param name="extendedFields">Founded extended fields in controls.</param>
        private void _GetExtendedFieldsFromControl(ICollection<string> declareExtendedFields,
                                                   SectionCollection sections,
                                                   ref List<string> extendedFields)
        {
            Debug.Assert(null != declareExtendedFields);
            Debug.Assert(null != sections);
            Debug.Assert(null != extendedFields);

            // check in control's "DataField"
            for (int sectionIndex = 0; sectionIndex < sections.Count; ++sectionIndex)
            {
                Section section = sections[sectionIndex];
                for (int controlIndex = 0; controlIndex < section.Controls.Count; ++controlIndex)
                {
                    string dataField = section.Controls[controlIndex].DataField;
                    if (string.IsNullOrEmpty(dataField))
                        continue; // skip this

                    foreach (string field in declareExtendedFields)
                    {
                        if (dataField.Equals(field, StringComparison.OrdinalIgnoreCase) &&
                            !extendedFields.Contains(field))
                            extendedFields.Add(field);
                    }
                }
            }
        }

        /// <summary>
        /// Gets extended fields from script code.
        /// </summary>
        /// <param name="declareExtendedFields">Supported extended fields.</param>
        /// <param name="script">Report's script code.</param>
        /// <param name="extendedFields">Founded extended fields in controls.</param>
        private void _GetExtendedFieldsFromScript(ICollection<string> declareExtendedFields,
                                                  string script,
                                                  ref List<string> extendedFields)
        {
            Debug.Assert(null != declareExtendedFields);
            Debug.Assert(!string.IsNullOrEmpty(script));
            Debug.Assert(null != extendedFields);

            foreach (string field in declareExtendedFields)
            {
                if (extendedFields.Contains(field))
                    continue; // skip second and above time

                var usingFieldInScript =
                    string.Format(FIELD_IN_SCRIPT_FORMAT, field);
                if (-1 != script.IndexOf(usingFieldInScript))
                    extendedFields.Add(field);
            }
        }

        /// <summary>
        /// Gets report extended fields for report template.
        /// </summary>
        /// <param name="templatePath">Template file path.</param>
        /// <param name="extendedFields">Extended fields.</param>
        private void _GetReportExtendedFields(string templatePath, ref List<string> extendedFields)
        {
            Debug.Assert(!string.IsNullOrEmpty(templatePath));
            Debug.Assert(null != extendedFields);

            ActiveReport3 rpt = null;
            try
            {
                rpt = _LoadReportStructure(templatePath);

                ICollection<string> declareExtendedFields = _exporter.ExtendedFields;

                // check in control's "DataField"
                _GetExtendedFieldsFromControl(declareExtendedFields, rpt.Sections, ref extendedFields);

                // check in script
                if (extendedFields.Count != declareExtendedFields.Count)
                {
                    string script = rpt.Script;
                    if (!string.IsNullOrEmpty(script))
                    {
                        _GetExtendedFieldsFromScript(declareExtendedFields,
                                                     script,
                                                     ref extendedFields);
                    }
                }
            }
            finally
            {
                _DisposeReport(ref rpt);
            }
        }

        /// <summary>
        /// Gets report extended fields.
        /// </summary>
        /// <param name="info">Report template info.</param>
        /// <param name="extendedFields">Extended fields.</param>
        private void _GetReportExtendedFields(ReportInfo info, ref List<string> extendedFields)
        {
            Debug.Assert(null != info);
            Debug.Assert(null != extendedFields);

            _GetReportExtendedFields(info.TemplatePath, ref extendedFields);
            foreach (SubReportInfo subInfo in info.SubReports)
                _GetReportExtendedFields(subInfo.TemplatePath, ref extendedFields);
        }

        /// <summary>
        /// Initiates report description.
        /// </summary>
        /// <param name="sourceFileName">Data source file full name.</param>
        /// <param name="reportName">Report name.</param>
        /// <param name="info">Report template info.</param>
        /// <returns>Created report description.</returns>
        ReportStateDescription _InitReportDescription(string sourceFileName,
                                                      string reportName,
                                                      ReportInfo info)
        {
            Debug.Assert(!string.IsNullOrEmpty(sourceFileName));
            Debug.Assert(!string.IsNullOrEmpty(reportName));
            Debug.Assert(null != info);

            var description = new ReportStateDescription();
            description.SourceFilePath = sourceFileName;
            description.ReportName = reportName;
            description.ReportInfo = info;
            description.Report = null;

            return description;
        }

        /// <summary>
        /// Deletes file safely.
        /// </summary>
        /// <param name="fileName">File full name.</param>
        private void _DeleteFileSafe(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Prepares source file path.
        /// </summary>
        /// <returns>Created source file full name.</returns>
        private string _PrepareSourceFilePath()
        {
            string randFileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            string sourceFileName = _GetSourceFilePath(randFileName);
            if (!Directory.Exists(GetTempDirectoryName()))
                // create folder if not created
                Directory.CreateDirectory(GetTempDirectoryName());
            else
            {   // delete file if present
                if (File.Exists(sourceFileName))
                    File.Delete(sourceFileName);
            }

            return sourceFileName;
        }

        /// <summary>
        /// Exporter source report completed event handler.
        /// </summary>
        /// <param name="sender">Exporter.</param>
        /// <param name="e">Worker completed event arguments.</param>
        private void _exporter_AsyncExportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.Assert(null != _exporter);
            Debug.Assert(null != _reportInfos);
            Debug.Assert(!string.IsNullOrEmpty(_sourceFileName));

            _exporter.AsyncExportCompleted -= _exporter_AsyncExportCompleted;

            ICollection<ReportStateDescription> reports = null;
            if (!e.Cancelled && (null == e.Error))
            {
                var reportList = new List<ReportStateDescription>(_reportInfos.Count);
                foreach (string reportName in _reportInfos.Keys)
                {
                    ReportInfo info = _reportInfos[reportName];
                    reportList.Add(_InitReportDescription(_sourceFileName, reportName, info));
                }

                reports = reportList.AsReadOnly();
            }

            _ClearState();

            var args = new CreateReportsCompletedEventArgs(e.Error, e.Cancelled, reports);
            if (null != CreateReportsCompleted)
                CreateReportsCompleted(this, args);
        }

        #endregion // Private helpers

        #region Constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string DATA_SOURCE = "Data Source=";
        private const char DATA_SOURCE_END_SYMBOL = ';';
        private const string DATASOURCE_FILE_EXT = ".mdb";
        private const string RELATIVE_TEMP_DIRECTORY = @"ESRI\ArcLogistics\Reports";
        private const string REPORT_SECTION_NAME_DETAIL = "Detail";
        private const string FIELD_IN_SCRIPT_FORMAT = "\"{0}\"";

        #endregion // Constants

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Data exporter.
        /// </summary>
        private Exporter _exporter;
        /// <summary>
        /// Reports template settings keeper.
        /// </summary>
        private ReportsFile _reports;

        /// <summary>
        /// Reports to creation.
        /// </summary>
        private IDictionary<string, ReportInfo> _reportInfos;
        /// <summary>
        /// Created source file name.
        /// </summary>
        private string _sourceFileName;

        #endregion // Private members
    }
}
