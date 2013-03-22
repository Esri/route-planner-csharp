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
