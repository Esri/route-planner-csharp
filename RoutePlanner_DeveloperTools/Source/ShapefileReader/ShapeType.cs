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
