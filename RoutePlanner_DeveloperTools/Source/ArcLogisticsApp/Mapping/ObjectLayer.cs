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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;
using ESRI.ArcLogistics.Utility;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class, that supports drawing data collection on map.
    /// </summary>
    internal class ObjectLayer
    {
        #region constants

        public const int FRONTZINDEX = 4;
        public const int SELECTEDZINDEX = 1;
        public const int BACKZINDEX = 0;

        #endregion constants

        #region constructor

        /// <summary>
        /// Create object layer without creating new graphics layer.
        /// </summary>
        /// <param name="collection">Data collection for drawing.</param>
        /// <param name="type">Type of data.</param>
        /// <param name="useClusterer">Use clusterer.</param>
        /// <param name="layer">Graphics layer for data drawing.</param>
        public ObjectLayer(IEnumerable collection, Type type, bool useClusterer, GraphicsLayer layer)
        {
            _layer = layer;
            _Init(collection, type, useClusterer);
        }

        /// <summary>
        /// Create object layer with default graphic objects on each data type.
        /// </summary>
        /// <param name="collection">Data collection for drawing.</param>
        /// <param name="type">Type of data.</param>
        /// <param name="useClusterer">Use clusterer.</param>
        public ObjectLayer(IEnumerable collection, Type type, bool useClusterer)
        {
            _layer = new GraphicsLayer();
            _Init(collection, type, useClusterer);
        }

        /// <summary>
        /// Create object layer with concrete graphic object type.
        /// </summary>
        /// <param name="collection">Data collection for drawing.</param>
        /// <param name="type">Type of data.</param>
        /// <param name="useClusterer">Use clusterer.</param>
        /// <param name="graphicObjectType">Type of graphic object to create.</param>
        public ObjectLayer(IEnumerable collection, Type type, bool useClusterer, Type graphicObjectType)
        {
            _graphicObjectType = graphicObjectType;
            _layer = new GraphicsLayer();
            _Init(collection, type, useClusterer);
        }

        #endregion

        #region public members

        /// <summary>
        /// Elements collection.
        /// </summary>
        public IEnumerable Collection
        {
            get
            {
                return _collection;
            }
            set
            {
                // Disconnect from old collection.
                INotifyCollectionChanged notifyCollectionChanged;
                if (_collection is INotifyCollectionChanged)
                {
                    notifyCollectionChanged = (INotifyCollectionChanged)_collection;
                    notifyCollectionChanged.CollectionChanged -= _CollectionChanged;
                }

                // Find graphic objects to be deleted.
                var currentCollection = _collection == null ?
                    Enumerable.Empty<object>() : _collection.Cast<object>();
                var collectionData = new HashSet<object>(currentCollection);
                var graphicsToDelete = _layer.Graphics
                    .OfType<DataGraphicObject>()
                    .Where(dataGraphic => collectionData.Contains(dataGraphic.Data))
                    .ToHashSet();

                // Delete graphic objects from the layer.
                var removedItemsStart = _layer.Graphics.RemoveIf(graphicsToDelete.Contains);
                for (var i = _layer.Graphics.Count - 1; i >= removedItemsStart; --i)
                {
                    var dataGraphicObject = (DataGraphicObject)_layer.Graphics[i];
                    dataGraphicObject.UnsubscribeOnChange();

                    _layer.Graphics.RemoveAt(i);
                }

                _selectedItems.Clear();

                _graphicDictionary.Clear();

                _collection = value;

                // Connect to new collection.
                if (_collection is INotifyCollectionChanged)
                {
                    notifyCollectionChanged = (INotifyCollectionChanged)_collection;
                    notifyCollectionChanged.CollectionChanged += _CollectionChanged;
                }

                _InitializeGraphics();
            }
        }

        /// <summary>
        /// Layer visibility.
        /// </summary>
        public bool Visible
        {
            get
            {
                return _layer.Visible;
            }
            set
            {
                _layer.Visible = value;
            }
        }

        /// <summary>
        /// Is objects in layer selectable.
        /// </summary>
        public bool Selectable
        {
            get
            {
                return _selectable;
            }
            set
            {
                _selectable = value;
            }
        }

        /// <summary>
        /// Graphic layer.
        /// </summary>
        public GraphicsLayer MapLayer
        {
            get
            {
                return _layer;
            }
        }

        /// <summary>
        /// Collection of selected items.
        /// </summary>
        public ObservableCollection<object> SelectedItems
        {
            get
            {
                return _selectedItems;
            }
        }

        /// <summary>
        /// Is only one object can be selected at one moment.
        /// </summary>
        public bool SingleSelection
        {
            get
            {
                return _singleSelection;
            }
            set
            {
                _singleSelection = value;
            }
        }

        /// <summary>
        /// Is tips on map enabled for this layer.
        /// </summary>
        public bool MapTipEnabled
        {
            get { return _maptipEnabled; }
        }

        /// <summary>
        /// Type of data in this layer.
        /// </summary>
        public Type LayerType
        {
            get { return _dataType; }
        }

        /// <summary>
        /// Is clusterer need to be created for this layer.
        /// </summary>
        public bool UseClustering
        {
            get;
            private set;
        }

        /// <summary>
        /// Is layer opacity constant.
        /// </summary>
        public bool ConstantOpacity
        {
            get;
            set;
        }

        /// <summary>
        /// Object, depending on whose properties graphics changes their view.
        /// </summary>
        public object LayerContext
        {
            get
            {
                return _layerContext;
            }
            set
            {
                _layerContext = value;

                foreach (Graphic graphic in MapLayer.Graphics)
                {
                    DataGraphicObject dataGraphicObject = graphic as DataGraphicObject;
                    if (dataGraphicObject != null)
                    {
                        dataGraphicObject.ObjectContext = _layerContext;
                    }
                }
            }
        }

        /// <summary>
        /// Layer name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Map Spatial Reference.
        /// </summary>
        public int? SpatialReferenceID
        {
            get
            {
                return _spatialReferenceID;
            }
            set
            {
                Debug.Assert(_spatialReferenceID == null);

                _spatialReferenceID = value;

                // Project all graphics to map spatial reference.
                foreach (DataGraphicObject graphic in MapLayer.Graphics)
                {
                    graphic.ProjectGeometry();
                }
            }
        }

        /// <summary>
        /// Is layer not need to react on mouse events.
        /// </summary>
        public bool IsBackgroundLayer
        {
            get;
            set;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Remove selection.
        /// </summary>
        /// <param name="graphic">Graphic to remove selection.</param>
        public void DoUnselect(Graphic graphic)
        {
            int zIndex = graphic.GetZIndex();
            graphic.UnSelect();
            zIndex = zIndex ^ SELECTEDZINDEX;
            graphic.SetZIndex(zIndex);
        }

        /// <summary>
        /// Set selection.
        /// </summary>
        /// <param name="graphic">Graphic to set selection.</param>
        public void DoSelect(Graphic graphic)
        {
            int zIndex = graphic.GetZIndex();
            graphic.Select();
            zIndex = zIndex ^ SELECTEDZINDEX;
            graphic.SetZIndex(zIndex);
        }

        /// <summary>
        /// Enable showing tooltip on map.
        /// </summary>
        public void EnableToolTip()
        {
            Debug.Assert(!_maptipEnabled);

            _maptipEnabled = true;
        }

        /// <summary>
        /// Create graphic object for data element.
        /// </summary>
        /// <param name="data">Data object to show on map.</param>
        /// <param name="typeOfData">Type of data.</param>
        /// <param name="graphicObjectType">Graphic, associated with data object.</param>
        /// <param name="layerContext">Layer context.</param>
        /// <returns>Created graphic object.</returns>
        public Graphic CreateGraphic(object data, Type typeOfData, Type graphicObjectType, object layerContext)
        {
            Graphic graphic = null;
            Type type = data.GetType();

            if (type == typeof(Location))
                graphic = LocationGraphicObject.Create((Location)data);
            else if (type == typeof(Order))
                graphic = OrderGraphicObject.Create((Order)data);
            else if (type == typeof(EditingMarker))
                graphic = EditMarkerGraphicObject.Create((EditingMarker)data);
            else if (type == typeof(AddressCandidate))
                graphic = CandidateGraphicObject.Create((AddressCandidate)data);
            else if (type == typeof(Route))
                graphic = RouteGraphicObject.Create((Route)data);
            else if (type == typeof(Stop))
                graphic = StopGraphicObject.Create((Stop)data);
            else if (type == typeof(Zone))
                graphic = ZoneGraphicObject.Create((Zone)data);
            else if (type == typeof(Barrier))
                graphic = BarrierGraphicObject.Create((Barrier)data);
            else
                Debug.Assert(false);

            DataGraphicObject dataGraphicObject = graphic as DataGraphicObject;
            if (dataGraphicObject != null)
            {
                dataGraphicObject.ParentLayer = this;
                _graphicDictionary.Add(data, dataGraphicObject);
            }

            return graphic;
        }

        /// <summary>
        /// Create graphic object for data element.
        /// </summary>
        /// <param name="data">Object to show on map.</param>
        /// <returns>Graphic object associated with data object.</returns>
        public Graphic CreateGraphic(object data)
        {
            return CreateGraphic(data, null, null, null);
        }

        /// <summary>
        /// Delete graphic object from layer.
        /// </summary>
        /// <param name="data">Data object to delete.</param>
        /// <param name="layer">Graphics layer from which to delete.</param>
        public static void DeleteObject(object data, GraphicsLayer layer)
        {
            for (int index = layer.Graphics.Count - 1; index >= 0; index--)
            {
                DataGraphicObject graphic = (DataGraphicObject)layer.Graphics[index];
                if (graphic.Data == data)
                {
                    layer.Graphics.Remove(graphic);

                    DataGraphicObject dataGraphicObject = graphic as DataGraphicObject;
                    if (dataGraphicObject != null)
                        dataGraphicObject.UnsubscribeOnChange();
                    break;
                }
            }
        }

        /// <summary>
        /// Find graphic object by associated data object.
        /// </summary>
        /// <param name="data">Key data.</param>
        /// <returns>Graphic object, which represents data on map.</returns>
        public DataGraphicObject FindGraphicByData(object data)
        {
            DataGraphicObject graphicResult = null;
            if (_graphicDictionary.ContainsKey(data))
            {
                graphicResult = _graphicDictionary[data];
            }

            return graphicResult;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Init class instance and subscribe to events.
        /// </summary>
        /// <param name="collection">Data collection for drawing.</param>
        /// <param name="type">Type of data.</param>
        /// <param name="useClusterer">Use clusterer.</param>
        private void _Init(IEnumerable collection, Type type, bool useClusterer)
        {
            _selectedItems = new ObservableCollection<object>();
            _selectedItems.CollectionChanged +=
                new NotifyCollectionChangedEventHandler(_SelectedItemsCollectionChanged);

            _dataType = type;
            Collection = (IEnumerable)collection;

            // Stops and orders are depends on symbology settings.
            if (type == typeof(Stop) || type == typeof(Order))
                SymbologyManager.OnSettingsChanged += new EventHandler(_SymbologyManagerOnSettingsChanged);

            UseClustering = useClusterer;
        }

        /// <summary>
        /// React on symbology settings changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _SymbologyManagerOnSettingsChanged(object sender, EventArgs e)
        {
            foreach (Graphic graphic in MapLayer)
            {
                StopGraphicObject stopGraphic = graphic as StopGraphicObject;
                if (stopGraphic != null)
                    stopGraphic.InitSymbology();

                OrderGraphicObject orderGraphic = graphic as OrderGraphicObject;
                if (orderGraphic != null)
                    SymbologyManager.InitGraphic(orderGraphic);
            }
        }

        /// <summary>
        /// Add collection elements to map.
        /// </summary>
        private void _InitializeGraphics()
        {
            if (_collection != null)
            {
                foreach (object dataObject in _collection)
                {
                    Debug.Assert(_dataType == dataObject.GetType());
                    Graphic graphic = CreateGraphic(dataObject, _dataType, _graphicObjectType, LayerContext);

                    DataGraphicObject dataGraphicObject = graphic as DataGraphicObject;
                    if (dataGraphicObject != null && _layerContext != null)
                        dataGraphicObject.ObjectContext = _layerContext;

                    if (graphic != null)
                        _layer.Graphics.Add(graphic);
                }
            }
        }

        /// <summary>
        /// React on collection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        System.Diagnostics.Debug.Assert(e.NewItems.Count > 0);
                        foreach (object data in e.NewItems)
                        {
                            // If graphic for this item was already created (to show not yet committed item)
                            // - do not create it again.
                            if (FindGraphicByData(data) == null)
                            {
                                Graphic graphic = CreateGraphic(data, _dataType, _graphicObjectType, LayerContext);

                                if (graphic != null)
                                {
                                    DataGraphicObject dataGraphicObject = graphic as DataGraphicObject;
                                    if (dataGraphicObject != null && _layerContext != null)
                                        dataGraphicObject.ObjectContext = _layerContext;

                                    _layer.Graphics.Add(graphic);
                                }
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        System.Diagnostics.Debug.Assert(e.OldItems.Count > 0);
                        foreach (object data in e.OldItems)
                        {
                            DeleteObject(data, _layer);
                            _graphicDictionary.Remove(data);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (Graphic graphic in _layer.Graphics)
                        {
                            DataGraphicObject dataGraphicObject = graphic as DataGraphicObject;
                            if (dataGraphicObject != null)
                                dataGraphicObject.UnsubscribeOnChange();
                        }

                        _graphicDictionary.Clear();
                        _layer.Graphics.Clear();
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// React on selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Event args.</param>
        private void _SelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        System.Diagnostics.Debug.Assert(e.NewItems.Count > 0);
                        foreach (object data in e.NewItems)
                        {
                            Graphic graphic = FindGraphicByData(data);
                            if (graphic != null && !graphic.Selected)
                                DoSelect(graphic);
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        System.Diagnostics.Debug.Assert(e.OldItems.Count > 0);
                        foreach (object data in e.OldItems)
                        {
                            Graphic graphic = FindGraphicByData(data);
                            if (graphic != null && graphic.Selected)
                            {
                                DoUnselect(graphic);
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        if (_collection != null)
                        {
                            foreach (object data in _collection)
                            {
                                Graphic graphic = FindGraphicByData(data);
                                if (graphic != null && graphic.Selected)
                                {
                                    DoUnselect(graphic);
                                }
                            }
                        }
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region private members

        /// <summary>
        /// Layer selected items.
        /// </summary>
        private ObservableCollection<object> _selectedItems;

        /// <summary>
        /// Is objects in layer can be selected on map.
        /// </summary>
        private bool _selectable;

        /// <summary>
        /// Graphic layer to out on map.
        /// </summary>
        private GraphicsLayer _layer;

        /// <summary>
        /// Collection of data objects to show on map.
        /// </summary>
        private IEnumerable _collection;

        /// <summary>
        /// Type of data to show, using this objectlayer.
        /// </summary>
        private Type _dataType;

        /// <summary>
        /// Type of graphic objects to create for showing data.
        /// </summary>
        private Type _graphicObjectType;

        /// <summary>
        /// Is only object can be selected in one moment.
        /// </summary>
        private bool _singleSelection;

        /// <summary>
        /// Is map tip enabled for this layer.
        /// </summary>
        private bool _maptipEnabled;

        /// <summary>
        /// Layer context.
        /// </summary>
        private object _layerContext;

        /// <summary>
        /// Map spatial reference ID.
        /// </summary>
        private int? _spatialReferenceID;

        /// <summary>
        /// Dictionary to improve perfomance in find graphic by dataobject.
        /// </summary>
        private Dictionary<object, DataGraphicObject> _graphicDictionary = new Dictionary<object, DataGraphicObject>();

        #endregion
    }
}
