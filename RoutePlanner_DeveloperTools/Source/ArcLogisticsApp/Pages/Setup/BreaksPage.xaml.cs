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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Commands;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Dialogs;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Pages.Wizards;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.BreaksHelpers;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Settings;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for Breaks.xaml
    /// </summary>
    internal partial class BreaksPage :
        PageBase,
        ISupportSelection,
        ICancelDataObjectEditing,
        ISupportSelectionChanged
    {

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public BreaksPage()
        {
            InitializeComponent();
            _InitEventHandlers();
            _SetDefaults();
            _CheckPageComplete();

            // Init validation callout controller.
            var timeWindowVallidationCalloutController = new ValidationCalloutController(TimeWindowGrid);

            // Init validation callout controller.
            var driveTimeVallidationCalloutController = new ValidationCalloutController(DriveTimeGrid);

            // Init validation callout controller.
            var worktTimeVallidationCalloutController = new ValidationCalloutController(WorkTimeGrid);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occur when user starts editing cell.
        /// </summary>
        internal event EventHandler EditBegun;

        /// <summary>
        /// Occur when user ends editing cell.
        /// </summary>
        internal event EventHandler EditFinished;

        #endregion

        #region Public properties

        /// <summary>
        /// Flag, which show is some item in grid is editing or not.
        /// </summary>
        public bool IsEditingInProgress
        {
            get
            {
                return _isEditingInProgress;
            }

            protected set
            {
                _isEditingInProgress = value;
            }
        }

        #endregion

        #region ICancelDataObjectEditing Members

        /// <summary>
        /// Cancels editing Object.
        /// </summary>
        public void CancelObjectEditing()
        {
            _currentGrid.CancelEdit();
        }

        /// <summary>
        /// Cancels creating new Object (clears InsertionRow).
        /// </summary>
        public void CancelNewObject()
        {
            Debug.Assert(false); // Not implemented there.
        }

        #endregion

        #region ISupportSelection Members

        /// <summary>
        /// Items, selected in grid.
        /// </summary>
        public System.Collections.IList SelectedItems
        {
            get
            {
                return _currentGrid.SelectedItems;
            }
        }

        /// <summary>
        /// Not implemented there
        /// </summary>
        /// <returns></returns>
        public bool SaveEditedItem()
        {
            return false;
        }

        /// <summary>
        /// Set grid selection to items.
        /// </summary>
        /// <param name="items">Breaks, which must be selected.</param>
        public void Select(System.Collections.IEnumerable items)
        {
            // Check that editing is not in progress.
            if (IsEditingInProgress)
                throw new NotSupportedException(App.Current.FindString("EditingInProcessExceptionMessage"));

            // Check that all items are breaks.
            foreach (object item in items)
            {
                if (!(item is Break))
                    throw new ArgumentException(App.Current.FindString("BreaksTypeExceptionMessage"));
            }

            // Add items to selection.
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                SelectedItems.Clear();
                foreach (object item in items)
                    SelectedItems.Add(item);
            }), System.Windows.Threading.DispatcherPriority.Background);

        }

        #endregion

        #region ISupportSelectionChanged Members

        /// <summary>
        /// Occurs, when selected cell changed.
        /// </summary>
        public event EventHandler SelectionChanged;

        #endregion

        #region Page Overrided Members

        /// <summary>
        /// Page Name.
        /// </summary>
        public override string Name
        {
            get { return PAGE_NAME; }
        }

        /// <summary>
        /// Page Title.
        /// </summary>
        public override string Title
        {
            get { return (string)App.Current.FindResource("BreaksPageCaption"); }
        }

        /// <summary>
        /// Page's icon.
        /// </summary>
        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("BreaksBrush");
                return brush;
            }
        }

        #endregion

        #region PageBase Overrided members

        /// <summary>
        /// Saves grids layout.
        /// </summary>
        internal override void SaveLayout()
        {
            // Save layout settings for each grid.
            if (Properties.Settings.Default.TimeWindowBreaksGridSettings == null)
                Properties.Settings.Default.TimeWindowBreaksGridSettings = new SettingsRepository();
            TimeWindowGrid.SaveUserSettings(Properties.Settings.Default.TimeWindowBreaksGridSettings, 
                UserSettings.All);
          
            if (Properties.Settings.Default.DriveTimeBreaksGridSettings == null)
                Properties.Settings.Default.DriveTimeBreaksGridSettings = new SettingsRepository();
            DriveTimeGrid.SaveUserSettings(Properties.Settings.Default.DriveTimeBreaksGridSettings,
                UserSettings.All);
         
            if (Properties.Settings.Default.WorkTimeBreaksGridSettings == null)
                Properties.Settings.Default.WorkTimeBreaksGridSettings = new SettingsRepository();
            WorkTimeGrid.SaveUserSettings(Properties.Settings.Default.WorkTimeBreaksGridSettings,
                UserSettings.All);
          
            // Save settings.
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Show page help text.
        /// </summary>
        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.BreaksPagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        public override bool CanBeLeft
        {
            get
            {

                // If there are validation error in insertion row - we cannot leave page.
                if (_currentGrid.IsInsertionRowInvalid)
                    return false;
                // If there isnt - we must validate all grid source items.
                else
                    return base.CanBeLeft && CanBeLeftValidator<ESRI.ArcLogistics.Data.DataObject>.
                        IsValid(App.Current.Project.BreaksSettings.DefaultBreaks);
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion

        #region Internal Method

        /// <summary>
        /// Start Breaks Setup Wizard, wizard - where user can choose breaks type.
        /// </summary>
        internal void StartBreaksSetupWizard()
        {
            if (_breaksSetupWizard == null)
                _breaksSetupWizard = new BreaksSetupWizard(_defaultBreaksContoller);
            _breaksSetupWizard.Start();
        } 

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Checks page complete status.
        /// </summary>
        private void _CheckPageComplete()
        {
            Project project = App.Current.Project;
            IsComplete = (project != null && App.Current.Project.BreaksSettings.BreaksType != null);
        }

        /// <summary>
        /// Method inits Grids layout, loads grids structure for all types of Breaks.
        /// </summary>
        private void _InitDataGridLayout()
        {
            string gridStructure = null;
            string gridSettingsRepositoryName = null;
            DataGridCollectionViewSource gridCollectionViewSource = null;

            // Init TimeWindowGrid.
            gridStructure = GridSettingsProvider.TimeWindowBrakesGridStructure;
            gridSettingsRepositoryName = GridSettingsProvider.TimeWindowBreaksSettingsRepositoryName;
            gridCollectionViewSource = 
                (DataGridCollectionViewSource)LayoutRoot.FindResource(TIMEWINDOW_COLLECTION_SOURCE_KEY);
            _InitGridStructure(gridStructure, gridSettingsRepositoryName, TimeWindowGrid,
                gridCollectionViewSource);

            // Init DriveTimeGrid.
            gridStructure = GridSettingsProvider.TimeIntervalBrakesGridStructure;
            gridSettingsRepositoryName = GridSettingsProvider.DriveTimeBreaksSettingsRepositoryName;
            gridCollectionViewSource = 
                (DataGridCollectionViewSource)LayoutRoot.FindResource(DRIVETIME_COLLECTION_SOURCE_KEY);
            _InitGridStructure(gridStructure, gridSettingsRepositoryName, DriveTimeGrid,
                gridCollectionViewSource);

            // Init WorkTimeGrid.
            gridStructure = GridSettingsProvider.TimeIntervalBrakesGridStructure;
            gridSettingsRepositoryName = GridSettingsProvider.WorkTimeBreaksSettingsRepositoryName;
            gridCollectionViewSource = 
                (DataGridCollectionViewSource)LayoutRoot.FindResource(WORKTIME_COLLECTION_SOURCE_KEY);
            _InitGridStructure(gridStructure, gridSettingsRepositoryName, WorkTimeGrid,
                gridCollectionViewSource);
        }

        /// <summary>
        /// Init grid structure for Grid.
        /// </summary>
        /// <param name="gridStructure">Structure of the grid.</param>
        /// <param name="gridSettingsRepositoryName">Repository with grid settings.</param>
        /// <param name="grid">Grid.</param>
        /// <param name="collectionSource">Grid's collection source.</param>
        private void _InitGridStructure(string gridStructure, string gridSettingsRepositoryName,
            DataGridControlEx grid, DataGridCollectionViewSource collectionSource)
        {
            // Initializing gridstructure and gridlayout.
            GridStructureInitializer structureInitializer = new GridStructureInitializer
                (gridStructure);
            structureInitializer.BuildGridStructure(collectionSource, grid);
            GridLayoutLoader layoutLoader = new GridLayoutLoader(gridSettingsRepositoryName,
                collectionSource.ItemProperties);
            layoutLoader.LoadLayout(grid);
        }
        
        /// <summary>
        /// Init grid for the currently selected breaks type.
        /// </summary>
        private void _InitProperGrid()
        {
            // Init grid, corresponding to currently selected breaks type.
            if (App.Current.Project.BreaksSettings.BreaksType == BreakType.TimeWindow)
            {
                // Get repository with grid setting.
                _gridSettingsRepository = Properties.Settings.Default.TimeWindowBreaksGridSettings;

                // If there is no - create it.
                if (_gridSettingsRepository == null)
                {
                    Properties.Settings.Default.TimeWindowBreaksGridSettings = new SettingsRepository();
                    _gridSettingsRepository = Properties.Settings.Default.TimeWindowBreaksGridSettings;
                }

                // Get grid's collection source.
                _currentCollectionSource =
                    (DataGridCollectionViewSource)LayoutRoot.FindResource(TIMEWINDOW_COLLECTION_SOURCE_KEY);

                // Make this grid visible and hide all other grids.
                _InitGridVisibility(TimeWindowGrid);

                _currentGrid = TimeWindowGrid;
            }
            else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.DriveTime)
            {
                // Get repository with grid setting.
                _gridSettingsRepository = Properties.Settings.Default.DriveTimeBreaksGridSettings;

                // If there is no - create it.
                if (_gridSettingsRepository == null)
                {
                    Properties.Settings.Default.DriveTimeBreaksGridSettings = new SettingsRepository();
                    _gridSettingsRepository = Properties.Settings.Default.DriveTimeBreaksGridSettings;
                }

                // Get grid's collection source.
                _currentCollectionSource =
                    (DataGridCollectionViewSource)LayoutRoot.FindResource(DRIVETIME_COLLECTION_SOURCE_KEY);

                // Make this grid visible and hide all other grids.
                _InitGridVisibility(DriveTimeGrid);
                _currentGrid = DriveTimeGrid;
            }
            else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.WorkTime)
            {
                // Get repository with grid setting.
                _gridSettingsRepository = Properties.Settings.Default.WorkTimeBreaksGridSettings;

                // If there is no - create it.
                if (_gridSettingsRepository == null)
                {
                    Properties.Settings.Default.WorkTimeBreaksGridSettings = new SettingsRepository();
                    _gridSettingsRepository = Properties.Settings.Default.WorkTimeBreaksGridSettings;
                }

                // Get grid's collection source.
                _currentCollectionSource =
                    (DataGridCollectionViewSource)LayoutRoot.FindResource(WORKTIME_COLLECTION_SOURCE_KEY);

                // Make this grid visible and hide all other grids.
                _InitGridVisibility(WorkTimeGrid);
                _currentGrid = WorkTimeGrid;
            }
            else
                // This breaks type isn't supported.
                Debug.Assert(false);

            // Subscribe to event.
            _currentGrid.SelectionChanged += new DataGridSelectionChangedEventHandler
                (_XceedGridSelectionChanged);
        }

        /// <summary>
        /// Make this grid visible and hide all other.
        /// </summary>
        /// <param name="grid">DataGridControlEx.</param>
        private void _InitGridVisibility(DataGridControlEx grid)
        {
            DriveTimeGrid.Visibility = System.Windows.Visibility.Collapsed;
            WorkTimeGrid.Visibility = System.Windows.Visibility.Collapsed;
            TimeWindowGrid.Visibility = System.Windows.Visibility.Collapsed;

            grid.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Method inits collection of breaks.
        /// </summary>
        private void _InitDataGridCollection()
        {
            _currentCollectionSource.Source = null;
            App.Current.Project.BreaksSettings.DefaultBreaks.Sort();
            _currentCollectionSource.Source = App.Current.Project.BreaksSettings.DefaultBreaks;
        }

        /// <summary>
        /// Method fills page's properties by default values.
        /// </summary>
        private void _SetDefaults()
        {
            IsRequired = true;
            IsAllowed = true;
            CanBeLeft = true;
            DoesSupportCompleteStatus = true;

            // Init button for all grids.
            commandButtonGroup.Initialize(CategoryNames.BreaksCommands, TimeWindowGrid);
        }

        /// <summary>
        /// Method init event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            App.Current.MainWindow.Closing += new CancelEventHandler(_MainWindowClosing);
            this.Loaded += new RoutedEventHandler(_BreaksPageLoaded);
            this.Unloaded += new RoutedEventHandler(_BreaksPageUnloaded);
            App.Current.ProjectClosing += new EventHandler(_CurrentProjectClosing);
            App.Current.ProjectLoaded += new EventHandler(_AppProjectLoaded);
            _SubscribeToProjectEvents();
        }

        /// <summary>
        /// Subscribes to events.
        /// </summary>
        private void _SubscribeToProjectEvents()
        {
            Project project = App.Current.Project;
            if (null != project)
            {
                App.Current.Project.BreaksSettings.PropertyChanged +=
                    new System.ComponentModel.PropertyChangedEventHandler(_BreaksSettingsPropertyChanged);
                project.BreaksSettings.DefaultBreaks.CollectionChanged +=
                    new NotifyCollectionChangedEventHandler(_BreaksCollectionChanged);

                _projectEventsAttached = true;
            }
        }

        /// <summary>
        /// Add break in default breaks collection and select added break.
        /// </summary>
        /// <param name="breakObject"></param>
        private void _AddAndSelect(Break breakObject)
        {
            // Add break and save project.
            App.Current.Project.BreaksSettings.DefaultBreaks.Add(breakObject);
            App.Current.Project.Save();

            // Break added, need to sort breaks.
            _InitDataGridCollection();

            // Selecting added break.
            List<Break> list = new List<Break>();
            list.Add(breakObject);
            _isEditingInProgress = false;
            Select(list);
        }

        /// <summary>
        /// Increment breaks's duration, then decrement it.
        /// XCeedGrid workaround.
        /// </summary>
        /// <param name="e">DataGridItemEventArgs, which contains break.</param>
        private void _ChangeDuration(DataGridItemEventArgs e)
        {
            (e.Item as Break).Duration++;
            (e.Item as Break).Duration--;
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Occurs when project loads.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _AppProjectLoaded(object sender, EventArgs e)
        {
            // Check that breaks have type.
            _CheckPageComplete();

            // Subscribe to events if we havent done it before.
            if (!_projectEventsAttached)
                _SubscribeToProjectEvents();
        }

        /// <summary>
        /// Breaks type changed - save layout settings.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreaksSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            App.Current.Project.BreaksSettings.DefaultBreaks.CollectionChanged +=
                    new NotifyCollectionChangedEventHandler(_BreaksCollectionChanged);
            SaveLayout();
            _CheckPageComplete();

            // Init grid for selected breaks type.
            _InitProperGrid();
            _InitDataGridCollection();
        }

        /// <summary>
        /// Project closing - save layout settings.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CurrentProjectClosing(object sender, EventArgs e)
        {
            SaveLayout();
            if (_projectEventsAttached)
            {
                App.Current.Project.BreaksSettings.PropertyChanged -=
                    new System.ComponentModel.PropertyChangedEventHandler(_BreaksSettingsPropertyChanged);
                App.Current.Project.BreaksSettings.DefaultBreaks.CollectionChanged -=
                    new NotifyCollectionChangedEventHandler(_BreaksCollectionChanged);
                _projectEventsAttached = false;
            }
        }

        /// <summary>
        /// Occurs when page loads.
        /// </summary>
        /// <param name="sender">Ingored.</param>
        /// <param name="e">Ingored.</param>
        private void _BreaksPageLoaded(object sender, RoutedEventArgs e)
        {
            // Save default breaks collection state.
            _defaultBreaksContoller.OldDefaultBreaks =
                App.Current.Project.BreaksSettings.DefaultBreaks;

            ((MainWindow)App.Current.MainWindow).NavigationCalled += 
                new EventHandler(_BreaksPageNavigationCalled);
            
            _CheckPageComplete();

            // If default breaks type isn't selected then start wizard.
            if (App.Current.Project != null && App.Current.Project.BreaksSettings.BreaksType == null)
            {
                StartBreaksSetupWizard();
            }
            // Else - initialize grid.
            else
            {
                // If grids isn't inited - initialize it.
                if (!_isDataGridsLayoutLoaded)
                {
                    // Init all grids if they are not inited.
                    _InitDataGridLayout();

                    // Set flag to true.
                    _isDataGridsLayoutLoaded = true;
                }

                    // Init propergrid for currently selected breaks type.
                    _InitProperGrid();
                    _InitDataGridCollection();

                _needToUpdateStatus = true;
                _SetSelectionStatus();
            }
        }

        /// <summary>
        /// Occurs when breaks page unloaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreaksPageUnloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from events.
            ((MainWindow)App.Current.MainWindow).NavigationCalled -= _BreaksPageNavigationCalled;
            
            // If we have visible grid.
            if (_currentGrid != null)
            {
                // Unsubscribe from event.
                _currentGrid.SelectionChanged -= new DataGridSelectionChangedEventHandler
                    (_XceedGridSelectionChanged);

                // Set currents grid source collection to null.
                _currentCollectionSource.Source = null;

                // Cancel edit on current grid.
                this.CancelObjectEditing();
            }
        }

        /// <summary>
        /// Application is closing - need check default breaks for updates.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        void _MainWindowClosing(object sender, CancelEventArgs e)
        {
            _defaultBreaksContoller.CheckDefaultBreaksForUpdates();
        }

        /// <summary>
        /// Occurs when default breaks collection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreaksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _SetSelectionStatus();
        }

        /// <summary>
        /// Occurs when navigation called.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreaksPageNavigationCalled(object sender, EventArgs e)
        {
            try
            {
                _currentGrid.CancelEdit();
                CanBeLeft = true;
            }
            catch
            {
                CanBeLeft = false;
            }

            // If we will navigate to other page - check breaks for updates.
            if (CanBeLeft)
                _defaultBreaksContoller.CheckDefaultBreaksForUpdates();
            else CanBeLeftValidator<ESRI.ArcLogistics.Data.DataObject>.
                    ShowErrorMessagesInMessageWindow(App.Current.Project.BreaksSettings.DefaultBreaks);
        }

        /// <summary>
        /// Grid selected cell changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _XceedGridSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            _SetSelectionStatus();

            // NOTE : event raises to notify all necessary object about selection was changed. 
            // Added because XceedGrid.SelectedItems doesn't implement INotifyCollectionChanged.
            if (SelectionChanged != null)
                SelectionChanged(null, EventArgs.Empty);
        }

        #endregion

        #region Data Object Editing Event Handlers

        /// <summary>
        /// New item creating.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceCreatingNewItem(object sender,
            DataGridCreatingNewItemEventArgs e)
        {
            // Add new break of specific type.
            if (App.Current.Project.BreaksSettings.DefaultBreaks.Count < Breaks.MaximumBreakCount)
            {
                if (App.Current.Project.BreaksSettings.BreaksType == BreakType.TimeWindow)
                    e.NewItem = new TimeWindowBreak();
                else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.DriveTime)
                    e.NewItem = new DriveTimeBreak();
                else if (App.Current.Project.BreaksSettings.BreaksType == BreakType.WorkTime)
                    e.NewItem = new WorkTimeBreak();
                else
                    // Unknown type of break.
                    Debug.Assert(false);

                // Set flags.
                _isNewItemCreated = true;
                IsEditingInProgress = true;
                e.Handled = true;
            }
            else
            {
                IsEditingInProgress = false;
                e.Cancel = true;
                _isNewItemCreated = false;
            }
        }

        /// <summary>
        /// New Item created. Starting Editing
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceNewItemCreated(object sender, DataGridItemEventArgs e)
        {
            // Workaround: XCeedGrid doesnt allow to commit new item, that hasnt been changed.
            // Invoking changing of the break's duration. Those method must be done, otherwise 
            // grid wouldnt allow to commit this item.
            Dispatcher.BeginInvoke(new Action<DataGridItemEventArgs>(_ChangeDuration),
                DispatcherPriority.DataBind, e);

            if (EditBegun != null)
                EditBegun(this, new EventArgs());
        }

        /// <summary>
        /// New item commiting. Adding to collection.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceCommittingNewItem(object sender,
            Xceed.Wpf.DataGrid.DataGridCommittingNewItemEventArgs e)
        {
            e.Handled = true;

            // Detecting breake to add.
            ICollection<Break> source = e.CollectionView.SourceCollection as ICollection<Break>;
            Break currentBreak = e.Item as Break;
            _AddAndSelect(currentBreak);

            // Set new index.
            e.Index = App.Current.Project.BreaksSettings.DefaultBreaks.IndexOf(currentBreak);
            e.NewCount = source.Count;

            // Saving project.
            App.Current.Project.Save();

            _SetSelectionStatus();
            IsEditingInProgress = false;
        }

        /// <summary>
        /// New item committed. Raise EditFinished event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceNewItemCommited(object sender, DataGridItemEventArgs e)
        {
            if (EditFinished != null)
                EditFinished(this, null);
        }

        /// <summary>
        /// Canceling New Item.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceCancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            // set property to true if new item was created or to false if new item wasn't created
            // otherwise an InvalidOperationException will be thrown 
            // (see http://doc.xceedsoft.com/products/XceedWpfDataGrid/Inserting_Data.html)
            e.Handled = _isNewItemCreated;
            IsEditingInProgress = false;
            _SetSelectionStatus();
        }

        /// <summary>
        /// New item canceled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceNewItemCanceled(object sender, DataGridItemEventArgs e)
        {
            if (EditFinished != null)
                EditFinished(this, null);
        }

        /// <summary>
        /// Beggining edit.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceBeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            e.Handled = true;

            // If not canceled - raise EditBegun event and update editing status.
            IsEditingInProgress = true;
            _SetEditingStatus(e.Item.ToString());
        }

        /// <summary>
        /// Edit begun.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceEditBegun(object sender, DataGridItemEventArgs e)
        {
            if (EditBegun != null)
                EditBegun(this, null);
        }

        /// <summary>
        /// Edit commited.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceCommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            e.Handled = true;

            // Break changed, need to sort breaks.
            _InitDataGridCollection();

            if (!e.Cancel)
            {
                // If not canceled - saving project.
                if (App.Current.Project != null)
                    App.Current.Project.Save();

                // Update status label.
                IsEditingInProgress = false;
                _SetSelectionStatus();
            }
            else
                e.Cancel = true;
        }

        /// <summary>
        /// Breaks collection updated.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceEditCommitted(object sender, DataGridItemEventArgs e)
        {
            if (!IsEditingInProgress)
            {
                // If editing finished then raise EditeFinished event.
                if (EditFinished != null)
                    EditFinished(this, null);

                // Change selecthion in DataGrid.
                var selection = new ArrayList(_currentGrid.SelectedItems);
                Select(selection);
            }
        }

        /// <summary>
        /// Edit canceled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridCollectionViewSourceCancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
            IsEditingInProgress = false;
            if (EditFinished != null)
                EditFinished(this, null);
            _SetSelectionStatus();
        }

        #endregion

        #region Private Status Helpers

        /// <summary>
        /// Method sets selection status
        /// </summary>
        private void _SetSelectionStatus()
        {
            if (_needToUpdateStatus)
                _statusBuilder.FillSelectionStatus(App.Current.Project.BreaksSettings.DefaultBreaks.Count,
                    (string)App.Current.FindResource(OBJECT_TYPE_NAME), _currentGrid.SelectedItems.Count, this);

            _needToUpdateStatus = true;
        }

        /// <summary>
        /// Method sets editing status
        /// </summary>
        /// <param name="itemName"></param>
        private void _SetEditingStatus(string itemName)
        {
            _statusBuilder.FillEditingStatus(itemName, (string)App.Current.FindResource(OBJECT_TYPE_NAME), this);
            _needToUpdateStatus = false;
        }

        /// <summary>
        /// Method sets creating status
        /// </summary>
        private void _SetCreatingStatus()
        {
            _statusBuilder.FillCreatingStatus((string)App.Current.FindResource(OBJECT_TYPE_NAME), this);
            _needToUpdateStatus = false;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Names constants.
        /// </summary>
        private const string PAGE_NAME = "Breaks";
        protected const string NAME_PROPERTY_STRING = "Name";
        protected const string OBJECT_TYPE_NAME = "Break";

        /// <summary>
        /// Grid's collections names.
        /// </summary>
        private const string TIMEWINDOW_COLLECTION_SOURCE_KEY = "TimeWindowBreaksCollection";
        private const string DRIVETIME_COLLECTION_SOURCE_KEY = "DriveTimeBreaksCollection";
        private const string WORKTIME_COLLECTION_SOURCE_KEY = "WorkTimeBreaksCollection";

        /// <summary>
        /// Provide text for status label.
        /// </summary>
        private StatusBuilder _statusBuilder = new StatusBuilder();

        /// <summary>
        /// Flag shows is new item was created or not to set correct value of 
        /// Handled property in DataGridCollectionViewSource_CancelingNewItem.
        /// </summary>
        private bool _isNewItemCreated = false;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus = false;

        /// <summary>
        /// Flag, which show is some item in grid is editing or not.
        /// </summary>
        private bool _isEditingInProgress;

        /// <summary>
        /// Flags, shown was grid collection or layout initiated or not.
        /// </summary>
        private bool _isDataGridsLayoutLoaded = false;

        /// <summary>
        /// Project events attached flag.
        /// </summary>
        private bool _projectEventsAttached = false;

        /// <summary>
        /// Current repository for grid layout settings.
        /// </summary>
        private SettingsRepository _gridSettingsRepository;

        /// <summary>
        /// Visible grid for current breaks type.
        /// </summary>
        private DataGridControlEx _currentGrid;

        /// <summary>
        /// Current grid's collection view source.
        /// </summary>
        DataGridCollectionViewSource _currentCollectionSource;

        /// <summary>
        /// When default breaks changing - updates default routes breaks if necessary.
        /// </summary>
        private DefaultBreaksController _defaultBreaksContoller = 
            new DefaultBreaksController();

        /// <summary>
        /// Wizard, where user can choose breaks type.
        /// </summary>
        private BreaksSetupWizard _breaksSetupWizard;

        #endregion
    }
}
