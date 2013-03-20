using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class that represents stop as a gantt item.
    /// </summary>
    internal class StopGanttItemElement : IGanttItemElement
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>StopGanttItem</c> class.
        /// </summary>
        public StopGanttItemElement(Stop stop, IGanttItem parent)
        {
            Debug.Assert(stop != null);
            Debug.Assert(parent != null);

            // Initialize stop.
            _stop = stop;
            _route = stop.Route;

            // Subscribe on stop and route changes to notify about updates.
            _route.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_RoutePropertyChanged);
            _stop.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_StopPropertyChanged);

            // Initialize parent.
            _parent = parent;
        }

        #endregion

        #region IGanttItemElement Members

        /// <summary>
        /// Returns stop's arrive time.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                // Return start time as stop's arrive time.
                return _stop.ArriveTime.Value;
            }
        }

        /// <summary>
        /// Returns stop's arrive time + service time.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return new DateTime(_stop.ArriveTime.Value.Ticks + 
                    Convert.ToInt64(_stop.TimeAtStop * TimeSpan.TicksPerMinute));
            }
        }

        /// <summary>
        /// Returns stop instance associated with this gantt item element.
        /// </summary>
        public object Tag
        {
            get
            {
                return _stop;
            }
        }

        /// <summary>
        /// Returns parent gantt item.
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
        /// Raised each time when stop should be redraw.
        /// </summary>
        public event EventHandler RedrawRequired;

        /// <summary>
        /// Event is raised each time when stop's arrive time or depart time has changed.
        /// </summary>
        public event EventHandler TimeRangeChanged;

        /// <summary>
        /// Property changed event.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method draws element.
        /// </summary>
        /// <param name="context">Drawing context where element shoul be drawn.</param>
        public void Draw(GanttItemElementDrawingContext context)
        {
            Debug.Assert(_route != null);

            StopDrawer.StopInfo stopInfo = new StopDrawer.StopInfo();
            stopInfo.Stop = _stop;
            stopInfo.Route = _route;

            StopDrawer.DrawStop(stopInfo, context);

            // Draw glyph over stop element.
            _DrawDraggedOverGlyph(_stop, context);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Draws dragged over glyph.
        /// </summary>
        /// <param name="stop">Bound stop.</param>
        /// <param name="context">DrawingContext.</param>
        private void _DrawDraggedOverGlyph(Stop stop, GanttItemElementDrawingContext context)
        {
            if (context.GlyphPanel == null)
                return;

            Rect elementRect = StopDrawer.GetElementRect(context);

            bool isStopFirst = false;

            if (stop.SequenceNumber == 1)
                isStopFirst = true;

            if (context.DrawDraggedOver && !context.DrawSelected && !_route.IsLocked)
                context.GlyphPanel.AddGlyph(this, new StopDragOverGlyph(elementRect, isStopFirst));
            else
                context.GlyphPanel.RemoveGlyphByKey(this);
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Raises event about GanttItemElement should be redraw if changed property can affect it's appearance.
        /// </summary>
        /// <param name="sender">Route.</param>
        /// <param name="e">Event args.</param>
        private void _RoutePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Route.PropertyNameColor || e.PropertyName == Route.PropertyNameIsLocked)
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
            if (e.PropertyName == Stop.PropertyNameIsLocked)
            {
                if (RedrawRequired != null)
                    RedrawRequired(this, EventArgs.Empty);
            }

            // If stop's time bounds was changed - raise necessary event to define new control's bounds.
            else if (e.PropertyName == Stop.PropertyNameArriveTime || e.PropertyName == Stop.PropertyNameTimeAtStop || e.PropertyName == Stop.PropertyNameWaitTime)
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
        /// Is in progress flag value.
        /// </summary>
        private bool _isInProgress = false;

        #endregion
    }
}
