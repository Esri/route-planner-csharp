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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.Geometry
{
    /// <summary>
    /// Class that helps to convert compact geometry string to array of points.
    /// </summary>
    public class CompactGeometryConverter
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Converts compact geometry string to array of points.
        /// </summary>
        /// <param name="geometry">Compact geometry string.</param>
        /// <param name="points">Output parameter that contains array of points if conversion succeeded.</param>
        /// <returns>Returns <c>true</c> if conversion succeeded or <c>false</c> otherwise.</returns>
        public static bool Convert(string geometry, out Point[] points)
        {
            Debug.Assert(geometry != null);

            points = null;

            List<Point> ptList = new List<Point>();

            int? flags = 0;
            int indexXY = 0;
            double multBy_XY = 0;

            var firstElement = CompactGeometryStringWorker.ReadInt(geometry, ref indexXY);

            // If we couldn't read first element - return false.
            if (firstElement == null)
                return false;
            // Check that string is in post 9.3 format.
            if (firstElement == POST_93_FORMAT)
            {
                // Read version and check that it is current.
                var version = CompactGeometryStringWorker.ReadInt(geometry, ref indexXY);
                if(version != CURRENT_VERSION)
                    return false;

                // Flags showing what additional info string contains.
                flags = CompactGeometryStringWorker.ReadInt(geometry, ref indexXY);

                // Read XY multiplier.
                var succeeded = CompactGeometryStringWorker.ReadDouble(geometry, ref indexXY, ref multBy_XY);
                if (!succeeded)
                    return false;
            }
            // If it isnt - first element is XY multiplier.
            else
                multBy_XY = (double)firstElement;

            int nLength;

            int index_M = 0;
            int index_Z = 0;
            double multBy_M = 0;

            // If geometry has no additional info.
            if (flags == 0)
                nLength = geometry.Length;
            // If it has - read info about Z and M coordinates.
            else
            {
                nLength = geometry.IndexOf(CompactGeometryStringWorker.AdditionalPartsSeparator);

                // Check that compressed geometry has Z coordinate.
                if ((flags & FLAG_HAS_Z) == FLAG_HAS_Z) 
                {
                    index_Z = nLength + 1;
                    CompactGeometryStringWorker.ReadInt(geometry, ref index_Z);
                }

                // Check that it has M coordinate.
                if ((flags & FLAG_HAS_M) == FLAG_HAS_M)
                {
                    index_M = geometry.IndexOf(
                        CompactGeometryStringWorker.AdditionalPartsSeparator, index_Z) + 1;

                    // Read M multiplier.
                    var succeded = CompactGeometryStringWorker.ReadDouble(geometry, ref index_M, ref multBy_M);
                    if (!succeded)
                        return false;
                }
            }

            // Create readers for coordinates.
            var xReader = new CoordinateReader(geometry, multBy_XY);
            var yReader = new CoordinateReader(geometry, multBy_XY);
            var mReader = new CoordinateReader(geometry, multBy_M);

            while (indexXY != nLength)
            {
                // Read X and Y coordinates.
                double xCoordinate = 0;
                double yCoordinate = 0;
                if (!xReader.ReadNextCoordinate(ref indexXY, ref xCoordinate) ||
                    !yReader.ReadNextCoordinate(ref indexXY, ref yCoordinate))
                    return false;

                // If it has M coordinate.
                if ((flags & FLAG_HAS_M) == FLAG_HAS_M)
                {
                    // Read M coordinate.
                    double mCoordinate = 0; 
                    if(! mReader.ReadNextCoordinate(ref index_M, ref mCoordinate))
                        return false;

                    // Add point with M coordinate.
                    ptList.Add(new Point(xCoordinate, yCoordinate, mCoordinate));
                }
                // If point has no M coordinate - add default point.
                else
                    ptList.Add(new Point(xCoordinate, yCoordinate));
            }

            points = ptList.ToArray();

            return true;
        }

        /// <summary>
        /// Converts the specified collection of points into compact geometry.
        /// </summary>
        /// <param name="points">Collection of points to be converted to a compact geometry.</param>
        /// <returns>A string containing specified points in a compact geometry format.</returns>
        public static string Convert(IEnumerable<Point> points)
        {
            // Compute XY multiplier.
            var maxValue = points.Max(point => Math.Max(Math.Abs(point.X), Math.Abs(point.Y)));
            var multByXY = maxValue == 0.0 ? 1 : (int)(Int32.MaxValue / maxValue);
            
            var result = new StringBuilder();

            // Append info about geometry format.
            CompactGeometryStringWorker.AppendValue(POST_93_FORMAT, result);
            CompactGeometryStringWorker.AppendValue(CURRENT_VERSION, result);
            CompactGeometryStringWorker.AppendValue(FLAG_HAS_M, result);

            // Append XY multiplier.
            CompactGeometryStringWorker.AppendValue(multByXY, result);

            var lastX = default(int);
            var lastY = default(int);
            var lastM = default(int);
            foreach (var point in points)
            {
                lastX = CompactGeometryStringWorker.AppendValue(point.X, lastX, multByXY, result);
                lastY = CompactGeometryStringWorker.AppendValue(point.Y, lastY, multByXY, result);
            }

            // Write separtor to string.
            result.Append(CompactGeometryStringWorker.AdditionalPartsSeparator);

            // Calculate M multiplier.
            var multByM = (int)(1 / M_PRESICION);

            // Write M multiplier.
            CompactGeometryStringWorker.AppendValue(multByM, result);

            foreach (var point in points)
                lastM = CompactGeometryStringWorker.AppendValue(point.M, lastM, multByM, result);

            return result.ToString();
        }

        #endregion public methods

        #region private constants

        /// <summary>
        /// Value showing that compressed geom contains additional information.
        /// </summary>
        private const int POST_93_FORMAT = 0;

        /// <summary>
        /// Compressed geometry version.
        /// </summary>
        private const int CURRENT_VERSION = 1;

        /// <summary>
        /// Value showing that flag contains M.
        /// </summary>
        private const int FLAG_HAS_M = 2;

        /// <summary>
        /// Value showing that flag contains Z.
        /// </summary>
        private const int FLAG_HAS_Z = 1;

        /// <summary>
        /// Presicion for converting M values.
        /// </summary>
        private const double M_PRESICION = .00001;

        #endregion
    }


}
