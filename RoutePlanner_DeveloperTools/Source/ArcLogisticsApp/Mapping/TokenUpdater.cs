using System;
using System.Diagnostics;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.Services;
using System.Net;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class for updating layers tokens.
    /// </summary>
    class TokenUpdater
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Server">AgsServer.</param>
        /// <param name="ArcGISLayer">Layer, which token must be updated.</param>
        /// <param name="LayerType">Type of the layer.</param>
        public TokenUpdater(AgsServer Server, Layer ArcGISLayer, AgsLayerType LayerType)
        {
            Debug.Assert(Server != null);
            Debug.Assert(ArcGISLayer != null);
            Debug.Assert(LayerType != null);

            _server = Server;
            _arcGISLayer = ArcGISLayer;
            _layerType = LayerType;

            // Set layer token.
            _SetNewToken(Server.LastToken);

            _InitTimer();

            // If server is in authorized state - start timer.
            if (_server.State == AgsServerState.Authorized)
                _RestartTimer();

            // Subscribe to server state changed event.
            _server.StateChanged += new EventHandler(_ServerStateChanged);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Update token.
        /// </summary>
        public void SetNewToken()
        {
            try
            {
                // Set new token for layer.
                _SetNewToken(_server.GenerateNewToken());

                _timer.Interval = _CalculateTimerInterval();
            }
            // If we have connection error while getting new token - change timer interval.
            catch (WebException ex)
            {
                _timer.Interval = TimeSpan.FromMinutes(CONNECTION_ERROR_TIMER_INTERVAL_MINUTES);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Occured when server state changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ServerStateChanged(object sender, EventArgs e)
        {
            // If server is in authorized state - restart timer.
            if (_server.State == AgsServerState.Authorized)
                _RestartTimer();
            // If server is in other state - stop timer.
            else 
                _timer.Stop();
        }

        /// <summary>
        /// Init timer for tokens update.
        /// </summary>
        private void _InitTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Tick += new EventHandler(_Tick);
        }

        /// <summary>
        /// Update timer interval and start timer.
        /// </summary>
        private void _RestartTimer()
        {
            _timer.Interval = _CalculateTimerInterval();
            _timer.Start();
        }

        /// <summary>
        /// On each timer tick update token.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _Tick(object sender, EventArgs e)
        {
            SetNewToken();
        }

        /// <summary>
        /// Calculate time in which next token must be requested.
        /// This must happen before token expired.
        /// </summary>
        /// <returns>TimeSpan.</returns>
        private TimeSpan _CalculateTimerInterval()
        {
            var ticks = (_server.LastTokenExpirationTime - DateTime.Now.ToUniversalTime()).Ticks / 2;
            return new TimeSpan(ticks);
        }

        /// <summary>
        /// Set new token.
        /// </summary>
        /// <param name="token">Token to set.</param>
        private void _SetNewToken(string token)
        {
            // Detect layer type and set layer token.
            switch (_layerType)
            {
                case AgsLayerType.Cached:
                    ArcGISTiledMapServiceLayer arcGISTiledMapServiceLayer =
                        (ArcGISTiledMapServiceLayer)_arcGISLayer;
                    arcGISTiledMapServiceLayer.Token = token;
                    break;
                case AgsLayerType.Dynamic:
                    ArcGISDynamicMapServiceLayer arcGISDynamicMapServiceLayer =
                        (ArcGISDynamicMapServiceLayer)_arcGISLayer;
                    arcGISDynamicMapServiceLayer.Token = token;
                    break;
                default:
                    Debug.Assert(false, "New enum value was added.");
                    break;
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// Interval(in minutes) in which we will try to get new token when we got connection error.
        /// </summary>
        private const int CONNECTION_ERROR_TIMER_INTERVAL_MINUTES = 1;

        /// <summary>
        /// Layer's source server.
        /// </summary>
        private AgsServer _server;

        /// <summary>
        /// Layer.
        /// </summary>
        private Layer _arcGISLayer;

        /// <summary>
        /// Type of the layer.
        /// </summary>
        private AgsLayerType _layerType;

        /// <summary>
        /// Timer, updating token.
        /// </summary>
        private DispatcherTimer _timer;

        #endregion
    }
}
