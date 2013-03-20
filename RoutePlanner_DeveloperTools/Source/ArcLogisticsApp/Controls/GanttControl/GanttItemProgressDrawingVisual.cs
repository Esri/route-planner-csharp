using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// DrawingVisual derived class that draws gantt item progress label.
    /// </summary>
    internal class GanttItemProgressDrawingVisual : DrawingVisual
    {
        #region Constructors

        public GanttItemProgressDrawingVisual(IGanttItem item)
        {
            GanttItem = item;
            RedrawRequired = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets bound gantt item.
        /// </summary>
        public IGanttItem GanttItem
        {
            get;
            private set;
        }

        /// <summary>
        /// Indicates that it is necessary to redraw the visual.
        /// </summary>
        public bool RedrawRequired
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Draw visual content.
        /// </summary>
        /// <param name="drawingContext">Drawing context.</param>
        public void Draw(GanttItemProgressDrawingContext drawingContext)
        {
            // Open drawing context.
            drawingContext.DrawingContext = this.RenderOpen();

            // Close drawing context to null.
            drawingContext.DrawingContext.Close();
            drawingContext.DrawingContext = null;

            RedrawRequired = false;
        }

        #endregion
    }
}
