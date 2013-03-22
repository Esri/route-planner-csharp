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
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.DragAndDrop;
using System.Windows.Threading;
using ESRI.ArcLogistics.App.Converters;
using System.Globalization;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Commands;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Interaction logic for TimeView.xaml
    /// </summary>
    internal partial class TimeView : DockableContent
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeView()
        {
            InitializeComponent();
            _InitEventHandlers();

            new ViewButtonsMarginUpdater(this, commandButtonsGroup);
            commandButtonsGroup.Initialize(CategoryNames.TimeViewRoutesRoutingCommands, ganttControl);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets schedule page.
        /// </summary>
        internal OptimizeAndEditPage ParentPage
        {
            get { return _optimizeAndEditPage; }
            set
            {
                if (_optimizeAndEditPage != null)
                {
                    _optimizeAndEditPage.CurrentScheduleChanged -= _CurrentScheduleChanged;
                    _optimizeAndEditPage.LockedPropertyChanged -= _LockedPropertyChanged;
                   
                }

                _optimizeAndEditPage = value;
                _optimizeAndEditPage.CurrentScheduleChanged += new EventHandler(_CurrentScheduleChanged);
                _optimizeAndEditPage.LockedPropertyChanged += new EventHandler(_LockedPropertyChanged);                
            }
        }

        /// <summary>
        /// Gets/sets collection of selected items.
        /// </summary>
        public ObservableCollection<object> SelectedItems
        {
            get { return _selectedItems; }
            set { _selectedItems = value; }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates all necessary event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            this.VisibileStateChanged += new EventHandler(_VisibileStateChanged);
            _selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(_SelectedItemsCollectionChanged);
            ganttControl.Loaded += new RoutedEventHandler(_GanttControlLoaded);
            ganttControl.SelectionChanged += new EventHandler(_GanttControlSelectionChanged);
        }

        private void _VisibileStateChanged(object sender, EventArgs e)
        {
            if (this.IsVisible)
            {
                if (_optimizeAndEditPage.CurrentSchedule != null && _needToUpdateGanttItems)
                    _CreateGanttItems();

                // Update UI locked/unlocked.
                _CheckLocked();
            }
        }

        /// <summary>
        /// Updates Locked status.
        /// </summary>
        private void _CheckLocked()
        {
            lockedGrid.Visibility = (_optimizeAndEditPage.IsLocked) ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Creates gantt items.
        /// </summary>
        private void _CreateGanttItems()
        {
            ganttControl.RemoveAllGanttItems();

            if (_optimizeAndEditPage.CurrentSchedule.Routes.Count > 0)
            {
                SortedDataObjectCollection<Route> sortedRoutes = new SortedDataObjectCollection<Route>(_optimizeAndEditPage.CurrentSchedule.Routes, new RoutesComparer());

                foreach (Route route in sortedRoutes)
                {
                    ganttControl.AddGanttItem(new RouteGanttItem(route));
                }
            }
        }

        /// <summary>
        /// Starts changing selection - call method to select gantt item elements using BeginInvoke to avoid multiple calls.
        /// </summary>
        private void _StartChangeSelection()
        {
            // Don't make such calls if the flag is true and this method is called again
            if (_needToSelect)
                return;

            // Mark selection should be changed.
            _needToSelect = true;

            // Update selected elements using BeginInvoke, since changes can be complex and we don't want to update selection on each one.
            this.Dispatcher.BeginInvoke(new _NoParamsDelegate(_SelectGanttItemElements), DispatcherPriority.Render);
        }

        /// <summary>
        /// No-params delegate for update selection by BeginInvoke.
        /// </summary>
        private delegate void _NoParamsDelegate();

        /// <summary>
        /// Calls Select method of gantt control.
        /// </summary>
        /// <remarks>
        /// Used in BeginInvoke.
        /// </remarks>
        private void _SelectGanttItemElements()
        {
            // Collection of selected gantt item elements.
            Collection<IGanttItemElement> selectedGanttItemsElements = new Collection<IGanttItemElement>();

            // Get collection of selected Gantt item elements.
            foreach (IGanttItem ganttItem in ganttControl.GanttItems)
            {
                foreach (IGanttItemElement ganttItemElement in ganttItem.GanttItemElements)
                {
                    if (_selectedItems.Contains(ganttItemElement.Tag) && !selectedGanttItemsElements.Contains(ganttItemElement))
                        selectedGanttItemsElements.Add(ganttItemElement);
                }
            }

            _needToChangeSelection = false;
            ganttControl.Select(selectedGanttItemsElements);
            _needToSelect = false;
        }

        /// <summary>
        /// Method checks is dragging allowed and starts dragging if possible
        /// </summary>
        private void _TryToStartDragging()
        {
            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();

            Collection<Object> selection = _GetSelectedStopsAndOrders();
            if (dragAndDropHelper.IsDragAllowed(selection))
                dragAndDropHelper.StartDragOrders(selection, DragSource.TimeView);
        }

        /// <summary>
        /// Method separates stops and orders from items control selection
        /// </summary>
        /// <returns></returns>
        private Collection<Object> _GetSelectedStopsAndOrders()
        {
            Collection<Object> selectedStopsAndOrders = new Collection<Object>();
            foreach (IGanttItemElement element in ganttControl.SelectedGanttItemElements)
            {
                if ((element.Tag is Stop) || (element.Tag is Order))
                    selectedStopsAndOrders.Add(element.Tag);
            }

            return selectedStopsAndOrders;
        }

        /// <summary>
        /// Method changes command buttons set.
        /// </summary>
        private void _ChangeCommandButtonsSet()
        {
            string currentCommands = CategoryNames.TimeViewRoutesRoutingCommands;

            // If selection contains at least one stop - show stops commands set. Otherwise show route commands set.
            if (_optimizeAndEditPage != null && _optimizeAndEditPage.SelectedItems != null)
            {
                foreach (Object item in _optimizeAndEditPage.SelectedItems)
                {
                    if (item is Stop)
                    {
                        currentCommands = CategoryNames.TimeViewStopsRoutingCommands;
                        break;
                    }
                }
            }

            commandButtonsGroup.Initialize(currentCommands, ganttControl);
        }

        #endregion

        #region Private Drag'n'Drop methods

        /// <summary>
        /// Method starts drop if possible
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _TryToDrop(DragEventArgs e)
        {
            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();

            Point mousePoint = e.GetPosition(ganttControl);
            object hoveredObject = ganttControl.HitTest(mousePoint); // Get object under mouse cursor.
            Object dropTarget = _GetTargetData(hoveredObject); // Get Tag from hovered object.

            if (dropTarget == null) // If target drop object is "null" - do nothing.
                return;

            // Get collection of dragging orders.
            ICollection<Order> draggingOrders = dragAndDropHelper.GetDraggingOrders(e.Data);

            // If user try to change single stop position - define additional parameters.
            if ((1 == draggingOrders.Count) && (dropTarget is Route) && (hoveredObject is IGanttItemElement) && ((Route)dropTarget).Stops.Count > 0)
            {
                IGanttItem parentItem = null;
                int index = 0;
                parentItem = ((IGanttItemElement)hoveredObject).ParentGanttItem;
                index = parentItem.GanttItemElements.IndexOf((IGanttItemElement)hoveredObject);

                Debug.Assert(index + 1 < parentItem.GanttItemElements.Count);
                if (parentItem.GanttItemElements[index + 1].Tag is Stop)
                    dropTarget = (Stop)parentItem.GanttItemElements[index + 1].Tag;
            }

            // Do Drop.
            dragAndDropHelper.Drop(dropTarget, e.Data);
        }
        
        /// <summary>
        /// Method returns data from UIElement.
        /// </summary>
        /// <param name="target">Drop target.</param>
        /// <returns>Tag object (stop or route).</returns>
        private Object _GetTargetData(object target)
        {
            Object data = null;

            if (target is IGanttItem)
                data = ((IGanttItem)target).Tag;
            else if (target is IGanttItemElement)
                data = ((IGanttItemElement)target).Tag;
            else
                data = null;

            return data;
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Creates gantt items when control is loaded and items collection is empty.
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Event args.</param>
        private void _GanttControlLoaded(object sender, RoutedEventArgs e)
        {
            // If control loads first time - need to init GetTooltipDelegate property.
            if (ganttControl.GetTooltipCallback == null)
                ganttControl.GetTooltipCallback = new TimeTooltipCallback();

            if (ganttControl.GanttItems.Count == 0)
                _CreateGanttItems();

            _ChangeCommandButtonsSet();
        }

        /// <summary>
        /// Handler calls methods to update gantt control selection.
        /// </summary>
        /// <param name="sender">Collection of selected elements.</param>
        /// <param name="e">Event args.</param>
        private void _SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _StartChangeSelection();
        }

        /// <summary>
        /// Changes selection/
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Event args.</param>
        private void _GanttControlSelectionChanged(object sender, EventArgs e)
        {
            if (!_needToChangeSelection)
            {
                _needToChangeSelection = true;
                return;
            }

            _selectedItems.CollectionChanged -= _SelectedItemsCollectionChanged;

            _selectedItems.Clear();

            foreach (IGanttItemElement itemElement in ganttControl.SelectedGanttItemElements)
            {
                if (!_selectedItems.Contains(itemElement.Tag))
                    _selectedItems.Add(itemElement.Tag);
            }

            _ChangeCommandButtonsSet();

            _selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(_SelectedItemsCollectionChanged);
        }

        /// <summary>
        /// Changes visibility state.
        /// </summary>
        /// <param name="sender">Optimize and edit page.</param>
        /// <param name="e">Event args.</param>
        private void _LockedPropertyChanged(object sender, EventArgs e)
        {
            _CheckLocked();
        }

        /// <summary>
        /// Creates gantt items to new schedule.
        /// </summary>
        /// <param name="sender">Optimize and edit page.</param>
        /// <param name="e">Event args.</param>
        private void _CurrentScheduleChanged(object sender, EventArgs e)
        {
            _optimizeAndEditPage.CurrentSchedule.Routes.CollectionChanged -= _RoutesCollectionChanged;
            _optimizeAndEditPage.CurrentSchedule.Routes.CollectionChanged += new NotifyCollectionChangedEventHandler(_RoutesCollectionChanged);

            // Create gantt items.
            if (this.IsVisible)
            {
                _CreateGanttItems();
                _needToUpdateGanttItems = false;
            }
            else
                _needToUpdateGanttItems = true;

            _ChangeCommandButtonsSet();
        }

        /// <summary>
        /// Updates gantt control.
        /// </summary>
        /// <param name="sender">Routes collection.</param>
        /// <param name="e">Event args.</param>
        private void _RoutesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                _CreateGanttItems();
                _needToUpdateGanttItems = false;
            }
            else
                _needToUpdateGanttItems = true;
        }

        /// <summary>
        /// Drops objects to gantt control.
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Drag event args.</param>
        private void _GanttControlDrop(object sender, DragEventArgs e)
        {
            _TryToDrop(e);
        }

        /// <summary>
        /// Handler starts drag'n'drop from gantt control.
        /// </summary>
        /// <param name="sender">Gantt control.</param>
        /// <param name="e">Event args.</param>
        /// <remarks>
        /// We used special event because Mouse events that should change selection in gantt control 
        /// occur there earlier than in gantt control. 
        /// </remarks>
        private void _GanttControlDragItemsStarted(object sender, EventArgs e)
        {
            _TryToStartDragging();
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Reference to optimize and edit page.
        /// </summary>
        private OptimizeAndEditPage _optimizeAndEditPage = null;

        /// <summary>
        /// Collection of selected items.
        /// </summary>
        private ObservableCollection<Object> _selectedItems = new ObservableCollection<Object>();

        /// <summary>
        /// Flag shows that selection should be changed.
        /// </summary>
        private bool _needToSelect = false;

        /// <summary>
        /// Flag used to react on SelectionChanged event from gantt control.
        /// </summary>
        private bool _needToChangeSelection = false;

        /// <summary>
        /// Flag shows whether gantt items should be created (e.g. after schedule chaged).
        /// </summary>
        private bool _needToUpdateGanttItems = true;

        #endregion
    }

    /// <summary>
    /// Class used to create tooltip content for objects in TimeView. Inherit from 
    /// </summary>
    internal class TimeTooltipCallback : IGetTooltipCallback
    {
        #region IGetTooltipCallback Members

        /// <summary>
        /// Creates tooltip for hovered object.
        /// </summary>
        /// <param name="hoveredObject">Object under mouse cursor.</param>
        /// <returns>Tooltip content.</returns>
        public object GetTooltip(object hoveredObject)
        {
            object tooltipContent = null;

            GanttTooltipConverter converter = new GanttTooltipConverter();
            tooltipContent = converter.Convert(hoveredObject, typeof(object), null, CultureInfo.CurrentCulture);

            return tooltipContent;
        }

        #endregion
    }
}
