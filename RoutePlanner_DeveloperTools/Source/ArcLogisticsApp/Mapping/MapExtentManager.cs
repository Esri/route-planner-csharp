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
using System.Windows;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class for managing map extent.
    /// </summary>
    class MapExtentManager
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mapControl">Parent map control.</param>
        /// <param name="clustering">Clustering manager.</param>
        /// <param name="mapSelectionManager">Map selection manager.</param>
        public MapExtentManager(MapControl mapControl, Clustering clustering, MapSelectionManager mapSelectionManager)
        {
            Debug.Assert(mapControl != null);
            Debug.Assert(clustering != null);
            Debug.Assert(mapSelectionManager != null);

            _mapControl = mapControl;
            _clustering = clustering;
            _mapSelectionManager = mapSelectionManager;

            _InitEventHandlers();
        }

        #endregion

        #region Public members

        /// <summary>
        /// Map Extent.
        /// </summary>
        public ESRI.ArcLogistics.Geometry.Envelope Extent
        {
            get
            {
                Debug.Assert(_mapControl != null);

                ESRI.ArcLogistics.Geometry.Envelope rect;
                if (_mapControl.map.Extent != null)
                {
                    rect = GeometryHelper.CreateRect(_mapControl.map.Extent, _mapControl.Map.SpatialReferenceID);
                }
                else
                {
                    rect = new ESRI.ArcLogistics.Geometry.Envelope();
                    rect.SetEmpty();
                }

                return rect;
            }
            set
            {
                Debug.Assert(_mapControl != null);

                Envelope extent = GeometryHelper.CreateExtent(value, _mapControl.Map.SpatialReferenceID);
                _mapControl.map.Extent = extent;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Zoom to resolution extent.
        /// </summary>
        /// <param name="extent">New extent.</param>
        public void ZoomTo(ESRI.ArcGIS.Client.Geometry.Geometry extent)
        {
            Debug.Assert(_mapControl != null);

            // Catching exception from map.
            try
            {
                if (_mapControl.Map.SpatialReferenceID.HasValue)
                {
                    extent.SpatialReference = new SpatialReference(_mapControl.Map.SpatialReferenceID.Value);
                }

                _mapControl.map.ZoomTo(extent);
            }
            catch { }
        }

        /// <summary>
        /// Set map extent to collection.
        /// </summary>
        /// <param name="coll">Collection of items, which needs to be in extent.</param>
        public void SetExtentOnCollection(IList coll)
        {
            Debug.Assert(coll != null);
            Debug.Assert(_mapControl != null);

            List<ESRI.ArcLogistics.Geometry.Point> points = MapExtentHelpers.GetPointsInExtent(coll);
            // Save extent.
            _oldExtent = MapExtentHelpers.GetCollectionExtent(points);
            MapExtentHelpers.SetExtentOnCollection(_mapControl, points);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates all common event handlers.
        /// </summary>
        private void _InitEventHandlers()
        {
            Debug.Assert(_mapControl != null);

            _mapControl.map.ExtentChanging += new EventHandler<ExtentEventArgs>(_MapExtentChanging);
            _mapControl.map.ExtentChanged += new EventHandler<ExtentEventArgs>(_MapExtentChanged);

            _mapControl.map.SizeChanged += new SizeChangedEventHandler(_MapSizeChanged);

            ObservableCollection<object> selection = (ObservableCollection<object>)_mapControl.SelectedItems;
            selection.CollectionChanged += new NotifyCollectionChangedEventHandler(_SelectionCollectionChanged);
        }

        /// <summary>
        /// React on selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Collection changed event args.</param>
        private void _SelectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.Assert(_mapSelectionManager != null);
            Debug.Assert(_mapControl != null);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (!_mapSelectionManager.SelectionChangedFromMap)
                {
                    _mapControl.Dispatcher.BeginInvoke(new ExtentChangingDelegate(_PostponedExtentChanging), DispatcherPriority.Input);
                    _callsNeeded++;
                }
            }
        }

        /// <summary>
        /// Do postponed extent changed. Needed because selection can change more than once,
        /// but extent need to be changed only once.
        /// </summary>
        private void _PostponedExtentChanging()
        {
            Debug.Assert(_mapSelectionManager != null);
            Debug.Assert(_mapControl != null);

            _calls++;
            if (_calls == _callsNeeded)
            {
                _calls = 0;
                _callsNeeded = 0;

                // Workaround:
                // If current selection equals previous - not need to change extent.
                // Needed because selection changed event can come from listview on drag&drop,
                // but if selection really not changed than do not change extent.
                bool needToChangeExtent = _mapControl.SelectedItems.Count != _mapSelectionManager.PreviousSelection.Count;
                if (!needToChangeExtent)
                {
                    foreach (object item in _mapSelectionManager.PreviousSelection)
                    {
                        if (!_mapControl.SelectedItems.Contains(item))
                        {
                            needToChangeExtent = true;
                            break;
                        }
                    }

                    foreach (object item in _mapControl.SelectedItems)
                    {
                        if (!_mapSelectionManager.PreviousSelection.Contains(item))
                        {
                            needToChangeExtent = true;
                            break;
                        }
                    }
                }

                if (needToChangeExtent)
                {
                    _mapControl.SetExtentOnCollection(_mapControl.SelectedItems);
                }

                // Clear previous selection.
                _mapSelectionManager.PreviousSelection.Clear();
                _mapSelectionManager.IsSelectionStored = false;
            }
        }

        /// <summary>
        /// React on map size changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Size changed events.</param>
        private void _MapSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.Assert(_mapControl != null);

            // Do not react on map size changed if it is not initialized.
            if (!_mapControl.Map.IsInitialized())
            {
                return;
            }

            if (_mapControl.IgnoreSizeChanged)
            {
                _mapControl.IgnoreSizeChanged = false;
            }
            else
            {
                if (!_mapSizeChangedProcessed)
                {
                    // Increase counter of changing extent by changing map control size, but not more than 2.
                    _mapExtentChangedPostponedCount++;

                    // Call suspended extent changing with low priority because layout need to be updated.
                    _mapControl.Dispatcher.BeginInvoke(new ExtentRestoringDelegate(_ChangeExtentOnViewVisibilityChanged),
                        DispatcherPriority.Background, _mapControl.Extent);

                    _mapSizeChangedProcessed = true;
                }
            }
        }

        /// <summary>
        /// Suspended extent changing.
        /// </summary>
        private void _ChangeExtentOnViewVisibilityChanged(ESRI.ArcLogistics.Geometry.Envelope oldExtent)
        {
            Debug.Assert(_mapControl != null);

            if (_oldExtent != null)
            {
                // Increase counter of changing extent by changing map control size, but not more than 2
                _mapExtentChangedPostponedCount++;
                if (_mapExtentChangedPostponedCount > MAX_EXTENT_CHANGING_BY_CHANGE_SIZE)
                {
                    _mapExtentChangedPostponedCount = MAX_EXTENT_CHANGING_BY_CHANGE_SIZE;
                }

                ESRI.ArcGIS.Client.Geometry.Envelope extent = GeometryHelper.CreateExtent(_oldExtent.Value, _mapControl.Map.SpatialReferenceID);
                _mapControl.ZoomTo(extent);
            }

            _mapSizeChangedProcessed = false;
        }

        /// <summary>
        /// React on map extent changing.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapExtentChanging(object sender, ExtentEventArgs e)
        {
            Debug.Assert(_clustering != null);

            _clustering.UnexpandIfExpanded();
        }

        /// <summary>
        /// React on map extent changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapExtentChanged(object sender, ExtentEventArgs e)
        {
            Debug.Assert(_clustering != null);

            _clustering.UnexpandIfExpanded();

            // Save extent if extent was changed by user pan or zoom
            if (_mapExtentChangedPostponedCount > 0)
            {
                // Decrease counter of changing extent by changing map control size
                _mapExtentChangedPostponedCount--;
            }
            else
            if (_mapExtentChangedPostponedCount == 0)
            {
                _oldExtent = _mapControl.Extent;
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// Maximum value of extent changing counter.
        /// </summary>
        private const int MAX_EXTENT_CHANGING_BY_CHANGE_SIZE = 2;

        #endregion

        #region Private fields

        /// <summary>
        /// Parent map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Counter of how many times postponed extent changing method was executed.
        /// </summary>
        private int _calls;

        /// <summary>
        /// Counter of how many times postponed extent changing method was invoked.
        /// </summary>
        private int _callsNeeded;

        /// <summary>
        /// Counter of not finalized extent changed actions.
        /// </summary>
        private int _mapExtentChangedPostponedCount;

        /// <summary>
        /// Saved extent, made by pan or zoom on map.
        /// </summary>
        private ESRI.ArcLogistics.Geometry.Envelope? _oldExtent;

        /// <summary>
        /// 
        /// </summary>
        private bool _mapSizeChangedProcessed;

        /// <summary>
        /// Clustering manager.
        /// </summary>
        private Clustering _clustering;

        /// <summary>
        /// Map selection manager.
        /// </summary>
        private MapSelectionManager _mapSelectionManager;

        /// <summary>
        /// Delegate for invoking extent changing.
        /// </summary>
        private delegate void ExtentChangingDelegate();

        /// <summary>
        /// Delegate for invoking extent restoring.
        /// </summary>
        private delegate void ExtentRestoringDelegate(ESRI.ArcLogistics.Geometry.Envelope oldExtent);

        #endregion
    }
}
