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
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.Services.Serialization;

namespace ESRI.ArcLogistics
{
    /// <summary>
    /// Scale bar units enum
    /// </summary>
    public enum ScaleBarUnit
    {
        Undefined,
        DecimalDegrees,
        Millimeters,
        Centimeters,
        Inches,
        Decimeters,
        Feet,
        Yards,
        Meters,
        Kilometers,
        Miles,
        NauticalMiles,
    }

    /// <summary>
    /// Map class.
    /// </summary>
    public class Map
    {
        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mapInfo">Map information.</param>
        /// <param name="servers">Servers collection.</param>
        /// <param name="exceptionHandler">Exceptions handler.</param>
        internal Map(MapInfoWrap mapInfo,
            ICollection<AgsServer> servers,
            IServiceExceptionHandler exceptionHandler)
        {
            Debug.Assert(mapInfo != null);
            Debug.Assert(exceptionHandler != null);

            _exceptionHandler = exceptionHandler;

            // create layers
            _CreateLayers(mapInfo.Services, servers);

            // set selected basemap layer
            _SetSelectedBaseMap();

            _mapInfo = mapInfo;

            // Set current map server.
            _mapServer = _GetMapServer(_layers);

            // Map should be initialized later.
            _inited = false;
        }

        #endregion constructors

        #region public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns read-only collection of map layers.
        /// </summary>
        public ICollection<MapLayer> Layers
        {
            get { return _layers.AsReadOnly(); }
        }

        /// <summary>
        /// Returns true of map has defined startup extent.
        /// </summary>
        public bool HasStartupExtent
        {
            get { return !StartupExtent.IsNull && !StartupExtent.IsEmpty; }
        }

        /// <summary>
        /// Returns map startup extent.
        /// </summary>
        public Envelope StartupExtent
        {
            get { return _mapInfo.StartupExtent; }
        }

        /// <summary>
        /// Returns import check extent.
        /// </summary>
        public Envelope ImportCheckExtent
        {
            get { return _mapInfo.ImportCheckExtent; }
        }

        /// <summary>
        /// Selected base map layer.
        /// </summary>
        public MapLayer SelectedBaseMapLayer
        {
            get { return _baseMapLayer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();

                if (value != _baseMapLayer)
                {
                    if (_baseMapLayer != null)
                        _baseMapLayer.SetVisible(false);

                    value.SetVisible(true);
                    _baseMapLayer = value;
                }
            }
        }

        /// <summary>
        /// Map spatial reference ID
        /// </summary>
        public int? SpatialReferenceID
        {
            get
            {
                _ValidateMapServerState();
                return _spatialReferenceId;
            }

            private set
            {
                _spatialReferenceId = value;
            }
        }

        /// <summary>
        /// Map scale bar unit
        /// </summary>
        public ScaleBarUnit? ScaleBarUnits
        {
            get
            {
                _ValidateMapServerState();
                return _scaleBarUnits;
            }
            private set
            {
                _scaleBarUnits = value;
            }
        }

        #endregion public properties

        #region public methods

        /// <summary>
        /// Is map initialized.
        /// </summary>
        /// <returns>True if map is initialized, otherwise false.</returns>
        public bool IsInitialized()
        {
            _ValidateMapServerState();

            return _inited;
        }

        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method do validation of servers state and re-initialization if needed.
        /// </summary>
        private void _ValidateMapServerState()
        {
            try
            {
                // Check servers state.
                ServiceHelper.ValidateServerState(_mapServer);

                // Check if services were initialized.
                if (!_inited)
                {
                    _InitializeMap();
                }
            }
            catch (Exception ex)
            {
                if (!_exceptionHandler.HandleException(ex,
                    Properties.Resources.ServiceNameMap))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Initialize map.
        /// </summary>
        /// <returns>Result: true - is success, otherwise false.</returns>
        private bool _InitializeMap()
        {
            _GetMapLayerProperties(true);

            return _inited;
        }

        private void _CreateLayers(ICollection<MapServiceInfoWrap> services,
            ICollection<AgsServer> servers)
        {
            Debug.Assert(servers != null);

            if (services != null)
            {
                foreach (MapServiceInfoWrap service in services)
                {
                    AgsServer server = ServiceHelper.FindServerByName(service.ServerName, servers);

                    if (server != null)
                    {
                        try
                        {
                            // create map layer
                            AgsMapLayer layer = new AgsMapLayer(service, server, this);
                            _layers.Add(layer);
                        }
                        catch (Exception e)
                        {
                            // skip layer
                            Logger.Warning(e);
                        }
                    }
                }
            }
        }

        private void _SetSelectedBaseMap()
        {
            foreach (MapLayer layer in _layers)
            {
                if (layer.IsBaseMap && layer.IsVisible)
                {
                    if (_baseMapLayer == null)
                        _baseMapLayer = layer;
                    else
                        layer.IsVisible = false;
                }
            }
        }

        /// <summary>
        /// Method gets current map server.
        /// </summary>
        /// <param name="layers">Collection of map layers.</param>
        /// <returns>Map server reference, if it is, otherwise - null.</returns>
        private AgsServer _GetMapServer(List<MapLayer> layers)
        {
            AgsServer server = null;
            
            // Get server from first layer.
            if (layers.Count > 0)
                server = ((AgsMapLayer)layers[0]).Server;

            return server;
        }

        /// <summary>
        /// Initialize map spatial reference and map scalebar units from server
        /// </summary>
        /// <param name="isInitializationInProgress">True if initialization is in progress,
        /// otherwise false..</param>
        private void _GetMapLayerProperties(bool isInitializationInProgress)
        {
            if (_layers.Count > 0)
            {
                // TODO: check spatial references from other layers and remove hardcode
                // Create client
                MapLayer mapLayer = _layers[0];
                _mapServer = ((AgsMapLayer)mapLayer).Server;

                if (_mapServer.State == AgsServerState.Authorized)
                {
                    MapServiceClient mapservice = new MapServiceClient(
                        mapLayer.Url,
                        _mapServer.OpenConnection());

                    // Get map service info
                    ESRI.ArcLogistics.MapService.MapServerInfo serverInfo =
                        mapservice.GetServerInfo(mapservice.GetDefaultMapName());

                    // Get spatial reference ID
                    _spatialReferenceId = serverInfo.SpatialReference.WKID;
                    _scaleBarUnits = _ConvertScalebarUnits(serverInfo.Units);

                    // Now map is initialized.
                    _inited = true;
                }
                else
                {
                    // Subscribe on server state changed in case of Map is not inited
                    if (isInitializationInProgress)
                    {
                        _mapServer.StateChanged += new EventHandler(_Server_StateChanged);
                    }
                }
            }
        }

        /// <summary>
        /// Convert map units to scalebar units
        /// </summary>
        /// <param name="esriUnits">Map layer units</param>
        /// <returns>Scalebar units</returns>
        private ScaleBarUnit _ConvertScalebarUnits(ESRI.ArcLogistics.MapService.esriUnits esriUnits)
        {
            ScaleBarUnit scalebarUnits = ScaleBarUnit.Undefined;

            switch (esriUnits)
            {
                case ESRI.ArcLogistics.MapService.esriUnits.esriUnknownUnits:
                    {
                        scalebarUnits = ScaleBarUnit.Undefined;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriInches:
                    {
                        scalebarUnits = ScaleBarUnit.Inches;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriFeet:
                    {
                        scalebarUnits = ScaleBarUnit.Feet;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriYards:
                    {
                        scalebarUnits = ScaleBarUnit.Yards;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriMiles:
                    {
                        scalebarUnits = ScaleBarUnit.Miles;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriNauticalMiles:
                    {
                        scalebarUnits = ScaleBarUnit.NauticalMiles;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriMillimeters:
                    {
                        scalebarUnits = ScaleBarUnit.Millimeters;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriCentimeters:
                    {
                        scalebarUnits = ScaleBarUnit.Centimeters;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriMeters:
                    {
                        scalebarUnits = ScaleBarUnit.Meters;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriKilometers:
                    {
                        scalebarUnits = ScaleBarUnit.Kilometers;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriDecimalDegrees:
                    {
                        scalebarUnits = ScaleBarUnit.DecimalDegrees;
                        break;
                    }
                case ESRI.ArcLogistics.MapService.esriUnits.esriDecimeters:
                    {
                        scalebarUnits = ScaleBarUnit.Decimeters;
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }

            return scalebarUnits;
        }

        /// <summary>
        /// React on server state changed
        /// </summary>
        private void _Server_StateChanged(object sender, EventArgs e)
        {
            AgsServer server = (AgsServer)sender;
            if (server.State == AgsServerState.Authorized)
            {
                _GetMapLayerProperties(false);
                server.StateChanged -= new EventHandler(_Server_StateChanged);
            }
        }

        #endregion private methods

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Map layers collection.
        /// </summary>
        private List<MapLayer> _layers = new List<MapLayer>();

        /// <summary>
        /// Base map layer.
        /// </summary>
        private MapLayer _baseMapLayer;

        /// <summary>
        /// Map info.
        /// </summary>
        private MapInfoWrap _mapInfo;

        /// <summary>
        /// Map server.
        /// </summary>
        private AgsServer _mapServer;

        /// <summary>
        /// Spatial reference Id.
        /// </summary>
        private int? _spatialReferenceId;

        /// <summary>
        /// Scale bar units.
        /// </summary>
        private ScaleBarUnit? _scaleBarUnits;

        /// <summary>
        /// Service exception handler.
        /// </summary>
        private IServiceExceptionHandler _exceptionHandler;

        /// <summary>
        /// Is map initialized.
        /// </summary>
        private bool _inited;

        #endregion private members
    }
}
