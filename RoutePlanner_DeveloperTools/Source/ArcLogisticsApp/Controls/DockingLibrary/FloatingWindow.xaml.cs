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
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for FloatingWindow.xaml
    /// </summary>
    internal partial class FloatingWindow : System.Windows.Window
    {
        #region Constructors
        /// <summary>
        /// Create floating window
        /// </summary>
        /// <param name="pane">Relative pane</param>
        public FloatingWindow(DockablePane pane)
        {
            InitializeComponent();
            _isTooltipEnabled = true;

            _paneHosted = pane;
            Content = _paneHosted;
            Title = _paneHosted.PaneContent.Title;

            // add code to update window header.
            _dpDescriptor = DependencyPropertyDescriptor.FromProperty(Window.TitleProperty, typeof(Window));
            if (_dpDescriptor != null)
                _dpDescriptor.AddValueChanged(_paneHosted.PaneContent, delegate
                {
                    if (Content != null)
                        Title = ((DockablePane)Content).PaneContent.Title;
                });

            _paneHosted.PaneContent.DockManager.RegisterFloatingWnd(this);
        }

        #endregion // Constructors

        #region Specials routine
        // NOTE: if window close - real hided

        /// <summary>
        /// Is window closed.
        /// </summary>
        public bool IsClosed
        {
            get { return _isClosed; }
        }

        /// <summary>
        /// Close window special routine.
        /// </summary>
        /// <remarks>Real window hided.</remarks>
        public void ForceClose()
        {
            _isClosed = true;
            base.Hide();
        }

        /// <summary>
        /// Show window on the display.
        /// </summary>
        public new void Show()
        {
            _isClosed = false;
            base.Show();
        }

        #endregion // Specials routine

        /// <summary>
        /// Closes window.
        /// </summary>
        public new void Close()
        {
            if (_ignoreClose)
                return;

            _ignoreClose = true;

            base.Close();

            if (null != _paneHosted)
            {
                DockableContent content = _paneHosted.PaneContent;
                content.ContainerPane.UseSpecAllocation = true;
                content.ContainerPane.Show();
            }
        }

        #region Events routine

        /// <summary>
        /// Handles window closing.
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            // Remove code to update window title.
            if (_dpDescriptor != null)
            {
                _dpDescriptor.RemoveValueChanged(_paneHosted.PaneContent, delegate
                {
                    if (Content != null)
                        Title = ((DockablePane)Content).PaneContent.Title;
                });
            }

            _paneHosted.StoreFloatingWindowDimensions(this);
            _paneHosted.PaneContent.DockManager.UnregisterFloatingWnd(this);
            _paneHosted.FloatingWindow = null;

            _RemoveTooltip();

            Content = null;
            _paneHosted = null;

            if (_hwndSource != null)
                _hwndSource.RemoveHook(_wndProcHandler);
            _hwndSource = null;

            base.OnClosing(e);
        }

        /// <summary>
        /// Handles window loading.
        /// </summary>
        private void OnLoaded(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _wndProcHandler = new HwndSourceHook(HookHandler);
            _hwndSource.AddHook(_wndProcHandler);
        }

        /// <summary>
        /// Handles tooltip timer tick.
        /// </summary>
        private void _TooltipTimerTick(object sender, EventArgs e)
        {
            if (!this.IsFocused)
            {
                _RemoveTooltip();
                _isTooltipEnabled = false;
            }
        }

        /// <summary>
        /// Handles hook.
        /// </summary>
        /// <param name="hwnd">Ignored.</param>
        /// <param name="msg">Message code.</param>
        /// <param name="wParam">Messages param.</param>
        /// <param name="lParam">Messages param.</param>
        /// <param name="handled">Handled flag.</param>
        private IntPtr HookHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            switch (msg)
            {
                case WM_CLOSE:
                    {
                        if (!_ignoreClose)
                        {
                            _RemoveTooltip();
                            ForceClose();
                            _paneHosted.PaneContent.FireVisibileStateChanged();
                            // NOTE: if need real close after "X" click - uncomment next lines and remove previous
                            //_paneHosted.Close();
                            handled = true;
                        }
                    }
                    break;

                case WM_NCLBUTTONDOWN:
                    {
                        _RemoveTooltip();
                        var code = wParam.ToInt32();
                        if ((_paneHosted.State == PaneState.DockableWindow) &&
                            (HTCLOSE != code) && (HTCAPTION == code))
                        {
                            var coords = lParam.ToInt32();
                            var x = (short)(coords & 0xFFFF);
                            var y = (short)(coords >> 16);

                            var point = _paneHosted.PaneContent.DockManager.GetScreenPointInDefaultDpi(new Point(x, y));
                            _paneHosted.PaneContent.DockManager.Drag(this, point, new Point(point.X - Left, point.Y - Top));
                            handled = true;
                        }
                    }
                    break;

                case WM_NCMOUSEMOVE:
                    {
                        if (_isTooltipEnabled)
                        {
                            if (null == _tooltip)
                                _CreateTooltip();

                            if (!_tooltip.IsOpen)
                            {
                                // init tooltip state
                                var coords = lParam.ToInt32();
                                var x = (short)(coords & 0xFFFF);
                                var y = (short)(coords >> 16);
                                var screenPoint = new Point(x, y);
                                var point = _paneHosted.PaneContent.DockManager.GetScreenPointInDefaultDpi(screenPoint);
                                var posY = point.Y - Top;
                                if ((0 < posY) && (posY < SystemParameters.WindowCaptionHeight) &&
                                    (Left + SystemParameters.FixedFrameVerticalBorderWidth < point.X) &&
                                    (point.X < Left + Width - SystemParameters.FixedFrameVerticalBorderWidth))
                                {
                                    _tooltip.PlacementRectangle = new Rect(screenPoint, new Size(0, 0));
                                    _tooltip.IsOpen = true;
                                    _tooltip.Visibility = Visibility.Visible;
                                    handled = true;
                                }

                                // start timer to support delay between tool tips showing
                                _tooltipTimer.Interval = TimeSpan.FromMilliseconds(TOOLTIP_DELAY);
                                _tooltipTimer.Tick += new EventHandler(_TooltipTimerTick);
                                _tooltipTimer.Start();
                            }
                        }
                    }
                    break;

                case WM_NCMOUSELEAVE:
                case WM_MOUSEMOVE:
                case WM_MOUSEHOVER:
                case WM_MOUSELEAVE:
                    _isTooltipEnabled = true;
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion // Events routine

        /// <summary>
        /// Creates tooltip window.
        /// </summary>
        private void _CreateTooltip()
        {
            Debug.Assert(null == _tooltip);

            _tooltip = new Popup();
            _tooltip.AllowsTransparency = true;
            ContentControl control = new ContentControl();
            control.Content = TOOLTIP_TEXT;
            control.Style = TOOLTIP_STYLE;
            _tooltip.Child = control;
            _tooltip.Placement = PlacementMode.AbsolutePoint;
        }

        /// <summary>
        /// Hides tooltip window.
        /// </summary>
        private void _HideTooltip()
        {
            if (null != _tooltip)
            {
                _tooltipTimer.Stop();

                _tooltip.Visibility = Visibility.Hidden;
                _tooltip.IsOpen = false;
            }
        }

        /// <summary>
        /// Removes tooltip window.
        /// </summary>
        private void _RemoveTooltip()
        {
            if (null != _tooltip)
            {
                _HideTooltip();
                _tooltip = null;
            }
        }

        #region Private constants

        private const int WM_NCMOUSEMOVE = 0x00A0;
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_NCMOUSEHOVER = 0x02A0;
        private const int WM_MOUSEHOVER = 0x02A1;
        private const int WM_NCMOUSELEAVE = 0x02A2;
        private const int WM_MOUSELEAVE = 0x02A3;

        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_CLOSE = 0x0010;

        private const int HTCAPTION = 2;
        private const int HTCLOSE = 20;

        private const int TOOLTIP_DELAY = 1000;
        private readonly string TOOLTIP_TEXT = App.Current.FindString("UndockedViewTooltip");
        private readonly Style TOOLTIP_STYLE = (Style)App.Current.FindResource("MapPopupStyle");

        #endregion // Private constants

        #region Private fields

        /// <summary>
        /// Get floating window hosted pane.
        /// </summary>
        internal DockablePane PaneHosted
        {
            get
            {
                Debug.Assert(null != _paneHosted);
                return _paneHosted;
            }
        }

        /// <summary>
        /// Floating window hosted pane.
        /// </summary>
        private DockablePane _paneHosted;

        /// <summary>
        /// Hwnd source.
        /// </summary>
        private HwndSource _hwndSource;

        /// <summary>
        /// Hook of hwnd source.
        /// </summary>
        private HwndSourceHook _wndProcHandler;

        /// <summary>
        /// Ignore close flag.
        /// </summary>
        private bool _ignoreClose;

        /// <summary>
        /// Is window closed.
        /// </summary>
        private bool _isClosed;

        /// <summary>
        /// Used for update window title.
        /// </summary>
        private DependencyPropertyDescriptor _dpDescriptor;

        /// <summary>
        /// Tooltip emulation window.
        /// </summary>
        private Popup _tooltip;

        /// <summary>
        /// Timer to correctly show tool tip.
        /// </summary>
        private DispatcherTimer _tooltipTimer = new DispatcherTimer();

        /// <summary>
        /// Is tooltip enabled flag.
        /// </summary>
        private bool _isTooltipEnabled;

        #endregion // Private fields
    }
}
