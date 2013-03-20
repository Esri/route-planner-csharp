using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.DomainObjects;
using System.Windows.Media;
using System.Windows;
using ESRI.ArcLogistics.App.DragAndDrop;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class that represents a gantt item element that corresponds to a route without stops.
    /// </summary>
    internal class EmptyGanttItemElement : IGanttItemElement
    {
        /// <summary>
        /// Initializes a new instance of the <c>EmptyGanttItemElement</c> class.
        /// </summary>
        public EmptyGanttItemElement(IGanttItem parent)
        {
            _startTime = DateTime.MinValue;
            _endTime = DateTime.MaxValue;
            _parent = parent;
        }

        #region IGanttItemElement Members

        /// <summary>
        /// Gets start time.
        /// </summary>
        public DateTime StartTime
        {
            get
            {
                return _startTime;
            }
        }

        /// <summary>
        /// Gets end time.
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return _endTime;
            }
        }

        /// <summary>
        /// Gets route instance associated with this gantt item element. 
        /// </summary>
        /// <remarks>
        /// All empty elements that belong to the same gantt item have the same tag value.
        /// </remarks>
        public object Tag
        {
            get
            {
                return _parent.Tag;
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
            get { return false; }
        }

        /// <summary>
        /// Raised each time when element should be redraw.
        /// </summary>
        public event EventHandler RedrawRequired;

        /// <summary>
        /// Event is raised each time when drive time arrive time or depart time has changed.
        /// </summary>
        public event EventHandler TimeRangeChanged;

        /// <summary>
        /// Property cahnged event.
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Draws transparent element with container's width in default. If element is dragged over highlights Time windows of dregged order.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        public void Draw(GanttItemElementDrawingContext context)
        {
            Rect elementSize = new Rect(context.DrawingArea.X, context.DrawingArea.Top, context.DrawingArea.Width, context.DrawingArea.Height);

            SolidColorBrush fillBrush = new SolidColorBrush(Colors.Transparent);
            Pen drawPen = GanttControlHelper.GetPen();
            
            // Draw element.
            context.DrawingContext.DrawRectangle(fillBrush, drawPen, elementSize);

            // Draw dragged over element.
            if (context.DrawDraggedOver)
            {
                Debug.Assert(ParentGanttItem.Tag != null);

                Brush draggedOverBrush = GanttControlHelper.GetEmptyElementDragOverFillBrush(((Route)ParentGanttItem.Tag).Color);
                Pen pen = new Pen(fillBrush, 0);
                pen.Freeze();

                // Define collection of highlighted rectangles.
                Collection<Rect> highlightedRects = _GetHighlightedRects(context);

                if (highlightedRects == null)
                    return;

                // Draw dragged over areas.
                foreach (Rect rect in highlightedRects)
                    context.DrawingContext.DrawRoundedRectangle(draggedOverBrush, pen, rect, ROUND_RADIUS, ROUND_RADIUS);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns collection of rectangles which should be highlighted.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        /// <returns>Collection of rectangles.</returns>
        private Collection<Rect> _GetHighlightedRects(GanttItemElementDrawingContext context)
        {
            DragAndDropHelper helper = new DragAndDropHelper();

            // Define collection of dragged orders.
            Collection<Order> orders = helper.GetDraggingOrders(context.DraggedData);

            // If more than one order is dragged - we don't need to highlight anything. Just return.
            if (orders.Count > 1)
                return null;

            // Define default values for dates.
            DateTime startTime = DateTime.MinValue;
            DateTime endTime = DateTime.MaxValue;

            // Create result collection.
            Collection<Rect> rectResults = new Collection<Rect>();
            
            Order order = orders[0];
            Debug.Assert(order != null);

            // If both indows are wideopen - define wideopen time span - from MinDate to MaxDate. 
            if (order.TimeWindow.IsWideOpen && order.TimeWindow2.IsWideOpen)
                rectResults.Add(new Rect(context.DrawingArea.X, context.DrawingArea.Top, context.DrawingArea.Width, context.DrawingArea.Height + ANTI_ALIASING_GAP));

            // If first time window is not wideopen - define first time span.
            if (!order.TimeWindow.IsWideOpen)
            {
                startTime = new DateTime(order.TimeWindow.EffectiveFrom.Ticks);
                endTime = new DateTime(order.TimeWindow.EffectiveTo.Ticks);

                rectResults.Add(_GetRect(startTime, endTime, context));
            }

            // If second time window is not wideopen - define second time span.
            if (!order.TimeWindow2.IsWideOpen)
            {
                startTime = new DateTime(order.TimeWindow2.EffectiveFrom.Ticks);
                endTime = new DateTime(order.TimeWindow2.EffectiveTo.Ticks);

                rectResults.Add(_GetRect(startTime, endTime, context));
            }

            return rectResults;
        }

        /// <summary>
        /// Returns rect area by input parameters.
        /// </summary>
        /// <param name="startTime">Start area time.</param>
        /// <param name="endTime">End area time.</param>
        /// <param name="context">Drawing context.</param>
        /// <returns>Rect area.</returns>
        private Rect _GetRect(DateTime startTime, DateTime endTime, GanttItemElementDrawingContext context)
        {
            // Define count of pixels in minimal time span.
            double pixelsPerTick = context.DrawingArea.Width / (context.EndTime.TimeOfDay - context.StartTime.TimeOfDay).Ticks;

            // Define vertical dimensions.
            double yPos = context.DrawingArea.Top;
            double height = context.DrawingArea.Height;

            // Define horisontal dimensions.
            double xPos = (startTime.TimeOfDay - context.StartTime.TimeOfDay).Ticks * pixelsPerTick;
            double width = Math.Abs((endTime.TimeOfDay - startTime.TimeOfDay).Ticks* pixelsPerTick);

            // Return element size.
            return new Rect(xPos, yPos, width, height + ANTI_ALIASING_GAP);
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Radius of rounded rectangle.
        /// </summary>
        private const int ROUND_RADIUS = 3;

        /// <summary>
        /// Gap in 1 pixel to correct anti-aliasing.
        /// </summary>
        private const int ANTI_ALIASING_GAP = 1;

        #endregion

        #region Private fields

        /// <summary>
        /// Parent gantt item.
        /// </summary>
        private IGanttItem _parent;

        /// <summary>
        /// Start time.
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// End time.
        /// </summary>
        private DateTime _endTime;

        #endregion
    }
}
