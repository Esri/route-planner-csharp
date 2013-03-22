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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Widgets;
using ESRI.ArcLogistics.App.Reports;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    internal partial class ReportsPage : PageBase
    {
        /// <summary>
        /// Unique page name.
        /// </summary>
        public static string PageName
        {
            get { return PAGE_NAME; }
        }

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ReportsPage</c> class.
        /// </summary>
        public ReportsPage()
        {
            InitializeComponent();

            IsRequired = true;
            IsAllowed = true;

            _Init();

            _processor = new ReportProcessor(this);

            // attach to events
            this.Loaded += new RoutedEventHandler(_PageLoaded);
            this.Unloaded += new RoutedEventHandler(_PageUnloaded);
            App.Current.ProjectClosing += new EventHandler(_ProjectPreClose);
            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);
            App.Current.MainWindow.Closed += new EventHandler(_MainWindow_Closed);
        }

        #endregion // Constructors

        #region Page Overrided Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns unique page name.
        /// </summary>
        public override string Name
        {
            get { return PageName; }
        }

        /// <summary>
        /// Returns page title.
        /// </summary>
        public override string Title
        {
            get { return App.Current.FindString("ReportsPageCaption"); }
        }

        /// <summary>
        /// Returns page icon as a TileBrush (DrawingBrush or ImageBrush).
        /// </summary>
        public override TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("ReportsBrush"); }
        }

        /// <summary>
        /// Returns name of Help Topic.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.ReportsPagePath); }
        }

        /// <summary>
        /// Returns category name of commands that will be present in Tasks widget.
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion // Page overrided members

        #region BasePage overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates widgets that are shown for all pages.
        /// </summary>
        protected override void CreateWidgets()
        {
            base.CreateWidgets();

            // add and create calndar widget
            var calendarWidget = new DateRangeCalendarWidget("CalendarWidgetCaption");
            calendarWidget.Initialize(this);
            this.EditableWidgetCollection.Insert(0, calendarWidget);
        }

        #endregion // BasePage overrided members

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        /// <remarks>Inits GUI.</remarks>
        private void _PageLoaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, "");

            if (!_isInited)
                _Init();

            _UpdateSchedules();
            _UpdateGui();

            // attach to solver event
            App.Current.Solver.AsyncSolveCompleted +=
                new AsyncSolveCompletedEventHandler(_Cmd_AsyncSolveCompleted);

            // attach to calendar event
            _GetCalendarWidget().SelectedDatesChanged +=
                new EventHandler(_calendarWidget_SelectedDatesChanged);

            _ExpandAllDetail();
        }

        /// <summary>
        /// Page unloaded handler.
        /// </summary>
        /// <remarks>Update GUI.</remarks>
        private void _PageUnloaded(object sender, RoutedEventArgs e)
        {
            // detach events
            _GetCalendarWidget().SelectedDatesChanged -= _calendarWidget_SelectedDatesChanged;
            App.Current.Solver.AsyncSolveCompleted -= _Cmd_AsyncSolveCompleted;

            // do report templates empty
            _viewSourceTemplates.Source = new List<SelectReportWrapper>();

            _schedulesToReport.Clear();
        }

        /// <summary>
        /// Project loaded handler.
        /// </summary>
        /// <remarks>Free reports components.</remarks>
        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _FreeReports();
        }

        /// <summary>
        /// MainWindow closed handler.
        /// </summary>
        /// <remarks>Stop reports generation.</remarks>
        private void _MainWindow_Closed(object sender, EventArgs e)
        {
            ReportsGenerator generator = App.Current.ReportGenerator;
            if (null != generator)
            {
                generator.CreateReportsCompleted -= generator_CreateReportsCompleted;
                generator.AbortCreateReports();
            }
        }

        /// <summary>
        /// Check All button click event handler.
        /// </summary>
        /// <remarks>Updates route selection in table.</remarks>
        private void ButtonCheckAll_Click(object sender, RoutedEventArgs e)
        {
            _SetAllRoutesToOneState(true);
        }

        /// <summary>
        /// Uncheck All button click event handler.
        /// </summary>
        /// <remarks>Updates route selection in table.</remarks>
        private void ButtonUncheckAll_Click(object sender, RoutedEventArgs e)
        {
            _SetAllRoutesToOneState(false);
        }

        /// <summary>
        /// Generate button click event handler.
        /// </summary>
        /// <remarks>Starts reports generation process.</remarks>
        private void ButtonGenerate_Click(object sender, RoutedEventArgs e)
        {
            App app = App.Current;
            if (_IsSolveStartedOnSelectedDates())
            {
                // solve operation started - show warning
                string statusMessage = app.FindString("ExportSolveOperationDetected");

                app.MainWindow.StatusBar.SetStatus(this, statusMessage);
                app.MainWindow.MessageWindow.AddWarning(statusMessage);
                buttonGenerate.IsEnabled = false;
                xceedGridRoutes.Focus();
            }
            else
            {
                if (0 < _schedulesToReport.Count)
                {
                    // start generation
                    app.UIManager.Lock(true);

                    ReportsGenerator generator = app.ReportGenerator;
                    var extendedFields = generator.GetReportsExtendedFields(_GetSelectedTemplates());
                    if (0 < extendedFields.Count)
                        _CreateGenerateDirectionsRouteList();
                    _DoGenerationProcess();
                }
                else
                {
                    // schedule absent - show warning
                    string statusMessage = _GetMessageScheduleNotFounded();
                    app.MainWindow.StatusBar.SetStatus(this, statusMessage);
                    buttonGenerate.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Check box Separate Reports click event handler.
        /// </summary>
        /// <remarks>Updates reports type.</remarks>
        private void _CheckBoxSeparateReports_Click(object sender, RoutedEventArgs e)
        {
            _type = _GetReportType(_IsSelectedSingleDay());
        }

        /// <summary>
        /// Table element check box click event handler.
        /// </summary>
        /// <remarks>Updates buttons state.</remarks>
        private void _TableElementCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _UpdateRoutesButtonsState();
            _UpdateBuildButtonState();
        }

        /// <summary>
        /// Template table element check box click event handler.
        /// </summary>
        /// <remarks>Updates elements of template table visible state.</remarks>
        private void _TemplateTableElementCheckBox_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(null != _templates);

            var wrapper = xceedGridTemplates.SelectedItem as SelectReportWrapper;
            if (null == wrapper)
                // click to sub report
                _UpdateSubReportsGui();
            else
                // check to master report
                _UpdateMasterReportGui(wrapper);

            _UpdateRoutesButtonsState();
            _UpdateBuildButtonState();

            e.Handled = true;
        }

        /// <summary>
        /// Project preclose event handler.
        /// </summary>
        /// <remarks>Free reports components.</remarks>
        private void _ProjectPreClose(object sender, EventArgs e)
        {
            _FreeReports();
        }

        /// <summary>
        /// Button Preview click event handler.
        /// </summary>
        /// <remarks>Starts reports preview process.</remarks>
        private void ButtonPreview_Click(object sender, RoutedEventArgs e)
        {
            _DoReportsProcess(ProcessType.Preview);
        }

        /// <summary>
        /// Button Print click event handler.
        /// </summary>
        /// <remarks>Starts reports print process.</remarks>
        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            _DoReportsProcess(ProcessType.Print);
        }

        /// <summary>
        /// Button Save click event handler.
        /// </summary>
        /// <remarks>Starts reports save process.</remarks>
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _DoReportsProcess(ProcessType.Save);
        }

        /// <summary>
        /// Calendar selected dates changed event handler.
        /// </summary>
        /// <remarks>Updates GUI.</remarks>
        private void _calendarWidget_SelectedDatesChanged(object sender, EventArgs e)
        {
            _UpdateSchedules();
            _UpdateGui();
        }

        /// <summary>
        /// Button enabled changed event handler.
        /// </summary>
        /// <remarks>Updates tooltip for relative button.</remarks>
        private void button_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var button = sender as Button;
            Debug.Assert(null != button);

            string tooltipRscEnabled = null;
            string tooltipRscDisabled = null;
            if (button == buttonPreview)
            {
                tooltipRscEnabled = "PreviewReportButtonEnabledTooltip";
                tooltipRscDisabled = "PreviewReportButtonDisabledTooltip";
            }
            else if (button == buttonPrint)
            {
                tooltipRscEnabled = "PrintReportButtonEnabledTooltip";
                tooltipRscDisabled = "PrintReportButtonDisabledTooltip";
            }
            else if (button == buttonSave)
            {
                tooltipRscEnabled = "SaveReportButtonEnabledTooltip";
                tooltipRscDisabled = "SaveReportButtonDisabledTooltip";
            }
            else if (button == buttonGenerate)
            {
                tooltipRscEnabled = "BuildReportsCommandEnabledTooltip";
                tooltipRscDisabled = "BuildReportsCommandDisabledTooltip";
            }
            else
            {
                Debug.Assert(false); // not supported
            }

            string tooltipResource = (button.IsEnabled) ?
                                        tooltipRscEnabled : tooltipRscDisabled;
            Debug.Assert(!string.IsNullOrEmpty(tooltipResource));

            button.ToolTip = App.Current.FindString(tooltipResource);
        }

        /// <summary>
        /// Preview mouse left button down event handler.
        /// </summary>
        /// <remarks>Updates reports buttons state.</remarks>
        private void _exceedGridRow_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            _UpdateReportsButtonsState();
        }

        /// <summary>
        /// Mouse double click event handler.
        /// </summary>
        /// <remarks>Starts preview report process.</remarks>
        private void _exceedGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _DoReportsProcess(ProcessType.Preview);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Reports table On item source changed event handler.
        /// </summary>
        /// <remarks>Updates reports buttons state.</remarks>
        private void _xceedGridReports_OnItemSourceChanged(object sender, EventArgs e)
        {
            _UpdateReportsButtonsState();
        }

        /// <summary>
        /// Report template table On item source changed event handler.
        /// </summary>
        /// <remarks>Shows all subreports.</remarks>
        private void _xceedGridTemplates_OnItemSourceChanged(object sender, EventArgs e)
        {
            _ExpandAllDetail();
        }

        /// <summary>
        /// Cancel button click event handler.
        /// </summary>
        /// <remarks>Does cancel create reports process.</remarks>
        private void _ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ReportsGenerator generator = App.Current.ReportGenerator;
            generator.AbortCreateReports();
        }

        /// <summary>
        /// Async. solve completed  event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Async. solve completed event arguments.</param>
        private void _Cmd_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            if (e.OperationId.Equals(_operationID))
            {
                Debug.Assert(0 < _listOfRoutes.Count);

                // process operation results

                if (e.Error != null)
                    // error occured during operation
                    _OnSolveError(e.Error);

                else if (e.Cancelled)
                {   // operation was cancelled
                    _listOfRoutes.Clear();
                    App.Current.UIManager.Unlock();
                    WorkingStatusHelper.SetReleased();

                    var message = App.Current.FindString("GenerateDirectionsCancelledText");
                    App.Current.Messenger.AddInfo(message);
                }

                else
                {   // operation successed
                    _listOfRoutes.RemoveAt(0);

                    var message = App.Current.FindString("GenerateDirectionsCompletedText");
                    App.Current.Messenger.AddInfo(message);

                    if (0 == _listOfRoutes.Count)
                    {   // all routes updated
                        try
                        {
                            App.Current.Project.Save();
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }

                        WorkingStatusHelper.SetReleased();
                    }

                    // restart generation procedure
                    _DoGenerationProcess();
                }
            }

            else
            {
                // update page content
                _UpdateSchedules();
                _UpdateGui();
            }
        }

        /// <summary>
        /// Generator create reports completed event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Create reports completed event arguments.</param>
        private void generator_CreateReportsCompleted(object sender,
                                                      CreateReportsCompletedEventArgs e)
        {
            bool finished = true;

            // detouch events
            ReportsGenerator generator = App.Current.ReportGenerator;
            generator.CreateReportsCompleted -= generator_CreateReportsCompleted;

            Debug.Assert(null != _buttonCancel);
            _buttonCancel.Click -= _ButtonCancel_Click;

            // process resultates
            string statusMessage = null;
            if (e.Error != null)
            {   // error occured during operation
                statusMessage = App.Current.FindString("ReportFailedMessage");
                _PopulateReportError(e.Error);
            }
            else if (e.Cancelled)
            {   // operation was cancelled
                statusMessage = App.Current.FindString("ReportCancelledMessage");
                App.Current.Messenger.AddWarning(statusMessage);
            }
            else
            {   // operation successes ending
                ICollection<ReportStateDescription> createdReports = e.Reports;
                Debug.Assert(null != createdReports);
                Debug.Assert(0 < createdReports.Count);

                // if build reports separately
                if (0 < _waitReports.Count)
                {
                    // store created reports
                    if (null == _processor.Reports)
                        _processor.Reports = new List<ReportStateDescription>();
                    foreach (ReportStateDescription report in createdReports)
                        _processor.Reports.Add(report);

                    // remove from wait reports
                    if (1 == createdReports.Count)
                    {
                        ReportStateDescription createdReport = createdReports.First();
                        string createdReportName = createdReport.ReportName;
                        if (_waitReports.Keys.Contains(createdReportName))
                        {
                            _waitReports.Remove(createdReportName);
                        }
                    }

                    createdReports = _processor.Reports;
                }

                if (0 < _waitReports.Count)
                {
                    // do next report creation
                    _buttonCancel.Click += new RoutedEventHandler(_ButtonCancel_Click);

                    _StartFirstWaitReportGeneration();
                    finished = false;
                }
                else
                {
                    // build operation done

                    // sort by template name
                    ICollection<ReportInfo> reportInfos = _GetSelectedTemplates();
                    _processor.Reports = _SortReportsByTemplateName(reportInfos, createdReports);
                    _InitReportsTable();

                    statusMessage = App.Current.FindString("ReportSuccessedMessage");
                    App.Current.Messenger.AddInfo(statusMessage);
                }
            }

            if (finished)
            {   // do GUI to work state
                _statusLabel = null;
                _buttonCancel = null;

                _waitReports.Clear();

                App.Current.MainWindow.StatusBar.SetStatus(this, statusMessage);

                App.Current.UIManager.Unlock();
            }
        }

        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits check box in greed.
        /// </summary>
        /// <param name="columns">Greed colums.</param>
        /// <param name="cellEditorName">CheckBox cell editor resource name.</param>
        private void _InitCheckBox(ColumnCollection columns, string cellEditorName)
        {
            Debug.Assert(null != columns);
            Debug.Assert(!string.IsNullOrEmpty(cellEditorName));

            ColumnBase columnCkecked = columns[CHECKBOX_COLUMN_NAME];
            columnCkecked.CellEditor = (CellEditor)mainGrid.FindResource(cellEditorName);
            columnCkecked.Width = 25;
        }

        /// <summary>
        /// Inits grid structure.
        /// </summary>
        /// <param name="source">Grid view source.</param>
        /// <param name="xceedGrid">Grid object.</param>
        /// <param name="sourceXAML">Source greed XAML-file.</param>
        /// <param name="cellEditorName">CheckBox cell editor resource name (can be null).</param>
        private void _InitGridStructure(DataGridCollectionViewSource source,
                                        DataGridControl xceedGrid,
                                        string sourceXAML,
                                        string cellEditorName)
        {
            Debug.Assert(null != source);
            Debug.Assert(null != xceedGrid);
            Debug.Assert(!string.IsNullOrEmpty(sourceXAML));

            var structureInitializer = new GridStructureInitializer(sourceXAML);
            structureInitializer.BuildGridStructure(source, xceedGrid);
            if (!string.IsNullOrEmpty(cellEditorName))
                _InitCheckBox(xceedGrid.Columns, cellEditorName);
        }

        /// <summary>
        /// Inits page state.
        /// </summary>
        private void _Init()
        {
            Debug.Assert(!_isInited);

            _viewSourceRoutes = mainGrid.FindResource(ROUTES_TABLE_CONFIGURATION_NAME)
                                    as DataGridCollectionViewSource;
            _InitGridStructure(_viewSourceRoutes, xceedGridRoutes,
                               GridSettingsProvider.ReportRoutesGridStructure,
                               CELL_EDITOR_RESOURCE_NAME);

            _viewSourceTemplates = mainGrid.FindResource(TEMPLATES_TABLE_CONFIGURATION_NAME)
                                        as DataGridCollectionViewSource;
            _InitGridStructure(_viewSourceTemplates, xceedGridTemplates,
                               GridSettingsProvider.ReportTemplatesGridStructure,
                               CELL_EDITOR_RESOURCE_NAME_EX);

            // init template detail structure
            var subReport =
                 mainGrid.FindResource(REPORT_TEMPLATE_DETAIL_CONFIGURATION_NAME) as DetailConfiguration;
            var detailInitializer =
                new GridStructureInitializer(GridSettingsProvider.ReportTemplatesGridStructure);
            detailInitializer.BuildDetailStructure(_viewSourceTemplates, xceedGridTemplates,
                                                   subReport);
            _InitCheckBox(subReport.Columns, CELL_EDITOR_RESOURCE_NAME_EX);
            // subscribe on value change event of ItemSource property
            var dpd = DependencyPropertyDescriptor.FromProperty(DataGridControl.ItemsSourceProperty,
                                                                typeof(DataGridControl));
            dpd.AddValueChanged(xceedGridTemplates, _xceedGridTemplates_OnItemSourceChanged);

            _viewSourceReports = mainGrid.FindResource(REPORTS_TABLE_CONFIGURATION_NAME)
                                    as DataGridCollectionViewSource;
            _InitGridStructure(_viewSourceReports, xceedGridReports,
                               GridSettingsProvider.ReportReportsGridStructure, null);
            _viewSourceReports.Source = new List<ReportDataWrapper>();
            xceedGridReports.OnItemSourceChanged +=
                new EventHandler(_xceedGridReports_OnItemSourceChanged);

            _isInited = true;
        }

        /// <summary>
        /// Gets calendar widget.
        /// </summary>
        /// <returns>Calendar widget.</returns>
        private DateRangeCalendarWidget _GetCalendarWidget()
        {
            Debug.Assert(this.EditableWidgetCollection[0] is DateRangeCalendarWidget);
            return this.EditableWidgetCollection[0] as DateRangeCalendarWidget;
        }

        /// <summary>
        /// Checks is single day selected.
        /// </summary>
        /// <returns>TRUE if selecetd one day, FALSE - selected date range.</returns>
        private bool _IsSelectedSingleDay()
        {
            DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
            return (calendarWidget.StartDate == calendarWidget.EndDate);
        }

        /// <summary>
        /// Gets report type.
        /// </summary>
        /// <param name="isSingleDaySelect">Single day selected flag.</param>
        /// <returns>Report type.</returns>
        private ReportType _GetReportType(bool isSingleDaySelect)
        {
            return (!isSingleDaySelect) ? ReportType.DateRange :
                                          ((bool)checkSeparateReports.IsChecked) ?
                                                ReportType.SingleDaySeparate :
                                                ReportType.SingleDay;
        }

        /// <summary>
        /// Gets message for case schedule not founded.
        /// </summary>
        /// <returns>Formated message.</returns>
        private string _GetMessageScheduleNotFounded()
        {
            DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
            return App.Current.GetString("ExportNotFoundScheduleMessageFormat",
                                         calendarWidget.StartDate.ToShortDateString(),
                                         calendarWidget.EndDate.ToShortDateString());
        }

        /// <summary>
        /// Inits routes table. Update source collection.
        /// </summary>
        /// <param name="isSingleDaySelect">Single day selected flag.</param>
        /// <param name="isChecked">Selected flag.</param>
        private void _InitRoutesTable(bool isSingleDaySelect, bool isChecked)
        {
            _viewSourceRoutes.Source = new List<SelectPropertiesWrapper>();

            string statusMessage = null;
            if (!isSingleDaySelect)
            {
                statusMessage = (0 == _schedulesToReport.Count) ? _GetMessageScheduleNotFounded() : null;
            }
            else
            {
                var routesWrap = new List<SelectPropertiesWrapper>();

                DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
                Schedule schedule = ScheduleHelper.GetCurrentScheduleByDay(calendarWidget.StartDate);
                if (!ScheduleHelper.DoesScheduleHaveBuiltRoutes(schedule))
                    statusMessage = _GetMessageScheduleNotFounded();
                else
                {
                    var sortedRoutes =
                        new SortedDataObjectCollection<Route>(schedule.Routes, new RoutesComparer());
                    foreach (Route route in sortedRoutes)
                    {
                        if ((null != route.Stops) && (0 < route.Stops.Count))
                            routesWrap.Add(new SelectPropertiesWrapper(route.Name, null, isChecked));
                    }

                    _viewSourceRoutes.Source = routesWrap;
                    statusMessage = null;
                }
            }

            App.Current.MainWindow.StatusBar.SetStatus(this, statusMessage);
        }

        /// <summary>
        /// Expands detail for selected report in report templates table.
        /// </summary>
        private void _ExpandDetail(SelectReportWrapper template)
        {
            Debug.Assert(null != template);

            try
            {
                _viewSourceTemplates.View.Refresh();
                // WORKAROUND: autkin : need call after source updating
                //                      without this code ExpandDetails do not work.
            }
            catch
            { }

            foreach (SelectReportWrapper wp in xceedGridTemplates.Items)
            {
                if (wp.Name.Name == template.Name.Name)
                {
                    if (0 < wp.SubReportWrappers.Count)
                        xceedGridTemplates.ExpandDetails(wp);
                    break; // operation done
                }
            }
        }

        /// <summary>
        /// Expands all detail in report templates table.
        /// </summary>
        private void _ExpandAllDetail()
        {
            if (null == _viewSourceTemplates.View)
                return; // inconsistent - stop operation

            try
            {
                _viewSourceTemplates.View.Refresh();
                // WORKAROUND: autkin : need call after source updating
                //                      without this code ExpandDetails do not work.
            }
            catch
            { } // do nothing

            foreach (SelectReportWrapper wp in xceedGridTemplates.Items)
            {
                if (0 < wp.SubReportWrappers.Count)
                    xceedGridTemplates.ExpandDetails(wp);
            }
        }

        /// <summary>
        /// Updates sub reports GUI state.
        /// </summary>
        private void _UpdateSubReportsGui()
        {
            IList contexts = _SelectedItemsFromAllContexts(xceedGridTemplates);
            var wrapper = contexts[0] as SelectReportWrapper;
            Debug.Assert(null != wrapper);

            foreach (SelectReportWrapper template in _templates)
            {   // find master report for this sub report
                bool isSubTemplateFromThisReport = false;
                foreach (SelectReportWrapper subTemplate in template.SubReportWrappers)
                {
                    if (subTemplate.Name.Name == wrapper.Name.Name)
                    {
                        isSubTemplateFromThisReport = true;
                        if (isSubTemplateFromThisReport)
                            break; // NOTE: result founded
                    }
                }

                if (isSubTemplateFromThisReport)
                {
                    _UpdateTemplateState(template, wrapper);
                    break; // NOTE: report template wrapper updated
                }
            }
        }

        /// <summary>
        /// Updates master report GUI state.
        /// </summary>
        /// <param name="wrapper">Selected wrapper.</param>
        private void _UpdateMasterReportGui(SelectReportWrapper wrapper)
        {
            Debug.Assert(null != wrapper);

            foreach (SelectReportWrapper template in _templates)
            {
                if (template.Name.Name == wrapper.Name.Name)
                {
                    if (0 < wrapper.SubReportWrappers.Count)
                    {   // do only for master report with subreports
                        bool isChecked = template.IsChecked;
                        if (wrapper.IsChecked)
                            _ExpandDetail(wrapper);
                        else
                            _UpdateTemplateState(template, wrapper);
                        template.IsChecked = isChecked; // WORKAROUND: autkin:
                        // after calling _viewSourceTemplates.View.Refresh();
                        // CheckBox return to previosly state
                    }

                    break; // NOTE: report template wrapper updated
                }
            }
        }

        /// <summary>
        /// Inits templates table. Update source collection.
        /// </summary>
        /// <param name="isSelectedSingleDay">Is selected single day flag.</param>
        private void _InitTemplatesTable(bool isSelectedSingleDay)
        {
            _templates.Clear();

            // reinit report template table source
            ReportsGenerator generator = App.Current.ReportGenerator;
            foreach (string name in generator.GetPresentedNames(false))
            {
                ReportInfo info = generator.GetReportInfo(name);
                if (isSelectedSingleDay ||
                    (!isSelectedSingleDay && (ContextType.Schedule == info.Context)))
                {
                    // init subreports
                    var subReports = new List<SelectReportWrapper>();
                    foreach (SubReportInfo subReport in info.SubReports)
                    {
                        if (subReport.IsVisible)
                            subReports.Add(new SelectReportWrapper(subReport.Name,
                                                                   subReport.Description,
                                                                   false, false,
                                                                   new List<SelectReportWrapper>()));
                    }

                    _templates.Add(new SelectReportWrapper(info.Name, info.Description,
                                                           false, false, subReports));
                }
            }

            // update GUI
            _viewSourceTemplates.Source = _templates;

            _ExpandAllDetail();

            buttonGenerate.IsEnabled = false;
        }

        /// <summary>
        /// Inits reports table. Update source collection.
        /// </summary>
        private void _InitReportsTable()
        {
            var reportWrappers = new List<ReportDataWrapper>();

            IList<ReportStateDescription> createdReports = _processor.Reports;
            if (null != createdReports)
            {
                foreach (ReportStateDescription report in createdReports)
                    reportWrappers.Add(new ReportDataWrapper(report.ReportName));
            }
            _viewSourceReports.Source = reportWrappers;
        }

        /// <summary>
        /// Sets routes to selected\unselected state.
        /// </summary>
        /// <param name="isChecked">Selected flag.</param>
        private void _SetAllRoutesToOneState(bool isChecked)
        {
            Debug.Assert(null != _viewSourceRoutes.Source);
            var routesWrap = _viewSourceRoutes.Source as List<SelectPropertiesWrapper>;

            // NOTE: XceedGrid workaround:for support real CheckBox state - need recreate items
            var routesWrapUpdated = new List<SelectPropertiesWrapper>();
            foreach (SelectPropertiesWrapper routeWrap in routesWrap)
                routesWrapUpdated.Add(new SelectPropertiesWrapper(routeWrap.Name, null, isChecked));
            _viewSourceRoutes.Source = routesWrapUpdated;

            _UpdateRoutesButtonsState();
            _UpdateBuildButtonState();
        }

        /// <summary>
        /// Updates routes buttons state.
        /// </summary>
        private void _UpdateRoutesButtonsState()
        {
            bool uncheckAllEnable = false;
            bool checkAllEnable = false;
            if (_type != ReportType.DateRange)
            {
                ICollection<Route> routes = _GetSelectedRoutes();
                if (0 < routes.Count)
                    uncheckAllEnable = (0 < routes.Count);

                if (0 < _schedulesToReport.Count)
                {
                    int buildedRoutesNumber = 0;
                    foreach (Route route in _schedulesToReport.First().Routes)
                    {
                        if ((null != route.Stops) && (0 < route.Stops.Count))
                            ++buildedRoutesNumber;
                    }
                    checkAllEnable = (buildedRoutesNumber != routes.Count);
                }
            }

            buttonUncheckAll.IsEnabled = uncheckAllEnable;
            buttonCheckAll.IsEnabled = checkAllEnable;
        }

        /// <summary>
        /// Update build button state.
        /// </summary>
        private void _UpdateBuildButtonState()
        {
            // NOTE: reports can generated:
            buttonGenerate.IsEnabled =
                //  if selected one or more report(s) template AND
                ((0 < _GetSelectedTemplates().Count) &&
                //  for single date selected one or more route(s) OR
                (((_type != ReportType.DateRange) &&
                  (0 < _GetSelectedRoutes().Count)) ||
                //  for date range present one or more proper schedule(s) AND
                (((_type == ReportType.DateRange) &&
                  (0 < _schedulesToReport.Count)))));
        }

        /// <summary>
        /// Updates GUI elements.
        /// </summary>
        private void _UpdateGui()
        {
            Debug.Assert(_isInited);

            checkSeparateReports.IsChecked = false;

            bool isSingleDaySelect = _IsSelectedSingleDay();
            _type = _GetReportType(isSingleDaySelect);
            checkSeparateReports.IsEnabled = xceedGridRoutes.IsEnabled = isSingleDaySelect;

            _InitRoutesTable(isSingleDaySelect, false);
            _InitTemplatesTable(isSingleDaySelect);

            _UpdateRoutesButtonsState();
            _UpdateBuildButtonState();
            _UpdateReportsButtonsState();
        }

        /// <summary>
        /// Updates report buttons state.
        /// </summary>
        private void _UpdateReportsButtonsState()
        {
            buttonPrint.IsEnabled =
                buttonSave.IsEnabled =
                    buttonPreview.IsEnabled =
                        ((null != xceedGridReports.SelectedItems) &&
                         (0 < xceedGridReports.SelectedItems.Count));
        }

        /// <summary>
        /// Updates report template state.
        /// </summary>
        /// <param name="template">Current template.</param>
        /// <param name="wrapper">Subreport template.</param>
        private void _UpdateTemplateState(SelectReportWrapper template, SelectReportWrapper wrapper)
        {
            Debug.Assert(null != template);
            Debug.Assert(null != wrapper);

            //
            // WORKAROUND: autkin : CheckBox in xceedGreed work incorrect
            //

            // store check state
            bool isWrapperChecked = wrapper.IsChecked;

            // WORKAROUND - remove report in old state
            int index = _templates.IndexOf(template);
            _templates.RemoveAt(index);
            _viewSourceTemplates.View.Refresh();

            bool isChangedMasterReport = (0 < wrapper.SubReportWrappers.Count);
            if (isChangedMasterReport)
            {
                Debug.Assert(!isWrapperChecked);

                // when master report unselected - unselect all sub reports
                foreach (SelectReportWrapper subTemplate in template.SubReportWrappers)
                    subTemplate.IsChecked = false;

                template.IsChecked = isWrapperChecked; // restore real state
            }
            else
            {
                // if master report not selected - need select it
                if (!template.IsChecked && isWrapperChecked)
                    template.IsChecked = true;

                // restore sub report state
                foreach (SelectReportWrapper subTemplate in template.SubReportWrappers)
                {
                    if (subTemplate.Name.Name == wrapper.Name.Name)
                        subTemplate.IsChecked = isWrapperChecked;
                }
            }

            // update name (can changed by select\deselect sub reports)
            ReportInfo currentInfo = _GetReportInfoWithSelectedSubReports(template);
            ReportsGenerator generator = App.Current.ReportGenerator;
            template.Name = new SpecialName(template.Name.Name,
                                            generator.IsReportEnforceSplitted(currentInfo));

            // WORKAROUND - insert report in new state to last position.
            //              Only this case xceedGreed real updated view.
            _templates.Insert(index, template);

            // table source updated - need refresh
            _viewSourceTemplates.View.Refresh();
        }

        /// <summary>
        /// Gets all selected items list.
        /// </summary>
        /// <param name="xceed">Xceed control.</param>
        public IList _SelectedItemsFromAllContexts(DataGridControl xceed)
        {
            Debug.Assert(null != xceed);

            var selection = new List<object>(xceed.SelectedItems.Cast<object>().ToArray());

            IEnumerable<DataGridContext> childContexts = xceed.GetChildContexts();
            foreach (DataGridContext dataGridContext in childContexts)
                selection.AddRange(dataGridContext.SelectedItems);

            return selection.AsReadOnly();
        }

        #region Generate reports helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks solve started on seleceted date range.
        /// </summary>
        /// <returns>TRUE if on seleceted date range has solve operation.</returns>
        private bool _IsSolveStartedOnSelectedDates()
        {
            bool result = false;
            IVrpSolver solver = App.Current.Solver;
            if ((null != solver) &&
                solver.HasPendingOperations)
            {
                DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
                DateTime day = calendarWidget.StartDate;
                while (!result &&
                       day <= calendarWidget.EndDate)
                {
                    result = (0 < solver.GetAsyncOperations(day).Count);
                    day = day.AddDays(1);
                }
            }

            return result;
        }

        /// <summary>
        /// Updates report schedules.
        /// </summary>
        private void _UpdateSchedules()
        {
            _schedulesToReport.Clear();

            // create list of not empty schedule in date range
            DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();

            DateTime day = calendarWidget.StartDate;
            while (day <= calendarWidget.EndDate)
            {
                Schedule schedule = ScheduleHelper.GetCurrentScheduleByDay(day);
                if (null != schedule)
                {
                    if (ScheduleHelper.DoesScheduleHaveBuiltRoutes(schedule))
                        _schedulesToReport.Add(schedule);
                }

                day = day.AddDays(1);
            }
        }

        /// <summary>
        /// Gets seleceted routes.
        /// </summary>
        /// <returns>Seleceted routes.</returns>
        public ICollection<Route> _GetSelectedRoutes()
        {
            Debug.Assert(_type != ReportType.DateRange);

            var routes = new List<Route>();
            if (0 < _schedulesToReport.Count)
            {
                Debug.Assert(1 == _schedulesToReport.Count);

                IDataObjectCollection<Route> scheduleRoutes = _schedulesToReport.First().Routes;

                Debug.Assert(null != _viewSourceRoutes.Source);
                var routesWrap = _viewSourceRoutes.Source as IList<SelectPropertiesWrapper>;
                foreach (SelectPropertiesWrapper routeWrap in routesWrap)
                {
                    if (false == routeWrap.IsChecked)
                        continue; // NOTE: skip not selected routes

                    foreach (Route route in scheduleRoutes)
                    {
                        if (routeWrap.Name == route.Name)
                        {
                            routes.Add(route);
                            break; // operation done
                        }
                    }
                }
            }

            return routes.AsReadOnly();
        }

        /// <summary>
        /// Gets selected templates.
        /// </summary>
        /// <returns>Selected report templates.</returns>
        private ICollection<ReportInfo> _GetSelectedTemplates()
        {
            var reports = new List<ReportInfo>();

            ReportsGenerator generator = App.Current.ReportGenerator;
            foreach (SelectReportWrapper template in _templates)
            {
                if (template.IsChecked)
                {
                    ReportInfo selectedReport = _GetReportInfoWithSelectedSubReports(template);
                    reports.Add(selectedReport);
                }
            }

            return reports.AsReadOnly();
        }

        /// <summary>
        /// Gets hard (big consumption of memory) report templates.
        /// </summary>
        /// <param name="reportInfos">Report templates</param>
        /// <returns>Hard report templates from input list.</returns>
        private IList<ReportInfo> _GetHardRepotrs(ICollection<ReportInfo> reportInfos)
        {
            Debug.Assert(null != reportInfos);

            var reports = new List<ReportInfo>();

            ReportsGenerator generator = App.Current.ReportGenerator;
            foreach (ReportInfo info in reportInfos)
            {
                if (generator.IsReportEnforceSplitted(info))
                    reports.Add(info);
            }

            return reports;
        }

        /// <summary>
        /// Gets filtred report templates.
        /// </summary>
        /// <param name="allReportInfos">All supported report templates.</param>
        /// <param name="ignoredReportInfos">Ignored report templates.</param>
        /// <returns>Filtred report template list</returns>
        private IList<ReportInfo> _GetFiltredReports(ICollection<ReportInfo> allReportInfos,
                                                     ICollection<ReportInfo> ignoredReportInfos)
        {
            Debug.Assert(null != allReportInfos);
            Debug.Assert(null != ignoredReportInfos);

            var reports = new List<ReportInfo>();

            ReportsGenerator generator = App.Current.ReportGenerator;
            foreach (ReportInfo info in allReportInfos)
            {
                if (!ignoredReportInfos.Contains(info))
                    reports.Add(info);
            }

            return reports;
        }

        /// <summary>
        /// Sorts report by template name.
        /// </summary>
        /// <param name="reportInfos">Report infos.</param>
        /// <param name="reportDescriptions">Report descriptions.</param>
        /// <returns>Sorted report list.</returns>
        private IList<ReportStateDescription> _SortReportsByTemplateName(
            ICollection<ReportInfo> reportInfos,
            ICollection<ReportStateDescription> reportDescriptions)
        {
            Debug.Assert(null != reportInfos);
            Debug.Assert(null != reportDescriptions);

            var reports = new List<ReportStateDescription>();
            foreach (ReportInfo info in reportInfos)
            {
                foreach (ReportStateDescription description in reportDescriptions)
                {
                    if (info.TemplatePath == description.ReportInfo.TemplatePath)
                        reports.Add(description);
                }
            }

            return reports;
        }

        /// <summary>
        /// Creates generate directions route list.
        /// </summary>
        private void _CreateGenerateDirectionsRouteList()
        {
            Debug.Assert(0 == _listOfRoutes.Count);
            _listOfRoutes.Clear();

            foreach (Schedule schedule in _schedulesToReport)
            {
                var routesWihtoutGeometry = new List<Route>();
                foreach (Route route in schedule.Routes)
                {
                    IDataObjectCollection<Stop> stops = route.Stops;
                    if ((null == stops) || (0 == stops.Count))
                        continue; // NOTE: skip empty route

                    bool needDirectionsGenerate = true;
                    foreach (Stop stop in stops)
                    {
                        if ((StopType.Location == stop.StopType) ||
                            (StopType.Order == stop.StopType))
                        {
                            if ((null != stop.Directions) && (0 < stop.Directions.Length))
                            {
                                needDirectionsGenerate = false;
                                break; // result found
                            }
                        }
                    }

                    if (needDirectionsGenerate)
                        routesWihtoutGeometry.Add(route);
                }

                if (0 < routesWihtoutGeometry.Count)
                    _listOfRoutes.Add(routesWihtoutGeometry);
            }
        }

        /// <summary>
        /// Does generation directions. Start solver operation.
        /// </summary>
        private void _DoGenerateDirections()
        {
            Debug.Assert(0 < _listOfRoutes.Count);

            WorkingStatusHelper.SetBusy(App.Current.FindString("GenerateDirections"));

            Route route = _listOfRoutes[0][0];
            var message = App.Current.GetString("GenerateDirectionsStartText",
                                                route.Schedule.PlannedDate.Value.ToShortDateString());
            App.Current.Messenger.AddInfo(message);

            try
            {
                _operationID = App.Current.Solver.GenerateDirectionsAsync(_listOfRoutes[0]);
            }
            catch (Exception e)
            {
                _OnSolveError(e);
            }
        }

        /// <summary>
        /// Inits status stack control.
        /// </summary>
        private void _InitStatusStack()
        {
            // load status control
            StackPanel status = (StackPanel)App.Current.FindResource("textStatusStack");
            foreach (UIElement element in status.Children)
            {
                var label = element as Label;
                var progress = element as ProgressBar;
                var button = element as Button;

                if (null != label)
                {
                    if (null == _statusLabel)
                        _statusLabel = label;
                }

                else if (null != button)
                    _buttonCancel = button;

                // else Do nothing
            }

            // init status state
            _statusLabel.Content = App.Current.FindString("ReportStartMessage");

            _buttonCancel.IsEnabled = true;
            _buttonCancel.Click += new RoutedEventHandler(_ButtonCancel_Click);

            App.Current.MainWindow.StatusBar.SetStatus(this, status);
        }

        /// <summary>
        /// Creates reports for all routes in selected scdedules (Asynchronous).
        /// </summary>
        /// <param name="reportInfos">Report template info to creation.</param>
        /// <param name="schedules">Schedules for data reports.</param>
        /// <param name="routes">Selected routes from schedule (can be null).</param>
        /// <returns>List of created reports.</returns>
        /// <remarks>Routes must belong to the <c>schedule</c>. If <c>routes</c> collection is empty,
        /// Generator will use all the routes from the <c>schedules</c>.</remarks>
        private void _StartCreateReportsAsync(IDictionary<string, ReportInfo> reportInfos,
                                              ICollection<Schedule> schedules,
                                              ICollection<Route> routes)
        {
            Debug.Assert(null != reportInfos);
            Debug.Assert(null != schedules);

            ReportsGenerator generator = App.Current.ReportGenerator;
            generator.CreateReportsCompleted +=
                new CreateReportsCompletedEventHandler(generator_CreateReportsCompleted);
            generator.CreateReportsAsync(reportInfos, schedules, routes);
        }

        /// <summary>
        /// Starts first wait report build source process.
        /// </summary>
        private void _StartFirstWaitReportGeneration()
        {
            Debug.Assert(null != _waitReports);
            Debug.Assert(0 < _waitReports.Count);

            CreateReportDescription description = _waitReports.Values.First();

            var reports = new Dictionary<string, ReportInfo>();
            reports.Add(description.ReportName, description.ReportInfo);

            _StartCreateReportsAsync(reports, description.Schedules, description.Routes);
        }

        /// <summary>
        /// Inits wait(lazy) report collection.
        /// </summary>
        /// <param name="reportInfos">Report infos.</param>
        /// <param name="schedule">Schedule object.</param>
        /// <param name="routes">Route list.</param>
        private void _InitWaitReports(ICollection<ReportInfo> reportInfos,
                                      Schedule schedule,
                                      ICollection<Route> routes)
        {
            Debug.Assert(null != reportInfos);
            Debug.Assert(null != schedule);
            Debug.Assert(null != routes);

            Debug.Assert(null != _waitReports);
            Debug.Assert(0 == _waitReports.Count);

            ReportsGenerator generator = App.Current.ReportGenerator;

            ICollection<Schedule> schedules =
                CommonHelpers.CreateCollectionWithOneObject(schedule);
            string formattedDate = schedule.PlannedDate.Value.ToShortDateString();

            foreach (ReportInfo info in reportInfos)
            {
                foreach (Route route in routes)
                {
                    Debug.Assert(schedule.PlannedDate.HasValue);
                    var reportName = App.Current.GetString("ReportNameFormatSingleDaySeparate",
                                                           info.Name,
                                                           formattedDate,
                                                           route.Name);

                    var description = new CreateReportDescription(reportName,
                                                                  info,
                                                                  schedules,
                                                                  route);
                    _waitReports.Add(reportName, description);
                }
            }
        }

        /// <summary>
        /// Does separated reports.
        /// </summary>
        private void _DoSeparateReports()
        {
            _InitStatusStack();

            _StartFirstWaitReportGeneration();
        }

        /// <summary>
        /// Does reports.
        /// </summary>
        /// <param name="reportInfos">Report infos.</param>
        /// <param name="schedules">Schedule list.</param>
        /// <param name="startDate">Start date.</param>
        /// <param name="endDate">Finish date.</param>
        private void _DoReports(ICollection<ReportInfo> reportInfos,
                                ICollection<Schedule> schedules,
                                DateTime? startDate,
                                DateTime? endDate)
        {
            string startDateStr = startDate.Value.ToShortDateString();
            string finishDateStr = endDate.Value.ToShortDateString();

            var reports = new Dictionary<string, ReportInfo>();
            foreach (ReportInfo info in reportInfos)
            {
                Debug.Assert(startDate.HasValue);
                Debug.Assert(endDate.HasValue);

                string reportName = App.Current.GetString("ReportNameFormatDateRange",
                                                          info.Name,
                                                          startDateStr,
                                                          finishDateStr);
                reports.Add(reportName, info);
            }

            _InitStatusStack();

            _StartCreateReportsAsync(reports, schedules, null);
        }

        /// <summary>
        /// Does reports generation.
        /// </summary>
        /// <param name="reportInfos">Selected report infos</param>
        /// <param name="schedule">Schedule object.</param>
        /// <param name="routes">Selected routes list.</param>
        private void _DoReports(ICollection<ReportInfo> reportInfos,
                                Schedule schedule,
                                ICollection<Route> routes)
        {
            string formattedDate = schedule.PlannedDate.Value.ToShortDateString();

            var reports = new Dictionary<string, ReportInfo>();
            foreach (ReportInfo info in reportInfos)
            {
                string reportName = App.Current.GetString("ReportNameFormatSingleDay",
                                                          info.Name,
                                                          formattedDate);
                reports.Add(reportName, info);
            }

            _InitStatusStack();

            ICollection<Schedule> schedules = CommonHelpers.CreateCollectionWithOneObject(schedule);
            _StartCreateReportsAsync(reports, schedules, routes);
        }

        /// <summary>
        /// Filtr report infos.
        /// </summary>
        /// <param name="source">Source collection to filtration.</param>
        /// <param name="ignoredElems">Ignored elements.</param>
        /// <returns>Created collection of source elements with out ignored.</returns>
        private ICollection<ReportInfo> _FiltrReports(ICollection<ReportInfo> source, ICollection<ReportInfo> ignoredElems)
        {
            IList<ReportInfo> result = new List<ReportInfo>();
            foreach (ReportInfo reportInfo in source)
            {
                if (!ignoredElems.Contains(reportInfo))
                {
                    result.Add(reportInfo);
                }
            }

            return result;
        }

        /// <summary>
        /// Does report generation process.
        /// </summary>
        private void _DoReports()
        {
            _FreeReports();
            _waitReports.Clear();

            try
            {
                ICollection<ReportInfo> reportInfos = _GetSelectedTemplates();
                ICollection<ReportInfo> hardReportInfos = _GetHardRepotrs(reportInfos);
                // NOTE: hard reports use lot memory need generate single day separate

                if (ReportType.DateRange == _type)
                {   // do report for date range
                    Debug.Assert(0 == hardReportInfos.Count);
                    DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
                    _DoReports(reportInfos,
                               _schedulesToReport,
                               calendarWidget.StartDate,
                               calendarWidget.EndDate);
                }
                else
                {
                    ICollection<Route> routes = _GetSelectedRoutes();
                    if (null != routes)
                    {
                        if (ReportType.SingleDaySeparate == _type)
                        {   // create reports separately if user it's choice or hard report selected
                            Debug.Assert(1 == _schedulesToReport.Count);

                            _InitWaitReports(reportInfos, _schedulesToReport.First(), routes);

                            _DoSeparateReports();
                        }
                        else
                        {   // create reports
                            Debug.Assert(ReportType.SingleDay == _type);
                            Debug.Assert(1 == _schedulesToReport.Count);

                            // initialize hard reports construction
                            if (0 < hardReportInfos.Count)
                            {
                                // exclude hard report in start creation
                                reportInfos = _FiltrReports(reportInfos, hardReportInfos);

                                // init hard reports as lazy
                                _InitWaitReports(hardReportInfos, _schedulesToReport.First(), routes);
                            }


                            if (0 < reportInfos.Count)
                            {
                                _DoReports(reportInfos, _schedulesToReport.First(), routes);
                            }
                            else
                            {
                                _DoSeparateReports();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _PopulateReportError(ex);

                string statusMessage = App.Current.FindString("ReportFailedMessage");
                App.Current.MainWindow.StatusBar.SetStatus(this, statusMessage);
                App.Current.UIManager.Unlock();
            }
        }

        /// <summary>
        /// Does generation operation.
        /// </summary>
        private void _DoGenerationProcess()
        {
            if (0 < _listOfRoutes.Count)
                _DoGenerateDirections();
            else
                _DoReports();
        }

        #endregion // Generate reports helpers

        #region Process reports helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Free resource reports. Reomeves temparary files.
        /// </summary>
        private void _FreeReports()
        {
            _viewSourceReports.Source = new List<ReportDataWrapper>();

            _processor.FreeReports();
        }

        /// <summary>
        /// Gets selected report index.
        /// </summary>
        /// <param name="selectedItem">Selected item.</param>
        /// <returns>Report index.</returns>
        private int _GetSelectedReportIndex(ReportDataWrapper selectedItem)
        {
            Debug.Assert(null != selectedItem);

            IList<ReportStateDescription> createdReports = _processor.Reports;

            int result = -1;
            for (int index = 0; index < createdReports.Count; ++index)
            {
                if (selectedItem.Name == createdReports[index].ReportName)
                {
                    result = index;
                    break; // result founded
                }
            }

            Debug.Assert(-1 != result);
            return result;
        }

        /// <summary>
        /// Normalize subreport group.
        /// </summary>
        /// <param name="subReports">SubReport info</param>
        /// <remarks>Use default only no one subreport by some groupId not selecetd</remarks>
        private void _NormalizeSubReportGroup(IList<SubReportInfo> subReports)
        {
            Debug.Assert(null != subReports);

            List<SubReportInfo> removeSubReports = new List<SubReportInfo>();
            foreach (SubReportInfo subreport in subReports)
            {
                if (!subreport.IsDefault)
                    continue; // ignore if not default

                Debug.Assert(!string.IsNullOrEmpty(subreport.GroupId));

                foreach (SubReportInfo curSubreport in subReports)
                {
                    if (!curSubreport.IsDefault && !string.IsNullOrEmpty(curSubreport.GroupId))
                    {
                        if (curSubreport.GroupId == subreport.GroupId)
                        {
                            removeSubReports.Add(subreport);
                            break; // default report marks for deleting
                        }
                    }
                }
            }

            for (int index = 0; index < removeSubReports.Count; ++index)
                subReports.Remove(removeSubReports[index]);
        }

        /// <summary>
        /// Gets report template info with selecetd sub reports.
        /// </summary>
        /// <param name="template">Seleceted template.</param>
        /// <returns>Report info with only selected sybreports.</returns>
        private ReportInfo _GetReportInfoWithSelectedSubReports(SelectReportWrapper template)
        {
            Debug.Assert(null != template);

            ReportsGenerator generator = App.Current.ReportGenerator;
            ReportInfo info = generator.GetReportInfo(template.Name.Name);

            var subReports = new List<SubReportInfo>();
            foreach (SubReportInfo subReport in info.SubReports)
            {
                bool isSelected = false;
                if (subReport.IsDefault)
                    isSelected = true; // NOTE: added as default
                else
                {   // check user's choice
                    foreach (SelectReportWrapper subTemplate in template.SubReportWrappers)
                    {
                        if (subTemplate.Name.Name == subReport.Name)
                        {
                            isSelected = subTemplate.IsChecked;
                            break; // NOTE: result founded
                        }
                    }
                }

                // add selected subreport
                if (isSelected)
                    subReports.Add(subReport);
            }

            _NormalizeSubReportGroup(subReports);

            return new ReportInfo(info.Name,
                                  info.Context,
                                  info.TemplatePath,
                                  info.Description,
                                  info.IsPredefined,
                                  subReports);
        }

        /// <summary>
        /// Gets selected reports.
        /// </summary>
        /// <returns>List of selected reports.</returns>
        private IList<ReportStateDescription> _GetSelectedReports()
        {
            IList<ReportStateDescription> createdReports = _processor.Reports;

            var reports = new List<ReportStateDescription>();
            foreach (ReportDataWrapper selectedItem in xceedGridReports.SelectedItems)
            {
                int index = _GetSelectedReportIndex(selectedItem);
                reports.Add(createdReports[index]);
            }

            return reports;
        }

        /// <summary>
        /// Starts reports operation.
        /// </summary>
        /// <param name="type">Selected reports operation.</param>
        private void _DoReportsProcess(ProcessType type)
        {
            if (null == _processor.Reports)
                return; // ignore routine

            IList<ReportStateDescription> selectedReports = _GetSelectedReports();
            _processor.DoProcess(type, selectedReports);
        }

        #endregion // Process reports helpers

        /// <summary>
        /// Reports error occured due to failure of the specified service.
        /// </summary>
        /// <param name="service">The name of the failed service.</param>
        /// <param name="exception">The exception occured during reports building.</param>
        private void _ReportServiceError(string service, Exception exception)
        {
            Debug.Assert(exception != null);
            Debug.Assert(service != null);

            Logger.Error(exception);

            var message =
                App.Current.GetString(REPORT_SERVICE_FAILED_MESSAGE_KEY, service);

            if (exception is CommunicationException)
            {
                var detailsMessage =
                    App.Current.GetString(REPORT_SERVICE_CONNECTION_ERROR_KEY, service);

                var detail = new MessageDetail(MessageType.Error, detailsMessage);
                ICollection<MessageDetail> details =
                    CommonHelpers.CreateCollectionWithOneObject(detail);

                App.Current.Messenger.AddError(message, details);
            }
            else
            {
                CommonHelpers.AddServiceMessageWithDetail(message, service, exception);
            }
        }

        /// <summary>
        /// Shows report error in message window.
        /// </summary>
        /// <param name="exception">Exception.</param>
        private void _PopulateReportError(Exception exception)
        {
            Debug.Assert(null != exception);

            string statusMessage = App.Current.FindString("ReportFailedMessage");

            if (exception is AuthenticationException || exception is CommunicationException)
            {
                string service = App.Current.FindString("ServiceNameMap");
                _ReportServiceError(service, exception);
            }
            else
            {
                string message = string.Format(MESSAGE_FORMAT, statusMessage, exception.Message);
                App.Current.Messenger.AddError(message);
                Logger.Critical(exception);
            }
        }

        /// <summary>
        /// Handles solve error occured upond directions report generation.
        /// </summary>
        /// <param name="exception">The exception occured upon solving.</param>
        private void _OnSolveError(Exception exception)
        {
            Debug.Assert(null != exception);

            _listOfRoutes.Clear();
            App.Current.UIManager.Unlock();
            WorkingStatusHelper.SetReleased();

            var service = App.Current.FindString(SERVICE_NAME_ROUTING_KEY);
            _ReportServiceError(service, exception);
        }

        #endregion // Private methods

        #region Private types
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Class - keeper of report descritpion for creation.
        /// </summary>
        private sealed class CreateReportDescription
        {
            /// <summary>
            /// Initializes a new instance of the <c>CreateReportDescription</c> class.
            /// </summary>
            /// <param name="reportName">Name of report.</param>
            /// <param name="reportInfo">Information of report.</param>
            /// <param name="schedule">Schedule to report creation.</param>
            /// <param name="route">Route to report creation.</param>
            public CreateReportDescription(string reportName,
                                           ReportInfo reportInfo,
                                           Schedule schedule,
                                           Route route) :
                this(reportName,
                     reportInfo,
                     CommonHelpers.CreateCollectionWithOneObject(schedule),
                     route)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <c>CreateReportDescription</c> class.
            /// </summary>
            /// <param name="reportName">Name of report.</param>
            /// <param name="reportInfo">Information of report.</param>
            /// <param name="schedules">Schedules to report creation (1 schedule supported).</param>
            /// <param name="route">Route to report creation.</param>
            public CreateReportDescription(string reportName,
                                           ReportInfo reportInfo,
                                           ICollection<Schedule> schedules,
                                           Route route)
            {
                Debug.Assert(!string.IsNullOrEmpty(reportName));
                Debug.Assert(null != reportInfo);
                Debug.Assert(null != schedules);
                Debug.Assert(1 == schedules.Count);
                Debug.Assert(null != route);

                ReportName = reportName;
                ReportInfo = reportInfo;
                Schedules = schedules;
                Routes = CommonHelpers.CreateCollectionWithOneObject(route);
            }

            /// <summary>
            /// Name of report.
            /// </summary>
            public readonly string ReportName;
            /// <summary>
            /// Information of report.
            /// </summary>
            public readonly ReportInfo ReportInfo;
            /// <summary>
            /// Schedules to report creation.
            /// </summary>
            public readonly ICollection<Schedule> Schedules;
            /// <summary>
            /// Routes to report creation.
            /// </summary>
            public readonly ICollection<Route> Routes;
        }

        #endregion // Private types

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Predifined page name.
        /// </summary>
        private const string PAGE_NAME = "Reports";

        /// <summary>
        /// Resource key for the message about report building failure due to service error.
        /// </summary>
        private const string REPORT_SERVICE_FAILED_MESSAGE_KEY = "ReportServiceFailedMessage";

        /// <summary>
        /// Resource key for the routing service name string.
        /// </summary>
        private const string SERVICE_NAME_ROUTING_KEY = "ServiceNameRouting";

        /// <summary>
        /// Resource key for the message about service connection error during
        /// report building.
        /// </summary>
        private const string REPORT_SERVICE_CONNECTION_ERROR_KEY = "ReportServiceConnectionError";

        /// <summary>
        /// Routes source grid configuration resource name.
        /// </summary>
        private const string ROUTES_TABLE_CONFIGURATION_NAME = "routesTable";
        /// <summary>
        /// Templates source grid configuration resource name.
        /// </summary>
        private const string TEMPLATES_TABLE_CONFIGURATION_NAME = "templatesTable";
        /// <summary>
        /// Reports source grid configuration resource name.
        /// </summary>
        private const string REPORTS_TABLE_CONFIGURATION_NAME = "reportsTable";
        /// <summary>
        /// Templates grid detail configuration resource name.
        /// </summary>
        private const string REPORT_TEMPLATE_DETAIL_CONFIGURATION_NAME =
                                                            "reportTemplateDetailConfiguration";

        // CheckBox field conts.
        private const string CHECKBOX_COLUMN_NAME = "IsChecked";
        private const string CELL_EDITOR_RESOURCE_NAME = "CheckBoxCellEditor";
        private const string CELL_EDITOR_RESOURCE_NAME_EX = "CheckBoxCellEditorEx";

        /// <summary>
        /// Error message format.
        /// </summary>
        private const string MESSAGE_FORMAT = "{0} {1}";

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Schedules to report.
        /// </summary>
        private ICollection<Schedule> _schedulesToReport = new List<Schedule>();

        /// <summary>
        /// Inited flag.
        /// </summary>
        private bool _isInited;

        /// <summary>
        /// List of presented routes.
        /// </summary>
        private List<List<Route>> _listOfRoutes = new List<List<Route>>();
        /// <summary>
        /// Solver.Generate directions async operation ID.
        /// </summary>
        private Guid _operationID = Guid.Empty;

        /// <summary>
        /// Supported report types.
        /// </summary>
        private enum ReportType
        {
            SingleDay,
            SingleDaySeparate,
            DateRange
        }
        /// <summary>
        /// Current report type.
        /// </summary>
        ReportType _type = ReportType.SingleDay;

        /// <summary>
        /// View source for routes table.
        /// </summary>
        private DataGridCollectionViewSource _viewSourceRoutes;
        /// <summary>
        /// View source for templates table.
        /// </summary>
        private DataGridCollectionViewSource _viewSourceTemplates;
        /// <summary>
        /// View source for reports table.
        /// </summary>
        private DataGridCollectionViewSource _viewSourceReports;

        /// <summary>
        /// Current report templates.
        /// </summary>
        private IList<SelectReportWrapper> _templates = new List<SelectReportWrapper>();

        /// <summary>
        /// Report processor.
        /// </summary>
        private ReportProcessor _processor;

        /// <summary>
        /// Status control label.
        /// </summary>
        private Label _statusLabel;
        /// <summary>
        /// Status control Cancel button.
        /// </summary>
        private Button _buttonCancel;

        /// <summary>
        /// Wait report create description for separately generation (by report name).
        /// </summary>
        private Dictionary<string, CreateReportDescription> _waitReports =
            new Dictionary<string, CreateReportDescription>();

        #endregion // Private fields
    }
}
