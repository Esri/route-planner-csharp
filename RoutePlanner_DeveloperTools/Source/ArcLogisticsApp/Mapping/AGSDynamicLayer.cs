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
    /// AGSDynamicLayer class.
    /// </summary>
    internal class AgsDynamicLayer : AgsLayer
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public AgsDynamicLayer(AgsServer server, MapLayer layer)
            : base(layer)
        {
            Debug.Assert(layer.MapServiceInfo != null);

            LayerType = AgsLayerType.Dynamic;
            Server = server;

            if (String.IsNullOrEmpty(layer.MapServiceInfo.Url))
                throw new SettingsException((string)App.Current.FindResource("InvalidMapLayerURL"));

            // format REST URL
            string restUrl = FormatRestUrl(layer.MapServiceInfo.Url);
            if (restUrl == null)
                throw new SettingsException((string)App.Current.FindResource("FailedFormatRESTURL"));

            // create ArcGIS layer
            ArcGISDynamicMapServiceLayer arcGISDynamicMapServiceLayer = new ArcGISDynamicMapServiceLayer();

            arcGISDynamicMapServiceLayer.ID = "map";
            arcGISDynamicMapServiceLayer.Url = restUrl;
            arcGISDynamicMapServiceLayer.Visible = layer.MapServiceInfo.IsVisible;
            arcGISDynamicMapServiceLayer.Opacity = layer.MapServiceInfo.Opacity;

            ArcGISLayer = arcGISDynamicMapServiceLayer;

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
