using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments.Controls
{
    /// <summary>
    /// Framwork element that shows gantt item elements.
    /// </summary>
    /// <remarks>
    /// This class is necessary to show gantt item element in drag and drop cursor.
    /// </remarks>
    internal class GanttElementFrameworkElement : FrameworkElement
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>GanttElementFrameworkElement.</c>.
        /// </summary>
        /// <param name="stop">Stop.</param>
        public GanttElementFrameworkElement(Stop stop)
        {
            Debug.Assert(stop != null);
            _stop = stop;

            _children = new VisualCollection(this);

            _CreateVisuals();
        }

        /// <summary>
        /// Creates new instance of <c>GanttElementFrameworkElement</c>.
        /// </summary>
        /// <param name="color">Stop color.</param>
        public GanttElementFrameworkElement(System.Drawing.Color color)
        {
            _children = new VisualCollection(this);

            _color = color;

            _CreateVisuals();
        }

        #endregion

        #region Protected Overriden Methods

        protected override int VisualChildrenCount
        {
            get
            {
                return _children.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new Size(GANTT_ELEMENT_WIDTH, GanttControlHelper.ItemHeight);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates visuals to represent the stop.
        /// </summary>
        private void _CreateVisuals()
        {
            // Create visual.
            DrawingVisual visual = new DrawingVisual();

            // Prepare context.
            GanttItemElementDrawingContext drawingContext = new GanttItemElementDrawingContext();
            drawingContext.DrawingContext = visual.RenderOpen();
            drawingContext.DrawDraggedOver = false;
            drawingContext.DrawSelected = false;
            drawingContext.DrawingArea = new Rect(0, 0, GANTT_ELEMENT_WIDTH, GanttControlHelper.ItemHeight);

            Debug.Assert(_stop != null && !_color.HasValue ||
                         _stop == null && _color.HasValue);

            // Draw visual either by stop or by color.
            if (_stop != null)
                StopDrawer.DrawStop(_stop.Route.Color, drawingContext);
            else
                StopDrawer.DrawStop(_color.Value, drawingContext);

            drawingContext.DrawingContext.Close();

            // Add visual.
            _children.Add(visual);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Default gantt element width.
        /// </summary>
        private static double GANTT_ELEMENT_WIDTH = 40;

        /// <summary>
        /// Collection of visuals.
        /// </summary>
        private VisualCollection _children;

        /// <summary>
        /// Stop.
        /// </summary>
        private Stop _stop = null;

        /// <summary>
        /// Stop color. Either _stop or _color is specified - not both.
        /// </summary>
        private System.Drawing.Color? _color = null;

        #endregion
    }
}
