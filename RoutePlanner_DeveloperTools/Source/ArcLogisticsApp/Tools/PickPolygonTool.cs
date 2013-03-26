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
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Tools
{
    /// <summary>
    /// Class for creating abstract polylines.
    /// </summary>
    abstract class PickPolygonTool : IMapTool
    {

        #region Public members

        /// <summary>
        /// Picked geometry.
        /// </summary>
        public ESRI.ArcGIS.Client.Geometry.Polygon Geometry
        {
            get;
            private set;
        }

        #endregion

        #region ITool members

        /// <summary>
        /// Initializes tool with map control.
        /// </summary>
        /// <param name="mapControl">Parent map control.</param>
        public void Initialize(MapControl mapControl)
        {
            _mapControl = mapControl;
            _cursor = Cursors.Cross;

            CartographicLineSymbol simpleLineSymbol = new CartographicLineSymbol()
            {
                Color=new SolidColorBrush(Colors.Blue),
                Width=2
            };

            SimpleFillSymbol simpleFillSymbol = new SimpleFillSymbol()
            {
                BorderThickness = 1,
                BorderBrush = new System.Windows.Media.SolidColorBrush(Colors.Black),
                Fill = new SolidColorBrush(Color.FromArgb(128,128,128,128))
            };

            _polygonDrawObject = new Draw(mapControl.map);
            _polygonDrawObject.LineSymbol = simpleLineSymbol;
            _polygonDrawObject.FillSymbol = simpleFillSymbol;
            _polygonDrawObject.DrawComplete += _PolygonDrawComplete;
            _polygonDrawObject.DrawMode = DrawMode.Polygon;
        }

        /// <summary>
        /// Tool's cursor.
        /// </summary>
        public Cursor Cursor
        {
            get
            {
                return _cursor;
            }
        }

        /// <summary>
        /// Is tool enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                _NotifyEnabledChanged();
            }
        }

        /// <summary>
        /// Is tool activated.
        /// </summary>
        public bool IsActivated
        {
            get
            {
                return _isActivated;
            }
            private set
            {
                _isActivated = value;
                _NotifyActivatedChanged();
            }
        }

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        public abstract string TooltipText { get; }

        /// <summary>
        /// Tool's title text.
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// Icon's URI source.
        /// </summary>
        public abstract string IconSource { get; }

        /// <summary>
        /// Called when tool is activated on toolbar.
        /// </summary>
        public void Activate()
        {
            _polygonDrawObject.IsEnabled = true;
            IsActivated = true;
        }

        /// <summary>
        /// Called when tool is deactivated on toolbar.
        /// </summary>
        public void Deactivate()
        {
            _polygonDrawObject.IsEnabled = false;
            IsActivated = false;
        }
        
        public bool OnContextMenu(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called on Key down event raised from Map Control when tool is activated.
        /// </summary>
        public void OnKeyDown(int keyCode, int shift)
        {
        }

        /// <summary>
        /// Called on Key up event raised from Map Control when tool is activated.
        /// </summary>
        public void OnKeyUp(int keyCode, int shift)
        {
        }

        /// <summary>
        /// Called on Mouse double click event raised from Map Control when tool is activated.
        /// </summary>
        public void OnDblClick(ModifierKeys modifierKeys, double x, double y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called on MouseDown event raised from Map Control when tool is activated.
        /// </summary>
        public void OnMouseDown(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y)
        {
        }

        /// <summary>
        /// Called on MouseMove event raised from Map Control when tool is activated.
        /// </summary>
        public void OnMouseMove(MouseButtonState left, MouseButtonState right,
            MouseButtonState middle, ModifierKeys modifierKeys, double x, double y)
        {
        }

        /// <summary>
        /// Called on MouseUp event raised from Map Control when tool is activated.
        /// </summary>
        public void OnMouseUp(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y)
        {
        }

        public void Refresh(int hdc)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Event is raised when tool activated.
        /// </summary>
        public event EventHandler ActivatedChanged;

        /// <summary>
        /// Event is raised when tool finished its job.
        /// </summary>
        public event EventHandler OnComplete;

        /// <summary>
        /// Event is raised when tool enability changed.
        /// </summary>
        public event EventHandler EnabledChanged;

        #endregion

        #region Private methods

        /// <summary>
        /// React on draw complete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="args">Event args.</param>
        private void _PolygonDrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            Geometry = (ESRI.ArcGIS.Client.Geometry.Polygon)args.Geometry;
            if (OnComplete != null)
                OnComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises tool enability changed.
        /// </summary>
        private void _NotifyEnabledChanged()
        {
            if (EnabledChanged != null)
                EnabledChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises on tool activated.
        /// </summary>
        private void _NotifyActivatedChanged()
        {
            if (ActivatedChanged != null)
                ActivatedChanged(this, EventArgs.Empty);
        }

        #endregion

        #region Private members

        /// <summary>
        /// Parent map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Tool cursor.
        /// </summary>
        private Cursor _cursor;

        /// <summary>
        /// Is tool enabled.
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// Is tool activated.
        /// </summary>
        private bool _isActivated;

        /// <summary>
        /// Draw object fot polygons.
        /// </summary>
        private Draw _polygonDrawObject;

        #endregion
    }
}