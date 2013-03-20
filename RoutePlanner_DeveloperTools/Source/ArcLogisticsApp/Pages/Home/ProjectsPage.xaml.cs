using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;

using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;
using Xceed.Wpf.DataGrid.ValidationRules;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.App.GridHelpers;
using System.Windows.Threading;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for ProjectsPage.xaml
    /// </summary>
    internal partial class ProjectsPage : PageBase
    {
        public const string PAGE_NAME = "Projects";

        #region Constructors

        public ProjectsPage()
        {
            InitializeComponent();
            App.Current.ApplicationInitialized += new EventHandler(App_ApplicationInitialized);
            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.Exit += new ExitEventHandler(Current_Exit);

            this.Loaded += new RoutedEventHandler(ProjectsPage_Loaded);
            this.Unloaded += new RoutedEventHandler(ProjectsPage_Unloaded);

            IsRequired = true;
            IsAllowed = true;
            IsComplete = true;
            DoesSupportCompleteStatus = true;
        }

        #endregion

        #region Page Overrided Members

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("ProjectsPageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("ProjectsBrush"); }
        }

        #endregion

        #region PageBase overrided members

        internal override void SaveLayout()
        {
            if (Properties.Settings.Default.ProjectGridSettings == null)
                Properties.Settings.Default.ProjectGridSettings = new SettingsRepository();

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.ProjectGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Event raised when any data cell changed
        /// </summary>
        public static readonly RoutedEvent DataChangedEvent = EventManager.RegisterRoutedEvent("DataChanged",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ProjectsPage));

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.ProjectsPagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return CategoryNames.ProjectTaskWidgetCommands; }
        }

        #endregion

        #region Internal interface

        /// <summary>
        /// Name of current loaded project.
        /// </summary>
        internal string CurrentProjectName
        {
            get
            {
                IProject project = App.Current.Project;
                return (project == null)? null : project.Name;
            }
        }

        /// <summary>
        /// Name of current selected project
        /// </summary>
        internal string SelectedProjectName
        {
            get
            {
                if (XceedGrid.SelectedItem == null)
                    return null;

                return (XceedGrid.SelectedItem as ProjectDataWrapper).Name;
            }
        }

        /// <summary>
        /// Update view content
        /// </summary>
        internal void UpdateView()
        {
            if (_isInited)
            {
                App.Current.ProjectCatalog.Refresh();

                _selectedProjectName = SelectedProjectName;
                _BuildProjectsWrapperCollection();
                _UpdateProjectsCheckboxes();
                _collectionSource.Source = _projectsDataCollection;
                _UpdateDeleteButtonState();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks is page complete
        /// </summary>
        private void _CheckPageComplete()
        {
            IsComplete = (App.Current.Project != null);
        }

        /// <summary>
        /// Loads grid layout if it wasn't loaded before.
        /// </summary>
        private void _LoadLayout()
        {
            if (Properties.Settings.Default.ProjectGridSettings == null)
                return;
            if (!_isLayoutLoaded)
            {
                try
                {
                    this.XceedGrid.LoadUserSettings(Properties.Settings.Default.ProjectGridSettings, UserSettings.All);
                    _isLayoutLoaded = true;
                }
                catch
                {
                    _isLayoutLoaded = true;
                }
            }

            XceedGrid.UpdateLayout();
        }

        /// <summary>
        /// Method inits collection of projects.
        /// </summary>
        private void _InitProjectsCollection()
        {
            _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource("projectSource");

            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.ProjectsGridStructure);
            structureInitializer.BuildGridStructure(_collectionSource, XceedGrid);

            XceedGrid.Columns[IS_CURRENT_COLUMN_CAPTION].CellEditor = (CellEditor)LayoutRoot.FindResource("ProjectsRadioButtonEditor");
            XceedGrid.Columns[IS_CURRENT_COLUMN_CAPTION].CellContentTemplate = (DataTemplate)LayoutRoot.FindResource("RadioButtonTemplate");
            XceedGrid.Columns[IS_CURRENT_COLUMN_CAPTION].CellEditorDisplayConditions = CellEditorDisplayConditions.Always;

            XceedGrid.Columns[NAME_COLUMN_CAPTION].CellValidationRules.Add(new ProjectNameValidationRule());

            _BuildProjectsWrapperCollection();
            _UpdateProjectsCheckboxes();
            _collectionSource.Source = _projectsDataCollection;

            _isInited = true;
        }

        private void _UpdateDeleteButtonState()
        {
            string name = SelectedProjectName;
            // selected project is not current project
            bool isEnabled = string.IsNullOrEmpty(name) || !name.Equals(CurrentProjectName, StringComparison.InvariantCultureIgnoreCase);
            DeleteButton.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Method rebuilds collection of projects.
        /// </summary>
        private void _BuildProjectsWrapperCollection()
        {
            _projectsDataCollection = new List<ProjectDataWrapper>();
            foreach (ProjectConfiguration project in App.Current.ProjectCatalog.Projects)
                _projectsDataCollection.Add(new ProjectDataWrapper(false, project.Name, project.Description));
        }

        /// </summary>
        /// Method updates projects selection.
        /// </summary>
        private void _UpdateProjectsCheckboxes()
        {
            if (!string.IsNullOrEmpty(CurrentProjectName))
            {
                foreach (ProjectDataWrapper pr in _projectsDataCollection)
                    pr.IsCurrent = pr.Name.Equals(CurrentProjectName, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Reloads project when user checks any CheckBox.
        /// </summary>
        protected void _LoadProject(string checkedProjectName)
        {
            try
            {
                WorkingStatusHelper.SetBusy((string)App.Current.FindResource("LoadingProjectStatus"));

                // init old project path
                string oldProjectPath = String.Empty;
                if (App.Current.Project != null)
                    oldProjectPath = App.Current.Project.Path;

                if (!checkedProjectName.Equals(CurrentProjectName, StringComparison.OrdinalIgnoreCase))
                    _TryToLoadProject(oldProjectPath, checkedProjectName);
            }
            finally
            {
                WorkingStatusHelper.SetReleased();
                _CheckPageComplete();
            }
        }

        /// <summary>
        /// Try to load project.
        /// </summary>
        /// <param name="oldProjectPath">Old project name.</param>
        /// <param name="checkedProjectName">Project to load.</param>
        private void _TryToLoadProject(string oldProjectPath, string checkedProjectName)
        {
            // find checked project configuration
            ProjectConfiguration checkedPrjConfig = null;
            foreach (ProjectConfiguration prj in App.Current.ProjectCatalog.Projects)
            {
                if (checkedProjectName.Equals(prj.Name, StringComparison.OrdinalIgnoreCase))
                {
                    checkedPrjConfig = prj;
                    break; // NOTE: founded
                }
            }

            try
            {
                App.Current.OpenProject(checkedPrjConfig.FilePath, true);
            }
            catch (DataException ex)
            {
                Logger.Info(ex);
                if (DataError.NotSupportedFileVersion == ex.ErrorCode)
                {
                    List<MessageDetail> details = new List<MessageDetail>();
                    details.Add(new MessageDetail(MessageType.Warning, ex.Message));
                    App.Current.Messenger.AddWarning(App.Current.FindString("UnableOpenProject"), details);
                }
                else
                {
                    string text = App.Current.FindString((DataError.FileSharingViolation == ex.ErrorCode) ?
                                                         "UnableOpenAlreadyOpenedProject" : "UnableOpenProject");
                    App.Current.Messenger.AddWarning(text);
                }
            }
            catch (ApplicationException ex)
            {
                Logger.Info(ex);
                App.Current.Messenger.AddWarning(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Info(ex);

                List<MessageDetail> details = new List<MessageDetail>();
                details.Add(new MessageDetail(MessageType.Warning, ex.Message));
                App.Current.Messenger.AddWarning(App.Current.FindString("UnableOpenProject"), details);
            }

            // open last project if current project opening failed
            if (App.Current.Project == null)
            {
                if (!String.IsNullOrEmpty(oldProjectPath))
                    App.Current.OpenProject(oldProjectPath, false);
            }

            _UpdateProjectsCheckboxes();
            _collectionSource.Source = _projectsDataCollection;
            _UpdateDeleteButtonState();
        }

        /// <summary>
        /// Method edits project.
        /// </summary>
        /// <param name="selectedItem"></param>
        private void _EditProject(ProjectDataWrapper selectedItem)
        {
            try
            {
                string nameOfEditedProject = selectedItem.Name;

                if (!string.IsNullOrEmpty(_projectNameBeforeEditing))
                {
                    nameOfEditedProject = _projectNameBeforeEditing;
                    _projectNameBeforeEditing = string.Empty;
                }

                // Search project with necessary name in ProjectCatalog.
                var configs = from cfg in App.Current.ProjectCatalog.Projects
                                                         where cfg.Name.Equals(nameOfEditedProject)
                                                         select cfg;

                Debug.Assert(configs.ToList<ProjectConfiguration>().Count > 0);

                ProjectConfiguration editedProjectCfg = configs.First();


               // ProjectConfiguration editedProjectCfg = App.Current.ProjectCatalog.Projects.ElementAt(XceedGrid.SelectedIndex);

                Logger.Info("////////// Edited project " + editedProjectCfg.Name);

                Logger.Info("////////// Selected project " + selectedItem.Name);

                if (selectedItem.Description != editedProjectCfg.Description)
                {
                    editedProjectCfg.Description = selectedItem.Description;

                    // in case of editing current project need to rename at project 
                    if (editedProjectCfg.Name.Equals(App.Current.Project.Name, StringComparison.OrdinalIgnoreCase))
                        App.Current.Project.Description = selectedItem.Description;
                }

                if (!selectedItem.Name.Equals(editedProjectCfg.Name, StringComparison.OrdinalIgnoreCase))
                {
                    string oldProjectName = editedProjectCfg.Name;
                    ProjectFactory.RenameProject(editedProjectCfg, selectedItem.Name, App.Current.ProjectCatalog);
                    if (oldProjectName.Equals(App.Current.Project.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Settings settings = Settings.Default;
                        settings.LastProjectName = System.IO.Path.GetFileName(editedProjectCfg.FilePath);
                        settings.Save();
                    }
                }

                editedProjectCfg.Save();
                App.Current.ProjectCatalog.Refresh();
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }

        /// <summary>
        /// Starts editing in "Name" cell
        /// </summary>
        private void _BeginEditNameCell()
        {
            _InsertionRow.Cells[NAME_COLUMN_CAPTION].BeginEdit();
        }

        /// <summary>
        /// Changed selection in RadioButtons and reloads project
        /// </summary>
        /// <param name="e"></param>
        private void _ChangeSelectedProject(RoutedEventArgs e)
        {
            if (XceedGrid.SelectedIndex != -1)
            {
                Row row = XceedVisualTreeHelper.GetRowByEventArgs(e);

                Debug.Assert(row != null);

                // NOTE : when user clicks so fast on radio buttons sometimes row's content can't be updated in time.
                // In that case we should do nothing
                if (row.Cells[NAME_COLUMN_CAPTION].Content == null)
                    return;

                string checkedProjectName = row.Cells[NAME_COLUMN_CAPTION].Content.ToString();

                _LoadProject(checkedProjectName);
                try
                {
                    XceedGrid.EndEdit();
                }
                catch
                {
                    XceedGrid.CancelEdit();
                }
            }
        }

        #endregion

        #region Event Handlers

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            SaveLayout();
        }

        private void App_ProjectLoaded(object sender, EventArgs e)
        {
            if (_isInited)
            {
                _UpdateProjectsCheckboxes();
                _UpdateDeleteButtonState();
            }

            _CheckPageComplete();
            App.Current.MainWindow.StatusBar.SetStatus(this, null);
        }

        private void ProjectsPage_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled += new EventHandler(ProjectsPage_NavigationCalled);
            if (!_isInited && App.Current.ProjectCatalog != null)
                _InitProjectsCollection();

            _LoadLayout();
            _CheckPageComplete();

            // set void status bar content
            App.Current.MainWindow.StatusBar.SetStatus(this, null);
        }

        private void ProjectsPage_NavigationCalled(object sender, EventArgs e)
        {
            try
            {
                XceedGrid.CancelEdit();
                CanBeLeft = true;
            }
            catch
            {
                CanBeLeft = false;
            }
        }

        private void ProjectsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled -= ProjectsPage_NavigationCalled;
        }

        private void App_ApplicationInitialized(object sender, EventArgs e)
        {
            // If project workspace was inited.
            if (App.Current.ProjectCatalog != null)
            {
                _CheckPageComplete();

                if (!_isInited)
                    _InitProjectsCollection();
            }
            else
                IsAllowed = false;
        }

        private void InsertionRow_Initialized(object sender, EventArgs e)
        {
            _InsertionRow = sender as InsertionRow;
            if (0 < _InsertionRow.Cells.Count)
                _InsertionRow.Cells[IS_CURRENT_COLUMN_CAPTION].Template = null;
        }

        /// <summary>
        /// Delete button click handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _DeleteProject();
        }

        /// <summary>
        /// Delete current project.
        /// </summary>
        private void _DeleteProject()
        {
            // deletes item in insertion row
            if (_InsertionRow.IsBeingEdited)
            {
                XceedGrid.CancelEdit();
                return;
            }

            if (XceedGrid.IsBeingEdited)
                XceedGrid.CancelEdit();

            string selectedProjectName = SelectedProjectName;
            if (selectedProjectName == null)
                return;

            bool doProcess = true;
            if (Settings.Default.IsAllwaysAskBeforeDeletingEnabled)
                // show warning dialog
                doProcess = DeletingWarningHelper.Execute(selectedProjectName, "Project", "Project");

            // do process
            if (doProcess)
            {
                _itemIndexToSelection = XceedGrid.SelectedIndex;

                string path = string.Empty;
                foreach (ProjectConfiguration project in App.Current.ProjectCatalog.Projects)
                {
                    if (project.Name.Equals(selectedProjectName, StringComparison.OrdinalIgnoreCase))
                    {
                        path = project.FilePath;
                        break; // NOTE: founded
                    }
                }

                try
                {
                    ProjectFactory.DeleteProject(path);
                    UpdateView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(App.Current.MainWindow, ex.Message, (string)App.Current.FindResource("WarningMessageBoxTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Occcurs when user clicks on radio button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _ChangeSelectedProject(e);
        }

        /// <summary>
        /// Occurs when user press keyboard key on radio button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                _ChangeSelectedProject(e);
        }

        /// <summary>
        /// Occurs when user set focus to any grid cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataCellGotFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentProjectName != null)
            {
                ProjectDataWrapper selectedItem = (ProjectDataWrapper)XceedGrid.CurrentItem;
                if (selectedItem == null)
                    return;

                DataCell cell = sender as DataCell;
                if (cell.ParentColumn == XceedGrid.Columns[IS_CURRENT_COLUMN_CAPTION]) // if user clicks to "IsCurrent" cell - don't show editing status.
                    return;

                bool stopEdit = false;
                if (cell.ParentColumn == XceedGrid.Columns[NAME_COLUMN_CAPTION])
                    stopEdit = (string.Compare(SelectedProjectName, CurrentProjectName, true) == 0);

                cell.ReadOnly = stopEdit;
                if (stopEdit)
                {
                    string status = (string)App.Current.FindResource("UnaEditCurrentProjectNameTooltip");
                    cell.ToolTip = status;
                    cell.EndEdit();
                    cell.ParentRow.EndEdit();
                    _isForcingEndEdit = true;

                    App.Current.MainWindow.StatusBar.SetStatus(this, status);
                    _needToUpdateStatus = false;
                }
            }
        }

        private void CellEditCanceled(object sender, RoutedEventArgs e)
        {
            Cell cell = (Cell)sender;
            if (cell.ParentColumn == XceedGrid.Columns[NAME_COLUMN_CAPTION])
                cell.ParentRow.Cells[IS_CURRENT_COLUMN_CAPTION].IsEnabled = true;
            e.Handled = true;
        }

        /// <summary>
        /// Occurs insertion row initialized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XceedGrid_InitializingInsertionRow(object sender, InitializingInsertionRowEventArgs e)
        {
            e.InsertionRow.Cells[IS_CURRENT_COLUMN_CAPTION].Visibility = Visibility.Hidden; // NOTE: hide redundant check box
        }

        // Delegate with zero parameters
        private delegate void ZeroParamsDelegate();

        /// <summary>
        /// Occurs when set focus to inits insertion row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCollectionViewSource_CreatingNewItem(object sender, DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new ProjectDataWrapper(true, string.Empty, string.Empty);
            e.Handled = true;

            DeleteButton.IsEnabled = true;

            // If user clicks into "IsCurrent" cell in insertion row - move focus to "Name" cell and start edit it.
            if (_InsertionRow.Cells[IS_CURRENT_COLUMN_CAPTION].IsCurrent)
                Dispatcher.BeginInvoke(new ZeroParamsDelegate(_BeginEditNameCell), System.Windows.Threading.DispatcherPriority.SystemIdle);

            _statusBuilder.FillCreatingStatus(PROJECT_TYPE_NAME, this);
            _needToUpdateStatus = false;
        }

        private delegate void ParamsDelegate(DataGridItemEventArgs item);

        /// <summary>
        /// Change the name of the project.
        /// </summary>
        /// <param name="e">DataGridItemEventArgs.</param>
        private void _ChangeNewProjectName(DataGridItemEventArgs e)
        {
            // Check that item's name is null.
            if (!string.IsNullOrEmpty((e.Item as ProjectDataWrapper).Name))
                return;

            // Get new project name.
            (e.Item as ProjectDataWrapper).Name = DataObjectNamesConstructor.GetNewNameForProject();

            // Find TextBox inside the cell and select new name.
            Cell currentCell = _InsertionRow.Cells[XceedGrid.CurrentContext.CurrentColumn];
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Changing project's name.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">DataGridItemEventArgs.</param>
        private void DataGridCollectionViewSource_NewItemCreated(object sender, DataGridItemEventArgs e)
        {
            // Invoking changing of the project's name. Those methode must be invoke, otherwise 
            // grid didnt understand that item in insertion was changed and grid wouldnt allow to 
            // commit this item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_ChangeNewProjectName),
                    DispatcherPriority.Render, e);
        }

        private void DataGridCollectionViewSource_NewItemCommitted(object sender,
                                                                   DataGridItemEventArgs e)
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, null);
        }

        /// <summary>
        /// Occurs when user press enter in inserion row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCollectionViewSource_CommittingNewItem(object sender, DataGridCommittingNewItemEventArgs e)
        {
            try
            {
                WorkingStatusHelper.SetBusy((string)App.Current.FindResource("LoadingProjectStatus"));

                ProjectCatalog projectBrowser = App.Current.ProjectCatalog;

                List<ProjectDataWrapper> source = e.CollectionView.SourceCollection as List<ProjectDataWrapper>;
                ProjectDataWrapper projectDataTemplate = e.Item as ProjectDataWrapper;

                App.Current.NewProject(projectDataTemplate.Name,
                    projectBrowser.FolderPath,
                    projectDataTemplate.Description);

                source.Add(projectDataTemplate);

                // update layout
                UpdateView();

                e.Index = source.Count - 1;
                e.NewCount = source.Count;
                e.Handled = true;
            }
            catch (ApplicationException ex)
            {
                Logger.Info(ex);
                App.Current.Messenger.AddWarning(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
                App.Current.Messenger.AddWarning(ex.Message);
            }
            finally
            {
                WorkingStatusHelper.SetReleased();
                App.Current.MainWindow.StatusBar.SetStatus(this, null);
            }
        }

        /// <summary>
        /// Occurs when user press "enter" on edited grid row.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCollectionViewSource_CommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, null);

            Row parentRow = (Row)XceedGrid.GetContainerFromItem(e.Item);
            parentRow.Cells[IS_CURRENT_COLUMN_CAPTION].IsEnabled = true;

            ProjectDataWrapper editedItem = e.Item as ProjectDataWrapper;
            _EditProject(editedItem);

            e.Handled = true;
        }

        private void DataGridCollectionViewSource_EditBegun(object sender, DataGridItemEventArgs e)
        {
            if (XceedGrid.CurrentColumn == XceedGrid.Columns[IS_CURRENT_COLUMN_CAPTION]) // if user clicks to "IsCurrent" cell - don't show editing status.
                return;

            string selectedProjectName = SelectedProjectName;

            Row parentRow = (Row)XceedGrid.GetContainerFromItem(e.Item);
            if (null == parentRow)
                return;

            bool stopEdit = false;
            if (XceedGrid.CurrentColumn == XceedGrid.Columns[NAME_COLUMN_CAPTION])
            {
                _projectNameBeforeEditing = ((ProjectDataWrapper)e.Item).Name;

                if (null != CurrentProjectName)
                {
                    if ((null != selectedProjectName) && selectedProjectName.Equals(CurrentProjectName, StringComparison.OrdinalIgnoreCase))
                        stopEdit = true;
                }
            }
            parentRow.Cells[IS_CURRENT_COLUMN_CAPTION].IsEnabled = stopEdit;

            Cell cell = parentRow.Cells[NAME_COLUMN_CAPTION];
            cell.ReadOnly = stopEdit;
            if (stopEdit)
            {
                string status = (string)App.Current.FindResource("UnaEditCurrentProjectNameTooltip");
                cell.ToolTip = status;
                cell.EndEdit();
                parentRow.CancelEdit();
                _isForcingEndEdit = true;

                App.Current.MainWindow.StatusBar.SetStatus(this, status);
                _needToUpdateStatus = false;
            }
            else
            {
                if (null != selectedProjectName)
                {
                    _statusBuilder.FillEditingStatus(selectedProjectName, PROJECT_TYPE_NAME, this);
                    _needToUpdateStatus = false;
                }
            }
        }

        private void DataGridCollectionViewSource_BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            e.Handled = true;
        }

        private void DataGridCollectionViewSource_CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            if (!_isForcingEndEdit)
                App.Current.MainWindow.StatusBar.SetStatus(this, null);
            _isForcingEndEdit = false;

            Row parentRow = (Row)XceedGrid.GetContainerFromItem(e.Item);
            parentRow.Cells[IS_CURRENT_COLUMN_CAPTION].IsEnabled = true;

            _projectNameBeforeEditing = string.Empty;

            e.Handled = true;
        }

        private void DataGridCollectionViewSource_CancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, null);
            e.Handled = true;
        }

        private void XceedGrid_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (_needToUpdateStatus)
                App.Current.MainWindow.StatusBar.SetStatus(this, null);

            _needToUpdateStatus = true;
            _UpdateDeleteButtonState();
        }

        /// <summary>
        /// ExceedGrid Item Source Changed handler.
        /// </summary>
        private void XceedGrid_OnItemSourceChanged(object sender, EventArgs e)
        {
            // select item
            if (-1 != _itemIndexToSelection)
                XceedGrid.SelectedIndex = Math.Min(_itemIndexToSelection, XceedGrid.Items.Count - 1);
            else if (!string.IsNullOrEmpty(_selectedProjectName))
            {
                for (int index = 0; index < _projectsDataCollection.Count; ++index)
                {
                    ProjectDataWrapper wrapper = _projectsDataCollection[index];
                    if (_selectedProjectName == wrapper.Name)
                    {
                        XceedGrid.SelectedItem = wrapper;
                        break;
                    }
                }
            }
            XceedGrid.CurrentItem = XceedGrid.SelectedItem;

            _selectedProjectName = null;
            _itemIndexToSelection = -1;
        }

        private void DeleteButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DeleteButton.ToolTip = (string)App.Current.FindResource((DeleteButton.IsEnabled)?
                                                                        "DeleteCommandEnabledTooltip" :
                                                                        "DeleteCommandDisabledTooltip");
        }

        private void DataGridCollectionViewSource_NewItemCanceled(object sender, DataGridItemEventArgs e)
        {
            DeleteButton.IsEnabled = false;
            App.Current.MainWindow.StatusBar.SetStatus(this, null);
        }

        /// <summary>
        /// React on key down.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Key down event args.</param>
        private void _LayoutRootKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == DELETE_KEY && Keyboard.Modifiers == ModifierKeys.None && DeleteButton.IsEnabled)
            {
                _DeleteProject();
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Key for executing Delete.
        /// </summary>
        private const Key DELETE_KEY = Key.Delete;

        #endregion

        #region Private fields

        private static string PROJECT_TYPE_NAME = "Project";
        private static string NAME_COLUMN_CAPTION = "Name";
        private static string IS_CURRENT_COLUMN_CAPTION = "IsCurrent";

        private InsertionRow _InsertionRow;

        /// <summary>
        /// Data source for grid.
        /// </summary>
        private List<ProjectDataWrapper> _projectsDataCollection;

        /// <summary>
        /// Collection view source.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource;

        /// <summary>
        /// Flad sets to true if project already loaded
        /// </summary>
        private bool _isInited = false;

        private bool _isLayoutLoaded = false;

        private bool _isForcingEndEdit = false;

        private string _selectedProjectName = null;

        private StatusBuilder _statusBuilder = new StatusBuilder();
 
        /// <summary>
        /// Project name before editing.
        /// </summary>
        private string _projectNameBeforeEditing = string.Empty;

        /// <summary>
        /// Item index to selection. Index of deleted item
        /// </summary>
        private int _itemIndexToSelection = -1;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus = false;

        #endregion
    }

    /// <summary>
    /// Class defines validation rule for project name field.
    /// </summary>
    internal class ProjectNameValidationRule : CellValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo culture, CellValidationContext context)
        {
            ValidationResult result = null;

            // not empty check
            bool isObjectEmpty = true;
            if (null != value)
                isObjectEmpty = string.IsNullOrEmpty(value.ToString().Trim()); // project name cannot consist only in blanks
            if (isObjectEmpty)
                result = new ValidationResult(false, (string)Application.Current.FindResource("ProjectNameValidationRuleIncorrectNameError"));

            if (null == result)
            {
                string name = value.ToString().Trim();
                if (-1 != name.IndexOfAny(new char[] {'\\', '/', '*', ';', ',', ':', '|', '"'}))
                    result = new ValidationResult(false, (string)Application.Current.FindResource("ProjectNameValidationRuleIncorrectNameError"));
                else
                {
                    ProjectsPage projectsPage = (ProjectsPage)App.Current.MainWindow.GetPage(PagePaths.ProjectsPagePath);
                    ProjectDataWrapper currentItem = (ProjectDataWrapper)projectsPage.XceedGrid.CurrentItem;

                    // check duplicate
                    ItemCollection wrappedCollection = projectsPage.XceedGrid.Items;
                    foreach (ProjectDataWrapper wrapper in wrappedCollection)
                    {
                        if (name.Equals(wrapper.Name, StringComparison.InvariantCultureIgnoreCase) && (wrapper != currentItem))
                        {
                            result = new ValidationResult(false, (string)App.Current.FindResource("ProjectNameValidationRuleDuplicateNameError"));
                            break; // NOTE: exit - error founded
                        }
                    }

                    if (null == result)
                    {   // normal length check
                        string fileName = name + ProjectConfiguration.FILE_EXTENSION; // NOTE: check only one file name,
                            // but real created two files: ProjectConfiguration.FILE_EXTENSION and DatabaseEngine.DATABASE_EXTENSION
                        string filePath = null;
                        try
                        {
                            filePath = _GetDatabaseAbsolutPath(App.Current.ProjectCatalog.FolderPath, fileName);
                        }
                        catch
                        {
                        }

                        // valid name check
                        if (!FileHelpers.IsFileNameCorrect(filePath) || !FileHelpers.ValidateFilepath(filePath))
                            result = new ValidationResult(false, (string)Application.Current.FindResource("ProjectNameValidationRuleIncorrectNameError"));
                    }
                }
            }

            return (null == result)? ValidationResult.ValidResult : result;
        }

        // NOTE: copy of ProjectConfiguration.GetDatabaseAbsolutPath()
        private string _GetDatabaseAbsolutPath(string projectFolder, string filepath)
        {
            return (FileHelpers.IsAbsolutPath(filepath))? filepath : Path.Combine(projectFolder, filepath);
        }
    }
}
