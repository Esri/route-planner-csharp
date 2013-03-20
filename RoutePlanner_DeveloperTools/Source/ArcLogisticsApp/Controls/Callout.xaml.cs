using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for Callout.xaml
    /// </summary>
    internal partial class Callout : Popup
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public Callout()
        {
            InitializeComponent();

            CustomPopupPlacementCallback = new CustomPopupPlacementCallback(_GetTopLeftPointShowPointer);

            _GetDimentionsFromResources();
        }

        #endregion

        #region Public method

        /// <summary>
        /// Closing popup.
        /// </summary>
        /// <param name="useAnimation">If 'true' closing will be animated, 
        /// otherwise popup will be immediately closed.</param>
        public void Close(bool useAnimation)
        {
            // Unsubscribe from events.
            App.Current.MainWindow.MouseMove -= _MainWindowMouseMove;
            this.MouseMove -= _MainWindowMouseMove;

            // Close popup with animation.
            if (stackPanel.Opacity == 1 && useAnimation)
            {
                // Find storyboard.
                Storyboard storyBoard = (Storyboard)this.FindResource("CloseStoryboard");

                // When storyboard will be completed - set IsOpen to false.
                storyBoard.Completed += delegate(object sender, System.EventArgs e)
                {
                    this.IsOpen = false;
                };

                // Start storyboard.
                stackPanel.BeginStoryboard(storyBoard, HandoffBehavior.Compose);
            }
            // Immediately close popup.
            else
                IsOpen = false;
        }

        #endregion

        #region Protected method

        /// <summary>
        /// Apply animation to callout opening.
        /// </summary>
        /// <param name="e">Ignored.</param>
        protected override void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);

            // Start opening story board.
            _StartStoryBoardAtGrid("OpenStoryboard", HandoffBehavior.Compose);

            // Subscribe to mouse events.
            App.Current.MainWindow.MouseMove += new System.Windows.Input.
                MouseEventHandler(_MainWindowMouseMove);
            this.MouseMove += new System.Windows.Input.MouseEventHandler(_MainWindowMouseMove);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Get pointer margin and sizes from xaml resources.
        /// </summary>
        private void _GetDimentionsFromResources()
        {
            _pointerMargin = (double)this.Resources["StackPanelsMargin"];
            _pointerHorizontalOffset = (double)this.Resources["PointerHorizontalOffset"];
            _pointerWidth = (double)this.Resources["PointerWidth"];
            _pointerHeight = (double)this.Resources["PointerHeight"];
        }

        /// <summary>
        /// Get the point at which callout will be placed and show the correct pointer.
        /// </summary>
        /// <param name="popupSize">Size.</param>
        /// <param name="targetSize">Size.</param>
        /// <param name="offset">Point.</param>
        /// <returns>If the Popup is hidden by a screen edge at the first position, 
        /// the Popup is placed at the second position and so on.</returns>
        private CustomPopupPlacement[] _GetTopLeftPointShowPointer
            (Size popupSize, Size targetSize, Point offset)
        {
            // If visible part of the cell is smaller then callout pointer - dont show callout.
            if (targetSize.Height < _pointerHeight ||
                targetSize.Width < _ConvertWidth(_pointerHorizontalOffset))
            {
                Close(true);
                return null;
            }

            // Point in which top left corner of callout will be placed.
            CustomPopupPlacement topLeftPoint;

            // Check that this is callout with upper pointer.
            if (_IsUpperPointer())
            {
                _MakeTopPointerVisible();

                // Check that this is left/right pointer and provide proper points for pointer.
                if (_IsLeftPointer())
                {
                    TopPointer.Points = _GetTopLeftPoints();
                    // Callout will point to bottom left corner of target.
                    topLeftPoint = new CustomPopupPlacement(
                        new Point(0, targetSize.Height), PopupPrimaryAxis.Vertical);
                }
                else
                {
                    TopPointer.Points = _GetTopRightPoints(grid.ActualWidth);

                    // Callout will point to bottom right corner of target.
                    topLeftPoint = new CustomPopupPlacement(
                        new Point(targetSize.Width - popupSize.Width, targetSize.Height), PopupPrimaryAxis.Vertical);
                }
            }
            else
            {
                _MakeBottomPointerVisible();

                if (_IsLeftPointer())
                {
                    BottomPointer.Points = _GetBottomLeftPoints();
                    // Callout will point to upper left corner of target.
                    topLeftPoint = new CustomPopupPlacement(
                        new Point(0, -popupSize.Height), PopupPrimaryAxis.Horizontal);
                }
                else
                {
                    BottomPointer.Points = _GetBottomRightPoints(grid.ActualWidth);
                    // Callout will point to upper right corner of target.
                    topLeftPoint = new CustomPopupPlacement(
                        new Point(targetSize.Width - popupSize.Width, -popupSize.Height), PopupPrimaryAxis.Horizontal);
                }
            }
            CustomPopupPlacement[] points = new CustomPopupPlacement[] { topLeftPoint };

            return points;
        }

        /// <summary>
        /// Check that pointer must occure in left part of callout.
        /// </summary>
        /// <returns>'True' if pointer must be in left part of callout, 'false' otherwise.</returns>
        private bool _IsLeftPointer()
        {
            var calloutLeft = PlacementRectangle.Left;
            var screen = _GetCurrentScreen();

            // If there is enough space under placement rectangle this is callout with "left" pointer.
            if (calloutLeft + _ConvertWidth(stackPanel.ActualWidth) < screen.Bounds.Right)
                return true;
            // Otherwise, this is callout with "right" pointer.
            else
                return false;
        }

        /// <summary>
        /// Check that pointer must occure in upper part of callout.
        /// </summary>
        /// <returns>'True' if pointer must be in upper part of callout, 'false' otherwise.</returns>
        private bool _IsUpperPointer()
        {
            var calloutTop = PlacementRectangle.Bottom;
            var calloutHeight = stackPanel.ActualHeight + _pointerHeight + _pointerMargin;
            var screen = _GetCurrentScreen();

            // If there is enough space under placement rectangle this is callout with "upper" pointer.
            if (calloutTop + _ConvertWidth(calloutHeight) < screen.Bounds.Bottom)
                return true;
            // Otherwise, this is callout with "bottom" pointer.
            else
                return false;
        }

        /// <summary>
        /// Make visible bottom pointer and hide top.
        /// </summary>
        private void _MakeBottomPointerVisible()
        {
            TopStackPanel.Visibility = Visibility.Collapsed;
            BottomPointer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Make visible top pointer and hide bottom.
        /// </summary>
        private void _MakeTopPointerVisible()
        {
            TopStackPanel.Visibility = Visibility.Visible;
            BottomPointer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Convert width to device dependent units.
        /// </summary>
        /// <param name="width">Width in device independent units.</param>
        /// <returns>Converted width.</returns>
        private double _ConvertWidth(double width)
        {
            // Convert from device-independent units 
            var presentationSource = PresentationSource.FromVisual
                (System.Windows.Application.Current.MainWindow);

            Matrix matrix = Matrix.Identity;
            if (presentationSource != null)
                matrix = presentationSource.CompositionTarget.TransformToDevice;

            return width * matrix.M11;
        }

        /// <summary>
        /// Convert height to device dependent units.
        /// </summary>
        /// <param name="height">Height in device independent units.</param>
        /// <returns>Converted height.</returns>
        private double _ConvertHeight(double height)
        {
            // Convert from device-independent units 
            var presentationSource = PresentationSource.FromVisual
                (System.Windows.Application.Current.MainWindow);

            Matrix matrix = Matrix.Identity;
            if (presentationSource != null)
                matrix = presentationSource.CompositionTarget.TransformToDevice;

            return height * matrix.M22;
        }

        /// <summary>
        /// Get screen where current callout is hosted.
        /// </summary>
        /// <returns>Screen with current window.</returns>
        private Screen _GetCurrentScreen()
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(App.Current.MainWindow);
            return System.Windows.Forms.Screen.FromHandle(windowInteropHelper.Handle);
        }

        /// <summary>
        /// Get points for top-left pointer.
        /// </summary>
        /// <returns>PointCollection.</returns>
        private PointCollection _GetTopLeftPoints()
        {
            var result = new PointCollection();

            result.Add(new Point(_pointerHorizontalOffset, _pointerHeight));
            result.Add(new Point(_pointerHorizontalOffset, 0));
            result.Add(new Point(_pointerHorizontalOffset + _pointerWidth, _pointerHeight));

            return result;
        }

        /// <summary>
        /// Get points for top-right pointer.
        /// </summary>
        /// <returns>PointCollection.</returns>
        private PointCollection _GetTopRightPoints(double calloutWidth)
        {
            var result = new PointCollection();

            result.Add(new Point(calloutWidth - _pointerWidth - _pointerHorizontalOffset, _pointerHeight));
            result.Add(new Point(calloutWidth - _pointerHorizontalOffset, 0));
            result.Add(new Point(calloutWidth - _pointerHorizontalOffset, _pointerHeight));

            return result;
        }

        /// <summary>
        /// Get points for bottom-left pointer.
        /// </summary>
        /// <returns>PointCollection.</returns>
        private PointCollection _GetBottomLeftPoints()
        {
            var result = new PointCollection();

            result.Add(new Point(_pointerHorizontalOffset, 0));
            result.Add(new Point(_pointerHorizontalOffset, _pointerHeight));
            result.Add(new Point(_pointerHorizontalOffset + _pointerWidth, 0));

            return result;
        }

        /// <summary>
        /// Get points for bottom-right pointer.
        /// </summary>
        /// <returns>PointCollection.</returns>
        private PointCollection _GetBottomRightPoints(double calloutWidth)
        {
            var result = new PointCollection();

            result.Add(new Point(calloutWidth - _pointerWidth - _pointerHorizontalOffset, 0));
            result.Add(new Point(calloutWidth - _pointerHorizontalOffset, _pointerHeight));
            result.Add(new Point(calloutWidth - _pointerHorizontalOffset, 0));

            return result;
        }

        /// <summary>
        /// When mouse move inside callout, we need to hide it.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">MouseEventArgs.</param>
        private void _MainWindowMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Get mouse position.
            Point point = e.GetPosition(grid);

            // If mouse is inside callout - hide it.
            if (point.X > 0 && point.X < grid.ActualWidth && point.Y > _pointerHeight && point.Y < grid.ActualHeight)
            {
                if (stackPanel.Opacity == 1)
                    _StartStoryBoardAtGrid("HideStoryboard", HandoffBehavior.SnapshotAndReplace);
            }
            // If mouse moved out of popup - show popup.
            else if (stackPanel.Opacity == 0)
                _StartStoryBoardAtGrid("ShowStoryboard", HandoffBehavior.SnapshotAndReplace);
        }

        /// <summary>
        /// Start animation.
        /// </summary>
        /// <param name="storyBoardName">Name of the story board.</param>
        /// <param name="handoffBehavior">HandoffBehavior.</param>
        private void _StartStoryBoardAtGrid(string storyBoardName, HandoffBehavior handoffBehavior)
        {
            // Find storyboard.
            Storyboard s = (Storyboard)this.FindResource(storyBoardName);

            // Start animation.
            stackPanel.BeginStoryboard(s, handoffBehavior);
        }

        #endregion

        #region private members

        /// <summary>
        /// Pointer margin.
        /// </summary>
        private double _pointerMargin;

        /// <summary>
        /// Pointer sizes.
        /// </summary>
        private double _pointerHorizontalOffset = 18;
        private double _pointerWidth = 35;
        private double _pointerHeight = 15;

        #endregion
    }
}
