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

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Extended direction with information about its type.
    /// </summary>
    internal sealed class DirectionEx
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public DirectionEx()
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Direction length. 
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Direction time.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Direction text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Direction shape in compact format.
        /// </summary>
        /// <remarks>
        /// Use <c>CompactGeometryConverter</c> to get array of points from the string.
        /// </remarks>
        public string Geometry { get; set; }

        /// <summary>
        /// Type of direction maneuver.
        /// </summary>
        public StopManeuverType ManeuverType { get; set; }

        /// <summary>
        /// Type of direction.
        /// </summary>
        public DirectionType DirectionType { get; set; }

        #endregion
    }
}