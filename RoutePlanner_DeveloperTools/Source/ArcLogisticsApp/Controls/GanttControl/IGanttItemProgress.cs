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
