using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class that contains all necessary data for gantt item element to draw itself.
    /// </summary>
    internal class GanttItemElementDrawingContext
    {
        /// <summary>
        /// Drawing context that must be used to draw the element.
        /// </summary>
        public DrawingContext DrawingContext
        {
            get;
            set;
        }

        /// <summary>
        /// Panel where should be drawn any glyph. Can be null.
        /// </summary>
        public GanttGlyphPanel GlyphPanel
        {
            get;
            set;
        }

        /// <summary>
        /// Drawing area where gantt item element should be drawn.
        /// </summary>
        public Rect DrawingArea
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether gantt item element must be draw as selected.
        /// </summary>
        public bool DrawSelected
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether gantt item element must be drawn as it is under cursor with dragged object.
        /// </summary>
        public bool DrawDraggedOver
        {
            get;
            set;
        }

        /// <summary>
        /// Dragged object.
        /// </summary>
        public IDataObject DraggedData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets start time.
        /// </summary>
        public DateTime StartTime
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets end time.
        /// </summary>
        public DateTime EndTime
        {
            get;
            set;
        }
    }
}
