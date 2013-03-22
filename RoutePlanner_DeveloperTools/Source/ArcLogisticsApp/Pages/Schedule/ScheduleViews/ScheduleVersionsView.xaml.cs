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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for ScheduleVersionsView.xaml
    /// </summary>
    internal partial class ScheduleVersionsView : DockableContent
    {
        #region Constructors

        /// <summary>
        /// Constructor. Creates schedule versions view.
        /// </summary>
        public ScheduleVersionsView()
        {
            InitializeComponent();

            _InitEvents();

            // Init data grid layout if project is not null.
            if (App.Current.Project != null)
                _InitDataGridLayout();

            new ViewButtonsMarginUpdater(this, ButtonsWrapPanel);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets schedule page.
        /// </summary>
        public OptimizeAndEditPage ParentPage
        {
            get
            {
                return _schedulePage;
            }
            set
            {
                Debug.Assert(_schedulePage == null);
                _schedulePage = value;

                if (_DataGridSource.Source == null)
                {
                    _InitDataGridCollection();

                    // We can select current item right now. SelectionChanged event won't raised by Data Grid.
                    XceedGrid.SelectedItem = _schedulePage.CurrentSchedule;
                }

                _schedulePage.CurrentScheduleChanged += new EventHandler(_CurrentScheduleChanged);
                _schedulePage.LockedPropertyChanged += new EventHandler(_OptimizeAndEditPageLockedPropertyChanged);
            }
        }

        /// <summary>
        /// Gets current selected schedule, sets it as current application schedule for this date.
        /// </summary>
        public Schedule _SelectedSchedule
        {
            get
            {
                Debug.Assert(XceedGrid.SelectedItem == null || XceedGrid.SelectedItem is Schedule);
                return (Schedule)XceedGrid.SelectedItem;
            }
        }

        #endregion

        #region Private Event handlers

        /// <summary>
        /// Handles project losded event. Updates layout if necessary.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OnProjectLoaded(object sender, EventArgs e)
        {
            // Reset data grid source.
            _DataGridSource.Source = null;

            // Init layout if this not done before.
            if (!_isLayoutInitialized)
                _InitDataGridLayout();
        }

        /// <summary>
        /// Change type of current schedule to current.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CommitToCurrentButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                WorkingStatusHelper.SetBusy((string)App.Current.FindResource("CommitingScheduleVersionStatus"));

                // Remove all schedules except one with Original type and curently selected one.
                Collection<Schedule> removingSchedules = new Collection<Schedule>();
                foreach (Schedule schedule in _ScheduleVersions)
                {
                    if (schedule.Type == ScheduleType.Current &&
                        !schedule.Equals(_SelectedSchedule))
                    {
                        removingSchedules.Add(schedule);
                    }
                }

                // Unsubscribe from selection since data grid raises this event when data source changes.
                _UnsubscribeFromGridSelection();

                // Workaround: data grid changes selection for some reason, so we need to save current selection
                Schedule selectedItem = _SelectedSchedule;

                foreach (Schedule schedule in removingSchedules)
                {
                    if (schedule.UnassignedOrders != null)
                        schedule.UnassignedOrders.Dispose();
                    App.Current.Project.Schedules.Remove(schedule);
                }

                // Select remembered item.
                XceedGrid.SelectedItem = selectedItem;

                // Subscribe to SelectionChanged event again.
                _SubscribeToGridSelection();

                Schedule newCurrentSchedule = _MakeScheduleVersion(_SelectedSchedule, _ScheduleVersions);
                newCurrentSchedule.Name = (string)App.Current.FindResource("CurrentScheduleName");
                newCurrentSchedule.Type = ScheduleType.Current;

                App.Current.Project.Schedules.Add(newCurrentSchedule);
                App.Current.Project.Save(); // need to save before seleting
                XceedGrid.SelectedItem = newCurrentSchedule;

                // Save all changes.
                App.Current.Project.Save();

                _UpdateButtonsState();
            }
            finally
            {
                WorkingStatusHelper.SetReleased();
            }
        }

        /// <summary>
        /// React on new version button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _NewVersionButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                XceedGrid.EndEdit();
                WorkingStatusHelper.SetBusy((string)App.Current.FindResource("CreatingScheduleVersionStatus"));

                _UnsubscribeFromGridSelection();

                // Make version, add to project and save.
                Debug.Assert(_SelectedSchedule != null);
                Schedule newSchedule = _MakeScheduleVersion(_SelectedSchedule, _ScheduleVersions);
                App.Current.Project.Schedules.Add(newSchedule);
                App.Current.Project.Save();

                _SubscribeToGridSelection();

                // Select new schedule.
                XceedGrid.SelectedItem = newSchedule;
            }
            finally
            {
                WorkingStatusHelper.SetReleased();
            }
        }

        /// <summary>
        /// React on edits cancelled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            _DeleteSelectedSchedule();
        }

        /// <summary>
        /// React on page locked changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OptimizeAndEditPageLockedPropertyChanged(object sender, EventArgs e)
        {
            if (!_schedulePage.IsLocked)
                lockedGrid.Visibility = Visibility.Hidden;
            else
                lockedGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// React on current schedule changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CurrentScheduleChanged(object sender, EventArgs e)
        {
            // Remove handler to not handle Selection changed event during collection changing.
            _UnsubscribeFromGridSelection();

            // If data grid wasn't initialized yet.
            if (_DataGridSource.Source == null)
            {
                _InitDataGridCollection();

                // We can select current item right now. SelectionChanged event won't raised by Data Grid.
                XceedGrid.SelectedItem = _schedulePage.CurrentSchedule;
            }
            else // Refresh data grid.
            {
                // Grid data source changes, its previous value isn't null. 
                _InitDataGridCollection();

                // Set current selection to null, so data grid will rais only on SelectionChanged event.
                XceedGrid.SelectedItem = null;

                // Set the indicator to true. SelectionChanged event will be raised so it can be processed in a specific manner.
                _dataGridCollectionChanged = true;
            }

            _UpdateButtonsState();

            // Add handler again.
            _SubscribeToGridSelection();
        }

        /// <summary>
        /// React on selection changed in data grid.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            // Workaround: in case data grid source has changed we just need to get current schedule and select it in the grid.
            // This event is raised by data grid control when source changes but should not.
            if (_dataGridCollectionChanged)
            {
                _UnsubscribeFromGridSelection();
                XceedGrid.SelectedItem = _schedulePage.CurrentSchedule;
                _SubscribeToGridSelection();
                _dataGridCollectionChanged = false; // reset the indicator
            }
            // WORKAROUND: check for _SelectedSchedule != null because after selecting schedule in grid, selection changed event comes twice
            // in some cases. First - with _SelectedSchedule = null.
            else if (_schedulePage.CurrentSchedule != _SelectedSchedule && _SelectedSchedule != null)
            {
                // User changed selection if the Versions view - so we need to change current schedule in Optimize and Edit page.
                _schedulePage.CurrentScheduleChanged -= _CurrentScheduleChanged;
                _schedulePage.CurrentSchedule = _SelectedSchedule;
                _schedulePage.CurrentScheduleChanged += new EventHandler(_CurrentScheduleChanged);
            }

            _UpdateButtonsState();
        }

        /// <summary>
        /// React on data grid control initialized.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridControlInitialized(object sender, EventArgs e)
        {
            _SubscribeToGridSelection();
        }

        /// <summary>
        /// React on editing started in grid.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Edited param args.</param>
        private void _DataGridBeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            // We shouldn't allow renaming for current schedule and build routes snapshot.
            e.Cancel = _WasRenamingProhibited(e.Item as Schedule);
            e.Handled = true;
        }

        /// <summary>
        /// React on editing committed in grid.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Commiting edit param args.</param>
        private void _DataGridCommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// React on editing cancelled in grid.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Cancelling edit param args.</param>
        private void _DataGridCancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// React on routes sent changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _FleetManagerRoutesSentChanged(object sender, EventArgs e)
        {
            _UpdateButtonsState();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets collection source of data grid.
        /// </summary>
        private DataGridCollectionViewSource _DataGridSource
        {
            get
            {
                return (DataGridCollectionViewSource)LayoutRoot.FindResource("scheduleVersionsCollection");
            }
        }

        /// <summary>
        /// Returns schedule versions collection from data grid.
        /// </summary>
        private IDataObjectCollection<Schedule> _ScheduleVersions
        {
            get
            {
                DataGridCollectionViewSource source = _DataGridSource;
                SortedDataObjectCollection<Schedule> sortedSchedules = source.Source as SortedDataObjectCollection<Schedule>;

                if (sortedSchedules == null)
                    return null;
                else
                    return (IDataObjectCollection<Schedule>)sortedSchedules.InternalCollection;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEvents()
        {
            App.Current.ProjectLoaded += new EventHandler(_OnProjectLoaded);
        }

        /// <summary>
        /// Method sets grid data binding.
        /// </summary>
        private void _InitDataGridCollection()
        {
            IDataObjectCollection<Schedule> schedules = _ScheduleVersions;

            if (schedules != null)
                schedules.Dispose();

            schedules = (IDataObjectCollection<Schedule>)App.Current.Project.Schedules.Search(App.Current.CurrentDate, true);
            SortedDataObjectCollection<Schedule> sortedScheduleCollection = new SortedDataObjectCollection<Schedule>(schedules, new ScheduleVersionComparer());
            _DataGridSource.Source = sortedScheduleCollection;
        }

        /// <summary>
        /// Method initializes grid layout.
        /// </summary>
        private void _InitDataGridLayout()
        {
            GridStructureInitializer structureInitializer = new GridStructureInitializer("ESRI.ArcLogistics.App.GridHelpers.ScheduleVersionsGridStructure.xaml");
            structureInitializer.BuildGridStructure(_DataGridSource, XceedGrid);
            _isLayoutInitialized = true;
        }

        /// <summary>
        /// Sets enabled/disabled property for buttons.
        /// </summary>
        private void _UpdateButtonsState()
        {
            // "New Version" enabled in case of schedule selected.
            NewVersionButton.IsEnabled = _SelectedSchedule != null;

            // Disable/enable "Commit to Current" : selected schedule must be not current one and routes not sent.
            CommitToCurrentButton.IsEnabled = _SelectedSchedule != null && _SelectedSchedule.Type != ScheduleType.Current;

            DeleteButton.IsEnabled =
                _SelectedSchedule != null &&
                _SelectedSchedule.Type == ScheduleType.Edited;
        }

        /// <summary>
        /// Prohibit renaming of current schedule.
        /// </summary>
        /// <returns>True if renaming was prohibited and false otherwise.</returns>
        /// <param name="currentSchedule">Current schedule.</param>
        private bool _WasRenamingProhibited(Schedule currentSchedule)
        {
            return ((currentSchedule.Type == ScheduleType.BuildRoutesSnapshot) ||
                    (currentSchedule.Type == ScheduleType.Current));
        }

        /// <summary>
        /// Subscribe to grid selection changes.
        /// </summary>
        private void _SubscribeToGridSelection()
        {
            XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(_DataGridSelectionChanged);
        }

        /// <summary>
        /// Unsubscribe from grid selection changes.
        /// </summary>
        private void _UnsubscribeFromGridSelection()
        {
            XceedGrid.SelectionChanged -= _DataGridSelectionChanged;
        }

        /// <summary>
        /// Method makes new edited version of a schedule.
        /// </summary>
        /// <param name="baseSchedule">Source schedule version.</param>
        /// <param name="otherVersions">Existed schedule versions.</param>
        /// <returns>Copied schedule version.</returns>
        private Schedule _MakeScheduleVersion(Schedule baseSchedule, IDataObjectCollection<Schedule> otherVersions)
        {
            Debug.Assert(baseSchedule != null);

            Schedule newVersion = (Schedule)baseSchedule.Clone();

            int latestVersionIndex = _GetLatestVersionIndex(otherVersions);

            newVersion.Name = (latestVersionIndex == 0) ?
                (string)App.Current.FindResource("ClonedScheduleNameFormatWithoutNumber") :
                string.Format((string)App.Current.FindResource("ClonedScheduleNameFormat"), latestVersionIndex);

            newVersion.Type = ScheduleType.Edited;

            return newVersion;
        }

        /// <summary>
        /// Method gets index of the latest schedule version.
        /// </summary>
        /// <param name="otherVersions">Existed schedule versions.</param>
        /// <returns>Index of the latest schedule version.</returns>
        private int _GetLatestVersionIndex(IDataObjectCollection<Schedule> otherVersions)
        {
            string versionName = (string)App.Current.FindResource("ClonedScheduleNameFormatWithoutNumber");

            // Find the latest schedule version index.
            int? result = null;
            foreach (Schedule schedule in otherVersions)
            {
                if (!schedule.Name.StartsWith(versionName))
                    continue;

                if (!result.HasValue)
                    result = 0; // so at least name of the schedule is "Edited", this means 0 index

                string indexString = schedule.Name.Remove(0, versionName.Length);
                indexString.Trim();

                int newIndex;
                if (int.TryParse(indexString, out newIndex))
                {
                    if (!result.HasValue || newIndex > result)
                        result = newIndex;
                }
            }

            if (result.HasValue)
                return result.Value + 1;

            return 0;
        }

        /// <summary>
        /// Deletes currently selected schedule.
        /// </summary>
        private void _DeleteSelectedSchedule()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var currentSchedule = _ScheduleVersions
                    .Where(schedule => schedule.Type == ScheduleType.Current)
                    .FirstOrDefault();
                Debug.Assert(currentSchedule != null);

                var scheduleToDelete = _SelectedSchedule;
                Debug.Assert(scheduleToDelete.Type != ScheduleType.Current);

                // Select current schedule.
                XceedGrid.SelectedItem = currentSchedule;

                if (scheduleToDelete.UnassignedOrders != null)
                {
                    scheduleToDelete.UnassignedOrders.Dispose();
                }

                App.Current.Project.Schedules.Remove(scheduleToDelete);

                // Save all changes.
                App.Current.Project.Save();

                _UpdateButtonsState();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Handles key down event.
        /// </summary>
        /// <param name="sender">The reference to the event sender object.</param>
        /// <param name="e">The arguments for the event.</param>
        private void _HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Delete || Keyboard.Modifiers != ModifierKeys.None)
            {
                return;
            }

            e.Handled = true;
            if (this.DeleteButton.IsEnabled)
            {
                _DeleteSelectedSchedule();
            }
        }
        #endregion

        #region Private constants

        /// <summary>
        /// Index if Name field. Neccessary for disabling if editing.
        /// </summary>
        private const int NAME_FIELD_INDEX = 0;

        #endregion

        #region Private Fields

        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _schedulePage;

        /// <summary>
        /// Indicates that data grid collection just changed.
        /// </summary>
        private bool _dataGridCollectionChanged;

        /// <summary>
        /// Is page layout initialized.
        /// </summary>
        private bool _isLayoutInitialized;

        #endregion
    }

    /// <summary>
    /// Class compares versions by creation time.
    /// </summary>
    internal class ScheduleVersionComparer : IComparer<Schedule>
    {
        #region IComparer<T> Members

        public int Compare(Schedule x, Schedule y)
        {
            // in case type is the same - sort by creation time
            if (x.Type == y.Type)
            {
                if (x.CreationTime == null || y.CreationTime == null)
                    return 0;
                else if (x.CreationTime < y.CreationTime)
                    return 1;
                else if (x.CreationTime > y.CreationTime)
                    return -1;
                else
                    return 0;
            }

            // Original schedule should be in bottom.
            else if (x.Type == ScheduleType.BuildRoutesSnapshot || y.Type == ScheduleType.BuildRoutesSnapshot)
                return x.Type == ScheduleType.BuildRoutesSnapshot ? 1 : -1;

            // Current schedule should be upper than build routes snapshot but lower then edited versions.
            else
                return x.Type == ScheduleType.Current ? 1 : -1;
        }

        #endregion
    }
}
