using System.Windows;
using System.Windows.Controls;

using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Overlay window docking button
    /// </summary>
    internal class OverlayWindowDockingButton : IDropSurface
    {
        #region Constants
        
        /// <summary>
        /// Buttons mouse over template resource name.
        /// </summary>
        private const string HOVERED_TEMPLATE = "HoveredGlassButton";

        /// <summary>
        /// Buttons default resource name.
        /// </summary>
        private const string NORMAL_TEMPLATE = "GlassButton";

        #endregion Constants

        #region Members
        /// <summary>
        /// Owner overlay window
        /// </summary>
        private OverlayWindow _owner = null;

        /// <summary>
        /// Dock button
        /// </summary>
        private Button _btnDock = null;

        /// <summary>
        /// Enabled flag
        /// </summary>
        private bool _enabled = true;

        /// <summary>
        /// Set enabled flag
        /// </summary>
        public bool Enabled
        {
            set { _enabled = value; }
        }
        #endregion // Members

        #region Constructors
        /// <summary>
        /// Create overlay window docking button
        /// </summary>
        /// <param name="btnDock">Button dock</param>
        /// <param name="owner">Overlay window</param>
        public OverlayWindowDockingButton(Button btnDock, OverlayWindow owner) : this(btnDock, owner, true) {}
        /// <summary>
        /// Create overlay window docking button
        /// </summary>
        /// <param name="btnDock">Button dock</param>
        /// <param name="owner">Overlay window</param>
        /// <param name="enabled">Enabled flag</param>
        public OverlayWindowDockingButton(Button btnDock, OverlayWindow owner, bool enabled)
        {
            _btnDock = btnDock;
            _owner = owner;
            _enabled = enabled;
        }
        #endregion // Constructors

        #region IDropSurface
        /// <summary>
        /// Get surface rectangle
        /// </summary>
        /// <returns>Returns a rectangle where this surface is active</returns>
        public Rect SurfaceRectangle
        {
            get 
            {
                if (!_owner.IsLoaded)
                    return new Rect();

                Point pt = _owner.DockManager.ConvertRelativePointToScreenInDefaultDpi(_btnDock, new Point(0, 0));
                return new Rect(pt, new Size(_btnDock.ActualWidth, _btnDock.ActualHeight)); 
            }
        }

        /// <summary>
        /// Handles this sourface mouse entering
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public void OnDragEnter(Point point)
        {
        }

        /// <summary>
        /// Handles mouse overing this surface
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public void OnDragOver(Point point)
        {
            _btnDock.Template = (ControlTemplate)App.Current.FindResource(HOVERED_TEMPLATE);
        }

        /// <summary>
        /// Handles mouse leave event during drag
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public void OnDragLeave(Point point)
        {
            _btnDock.Template = (ControlTemplate)App.Current.FindResource(NORMAL_TEMPLATE);
        }

        /// <summary>
        /// Handler drop events
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public bool OnDrop(Point point)
        {
            _btnDock.Template = (ControlTemplate)App.Current.FindResource(NORMAL_TEMPLATE);

            if (!_enabled)
                return false;

            return _owner.OnDrop(_btnDock);
        }
        #endregion // IDropSurface
    }

    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    internal partial class OverlayWindow : Window
    {
        #region Members
        /// <summary>
        /// Left overlay window docking button
        /// </summary>
        private readonly OverlayWindowDockingButton _owdLeft = null;
        /// <summary>
        /// Right overlay window docking button
        /// </summary>
        private readonly OverlayWindowDockingButton _owdRight = null;
        /// <summary>
        /// Top overlay window docking button
        /// </summary>
        private readonly OverlayWindowDockingButton _owdTop = null;
        /// <summary>
        /// Bottom overlay window docking button
        /// </summary>
        private readonly OverlayWindowDockingButton _owdBottom = null;

        /// <summary>
        /// Current pane to drop
        /// </summary>
        private DockablePane _paneDropCurrent = null;

        /// <summary>
        /// Dock manager
        /// </summary>
        private readonly DockManager _owner = null;
        #endregion // Members

        #region Constructors
        /// <summary>
        /// Create overlay window
        /// </summary>
        /// <param name="owner">Dock manager</param>
        public OverlayWindow(DockManager owner)
        {
            InitializeComponent();

            _owner = owner;

            _owner.DragPaneServices.Register(new OverlayWindowDockingButton(btnDockBottom, this));
            _owner.DragPaneServices.Register(new OverlayWindowDockingButton(btnDockTop, this));
            _owner.DragPaneServices.Register(new OverlayWindowDockingButton(btnDockLeft, this));
            _owner.DragPaneServices.Register(new OverlayWindowDockingButton(btnDockRight, this));

            _owdBottom = new OverlayWindowDockingButton(btnDockPaneBottom, this, false);
            _owdTop = new OverlayWindowDockingButton(btnDockPaneTop, this, false);
            _owdLeft = new OverlayWindowDockingButton(btnDockPaneLeft, this, false);
            _owdRight = new OverlayWindowDockingButton(btnDockPaneRight, this, false);

            _owner.DragPaneServices.Register(_owdBottom);
            _owner.DragPaneServices.Register(_owdTop);
            _owner.DragPaneServices.Register(_owdLeft);
            _owner.DragPaneServices.Register(_owdRight);
        }
        #endregion // Constructors

        #region Public functions
        /// <summary>
        /// Show overlay pane docking options
        /// </summary>
        /// <param name="owner">Pane to manage</param>
        public void ShowOverlayPaneDockingOptions(DockablePane pane)
        {
            Rect rectPane = pane.SurfaceRectangle;

            Point myScreenTopLeft = _owner.ConvertRelativePointToScreenInDefaultDpi(this, new Point(0, 0));
            rectPane.Offset(-myScreenTopLeft.X, -myScreenTopLeft.Y); // relative to me
            gridPaneRelativeDockingOptions.SetValue(Canvas.LeftProperty, rectPane.Left + rectPane.Width / 2 - gridPaneRelativeDockingOptions.Width / 2);
            gridPaneRelativeDockingOptions.SetValue(Canvas.TopProperty, rectPane.Top + rectPane.Height / 2 - gridPaneRelativeDockingOptions.Height / 2);
            gridPaneRelativeDockingOptions.Visibility = Visibility.Visible;

            _owdBottom.Enabled = true;
            _owdTop.Enabled = true;
            _owdLeft.Enabled = true;
            _owdRight.Enabled = true;
            _paneDropCurrent = pane;
        }

        /// <summary>
        /// Hide overlay pane docking options
        /// </summary>
        public void HideOverlayPaneDockingOptions()
        {
            _owdBottom.Enabled = false;
            _owdTop.Enabled = false;
            _owdLeft.Enabled = false;
            _owdRight.Enabled = false;

            gridPaneRelativeDockingOptions.Visibility = Visibility.Collapsed;
            _paneDropCurrent = null;
        }

        /// <summary>
        /// Show overlay pane docking options
        /// </summary>
        /// <param name="btnDock">Clicked button</param>
        public bool OnDrop(Button btnDock)
        {
            // hack
            DockablePane pane = _owner.DragPaneServices.FloatingWindow.PaneHosted;
            pane.UseSpecAllocation = false;
            Size sz = pane.Dimensions;
            if ((btnDock == btnDockLeft) || (btnDock == btnDockRight))
                sz.Width = 150;
            else // ((btnDock == btnDockTop) || (btnDock == btnDockBottom))
                sz.Height = 150;
            pane.Dimensions = sz;

            if (btnDock == btnDockTop)
                pane.ChangeDock(Dock.Top);
            else if (btnDock == btnDockLeft)
                pane.ChangeDock(Dock.Left);
            else if (btnDock == btnDockRight)
                pane.ChangeDock(Dock.Right);
            else if (btnDock == btnDockBottom)
                pane.ChangeDock(Dock.Bottom);

            else if (btnDock == btnDockPaneTop)
                pane.MoveTo(_paneDropCurrent, Dock.Top);
            else if (btnDock == btnDockPaneBottom)
                pane.MoveTo(_paneDropCurrent, Dock.Bottom);
            else if (btnDock == btnDockPaneLeft)
                pane.MoveTo(_paneDropCurrent, Dock.Left);
            else if (btnDock == btnDockPaneRight)
                pane.MoveTo(_paneDropCurrent, Dock.Right);

            else
            {
                System.Diagnostics.Debug.Assert(false); // NOTE: not supported
            }

            return true;
        }

        /// <summary>
        /// Get dock manager
        /// </summary>
        public DockManager DockManager
        {
            get { return _owner; }
        }
        #endregion // Public functions
    }
}
