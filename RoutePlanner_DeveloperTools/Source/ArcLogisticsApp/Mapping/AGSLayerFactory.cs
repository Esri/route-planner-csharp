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

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// MapLayerFactory class.
    /// </summary>
    internal class AgsLayerFactory
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // service type names
        private const string DYNAMIC_SERVICE = "ArcGISDynamic";
        private const string CACHED_SERVICE = "ArcGISCached";

        #endregion constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private AgsLayerFactory()
        { }

        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static AgsLayer CreateLayer(AgsServer server, MapLayer layer)
        {
            if (layer.MapServiceInfo.Type == null)
                throw new SettingsException((string)App.Current.FindResource("InvalidMapLayerType"));

            AgsLayer agsLayer = null;
            if (layer.MapServiceInfo.Type.Equals(DYNAMIC_SERVICE,
                StringComparison.OrdinalIgnoreCase))
            {
                agsLayer = new AgsDynamicLayer(server, layer);
            }
            else if (layer.MapServiceInfo.Type.Equals(CACHED_SERVICE,
                StringComparison.OrdinalIgnoreCase))
            {
                agsLayer = new AgsCachedLayer(server, layer);
            }
            else
                throw new SettingsException((string)App.Current.FindResource("UnknownMapLayerType"));

            return agsLayer;
        }

        #endregion public methods
    }
}
