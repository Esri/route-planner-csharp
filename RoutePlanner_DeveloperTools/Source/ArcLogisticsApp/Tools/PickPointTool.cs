using System;
using System.Windows.Input;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.Geometry;

namespace ESRI.ArcLogistics.App.Tools
{
    abstract class PickPointTool : IMapTool
    {
        #region ITool members

        /// <summary>
        /// Initializes tool with map control.
        /// </summary>
        /// <param name="mapControl"></param>
        public void Initialize(MapControl mapControl)
        {
            _mapControl = mapControl;
            _cursor = Cursors.Cross;

            _mapControl.map.MouseEnter += new MouseEventHandler(_MapMouseEnter);
            _mapControl.map.MouseLeave += new MouseEventHandler(_MapMouseLeave);

            _popupAddress.Initialize(_mapControl);
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
            IsActivated = true;
        }

        /// <summary>
        /// Called when tool is deactivated on toolbar.
        /// </summary>
        public void Deactivate()
        {
            IsActivated = false;
        }
        
        public bool OnContextMenu(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void OnDblClick(ModifierKeys modifierKeys, double x, double y)
        {
            throw new NotImplementedException();
        }

        public void OnKeyDown(int keyCode, int shift)
        {
        }

        public void OnKeyUp(int keyCode, int shift)
        {
        }

        public void OnMouseDown(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y)
        {
            if (pressedButton == MouseButton.Left)
            {
                _clickedPoint = new Point(x, y);
            }
        }

        public void OnMouseMove(MouseButtonState left, MouseButtonState right,
            MouseButtonState middle, ModifierKeys modifierKeys, double x, double y)
        {
            Point point = new Point(x, y);

            // Project point from Web Mercator to WGS84 if spatial reference of map is Web Mercator.
            if (_mapControl.Map.SpatialReferenceID.HasValue)
            {
                point = WebMercatorUtil.ProjectPointFromWebMercator(point, _mapControl.Map.SpatialReferenceID.Value);
            }

            _popupAddress.OnMouseMove(point.X, point.Y);
        }

        public void OnMouseUp(MouseButton pressedButton,
            ModifierKeys modifierKeys, double x, double y)
        {
            // If mouse down and mouse up position is equals - pick point.
            if (_clickedPoint.HasValue && _clickedPoint.Value.X == x && _clickedPoint.Value.Y == y)
            {
                X = x;
                Y = y;
                if (OnComplete != null)
                    OnComplete(this, EventArgs.Empty);
                _clickedPoint = null;

                _popupAddress.Disable();
            }
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

        #region public members

        public double? X
        {
            get;
            private set;
        }

        public double? Y
        {
            get;
            private set;
        }

        #endregion

        #region private methods

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

        /// <summary>
        /// React on mouse enter. Enable popup address.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapMouseEnter(object sender, MouseEventArgs e)
        {
            if (_isActivated)
            {
                if (_mapControl.EditedObject != null)
                {
                    _popupAddress.EditingObject = _mapControl.EditedObject;

                    _popupAddress.Enable();
                }
                else if (_mapControl.SelectedItems.Count > 0)
                {
                    _popupAddress.EditingObject = _mapControl.SelectedItems[0];

                    _popupAddress.Enable();
                }
                else
                {
                    // Do nothing: editing object is absent.
                }
            }
        }

        /// <summary>
        /// React on mouse leave. Disable popup address.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MapMouseLeave(object sender, MouseEventArgs e)
        {
            _popupAddress.Disable();
        }

        #endregion

        #region private members

        /// <summary>
        /// Parent map control.
        /// </summary>
        private MapControl _mapControl;

        /// <summary>
        /// Tool cursor.
        /// </summary>
        private Cursor _cursor;

        /// <summary>
        /// Is tool enabled
        /// </summary>
        private bool _isEnabled;

        /// <summary>
        /// Is tool activated.
        /// </summary>
        private bool _isActivated;

        /// <summary>
        /// Point, on which mouse was pressed down.
        /// </summary>
        private Point? _clickedPoint;

        /// <summary>
        /// Popup address helper.
        /// </summary>
        private AddressPopupHelper _popupAddress = new AddressPopupHelper();

        #endregion
    }
}
