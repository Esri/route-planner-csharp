using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Reports;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for ReportsPreferencesPage.xaml
    /// </summary>
    internal partial class ReportsPreferencesPage : PageBase
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static string PageName
        {
            get { return PAGE_NAME; }
        }

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ReportsPreferencesPage()
        {
            InitializeComponent();

            IsRequired = true;
            IsAllowed = true;
            DoesSupportCompleteStatus = false;

            App.Current.ApplicationInitialized += new EventHandler(_ApplicationInitialized);
            App.Current.ProjectClosing += new EventHandler(_ProjectPreClose);
            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);

            this.Loaded += new RoutedEventHandler(_Page_Loaded);
            this.Unloaded += new RoutedEventHandler(_Page_Unloaded);

            _InitPageContent();
        }

        #endregion // Constructors

        #region Page Overrided Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("ReportsPreferencesPageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("ReportsPreferencesBrush"); }
        }

        #endregion

        #region Public PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.ReportsPreferencesPagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion // PageBase overrided members

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occcurs when user clicks on CheckBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataCellGotFocus(object sender, RoutedEventArgs e)
        {
            ReportDataWrapper selectedItem = xceedGridReports.CurrentItem as ReportDataWrapper;
            if (null != selectedItem)
            {
                ReportInfo info = App.Current.ReportGenerator.GetReportInfo(selectedItem.Name);
                if (null != info)
                {
                    _selectedProfileName = info.Name;
                    ReportDataWrapper.StartTemplatePath = info.TemplatePath;
                    ReportDataWrapper.StartTemplateName = _selectedProfileName;

                    if (info.IsPredefined)
                    {
                        DataCell cell = sender as DataCell;
                        if (cell.ParentColumn == xceedGridReports.Columns["Name"])
                        {
                            ToolTip tt = new ToolTip();
                            tt.Style = (Style)mainGrid.FindResource("ErrorToolTipStyle");
                            cell.ToolTip = tt;
                            cell.EndEdit();
                        }
                    }

                    if (!_isEditeStart)
                        _UpdateButtonsState();
                }
            }
        }

        /// <summary>
        /// Occurs when cell lost focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataCellLostFocus(object sender, RoutedEventArgs e)
        {
            DataCell cell = sender as DataCell;
            if (null != cell.ToolTip)
            {
                ToolTip tt = cell.ToolTip as ToolTip;
                if (null != tt)
                    if (tt.Content.ToString() == (string)Application.Current.FindResource("ReportsPreferencesPageRenameErrorToolTip"))
                        cell.ToolTip = null;
            }
        }

        /// <summary>
        /// Occurs when user press start edit name.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCollectionViewSource_BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            ReportDataWrapper selectedItem = xceedGridReports.CurrentItem as ReportDataWrapper;
            if (null != selectedItem)
            {
                ReportInfo info = App.Current.ReportGenerator.GetReportInfo(selectedItem.Name);
                if (null != info)
                {
                    if (!info.IsPredefined)
                    {
                        buttonDeleteTemplate.IsEnabled = buttonEditTemplate.IsEnabled = buttonDuplicateTemplate.IsEnabled = false;
                        _isEditeStart = true;
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Occurs when user press "Esc" on edited grid row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCollectionViewSource_CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            _isEditeStart = false;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs when user press "Enter" on edited grid row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCollectionViewSource_CommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            try
            {
                if (_isEditeStart && !string.IsNullOrEmpty(_selectedProfileName))
                {
                    ReportInfo editedInfo  = App.Current.ReportGenerator.GetReportInfo(_selectedProfileName);
                    if (null != editedInfo)
                    {
                        Debug.Assert(!editedInfo.IsPredefined);

                        ReportDataWrapper selectedItem = xceedGridReports.CurrentItem as ReportDataWrapper;

                        string templatePath = ReportsGenerator.GetNewTemplatePath(selectedItem.Name, editedInfo.TemplatePath);

                        string fileSrcName = ReportsGenerator.GetTemplateAbsolutelyPath(editedInfo.TemplatePath);
                        string fileDestName = ReportsGenerator.GetTemplateAbsolutelyPath(templatePath);
                        File.Move(fileSrcName, fileDestName);

                        _selectedProfileName = editedInfo.Name = selectedItem.Name;
                        ReportDataWrapper.StartTemplatePath = templatePath;
                        ReportDataWrapper.StartTemplateName = _selectedProfileName;
                        editedInfo.TemplatePath = templatePath;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                App.Current.Messenger.AddError(ex.Message);
            }

            _isEditeStart = false;
            e.Handled = true;
        }

        /// <summary>
        /// Edit button click handler.
        /// </summary>
        private void _EditTemplate_Click(object sender, RoutedEventArgs e)
        {
            ReportInfo info = _GetSelectedInfo();
            if (null != info)
            {
                string reportTemplatePath = ReportsGenerator.GetTemplateAbsolutelyPath(info.TemplatePath);

                EndUserDesignerForm form = new EndUserDesignerForm(info.Name, reportTemplatePath);
                form.Show();
            }
        }

        /// <summary>
        /// Duplicate button click handler.
        /// </summary>
        private void _DuplicateTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportInfo selectedInfo = _GetSelectedInfo();
                if (null != selectedInfo)
                {
                    ReportsGenerator generator = App.Current.ReportGenerator;

                    // generate new name
                    string dir = Path.GetDirectoryName(selectedInfo.TemplatePath);

                    // create sub reports
                    List<SubReportInfo> subReports = new List<SubReportInfo>();
                    foreach (SubReportInfo subReport in selectedInfo.SubReports)
                    {
                        string subNameNew = _GetNameForDublicate(subReport.Name);
                        string subTemplatePath = ReportsGenerator.GetNewTemplatePath(subNameNew, subReport.TemplatePath);
                        SubReportInfo newSubReport = new SubReportInfo(subNameNew, subTemplatePath, subReport.Description,
                                                                       subReport.IsDefault, subReport.GroupId,
                                                                       subReport.IsVisible);
                        // copy template file for subreport template
                        _DuplicateReportFile(subReport.TemplatePath, newSubReport.TemplatePath);

                        subReports.Add(newSubReport);
                    }

                    // create new info
                    string nameNew = _GetNameForDublicate(selectedInfo.Name);
                    string templatePath = ReportsGenerator.GetNewTemplatePath(nameNew, selectedInfo.TemplatePath);
                    ReportInfo newInfo = new ReportInfo(nameNew, selectedInfo.Context, templatePath, selectedInfo.Description,
                                                        false, subReports);

                    // copy template file for template
                    _DuplicateReportFile(selectedInfo.TemplatePath, newInfo.TemplatePath);

                    generator.AddReportInfo(newInfo);
                    _itemToSelection = newInfo.Name;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                App.Current.Messenger.AddError(ex.Message);
            }

            _InitReportTable();
        }

        /// <summary>
        /// Delete button click handler.
        /// </summary>
        private void _DeleteTemplate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ReportInfo selectedInfo = _GetSelectedInfo();
                if (null != selectedInfo)
                {
                    bool doProcess = true;
                    if (Settings.Default.IsAllwaysAskBeforeDeletingEnabled)
                        // show warning dialog
                        doProcess = DeletingWarningHelper.Execute(selectedInfo.Name, "ReportTemplate", "ReportTemplate");

                    if (doProcess)
                    {
                        App.Current.ReportGenerator.DeleteReportInfo(selectedInfo.Name);
                        _itemIndexToSelection = xceedGridReports.SelectedIndex;

                        // remove template file
                        string reportTemplatePath = ReportsGenerator.GetTemplateAbsolutelyPath(selectedInfo.TemplatePath);
                        _DeleteFileSafe(reportTemplatePath);

                        // remove sub report templates
                        foreach (SubReportInfo subReport in selectedInfo.SubReports)
                        {
                            reportTemplatePath = ReportsGenerator.GetTemplateAbsolutelyPath(subReport.TemplatePath);
                            _DeleteFileSafe(reportTemplatePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            _InitReportTable();
        }

        /// <summary>
        /// Project preclose handler.
        /// </summary>
        private void _ProjectPreClose(object sender, EventArgs e)
        {
            _StorePageContent();

            if (null != _viewSourceReports)
                _viewSourceReports.Source = null;
        }

        /// <summary>
        /// Project loaded handler.
        /// </summary>
        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _InitReportTable();
        }

        /// <summary>
        /// Page unloaded handler.
        /// </summary>
        private void _Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _StorePageContent();
        }

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        private void _Page_Loaded(object sender, RoutedEventArgs e)
        {
            _InitPageContent();
        }

        /// <summary>
        /// Application initialized handler.
        /// </summary>
        private void _ApplicationInitialized(object sender, EventArgs e)
        {
            _InitPageContent();
        }

        /// <summary>
        /// Update Enable\Disable state process buttons
        /// </summary>
        private void _UpdateButtonsState()
        {
            ReportInfo selectedInfo = _GetSelectedInfo();
            bool hasSelected = (null != selectedInfo);
            bool canEdit = false;
            if (hasSelected)
                canEdit = !selectedInfo.IsPredefined;

            buttonDuplicateTemplate.IsEnabled = hasSelected;
            buttonDeleteTemplate.IsEnabled = buttonEditTemplate.IsEnabled = canEdit;
        }

        /// <summary>
        /// ExceedGrid Item Source Changed handler.
        /// </summary>
        private void _OnItemSourceChanged(object sender, EventArgs e)
        {
            // select item
            if (null != _itemToSelection)
            {   // find wrapper by name
                IList<ReportDataWrapper> wrappers = _viewSourceReports.Source as IList<ReportDataWrapper>;
                for (int index = 0; index < wrappers.Count; ++index)
                {
                    ReportDataWrapper wrapper = wrappers[index];
                    if (_itemToSelection == wrapper.Name)
                    {
                        xceedGridReports.SelectedItem = wrapper;
                        break;
                    }
                }
            }
            else
                xceedGridReports.SelectedIndex = Math.Min(_itemIndexToSelection, xceedGridReports.Items.Count - 1);
            xceedGridReports.CurrentItem = xceedGridReports.SelectedItem;
            _itemIndexToSelection = 0;
            _itemToSelection = null;

            _UpdateButtonsState();
        }

        /// <summary>
        /// Button is enabled changed handler.
        /// </summary>
        private void button_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string enabledTooltip = null;
            string disabledTooltip = null;

            Button button = (Button)sender;

            if (button == buttonDeleteTemplate)
            {
                enabledTooltip = (string)App.Current.FindResource("DeleteCommandEnabledTooltip");
                disabledTooltip = (string)App.Current.FindResource("DeleteCommandDisabledTooltip");
            }
            else if (button == buttonDuplicateTemplate)
            {
                enabledTooltip = (string)App.Current.FindResource("DuplicateCommandEnabledTooltip");
                disabledTooltip = (string)App.Current.FindResource("DuplicateCommandDisabledTooltip");
            }
            else if (button == buttonEditTemplate)
            {
                enabledTooltip = (string)App.Current.FindResource("EditCommandEnabledTooltip");
                disabledTooltip = (string)App.Current.FindResource("EditCommandDisabledTooltip");
            }

            if (button.IsEnabled)
                button.ToolTip = enabledTooltip;
            else
                button.ToolTip = disabledTooltip;
        }

        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits report table.
        /// </summary>
        private void _InitReportTable()
        {
            ReportsGenerator generator = App.Current.ReportGenerator;
            if (null != generator)
            {
                Debug.Assert(null != _viewSourceReports);

                List<ReportDataWrapper> reportsWrap = new List<ReportDataWrapper>();
                foreach (string name in generator.GetPresentedNames(false))
                    reportsWrap.Add(new ReportDataWrapper(name));

                _viewSourceReports.Source = reportsWrap;
            }
        }

        /// <summary>
        /// Inits page GUI elements.
        /// </summary>
        protected void _InitPageContent()
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, null);

            if (!_isLoaded)
            {
                _viewSourceReports = (DataGridCollectionViewSource)mainGrid.FindResource("reportsTable");

                GridStructureInitializer structureInitializer = new GridStructureInitializer(GRID_STRUCTURE_REPORT_PREFERENCES);
                structureInitializer.BuildGridStructure(_viewSourceReports, xceedGridReports);
                ColumnBase columnName = xceedGridReports.Columns["Name"];
                columnName.ReadOnly = false;
                columnName.CellValidationRules.Add(new ReportTemplateValidationRule());

                xceedGridReports.OnItemSourceChanged += new EventHandler(_OnItemSourceChanged);

                _InitReportTable();

                _isLoaded = true;
            }
        }

        /// <summary>
        /// Stores page changes.
        /// </summary>
        private void _StorePageContent()
        {
            if (null != App.Current)
            {
                ReportsGenerator generator = App.Current.ReportGenerator;
                if (null != generator)
                    generator.StoreChanges();
            }
        }

        /// <summary>
        /// Gets template dublicate unique name.
        /// </summary>
        /// <param name="oldName">Source template name.</param>
        /// <returns>Dublicate unique name.</returns>
        private string _GetNameForDublicate(string sourceName)
        {
            ReportsGenerator generator = App.Current.ReportGenerator;
            ICollection<string> collection = generator.GetPresentedNames(true);

            int k = 2;

            string newName = string.Format((string)App.Current.FindResource("ItemCopyShortName"), sourceName);
            if (collection.Contains(newName))
            {
                newName = string.Format((string)App.Current.FindResource("ItemCopyLongName"), k, sourceName);
                while (collection.Contains(newName))
                {
                    ++k;
                    newName = string.Format((string)App.Current.FindResource("ItemCopyLongName"), k, sourceName);
                }
            }

            return newName;
        }

        /// <summary>
        /// Gets from xceed selected template info.
        /// </summary>
        /// <returns></returns>
        private ReportInfo _GetSelectedInfo()
        {
            ReportDataWrapper selectedItem = xceedGridReports.SelectedItem as ReportDataWrapper;
            return (null == selectedItem) ? null : App.Current.ReportGenerator.GetReportInfo(selectedItem.Name);
        }

        /// <summary>
        /// Duplicates report template file.
        /// </summary>
        /// <param name="srcTemplatePath">Source template file path.</param>
        /// <param name="dubTemplatePath">Doublecate template file path.</param>
        private void _DuplicateReportFile(string srcTemplatePath, string dubTemplatePath)
        {
            Debug.Assert(!string.IsNullOrEmpty(srcTemplatePath));
            Debug.Assert(!string.IsNullOrEmpty(dubTemplatePath));

            string fileSrcName = ReportsGenerator.GetTemplateAbsolutelyPath(srcTemplatePath);
            string fileDestName = ReportsGenerator.GetTemplateAbsolutelyPath(dubTemplatePath);
            File.Copy(fileSrcName, fileDestName, true);
        }

        /// <summary>
        /// Deletes file safely.
        /// </summary>
        /// <param name="fileName"></param>
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

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page layout loaded.
        /// </summary>
        private bool _isLoaded = false;

        /// <summary>
        /// Glag template name edited.
        /// </summary>
        private bool _isEditeStart = false;

        /// <summary>
        /// Item index to selection. Index of deleted item.
        /// </summary>
        private int _itemIndexToSelection = 0;

        /// <summary>
        /// Item index to selection.
        /// </summary>
        private string _itemToSelection = null;

        /// <summary>
        /// Collection view source.
        /// </summary>
        private DataGridCollectionViewSource _viewSourceReports = null;

        /// <summary>
        /// Selected profile name.
        /// </summary>
        private string _selectedProfileName = null;

        private const string OPENFILE_DIALOG_FILTER =
            "All image files (*.bmp, *.gif, *.jpg, *.jpeg, *.png, *.ico, *.emf, *.wmf)|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.ico;*.emf;*.wmf|Bitmap files (*.bmp, *.gif, *.jpg, *.jpeg, *.png, *.ico)|*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.ico|Metafiles (*.emf, *.wmf)|*.emf;*.wmf";
        private const string OPENFILE_DIALOG_DEFAULTEXT = "*.bmp;*.gif;*.jpg;*.jpeg;*.png;*.ico;*.emf;*.wmf";

        private const string PAGE_NAME = "Reports";

        private const string GRID_STRUCTURE_REPORT_PREFERENCES = "ESRI.ArcLogistics.App.GridHelpers.ReportGridStructure.xaml";

        #endregion // Private members
    }

    /// <summary>
    /// Class defines validation rule for report template name.
    /// </summary>
    internal class ReportTemplateValidationRule : Xceed.Wpf.DataGrid.ValidationRules.CellValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo culture, CellValidationContext context)
        {
            // not empty check
            bool isObjectEmpty = true;
            if (null != value)
                isObjectEmpty = string.IsNullOrEmpty(value.ToString().Trim());
            if (isObjectEmpty)
                return new ValidationResult(false, (string)Application.Current.FindResource("ReportTemplateEmptyName"));

            // unique name check
            string name = value.ToString();
            if (!string.IsNullOrEmpty(ReportDataWrapper.StartTemplateName))
            {
                if (0 == string.Compare(ReportDataWrapper.StartTemplateName, name, true))
                    return ValidationResult.ValidResult;
            }

            ReportsGenerator generator = App.Current.ReportGenerator;
            ICollection<string> presentedNames = generator.GetPresentedNames(true);
            foreach (string nameTemplate in presentedNames)
            {
                if (0 == string.Compare(nameTemplate, name, true))
                    return new ValidationResult(false, (string)Application.Current.FindResource("ReportTemplateNotUniqueName"));
            }

            // normal length check
            bool isLong = false;
            string templatePath = ReportsGenerator.GetNewTemplatePath(name, ReportDataWrapper.StartTemplatePath);
            string fileName = null;
            try
            {
                fileName = ReportsGenerator.GetTemplateAbsolutelyPath(templatePath);
                new FileInfo(fileName);
            }
            catch (PathTooLongException)
            {
                isLong =true;
            }
            catch (Exception)
            {
            }

            if (isLong)
                return new ValidationResult(false, (string)Application.Current.FindResource("ReportTemplateLongName"));

            // valid name check
            if (!FileHelpers.ValidateFilepath(fileName))
                return new ValidationResult(false, (string)Application.Current.FindResource("ReportTemplateInvalidName"));

            return ValidationResult.ValidResult;
        }
    }
}
