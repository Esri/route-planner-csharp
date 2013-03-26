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
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// GeometryHelper class.
    /// </summary>
    internal static class GeometryHelper
    {
        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get rect from extent envelope
        /// </summary>
        /// <param name="envelope">Extent envelope in map spatial reference</param>
        /// <param name="spatialReferenceID">Map spatial reference ID</param>
        /// <returns>Extent envelope in WGS84</returns>
        public static ESRI.ArcLogistics.Geometry.Envelope CreateRect(ESRI.ArcGIS.Client.Geometry.Envelope env,
            int? spatialReferenceID)
        {
            // Project extent to map spatial reference
            ESRI.ArcLogistics.Geometry.Point leftTop = new ESRI.ArcLogistics.Geometry.Point(env.XMin, env.YMax);
            ESRI.ArcLogistics.Geometry.Point rightBottom = new ESRI.ArcLogistics.Geometry.Point(env.XMax, env.YMin);

            // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator
            if (spatialReferenceID.HasValue)
            {
                leftTop = WebMercatorUtil.ProjectPointFromWebMercator(leftTop, spatialReferenceID.Value);
                rightBottom = WebMercatorUtil.ProjectPointFromWebMercator(rightBottom, spatialReferenceID.Value);
            }

            ESRI.ArcLogistics.Geometry.Envelope rect = new ESRI.ArcLogistics.Geometry.Envelope(
                leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);

            return rect;
        }

        /// <summary>
        /// Create extent envelope
        /// </summary>
        /// <param name="envelope">Extent envelope in WGS84</param>
        /// <param name="spatialReferenceID">Map spatial reference ID</param>
        /// <returns>Extent envelope in map spatial reference</returns>
        public static ESRI.ArcGIS.Client.Geometry.Envelope CreateExtent(ESRI.ArcLogistics.Geometry.Envelope envelope,
            int? spatialReferenceID)
        {
            // Project extent to map spatial reference
            ESRI.ArcLogistics.Geometry.Point leftTop = new ESRI.ArcLogistics.Geometry.Point(envelope.left, envelope.top);
            ESRI.ArcLogistics.Geometry.Point rightBottom = new ESRI.ArcLogistics.Geometry.Point(envelope.right, envelope.bottom);

            // Project point from WGS84 to Web Mercator if spatial reference of map is Web Mercator
            if (spatialReferenceID.HasValue)
            {
                leftTop = WebMercatorUtil.ProjectPointToWebMercator(leftTop, spatialReferenceID.Value);
                rightBottom = WebMercatorUtil.ProjectPointToWebMercator(rightBottom, spatialReferenceID.Value);
            }

            // Create map extent envelope
            ESRI.ArcGIS.Client.Geometry.Envelope extent = new ESRI.ArcGIS.Client.Geometry.Envelope();

            extent.XMin = leftTop.X;
            extent.XMax = rightBottom.X;
            extent.YMin = rightBottom.Y;
            extent.YMax = leftTop.Y;

            // Project point from WGS84 to Web Mercator if spatial reference of map is Web Mercator
            if (spatialReferenceID.HasValue)
            {
                extent.SpatialReference = new ESRI.ArcGIS.Client.Geometry.SpatialReference(spatialReferenceID.Value);
            }

            return extent;
        }

        #endregion public methods
    }
}
