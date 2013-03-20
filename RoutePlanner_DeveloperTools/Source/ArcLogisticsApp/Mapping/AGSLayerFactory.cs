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
