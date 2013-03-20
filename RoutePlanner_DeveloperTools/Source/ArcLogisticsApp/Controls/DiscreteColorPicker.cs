using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Panel with discret colors set
    /// </summary>

    [TemplatePart(Name = "PART_ColorsContainer", Type = typeof(ListBox))]
    internal class DiscreteColorPicker : Control, INotifyPropertyChanged
    {
        #region Public Constants

        public readonly string DISCRETE_COLOR_PROPERTY_NAME = "DiscreteColor";

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        static DiscreteColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DiscreteColorPicker), new FrameworkPropertyMetadata(typeof(DiscreteColorPicker)));
        }

        /// <summary>
        /// Applies template to control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            this.Loaded += new RoutedEventHandler(_OnLoaded);
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raises when any porperty changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises event about property changed.
        /// </summary>
        /// <param name="propName">Changed porperty name.</param>
        protected virtual void _NotifyPropertyChanged(string propName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        /// <summary>
        /// Raises when user selects necessary color.
        /// </summary>
        public event EventHandler ColorSelected;

        /// <summary>
        /// Raises event about color was selected.
        /// </summary>
        protected virtual void _NotifyColorSelected()
        {
            if (null != ColorSelected)
                ColorSelected(this, EventArgs.Empty);
        }

        #endregion

        #region Public Propeties

        /// <summary>
        /// Gets/sets selected color.
        /// </summary>
        public Color DiscreteColor
        {
            get
            {
                return _predefinedColor;
            }
            set
            {
                _predefinedColor = value;
                _NotifyPropertyChanged(DISCRETE_COLOR_PROPERTY_NAME);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method updates colors set
        /// </summary>
        public void UpdateColors()
        {
            _FillColorsContainer();
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes all components.
        /// </summary>
        private void _InitComponents()
        {
            _ColorsContainer = (ListBox)this.GetTemplateChild("PART_ColorsContainer");
            _ColorsContainer.SelectionChanged += new SelectionChangedEventHandler(_SelectionChanged);
        }

        /// <summary>
        /// Creates collection of "color points" and set it into container.
        /// </summary>
        private void _FillColorsContainer()
        {
            _ColorsContainer.Items.Clear();
            _colorsSet = RouteColorManager.Instance.ColorsSet.ToArray();

            foreach (Color color in _colorsSet)
            {
                Button colorPoint = new Button();
                colorPoint.Style = (Style)Application.Current.FindResource("DiscreteColorPickerColorStyle");
                colorPoint.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));
                colorPoint.MouseEnter += new MouseEventHandler(_ColorPointMouseEnter);
                colorPoint.MouseLeave += new MouseEventHandler(_ColorPointMouseLeave);
                colorPoint.Click += new RoutedEventHandler(_ColorPointClick);
                _ColorsContainer.Items.Add(colorPoint);
            }

            _ColorsContainer.Focus();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Creates color points.
        /// </summary>
        /// <param name="sender">Discrete Color Picker.</param>
        /// <param name="e">Event args.</param>
        private void _OnLoaded(object sender, RoutedEventArgs e)
        {
            _FillColorsContainer();
        }

        /// <summary>
        /// Changes selected color.
        /// </summary>
        /// <param name="sender">Color point.</param>
        /// <param name="e">Event args.</param>
        private void _ColorPointClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            Debug.Assert(sender != null);

            int index = _ColorsContainer.Items.IndexOf(sender);

            if (index >= 0)
            {
                DiscreteColor = _colorsSet[index];
                _NotifyColorSelected();
            }
        }

        /// <summary>
        /// Changes selected color by mouse enter.
        /// </summary>
        /// <param name="sender">Color point.</param>
        /// <param name="e">Mouse event args.</param>
        private void _ColorPointMouseEnter(object sender, MouseEventArgs e)
        {
            Debug.Assert(_ColorsContainer.Items != null);

            if (_ColorsContainer.Items.IndexOf((Control)sender) >= 0)
                DiscreteColor = _colorsSet[_ColorsContainer.Items.IndexOf((Control)sender)];
        }

        /// <summary>
        /// Returns old selected color by mouse leave.
        /// </summary>
        /// <param name="sender">Color point.</param>
        /// <param name="e">Mouse event args.</param>
        private void _ColorPointMouseLeave(object sender, MouseEventArgs e)
        {
            DiscreteColor = Color.Transparent;
        }

        /// <summary>
        /// Changes selected color when selection changes in color points list.
        /// </summary>
        /// <param name="sender">Color points list.</param>
        /// <param name="e">Event args.</param>
        private void _SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_ColorsContainer.SelectedIndex < 0)
                return;

            DiscreteColor = _colorsSet[_ColorsContainer.SelectedIndex];
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Collection of discrete colors.
        /// </summary>
        private Color[] _colorsSet;

        /// <summary>
        /// List box with color point controls.
        /// </summary>
        private ListBox _ColorsContainer;

        /// <summary>
        /// Initial color.
        /// </summary>
        private Color _predefinedColor;

        #endregion
    }
}
