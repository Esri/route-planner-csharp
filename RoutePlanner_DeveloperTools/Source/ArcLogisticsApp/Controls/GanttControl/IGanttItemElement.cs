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
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Controls
{
    internal interface IGanttItemElement : INotifyPropertyChanged
    {
        /// <summary>
        /// Element's start date and time.
        /// </summary>
        DateTime StartTime
        {
            get;
        }

        /// <summary>
        /// Element's end date and time.
        /// </summary>
        DateTime EndTime
        {
            get;
        }

        /// <summary>
        /// Returns parent gantt item.
        /// </summary>
        IGanttItem ParentGanttItem
        {
            get;
        }

        /// <summary>
        /// Unique value associated with the element.
        /// </summary>
        object Tag
        {
            get;
        }

        /// <summary>
        /// Indicates whether current element is in progress.
        /// </summary>
        bool IsInProgress
        {
            get;
        }

        /// <summary>
        /// Raised each time when ganttItemElement should be redraw.
        /// </summary>
        event EventHandler RedrawRequired;

        /// <summary>
        /// Event is raised when start or end time has changed.
        /// </summary>
        event EventHandler TimeRangeChanged;

        /// <summary>
        /// Draws gantt item element.
        /// </summary>
        /// <param name="context">Drawing context.</param>
        /// <remarks>
        /// Don't close drawing context in the end of the drawing.
        /// </remarks>
        void Draw(GanttItemElementDrawingContext context);
    }
}
