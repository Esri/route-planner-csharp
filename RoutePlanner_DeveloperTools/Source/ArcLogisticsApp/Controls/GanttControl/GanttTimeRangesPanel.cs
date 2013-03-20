using System;
using System.Windows;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Gantt time ranges panel - represent time intervals (vertical dashes lines)
    /// </summary>
    internal class GanttTimeRangesPanel : GanttTimePanelBase
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public GanttTimeRangesPanel()
        {
        }

        #endregion // Constructor

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create hour range visual element - time range boundaries.
        /// </summary>
        protected override Size _CreateVisualChildren(Size dimension, double rangeWidth, int rangeStepInHour)
        {
            Point pt1 = new Point(0, 1);
            Point pt2 = new Point(0, dimension.Height - 1);
            Pen pen = _style.RangeBoundaryPen;

            DrawingVisual visualItem = new DrawingVisual();
            RenderOptions.SetEdgeMode((DependencyObject)visualItem, EdgeMode.Aliased);
            using (DrawingContext dc = visualItem.RenderOpen())
            {
                // draw time range boundaryes for every hour
              //  DateTime currentHour = _startHour;
                int current = 0;
                while (current <= (int)_duration)
                {
                    // draw vertical line
                    double x = Math.Floor(pt1.X);
                    dc.DrawLine(pen, new Point(x, pt1.Y), new Point(x, pt2.Y));

                    pt1.X += rangeWidth;

                //    currentHour = currentHour.AddHours(rangeStepInHour);
                    current += rangeStepInHour;
                }
            }

            _children.Add(visualItem);

            return visualItem.ContentBounds.Size;
        }

        #endregion // Private helpers
    }
}
