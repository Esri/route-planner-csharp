using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;
using Point = System.Windows.Point;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// Class for managing popup which should point at cell with invalid item.
    /// </summary>
    internal class ValidationCalloutController
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dataGrid">DataGridControlEx.</param>
        /// 
        public ValidationCalloutController(DataGridControlEx dataGrid)
        {
            // Check input parameter.
            Debug.Assert(dataGrid != null);

            // Init fields.
            _dataGrid = dataGrid;
            var context = DataGridControl.GetDataGridContext(_dataGrid);
            _collection = context.Items;

            // When page loaded - do validation and subscribe to events.
            dataGrid.Loaded += (sender, args) =>
            {
                // Do initial validation.
                _Validate();

                // Subscribe to events.
                _InitEventHandlers();
            };

            // When page unloaded - close callout and unsubscribe from events.
            dataGrid.Unloaded += (sender, args) =>
            {
                // Close popup.
                _ClosePopup(false);

                // Unsubscribe from events.
                _UnsubscribeFromEvents();
            };

            // Init timer, which will hide callout.
            _InitTimer();
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Popup, which will be shown near error cell.
        /// Use this property instead of '_callout' field.
        /// </summary>
        private Callout _Callout
        {
            get
            {
                if (_callout == null)
                    _callout = new Callout();
                return _callout;
            }
        }

        /// <summary>
        /// Invalid item.
        /// Use this property instead of "_invalidItem" field.
        /// </summary>
        /// <remarks>When setting new value trying to unsubscribe 
        /// from previous item's property changed event.</remarks>
        private IDataErrorInfo _InvalidItem
        {
            get
            {
                return _invalidItem;
            }

            set
            {
                // Unsubscribe from property changed event.
                if (_invalidItem as INotifyPropertyChanged != null)
                    (_invalidItem as INotifyPropertyChanged).PropertyChanged -=
                        new PropertyChangedEventHandler(_InvalidItemPropertyChanged);

                // Set new value.
                _invalidItem = value;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Subscribe to events.
        /// </summary>
        private void _InitEventHandlers()
        {
            // If datagrid items source exists - subscribe to its events.
            if (_dataGrid.ItemsSource is DataGridCollectionView)
                _SubscribeToItemsSourceEvents();

            // Subscribe to data grid events.
            _dataGrid.OnItemSourceChanged += new EventHandler(_DataGridOnItemSourceChanged);
            _dataGrid.InitializingInsertionRow += new EventHandler<InitializingInsertionRowEventArgs>
                (_DataGridInitializingInsertionRow);
            _dataGrid.MouseMove += new System.Windows.Input.MouseEventHandler(_dataGridMouseMove);

            // Subscribe to window events.
            _SubscribeToWindowEvents();

            // Subscribe to UI manager events.
            App.Current.UIManager.Locked += new EventHandler(_UIManagerLocked);
            App.Current.UIManager.UnLocked += new EventHandler(_UIManagerUnLocked);
        }

        /// <summary>
        /// Unsubscribe from all events.
        /// </summary>
        private void _UnsubscribeFromEvents()
        {
            if (_dataGrid.ItemsSource is DataGridCollectionView)
                _UnsubscribeFromItemsSourceEvents();

            _dataGrid.InitializingInsertionRow -= new EventHandler<InitializingInsertionRowEventArgs>
                (_DataGridInitializingInsertionRow);
            _dataGrid.OnItemSourceChanged -= new EventHandler(_DataGridOnItemSourceChanged);
            _dataGrid.MouseMove -= new System.Windows.Input.MouseEventHandler(_dataGridMouseMove);

            _UnsubscribeFromWindowEvents();

            _StopTimer();
        }

        /// <summary>
        /// Subscribe to grid's items source events.
        /// </summary>
        private void _SubscribeToItemsSourceEvents()
        {
            // If data grid items source was set to null
            // we don't need to subscribe to events.
            if (_dataGrid.ItemsSource == null)
                return;

            // Remember collection to unsubscribe from its items in future.
            _dataGridCollectionView = _dataGrid.ItemsSource as DataGridCollectionView;

            // Check that collection's type.
            Debug.Assert(_dataGridCollectionView != null);

            // Subscribing to data grid source collection events.
            (_dataGridCollectionView as INotifyCollectionChanged).CollectionChanged +=
                new NotifyCollectionChangedEventHandler(_SourceItemsCollectionChanged);

            // If user is adding new item we need to close popup and subscribe to 
            // property changed event.
            _dataGridCollectionView.NewItemCreated += new EventHandler<DataGridItemEventArgs>
                (_CollectionViewSourceCreatingNewItem);

            // If user is editing existence item, need to close popup, subscribe to 
            // property changed event and open popup for item invalid property.
            _dataGridCollectionView.BeginningEdit += new EventHandler<DataGridItemCancelEventArgs>
                (_CollectionViewSourceBeginningEdit);

            // When user finish/editing item, we need 
            // to re-validate data grid's source collection.
            _dataGridCollectionView.NewItemCommitted += new EventHandler<DataGridItemEventArgs>
                (_CollectionViewSourceNewItemFinishEdit);
            _dataGridCollectionView.NewItemCanceled += new EventHandler<DataGridItemEventArgs>
                (_CollectionViewSourceNewItemFinishEdit);
            _dataGridCollectionView.CommittingEdit += new EventHandler<DataGridItemCancelEventArgs>
                (_CollectionViewSourceFinishEdit);
            _dataGridCollectionView.CancelingEdit += new EventHandler<DataGridItemHandledEventArgs>
                (_CollectionViewSourceFinishEdit);
        }

        /// <summary>
        /// Unsubscribe from grid's items source events.
        /// </summary>
        private void _UnsubscribeFromItemsSourceEvents()
        {
            // If data grid items source is null 
            // dont need to unsubscibe from events.
            if (_dataGridCollectionView == null)
                return;

            (_dataGridCollectionView as INotifyCollectionChanged).CollectionChanged -= new
                NotifyCollectionChangedEventHandler(_SourceItemsCollectionChanged);

            _dataGridCollectionView.NewItemCreated -= new EventHandler<DataGridItemEventArgs>
                (_CollectionViewSourceCreatingNewItem);

            _dataGridCollectionView.BeginningEdit -= new EventHandler<DataGridItemCancelEventArgs>
                (_CollectionViewSourceBeginningEdit);

            _dataGridCollectionView.NewItemCommitted -= new EventHandler<DataGridItemEventArgs>
                (_CollectionViewSourceNewItemFinishEdit);
            _dataGridCollectionView.NewItemCanceled -= new EventHandler<DataGridItemEventArgs>
                (_CollectionViewSourceNewItemFinishEdit);
            _dataGridCollectionView.CommittingEdit -= new EventHandler<DataGridItemCancelEventArgs>
                (_CollectionViewSourceFinishEdit);
            _dataGridCollectionView.CancelingEdit -= new EventHandler<DataGridItemHandledEventArgs>
                (_CollectionViewSourceFinishEdit);
        }

        /// <summary>
        /// Gets parent window of Xceed Grid.
        /// </summary>
        /// <returns></returns>
        private Window _GetXceedGridParentWindow()
        {
            Window window = null;

            FrameworkElement parentElement = _dataGrid;
            Debug.Assert(parentElement != null);

            // Find grid's parent window.
            while (parentElement != null && !(parentElement is Window))
            {
                parentElement = (FrameworkElement)VisualTreeHelper.GetParent(parentElement);
            }

            if (parentElement != null)
                window = parentElement as Window;
            else
                window = App.Current.MainWindow;

            return window;
        }

        /// <summary>
        /// Find grid's window and subscribe to it's events.
        /// </summary>
        private void _SubscribeToWindowEvents()
        {
            // Get parent window of Xceed Grid.
            _window = _GetXceedGridParentWindow();

            // Subscribe to window's events.
            _window.Deactivated += new EventHandler(_WindowDeactivated);
            _window.LocationChanged += new EventHandler(_WindowLocationChanged);
            _window.SizeChanged += new SizeChangedEventHandler(_WindowSizeChanged);
            _window.StateChanged += new EventHandler(_WindowStateChanged);
        }

        /// <summary>
        /// Unsubscribe from grid's parent window events.
        /// </summary>
        private void _UnsubscribeFromWindowEvents()
        {
            _window.Deactivated -= new EventHandler(_WindowDeactivated);
            _window.LocationChanged -= new EventHandler(_WindowLocationChanged);
            _window.SizeChanged -= new SizeChangedEventHandler(_WindowSizeChanged);
            _window.StateChanged -= new EventHandler(_WindowStateChanged);
        }

        /// <summary>
        /// Unsubscribe from events.
        /// </summary>
        /// <param name="item">Edited item.</param>
        private void _UnsubscribeFromEditingEvents(object item)
        {
            (item as INotifyPropertyChanged).PropertyChanged -=
                new PropertyChangedEventHandler(_EditedItemPropertyChanged);
            DataRow row = _dataGrid.GetContainerFromItem(item) as DataRow;
            if (row == null)
                row = _dataGrid.InsertionRow;

            // When grid view is TableFlow, here can come null row.
            if (row != null)
                foreach (var cell in row.Cells)
                    cell.PropertyChanged -= new PropertyChangedEventHandler(_CellPropertyChanged);
        }

        /// <summary>
        /// Init timer, which will hide callout.
        /// </summary>
        private void _InitTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, SECONDS_AFTER_CALLOUT_WILL_BE_HIDED);
            _timer.Tick += delegate
            {
                _timer.Stop();
                _showCallout = false;
                _Callout.Close(true);
            };
        }

        /// <summary>
        /// Close popup, unsubscribe from callout events.
        /// </summary>
        /// <param name="animate">Animate closing or not.</param>
        private void _DeactivatePopup(bool animate)
        {
            if (_InvalidItem as INotifyPropertyChanged != null)
            {
                (_InvalidItem as INotifyPropertyChanged).PropertyChanged -=
                    new PropertyChangedEventHandler(_InvalidItemPropertyChanged);
            }

            _ClosePopup(animate);
        }

        /// <summary>
        /// Find ScrollViewer from cell and subscribe to it scroll changed event.
        /// </summary>
        /// <param name="cell">Cell.</param>
        private void _SubscribeOnScroller(Cell cell)
        {
            _scrollViewer = XceedVisualTreeHelper.FindScrollViewer(cell);
            if (_scrollViewer != null)
                _scrollViewer.ScrollChanged += new System.Windows.Controls.
                    ScrollChangedEventHandler(_ScrollViewerScrollChanged);
        }

        /// <summary>
        /// If item has error in visible properies - show first of them.
        /// </summary>
        /// <param name="objectToValidate">IDataErrorInfo implementation.</param>
        private void _ShowCalloutOnInvalidItem(IDataErrorInfo objectToValidate)
        {
            // If there is nothing to validate - do nothing.
            if (objectToValidate == null)
                return;

            // Find column, corresponding to invalid property.
            Column column = _GetColumnWithError(objectToValidate);
            if (column != null)
            {
                _InvalidItem = objectToValidate;
                _InitCallout(column);
            }
        }

        /// <summary>
        /// Close current popup, set new popup to _callout field.
        /// </summary>
        /// <param name="animate">Animate closing or not.</param>
        private void _ClosePopup(bool animate)
        {
            _Callout.Close(animate);
            _callout = new Callout();
        }

        /// <summary>
        /// Check that all items in collection are valid.
        /// </summary>
        private void _Validate()
        {
            if (App.Current.UIManager.IsLocked || !_dataGrid.IsVisible)
                return;

            // Need to stop timer.
            _StopTimer();

            // Enable showing callout.
            _showCallout = true;

            // For all items in data grid source collection.
            foreach (var item in _collection)
                // If item isn't valid.
                if ((item is IDataErrorInfo) &&
                    !(string.IsNullOrEmpty((item as IDataErrorInfo).Error)))
                {
                    // Cast item to IDataErrorInfo.
                    var dataObject = item as IDataErrorInfo;

                    // Find first visible column with error.
                    Column column = _GetColumnWithError(dataObject);
                    if (column != null)
                    {
                        // Remeber data object as invalid item.
                        _InvalidItem = dataObject;

                        // If row with this item isn't shown in grid - show it.
                        if (_IsItemVisible(dataObject))
                            _dataGrid.BringItemIntoView(dataObject);

                        // Set grid's current item on invalid item.
                        // There must be so much invoking, because grid is working async.
                        _dataGrid.Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            _dataGrid.CurrentItem = dataObject;

                            // Show column with wrong property.
                            _dataGrid.Dispatcher.BeginInvoke(new Action(delegate()
                            {
                                _dataGrid.CurrentColumn = column;

                                // Show callout.
                                _dataGrid.Dispatcher.BeginInvoke(new Action(delegate()
                                {
                                    _InitCallout(column);
                                    _StartTimerSubscribeToEvent();
                                }
                                ), DispatcherPriority.ContextIdle);
                            }),
                            DispatcherPriority.ContextIdle);

                        }), DispatcherPriority.ContextIdle);

                        // If we found invalid item - stop searching.
                        return;
                    }
                }
        }

        /// <summary>
        /// Return first visible column with error for this item.
        /// </summary>
        /// <param name="itemToCheck">Item, which property must be checked.</param>
        /// <returns>First visible column with error.</returns>
        private Column _GetColumnWithError(IDataErrorInfo itemToCheck)
        {
            // For each grid visible column.
            foreach (var column in _dataGrid.GetVisibleColumns())
            {
                // Find error message for corresponding property.
                string error = itemToCheck[column.FieldName];

                // If error message isn't empty.
                if (!string.IsNullOrEmpty(error))
                    return column;
            }

            // If we come here - there is no invalid columns.
            return null;
        }

        /// <summary>
        /// Check that row is shown in current grid layout.
        /// </summary>
        /// <param name="dataObject">Data object, which row must be checked.</param>
        /// <returns>'True' if row is shown, 'false' otherwise.</returns>
        private bool _IsItemVisible(IDataErrorInfo dataObject)
        {
            // Get row for this item.
            var row = _dataGrid.GetContainerFromItem(dataObject) as Row;

            if (row != null)
            {
                // Check that row is shown.
                var point = row.PointToScreen(new System.Windows.Point(0, row.Height));
                return _IsPointInsideGrid(row, point);
             }

            return false;
        }

        /// <summary>
        /// Check that point is inside of the grid.
        /// </summary>
        /// <param name="row">Row with point.</param>
        /// <param name="point">Point to check.</param>
        /// <returns>'True' if point is inside visible part of the grid, 'false' otherwise.</returns>
        private bool _IsPointInsideGrid(Row row, Point point)
        {
            if (_dataGrid.InsertionRow == row && _dataGrid.IsBeingEdited)
                return _IsPointInsideGridOnVerticalHorizontalAxis(point, false);
            else
                return _IsPointInsideGridOnVerticalHorizontalAxis(point, true);
        }

        /// <summary>
        /// Check that point is inside visible part of grid.
        /// </summary>
        /// <param name="point">Point to check. It's coordinate must 
        /// be in device-independent units.</param>
        /// <param name="checkVertical">If 'false' there is no need to check is point 
        /// inside grid on </param>
        /// <returns>'True' if point is inside visible part, 'false' otherwise.</returns>
        /// <remarks>Do not use this method directly. Use _IsPointInsideGrid instead.</remarks>
        private bool _IsPointInsideGridOnVerticalHorizontalAxis(Point point, bool checkVertical)
        {
            // Find grid top left, and lower right visible points.
            var minPoint = _dataGrid.PointToScreen(new Point(0, 0));
            var maxPoint = _dataGrid.PointToScreen(
                new Point(_dataGrid.ActualWidth, _dataGrid.ActualHeight));

            // Check, that point is inside grid on Y axis.
            // If we are adding new item then point is definitely visible on Y axis.
            // Otherwise point must be lower then grid's header.
            if (checkVertical &&
                (!(point.Y > minPoint.Y + _gridHeadingRowAndInsertionRowHeight && point.Y < maxPoint.Y)))
                return false;

            // Check, that point is inside grid on X axis.
            if (!(point.X > minPoint.X && point.X <= maxPoint.X))
                return false;

            return true;
        }

        /// <summary>
        /// Show callout in proper position.
        /// </summary>
        /// <param name="column">Collumn with invalid property.</param>
        private void _InitCallout(ColumnBase column)
        {
            // Remember column with invalid property.
            _column = column;

            // Show callout in right place.
            _SetPopupPosition();
        }

        /// <summary>
        /// Start timer that will hide callout and subscribe to invalid item 
        /// and grid invalid cells events.
        /// </summary>
        private void _StartTimerSubscribeToEvent()
        {
            // Start timer.
            _timer.Start();

            // When invalid item's property changed - need to re-validate all items in table.
            (_InvalidItem as INotifyPropertyChanged).PropertyChanged +=
                new PropertyChangedEventHandler(_InvalidItemPropertyChanged);
        }

        /// <summary>
        /// Stop timer.
        /// </summary>
        private void _StopTimer()
        {
            _timer.Stop();            

            // Need to unsubscribe from invalid item's property changed event.
            if(_InvalidItem != null)
                (_InvalidItem as INotifyPropertyChanged).PropertyChanged -=
                new PropertyChangedEventHandler(_InvalidItemPropertyChanged);
        }

        /// <summary>
        /// Get cell with error.
        /// </summary>
        /// <returns><c>Xceed.Wpf.DataGrid.Cell</c>.</returns>
        private Cell _FindCellWithError(Row row)
        {
            // If row is null - return null.
            if (row != null)
            {
                // Get cell with invalid property.
                var cell = (row as Row).Cells[_column];

                // If we havent found scroll viever - do find it.
                if (_scrollViewer == null)
                    _SubscribeOnScroller(cell);

                // If this cell is visible - return it.
                if (cell != null && cell.IsVisible)
                    return cell;
            }

            // If we come here - cell couldnt be found.
            return null;
        }

        /// <summary>
        /// Get cell with error.
        /// </summary>
        /// <returns><c>Xceed.Wpf.DataGrid.Cell</c>.</returns>
        private Row _FindRowWithError()
        {
            // Check that invalid item is not empty and that collumn is still displayed.
            if (_InvalidItem != null && _column != null)
            {
                // Get the row with invalid item.
                var row = _dataGrid.GetContainerFromItem(_InvalidItem);

                // If row is null - then get insertion row.
                if (row == null && _dataGrid.IsNewItemBeingAdded)
                    row = _dataGrid.InsertionRow;

                return row as Row;
            }

            // Callout cannot be displayed so return "bad" point.
            return null;
        }

        /// <summary>
        /// Updating popup position.
        /// </summary>
        private void _SetPopupPosition()
        {
            // Check that there are invalid item and column with error
            if (_InvalidItem == null || _column == null)
                return;

            // If grid isnt in editing state and
            // we cannot show callout - do not show.
            if (!_dataGrid.IsBeingEdited && !_showCallout && !_Callout.IsOpen)
                return;

            var invalidItemError = _InvalidItem[_column.FieldName];

            // Set popup text.
            _Callout.DataContext = invalidItemError;

            // Detect error cell.
            var row = _FindRowWithError();
            var cell = _FindCellWithError(row);
            if (cell != null && !string.IsNullOrEmpty(invalidItemError))
            {
                // Get visible part of the cell.
                Rect placementRect = _GetCellVisibleRectangle(cell, row);
                if (placementRect != Rect.Empty )
                {
                    // Init callout.
                    _Callout.PlacementRectangle = placementRect;
                    _Callout.Placement = System.Windows.Controls.Primitives.PlacementMode.Custom;
                    _Callout.IsOpen = true;
                }
                else if (_Callout.IsOpen)
                    _ClosePopup(false);
            }
            else
                _ClosePopup(false);
        }

        /// <summary>
        /// Get visible part of the cell.
        /// </summary>
        /// <param name="cell">Cell with error.</param>
        /// <param name="row">Row with error.</param>
        /// <returns>Rect, representing visible part of the cell.</returns>
        private Rect _GetCellVisibleRectangle(Cell cell, Row row)
        {
            // Get top left and bottom right points of the cell.
            var topLeft = cell.PointToScreen(new Point(0, 0));
            var bottomRight = cell.PointToScreen(new Point(cell.ActualWidth, cell.ActualHeight));

            // If both points isnt inside of the grid, thath mean that cell isnt visible.
            if (!_IsPointInsideGrid(row, topLeft) && !_IsPointInsideGrid(row, bottomRight))
                return Rect.Empty;

            // Calculate vertical offset.
            double verticalOffset;
            // If cell is in insertion row or if grid have no insertion row -
            // offset is equal to the height of the grid heading row.
            if (row == _dataGrid.InsertionRow || _dataGrid.InsertionRow == null)
                verticalOffset = HEADING_ROW_HEIGHT;
            else
                verticalOffset = _gridHeadingRowAndInsertionRowHeight;

            // Detect grid first row top left point.
            var gridTopLeftPoint = _dataGrid.PointToScreen(new Point(0, verticalOffset));
            // Translate this point to cell coordinate.
            var cellGridTopLeftPoint = cell.PointFromScreen(gridTopLeftPoint);
            // Correct cell visible rectangle if necessary.
            if (cellGridTopLeftPoint.X > 0)
                topLeft.X = gridTopLeftPoint.X;
            if (cellGridTopLeftPoint.Y > 0)
                topLeft.Y = gridTopLeftPoint.Y;

            // Detect grid bottom right point.
            var gridRightEdge = _dataGrid.PointToScreen(
                new Point(_dataGrid.ActualWidth, _dataGrid.ActualHeight));
            // Translate this point to cell coordinate.
            var cellGridRightEdge = cell.PointFromScreen(gridRightEdge);
            // Correct cell visible rectangle if necessary.
            if (cellGridRightEdge.X < cell.ActualWidth)
                bottomRight.X = gridRightEdge.X;
            if (cellGridRightEdge.Y < cell.ActualHeight)
                bottomRight.Y = gridRightEdge.Y;

            return new Rect(topLeft, bottomRight);
        }

        /// <summary>
        /// Return first column from address scope.
        /// </summary>
        /// <returns>First visible address column or false if it wasnt found.</returns>
        private ColumnBase _GetFirstAddressColumn()
        {
            foreach (var column in _dataGrid.GetVisibleColumns())
                if (Address.IsAddressPropertyName(column.FieldName))
                    return column;

            return null;
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Occure when UI interface is unlocked.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _UIManagerUnLocked(object sender, EventArgs e)
        {
            _Validate();
        }

        /// <summary>
        /// Occure when UI interface is locked.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _UIManagerLocked(object sender, EventArgs e)
        {
            _ClosePopup(false);
        }

        /// <summary>
        /// When mouse move in invalid cell - show corresponding popup.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _dataGridMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Find cell on which mouse moved.
            Cell cell = XceedVisualTreeHelper.GetCellByEventArgs(e);

            // If cell has error, grid isnt in editing state and timer is running - show callout.
            if (cell != null && cell.HasValidationError && !_timer.IsEnabled &&
                 !_dataGrid.IsBeingEdited && 
                 !(_dataGrid.InsertionRow != null && _dataGrid.InsertionRow.IsBeingEdited))
            {
                _showCallout = true;

                // Detect current column and item. Show callout.
                IDataErrorInfo currentItem =
                    _dataGrid.GetItemFromContainer(cell) as IDataErrorInfo;

                var columns = _dataGrid.GetVisibleColumns();
                _column = columns.FirstOrDefault(x => x.FieldName == cell.FieldName);
                _InvalidItem = currentItem;
                _SetPopupPosition();

                _showCallout = false;

                // When mouse leave current cell - callout must be hided.
                cell.MouseLeave += new System.Windows.Input.MouseEventHandler(_MouseLeaveCellWithInvalidProperty);
            }
        }

        /// <summary>
        /// When mouse leave invalid cell - close popup.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MouseLeaveCellWithInvalidProperty(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Unsubscribe from this event.
            (sender as Cell).MouseLeave -= new System.Windows.Input.
                MouseEventHandler(_MouseLeaveCellWithInvalidProperty);

            // If grid isnt edited and if timer isnt running - close callout.
            if (!_dataGrid.IsBeingEdited && !_timer.IsEnabled)
                _ClosePopup(true);
        }

        /// <summary>
        /// When some of the error item's property changed we need to re-validate
        /// all items in grid source collection.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _InvalidItemPropertyChanged(object sender,
            PropertyChangedEventArgs e)
        {
            if (_GetInvalidColumns(_invalidItem).Contains(_column))
                return;

            // Need to close old popup.
            _DeactivatePopup(true);

            // Start new validation.
            _Validate();
        }

        /// <summary>
        /// Return first visible column with error for this item.
        /// </summary>
        /// <param name="itemToCheck">Item, which property must be checked.</param>
        /// <returns>First visible column with error.</returns>
        private List<Column> _GetInvalidColumns(IDataErrorInfo itemToCheck)
        {
            List<Column> columns = new List<Column>();

            // For each grid visible column.
            foreach (var column in _dataGrid.GetVisibleColumns())
            {
                // Find error message for corresponding property.
                string error = itemToCheck[column.FieldName];

                // If error message isn't empty.
                if (!string.IsNullOrEmpty(error))
                    columns.Add(column);
            }

            // If we come here - there is no invalid columns.
            return columns;
        }

        /// <summary>
        /// If window state changed - update callout.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _WindowStateChanged(object sender, EventArgs e)
        {
            // If window was restored - show callout.
            if (_window.WindowState == WindowState.Normal || _window.WindowState == WindowState.Maximized)
                if (!_dataGrid.IsBeingEdited)
                    _Validate();
                else
                    _ShowCalloutOnInvalidItem(_InvalidItem);
            // If window was minimized - close callout.
            else
                _DeactivatePopup(false);
        }

        /// <summary>
        /// If window size changed - update callout position.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _WindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _SetPopupPosition();
        }

        /// <summary>
        /// If window location changed - update callout position.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _WindowLocationChanged(object sender, EventArgs e)
        {
            // Close callout if it is open.
            if (_Callout.IsOpen)
                _DeactivatePopup(true);
        }

        /// <summary>
        /// When window deactivated - need to hide callout.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _WindowDeactivated(object sender, EventArgs e)
        {
            _DeactivatePopup(false);
        }

        /// <summary>
        /// If another collection was used as grid's source - subscribe to it's 
        /// collection changed event.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _DataGridOnItemSourceChanged(object sender, EventArgs e)
        {
            _UnsubscribeFromItemsSourceEvents();
            _SubscribeToItemsSourceEvents();
        }

        /// <summary>
        /// If collection changed - need to re-init callout.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _SourceItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Deactivate popup and do validation for whole collection.
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                _DeactivatePopup(true);
                _Validate();
            }
        }

        /// <summary>
        /// Subscribe to cell property changed events. Doing so we will know when user change active cell.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _DataGridInitializingInsertionRow(object sender, InitializingInsertionRowEventArgs e)
        {
            DataRow row = _dataGrid.InsertionRow;
            foreach (var cell in row.Cells)
                cell.PropertyChanged += new PropertyChangedEventHandler(_CellPropertyChanged);
        }

        /// <summary>
        /// Starting editing of new item.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">DataGridItemEventArgs.</param>
        private void _CollectionViewSourceCreatingNewItem(object sender, DataGridItemEventArgs e)
        {
            // Stop timer and unsubscribe from mouse move events.
            _StopTimer();

            // Turn off geocoding validation.
            if (e.Item is IGeocodable)
                (e.Item as IGeocodable).IsAddressValidationEnabled = false;

            // Close popup.
            _DeactivatePopup(true);

            // Remember edited item and subscribe to it's property changed event.
            _InvalidItem = e.Item as IDataErrorInfo;
            (e.Item as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler
                (_EditedItemPropertyChanged);

            // If new item already has errors - show callout.
            _dataGrid.Dispatcher.BeginInvoke(new Action(delegate()
            {
                var column = _GetColumnWithError(_InvalidItem);
                if (column != null)
                    _InitCallout(column);
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// If user scrolled grid - need to change callout position.
        /// </summary>
        /// <param name="sender">Ignore.</param>
        /// <param name="e">Ignore.</param>
        private void _ScrollViewerScrollChanged(object sender, System.Windows.Controls.
            ScrollChangedEventArgs e)
        {
            _dataGrid.Dispatcher.BeginInvoke(new Action(delegate()
            {
                _SetPopupPosition();
            }), DispatcherPriority.ApplicationIdle); 
        }

        /// <summary>
        /// Need to close current popup and open popup if current item has invalid property.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">DataGridItemCancelEventArgs.</param>
        private void _CollectionViewSourceBeginningEdit(object sender, DataGridItemCancelEventArgs e)
        {
            // Do not show callout, when pointing on invalid cell.
            _StopTimer();

            // If callout pointing at another item - close it.
            if (_InvalidItem != e.Item && _Callout.IsOpen)
            {
                _ClosePopup(true);
                _callout = new Callout();
            }

            // Remember edited item and subscribe to it's property changed event.
            _InvalidItem = e.Item as IDataErrorInfo;
            (e.Item as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler
                (_EditedItemPropertyChanged);

            // If current column have error.
            if (_dataGrid.CurrentColumn != null &&
                !string.IsNullOrEmpty((e.Item as IDataErrorInfo)[_dataGrid.CurrentColumn.FieldName]))
            {
                // If callout is pointing at another column - close it.
                if (_column != _dataGrid.CurrentColumn)
                {
                    _ClosePopup(true);
                    _callout = new Callout();
                }

                // If callout closed - open it and point at current cell.
                if (!_Callout.IsOpen)
                {
                    // Detect column to show callout.
                    var column = _dataGrid.CurrentColumn;
                    if (Address.IsAddressPropertyName(column.FieldName))
                        column = _GetFirstAddressColumn();

                    _InitCallout(column);
                }
            }
            // If callout closed - seek for item's invalid properties.
            else if (!_Callout.IsOpen && !string.IsNullOrEmpty(_InvalidItem.Error))
                _ShowCalloutOnInvalidItem(e.Item as IDataErrorInfo);

            // Subscribe to cell property changed events. Doing so we will know when user change active cell.
            DataRow row = _dataGrid.GetContainerFromItem(e.Item) as DataRow;

            // If grid view is table flow - even in editing state row can be null.
            if (row != null)
                foreach (var cell in row.Cells)
                    cell.PropertyChanged += new PropertyChangedEventHandler(_CellPropertyChanged);
        }

        /// <summary>
        /// When editing item property changed need to check its properties for errors.
        /// </summary>
        /// <param name="sender">IDataErrorInfo implementation.</param>
        /// <param name="e">Ignored.</param>
        private void _EditedItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Detect current column.
            var column = _dataGrid.CurrentColumn;

            // If grid view is table flow - current column can be null.
            if (column == null)
                return;

            // Edited property is address property - do not check anything.
            if (Address.IsAddressPropertyName(_dataGrid.CurrentColumn.FieldName))
                return;

            // Set currently editing item as invalid item.
            _InvalidItem = sender as IDataErrorInfo;

            // If currently edited property is invalid - point callout at it.
            if (!string.IsNullOrEmpty(_InvalidItem[column.FieldName]))
            {
                // If current callout doesnt point at this cell - close callout 
                // and open it on right place.
                if (_column != column)
                {
                    _ClosePopup(true);
                    _InitCallout(column);
                }
                // If callout closed - show it.
                else if (!_callout.IsOpen)
                    _InitCallout(column);
            }
            // If currently edited property became valid or 
            // if property on which callout is pointing became valid - 
            // close callout and seek for another invalid property to point at.
            else if (_column == _dataGrid.CurrentColumn ||
                (_column != null && string.IsNullOrEmpty(_InvalidItem[_column.FieldName])))
            {
                _ClosePopup(true);
                _ShowCalloutOnInvalidItem(sender as IDataErrorInfo);
            }
            // If currently edited item has no errors - close popup.
            else if (string.IsNullOrEmpty((sender as IDataErrorInfo).Error))
                _ClosePopup(true);
        }

        /// <summary>
        /// Editing in grid finished, need check for validation.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CollectionViewSourceFinishEdit(object sender, DataGridItemEventArgs e)
        {
            // Unsubscribe from events.
            _UnsubscribeFromEditingEvents(e.Item);

            // If there are no cell with error on edited item - do validation for whole collection.
            if (_GetInvalidColumns(e.Item as IDataErrorInfo).Count == 0)
                _Validate();

            // Start timer and subscribe to item's, cells events.
            _StartTimerSubscribeToEvent();
        }

        /// <summary>
        /// Editing new item in grid finished, need check for validation.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CollectionViewSourceNewItemFinishEdit(object sender, DataGridItemEventArgs e)
        {
            // Close callout.
            _ClosePopup(false);

            // Enable geocoding validation.
            if (e.Item is IGeocodable)
                (e.Item as IGeocodable).IsAddressValidationEnabled = true;

            // Unsubscribe from events.
            _UnsubscribeFromEditingEvents(e.Item);

            // We need to do invoking, to wait while new item will be comitted to the view.
            _dataGrid.Dispatcher.BeginInvoke(new Action(delegate()
            {
                _Validate();
            }), DispatcherPriority.ApplicationIdle);
        }

        /// <summary>
        /// If another cell become active, check corresponding property for errors.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">PropertyChangedEventArgs.</param>
        private void _CellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // If last column was address column and current
            // is address column - do not move callout.
            if (_column != null && Address.IsAddressPropertyName(_dataGrid.CurrentColumn.FieldName) &&
                Address.IsAddressPropertyName(_column.FieldName))
                return;

            // If this cell become active, check that corresponding property has error.
            if (e.PropertyName == IS_CELL_EDITOR_DISPLAYED_PROPERTY_NAME)
                if (!string.IsNullOrEmpty(_InvalidItem[_dataGrid.CurrentColumn.FieldName])
                    && _column != _dataGrid.CurrentColumn)
                {
                    _ClosePopup(true);

                    // If this is address property - point callout to first address column.
                    if (Address.IsAddressPropertyName(_dataGrid.CurrentColumn.FieldName))
                        _InitCallout(_GetFirstAddressColumn());
                    // If it is any other property - point callout to it.	
                    else
                        _InitCallout(_dataGrid.CurrentColumn);
                }
        }

        #endregion

        #region Private Constant Fields

        /// <summary>
        /// Default row height resource name.
        /// </summary>
        private const string DEFAULT_ROW_HEIGHT = "XceedRowDefaultHeight";

        /// <summary>
        /// Is cell editor displayed property name.
        /// </summary>
        private const string IS_CELL_EDITOR_DISPLAYED_PROPERTY_NAME = "IsCellEditorDisplayed";

        /// <summary>
        /// At least 10 px of row with invalid item must be visible to show callout.
        /// </summary>
        private const int HEADING_ROW_HEIGHT = 10;

        /// <summary>
        /// Time after which callout will be hided. In seconds.
        /// </summary>
        private const int SECONDS_AFTER_CALLOUT_WILL_BE_HIDED = 5;

        #endregion

        #region Private Static field

        /// <summary>
        /// Height of the table header: row with column names and insertion row.
        /// </summary>
        private static double _gridHeadingRowAndInsertionRowHeight = HEADING_ROW_HEIGHT +
            (double)System.Windows.Application.Current.FindResource(DEFAULT_ROW_HEIGHT);

        #endregion

        #region Private Fields

        /// <summary>
        /// Grid wich needs to be validated.
        /// </summary>
        private DataGridControlEx _dataGrid;

        /// <summary>
        /// Collection which needs to validate.
        /// </summary>
        private CollectionView _collection;

        /// <summary>
        /// Popup, which will be shown near error cell.
        /// </summary>
        private Callout _callout;


        /// <summary>
        /// Invalid item.
        /// </summary>
        private IDataErrorInfo _invalidItem;

        /// <summary>
        /// Column corresponding to invalid property.
        /// </summary>
        private ColumnBase _column;

        /// <summary>
        /// Grid's scroller.
        /// </summary>
        private TableViewScrollViewer _scrollViewer;

        /// <summary>
        /// Grid's parent window.
        /// </summary>
        private Window _window;

        /// <summary>
        /// Data grid collection source. Need for unsubscribing from old collection.
        /// </summary>
        private DataGridCollectionView _dataGridCollectionView;

        /// <summary>
        /// Timer, which hide callout after some time.
        /// </summary>
        private DispatcherTimer _timer;

        /// <summary>
        /// If this flag set to 'false' callout wouldn't be shown 
        /// on next scrolling/window activating.
        /// </summary>
        private bool _showCallout = true;

        #endregion
    }
}
