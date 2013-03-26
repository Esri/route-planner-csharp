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
using ESRI.ArcLogistics.App.GraphicObjects;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcLogistics.App.Controls;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Symbols;
using System.Windows.Media;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class that implement clustering issues
    /// </summary>
    class Clustering
    {
        #region constants

        // Minimal distance from cluster center to expanded elements
        private const int MINIMAL_EXPANDED_CLUSTER_RADIUS = 30;
        private const int DIST_BETWEEN_EXPANDED_SYMBOLS = 30;
        // Area in which cluster will not unexpand
        private const int ADDITIONAL_EXPANDED_AREA = 30;
        private const int CLUSTER_RADIUS_IN_PIXELS = 10;
        private const int MAXIMUM_GRAPHICS_IN_CIRCLE_EXPANDED = 10;

        private const double START_DELTA_RADIUS = 4;
        private const double DELTA_RADIUS_MULTIPLIER = 0.98;

        private const double START_ANGLE = 0;
        private const double START_DELTA_ANGLE = Math.PI * 50 / 180;
        private const double DELTA_ANGLE_MULTIPLIER = 0.97;

        #endregion

        #region constructors

        public Clustering(MapControl mapctrl)
        {
            _mapctrl = mapctrl;
            _Init();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Create clusterer for graphics layer
        /// </summary>
        /// <returns>Created clusterer</returns>
        public ALClusterer GetClusterer()
        {
            ALClusterer clusterer = new ALClusterer();
            clusterer.Radius = CLUSTER_RADIUS_IN_PIXELS;
            clusterer.OnClusteringComplete += new EventHandler(clusterer_OnClusteringComplete);
            return clusterer;
        }

        /// <summary>
        /// Check mouse cursor position and unexpand if needed
        /// </summary>
        /// <param name="position">Mouse cursor position</param>
        internal void UnexpandIfNeeded(System.Windows.Point position)
        {
            if (ClusterExpanded)
            {
                double dx = _clusterCenter.Value.X - position.X;
                double dy = _clusterCenter.Value.Y - position.Y;
                if (Math.Sqrt(dx * dx + dy * dy) > _expandedAreaRadius)
                    _UnexpandCluster();
            }
        }

        /// <summary>
        /// Unexpand cluster if expanded
        /// </summary>
        internal void UnexpandIfExpanded()
        {
            if (ClusterExpanded)
                _UnexpandCluster();
        }

        /// <summary>
        /// Expand cluster if mouse on graphic and it can be expanded
        /// </summary>
        /// <param name="graphic">Graphic under mouse</param>
        internal void ExpandIfNeeded(Graphic graphic)
        {
            if (!ClusterExpanded)
            {
                ClusterGraphicObject clusterGraphic = graphic as ClusterGraphicObject;
                if (clusterGraphic != null)
                {
                    _ExpandCluster(clusterGraphic);
                }
            }
        }

        /// <summary>
        /// Select element in cluster
        /// </summary>
        /// <param name="data">Element to select</param>
        internal void AddToSelection(object data)
        {
            if (_clusteringLayerColl != null && _clusteringLayerColl.Contains(data))
                _clusteringLayer.SelectedItems.Add(data);
        }

        /// <summary>
        /// Select element in cluster
        /// </summary>
        /// <param name="data">Element to unselect</param>
        internal void RemoveFromSelection(object data)
        {
            if (_clusteringLayerColl != null && _clusteringLayerColl.Contains(data))
                _clusteringLayer.SelectedItems.Remove(data);
        }

        public IList<object> GetClusteredData(ClusterGraphicObject clusterGraphic)
        {
            List<object> clusteredData = new List<object>();
            
            int count = (int)clusterGraphic.Attributes[ALClusterer.COUNT_PROPERTY_NAME];
            for (int index = 0; index < count; index++)
            {
                string attributeName = ALClusterer.GRAPHIC_PROPERTY_NAME + index.ToString();
                DataGraphicObject dataGraphic = (DataGraphicObject)clusterGraphic.Attributes[attributeName];
                clusteredData.Add(dataGraphic.Data);
            }

            return clusteredData;
        }

        /// <summary>
        /// Add elements from cluster and not in frame
        /// </summary>
        /// <param name="objectLayer">Object layer to find cluster</param>
        /// <param name="frame">Selection fram envelope</param>
        /// <param name="elementsInFrame">Already selected elements list</param>
        public void AddElementsFromClusterAndNotInFrame(ObjectLayer objectLayer, Envelope frame, IList<object> elementsInFrame)
        {
            foreach (ClusterGraphicObject clusterGraphic in ClusterGraphicObject.Graphics)
            {
                if (MapHelpers.IsIntersects(frame, clusterGraphic.Geometry))
                {
                    IList<object> clusterDataList = GetClusteredData(clusterGraphic);

                    foreach (object data in clusterDataList)
                    {
                        // if graphic not in frame yet, data type is equals to objectLayer data type and data contains in objectayercollection
                        if (data.GetType() == objectLayer.LayerType && !elementsInFrame.Contains(data) &&
                            MapHelpers.CollectionContainsData(objectLayer.Collection, data))
                        {
                            elementsInFrame.Add(data);
                        }
                    }
                }
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Layer that contains expanded cluster objects
        /// </summary>
        public ObjectLayer ClusteringLayer
        {
            get { return _clusteringLayer; }
        }

        /// <summary>
        /// Layer that contains leader lines
        /// </summary>
        public GraphicsLayer LeaderLinesLayer
        {
            get { return _leaderLinesLayer; }
        }

        /// <summary>
        /// If cluster expanded returns true and false othetwise
        /// </summary>
        public bool ClusterExpanded
        {
            get { return _expandedClusterGraphic != null; }
        }

        public ClusterGraphicObject ExpandedClusterGraphic
        {
            get { return _expandedClusterGraphic; }
        }

        #endregion public properties

        #region private methods

        /// <summary>
        /// Do initialisation
        /// </summary>
        private void _Init()
        {
            _clusteringLayerColl = new ObservableCollection<object>();

            _clusteringLayer = new ObjectLayer(_clusteringLayerColl, typeof(object), false);
            _clusteringLayer.EnableToolTip();
            _clusteringLayer.Selectable = true;
            _clusteringLayer.ConstantOpacity = true;

            _leaderLinesLayer = new GraphicsLayer();
        }

        /// <summary>
        /// Expand cluster from cluster graphic
        /// </summary>
        /// <param name="clusterGraphic">Cluster graphic to expand</param>
        private void _ExpandCluster(ClusterGraphicObject clusterGraphic)
        {
            if (_mapctrl.IsInEditedMode || _MaxGraphicsToExpandExceeded(clusterGraphic))
                return;

            if (!_mapctrl.IsInEditedMode)
                _mapctrl.SetOpacityToLayers(MapControl.HalfOpacity);

            _expandedClusterGraphic = clusterGraphic;
            _clusteringLayer.Visible = true;
            int count = (int)clusterGraphic.Attributes[ALClusterer.COUNT_PROPERTY_NAME];
            Debug.Assert(count > 0);

            for (int index = 0; index < count; index++)
            {
                string attrKey = ALClusterer.GRAPHIC_PROPERTY_NAME + index.ToString();
                DataGraphicObject grObj = (DataGraphicObject)clusterGraphic.Attributes[attrKey];
                _clusteringLayerColl.Add(grObj.Data);

                // support already selected items
                if (_mapctrl.SelectedItems.Contains(grObj.Data))
                    _clusteringLayer.SelectedItems.Add(grObj.Data);
            }
            _clusterCenter = _mapctrl.map.MapToScreen((MapPoint)clusterGraphic.Geometry);

            if (_clusteringLayer.MapLayer.Graphics.Count > MAXIMUM_GRAPHICS_IN_CIRCLE_EXPANDED)
                _SetSpiraledPositionsToExpandedClusterGraphics();
            else
                _SetCircledPositionsToExpandedClusterGraphics();

            _CreateLeaderLines();

            _expandedClusterGraphic.SetZIndex(ObjectLayer.FRONTZINDEX);
        }

        /// <summary>
        /// Set map positions to graphics in clustering layer by circle
        /// </summary>
        private void _SetCircledPositionsToExpandedClusterGraphics()
        {
            int count = _clusteringLayer.MapLayer.Graphics.Count;

            // do visual expand
            double sinus = Math.Sin(Math.PI / count);
            double radius = DIST_BETWEEN_EXPANDED_SYMBOLS / (2 * sinus);

            if (radius < MINIMAL_EXPANDED_CLUSTER_RADIUS)
                radius = MINIMAL_EXPANDED_CLUSTER_RADIUS;

            _expandedAreaRadius = radius + ADDITIONAL_EXPANDED_AREA;

            for (int index = 0; index < count; index++)
            {
                double angle = (2 * Math.PI / count) * index;
                double dx = radius * Math.Cos(angle);
                double dy = radius * Math.Sin(angle);
                DataGraphicObject graphic = (DataGraphicObject)_clusteringLayer.MapLayer.Graphics[index];
                MarkerSymbol symbol = (MarkerSymbol)graphic.Symbol;

                System.Windows.Point symbolPoint = _mapctrl.map.MapToScreen((MapPoint)graphic.Geometry);

                symbol.OffsetX = dx - _clusterCenter.Value.X + symbolPoint.X;
                symbol.OffsetY = dy - _clusterCenter.Value.Y + symbolPoint.Y;

                graphic.SetZIndex(ObjectLayer.FRONTZINDEX);
            }
        }

        /// <summary>
        /// Set map positions to graphics in clustering layer by spiral
        /// </summary>
        /// <param name="count"></param>
        private void _SetSpiraledPositionsToExpandedClusterGraphics()
        {
            int count = _clusteringLayer.MapLayer.Graphics.Count;
            double radius = MINIMAL_EXPANDED_CLUSTER_RADIUS;
            double dRadius = START_DELTA_RADIUS;

            double angle = START_ANGLE;
            double dAngle = START_DELTA_ANGLE;

            for (int index = 0; index < count; index++)
            {
                double dx = radius * Math.Cos(angle);
                double dy = radius * Math.Sin(angle);
                DataGraphicObject graphic = (DataGraphicObject)_clusteringLayer.MapLayer.Graphics[index];
                MarkerSymbol symbol = (MarkerSymbol)graphic.Symbol;

                System.Windows.Point symbolPoint = _mapctrl.map.MapToScreen((MapPoint)graphic.Geometry);

                symbol.OffsetX = -(dx + _clusterCenter.Value.X - symbolPoint.X);
                symbol.OffsetY = -(dy + _clusterCenter.Value.Y - symbolPoint.Y);

                graphic.SetZIndex(ObjectLayer.FRONTZINDEX);

                radius += dRadius;
                dRadius *= DELTA_RADIUS_MULTIPLIER;

                angle += dAngle;
                dAngle *= DELTA_ANGLE_MULTIPLIER;
            }

            _expandedAreaRadius = radius + ADDITIONAL_EXPANDED_AREA;
        }
        
        /// <summary>
        /// Add leader lines to expanded cluster elements
        /// </summary>
        private void _CreateLeaderLines()
        {
            List<Graphic> _leaderLines = new List<Graphic>();

            foreach (DataGraphicObject dataGraphic in _clusteringLayer.MapLayer.Graphics)
            {
                // TODO: remove hardcode
                LineSymbol simpleLineSymbol = new LineSymbol()
                {
                    Color = (SolidColorBrush)App.Current.FindResource("ClusteringLineBrush"),
                    Width = 2
                };

                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();

                MapPoint graphicPosition = dataGraphic.Geometry as MapPoint;
                MapPoint startPoint = (MapPoint)_expandedClusterGraphic.Geometry;
                System.Windows.Point point = _mapctrl.map.MapToScreen(startPoint);

                MarkerSymbol symbol = dataGraphic.Symbol as MarkerSymbol;
                point.X -= symbol.OffsetX;
                point.Y -= symbol.OffsetY;

                MapPoint endPoint = _mapctrl.map.ScreenToMap(point);
                endPoint.X -= startPoint.X - graphicPosition.X;
                endPoint.Y -= startPoint.Y - graphicPosition.Y; 

                points.Add(startPoint);
                points.Add(endPoint);

                Polyline lineGeometry = new Polyline();
                lineGeometry.Paths.Add(points);

                Graphic lineGraphic = new Graphic()
                {
                    Symbol = simpleLineSymbol,
                    Geometry = lineGeometry
                };

                _leaderLines.Add(lineGraphic);
            }

            foreach (Graphic graphic in _leaderLines)
                _leaderLinesLayer.Graphics.Add(graphic);
        }

        /// <summary>
        /// Check graphics count
        /// </summary>
        /// <param name="clusterGraphic">Cluster graphic to expand</param>
        /// <returns>True if exceeded</returns>
        private bool _MaxGraphicsToExpandExceeded(ClusterGraphicObject clusterGraphic)
        {
            IList<object> clusterData = GetClusteredData(clusterGraphic);
            ObjectLayer objectLayer = MapHelpers.GetLayerWithData(clusterData[0], _mapctrl.ObjectLayers);
            FlareClusterer clusterer = (FlareClusterer)objectLayer.MapLayer.Clusterer;
            int objectsInCluster = (int)clusterGraphic.Attributes[ALClusterer.COUNT_PROPERTY_NAME];

            bool exceeded = objectsInCluster > clusterer.MaximumFlareCount;
            return exceeded;
        }

        /// <summary>
        /// Unexpand cluster
        /// </summary>
        private void _UnexpandCluster()
        {
            if (!_mapctrl.IsInEditedMode)
                _mapctrl.SetOpacityToLayers(MapControl.FullOpacity);

            Debug.Assert(ClusterExpanded);
            _expandedClusterGraphic.SetZIndex(ObjectLayer.BACKZINDEX);
            _expandedClusterGraphic = null;
            _clusterCenter = null;
            _clusteringLayerColl.Clear();
            _clusteringLayer.SelectedItems.Clear();
            _leaderLinesLayer.Graphics.Clear();
        }

        /// <summary>
        /// React on clustering complete
        /// </summary>
        private void clusterer_OnClusteringComplete(object sender, EventArgs e)
        {
            if (ClusterExpanded)
                _UnexpandCluster();
        }

        #endregion

        #region private members

        private MapControl _mapctrl;
        private ObjectLayer _clusteringLayer;
        private GraphicsLayer _leaderLinesLayer;
        private ObservableCollection<object> _clusteringLayerColl;
        private System.Windows.Point? _clusterCenter;
        private double _expandedAreaRadius;
        private ClusterGraphicObject _expandedClusterGraphic;

        #endregion
    }
}
