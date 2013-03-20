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
using System.ComponentModel;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    internal partial class ColorPicker : Border, INotifyPropertyChanged
    {
        #region Constructors

        public ColorPicker()
        {
            InitializeComponent();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Events

        public event EventHandler SelectionCancelled;

        #endregion

        #region Public Properties

        /// <summary>
        /// gets/sets selected color 
        /// </summary>
        public System.Drawing.Color CurrentColor
        {
            get
            {
                return _currentColor;
            }
            set
            {
                _currentColor = value;
                _NotifyPropertyChanged(CURRENT_COLOR_PROPERTY_NAME);
            }
        }

        /// <summary>
        /// gets/sets selected color 
        /// </summary>
        public System.Drawing.Color SelectedColor
        {
            get
            {
                return _selectedColor;
            }
            set
            {
                _selectedColor = value;
                _NotifyPropertyChanged(SELECTED_COLOR_PROPERTY_NAME);
            }
        }

        #endregion

        #region Private Methods

        private void _NotifyPropertyChanged(string propName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        private void _NotifySelectionCancelled()
        {
            if (null != SelectionCancelled)
                SelectionCancelled(this, EventArgs.Empty);
        }

        #endregion

        #region Event Handlers

        private void discreteColorPicker_ColorSelected(object sender, EventArgs e)
        {
            SelectedColor = ((DiscreteColorPicker)sender).DiscreteColor;
        }

        private void MoreColorsButton_Click(object sender, RoutedEventArgs e)
        {
            //NOTE: set gradient color picker visible and discrete color picker - hidden
            // unable to use "Visibility" property because in that case drop-down in GridColorPicker works incorrect
            gradientColorPickerPanel.Height = _gradientColorPickerHeight;
            discreteColorPickerPanel.Height = 0;
        }

        private void LessColorsButton_Click(object sender, RoutedEventArgs e)
        {
            //NOTE: set discrete color picker visible and gradient color picker - hidden
            // unable to use "Visibility" property because in that case drop-down in GridColorPicker works incorrect
            discreteColorPickerPanel.Height = _discreteColorPickerHeight;
            gradientColorPickerPanel.Height = 0;
        }

        private void gradientColorPicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(gradientColorPicker.GRADIENT_COLOR_PROPERTY_NAME))
                CurrentColor = ((GradientColorPicker)sender).GradientColor;
        }

        private void DiscreteColorPicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(discreteColorPicker.DISCRETE_COLOR_PROPERTY_NAME))
                CurrentColor = ((DiscreteColorPicker)sender).DiscreteColor;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _NotifySelectionCancelled();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = gradientColorPicker.GradientColor;
            RouteColorManager.Instance.AddUserColor(SelectedColor); // add user defined color to custom colors
        }

        private void ClearCustomColorsButton_Click(object sender, RoutedEventArgs e)
        {
            RouteColorManager.Instance.ClearUserColors();
            discreteColorPicker.UpdateColors();
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            //NOTE: when control's loaded we're remember heights of panels
            _gradientColorPickerHeight = (gradientColorPickerPanel.ActualHeight != 0) ? gradientColorPickerPanel.ActualHeight : _gradientColorPickerHeight;
            _discreteColorPickerHeight = (discreteColorPickerPanel.ActualHeight != 0) ? discreteColorPickerPanel.ActualHeight : _discreteColorPickerHeight;

            //NOTE: Hide gradientColorPickerPanel if necessary (if discreteColorPickerPanel is visible)
            if (discreteColorPickerPanel.Height != 0)
                gradientColorPickerPanel.Height = 0;
        }

        #endregion

        #region Private Fields

        private const string SELECTED_COLOR_PROPERTY_NAME = "SelectedColor";
        private const string CURRENT_COLOR_PROPERTY_NAME = "CurrentColor";

        private System.Drawing.Color _selectedColor;
        private System.Drawing.Color _currentColor;

        // fields to save height of color picker's parts for support different control states
        private double _gradientColorPickerHeight;
        private double _discreteColorPickerHeight;

        #endregion
    }
}
