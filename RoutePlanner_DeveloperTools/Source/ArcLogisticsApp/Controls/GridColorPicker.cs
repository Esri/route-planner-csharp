using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ESRI.ArcLogistics.App.Controls;
using System.Windows.Controls.Primitives;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_ColorPicker", Type = typeof(ColorPicker))]
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_OpenButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]

    // Color picker for Xceed grid color cells
    internal class GridColorPicker : ComboBox
    {
        static GridColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridColorPicker), new FrameworkPropertyMetadata(typeof(GridColorPicker)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();           
        }

        #region Public Properties

        public static readonly DependencyProperty FillColorProperty =
                DependencyProperty.Register("FillColor", typeof(SolidColorBrush), typeof(GridColorPicker));

        /// <summary>
        /// gets/sets cell fill color 
        /// </summary>
        public SolidColorBrush FillColor
        {
            get
            {
                return (SolidColorBrush)GetValue(FillColorProperty);
            }
            set
            {
                SetValue(FillColorProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedColorProperty =
                DependencyProperty.Register("SelectedColor", typeof(System.Drawing.Color), typeof(GridColorPicker));

        /// <summary>
        /// gets/sets current selected color 
        /// </summary>
        public System.Drawing.Color SelectedColor
        {
            get
            {
                return (System.Drawing.Color)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
                _SetFillColor();
            }
        }

        #endregion

        #region Private Methods

        private void _InitComponents()
        {
            _ColorPicker = (ColorPicker)this.GetTemplateChild("PART_ColorPicker");
            _PopupPanel = (Popup)this.GetTemplateChild("PART_PopupPanel");
            _TopLevelGrid = this.GetTemplateChild("PART_TopLevelGrid") as Grid;
        }

        private void _InitEventHandlers()
        {
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            _ColorPicker.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_ColorPicker_PropertyChanged);
            this.Loaded += new RoutedEventHandler(GridColorPicker_Loaded);
            _ColorPicker.SelectionCancelled += new EventHandler(_ColorPicker_SelectionCancelled);
            this.PreviewKeyDown += new KeyEventHandler(_PreviewKeyDown);
            _PopupPanel.Closed += new EventHandler(_PopupPanel_Closed);
            _PopupPanel.Opened += new EventHandler(_PopupPanel_Opened);
        }

        /// <summary>
        /// Method sets color presented in cell
        /// </summary>
        private void _SetFillColor()
        {
            FillColor  = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
               SelectedColor.A, SelectedColor.R, SelectedColor.G, SelectedColor.B));
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Method closes popup if it is opened and user clicked outside the control.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If mouse is clicked outside the control and popup is shown - we need to close it, 
            // so popup will lose its focus and mouse left button down event will come to grid.
            if (!_TopLevelGrid.IsMouseOver && _PopupPanel.IsOpen)
                _PopupPanel.IsOpen = false;
        }

        private void _PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _PopupPanel.StaysOpen = false;
            if ((e.Key == Key.Escape) && _PopupPanel.IsOpen)
            {
                this.IsDropDownOpen = false;
            }
            else if ((e.Key == Key.Enter || e.Key == Key.Tab) && _PopupPanel.IsOpen)
            {
                // If any color was selected in ColorPicker - need to set it as current.
                if (_ColorPicker.CurrentColor.A != Colors.Transparent.A) 
                    SelectedColor = _ColorPicker.CurrentColor;

                this.IsDropDownOpen = false;
            }
        }

        private void _ColorPicker_SelectionCancelled(object sender, EventArgs e)
        {
            _SetFillColor();
            this.IsDropDownOpen = false;
        }

        private void GridColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            //NOTE: set property IsDropDownOpen to true for open control when it was loaded
            this.IsDropDownOpen = true;
            _initialColor = SelectedColor;
            _SetFillColor();
        }

        private void _ColorPicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(SELECTED_COLOR_PROPERTY_NAME))
            {
                SelectedColor = ((ColorPicker)sender).SelectedColor;
                _initialColor = ((ColorPicker)sender).SelectedColor;
                this.IsDropDownOpen = false;
            }
            else if (e.PropertyName.Equals(CURRENT_COLOR_PROPERTY_NAME))
            {
                _initialColor = SelectedColor;
                System.Drawing.Color color;

                if (!((ColorPicker)sender).CurrentColor.Equals(System.Drawing.Color.Transparent))
                    color = ((ColorPicker)sender).CurrentColor;
                else
                    color = _initialColor;

                FillColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        private void _PopupPanel_Closed(object sender, EventArgs e)
        {
            // NOTE : set focus to parent cell for support arrow keys navigation
            UIElement cell = XceedVisualTreeHelper.GetCellByEditor(this);
            if (cell != null)
                Keyboard.Focus(cell);
        }

        /// <summary>
        /// Set correct position of Popup panel when it opens.
        /// </summary>
        /// <param name="sender">Popup panel.</param>
        /// <param name="e">Event args.</param>
        private void _PopupPanel_Opened(object sender, EventArgs e)
        {
            PopupPositionSynchronizer synchronizer = new PopupPositionSynchronizer(this, _PopupPanel);

            // Set popup's position.
            synchronizer.PositionPopupBelowCellEditor();
        }

        #endregion

        #region Private Fields

        private const string SELECTED_COLOR_PROPERTY_NAME = "SelectedColor";
        private const string CURRENT_COLOR_PROPERTY_NAME = "CurrentColor";

        ColorPicker _ColorPicker;
        Popup _PopupPanel;

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _TopLevelGrid;

        System.Drawing.Color _initialColor;

        #endregion
    }
}
