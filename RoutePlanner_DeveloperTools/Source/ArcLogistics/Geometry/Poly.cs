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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Geometry
{
    /// <summary>
    /// Class that represents a polycurve. Base class for polylines and polygons.
    /// </summary>
    public abstract class PolyCurve
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>PolyCurve</c> class.
        /// </summary>
        /// <param name="groups">Array of the groups that compose the polycurve. Each 
        /// array element represents a number of points inside of a group.</param>
        /// <param name="points">Array of all the polycurve points.</param>
        public PolyCurve(int[] groups, Point[] points)
        {
            Debug.Assert(groups != null);
            Debug.Assert(points != null);

            // calc. extent
            _extent.SetEmpty();
            foreach (Point pt in points)
                _extent.Union(pt);

            _groups = groups;
            _points = points;
        }

        /// <summary>
        /// Initializes a new instance of the <c>PolyCurve</c> class with one group of specified points.
        /// </summary>
        /// <param name="points">Array of points.</param>
        public PolyCurve(Point[] points)
            : this(new int[] { points.Length }, points)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>PolyCurve</c> class from bytes array.
        /// </summary>
        /// <param name="bytes">Array of bytes.</param>
        public PolyCurve(byte[] bytes)
        {
            Debug.Assert(bytes != null);

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    // groups number
                    int groupCount = reader.ReadInt32();

                    // points number
                    int pointCount = reader.ReadInt32();

                    // groups
                    int[] groups = new int[groupCount];

                    for (int nGroup = 0; nGroup < groups.Length; nGroup++)
                        groups[nGroup] = reader.ReadInt32();

                    _groups = groups;
                    _points = _GetPoints(reader, _HasMCoordinate(ms, pointCount), pointCount);
                }
            }
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets bounding box.
        /// </summary>
        public Envelope Extent
        {
            get { return _extent; }
        }

        /// <summary>
        /// Gets groups.
        /// </summary>
        public int[] Groups
        {
            get { return _groups; }
        }

        /// <summary>
        /// Gets total number of points.
        /// </summary>
        public int TotalPointCount
        {
            get { return _points.Length; }
        }

        /// <summary>
        /// Gets a boolean value indicating whether curve is empty (contains zero groups and zero points).
        /// </summary>
        public bool IsEmpty
        {
            get { return _groups.Length == 0; }
        }

        #endregion public properties

        #region protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets array that contains all points.
        /// </summary>
        protected Point[] Points
        {
            get { return _points; }
        }

        #endregion protected properties

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Retrieves points for the specified group.
        /// </summary>
        /// <param name="group">An integer that represents the group index.</param>
        /// <returns>Array of <c>Point</c> structures.</returns>
        public Point[] GetGroupPoints(int group)
        {
            if (group < 0 || group >= _groups.Length)
                throw new ArgumentException();

            int index = 0;
            for (int i = 0; i < group; i++)
                index += _groups[i];

            int pointsNum = _groups[group];

            Point[] resPoints = new Point[pointsNum];
            Array.Copy(_points, index, resPoints, 0, pointsNum);

            return resPoints;
        }

        /// <summary>
        /// Retrieves a range of points.
        /// </summary>
        /// <param name="fromIndex">An integer that represents the starting point index.</param>
        /// <param name="points">An integer that represents number of returned points.</param>
        /// <returns>Array of <c>Point</c> structures.</returns>
        public Point[] GetPoints(int fromIndex, int points)
        {
            if (fromIndex < 0 || fromIndex >= TotalPointCount)
                throw new ArgumentException();

            if (points <= 0 || (fromIndex + points) > TotalPointCount)
                throw new ArgumentException();

            Point[] resPoints = new Point[points];
            Array.Copy(_points, fromIndex, resPoints, 0, points);

            return resPoints;
        }

        /// <summary>
        /// Retrieves  point.
        /// </summary>
        /// <param name="pointIndex">An integer that represents point index.</param>
        /// <returns><c>Point</c> by index.</returns>
        public Point GetPoint(int pointIndex)
        {
            if (pointIndex < 0 || pointIndex >= TotalPointCount)
                throw new ArgumentException();

            return _points[pointIndex];
        }

        /// <summary>
        /// Converts polycurve to array of bytes.
        /// </summary>
        /// <returns>Bytes array.</returns>
        public byte[] ToByteArray()
        {
            byte[] bytes= null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    // groups number
                    writer.Write(_groups.Length);

                    // points number
                    writer.Write(_points.Length);

                    // groups
                    foreach (int group in _groups)
                        writer.Write(group);

                    // points
                    foreach (Point pt in _points)
                    {
                        writer.Write(pt.X);
                        writer.Write(pt.Y);
                        writer.Write(pt.M);
                    }

                    bytes = ms.ToArray();
                }
            }

            return bytes;
        }

        #endregion public methods

        #region Private methods

        /// <summary>
        /// Read points from memory stream.
        /// </summary>
        /// <param name="reader">Binary reader.</param>
        /// <param name="hasMCoordinate">Flag, showing that points has M coordinate.</param>
        /// <param name="pointCount">Number of points to be read.</param>
        /// <returns>Collection of points read from stream.</returns>
        private Point[] _GetPoints(BinaryReader reader, bool hasMCoordinate, int pointCount)
        {
            Point[] points = new Point[pointCount];
            for (int nPoint = 0; nPoint < points.Length; nPoint++)
            {
                Point pt;

                // If points has M coordinates - read 3 coordinates.
                if (hasMCoordinate)
                    pt = new Point(reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
                // If doesn't - read 2 coordinates.
                else
                    pt = new Point(reader.ReadDouble(), reader.ReadDouble());

                points[nPoint] = pt;
            }

            return points;
        }

        /// <summary>
        /// Check that points has M coordinate.
        /// </summary>
        /// <param name="stream">Stream with points.</param>
        /// <param name="pointCount">Number of points in stream.</param>
        /// <returns>'True' if points has M coordinate, 'false' otherwise.</returns>
        private bool _HasMCoordinate(Stream stream, int pointCount)
        {
            // Get count of bytes left in a stream.
            var bytesLeft = stream.Length - stream.Position;

            // Calculate point number of coordinates.
            var coordCount = bytesLeft / (sizeof(double) * pointCount);

            // Check that points has third, M coordinate.
            return coordCount == 3;
        }

        #endregion

        #region private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private int[] _groups;
        private Point[] _points;
        private Envelope _extent;

        #endregion private fields
    }

    /// <summary>
    /// Class that represents a polyline.
    /// </summary>
    public class Polyline : PolyCurve, ICloneable
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Polyline</c> class with specified groups and points.
        /// </summary>
        /// <param name="groups">Array of the groups that compose the polyline. Each array element represents a start index of group's points inside of the <c>points</c> array.</param>
        /// <param name="points">Array of all the polyline points.</param>
        public Polyline(int[] groups, Point[] points)
            : base(groups, points)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polyline</c> class with one group of specified points.
        /// </summary>
        /// <param name="points">Array of points.</param>
        public Polyline(Point[] points)
            : base(points)
        {
        }

        /// <summary>
        /// Initializes a new instance of <c>Polyline</c> class from bytes array.
        /// </summary>
        /// <param name="bytes">Array of bytes.</param>
        public Polyline(byte[] bytes)
            : base(bytes)
        {
        }

        #endregion constructors

        #region ICloneable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            // groups
            int[] groups = new int[Groups.Length];
            Groups.CopyTo(groups, 0);

            // points
            Point[] points = new Point[Points.Length];
            Points.CopyTo(points, 0);

            return new Polyline(groups, points);
        }

        #endregion ICloneable interface members

    }

    /// <summary>
    /// Class that represents a polygon.
    /// </summary>
    public class Polygon : PolyCurve, ICloneable
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>Polygon</c> class with specified groups and points.
        /// </summary>
        /// <param name="groups">Array of the groups that compose the polygon. Each array element represents a start index of group's points inside of the <c>points</c> array.</param>
        /// <param name="points">Array of all the polygon points.</param>
        public Polygon(int[] groups, Point[] points)
            : base(groups, points)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Polygon</c> class with one group of specified points.
        /// </summary>
        /// <param name="points">Array of points.</param>
        public Polygon(Point[] points)
            : base(points)
        {
        }

        /// <summary>
        /// Initializes a new instance of <c>Polygon</c> class from bytes array.
        /// </summary>
        /// <param name="bytes">Array of bytes.</param>
        public Polygon(byte[] bytes)
            : base(bytes)
        {
        }

        #endregion constructors

        #region ICloneable interface members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            // groups
            int[] groups = new int[Groups.Length];
            Groups.CopyTo(groups, 0);

            // points
            Point[] points = new Point[Points.Length];
            Points.CopyTo(points, 0);

            return new Polygon(groups, points);
        }

        #endregion ICloneable interface members
    }

}
