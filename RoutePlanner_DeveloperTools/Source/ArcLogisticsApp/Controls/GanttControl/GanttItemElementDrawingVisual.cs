using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// DrawingVisual derived class that has a reference to a gantt item element.
    /// </summary>
    internal class GanttItemElementDrawingVisual : DrawingVisual
    {
        #region Constructors

        public GanttItemElementDrawingVisual(IGanttItemElement element)
        {
            GanttItemElement = element;
            RedrawRequired = true;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets bound gantt item element.
        /// </summary>
        public IGanttItemElement GanttItemElement
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
        public void Draw(GanttItemElementDrawingContext drawingContext)
        {
            // Open drawing context.
            drawingContext.DrawingContext = this.RenderOpen();

            GanttItemElement.Draw(drawingContext);

            // Close drawing context to null.
            drawingContext.DrawingContext.Close();
            drawingContext.DrawingContext = null;

            RedrawRequired = false;
        }

        #endregion
    }
}
