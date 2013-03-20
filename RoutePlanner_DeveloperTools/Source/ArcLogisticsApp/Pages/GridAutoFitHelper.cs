using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Helper class for autofit items on page.
    /// </summary>
    internal class GridAutoFitHelper
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataGridControl">Grid from parent page.</param>
        /// <param name="layoutRoot">Parent page layout root.</param>
        /// <param name="mapContainer">Container element for map.</param>
        public GridAutoFitHelper(DataGridControlEx dataGridControl, Grid layoutRoot, FrameworkElement mapContainer)
        {
            _dataGridControl = dataGridControl;
            _dataGridControl.OnItemSourceChanged += new EventHandler(_DataGridControlItemSourceChanged);

            _layoutRoot = layoutRoot;

            _mapContainer = mapContainer;
            _mapContainer.SizeChanged += new SizeChangedEventHandler(_MapContainerSizeChanged);

            _ProcessNewItemsSource();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// React on changes in grid items collection.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Collection changed event args.</param>
        private void _ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                _SetLayout();
            }
        }

        /// <summary>
        /// React on items collection of datagrid changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DataGridControlItemSourceChanged(object sender, EventArgs e)
        {
            if (_items != null)
            {
                _items.CollectionChanged -= new NotifyCollectionChangedEventHandler(_ItemsCollectionChanged);
            }

            _ProcessNewItemsSource();
        }

        /// <summary>
        /// Subscribe to items collection changed and set layout.
        /// </summary>
        private void _ProcessNewItemsSource()
        {
            _items = _dataGridControl.Items;
            _items.CollectionChanged += new NotifyCollectionChangedEventHandler(_ItemsCollectionChanged);

            _SetLayout();
        }

        /// <summary>
        /// Set grid size.
        /// </summary>
        private void _SetLayout()
        {
            if (_dataGridControl.Items.Count < MINIMUM_ROW_COUNT)
            {
                // If items count less than 5 - set size for 5 items.
                _layoutRoot.RowDefinitions[DATA_GRID_ROW_DEFINITION_INDEX].Height =
                    new System.Windows.GridLength(DEFAULT_ROW_HEIGHT * (MINIMUM_ROW_COUNT + HEADER_ROWS_COUNT));
            }
            else
            {
                int rowsToShowCount = HEADER_ROWS_COUNT + _dataGridControl.Items.Count;
                Border border = _layoutRoot.Children[DATA_GRID_ROW_INDEX] as Border;

                if (_layoutRoot.ActualHeight / 2 > rowsToShowCount * DEFAULT_ROW_HEIGHT)
                {
                    // If all items accommodate in grid with half size of frame, set size on items.
                    _layoutRoot.RowDefinitions[DATA_GRID_ROW_DEFINITION_INDEX].Height =
                        new System.Windows.GridLength(rowsToShowCount * DEFAULT_ROW_HEIGHT);
                }
                else
                {
                    // Otherwise set grid size to half of frame.
                    _layoutRoot.RowDefinitions[DATA_GRID_ROW_DEFINITION_INDEX].Height = new System.Windows.GridLength(0.5, System.Windows.GridUnitType.Star);
                }
            }

            _UpdateMapHeight();
        }

        /// <summary>
        /// React on map container size changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _UpdateMapHeight();
        }

        /// <summary>
        /// Update map container height.
        /// </summary>
        private void _UpdateMapHeight()
        {
            // Check that map container height is at least quarter of layout root height.
            if (_mapContainer.ActualHeight < _layoutRoot.ActualHeight * MINIMAL_MAP_CONTAINER_AREA_RATIO)
            {
                double buttonsGroupHeight = _layoutRoot.RowDefinitions[BUTTONS_GROUP_ROW_DEFINITION_INDEX].ActualHeight;

                // Calculate height for datagrid row if it too large.
                double newHeight = _layoutRoot.ActualHeight * (1 - MINIMAL_MAP_CONTAINER_AREA_RATIO) - buttonsGroupHeight;

                // New height must be not negatice
                if (newHeight < 0)
                {
                    newHeight = 0;
                }

                _layoutRoot.RowDefinitions[DATA_GRID_ROW_DEFINITION_INDEX].Height = new System.Windows.GridLength(newHeight);
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Index of row in definions, which contains datagrid.
        /// </summary>
        private const int DATA_GRID_ROW_DEFINITION_INDEX = 2;

        /// <summary>
        /// Index of row in definions, which contains buttons group.
        /// </summary>
        private const int BUTTONS_GROUP_ROW_DEFINITION_INDEX = 1;

        /// <summary>
        /// Index of row in grid, which contains datagrid.
        /// </summary>
        private const int DATA_GRID_ROW_INDEX = 1;

        /// <summary>
        /// Header rows in datagrid.
        /// </summary>
        private const int HEADER_ROWS_COUNT = 3;

        /// <summary>
        /// Minimum rows count to show.
        /// </summary>
        private const int MINIMUM_ROW_COUNT = 5;

        /// <summary>
        /// Datagrid control default row height.
        /// </summary>
        private readonly double DEFAULT_ROW_HEIGHT = (double)App.Current.FindResource("XceedRowDefaultHeight");

        /// <summary>
        /// Minimal ratio of area for map container.
        /// </summary>
        private const double MINIMAL_MAP_CONTAINER_AREA_RATIO = 0.25;

        #endregion

        #region Private members

        /// <summary>
        /// Data grid control.
        /// </summary>
        private DataGridControlEx _dataGridControl;

        /// <summary>
        /// Grid items collection.
        /// </summary>
        private INotifyCollectionChanged _items;

        /// <summary>
        /// Parent page layout root.
        /// </summary>
        private Grid _layoutRoot;

        /// <summary>
        /// Container element for map.
        /// </summary>
        private FrameworkElement _mapContainer;

        #endregion
    }
}
