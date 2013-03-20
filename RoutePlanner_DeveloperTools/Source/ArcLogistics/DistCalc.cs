using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.MapService;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Class for calculating earth distance
    /// </summary>
    public static class  DistCalc
    {
        #region constants

        const double RADIAN_PER_UNIT = 0.017453292519943295;
        const double EARTH_MIDDLE_RADIUS = 6378136;
        
        #endregion

        #region internal members

        /// <summary>
        /// Get Extent Radius
        /// </summary>
        /// <param name="x">X coord</param>
        /// <param name="y">Y coord</param>
        /// <param name="extentRadiusInMeters">Extent radius in meters</param>
        /// <returns>Extent radius</returns>
        public static double GetExtentRadius(double x, double y, double extentRadiusInMeters)
        {
            double metersPerDegree = GetMetersPerDegree(y * RADIAN_PER_UNIT);

            double degreeScale = extentRadiusInMeters / metersPerDegree;

            double radiansScale = degreeScale * Math.PI / 180;

            double scaleUnits = radiansScale / RADIAN_PER_UNIT;
            return scaleUnits;
        }

        /// <summary>
        /// Get Meters Per Degree
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <returns>Meters per degree</returns>
        public static double GetMetersPerDegree(double latitude)
        {
            double dblEarthRadius = EARTH_MIDDLE_RADIUS * Math.Cos(latitude); 
            double metersPerDegree = (Math.PI/180) * dblEarthRadius;
            return metersPerDegree;
        }

        #endregion
    }
}
