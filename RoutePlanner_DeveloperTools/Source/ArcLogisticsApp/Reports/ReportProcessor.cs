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
using System.Windows;
using WinForms = System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.ComponentModel;
using System.Diagnostics;

using DataDynamics.ActiveReports.Export;
using DDActiveReports = DataDynamics.ActiveReports;

using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Reports;

namespace ESRI.ArcLogistics.App.Reports
{
    /// <summary>
    /// Supported report operation types.
    /// </summary>
    internal enum ProcessType
    {
        Preview,
        Print,
        Save
    }

    /// <summary>
    /// Class provide function to reports processing.
    /// </summary>
    internal sealed class ReportProcessor
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ReportProcessor</c> class.
        /// </summary>
        /// <param name="parentPage">Paren page for status update.</param>
        public ReportProcessor(Page parentPage)
        {
            Debug.Assert(null != parentPage);

            _exportDialogFilter = ReportsHelpers.AssemblyDialogFilter();
            _parentPage = parentPage;
        }

        #endregion Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Created reports.
        /// </summary>
        public IList<ReportStateDescription> Reports
        {
            get { return _reports; }
            set { _reports = value; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts reports operation.
        /// </summary>
        /// <param name="type">Selected reports operation.</param>
        /// <param name="reports">Reports to processing.</param>
        public void DoProcess(ProcessType type, IList<ReportStateDescription> reports)
        {
            Debug.Assert(null != _reports);
            Debug.Assert(null != reports);

            // dispose report only if it not selected for this operation and not loked
            ReportsGenerator generator = App.Current.ReportGenerator;
            for (int index = 0; index < _reports.Count; ++index)
            {
                ReportStateDescription report = _reports[index];
                if (!reports.Contains(report) && !report.IsLocked)
                    generator.DisposeReport(report);
            }

            // start process
            if (0 < _reports.Count)
            {
                switch (type)
                {
                    case ProcessType.Preview:
                        _DoPreview(generator, reports);
                        break;
                    case ProcessType.Print:
                        _DoPrint(generator, reports);
                        break;
                    case ProcessType.Save:
                        _DoSave(generator, reports);
                        break;
                    default:
                        {
                            Debug.Assert(false); // NOTE: not supported
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Free resource reports. Reomeves temparary files.
        /// </summary>
        public void FreeReports()
        {
            if (null != _reports)
            {
                _DisposeReports();

                _reports.Clear();
                _reports = null;

                // delete all files from reports temp directory
                var di = new DirectoryInfo(ReportsGenerator.GetTempDirectoryName());
                FileInfo[] rgFiles = di.GetFiles(FILE_FILTER_ALL);
                foreach (FileInfo fi in rgFiles)
                {
                    // delete file safely
                    try
                    {
                        fi.Delete();
                    }
                    catch
                    { } // do nothing
                }
            }
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Disposes report resources.
        /// </summary>
        private void _DisposeReports()
        {
            Debug.Assert(null != _reports);

            ReportsGenerator generator = App.Current.ReportGenerator;
            for (int index = 0; index < _reports.Count; ++index)
                generator.DisposeReport(_reports[index]);
        }

        /// <summary>
        /// Does exception routine.
        /// </summary>
        /// <param name="message">Head message text.</param>
        /// <param name="exception">Exception object.</param>
        /// <param name="generator">Report generator.</param>
        /// <param name="description">Report state description.</param>
        /// <returns>Exception status message.</returns>
        private MessageDetail _DoExceptionRoutine(string message,
                                                  Exception exception,
                                                  ReportsGenerator generator,
                                                  ReportStateDescription description)
        {
            Debug.Assert(!string.IsNullOrEmpty(message));
            Debug.Assert(null != exception);
            Debug.Assert(null != generator);
            Debug.Assert(null != description);

            Logger.Error(exception);

            generator.DisposeReport(description);

            string messageReason = (exception is OutOfMemoryException) ?
                                        App.Current.FindString("ReportPreviewReasonOutOfMemory") :
                                        exception.Message;

            if (!string.IsNullOrEmpty(messageReason))
                message = string.Format(MESSAGE_FORMAT, message, messageReason);

            return new MessageDetail(MessageType.Error, message);
        }

        /// <summary>
        /// Populates status messages.
        /// </summary>
        /// <param name="errorMessage">Head error message.</param>
        /// <param name="infoMessage">Head info message.</param>
        /// <param name="details">Status messages.</param>
        private void _PopulateMessages(string errorMessage,
                                       string infoMessage,
                                       IList<MessageDetail> details)
        {
            Debug.Assert(!string.IsNullOrEmpty(errorMessage));
            Debug.Assert(!string.IsNullOrEmpty(infoMessage));
            Debug.Assert(null != details);

            if (0 < details.Count)
            {
                string statusMessage = null;
                if (1 == details.Count)
                {
                    statusMessage = details[0].Text;
                    App.Current.Messenger.AddMessage(details[0].Type, statusMessage);
                }
                else
                {
                    // seect message type
                    MessageType messageType = MessageType.Information;
                    foreach (MessageDetail detail in details)
                    {
                        if (messageType < detail.Type)
                            messageType = detail.Type;
                    }

                    statusMessage = (messageType == MessageType.Error) ? errorMessage : infoMessage;
                    App.Current.Messenger.AddMessage(messageType, statusMessage, details);
                }

                App.Current.MainWindow.StatusBar.SetStatus(_parentPage, statusMessage);
            }
        }

        #region Preview operation
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Does report preview.
        /// </summary>
        /// <param name="generator">Report generator object.</param>
        /// <param name="description">Report state description.</param>
        /// <returns>Operation status message.</returns>
        private MessageDetail _DoPreview(ReportsGenerator generator,
                                         ReportStateDescription description)
        {
            Debug.Assert(null != generator);
            Debug.Assert(null != description);

            MessageDetail detail = null;
            try
            {
                PreviewForm viewerForm = new PreviewForm(null, description, false);
                viewerForm.Show();
            }
            catch (Exception ex)
            {
                string message = App.Current.GetString("ReportPreviewPreviewFailFormat",
                                                       description.ReportName);
                detail = _DoExceptionRoutine(message, ex, generator, description);
            }

            return detail;
        }

        /// <summary>
        /// Does preview reports.
        /// </summary>
        /// <param name="generator">Report generator object.</param>
        /// <param name="descriptions">Report state description list.</param>
        private void _DoPreview(ReportsGenerator generator,
                                IList<ReportStateDescription> descriptions)
        {
            Debug.Assert(null != generator);
            Debug.Assert(null != descriptions);
            Debug.Assert(0 < descriptions.Count);

            var details = new List<MessageDetail>();
            for (int index = 0; index < descriptions.Count; ++index)
            {
                ReportStateDescription description = descriptions[index];
                MessageDetail detail = _DoPreview(generator, description);
                if (null != detail)
                    details.Add(detail);
            }

            // populate errors
            if (0 < details.Count)
            {   // NOTE: only error supported
                string statusMessage = null;
                if (1 == details.Count)
                {
                    statusMessage = details[0].Text;
                    App.Current.Messenger.AddMessage(details[0].Type, statusMessage);
                }
                else
                {
                    statusMessage = App.Current.FindString("ReportPreviewTopLevelPreviewFail");
                    App.Current.Messenger.AddMessage(MessageType.Error, statusMessage, details);
                }

                App.Current.MainWindow.StatusBar.SetStatus(_parentPage, statusMessage);
            }
        }

        #endregion // Preview operation

        #region Print operation
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Print aborted handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Document_PrintAborted(object sender, EventArgs e)
        {
            string message = App.Current.FindString("ReportPreviewPrintingAborted");
            App.Current.Messenger.AddWarning(message);
        }

        /// <summary>
        /// Does print report.
        /// </summary>
        /// <param name="generator">Report generator object.</param>
        /// <param name="description">Report state description.</param>
        /// <param name="showPrintDialog">Need show print dialog flag.</param>
        /// <param name="copiesCount">Selecetd copies count.</param>
        /// <param name="isCanceled">Is operation ccancel by user.</param>
        /// <returns>Operation status message.</returns>
        private MessageDetail _DoPrint(ReportsGenerator generator,
                                       ReportStateDescription description,
                                       bool showPrintDialog,
                                       ref int copiesCount,
                                       ref bool isCanceled)
        {
            Debug.Assert(null != generator);
            Debug.Assert(null != description);

            isCanceled = false;
            MessageDetail detail = null;
            try
            {
                // generate report
                if (null == description.Report)
                    generator.RunReport(description);

                // init print dialog
                DDActiveReports.Document.Document document = description.Report.Document;
                document.PrintAborted +=
                    new DDActiveReports.Document.PrintAbortedEventHandler(Document_PrintAborted);
                DDActiveReports.Document.Printer printer = document.Printer;
                printer.PrinterSettings.PrintRange = PrintRange.SomePages;
                printer.PrinterSettings.FromPage = 1;
                printer.PrinterSettings.ToPage = description.Report.Document.Pages.Count;
                printer.PrinterSettings.PrintRange = PrintRange.AllPages;

                // select application printer
                ArcLogistics.App.PrinterSettingsStore settings = App.Current.PrinterSettingsStore;
                if (!string.IsNullOrEmpty(settings.PrinterName))
                {
                    description.Report.Document.Printer.PrinterName = settings.PrinterName;
                    description.Report.Document.Printer.DefaultPageSettings = settings.PageSettings;
                }

                // do printing
                if (!description.Report.Document.Print(showPrintDialog, true))
                    isCanceled = true;
                else
                {
                    if (showPrintDialog)
                    {
                        // store copies count
                        copiesCount = description.Report.Document.Printer.PrinterSettings.Copies;

                        // store printer settings in application
                        var prntSettings = description.Report.Document.Printer.PrinterSettings;
                        if (!string.IsNullOrEmpty(prntSettings.PrinterName))
                            settings.StoreSetting(prntSettings.PrinterName,
                                                  prntSettings.DefaultPageSettings);
                    }

                    string message = App.Current.GetString("ReportPreviewPrintDoneFormat",
                                                           description.ReportName);
                    detail = new MessageDetail(MessageType.Information, message);
                }
            }
            catch (Exception ex)
            {
                string message = App.Current.GetString("ReportPreviewPrintFailFormat",
                                                       description.ReportName);
                detail = _DoExceptionRoutine(message, ex, generator, description);
            }

            return detail;
        }

        /// <summary>
        /// Does print operation.
        /// </summary>
        /// <param name="generator">Report generator object.</param>
        /// <param name="descriptions">Report state description list.</param>
        private void _DoPrint(ReportsGenerator generator,
                              IList<ReportStateDescription> descriptions)
        {
            Debug.Assert(null != generator);
            Debug.Assert(null != descriptions);
            Debug.Assert(0 < descriptions.Count);

            List<MessageDetail> details = new List<MessageDetail>();

            bool multiPrint = (1 < descriptions.Count);

            bool isCanceled = false;
            int copiesCount = 1;
            for (int index = 0; index < descriptions.Count; ++index)
            {
                ReportStateDescription description = descriptions[index];
                MessageDetail detail = _DoPrint(generator, description, (0 == index),
                                                ref copiesCount, ref isCanceled);
                if (null != detail)
                    details.Add(detail);

                if (isCanceled)
                    break;
            }

            if (!isCanceled)
            {
                string errorMessage = App.Current.FindString("ReportPreviewTopLevelPrintFail");
                string infoMessage = App.Current.FindString("ReportPreviewTopLevelPrintDone");
                _PopulateMessages(errorMessage, infoMessage, details);
            }
        }

        #endregion // Print operation

        #region Save operation
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Validates report name for save to file,
        /// </summary>
        /// <param name="reportName">Report name.</param>
        /// <returns>Valid file name.</returns>
        private string _ValidateFileName(string reportName)
        {
            Debug.Assert(!string.IsNullOrEmpty(reportName));

            // get a list of invalid file characters.
            char[] invalidFileChars = Path.GetInvalidFileNameChars();

            // replace each invalid character by special symbol.
            foreach (char invalidFChar in invalidFileChars)
                reportName = reportName.Replace(invalidFChar, SEPARATOR_SYMBOL);

            return reportName;
        }

        /// <summary>
        /// Saves report routine.
        /// </summary>
        /// <param name="exporter">Exporter object.</param>
        /// <param name="exportDir">Export directory.</param>
        /// <param name="fileName">Export report file name (can be null).</param>
        /// <param name="fileExt">Export file extension.</param>
        /// <param name="generator">Report generator.</param>
        /// <param name="description">Report state descriiption.</param>
        /// <returns>Status messages.</returns>
        private MessageDetail _DoSaveReport(IDocumentExport exporter,
                                            string exportDir,
                                            string fileName,
                                            string fileExt,
                                            ReportsGenerator generator,
                                            ReportStateDescription description)
        {
            Debug.Assert(null != exporter);
            Debug.Assert(null != generator);
            Debug.Assert(null != description);
            Debug.Assert((null != exportDir) && (null != fileExt));

            MessageDetail detail = null;
            string fullFileName = null;
            try
            {
                string filename = (string.IsNullOrEmpty(fileName)) ?
                                    _ValidateFileName(description.ReportName) : fileName;
                fullFileName = Path.Combine(exportDir, filename + fileExt);

                if (null == description.Report)
                    generator.RunReport(description);

                exporter.Export(description.Report.Document, fullFileName);
                string message = App.Current.GetString("ReportPreviewSaveDoneFormat",
                                                       description.ReportName, fullFileName);
                detail = new MessageDetail(MessageType.Information, message);
            }
            catch (Exception ex)
            {
                string message = App.Current.GetString("ReportPreviewSaveFailFormat",
                                                       description.ReportName, fullFileName);
                detail = _DoExceptionRoutine(message, ex, generator, description);
            }

            return detail;
        }

        /// <summary>
        /// Does save init exception routine.
        /// </summary>
        /// <param name="exception">Exception.</param>
        private void _DoSaveInitExceptionRoutine(Exception exception)
        {
            Debug.Assert(null != exception);

            Logger.Error(exception);
            var message = App.Current.FindString("ReportPreviewTopLevelSaveFail");
            var messageReason = (exception is OutOfMemoryException) ?
                                    App.Current.FindString("ReportPreviewReasonOutOfMemory") :
                                    exception.Message;
            if (!string.IsNullOrEmpty(messageReason))
                message = string.Format(MESSAGE_FORMAT, message, messageReason);

            App.Current.Messenger.AddError(message);
        }

        /// <summary>
        /// Dialog save FileOk handler.
        /// </summary>
        /// <param name="sender">Sender object (SaveFileDialog).</param>
        /// <param name="e">Ignored.</param>
        private void _DialogSave_FileOk(object sender, CancelEventArgs e)
        {
            Debug.Assert(null != sender);

            var dlgSave = sender as WinForms.SaveFileDialog;
            int index = dlgSave.FilterIndex - 1;
            dlgSave.FileName =
                Path.ChangeExtension(dlgSave.FileName, ReportsHelpers.GetFileExtension(index));
            // NOTE: FilterIndex 1st based
        }

        /// <summary>
        /// Gets export output settings for files (directory, file name and extension).
        /// </summary>
        /// <param name="reportName">Report name.</param>
        /// <param name="exportDir">Export directory.</param>
        /// <param name="fileName">Report file name.</param>
        /// <param name="fileExt">Export file extension.</param>
        private void _GetExportOutputSettings(string reportName,
                                              out string exportDir,
                                              out string fileName,
                                              out string fileExt)
        {
            Debug.Assert(!string.IsNullOrEmpty(reportName));

            exportDir = null;
            fileName = null;
            fileExt = null;

            using (var dlgSave = new WinForms.SaveFileDialog())
            {
                dlgSave.AutoUpgradeEnabled = true;
                dlgSave.RestoreDirectory = true;
                dlgSave.CheckPathExists = true;
                dlgSave.CheckFileExists = false;
                dlgSave.OverwritePrompt = false;
                int typeIndex = ReportsHelpers.DefaultSelectedTypeIndex;
                dlgSave.FilterIndex = typeIndex + 1; // NOTE: FilterIndex 1st based
                dlgSave.Filter = _exportDialogFilter;
                string extension = ReportsHelpers.GetFileExtension(typeIndex);

                var format = App.Current.FindString("ReportPreviewSaveDialogTitleFormat");
                dlgSave.Title = string.Format(format, reportName);
                dlgSave.FileName = _ValidateFileName(reportName) + extension;
                dlgSave.FileOk += new CancelEventHandler(_DialogSave_FileOk);
                dlgSave.DefaultExt = extension;

                if (dlgSave.ShowDialog(null) == WinForms.DialogResult.OK)
                {
                    string selectedFileName = dlgSave.FileName;
                    exportDir = Path.GetDirectoryName(selectedFileName);
                    fileExt = ReportsHelpers.GetFileExtension(dlgSave.FilterIndex - 1);
                    // NOTE: FilterIndex 1st based
                    fileName = Path.GetFileNameWithoutExtension(selectedFileName);
                }
            }
        }

        /// <summary>
        /// Gets export output settings for files (directory and file extension).
        /// </summary>
        /// <param name="exportDir">Export directory.</param>
        /// <param name="fileExt">Export file extension.</param>
        private void _GetExportOutputSettings(out string exportDir,
                                              out string fileExt)
        {
            exportDir = null;
            fileExt = null;

            // create dialog
            ReportsSaveDlg dlg = new ReportsSaveDlg();
            dlg.Owner = App.Current.MainWindow;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // show dialog
            if (true == dlg.ShowDialog())
            {
                exportDir = dlg.savePathEdit.Text;
                string typeName = dlg.comboboxType.Text;
                fileExt = ReportsHelpers.GetFileExtensionByName(typeName);
            }
        }

        /// <summary>
        /// Saves operation.
        /// </summary>
        /// <param name="generator">Reports generator.</param>
        /// <param name="descriptions">Report state description.</param>
        private void _DoSave(ReportsGenerator generator, IList<ReportStateDescription> descriptions)
        {
            Debug.Assert(null != generator);
            Debug.Assert(null != descriptions);
            Debug.Assert(0 < descriptions.Count);

            try
            {
                string exportDir = null;
                string fileName = null;
                string fileExt = null;

                // get export settings
                if (1 == descriptions.Count)
                {   // get settings for one file
                    _GetExportOutputSettings(descriptions[0].ReportName,
                                             out exportDir,
                                             out fileName,
                                             out fileExt);
                }
                else
                {   // get settings for files
                    Debug.Assert(1 < descriptions.Count);
                    _GetExportOutputSettings(out exportDir, out fileExt);
                }

                // export reports
                if (!string.IsNullOrEmpty(exportDir) && !string.IsNullOrEmpty(fileExt))
                {
                    IDocumentExport exporter = ReportsHelpers.GetExporterByFileExtension(fileExt);
                    // do save export files
                    List<MessageDetail> details = new List<MessageDetail>();
                    for (int index = 0; index < descriptions.Count; ++index)
                    {
                        ReportStateDescription description = descriptions[index];
                        MessageDetail detail = _DoSaveReport(exporter, exportDir, fileName, fileExt,
                                                             generator, description);
                        if (null != detail)
                            details.Add(detail);
                    }

                    // export done - show messages
                    string infoMessage = App.Current.FindString("ReportPreviewTopLevelSaveDone");
                    string errorMessage = App.Current.FindString("ReportPreviewTopLevelSaveFail");
                    _PopulateMessages(errorMessage, infoMessage, details);
                }
            }
            catch (Exception ex)
            {
                _DoSaveInitExceptionRoutine(ex);
            }
        }

        #endregion // Save operation

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// File filter "all files".
        /// </summary>
        private const string FILE_FILTER_ALL = "*.*";

        /// <summary>
        /// Error message format.
        /// </summary>
        private const string MESSAGE_FORMAT = "{0} {1}";

        /// <summary>
        /// Symbol to replace invalid characters in report name.
        /// </summary>
        private const char SEPARATOR_SYMBOL = '_';

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parent page.
        /// </summary>
        private Page _parentPage;

        /// <summary>
        /// Created reports.
        /// </summary>
        private IList<ReportStateDescription> _reports;

        /// <summary>
        /// Export dialog filter.
        /// </summary>
        private readonly string _exportDialogFilter;

        #endregion // Private fields
    }
}
