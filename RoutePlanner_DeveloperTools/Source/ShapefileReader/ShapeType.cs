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

namespace ESRI.ArcLogistics.ShapefileReader
{
    /// <summary>
    /// Shape type.
    /// </summary>
    public enum ShapeType
    {
        /// <summary>
        /// Null.
        /// </summary>
        Null = 0,

        /// <summary>
        /// Point.
        /// </summary>
        Point = 1,

        /// <summary>
        /// Polyline.
        /// </summary>
        PolyLine = 3,

        /// <summary>
        /// Polygon.
        /// </summary>
        Polygon = 5,

        /// <summary>
        /// Multipoint.
        /// </summary>
        MultiPoint = 8,

        /// <summary>
        /// Point Z.
        /// </summary>
        PointZ = 11,

        /// <summary>
        /// Polyline Z
        /// </summary>
        PolyLineZ = 13,

        /// <summary>
        /// Polygon Z.
        /// </summary>
        PolygonZ = 15,

        /// <summary>
        /// Multi pointZ.
        /// </summary>
        MultiPointZ = 18,

        /// <summary>
        /// Point M.
        /// </summary>
        PointM = 21,

        /// <summary>
        /// Polyline M.
        /// </summary>
        PolyLineM = 23,

        /// <summary>
        /// Polygon M.
        /// </summary>
        PolygonM = 25,

        /// <summary>
        /// Multipoint M.
        /// </summary>
        MultiPointM = 28,

        /// <summary>
        /// Multipatch.
        /// </summary>
        MultiPatch = 31,
    }
}
