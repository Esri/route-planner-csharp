using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.DragAndDrop;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for FindOrdersView.xaml.
    /// </summary>
    internal partial class FindOrdersView : DockableContent
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public FindOrdersView()
        {
            InitializeComponent();

            // Create grid length for hided time range controls.
            _timeRangeHided = new GridLength(0, GridUnitType.Star);

            // Grid length for visible time range controls.
            _timeRangeShowed = LayoutRoot.RowDefinitions[RANGE_ITEM_ROW_INDEX].Height;

            // Prepare grid view.
            _collectionSource = (DataGridCollectionViewSource)LayoutRoot.FindResource("ordersCollection");

            if (App.Current.Project != null)
            {
                GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.FindOrdersGridStructure);
                structureInitializer.BuildGridStructure(_collectionSource, DataGridControl);
                _collectionSource.Source = new Collection<Order>();
            }

            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);

            // Subscribe on mouse move for drag and drop.
            Application.Current.MainWindow.MouseMove += new MouseEventHandler(_MouseMove);
 
            UpdateLayout();
        }

        #endregion

        #region Public members

        /// <summary>
        /// Gets/sets schedule page.
        /// </summary>
        internal OptimizeAndEditPage ParentPage
        {
            get
            {
                return _optimizeAndEditPage;
            }
            set
            {
                _optimizeAndEditPage = value;

                if (_optimizeAndEditPage != null)
                    _optimizeAndEditPage.LockedPropertyChanged += new EventHandler(_LockedPropertyChanged);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Create new grid structure, using current project settings.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ProjectLoaded(object sender, EventArgs e)
        {
            GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.FindOrdersGridStructure);
            structureInitializer.BuildGridStructure(_collectionSource, DataGridControl);
        }

        /// <summary>
        /// React on range type selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _RangeTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            _SearchCriteriaChanged();
        }

        /// <summary>
        /// React on search criteria changed.
        /// </summary>
        private void _SearchCriteriaChanged()
        {
            if (_loaded)
            {
                ComboBoxItem item = (ComboBoxItem)cbSearch.SelectedItem;
                if (item.Name.Equals(RANGE_ITEM_NAME, StringComparison.OrdinalIgnoreCase))
                {
                    // Time range selected - show time range controls.
                    if (FindButton != null)
                        FindButton.Visibility = Visibility.Collapsed;

                    LayoutRoot.RowDefinitions[RANGE_ITEM_ROW_INDEX].Height = _timeRangeShowed;
                    if (TimeRangeElements != null)
                        TimeRangeElements.Visibility = Visibility.Visible;

                    UpdateLayout();
                }
                else
                {
                    // Not time range selected - hide time range controls.
                    if (FindButton != null)
                        FindButton.Visibility = Visibility.Visible;

                    if (LayoutRoot.RowDefinitions[RANGE_ITEM_ROW_INDEX].Height == _timeRangeShowed)
                    {
                        LayoutRoot.RowDefinitions[RANGE_ITEM_ROW_INDEX].Height = _timeRangeHided;
                        if (TimeRangeElements != null)
                            TimeRangeElements.Visibility = Visibility.Collapsed;

                        UpdateLayout();
                    }
                }
            }
        }

        /// <summary>
        /// React on find button click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _FindButtonClick(object sender, RoutedEventArgs e)
        {
            _FindOrder();
        }

        /// <summary>
        /// If user press "Enter" - start finding order.
        /// </summary>
        /// <param name="sender">Ingored.</param>
        /// <param name="e">KeyEventArgs.</param>
        private void _OrderTextKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _FindOrder();
        }

        /// <summary>
        /// Find order.
        /// </summary>
        private void _FindOrder()
        {
            Cursor = Cursors.Wait;

            try
            {
                // Get range.
                string keyword = txtOrderText.Text;

                DateTime currentDate = DateTime.Now.Date;
                TimeRanges rangeType = (TimeRanges)cbSearch.SelectedIndex;
                TimeRange range = null;
                if (rangeType == TimeRanges.SpecifiedTimeRange)
                {
                    if (dpFrom.SelectedDate.HasValue && dpTo.SelectedDate.HasValue)
                    {
                        range = new TimeRange(dpFrom.SelectedDate.Value.Date, dpTo.SelectedDate.Value.Date);
                    }
                }
                else
                    range = TimeRangeHelper.GetRange(currentDate, rangeType);

                // If range valid make search.
                if (range != null)
                {
                    if (_previousOrdersCollection != null)
                    {
                        _previousOrdersCollection.Dispose();
                        _previousOrdersCollection = null;
                    }

                    OrderManager manager = App.Current.Project.Orders;
                    IDataObjectCollection<Order> orders = manager.SearchByKeyword(range.Start, range.End.AddDays(1), keyword, true);

                    // Store orders collection to dispose.
                    _previousOrdersCollection = orders;

                    UpdateLayout();

                    _collectionSource.Source = null;
                    // Set orders collection.
                    _collectionSource.Source = orders;

                    // Save last search criteria.
                    Properties.Settings.Default.FindOrdersLastKeyword = txtOrderText.Text;
                    Properties.Settings.Default.FindOrdersSearchCriteria =
                        TimeRangeHelper.GetRangeSettingsName(rangeType);
                    Properties.Settings.Default.Save();

                    _needToClearSelection = true;
                }
            }
            finally
            {
                Cursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// React on click.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get order which will be selected later by parent row.
            Row row = XceedVisualTreeHelper.GetRowByEventArgs(e);

            if (row != null)
            {
                Order order = row.DataContext as Order;

                if (order != null && order.PlannedDate.HasValue)
                {
                    List<Order> orderList = new List<Order>();
                    orderList.Add(order);
                    _optimizeAndEditPage.Select(orderList); // exception
                }

                // We must start dragging on mouse move.
                _mustStartDraggingOnMouseMove = true;
            }
        }

        /// <summary>
        /// Init View.
        /// </summary>
        private void _Init()
        {
            if (!_loaded)
            {
                _loaded = true;

                App.Current.ProjectClosed += new EventHandler(_ProjectClosed);

                // Get last search options.
                txtOrderText.Text = Properties.Settings.Default.FindOrdersLastKeyword;
                if (Properties.Settings.Default.FindOrdersSearchCriteria.Length != 0)
                    cbSearch.SelectedIndex = (int)TimeRangeHelper.GetRangeType(
                        Properties.Settings.Default.FindOrdersSearchCriteria);

                _SearchCriteriaChanged();
            }
        }

        /// <summary>
        /// Clear finded orders collection.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ProjectClosed(object sender, EventArgs e)
        {
            _collectionSource.Source = null;
        }

        /// <summary>
        /// React on main grid loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _GridLoaded(object sender, RoutedEventArgs e)
        {
            _Init();
        }

        /// <summary>
        /// React on locked property changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _LockedPropertyChanged(object sender, EventArgs e)
        {
            if (_optimizeAndEditPage.IsLocked)
                lockedGrid.Visibility = Visibility.Visible;
            else
                lockedGrid.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// React on selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridControlSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (_needToClearSelection)
            {
                // Invoke postponed clearing selection.
                Dispatcher.BeginInvoke(new Action(delegate() { DataGridControl.SelectedItems.Clear(); }),
                    DispatcherPriority.Send);

                _needToClearSelection = false;
            }
        }

        /// <summary>
        /// Starts drag object.
        /// </summary>
        /// <param name="sender">Either Main Window or data grid control.</param>
        /// <param name="e">Event args.</param>
        private void _MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && _mustStartDraggingOnMouseMove)
            {
                Row currentRow = DataGridControl.GetContainerFromItem(DataGridControl.CurrentItem) as Row;
                if (currentRow == null)
                    return;

                // If user try to select text in cell we shouldn't begin drag'n'drop.
                Cell currentCell = currentRow.Cells[DataGridControl.CurrentContext.CurrentColumn];
                if (currentCell == null || currentCell.IsBeingEdited)
                    return;

                // If items are selected - try to start dragging.
                if (DataGridControl.SelectedItems.Count > 0)
                    _TryToStartDragging();
            }

            // Reset the flat on any mouse move without pressed left button.
            _mustStartDraggingOnMouseMove = false;
        }

        /// <summary>
        /// Method checks is dragging allowed and starts dragging if possible.
        /// </summary>
        private void _TryToStartDragging()
        {
            Collection<object> selection = _GetSelectedOrders();

            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();
            bool isDragAllowed = dragAndDropHelper.IsDragAllowed(selection);

            if (isDragAllowed && selection.Count > 0)
            {
                // We use deferred call to allow grid complete BringItemIntoView.
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    dragAndDropHelper.StartDragOrders(selection, DragSource.FindView);
                }));
            }
        }

        /// <summary>
        /// Method separates orders from items control selection.
        /// </summary>
        /// <returns>Collection of selected orders.</returns>
        private Collection<object> _GetSelectedOrders()
        {
            Collection<object> selectedOrders = new Collection<object>();
            foreach (object obj in DataGridControl.SelectedItems)
            {
                Debug.Assert(obj is Order);
                selectedOrders.Add(obj);
            }

            return selectedOrders;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Name of item for time range.
        /// </summary>
        private const string RANGE_ITEM_NAME = "TimeRange";

        /// <summary>
        /// Row index of time range elements.
        /// </summary>
        private const int RANGE_ITEM_ROW_INDEX = 2;

        #endregion

        #region Private members

        /// <summary>
        /// Orders collection source.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource;

        /// <summary>
        /// Grid length for hided time range controls.
        /// </summary>
        private GridLength _timeRangeHided;

        /// <summary>
        /// Grid length for showed time range controls.
        /// </summary>
        private GridLength _timeRangeShowed;

        /// <summary>
        /// Parent page.
        /// </summary>
        private OptimizeAndEditPage _optimizeAndEditPage;

        /// <summary>
        /// Is view loaded.
        /// </summary>
        private bool _loaded;

        /// <summary>
        /// Previous synchronized orders collection.
        /// </summary>
        private IDataObjectCollection<Order> _previousOrdersCollection;

        /// <summary>
        /// Flag shows that selection was set by grid and have to be cleared.
        /// </summary>
        private bool _needToClearSelection;

        /// <summary>
        /// Flag shows whether control must start dragging on mouse move.
        /// </summary>
        private bool _mustStartDraggingOnMouseMove;

        #endregion
    }
}
