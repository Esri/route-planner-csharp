using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// GeometryHolder class.
    /// </summary>
    [Serializable]
    internal class GeometryHolder : ISerializable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        public GeometryHolder()
        {
        }

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="info">Serialization Info.</param>
        /// <param name="context">Streaming Context.</param>
        /// <exception cref="SerializationException">If geometry type is not supported.</exception>
        protected GeometryHolder(SerializationInfo info, StreamingContext context)
        {
            if (GPObjectHelper.IsPoint(info))
            {
                // Create point.
                this._value = _CreatePoint(info);
            }
            else if (GPObjectHelper.IsPolyline(info))
            {
                this._value = _ReadPolyline(info);
            }
            else
            {
                // Unsupported geometry type.
                throw new SerializationException(
                    Properties.Messages.Error_UnsupportedGPGeometryObject);
            }
        }

        #endregion


        #region Properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// GPGeometry property.
        /// </summary>
        public GPGeometry Value
        {
            get { return _value; }
            set { _value = value; }
        }

        #endregion

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method fills Serialization information about GPGeometry.
        /// </summary>
        /// <param name="info">Serialization Info.</param>
        /// <param name="context">Streaming Context.</param>
        /// <exception cref="SerializationException">In case of geometry type is not
        /// supported.</exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (_value is GPPoint)
            {
                GPPoint pt = (GPPoint)_value;
                info.AddValue(GPAttribute.POINT_X, pt.X);
                info.AddValue(GPAttribute.POINT_Y, pt.Y);
            }
            else if (_value is GPPolygon)
            {
                GPPolygon polygon = _value as GPPolygon;
                info.AddValue(GPAttribute.POLYGON_RINGS, polygon.Rings);
            }
            else if (_value is GPPolyline)
            {
                GPPolyline polyline = _value as GPPolyline;
                info.AddValue(GPAttribute.POLYLINE_PATHS, polyline.Paths);
            }
            else
            {
                // Unsupported geometry type.
                throw new SerializationException(
                    Properties.Messages.Error_UnsupportedGPGeometryObject);
            }
        }

        #endregion

        #region Private static methods
        /// <summary>
        /// Reads <see cref="GPPolyline"/> object from the specified serialization info.
        /// </summary>
        /// <param name="info">The reference to the serialization info object to read
        /// polyline from.</param>
        /// <returns>A new deserialized <see cref="GPPolyline"/> object,</returns>
        private static GPPolyline _ReadPolyline(SerializationInfo info)
        {
            Debug.Assert(info != null);

            var paths = (object[])info.GetValue(GPAttribute.POLYLINE_PATHS, typeof(object[]));
            var data = new double[paths.Length][][];
            for (var i = 0; i < paths.Length; ++i)
            {
                // Read a single path.
                var path = (object[])paths[i];
                var dataPath = data[i] = new double[path.Length][];
                for (var j = 0; j < path.Length; ++j)
                {
                    // Read a single point from the path.
                    var point = (object[])path[j];
                    var dataPoint = dataPath[j] = new double[point.Length];

                    // Copy point data.
                    for (var k = 0; k < point.Length; ++k)
                    {
                        dataPoint[k] = Convert.ToDouble(point[k]);
                    }
                }
            }

            var result = new GPPolyline
            {
                Paths = data,
            };

            return result;
        }
        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method creates GPPoint from Serialization Info.
        /// </summary>
        /// <param name="info">Serialization Info.</param>
        /// <returns>GPPoint.</returns>
        private GPPoint _CreatePoint(SerializationInfo info)
        {
            Debug.Assert(info != null);

            GPPoint pt = new GPPoint();
            pt.X = info.GetDouble(GPAttribute.POINT_X);
            pt.Y = info.GetDouble(GPAttribute.POINT_Y);

            return pt;
        }

        #endregion

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Geometry value.
        /// </summary>
        private GPGeometry _value;

        #endregion
    }
}
