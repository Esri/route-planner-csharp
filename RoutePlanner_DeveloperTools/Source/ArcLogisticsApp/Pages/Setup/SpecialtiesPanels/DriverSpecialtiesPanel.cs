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
using System.Linq;
using System.Text;
using Xceed.Wpf.DataGrid;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.App.Commands;
using Xceed.Wpf.DataGrid.Settings;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Pages
{
    class DriverSpecialtiesPanel : SpecialtiesPanelBase
    {
        #region Constructor

        public DriverSpecialtiesPanel()
        {
            // fill strings properties by vehicle specialties strings
            _SetDefaults();
            this.Loaded += new System.Windows.RoutedEventHandler(DriverSpecialtiesPanel_Loaded);
            commandButtonsGroup.Initialize(CategoryNames.DriverSpecialtiesCommands, XceedGrid);
        }

        #endregion

        #region Protected Porperties

        protected override string SettingsRepositoryName
        {
            get;
            set;
        }

        protected override string CommandCategoryName
        {
            get;
            set;
        }

        protected override string GridStructureName
        {
            get;
            set;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Inits collecton of grid items
        /// </summary>
        protected override void _InitDataGridCollection()
        {
            DataGridCollectionViewSource driverCollectionSource = (DataGridCollectionViewSource)base.LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);
            IDataObjectCollection<DriverSpecialty> driversSpecialtiesCollection = (IDataObjectCollection<DriverSpecialty>)App.Current.Project.DriverSpecialties;
            SortedDataObjectCollection<DriverSpecialty> sortedDriverSpecialtiesCollection = new SortedDataObjectCollection<DriverSpecialty>(driversSpecialtiesCollection, new CreationTimeComparer<DriverSpecialty>());
            driverCollectionSource.Source = sortedDriverSpecialtiesCollection;
        }

        protected override void _ClearDataGridCollection()
        {
            DataGridCollectionViewSource driverCollectionSource = (DataGridCollectionViewSource)base.LayoutRoot.FindResource(COLLECTION_SOURCE_KEY);
            driverCollectionSource.Source = null;
        }

        /// <summary>
        /// Creates new item
        /// </summary>
        /// <param name="e"></param>
        protected override void _CreateNewItem(DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new DriverSpecialty();
        }

        /// <summary>
        /// Change item's name.
        /// </summary>
        /// <param name="e">DataGridItemEventArgs.</param>
        /// <param name="insertionRow">InsertionRow in which new item is placed.</param>
        protected override void _ChangeName(Xceed.Wpf.DataGrid.DataGridItemEventArgs e,
            InsertionRow insertionRow)
        {
            // Check that item's name is null.
            if (!string.IsNullOrEmpty((e.Item as DriverSpecialty).Name))
                return;

            // Get new item's name.
            (e.Item as DriverSpecialty).Name = DataObjectNamesConstructor.GetNameForNewDataObject(
                App.Current.Project.DriverSpecialties, e.Item as DriverSpecialty, true);

            // Find TextBox inside the cell and select new name.
            Cell currentCell = insertionRow.Cells[XceedGrid.CurrentContext.CurrentColumn];
            TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);
            if (textBox != null)
                textBox.SelectAll();
        }

        /// <summary>
        /// Saves new item in collection
        /// </summary>
        /// <param name="e"></param>
        protected override void _CommitNewItem(DataGridCommittingNewItemEventArgs e)
        {
            ICollection<DriverSpecialty> source = e.CollectionView.SourceCollection as ICollection<DriverSpecialty>;

            DriverSpecialty currentSpecialty = e.Item as DriverSpecialty;
            source.Add(currentSpecialty);

            e.Index = source.Count - 1;
            e.NewCount = source.Count;

            App.Current.Project.Save();
        }

        /// <summary>
        /// Saves Layout
        /// </summary>
        protected override void SaveLayout()
        {
            if (Properties.Settings.Default.DriverSpecialtiesGridSettings == null)
            {
                Properties.Settings.Default.DriverSpecialtiesGridSettings = new SettingsRepository();
            }

            this.XceedGrid.SaveUserSettings(Properties.Settings.Default.DriverSpecialtiesGridSettings, UserSettings.All);
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Selects items
        /// </summary>
        /// <param name="items"></param>
        protected override void _Select(System.Collections.IEnumerable items)
        {
            // check that editing is not in progress
            if (IsEditingInProgress)
                throw new NotSupportedException((string)App.Current.FindResource("EditingInProcessExceptionMessage"));

            // check that all items are locations
            foreach (object item in items)
            {
                if (!(item is DriverSpecialty))
                    throw new ArgumentException("DriverSpecialtiesTypeExceptionMessage");
            }

            // add items to selection
            SelectedItems.Clear();
            foreach (object item in items)
                SelectedItems.Add(item);
        }

        /// <summary>
        /// Method fills page's properties by default values
        /// </summary>
        protected void _SetDefaults()
        {
            PanelHeader = (string)App.Current.FindResource("DriverSpecialtiesHeader");
            CommandCategoryName = CategoryNames.DriverSpecialtiesCommands;
            SettingsRepositoryName = SETTINGS_REPOSITORY_NAME;
            GridStructureName = GRID_STRUCTURE_NAME;
        }

        #endregion

        #region Overrided Status Helpers

        protected override void _SetCreatingStatus()
        {
            _statusBuilder.FillCreatingStatus((string)App.Current.FindResource(OBJECT_TYPE_NAME), _specialtiesPage);
            _needToUpdateStatus = false;
        }

        protected override void _SetEditingStatus(string name)
        {
            _statusBuilder.FillEditingStatus(name, (string)App.Current.FindResource(OBJECT_TYPE_NAME), _specialtiesPage);
            _needToUpdateStatus = false;
        }

        protected override void _SetSelectionStatus()
        {
            if (_needToUpdateStatus)
                _statusBuilder.FillSelectionStatus(App.Current.Project.DriverSpecialties.Count, (string)App.Current.FindResource(OBJECT_TYPE_NAME), XceedGrid.SelectedItems.Count, _specialtiesPage);

            _needToUpdateStatus = true;
        }

        #endregion

        #region Private Event Handlers

        private void DriverSpecialtiesPanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _specialtiesPage = (SpecialtiesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SpecialtiesPagePath);

            if (XceedGrid.SelectedItems.Count > 0)
            {
                _needToUpdateStatus = true;
                _SetSelectionStatus();
            }
        }

        #endregion

        #region Private Members

        private const string SETTINGS_REPOSITORY_NAME = "DriverSpecialtiesGridSettings";
        private const string GRID_STRUCTURE_NAME = "ESRI.ArcLogistics.App.GridHelpers.DriverSpecialtiesGridStructure.xaml";
        protected const string OBJECT_TYPE_NAME = "DriverSpecialty";

        private StatusBuilder _statusBuilder = new StatusBuilder();
        private SpecialtiesPage _specialtiesPage;

        /// <summary>
        /// Flag shows whether status should be changed.
        /// </summary>
        private bool _needToUpdateStatus = false;

        #endregion
    }
}
