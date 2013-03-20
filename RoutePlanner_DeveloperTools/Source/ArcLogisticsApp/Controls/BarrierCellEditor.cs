using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Barrier type editor internal logic.
    /// </summary>
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_CellLabel", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_OpenButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_BarrierEditor", Type = typeof(BarrierEditor))]
    internal class BarrierCellEditor : ComboBox
    {
        #region Constructors & override methods

        /// <summary>
        /// Create a new instance of the <c>DaysEditor</c> class.
        /// </summary>
        static BarrierCellEditor()
        {
            var typeofMetadata = new FrameworkPropertyMetadata(typeof(BarrierCellEditor));
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BarrierCellEditor), typeofMetadata);
        }

        /// <summary>
        /// Applyes template. Initialites control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _InitParts();
            _InitEventHandlers();
            _InitVisibility();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Identifies the BarrierEffectProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty BarrierEffectProperty =
            DependencyProperty.Register("BarrierEffect", typeof(BarrierEffect), typeof(BarrierCellEditor),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnValueChanged)));

        /// <summary>
        /// Gets/sets barrier type.
        /// </summary>
        public BarrierEffect BarrierEffect
        {
            get { return (BarrierEffect)GetValue(BarrierEffectProperty); }
            set
            {
                if (value != null)
                    SetValue(BarrierEffectProperty, value);
            }
        }

        #endregion

        #region Private static methods

        /// <summary>
        /// React on Value property changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((BarrierCellEditor)obj)._OnValueChanged();
        }

        #endregion
        
        #region Private methods

        /// <summary>
        /// If it is polyline barrier then hide combobox and show textblock.
        /// </summary>
        private void _InitVisibility()
        {
            // Get Barrier.Geometry type.
            Cell cell = XceedVisualTreeHelper.GetCellByEditor(this);
            Row row = XceedVisualTreeHelper.FindParent<Row>(cell);
            Barrier barrier = row.DataContext as Barrier;

            // If barrier is polyline or have no geometry hide edit control.
            if (barrier.Geometry == null || barrier.Geometry is ESRI.ArcLogistics.Geometry.Polyline)
            {
                _border.Visibility = System.Windows.Visibility.Hidden;
                _popupPanel.Child = null;
                _textBlock.Visibility = System.Windows.Visibility.Visible;
                if (barrier.Geometry is ESRI.ArcLogistics.Geometry.Polyline)
                    _textBlock.Text = (string)App.Current.FindResource("BlockTravelString");
            }
        }

        /// <summary>
        /// Inits parts of control.
        /// </summary>
        private void _InitParts()
        {
            _topLevelGrid = this.GetTemplateChild("PART_TopLevelGrid") as Grid;
            _cellLabel = this.GetTemplateChild("PART_CellLabel") as TextBlock;
            _openButton = this.GetTemplateChild("PART_OpenButton") as ToggleButton;
            _popupPanel = this.GetTemplateChild("PART_PopupPanel") as Popup;
            _barrierEditor = this.GetTemplateChild("PART_BarrierEditor") as BarrierEditor;
            _textBlock = this.GetTemplateChild("PART_TextBox") as TextBlock;
            _border = this.GetTemplateChild("PART_Border") as Border;
        }

        /// <summary>
        /// Method sets handlers to events.
        /// </summary>
        private void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(_EditorLoaded);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);

            _popupPanel.Opened += new EventHandler(_PopupPanelOpened);
        }

        /// <summary>
        /// Method open popup when control was loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _EditorLoaded(object sender, RoutedEventArgs e)
        {
            if (_barrier.Geometry is ESRI.ArcLogistics.Geometry.Point ||
                _barrier.Geometry is ESRI.ArcLogistics.Geometry.Polygon ||
                _barrier.Geometry is ESRI.ArcLogistics.Geometry.Polyline)
            {
                this.IsDropDownOpen = true;

                _barrierEditor.Barrier = _barrier;
            }
        }

        /// <summary>
        /// React on popup panel opened.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PopupPanelOpened(object sender, EventArgs e)
        {
            if (_barrier.Geometry is ESRI.ArcLogistics.Geometry.Point ||
                _barrier.Geometry is ESRI.ArcLogistics.Geometry.Polygon)
            {
                _cellLabel.Text = CommonHelpers.ConvertBarrierEffect(_barrier);

                _barrierEditor.Barrier = _barrier;

                PopupPositionSynchronizer synchronizer = new PopupPositionSynchronizer(this, _popupPanel);

                // Set popup's position.
                synchronizer.PositionPopupBelowCellEditor();
            }
        }

        /// <summary>
        /// Closes popup if it is opened and user clicked outside the control.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If mouse is clicked outside the control and popup is shown - we need to close it,
            // so popup will lose its focus and mouse left button down event will come to grid.
            if (!_topLevelGrid.IsMouseOver && _popupPanel.IsOpen)
                _popupPanel.IsOpen = false;
        }

        /// <summary>
        /// React on numeric field changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _TextboxTextChanged(object sender, TextChangedEventArgs e)
        {
            _SaveState();
        }

        /// <summary>
        /// Save control state.
        /// </summary>
        private void _SaveState()
        {
            _cellLabel.Text = CommonHelpers.ConvertBarrierEffect(_barrier);
        }

        /// <summary>
        /// React on dependency property changing.
        /// </summary>
        private void _OnValueChanged()
        {
            if (_barrier != null)
            {
                _barrier.PropertyChanged -= new PropertyChangedEventHandler(_BarrierPropertyChanged);
            }

            Cell cell = XceedVisualTreeHelper.GetCellByEditor(this);
            Row row = XceedVisualTreeHelper.FindParent<Row>(cell);

            _barrier = row.DataContext as Barrier;

            if (_barrierEditor != null)
            {
                _barrierEditor.Barrier = _barrier;
            }

            if (_barrier != null)
            {
                _barrier.PropertyChanged += new PropertyChangedEventHandler(_BarrierPropertyChanged);
            }
        }

        /// <summary>
        /// React on barrier property changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _BarrierPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(Barrier.PropertyNameGeometry) ||
                e.PropertyName.Equals(Barrier.PropertyNameBarrierEffect) && _barrier.Geometry != null)
            {
                _SetControlState();
            }
        }

        /// <summary>
        /// Set control state.
        /// </summary>
        private void _SetControlState()
        {
            _cellLabel.Text = CommonHelpers.ConvertBarrierEffect(_barrier);
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _topLevelGrid;

        /// <summary>
        /// Cell editor text.
        /// </summary>
        private TextBlock _cellLabel;

        /// <summary>
        /// Editor popup panel.
        /// </summary>
        private ToggleButton _openButton;

        /// <summary>
        /// Editor popup panel.
        /// </summary>
        private Popup _popupPanel;

        /// <summary>
        /// Internal barrier editor.
        /// </summary>
        private BarrierEditor _barrierEditor;

        /// <summary>
        /// Edited barrier.
        /// </summary>
        private Barrier _barrier;

        /// <summary>
        /// Textblock for message.
        /// </summary>
        TextBlock _textBlock;
        
        /// <summary>
        /// Border with CellLabel and togle button.
        /// </summary>
        Border _border; 

        #endregion
    }
}
