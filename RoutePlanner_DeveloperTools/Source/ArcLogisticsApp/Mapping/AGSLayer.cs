using System;
using System.Diagnostics;
using ESRI.ArcLogistics.Services;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// AgsLayer class.
    /// </summary>
    internal abstract class AgsLayer
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected AgsLayer(MapLayer layer)
        {
            _id = layer.Id;

            // attach to events
            layer.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(layer_PropertyChanged);
            MapLayer = layer;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Guid Id
        {
            get { return _id; }
        }

        public Layer ArcGISLayer
        {
            get;
            protected set;
        }

        public abstract bool IsVisible
        {
            get;
            set;
        }

        public abstract double Opacity
        {
            get;
            set;
        }

        public MapLayer MapLayer
        {
            get;
            private set;
        }

        public AgsServer Server
        {
            get;
            protected set;
        }

        public AgsLayerType LayerType
        {
            get;
            protected set;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Set token to layer from associated server
        /// </summary>
        public void UpdateTokenIfNeeded()
        {
            Debug.Assert(ArcGISLayer != null);

            bool needToUpdateToken = false;

            try 
            {
                needToUpdateToken = Server.RequiresTokens;
            }
            catch (InvalidOperationException)
            {
                // Eat exception in case of unavailable server
            }

            // set last server token to layer
            if (needToUpdateToken)
            {
                // If token updater didnt exist - create it.
                if (_tokenUpdater == null)
                    _tokenUpdater = new TokenUpdater(Server, ArcGISLayer, LayerType);
                // Otherwise - use token updater to update token.
                else
                    _tokenUpdater.SetNewToken();
            }
        }

        #endregion public methods

        #region private methods

        private void layer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(MapLayer.PropertyNameVisible, StringComparison.OrdinalIgnoreCase))
            {
                this.IsVisible = ((MapLayer)sender).IsVisible;
            }
            else if (e.PropertyName.Equals(MapLayer.PropertyNameOpacity, StringComparison.OrdinalIgnoreCase))
            {
                this.Opacity = ((MapLayer)sender).Opacity;
            }
        }

        /// <summary>
        /// Creates REST URL by SOAP web service URL.
        /// </summary>
        /// <param name="soapUrl">
        /// SOAP web service URL.
        /// </param>
        /// <returns>
        /// REST URL.
        /// </returns>
        protected static string FormatRestUrl(string soapUrl)
        {
            // ArcGIS service endpoint URL format is
            // http://<host>/<instance>/services/<folder>
            //
            // "/<instance>" part for REST service should be ended by "/rest";
            // the default value is "/arcgis/rest".

            string restUrl = null;
            if (soapUrl != null)
            {
                Uri soapUri = new Uri(soapUrl);
                string path = soapUri.AbsolutePath;

                int idx = path.IndexOf("/services", 1);
                if (idx != -1)
                {
                    path = path.Insert(idx, "/rest");
                    restUrl = new Uri(soapUri, path).ToString();
                }
            }

            return restUrl;
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Guid _id;

        /// <summary>
        /// Update laye's token.
        /// </summary>
        private TokenUpdater _tokenUpdater;

        #endregion private members
    }
}
