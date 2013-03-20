using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interface represents gantt item progress.
    /// </summary>
    internal interface IGanttItemProgress
    {
        /// <summary>
        /// Gets current element in progress.
        /// </summary>
        IGanttItemElement CurrentElement
        {
            get;
        }

        /// <summary>
        /// Draws gantt item progress.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        /// <remarks>
        /// Don't close drawing context in the end of the drawing.
        /// </remarks>
        void Draw(GanttItemProgressDrawingContext context);

        /// <summary>
        /// Raised each time when progress has changed.
        /// </summary>
        event EventHandler ProgressChanged;

        /// <summary>
        /// Raised each time when element needs to be redrawn.
        /// </summary>
        event EventHandler RedrawRequired;
    }
}
