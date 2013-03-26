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
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Navigation;

using System.Windows.Controls;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Utility;
using ESRI.ArcLogistics.Routing;
using ESRI.ArcLogistics.Export;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Widgets;
using ESRI.ArcLogistics.App.GridHelpers;
using AppCommands = ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for ExportPage.xaml
    /// </summary>
    internal partial class ExportPage : PageBase
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

        public ExportPage()
        {
            InitializeComponent();

            _isValidationEnabled = true;

            _viewSourceRoute =
                (DataGridCollectionViewSource)ctrlFields.FindResource("fieldTableRoute");
            _viewSourceStop =
                (DataGridCollectionViewSource)ctrlFields.FindResource("fieldTableStop");

            // hide GUI elements
            var lbName = (Label)ctrlGeneral.FindName("labelName");
            var tbName = (TextBox)ctrlGeneral.FindName("editName");
            var lbDescription = (Label)ctrlGeneral.FindName("labelDescription");
            var tbDescription = (TextBox)ctrlGeneral.FindName("editDescription");

            lbName.Visibility =
                tbDescription.Visibility =
                    lbDescription.Visibility =
                        tbName.Visibility = Visibility.Collapsed;

            IsRequired = true;
            IsAllowed = true;

            // attach to events
            App.Current.ApplicationInitialized += new EventHandler(_App_ApplicationInitialized);
            App.Current.ProjectClosing += new EventHandler(_ProjectClosing);
            this.Loaded += new RoutedEventHandler(_Page_Loaded);
            this.Unloaded += new RoutedEventHandler(_Page_Unloaded);
            App.Current.MainWindow.Closed += new EventHandler(_MainWindow_Closed);
        }

        #endregion // Constructors

        #region Page overrided members
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
            get { return App.Current.FindString("ExportPageCaption"); }
        }

        /// <summary>
        /// Returns page icon as a TileBrush (DrawingBrush or ImageBrush).
        /// </summary>
        public override TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("ExportProfilesBrush"); }
        }

        /// <summary>
        /// Returns name of Help Topic.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.ExportPagePath); }
        }

        /// <summary>
        /// Returns category name of commands that will be present in Tasks widget.
        /// </summary>
        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion // Page overrided members

        #region PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates widgets that are shown for all pages.
        /// </summary>
        protected override void CreateWidgets()
        {
            base.CreateWidgets();

            var calendarWidget = new DateRangeCalendarWidget("CalendarWidgetCaption");
            calendarWidget.Initialize(this);
            this.EditableWidgetCollection.Insert(0, calendarWidget);
        }

        #endregion // PageBase overrided members

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Application initialized handler.
        /// </summary>
        private void _App_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitGrids();
        }

        /// <summary>
        /// Porject closing handler.
        /// </summary>
        private void _ProjectClosing(object sender, EventArgs e)
        {
            if (_isInited)
            {   // create and store profile
                Profile profile = _CreateProfile(false);
                if (_IsDataValid() && _IsValidFieldSelection(profile, false))
                    _StoreProfile(profile);
            }

            _fieldsRouteMDB = null;
            _fieldsStopMDB = null;
            _fieldsRouteTXT = null;
            _fieldsStopTXT = null;
            _fieldsOrderTXT = null;
            _fieldsRouteSHP = null;
            _fieldsStopSHP = null;
        }

        /// <summary>
        /// "Export" button click handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonExport_Click(object sender, RoutedEventArgs e)
        {
            if (!_IsDataValid())
                App.Current.Messenger.AddError(App.Current.FindString("ProfileInvalidState"));
            else
            {   // create and store profile
                Profile profile = _CreateProfile(false);
                if (_IsValidFieldSelection(profile, true))
                {   // clear state
                    _fieldsRouteMDB = null;
                    _fieldsStopMDB = null;
                    _fieldsRouteTXT = null;
                    _fieldsStopTXT = null;
                    _fieldsOrderTXT = null;
                    _fieldsRouteSHP = null;
                    _fieldsStopSHP = null;

                    _StoreProfile(profile);

                    // start export
                    _DoExport(profile, _schedulesToExport);
                }
            }
        }

        /// <summary>
        /// "Export" button enabled state changed handler.
        /// </summary>
        private void buttonExport_IsEnabledChanged(object sender,
                                                   DependencyPropertyChangedEventArgs e)
        {
            // update button tooltip
            buttonExport.ToolTip = buttonExport.IsEnabled ? null :
                                        App.Current.FindString("ProfileDisableButtonTooltip");
        }

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        private void _Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isInited)
                _InitGrids();

            // reinit page state
            App.Current.MainWindow.StatusBar.SetStatus(this, "");

            _UpdateSchedules();

            _UpdateContent(_GetDeafultProfile());

            _UpdateGui();

            _GetCalendarWidget().SelectedDatesChanged +=
                new EventHandler(_calendarWidget_SelectedDatesChanged);

            if (null != App.Current.Solver)
            {
                App.Current.Solver.AsyncSolveCompleted +=
                        new AsyncSolveCompletedEventHandler(solver_AsyncSolveCompleted);
            }
        }

        /// <summary>
        /// Page unloaded handler.
        /// </summary>
        private void _Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // detach events
            if (null != App.Current.Solver)
                App.Current.Solver.AsyncSolveCompleted -= solver_AsyncSolveCompleted;
            _GetCalendarWidget().SelectedDatesChanged -= _calendarWidget_SelectedDatesChanged;

            // create and store profile
            Profile profile = _CreateProfile(false);
            if (_IsDataValid() && _IsValidFieldSelection(profile, false))
                _StoreProfile(profile);
        }

        /// <summary>
        /// MainWindow closed handler.
        /// </summary>
        private void _MainWindow_Closed(object sender, EventArgs e)
        {
            Exporter exporter = App.Current.Exporter;
            if (null != exporter)
            {
                exporter.AsyncExportCompleted -= exporter_AsyncExportCompleted;
                exporter.AbortExport();
            }
        }

        /// <summary>
        /// Calendar selecetd dates changed.
        /// </summary>
        private void _calendarWidget_SelectedDatesChanged(object sender, EventArgs e)
        {
            _UpdateSchedules();
            _UpdateGui();
        }

        /// <summary>
        /// Name changed handler.
        /// </summary>
        private void editName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isValidationEnabled)
                _IsValidName();
        }

        /// <summary>
        /// File full name changed handler.
        /// </summary>
        private void file_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isValidationEnabled)
                _IsValidFile();
        }

        /// <summary>
        /// Type changed handler.
        /// </summary>
        private void comboboxType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _UpdateFormat((string)e.AddedItems[0]);
            if (_isValidationEnabled && (0 < e.RemovedItems.Count))
            {   // update relative controls
                var tbFile = (TextBox)ctrlGeneral.FindName("textboxFile");
                if (FileHelpers.ValidateFilepath(tbFile.Text))
                {
                    var cbFormat = (ComboBox)ctrlGeneral.FindName("comboboxFormat");
                    ExportType type = _ConvertNames2Type((string)e.AddedItems[0], cbFormat.Text);

                    if (ExportType.Access == type)
                        tbFile.Text = Path.ChangeExtension(tbFile.Text, FILE_EXTENSION_MDB);
                    else if ((ExportType.TextRoutes == type) ||
                             (ExportType.TextStops == type) ||
                             (ExportType.TextOrders == type))
                        tbFile.Text = Path.ChangeExtension(tbFile.Text, FILE_EXTENSION_CSV);
                    else
                    {
                        Debug.Assert((ExportType.ShapeRoutes == type) ||
                                     (ExportType.ShapeStops == type));
                        tbFile.Text = Path.ChangeExtension(tbFile.Text, FILE_EXTENSION_SHP);
                    }
                }

                _IsValidType();

                _UpdateContent(_CreateProfile(true));
            }
        }

        /// <summary>
        /// Format changed handler.
        /// </summary>
        private void comboboxFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tbFile = (TextBox)ctrlGeneral.FindName("textboxFile");
            if (FileHelpers.ValidateFilepath(tbFile.Text) &&
                (0 < e.AddedItems.Count))
            {
                switch ((string)e.AddedItems[0])
                {
                    case FORMAT_FACE_NAME_ACCESS:
                        tbFile.Text = Path.ChangeExtension(tbFile.Text, FILE_EXTENSION_MDB);
                        break;
                    case FORMAT_FACE_NAME_TEXT:
                        tbFile.Text = Path.ChangeExtension(tbFile.Text, FILE_EXTENSION_CSV);
                        break;
                    case FORMAT_FACE_NAME_SHAPE:
                        tbFile.Text = Path.ChangeExtension(tbFile.Text, FILE_EXTENSION_SHP);
                        break;
                }
            }
        }

        /// <summary>
        /// "Browse" button click handler.
        /// </summary>
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            // select Save File Dialog properties
            var cbType = (ComboBox)ctrlGeneral.FindName("comboboxType");
            var cbFormat = (ComboBox)ctrlGeneral.FindName("comboboxFormat");
            if (!string.IsNullOrEmpty(cbType.Text) &&
                !string.IsNullOrEmpty(cbFormat.Text))
            {
                string defaultExtension = FILE_EXTENSION_MDB;
                string dialogFilter = FILE_DIALOG_FILTER_ACCESS;

                ExportType type = _ConvertNames2Type(cbType.Text, cbFormat.Text);
                switch (type)
                {
                    case ExportType.Access:
                        defaultExtension = FILE_EXTENSION_MDB;
                        dialogFilter = FILE_DIALOG_FILTER_ACCESS;
                        break;

                    case ExportType.TextRoutes:
                    case ExportType.TextOrders:
                    case ExportType.TextStops:
                        defaultExtension = FILE_EXTENSION_CSV;
                        dialogFilter = FILE_DIALOG_FILTER_TEXT;
                        break;

                    case ExportType.ShapeRoutes:
                    case ExportType.ShapeStops:
                        defaultExtension = FILE_EXTENSION_SHP;
                        dialogFilter = FILE_DIALOG_FILTER_SHAPE;
                        break;
                }

                // show Save File Dialog (NOTE: WPF not supported WinForms)
                var fd = new Microsoft.Win32.SaveFileDialog();
                fd.RestoreDirectory = true;
                fd.Filter = dialogFilter;
                fd.DefaultExt = defaultExtension;
                fd.FilterIndex = 1;

                if (true == fd.ShowDialog(App.Current.MainWindow))
                    // result could be true, false, or null
                    ((TextBox)ctrlGeneral.FindName("textboxFile")).Text = fd.FileName;
            }
        }

        /// <summary>
        /// Solver async solve completed handler.
        /// </summary>
        private void solver_AsyncSolveCompleted(object sender, AsyncSolveCompletedEventArgs e)
        {
            _UpdateSchedules();
            _UpdateGui();
        }

        /// <summary>
        /// Cancel button click event handler.
        /// </summary>
        /// <remarks>Does cancel create reports process.</remarks>
        private void _ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Exporter exporter = App.Current.Exporter;
            exporter.AbortExport();
        }

        /// <summary>
        /// Exporter completed event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Run worker complete event arguments.</param>
        private void exporter_AsyncExportCompleted(object sender,
                                                   RunWorkerCompletedEventArgs e)
        {
            // detouch events
            App.Current.Exporter.AsyncExportCompleted -= exporter_AsyncExportCompleted;

            Debug.Assert(null != _buttonCancel);
            _buttonCancel.Click -= _ButtonCancel_Click;

            // process resultates
            string statusMessage = null;
            Profile profile = _GetDeafultProfile();
            if (e.Error != null)
            {   // error occured during operation
                statusMessage = App.Current.GetString("ExportMessageFormatFailed",
                                                      _GetTypeFaceName(profile.Type),
                                                      profile.FilePath);
                _PopulateReportError(e.Error);
            }
            else if (e.Cancelled)
            {   // operation was cancelled
                statusMessage = App.Current.FindString("ExportMessageCancelled");
                App.Current.Messenger.AddWarning(statusMessage);
            }
            else
            {   // operation successes ending
                statusMessage = App.Current.GetString("ExportMessageFormatSucceded",
                                                      _GetTypeFaceName(profile.Type),
                                                      profile.FilePath);
                App.Current.Messenger.AddInfo(statusMessage);
            }

            // do GUI to work state
            _statusLabel = null;
            _buttonCancel = null;

            App.Current.MainWindow.StatusBar.SetStatus(this, statusMessage);
            App.Current.UIManager.Unlock();
        }

        #endregion // Event handlers

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits grids.
        /// </summary>
        private void _InitGrids()
        {
            _InitGridStructure(_viewSourceRoute, XCEED_GRID_NAME_ROUTE);
            _InitGridStructure(_viewSourceStop, XCEED_GRID_NAME_STOP);
            _isInited = true;
        }

        /// <summary>
        /// Calendar widget accessor.
        /// </summary>
        /// <returns></returns>
        private DateRangeCalendarWidget _GetCalendarWidget()
        {
            Debug.Assert(this.EditableWidgetCollection[0] is DateRangeCalendarWidget);
            return this.EditableWidgetCollection[0] as DateRangeCalendarWidget;
        }

        /// <summary>
        /// Updates table layout.
        /// </summary>
        /// <param name="type">Export type.</param>
        private void _UpdateTableLayout(ExportType type)
        {
            var grid = (Grid)ctrlFields;
            switch (type)
            {
                case ExportType.Access:
                    grid.ColumnDefinitions[0].Width = new GridLength(0.5, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = new GridLength(0.5, GridUnitType.Star);
                    break;

                case ExportType.TextRoutes:
                case ExportType.ShapeRoutes:
                    grid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid.ColumnDefinitions[1].Width = new GridLength(0);
                    break;

                case ExportType.TextStops:
                case ExportType.TextOrders:
                case ExportType.ShapeStops:
                    grid.ColumnDefinitions[0].Width = new GridLength(0);
                    grid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }
        }

        /// <summary>
        /// Updates GUI.
        /// </summary>
        private void _UpdateGui()
        {
            string message = "";
            if (0 == _schedulesToExport.Count)
            {
                DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();
                message = App.Current.GetString("ExportNotFoundScheduleMessageFormat",
                                                calendarWidget.StartDate.ToShortDateString(),
                                                calendarWidget.EndDate.ToShortDateString());
            }
            App.Current.MainWindow.StatusBar.SetStatus(this, message);

            _UpdateExportButtonState();
        }

        /// <summary>
        /// Updates export button state.
        /// </summary>
        private void _UpdateExportButtonState()
        {
            var tbFile = (TextBox)ctrlGeneral.FindName("textboxFile");
            var tbName = (TextBox)ctrlGeneral.FindName("editName");
            buttonExport.IsEnabled = ((0 < _schedulesToExport.Count) &&
                                      !string.IsNullOrEmpty(tbFile.Text) &&
                                      !string.IsNullOrEmpty(tbName.Text));
        }

        /// <summary>
        /// Updates export schedules.
        /// </summary>
        private void _UpdateSchedules()
        {
            _schedulesToExport.Clear();

            // create list of not empty schedule in date range
            DateRangeCalendarWidget calendarWidget = _GetCalendarWidget();

            DateTime day = calendarWidget.StartDate;
            while (day <= calendarWidget.EndDate)
            {
                Schedule schedule = ScheduleHelper.GetCurrentScheduleByDay(day);
                if (null != schedule)
                {
                    bool isScheduleEmpty = true;
                    if (ScheduleHelper.DoesScheduleHaveBuiltRoutes(schedule))
                        isScheduleEmpty = false;
                    else
                    {
                        IDataObjectCollection<Order> unassignedOrders = schedule.UnassignedOrders;
                        if (null == unassignedOrders)
                        {
                            unassignedOrders = App.Current.Project.Orders.Search(day);
                            if (null != unassignedOrders)
                                schedule.UnassignedOrders = unassignedOrders;
                        }

                        if (null != unassignedOrders)
                            isScheduleEmpty = (0 == unassignedOrders.Count);
                    }

                    if (!isScheduleEmpty)
                        _schedulesToExport.Add(schedule);
                }

                day = day.AddDays(1);
            }
        }

        /// <summary>
        /// Gets export type face name.
        /// </summary>
        /// <param name="type">Export type.</param>
        /// <returns>Export type face name.</returns>
        public string _GetTypeFaceName(ExportType type)
        {
            string resourceName = null;
            switch (type)
            {
                case ExportType.Access:
                    resourceName = RESOURCE_NAME_SCHEDULE;
                    break;
                case ExportType.TextRoutes:
                case ExportType.ShapeRoutes:
                    resourceName = RESOURCE_NAME_ROUTES;
                    break;
                case ExportType.TextStops:
                case ExportType.ShapeStops:
                    resourceName = RESOURCE_NAME_STOPS;
                    break;
                case ExportType.TextOrders:
                    resourceName = RESOURCE_NAME_ORDERS;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return App.Current.FindString(resourceName);
        }

        /// <summary>
        /// Inits grid structure.
        /// </summary>
        /// <param name="source">Data view source.</param>
        /// <param name="gridName">Grid name.</param>
        private void _InitGridStructure(DataGridCollectionViewSource source, string gridName)
        {
            Debug.Assert(null != source);
            Debug.Assert(!string.IsNullOrEmpty(gridName));

            var structureInitializer =
                new GridStructureInitializer(GridSettingsProvider.ExportFieldsGridStructure);
            var xceedGrid = (DataGridControl)ctrlFields.FindName(gridName);
            structureInitializer.BuildGridStructure(source, xceedGrid);
            ColumnBase columnCkecked = xceedGrid.Columns["IsChecked"];
            columnCkecked.CellEditor = (CellEditor)layoutRoot.FindResource("CheckBoxCellEditor");
            columnCkecked.Width = 25;
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
            _statusLabel.Content = App.Current.FindString("ExportStartMessage");

            _buttonCancel.IsEnabled = true;
            _buttonCancel.Click += new RoutedEventHandler(_ButtonCancel_Click);

            App.Current.MainWindow.StatusBar.SetStatus(this, status);
        }

        #region Validation methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private bool _ValidationResultRoutine(bool isValid, int validationElement)
        {
            _validationRes.Set(validationElement, isValid);

            _UpdateExportButtonState();

            return isValid;
        }

        /// <summary>
        /// Checks is profile name correct.
        /// </summary>
        /// <returns>TRUE if name is correct.</returns>
        private bool _IsValidName()
        {
            var tbName = (TextBox)ctrlGeneral.FindName("editName");
            return _ValidationResultRoutine(!string.IsNullOrEmpty(tbName.Text), ValidationRes_Name);
        }

        /// <summary>
        /// Checks is export type correct.
        /// </summary>
        /// <returns>TRUE if type correct.</returns>
        private bool _IsValidType()
        {
            var cbType = (ComboBox)ctrlGeneral.FindName("comboboxType");
            string value = (null == cbType.SelectedItem) ? null : cbType.SelectedItem.ToString();

            var validator = new ExportProfileNotEmptyValidationRule();
            ValidationResult res = validator.Validate(value, CultureInfo.CurrentCulture);
            return _ValidationResultRoutine(res.IsValid, ValidationRes_Type);
        }

        /// <summary>
        /// Checks is export file name correct.
        /// </summary>
        /// <returns>TRUE if file name correct.</returns>
        private bool _IsValidFile()
        {
            var tbFile = (TextBox)ctrlGeneral.FindName("textboxFile");
            string fileName = (FileHelpers.ValidateFilepath(tbFile.Text)) ? tbFile.Text : null;

            bool isValid = false;
            if (!string.IsNullOrEmpty(fileName))
            {
                var validator = new ExportProfileNotEmptyValidationRule();
                ValidationResult res = validator.Validate(fileName, CultureInfo.CurrentCulture);
                isValid = res.IsValid;
            }

            return _ValidationResultRoutine(isValid, ValidationRes_File);
        }

        /// <summary>
        /// Does validation data procedure.
        /// </summary>
        private void _ValidateData()
        {
            // general
            _IsValidName();
            // NOTE: Description and Format not validate
            _IsValidType();

            // data source
            _IsValidFile();
        }

        /// <summary>
        /// Checks is all inputed values correct.
        /// </summary>
        /// <returns>TRUE if all data correct.</returns>
        private bool _IsDataValid()
        {
            bool isValid = true;
            foreach (bool res in _validationRes)
            {
                if (!res)
                {
                    isValid = false;
                    break; // NOTE: result founded
                }
            }

            return isValid;
        }

        /// <summary>
        /// Checks is fields selection correct.
        /// </summary>
        /// <param name="profile">Export profile.</param>
        /// <param name="showMessage">Show problem message flag.</param>
        /// <returns>TRUE if fields selection correct.</returns>
        private bool _IsValidFieldSelection(Profile profile, bool showMessage)
        {
            Debug.Assert(null != profile);

            bool isValid = true;
            ICollection<ITableDefinition> tables = profile.TableDefinitions;
            foreach (ITableDefinition table in tables)
            {
                if (0 == table.Fields.Count)
                {
                    if (showMessage)
                    {
                        string format = App.Current.FindString("ExportTableHaveEmptyFieldsList");
                        App.Current.Messenger.AddError(string.Format(format, table.Name));
                    }
                    isValid = false;
                    break; // NOTE: result founded
                }
            }

            return isValid;
        }

        #endregion // Validation methods

        /// <summary>
        /// Updates type name format.
        /// </summary>
        /// <param name="faceNameType">New face type name.</param>
        private void _UpdateFormat(string faceNameType)
        {
            Debug.Assert(!string.IsNullOrEmpty(faceNameType));

            var typesFormatNameCollections = new StringCollection();
            if (faceNameType == App.Current.FindString(RESOURCE_NAME_SCHEDULE))
                typesFormatNameCollections.Add(FORMAT_FACE_NAME_ACCESS);
            else
            {
                Debug.Assert((faceNameType == App.Current.FindString(RESOURCE_NAME_ROUTES)) ||
                             (faceNameType == App.Current.FindString(RESOURCE_NAME_STOPS)) ||
                             (faceNameType == App.Current.FindString(RESOURCE_NAME_ORDERS)));
                typesFormatNameCollections.Add(FORMAT_FACE_NAME_TEXT);
                // ToDo - yet not supported
                // typesFormatNameCollections.Add(FORMAT_FACE_NAME_SHAPE);
            }

            var cbFromat = (ComboBox)ctrlGeneral.FindName("comboboxFormat");
            cbFromat.ItemsSource = typesFormatNameCollections;
            cbFromat.SelectedIndex = 0;
        }

        /// <summary>
        /// Gets selected type name index.
        /// </summary>
        /// <param name="type">Export type.</param>
        /// <param name="names">Export type's face names.</param>
        /// <returns>Ordering index.</returns>
        private int _GetSelectTypeNameIndex(ExportType type, StringCollection names)
        {
            Debug.Assert(null != names);

            int index = -1;
            string name = _GetTypeFaceName(type);
            for (int i = 0; i < names.Count; ++i)
            {
                if (names[i] == name)
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Creates export type's face names.
        /// </summary>
        /// <returns>Export type's face names.</returns>
        private StringCollection _CreateTypeFaceNames()
        {
            var typesFaceNameCollections = new StringCollection();
            foreach (var item in EnumHelpers.GetValues<ExportType>())
            {
                string name = _GetTypeFaceName(item);
                if (!typesFaceNameCollections.Contains(name))
                    typesFaceNameCollections.Add(name);
            }

            return typesFaceNameCollections;
        }

        /// <summary>
        /// Checks is routes export face name.
        /// </summary>
        /// <param name="faceNameType">Face name for export type.</param>
        /// <returns>TRUE if it's name for routes export.</returns>
        private bool _IsRoutesFaceName(string faceNameType)
        {
            Debug.Assert(!string.IsNullOrEmpty(faceNameType));
            return (faceNameType == App.Current.FindString(RESOURCE_NAME_ROUTES));
        }

        /// <summary>
        /// Gets text export type by title.
        /// </summary>
        /// <param name="faceNameType">Type face name (title).</param>
        /// <returns>Export type.</returns>
        private ExportType _GetTextExportTypeByFaceName(string faceNameType)
        {
            Debug.Assert(!string.IsNullOrEmpty(faceNameType));

            ExportType type = ExportType.TextRoutes;
            if (_IsRoutesFaceName(faceNameType))
                type = ExportType.TextRoutes;
            else if (faceNameType == App.Current.FindString(RESOURCE_NAME_STOPS))
                type = ExportType.TextStops;
            else if (faceNameType == App.Current.FindString(RESOURCE_NAME_ORDERS))
                type = ExportType.TextOrders;
            else
            {
                Debug.Assert(false); // not supported
            }

            return type;
        }

        /// <summary>
        /// Converts export name type and name format to export type.
        /// </summary>
        /// <param name="faceNameType">Face name for export type.</param>
        /// <param name="faceNameFormat">Face name for export format.</param>
        /// <returns>Export type.</returns>
        private ExportType _ConvertNames2Type(string faceNameType, string faceNameFormat)
        {
            Debug.Assert(!string.IsNullOrEmpty(faceNameType));
            Debug.Assert(!string.IsNullOrEmpty(faceNameFormat));

            ExportType type = ExportType.Access;

            var typesFaceNameCollections = new StringCollection();
            foreach (var item in EnumHelpers.GetValues<ExportType>())
            {
                string name = _GetTypeFaceName(item);
                if (name == faceNameType)
                {
                    switch (faceNameFormat)
                    {
                        case FORMAT_FACE_NAME_ACCESS:
                            Debug.Assert(faceNameType == App.Current.FindString(RESOURCE_NAME_SCHEDULE));
                            type = ExportType.Access;
                            break;
                        case FORMAT_FACE_NAME_TEXT:
                            Debug.Assert(faceNameType != App.Current.FindString(RESOURCE_NAME_SCHEDULE));
                            type = _GetTextExportTypeByFaceName(faceNameType);
                            break;
                        case FORMAT_FACE_NAME_SHAPE:
                            Debug.Assert(faceNameType != App.Current.FindString(RESOURCE_NAME_SCHEDULE));
                            type = _IsRoutesFaceName(faceNameType) ?
                                        ExportType.ShapeRoutes : ExportType.ShapeStops;
                            break;
                        default:
                            Debug.Assert(false); // NOTE: not supported
                            break;
                    }

                    break; // result founded
                }
            }

            return type;
        }

        /// <summary>
        /// Gets previously selected fields for selected table and export type.
        /// </summary>
        /// <param name="exportType">Export type.</param>
        /// <param name="tableType">Table type.</param>
        /// <returns>Previously selected fields.</returns>
        private IList<SelectPropertiesWrapper> _GetPrevChoiceFields(ExportType exportType,
                                                                    TableType tableType)
        {
            // NOTE: multy return function

            switch (exportType)
            {
                case ExportType.Access:
                    Debug.Assert((TableType.Routes == tableType) || (TableType.Stops == tableType));
                    return (TableType.Routes == tableType) ? _fieldsRouteMDB : _fieldsStopMDB;

                case ExportType.TextRoutes:
                    Debug.Assert(TableType.Routes == tableType);
                    return (TableType.Routes == tableType) ? _fieldsRouteTXT : null;

                case ExportType.TextStops:
                    Debug.Assert(TableType.Stops == tableType);
                    return (TableType.Stops == tableType) ? _fieldsStopTXT : null;

                case ExportType.TextOrders:
                    Debug.Assert(TableType.Orders == tableType);
                    return (TableType.Orders == tableType) ? _fieldsOrderTXT : null;

                case ExportType.ShapeRoutes:
                    Debug.Assert(TableType.Routes == tableType);
                    return (TableType.Routes == tableType) ? _fieldsRouteSHP : null;

                case ExportType.ShapeStops:
                    Debug.Assert(TableType.Stops == tableType);
                    return (TableType.Stops == tableType) ? _fieldsStopSHP : null;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }

            return null;
        }

        /// <summary>
        /// Stores choiced fields as previously state.
        /// </summary>
        /// <param name="exportType">Export type.</param>
        /// <param name="tableType">Table type.</param>
        /// <param name="fields">Selected table fields.</param>
        private void _SetPrevChoiceFields(ExportType exportType, TableType tableType,
                                          IList<SelectPropertiesWrapper> fields)
        {
            Debug.Assert(null != fields);

            switch (exportType)
            {
                case ExportType.Access:
                    Debug.Assert((TableType.Routes == tableType) || (TableType.Stops == tableType));
                    if (TableType.Routes == tableType)
                        _fieldsRouteMDB = fields;
                    else
                        _fieldsStopMDB = fields;
                    break;

                case ExportType.TextRoutes:
                    Debug.Assert(TableType.Routes == tableType);
                    _fieldsRouteTXT = fields;
                    break;

                case ExportType.TextStops:
                    Debug.Assert(TableType.Stops == tableType);
                    _fieldsStopTXT = fields;
                    break;

                case ExportType.TextOrders:
                    Debug.Assert(TableType.Orders == tableType);
                    _fieldsOrderTXT = fields;
                    break;

                case ExportType.ShapeRoutes:
                    Debug.Assert(TableType.Routes == tableType);
                    _fieldsRouteSHP = fields;
                    break;

                case ExportType.ShapeStops:
                    Debug.Assert(TableType.Stops == tableType);
                    _fieldsStopSHP = fields;
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }
        }

        /// <summary>
        /// Changes fields controls visibility.
        /// </summary>
        /// <param name="type">Table type.</param>
        /// <param name="visibility">New visibility state.</param>
        private void _ShowTable(TableType type, Visibility visibility)
        {
            string xreedName = (TableType.Routes == type) ? "xceedGridRoute" : "xceedGridStop";
            ((DataGridControl)ctrlFields.FindName(xreedName)).Visibility = visibility;
            string labelName = (TableType.Routes == type) ? "labelRoute" : "labelStop";
            ((Label)ctrlFields.FindName(labelName)).Visibility = visibility;

            var lableName = (Label)ctrlFields.FindName("labelStop");
            lableName.Content = App.Current.FindResource((type == TableType.Orders) ?
                                                            "ExportProfilesEditPageOrderLabel" :
                                                            "ExportProfilesEditPageStopLabel");
        }

        /// <summary>
        /// Inits GUI table fields from profile.
        /// </summary>
        /// <param name="profile">Export profile.</param>
        private void _InitFieldsTables(Profile profile)
        {
            Debug.Assert(null != profile);

            // hide tables
            _ShowTable(TableType.Routes, Visibility.Collapsed);
            _ShowTable(TableType.Stops, Visibility.Collapsed);

            foreach (ITableDefinition table in profile.TableDefinitions)
            {
                if ((TableType.Routes == table.Type) ||
                    (TableType.Stops == table.Type) ||
                    (TableType.Orders == table.Type))
                {
                    IList<SelectPropertiesWrapper> fields =
                        _GetPrevChoiceFields(profile.Type, table.Type);
                    if (null == fields)
                    {   // init from previously choice
                        fields = new List<SelectPropertiesWrapper>();

                        ICollection<string> fieldsSelected = table.Fields;
                        foreach (string field in table.SupportedFields)
                        {
                            bool isSelected = fieldsSelected.Contains(field);
                            string name = table.GetFieldTitleByName(field);
                            string description = table.GetDescriptionByName(field);
                            fields.Add(new SelectPropertiesWrapper(name, description, isSelected));
                        }

                        _SetPrevChoiceFields(profile.Type, table.Type, fields);
                    }

                    if (TableType.Routes == table.Type)
                        _viewSourceRoute.Source = fields;
                    else
                        _viewSourceStop.Source = fields;

                    _ShowTable(table.Type, Visibility.Visible);
                }
            }
        }

        /// <summary>
        /// Updates profile export fields from fields tables (route and stop table).
        /// </summary>
        /// <param name="profile">Export profile.</param>
        private void _UpdateProfileFromFieldsTables(Profile profile)
        {
            Debug.Assert(null != profile);

            foreach (ITableDefinition table in profile.TableDefinitions)
            {
                if ((TableType.Routes == table.Type) ||
                    (TableType.Stops == table.Type) ||
                    (TableType.Orders == table.Type))
                {
                    var fieldsName =
                        (List<SelectPropertiesWrapper>)((TableType.Routes == table.Type) ?
                            _viewSourceRoute.Source : _viewSourceStop.Source);
                    // remove previuosly
                    table.ClearFields();

                    // select new choice
                    foreach (SelectPropertiesWrapper field in fieldsName)
                    {
                        if ((bool)field.IsChecked)
                        {
                            string fieldName = table.GetFieldNameByTitle(field.Name);
                            table.AddField(fieldName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Stores export profile.
        /// </summary>
        /// <param name="profile">Export profile.</param>
        private void _StoreProfile(Profile profile)
        {
            Debug.Assert(null != profile);

            Exporter exporter = App.Current.Exporter;
            Debug.Assert(null != exporter);

            // remove replaced profile
            Profile profileToRemove = null;
            foreach (Profile currentProfile in exporter.Profiles)
            {
                if (currentProfile.Name == profile.Name)
                {
                    profileToRemove = currentProfile;
                    break; // NOTE: replace profile founded - stop process
                }
            }

            if (null != profileToRemove)
                exporter.RemoveProfile(profileToRemove);

            // add edited profile
            exporter.AddProfile(profile); // add new profile
            // store export profiles changes
            App.Current.SaveExportProfiles();
        }

        /// <summary>
        /// Gets export profile unique name.
        /// </summary>
        /// <returns>Export profile unique name.</returns>
        private string _GetUniqueName()
        {
            string profileNameSimple = App.Current.FindString("ExportProfile");
            string profileNameFull = App.Current.GetString("ProfilesEditPageOnTimeTitleFormat",
                                                           profileNameSimple);
            var profileNames = new StringCollection();
            ICollection<Profile> profiles = App.Current.Exporter.Profiles;
            foreach (Profile currentProfile in profiles)
                profileNames.Add(currentProfile.Name);

            string profileName = profileNameFull;
            if (profileNames.Contains(profileName))
            {   // seek possible archive file name
                for (int count = 1; count < int.MaxValue; ++count)
                {
                    profileName =
                        string.Format(NAME_FORMAT_DUBLICATE_FORMAT, profileNameFull, count);
                    if (!profileNames.Contains(profileName))
                        break; // NOTE: result founded. Exit.
                }

                if (profileNames.Contains(profileName))
                    throw new NotSupportedException(); // exception
            }

            return profileName;
        }

        /// <summary>
        /// Sets profile property to controls.
        /// </summary>
        /// <param name="profile">Export profile.</param>
        private void _UpdateContent(Profile profile)
        {
            Debug.Assert(null != profile);

            _validationRes.SetAll(false);

            // update General
            _isValidationEnabled = false;
            var tbName = (TextBox)ctrlGeneral.FindName("editName");
            tbName.Text = profile.Name;

            var tbDescription = (TextBox)ctrlGeneral.FindName("editDescription");
            tbDescription.Text = profile.Description;

            var cbType = (ComboBox)ctrlGeneral.FindName("comboboxType");
            StringCollection typeNames = _CreateTypeFaceNames();
            cbType.ItemsSource = typeNames;
            cbType.SelectedIndex = _GetSelectTypeNameIndex(profile.Type, typeNames);

            _UpdateFormat(cbType.SelectedItem.ToString());

            var tbFile = (TextBox)ctrlGeneral.FindName("textboxFile");
            tbFile.Text = profile.FilePath;

            _InitFieldsTables(profile);
            _isValidationEnabled = true;

            _UpdateTableLayout(profile.Type);

            _ValidateData();
        }

        /// <summary>
        /// Create default profile.
        /// </summary>
        /// <returns>Created profile with default settings.</returns>
        private Profile _GetDeafultProfile()
        {
            // obtain default profile
            Exporter exporter = App.Current.Exporter;
            Debug.Assert(null != exporter);

            Profile defaultProfile = null;
            foreach (Profile profile in App.Current.Exporter.Profiles)
            {
                if (profile.IsDefault)
                {
                    defaultProfile = profile;
                    break; // NOTE: result founded. Exit.
                }
            }

            if (null == defaultProfile)
            {   // not founded - create new profile
                defaultProfile = exporter.CreateProfile(ExportType.Access, string.Empty);
                defaultProfile.Name = _GetUniqueName();
            }

            return defaultProfile;
        }

        /// <summary>
        /// Controls content to new instance of profile property.
        /// </summary>
        /// <param name="doesDefaultUse">Default use flag.</param>
        /// <returns>Created profile.</returns>
        private Profile _CreateProfile(bool doesDefaultUse)
        {
            var cbType = (ComboBox)ctrlGeneral.FindName("comboboxType");
            var cbFormat = (ComboBox)ctrlGeneral.FindName("comboboxFormat");
            ExportType type = _ConvertNames2Type(cbType.SelectedItem.ToString(),
                                                 cbFormat.SelectedItem.ToString());

            var tbFile = (TextBox)ctrlGeneral.FindName("textboxFile");
            Profile profile = App.Current.Exporter.CreateProfile(type, tbFile.Text);

            var tbName = (TextBox)ctrlGeneral.FindName("editName");
            profile.Name = tbName.Text;

            var tbDescription = (TextBox)ctrlGeneral.FindName("editDescription");
            profile.Description = tbDescription.Text;

            if (!doesDefaultUse)
                _UpdateProfileFromFieldsTables(profile);

            return profile;
        }

        /// <summary>
        /// Shows report error in message window.
        /// </summary>
        /// <param name="exception">Exception.</param>
        private void _PopulateReportError(Exception exception)
        {
            Debug.Assert(null != exception);

            Profile profile = _GetDeafultProfile();

            string statusMessage = App.Current.GetString("ExportMessageFormatFailed",
                                                        _GetTypeFaceName(profile.Type),
                                                        profile.FilePath);

            if (exception is AuthenticationException || exception is CommunicationException)
            {
                string service = App.Current.FindString("ServiceNameMap");
                CommonHelpers.AddServiceMessageWithDetail(statusMessage, service, exception);
                Logger.Error(exception);
            }
            else
            {
                string message = string.Format("{0} {1}", statusMessage, exception.Message);
                App.Current.Messenger.AddError(message);
                Logger.Critical(exception);
            }
        }

        /// <summary>
        /// Starts export process.
        /// </summary>
        /// <param name="profile">Ecport profile.</param>
        /// <param name="schedules">Schedules to export.</param>
        private void _DoExport(Profile profile, ICollection<Schedule> schedules)
        {
            Debug.Assert(null != profile);
            Debug.Assert(null != schedules);

            try
            {
                App.Current.UIManager.Lock(true);

                _InitStatusStack();

                Exporter exporter = App.Current.Exporter;
                exporter.AsyncExportCompleted +=
                    new AsyncExportCompletedEventHandler(exporter_AsyncExportCompleted);

                ExportOptions options = new ExportOptions();
                options.ShowLeadingStemTime = App.Current.MapDisplay.ShowLeadingStemTime;
                options.ShowTrailingStemTime = App.Current.MapDisplay.ShowTrailingStemTime;

                MapLayer currentMapLayer = CommonHelpers.GetCurrentLayer();
                exporter.DoExportAsync(profile, schedules, currentMapLayer, options);
            }
            catch (Exception ex)
            {
                _PopulateReportError(ex);

                string statusMessage = App.Current.GetString("ExportMessageFormatFailed",
                                                             _GetTypeFaceName(profile.Type),
                                                             profile.FilePath);
                App.Current.MainWindow.StatusBar.SetStatus(this, statusMessage);
                App.Current.UIManager.Unlock();
            }
        }

        #endregion // Private helpers

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page name.
        /// </summary>
        private const string PAGE_NAME = "Export";

        /// <summary>
        /// Profile name format for duplicate.
        /// </summary>
        private const string NAME_FORMAT_DUBLICATE_FORMAT = "{0} ({1})";

        // Export file extensions.
        private const string FILE_EXTENSION_MDB = "mdb";
        private const string FILE_EXTENSION_CSV = "csv";
        private const string FILE_EXTENSION_SHP = "shp";

        // Export file filtres.
        private const string FILE_DIALOG_FILTER_ACCESS = "Access database files (*.mdb)|*.mdb||";
        private const string FILE_DIALOG_FILTER_TEXT = "Text file (*.txt; *.csv)|*.txt;*.csv||";
        private const string FILE_DIALOG_FILTER_SHAPE = "Shape file (*.shp)|*.shp||";

        // Export format names.
        private const string FORMAT_FACE_NAME_ACCESS = "Access database files (*.mdb)";
        private const string FORMAT_FACE_NAME_TEXT = "Text file (*.txt; *.csv)";
        private const string FORMAT_FACE_NAME_SHAPE = "Shape file (*.shp)";

        // Export resource face names.
        private const string RESOURCE_NAME_SCHEDULE = "ExportProfilesEditPageTypeShedule";
        private const string RESOURCE_NAME_ROUTES = "ExportProfilesEditPageTypeRoutes";
        private const string RESOURCE_NAME_STOPS = "ExportProfilesEditPageTypeStops";
        private const string RESOURCE_NAME_ORDERS = "ExportProfilesEditPageTypeOrders";

        private const string XCEED_GRID_NAME_ROUTE = "xceedGridRoute";
        private const string XCEED_GRID_NAME_STOP = "xceedGridStop";

        // Validation definitions.
        private const int ValidationRes_Name = 0;
        private const int ValidationRes_Type = 1;
        private const int ValidationRes_File = 2;
        private const int ValidationRes_Count = ValidationRes_File + 1;

        #endregion // Private consts

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Validation results.
        /// </summary>
        private BitArray _validationRes = new BitArray(ValidationRes_Count, false);

        /// <summary>
        /// Schedules to export.
        /// </summary>
        private ICollection<Schedule> _schedulesToExport = new List<Schedule>();

        /// <summary>
        /// Is validation enabled flag.
        /// </summary>
        private bool _isValidationEnabled;
        /// <summary>
        /// Is inited flag.
        /// </summary>
        private bool _isInited;

        /// <summary>
        /// Xceed route table data source.
        /// </summary>
        private DataGridCollectionViewSource _viewSourceRoute;
        /// <summary>
        /// Xceed stop table data source.
        /// </summary>
        private DataGridCollectionViewSource _viewSourceStop;

        // For store previouly user selection.
        private IList<SelectPropertiesWrapper> _fieldsRouteMDB;
        private IList<SelectPropertiesWrapper> _fieldsStopMDB;
        private IList<SelectPropertiesWrapper> _fieldsRouteTXT;
        private IList<SelectPropertiesWrapper> _fieldsStopTXT;
        private IList<SelectPropertiesWrapper> _fieldsOrderTXT;
        private IList<SelectPropertiesWrapper> _fieldsRouteSHP;
        private IList<SelectPropertiesWrapper> _fieldsStopSHP;

        /// <summary>
        /// Status control label.
        /// </summary>
        private Label _statusLabel;
        /// <summary>
        /// Status control Cancel button.
        /// </summary>
        private Button _buttonCancel;

        #endregion // Private members
    }
}
