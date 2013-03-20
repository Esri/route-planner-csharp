using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcLogistics.Services;
using ESRI.ArcLogistics.App.Mapping;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_HeaderButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ContentBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_BaseLayersStack", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_OtherLayersStack", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_ObjectLayersStack", Type = typeof(StackPanel))]

    // Widget shows map layer in two sections 
    internal class MapLayersWidget : Control
    {
        static MapLayersWidget()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapLayersWidget), new FrameworkPropertyMetadata(typeof(MapLayersWidget)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
        }

        #region Public Properties

        /// <summary>
        /// ActiveBaseLayer changed event.
        /// </summary>
        public static readonly RoutedEvent ActiveBaseLayerEventChanged = EventManager.RegisterRoutedEvent("ActiveBaseLayerChanged",
           RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MapLayersWidget));

        /// <summary>
        /// ActiveBaseLayer changed event hamdler.
        /// </summary>
        public event RoutedEventHandler ActiveBaseLayerChanged
        {
            add { AddHandler(MapLayersWidget.ActiveBaseLayerEventChanged, value); }
            remove { RemoveHandler(MapLayersWidget.ActiveBaseLayerEventChanged, value); }
        }

        public static readonly DependencyProperty AllLayersProperty =
            DependencyProperty.Register("AllLayers", typeof(ICollection<MapLayer>), typeof(MapLayersWidget));

        public static readonly DependencyProperty ActivetBaseLayerProperty =
            DependencyProperty.Register("ActiveBaseLayer", typeof(MapLayer), typeof(MapLayersWidget));

        /// <summary>
        /// Gets/sets collection of all layers
        /// </summary>
        public ICollection<MapLayer> AllLayers
        {
            get { return (ICollection<MapLayer>)GetValue(AllLayersProperty); }
            set
            {
                SetValue(AllLayersProperty, value);
                if (value != null)
                {
                    if (!_isLoaded)
                        _SortLayersByType();
                }
                else
                    _ClearLayersCollections();
            }
        }

        /// <summary>
        /// Gets/sets current selected base layer
        /// </summary>
        public MapLayer ActiveBaseLayer
        {
            get { return (MapLayer)GetValue(ActivetBaseLayerProperty); }
            set { SetValue(ActivetBaseLayerProperty, value); }
        }

        /// <summary>
        /// Object layers, controlled by widget
        /// </summary>
        public IList<ObjectLayer> ObjectLayers
        {
            get
            {
                return _objectLayers;
            }
            set
            {
                _objectLayers = (List<ObjectLayer>)value;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Clear selection elements collections.
        /// </summary>
        protected void _ClearListOfSelectionElements()
        {
            _baseLayersStack.Children.Clear();
            _otherLayersStack.Children.Clear();
            _objectLayersStack.Children.Clear();
        }

        /// <summary>
        /// Clear layers collections.
        /// </summary>
        protected void _ClearLayersCollections()
        {
            _baseLayers.Clear();
            _otherLayers.Clear();
        }

        protected void _InitComponents()
        {
            _btnHeaderButton = this.GetTemplateChild("PART_HeaderButton") as ToggleButton;
            _contentBorder = this.GetTemplateChild("PART_ContentBorder") as Border;
            _otherLayersStack = this.GetTemplateChild("PART_OtherLayersStack") as StackPanel;
            _baseLayersStack = this.GetTemplateChild("PART_BaseLayersStack") as StackPanel;
            _objectLayersStack = this.GetTemplateChild("PART_ObjectLayersStack") as StackPanel;
        }

        protected void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(MapLayersWiget_Loaded);
            this.MouseEnter += new MouseEventHandler(MapLayersWidget_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MapLayersWidget_MouseLeave);
        }

        /// <summary>
        /// Sorts layers by type (base or other)
        /// </summary>
        protected void _SortLayersByType()
        {
            foreach (MapLayer layer in AllLayers)
            {
                if (layer.IsBaseMap)
                    _baseLayers.Add(layer);
                else
                    _otherLayers.Add(layer);
            }
        }

        /// <summary>
        /// Adds radio buttons to control dependency on layers collections. 
        /// </summary>
        protected void _BuildListOfBaseSelectionElements()
        {
            foreach (MapLayer baseLayer in _baseLayers)
            {
                RadioButton rb = new RadioButton();
                rb.Content = baseLayer.Name;

                // if layer is AgsMapLayer selector is enabled if layer's server is authorized
                if (baseLayer is AgsMapLayer)
                    rb.IsEnabled = (((AgsMapLayer)baseLayer).Server.State == AgsServerState.Authorized);
                else
                    rb.IsEnabled = true;

                rb.GroupName = "baseLayers";
                rb.Margin = (Thickness)Application.Current.FindResource("radioButtonThickness");

                rb.Style = (Style)Application.Current.FindResource("MapLayersWidgetRadioButtonStyle");

                _baseLayersStack.Children.Add(rb);

                if (baseLayer == ActiveBaseLayer)
                    rb.IsChecked = true;

                rb.Checked += new RoutedEventHandler(rb_Checked);
                rb.Unchecked += new RoutedEventHandler(rb_Unchecked);
            }
        }
        
        /// <summary>
        /// Adds checkboxes to control dependency on layers collections. 
        /// </summary>
        protected void _BuildListOfOtherSelectionElements()
        {
            foreach (MapLayer otherLayer in _otherLayers)
            {
                CheckBox cb = new CheckBox();
                cb.Content = otherLayer.Name;

                // if layer is AgsMapLayer selector is enabled if layer's server is authorized
                if (otherLayer is AgsMapLayer)
                    cb.IsEnabled = (((AgsMapLayer)otherLayer).Server.State == AgsServerState.Authorized);
                else
                    cb.IsEnabled = true;

                cb.Margin = (Thickness)Application.Current.FindResource("radioButtonThickness");

                cb.Style = (Style)Application.Current.FindResource("CheckBoxInMapLAyersWidgetStyle");

                cb.IsChecked = otherLayer.IsVisible;
                _otherLayersStack.Children.Add(cb);

                cb.Checked += new RoutedEventHandler(cb_Checked);
                cb.Unchecked += new RoutedEventHandler(cb_Unchecked);
            }
        }

        /// <summary>
        /// Adds checkboxes to control dependency on layers collections. 
        /// </summary>
        protected void _BuildListOfObjectSelectionElements()
        {
            foreach (ObjectLayer objectLayer in _objectLayers)
            {
                CheckBox cb = new CheckBox();
                cb.Content = objectLayer.Name;

                cb.Margin = (Thickness)Application.Current.FindResource("radioButtonThickness");

                cb.Style = (Style)Application.Current.FindResource("CheckBoxInMapLAyersWidgetStyle");

                cb.IsChecked = objectLayer.Visible;
                _objectLayersStack.Children.Add(cb);

                cb.Checked += new RoutedEventHandler(cb_Checked);
                cb.Unchecked += new RoutedEventHandler(cb_Unchecked);
            }
        }

        #endregion

        #region Event Handlers

        void cb_Unchecked(object sender, RoutedEventArgs e)
        {
            _ReactOnObjectLayerClicked(sender as CheckBox, false);
        }

        void cb_Checked(object sender, RoutedEventArgs e)
        {
            _ReactOnObjectLayerClicked(sender as CheckBox, true);
        }

        /// <summary>
        /// Helper method to change visibility of object layer
        /// </summary>
        /// <param name="cb">Checked checkbox</param>
        /// <param name="state">New state of visibility</param>
        private void _ReactOnObjectLayerClicked(CheckBox cb, bool state)
        {
            int index = _otherLayersStack.Children.IndexOf(cb);

            if (index > -1)
            {
                // layer is maplayer
                _otherLayers[index].IsVisible = state;
            }
            else
            {
                // layer is object layer
                index = _objectLayersStack.Children.IndexOf(cb);
                _objectLayers[index].Visible = state;

                // save new option state
                if (_objectLayers[index].LayerType == typeof(Zone))
                {
                    App.Current.MapDisplay.ShowZones = state;
                    App.Current.MapDisplay.Save();
                }
                else if (_objectLayers[index].LayerType == typeof(Barrier))
                {
                    App.Current.MapDisplay.ShowBarriers = state;
                    App.Current.MapDisplay.Save();
                }
            }
        }

        void rb_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            int index = _baseLayersStack.Children.IndexOf(rb);

            foreach (MapLayer baseLayer in _baseLayers)
                baseLayer.IsVisible = false;

            _baseLayers[index].IsVisible = true;
            ActiveBaseLayer = _baseLayers[index];
            this.RaiseEvent(new RoutedEventArgs(MapLayersWidget.ActiveBaseLayerEventChanged));
        }

        void rb_Unchecked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            int index = _baseLayersStack.Children.IndexOf(rb);
            if (index >= 0)
                _baseLayers[index].IsVisible = false;
        }

        void MapLayersWiget_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                if (AllLayers != null)
                {
                    _ClearLayersCollections();
                    _SortLayersByType();
                    _isLoaded = true;
                }
            }

            // Set common application base layer.
            ActiveBaseLayer = App.Current.Map.SelectedBaseMapLayer;

            _defaultButtonWidth = _btnHeaderButton.ActualWidth;
        }

        void MapLayersWidget_MouseLeave(object sender, MouseEventArgs e)
        {
            _contentBorder.Visibility = Visibility.Hidden;
            _btnHeaderButton.Width = _defaultButtonWidth;
            _btnHeaderButton.IsChecked = false;
        }

        void MapLayersWidget_MouseEnter(object sender, MouseEventArgs e)
        {
            _ClearListOfSelectionElements();
            _BuildListOfBaseSelectionElements();
            _BuildListOfOtherSelectionElements();
            _BuildListOfObjectSelectionElements();

            _contentBorder.Visibility = Visibility.Visible;
            _btnHeaderButton.IsChecked = true;
            _btnHeaderButton.Width = _contentBorder.ActualWidth;
        }

        #endregion

        #region Private fields

        private ToggleButton _btnHeaderButton;
        private Border _contentBorder;

        private StackPanel _otherLayersStack;
        private StackPanel _baseLayersStack;
        private StackPanel _objectLayersStack;

        private List<MapLayer> _baseLayers = new List<MapLayer>();
        private List<MapLayer> _otherLayers = new List<MapLayer>();
        private List<ObjectLayer> _objectLayers = new List<ObjectLayer>();

        private double _defaultButtonWidth;
        private bool _isLoaded;

        #endregion
    }
}
