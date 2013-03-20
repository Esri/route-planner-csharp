using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.DomainObjects;
using AppData = ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.Import;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interaction logic for FleetSetupWizardImportObjectsPage.xaml
    /// </summary>
    internal partial class FleetSetupWizardImportObjectsPage : WizardPageBase,
        ISupportBack, ISupportNext
    {
        // NOTE: implementation constaint customize logic for support use
        //       as one page of Wizard and as "one-time" import page.

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public FleetSetupWizardImportObjectsPage()
        {
            InitializeComponent();

            var radiobutton = (RadioButton)ctrlDataSource.FindName(RADIOBUTTON_SELECT_DEF);
            radiobutton.IsChecked = true;

            this.Loaded += new RoutedEventHandler(_Page_Loaded);
            this.Unloaded += new RoutedEventHandler(_Page_Unloaded);

            _collectionSource =
                (DataGridCollectionViewSource)layoutRoot.FindResource(COLLECTION_SOURCE_KEY);
        }

        #endregion // Constructors

        #region ISupportBack members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Back" button clicked.
        /// </summary>
        public event EventHandler BackRequired;

        #endregion // ISupportBack members

        #region ISupportNext members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Next" button clicked.
        /// </summary>
        public event EventHandler NextRequired;

        #endregion // ISupportNext members

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fires when corresponding button click.
        /// </summary>
        public event EventHandler EditOK;
        public event EventHandler EditCancel;

        #endregion // Public events

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is process canceled flag.
        /// </summary>
        public bool IsProcessCanceled
        {
            get { return _isProcessCancelled; }
        }

        /// <summary>
        /// Profile to editing.
        /// </summary>
        public ImportProfile Profile
        {
            get { return _CreateProfile(); }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits page state.
        /// </summary>
        /// <param name="profile">Import profile.</param>
        public void PostInit(ImportProfile profile)
        {
            string importeObjectName = CommonHelpers.GetImportObjectsName(profile.Type);

            labelTitle.Content = App.Current.GetString("ImportObjectsTitleFormat", importeObjectName);

            // update source hint
            var lbSourceHint = (Label)ctrlDataSource.FindName("sourceHint");
            lbSourceHint.Content =
                App.Current.GetString("ImportProfilesEditPageChooseSourceLabelFormat", importeObjectName);

            _profile = profile; // store new state
        }

        #endregion // Public methods

        #region Private properties

        /// <summary>
        /// Specialized context.
        /// </summary>
        private FleetSetupWizardDataContext DataKeeper
        {
            get
            {
                return DataContext as FleetSetupWizardDataContext;
            }
        }

        #endregion
        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Link source text changed handler.
        /// </summary>
        private void linkSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.Assert(sender is TextBox);

            if (_isValidationEnabled)
                _UpdateConnectionString(((TextBox)sender).Text, false);
        }

        /// <summary>
        /// ComboBox table selection changed handler.
        /// </summary>
        private void comboBoxTable_SelectionChanged(object sender, EventArgs e)
        {
            if (_isValidationEnabled)
                _IsValidTable();
        }

        /// <summary>
        /// ComboBox field loaded handler.
        /// </summary>
        protected void comboBoxField_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(sender is ComboBox);
            var cmb = sender as ComboBox;
            cmb.ItemsSource = _sourceFieldList; // update source field list
        }

        /// <summary>
        /// ComboBox field dropdown closed handler.
        /// </summary>
        private void comboBoxField_DropDownClosed(object sender, EventArgs e)
        {
            // WORKAROUND : XGREED control update binding data only if user "Enter" pressed,
            //              code set value after possible changes
            Cell cell = XceedVisualTreeHelper.GetCellByEditor((UIElement)sender);
            if (cell == null)
                return;

            Row row = cell.ParentRow;

            var combobox = sender as ComboBox;
            object selectedItem = combobox.SelectedItem;
            if (null != selectedItem)
            {
                string selectedName = selectedItem.ToString();

                // remove visual marks
                string visualName = row.Cells["ObjectFieldName"].Content.ToString();
                string name = _ClearSpecialIndication(visualName);

                foreach (ImportProfileEditFieldMapping map in _mapFields)
                {
                    if (map.ObjectFieldName != name)
                        continue; // NOTE: skip unsuitable

                    map.SourceFieldName = selectedName;

                    bool isUpdated = false;
                    foreach (ImportProfileEditFieldMapping mapField in _prevMapFields)
                    {
                        if (mapField.ObjectFieldName != name)
                            continue; // NOTE: skip unsuitable

                        mapField.SourceFieldName = selectedName;
                        isUpdated = true;
                        break; // NOTE: founded - exit
                    }

                    if (!isUpdated)
                    {
                        var mapElement = new ImportProfileEditFieldMapping(map.ObjectFieldName,
                                                                           map.SourceFieldName);
                        _prevMapFields.Add(mapElement);
                    }

                    break; // NOTE: founded - exit
                }

                _UpdateButtonOKState();
                _UpdatePreviewTable();
            }
        }

        /// <summary>
        /// RadioButton checked handler.
        /// </summary>
        private void radioButton_Checked(object sender, RoutedEventArgs e)
        {
            bool isFileChecked = _IsDataSourceFile();
            if (isFileChecked != _prevCheckedState)
            {   // update source GUI
                _UpdateDataSourceGrid(isFileChecked);
                string path = (isFileChecked) ? _prevFilePath : _prevConnectionString;
                _UpdateConnectionString(path, true);
                _prevCheckedState = isFileChecked;
            }
        }

        /// <summary>
        /// Button "Browse" click handler.
        /// </summary>
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            bool isFileChecked = _IsDataSourceFile();
            DataSourceOpener.FilterType type = _GetFilterType();
            if (DataSourceOpener.QueryDataSource(isFileChecked, App.Current.MainWindow, type))
            {
                string sourceLink = (isFileChecked) ? DataSourceOpener.FilePath :
                                                      DataSourceOpener.ConnectionString;
                _UpdateConnectionString(sourceLink, false);
            }
        }

        /// <summary>
        /// Button "Back" click handler.
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            if (null != BackRequired)
                BackRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Button "OK"\"Import" click handler.
        /// </summary>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            bool isWizardMode = _IsWizardMode();
            if (!_IsDataValid())
            {
                if (!isWizardMode)
                    App.Current.Messenger.AddError(App.Current.FindString("ProfileInvalidState"));
            }

            else if (_IsValidFieldMapping(!isWizardMode))
            {
                DataSourceOpener.Reset();
                if (isWizardMode)
                {   // do import
                    ImportProfile profile = Profile;
                    App.Current.ImportProfilesKeeper.AddOrUpdateProfile(profile);

                    var manager = new ImportManager();
                    manager.ImportCompleted +=
                        new ImportCompletedEventHandler(_ImportCompleted);
                    manager.ImportAsync(DataKeeper.ParentPage, profile, _defaultDate);
                }
                else
                {   // "one-time" mode
                    if (null != NextRequired)
                        NextRequired(this, EventArgs.Empty);

                    if (null != EditOK)
                        EditOK(this, EventArgs.Empty);
                }
            }

            // else Do nothing
        }

        /// <summary>
        /// Button "Next" click handler.
        /// </summary>
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (null != EditCancel)
                EditCancel(null, EventArgs.Empty);

            if (null != NextRequired)
                NextRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Button "Cancel"\"Cancel Import" click handler.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            _isProcessCancelled = true;

            if (null != EditCancel)
                EditCancel(null, EventArgs.Empty);

            if (null != NextRequired)
                NextRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        private void _Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isColumnInited)
                _InitColumnHeaders();

            App.Current.CurrentDateChanged += new EventHandler(_CurrentDateChanged);
            App.Current.MainWindow.PreviewKeyDown += new KeyEventHandler(MainWindow_PreviewKeyDown);

            if (null != _profile)
            {
                _UpdateContent(_profile);
                buttonOk.IsEnabled = _IsValidSourceLink();
            }

            ctrlDataSource.Visibility = Visibility.Visible;
            _isProcessCancelled = false;

            Visibility visibility = Visibility.Visible;
            if (_IsWizardMode())
            {   // part of wizard mode
                buttonOk.Content = App.Current.FindString("ButtonHeaderImport");
                buttonNext.IsEnabled = DataKeeper.AddedOrders.Any(order => !order.IsGeocoded);

                App.Current.UIManager.LockMessageWindow();
                App.Current.CurrentDate = DateTime.Today; // NOTE: wizard work only today
            }
            else
            {   // "one-time" mode
                buttonOk.Content = App.Current.FindString("ButtonHeaderOk");
                visibility = Visibility.Collapsed;
            }

            specialTooltip.Visibility =
                buttonBack.Visibility =
                    buttonNext.Visibility = visibility;

            _InitDefaultDate();
            _InitPreviewDataGridLayout();
        }

        /// <summary>
        /// Application's current date changed.
        /// </summary>
        private void _CurrentDateChanged(object sender, EventArgs e)
        {
            _InitDefaultDate();
            _UpdatePreviewTable();
        }

        /// <summary>
        /// Page unloaded handler.
        /// </summary>
        private void _Page_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.PreviewKeyDown -= MainWindow_PreviewKeyDown;
            App.Current.CurrentDateChanged -= _CurrentDateChanged;

            if (_IsWizardMode())
            {
                App.Current.UIManager.UnlockMessageWindow();
                if (_IsDataValid())
                    _profile = _CreateProfile(); // store state
            }
        }

        /// <summary>
        /// Button "OK"\"Import" IsEnabled changed handler.
        /// </summary>
        private void buttonOk_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            buttonOk.ToolTip = buttonOk.IsEnabled?
                                    null : App.Current.FindString("ProfileDisableButtonTooltip");
        }

        /// <summary>
        /// MainWindow key down handler.
        /// </summary>
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {   // do cancel
                _isProcessCancelled = true;

                if (null != EditCancel)
                    EditCancel(null, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Import completed handler.
        /// </summary>
        private void _ImportCompleted(object sender, ImportCompletedEventArgs e)
        {
            Debug.Assert(_IsWizardMode()); // used only in wizard mode

            MainWindow mainWindow = App.Current.MainWindow;
            if (!mainWindow.IsLocked) // importing do lock\unlock main window
                mainWindow.Lock(false); // need store valid state

            var manager = sender as ImportManager;
            Debug.Assert(null != manager);
            manager.ImportCompleted -= _ImportCompleted;
            manager.Dispose();

            _isProcessCancelled = e.Cancelled;
            if (!_isProcessCancelled)
            {
                // store imported orders
                IList<AppData.DataObject> importedObjects = e.ImportedObjects;
                var orders = new List<Order> ();

                if (0 < importedObjects.Count)
                {
                    foreach (Order order in importedObjects)
                        orders.Add(order);
                }

                DataKeeper.AddedOrders = orders;
            }

            if (null != NextRequired)
                NextRequired(this, EventArgs.Empty);
        }

        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Inits column headres.
        /// </summary>
        private void _InitColumnHeaders()
        {
            Debug.Assert(!_isColumnInited);

            var xceedGrid = (DataGridControl)ctrlFieldMapping.FindName("xceedGrid");
            xceedGrid.Columns["ObjectFieldName"].Title =
                App.Current.FindString("ImportProfilesGridCaptionObjectField");
            xceedGrid.Columns["SourceFieldName"].Title =
                App.Current.FindString("ImportProfilesGridCaptionSourceField");

            _isColumnInited = true;
        }

        /// <summary>
        /// Checks is data source selecetd file.
        /// </summary>
        /// <returns>TRUE if data source is file.</returns>
        private bool _IsDataSourceFile()
        {
            var radiobuttonFile = (RadioButton)ctrlDataSource.FindName(RADIOBUTTON_FILE);
            return (bool)radiobuttonFile.IsChecked;
        }

        /// <summary>
        /// Gets datasource opener filter type.
        /// </summary>
        /// <returns>Datasource opener filter type.</returns>
        private DataSourceOpener.FilterType _GetFilterType()
        {
            var filterType = DataSourceOpener.FilterType.WithoutShape;
            if ((ImportType.Barriers == _importProfile.Type) ||
                (ImportType.Zones == _importProfile.Type))
                filterType = DataSourceOpener.FilterType.OnlyShape;
            else if ((ImportType.Locations == _importProfile.Type) ||
                     (ImportType.Orders == _importProfile.Type))
                        filterType = DataSourceOpener.FilterType.AllSupported;

            return filterType;
        }

        /// <summary>
        /// Updates link source label hint.
        /// </summary>
        private void _UpdateLinkSourceLabelHint()
        {
            string hintResource = "ImportProfilesEditPageToolTipFilePath";
            if ((ImportType.Barriers == _importProfile.Type) ||
                (ImportType.Zones == _importProfile.Type))
                hintResource = "ImportProfilesEditPageToolTipFilePathShape";
            else if ((ImportType.Locations == _importProfile.Type) ||
                     (ImportType.Orders == _importProfile.Type))
                        hintResource = "ImportProfilesEditPageToolTipFilePathAll";

            var linkHint = (Label)ctrlDataSource.FindName("linkHint");
            linkHint.Content = App.Current.FindString(hintResource);
        }

        /// <summary>
        /// Updates data source grid.
        /// </summary>
        /// <param name="isFileSelected">Is file selected flag.</param>
        private void _UpdateDataSourceGrid(bool isFileSelected)
        {
            var label = (Label)ctrlDataSource.FindName("labelSource");
            var button = (Button)ctrlDataSource.FindName("buttonSource");
            if (isFileSelected)
            {
                label.Content = App.Current.FindString("ProfilesEditPageSourceLabelFilePath");
                button.Content = App.Current.FindString("ProfilesEditPageSourceButtonBrowse");
                _UpdateLinkSourceLabelHint();
            }
            else
            {
                label.Content = App.Current.FindString("ImportProfilesEditPageSourceLabelConnectionString");
                button.Content = App.Current.FindString("ImportProfilesEditPageSourceButtonBuild");
                var linkHint = (Label)ctrlDataSource.FindName("linkHint");
                linkHint.Content = App.Current.FindString("ImportProfilesEditPageToolTipConnectionString");
            }
        }

        /// <summary>
        /// Gets alliases for dynamical fields (AddressesFields, Capacities, CustomOrderProperties).
        /// </summary>
        /// <returns>Dictonary aliases by dynamical field name.</returns>
        StringDictionary _GetSpecialAliases()
        {
            if (null != _aliasesSpecial)
                return _aliasesSpecial; // create only once

            _aliasesSpecial = new StringDictionary();

            // NOTE: address fields have special aliases
            //       (in different servers present different fields)
            AddressField[] fields = App.Current.Geocoder.AddressFields;
            for (int index = 0; index < fields.Length; ++index)
            {
                AddressField field = fields[index];
                string fieldName = field.Title;
                string aliases = string.Format(FORMAT, fieldName, field.Type.ToString());
                _aliasesSpecial.Add(fieldName, aliases);
            }

            Project project = App.Current.Project;

            // add capacities name
            CapacitiesInfo capacitiesInfo = project.CapacitiesInfo;
            for (int index = 0; index < capacitiesInfo.Count; ++index)
            {
                string fieldName = capacitiesInfo[index].Name;
                string capacityName = Capacities.GetCapacityPropertyName(index);
                string aliases = string.Format(FORMAT, fieldName, capacityName);
                _aliasesSpecial.Add(fieldName, aliases);
            }

            // add order custom properties name
            OrderCustomPropertiesInfo customPropertiesInfo = project.OrderCustomPropertiesInfo;
            for (int index = 0; index < customPropertiesInfo.Count; ++index)
            {
                string fieldName = customPropertiesInfo[index].Name;
                string customPropertyName = OrderCustomProperties.GetCustomPropertyName(index);
                string aliases = string.Format(FORMAT, fieldName, customPropertyName);
                _aliasesSpecial.Add(fieldName, aliases);
            }

            return _aliasesSpecial;
        }

        #region Validation methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Validation result routine.
        /// </summary>
        /// <param name="isValid">Validation result.</param>
        /// <param name="validationElement">Validation element index.</param>
        /// <returns></returns>
        private bool _ValidationResultRoutine(bool isValid, int validationElement)
        {
            _validationRes.Set(validationElement, isValid);

            _UpdateButtonOKState();
            _UpdatePreviewTable();

            return isValid;
        }

        /// <summary>
        /// Checks source link is valid.
        /// </summary>
        /// <returns>Return TRUE if source link is valid.</returns>
        private bool _IsValidSourceLink()
        {
            var source = (TextBox)ctrlDataSource.FindName("linkSource");

            bool isValid = false;
            if (!string.IsNullOrEmpty(source.Text))
            {
                ProfileSourceLinkValidationRule validator = new ProfileSourceLinkValidationRule();
                ValidationResult res = validator.Validate(source.Text, CultureInfo.CurrentCulture);
                isValid = res.IsValid;
                if (!isValid)
                {   // indicate problem
                    string connectionString = source.Text;
                    if (!DataSourceOpener.IsConnectionString(connectionString))
                    {
                        if (System.IO.File.Exists(connectionString) &&
                            !FileHelpers.IsShapeFile(connectionString))
                        {
                            string message = null;
                            DataSourceOpener.IsConnectionPossible(DataSourceOpener.ConnectionString,
                                                                  out message); // NOTE: ignore result
                            if (!string.IsNullOrEmpty(message) && !_IsWizardMode())
                                App.Current.Messenger.AddError(message);
                        }
                    }
                }
            }

            return _ValidationResultRoutine(isValid, ValidationRes_Source);
        }

        /// <summary>
        /// Checks table name is valid.
        /// </summary>
        /// <returns>Return TRUE if table name is valid.</returns>
        private bool _IsValidTable()
        {
            var comboboxTable = (ComboBox)ctrlDataSource.FindName("comboboxTable");
            string value = (null == comboboxTable.SelectedItem) ?
                                null : comboboxTable.SelectedItem.ToString();

            var validator = new ProfileTableNameValidationRule();
            ValidationResult res = validator.Validate(value, CultureInfo.CurrentCulture);
            bool isValid = _ValidationResultRoutine(res.IsValid, ValidationRes_Table);

            _UpdateFieldMapping();

            return isValid;
        }

        /// <summary>
        /// Starts data validation.
        /// </summary>
        private void _ValidateData()
        {
            // data source
            _IsValidSourceLink();
            _IsValidTable();
        }

        /// <summary>
        /// Checks all mandatory fields is mapped.
        /// </summary>
        /// <param name="showMessage">Show user warning message flag.</param>
        /// <returns>Return TRUE if all mandatory fields is mapped.</returns>
        private bool _IsAllMandatoryFieldMapped(bool showMessage)
        {
            // get import description
            bool isSourceShape = (_IsDataSourceFile() &&
                                  FileHelpers.IsShapeFile(DataSourceOpener.FilePath));
            ICollection<ObjectDataFieldInfo> infos =
                PropertyHelpers.GetDestinationProperties(_importProfile.Type, isSourceShape);

            // check all mandatory fields must be mapped
            var sb = new StringBuilder();
            foreach (ObjectDataFieldInfo info in infos)
            {
                if (!info.IsMandatory)
                    continue; // NOTE: skip not mandatory fields

                foreach (ImportProfileEditFieldMapping item in _mapFields)
                {
                    if (item.ObjectFieldName == info.Info.Name)
                    {
                        if (string.IsNullOrEmpty(item.SourceFieldName))
                        {
                            if (!string.IsNullOrEmpty(sb.ToString()))
                                sb.Append(FIELD_ALIASES_DELIMITER);
                            sb.Append(item.ObjectFieldName);
                        }
                    }
                }
            }

            bool result = true;
            string notInitedMandatoryFields = sb.ToString();
            if (!string.IsNullOrEmpty(notInitedMandatoryFields))
            {
                if (showMessage)
                {
                    string format = App.Current.FindString("ImportProfileInvalidFieldMapping3");
                    App.Current.Messenger.AddError(string.Format(format, notInitedMandatoryFields));
                }
                result = false; // NOTE: error detected
            }

            return result;
        }

        /// <summary>
        /// Checks field mapping is valid.
        /// </summary>
        /// <param name="showMessage">Show user warning message flag.</param>
        /// <returns>Return TRUE if field mapping is valid.</returns>
        private bool _IsValidFieldMapping(bool showMessage)
        {
            bool isValid = true;
            if (null == _mapFields)
            {
                if (showMessage)
                {
                    string error = App.Current.FindString("ImportProfileInvalidFieldMapping");
                    App.Current.Messenger.AddError(error);
                }

                isValid = false;
            }
            else
                isValid = _IsAllMandatoryFieldMapped(showMessage);

            return isValid;
        }

        /// <summary>
        /// Checks data is valid.
        /// </summary>
        /// <returns>Return TRUE if all validation is true.</returns>
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

        #endregion // Validation methods

        /// <summary>
        /// Gets default table name.
        /// </summary>
        /// <param name="listTables">Source list tables.</param>
        /// <param name="isTextFileSelected">Is text file selected flag.</param>
        /// <returns>Default table name.</returns>
        private string _GetDefaultTableName(IList<string> listTables, out bool isTextFileSelected)
        {
            isTextFileSelected = false;
            string selectedTableName = null;

            if (1 == listTables.Count) // NOTE: if table only one select this, else - user must select
                selectedTableName = listTables[0];
            else if (_IsDataSourceFile())
            {
                // NOTE: by default for csv\txt select first file
                string filePath = DataSourceOpener.FilePath;
                string fileExt = System.IO.Path.GetExtension(filePath);
                if (fileExt.Equals(FILE_EXTENSION_CSV, StringComparison.OrdinalIgnoreCase) ||
                    fileExt.Equals(FILE_EXTENSION_TXT, StringComparison.OrdinalIgnoreCase))
                {
                    fileExt = fileExt.Remove(0, 1); // NOTE: remove dot from extension

                    string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    for (int index = 0; index < listTables.Count; ++index)
                    {
                        string tableName = listTables[index];
                        string[] nameParts = tableName.Split(TEXT_FILE_NAME_SEPARATOR);
                        Debug.Assert(nameParts.Length == 2);
                        if (fileName.Equals(nameParts[0], StringComparison.OrdinalIgnoreCase) &&
                            fileExt.Equals(nameParts[1], StringComparison.OrdinalIgnoreCase))
                        {
                            selectedTableName = tableName;
                            isTextFileSelected = true;
                            break;
                        }
                    }
                }
            }

            return selectedTableName;
        }

        /// <summary>
        /// Updates source table names.
        /// </summary>
        private void _UpdateTableNames()
        {
            int tableCount = 0;
            bool isTextFileSelected = false;
            var tables = (ComboBox)ctrlDataSource.FindName("comboboxTable");
            if (!string.IsNullOrEmpty(DataSourceOpener.ConnectionString))
            {   // set table list to GUI
                string messageFailure = null;
                IList<string> listTables = DataSourceOpener.GetTableNameList(out messageFailure);
                if (!string.IsNullOrEmpty(messageFailure) && !_IsWizardMode())
                    App.Current.Messenger.AddError(messageFailure);

                tableCount = listTables.Count;
                if (0 < tableCount)
                {
                    tables.ItemsSource = listTables;
                    tables.SelectedItem =_GetDefaultTableName(listTables, out isTextFileSelected);
                }
            }

            // update GUI
            var labelTable = (Label)ctrlDataSource.FindName("labelTable");
            var tableHint = (Label)ctrlDataSource.FindName("tableHint");
            bool isShow = ((1 < tableCount) && !isTextFileSelected);
            tables.IsEnabled = isShow;
            tableHint.Visibility =
                labelTable.Visibility =
                    tables.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Updates connection string.
        /// </summary>
        /// <param name="connectionString">Source connection string.</param>
        /// <param name="isSourceTypeChanged">Is source type changed flag.</param>
        private void _UpdateConnectionString(string connectionString, bool isSourceTypeChanged)
        {
            _isValidationEnabled = false;
            string strTableName = null;
            var comboboxTable = (ComboBox)ctrlDataSource.FindName("comboboxTable");

            var source = (TextBox)ctrlDataSource.FindName("linkSource");
            if (isSourceTypeChanged)
            {   // store previous choice
                if (_IsDataSourceFile())
                    _prevConnectionString = source.Text;
                else
                    _prevFilePath = source.Text;

                if (!string.IsNullOrEmpty(_prevTableName))
                    strTableName = _prevTableName;

                _prevTableName = comboboxTable.Text;
            }

            if (_IsDataSourceFile())
                DataSourceOpener.FilePath = connectionString;
            else
                DataSourceOpener.ConnectionString = connectionString;

            // update data link source
            source.Text = connectionString;

            var tables = (ComboBox)ctrlDataSource.FindName("comboboxTable");
            tables.ItemsSource = null;
            if (_IsValidSourceLink())
            {
                _UpdateTableNames();
                if (!string.IsNullOrEmpty(strTableName))
                    comboboxTable.SelectedItem = strTableName;
            }
            else
            {
                var labelTable = (Label)ctrlDataSource.FindName("labelTable");
                var tableHint = (Label)ctrlDataSource.FindName("tableHint");
                tableHint.Visibility =
                    labelTable.Visibility =
                        comboboxTable.Visibility = Visibility.Collapsed;
                comboboxTable.Text = null;
            }

            _isValidationEnabled = true;
            _IsValidTable();
        }

        /// <summary>
        /// Updates field mapping previously.
        /// </summary>
        private void _UpdateFieldMappingPrev()
        {
            if (0 == _prevMapFields.Count)
            {   // init only once
                foreach (ImportProfileEditFieldMapping map in _mapFields)
                {
                    _prevMapFields.Add(new ImportProfileEditFieldMapping(map.ObjectFieldName,
                                                                         map.SourceFieldName));
                }
            }
            else
            {
                // update from previously select
                foreach (ImportProfileEditFieldMapping map in _prevMapFields)
                {
                    if (string.IsNullOrEmpty(map.SourceFieldName))
                        continue; // skip empty source field

                    foreach (ImportProfileEditFieldMapping mapField in _mapFields)
                    {
                        if ((map.ObjectFieldName == mapField.ObjectFieldName) &&
                             string.IsNullOrEmpty(mapField.SourceFieldName) &&
                             _sourceFieldList.Contains(map.SourceFieldName))
                        {
                            mapField.SourceFieldName = map.SourceFieldName;
                            break; // work done
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates field mapping.
        /// </summary>
        private void _CreateFieldMapping()
        {
            // create default map fields - use automapping
            StringDictionary aliases = App.Current.ImportProfilesKeeper.FieldAliases;
            StringDictionary aliasesSpecial = _GetSpecialAliases();

            foreach (string name in _destinationFieldList)
            {
                string sourceName = string.Empty;

                // do auto map
                string altnames = aliases[name];
                if (string.IsNullOrEmpty(altnames))
                    altnames = aliasesSpecial[name]; // NOTE: seek in special fields aliases

                if (!string.IsNullOrEmpty(altnames))
                {
                    string[] altnamesList = altnames.Split(FIELD_ALIASES_DELIMITER);
                    for (int altIndex = 0; altIndex < altnamesList.Length; ++altIndex)
                    {
                        for (int sourceIndex = 0; sourceIndex < _sourceFieldList.Count; ++sourceIndex)
                        {
                            string sourceNameCompare = _sourceFieldList[sourceIndex].Trim();
                            if (sourceNameCompare.Equals(altnamesList[altIndex],
                                                         StringComparison.OrdinalIgnoreCase))
                            {
                                sourceName = _sourceFieldList[sourceIndex];
                                break; // work done
                            }
                        }

                        if (!string.IsNullOrEmpty(sourceName))
                            break; // work done
                    }
                }

                _mapFields.Add(new ImportProfileEditFieldMapping(name, sourceName));
            }
        }

        /// <summary>
        /// Inits data provider.
        /// </summary>
        /// <param name="showMessage">Show problem message to users.</param>
        /// <returns>Data provider interface.</returns>
        private IDataProvider _InitDataProvider(bool showMessage)
        {
            IDataProvider provider = null;

            var comboboxTable = (ComboBox)ctrlDataSource.FindName("comboboxTable");
            string messageFailure = null;
            if (null != comboboxTable.SelectedItem)
            {
                provider = DataSourceOpener.Open(comboboxTable.SelectedItem.ToString(),
                                                 out messageFailure);
            }

            if ((null == provider) && showMessage && !string.IsNullOrEmpty(messageFailure))
                App.Current.Messenger.AddError(messageFailure);

            return provider;
        }

        /// <summary>
        /// Rebuilds field mapping source collection.
        /// </summary>
        private void _UpdateFieldMapping()
        {
            _sourceFieldList.Clear();
            ctrlFieldMapping.Visibility = Visibility.Collapsed;

            if (_validationRes[ValidationRes_Table])
            {
                IDataProvider provider = _InitDataProvider(!_IsWizardMode());
                if (null != provider)
                {
                    // create list of source field
                    ICollection<DataFieldInfo> infoSource = provider.FieldsInfo;
                    _sourceFieldList.Add(string.Empty);
                    foreach (DataFieldInfo info in infoSource)
                        _sourceFieldList.Add(info.Name);

                    _mapFields.Clear();

                    // obtain list of object property info
                    bool isSourceShape = (_IsDataSourceFile() &&
                                          FileHelpers.IsShapeFile(DataSourceOpener.FilePath));
                    _destinationFieldList =
                        PropertyHelpers.GetDestinationPropertiesTitle(_importProfile.Type, isSourceShape);

                    _CreateFieldMapping();

                    _UpdateFieldMappingPrev();

                    ctrlFieldMapping.Visibility = Visibility.Visible;
                    _UpdateFieldMappingTableView();
                }
            }
        }

        /// <summary>
        /// Updates field mapping table view.
        /// </summary>
        private void _UpdateFieldMappingTableView()
        {
            bool isSourceShape = (_IsDataSourceFile() &&
                                  FileHelpers.IsShapeFile(DataSourceOpener.FilePath));
            ICollection<ObjectDataFieldInfo> infos =
                PropertyHelpers.GetDestinationProperties(_importProfile.Type, isSourceShape);
            var fields = new List<ImportProfileEditFieldMapping>();
            string plannedDateName = App.Current.FindString("PlannedDateColumnHeader");
            foreach (ImportProfileEditFieldMapping map in _mapFields)
            {
                string name = map.ObjectFieldName;
                foreach (ObjectDataFieldInfo info in infos)
                {
                    if (map.ObjectFieldName == plannedDateName)
                    {
                        name = _AddSpecialIndication(name);
                        break; // work done
                    }
                    else if (info.Info.Name == name)
                    {
                        if (info.IsMandatory)
                            name = PropertyHelpers.AddOblygatoryIndication(name);
                        break; // work done
                    }
                }

                fields.Add(new ImportProfileEditFieldMapping(name, map.SourceFieldName));
            }

            _collectionSource.Source = fields;

            _UpdateButtonOKState();
            _UpdatePreviewTable();
        }

        /// <summary>
        /// Updates field mapping.
        /// </summary>
        /// <param name="type">Import type.</param>
        /// <param name="mapFields">Fields map.</param>
        private void _UpdateFieldMapping(ImportType type, List<FieldMap> mapFields)
        {
            StringDictionary mapTitle2Name = PropertyHelpers.GetTitle2NameMap(type);
            foreach (ImportProfileEditFieldMapping map in _mapFields)
            {
                bool isFounded = false;
                foreach (FieldMap mapProfile in mapFields)
                {
                    if (mapTitle2Name[map.ObjectFieldName] == mapProfile.ObjectFieldName)
                    {
                        map.SourceFieldName = mapProfile.SourceFieldName;
                        isFounded = true;
                        break; // result founded
                    }
                }

                if (!isFounded)
                    map.SourceFieldName = null;
            }

            _UpdateFieldMappingPrev();

            _UpdateFieldMappingTableView();
        }

        /// <summary>
        /// Creates field mapping.
        /// </summary>
        /// <param name="type">Type of import.</param>
        /// <returns>Field mapping.</returns>
        private List<FieldMap> _CreateFieldMap(ImportType type)
        {
            StringDictionary mapTitle2Name = PropertyHelpers.GetTitle2NameMap(type);

            var mapFields = new List<FieldMap> ();
            foreach (ImportProfileEditFieldMapping map in _mapFields)
            {
                Debug.Assert(null != mapTitle2Name[map.ObjectFieldName]);
                mapFields.Add(new FieldMap(mapTitle2Name[map.ObjectFieldName], map.SourceFieldName));
            }

            return mapFields;
        }

        /// <summary>
        /// Checks is imported data source invalid.
        /// </summary>
        /// <param name="importResult">Import process result.</param>
        /// <returns>TRUE if data source has invalid values.</returns>
        private bool _IsImportDataSourceInvalid(ImportResult importResult)
        {
            return importResult.Desciptions.Any(description =>
                                                    ImportedValueStatus.Failed == description.Status);
        }

        /// <summary>
        /// Updates preview table content.
        /// </summary>
        private void _UpdatePreviewTable<T>(int recordCount, ICollection<T> importedObjects)
            where T : AppData.DataObject
        {
            var lockedGridFrame = (Grid)ctrlFieldMapping.FindName("lockedGridFrame");
            var xceedGrid = (DataGridControl)ctrlFieldMapping.FindName(XCEEDGRID_PREVIEW_KEY);
            if (0 < importedObjects.Count)
            {
                _collectionSourcePreview.Source = importedObjects;
                xceedGrid.Visibility = Visibility.Visible;
                lockedGridFrame.Visibility = Visibility.Hidden;
            }
            else
            {
                var label = (Label)lockedGridFrame.Children[0];
                Debug.Assert("lockedLabel" == label.Name);

                // update lock message
                string labelResource = null;
                if (0 == recordCount)
                    labelResource = "ImportStatusFileIsEmpty";
                else if (_IsAllMandatoryFieldMapped(false))
                    labelResource = "ImportStatusInvalidData";
                else
                    labelResource = "ImportStatusRequiredFieldEmpty";

                buttonOk.IsEnabled = false;

                Debug.Assert(!string.IsNullOrEmpty(labelResource));
                label.Content = App.Current.FindString(labelResource);

                _collectionSourcePreview.Source = new List<T>();
                lockedGridFrame.Visibility = Visibility.Visible;
                xceedGrid.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Updates preview table content.
        /// </summary>
        private void _UpdatePreviewTable<T>()
            where T : AppData.DataObject
        {
            if (null == _collectionSourcePreview)
                return; // update not need - skip other work

            IDataProvider provider = _InitDataProvider(false);
            if (null == provider)
            {
                buttonOk.IsEnabled = false;
                return; // invalid state - skip other work
            }

            int recordsCount = Math.Min(provider.RecordsCount, PREVIEW_RECORDS_COUNT);
            var importedObjects = new List<T>(recordsCount);
            if ((0 < recordsCount) && _IsInputValid())
            {
                List<FieldMap> fieldMap = _CreateFieldMap(_importProfile.Type);
                Dictionary<string, int> references = PropertyHelpers.CreateImportMap(fieldMap, provider);

                using (var projectData = new ProjectDataContext(App.Current.Project))
                {
                    // import objects
                    provider.MoveFirst();
                    for (int index = 0; index < provider.RecordsCount; ++index)
                    {
                        ImportResult result = CreateHelpers.Create(_importProfile.Type,
                                                                  references,
                                                                  provider,
                                                                  projectData,
                                                                  _defaultDate);
                        AppData.DataObject obj = result.Object;

                        if (string.IsNullOrEmpty(obj.ToString()))
                            continue; // skip invalid object
                        string importedObjName = obj.ToString().Trim();
                        if (_IsImportDataSourceInvalid(result) ||
                            string.IsNullOrEmpty(importedObjName))
                            continue; // skip invalid object

                        // NOTE: set fake point - hide ungeocoded error
                        IGeocodable geocodable = obj as IGeocodable;
                        if ((null != geocodable) && !geocodable.IsGeocoded)
                            geocodable.GeoLocation = new ESRI.ArcLogistics.Geometry.Point(0, 0);

                        CreateHelpers.SpecialInit(importedObjects, obj);

                        importedObjects.Add((T)obj);
                        if (importedObjects.Count == PREVIEW_RECORDS_COUNT)
                            break; // work done

                        provider.MoveNext();
                    }
                }
            }

            _UpdatePreviewTable(provider.RecordsCount, importedObjects);
        }

        /// <summary>
        /// Updates preview table content.
        /// </summary>
        private void _UpdatePreviewTable()
        {
            switch (_importProfile.Type)
            {
                case ImportType.Orders:
                    _UpdatePreviewTable<Order>();
                    break;

                case ImportType.Locations:
                    _UpdatePreviewTable<Location>();
                    break;

                case ImportType.Drivers:
                    _UpdatePreviewTable<Driver>();
                    break;

                case ImportType.Vehicles:
                    _UpdatePreviewTable<Vehicle>();
                    break;

                case ImportType.MobileDevices:
                    _UpdatePreviewTable<MobileDevice>();
                    break;

                case ImportType.DefaultRoutes:
                    _UpdatePreviewTable<Route>();
                    break;

                case ImportType.DriverSpecialties:
                    _UpdatePreviewTable<DriverSpecialty>();
                    break;

                case ImportType.VehicleSpecialties:
                    _UpdatePreviewTable<VehicleSpecialty>();
                    break;

                case ImportType.Barriers:
                    _UpdatePreviewTable<Barrier>();
                    break;

                case ImportType.Zones:
                    _UpdatePreviewTable<Zone>();
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported type
                    break;
            }
        }

        /// <summary>
        /// Checks is input values valid.
        /// </summary>
        /// <returns>TRUE if all inputed values valid.</returns>
        private bool _IsInputValid()
        {
            var tbSource = (TextBox)ctrlDataSource.FindName("linkSource");
            var cbTable = (ComboBox)ctrlDataSource.FindName("comboboxTable");
            return (!string.IsNullOrEmpty(tbSource.Text) &&
                    (-1 < cbTable.SelectedIndex) &&
                    _IsValidFieldMapping(false));
        }

        /// <summary>
        /// Updates button "OK"\"Import" state.
        /// </summary>
        private void _UpdateButtonOKState()
        {
            buttonOk.IsEnabled = _IsInputValid();
        }

        /// <summary>
        /// Updates GUI controls from profile property.
        /// </summary>
        /// <param name="profile">Import profile properties to init contols.</param>
        private void _UpdateContent(ImportProfile profile)
        {
            ctrlDataSource.Visibility = Visibility.Collapsed;
            _validationRes.SetAll(false);

            WorkingStatusHelper.SetBusy(null);
            try
            {
                _isValidationEnabled = false;
                _mapFields.Clear();
                _prevMapFields.Clear();
                _importProfile = (ImportProfile)profile.Clone();
                ProfileSourceLinkValidationRule.ImportType = profile.Type;

                _prevConnectionString = null;
                _prevFilePath = null;
                _prevTableName = null;

                // update Data Sources
                ImportSettings settings = profile.Settings;

                bool isFileChecked = !DataSourceOpener.IsConnectionString(settings.Source);
                RadioButton rb = null;
                if (isFileChecked)
                {
                    rb = (RadioButton)ctrlDataSource.FindName(RADIOBUTTON_FILE);
                    DataSourceOpener.FilePath = settings.Source;
                }
                else
                {
                    rb = (RadioButton)ctrlDataSource.FindName(RADIOBUTTON_PROVIDER);
                    DataSourceOpener.ConnectionString = settings.Source;
                }
                rb.IsChecked = true;
                _UpdateDataSourceGrid(isFileChecked);

                // update link source
                var tbSource = (TextBox)ctrlDataSource.FindName("linkSource");
                tbSource.Text = settings.Source;

                _UpdateTableNames();

                // update seleted table
                var cbTable = (ComboBox)ctrlDataSource.FindName("comboboxTable");
                string tableName = settings.TableName;
                if (string.IsNullOrEmpty(tableName))
                    cbTable.SelectedIndex = -1;
                else
                    cbTable.SelectedItem = tableName;

                _isValidationEnabled = true;
                _ValidateData();

                // update Field Mapping from profile
                _prevMapFields.Clear();
                _UpdateFieldMapping(profile.Type, settings.FieldsMap);
            }
            finally
            {
                WorkingStatusHelper.SetReleased();
            }
        }

        /// <summary>
        /// Controls content to profile property.
        /// </summary>
        private ImportProfile _CreateProfile()
        {
            // Data Sources
            ImportSettings settings = _importProfile.Settings;

            var source = (TextBox)ctrlDataSource.FindName("linkSource");
            settings.Source = source.Text;

            var comboboxTable = (ComboBox)ctrlDataSource.FindName("comboboxTable");
            settings.TableName = comboboxTable.Text;

            settings.FieldsMap = _CreateFieldMap(_importProfile.Type);

            return _importProfile;
        }

        /// <summary>
        /// Inits preview data grid layout.
        /// </summary>
        protected void _InitPreviewDataGridLayout()
        {
            // select grid settings
            string structureFileName = null;
            string layoutSettings = null;
            switch (_importProfile.Type)
            {
                case ImportType.Orders:
                    structureFileName = GridSettingsProvider.OrdersGridStructure;
                    layoutSettings = GridSettingsProvider.OrdersSettingsRepositoryName;
                    break;

                case ImportType.Locations:
                    structureFileName = GridSettingsProvider.LocationsGridStructure;
                    layoutSettings = GridSettingsProvider.LoactionsSettingsRepositoryName;
                    break;

                case ImportType.Drivers:
                    structureFileName = GridSettingsProvider.DriversGridStructure;
                    layoutSettings = GridSettingsProvider.DriversSettingsRepositoryName;
                    break;

                case ImportType.Vehicles:
                    structureFileName = GridSettingsProvider.VehiclesGridStructure;
                    layoutSettings = GridSettingsProvider.VehiclesSettingsRepositoryName;
                    break;

                case ImportType.MobileDevices:
                    structureFileName = GridSettingsProvider.MobileDevicesGridStructure;
                    layoutSettings = GridSettingsProvider.MobileDevicesSettingsRepositoryName;
                    break;

                case ImportType.DefaultRoutes:
                    structureFileName = GridSettingsProvider.DefaultRoutesGridStructure;
                    layoutSettings = GridSettingsProvider.DefaultRoutesSettingsRepositoryName;
                    break;

                case ImportType.DriverSpecialties:
                    structureFileName = GridSettingsProvider.DriverSpecialtiesGridStructure;
                    layoutSettings = GridSettingsProvider.DriverSpecialtiesSettingsRepositoryName;
                    break;

                case ImportType.VehicleSpecialties:
                    structureFileName = GridSettingsProvider.VehicleSpecialtiesGridStructure;
                    layoutSettings = GridSettingsProvider.VehicleSpecialtiesSettingsRepositoryName;
                    break;

                case ImportType.Barriers:
                    structureFileName = GridSettingsProvider.BarriersGridStructure;
                    layoutSettings = GridSettingsProvider.BarriersSettingsRepositoryName;
                    break;

                case ImportType.Zones:
                    structureFileName = GridSettingsProvider.ZonesGridStructure;
                    layoutSettings = GridSettingsProvider.ZonesSettingsRepositoryName;
                    break;

                default:
                    Debug.Assert(false); // NOTE: not supported type
                    break;
            }

            // init grid structure
            if (_collectionSourcePreview == null)
            {
                _collectionSourcePreview =
                    (DataGridCollectionViewSource)layoutRoot.FindResource(COLLECTION_PREVIEW_SOURCE_KEY);
            }

            // set collection source to null to refresh items collection.
            _collectionSourcePreview.Source = null;

            // init grid
            var xceedGrid = (DataGridControl)ctrlFieldMapping.FindName(XCEEDGRID_PREVIEW_KEY);
            var structureInitializer = new GridStructureInitializer(structureFileName);
            structureInitializer.BuildGridStructure(_collectionSourcePreview, xceedGrid);

            // load grid layout
            var layoutLoader = new GridLayoutLoader(layoutSettings,
                                                    _collectionSourcePreview.ItemProperties);
            layoutLoader.LoadLayout(xceedGrid);

            _UpdatePreviewTable();
        }

        /// <summary>
        /// Inits default import date.
        /// </summary>
        private void _InitDefaultDate()
        {
            _defaultDate = App.Current.CurrentDate;

            if (_IsWizardMode())
            {
                double nextWorkDayOffset = 1; // import orders for the next day
                if (_defaultDate.DayOfWeek == DayOfWeek.Friday)
                    nextWorkDayOffset += 2; // if current day is Friday, push until Monday
                                            // (+ Saturday and Sunday)
                _defaultDate = _defaultDate.AddDays(nextWorkDayOffset);
            }
        }

        /// <summary>
        /// Adds special indication mark.
        /// </summary>
        /// <param name="name">Object field name.</param>
        /// <returns>Name with special mark if need.</returns>
        private string _AddSpecialIndication(string name)
        {
            string result = name;
            if (_IsWizardMode())
            {   // CR 165731 - add special mark
                result += SPECIAL_FIELD_MARK;
            }

            return result;
        }

        /// <summary>
        /// Clears all indication marks.
        /// </summary>
        /// <param name="name">Visual name.</param>
        /// <returns>Name without indication marks.</returns>
        private string _ClearSpecialIndication(string name)
        {
            string result = name;
            if (name.EndsWith(SPECIAL_FIELD_MARK))
            {   // CR 165731 - remove hard code indication
                int markLenght = SPECIAL_FIELD_MARK.Length;
                result = name.Remove(name.Length - markLenght, markLenght);
            }

            return PropertyHelpers.ClearOblygatoryIndication(result);
        }

        /// <summary>
        /// Checks is wizard mode.
        /// </summary>
        /// <returns>TRUE if page call from wizard.</returns>
        private bool _IsWizardMode()
        {
            return (null != DataKeeper);
        }

        #endregion // Private methods

        #region Private consts
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string COLLECTION_SOURCE_KEY = "fieldMappingCollection";
        private const string COLLECTION_PREVIEW_SOURCE_KEY = "importedObjectCollection";
        private const string XCEEDGRID_PREVIEW_KEY = "xceedGridPreview";

        private const string RADIOBUTTON_FILE = "radiobuttonFile";
        private const string RADIOBUTTON_PROVIDER = "radiobuttonProvider";
        private const string RADIOBUTTON_SELECT_DEF = RADIOBUTTON_FILE;

        private const string FORMAT = "{0},{1}";
        private const char FIELD_ALIASES_DELIMITER = ',';

        private const string FILE_EXTENSION_CSV = ".csv";
        private const string FILE_EXTENSION_TXT = ".txt";
        private const char TEXT_FILE_NAME_SEPARATOR = '#';

        private const string SPECIAL_FIELD_MARK = "**";

        private const int PREVIEW_RECORDS_COUNT = 2;

        #endregion // Private consts

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Column inited flag.
        /// </summary>
        private bool _isColumnInited;

        /// <summary>
        /// Is edit profile canceled flag.
        /// </summary>
        private bool _isProcessCancelled;

        /// <summary>
        /// Data source for grid.
        /// </summary>
        private List<ImportProfileEditFieldMapping> _mapFields =
                                                        new List<ImportProfileEditFieldMapping>();

        /// <summary>
        /// Collection view source.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource;

        /// <summary>
        /// Collection view source.preview.
        /// </summary>
        private DataGridCollectionViewSource _collectionSourcePreview;

        /// <summary>
        /// Previous state description.
        /// </summary>
        private bool _prevCheckedState = true;
        private string _prevConnectionString;
        private string _prevFilePath;
        private string _prevTableName;
        private List<ImportProfileEditFieldMapping> _prevMapFields =
                                                        new List<ImportProfileEditFieldMapping>();

        /// <summary>
        /// Special aliases for dynamical fields (AddressFields, Capacities and CustomOrderProperties).
        /// </summary>
        private StringDictionary _aliasesSpecial;

        /// <summary>
        /// Stored state.
        /// </summary>
        private ImportProfile _importProfile;

        // Validation state members.
        private const int ValidationRes_Source = 0;
        private const int ValidationRes_Table = 1;
        private const int ValidationRes_Count = ValidationRes_Table + 1;

        private BitArray _validationRes = new BitArray(ValidationRes_Count, false);
        private bool _isValidationEnabled = true;

        // Grid data sources.
        private StringCollection _sourceFieldList = new StringCollection();
        private StringCollection _destinationFieldList;

        /// <summary>
        /// Default date for inits objects.
        /// </summary>
        private DateTime _defaultDate;

        /// <summary>
        /// Import profile to store state.
        /// </summary>
        private ImportProfile _profile;

        #endregion // Private fields
    }
}
