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
