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
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Rappresents a content embeddable in a dockable pane or in a documents pane
    /// </summary>
    internal class DockableContent : Window
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create dockable content
        /// </summary>
        public DockableContent()
        {
        }

        #endregion // Constructors

        #region Event
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Event close\open
        /// </summary>
        public event EventHandler VisibileStateChanged;

        #endregion // Event

        #region Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Container pane accessors
        /// </summary>
        public DockablePane ContainerPane
        {
            get
            {
                if (null == _paneContainer && this.DockManager != null)
                    _paneContainer = new DockablePane(this);

                return _paneContainer;
            }

            set 
            {
                _paneContainer = value;
            }
        }

        /// <summary>
        /// Dock manager accessors
        /// </summary>
        public DockManager DockManager
        {
            set { _dockManager = value; }
            get
            {
                return _dockManager;
            }
        }

        /// <summary>
        /// Is content visible on display
        /// </summary>
        public new bool IsVisible
        {
            get
            {
                if (null == _paneContainer)
                    return false;

                FloatingWindow window = _GetRelatedFloatingWnd();
                if (null != window)
                    return !window.IsClosed;

                return !_paneContainer.IsHidden;
            }
        }

        /// <summary>
        /// Is inited
        /// </summary>
        public bool IsInited
        {
            get { return (null != _dockManager); }
        }

        #endregion // Members

        #region Public function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Show this content
        /// </summary>
        /// <remarks>Show this content in a dockable pane. If no pane was previuosly created, it creates a new one with default right dock. </remarks>
        public virtual new void Show()
        {
            _Show((null != _paneContainer)? _paneContainer.DockType : Dock.Right);
        }

        /// <summary>
        /// Close content
        /// </summary>
        public virtual new void Close()
        {
            FloatingWindow window = _GetRelatedFloatingWnd();
            if (null != window)
                window.ForceClose();
            else
                _paneContainer.Close();

            FireVisibileStateChanged();
        }

        /// <summary>
        /// Call - fire event closed
        /// </summary>
        /// <remarks>hack</remarks>
        public void FireVisibileStateChanged()
        {
            if (null != VisibileStateChanged)
                VisibileStateChanged(this, EventArgs.Empty);
        }

        #endregion // Public function

        #region Private function
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Find related floating window in manager
        /// </summary>
        private FloatingWindow _GetRelatedFloatingWnd()
        {
            FloatingWindow wndFloating = null;
            if (null != _paneContainer)
                wndFloating = _paneContainer.FloatingWindow;

            return wndFloating;
        }

        /// <summary>
        /// Show this content
        /// </summary>
        /// <param name="dock">New dock border</param>
        /// <remarks>Show this content in a dockable pane. If no pane was previuosly created, it creates a new one with passed initial dock. </remarks>
        private void _Show(Dock dock)
        {
            FloatingWindow window = _GetRelatedFloatingWnd();
            if (null != window)
                window.Show();
            else
            {
                ContainerPane.UseSpecAllocation = true;
                ContainerPane.Show(dock);
            }

            FireVisibileStateChanged();
        }
        #endregion // Private function

        #region Members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Container pane
        /// </summary>
        protected DockablePane _paneContainer = null;

        /// <summary>
        /// Dock manager
        /// </summary>
        private DockManager _dockManager = null;

        #endregion // Members
    }
}
