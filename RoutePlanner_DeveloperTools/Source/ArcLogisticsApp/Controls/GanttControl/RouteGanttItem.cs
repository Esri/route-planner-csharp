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
using System.ComponentModel;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Data;
using System.Diagnostics;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class that represent route as a gantt item.
    /// </summary>
    internal class RouteGanttItem : IGanttItem, INotifyPropertyChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>RouteGanttItem</c> class.
        /// </summary>
        public RouteGanttItem(Route route)
        {
            // Initialize route.
            _route = route;

            // Subscribe on route name changes to update Title.
            _route.PropertyChanged += new PropertyChangedEventHandler(_RoutePropertyChanged);

            // Initialize collection of all gantt item elements.
            _ganttItemElements = new List<IGanttItemElement>();

            // Create gantt item elements collection for all stops.
            _CreateGanttItemElements(route);

            if (_ganttItemElements.Count != 0)
                _SubscribeToFirstAndLastElementTimeRangeChangedEvent(); // Subscribe on firs and lst elements time range changed events. 
        }

        /// <summary>
        /// Raises event about progress changed.
        /// </summary>
        /// <param name="sender">Progress element.</param>
        /// <param name="e">Event args.</param>
        private void _ProgressChanged(object sender, EventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, e);

            if (TimeRangeChanged != null)
                TimeRangeChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Public Static properties

        /// <summary>
        /// Gets "Title" string.
        /// </summary>
        public static string TitlePropertyName
        {
            get
            {
                return TITLE_PROPERTY_NAME;
            }
        }

        #endregion

        #region IGanttItem Members

        /// <summary>
        /// Returns Route name as a title. PropertyChanged event is raised for this property when route name changes.
        /// </summary>
        public string Title
        {
            get
            {
                return _route.Name;
            }
        }

        /// <summary>
        /// Collection of all gantt item elements.
        /// </summary>
        public IList<IGanttItemElement> GanttItemElements
        {
            get
            {
                return _ganttItemElements;
            }
        }

        /// <summary>
        /// Returns Route instance associated with this gantt item.
        /// </summary>
        public object Tag
        {
            get
            {
                return _route;
            }
        }

        /// <summary>
        /// Returns route start time.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }

        /// <summary>
        /// Returns route end time.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
        }

        /// <summary>
        /// Raised each time when ganttItemElement should be redraw.
        /// </summary>
        public event EventHandler ProgressChanged;

        /// <summary>
        /// Raises when common time ranges changed.
        /// </summary>
        public event EventHandler TimeRangeChanged;

        /// <summary>
        /// Searches element by tag.
        /// </summary>
        /// <param name="tag">Necessary tag.</param>
        /// <returns>Result collection.</returns> 
        public ReadOnlyCollection<IGanttItemElement> FindElementsByTag(object tag)
        {
            List<IGanttItemElement> foundElements = new List<IGanttItemElement>();

            // Add all elements with necessary tag into result collection.
            foreach (IGanttItemElement element in _ganttItemElements)
            {
                if (element.Tag.Equals(tag))
                    foundElements.Add(element);
            }

            return foundElements.AsReadOnly();
        }

        /// <summary>
        /// Finds first element in collection by tag.
        /// </summary>
        /// <param name="tag">Necessary tag.</param>
        /// <returns>Result element.</returns> 
        public IGanttItemElement FindFirstElementByTag(object tag)
        {
            IGanttItemElement foundElement = null;

            // Add all elements with necessary tag into result collection.
            foreach (IGanttItemElement element in _ganttItemElements)
            {
                if (element.Tag.Equals(tag))
                {
                    foundElement = element;
                    break;
                }
            }

            return foundElement;
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises when property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private methods

        /// <summary>
        /// Creates gantt item element for each stop and drive time between stops.
        /// </summary>
        /// <param name="route"></param>
        private void _CreateGanttItemElements(Route route)
        {
            // Sort route's tops.
            SortedDataObjectCollection<Stop> sortedStops = new SortedDataObjectCollection<Stop>(route.Stops, new StopsComparer());

            Debug.Assert(_ganttItemElements != null); // Must be initialized.

            foreach (Stop stop in sortedStops)
            {
                // Skip creating drive time element for the first stop.
                if (sortedStops.IndexOf(stop) > 0)
                    _ganttItemElements.Add(new DriveTimeGanttItemElement(stop, this)); // Create drive time element previous to stop.

                StopGanttItemElement newElement = new StopGanttItemElement(stop, this);

                _ganttItemElements.Add(newElement); // Add stop element.
            }

            if (_ganttItemElements.Count != 0)
            {
                // Define start time of gantt item.
                _startTime = _ganttItemElements[0].StartTime;

                // Define end time of gantt item.
                _endTime = _ganttItemElements[_ganttItemElements.Count - 1].EndTime;
            }

            // If route has not stops - add empty gantt item element to support common selection logic.
            if (sortedStops.Count == 0)
                _ganttItemElements.Add(new EmptyGanttItemElement(this));
        }

        /// <summary>
        /// Subscribes to first and last elements Time range Changed event. 
        /// </summary>
        /// <remarks>
        /// We need to handle these events to recalculate Gantt item time bounds.
        /// </remarks>
        private void _SubscribeToFirstAndLastElementTimeRangeChangedEvent()
        {
            Debug.Assert(_ganttItemElements != null);
            Debug.Assert(_ganttItemElements.Count > 0); // First and last elements should be already created.

            // Subscribe on first and last sub element time range event. 
            _ganttItemElements[0].TimeRangeChanged += new EventHandler(_ElementTimeRangeChanged);
            _ganttItemElements[_ganttItemElements.Count - 1].TimeRangeChanged += new EventHandler(_ElementTimeRangeChanged);
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handler check that property was changed in route and 
        /// if this property is "Name" - raises event about title property was changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _RoutePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Route.PropertyNameName && PropertyChanged != null)
                PropertyChanged(this, e);
        }

        /// <summary>
        /// Called when one of the elements raised time range event.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void _ElementTimeRangeChanged(object sender, EventArgs e)
        {
            IGanttItemElement changedElement = sender as IGanttItemElement;

            Debug.Assert(changedElement != null);

            bool needToRaiseEvent = false;

            // Check whether new end time is later then saved value:
            if (changedElement.EndTime > EndTime)
            {
                _endTime = changedElement.EndTime;
                needToRaiseEvent = true;
            }

            // Check whether new start time is earlier then saved value:
            if (changedElement.StartTime < StartTime)
            {
                _startTime = changedElement.StartTime;
                needToRaiseEvent = true;
            }

            // Raise event if necessary.
            if (needToRaiseEvent && TimeRangeChanged != null)
                TimeRangeChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Title property name.
        /// </summary>
        private const string TITLE_PROPERTY_NAME = "Title";

        #endregion

        #region Private fields

        /// <summary>
        /// Associated route.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Route start time.
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// Route end time.
        /// </summary>
        private DateTime _endTime;

        /// <summary>
        /// Collection of all gantt item elements.
        /// </summary>
        private List<IGanttItemElement> _ganttItemElements;

        #endregion
    }
}
