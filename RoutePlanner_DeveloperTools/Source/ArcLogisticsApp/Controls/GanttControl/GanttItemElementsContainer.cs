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
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ESRI.ArcLogistics.App.DragAndDrop;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class represents gantt diagram that consists of gantt item elements.
    /// </summary>
    internal class GanttItemElementsContainer : FrameworkElement
    {
        #region Constructors

        /// <summary>
        /// Constructor. Creates new visual children collection and initialize Row height value.
        /// </summary>
        public GanttItemElementsContainer()
        {
            _children = new VisualCollection(this);

            // Initialize hardcoded row height.
            _rowHeight = GanttControlHelper.ItemHeight;

            this.DragOver += new DragEventHandler(_DragOver);
            this.Drop += new DragEventHandler(_Drop);
            this.DragLeave += new DragEventHandler(_DragLeave);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_MouseLeftButtonDown);
            this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(_PreviewMouseLeftButtonUp);
            this.Loaded += new RoutedEventHandler(_Loaded);
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
        public event EventHandler DragItemsStarted;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets pixels per tick.
        /// </summary>
        public static double PixelsPerTick
        {
            get { return _pixelsPerTick; }
        }

        /// <summary>
        /// Gets collection of selected gantt item elements.
        /// </summary>
        public ReadOnlyCollection<IGanttItemElement> SelectedGanttItemElements
        {
            get
            {
                List<IGanttItemElement> selectedElements = new List<IGanttItemElement>(_selectedElements.Values);
                return selectedElements.AsReadOnly();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes container with gantt items. Container listens on ganttItems collection changes and updates itself.
        /// </summary>
        /// <param name="?"></param>
        public void Initialize(ObservableCollection<IGanttItem> ganttItems, GanttGlyphPanel glyphPanel)
        {
            Debug.Assert(ganttItems != null);

            // Initialize collection of visual childs.
            _CreateVisuals(ganttItems);

            _ganttItems = ganttItems;

            // Initialize Glyph panel.
            _glyphPanel = glyphPanel;

            // Listen events from ganttItems collection: Add, Remove, Reset and handle them adding, removing corresponding visuals.
            ganttItems.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(_GanttItemsCollectionChanged);
        }

        /// <summary>
        /// Updates layout of the control.
        /// </summary>
        /// <param name="startTime">Start control time.</param>
        /// <param name="endTime">End control time.</param>
        /// <param name="controlWidth">Necessary control width.</param>
        public void UpdateLayout(DateTime startTime, DateTime endTime, Size newControlSize)
        {
            _startTime = startTime;
            _endTime = endTime;
            _size = newControlSize;

            // Define how much pixels can be set in one tick.
            double containerDurationInTicks = _GetContainerDurationInTicks();
            double pixelsPerTick = _size.Width / containerDurationInTicks;

            if (_pixelsPerTick != pixelsPerTick)
                _pixelsPerTick = pixelsPerTick;

            // Set RedrawRequired to true for all visuals.
            foreach (GanttItemElementDrawingVisual visual in _children)
                visual.RedrawRequired = true;

            InvalidateMeasure();
            InvalidateVisual();
        }

        /// <summary>
        /// Selects input elements on gantt diagram (copy elements to collection of selected items.).
        /// </summary>
        /// <param name="elements">Elements to select.</param>
        public void Select(IEnumerable<IGanttItemElement> elements)
        {
            // Set "RedrawRequired" status to true in necessary elements.
            _UpdateRedrawRequiredStatusWhenSelectionChanged(elements);

            _selectedElements.Clear();
            foreach (IGanttItemElement element in elements)
                _selectedElements.Add(element, element);

            InvalidateVisual();
            _OnSelectionChanged();
        }

        /// <summary>
        /// All gantt item elements become unselected.
        /// </summary>
        public void ClearSelection()
        {
            foreach (GanttItemElementDrawingVisual visual in _children)
                visual.RedrawRequired = true;

            _selectedElements.Clear();
            InvalidateVisual();
            _OnSelectionChanged();
        }

        /// <summary>
        /// Method determines what object is located under point.
        /// </summary>
        /// <param name="pt">Input point for test.</param>
        /// <returns>Returns IGanttItemElement instance if gantt item element presentation is under point.
        /// Returns null if nor gantt item nor gantt item element is found under point.</returns>
        public object HitTest(Point pt)
        {
            // Find object under necessary point using Visual tree helper.
            HitTestResult foundObject = VisualTreeHelper.HitTest(this, pt);

            if (foundObject == null)
                return null;

            GanttItemElementDrawingVisual foundVisual = foundObject.VisualHit as GanttItemElementDrawingVisual;

            // If found object is not GanttItemElementDrawingVisual - return null.
            if (foundVisual == null)
                return null;

            // If IGanttItemElement was found - return it.
            return foundVisual.GanttItemElement;
        }

        /// <summary>
        /// Retruns bounds of element.
        /// </summary>
        /// <param name="element">IGanttItemElement.</param>
        /// <returns>Element bounds.</returns>
        public Rect GetElementBounds(IGanttItemElement element)
        {
            return _GetChildDrawingArea(element, new GanttItemElementDrawingContext());
        }

        /// <summary>
        /// Updates layout after Drag'n'drop finished.
        /// </summary>
        public void DropGanttItemElements()
        {
            // If there is an element under drag cursor.
            if (_draggedOverVisual != null)
            {
                // Mark dragged over element as needed to be redrawn.
                _draggedOverVisual.RedrawRequired = true;

                // Clear dragged over element.
                _draggedOverVisual = null;

                // Update layout.
                InvalidateVisual();
            }
        }

        #endregion

        #region Protected overriden methods

        /// <summary>
        /// Gets collection of visual children.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return _children.Count;
            }
        }

        /// <summary>
        /// Gets visual child by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override Visual GetVisualChild(int index)
        {
            Debug.Assert(index >= 0 && index < _children.Count);

            return _children[index];
        }

        /// <summary>
        /// Define new size of control.
        /// </summary>
        /// <param name="availableSize">Old size.</param>
        /// <returns>New size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            return _size;
        }

        /// <summary>
        /// Draws all necessary childs.
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // Go through all the drawing visuals and call Draw method for ones that have RedrawRequired == true.
            _DrawVisuals();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Subscribes on Gantt Control and Main Window mouse moves.
        /// </summary>
        private void _SubscribeOnMouseMoves()
        {
            // Subscribe on mouse moves of Main Window.
            Application.Current.MainWindow.MouseMove += new MouseEventHandler(_MouseMove);

            // Find Gantt Control and subscribe on mouse moves. This is necessary because
            // time view can be undocked and gantt control won't belong to the same visual tree as main window.
            Visual element = this;
            while (element != null && !(element is GanttControl))
                element = (Visual)VisualTreeHelper.GetParent(element);

            Debug.Assert(element != null && element is GanttControl);

            (element as FrameworkElement).MouseMove += new MouseEventHandler(_MouseMove);

            _subscribedOnMouseMoves = true;
        }

        /// <summary>
        /// Creates collection of visual from colection of gantt items.
        /// </summary>
        /// <param name="ganttItems">Gantt items collection.</param>
        private void _CreateVisuals(ICollection<IGanttItem> ganttItems)
        {
            Debug.Assert(ganttItems != null);

            foreach (IGanttItem ganttItem in ganttItems)
            {
                foreach (IGanttItemElement element in ganttItem.GanttItemElements)
                {
                    GanttItemElementDrawingVisual visual = new GanttItemElementDrawingVisual(element);

                    // Add visual to children collection.
                    _children.Add(visual);

                    // Add key-value pair to dictionary.
                    _mapElementToVisual.Add(element, visual);
                    element.RedrawRequired += new EventHandler(_ElementRedrawRequired);
                }
            }
        }

        /// <summary>
        /// Calculates drawing area for necessary visual children.
        /// </summary>
        /// <param name="element">Element for which bounds should be calculated.</param>
        /// <returns>New drawing area in relative coordinates.</returns>
        private Rect _GetChildDrawingArea(IGanttItemElement element, GanttItemElementDrawingContext context)
        {
            Debug.Assert(element != null);
            Debug.Assert(element.ParentGanttItem != null);

            // Define Y position of element.
            double yPos = _ganttItems.IndexOf(element.ParentGanttItem) * _rowHeight;

            // Copy start and end time from element to context.
            context.StartTime = element.StartTime;
            context.EndTime = element.EndTime;

            // If elemnts start time or end time is out of 
            if (element.StartTime < _startTime)
                context.StartTime = _startTime;
            if (element.EndTime > _endTime)
                context.EndTime = _endTime;

            // Define element duration in time units.
            TimeSpan elementDuration = context.EndTime - context.StartTime;

            // Define element shift from left container border. We are always make a gap with width = 1 for correctly show elements styles.
            double xPos = Math.Max(1, (context.StartTime - _startTime).Ticks * _pixelsPerTick);

            // Define element duration in pixels.
            double elementWidth = Math.Max(0, Math.Abs(elementDuration.Ticks) * _pixelsPerTick);

            if (elementWidth == 0)
                elementWidth = DEFAULT_ELEMENT_WIDTH;

            // Define element height - it's the same for all elements and equals row height.
            double elementHeight = _rowHeight;

            return new Rect(xPos, yPos, elementWidth, elementHeight);
        }

        /// <summary>
        /// Calculates common container duration in ticks.
        /// </summary>
        /// <returns>Container's duration in ticks.</returns>
        private double _GetContainerDurationInTicks()
        {
            // Define control's duration in hours.
            double hoursDuration = Math.Floor((_endTime - _startTime).TotalHours);
            return hoursDuration * TimeSpan.TicksPerHour;
        }

        /// <summary>
        /// Goes through all the visuals and draw ones which require redraw.
        /// </summary>
        private void _DrawVisuals()
        {
            foreach (GanttItemElementDrawingVisual visual in _children)
            {
                if (visual.RedrawRequired)
                {
                    GanttItemElementDrawingContext context = new GanttItemElementDrawingContext();

                    // Calculate drawing rect for the element.
                    context.DrawingArea = _GetChildDrawingArea(visual.GanttItemElement, context);

                    // Set selected if necessary.
                    context.DrawSelected = (_selectedElements.ContainsValue(visual.GanttItemElement));

                    // Set dragged over if necessay.
                    context.DrawDraggedOver = (_draggedOverVisual != null && _draggedOverVisual == visual);

                    // Set glyph panel.
                    context.GlyphPanel = _glyphPanel;

                    // Set dragged data.
                    if (context.DrawDraggedOver)
                        context.DraggedData = _draggedData;

                    visual.Draw(context);
                }
            }
        }


        /// <summary>
        /// Raises event about selection was changed.
        /// </summary>
        private void _OnSelectionChanged()
        {
            if (SelectionChanged != null)
                SelectionChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds one "row" of visual elements to necessary gantt item.
        /// </summary>
        /// <param name="ganttItem">Gantt item.</param>
        private void _AddVisualsForGanttItem(IGanttItem ganttItem)
        {
            // Define next Gantt item in collection to insert new visuals before it's visuals.
            int indexOfNextGanttItem = _ganttItems.IndexOf(ganttItem) + 1;

            if (indexOfNextGanttItem == _ganttItems.Count) // If new item was added to the end of collection.
            {
                // Add new visuals to the end of collection.
                foreach (IGanttItemElement element in ganttItem.GanttItemElements)
                {
                    GanttItemElementDrawingVisual newVisual = new GanttItemElementDrawingVisual(element);
                    newVisual.RedrawRequired = true;
                    _mapElementToVisual.Add(element, newVisual); // Add pair to dictionary.
                    _children.Add(newVisual);
                    element.RedrawRequired += new EventHandler(_ElementRedrawRequired);
                }
            }
            else
            {
                Debug.Assert(false);
                // TODO : Add code to insert visual to necessary position later if necessary
            }
        }

        /// <summary>
        /// Method removes all visuals from child collection if visual is bound with gantt item. Also removes objects from selection.
        /// </summary>
        /// <param name="ganttItem">Gantt item.</param>
        private void _RemoveVisualsForGanttItem(IGanttItem ganttItem)
        {
            Collection<IGanttItemElement> itemsToRemove = new Collection<IGanttItemElement>();

            foreach (GanttItemElementDrawingVisual visual in _children) //(int i = 0; i < VisualChildrenCount; i++)
            {
                IGanttItemElement element = ((GanttItemElementDrawingVisual)visual).GanttItemElement;

                if (element.ParentGanttItem.Equals(ganttItem))
                    itemsToRemove.Add(element);
            }

            // Remove necessary elements from collection.
            foreach (IGanttItemElement element in itemsToRemove)
            {
                element.RedrawRequired -= _ElementRedrawRequired;
                _children.Remove(_mapElementToVisual[element]);
                _mapElementToVisual.Remove(element);
                _selectedElements.Remove(element);
            }
        }

        /// <summary>
        /// Removes all IGanttItemElements in necessary item.GanttItemElements from selection.
        /// </summary>
        /// <param name="ganttItem">Item whose elements should be removed.</param>
        /// <returns>True if even one element was removed.</returns>
        private bool _RemoveVisualsFromSelectionForGanttItem(IGanttItem ganttItem)
        {
            bool isVisualsWereRemoved = false;

            foreach (IGanttItemElement element in ganttItem.GanttItemElements)
            {
                if (_selectedElements.ContainsValue(element))
                {
                    _selectedElements.Remove(element);
                    isVisualsWereRemoved = true;
                }
            }

            return isVisualsWereRemoved;
        }

        #endregion

        #region Private Selection Methods

        /// <summary>
        /// Selects gantt elements depending on keyboard state.
        /// </summary>
        /// <param name="ganttItemElement">Element to select.</param>
        private void _Select(IGanttItemElement ganttItemElement)
        {
            Debug.Assert(ganttItemElement != null);

            ICollection<IGanttItemElement> newSelection = null;

            // If no modifier keys was pressed on keyboard or selection is empty - use single selection logic.
            if (_selectedElements.Count == 0 || Keyboard.Modifiers == ModifierKeys.None)
            {
                newSelection = _SelectItem(ganttItemElement);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift) // If "Shift" key was pressed.
            {
                newSelection = _SelectItemsByShift(ganttItemElement);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control) // if "Ctrl" key was pressed.
            {
                newSelection = _SelectItemsByCtrl(ganttItemElement);
            }

            // If new selected collection is null - selection should not be changed.
            if (newSelection == null)
                return;

            // Set "RedrawRequired" status to true in necessary elements.
            _UpdateRedrawRequiredStatusWhenSelectionChanged(newSelection);

            // Update collection of selected items
            _selectedElements.Clear();

            foreach (IGanttItemElement element in newSelection)
                _selectedElements.Add(element, element);

            InvalidateVisual();
            _OnSelectionChanged();
        }

        /// <summary>
        /// Selection logic if "Shift" button was pressed in selection.
        /// </summary>
        /// <param name="pressedElement">Last clicked element.</param>
        /// <returns>New selection collection. If selection shouldn't be changed - returns null.</returns>
        private ICollection<IGanttItemElement> _SelectItemsByShift(IGanttItemElement clickedElement)
        {
            Debug.Assert(_selectedElements.Count > 0); // Collection should contain at least one element. Otherwise we use _SelectItem method.

            // If user clicked to element in other GanttItem - do nothing.
            if (_selectedElements.First().Value.ParentGanttItem != clickedElement.ParentGanttItem)
                return null;

            // Else - need to select range between first selected item and clicked item.

            // If just clicked element has the same tag as current GanttItem - 
            // elemet is already in selection (was added earlier by custom logic in _SelectItem() method). And we should do nothing.
            if (_selectedElements.First().Key.ParentGanttItem.Tag == clickedElement.Tag)
                return null;

            // Check that clicked item is the same type as items in collection.
            Type selectedItemsType = _selectedElements.First().Value.Tag.GetType();

            // If new element Tag type is nos same as Tag type of already selected elements - do nothing.
            if (clickedElement.Tag.GetType() != selectedItemsType)
                return null;

            // Rename vars to make code more clear.
            IGanttItemElement firstSelectedElement = _selectedElements.First().Key;
            IGanttItemElement lastSelectedElement = clickedElement;

            IGanttItem parentItem = lastSelectedElement.ParentGanttItem;

            // Defines whether selection bounds are in revert order. 
            bool doSelectionBoundsHaveInvalidOrder = (parentItem.GanttItemElements.IndexOf(lastSelectedElement) < parentItem.GanttItemElements.IndexOf(firstSelectedElement));

            // If index of last selected item parent is smaller than first selected item parent - interchange items.
            if (doSelectionBoundsHaveInvalidOrder)
            {
                IGanttItemElement buffer = firstSelectedElement;
                firstSelectedElement = lastSelectedElement;
                lastSelectedElement = buffer;
            }

            // Collection of all element which should be selected now.
            ICollection<IGanttItemElement> newSelectedElements = new Collection<IGanttItemElement>();

            // Select elements from selection range.
            newSelectedElements = _GetSelectedElementsByGanttItem(selectedItemsType, parentItem,
                firstSelectedElement, lastSelectedElement);

            return newSelectedElements;
        }

        /// <summary>
        /// Selection logic if "Ctrl" button was pressed in selection.
        /// </summary>
        /// <param name="clickedElement">Last clicked element.</param>
        /// <returns>Collection of new selected elements. Null if selection shouldn't be changed.</returns>
        private ICollection<IGanttItemElement> _SelectItemsByCtrl(IGanttItemElement clickedElement)
        {
            Debug.Assert(_selectedElements.Count > 0); // Collection should contain at least one element. Otherwise we use _SelectItem method.

            Collection<IGanttItemElement> oldSelectedElements = new Collection<IGanttItemElement>(_selectedElements.Values.ToList<IGanttItemElement>());

            // Check that clicked item is the same type as items in collection.
            Type selectedItemsType = _selectedElements.First().Key.Tag.GetType();

            // If new element Tag type is nos same as Tag type of already selected elements - do nothing.
            if (clickedElement.Tag.GetType() != selectedItemsType)
                return null;

            Collection<IGanttItemElement> selectedElements = new Collection<IGanttItemElement>();

            // Add clicked element to collection.
            selectedElements.Add(clickedElement);

            // If clicked element has same Tag as parent Gantt Item - select all elements with this tag.
            if (clickedElement.Tag == clickedElement.ParentGanttItem.Tag)
                selectedElements = _GetSelectedElementsWithGanttItemTag(clickedElement.ParentGanttItem);

            // If clicked element is not contains in selected collection - add it (select).
            if (!oldSelectedElements.Contains(clickedElement))
            {
                foreach (IGanttItemElement element in selectedElements)
                    oldSelectedElements.Add(element);
            }

            // Otherwise - remove elements (deselect).
            else
            {
                foreach (IGanttItemElement element in selectedElements)
                    if (oldSelectedElements.Contains(element))
                        oldSelectedElements.Remove(element);
            }

            return oldSelectedElements;
        }

        /// <summary>
        /// Adds item into collection of selected items.
        /// </summary>
        /// <param name="pressedElement"></param>
        /// <returns>Collection of selected elements.</returns>
        private ICollection<IGanttItemElement> _SelectItem(IGanttItemElement ganttItemElement)
        {
            List<IGanttItemElement> newSelection = new List<IGanttItemElement>();

            // If selected element has the same Tag as parent Gantt Item - select all elements with this tag.
            if (ganttItemElement.Tag == ganttItemElement.ParentGanttItem.Tag)
            {
                ICollection<IGanttItemElement> selectedElements = _GetSelectedElementsWithGanttItemTag(ganttItemElement.ParentGanttItem);

                if (selectedElements.Count == 0)
                    return null;

                newSelection.AddRange(selectedElements);
            }
            else
                newSelection.Add(ganttItemElement);

            return newSelection;
        }

        /// <summary>
        /// Gets collection of selected elements in necessary Gantt item.
        /// </summary>
        /// <param name="selectedType">TYpe of elements which can be added to selection.</param>
        /// <param name="ganttItem">Parent gantt item.</param>
        /// <param name="firstSelectedElement">First element in global selection.</param>
        /// <param name="lastSelectedElement">Last element in global selection.</param>
        /// <returns>Collection of selected elements.</returns>
        private ICollection<IGanttItemElement> _GetSelectedElementsByGanttItem(Type selectedType, IGanttItem ganttItem,
            IGanttItemElement firstSelectedElement, IGanttItemElement lastSelectedElement)
        {
            Collection<IGanttItemElement> selectedElements = new Collection<IGanttItemElement>();

            int startIndex = 0;
            int endIndex = ganttItem.GanttItemElements.Count;

            if (ganttItem.GanttItemElements.Contains(firstSelectedElement))
                startIndex = ganttItem.GanttItemElements.IndexOf(firstSelectedElement);
            if (ganttItem.GanttItemElements.Contains(lastSelectedElement))
                endIndex = ganttItem.GanttItemElements.IndexOf(lastSelectedElement) + 1;

            for (int i = startIndex; i < endIndex; i++)
            {
                if (ganttItem.GanttItemElements[i].Tag.GetType() == selectedType)
                    selectedElements.Add(ganttItem.GanttItemElements[i]);
            }

            return selectedElements;
        }

        /// <summary>
        /// Gets collection of elements which Tag is the same with Gantt item tag.
        /// </summary>
        /// <param name="ganttItem">Gantt item.</param>
        /// <returns>Collection of IganttItemElements with tags same to gantt item tag.</returns>
        private Collection<IGanttItemElement> _GetSelectedElementsWithGanttItemTag(IGanttItem ganttItem)
        {
            Collection<IGanttItemElement> selectedElements = new Collection<IGanttItemElement>();

            foreach (IGanttItemElement element in ganttItem.GanttItemElements)
            {
                if (element.Tag == ganttItem.Tag)
                    selectedElements.Add(element);
            }

            return selectedElements;
        }

        /// <summary>
        /// Compares collection of current and new selection and updates "RedrawRequired" status if necessary.
        /// </summary>
        /// <param name="newSelection">Collection of new selected items.</param>
        private void _UpdateRedrawRequiredStatusWhenSelectionChanged(IEnumerable<IGanttItemElement> newSelection)
        {
            // Define which visuals should be redraw (they are contains only in one collection - either in old selection or in new selection).
            foreach (GanttItemElementDrawingVisual visual in _children)
            {
                bool isGanttItemElementContainsInOldSelection = _selectedElements.ContainsKey(visual.GanttItemElement);
                bool isGanttItemElementContainsInNewSelection = newSelection.Contains(visual.GanttItemElement);

                // If contains only in one collection - it's selection state was changed and we need to redraw item.
                if (isGanttItemElementContainsInNewSelection != isGanttItemElementContainsInOldSelection)
                    visual.RedrawRequired = true;
            }
        }

        #endregion

        #region Private Drag'n'Drop methods

        /// <summary>
        /// Method starts dragging objects from selection - create collection of dragged objects and add it to Drag'n'drop event args.
        /// </summary>
        private void _StartDragging()
        {
            if (DragItemsStarted != null)
                DragItemsStarted(this, EventArgs.Empty);

            _mustStartDraggingOnMouseMove = false; // Set flag to false to not raise event again when mouse move.
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handler defines type of changes in collection and do necessary actions.
        /// </summary>
        /// <param name="sender">Collection of gantt items.</param>
        /// <param name="e">Collection changed event args.</param>
        private void _GanttItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Collection<IGanttItem> ganttItems = sender as Collection<IGanttItem>;

            Debug.Assert(ganttItems != null);

            bool isSelectionChanged = false;

            // Make quick check - if ganttItems collection is empty - just call _children.Clear for fast removing.
            if (ganttItems.Count == 0)
            {
                _children.Clear();

                if (_selectedElements.Count > 0)
                {
                    _selectedElements.Clear();
                    isSelectionChanged = true;
                }

                _mapElementToVisual.Clear();
            }

            // If action type is "Remove" - remove all redundant items.
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (IGanttItem item in e.OldItems)
                {
                    _RemoveVisualsForGanttItem(item);

                    // If items was removed from selection even in one gantt item - set flag to true.
                    if (_RemoveVisualsFromSelectionForGanttItem(item))
                        isSelectionChanged = true;
                }
            }

            // If action type is "Add" - add new visuals to necessary positions.
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (IGanttItem item in e.NewItems)
                    _AddVisualsForGanttItem(item);
            }

            // If changes affect selection - notify about selection changed.
            if (isSelectionChanged)
                _OnSelectionChanged();
        }

        /// <summary>
        /// Redraws element when it necessary.
        /// </summary>
        /// <param name="sender">Element to redraw.</param>
        /// <param name="e">Event args.</param>
        private void _ElementRedrawRequired(object sender, EventArgs e)
        {
            IGanttItemElement element = sender as IGanttItemElement;

            Debug.Assert(element != null);

            GanttItemElementDrawingVisual visual = null;

            if (_mapElementToVisual.TryGetValue(element, out visual))
                visual.RedrawRequired = true;

            InvalidateVisual();
        }

        /// <summary>
        /// Handler of DragOver event: defines element under drag cursor and initiates Update Layout to highlight it.
        /// </summary>
        /// <param name="sender">Container.</param>
        /// <param name="e">Drag event args.</param>
        private void _DragOver(object sender, DragEventArgs e)
        {
            // Mark previous dragged over element as needed to be redrawn.
            if (_draggedOverVisual != null)
                _draggedOverVisual.RedrawRequired = true;

            // Find object under necessary point using Visual tree helper.
            HitTestResult foundObject = VisualTreeHelper.HitTest(this, e.GetPosition(this));

            // If there is an element under drag cursor.
            if (foundObject != null)
            {
                // Set element under drag cursor.
                _draggedOverVisual = foundObject.VisualHit as GanttItemElementDrawingVisual;

                if (_draggedOverVisual != null)
                {
                    // Mark dragged over element as needed to be redrawn.
                    _draggedOverVisual.RedrawRequired = true;

                    // Store dragged data.
                    _draggedData = e.Data;
                }
            }
            // No elements under drug cursor found.
            else
            {
                _draggedOverVisual = null;
            }

            // Update layout.
            InvalidateVisual();
        }

        /// <summary>
        /// Handler updates layout after drop event occurs.
        /// </summary>
        /// <param name="sender">Container.</param>
        /// <param name="e">Drag event args.</param>
        private void _Drop(object sender, DragEventArgs e)
        {
            DropGanttItemElements();
            _draggedData = null;
        }

        /// <summary>
        /// Handler updates layout after drop event occurs.
        /// </summary>
        /// <param name="sender">Container.</param>
        /// <param name="e">Drag event args.</param>
        private void _DragLeave(object sender, DragEventArgs e)
        {
            DropGanttItemElements();
        }

        /// <summary>
        /// Handler changes selection by mouse click.
        /// </summary>
        /// <param name="sender">Container.</param>
        /// <param name="e">Mouse event args.</param>
        private void _MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.Assert(_selectedElements != null);

            // Get clicked element.
            IGanttItemElement ganttItemElement = this.HitTest(Mouse.GetPosition(this)) as IGanttItemElement;

            if (ganttItemElement == null)
                return;

            _mustStartDraggingOnMouseMove = true;

            if (_selectedElements.ContainsKey(ganttItemElement))
            {
                _needToReleaseSelection = true;
                return;
            }

            _Select(ganttItemElement);
        }

        /// <summary>
        /// Handler updates selection if necessary.
        /// </summary>
        /// <param name="sender">Container.</param>
        /// <param name="e">Mouse event args.</param>
        private void _PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_needToReleaseSelection)
            {
                // Get clicked element.
                IGanttItemElement ganttItemElement = this.HitTest(Mouse.GetPosition(this)) as IGanttItemElement;

                if (ganttItemElement == null)
                    return;

                _Select(ganttItemElement);
            }
            _needToReleaseSelection = false;
            _mustStartDraggingOnMouseMove = false;
        }

        /// <summary>
        /// Starts dragging selected objects.
        /// </summary>
        /// <param name="sender">Container.</param>
        /// <param name="e">Event args.</param>
        private void _MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && _mustStartDraggingOnMouseMove)
                _StartDragging();
        }

        /// <summary>
        /// Called when element is loaded into the visual tree.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Loaded(object sender, RoutedEventArgs e)
        {
            if (!_subscribedOnMouseMoves)
                _SubscribeOnMouseMoves();
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Min width of element.
        /// </summary>
        private const int DEFAULT_ELEMENT_WIDTH = 1;

        /// <summary>
        /// Const default duration for empty state.
        /// </summary>
        private const long DEFAULT_ELEMENT_HOURS_DURATION_FOR_EMPTY_STATE = 12;

        #endregion

        #region Private fields

        /// <summary>
        /// Start time of the gantt diagram.
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// End time of the gantt diagram.
        /// </summary>
        private DateTime _endTime;

        /// <summary>
        /// Control's size. "0" by default.
        /// </summary>
        private Size _size = new Size(0, 0);

        /// <summary>
        /// Collection of gantt items.
        /// </summary>
        private ObservableCollection<IGanttItem> _ganttItems;

        /// <summary>
        /// Selected gantt item elements.
        /// </summary>
        private Dictionary<IGanttItemElement, IGanttItemElement> _selectedElements = new Dictionary<IGanttItemElement, IGanttItemElement>();

        /// <summary>
        /// Dictionary that map gantt item element to drawing visual for quick search.
        /// </summary>
        private Dictionary<IGanttItemElement, GanttItemElementDrawingVisual> _mapElementToVisual = new Dictionary<IGanttItemElement, GanttItemElementDrawingVisual>();

        /// <summary>
        /// Collection of drawing visuals that container consists of.
        /// </summary>
        private VisualCollection _children;

        /// <summary>
        /// Default height of gantt row.
        /// </summary>
        private double _rowHeight;

        /// <summary>
        /// Count of pixels in one time tick.
        /// </summary>
        private static double _pixelsPerTick = 0;

        /// <summary>
        /// Visual element under drag cursor.
        /// </summary>
        private GanttItemElementDrawingVisual _draggedOverVisual;

        /// <summary>
        /// Flag shows that selection should be released by mouse LeftButton Up.
        /// </summary>
        private bool _needToReleaseSelection = false;

        /// <summary>
        /// Flag shows whether control must start dragging on mouse move.
        /// </summary>
        private bool _mustStartDraggingOnMouseMove = false;

        /// <summary>
        /// Indicates whether element is already subscribed on mouse moves. Used to avoid repeated subscriptions.
        /// </summary>
        private bool _subscribedOnMouseMoves = false;

        /// <summary>
        /// Dragged object.
        /// </summary>
        private IDataObject _draggedData = null;

        /// <summary>
        /// Glyph panel.
        /// </summary>
        private GanttGlyphPanel _glyphPanel = null;

        #endregion
    }
}
