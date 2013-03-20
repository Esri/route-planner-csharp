using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Services;

namespace ESRI.ArcLogistics.App.Controls
{
    internal delegate bool CanSelectHandler(object item);
    internal delegate bool CanActivateToolHandler();
    internal delegate void StartEditHandler(object item);
    internal delegate void EndEditHandler(bool commit);
    internal delegate bool IsGeocodingInProgressHandler();

    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    internal partial class MapControl : UserControl
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor.
        /// </summary>
        public MapControl()
        {
            InitializeComponent();

            MapLayersWidget.ActiveBaseLayerChanged += new RoutedEventHandler(MapLayersWidget_ActiveBaseLayerChanged);

            _mapTips = new MapTips();
            _tools = new MapTools(this, toolPanel);
            _clustering = new Clustering(this);

            _mapSelectionManager = new MapSelectionManager(this, _clustering, _objectLayers);
            _mapEventsManager = new MapEventsManager(this, _mapSelectionManager, _clustering, _tools, _mapTips);
            _mapExtentManager = new MapExtentManager(this, _clustering, _mapSelectionManager);
        }

        #endregion

        #region Public static properties

        /// <summary>
        /// Opacity of fully visible layer.
        /// </summary>
        public static double FullOpacity
        {
            get
            {
                return FULL_OPACITY;
            }
        }

        /// <summary>
        /// Opacity of semitransparent layer.
        /// </summary>
        public static double HalfOpacity
        {
            get
            {
                return HALF_OPACITY;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Map.
        /// </summary>
        public Map Map
        {
            get
            {
                return (Map)GetValue(mapProperty);
            }
            set
            {
                if (value != this.Map)
                {
                    SetValue(mapProperty, value);
                    _InitMap();
                    UpdateLayout();
                }
            }
        }

        /// <summary>
        /// Map extent.
        /// </summary>
        public ESRI.ArcLogistics.Geometry.Envelope Extent
        {
            get
            {
                Debug.Assert(_mapEventsManager != null);

                return _mapExtentManager.Extent;
            }
            set
            {
                Debug.Assert(_mapEventsManager != null);

                _mapExtentManager.Extent = value;
            }
        }

        /// <summary>
        /// Current selection 
        /// </summary>
        public IList SelectedItems
        {
            get
            {
                Debug.Assert(_mapSelectionManager != null);

                return _mapSelectionManager.SelectedItems;
            }
        }

        /// <summary>
        /// Current pointed graphic.
        /// </summary>
        public Graphic PointedGraphic
        {
            get
            {
                Debug.Assert(_mapEventsManager != null);

                return _mapEventsManager.PointedGraphic;
            }
        }

        /// <summary>
        ///  Last clicked coords.
        /// </summary>
        public System.Windows.Point? ClickedCoords
        {
            get
            {
                Debug.Assert(_mapEventsManager != null);

                return _mapEventsManager.ClickedCoords;
            }
            set
            {
                Debug.Assert(_mapEventsManager != null);

                _mapEventsManager.ClickedCoords = value;
            }
        }

        /// <summary>
        /// Flag, which indicates to map control, that next map changing dont need to change extent
        /// </summary>
        public bool IgnoreSizeChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Is editing in progress.
        /// </summary>
        public bool IsInEditedMode
        {
            get
            {
                return EditedObject != null;
            }
        }

        /// <summary>
        /// Object layers collection.
        /// </summary>
        public IList<ObjectLayer> ObjectLayers
        {
            get
            {
                return _objectLayers;
            } 
        }

        /// <summary>
        /// Current edited object.
        /// </summary>
        public object EditedObject
        {
            get;
            private set;
        }

        /// <summary>
        /// Callback for checking is item can be selected.
        /// </summary>
        public CanSelectHandler CanSelectCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Callback for starting editing of orders and locations.
        /// </summary>
        public StartEditHandler StartEditGeocodableCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Callback for starting editing of zones and barriers.
        /// </summary>
        public StartEditHandler StartEditRegionCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Callback for ending editing of orders and locations.
        /// </summary>
        public EndEditHandler EndEditGeocodableCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Callback for ending editing of zones and barriers.
        /// </summary>
        public EndEditHandler EndEditRegionCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Callback for ending editing of routes.
        /// </summary>
        public EndEditHandler EndEditRouteCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Callback to ask parent is geocoding in progress.
        /// </summary>
        public IsGeocodingInProgressHandler IsGeocodingInProgressCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Is zones and barriers visibility can be changed from widget.
        /// </summary>
        public bool AddRegionsLayersToWidget
        {
            get;
            set;
        }

        /// <summary>
        /// Extent to set on loading map
        /// </summary>
        public ESRI.ArcLogistics.Geometry.Envelope? StartupExtent
        {
            get
            {
                return _startupExtent;
            }
            set
            {
                _startupExtent = value;
            }
        }

        /// <summary>
        /// Margins for tools panel.
        /// </summary>
        public Thickness ToolsMargin
        {
            get
            {
                return ToolsOffset.Margin;
            }
            set
            {
                ToolsOffset.Margin = value;
            }
        }

        /// <summary>
        /// Current tool.
        /// </summary>
        public IMapTool CurrentTool
        {
            get
            {
                return _tools.CurrentTool;
            }
            set
            {
                _tools.CurrentTool = value;
            }
        }

        /// <summary>
        /// Last cursor move position on map.
        /// </summary>
        public System.Windows.Point? LastCursorPos
        {
            get
            {
                return _mapEventsManager.LastCursorPos;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Add layer.
        /// </summary>
        /// <param name="layer">ObjectLayer for add.</param>
        public void AddLayer(ObjectLayer layer)
        {
            _objectLayers.Add(layer);
            FrameworkElement mapTip = (FrameworkElement)LayoutRoot.FindResource("MapTip");
            _mapTips.CreateMapTipIfNeeded(layer, mapTip);
        }

        /// <summary>
        /// Zoom to resolution extent.
        /// </summary>
        /// <param name="extent">New extent.</param>
        public void ZoomTo(ESRI.ArcGIS.Client.Geometry.Geometry extent)
        {
            _mapExtentManager.ZoomTo(extent);
        }

        /// <summary>
        /// Set map extent to collection.
        /// </summary>
        /// <param name="collection">Collection which needs to be in extent.</param>
        public void SetExtentOnCollection(IList collection)
        {
            if (Map.IsInitialized())
                _mapExtentManager.SetExtentOnCollection(collection);
        }

        /// <summary>
        /// Set opacity to all layer with turned off constant opacity.
        /// </summary>
        /// <param name="opacity">Opacity value.</param>
        public void SetOpacityToLayers(double opacity)
        {
            foreach (ObjectLayer layer in _objectLayers)
            {
                if (!layer.ConstantOpacity)
                {
                    layer.MapLayer.Opacity = opacity;
                }
            }
        }

        /// <summary>
        /// Start edit.
        /// </summary>
        /// <param name="item">Editing object.</param>
        public void StartEdit(object item)
        {
            if (IsInEditedMode)
                return;

            EditedObject = item;

            if (item is Route || item is Stop)
                return;

            Debug.Assert(item is IGeocodable || item is Zone || item is Barrier);

            if (_clustering != null)
            {
                _clustering.ClusteringLayer.Selectable = false;

                // Workaround: clusters dont unexpands in case of only one cluster on view.
                _clustering.UnexpandIfExpanded();
            }

            // Find graphic, which represents edited item.
            DataGraphicObject graphic = null;
            foreach (ObjectLayer layer in _objectLayers)
            {
                graphic = layer.FindGraphicByData(item);

                if (graphic != null)
                {
                    break;
                }
            }

            SetOpacityToLayers(HALF_OPACITY);

            _tools.StartEdit(item);

            // Hide graphic which represents edited item.
            graphic.IsVisible = false;
        }

        /// <summary>
        /// End Edit.
        /// </summary>
        public void EditEnded()
        {
            Debug.Assert(IsInEditedMode);

            if (EditedObject is Route || EditedObject is Stop)
            {
                EditedObject = null;
                return;
            }

            SetOpacityToLayers(FULL_OPACITY);

            if (_clustering != null)
            {
                _clustering.ClusteringLayer.Selectable = true;
            }

            // Find graphic, which represents edited item.
            Graphic graphic = MapHelpers.GetGraphicByDataItem(EditedObject, _objectLayers);

            EditedObject = null;

            _tools.EndEdit(graphic);

            // Show graphic which represents edited item.
            ((DataGraphicObject)graphic).IsVisible = true;
        }

        /// <summary>
        /// Add tool.
        /// </summary>
        /// <param name="tool">Tool for adding.</param>
        /// <param name="canActivateToolHandler">Callback for checking is tool can be activated.</param>
        public void AddTool(IMapTool tool, CanActivateToolHandler canActivateToolHandler)
        {
            _tools.AddTool(tool, canActivateToolHandler);
        }

        /// <summary>
        /// Add tools.
        /// </summary>
        /// <param name="tools">Tools for adding.</param>
        /// <param name="canActivateToolHandler">Callback for checking is tool can be activated.</param>
        public void AddTools(IMapTool[] tools, CanActivateToolHandler canActivateToolHandler)
        {
            _tools.AddTools(tools,  canActivateToolHandler);
        }

        /// <summary>
        /// Set opacity of editing layers.
        /// </summary>
        /// <param name="isEditingStarted">Is need to make layers semitransparent.</param>
        public void SetEditingMapLayersOpacity(bool isEditingStarted)
        {
            double opacity;

            if (isEditingStarted)
            {
                opacity = HALF_OPACITY;
            }
            else
            {
                opacity = FULL_OPACITY;
            }
            _tools.EditedObjectLayer.MapLayer.Opacity = opacity;
            _tools.EditMarkersLayer.MapLayer.Opacity = opacity;
        }

        /// <summary>
        /// Unexpand cluster if expanded.
        /// </summary>
        public void UnexpandIfExpanded()
        {
            _clustering.UnexpandIfExpanded();
        }

        /// <summary>
        /// Clear editing markers.
        /// </summary>
        public void ClearEditMarkers()
        {
            _tools.ClearEditMarkers();
        }

        /// <summary>
        /// Fill editing markers.
        /// </summary>
        /// <param name="edited">Edited object.</param>
        public void FillEditMarkers(object edited)
        {
            _tools.FillEditMarkers(edited);
        }

        /// <summary>
        /// Show hint "Could not located, but zoomed..."
        /// </summary>
        /// <param name="geocodableItem">Geocodable object.</param>
        /// <param name="candidatesToZoom">Address to which map was zoomed. Null if not zoomed.</param>
        public void ShowZoomToCandidatePopup(IGeocodable geocodableItem, AddressCandidate[] candidatesToZoom)
        {
            if (_isGeocodingHintInitialized)
            {
                // Show hint suspendedly because final selection changed event in datagrid control is suspended.
                // And so hint will be hided on selection changed.
                Dispatcher.BeginInvoke(new Action(delegate() 
                    {
                        if (SelectedItems.Count > 0 && SelectedItems[0] == geocodableItem)
                        {
                            GeocodingHint.ShowHint(geocodableItem, candidatesToZoom);
                        }
                    }), DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Hide hint "Could not located, but zoomed..."
        /// </summary>
        public void HideZoomToCandidatePopup()
        {
            GeocodingHint.HideHint();
        }

        #endregion

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// React on map loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void map_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (System.Threading.ThreadStart)delegate { _OnMapLoaded(); });
        }

        private void _OnMapLoaded()
        {
            if (!_isInited)
            {
                _InitMap();

                if (Map != null)
                    MapLayersWidget.ActiveBaseLayer = this.Map.SelectedBaseMapLayer;

                // Set initial extent.
                if (this.StartupExtent.HasValue)
                {
                    // If startup extent was set by parent - use it.
                    Extent = this.StartupExtent.Value;
                    this.StartupExtent = null;
                }
                else
                {
                    // Otherwise - use default startup extent.
                    if (Map != null && this.Map.HasStartupExtent)
                    {
                        Extent = this.Map.StartupExtent;
                    }
                }
            }
        }

        /// <summary>
        /// React on scalebar widget loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ScaleBarWidget_Loaded(object sender, RoutedEventArgs e)
        {
            if (Map != null &&
                this.Map.IsInitialized() &&
                Map.ScaleBarUnits.HasValue)
            {
                ScaleBarWidget.MapUnit = MapHelpers.ConvertScalebarUnits(Map.ScaleBarUnits.Value);
            }

            if (RegionInfo.CurrentRegion.IsMetric)
            {
                ScaleBarWidget.DisplayUnit = ESRI.ArcGIS.Client.ScaleBarUnit.Kilometers;
            }
            else
            {
                ScaleBarWidget.DisplayUnit = ESRI.ArcGIS.Client.ScaleBarUnit.Miles;
            }

            if (!_isInited)
            {
                // Check that application is ArcLogistics. So it is possible to load map control in visual studio designer.
                if (Application.Current is ESRI.ArcLogistics.App.App)
                    ScaleBarWidget.Style = (Style)Application.Current.FindResource("ScaleBarStyleKey");
            }
        }

        /// <summary>
        /// Do map initialization.
        /// </summary>
        private void _InitMap()
        {
            if (this.Map != null)
            {
                AddLayer(_clustering.ClusteringLayer);

                ScaleBarWidget.Map = map;

                // Do not initialize layers if map is not initialized.
                if (Map.IsInitialized())
                    _InitServiceLayers();

                // init Map layers
                MapLayersWidget.AllLayers = this.Map.Layers;

                _clustering.LeaderLinesLayer.Opacity = HALF_OPACITY;
                map.Layers.Add(_clustering.LeaderLinesLayer);

                _AddLayerToMapFromObjectLayers();

                map.Layers.Add(_mapSelectionManager.SelectionFrameLayer);

                // if parent of map control wants to add regions layer to widget than do it
                if (AddRegionsLayersToWidget)
                {
                    List<ObjectLayer> layersForWidget = new List<ObjectLayer>();
                    foreach (ObjectLayer layer in _objectLayers)
                    {
                        if (layer.LayerType == typeof(Zone) || layer.LayerType == typeof(Barrier))
                            layersForWidget.Add(layer);
                    }

                    MapLayersWidget.ObjectLayers = layersForWidget;
                }

                _InitGeocodingHint();

                _isInited = true;
            }
        }

        /// <summary>
        /// Init geocoding hint if map control from geocodable page.
        /// </summary>
        private void _InitGeocodingHint()
        {
            // Find address by point tool.
            AddressByPointTool addressByPointTool = null;
            foreach (IMapTool tool in _tools.Tools)
            {
                if (tool is AddressByPointTool)
                {
                    addressByPointTool = tool as AddressByPointTool;
                    break;
                }
            }

            if (addressByPointTool != null)
            {
                GeocodingHint.Initialize(canvas, this, addressByPointTool);
                _isGeocodingHintInitialized = true;
            }
        }

        /// <summary>
        /// Add map layers collection from object layers.
        /// </summary>
        private void _AddLayerToMapFromObjectLayers()
        {
            foreach (ObjectLayer objectLayer in _objectLayers)
            {
                // init layer spatial reference by map spatial reference
                objectLayer.SpatialReferenceID = Map.SpatialReferenceID;

                // If more than one object layer sharing one maplayer - add maplayer only once
                if (!map.Layers.Contains(objectLayer.MapLayer))
                {
                    map.Layers.Add(objectLayer.MapLayer);

                    if (objectLayer.MapLayer is GraphicsLayer)
                    { 
                        // Subscribe to all needed mouse events
                        GraphicsLayer graphicsLayer = objectLayer.MapLayer;

                        if (!objectLayer.IsBackgroundLayer)
                        {
                            _mapEventsManager.RegisterLayer(graphicsLayer);
                        }

                        // If clustering for object layer is on - create clusterer for maplayer
                        if (objectLayer.UseClustering)
                        {
                            ALClusterer clusterer = _clustering.GetClusterer();
                            graphicsLayer.Clusterer = clusterer;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add available service layers to map and process unavailable.
        /// </summary>
        private void _InitServiceLayers()
        {
            // Save AgsLayers to list 
            _agsLayers = new List<AgsLayer>();
            map.Layers.Clear();
            foreach (MapLayer layer in this.Map.Layers)
            {
                AgsMapLayer agsMapLayer = layer as AgsMapLayer;
                AgsLayer wrap = AgsLayerFactory.CreateLayer(agsMapLayer.Server, layer);
                _agsLayers.Add(wrap);

                // subscribe to StateChanged event to react on server state changing
                agsMapLayer.Server.StateChanged += new EventHandler(Server_StateChanged);
                wrap.ArcGISLayer.InitializationFailed += new EventHandler<EventArgs>(ArcGISLayer_InitializationFailed);
                if (agsMapLayer.Server.State == ESRI.ArcLogistics.Services.AgsServerState.Authorized)
                    map.Layers.Add(wrap.ArcGISLayer);
            }

            // Check all layers availability and save it to details list
            List<MessageDetail> details = new List<MessageDetail>();

            foreach (MapLayer mapLayer in Map.Layers)
            {
                AgsMapLayer agsMapLayer = mapLayer as AgsMapLayer;
                if (agsMapLayer != null)
                {
                    if (agsMapLayer.Server.State == AgsServerState.Unavailable)
                    {
                        string format = (string)App.Current.FindResource("LayerUnavailable");
                        string errorMessage = string.Format(format, mapLayer.Name);
                        MessageDetail detail = new MessageDetail(MessageType.Error, errorMessage);
                        details.Add(detail);
                    }
                    else if (agsMapLayer.Server.State == AgsServerState.Unauthorized)
                    {
                        string format = (string)App.Current.FindResource("LayerNotAuthorized");
                        string errorMessage = string.Format(format, mapLayer.Name, agsMapLayer.Server.Title);
                        Link link = new Link((string)App.Current.FindResource("LicencePanelText"),
                            Pages.PagePaths.LicensePagePath, LinkType.Page);
                        MessageDetail detail = new MessageDetail(MessageType.Error, errorMessage, link);
                        details.Add(detail);
                    }
                }
            }

            // Add info to messenger in case of at least one unavailable layer
            if (details.Count > 0)
                App.Current.Messenger.AddError((string)App.Current.FindResource("SomeMapLayersCannotBeLoaded"), details);
        }

        /// <summary>
        /// React on map server state changed.
        /// </summary>
        /// <param name="sender">Layer, which server state changed.</param>
        /// <param name="e">Ignored.</param>
        private void Server_StateChanged(object sender, EventArgs e)
        {
            AgsServer agsServer = (AgsServer)sender;

            // index for inserting layer
            int index = 0;
            foreach (AgsLayer wrap in _agsLayers)
            {
                if (map.Layers.Contains(wrap.ArcGISLayer))
                {
                    index++;
                }
                else
                {
                    AgsMapLayer agsMapLayer = wrap.MapLayer as AgsMapLayer;
                    if (agsMapLayer != null && agsMapLayer.Server == agsServer && agsServer.State == AgsServerState.Authorized)
                    {
                        wrap.UpdateTokenIfNeeded();
                        map.Layers.Insert(index, wrap.ArcGISLayer);

                        // Check spatial reference ID is present in Map
                        if (Map.SpatialReferenceID.HasValue)
                        {
                            _SetSpatialReferenceIDToObjectLayersIfNeeded();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// React on layer initialization failed.
        /// </summary>
        /// <param name="sender">Layer, which server state changed.</param>
        /// <param name="e">Ignored.</param>
        private void ArcGISLayer_InitializationFailed(object sender, EventArgs e)
        {
            Layer layer = (Layer)sender;

            // Find associated AgsLayer.
            AgsLayer failedAgsLayer = null;
            foreach (AgsLayer agsLayer in _agsLayers)
            {
                if (agsLayer.ArcGISLayer == layer)
                {
                    failedAgsLayer = agsLayer;
                    break;
                }
            }

            _failedLayers.Add(failedAgsLayer);

            // Call suspended method to add info about failed layers in messagewindow
            this.Dispatcher.BeginInvoke(new Action(delegate() { _AddFailedLayersInfo(); }), DispatcherPriority.Background);
        }

        /// <summary>
        /// Add info about failed layers in messagewindow.
        /// </summary>
        private void _AddFailedLayersInfo()
        {
            if (_failedLayers.Count > 0)
            {
                List<MessageDetail> details = new List<MessageDetail>();

                foreach (AgsLayer agsLayer in _failedLayers)
                {
                    string message = string.Format((string)App.Current.FindResource("LayerFailed"), agsLayer.MapLayer.Name);
                    MessageDetail messageDetail = new MessageDetail(MessageType.Error, message);
                    details.Add(messageDetail);
                }

                App.Current.Messenger.AddError((string)App.Current.FindResource("SomeMapLayersFailed"), details);

                // Clear to prevent error copies.
                _failedLayers.Clear();
            }
        }

        /// <summary>
        /// Set map spatial reference ID to all layers.
        /// </summary>
        private void _SetSpatialReferenceIDToObjectLayersIfNeeded()
        {
            foreach (ObjectLayer objectLayer in _objectLayers)
            {
                if (!objectLayer.SpatialReferenceID.HasValue)
                {
                    objectLayer.SpatialReferenceID = Map.SpatialReferenceID;
                }
            }
        }

        /// <summary>
        /// React on active base layer changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void MapLayersWidget_ActiveBaseLayerChanged(object sender, RoutedEventArgs e)
        {
            if (this.Map != null)
            {
                this.Map.SelectedBaseMapLayer = MapLayersWidget.ActiveBaseLayer;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Opacity of fully visible layer.
        /// </summary>
        private const double FULL_OPACITY = 1;

        /// <summary>
        /// Opacity of semitransparent layer.
        /// </summary>
        private const double HALF_OPACITY = 0.4;

        #endregion

        #region Private fields

        /// <summary>
        /// Map dependency property.
        /// </summary>
        private static DependencyProperty mapProperty = DependencyProperty.Register("Map", typeof(Map), typeof(MapControl));

        /// <summary>
        /// Is map inited.
        /// </summary>
        private bool _isInited;

        /// <summary>
        /// Object layers collection.
        /// </summary>
        private List<ObjectLayer> _objectLayers = new List<ObjectLayer>();

        /// <summary>
        /// Clustering manager.
        /// </summary>
        private Clustering _clustering;

        /// <summary>
        /// Tools manager.
        /// </summary>
        private MapTools _tools;

        /// <summary>
        /// Maptips manager.
        /// </summary>
        private MapTips _mapTips;

        /// <summary>
        /// Map selection manager.
        /// </summary>
        private MapSelectionManager _mapSelectionManager;

        /// <summary>
        /// Ags layers collection.
        /// </summary>
        private List<AgsLayer> _agsLayers;

        /// <summary>
        /// Layers, which failed during initialization.
        /// </summary>
        private List<AgsLayer> _failedLayers = new List<AgsLayer>();

        /// <summary>
        /// Map events manager.
        /// </summary>
        private MapEventsManager _mapEventsManager;

        /// <summary>
        /// Map extent manager.
        /// </summary>
        private MapExtentManager _mapExtentManager;

        /// <summary>
        /// Extent, to set on load.
        /// </summary>
        private ESRI.ArcLogistics.Geometry.Envelope? _startupExtent;

        /// <summary>
        /// Is geocoding hint initialized.
        /// </summary>
        private bool _isGeocodingHintInitialized;

        #endregion
    }
}
