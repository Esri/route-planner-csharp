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
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Controls
{
    internal interface IGanttItem
    {
        /// <summary>
        /// Title of the gantt item.
        /// </summary>
        string Title
        {
            get;
        }

        /// <summary>
        /// Collection of gantt item elements that gantt item consists of.
        /// </summary>
        IList<IGanttItemElement> GanttItemElements
        {
            get;
        }

        /// <summary>
        /// Unique value associated with the gantt item.
        /// </summary>
        object Tag
        {
            get;
        }

        /// <summary>
        /// Gantt item start time - earliest start time among all elements.
        /// </summary>
        DateTime StartTime
        {
            get;
        }

        /// <summary>
        /// Gantt item end time - latest end time among all elements.
        /// </summary>
        DateTime EndTime
        {
            get;
        }

        /// <summary>
        /// Event is raised when gantt item start or end time changed.
        /// </summary>
        event EventHandler TimeRangeChanged;

        /// <summary>
        /// Finds all elements inside of the item by tag.
        /// </summary>
        /// <param name="tag">Tag value to find.</param>
        /// <returns>Returns collection of found elements. If there are not such elements then returns <code>null</code>.</returns>
        ReadOnlyCollection<IGanttItemElement> FindElementsByTag(object tag);

        /// <summary>
        /// Finds first element inside of the item by tag.
        /// </summary>
        /// <param name="tag">Tag value to find.</param>
        /// <returns>Returns first found element. If there is not such element then returns <code>null</code>.</returns>
        IGanttItemElement FindFirstElementByTag(object tag);
    }
}
