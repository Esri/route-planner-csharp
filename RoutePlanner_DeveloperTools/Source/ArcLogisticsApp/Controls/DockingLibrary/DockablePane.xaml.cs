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
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// States that a dockable pane can assume
    /// </summary>
    internal enum PaneState
    {
        Hidden,
        Docked,
        FloatingWindow,
        DockableWindow
    }

    /// <summary>
    /// Interaction logic for DockablePane.xaml
    /// </summary>
    /// <remarks>A dockable pane is a resizable and movable window region which can host one or more dockable content.
    /// A dockable pane occupies a window region. It can be in two different states: docked to a border or hosted in a floating window.
    /// When is docked it can be resizes only in a direction. User can switch between pane states using mouse or context menus.</remarks>
    internal partial class DockablePane : UserControl, IDropSurface, ILayoutSerializable, INotifyPropertyChanged
    {
        /// <summary>
        /// Minimal pane size
        /// </summary>
        public const int MIN_PANE_SIZE = 50;

        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create pane with content
        /// </summary>
        public DockablePane()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create pane with content
        /// </summary>
        /// <param name="content">Content</param>
        public DockablePane(DockableContent content)
        {
            InitializeComponent();

            Debug.Assert(null != content);

            _SetContent(content);
        }
        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pane dimensions accessors
        /// </summary>
        public Size Dimensions
        {
            get { return _size; }
            set { _size = value; }
        }

        /// <summary>
        /// Get current content
        /// </summary>
        public DockableContent PaneContent
        {
            get { return _content; }
        }

        /// <summary>
        /// Current pane state
        /// </summary>
        public PaneState State
        {
            get
            {
                return _state;
            }
            set
            {
                _state = value;

                if (null != PropertyChanged)
                    PropertyChanged(this, new PropertyChangedEventArgs(PROP_NAME_State));
            }
        }

        /// <summary>
        /// Dock state accessors
        /// </summary>
        public Dock DockType
        {
            get { return _dockType; }
        }

        /// <summary>
        /// Floating window
        /// </summary>
        public FloatingWindow FloatingWindow
        {
            set
            {
                Debug.Assert(null == value); // NOTE: use this only for clearing
                _wndFloating = value;
            }

            get { return _wndFloating; }
        }

        /// <summary>
        /// Pane set as hidden
        /// </summary>
        /// <returns>Return true if pane is hidden</returns>
        /// <remarks>Return true if pane is hidden, ie State is different from PaneState.Docked</remarks>
        public bool IsHidden
        {
            get { return (_state != PaneState.Docked); }
        }

        /// <summary>
        /// Pane add with special allocation
        /// </summary>
        public bool UseSpecAllocation
        {
            set { _useSpecAllocation = value; }
            get { return _useSpecAllocation; }
        }

        #endregion Public properties

        #region Override functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show this pane and content
        /// </summary>
        public virtual void Show(Dock dockType)
        {
            _dockType = dockType;
            Show();
        }

        /// <summary>
        /// Show this pane and content
        /// </summary>
        public virtual void Show()
        {
            tbTitle.Text = _content.Title;
            cpClientWindowContent.Content = _content.Content;
            cpClientWindowContent.Visibility = Visibility.Visible;
            ShowHeader = true;

            if (PaneState.Docked != State)
                ChangeState(PaneState.Docked);
        }

        /// <summary>
        /// Hide this pane and all contained contents
        /// </summary>
        public virtual void Hide()
        {
            _useSpecAllocation = true;
            cpClientWindowContent.Visibility = Visibility.Collapsed;
            cpClientWindowContent.Content = null;

            ChangeState(PaneState.Hidden);
        }

        /// <summary>
        /// Close this pane
        /// </summary>
        /// <remarks>Consider that in this version library this method simply hides the pane.</remarks>
        public virtual void Close()
        {
            Hide();
        }

        /// <summary>
        /// Store current pane size
        /// </summary>
        public void StoreSize()
        {
            Size sz = Dimensions;
            if ((_dockType == Dock.Left) || (_dockType == Dock.Right))
                sz.Width = (_content.DockManager.ActualWidth <= sz.Width) ? MIN_PANE_SIZE : Math.Max(sz.Width, MIN_PANE_SIZE);
            else // ((DockType == Dock.Top) || (DockType == Dock.Bottom))
                sz.Height = (_content.DockManager.ActualHeight <= sz.Height) ? MIN_PANE_SIZE : Math.Max(sz.Height, MIN_PANE_SIZE);
            Dimensions = sz;
        }

        /// <summary>
        /// Handles effective pane resizing
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            Dimensions = sizeInfo.NewSize;
            StoreSize();

            base.OnRenderSizeChanged(sizeInfo);
        }
        #endregion // Overrided functions

        #region Public functions

        /// <summary>
        /// Dock this pane to a destination pane border
        /// </summary>
        /// <param name="destinationPane"></param>
        /// <param name="relativeDock">Relative dock inside destination pane</param>
        public void MoveTo(DockablePane destinationPane, Dock relativeDock)
        {
            _useSpecAllocation = false;
            _CloseFloatingWindow();

            DockablePane dockableDestPane = destinationPane as DockablePane;

            StoreSize();
            State = PaneState.Docked;

            _dockType = relativeDock;
            ShowHeader = true;

            _content.DockManager.MoveTo(this, dockableDestPane, relativeDock);
        }

        /// <summary>
        /// Change dock border
        /// </summary>
        /// <param name="dock">New dock border</param>
        public void ChangeDock(Dock dock)
        {
            _useSpecAllocation = false;
            _CloseFloatingWindow();

            _dockType = dock;
            ShowHeader = true;

            ChangeState(PaneState.Docked);
        }

        /// <summary>
        /// Change pane internal state
        /// </summary>
        /// <param name="newState">New pane state</param>
        /// <remarks>OnStateChanged event is raised only if newState is different from State.</remarks>
        public void ChangeState(PaneState newState)
        {
            if (State != newState)
            {
                StoreSize();

                State = newState;

                if (PaneState.Docked == newState)
                    _content.DockManager.Add(this);
                else
                    _content.DockManager.Remove(this);
            }
        }
        #endregion // Public functions

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises when property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Protected functions
        /// <summary>
        /// Show/hide pane header (title, buttons etc...)
        /// </summary>
        protected bool ShowHeader
        {
            get { return brHeader.Visibility == Visibility.Visible; }
            set { brHeader.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed; }
        }
        #endregion // Protected functions

        #region Events
        /// <summary>
        /// Event raised when Dock property is changed
        /// </summary>
        public event EventHandler OnDockChanged;

        /// <summary>
        /// Fires OnDockChanged
        /// </summary>
        private void _FireOnDockChanged()
        {
            if (OnDockChanged != null)
                OnDockChanged(this, EventArgs.Empty);
        }
        #endregion // Events

        #region Floating window state
        /// <summary>
        /// Position of floating window (LeftTop corner)
        /// </summary>
        private Point _ptFloatingWindow = new Point(0,0);

        /// <summary>
        /// Size of floating window
        /// </summary>
        private Size _szFloatingWindow = new Size(300, 300);

        /// <summary>
        /// Store size and point of floating window
        /// </summary>
        /// <param name="fw">Relative floating window</param>
        public void StoreFloatingWindowDimensions(Window fw)
        {
            if (!double.IsNaN(fw.Left) && !double.IsNaN(fw.Top))
                _ptFloatingWindow = new Point(fw.Left, fw.Top);

            if (!double.IsNaN(fw.Width) && !double.IsNaN(fw.Height))
                _szFloatingWindow = new Size(fw.Width, fw.Height);
        }

        #endregion // Floating window state

        #region Menu routine

        /// <summary>
        /// Handles user click event on close button
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Routed event arguments</param>
        private void OnBtnCloseMouseDown(object sender, RoutedEventArgs e)
        {
            _content.Close();
            e.Handled = true;
        }
 
        #endregion // Menu routine

        #region Draging routine
        /// <summary>
        /// Drag starting point
        /// </summary>
        private Point _ptStartDrag;

        /// <summary>
        /// Handles mouse douwn event on pane header
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Mouse event arguments</param>
        /// <remarks>Save current mouse position in ptStartDrag and capture mouse event on brHeader object.</remarks>
        private void OnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (_content.DockManager == null)
                return;

            if (!brHeader.IsMouseCaptured)
            {
                _ptStartDrag = e.GetPosition(this);
                brHeader.CaptureMouse();
            }
        }

        /// <summary>
        /// Handles mouse up event on pane header
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Ignored</param>
        /// <remarks>Release any mouse capture</remarks>
        private void OnHeaderMouseUp(object sender, MouseEventArgs e)
        {
            brHeader.ReleaseMouseCapture();
        }

        /// <summary>
        /// Handles mouse move event and eventually starts draging this pane
        /// </summary>
        /// <param name="sender">Ignored</param>
        /// <param name="e">Mouse event arguments</param>
        private void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (brHeader.IsMouseCaptured && 
                ((Math.Abs(_ptStartDrag.X - e.GetPosition(this).X) > 4) || (Math.Abs(_ptStartDrag.Y - e.GetPosition(this).Y) > 4)))
            {
                brHeader.ReleaseMouseCapture();
                Point point = _content.DockManager.ConvertRelativePointToScreenInDefaultDpi(this, e.GetPosition(this));
                DragPane(point, e.GetPosition(brHeader));
            }
        }

        /// <summary>
        /// Initiate a dragging operation of this pane, relative DockManager is also involved
        /// </summary>
        /// <param name="ptStartDrag">Current mouse position</param>
        /// <param name="offset">Offset to be use to set floating window screen position</param>
        protected virtual void DragPane(Point ptStartDrag, Point offset)
        {
            _CreateFloatingWindow(PaneState.DockableWindow);
            _content.DockManager.Drag(_wndFloating, ptStartDrag, offset); // NOTE: ignore result
        }

        /// <summary>
        /// Is pane drag supported
        /// </summary>
        public bool IsDragSupported
        {
            get { return (null == _wndFloating); }
        }

        private void _CloseFloatingWindow()
        {
            if (null != _wndFloating)
            {
                _wndFloating.Close();
                _wndFloating = null;
            }
        }
        #endregion // Draging routine

        #region IDropSurface
        /// <summary>
        /// Get surface rectangle
        /// </summary>
        /// <returns>Returns a rectangle where this surface is active</returns>
        public virtual Rect SurfaceRectangle
        {
            get
            {
                if (IsHidden)
                    return new Rect();

                Point pt = _content.DockManager.ConvertRelativePointToScreenInDefaultDpi(this, new Point(0, 0));
                return new Rect(pt, new Size(ActualWidth, ActualHeight));
            }
        }

        /// <summary>
        /// Handles this sourface mouse entering
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public virtual void OnDragEnter(Point point)
        {
            _content.DockManager.OverlayWindow.ShowOverlayPaneDockingOptions(this);
        }

        /// <summary>
        /// Handles mouse overing this surface
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public virtual void OnDragOver(Point point)
        {
        }

        /// <summary>
        /// Handles mouse leave event during drag
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public virtual void OnDragLeave(Point point)
        {
            _content.DockManager.OverlayWindow.HideOverlayPaneDockingOptions();
        }

        /// <summary>
        /// Handler drop events
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public virtual bool OnDrop(Point point)
        {
            return false;
        }
        #endregion // IDropSurface

        #region ILayoutSerializable
        /// <summary>
        /// Serialize layout
        /// </summary>
        /// <param name="doc">Document to save</param>
        /// <param name="nodeParent">Parent node</param>
        public void Serialize(XmlDocument doc, XmlNode nodeParent)
        {
            StoreSize();
            nodeParent.Attributes.Append(doc.CreateAttribute(ATTRIBUTE_NAME_SIZE));
            nodeParent.Attributes[ATTRIBUTE_NAME_SIZE].Value = TypeDescriptor.GetConverter(typeof(Size)).ConvertToInvariantString(_size);

            nodeParent.Attributes.Append(doc.CreateAttribute(ATTRIBUTE_NAME_DOCK));
            nodeParent.Attributes[ATTRIBUTE_NAME_DOCK].Value = _dockType.ToString();
            nodeParent.Attributes.Append(doc.CreateAttribute(ATTRIBUTE_NAME_STATE));
            nodeParent.Attributes[ATTRIBUTE_NAME_STATE].Value = _state.ToString();

            if (null != _wndFloating)
                StoreFloatingWindowDimensions(_wndFloating);
            nodeParent.Attributes.Append(doc.CreateAttribute(ATTRIBUTE_NAME_PTFLOATWND));
            nodeParent.Attributes[ATTRIBUTE_NAME_PTFLOATWND].Value =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point)).ConvertToInvariantString(_ptFloatingWindow);
            nodeParent.Attributes.Append(doc.CreateAttribute(ATTRIBUTE_NAME_SZFLOATWND));
            nodeParent.Attributes[ATTRIBUTE_NAME_SZFLOATWND].Value =
                System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size)).ConvertToInvariantString(_szFloatingWindow);

            XmlNode nodeDockableContent = doc.CreateElement(_content.GetType().ToString());
            nodeParent.AppendChild(nodeDockableContent);
        }

        /// <summary>
        /// Deserialize layout
        /// </summary>
        /// <param name="manager">Dock manager for initing objects</param>
        /// <param name="node">Node to parse</param>
        /// <param name="handlerObject">Delegate used to get user defined dockable contents</param>
        public void Deserialize(DockManager manager, XmlNode node, GetContentFromTypeString handlerObject)
        {
            _size = (Size)TypeDescriptor.GetConverter(typeof(Size)).ConvertFromInvariantString(node.Attributes[ATTRIBUTE_NAME_SIZE].Value);

            _dockType = (Dock)Enum.Parse(typeof(Dock), node.Attributes[ATTRIBUTE_NAME_DOCK].Value);

            PaneState state = (PaneState)Enum.Parse(typeof(PaneState), node.Attributes[ATTRIBUTE_NAME_STATE].Value);
            State = (PaneState.FloatingWindow == state) ? PaneState.DockableWindow : state;
                // NOTE: for support old versions

            _ptFloatingWindow = (Point)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point)).ConvertFromInvariantString(node.Attributes[ATTRIBUTE_NAME_PTFLOATWND].Value);
            _szFloatingWindow = (Size)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size)).ConvertFromInvariantString(node.Attributes[ATTRIBUTE_NAME_SZFLOATWND].Value);

            DockableContent content = handlerObject(node.ChildNodes[0].Name);

            // set state
            content.DockManager = manager;
            _SetContent(content);

            if ((PaneState.Docked == State) || (PaneState.Hidden == State))
                Show();
            else
            {
                tbTitle.Text = _content.Title;
                cpClientWindowContent.Content = _content.Content;
                cpClientWindowContent.Visibility = Visibility.Visible;

                _CreateFloatingWindow(_state);
                _InitFloatingWindowPosition();
            }
        }

        /// <summary>
        /// Serialize\Deserialize const
        /// </summary>
        private const string ATTRIBUTE_NAME_SIZE = "Size";
        private const string ATTRIBUTE_NAME_DOCK = "Dock";
        private const string ATTRIBUTE_NAME_STATE = "State";
        private const string ATTRIBUTE_NAME_PTFLOATWND = "ptFloatingWindow";
        private const string ATTRIBUTE_NAME_SZFLOATWND = "sizeFloatingWindow";

        #endregion // ILayoutSerializable

        /// <summary>
        /// Init floating window position
        /// </summary>
        private void _InitFloatingWindowPosition()
        {
            if ((0 == _ptFloatingWindow.X) && (_ptFloatingWindow.X == _ptFloatingWindow.Y))
                _wndFloating.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            else
            {
                _wndFloating.WindowStartupLocation = WindowStartupLocation.Manual;
                _wndFloating.Left = _ptFloatingWindow.X;
                _wndFloating.Top = _ptFloatingWindow.Y;
            }
        }

        /// <summary>
        /// Create a floating window hosting this pane
        /// </summary>
        private void _CreateFloatingWindow(PaneState state)
        {
            Debug.Assert((PaneState.DockableWindow == state));

            StoreSize();
            State = state;

            if (null == _wndFloating)
            {
                _content.DockManager.Remove(this);
                ShowHeader = false;

                _wndFloating = new FloatingWindow(this);

                _wndFloating.Owner = _content.DockManager.ParentWindow;
                _wndFloating.Width = _szFloatingWindow.Width;
                _wndFloating.Height = _szFloatingWindow.Height;
            }
        }

        /// <summary>
        /// Create and show a floating window hosting this pane
        /// </summary>
        private void _CreateAndShowFloatingWindow(PaneState state)
        {
            _CreateFloatingWindow(state);
            Debug.Assert(null != _wndFloating);

            _InitFloatingWindowPosition();

            _wndFloating.Show();
        }

        /// <summary>
        /// Set content
        /// </summary>
        /// <param name="content">New content</param>
        private void _SetContent(DockableContent content)
        {
            Debug.Assert(null != content);
            Debug.Assert(null == _content);

            if (null == _content)
                StoreFloatingWindowDimensions(content);

            content.DockManager.Add(content);
            content.ContainerPane = this;
            _content = content;
        }

        #region Private constants

        /// <summary>
        /// Pane state property name.
        /// </summary>
        public const string PROP_NAME_State = "State";

        #endregion

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Pane current size
        /// </summary>
        private Size _size = new Size(150, 150);

        /// <summary>
        /// Content member
        /// </summary>
        private DockableContent _content = null;

        /// <summary>
        /// Current pane state
        /// </summary>
        /// <remarks>When created pane is hidden</remarks>
        protected PaneState _state = PaneState.Hidden;

         /// <summary>
        /// Current docking border
        /// </summary>
        /// <remarks>When created pane is right docked</remarks>
        private Dock _dockType = Dock.Right;

        /// <summary>
        /// Floating window
        /// </summary>
        private FloatingWindow _wndFloating = null;

        /// <summary>
        /// Floating window
        /// </summary>
        private bool _useSpecAllocation = false;

        #endregion // Members
    }
}
