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
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Interop;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Manages and controls panes layout.
    /// </summary>
    /// <remarks>This is the main user control which is usually embedded in a window.
    /// DockManager can control other windows arraging them in panes like VS.
    /// Each pane can be docked to a DockManager border, can be shown/hidden or auto-hidden.</remarks>
    internal partial class DockManager : UserControl, IDropSurface
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create dock manager.
        /// </summary>
        public DockManager()
        {
            InitializeComponent();

            DragPaneServices.Register(this);

            _overlayWindow = new OverlayWindow(this);
        }

        #endregion // Constructors

        #region Public functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add dockable content to layout management.
        /// </summary>
        /// <param name="content">Content to manage.</param>
        public void Add(DockableContent content)
        {
            if (!_contents.Contains(content))
                _contents.Add(content);
        }

        /// <summary>
        /// Add a dockapble to layout management.
        /// </summary>
        /// <param name="pane">Pane to manage.</param>
        public void Add(DockablePane pane)
        {
            gridDocking.Add(pane);
        }

        /// <summary>
        /// Remove a dockable pane from layout management.
        /// </summary>
        /// <param name="pane">Pane to remove.</param>
        public void Remove(DockablePane pane)
        {
            if (pane.IsDragSupported)
                DragPaneServices.Unregister(pane);

            gridDocking.Remove(pane);
        }

        /// <summary>
        /// Remove a dockable content from internal contents list.
        /// </summary>
        /// <param name="content">Content to remove.</param>
        public void Remove(DockableContent content)
        {
            _contents.Remove(content);
        }

        /// <summary>
        /// Handle dockable pane layout changing.
        /// </summary>
        /// <param name="sourcePane">Source pane to move.</param>
        /// <param name="destinationPane">Relative pane.</param>
        /// <param name="relativeDock">Dock type.</param>
        public void MoveTo(DockablePane sourcePane, DockablePane destinationPane, Dock relativeDock)
        {
            gridDocking.Add(sourcePane, destinationPane, relativeDock);
        }

        /// <summary>
        /// Release object content.
        /// </summary>
        /// <remarks>Call this method before closing application.</remarks>
        public void Release()
        {
            Debug.Assert(null != _overlayWindow);
            _overlayWindow.Close();
        }

        /// <summary>
        /// Gets screen point in default DPI.
        /// </summary>
        /// <param name="screenPoint">Point is screen coordinates.</param>
        /// <returns>Point in default DPI.</returns>
        public Point GetScreenPointInDefaultDpi(Point screenPoint)
        {
            return new Point(screenPoint.X / _desktopDpiFactorX, screenPoint.Y / _desktopDpiFactorY);
        }

        /// <summary>
        /// Converts point in control to screen point in default DPI.
        /// </summary>
        /// <param name="control">Parent control.</param>
        /// <param name="point">Current mouse position.</param>
        /// <returns>Point in default DPI.</returns>
        public Point ConvertRelativePointToScreenInDefaultDpi(Visual control, Point point)
        {
            Point screenPoint = control.PointToScreen(point);
            return GetScreenPointInDefaultDpi(screenPoint);
        }
        #endregion // Public functions

        #region Events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Load process show all floating windows.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _InitDesktopDpiFactors();

            foreach (FloatingWindow wnd in _floatingWnds)
            {
                if (!wnd.IsClosed)
                    wnd.Show();
            }
        }

        /// <summary>
        /// During unolad process hide all floating windows.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void OnUnloaded(object sender, EventArgs e)
        {
            foreach (FloatingWindow wnd in _floatingWnds)
                wnd.Hide();
        }

        #endregion // Events

        /// <summary>
        /// Default desktop DPI.
        /// </summary>
        private const int DEFAULT_DESCTOP_DPI = 96;

        /// <summary>
        /// Init desktop DPI factors for conversion.
        /// </summary>
        private void _InitDesktopDpiFactors()
        {
            // 1. Variant.
            // The code gets the horizontal (M11), vertical (M22) DPI.
            // The actual value are divided by 96 [going back to WPF's device independent logical
            // units in wpf being 1/96 of an inch],
            // so for example, on system at 144 DPI - get 1.5
            // System.Windows.Media.Matrix m =
            //      PresentationSource.FromVisual(App.Current.MainWindow).CompositionTarget.TransformToDevice;
            // _desktopDpiFactorX = m.M11;
            // _desktopDpiFactorY = m.M22;

            // 2. Variant (MSDN).
            // Obtain the window handle for WPF application
            IntPtr mainWindowPtr = new WindowInteropHelper(App.Current.MainWindow).Handle;
            // Get System Dpi
            System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
            _desktopDpiFactorX = desktop.DpiX / DEFAULT_DESCTOP_DPI;
            _desktopDpiFactorY = desktop.DpiY / DEFAULT_DESCTOP_DPI;
        }

        #region DragDrop operations
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Parent window hosting DockManager user control.
        /// </summary>
        private Window _wndParent;

        /// <summary>
        /// Parent window hosting DockManager user control accessors.
        /// </summary>
        public Window ParentWindow
        {
            get { return _wndParent; }
            set { _wndParent = value; }
        }

        /// <summary>
        /// Begins dragging operations.
        /// </summary>
        /// <param name="floatingWindow">Floating window containing pane which is dragged by user.</param>
        /// <param name="point">Current mouse position.</param>
        /// <param name="offset">Offset to be use to set floating window screen position.</param>
        /// <returns>Retruns True is drag is completed, false otherwise.</returns>
        public bool Drag(FloatingWindow floatingWindow, Point point, Point offset)
        {
            if (IsMouseCaptured)
                return false;

            if (!CaptureMouse())
                return false;

            DragPaneServices.StartDrag(floatingWindow, point, offset);
            return true;
        }

        /// <summary>
        /// Handles mousemove event.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Mouse event arguments.</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Point point = ConvertRelativePointToScreenInDefaultDpi(this, e.GetPosition(this));
                DragPaneServices.MoveDrag(point);
            }
        }

        /// <summary>
        /// Handles mouseUp event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Mouse event arguments.</param>
        /// <remarks>Releases eventually camptured mouse events.</remarks>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Point point = ConvertRelativePointToScreenInDefaultDpi(this, e.GetPosition(this));
                DragPaneServices.EndDrag(point);
                ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Drag pane service.
        /// </summary>
        private DragPaneServices _dragPaneServices;

        /// <summary>
        /// Get drag pane services.
        /// </summary>
        public DragPaneServices DragPaneServices
        {
            get
            {
                if (null == _dragPaneServices)
                    _dragPaneServices = new DragPaneServices();

                return _dragPaneServices;
            }
        }

        /// <summary>
        /// Registry floating windows to manage.
        /// </summary>
        /// <param name="wnd">Floating windows to manage.</param>
        public void RegisterFloatingWnd(FloatingWindow wnd)
        {
            Debug.Assert(!_floatingWnds.Contains(wnd));

            _floatingWnds.Add(wnd);
        }

        /// <summary>
        /// Unregistry floating windows to manage
        /// </summary>
        /// <param name="wnd">Floating windows to manage.</param>
        /// <remarks>Call this routine if system destroy\close window.</remarks>
        public void UnregisterFloatingWnd(FloatingWindow wnd)
        {
            Debug.Assert(_floatingWnds.Contains(wnd));

            _floatingWnds.Remove(wnd);
        }

        #endregion // DragDrop operations

        #region IDropSurface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get surface rectangle.
        /// </summary>
        /// <returns>Returns a rectangle where this surface is active.</returns>
        public Rect SurfaceRectangle
        {
            get
            {
                Point pt = ConvertRelativePointToScreenInDefaultDpi(this, new Point(0, 0));
                return new Rect(pt, new Size(ActualWidth, ActualHeight));
            }
        }

        /// <summary>
        /// Overlay window which shows docking placeholders
        /// </summary>
        private OverlayWindow _overlayWindow;

        /// <summary>
        /// Get current overlay window.
        /// </summary>
        public OverlayWindow OverlayWindow
        {
            get { return _overlayWindow; }
        }

        /// <summary>
        /// Handles this sourface mouse entering.
        /// </summary>
        /// <param name="point">Current mouse position.</param>
        /// <remarks>Show current overlay window.</remarks>
        public void OnDragEnter(Point point)
        {
            Point pt = ConvertRelativePointToScreenInDefaultDpi(this, new Point(0, 0));
            OverlayWindow.Left = pt.X;
            OverlayWindow.Top = pt.Y;
            OverlayWindow.Width = ActualWidth;
            OverlayWindow.Height = ActualHeight;
            OverlayWindow.Show();
        }

        /// <summary>
        /// Handles mouse overing this surface.
        /// </summary>
        /// <param name="point">Current mouse position.</param>
        public void OnDragOver(Point point)
        {
        }

        /// <summary>
        /// Handles mouse leave event during drag.
        /// </summary>
        /// <param name="point">Current mouse position.</param>
        /// <remarks>Hide overlay window.</remarks>
        public void OnDragLeave(Point point)
        {
            _overlayWindow.Owner = null;
            _overlayWindow.Hide();
            ParentWindow.Activate();
        }

        /// <summary>
        /// Handler drop events.
        /// </summary>
        /// <param name="point">Current mouse position.</param>
        /// <returns>Returns alwasy FALSE because this surface doesn't support direct drop.</returns>
        public bool OnDrop(Point point)
        {
            return false;
        }

        #endregion // IDropSurface

        #region ILayoutSerializable
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Serialize layout state of panes and contents into a xml string.
        /// </summary>
        /// <returns>Xml containing layout state as string.</returns>
        public string GetLayoutAsXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement(ELEMENT_NAME_DOCKLIB));

            XmlNode nodeRoot = doc.CreateElement(ELEMENT_NAME_ROOT);
            gridDocking.Serialize(doc, nodeRoot);
            doc.DocumentElement.AppendChild(nodeRoot);

            // serialize floating windows
            XmlNode nodeFloatWnds = doc.CreateElement(ELEMENT_NAME_FLTWNDS);
            foreach (FloatingWindow wnd in _floatingWnds)
            {
                if (!wnd.IsClosed)
                {
                    XmlNode nodeAttachedPane = doc.CreateElement(ELEMENT_NAME_DOCKPANE);
                    wnd.PaneHosted.Serialize(doc, nodeAttachedPane);
                    nodeFloatWnds.AppendChild(nodeAttachedPane);
                }
            }
            doc.DocumentElement.AppendChild(nodeFloatWnds);

            return doc.OuterXml;
        }

        /// <summary>
        /// Restore docking layout reading a xml string which is previously generated by a call
        /// to GetLayoutState.
        /// </summary>
        /// <param name="xml">Xml containing layout state.</param>
        /// <param name="getContentHandler">Delegate used by serializer to get user defined
        /// dockable contents.</param>
        public void RestoreLayoutFromXml(string xml, GetContentFromTypeString getContentHandler)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            _ResetInternalState();

            foreach (XmlNode nodeChild in doc.ChildNodes[0])
            {
                if (ELEMENT_NAME_ROOT == nodeChild.Name)
                    gridDocking.Deserialize(this, nodeChild, getContentHandler);
                else if (ELEMENT_NAME_FLTWNDS == nodeChild.Name)
                {
                    foreach (XmlNode node in nodeChild.ChildNodes)
                    {
                        var pane = new DockablePane();
                        pane.Deserialize(this, node, getContentHandler);
                    }
                }
            }
        }

        /// <summary>
        /// Serialize\Deserialize const.
        /// </summary>
        private const string ELEMENT_NAME_ROOT = "_root";
        private const string ELEMENT_NAME_DOCKLIB = "DockingLibrary_Layout";
        private const string ELEMENT_NAME_FLTWNDS = "FloatingWindows";
        private const string ELEMENT_NAME_DOCKPANE = "DockablePane";

        #endregion // ILayoutSerializable

        /// <summary>
        /// Resets docking panels state.
        /// </summary>
        private void _ResetInternalState()
        {
            // remove previously floating window
            if (0 < _floatingWnds.Count)
            {
                var winds = new Collection<FloatingWindow>();
                foreach (FloatingWindow wnd in _floatingWnds)
                    winds.Add(wnd);
                foreach (FloatingWindow wnd in winds)
                    wnd.Close();
            }

            // remove visible panels
            for (int index = 0; index < _contents.Count; ++index)
            {
                Remove(_contents[index].ContainerPane);
                _contents[index].ContainerPane = null;
            }
        }

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// List of managed contents (hiddens too).
        /// </summary>
        private readonly Collection<DockableContent> _contents = new Collection<DockableContent>();

        /// <summary>
        /// List of managed floating windows.
        /// </summary>
        private Collection<FloatingWindow> _floatingWnds = new Collection<FloatingWindow>();

        /// <summary>
        /// Desktop DPI factor to X.
        /// </summary>
        private float _desktopDpiFactorX = 1;
        /// <summary>
        /// Desktop DPI factor to Y.
        /// </summary>
        private float _desktopDpiFactorY = 1;

        #endregion // Private members
    }
}
