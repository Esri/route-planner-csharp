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
using ESRI.ArcLogistics.DomainObjects.Attributes;

namespace ESRI.ArcLogistics.Geometry
{
    /// <summary>
    /// Structure that represents a point.
    /// </summary>
    public struct Point
    {
        #region constants

        private const string FORMAT_STR = "X:{0} Y:{1}";

        #endregion

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> structure.
        /// </summary>
        /// <param name="x">X coordinate in WGS84 projection.</param>
        /// <param name="y">Y coordinate in WGS84 projection.</param>
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
            this._m = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Point</c> structure.
        /// </summary>
        /// <param name="x">X coordinate in WGS84 projection.</param>
        /// <param name="y">Y coordinate in WGS84 projection.</param>
        /// <param name="m">Point M value.</param>
        public Point(double x, double y, double m)
        {
            this.x = x;
            this.y = y;
            this._m = m;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// X coordinate in WGS84 projection.
        /// </summary>
        [DomainProperty("DomainPropertyNameX")]
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        /// <summary>
        /// Y coordinate in WGS84 projection.
        /// </summary>
        [DomainProperty("DomainPropertyNameY")]
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        /// <summary>
        /// Point M value.
        /// </summary>
        public double M
        {
            get { return _m; }
            set { _m = value; }
        }

        #endregion public properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the point's coordinates as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string result = string.Format(FORMAT_STR, X, Y);
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
                return false;

            return this == (Point)obj;
        }

        public override int GetHashCode()
        {
            int nX = _GetInt(this.X);
            int nY = _GetInt(this.Y);
            int nM = _GetInt(this.M);

            return (int)(nX ^ nY ^nM);
        }

        #endregion public methods

        #region public operators
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static bool operator ==(Point pt1, Point pt2)
        {
            return (pt1.x == pt2.x && pt1.y == pt2.y && pt1._m == pt2._m);
        }

        public static bool operator !=(Point pt1, Point pt2)
        {
            return !(pt1 == pt2);
        }

        #endregion public operators

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Convert double to int.
        /// </summary>
        /// <param name="number">Double to convert.</param>
        /// <returns>Integer representating double.</returns>
        private static int _GetInt(double number)
        {
            if (number > (double)int.MaxValue)
                return int.MaxValue;
            else if (number < (double)int.MinValue)
                return int.MinValue;
            else
                return (int)number;
        }

        private double x;
        private double y; 
        private double _m;

        #endregion
    }
}
