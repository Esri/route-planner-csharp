using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class that contains all necessary data for gantt item progress to draw itself.
    /// </summary>
    internal class GanttItemProgressDrawingContext
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
        /// Center point where drawing must be drawn.
        /// </summary>
        public Point DrawingCenterPoint
        {
            get;
            set;
        }
    }
}
