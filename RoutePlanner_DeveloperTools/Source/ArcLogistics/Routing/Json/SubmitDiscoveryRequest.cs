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

namespace ESRI.ArcLogistics.Routing.Json
{
    /// <summary>
    /// Format for a discovery request.
    /// Format description can be found at this url:
    /// http://networkanalysis.arcgisonline.com/arcgis/sdk/rest/index.html
    /// </summary>
    internal class SubmitDiscoveryRequest
    {
        /// <summary>
        /// The response format.
        /// </summary>
        [QueryParameter(Name = "f")]
        public string ResponseFormat
        {
            get;
            set;
        }

        /// <summary>
        /// The geometry to identify on.
        /// </summary>
        [QueryParameter(Name = "geometry")]
        public GPGeometry Geometry
        {
            get;
            set;
        }

        /// <summary>
        /// The type of geometry specified by the geometry parameter.
        /// </summary>
        [QueryParameter(Name = "geometryType")]
        public string GeometryType
        {
            get;
            set;
        }

        /// <summary>
        /// Output and input spatial reference.
        /// </summary>
        [QueryParameter(Name = "sr")]
        public int SpatialReference
        {
            get;
            set;
        }

        /// <summary>
        /// Definition expression for filtering the features
        /// of individual layers in the exported map.
        /// </summary>
        [QueryParameter(Name = "layerDefs")]
        public string LayerDefinitions
        {
            get;
            set;
        }

        /// <summary>
        ///  The time instant or the time extent
        ///  of the features to be identified.
        /// </summary>
        [QueryParameter(Name = "time")]
        public string Time
        {
            get;
            set;
        }

        /// <summary>
        /// The time options per layer.
        /// </summary>
        [QueryParameter(Name = "layerTimeOptions")]
        public string LayerTimeOptions
        {
            get;
            set;
        }

        /// <summary>
        /// The layers to perform the identify operation on.
        /// </summary>
        [QueryParameter(Name = "layers")]
        public string Layers
        {
            get;
            set;
        }

        /// <summary>
        /// The distance in screen pixels from the specified geometry
        /// within which the identify should be performed.
        /// </summary>
        [QueryParameter(Name = "tolerance")]
        public int Tolerance
        {
            get;
            set;
        }

        /// <summary>
        /// The extent or bounding box of the map currently being viewed.
        /// </summary>
        [QueryParameter(Name = "mapExtent")]
        public string MapExtent
        {
            get;
            set;
        }

        /// <summary>
        /// The screen image display parameters (width, height and DPI)
        /// of the map being currently viewed.
        /// </summary>
        [QueryParameter(Name = "imageDisplay")]
        public string ImageDisplay
        {
            get;
            set;
        }
        /// <summary>
        /// If true, the resultset will include
        /// the geometries associated with each result.
        /// </summary>
        [QueryParameter(Name = "returnGeometry")]
        public bool ReturnGeometry
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum allowable offset to be used for generalizing
        /// geometries returned by the identify operation.
        /// </summary>
        [QueryParameter(Name = "maxAllowableOffset")]
        public string MaxAllowableOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Property allows to reorder layers and
        /// change the layer data source.
        /// </summary>
        [QueryParameter(Name = "dynamicLayers")]
        public string DynamicLayers
        {
            get;
            set;
        }

        /// <summary>
        /// Is need to include Z values to results.
        /// Applies if returnGeometry is true.
        /// </summary>
        [QueryParameter(Name = "returnZ")]
        public bool ReturnZ
        {
            get;
            set;
        }

        /// <summary>
        /// Is need to include M values to results.
        /// Applies if returnGeometry is true.
        /// </summary>
        [QueryParameter(Name = "returnM")]
        public bool ReturnM
        {
            get;
            set;
        }
    }
}
