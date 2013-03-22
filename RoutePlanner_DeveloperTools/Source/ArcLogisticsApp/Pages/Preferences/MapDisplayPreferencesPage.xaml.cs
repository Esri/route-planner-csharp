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
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Properties;
using ESRI.ArcLogistics.App.Validators;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.OrderSymbology;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for MapDisplayPreferencesPage.xaml
    /// </summary>
    internal partial class MapDisplayPreferencesPage : PageBase
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public const string PAGE_NAME = "MapDisplay";

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MapDisplayPreferencesPage()
        {
            InitializeComponent();

            IsRequired = true;
            IsAllowed = true;
            DoesSupportCompleteStatus = false;

            _InitEventHandlers();

            MapDisplay mapDisplay = App.Current.MapDisplay;
            LabelingOnButton.IsChecked = mapDisplay.LabelingEnabled;

            TrueRouteButton.IsChecked = mapDisplay.TrueRoute;
            StraightRouteButton.IsChecked = !TrueRouteButton.IsChecked;

            BarriersOnButton.IsChecked = mapDisplay.ShowBarriers;

            ZonesOnButton.IsChecked = mapDisplay.ShowZones;

            ShowLeadingStemTimeButton.IsChecked = mapDisplay.ShowLeadingStemTime;
            ShowTrailingStemTimeButton.IsChecked = mapDisplay.ShowTrailingStemTime;

            AutoZoomButton.IsChecked = mapDisplay.AutoZoom;

            if (App.Current.Project != null)
                _InitSymbology();
            else
            {
                SymbologyLabel.Visibility = Visibility.Collapsed;
                SymbologyGrid.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

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
            get { return App.Current.FindString("MapDisplayPreferencesPageCaption"); }
        }

        public override TileBrush Icon
        {
            get { return (ImageBrush)App.Current.FindResource("MapDisplayPreferencesBrush"); }
        }

        #endregion

        #region Public PageBase overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.MapDisplayPreferencesPagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion // PageBase overrided members

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates all common event handlers (loaded/unloaded, collection changed etc.).
        /// </summary>
        private void _InitEventHandlers()
        {
            App.Current.Exit += new ExitEventHandler(Current_Exit);
            this.Loaded += new RoutedEventHandler(Page_Loaded);
            this.Unloaded += new RoutedEventHandler(Page_Unloaded);

            App.Current.ProjectLoaded += new EventHandler(App_ProjectLoaded);
            App.Current.ProjectClosed += new EventHandler(App_ProjectClosed);

            App.Current.MapDisplay.ShowBarriersChanged += new EventHandler(MapDisplay_ShowBarriersChanged);
            App.Current.MapDisplay.ShowZonesChanged += new EventHandler(MapDisplay_ShowZonesChanged);

            App.Current.MapDisplay.TrueRouteChanged += new EventHandler(MapDisplay_TrueRouteChanged);
        }

        private void _InitSymbology()
        {
            SymbologyManager.Init();
            _InitFields();
            _InitCategoriesDataGridLayout();
            _InitQuantitiesDataGridLayout();

            if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
            {
                CategoriesButton.IsChecked = true;
                QuantitiesButton.IsChecked = false;
                CategoryXceedGrid.Visibility = Visibility.Visible;
                QuantityXceedGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                CategoriesButton.IsChecked = false;
                QuantitiesButton.IsChecked = true;
                CategoryXceedGrid.Visibility = Visibility.Collapsed;
                QuantityXceedGrid.Visibility = Visibility.Visible;
            }
            _SetFieldBoxIndex();
            _OnFieldChanged();

            SymbologyLabel.Visibility = Visibility.Visible;
            SymbologyGrid.Visibility = Visibility.Visible;

            _SetShadowGrid();
            _symbologyInited = true;
        }

        /// <summary>
        /// React on show zones option changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void MapDisplay_ShowZonesChanged(object sender, EventArgs e)
        {
            ZonesOnButton.IsChecked = App.Current.MapDisplay.ShowZones;
        }

        /// <summary>
        /// React on show barriers option changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void MapDisplay_ShowBarriersChanged(object sender, EventArgs e)
        {
            BarriersOnButton.IsChecked = App.Current.MapDisplay.ShowBarriers;
        }

        /// <summary>
        /// React on true route option changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        void MapDisplay_TrueRouteChanged(object sender, EventArgs e)
        {
            TrueRouteButton.IsChecked = App.Current.MapDisplay.TrueRoute;
            StraightRouteButton.IsChecked = !TrueRouteButton.IsChecked;
        }

        /// <summary>
        /// React on project loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void App_ProjectLoaded(object sender, EventArgs e)
        {
            _InitSymbology();
        }

        /// <summary>
        /// React on project closed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void App_ProjectClosed(object sender, EventArgs e)
        {
            SymbologyGrid.Visibility = Visibility.Collapsed;
            SymbologyLabel.Visibility = Visibility.Collapsed;
            _symbologyInited = false;

            _StoreState();
        }

        /// <summary>
        /// React on page loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.StatusBar.SetStatus(this, null);
        }

        /// <summary>
        /// React on page unloaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _StoreState();
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            _StoreState();
        }

        private void _StoreState()
        {
            if (_edited != null)
            {
                if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
                {
                    try
                    {
                        CategoryXceedGrid.EndEdit();
                    }
                    catch
                    {
                        CategoryXceedGrid.CancelEdit();
                    }
                }
                else
                {
                    try
                    {
                        QuantityXceedGrid.EndEdit();
                    }
                    catch
                    {
                        QuantityXceedGrid.CancelEdit();
                    }
                }
            }

            if (_edited != null)
                _EndEdit();
            if (App.Current.Project != null)
                SymbologyManager.SaveConfig();

            MapDisplay mapDisplay = App.Current.MapDisplay;
            mapDisplay.TrueRoute = TrueRouteButton.IsChecked.Value;
            mapDisplay.LabelingEnabled = LabelingOnButton.IsChecked.Value;

            mapDisplay.ShowBarriers = BarriersOnButton.IsChecked.Value;
            mapDisplay.ShowZones = ZonesOnButton.IsChecked.Value;

            mapDisplay.ShowLeadingStemTime = ShowLeadingStemTimeButton.IsChecked.Value;
            mapDisplay.ShowTrailingStemTime = ShowTrailingStemTimeButton.IsChecked.Value;
            mapDisplay.AutoZoom = AutoZoomButton.IsChecked.Value;
            mapDisplay.Save();
        }

        private void _InitFields()
        {
            _categoryFieldNames.Clear();
            _categoryFieldNames.Add(NONE_FIELD_NAME);
            foreach (string title in SymbologyManager.CategoryFieldTitles)
                _categoryFieldNames.Add(title);

            _quantityFieldNames.Clear();
            _quantityFieldNames.Add(NONE_FIELD_NAME);
            foreach (string title in SymbologyManager.QuantityFieldTitles)
                _quantityFieldNames.Add(title);
        }

        private void _SetFieldBoxIndex()
        {
            int index;
            _userChange = false;

            if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
            {
                FieldBox.ItemsSource = _categoryFieldNames;
                index = _categoryFieldNames.IndexOf(SymbologyManager.CategoryOrderField);
            }
            else
            {
                FieldBox.ItemsSource = _quantityFieldNames;
                index = _quantityFieldNames.IndexOf(SymbologyManager.QuantityOrderField);
            }

            if (index == -1)
                index = 0;

            FieldBox.SelectedIndex = index;
            _userChange = true;
        }

        /// <summary>
        /// Method loads grid layout
        /// </summary>
        private void _InitCategoriesDataGridLayout()
        {
            DataGridCollectionViewSource collectionSource =
                (DataGridCollectionViewSource)LayoutRoot.FindResource("categoryCollection");

            collectionSource.Source = SymbologyManager.OrderCategories;

            GridStructureInitializer structureInitializer =
                new GridStructureInitializer("ESRI.ArcLogistics.App.GridHelpers.CategorySymbologyGridStructure.xaml");

            structureInitializer.BuildGridStructure(collectionSource, CategoryXceedGrid);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader("CategoriesGridSettings", collectionSource.ItemProperties);
            layoutLoader.LoadLayout(CategoryXceedGrid);

            CategoryValidationRule valueValidationRule = new CategoryValidationRule();
            CategoryXceedGrid.Columns[VALUE_COLUMN_INDEX].CellValidationRules.Add(valueValidationRule);
        }

        /// <summary>
        /// Method loads grid layout
        /// </summary>
        private void _InitQuantitiesDataGridLayout()
        {
            DataGridCollectionViewSource collectionSource =
                (DataGridCollectionViewSource)LayoutRoot.FindResource("quantityCollection");

            collectionSource.Source = SymbologyManager.OrderQuantities;

            GridStructureInitializer structureInitializer =
                new GridStructureInitializer("ESRI.ArcLogistics.App.GridHelpers.QuantitySymbologyGridStructure.xaml");

            structureInitializer.BuildGridStructure(collectionSource, QuantityXceedGrid);

            // load grid layout
            GridLayoutLoader layoutLoader = new GridLayoutLoader("QuantitiesGridSettings", collectionSource.ItemProperties);
            layoutLoader.LoadLayout(QuantityXceedGrid);

            QuantityValidationRule valuesValidationRule = new QuantityValidationRule();
            QuantityXceedGrid.Columns[MINVALUE_COLUMN_INDEX].CellValidationRules.Add(valuesValidationRule);
            QuantityXceedGrid.Columns[MAXVALUE_COLUMN_INDEX].CellValidationRules.Add(valuesValidationRule);
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_edited != null)
                QuantityXceedGrid.CancelEdit();

            SymbologyManager.SymbologyType = SymbologyType.CategorySymbology;
            QuantityXceedGrid.Visibility = Visibility.Collapsed;
            CategoryXceedGrid.Visibility = Visibility.Visible;
            _SetFieldBoxIndex();

            _SetShadowGrid();
        }

        private void QuantitiesButton_Click(object sender, RoutedEventArgs e)
        {
            if (_edited != null)
                CategoryXceedGrid.CancelEdit();

            SymbologyManager.SymbologyType = SymbologyType.QuantitySymbology;
            QuantityXceedGrid.Visibility = Visibility.Visible;
            CategoryXceedGrid.Visibility = Visibility.Collapsed;
            _SetFieldBoxIndex();

            _SetShadowGrid();
        }

        private void _SetShadowGrid()
        {
            if (NONE_FIELD_NAME.Equals((string)FieldBox.SelectedItem))
                lockedGrid.Visibility = Visibility.Visible;
            else
                lockedGrid.Visibility = Visibility.Hidden;
        }

        private void InsertionRow_Initialized(object sender, EventArgs e)
        {
            _InsertionRow = sender as InsertionRow;
        }

        /// <summary>
        /// Ocuurs when user selects any row in table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataRowGotFocus(object sender, RoutedEventArgs e)
        {
            DeleteButton.IsEnabled = true;
        }

        private void CollectionView_CreatingNewItem(object sender, DataGridCreatingNewItemEventArgs e)
        {
            DeleteButton.IsEnabled = false;
            if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
                e.NewItem = new OrderCategory(false);
            else
                e.NewItem = new OrderQuantity(false);

            e.Handled = true;
            _StartEdit((SymbologyRecord)e.NewItem);
        }

        private void DataGridCollectionViewSource_CancelingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
            _EndEdit();
        }

        private void DataGridCollectionViewSource_CommittingNewItem(object sender, DataGridCommittingNewItemEventArgs e)
        {
            if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
            {
                ICollection<OrderCategory> source = 
                    e.CollectionView.SourceCollection as ICollection<OrderCategory>;
                source.Add((OrderCategory)e.Item);
                e.Index = source.Count - 1;
                e.NewCount = source.Count;
            }
            else
            {
                _Validate((OrderQuantity)e.Item);
                ICollection<OrderQuantity> source = 
                    e.CollectionView.SourceCollection as ICollection<OrderQuantity>;
                source.Add((OrderQuantity)e.Item);
                e.Index = source.Count - 1;
                e.NewCount = source.Count;
            }

            e.Handled = true;

            _EndEdit();
        }

        private void DataGridCollectionViewSource_CommittingEdit(object sender, DataGridItemCancelEventArgs e)
        {
            e.Handled = true;
            if (SymbologyManager.SymbologyType == SymbologyType.QuantitySymbology)
                _Validate((OrderQuantity)e.Item);
            _EndEdit();
        }

        private void DataGridCollectionViewSource_BeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            _StartEdit((SymbologyRecord)e.Item);
            e.Handled = true;

            if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
            {
                if (((OrderCategory)e.Item).DefaultValue)
                    CategoryXceedGrid.Columns[VALUE_COLUMN_INDEX].ReadOnly = true;
            }
            else
            {
                if (((OrderQuantity)e.Item).DefaultValue)
                {
                    QuantityXceedGrid.Columns[MINVALUE_COLUMN_INDEX].ReadOnly = true;
                    QuantityXceedGrid.Columns[MAXVALUE_COLUMN_INDEX].ReadOnly = true;
                }
            }
        }

        private void DataGridCollectionViewSource_CancelingEdit(object sender, DataGridItemHandledEventArgs e)
        {
            _EndEdit();
            e.Handled = true;
        }

        private void _StartEdit(SymbologyRecord symbologyRecord)
        {
            Debug.Assert(_edited == null);
            _edited = symbologyRecord;
        }

        private void _EndEdit()
        {
            Debug.Assert(_edited != null);
            if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
            {
                if (CategoryXceedGrid.Columns.Count > 0)
                    CategoryXceedGrid.Columns[VALUE_COLUMN_INDEX].ReadOnly = false;
            }
            else
            {
                if (QuantityXceedGrid.Columns.Count > 0)
                {
                    QuantityXceedGrid.Columns[MINVALUE_COLUMN_INDEX].ReadOnly = false;
                    QuantityXceedGrid.Columns[MAXVALUE_COLUMN_INDEX].ReadOnly = false;
                }
            }
            _edited = null;
        }

        private void FieldBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Finish editing if in edit mode.
            if (_edited != null)
            {
                DataGridControl dataGridControl;
               
                if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
                {
                    dataGridControl = CategoryXceedGrid;
                }
                else
                {
                    dataGridControl = QuantityXceedGrid;
                }

                try
                {
                    dataGridControl.EndEdit();
                }
                catch 
                {
                    dataGridControl.CancelEdit();
                }
            }

            DeleteButton.IsEnabled = false;
            if (_symbologyInited && _userChange)
            {
                if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
                {
                    SymbologyManager.CategoryOrderField = (string)FieldBox.SelectedItem;
                    SymbologyManager.OrderCategories.Clear();
                    SymbologyManager.AddDefaultCategory();
                }
                else
                {
                    SymbologyManager.QuantityOrderField = (string)FieldBox.SelectedItem;
                    SymbologyManager.OrderQuantities.Clear();
                    SymbologyManager.AddDefaultQuantity();
                }

                _SetShadowGrid();
            }
            _OnFieldChanged();
        }

        private void _OnFieldChanged()
        {
            bool enabled = (string)FieldBox.SelectedItem != NONE_FIELD_NAME;
            CategoryXceedGrid.IsEnabled = enabled;
            QuantityXceedGrid.IsEnabled = enabled;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SymbologyManager.SymbologyType == SymbologyType.CategorySymbology)
            {
                List<OrderCategory> categoryList = new List<OrderCategory>();
                foreach (OrderCategory orderCategory in CategoryXceedGrid.SelectedItems)
                {
                    if (!orderCategory.DefaultValue)
                    {
                        categoryList.Add(orderCategory);
                        if (orderCategory == _edited)
                            CategoryXceedGrid.CancelEdit();
                    }
                }
                foreach (OrderCategory orderCategory in categoryList)
                    SymbologyManager.OrderCategories.Remove(orderCategory);
            }
            else
            {
                List<OrderQuantity> quantityList = new List<OrderQuantity>();
                foreach (OrderQuantity orderQuantity in QuantityXceedGrid.SelectedItems)
                {
                    if (orderQuantity == _edited)
                        QuantityXceedGrid.CancelEdit();
                    if (!orderQuantity.DefaultValue)
                        quantityList.Add(orderQuantity);
                }
                foreach (OrderQuantity orderQuantity in quantityList)
                    SymbologyManager.OrderQuantities.Remove(orderQuantity);
            }

            DeleteButton.IsEnabled = false;
        }

        private void _Validate(OrderQuantity checkingObject)
        {
            if (checkingObject.DefaultValue)
                return;

            double minValue = checkingObject.MinValue;
            double maxValue = checkingObject.MaxValue;
            foreach (OrderQuantity element in SymbologyManager.OrderQuantities)
            {
                if (!element.Equals(checkingObject) && !element.DefaultValue &&
                    ((minValue < element.MinValue && maxValue > element.MinValue)
                    || (minValue < element.MaxValue && maxValue > element.MaxValue)
                    || minValue == element.MinValue))
                {
                    string mes = string.Format((string)App.Current.FindResource("RangeIntersectsText"),
                        element.MinValue, element.MaxValue);
                    throw new NotSupportedException(mes);
                }
            }
        }

        private void _ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.DockingLayoutStateShedulePage = null;
            Settings.Default.Save();
        }

        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ObservableCollection<string> _categoryFieldNames = new ObservableCollection<string>();
        private ObservableCollection<string> _quantityFieldNames = new ObservableCollection<string>();

        private InsertionRow _InsertionRow;

        private SymbologyRecord _edited;

        private bool _symbologyInited;
        // flag for indicating fieldbox selection changed by user
        private bool _userChange = true;

        #endregion

        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string COUNT_PROPERTY_NAME = "Count";
        private const string NONE_FIELD_NAME = "";
        private const int VALUE_COLUMN_INDEX = 4;
        private const int MINVALUE_COLUMN_INDEX = 4;
        private const int MAXVALUE_COLUMN_INDEX = 5;

        #endregion
    }
}
