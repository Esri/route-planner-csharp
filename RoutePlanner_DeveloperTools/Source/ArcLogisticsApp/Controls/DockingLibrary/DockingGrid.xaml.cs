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
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for DockingGrid.xaml
    /// </summary>
    internal partial class DockingGrid : UserControl, ILayoutSerializable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create docking grid.
        /// </summary>
        public DockingGrid()
        {
            InitializeComponent();
        }

        #endregion // Constructors

        #region Public functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add pane.
        /// </summary>
        /// <param name="pane">Inserted pane.</param>
        public void Add(DockablePane pane)
        {
            if (null == _root)
            {
                _root = new Composition(null, pane); // first creation

                _ArrangeLayout();

                _RegisterDropService(pane);
            }
            else if (_root.IsInited && (null == _root.Find(pane)))
            {
                if (pane.UseSpecAllocation)
                    _root.AddSpec(pane, pane.DockType);
                else
                    _root.Add(pane, pane.DockType);

                _ArrangeLayout();

                _RegisterDropService(pane);
            }
        }

        /// <summary>
        /// Add pane relative.
        /// </summary>
        /// <param name="pane">Inserted pane.</param>
        /// <param name="relativePane">Relative pane.</param>
        /// <param name="relativeDock">Dock relative "relative" pane.</param>
        public void Add(DockablePane pane, DockablePane relativePane, Dock relativeDock)
        {
            Debug.Assert(null != _root);
            Debug.Assert(_root.IsInited);

            if (null == _root.Find(pane))
            {
                Debug.Assert(null != _root.Find(relativePane));
                _root.Add(pane, relativePane, relativeDock);
                _ArrangeLayout();

                _RegisterDropService(pane);
            }
        }

        /// <summary>
        /// Remove pane.
        /// </summary>
        /// <param name="pane">Pane for identification.</param>
        public void Remove(DockablePane pane)
        {
            if (null != _root)
            {
                Debug.Assert(_root.IsInited);
                if (null != _root.Find(pane))
                {
                    bool isRemoved = false;
                    if (CompositionType.Terminal != _root.Type)
                        isRemoved = _root.Remove(pane);
                    else
                    {
                        if (_root.AttachedPane.Equals(pane))
                        {
                            _root = null;
                            isRemoved = true;
                        }
                        else
                        {
                            Debug.Assert(false); // not supported
                        }
                    }

                    if (isRemoved)
                        _ArrangeLayout();

                    _UnregisterDropService(pane);

                    // clearing margins
                    var margin = new Thickness(0);
                    pane.SetValue(FrameworkElement.MarginProperty, margin);
                }
            }
        }

        #endregion // Public functions

        #region Private functions
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Remove all children of grid.
        /// </summary>
        /// <param name="grid">Grid to manage.</param>
        /// <remarks>Function use recursion.</remarks>
        private void _Clear(Grid grid)
        {
            foreach (UIElement child in grid.Children)
            {
                if (child is Grid)
                    _Clear(child as Grid);
            }

            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
        }

        /// <summary>
        /// Rebuild layout.
        /// </summary>
        private void _ArrangeLayout()
        {
            _Clear(_panel);

            if (null != _root)
            {
                Debug.Assert(_root.IsInited);

                if (CompositionType.Terminal == _root.Type)
                    _panel.Children.Add(_root.AttachedPane);
                else
                    _root.Arrange(_panel);
            }
        }

        /// <summary>
        /// Registry pane as new drop surface.
        /// </summary>
        /// <param name="pane">Pane to registry as drop surface.</param>
        private void _RegisterDropService(DockablePane pane)
        {
            if (pane.IsDragSupported)
                pane.PaneContent.DockManager.DragPaneServices.Register(pane);
        }

        /// <summary>
        /// Unregistry pane as drop surface.
        /// </summary>
        /// <param name="pane">Pane to unregistry as drop surface.</param>
        private void _UnregisterDropService(DockablePane pane)
        {
            if (pane.IsDragSupported)
                pane.PaneContent.DockManager.DragPaneServices.Unregister(pane);
        }

        #endregion // Private functions

        #region ILayoutSerializable
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Serialize layout.
        /// </summary>
        /// <param name="doc">Document to save.</param>
        /// <param name="nodeRoot">Parent node.</param>
        public void Serialize(XmlDocument doc, XmlNode nodeRoot)
        {
            if (null != _root)
                _root.Serialize(doc, nodeRoot);
        }

        /// <summary>
        /// Deserialize layout.
        /// </summary>
        /// <param name="manager">Dock manager for initing objects.</param>
        /// <param name="rootNode">Node to parse.</param>
        /// <param name="handlerObject">Delegate used to get user defined dockable contents.</param>
        public void Deserialize(DockManager manager,
                                XmlNode rootNode,
                                GetContentFromTypeString handlerObject)
        {
            if (null != _root)
            {
                Debug.Assert(_root.Type == CompositionType.Terminal);
                _root.AttachedPane.Close();
                _root = null;
            }

            if (0 == rootNode.ChildNodes.Count)
                _root = null;
            else
            {
                _root = new Composition();
                _root.Deserialize(manager, rootNode.ChildNodes[0], handlerObject);
                _ArrangeLayout();
            }
        }

        #endregion // ILayoutSerializable

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Root group
        /// </summary>
        private Composition _root = null;

        #endregion // Private members
    }
}
