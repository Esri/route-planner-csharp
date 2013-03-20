using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
//using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App;
using ESRI.ArcLogistics.App.Symbols;

namespace ESRI.ArcLogistics.App.GraphicObjects
{
    /// <summary>
    /// Graphic object for showing Frames
    /// </summary>
    class FrameGraphicObject : Graphic
    {
        #region constructors

        private FrameGraphicObject(int? spatialReferenceID)
        {
            _spatialReferenceID = spatialReferenceID;
        }

        #endregion constructors

        #region public static methods

        /// <summary>
        /// Create graphic object for Frame
        /// </summary>
        /// <param name="spatialReferenceID">Map spatial reference ID</param>
        /// <returns>Graphic object for Frame</returns>
        public static FrameGraphicObject Create(int? spatialReferenceID)
        {
            // TODO: Remove hardcode
            Color color = Color.FromArgb(100, 180, 180, 255); 

            SimpleFillSymbol simpleFillSymbol = new SimpleFillSymbol()
            {
                BorderThickness = 2,
                BorderBrush = new System.Windows.Media.SolidColorBrush(Colors.Green),
                Fill = new System.Windows.Media.SolidColorBrush(color)
            };

            FrameGraphicObject graphic = new FrameGraphicObject(spatialReferenceID)
            {
                Symbol = simpleFillSymbol,
                Geometry = null
            };

            return graphic;
        }

        #endregion public static methods

        #region private methods

        private ESRI.ArcGIS.Client.Geometry.Geometry _CreateFrameGeometry()
        {
            ESRI.ArcGIS.Client.Geometry.Geometry geometry = null;
            _end = new ESRI.ArcLogistics.Geometry.Point(_start.Value.X, _start.Value.Y);
            if (_start.HasValue && _end.HasValue)
            {
                ESRI.ArcGIS.Client.Geometry.PointCollection pointCollection = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                pointCollection.Add(new MapPoint(_start.Value.X, _start.Value.Y));
                pointCollection.Add(new MapPoint(_start.Value.X, _end.Value.Y));
                pointCollection.Add(new MapPoint(_end.Value.X,   _end.Value.Y));
                pointCollection.Add(new MapPoint(_end.Value.X,   _start.Value.Y));
                pointCollection.Add(new MapPoint(_start.Value.X, _start.Value.Y));

                ESRI.ArcGIS.Client.Geometry.Polygon polygon = new ESRI.ArcGIS.Client.Geometry.Polygon();
                polygon.Rings.Add(pointCollection);

                geometry = (ESRI.ArcGIS.Client.Geometry.Geometry)polygon;
            }

            return geometry;
        }

        #endregion private methods

        #region public members

        /// <summary>
        /// Start point of frame
        /// </summary>
        public ESRI.ArcLogistics.Geometry.Point Start
        {
            get
            {
                return _start.Value;
            }
            set
            {
                _start = new ESRI.ArcLogistics.Geometry.Point(value.X, value.Y);
                Geometry = _CreateFrameGeometry();
            }
        }

        /// <summary>
        /// Start point of frame
        /// </summary>
        public ESRI.ArcLogistics.Geometry.Point End
        {
            get
            {
                return _end.Value;
            }
            set
            {
                _end = new ESRI.ArcLogistics.Geometry.Point(value.X, value.Y);

                ESRI.ArcGIS.Client.Geometry.Polygon polygon = (ESRI.ArcGIS.Client.Geometry.Polygon)Geometry;

                MapPoint endMapPoint = new MapPoint(_end.Value.X, _end.Value.Y);

                polygon.Rings[0][1].Y = _end.Value.Y;
                polygon.Rings[0][2].Y = _end.Value.Y;
                polygon.Rings[0][2].X = _end.Value.X;
                polygon.Rings[0][3].X = _end.Value.X;
            }
        }

        #endregion public members

        #region private members

        private ESRI.ArcLogistics.Geometry.Point? _start = null;
        private ESRI.ArcLogistics.Geometry.Point? _end = null;

        // Map spatial reference ID
        private int? _spatialReferenceID;

        #endregion private members
    }
}
