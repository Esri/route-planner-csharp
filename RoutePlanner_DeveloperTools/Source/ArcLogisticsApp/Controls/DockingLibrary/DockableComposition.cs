using System;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Composition Type
    /// </summary>
    internal enum CompositionType
    {
        Terminal,
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Composition class
    /// </summary>
    internal class Composition : ILayoutSerializable
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Create composition
        /// </summary>
        /// <remarks>Used only for Deserialize</remarks>
        public Composition()
        {
            _type = CompositionType.Terminal;
            _isInited = false;
        }

        public Composition(Composition parent, DockablePane pane)
        {
            _attachedPane = pane;
            _type = CompositionType.Terminal;
            _isInited = true;
        }

        public Composition(Composition parent, Composition first, Composition second, Dock dockType)
        {
            _CreateLineComposition(first, second, dockType);
            _isInited = true;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Type of composition.
        /// </summary>
        public CompositionType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Composition children read-only collection.
        /// </summary>
        /// <remarks>Actual only for CompositionType.Horizontal or CompositionType.Vertical</remarks>
        public ICollection<Composition> Children
        {
            get
            {
                Debug.Assert(CompositionType.Terminal != _type);
                return _children.AsReadOnly();
            }
        }

        /// <summary>
        /// Attached pane
        /// </summary>
        /// <remarks>Actual only for CompositionType.Terminal</remarks>
        public DockablePane AttachedPane
        {
            get
            {
                Debug.Assert(CompositionType.Terminal == _type);
                return _attachedPane;
            }
        }

        /// <summary>
        /// Space arrange factor
        /// </summary>
        public double SpaceFactor
        {
            get { return _spaceFactor; }
            set { _spaceFactor = value; }
        }

        /// <summary>
        /// Is object initialized
        /// </summary>
        public bool IsInited
        {
            get { return _isInited; }
        }

        #endregion // Public properties

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public void AddSpec(DockablePane pane, Dock dockType)
        {
            if (!_isInited)
                return;

            if (CompositionType.Terminal == _type)
                _Add(pane, dockType);
            else
            {
                _CalculateSpaceFactors();

                // find largest pane
                Size maxSize = new Size(0, 0);
                DockablePane largestPane = null;
                _FindLargestPane(ref maxSize, ref largestPane);
                if (null == largestPane)
                    _FindLastPane(ref largestPane);
                Debug.Assert(null != largestPane);

                bool isVertical = (maxSize.Width <= maxSize.Height);
                Add(pane, largestPane, (isVertical) ? Dock.Bottom : Dock.Right);
            }
        }

        public void Add(DockablePane pane, Dock dockType)
        {
            if (!_isInited)
                return;

            _CalculateSpaceFactors();

            _Add(pane, dockType);
        }

        public void Add(DockablePane pane, DockablePane relativePane, Dock relativeDock)
        {
            if (!_isInited)
                return;

            _CalculateSpaceFactors();

            if (CompositionType.Terminal == _type)
            {
                if (_attachedPane.Equals(relativePane))
                    _Add(pane, relativeDock);
            }
            else
            {
                // find relative terminal element index
                int relativeIndex = -1;
                for (int index = 0; index < _children.Count; ++index)
                {
                    Composition currentComposition = _children[index];
                    if (CompositionType.Terminal == currentComposition.Type)
                    {
                        if (currentComposition.AttachedPane.Equals(relativePane))
                        {
                            relativeIndex = index;
                            break; // NOTE: founded
                        }
                    }
                    else
                        currentComposition.Add(pane, relativePane, relativeDock);
                }

                // add new item
                if (-1 != relativeIndex)
                {
                    if (_DoesAdding2Line(relativeDock))
                    {   // add new item to current linear composition
                        // left\top insert to relative position, other - insert after relive

                        Composition newComposition = new Composition(this, pane);

                        _children[relativeIndex].SpaceFactor *= DEFAULT_SPACE_FACTOR; // new child requared part of relative pane
                        newComposition.SpaceFactor = _children[relativeIndex].SpaceFactor;

                        int insertIndex = ((Dock.Left == relativeDock) || (Dock.Top == relativeDock)) ?
                                            relativeIndex : relativeIndex + 1;
                        _children.Insert(insertIndex, newComposition);
                    }
                    else
                    {   // add new pane to terminal composition
                        _children[relativeIndex].Add(pane, relativeDock);
                    }
                }

                Debug.Assert(_IsNormalized());
            }
        }

        /// <summary>
        /// Remove pane
        /// </summary>
        public bool Remove(DockablePane pane)
        {
            if (!_isInited)
                return false;

            Debug.Assert(CompositionType.Terminal != _type);

            _CalculateSpaceFactors();

            bool isRemoved = false;

            // find terminal element to deleting
            Composition terminalComposition2Delete = null;
            for (int index = 0; index < _children.Count; ++index)
            {
                Composition currentComposition = _children[index];
                if (CompositionType.Terminal == currentComposition.Type)
                {
                    if (currentComposition.AttachedPane.Equals(pane))
                    {
                        terminalComposition2Delete = currentComposition;
                        break; // NOTE: founded
                    }
                }
                else
                {   // remove from child composition
                    if (currentComposition.Remove(pane))
                    {
                        isRemoved = true;
                        break;
                    }
                }
            }

            // remove terminal element
            if (null != terminalComposition2Delete)
            {
                _children.Remove(terminalComposition2Delete);
                isRemoved = true;

                if (1 == _children.Count)
                {   // change state to terminal
                    Composition lastChield = _children[0];
                    _children.Clear();
                    if (CompositionType.Terminal == lastChield.Type)
                    {
                        _type = CompositionType.Terminal;
                        _attachedPane = lastChield.AttachedPane;
                    }
                    else
                    {
                        _type = lastChield.Type;
                        ICollection<Composition> children = lastChield.Children;
                        foreach (Composition child in children)
                            _children.Add(child);
                    }
                }
                else
                {
                    // recalculate new space factors
                    Size sz = _CalculateSpaceSize();
                    double fullSize = (_type == CompositionType.Horizontal) ? sz.Height : sz.Width;
                    fullSize += SPLITTER_SIZE;

                    double splitterFree = SPLITTER_SIZE / _children.Count;

                    Size freeSize = terminalComposition2Delete._CalculateSpaceSize();
                    for (int index = 0; index < _children.Count; ++index)
                    {
                        Composition child = _children[index];
                        Size childSize = child._CalculateSpaceSize();
                        child.SpaceFactor = (_type == CompositionType.Horizontal) ?
                             ((childSize.Height + freeSize.Height * child.SpaceFactor + splitterFree) / Math.Max(fullSize, 1)) :
                             ((childSize.Width + freeSize.Width * child.SpaceFactor + splitterFree) / Math.Max(fullSize, 1));
                    }
                }
            }
            else if (isRemoved)
            {   // normalize composition - if child presented as one line
                for (int index = 0; index < _children.Count; ++index)
                {
                    Composition currentChild = _children[index];
                    if (currentChild.Type == _type)
                    {
                        ICollection<Composition> children = currentChild.Children;
                        Debug.Assert(currentChild.Type != CompositionType.Terminal);
                        Debug.Assert(1 < currentChild.Children.Count);

                        Collection<Composition> fromRemoved = new Collection<Composition> ();
                        foreach (Composition child in children)
                            fromRemoved.Add(child);

                        _children.Remove(currentChild);
                        _children.InsertRange(index, fromRemoved);
                    }
                }

                _CalculateSpaceFactors();
            }

            Debug.Assert(_IsNormalized());
            return isRemoved;
        }

        public Composition Find(DockablePane pane)
        {
            if (!_isInited)
                return null;

            Composition terminalComposition = null;
            if (CompositionType.Terminal == _type)
            {
                if (_attachedPane.Equals(pane))
                    terminalComposition = this;
            }
            else
            {
                // find relative terminal element index
                for (int index = 0; index < _children.Count; ++index)
                {
                    Composition currentComposition = _children[index];
                    terminalComposition = currentComposition.Find(pane);
                    if (null != terminalComposition)
                        break;
                }
            }

            return terminalComposition;
        }

        /// <summary>
        /// Recalculate layout for this composition
        /// </summary>
        /// <param name="grid"></param>
        public void Arrange(Grid grid)
        {
            Debug.Assert(CompositionType.Terminal != _type);

            double value = 0;
            if (_type == CompositionType.Horizontal)
                _ArrangeHorizontal(grid, ref value);
            else
                _ArrangeVertical(grid, ref value);
        }
        #endregion // Public methods

        #region ILayoutSerializable
        /// <summary>
        /// Serialize layout
        /// </summary>
        /// <param name="doc">Document to save</param>
        /// <param name="nodeParent">Parent node</param>
        public void Serialize(XmlDocument doc, XmlNode nodeParent)
        {
            XmlNode nodeChild = doc.CreateElement(ELEMENT_NAME_CHILD);
            nodeChild.Attributes.Append(doc.CreateAttribute(ATTRIBUTE_NAME_TYPE));
            nodeChild.Attributes[ATTRIBUTE_NAME_TYPE].Value = _type.ToString();

            nodeChild.Attributes.Append(doc.CreateAttribute(ATTRIBUTE_NAME_SFACTOR));
            nodeChild.Attributes[ATTRIBUTE_NAME_SFACTOR].Value = _spaceFactor.ToString(CultureInfo.GetCultureInfo(STORAGE_CULTURE));

            if (_type == CompositionType.Terminal)
            {
                Debug.Assert(null != _attachedPane);

                XmlNode nodeAttachedPane = doc.CreateElement(ELEMENT_NAME_DOCKPANE);
                _attachedPane.Serialize(doc, nodeAttachedPane);
                nodeChild.AppendChild(nodeAttachedPane);
            }
            else
            {
                _CalculateSpaceFactors();

                XmlNode nodeChildGroups = doc.CreateElement(ELEMENT_NAME_CHILDGROUPS);
                for (int index = 0; index < _children.Count; ++index)
                    _children[index].Serialize(doc, nodeChildGroups);

                nodeChild.AppendChild(nodeChildGroups);
            }

            nodeParent.AppendChild(nodeChild);
        }

        /// <summary>
        /// Deserialize layout
        /// </summary>
        /// <param name="manager">Dock manager for initing objects</param>
        /// <param name="node">Node to parse</param>
        /// <param name="handlerObject">Delegate used to get user defined dockable contents</param>
        public void Deserialize(DockManager manager, XmlNode node, GetContentFromTypeString handlerObject)
        {
            _type = (CompositionType)Enum.Parse(typeof(CompositionType), node.Attributes[ATTRIBUTE_NAME_TYPE].Value);
            _spaceFactor = double.Parse(node.Attributes[ATTRIBUTE_NAME_SFACTOR].Value, CultureInfo.GetCultureInfo(STORAGE_CULTURE));

            if (_type == CompositionType.Terminal)
            {
                Debug.Assert(node.ChildNodes[0].Name == ELEMENT_NAME_DOCKPANE);

                DockablePane pane = new DockablePane();
                pane.Deserialize(manager, node.ChildNodes[0], handlerObject);
                _attachedPane = pane;

                if (pane.IsDragSupported)
                    manager.DragPaneServices.Register(pane);
            }
            else
            {
                Debug.Assert(node.ChildNodes[0].Name == ELEMENT_NAME_CHILDGROUPS);

                foreach (XmlNode nodeChild in node.ChildNodes[0].ChildNodes)
                {
                    Composition composition = new Composition();
                    composition.Deserialize(manager, nodeChild, handlerObject);
                    _children.Add(composition);
                }
            }

            _isInited = true;
        }
        #endregion // ILayoutSerializable

        #region Private helpers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Calculate composition space size
        /// </summary>
        private Size _CalculateSpaceSize()
        {
            Size spaceSize = new Size(0, 0);
            if (CompositionType.Terminal == _type)
                spaceSize = new Size(_attachedPane.ActualWidth, _attachedPane.ActualHeight);
            else
            {
                for (int index = 0; index < _children.Count; ++index)
                {
                    Size currentSize = _children[index]._CalculateSpaceSize();
                    if (_type == CompositionType.Horizontal)
                    {
                        spaceSize.Width = Math.Max(spaceSize.Width, currentSize.Width);
                        spaceSize.Height += currentSize.Height;
                    }
                    else
                    {
                        spaceSize.Width += currentSize.Width;
                        spaceSize.Height = Math.Max(spaceSize.Height, currentSize.Height);
                    }
                }
            }

            return spaceSize;
        }

        /// <summary>
        /// Normalize factors - sum probably SPACE_FACTOR_FULL
        /// </summary>
        private void _NormalizeSpaceFactors(double sumFactor)
        {
            if (SPACE_FACTOR_FULL < sumFactor)
            {
                // find bigger pane
                int biggerIndex = 0;
                double maxFactor = _children[biggerIndex].SpaceFactor;
                for (int index = biggerIndex + 1; index < _children.Count; ++index)
                {
                    Composition child = _children[index];
                    if (maxFactor <= child.SpaceFactor)
                    {
                        maxFactor = child.SpaceFactor;
                        biggerIndex = index;
                    }
                }

                // gecrease bigger pane
                _children[biggerIndex].SpaceFactor -= sumFactor - SPACE_FACTOR_FULL;
            }
        }

        /// <summary>
        /// Calculation space factors
        /// </summary>
        private void _CalculateSpaceFactors()
        {
            Size fullSize = _CalculateSpaceSize();
            if ((fullSize.Height <= 0.0) && (fullSize.Height == fullSize.Width))
                return; // NOTE: exit

            // minimal factor - page have min size
            double minFactor = (_type == CompositionType.Horizontal)?
                (DockablePane.MIN_PANE_SIZE / Math.Max(fullSize.Height, 1)) :
                (DockablePane.MIN_PANE_SIZE / Math.Max(fullSize.Width, 1));

            double increaseFactorValue = 0; // pages with min size increase with this value
            for (int index = 0; index < _children.Count; ++index)
            {
                Composition child = _children[index];
                Size childSize = child._CalculateSpaceSize();

                double newScale = (_type == CompositionType.Horizontal) ?
                     (childSize.Height / Math.Max(fullSize.Height, 1)) :
                     (childSize.Width / Math.Max(fullSize.Width, 1));
                // reset tinkle - 1%
                if (SPACE_FACTOR_TINKLE < Math.Abs(child.SpaceFactor - newScale))
                    child.SpaceFactor = newScale;

                if (child.SpaceFactor < minFactor)
                {
                    increaseFactorValue += minFactor - child.SpaceFactor;
                    child.SpaceFactor = minFactor;
                }
            }

            // normalize factors - sum probably SPACE_FACTOR_FULL
            double sumFactor = 0.0;
            if (0 < increaseFactorValue)
            {
                // find count of pages for decreasing
                int decreaseCount = 0;
                for (int index = 0; index < _children.Count; ++index)
                {
                    if (minFactor + increaseFactorValue < _children[index].SpaceFactor)
                        ++decreaseCount;
                }

                // decrease big pages
                double decreaseFactor = increaseFactorValue / decreaseCount;
                for (int index = 0; index < _children.Count; ++index)
                {
                    Composition child = _children[index];
                    if (minFactor + decreaseFactor < child.SpaceFactor)
                        child.SpaceFactor -= decreaseFactor;

                    sumFactor += child.SpaceFactor;
                }
            }
            else
            {
                // callculate sum factors
                for (int index = 0; index < _children.Count; ++index)
                    sumFactor += _children[index].SpaceFactor;
            }

            _NormalizeSpaceFactors(sumFactor);
        }

        /// <summary>
        /// Check is composition normalized
        /// </summary>
        /// <remarks>Composition is normalized if all children have other type</remarks>
        private bool _IsNormalized()
        {
            bool isNormalized = true;
            for (int index = 0; index < _children.Count; ++index)
            {
                if (_children[index].Type != CompositionType.Terminal)
                {
                    if (_children[index].Type == _type)
                    {
                        isNormalized = false;
                        break; // NOTE: result founded
                    }
                }
            }

            return isNormalized;
        }

        /// <summary>
        /// Calculate hypotenuse
        /// </summary>
        private double _CalculateHypotenuse(Size size)
        {
            return (size.Width * size.Width + size.Height * size.Height);
        }

        /// <summary>
        /// Find largest pane
        /// </summary>
        /// <remarks>largest by hypotenuse</remarks>
        private void _FindLargestPane(ref Size maxSize, ref DockablePane largestPane)
        {
            for (int index = 0; index < _children.Count; ++index)
            {
                Composition child = _children[index];
                if (child.Type != CompositionType.Terminal)
                    child._FindLargestPane(ref maxSize, ref largestPane);
                else
                {
                    Size sz = child._CalculateSpaceSize();
                    if (_CalculateHypotenuse(maxSize) < _CalculateHypotenuse(sz))
                    {
                        largestPane = child.AttachedPane;
                        maxSize = sz;
                    }
                }
            }
        }

        /// <summary>
        /// Find last pane
        /// </summary>
        private void _FindLastPane(ref DockablePane largestPane)
        {
            for (int index = _children.Count - 1; 0 <= index; --index)
            {
                Composition child = _children[index];
                if (child.Type != CompositionType.Terminal)
                    child._FindLastPane(ref largestPane);
                else
                    largestPane = child.AttachedPane;

                if (null != largestPane)
                    break; // result founded
            }
        }

        /// <summary>
        /// Check is dock is horizontal
        /// </summary>
        private bool _IsHorizontalDock(Dock dockType)
        {
            return ((Dock.Top == dockType) || (Dock.Bottom == dockType));
        }

        /// <summary>
        /// Check - if add new elemet to composition by dock - new composition is a line
        /// </summary>
        private bool _DoesAdding2Line(Dock dockType)
        {
            return ((_IsHorizontalDock(dockType) && (CompositionType.Horizontal == _type)) ||
                    (!_IsHorizontalDock(dockType) && (CompositionType.Vertical == _type)));
        }

        /// <summary>
        /// Get composition type by dock
        /// </summary>
        private CompositionType _GetCompositionTypeByDock(Dock dockType)
        {
            return (_IsHorizontalDock(dockType)) ? CompositionType.Horizontal : CompositionType.Vertical;
        }

        /// <summary>
        /// Insert new composition element by dock
        /// </summary>
        private void _InsertElement2Line(Composition newElement, Dock dockType)
        {
            if ((Dock.Left == dockType) || (Dock.Top == dockType))
                _children.Insert(0, newElement);
            else if ((Dock.Right == dockType) || (Dock.Bottom == dockType))
                _children.Add(newElement);
            else
            {
                Debug.Assert(false); // NOTE: not supported
            }
        }

        /// <summary>
        /// Create line composition
        /// </summary>
        private void _CreateLineComposition(Composition first, Composition second, Dock dockType)
        {
            _attachedPane = null;
            _type = _GetCompositionTypeByDock(dockType);

            _children.Add(first);
            _InsertElement2Line(second , dockType);
        }

        /// <summary>
        /// Do copy of this composition
        /// </summary>
        private Composition _CreateCopy()
        {
            Composition obj = new Composition();
            obj._children.AddRange(this._children);
            obj._type = this._type;
            obj._attachedPane = this._attachedPane;
            obj._isInited = this._isInited;

            return obj;
        }

        /// <summary>
        /// Add pane to composition
        /// </summary>
        private void _Add(DockablePane pane, Dock dockType)
        {
            Composition newElement = new Composition(this, pane);
            if (CompositionType.Terminal == _type)
            {   // change compostion from terminal to linear
                Debug.Assert(0 == _children.Count);

                Composition first = new Composition(this, _attachedPane);
                _CreateLineComposition(first, newElement, dockType);
            }
            else
            {
                if (_DoesAdding2Line(dockType))
                {   // add new element to linear composition

                    // update children space factors
                    // new child - requared 50% of all layout
                    for (int index = 0; index < _children.Count; ++index)
                        _children[index].SpaceFactor /= 2;

                    _InsertElement2Line(newElement, dockType);
                }
                else
                {
                    // do copy from current composition - it is linear composition
                    Composition compositionCopy = _CreateCopy();
                    // remove old composition elements
                    _children.Clear();
                    // do one linear composition:
                    // add old elements as one linear composition and
                    // add new element to linear composition
                    _CreateLineComposition(compositionCopy, newElement, dockType);
                }
            }

            Debug.Assert(_IsNormalized());
        }

        /// <summary>
        /// Arrange vertical composition (creating grid with collumns)
        /// </summary>
        private void _ArrangeVertical(Grid grid, ref double minHeight)
        {
            minHeight = DockablePane.MIN_PANE_SIZE;
            for (int index = 0; index < _children.Count; ++index)
            {
                Composition currentComposition = _children[index];

                // create column for child
                ColumnDefinition column = new ColumnDefinition();
                column.Width = new GridLength(currentComposition.SpaceFactor, GridUnitType.Star);
                grid.ColumnDefinitions.Add(column);

                double minWidth = DockablePane.MIN_PANE_SIZE;
                UIElement gridElement = null;
                if (CompositionType.Terminal == currentComposition.Type)
                    gridElement = currentComposition.AttachedPane;
                else
                {   // if child is compostition - create grid for children
                    Grid lineGrid = new Grid();
                    currentComposition._ArrangeHorizontal(lineGrid, ref minWidth);
                    gridElement = lineGrid;

                    double splittersSpace = SPLITTER_SIZE * (currentComposition.Children.Count - 1);
                    lineGrid.MinHeight = grid.MinHeight = DockablePane.MIN_PANE_SIZE * currentComposition.Children.Count + splittersSpace;
                    minHeight = Math.Max(minHeight, grid.MinHeight);
                }
                column.MinWidth = minWidth;

                // inited column number in new element
                grid.Children.Add(gridElement);
                Grid.SetColumn(gridElement, index);

                // set margin for splitter
                Thickness margin = new Thickness(0);
                if ((index < _children.Count - 1) && (1 < _children.Count))
                    margin = new Thickness(0, 0, SPLITTER_SIZE, 0);
                gridElement.SetValue(FrameworkElement.MarginProperty, margin);

                if (0 < index)
                {   // add splitter
                    GridSplitter splitter = new GridSplitter();
                    splitter.Width = SPLITTER_SIZE;
                    splitter.HorizontalAlignment = HorizontalAlignment.Right;
                    splitter.VerticalAlignment = VerticalAlignment.Stretch;
                    splitter.Style = (Style)Application.Current.FindResource("DockingSplitterStyle");
                    Grid.SetColumn(splitter, index - 1);
                    grid.Children.Add(splitter);
                }
            }
        }

        /// <summary>
        /// Arrange horizontal composition (creating grid with rows)
        /// </summary>
        private void _ArrangeHorizontal(Grid grid, ref double minWidth)
        {
            Debug.Assert(CompositionType.Terminal != _type);

            minWidth = DockablePane.MIN_PANE_SIZE;
            for (int index = 0; index < _children.Count; ++index)
            {
                Composition currentComposition = _children[index];

                // create row for child
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(currentComposition.SpaceFactor, GridUnitType.Star);
                grid.RowDefinitions.Add(row);

                double minHeight = DockablePane.MIN_PANE_SIZE;
                UIElement gridElement = null;
                if (CompositionType.Terminal == currentComposition.Type)
                    gridElement = currentComposition.AttachedPane;
                else
                {   // if child is compostition - create grid for children
                    Grid lineGrid = new Grid();
                    currentComposition._ArrangeVertical(lineGrid, ref minHeight);
                    gridElement = lineGrid;

                    double splittersSpace = SPLITTER_SIZE * (currentComposition.Children.Count - 1);
                    lineGrid.MinWidth = grid.MinWidth = DockablePane.MIN_PANE_SIZE * currentComposition.Children.Count + splittersSpace;
                    minWidth = Math.Max(minWidth, grid.MinWidth);
                }
                row.MinHeight = minHeight;

                // inited row number in new element
                grid.Children.Add(gridElement);
                Grid.SetRow(gridElement, index);

                // set margin for splitter
                Thickness margin = new Thickness(0);
                if ((index < _children.Count - 1) && (1 < _children.Count))
                    margin = new Thickness(0, 0, 0, SPLITTER_SIZE);
                gridElement.SetValue(FrameworkElement.MarginProperty, margin);

                if (0 < index)
                {   // add splitter
                    GridSplitter splitter = new GridSplitter();
                    splitter.Height = SPLITTER_SIZE;
                    splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
                    splitter.VerticalAlignment = VerticalAlignment.Bottom;
                    splitter.Style = (Style)Application.Current.FindResource("DockingSplitterStyle");
                    Grid.SetRow(splitter, index - 1);
                    grid.Children.Add(splitter);
                }
            }
        }

        #endregion // Private helpers

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Type of composition
        /// </summary>
        private CompositionType _type = CompositionType.Terminal;

        /// <summary>
        /// Children collection
        /// </summary>
        /// <remarks>Actual only for CompositionType.Horizontal or CompositionType.Vertical</remarks>
        private List<Composition> _children = new List<Composition>();

        /// <summary>
        /// Attached pane
        /// </summary>
        /// <remarks>Actual only for CompositionType.Terminal</remarks>
        private DockablePane _attachedPane = null;

        /// <summary>
        /// Space arrange factor
        /// </summary>
        private double _spaceFactor = DEFAULT_SPACE_FACTOR;

        /// <summary>
        /// Is object inited
        /// </summary>
        private bool _isInited = false;

        /// <summary>
        /// Splitter size
        /// </summary>
        private const int SPLITTER_SIZE = 4;

        /// <summary>
        /// Default space factor
        /// </summary>
        private const double DEFAULT_SPACE_FACTOR = 0.5;

        /// <summary>
        /// Full space factor - 100%
        /// </summary>
        private const double SPACE_FACTOR_FULL = 1.0;

        /// <summary>
        /// Tinkle space factor - 1%
        /// </summary>
        private const double SPACE_FACTOR_TINKLE = 0.01;

        /// <summary>
        /// Serialize\Deserialize const
        /// </summary>
        private const string ELEMENT_NAME_CHILD = "Child";
        private const string ELEMENT_NAME_DOCKPANE = "DockablePane";
        private const string ELEMENT_NAME_CHILDGROUPS = "ChildGroups";
        private const string ATTRIBUTE_NAME_TYPE = "Type";
        private const string ATTRIBUTE_NAME_SFACTOR = "SpaceFactor";

        private const string STORAGE_CULTURE = "en-US";

        #endregion // Private members
    }
}
