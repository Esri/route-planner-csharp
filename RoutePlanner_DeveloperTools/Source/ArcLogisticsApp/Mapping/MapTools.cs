using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.Tools;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.Geocoding;

namespace ESRI.ArcLogistics.App.Mapping
{
    /// <summary>
    /// Class for work with map tools, including editing tool.
    /// </summary>
    internal class MapTools
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="mapCtrl">Parent map control.</param>
        /// <param name="toolButtonsPanel">Panel for tool buutons.</param>
        public MapTools(MapControl mapCtrl, StackPanel toolButtonsPanel)
        {
            _mapctrl = mapCtrl;
            _toolButtonsPanel = toolButtonsPanel;
        }

        #endregion

        #region Public members

        /// <summary>
        /// Get all tools.
        /// </summary>
        public IList<IMapTool> Tools
        {
            get
            {
                return _tools.AsReadOnly();
            }
        }

        /// <summary>
        /// Current tool.
        /// </summary>
        public IMapTool CurrentTool
        {
            get { return _currentTool; }
            set
            {
                if (_currentTool != null)
                    _DeactivateTool();

                if (value != null)
                    _ActivateTool(value);
            }
        }

        /// <summary>
        /// Editing tool.
        /// </summary>
        public IMapTool EditingTool
        {
            get { return _editingTool; }
        }

        /// <summary>
        /// Object layer, that contains editing object.
        /// </summary>
        public ObjectLayer EditedObjectLayer
        {
            get { return _editedObjectLayer; }
        }

        /// <summary>
        /// Object layer, that contains edit markers.
        /// </summary>
        public ObjectLayer EditMarkersLayer
        {
            get { return _editMarkersLayer; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Add tool.
        /// </summary>
        /// <param name="tool">Tool for adding.</param>
        /// <param name="canActivateToolHandler">Delegate to check can activated.</param>
        public void AddTool(IMapTool tool, CanActivateToolHandler canActivateToolHandler)
        {
            EditingTool editingTool = tool as EditingTool;

            if (editingTool != null)
            {
                _AddEditingTool(editingTool);
            }
            else
            {
                _AddTool(tool, canActivateToolHandler);
            }

            tool.OnComplete += new EventHandler(_OnCompleteTool);
        }

        /// <summary>
        /// Add tools.
        /// </summary>
        /// <param name="tools">Tools for adding.</param>
        /// <param name="canActivateToolHandler">Callback for checking is tool can be activated.</param>
        public void AddTools(IMapTool[] tools, CanActivateToolHandler canActivateToolHandler)
        {
            Debug.Assert(tools != null && tools.Length > 0);

            _AddTools(tools, canActivateToolHandler);
        }

        /// <summary>
        /// Clear tools.
        /// </summary>
        internal void ClearTools()
        {
            Debug.Assert(_tools.Count == _toolButtonsPanel.Children.Count);

            int i = 0;
            while (i < _tools.Count)
            {
                IMapTool tool = _tools[i];
                _tools.RemoveAt(i);
                tool.EnabledChanged -= new EventHandler(_ToolEnabledChanged);
                tool.OnComplete -= new EventHandler(_OnCompleteTool);

                _canActivateToolHandlers.RemoveAt(i);

                ToggleButton toggleButton = _toolButtonsPanel.Children[i] as ToggleButton;

                if (toggleButton != null)
                {
                    toggleButton.Click -= new RoutedEventHandler(_ToolClick);
                }
                else
                {
                    // Deleting of tool combobutton is not supported.
                    Debug.Assert(false);
                }

                _toolButtonsPanel.Children.RemoveAt(i);
            }
        }

        /// <summary>
        /// Start edit.
        /// </summary>
        /// <param name="item">Editing item.</param>
        internal void StartEdit(object item)
        {
            _editedCollection.Add(item);

            // In case of editing barriers need to support correct view.
            if (item is Barrier)
            {
                _editedObjectLayer.LayerContext = App.Current.CurrentDate;
            }

            _editedObjectLayer.MapLayer.Graphics[0].Select();

            // Prepare toolbar.
            for (int index = 0; index < _toolButtonsPanel.Children.Count; index++)
            {
                ToggleButton toggleButton = _toolButtonsPanel.Children[index] as ToggleButton;

                if (toggleButton != null)
                {
                    // TODO: check if this needed
                    toggleButton.IsEnabled = _canActivateToolHandlers[index]();
                    toggleButton.IsChecked = false;
                }
                else
                {
                    ToolComboButton toolComboButton = _toolButtonsPanel.Children[index] as ToolComboButton;
                    toolComboButton.IsEnabled = _canActivateToolHandlers[index]();
                }
            }

            // Prepare for edit.
            _ActivateTool(_editingTool);
            _editingTool.EditingObject = item;
            FillEditMarkers(item);
            _editMarkersLayer.Visible = true;
        }

        /// <summary>
        /// End edit.
        /// </summary>
        /// <param name="graphic">Edited graphic.</param>
        internal void EndEdit(Graphic graphic)
        {
            graphic.Geometry = _editedObjectLayer.MapLayer.Graphics[0].Geometry;
            _editedCollection.Clear();

            Debug.Assert(_currentTool != null);

            foreach (UIElement button in _toolButtonsPanel.Children)
            {
                ToggleButton toggleButton = button as ToggleButton;
                if (toggleButton != null)
                {
                    toggleButton.IsChecked = false;
                }
            }
            _DeactivateTool();

            _editMarkersLayer.Visible = false;
            _editMarkers.Clear();
        }

        /// <summary>
        /// Clear editing markers.
        /// </summary>
        public void ClearEditMarkers()
        {
            if (_editMarkers.Count > 0)
                _editMarkers.Clear();
        }

        /// <summary>
        /// Create edit markers to edit object.
        /// </summary>
        /// <param name="obj">Editing object.</param>
        public void FillEditMarkers(object obj)
        {
            if (obj is IGeocodable)
            {
                _AddMarker(-1, obj);
            }
            else if ((obj is Zone) || (obj is Barrier))
            {
                object geometry = (obj is Zone) ? (obj as Zone).Geometry : (obj as Barrier).Geometry;
                if (null != geometry)
                {
                    if (geometry is ESRI.ArcLogistics.Geometry.Point)
                    {
                        _AddMarker(-1, obj);
                    }
                    else if (geometry is ESRI.ArcLogistics.Geometry.Polygon)
                    {
                        _FillGeometryEditMarkers(obj, geometry as ESRI.ArcLogistics.Geometry.Polygon);
                    }
                    else if (geometry is ESRI.ArcLogistics.Geometry.Polyline)
                    {
                        _FillGeometryEditMarkers(obj, geometry as ESRI.ArcLogistics.Geometry.Polyline);
                    }
                    else
                        Debug.Assert(false);
                }
            }
            else
                Debug.Assert(false);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Add editing tool to tools list.
        /// </summary>
        /// <param name="tool">Editing tool.</param>
        private void _AddEditingTool(EditingTool tool)
        {
            Debug.Assert(_editingTool == null);

            _editedCollection = new ObservableCollection<object>();
            _editedObjectLayer = new ObjectLayer(_editedCollection, typeof(object), false);
            _editedObjectLayer.ConstantOpacity = true;
            _mapctrl.AddLayer(_editedObjectLayer);

            _editMarkers = new ObservableCollection<EditingMarker>();
            _editMarkersLayer = new ObjectLayer(_editMarkers, typeof(EditingMarker), false);
            _editMarkersLayer.ConstantOpacity = true;
            _mapctrl.AddLayer(_editMarkersLayer);

            _editingTool = (EditingTool)tool;
            _editingTool.Initialize(_mapctrl);

            _editingTool.SetLayer(_editMarkersLayer);
            _editingTool.CursorChanged += new EventHandler(_EditingToolCursorChanged);
        }

        /// <summary>
        /// Add custom tool.
        /// </summary>
        /// <param name="tool">Tool to add.</param>
        /// <param name="canActivateToolHandler">Handler for check activation.</param>
        private void _AddTool(IMapTool tool, CanActivateToolHandler canActivateToolHandler)
        {
            tool.Initialize(_mapctrl);
            _tools.Add(tool);
            _canActivateToolHandlers.Add(canActivateToolHandler);

            tool.EnabledChanged += new EventHandler(_ToolEnabledChanged);

            // Create tool button.
            ToggleButton button = new ToggleButton();
            button.ToolTip = tool.TooltipText;
            button.Style = (Style)App.Current.FindResource("MapToolButtonStyle");
            button.IsEnabled = false;
            button.Click += new RoutedEventHandler(_ToolClick);

            BitmapImage bitmap = new BitmapImage(new Uri(tool.IconSource, UriKind.Relative));
            Image img = new Image();
            img.Source = bitmap;
            img.Margin = (Thickness)App.Current.FindResource("ToolButtonImageMargin");
            img.VerticalAlignment = VerticalAlignment.Center;
            img.HorizontalAlignment = HorizontalAlignment.Center;
            button.Content = img;

            _toolButtonsPanel.Children.Add(button);
        }

        /// <summary>
        /// Add tools.
        /// </summary>
        /// <param name="tools">Tools for adding.</param>
        /// <param name="canActivateToolHandler">Callback for checking is tool can be activated.</param>
        private void _AddTools(IMapTool[] tools, CanActivateToolHandler canActivateToolHandler)
        {
            foreach (IMapTool tool in tools)
            {
                tool.Initialize(_mapctrl);
            }

            IMapTool toolOnPanel = tools[0];
            toolOnPanel.OnComplete += new EventHandler(_OnCompleteTool);
            toolOnPanel.EnabledChanged += new EventHandler(_ToolEnabledChanged);
            _tools.Add(toolOnPanel);

            _canActivateToolHandlers.Add(canActivateToolHandler);

            ToolComboButton button = new ToolComboButton();
            button.ToolActivated += new EventHandler(_OnToolActivated);
            button.Init(tools);

            _toolButtonsPanel.Children.Add(button);
        }

        /// <summary>
        /// React on tool enabled changed.
        /// </summary>
        /// <param name="sender">Tool.</param>
        /// <param name="e">Ignored.</param>
        private void _ToolEnabledChanged(object sender, EventArgs e)
        {
            IMapTool tool = (IMapTool)sender;

            int index = _tools.IndexOf(tool);

            if (index != -1)
            {
                if (tool == _currentTool && !tool.IsEnabled && _currentTool.IsActivated)
                {
                    _DeactivateTool();
                }

                ToggleButton toggleButton = _toolButtonsPanel.Children[index] as ToggleButton;
                if (toggleButton != null)
                {
                    toggleButton.IsEnabled = tool.IsEnabled;
                }
                else
                {
                    ToolComboButton toolComboButton = _toolButtonsPanel.Children[index] as ToolComboButton;
                    toolComboButton.Enable(tool.IsEnabled);
                }
            }
        }

        /// <summary>
        /// Change map cursor.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _EditingToolCursorChanged(object sender, EventArgs e)
        {
            Mouse.OverrideCursor = _editingTool.Cursor;
        }

        /// <summary>
        /// On tool complete event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OnCompleteTool(object sender, EventArgs e)
        {
            if (_currentTool != _editingTool)
            {
                _DeactivateTool();
            }
        }

        /// <summary>
        /// React on tool button click.
        /// </summary>
        /// <param name="sender">Tool button.</param>
        /// <param name="e">Ignored.</param>
        private void _ToolClick(object sender, RoutedEventArgs e)
        {
            int index = _toolButtonsPanel.Children.IndexOf((ToggleButton)sender);
            _OnToolClick(_tools[index]);
        }

        /// <summary>
        /// On tool activated by tool combo button.
        /// </summary>
        /// <param name="sender">Tool combo button.</param>
        /// <param name="e">Ignored.</param>
        private void _OnToolActivated(object sender, EventArgs e)
        {
            ToolComboButton toolComboButton = sender as ToolComboButton;
            int index = _toolButtonsPanel.Children.IndexOf(toolComboButton);

            _OnToolClick(toolComboButton.SelectedTool);

            _tools[index].OnComplete -= new EventHandler(_OnCompleteTool);
            _tools[index].EnabledChanged -= new EventHandler(_ToolEnabledChanged);

            _tools[index] = toolComboButton.SelectedTool;

            _tools[index].OnComplete += new EventHandler(_OnCompleteTool);
            _tools[index].EnabledChanged += new EventHandler(_ToolEnabledChanged);
        }

        /// <summary>
        /// React on tool click.
        /// </summary>
        /// <param name="tool"></param>
        private void _OnToolClick(IMapTool tool)
        {
            if (tool == _currentTool)
            {
                // Deactivate current tool.
                _DeactivateTool();
            }
            else
            {
                // Deactivate current tool and activate chosen.
                if (_currentTool != null)
                    _DeactivateTool();
                _ActivateTool(tool);
            }
        }

        /// <summary>
        /// Activate tool.
        /// </summary>
        /// <param name="tool">Tool to activate.</param>
        private void _ActivateTool(IMapTool tool)
        {
            _currentTool = tool;

            tool.Activate();

            if (tool != _editingTool)
            {
                _overridedCursor = _mapctrl.map.Cursor;
                _mapctrl.map.Cursor = tool.Cursor;
            }
        }

        /// <summary>
        /// Deactivate current tool.
        /// </summary>
        private void _DeactivateTool()
        {
            _mapctrl.ClickedCoords = null;
            if (_currentTool != _editingTool)
            {
                int oldIndex = _tools.IndexOf(_currentTool);
                ToggleButton toggleButton = _toolButtonsPanel.Children[oldIndex] as ToggleButton;
                if (toggleButton != null)
                {
                    toggleButton.IsChecked = false;
                }
                else
                {
                    ToolComboButton toolComboButton = _toolButtonsPanel.Children[oldIndex] as ToolComboButton;
                    toolComboButton.Check(false);
                }

                _mapctrl.map.Cursor = _overridedCursor;
                _overridedCursor = null;
            }

            if (_currentTool != null)
            {
                _currentTool.Deactivate();
            }
            if (_mapctrl.IsInEditedMode)
            {
                _currentTool = _editingTool;
                _editingTool.EditingObject = _mapctrl.EditedObject;
            }
            else
                _currentTool = null;
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Create edit markers for polygon .
        /// </summary>
        /// <param name="obj">Editing object.</param>
        /// <param name="polygon">Polygon geometry.</param>
        private void _FillGeometryEditMarkers(object obj, ESRI.ArcLogistics.Geometry.Polygon polygon)
        {
            for (int pointIndex = 0; pointIndex < polygon.TotalPointCount; pointIndex++)
            {
                _AddMarker(pointIndex, obj);
            }
        }

        /// <summary>
        /// Create edit markers for polygon.
        /// </summary>
        /// <param name="obj">Editing object.</param>
        /// <param name="polyCurve">Polygon geometry</param>
        private void _FillGeometryEditMarkers(object obj, ESRI.ArcLogistics.Geometry.PolyCurve polyCurve)
        {
            for (int pointIndex = 0; pointIndex < polyCurve.TotalPointCount; pointIndex++)
            {
                _AddMarker(pointIndex, obj);
            }
        }

        /// <summary>
        /// Add marker.
        /// </summary>
        /// <param name="multipleIndex">Marker index.</param>
        /// <param name="editingObject">Editing object.</param>
        private void _AddMarker(int multipleIndex, object editingObject)
        {
            EditingMarker editingMarker = new EditingMarker(multipleIndex, editingObject);
            _editMarkers.Add(editingMarker);
        }
        
        #endregion

        #region Private members

        /// <summary>
        /// Parent map control.
        /// </summary>
        private MapControl _mapctrl;

        /// <summary>
        /// Current tool.
        /// </summary>
        private IMapTool _currentTool;

        /// <summary>
        /// Editing tool.
        /// </summary>
        private EditingTool _editingTool;

        /// <summary>
        /// Available tools.
        /// </summary>
        private List<IMapTool> _tools = new List<IMapTool>();

        /// <summary>
        /// Handlers to check activate.
        /// </summary>
        private List<CanActivateToolHandler> _canActivateToolHandlers = new List<CanActivateToolHandler>();

        /// <summary>
        /// Layer for edit markers.
        /// </summary>
        private ObjectLayer _editMarkersLayer;

        /// <summary>
        /// Edit markers collection.
        /// </summary>
        private ObservableCollection<EditingMarker> _editMarkers;

        /// <summary>
        /// Layer to show edited object.
        /// </summary>
        private ObjectLayer _editedObjectLayer;

        /// <summary>
        /// Collection for edited object.
        /// </summary>
        private ObservableCollection<object> _editedCollection;

        /// <summary>
        /// Tool buttons panel.
        /// </summary>
        private StackPanel _toolButtonsPanel;

        /// <summary>
        /// Previous overrided cursor.
        /// </summary>
        private Cursor _overridedCursor;

        #endregion
    }
}
