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
using System.Globalization;
using System.Linq;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Routing.Json;

using Math = System.Math;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// Geometry type for barriers.
    /// </summary>
    internal enum BarrierGeometryType
    {
        /// <summary>
        /// Geometry is absent or not determined.
        /// </summary>
        None,

        /// <summary>
        /// Geometry point.
        /// </summary>
        Point,

        /// <summary>
        /// Geometry polygon.
        /// </summary>
        Polygon,

        /// <summary>
        /// Geometry polyline.
        /// </summary>
        Polyline
    }

    /// <summary>
    /// Class-converter from Barrier collection to GP layer of feature collection.
    /// </summary>
    internal class BarriersConverter
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="attributes">Collection of network attributes.</param>
        public BarriersConverter(IEnumerable<NetworkAttribute> attributes)
        {
            _attributes = attributes;
        }

        #endregion

        #region Public static methods

        /// <summary>
        /// Method determines barrier geometry type of Barrier.
        /// </summary>
        /// <param name="barriers">Barrier.</param>
        /// <returns>Barrier geometry type.</returns>
        public static BarrierGeometryType DetermineBarrierType(Barrier barrier)
        {
            Debug.Assert(barrier != null);

            return _GetBarrierType(barrier);
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Converts collection of Barriers to GPFeatureRecordSetLayer of Point barriers.
        /// </summary>
        /// <param name="barriers">Collection of Point barriers to convert.</param>
        /// <param name="solverSR">Reference to GP spatial.</param>
        /// <returns>GPFeature RecordSet Layer of Point barriers.</returns>
        public GPFeatureRecordSetLayer ConvertToPointBarriersLayer(IEnumerable<Barrier> barriers,
            GPSpatialReference solverSR)
        {
            Debug.Assert(barriers != null);

            return _GenerateLayer(barriers, BarrierGeometryType.Point, solverSR);
        }

        /// <summary>
        /// Converts collection of Barriers to GPFeatureRecordSetLayer of Polyline barriers.
        /// </summary>
        /// <param name="barriers">Collection of Polyline barriers to convert.</param>
        /// <param name="solverSR">Reference to GP spatial.</param>
        /// <returns>GPFeature RecordSet Layer of Polyline barriers.</returns>
        public GPFeatureRecordSetLayer ConvertToLineBarriersLayer(IEnumerable<Barrier> barriers,
            GPSpatialReference solverSR)
        {
            Debug.Assert(barriers != null);

            return _GenerateLayer(barriers, BarrierGeometryType.Polyline, solverSR);
        }

        /// <summary>
        /// Converts collection of Barriers to GPFeatureRecordSetLayer of Polygon barriers.
        /// </summary>
        /// <param name="barriers">Collection of polygon barriers to convert.</param>
        /// <param name="solverSR">Reference to GP spatial.</param>
        /// <returns>GPFeature RecordSet Layer of Polygon barriers.</returns>
        public GPFeatureRecordSetLayer ConvertToPolygonBarriersLayer(IEnumerable<Barrier> barriers,
            GPSpatialReference solverSR)
        {
            Debug.Assert(barriers != null);

            return _GenerateLayer(barriers, BarrierGeometryType.Polygon, solverSR);
        }

        /// <summary>
        /// Convert collection of Barriers to GPFeatures collection of Point barriers.
        /// </summary>
        /// <param name="barriers">Collection of point barriers to convert.</param>
        /// <returns>Collection of Point Features.</returns>
        public GPFeature[] ConvertToPointBarriersFeatures(IEnumerable<Barrier> barriers)
        {
            Debug.Assert(barriers != null);

            return _ConvertBarriers(barriers, BarrierGeometryType.Point);
        }

        /// <summary>
        /// Convert collection of Barriers to GPFeatures collection of Polyline barriers.
        /// </summary>
        /// <param name="barriers">Collection of polyline barriers to convert.</param>
        /// <returns>Collection of Polyline Features.</returns>
        public GPFeature[] ConvertToLineBarriersFeatures(IEnumerable<Barrier> barriers)
        {
            Debug.Assert(barriers != null);

            return _ConvertBarriers(barriers, BarrierGeometryType.Polyline);
        }

        /// <summary>
        /// Convert collection of Barriers to GPFeatures collection of Polygon barriers.
        /// </summary>
        /// <param name="barriers">Collection of polygon barriers to convert.</param>
        /// <returns>Collection of Polygon Features.</returns>
        public GPFeature[] ConvertToPolygonBarriersFeatures(IEnumerable<Barrier> barriers)
        {
            Debug.Assert(barriers != null);

            return _ConvertBarriers(barriers, BarrierGeometryType.Polygon);
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// Determine barrier geometry type.
        /// </summary>
        /// <param name="barrier">Barrier.</param>
        /// <returns>Barrier geometry type.</returns>
        private static BarrierGeometryType _GetBarrierType(Barrier barrier)
        {
            Debug.Assert(barrier != null);

            BarrierGeometryType result = BarrierGeometryType.None;

            if (barrier.Geometry != null)
            {
                if (barrier.Geometry is Point)
                    result = BarrierGeometryType.Point;
                else if (barrier.Geometry is Polygon)
                    result = BarrierGeometryType.Polygon;
                else if (barrier.Geometry is Polyline)
                    result = BarrierGeometryType.Polyline;
                else
                {
                    // Not supported.
                    Debug.Assert(false);
                }
            }

            return result;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Generate GPFeatureRecordSetLayer of specified barrier type.
        /// </summary>
        /// <param name="barriers">Collection of Typed Barriers to include into layer.</param>
        /// <param name="type">Type of barriers to convert in.</param>
        /// <param name="solverSR">Reference to GP spatial.</param>
        /// <returns>GPFeatureRecordSetLayer of barriers of specified type.</returns>
        private GPFeatureRecordSetLayer _GenerateLayer(IEnumerable<Barrier> barriers,
            BarrierGeometryType type, GPSpatialReference solverSR)
        {
            Debug.Assert(barriers != null);

            GPFeatureRecordSetLayer layer = null;

            GPFeature[] features = _ConvertBarriers(barriers, type);

            if (features.Length > 0)
            {
                layer = new GPFeatureRecordSetLayer();
                layer.SpatialReference = solverSR;
                layer.Features = features;

                switch (type)
                {
                    case BarrierGeometryType.Point:
                        layer.GeometryType = NAGeometryType.esriGeometryPoint;
                        break;
                    case BarrierGeometryType.Polygon:
                        layer.GeometryType = NAGeometryType.esriGeometryPolygon;
                        break;
                    case BarrierGeometryType.Polyline:
                        layer.GeometryType = NAGeometryType.esriGeometryPolyline;
                        break;
                    case BarrierGeometryType.None:
                        // Not supported type.
                        Debug.Assert(false);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            return layer;
        }

        /// <summary>
        /// Convert collection of baTypedBarriers to collection of GPFeature of specified
        /// geometry type.
        /// </summary>
        /// <param name="barriers">Collection of TypedBarriers to convert.</param>
        /// <param name="type">Geometry type to convert in.</param>
        /// <returns>Collection of barrier feature with geometry type.</returns>
        private GPFeature[] _ConvertBarriers(IEnumerable<Barrier> barriers,
            BarrierGeometryType type)
        {
            Debug.Assert(barriers != null);

            List<GPFeature> features = new List<GPFeature>();

            foreach (Barrier barrier in barriers)
            {
                if(_CanConvert(barrier, type))
                    features.Add(_ConvertBarrier(barrier, type));
                else
                {
                    // Type should appropriate to barrier, so it should be convertable.
                    Debug.Assert(false);
                }
            }

            return features.ToArray();
        }

        /// <summary>
        /// Determine if barrier can be converted into specified type.
        /// </summary>
        /// <param name="barrier">Barrier to convert from.</param>
        /// <param name="type">Type to convert.</param>
        /// <returns>True - if can be converted, otherwise - false.</returns>
        private bool _CanConvert(Barrier barrier, BarrierGeometryType type)
        {
            Debug.Assert(barrier != null);

            bool result = false;

            if (barrier.Geometry != null)
            {
                if ((type == BarrierGeometryType.Point && barrier.Geometry is Point) ||
                    (type == BarrierGeometryType.Polygon && barrier.Geometry is Polygon) ||
                    (type == BarrierGeometryType.Polyline && barrier.Geometry is Polyline))
                    result = true;
                else
                    // Not supported.
                    Debug.Assert(false);
            }

            return result;
        }

        /// <summary>
        /// Convert collection of Barriers to GPFeature collection of specified type.
        /// </summary>
        /// <param name="barrier">Barriers collection to convert.</param>
        /// param name="type">Type of barriers to convert in.</param>
        /// <returns>GPFeature of specified type. barrier.</returns>
        private GPFeature _ConvertBarrier(Barrier barrier, BarrierGeometryType type)
        {
            Debug.Assert(barrier != null);

            GPFeature feature = new GPFeature();

            if (type == BarrierGeometryType.Point)
                feature = _ConvertToPointFeature(barrier);
            else if (type == BarrierGeometryType.Polygon)
                feature = _ConvertToPolygonFeature(barrier);
            else if (type == BarrierGeometryType.Polyline)
                feature = _ConvertToPolylineFeature(barrier);
            else
            {
                // Not supported.
                Debug.Assert(false);
            }
            
            return feature;
        }

        /// <summary>
        /// Convert Point Barrier into GPFeature.
        /// </summary>
        /// <param name="barrier">Barrier to convert</param>
        /// <returns>Point barrier GPFeature</returns>
        private GPFeature _ConvertToPointFeature(Barrier barrier)
        {
            Debug.Assert(barrier != null);

            GPFeature feature = new GPFeature();
            feature.Geometry = new GeometryHolder();

            // Convert geometry.
            feature.Geometry.Value = GPObjectHelper.PointToGPPoint((Point)barrier.Geometry);

            // Fill attributes.
            feature.Attributes = _GetAttributes(barrier, BarrierGeometryType.Point);

            return feature;
        }

        /// <summary>
        /// Convert Polygon Barrier into GPFeature.
        /// </summary>
        /// <param name="barrier">Barrier to convert</param>
        /// <returns>Polygon barrier GPFeature</returns>
        private GPFeature _ConvertToPolygonFeature(Barrier barrier)
        {
            Debug.Assert(barrier != null);

            GPFeature feature = new GPFeature();
            feature.Geometry = new GeometryHolder();

            // Convert geometry.
            feature.Geometry.Value = GPObjectHelper.PolygonToGPPolygon(
                (Polygon)barrier.Geometry);

            // Fill attributes.
            feature.Attributes = _GetAttributes(barrier, BarrierGeometryType.Polygon);

            return feature;
        }

        /// <summary>
        /// Convert Polyline Barrier into GPFeature.
        /// </summary>
        /// <param name="barrier">Barrier to convert</param>
        /// <returns>Barrier GPFeature</returns>
        private GPFeature _ConvertToPolylineFeature(Barrier barrier)
        {
            Debug.Assert(barrier != null);

            GPFeature feature = new GPFeature();
            feature.Geometry = new GeometryHolder();

            // Convert geometry.
            feature.Geometry.Value = GPObjectHelper.PolylineToGPPolyline(
                    (Polyline)barrier.Geometry);

            // Fill attributes.
            feature.Attributes = _GetAttributes(barrier, BarrierGeometryType.Polyline);

            return feature;
        }

        /// <summary>
        /// Method gets attributes for barriers. 
        /// </summary>
        /// <param name="barrier">Barrier to set attributes for.</param>
        /// <param name="barrierType">Barrier type.</param>
        /// <returns>Attributes for barrier.</returns>
        private AttrDictionary _GetAttributes(Barrier barrier,
            BarrierGeometryType barrierType)
        {
            // Fill common barriers attributes.
            var attr = new AttrDictionary();
            attr.Add(NAAttribute.NAME, barrier.Id.ToString());

            // Adjust type and time attributes for Polygon barriers.
            if (barrierType == BarrierGeometryType.Polygon)
            {
                if (barrier.BarrierEffect.BlockTravel == false)
                    attr.Add(NAAttribute.BarrierType, (int)NABarrierType.esriBarrierScaledCost);
                else
                    attr.Add(NAAttribute.BarrierType, (int)NABarrierType.esriBarrierRestriction);

                double timeFactor = _GetTimeFactor(barrier.BarrierEffect.SpeedFactorInPercent);

                attr.Add(NAAttribute.AttributeTimeScale, timeFactor);
            }
            // Adjust type and time attributes for Polyline barriers.
            else if (barrierType == BarrierGeometryType.Polyline)
            {
                attr.Add(NAAttribute.BarrierType, (int)NABarrierType.esriBarrierRestriction);
            }
            // Adjust type and time attributes for Point barriers.
            else if (barrierType == BarrierGeometryType.Point)
            {
                if (barrier.BarrierEffect.BlockTravel == false)
                    attr.Add(NAAttribute.BarrierType, (int)NABarrierType.esriBarrierAddedCost);
                else
                    attr.Add(NAAttribute.BarrierType, (int)NABarrierType.esriBarrierRestriction);

                attr.Add(NAAttribute.FullEdge, false);
                attr.Add(NAAttribute.CURB_APPROACH,
                    (int)NACurbApproachType.esriNAEitherSideOfVehicle);

                attr.Add(NAAttribute.AttributeTimeDelay, barrier.BarrierEffect.DelayTime);
            }
            else
                // Not supported type of barrier.
                Debug.Assert(false);

            return attr;
        }

        /// <summary>
        /// Method converts speedfactor value, which set by user for polygon
        /// and polyline barriers, from percents to double value multiplier.
        /// See: http://help.arcgis.com/en/arcgisdesktop/10.0/help/index.html#/Barriers/004700000056000000/
        /// </summary>
        /// <param name="barrierEffect">Barrier Effect selected by user in percents:
        /// Negative for Slow Down operation, positive for Speed Up operation.
        /// </param>
        /// <returns>>Time Factor in ArcGIS format.</returns>
        private double _GetTimeFactor(double barrierEffect)
        {
            double factor = 100 / (100 + barrierEffect);

            return factor;
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Collection of network attributes.
        /// </summary>
        private IEnumerable<NetworkAttribute> _attributes;

        #endregion
    }
}
