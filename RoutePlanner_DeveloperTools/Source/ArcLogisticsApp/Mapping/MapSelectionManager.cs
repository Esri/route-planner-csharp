using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class for managing selection on map.
    /// </summary>
    class MapSelectionManager
    {
        #region Constructors

        /// <param name="mapControl">Parent map control.</param>
        /// <param name="clustering">Clustering manager.</param>
        /// <param name="objectLayers">Map object layers.</param>
        public MapSelectionManager(MapControl mapControl, Clustering clustering, List<ObjectLayer> objectLayers)
        {
            Debug.Assert(mapControl != null);
            Debug.Assert(clustering != null);
            Debug.Assert(objectLayers != null);

            _mapControl = mapControl;
            _clustering = clustering;
            _objectLayers = objectLayers;

            _InitEventHandlers();

            SelectionFrameLayer = new GraphicsLayer();
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Current selection.
        /// </summary>
        public IList SelectedItems
        {
            get
            {
                return _selectedItems;
            }
        }

        /// <summary>
        /// Previous selection.
        /// </summary>
        public IList PreviousSelection
        {
            get
            {
                return _previousSelection;
            }
        }

        /// <summary>
        /// Is selection changed from map.
        /// </summary>
        public bool SelectionChangedFromMap
        {
            get;
            set;
        }

        /// <summary>
        /// Layer, which contains selection frame.
        /// </summary>
        public GraphicsLayer SelectionFrameLayer
        {
            get;
            private set;
        }

        /// <summary>
        /// Is selection stored.
        /// </summary>
        public bool IsSelectionStored
        {
            get;
            set;
        }

        /// <summary>
        /// Is selection was made.
        /// </summary>
        public bool SelectionWasMade
        {
            get;
            set;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Show selection frame.
        /// </summary>
        /// <param name="startPoint">Selection frame start point.</param>
        public void ShowSelectionFrame(ESRI.ArcGIS.Client.Geometry.MapPoint startPoint)
        {
            Debug.Assert(startPoint != null);
            Debug.Assert(_mapControl != null);

            _selectionFrame = FrameGraphicObject.Create(_mapControl.Map.SpatialReferenceID);
            SelectionFrameLayer.Graphics.Add(_selectionFrame);

            _selectionFrame.Start = new ESRI.ArcLogistics.Geometry.Point(startPoint.X, startPoint.Y);
        }

        /// <summary>
        /// Move selection frame end point.
        /// </summary>
        /// <param name="endPoint">Selection frame end point.</param>
        public void MoveSelectionFrame(ESRI.ArcGIS.Client.Geometry.MapPoint endPoint)
        {
            Debug.Assert(endPoint != null);

            if (_selectionFrame != null)
            {
                _selectionFrame.End = new ESRI.ArcLogistics.Geometry.Point(endPoint.X, endPoint.Y);
            }
        }

        /// <summary>
        /// Finish selecting by selection frame.
        /// </summary>
        /// <param name="endPoint">Selection frame end point.</param>
        public void FinishSelectByFrame(ESRI.ArcGIS.Client.Geometry.MapPoint endPoint)
        {
            Debug.Assert(endPoint != null);
            Debug.Assert(_mapControl != null);

            if (_selectionFrame != null)
            {
                SelectionFrameLayer.Graphics.Clear();

                _selectionFrame.End = new ESRI.ArcLogistics.Geometry.Point(endPoint.X, endPoint.Y);
                _SelectInFrame();

                _mapControl.UpdateLayout();

                _selectionFrame = null;
            }
        }

        /// <summary>
        /// Cancel selection by selection frame.
        /// </summary>
        public void HideSelectionFrame()
        {
            Debug.Assert(SelectionFrameLayer != null);

            if (_selectionFrame != null)
            {
                SelectionFrameLayer.Graphics.Clear();
                _selectionFrame = null;
            }
        }

        /// <summary>
        /// Process graphic mouse events: expand or select.
        /// </summary>
        /// <param name="graphic">Graphic to process events.</param>
        /// <param name="clickedGraphic">Last clicked graphic.</param>
        public void ProcessGraphicMouseEvents(Graphic graphic, Graphic clickedGraphic)
        {
            Debug.Assert(graphic != null);

            DataGraphicObject dataGraphic = graphic as DataGraphicObject;

            ClusterGraphicObject clusterGraphic = graphic as ClusterGraphicObject;
            if (clusterGraphic != null)
            {
                _ProcessClusterGraphicMouseEvents(clusterGraphic, clickedGraphic);
            }
            else if (dataGraphic != null)
            {
                _ProcessDataGraphicMouseEvents(dataGraphic, clickedGraphic);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            Debug.Assert(_selectedItems != null);

            _selectedItems.CollectionChanged += new NotifyCollectionChangedEventHandler(_SelectionCollectionChanged);
        }

        /// <summary>
        /// React on selected items changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Collection changed args.</param>
        private void _SelectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(_objectLayers != null);
            Debug.Assert(_clustering != null);

            _StoreSelectionIfNeeded(e);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (object data in e.NewItems)
                        {
                            // Select item in object layer.
                            ObjectLayer objectLayer = MapHelpers.GetLayerWithData(data, _objectLayers);
                            if (objectLayer != null)
                            {
                                objectLayer.SelectedItems.Add(data);
                            }

                            // Select in clustering layer also.
                            if (_clustering != null)
                            {
                                _clustering.AddToSelection(data);
                            }
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (object data in e.OldItems)
                        {
                            // Deselect item in object layer.
                            ObjectLayer objectLayer = MapHelpers.GetLayerWithData(data, _objectLayers);
                            if (objectLayer != null)
                            {
                                objectLayer.SelectedItems.Remove(data);
                            }

                            // Remove from selection in clustering layer also.
                            if (_clustering != null)
                            {
                                _clustering.RemoveFromSelection(data);
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (ObjectLayer objectLayer in _objectLayers)
                        {
                            objectLayer.SelectedItems.Clear();
                        }
                        break;
                    }
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// If selection was not stored before serie of events - store it
        /// </summary>
        /// <param name="e">Collection changes params</param>
        private void _StoreSelectionIfNeeded(NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(_previousSelection != null);

            if (!IsSelectionStored)
            {
                Debug.Assert(_previousSelection.Count == 0);

                foreach (object item in SelectedItems)
                {
                    _previousSelection.Add(item);
                }

                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (object item in e.NewItems)
                    {
                        _previousSelection.Remove(item);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (object item in e.OldItems)
                    {
                        _previousSelection.Add(item);
                    }
                }

                IsSelectionStored = true;
            }
        }

        /// <summary>
        /// Find object layer, which contains graphic.
        /// </summary>
        /// <param name="graphic">Graphic to find.</param>
        /// <returns>Object layer, if it contains graphic. Null otherwise.</returns>
        private ObjectLayer _FindObjectLayer(Graphic graphic)
        {
            Debug.Assert(graphic != null);
            Debug.Assert(_objectLayers != null);
            Debug.Assert(_clustering != null);

            ObjectLayer layer = null;

            for (int index = 0; index < _objectLayers.Count; index++)
            {
                if (_objectLayers[index].MapLayer.Graphics.Contains(graphic))
                {
                    DataGraphicObject dataGraphic = graphic as DataGraphicObject;
                    if (dataGraphic != null && dataGraphic.Data.GetType() != _objectLayers[index].LayerType &&
                        _objectLayers[index] != _clustering.ClusteringLayer)
                        continue;

                    layer = _objectLayers[index];
                    break;
                }
            }

            return layer;
        }

        /// <summary>
        /// Get items in frame and select them.
        /// </summary>
        private void _SelectInFrame()
        {
            Debug.Assert(SelectedItems != null);
            Debug.Assert(_selectionFrame != null);
            Debug.Assert(_clustering != null);

            ESRI.ArcGIS.Client.Geometry.Envelope frame = _selectionFrame.Geometry.Extent;

            if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control)
            {
                SelectedItems.Clear();
            }

            for (int index = _objectLayers.Count - 1; index >= 0; index--)
            {
                ObjectLayer objectLayer = _objectLayers[index];
                if (objectLayer == _clustering.ClusteringLayer)
                    continue;

                if (objectLayer.Selectable && !objectLayer.SingleSelection)
                {
                    //Get elements in selection frame
                    List<object> elementsInFrame = _GetElementsInSelectionFrame(objectLayer, frame);
                    _ProcessSelectionChanges(elementsInFrame, objectLayer);
                }
            }
        }

        /// <summary>
        /// Get elements in frame.
        /// </summary>
        /// <param name="objectLayer">Layer to find elements.</param>
        /// <param name="frame">Frame.</param>
        /// <returns>Elements in frame.</returns>
        private List<object> _GetElementsInSelectionFrame(ObjectLayer objectLayer,
            ESRI.ArcGIS.Client.Geometry.Envelope frame)
        {
            Debug.Assert(SelectedItems != null);
            Debug.Assert(_selectionFrame != null);
            Debug.Assert(_clustering != null);

            List<object> elementsInFrame = new List<object>();

            foreach (DataGraphicObject graphic in objectLayer.MapLayer.Graphics)
            {
                // Null extent in case of empty polyline.
                if (graphic.Geometry != null && graphic.Geometry.Extent != null
                    && graphic.Data.GetType() == objectLayer.LayerType)
                {
                    // If graphic in frame, data type is equals to objectLayer data type and data contains in objectayercollection.
                    if (MapHelpers.IsIntersects(frame, graphic.Geometry) && graphic.Data.GetType() == objectLayer.LayerType
                        && MapHelpers.CollectionContainsData(objectLayer.Collection, graphic.Data))
                        elementsInFrame.Add(graphic.Data);
                }
            }

            _clustering.AddElementsFromClusterAndNotInFrame(objectLayer, frame, elementsInFrame);

            return elementsInFrame;
        }

        /// <summary>
        /// Process selection changes according to already selected items and keyboard status.
        /// </summary>
        /// <param name="elements">Elements to process.</param>
        /// <param name="objectLayer">Layer to change selection.</param>
        private void _ProcessSelectionChanges(IList<object> elements, ObjectLayer objectLayer)
        {
            Debug.Assert(elements != null);
            Debug.Assert(objectLayer != null);
            Debug.Assert(_mapControl != null);

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                // Foreach element check possibility to being selected and select if not selected yet.
                foreach (object data in elements)
                {
                    if (_mapControl.CanSelectCallback(data) && !objectLayer.SelectedItems.Contains(data))
                    {
                        _AddToSelection(data);
                    }
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Foreach element check possibility to being selected and invert selection.
                foreach (object data in elements)
                {
                    if (_mapControl.CanSelectCallback(data))
                    {
                        if (objectLayer.SelectedItems.Contains(data))
                        {
                            _RemoveFromSelection(data);
                        }
                        else
                        {
                            _AddToSelection(data);
                        }
                    }
                }
            }
            else
            {
                // Clear previous selection...
                objectLayer.SelectedItems.Clear();

                // ... and select elements, if possibility exists.
                if (_mapControl.Map.IsInitialized() &&
                    App.Current.InternalGeocoder.IsInitialized())
                {
                    foreach (object data in elements)
                    {
                        if (_mapControl.CanSelectCallback(data))
                        {
                            _AddToSelection(data);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add item to selection.
        /// </summary>
        /// <param name="item">Item to select.</param>
        private void _AddToSelection(object item)
        {
            Debug.Assert(item != null);
            Debug.Assert(_objectLayers != null);
            Debug.Assert(SelectedItems != null);

            // Candidates selects not in common map selected items, but diretly in object layer.
            if (item is AddressCandidate)
            {
                ObjectLayer candidatesLayer = null;

                foreach (ObjectLayer layer in _objectLayers)
                {
                    if (layer.LayerType == typeof(AddressCandidate))
                    {
                        candidatesLayer = layer;
                        break;
                    }
                }

                Debug.Assert(candidatesLayer != null);

                candidatesLayer.SelectedItems.Clear();
                candidatesLayer.SelectedItems.Add(item);
            }
            else
            {
                SelectionChangedFromMap = true;
                SelectedItems.Add(item);
                SelectionChangedFromMap = false;
            }
        }

        /// <summary>
        /// Remove item from selection.
        /// </summary>
        /// <param name="item">Item to remove from selection.</param>
        private void _RemoveFromSelection(object item)
        {
            Debug.Assert(item != null);
            Debug.Assert(SelectedItems != null);

            Debug.Assert(!(item is AddressCandidate));

            SelectionChangedFromMap = true;
            SelectedItems.Remove(item);
            SelectionChangedFromMap = false;
        }

        /// <summary>
        /// Process cluster graphic mouse events.
        /// </summary>
        /// <param name="clusterGraphic">Cluster graphic.</param>
        /// <param name="clickedGraphic">Last clicked item.</param>
        private void _ProcessClusterGraphicMouseEvents(ClusterGraphicObject clusterGraphic, Graphic clickedGraphic)
        {
            Debug.Assert(clusterGraphic != null);
            Debug.Assert(clickedGraphic != null);
            Debug.Assert(_clustering != null);
            Debug.Assert(_objectLayers != null);

            if (!_clustering.ClusterExpanded)
            {
                _clustering.ExpandIfNeeded(clusterGraphic);
            }
            else
            {
                IList<object> clusteredData = _clustering.GetClusteredData(clusterGraphic);
                ObjectLayer layer = MapHelpers.GetLayerWithData(clusteredData[0], _objectLayers);
                if (clickedGraphic == clusterGraphic && layer.Selectable)
                {
                    if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control)
                    {
                        _mapControl.SelectedItems.Clear();
                    }

                    _ProcessSelectionChanges(clusteredData, layer);
                }
            }
        }

        /// <summary>
        /// Process data graphic mouse events.
        /// </summary>
        /// <param name="dataGraphic">Data graphic.</param>
        /// <param name="clickedGraphic">Last clicked item.</param>
        private void _ProcessDataGraphicMouseEvents(DataGraphicObject dataGraphic, Graphic clickedGraphic)
        {
            Debug.Assert(dataGraphic != null);
            Debug.Assert(_mapControl != null);

            ObjectLayer layer = _FindObjectLayer(dataGraphic);

            // Candidates selects not in common map selected items, but diretly in object layer.
            if (dataGraphic is CandidateGraphicObject)
            {
                _AddToSelection(dataGraphic.Data);
            }
            else if (layer != null && layer.Selectable && !_mapControl.IsInEditedMode)
            {
                // Check down and up points equals.
                if (clickedGraphic == dataGraphic && layer.Selectable)
                {
                    // Do not need to clear selection and return the same element.
                    if (!(SelectedItems.Count == 1 && SelectedItems[0] == dataGraphic.Data))
                    {
                        if (Keyboard.Modifiers != ModifierKeys.Shift && Keyboard.Modifiers != ModifierKeys.Control && SelectedItems.Count > 0)
                        {
                            SelectedItems.Clear();
                        }

                        SelectionWasMade = true;

                        List<object> elementList = new List<object>();
                        elementList.Add(dataGraphic.Data);
                        _ProcessSelectionChanges(elementList, layer);
                    }
                }
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// Parent map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Selection frame.
        /// </summary>
        private FrameGraphicObject _selectionFrame;

        /// <summary>
        /// Selected items.
        /// </summary>
        private ObservableCollection<object> _selectedItems = new ObservableCollection<object>();
        
        /// <summary>
        /// Clustering manager.
        /// </summary>
        private Clustering _clustering;

        /// <summary>
        /// Map object layers.
        /// </summary>
        private List<ObjectLayer> _objectLayers;

        /// <summary>
        /// Previous selection.
        /// </summary>
        private List<object> _previousSelection = new List<object>();

        #endregion
    }
}