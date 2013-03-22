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
