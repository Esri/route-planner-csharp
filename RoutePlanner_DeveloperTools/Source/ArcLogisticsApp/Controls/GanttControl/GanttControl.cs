using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Data;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Delegate for use in callback from gannt control's container for generate tooltip.
    /// </summary>
    /// <param name="hoveredObject">Object under mouse cursor.</param>
    /// <returns>Object's tooltip.</returns>
    internal delegate object GetTooltipDelegate(object hoveredObject);

    /// <summary>
    /// Control's parts.
    /// </summary>
    [TemplatePart(Name = GANTT_ITEMS_ELEMENT_CONTAINER_NAME, Type = typeof(GanttItemElementsContainer))]
    [TemplatePart(Name = SCROLL_VIEWER_NAME, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = GANTT_ITEMS_ELEMENT_CONTAINER_GRID, Type = typeof(Grid))]
    [TemplatePart(Name = TIME_RANGES_PANEL, Type = typeof(GanttTimeRangesPanel))]
    [TemplatePart(Name = TIMELINE_PANEL, Type = typeof(GanttTimeLinePanel))]
    [TemplatePart(Name = TIMELINE_SCROLLER, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = GANTT_ITEMS_LIST, Type = typeof(ListBox))]
    [TemplatePart(Name = GANTT_ITEMS_LIST_SCROLLER, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = GANTT_ALTERNATE_ROW_PANEL, Type = typeof(ListBox))]
    [TemplatePart(Name = GANTT_GLYPH_PANEL, Type = typeof(GanttGlyphPanel))]

    /// <summary>
    /// Class that represents Gantt Control.
    /// </summary>
    internal class GanttControl : Control
    {
        #region Constructors

        /// <summary>
        /// Default constructor - applies control template to control.
        /// </summary>
        public GanttControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GanttControl), new FrameworkPropertyMetadata(typeof(GanttControl)));

            _RefreshTimeRange();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Raises when selection changes.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Raises when drag'n'drop items was started.
        /// </summary>
        public event EventHandler GanttItemElementDragging;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets callback function to create tooltip content.
        /// </summary>
        public IGetTooltipCallback GetTooltipCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Gets collection of gantt items.
        /// </summary>
        public ReadOnlyCollection<IGanttItem> GanttItems
        {
            get { return new List<IGanttItem>(_ganttItems).AsReadOnly(); }
        }

        /// <summary>
        /// Gets collection of selected gantt item elements.
        /// </summary>
        public ReadOnlyCollection<IGanttItemElement> SelectedGanttItemElements
        {
            get { return new List<IGanttItemElement>(_selectedGanttItemElements).AsReadOnly(); }
        }

        #endregion

        #region Public Methods For Working With Collections

        /// <summary>
        /// Adds gantt item.
        /// </summary>
        /// <param name="item">Gantt item to add.</param>
        public void AddGanttItem(IGanttItem item)
        {
            Debug.Assert(item != null);
            Debug.Assert(_ganttItems != null); // Must be initialized.

            // Adds gantt item to the collection.
            _ganttItems.Add(item);

            _RefreshTimeRange();

            // Subscribe on time range changed event.
            item.TimeRangeChanged += new EventHandler(_GanttItemTimeRangeChanged);
        }

        /// <summary>
        /// Removes all gantt items.
        /// </summary>
        public void RemoveAllGanttItems()
        {
            // Unsubscribe from all time range changed events.
            foreach (IGanttItem item in _ganttItems)
                item.TimeRangeChanged -= _GanttItemTimeRangeChanged;

            _ganttItems.Clear();

            // Update Layout using begin invoke.
            _StartLayoutUpdating();
        }

        /// <summary>
        /// Method searches gantt item elements and tries to find one by tag.
        /// </summary>
        /// <param name="tag">Tag to search.</param>
        /// <returns>Returns first found element or null if nothing found.</returns>
        public IGanttItem FindFirstGanttItemByTag(object tag)
        {
            IGanttItem foungItem = null;

            foreach (IGanttItem ganttItem in GanttItems)
            {
                if (ganttItem.Tag.Equals(tag))
                {
                    foungItem = ganttItem;
                    break;
                }
            }

            return foungItem;
        }

        #endregion

        #region Public Selection Methods

        /// <summary>
        /// Selects input elements on gantt diagram.
        /// </summary>
        /// <param name="elements">Elements to select.</param>
        public void Select(IEnumerable<IGanttItemElement> elements)
        {
            _needZoomToSelection = true;

            if (_itemElementsContainer != null)
                _itemElementsContainer.Select(elements);
        }

        /// <summary>
        /// All gantt item elements become unselected.
        /// </summary>
        public void ClearSelection()
        {
            _itemElementsContainer.ClearSelection();
        }

        /// <summary>
        /// Method determines what object is located under point.
        /// </summary>
        /// <param name="pt">Input point for test (in gantt control dependent coordinates).</param>
        /// <returns>Returns IGanttItem instance if gantt item presentation is under point.
        /// Returns IGanttItemElement instance if gantt item element presentation is under point.
        /// Returns null if nor gantt item nor gantt item element is found under point.</returns>
        public object HitTest(Point pt)
        {
            Point scrollViewerPoint = new Point(pt.X - _ganttItemsListScroller.ActualWidth, pt.Y - _timelineScroller.ActualHeight);

            object element = null;

            if (scrollViewerPoint.X >= 0 && scrollViewerPoint.Y >= 0)
            {
                Point elementsContainerPoint = new Point(scrollViewerPoint.X + _scrollViewer.HorizontalOffset - COMMON_SCROLL_VIEWER_CONTENT_MARGINS, scrollViewerPoint.Y + _scrollViewer.VerticalOffset);
                // Call same method from container of gantt elemnts.
                element = _itemElementsContainer.HitTest(elementsContainerPoint);
            }

            // If x point less than zero - point is located on ListBox with routes names.
            else if (scrollViewerPoint.X < 0)
            {
                HitTestResult result = VisualTreeHelper.HitTest(this, pt);

                if (result != null)
                    element = XceedVisualTreeHelper.FindParent<ListBoxItem>(result.VisualHit);

                if (element != null)
                    element = ((ListBoxItem)element).Content; 
            }

            return element;
        }

        #endregion

        #region Overrided Methods

        /// <summary>
        /// Method loads control template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Inits all control's parts used in code.
            _InitControlParts();

            // Inits all necessary event handlers.
            _InitEventHandlers();

            _ganttItemsList.ItemsSource = _ganttItems;
            _alternateRowsPanel.ItemsSource = _ganttItems;

            _isControlInitialized = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Inits control's parts by template.
        /// </summary>
        private void _InitControlParts()
        {
            GanttGlyphPanel glyphPanel = this.GetTemplateChild(GANTT_GLYPH_PANEL) as GanttGlyphPanel;
            Debug.Assert(glyphPanel != null);

            // Define container of gantt elements by control's template.
            _itemElementsContainer = this.GetTemplateChild(GANTT_ITEMS_ELEMENT_CONTAINER_NAME) as GanttItemElementsContainer;

            Debug.Assert(_itemElementsContainer != null);
            Debug.Assert(GanttItems != null); // Must be initialized.

            // Initialize elements container.
            _itemElementsContainer.Initialize(_ganttItems, glyphPanel);

            // Define scroll viewer.
            _scrollViewer = this.GetTemplateChild(SCROLL_VIEWER_NAME) as ScrollViewer;

            Debug.Assert(_scrollViewer != null);

            // Define items container parent grid.
            _itemsContainerGrid = GetTemplateChild(GANTT_ITEMS_ELEMENT_CONTAINER_GRID) as Grid;

            Debug.Assert(_itemsContainerGrid != null);

            // Define time ranges panel.
            _timeRangesPanel = this.GetTemplateChild(TIME_RANGES_PANEL) as GanttTimeRangesPanel;

            Debug.Assert(_timeRangesPanel != null);

            // Define timeline.
            _timeline = this.GetTemplateChild(TIMELINE_PANEL) as GanttTimeLinePanel;

            Debug.Assert(_timeline != null);

            // Define timeline scroll viewer.
            _timelineScroller = this.GetTemplateChild(TIMELINE_SCROLLER) as ScrollViewer;

            Debug.Assert(_timelineScroller != null);

            // Define list view with gantt items names.
            _ganttItemsList = this.GetTemplateChild(GANTT_ITEMS_LIST) as ListBox;

            Debug.Assert(_ganttItemsList != null);

            // Define scroll viewer with gantt items names.
            _ganttItemsListScroller = this.GetTemplateChild(GANTT_ITEMS_LIST_SCROLLER) as ScrollViewer;

            Debug.Assert(_ganttItemsListScroller != null);

            // Initialize alternate rows panel.
            _alternateRowsPanel = this.GetTemplateChild(GANTT_ALTERNATE_ROW_PANEL) as ListBox;

            Debug.Assert(_alternateRowsPanel != null);

            // Define item height.
            _itemHeight = GanttControlHelper.ItemHeight;
        }

        /// <summary>
        /// Creates handlers for all necessary events.
        /// </summary>
        private void _InitEventHandlers()
        {
            Debug.Assert(_scrollViewer != null);

            // Add handler to mouse move for support control panning.
            _scrollViewer.MouseMove += new MouseEventHandler(_ScrollViewerMouseMove);

            // Add handler to size changed event to define new control size.
            this.SizeChanged += new SizeChangedEventHandler(_SizeChanged);

            this.MouseLeave += new MouseEventHandler(_MouseLeave);

            _scrollViewer.PreviewMouseWheel += new MouseWheelEventHandler(_MouseWheel);

            _scrollViewer.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(_MouseLeftButtonUp);

            _scrollViewer.ScrollChanged += new ScrollChangedEventHandler(_ScrollChanged);

            Debug.Assert(_itemElementsContainer != null);

            _itemElementsContainer.SelectionChanged += new EventHandler(_ItemElementsContainerSelectionChanged);
            _itemElementsContainer.DragItemsStarted += new EventHandler(_DragItemsStarted);

            this.Loaded += new RoutedEventHandler(_Loaded);
            this.MouseMove += new MouseEventHandler(_MouseMove);

            Debug.Assert(_ganttItemsList != null);

            _ganttItemsList.SelectionChanged += new SelectionChangedEventHandler(_GanttItemsListSelectionChanged);

            Debug.Assert(_itemsContainerGrid != null);
            _itemsContainerGrid.MouseLeftButtonDown += new MouseButtonEventHandler(_ScrollViewerMouseLeftButtonDown);
        }

        /// <summary>
        /// Nethod calls UdateLayout by BeginInvoke.
        /// </summary>
        private void _StartLayoutUpdating()
        {
            // Don't make such calls if the flag is true and this method is called again
            if (_needToUpdateLayout)
                return;

            // Mark that layout needs to be updated.
            _needToUpdateLayout = true;

            // Refresh all the dependent components (time lines, gantt diagram, etc) using BeginInvoke, since changes can be complex and we don't want to update layout on each one.
            this.Dispatcher.BeginInvoke(new _NoParamsDelegate(_UpdateControlsLayout), DispatcherPriority.Render);
        }

        /// <summary>
        /// No-params delegate for update layout by BeginInvoke.
        /// </summary>
        private delegate void _NoParamsDelegate();

        /// <summary>
        /// Updates gantt digram, time lines control layout.
        /// </summary>
        /// <remarks>
        /// Used in BeginInvoke.
        /// </remarks>
        private void _UpdateControlsLayout()
        {
            if (!_isControlInitialized)
                return;

            Debug.Assert(_startTime != null); // Must be initialized.
            Debug.Assert(_endTime != null); // Must be initialized.

            Size newSize = _GetItemsContainerSize();

            // Calculate new control's layout.
            _itemElementsContainer.UpdateLayout(_startTime, _endTime, newSize);
            _timeRangesPanel.UpdateLayout(_startTime, _endTime, newSize);
            _timeline.UpdateLayout(_startTime, _endTime, newSize);

            _needToUpdateLayout = false;
        }

        /// <summary>
        /// Calculates new items container sze.
        /// </summary>
        /// <returns>New size of items container.</returns>
        private Size _GetItemsContainerSize()
        {
            Size itemsContainerSize = new Size(0, 0); // Size = 0 by default.

            itemsContainerSize.Width = _scrollViewer.ActualWidth - COMMON_SCROLL_VIEWER_CONTENT_MARGINS;
            itemsContainerSize.Height = Math.Max(_scrollViewer.ActualHeight - COMMON_SCROLL_VIEWER_CONTENT_MARGINS, _itemHeight * _ganttItems.Count);

            double scale = 1.0;

            double durationInHours = Math.Floor((_endTime - _startTime).TotalHours);

            // If duration in hours is positive - define scale.
            if (durationInHours > 0)
            {
                // Prevent dividing by zero.
                int hoursInView = (_hoursInView == 0) ? DEFAULT_HOURS_IN_VIEW : _hoursInView;

                int timeRange = _GetTimeRangeInHours();

                scale = timeRange / hoursInView;
            }

            itemsContainerSize.Width = itemsContainerSize.Width * scale;

            return itemsContainerSize;
        }

        /// <summary>
        /// Refresh control bounds and update layout if necessary.
        /// </summary>
        private void _RefreshTimeRange()
        {
            if (_RefreshControlBounds())
            {
                _hoursInView = _GetTimeRangeInHours();

                _StartLayoutUpdating();
            }
        }

        /// <summary>
        /// Defines control's start and end dates.
        /// </summary>
        private bool _RefreshControlBounds()
        {
            Debug.Assert(GanttItems != null); // Must be initialized.

            DateTime newStartTime = DateTime.MaxValue;
            DateTime newEndTime = DateTime.MinValue;

            // Define new start and end control dates.
            _DefineNewControlBounds(out newStartTime, out newEndTime);

            Debug.Assert(newStartTime != DateTime.MaxValue); // Must be changed.
            Debug.Assert(newEndTime != DateTime.MinValue); // Must be changed.

            bool isControlBoundsChanged = false;

            // Compare new bounds with old and replace ones if necessary.
            if (newStartTime != _startTime)
            {
                _startTime = newStartTime;
                isControlBoundsChanged = true;
            }
            if (newEndTime != _endTime)
            {
                _endTime = newEndTime;
                isControlBoundsChanged = true;
            }

            return isControlBoundsChanged;
        }

        /// <summary>
        /// Gets time range in hours.
        /// </summary>
        /// <returns>Time range in hours.</returns>
        private int _GetTimeRangeInHours()
        {
            return Convert.ToInt32(Math.Floor((_endTime - _startTime).TotalHours));
        }

        /// <summary>
        /// Defines control's start and end time bounds.
        /// </summary>
        /// <param name="newStartTime">New start time.</param>
        /// <param name="newEndTime">New end time.</param>
        private void _DefineNewControlBounds(out DateTime newStartTime, out DateTime newEndTime)
        {
            Debug.Assert(GanttItems != null); // Must be initialized.

            DateTime startTime = DateTime.MaxValue;
            DateTime endTime = DateTime.MinValue;

            // Define start and end control dates.
            foreach (IGanttItem ganttItem in GanttItems)
            {
                if (startTime > ganttItem.StartTime && ganttItem.StartTime != DateTime.MinValue)
                    startTime = ganttItem.StartTime;

                if (endTime < ganttItem.EndTime && ganttItem.EndTime != DateTime.MaxValue)
                    endTime = ganttItem.EndTime;
            }

            // Start time - it's found min value approximated to hour value (without minutes and seconds).
            startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);

            // End time - it's found max date value enlarged at 1 hour.
            endTime = new DateTime(endTime.Year, endTime.Month, endTime.Day, endTime.Hour, 0, 0);
            endTime = endTime.AddHours(1);

            // If init control bounds were not changed - define default start time as Min DateTime and control's duration equals 12 hours.
            if (startTime > endTime || GanttItems.Count == 0)
            {
                startTime = DateTime.MinValue;
                endTime = new DateTime(DateTime.MinValue.Ticks + DEFAULT_DURATION_FOR_EMPTY_STATE * TimeSpan.TicksPerHour);
            }

            newStartTime = startTime;
            newEndTime = endTime;
        }

        /// <summary>
        /// Clears selection.
        /// </summary>
        /// <remarks>
        /// WORKAROUND : Wpf controls don't support "MouseClick" event.
        /// We need to clear selection only if user didn't move mouse - just done click. 
        /// Therefore we have to calculate difference between scroll bar positions in 2 moments : when mouse was pressed and when mouse was released.
        /// </remarks>
        private void _ClearSelection()
        {
            // Position of item elements container scroll bars after user release mouse. 
            Point stopPanningScrollBarsPosition = new Point(_scrollViewer.HorizontalOffset, _scrollViewer.VerticalOffset);

            double horisontalDrag = Math.Abs(stopPanningScrollBarsPosition.X - _startPanningScrollBarsPosition.X);
            double verticalDrag = Math.Abs(stopPanningScrollBarsPosition.Y - _startPanningScrollBarsPosition.Y);

            // If mouse was moved less than 2 pixels by both points - clear selection.
            if (horisontalDrag < MIN_PANNING_DISTANCE && verticalDrag < MIN_PANNING_DISTANCE)
            {
                // Clear gantt items data selection.
                _itemElementsContainer.ClearSelection();

                // Hide tooltip.
                _HideTooltip();

                // Raise "SelectionChanged" event.
                if (SelectionChanged != null)
                    SelectionChanged(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Private Tooltip Methods

        /// <summary>
        /// Returns tag from hovered object.
        /// </summary>
        /// <param name="mousePoint">Mouse point on gantt control.</param>
        /// <returns>Tag from hovered object.</returns>
        private object _GetHoveredObject(Point mousePoint)
        {
            object hoveredObject = null;
            object hoveredElement = HitTest(mousePoint);

            // If hovered element is null - update saved hovered element and return null.
            if (hoveredElement == null)
                return null;

            // Define tag of hovered element and return it. 
            if (hoveredElement is IGanttItem)
                hoveredObject = ((IGanttItem)hoveredElement).Tag;
            else if (hoveredElement is IGanttItemElement)
                hoveredObject = ((IGanttItemElement)hoveredElement).Tag;
            else
                return null;

            return hoveredObject;
        }

        /// <summary>
        /// Updates tooltip content and state.
        /// </summary>
        /// <param name="tooltipContent">New tooltip content.</param>
        /// <param name="tooltipPosition">New Tooltip position.</param>
        private void _UpdateToolTip(object hoveredObject, Point tooltipPosition)
        {
            if (_isPanning || hoveredObject == null)
            {
                // Hide tooltip and stop timer.
                _HideTooltip();
                return;
            }

            // Hide tooltip without stop timer.
            if (this.ToolTip != null)
                ((ToolTip)this.ToolTip).IsOpen = false;

            this.ToolTip = _CreateToolTip(tooltipPosition, hoveredObject);

            // Start timer to support delay between tool tips showing.
            _tooltipTimer.Interval = TimeSpan.FromMilliseconds(TOOLTIP_DELAY);
            _tooltipTimer.Tick += new EventHandler(_TooltipTimerTick);
            _tooltipTimer.Start();
        }

        /// <summary>
        /// Hides tooltip and stops timer.
        /// </summary>
        private void _HideTooltip()
        {
            // Stop timer.
            _tooltipTimer.Stop();

            // Hide old ToolTip.
            if (this.ToolTip != null)
                ((ToolTip)this.ToolTip).IsOpen = false;
        }

        /// <summary>
        /// Creates tooltip with necessary context and position.
        /// </summary>
        /// <param name="tooltipPosition">Point for define tooltip position.</param>
        /// <param name="hoveredObject">Object under mouse cursor.</param>
        /// <returns>Tooltip.</returns>
        private ToolTip _CreateToolTip(Point tooltipPosition, object hoveredObject)
        {
            // If property GetTooltipDelegate is not initialized if gantt's container - tooltip will not be shown.
            if (GetTooltipCallback == null)
                return null;

            // Create new ToolTip.
            ToolTip toolTip = new ToolTip();

            // Call callback from gantt's container to define tooltip content.
            toolTip.Content = GetTooltipCallback.GetTooltip(hoveredObject);
            toolTip.PlacementRectangle = new Rect(tooltipPosition.X, tooltipPosition.Y, 0, 0);
            toolTip.IsOpen = false;

            return toolTip;
        }

        #endregion

        #region Private Scale and Panning Methods

        /// <summary>
        /// Sets control mode to "panning".
        /// </summary>
        private void _StartPanning()
        {
            _startPanningPoint = Mouse.GetPosition(_itemElementsContainer);
            _isPanning = true;
            Mouse.Capture(_scrollViewer);

            // WORKAROUND : Wpf controls don't support "MouseClick" event.
            // We need to remember position of scroll bars to use it when user release mouse and reset selection if necessary. 
            _startPanningScrollBarsPosition = new Point(_scrollViewer.HorizontalOffset, _scrollViewer.VerticalOffset);
            Cursor = Cursors.ScrollAll;
        }

        /// <summary>
        /// Pan scroll viewer to necessary point.
        /// </summary>
        private void _Pan()
        {
            Point pt = _CalculateNewGanttPosition(Mouse.GetPosition(_scrollViewer));

            _scrollViewer.ScrollToHorizontalOffset(pt.X);
            _scrollViewer.ScrollToVerticalOffset(pt.Y);
        }

        /// <summary>
        /// Sets control mode to "scrolling".
        /// </summary>
        private void _StopPanning()
        {
            _isPanning = false;
            Mouse.Capture(null);
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Calculates Gantt item elements container position dependence on mouse position during panning.
        /// </summary>
        /// <param name="mousePosition">Mouse position dependent scroll viewer.</param>
        /// <returns>New point position.</returns>
        private Point _CalculateNewGanttPosition(Point mousePosition)
        {
            Point newPiont = new Point();

            // Calculate difference between coordinates of point where panning was started and current mouse position.
            newPiont.X = _startPanningPoint.X - mousePosition.X;
            newPiont.Y = _startPanningPoint.Y - mousePosition.Y;
            return newPiont;
        }

        private void _DefineScale(int delta)
        {
            // Set new scale.
            int durationInHours = Convert.ToInt32(Math.Floor((_endTime - _startTime).TotalHours));

            int hoursDelta = Convert.ToInt32(Math.Ceiling(durationInHours * HOURS_DELTA_MULTIPLIER));

            if (delta < 0) // Scale was decreased - more hours should be visible in view.
                _hoursInView = Math.Min(_hoursInView + hoursDelta, durationInHours);
            else if (delta > 0) // Scale was increased - less hours should be visible in view.
                _hoursInView = Math.Max(_hoursInView - hoursDelta, 1);
        }

        /// <summary>
        /// Gets common bound frame of selected items.
        /// </summary>
        /// <returns>Thickness = selected items borders.</returns>
        /// <param name="leftBoundItemIndex">Index of item with min value of start time.</param>
        /// <param name="rightBoundItemIndex">Index of item with max value of end time.</param>
        private Thickness _GetSelectionBounds(out int leftBoundItemIndex, out int rightBoundItemIndex)
        {
            Debug.Assert(_selectedGanttItemElements != null);
            Debug.Assert(_selectedGanttItemElements.Count > 0); // Even one item must be selected.

            Rect bounds = _itemElementsContainer.GetElementBounds(_selectedGanttItemElements[0]);

            // Define default bounds as bounds of first selected element.
            Thickness newBounds = new Thickness(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);

            int leftItemIndex = 0;
            int rightItemIndex = 0;

            // Find items with min left, max right, min top and max bottom coordinates nd remember coordinates.
            foreach (IGanttItemElement element in _selectedGanttItemElements)
            {
                bounds = _itemElementsContainer.GetElementBounds(element);
                if (bounds.Left < newBounds.Left)
                {
                    newBounds.Left = bounds.Left;
                    leftItemIndex = _selectedGanttItemElements.IndexOf(element); // Save index of item with min left coordinate.
                }
                if (bounds.Right > newBounds.Right)
                {
                    newBounds.Right = bounds.Right;
                    rightItemIndex = _selectedGanttItemElements.IndexOf(element); // Save index of item with max right coordinate.
                }
                if (bounds.Top < newBounds.Top)
                    newBounds.Top = bounds.Top;
                if (bounds.Bottom > newBounds.Bottom)
                    newBounds.Bottom = bounds.Bottom;
            }

            // Define out parameters.
            leftBoundItemIndex = leftItemIndex;
            rightBoundItemIndex = rightItemIndex;

            // Define control - relative bounds.
            newBounds = _GetControlRelativeSelectionBounds(new Rect(newBounds.Left, newBounds.Top, (newBounds.Right - newBounds.Left), (newBounds.Bottom - newBounds.Top)));

            return newBounds;
        }

        /// <summary>
        /// Method brings items into visible frame by scroll scroll viewer. Changes scale if necessary.
        /// </summary>
        private void _BringItemsIntoView(bool needToZoomToSelection)
        {
            int leftBoundItemIndex = 0;
            int rightBoundItemIndex = 0;

            // If selection contains 1 elemnt - zoom should be changed in common case.
            if (_selectedGanttItemElements.Count == 1)
                needToZoomToSelection = true;

            // If selection contains 1 element and selected item is "wideopen" (stretched horisontally by control's bounds) - no need to change zoom.
            // NOTE: "Wideopen" items have time bounds like min amd max date values.
            if (_selectedGanttItemElements.Count == 1 && (_selectedGanttItemElements[0].StartTime == DateTime.MinValue && _selectedGanttItemElements[0].EndTime == DateTime.MaxValue))
                needToZoomToSelection = false;

            Thickness gap = _GetSelectionBounds(out leftBoundItemIndex, out rightBoundItemIndex);

            double newHorisontalOffset = _scrollViewer.HorizontalOffset;
            double newVerticalOffset = _scrollViewer.VerticalOffset;

            // Calculate new horisontal and vertical offsets.
            if (gap.Right < 0)
            {
                gap.Left += gap.Right;
                newHorisontalOffset = newHorisontalOffset - gap.Right;
                gap.Right = 0;
            }
            if (gap.Bottom < 0)
            {
                gap.Top += gap.Bottom;
                newVerticalOffset = newVerticalOffset - gap.Bottom;
            }
            if (gap.Left < 0)
            {
                gap.Right += gap.Left;
                newHorisontalOffset = newHorisontalOffset + gap.Left;
                gap.Left = 0;
            }
            if (gap.Top < 0)
            {
                gap.Bottom += gap.Top;
                newVerticalOffset = newVerticalOffset + gap.Top;
            }

            // Items container width before zoom.
            double itemsContainerOldWidth = _itemElementsContainer.ActualWidth;

            // Zoom index.
            double zoomIndex = 1;

            // If selection will not be visible into visible frame and input parameter allows zoom to selection - need to zoom container.
            if (gap.Left < 0 || gap.Right < 0 && needToZoomToSelection)
                _ZoomToSelection(leftBoundItemIndex, rightBoundItemIndex, itemsContainerOldWidth, out zoomIndex);

            // Scroll horisontal scroll bar to necessary position taking into account zoom index if input parameter allows it.
            if (newHorisontalOffset != _scrollViewer.HorizontalOffset && needToZoomToSelection)
                _scrollViewer.ScrollToHorizontalOffset(newHorisontalOffset * zoomIndex);

            // Scroll vertical scroll bar to necessary position if input parameter allows it.
            if (newVerticalOffset != _scrollViewer.VerticalOffset && needToZoomToSelection)
                _scrollViewer.ScrollToVerticalOffset(newVerticalOffset);

            _needZoomToSelection = false;
        }

        /// <summary>
        /// Zooms control in order to selection become be visible in scroll viewer.
        /// </summary>
        /// <param name="leftSelectedItemIndexout">Index of left item in selection.</param>
        /// <param name="rightSelectedItemIndex">Index of right item in selection.</param>
        /// <param name="oldItemsContainerWidth">Old width of items container.</param>
        /// <param name="zoomScale">Zoom scale.</param>
        private void _ZoomToSelection(int leftSelectedItemIndexout, int rightSelectedItemIndex, double oldItemsContainerWidth, out double zoomScale)
        {
            DateTime newStartTime = _selectedGanttItemElements[leftSelectedItemIndexout].StartTime;
            DateTime newEndTime = _selectedGanttItemElements[rightSelectedItemIndex].EndTime;

            int newHoursInView = Convert.ToInt32(Math.Floor((newEndTime - newStartTime).TotalHours) + 1); // Define count of hours in view as duration of diagramm in hours.

            // If new count of visible hour is less than old - need to increase scale. 
            if (newHoursInView > _hoursInView)
            {
                _hoursInView = newHoursInView;
                _StartLayoutUpdating();
                zoomScale = _GetItemsContainerSize().Width / oldItemsContainerWidth;
                return;
            }

            zoomScale = 1;
        }

        /// <summary>
        /// Convert selection bounds from items container coordinate system relative coordinates of Gantt control.
        /// Calculates difference in pixels between visible apperture and bound of selection for each side.
        /// If side is visible - difference is positive, otherwise - negative.
        /// </summary>
        /// <param name="bounds">Selection bounds in items container coordinate system.</param>
        /// <returns>Differences between scroll viewer sides and selection bound sides.</returns>
        private Thickness _GetControlRelativeSelectionBounds(Rect bounds)
        {
            Thickness newBounds = new Thickness(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);

            // Define bounds in gantt control coordinates taking into account scroll bars positions.
            newBounds.Left = _scrollViewer.Margin.Left - _scrollViewer.HorizontalOffset + newBounds.Left;
            newBounds.Right = _scrollViewer.ActualWidth + _scrollViewer.HorizontalOffset - newBounds.Right;
            newBounds.Top = _scrollViewer.Margin.Top - _scrollViewer.VerticalOffset + newBounds.Top;
            newBounds.Bottom = _scrollViewer.ActualHeight + _scrollViewer.VerticalOffset - newBounds.Bottom;

            return newBounds;
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handler updates gantt control when it's loaded.
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Event args.</param>
        private void _Loaded(object sender, RoutedEventArgs e)
        {
            _needToUpdateLayout = false;
            _StartLayoutUpdating();
        }

        /// <summary>
        /// Updates layout when control's time range chanhed.
        /// </summary>
        /// <param name="sender">Changed gantt item.</param>
        /// <param name="e">Event args.</param>
        private void _GanttItemTimeRangeChanged(object sender, EventArgs e)
        {
            _RefreshTimeRange();
        }

        /// <summary>
        /// Handler contains logic to select gantt elements depending on keyboard state.
        /// </summary>
        /// <param name="sender">Mouse button sender.</param>
        /// <param name="e">Event args.</param>
        private void _ScrollViewerMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get clicked element.
            IGanttItemElement ganttItemElement = this.HitTest(Mouse.GetPosition(this)) as IGanttItemElement;

            if (ganttItemElement == null) // If no GanttItemElement found under mouse cursor - just clear selection..
            {
                // Set control to panning mode.
                _StartPanning();
                return;
            }

            e.Handled = false;
        }

        /// <summary>
        /// Start control panning/drag'n'drop if necessary.
        /// </summary>
        /// <param name="sender">Scroll viewer.</param>
        /// <param name="e">Event args.</param>
        private void _ScrollViewerMouseMove(object sender, MouseEventArgs e)
        {
            if (_isPanning)
            {
                _HideTooltip();
                _Pan();
            }
        }

        /// <summary>
        /// Updates tooltip.
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Event args.</param>
        private void _MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = Mouse.GetPosition(this);

            // Get object under mouse cursor.
            object hoveredObject = _GetHoveredObject(mousePoint);

            // Update tooltip if necessary.
            if (hoveredObject == null || hoveredObject != _oldHoveredObject)
            {
                _UpdateToolTip(hoveredObject, mousePoint);
                _oldHoveredObject = hoveredObject;
            }
        }

        /// <summary>
        /// Switches control's mode from pan to scroll.
        /// </summary>
        /// <param name="sender">Scroll viewer.</param>
        /// <param name="e">Event args.</param>
        private void _MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                _StopPanning();

                if (HitTest(e.GetPosition(this)) != null || _itemElementsContainer.SelectedGanttItemElements.Count == 0)
                    return;

                _ClearSelection();
            }
        }

        /// <summary>
        /// Hides tooltip if it was opened.
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Event args.</param>
        private void _MouseLeave(object sender, MouseEventArgs e)
        {
            _HideTooltip();
        }

        /// <summary>
        /// Handler call method to start updating layout.
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Event args.</param>
        private void _SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _StartLayoutUpdating();
        }

        /// <summary>
        /// Handler zoom control if necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _DefineScale(e.Delta);
            _StartLayoutUpdating();
            e.Handled = true;
        }

        /// <summary>
        /// Handler updates collection of selected items and change its own layout if necessary (brings items into view and change scale).
        /// </summary>
        /// <param name="sender">Item elements container.</param>
        /// <param name="e">Event args.</param>
        private void _ItemElementsContainerSelectionChanged(object sender, EventArgs e)
        {
            // Update selection.
            _selectedGanttItemElements.Clear();
            _selectedGanttItemElements.AddRange(_itemElementsContainer.SelectedGanttItemElements);

            if (_selectedGanttItemElements.Count > 0)
                _BringItemsIntoView(_needZoomToSelection);

            if (_needToUpdateNamesListSelection)
                _UpdateNamesListSelection();

            _HideTooltip();

            // Raise "SelectionChanged" event.
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Updates selection in list with GanttItem names when selection changes in elements container.
        /// </summary>
        private void _UpdateNamesListSelection()
        {
            _ganttItemsList.SelectionChanged -= _GanttItemsListSelectionChanged;

            _ganttItemsList.SelectedItems.Clear();
            _alternateRowsPanel.SelectedItems.Clear();

            foreach (IGanttItemElement element in _selectedGanttItemElements)
            {
                if (element.Tag == element.ParentGanttItem.Tag && !_ganttItemsList.SelectedItems.Contains(element.ParentGanttItem))
                {
                    _ganttItemsList.SelectedItems.Add(element.ParentGanttItem);
                    _alternateRowsPanel.SelectedItems.Add(element.ParentGanttItem);
                }
            }

            _ganttItemsList.SelectionChanged += new SelectionChangedEventHandler(_GanttItemsListSelectionChanged);
        }

        /// <summary>
        /// Updates selcction in elements container when selection changes in list with GanttItem names.
        /// </summary>
        /// <param name="sender">GanttItem names.</param>
        /// <param name="e">Event args.</param>
        private void _GanttItemsListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ganttItemsList.SelectedIndex == -1)
                return;

            _alternateRowsPanel.SelectedItems.Clear();

            // Get collection of selected elements.
            Collection<IGanttItemElement> selectedElements = new Collection<IGanttItemElement>();

            foreach (IGanttItem item in _ganttItemsList.SelectedItems)
            {
                foreach (IGanttItemElement element in item.GanttItemElements)
                {
                    _alternateRowsPanel.SelectedItems.Add(item);

                    if (element.Tag == item.Tag && !selectedElements.Contains(element))
                        selectedElements.Add(element);
                }
            }

            _needToUpdateNamesListSelection = false;

            _itemElementsContainer.Select(selectedElements);

            _needToUpdateNamesListSelection = true;
        }

        /// <summary>
        /// Handler scrolls timeline to necessaru offset.
        /// </summary>
        /// <param name="sender">Main scroll viewer.</param>
        /// <param name="e">Scroll changed event args.</param>
        private void _ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            _timelineScroller.ScrollToHorizontalOffset(e.HorizontalOffset);
            _ganttItemsListScroller.ScrollToVerticalOffset(e.VerticalOffset);
        }

        /// <summary>
        /// Raises event about Drag items started when drag started in container.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _DragItemsStarted(object sender, EventArgs e)
        {
            if (GanttItemElementDragging != null)
                GanttItemElementDragging(this, EventArgs.Empty);
        }

        /// <summary>
        /// Shows toolTip  with delay.
        /// </summary>
        /// <param name="sender">Timer.</param>
        /// <param name="e">Event args.</param>
        private void _TooltipTimerTick(object sender, EventArgs e)
        {
            // Stop timer.
            _tooltipTimer.Stop();

            // Hide old ToolTip.
            if (this.ToolTip != null)
                ((ToolTip)this.ToolTip).IsOpen = true;
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Gantt items container name.
        /// </summary>
        private const string GANTT_ITEMS_ELEMENT_CONTAINER_NAME = "PART_GanttItemElementsContainer";

        /// <summary>
        /// Scroll viewer container name.
        /// </summary>
        private const string SCROLL_VIEWER_NAME = "PART_GanttScroller";

        /// <summary>
        /// Elements container parent grid name.
        /// </summary>
        private const string GANTT_ITEMS_ELEMENT_CONTAINER_GRID = "PART_GanttItemElementsContainerGrid";

        /// <summary>
        /// Time ranges panel name.
        /// </summary>
        private const string TIME_RANGES_PANEL = "PART_TimeRangesPanel";

        /// <summary>
        /// Timeline.
        /// </summary>
        private const string TIMELINE_PANEL = "PART_TimeLinePanel";

        /// <summary>
        /// Timeline scroller.
        /// </summary>
        private const string TIMELINE_SCROLLER = "PART_TimeLabelsScroller";

        /// <summary>
        /// List with gantt items names.
        /// </summary>
        private const string GANTT_ITEMS_LIST = "PART_GanttItemsList";

        /// <summary>
        /// Scroll viewer with gantt items list.
        /// </summary>
        private const string GANTT_ITEMS_LIST_SCROLLER = "PART_GanttItemsListScroller";

        /// <summary>
        /// Panel with progress icons.
        /// </summary>
        private const string GANTT_PROGRESS_PANEL = "PART_GanttProgressPanel";

        /// <summary>
        /// Gantt alternate row panel name.
        /// </summary>
        private const string GANTT_ALTERNATE_ROW_PANEL = "PART_GanttAlternateRowPanel";

        /// <summary>
        /// Gantt glyph panel name.
        /// </summary>
        private const string GANTT_GLYPH_PANEL = "PART_GanttGlyphPanel";

        /// <summary>
        /// Value on which percent of visible hours should be changed.
        /// </summary>
        private const double HOURS_DELTA_MULTIPLIER = 0.25;

        /// <summary>
        /// Delay between mouse will enter to element and tooltip will be shown.
        /// </summary>
        private const int TOOLTIP_DELAY = 500;

        /// <summary>
        /// 2 pixels - is minimal panning distance to define that selection should be clear when user release mouse button.
        /// </summary>
        private const int MIN_PANNING_DISTANCE = 2;

        /// <summary>
        /// Common scroll viewer content margins.
        /// </summary>
        private const int COMMON_SCROLL_VIEWER_CONTENT_MARGINS = 2;

        /// <summary>
        /// Const default duration for empty state.
        /// </summary>
        private const long DEFAULT_DURATION_FOR_EMPTY_STATE = 12;

        /// <summary>
        /// Default count of hours which is visible on gantt diagram.
        /// </summary>
        private const int DEFAULT_HOURS_IN_VIEW = 12;

        #endregion

        #region Private Fields

        /// <summary>
        /// Collection of gantt items.
        /// </summary>
        private ObservableCollection<IGanttItem> _ganttItems = new ObservableCollection<IGanttItem>();

        /// <summary>
        /// Collection of selected gantt item elements.
        /// </summary>
        private List<IGanttItemElement> _selectedGanttItemElements = new List<IGanttItemElement>();

        /// <summary>
        /// Start time of the gantt diagram. Max available time by default.
        /// </summary>
        private DateTime _startTime = DateTime.MinValue;

        /// <summary>
        /// End time of the gantt diagram. Min available time by default.
        /// </summary>
        private DateTime _endTime = DateTime.MaxValue;

        /// <summary>
        /// Count of hours visible on the gantt diagram.
        /// </summary>
        private int _hoursInView = 0;

        /// <summary>
        /// Gantt elemets container.
        /// </summary>
        private GanttItemElementsContainer _itemElementsContainer;

        /// <summary>
        /// Scroll viewer.
        /// </summary>
        private ScrollViewer _scrollViewer;

        /// <summary>
        /// Panel with vertical time lines.
        /// </summary>
        private GanttTimeRangesPanel _timeRangesPanel;

        /// <summary>
        /// Timeline panel.
        /// </summary>
        private GanttTimeLinePanel _timeline;

        /// <summary>
        /// Timeline scroller.
        /// </summary>
        private ScrollViewer _timelineScroller;

        /// <summary>
        /// Gantt items names list.
        /// </summary>
        private ListBox _ganttItemsList;

        /// <summary>
        /// Scroll viewer with gantt items list.
        /// </summary>
        private ScrollViewer _ganttItemsListScroller;

        /// <summary>
        /// Grid around Items container.
        /// </summary>
        private Grid _itemsContainerGrid;

        /// <summary>
        /// List box with alternate Row style.
        /// </summary>
        private ListBox _alternateRowsPanel;

        /// <summary>
        /// Flag defines whether layout needs to be updated.
        /// </summary>
        private bool _needToUpdateLayout = false;

        /// <summary>
        /// Flag shows whether selection should be zoomed to view.
        /// </summary>
        private bool _needZoomToSelection = false;

        /// <summary>
        /// Flag shows whether control is in panning mode.
        /// </summary>
        private bool _isPanning = false;

        /// <summary>
        /// Point where from panning was started.
        /// </summary>
        private Point _startPanningPoint;

        /// <summary>
        /// Height of gantt item.
        /// </summary>
        private double _itemHeight = 0;

        /// <summary>
        /// Timer to correctly show tool tip.
        /// </summary>
        private DispatcherTimer _tooltipTimer = new DispatcherTimer();

        /// <summary>
        /// Flag define whether we need to update selection in list with gantt item names. 
        /// </summary>
        private bool _needToUpdateNamesListSelection = true;

        /// <summary>
        /// Old mouse hovered object.
        /// </summary>
        private object _oldHoveredObject = null;

        /// <summary>
        /// Scroll bars position when user starts panning.
        /// </summary>
        private Point _startPanningScrollBarsPosition;

        /// <summary>
        /// Flag shows whether control was initialized.
        /// </summary>
        private bool _isControlInitialized = false;

        #endregion
    }
}
