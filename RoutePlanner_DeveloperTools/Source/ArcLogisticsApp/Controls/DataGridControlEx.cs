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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.Controls;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.DataGrid.Views;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// This class implements functionality that misses in XCEED DataGridControl out of the box.
    /// Use this class when you need custom application-specific behaviour.
    /// </summary>
    internal class DataGridControlEx : DataGridControl
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>DataGridControlEx</c> class.
        /// </summary>
        public DataGridControlEx()
        {
            // Subscribe on value change event of ItemSource property.
            var dpd = DependencyPropertyDescriptor.FromProperty(DataGridControl.ItemsSourceProperty,
                                                                typeof(DataGridControl));
            dpd.AddValueChanged(this, _OnItemSourceChanged);

            // Initialize default row height.
            _rowHeight = (double)Application.Current.FindResource(DEFAULT_ROW_HEIGHT);

            // Initialize input gestures. F2 should start editing.
            DataGridCommands.BeginEdit.InputGestures.Clear();
            DataGridCommands.BeginEdit.InputGestures.Add(new KeyGesture(START_EDITING_KEY_GESTURE));

            // Subscrube on keyboard events.
            this.KeyUp += new KeyEventHandler(_KeyUp);
            this.KeyDown += new KeyEventHandler(_KeyDown);
            this.PreviewKeyDown += new KeyEventHandler(_PreviewKeyDown);

            // Subscribe on mouse events.
            this.MouseMove += new MouseEventHandler(_MouseMove);
            this.PreviewMouseLeftButtonDown +=
                new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(_PreviewMouseLeftButtonUp);
            this.MouseDoubleClick += new MouseButtonEventHandler(_MouseDoubleClick);
            this.PreviewDragOver += new DragEventHandler(_DragOver);

            // Check that MainWindow is not null.
            // This makes possible to show this control in visual studio designer.
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Deactivated +=
                    new EventHandler(_MainWindowDeactivated);

            // Subscribe to datagrid selection changing.
            this.SelectionChanging += new DataGridSelectionChangingEventHandler(_SelectionChanging);

            // Subscribe to items changed event. 
            var collection = Items as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged +=
                    new NotifyCollectionChangedEventHandler(_CollectionChanged);

            // Add handler to Selection chandeg event to support slow mouse click.
            this.InitializingInsertionRow +=
                new EventHandler<InitializingInsertionRowEventArgs>(_InitializingInsertionRow);

            // Workaround for ToolTip error. 
            // See: http://silverlight-datagrid.com/CS/forums/permalink/29630/29370/ShowThread.aspx
            ToolTipService.SetShowOnDisabled(this, false);

            // Init timer for bringing row item into View.
            _bringItemIntoViewTimer = new DispatcherTimer();
            _bringItemIntoViewTimer.Interval = new TimeSpan(0, 0, 0, 0,
                BRING_ITEM_INTO_VIEW_TIME_INTERVAL);
            _bringItemIntoViewTimer.Tick += new EventHandler(_bringItemIntoViewTick);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Is raised when item source changed.
        /// </summary>
        public event EventHandler OnItemSourceChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets does control support multiple items drag.
        /// </summary>
        public bool MultipleItemsDragSupport
        {
            get { return _multipleItemsDragSupport; }
            set { _multipleItemsDragSupport = value; }
        }

        /// <summary>
        /// Gets all selected items list.
        /// </summary>
        public IList SelectedItemsFromAllContexts
        {
            get
            {
                var selection = new List<object>();

                selection.AddRange(SelectedItems.Cast<object>().ToArray());

                IEnumerable<DataGridContext> childContexts = GetChildContexts();

                foreach (DataGridContext dataGridContext in childContexts)
                {
                    selection.AddRange(dataGridContext.SelectedItems);
                }

                return selection.AsReadOnly();
            }
        }

        /// <summary>
        /// Check that grid's insertion row contains invalid item.
        /// </summary>
        /// <returns>'True' if grid has insertion row, its data context is dataobject and 
        /// this dataobject is invalid.</returns>
        public bool IsInsertionRowInvalid
        {
            get
            {
                if (InsertionRow != null &&
                    InsertionRow.DataContext is IDataErrorInfo &&
                    !string.IsNullOrEmpty((InsertionRow.DataContext as IDataErrorInfo).Error))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Is new item in edited mode.
        /// </summary>
        public bool IsNewItemBeingAdded
        {
            get;
            private set;
        }

        /// <summary>
        /// Is item in edited mode.
        /// </summary>
        public bool IsItemBeingEdited
        {
            get;
            private set;
        }

        /// <summary>
        /// Data Grid Insertion Row.
        /// </summary>
        public InsertionRow InsertionRow
        {
            get
            {
                return _insertionRow;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Try to select item, by using child contexts.
        /// </summary>
        /// <param name="item">Item to select.</param>
        public void SelectInChildContext(object item)
        {
            IEnumerable<DataGridContext> childContexts = GetChildContexts();

            foreach (DataGridContext dataGridContext in childContexts)
            {
                if (dataGridContext.Items.Contains(item))
                {
                    dataGridContext.SelectedItems.Add(item);
                    break; // Process done.
                }
            }
        }

        /// <summary>
        /// Try to deselect item, by using child contexts.
        /// </summary>
        /// <param name="item">Item to deselect.</param>
        public void RemoveFromChildContextSelection(object item)
        {
            IEnumerable<DataGridContext> childContexts = GetChildContexts();

            foreach (DataGridContext dataGridContext in childContexts)
            {
                if (dataGridContext.Items.Contains(item))
                {
                    dataGridContext.SelectedItems.Remove(item);
                    break; // Process done.
                }
            }
        }

        /// <summary>
        /// Clear all selection in grid.
        /// </summary>
        public void ClearSelectionInAllContexts()
        {
            SelectedItems.Clear();

            IEnumerable<DataGridContext> childContexts = GetChildContexts();

            foreach (DataGridContext dataGridContext in childContexts)
            {
                dataGridContext.SelectedItems.Clear();
            }
        }

        /// <summary>
        /// Begins edit current cell.
        /// </summary>
        /// <param name="row">Parent row for cell.</param>
        public void BeginEditCurrentCell(DataRow row)
        {
            Debug.Assert(null != row);

            ColumnBase currentColumn = this.CurrentColumn;
            Debug.Assert(null != currentColumn);
            Debug.Assert(null != row.Cells[currentColumn.FieldName]);
            row.Cells[currentColumn.FieldName].BeginEdit();
        }

        /// <summary>
        /// Method returns a list of visible columns sorted by visible index (as they are shown).
        /// </summary>
        /// <returns>List of columns.</returns>
        public IList<Column> GetVisibleColumns()
        {
            var columns = new List<Column>();

            // Fill collection.
            foreach (Column col in this.Columns)
            {
                // Add only visible columns.
                if (col.Visible)
                    columns.Add(col);
            }

            // Sort collection by visible position.
            columns.Sort(delegate(Column col1, Column col2)
            {
                if (col1.VisiblePosition > col2.VisiblePosition)
                    return 1;
                else if (col1.VisiblePosition < col2.VisiblePosition)
                    return -1;
                else
                    return 0;
            });

            return columns;
        }

        #endregion

        #region Selection Handlers

        /// <summary>
        /// Occurs when current application lost focus
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MainWindowDeactivated(object sender, EventArgs e)
        {
            // if property sets to false - return
            if (!_multipleItemsDragSupport)
                return;

            _OnMouseUp(); // change selection in the same mode as when mouse up occurs
        }

        /// <summary>
        /// Mouse move handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MouseMove(object sender, MouseEventArgs e)
        {
            // Check if some row is hovered.
            Row currentRow = XceedVisualTreeHelper.GetRowByEventArgs(e);

            if (currentRow != null)
            {
                // Check is hovered row are different from last hovered.
                if (!currentRow.Equals(_currentRow))
                {
                    // Start OR re-start timer since user hovered to some another row.
                    _currentRow = currentRow;
                    _bringItemIntoViewTimer.IsEnabled = true;
                    _bringItemIntoViewTimer.Start();
                }
            }
            else
            {
                // Disable timer since user leaved last row.
                _bringItemIntoViewTimer.Stop();
                _bringItemIntoViewTimer.IsEnabled = false;
            }

            // if property sets to false - return
            if (!_multipleItemsDragSupport)
                return;

            _ResetSelectionFields();
        }

        /// <summary>
        /// Method brings Item into view when timer tick occurs.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _bringItemIntoViewTick(object sender, EventArgs e)
        {
            // Make a deferred function call to bring row item into view.
            if (_currentRow != null)
                _currentRow.BringIntoView();

            // Disable timer.
            _bringItemIntoViewTimer.Stop();
            _bringItemIntoViewTimer.IsEnabled = false;
        }

        /// <summary>
        ///  Overrides mouse Left Button up 
        /// </summary>
        /// <param name="sender">Mouse button.</param>
        /// <param name="e">Event args.</param>
        private void _PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_multipleItemsDragSupport)
                _OnMouseUp();

            // If we need to skip editing start - then just reset flag.
            if (_skipStartEditingOnMouseUp)
                _skipStartEditingOnMouseUp = false;
            else
                _StartEditCell(e);
        }

        /// <summary>
        /// Overrides mouse Left Button down behaviour
        /// </summary>
        /// <param name="sender">Mouse.</param>
        /// <param name="e">Event args.</param>
        private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _CheckIfSelectionIsGoingToChangeByClick(e);

            object source = e.OriginalSource;

            // If user clicked on empty space in grid and insertion row can contain new record -
            // need to try commit it.
            if (_insertionRow != null && (source is TableViewScrollViewer))
                _CommitNewItem();

            // Get currently selected row.
            Row currentRow = this.GetContainerFromItem(this.SelectedItem) as Row;
            if (currentRow == null)
            {
                // Try to get row by mouse event args if it's not found by default
                // method GetContainerFromItem.
                currentRow = XceedVisualTreeHelper.GetRowByEventArgs(e);
                if (currentRow == null)
                    return;
            }

            // If user presses clicks on record that is being edited right now - 
            // skip any custom selection logic. If user clicks on blank gridspace - commit changes.
            if (currentRow.IsBeingEdited)
            {
                if (source is TableViewScrollViewer)
                    this._EndEdit();
                return;
            }

            // if property sets to false - return
            if (!_multipleItemsDragSupport)
                return;

            Row parentRow = null;

            if (e.OriginalSource == null)
                return;

            parentRow = XceedVisualTreeHelper.GetRowByEventArgs(e);

            if (_IsShiftOrCtrlPressed())
                return;

            // If ckicked row contains in selection - set flag to false and
            // add all selected items to savedSelection.
            if (parentRow != null &&
                this.SelectedItems != null &&
                this.SelectedItems.Contains(parentRow.DataContext))
            {
                _savedSelection.Clear();
                _useCustomSelectionLogic = true;

                foreach (Object obj in this.SelectedItems)
                    _savedSelection.Add(obj);

                // In case items won't be dragged and user just release the mouse button
                // we should select item that was clicked.
                _singleSelection = parentRow.DataContext;
            }
            else
            {
                _ResetSelectionFields();
            }
        }

        /// <summary>
        /// Checks if selection is going to change by mouse click and sets special flag that
        /// indicates whether editing shouldn't start by mouse up.
        /// </summary>
        /// <param name="mouseEventArgs"></param>
        private void _CheckIfSelectionIsGoingToChangeByClick(MouseButtonEventArgs mouseEventArgs)
        {
            // Find place where user clicked. 
            Point pt = mouseEventArgs.GetPosition(this);
            HitTestResult htResult = VisualTreeHelper.HitTest(this, pt);

            // Check whether user clicked on a row.
            Row clickedRow = null;
            if (htResult != null && htResult.VisualHit != null)
            {
                DependencyObject visual = htResult.VisualHit;
                while (visual != null && !(visual is Row))
                    visual = VisualTreeHelper.GetParent(visual);

                if (visual != null)
                    clickedRow = visual as Row;
            }

            // If user clicked on a row then
            if (clickedRow != null)
            {
                // Get clicked item from row.
                object item = this.GetItemFromContainer(clickedRow);
                if (item != null)
                {
                    // If item is not currently selected and user doesn't add new item to
                    // the selection using Shift or Ctrl.
                    if (!SelectedItems.Contains(item) &&
                        !_IsShiftOrCtrlPressed())
                    {
                        // then set the flag to skip start editing on mouse button up.
                        _skipStartEditingOnMouseUp = true;
                    }
                }
            }
        }

        #endregion

        #region Begin Edit Event Handlers

        /// <summary>
        /// Sets focus to the end of text box content if cell editor is TextBox. 
        /// </summary>
        /// <param name="sender">Data grid control.</param>
        /// <param name="e">Event args.</param>
        private void _MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Get cell that was double clicked.
            Cell cell = XceedVisualTreeHelper.GetCellByEventArgs(e);
            if (cell == null || cell.ReadOnly || cell.ParentRow.ReadOnly)
                return; // User clicked at some place other than cell or cell/row is read only.

            // Check if cell is being edited.
            if (cell.IsCellEditorDisplayed)
            {
                // Find TextBox inside the cell.
                TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(cell);

                if (textBox is AutoSelectTextBox)
                    ((AutoSelectTextBox)textBox).AutoSelectBehavior = AutoSelectBehavior.Never;

                // Set caret to the end of the cell.
                if (textBox != null)
                    textBox.CaretIndex = textBox.Text.Length;
            }
            else
            {
                // Subscribe on property changes to get the event when cell editor becomes visible.
                cell.PropertyChanged += new PropertyChangedEventHandler(_CellPropertyChanged);
            }
        }

        /// <summary>
        /// Called when cell property changed after user made double click on the cell.
        /// </summary>
        /// <param name="sender">Cell.</param>
        /// <param name="e">Event args.</param>
        private void _CellPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == IS_CELL_EDITOR_DISPLAYED_PROPERTY_NAME)
            {
                // Find AutoSelectTextBox inside the cell.
                TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(sender as Cell);
                if (textBox != null)
                {
                    var autoSelectTextBox = textBox as AutoSelectTextBox;
                    if (null != autoSelectTextBox)
                        autoSelectTextBox.AutoSelectBehavior = AutoSelectBehavior.Never;

                    // Get mouse cursor coordinates relative to the text box.
                    textBox.CaretIndex = textBox.Text.Length;

                    // Add handler to "TextChanged" event to set carrent to the end of the text when
                    // cell text will be created in converter. In case convertor is not used and
                    // there is just empty text - subscribe on TextInput event, so when user
                    // types something we will unsubscribe from handler that selects the content.
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.TextChanged +=
                            new TextChangedEventHandler(_TextBoxTextChangedForSetCarret);
                        textBox.PreviewTextInput +=
                            new TextCompositionEventHandler(_TextBoxPreviewTextInputForSetCarret);
                    }
                }
            }

            // Unsubscribe from property changes.
            (sender as Cell).PropertyChanged -= _CellPropertyChanged;
        }

        /// <summary>
        /// Methods unsubscribes from TextChanged to handle the case when convertor is not used and
        /// content was just empty.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _TextBoxPreviewTextInputForSetCarret(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Debug.Assert(textBox != null);

            textBox.PreviewTextInput -= _TextBoxPreviewTextInputForSetCarret;
            textBox.TextChanged -= _TextBoxTextChangedForSetCarret;
        }

        /// <summary>
        /// Occurs after fast double click amd F2 when converter creates cell content.
        /// Sets caret index to the end of TextBlock.
        /// </summary>
        /// <param name="sender">TextBox.</param>
        /// <param name="e">Event args.</param>
        private void _TextBoxTextChangedForSetCarret(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Debug.Assert(textBox != null);

            // Set caret index to the end of TextBox.
            textBox.CaretIndex = textBox.Text.Length;

            // Remove handler.
            textBox.TextChanged -= _TextBoxTextChangedForSetCarret;
            textBox.PreviewTextInput -= _TextBoxPreviewTextInputForSetCarret;
        }

        /// <summary>
        /// Starts editing when user presses F2.
        /// </summary>
        private void _StartEditingByPressingF2()
        {
            // Get row.
            DataRow row = this.GetContainerFromItem(this.CurrentContext.CurrentItem) as DataRow;

            // If row is not null and it is not edited and it is not an insertion row.
            if (row != null &&
                !row.IsBeingEdited &&
                row != _insertionRow)
            {
                // Get current cell.
                Cell currentCell = row.Cells[this.CurrentContext.CurrentColumn];

                // If cell is not read only.
                if (currentCell != null &&
                    !currentCell.ReadOnly &&
                    !currentCell.ParentRow.ReadOnly)
                {
                    // Subcscibe on property changed.
                    currentCell.PropertyChanged +=
                        new PropertyChangedEventHandler(_CellPropertyChangedAfterF2);
                }
            }
        }

        /// <summary>
        /// Called when cell property changed after user pressed F2 on the cell.
        /// </summary>
        /// <param name="sender">Current cell.</param>
        /// <param name="e">Event args.</param>
        private void _CellPropertyChangedAfterF2(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == IS_CELL_EDITOR_DISPLAYED_PROPERTY_NAME)
            {
                // Find AutoSelectTextBox inside the cell.
                TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(sender as Cell);
                if (textBox != null)
                {
                    var autoSelectTextBox = textBox as AutoSelectTextBox;
                    if (null != autoSelectTextBox)
                        autoSelectTextBox.AutoSelectBehavior = AutoSelectBehavior.Never;

                    // Get mouse cursor coordinates relative to the text box.
                    textBox.CaretIndex = textBox.Text.Length;

                    // Add handler to "TextChanged" event to set carrent to the end of the text when
                    // cell text will be created in converter. In case convertor is not used and
                    // there is just empty text - subscribe on TextInput event, so when user
                    // types something we will unsubscribe from handler that selects the content.
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.TextChanged +=
                            new TextChangedEventHandler(_TextBoxTextChangedForSetCarret);
                        textBox.PreviewTextInput +=
                            new TextCompositionEventHandler(_TextBoxPreviewTextInputForSetCarret);
                    }
                }
            }

            // Unsubscribe from property changes.
            (sender as Cell).PropertyChanged -= _CellPropertyChangedAfterF2;
        }

        /// <summary>
        /// Starts editing in necessary cell.
        /// </summary>
        /// <param name="cell">Cell which should be edited.</param>
        private void _StartEditCell(object eventArgs)
        {
            Debug.Assert(eventArgs is RoutedEventArgs);

            if (SelectedItems.Count != 1)
                return;

            Cell currentCell = XceedVisualTreeHelper.GetCellByEventArgs((RoutedEventArgs)eventArgs);

            // If cell not found - return.
            if (currentCell == null)
                return;
            try
            {
                currentCell.BeginEdit();

                // Subscribe on property changes to get the event when cell editor becomes visible.
                currentCell.PropertyChanged +=
                    new PropertyChangedEventHandler(_CellPropertyChangedAfterSlowClick);
            }
            catch
            {
                // Do nothing if
                // cell cannot be edited (locked or read-only or placed in read-only grid -
                // e.g. Stops grid).
            }
        }

        /// <summary>
        /// Called when cell property changed after user made slow double click on the cell.
        /// </summary>
        /// <param name="sender">Cell.</param>
        /// <param name="e">Event args.</param>
        private void _CellPropertyChangedAfterSlowClick(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == IS_CELL_EDITOR_DISPLAYED_PROPERTY_NAME)
            {
                // Find TextBox inside the cell.
                TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(sender as Cell);
                if (textBox != null)
                {
                    // Select all only in case if carent index is not in the end.
                    // If it is in the end this means that it was set there by double click.
                    if (textBox.CaretIndex != textBox.Text.Length)
                        textBox.SelectAll();

                    // Add handler to "TextChanged" event to select all when cell text will be
                    // created in converter. In case convertor is not used and there is
                    // justempty text - subscribe on TextInput event, so when user
                    // types something we will unsubscribe from handler that selects the content.
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.TextChanged +=
                            new TextChangedEventHandler(_TextBoxTextChangedForSelectAll);
                        textBox.PreviewTextInput +=
                            new TextCompositionEventHandler(_TextBoxPreviewTextInputForSelectAll);
                    }
                }
            }

            // Unsubscribe from property changes.
            (sender as Cell).PropertyChanged -= _CellPropertyChangedAfterSlowClick;
        }

        /// <summary>
        /// Methods unsubscribes from TextChanged to handle the case when convertor is not used and
        /// content was just empty.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _TextBoxPreviewTextInputForSelectAll(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Debug.Assert(textBox != null);

            textBox.PreviewTextInput -= _TextBoxPreviewTextInputForSelectAll;
            textBox.TextChanged -= _TextBoxTextChangedForSelectAll;
        }

        /// <summary>
        /// Occurs after slow double click and change focus when converter creates cell content.
        /// Selects all text in TextBlock.
        /// </summary>
        /// <param name="sender">TextBox.</param>
        /// <param name="e">Event args.</param>
        private void _TextBoxTextChangedForSelectAll(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Debug.Assert(textBox != null);

            // If caret index was not chaged before - select all text.
            if (textBox.CaretIndex != textBox.Text.Length)
                textBox.SelectAll();

            // Remove handler.
            textBox.TextChanged -= _TextBoxTextChangedForSelectAll;
            textBox.PreviewTextInput -= _TextBoxPreviewTextInputForSelectAll;
        }

        /// <summary>
        /// Handler added for start editing cell when cell got focus and previous cell was edited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                DataRow row = this.GetContainerFromItem(this.CurrentContext.CurrentItem) as DataRow;

                // If current item is null then probably insertion row is currently edited.
                if (row == null &&
                    _insertionRow != null &&
                    _insertionRow.IsCurrent)
                    row = _insertionRow;

                if (row != null && row.IsBeingEdited)
                {
                    Cell currentCell = row.Cells[this.CurrentContext.CurrentColumn];
                    if (currentCell != null &&
                        !currentCell.ReadOnly &&
                        !currentCell.ParentRow.ReadOnly)
                        try
                        {
                            // If cell is being edited already. This is the case of insertion row,
                            // when focus starts editings.
                            if (currentCell.IsBeingEdited)
                            {
                                // Find TextBox inside the cell.
                                TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(currentCell);

                                // Select all text in cell.
                                if (textBox != null)
                                    textBox.SelectAll();
                            }
                            else // Otherwise we should wait when editing will be started and
                                 // cell editor is instantiated.
                            {
                                currentCell.BeginEdit();

                                // Subscribe on property changes to get the event when cell editor
                                // becomes visible.
                                currentCell.PropertyChanged +=
                                    new PropertyChangedEventHandler(_CellPropertyChangedAfterFocusMoved);
                            }
                        }
                        catch (Exception ex)
                        {
                            // NOTE: xceed's exception - editable cell defines as readonly
                            Logger.Warning(ex.Message);
                        }
                }
            }
        }

        /// <summary>
        /// Called when cell property changed after user changes focus by arrovs keys or tab.
        /// </summary>
        /// <param name="sender">Cell.</param>
        /// <param name="e">Event args.</param>
        private void _CellPropertyChangedAfterFocusMoved(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == IS_CELL_EDITOR_DISPLAYED_PROPERTY_NAME)
            {
                // Find TextBox inside the cell.
                TextBox textBox = XceedVisualTreeHelper.FindTextBoxInsideElement(sender as Cell);

                // Select all text in cell.
                if (textBox != null)
                {
                    textBox.SelectAll();

                    // Add handler to "TextChanged" event to set focus in necessary position
                    // when cell text will be created in converter.
                    // In case convertor is not used and there is just empty text - subscribe 
                    // on TextInput event, so when user types something we will unsubscribe from
                    // handler that selects the content.
                    if (string.IsNullOrEmpty(textBox.Text))
                    {
                        textBox.TextChanged +=
                            new TextChangedEventHandler(_TextBoxTextChangedForSelectAll);
                        textBox.PreviewTextInput +=
                            new TextCompositionEventHandler(_TextBoxPreviewTextInputForSelectAll);
                    }
                }
            }

            // Unsubscribe from property changes.
            (sender as Cell).PropertyChanged -= _CellPropertyChangedAfterFocusMoved;
        }

        #endregion

        #region Change Focused Cell After Commit Methods

        // Declare necessary delegates.
        private delegate void NoParamsDelegate();
        private delegate void ParamsDelegate(object item);

        /// <summary>
        /// Called when new item is commited.
        /// </summary>
        /// <param name="sender">Data grid collection view source.</param>
        /// <param name="e">Event args.</param>
        private void _NewItemCommitted(object sender, DataGridItemEventArgs e)
        {
            Dispatcher.BeginInvoke(new ParamsDelegate(_MoveFocusToFirstCell),
                                   DispatcherPriority.Render, e.Item);

            Debug.Assert(IsNewItemBeingAdded);
            IsNewItemBeingAdded = false;

            Debug.Assert(IsItemBeingEdited);
            IsItemBeingEdited = false;
        }

        /// <summary>
        /// Moves focus to the first editable cell.
        /// </summary>
        /// <param name="newAddedItem"></param>
        private void _MoveFocusToFirstCell(object newAddedItem)
        {
            // Set focus to the first editable cell.
            Column column = _FindFirstColumnToSelect();
            if (column != null)
                this.CurrentColumn = column;

            // Subscrine on cell events to workaround the issue when click on the current cell
            // in the insertion row after commiting new item doesn't start adding new item.
            if (_insertionRow != null)
            {
                Cell cell = _insertionRow.Cells[column];

                _SubscribeOnCellInInsertionRow(cell);
            }
            Mouse.Capture(null);
            // Set default baclground to InsertionRow.
            _SetDefaultInsertionRowBackground();
            // Cancel edits.
            Dispatcher.BeginInvoke(new NoParamsDelegate(_CancelEdits),
                                   DispatcherPriority.Render,
                                   null);
            // Select new Item.
            Dispatcher.BeginInvoke(new ParamsDelegate(_SelectItem),
                                   DispatcherPriority.Render,
                                   newAddedItem);
        }

        /// <summary>
        /// Cancels edits. This method is called after focus is set to the first cell after
        /// new item is commited.
        /// </summary>
        private void _CancelEdits()
        {
            // Just cancel edits.
            this.CancelEdit();
        }

        /// <summary>
        /// Finds first column to select in the grid.
        /// </summary>
        /// <returns>Column or null if there is no appropriate column.</returns>
        private Column _FindFirstColumnToSelect()
        {
            Column resultColumn = null;

            // Get grid view source to access its item properties.
            var gridView = this.ItemsSource as DataGridCollectionView;
            Debug.Assert(gridView != null);

            // Get visible columns.
            IList<Column> visibleColumns = GetVisibleColumns();

            // Find appropraite column.
            foreach (Column col in visibleColumns)
            {
                // Get item property attached to the column.
                DataGridItemPropertyBase itemProperty = _FindItemProperty(col.FieldName,
                                                                          gridView.ItemProperties);
                Debug.Assert(itemProperty != null);

                if (itemProperty != null &&
                    !itemProperty.IsReadOnly && // if item property is not read only
                    itemProperty.DataType != typeof(bool)) // column is not bool - we need to skip
                                                           // boolean flags that can go before
                                                           // textbox fields.
                {
                    // We found the column.
                    resultColumn = col;
                    break;
                }
            }

            return resultColumn;
        }

        /// <summary>
        /// Finds property by name.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <param name="properties">Properties collection.</param>
        /// <returns>Property or null if property is not found.</returns>
        private DataGridItemPropertyBase _FindItemProperty(string name,
            DataGridItemPropertyCollection properties)
        {
            DataGridItemPropertyBase resultProp = null;
            foreach (DataGridItemPropertyBase prop in properties)
            {
                if (prop.Name == name)
                {
                    resultProp = prop;
                    break;
                }
            }

            return resultProp;
        }

        #endregion

        #region Commit New Item On Tab In The Last Column Methods

        #region No Editing Start Workaround Methods

        /// <summary>
        /// Subscribe to the following events of the cell from the insertion row:
        /// 1. IsCurrent property changed.
        /// 2. Preview mouse down.
        /// </summary>
        /// <param name="cell">Cell.</param>
        private void _SubscribeOnCellInInsertionRow(Cell cell)
        {
            cell.PreviewMouseDown +=
                new MouseButtonEventHandler(_FocusCellInInsertionRowPreviewMouseDown);
            var dpd = DependencyPropertyDescriptor.FromProperty(Cell.IsCurrentProperty, typeof(Cell));
            dpd.AddValueChanged(cell, _CellIsCurrentChanged);
        }

        /// <summary>
        /// Unsubscribes from the events from the cell in the insertion row.
        /// </summary>
        /// <param name="cell">Cell.</param>
        private void _UnsubscribeFromCellInInsertionRow(Cell cell)
        {
            cell.PreviewMouseDown -= _FocusCellInInsertionRowPreviewMouseDown;
            var dpd = DependencyPropertyDescriptor.FromProperty(Cell.IsCurrentProperty, typeof(Cell));
            if (dpd != null)
                dpd.RemoveValueChanged(cell, _CellIsCurrentChanged);
        }

        /// <summary>
        /// Cell IsCurrent property has changed. 
        /// </summary>
        /// <param name="sender">Cell that was current.</param>
        /// <param name="e">Ignored.</param>
        /// <remarks>
        /// We no longer need to handle click on this cell, because when the cell loses its focus
        /// repeated click on it will start editing of the insertion row.
        /// </remarks>
        private void _CellIsCurrentChanged(object sender, EventArgs e)
        {
            _UnsubscribeFromCellInInsertionRow(sender as Cell);
        }

        /// <summary>
        /// Starts insertion row editing if user clicked on the focused cell in the insertion row.
        /// </summary>
        /// <param name="sender">Current cell in the insertion row.</param>
        /// <param name="e">Ignored.</param>
        private void _FocusCellInInsertionRowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _UnsubscribeFromCellInInsertionRow(sender as Cell);

            if (!this.IsBeingEdited)
            {
                try
                {
                    _insertionRow.BeginEdit();
                }
                catch(DataGridException)
                {
                    // BeginEdit can throw exception if creation cancelled.
                }
            }
        }

        /// <summary>
        /// Subscribes to the event of the current cell in the insertion row to workaround the issue
        /// when editing doesn't start by current cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _NewItemCanceled(object sender, DataGridItemEventArgs e)
        {
            Debug.Assert(_insertionRow != null);

            // Find current cell in the insertion row.
            Cell currentCell = null;
            foreach (Cell cell in _insertionRow.Cells)
            {
                if (cell.IsCurrent && cell.IsVisible)
                {
                    currentCell = cell;
                    break; // Current cell is found.
                }
            }

            // Subscrine on cell events to workaround the issue when click on the current cell
            // in the insertion row after canceling new item doesn't start adding new item.
            if (currentCell != null)
                _SubscribeOnCellInInsertionRow(currentCell);

            Debug.Assert(IsNewItemBeingAdded);
            IsNewItemBeingAdded = false;

            Debug.Assert(IsItemBeingEdited);
            IsItemBeingEdited = false;
        }

        #endregion

        /// <summary>
        /// Commitsn new item by pressing tab on the last column.
        /// </summary>
        private void _CommitNewItemOnTabInTheLastColumn()
        {
            // Get visible columns sequence.
            IList<Column> visibleColumns = GetVisibleColumns();

            // If columns is the last.
            if (visibleColumns.Count > 0 &&
                this.CurrentColumn == visibleColumns[visibleColumns.Count - 1])
            {
                // End edit. It is called in async manner, because sync call will lead to focus set 
                // to the Status Bar, whereas we need to the first cell.
                Dispatcher.BeginInvoke(new NoParamsDelegate(_EndEdit),
                                       DispatcherPriority.Render,
                                       null);
            }
        }

        /// <summary>
        /// Ends edit after pressing Tab in the last column.
        /// </summary>
        private void _EndEdit()
        {
            try
            {
                // End edit may throw exception if some strict validation rule cannot be met.
                this.EndEdit();
            }
            catch
            {
                // Do nothing.
            }
        }

        #endregion

        #region Drag'n'Drop Event Handlers

        /// <summary>
        /// Handler scrolls grid content if necessary.
        /// </summary>
        /// <param name="sender">DataGridControl.</param>
        /// <param name="e">Event Args.</param>
        private void _DragOver(object sender, DragEventArgs e)
        {
            ScrollViewer scrollViewer = _GetScrollViewer(this);
            _DoAutoScroll(e.GetPosition(this).Y, scrollViewer);
        }

        #endregion

        #region Other Private Methods

        /// <summary>
        /// Sets carret to the end of text box content if cell editor is TextBox.
        /// </summary>
        /// <param name="sender">Data grid control.</param>
        /// <param name="e">Event args.</param>
        private void _PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                // By Escape - cancel edits.
                this.Dispatcher.BeginInvoke(new NoParamsDelegate(_CancelEdits),
                                            DispatcherPriority.Render,
                                            null);
            }
            else if (e.Key == START_EDITING_KEY_GESTURE &&
                     CurrentItem != null &&
                     CurrentColumn != null) // By F2 start editing.
            {
                _StartEditingByPressingF2();
            }
        }

        /// <summary>
        /// Handles Key Down event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                // If current row is insertion row and it is being edited.
                if (_insertionRow != null &&
                    _insertionRow.IsBeingEdited)
                    _CommitNewItemOnTabInTheLastColumn();
            }
        }

        /// <summary>
        /// Initializes insertion row.
        /// </summary>
        /// <param name="sender">Data grid control.</param>
        /// <param name="e">Event args.</param>
        private void _InitializingInsertionRow(object sender, InitializingInsertionRowEventArgs e)
        {
            // If insertion row was initialized before - remove handlers to avoid redundant calls.
            if (_insertionRow != null)
            {
                _insertionRow.EditBegun -= _InsertionRowEditBegun;
                _insertionRow.EditCanceled -= _InsertionRowEditCanceled;
            }

            _insertionRow = e.InsertionRow;

            // Remember default background of InsertionRow (VisualBrush with text "Click here...").
            if (_insertionRowDefaultBackground == null)
                _insertionRowDefaultBackground = _insertionRow.Background;

            // Subscribe to events to set valid background for InsertoinRow.
            _insertionRow.EditBegun += new RoutedEventHandler(_InsertionRowEditBegun);
            _insertionRow.EditCanceled += new RoutedEventHandler(_InsertionRowEditCanceled);
        }

        /// <summary>
        /// Handles cancel editing in InsertionRow. Changes it's background.
        /// </summary>
        /// <param name="sender">InsertionRow.</param>
        /// <param name="e">Event args.</param>
        private void _InsertionRowEditCanceled(object sender, RoutedEventArgs e)
        {
            _SetDefaultInsertionRowBackground();
        }

        /// <summary>
        /// Handles start editing in InsertionRow. Changes it's background.
        /// </summary>
        /// <param name="sender">InsertionRow.</param>
        /// <param name="e">Event args.</param>
        private void _InsertionRowEditBegun(object sender, RoutedEventArgs e)
        {
            _SetEditingInsertionRowBackground();
        }

        /// <summary>
        /// Called when item source changed.
        /// </summary>
        /// <param name="o">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OnItemSourceChanged(object o, EventArgs e)
        {
            // if items source is Data Grid Collection View - add handler for NewItemCommitted
            // event for select new item when it'll be added
            if (this.ItemsSource != null &&
                this.ItemsSource is DataGridCollectionView)
            {
                var source = this.ItemsSource as DataGridCollectionView;
                source.NewItemCommitted +=
                    new EventHandler<DataGridItemEventArgs>(_NewItemCommitted);
                source.NewItemCanceled +=
                    new EventHandler<DataGridItemEventArgs>(_NewItemCanceled);

                source.NewItemCreated += new EventHandler<DataGridItemEventArgs>(_NewItemCreated);
                source.EditBegun += new EventHandler<DataGridItemEventArgs>(_EditBegun);
                source.EditCanceled += new EventHandler<DataGridItemEventArgs>(_EditCanceled);
                source.EditCommitted += new EventHandler<DataGridItemEventArgs>(_EditCommitted);
            }

            if (OnItemSourceChanged != null)
                OnItemSourceChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on new item created.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _NewItemCreated(object sender, DataGridItemEventArgs e)
        {
            Debug.Assert(!IsNewItemBeingAdded);
            IsNewItemBeingAdded = true;

            Debug.Assert(!IsItemBeingEdited);
            IsItemBeingEdited = true;
        }


        /// <summary>
        /// React on edit begun.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _EditBegun(object sender, DataGridItemEventArgs e)
        {
            Debug.Assert(!IsItemBeingEdited);
            IsItemBeingEdited = true;
        }

        /// <summary>
        /// React on edit commited.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _EditCommitted(object sender, DataGridItemEventArgs e)
        {
            Debug.Assert(IsItemBeingEdited);
            IsItemBeingEdited = false;
        }

        /// <summary>
        /// React on edit canceled.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _EditCanceled(object sender, DataGridItemEventArgs e)
        {
            Debug.Assert(IsItemBeingEdited);
            IsItemBeingEdited = false;
        }

        /// <summary>
        /// Method realize custom selection logic.
        /// </summary>
        private void _OnMouseUp()
        {
            // Check is cell was edited.
            Row currentRow = this.GetContainerFromItem(this.CurrentItem) as Row;
            if (currentRow == null)
                return;

            // If user presses clicks on record that is being edited right now - skip any custom
            // selection logic.
            if (currentRow.IsBeingEdited)
                return;

            // If flag is true (default state) we should exit from method and allow grid to
            // change selection by default logic.
            if (!_useCustomSelectionLogic)
                return;

            this.SelectedItems.Clear();

            if (_singleSelection != null)
                this.SelectedItems.Add(_singleSelection);

            _ResetSelectionFields();
        }

        /// <summary>
        /// Method resets values of selection variables.
        /// </summary>
        private void _ResetSelectionFields()
        {
            _useCustomSelectionLogic = false;
            _singleSelection = null;
            _savedSelection.Clear();
        }

        /// <summary>
        /// Method sets focus to the added item.
        /// </summary>
        private void _SelectItem(object item)
        {
            this.SelectedItem = item;
        }

        /// <summary>
        /// Commits item in insertion row if possible. If item has any validation errors -
        /// it will be not commited.
        /// </summary>
        private void _CommitNewItem()
        {
            try
            {
                _insertionRow.EndEdit();
            }
            catch
            {
                // If New Item cannot be commited (e.g. has validation errors) - do nothing.
            }
        }

        /// <summary>
        /// React on selection changing.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Selection changing event args.</param>
        private void _SelectionChanging(object sender, DataGridSelectionChangingEventArgs e)
        {
            NotifyCollectionChangedEventArgs args =
                CommonHelpers.GetSelectionChangedArgsFromGrid(e.SelectionInfos);

            _previousSelectedIndex = null;
            if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                // Save preremoving selection.
                _previousSelectedIndex = SelectedIndex;
            }
        }

        /// <summary>
        /// React on items collection changed.
        /// </summary>
        /// <param name="sender">Items collection.</param>
        /// <param name="e">Collection changed event args.</param>
        private void _CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove &&
                _previousSelectedIndex != null)
            {
                if (Items.Count > 0)
                {
                    // Calculate new selected index.
                    int newSelectedIndex = Items.Count - 1;
                    if (Items.Count > _previousSelectedIndex.Value)
                    {
                        newSelectedIndex = _previousSelectedIndex.Value;
                    }

                    // Make postponed call to select item, which goes after deleted.
                    Dispatcher.BeginInvoke(new ParamsDelegate(_SelectItem),
                                           DispatcherPriority.Input,
                                           Items[newSelectedIndex]);
                }
            }
        }

        /// <summary>
        /// Scrolls grid content if necessary.
        /// </summary>
        /// <param name="yPos">Dragged object position.</param>
        /// <param name="scrollViewer">Scroll viewer.</param>
        private void _DoAutoScroll(double yPos, ScrollViewer scrollViewer)
        {
            double bottomTolerance = BOTTOM_TOLERANCE;
            double topTolerance = _rowHeight * 2;

            Debug.Assert(_rowHeight != 0); // Must be initialized.

            if (scrollViewer == null)
                return;

            if (yPos < topTolerance) // Top of visible list?
            {
                // Scroll up.
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - _rowHeight);
            }
            else if (yPos > this.ActualHeight - bottomTolerance) //Bottom of visible list?
            {
                //Scroll down.
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + _rowHeight);
            }
        }

        /// <summary>
        /// Returns scroll viewer from object's visual tree.
        /// </summary>
        /// <param name="obj">Object where need to found scrollViewer.</param>
        /// <returns>ScrollViewer.</returns>
        private ScrollViewer _GetScrollViewer(DependencyObject obj)
        {
            // Search immediate children first (breadth-first)
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is ScrollViewer)
                    return (ScrollViewer)child;

                else
                {
                    ScrollViewer childOfChild = _GetScrollViewer(child);

                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        /// <summary>
        /// Indicates if Shift or Ctrl is pressed (Left or Right).
        /// </summary>
        /// <returns>True if shift or ctrl is pressed or false otherwise.</returns>
        private bool _IsShiftOrCtrlPressed()
        {
            return ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) > 0) ||
                   ((Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) > 0) ||
                   ((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) > 0) ||
                   ((Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) > 0);
        }

        /// <summary>
        /// Sets Insertion row's background in editing state.
        /// </summary>
        private void _SetEditingInsertionRowBackground()
        {
            Debug.Assert(_insertionRow != null);
            _insertionRow.Background = (Brush)App.Current.FindResource(EDITED_INSERTION_BACKGROUND);
        }

        /// <summary>
        /// Sets Insertion row's default background.
        /// </summary>
        private void _SetDefaultInsertionRowBackground()
        {
            Debug.Assert(_insertionRow != null);
            _insertionRow.Background = _insertionRowDefaultBackground;
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Is cell editor displayed property name.
        /// </summary>
        private const string IS_CELL_EDITOR_DISPLAYED_PROPERTY_NAME = "IsCellEditorDisplayed";

        /// <summary>
        /// Default row height resource name.
        /// </summary>
        private const string DEFAULT_ROW_HEIGHT = "XceedRowDefaultHeight";

        /// <summary>
        /// Space between bottom control's border and mouse position during Drag'n'Drop when
        /// auto-scroll should be started.
        /// </summary>
        private const double BOTTOM_TOLERANCE = 10;

        /// <summary>
        /// Key which pressing causes editing start.
        /// </summary>
        private const Key START_EDITING_KEY_GESTURE = Key.F2;

        /// <summary>
        /// Edited InsertionROw background resource name.
        /// </summary>
        private const string EDITED_INSERTION_BACKGROUND = "EditedInsertionRowBackgroundBrush";

        /// <summary>
        /// Time interval before bring item into view.
        /// </summary>
        private const int BRING_ITEM_INTO_VIEW_TIME_INTERVAL = 500;

        #endregion

        #region Private Fields

        /// <summary>
        /// Bool flag shows should we use custom or default selection logic.
        /// </summary>
        private bool _useCustomSelectionLogic;

        /// <summary>
        /// Saved selected items collection.
        /// </summary>
        private Collection<Object> _savedSelection = new Collection<Object>();

        /// <summary>
        /// Single selected item.
        /// </summary>
        private Object _singleSelection;

        /// <summary>
        /// Flag shows whether control support drag'n'drop of multiple items.
        /// </summary>
        private bool _multipleItemsDragSupport;

        /// <summary>
        /// Flag shows whether editing should be started.
        /// </summary>
        private bool _skipStartEditingOnMouseUp;

        /// <summary>
        /// Control's insertion row.
        /// </summary>
        private InsertionRow _insertionRow;

        /// <summary>
        /// Previous selected item index.
        /// </summary>
        private int? _previousSelectedIndex;

        /// <summary>
        /// Default row height.
        /// </summary>
        private double _rowHeight;

        /// <summary>
        /// Default background of insertion row.
        /// </summary>
        private Brush _insertionRowDefaultBackground;

        /// <summary>
        /// Timer to bring row item into view if it is half visible.
        /// </summary>
        private DispatcherTimer _bringItemIntoViewTimer;

        /// <summary>
        /// Row to bring into view.
        /// </summary>
        private Row _currentRow;

        #endregion
    }
}
