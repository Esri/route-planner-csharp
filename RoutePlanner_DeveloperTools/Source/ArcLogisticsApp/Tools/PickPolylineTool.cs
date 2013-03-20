using System;
using System.Windows.Input;
using System.Windows.Media;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Tools
{
    /// <summary>
    /// Class for creating abstract polylines.
    /// </summary>
    abstract class PickPolylineTool : IMapTool
    {
        #region Public members

        /// <summary>
        /// Picked geometry.
        /// </summary>
        public ESRI.ArcGIS.Client.Geometry.Polyline Geometry
        {
            get;
            private set;
        }

        #endregion

        #region ITool members

        /// <summary>
        /// Initializes tool with map control.
        /// </summary>
        /// <param name="mapControl"></param>
        public void Initialize(MapControl mapControl)
        {
            _mapControl = mapControl;
            _cursor = Cursors.Cross;

            SimpleLineSymbol simpleLineSymbol = new SimpleLineSymbol()
            {
                Color=new SolidColorBrush(Colors.Red),
                Width=2
            };

            _polylineDrawObject = new Draw(mapControl.map);
            _polylineDrawObject.LineSymbol = simpleLineSymbol;
            _polylineDrawObject.DrawComplete += _PolylineDrawComplete;
            _polylineDrawObject.DrawMode = DrawMode.Polyline;
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
            _polylineDrawObject.IsEnabled = true;
            IsActivated = true;
        }

        /// <summary>
        /// Called when tool is deactivated on toolbar.
        /// </summary>
        public void Deactivate()
        {
            _polylineDrawObject.IsEnabled = false;
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
        /// Raises on tool activated.
        /// </summary>
        private void _NotifyActivatedChanged()
        {
            if (ActivatedChanged != null)
                ActivatedChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on draw complete.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="args">Event args.</param>
        private void _PolylineDrawComplete(object sender, ESRI.ArcGIS.Client.DrawEventArgs args)
        {
            Geometry = (ESRI.ArcGIS.Client.Geometry.Polyline)args.Geometry;
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
        /// Is tool activated.
        /// </summary>
        private bool _isActivated;

        /// <summary>
        /// Is tool enabled.
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// Draw object fot polylines.
        /// </summary>
        private Draw _polylineDrawObject;

        #endregion
    }
}