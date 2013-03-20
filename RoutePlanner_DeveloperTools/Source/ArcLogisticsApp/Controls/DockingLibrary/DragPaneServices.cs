using System.Windows;
using System.Collections.Generic;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Drag pane services
    /// </summary>
    internal class DragPaneServices
    {
        #region Members
        /// <summary>
        /// List of managed surfaces
        /// </summary>
        private List<IDropSurface> _surfaces = new List<IDropSurface>();

        /// <summary>
        /// List of managed surfaces with drag over
        /// </summary>
        private List<IDropSurface> _surfacesWithDragOver = new List<IDropSurface>();

        /// <summary>
        /// Offset to be use to set floating window screen position
        /// </summary>
        private Point _offset;

        /// <summary>
        /// Current managed floating window
        /// </summary>
        private FloatingWindow _wnd = null;

        /// <summary>
        /// Get current managed floating window
        /// </summary>
        public FloatingWindow FloatingWindow
        {
            get { return _wnd; }
        }
        #endregion // Members

        #region Constructors
        /// <summary>
        /// Create drag pane services
        /// </summary>
        public DragPaneServices()
        {
        }
        #endregion // Constructors

        #region Public functions
        /// <summary>
        /// Registry as new drop surface
        /// </summary>
        /// <param name="surface">Drop suface object</param>
        public void Register(IDropSurface surface)
        {
            if (!_surfaces.Contains(surface))
                _surfaces.Add(surface);
        }

        /// <summary>
        /// Unregistry as drop surface
        /// </summary>
        /// <param name="surface">Drop suface object</param>
        public void Unregister(IDropSurface surface)
        {
            _surfaces.Remove(surface);
        }

        /// <summary>
        /// Start drag routine
        /// </summary>
        /// <param name="wnd">Floating window to managed</param>
        /// <param name="point">Current mouse position</param>
        /// <param name="offset">Offset to be use to set floating window screen position</param>
        public void StartDrag(FloatingWindow wnd, Point point, Point offset)
        {
            _wnd = wnd;
            _offset = offset;

            if (_wnd.Width <= _offset.X)
                _offset.X = _wnd.Width / 2;

            _wnd.Left = point.X - _offset.X;
            _wnd.Top = point.Y - _offset.Y;
            if (!_wnd.IsActive)
                _wnd.Show();

            foreach (IDropSurface surface in _surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    _surfacesWithDragOver.Add(surface);
                    surface.OnDragEnter(point);
                }
            }
        }

        /// <summary>
        /// Move drag routine
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public void MoveDrag(Point point)
        {
            if (null == _wnd)
                return;

            _wnd.Left = point.X - _offset.X;
            _wnd.Top = point.Y - _offset.Y;

            List<IDropSurface> enteringSurfaces = new List<IDropSurface>();
            foreach (IDropSurface surface in _surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    if (!_surfacesWithDragOver.Contains(surface))
                        enteringSurfaces.Add(surface);
                    else
                        surface.OnDragOver(point);
                }

                else if (_surfacesWithDragOver.Contains(surface))
                {
                    _surfacesWithDragOver.Remove(surface);
                    surface.OnDragLeave(point);
                }
            }

            foreach (IDropSurface surface in enteringSurfaces)
            {
                _surfacesWithDragOver.Add(surface);
                surface.OnDragEnter(point);
            }
        }

        /// <summary>
        /// End drag routine
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public void EndDrag(Point point)
        {
            IDropSurface dropSufrace = null;
            foreach (IDropSurface surface in _surfaces)
            {
                if (!surface.SurfaceRectangle.Contains(point))
                    continue;

                if (surface.OnDrop(point))
                {
                    dropSufrace = surface;
                    break;
                }
            }

            foreach (IDropSurface surface in _surfacesWithDragOver)
            {
                if (surface != dropSufrace)
                    surface.OnDragLeave(point);
            }

            _surfacesWithDragOver.Clear();

            if (null != dropSufrace)
                _wnd.Close();

            _wnd = null;
        }
        #endregion // Public functions
    }
}
