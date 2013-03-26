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
using ESRI.ArcLogistics.Services;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// AGSCachedLayer class.
    /// </summary>
    internal class AgsCachedLayer : AgsLayer
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AgsCachedLayer(AgsServer server, MapLayer layer)
            : base(layer)
        {
            Debug.Assert(layer.MapServiceInfo != null);

            LayerType = AgsLayerType.Cached;
            Server = server;

            if (String.IsNullOrEmpty(layer.MapServiceInfo.Url))
                throw new SettingsException((string)App.Current.FindResource("InvalidMapLayerURL"));

            // format REST URL
            string restUrl = FormatRestUrl(layer.MapServiceInfo.Url);
            if (restUrl == null)
                throw new SettingsException((string)App.Current.FindResource("FailedFormatRESTURL"));

            // create ArcGIS layer
            ArcGISTiledMapServiceLayer arcGISTiledMapServiceLayer = new ArcGISTiledMapServiceLayer();
            arcGISTiledMapServiceLayer.ID = "map";
            arcGISTiledMapServiceLayer.Url = restUrl;
            arcGISTiledMapServiceLayer.Visible = layer.MapServiceInfo.IsVisible;
            arcGISTiledMapServiceLayer.Opacity = layer.MapServiceInfo.Opacity;
            ArcGISLayer = arcGISTiledMapServiceLayer;

            UpdateTokenIfNeeded();
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override bool IsVisible
        {
            get { return ArcGISLayer.Visible; }
            set { ArcGISLayer.Visible = value; }
        }

        public override double Opacity
        {
            get { return ArcGISLayer.Opacity; }
            set { ArcGISLayer.Opacity = value; }
        }

        #endregion public properties
    }
}
