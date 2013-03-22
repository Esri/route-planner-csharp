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
