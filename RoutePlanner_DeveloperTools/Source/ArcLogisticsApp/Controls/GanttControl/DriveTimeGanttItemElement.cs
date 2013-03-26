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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class that represents a gantt item element that corresponds to a drive time to some stop.
    /// </summary>
    internal class DriveTimeGanttItemElement : IGanttItemElement
    {
        /// <summary>
        /// Initializes a new instance of the <c>DriveTimeGanttItemElement</c> class.
        /// </summary>
        public DriveTimeGanttItemElement(Stop stop, IGanttItem parent)
        {
            // Creates route drive time as a gantt item element.
            _stop = stop;
            _route = stop.Route; // Cache route value.
            _parent = parent;

            // Subscribe on stop changes to notify about updates.
            _route.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_RoutePropertyChanged);
            _stop.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_StopPropertyChanged);
        }

        #region IGanttItemElement Members

        /// <summary>
        /// Gets driving start time.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                // Define start time as difference between stop's arrive time and stop travel time duration.
                return new DateTime(_stop.ArriveTime.Value.Ticks - Convert.ToInt64(_stop.TravelTime * TimeSpan.TicksPerMinute) - Convert.ToInt64(_stop.WaitTime * TimeSpan.TicksPerMinute));
            }
        }

        /// <summary>
        /// Gets driving end time.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return new DateTime(_stop.ArriveTime.Value.Ticks - Convert.ToInt64(_stop.WaitTime * TimeSpan.TicksPerMinute));
            }
        }

        /// <summary>
        /// Gets route instance associated with this gantt item element. 
        /// </summary>
        /// <remarks>
        /// All drive time elements that belong to the same gantt item have the same tag value.
        /// </remarks>
        public object Tag
        {
            get
            {
                return _route;
            }
        }

        /// <summary>
        /// Gets parent gantt item.
        /// </summary>
        public IGanttItem ParentGanttItem
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// Returns "true" if elemnt is in progress.
        /// </summary>
        public bool IsInProgress
        {
            get
            {
                return _isInProgress;
            }
            private set
            {
                _isInProgress = value;
                
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(PROP_NAME_ISINPROGRESS));
            }
        }

        /// <summary>
        /// Raised each time when drive time should be redraw.
        /// </summary>
        public event EventHandler RedrawRequired;

        /// <summary>
        /// Event is raised each time when drive time arrive time or depart time has changed.
        /// </summary>
        public event EventHandler TimeRangeChanged;

        /// <summary>
        /// Property changed event.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Draw(GanttItemElementDrawingContext context)
        {
            Debug.Assert(_stop != null);
            Debug.Assert(_route != null);

            // Define element size.
            double yPos = context.DrawingArea.Top + context.DrawingArea.Height / HEIGHT_INDEX;
            double height = context.DrawingArea.Height / HEIGHT_INDEX;
            Rect elementSize = new Rect(context.DrawingArea.X, yPos, context.DrawingArea.Width, height);

            Brush fillBrush = _GetFillBrush(context);
            Pen drawPen = GanttControlHelper.GetPen();

            // Draw element.
            context.DrawingContext.DrawRoundedRectangle(fillBrush, drawPen, elementSize, ROUND_RADIUS, ROUND_RADIUS);
        }

        /// <summary>
        /// Defines brush to draw drivetime element.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        /// <returns></returns>
        private Brush _GetFillBrush(GanttItemElementDrawingContext context)
        {
            Debug.Assert(_stop != null);
            Debug.Assert(_route != null);
            Debug.Assert(context != null);

            // Define selected color.
            if (context.DrawSelected)
                return GanttControlHelper.GetSelectedBrush();

            // Define dragged over color.
            if (context.DrawDraggedOver)
                return GanttControlHelper.GetDragOverBrush();

            // Define locked color.
            if (_stop.IsLocked || _route.IsLocked)
                return GanttControlHelper.GetLockedBrush(_route.Color);

            // If stop is Order, not locked, not selected and not dragged over - return normal fill color.
            return GanttControlHelper.GetFillBrush(_route.Color);
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Raises event about GanttItemElement should be redraw if changed property can affect it's appearance.
        /// </summary>
        /// <param name="sender">Stop.</param>
        /// <param name="e">Event args.</param>
        private void _RoutePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Route.PropertyNameIsLocked || e.PropertyName == Route.PropertyNameColor)
            {
                if (RedrawRequired != null)
                    RedrawRequired(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises event about GanttItemElement should be redraw if changed property can affect it's appearance.
        /// Or Time Range of Ganttcontrol should be changed.
        /// </summary>
        /// <param name="sender">Stop.</param>
        /// <param name="e">Event args.</param>
        private void _StopPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // If time bounds was changed - raise necessary event to update Gantt control's time bounds.
            if (e.PropertyName == Stop.PropertyNameArriveTime || e.PropertyName == Stop.PropertyNameTravelTime)
            {
                if (TimeRangeChanged != null)
                    TimeRangeChanged(this, EventArgs.Empty);

                if (RedrawRequired != null)
                    RedrawRequired(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Height index.
        /// </summary>
        private const int HEIGHT_INDEX = 3;

        /// <summary>
        /// Radius of rounded rectangle corners.
        /// </summary>
        private const int ROUND_RADIUS = 1;

        /// <summary>
        /// Selection color resource name.
        /// </summary>
        private const string SELECTION_COLOR_NAME = "SelectionColor";

        /// <summary>
        /// Drag over color resource name.
        /// </summary>
        private const string DRAG_OVER_COLOR_NAME = "DragOverObjectBackground";

        /// <summary>
        /// "IsInProgress" property name.
        /// </summary>
        private const string PROP_NAME_ISINPROGRESS = "IsInProgress";

        #endregion

        #region Private fields

        /// <summary>
        /// Associated stop.
        /// </summary>
        private Stop _stop;

        /// <summary>
        /// Cached value of stop's route.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Parent gantt item.
        /// </summary>
        private IGanttItem _parent;

        /// <summary>
        /// IsInProgress value.
        /// </summary>
        private bool _isInProgress = false;

        #endregion
    }
}
