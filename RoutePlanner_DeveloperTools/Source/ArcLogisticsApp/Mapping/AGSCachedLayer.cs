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
