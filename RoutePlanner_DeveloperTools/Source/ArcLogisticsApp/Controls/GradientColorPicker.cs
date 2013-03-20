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
using System.Drawing;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Color picker control
    /// </summary>
    [TemplatePart(Name = "PART_SolidColorGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_SpectrumGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_SpectrumSlider", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SelectionEllipse", Type = typeof(Ellipse))]

    internal class GradientColorPicker : Control, INotifyPropertyChanged
    {
        #region Constants

        public readonly string GRADIENT_COLOR_PROPERTY_NAME = "GradientColor";

        #endregion

        #region Constructors

        static GradientColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GradientColorPicker), new FrameworkPropertyMetadata(typeof(GradientColorPicker)));
        }

        #endregion

        #region Override Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
            _CreateSpectrumColors();
            _SpectrumGrid.Background = _spectrumGradientBrush;
            _ApplyColors();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void _NotifyPropertyChanged(string propName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #region Public Propeties

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
                "GradientColor", typeof(Color), typeof(GradientColorPicker));

        public Color GradientColor
        {
            get { return (Color)GetValue(ColorProperty); }
            set 
            { 
                SetValue(ColorProperty, value);
                _NotifyPropertyChanged(GRADIENT_COLOR_PROPERTY_NAME);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method returns color by point parameter
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Color _GetColorFromPoint(System.Windows.Point point)
        {
            // create target bitmap from control
            RenderTargetBitmap bmp = new RenderTargetBitmap((Int32)this.ActualWidth, (Int32)this.ActualHeight, POINTS_PER_INCH, POINTS_PER_INCH, System.Windows.Media.PixelFormats.Pbgra32);
            bmp.Render(this);

            MemoryStream stm = new MemoryStream();

            Debug.Assert(stm != null);

            // convert target bitmap to png and write it to stream
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(stm);

            // load bitmap from stream
            Bitmap bp = new Bitmap(stm);

            Debug.Assert(point.X >= 0);
            Debug.Assert(point.Y >= 0);


            if ((Int32)point.X + _SelectionEllipse.Height / 2 <= Height)
                point.X = point.X + _SelectionEllipse.Height / 2;

            if ((Int32)point.Y < _SelectionEllipse.Height/2)
                point.Y = _SelectionEllipse.Height / 2;

            Color color = bp.GetPixel((Int32)point.X, (Int32)point.Y);

            stm.Close();
            return color;
        }

        /// <summary>
        /// Method sets colors to components after first time loading control
        /// </summary>
        private void _ApplyColors()
        {
            int i = (int)_SpectrumSlider.Value;
            _SolidColorGrid.Background = new System.Windows.Media.SolidColorBrush(
                                                System.Windows.Media.Color.FromArgb(
                                                    _spectrumColorsCollection[i].A,
                                                    _spectrumColorsCollection[i].R,
                                                    _spectrumColorsCollection[i].G,
                                                    _spectrumColorsCollection[i].B));
        }

        private void _InitEventHandlers()
        {
            _SpectrumSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(_SpectrumSlider_ValueChanged);
            _SolidColorGrid.MouseLeftButtonDown += new MouseButtonEventHandler(_SolidColorGrid_MouseLeftButtonDown);
            _SolidColorGrid.MouseLeftButtonUp += new MouseButtonEventHandler(_SolidColorGrid_MouseLeftButtonUp);
            _SelectionEllipse.MouseLeftButtonDown += new MouseButtonEventHandler(_SelectionEllipse_MouseLeftButtonDown);
            _SolidColorGrid.MouseMove += new MouseEventHandler(_SolidColorGrid_MouseMove);
        }

        private void _MoveEllipse(System.Windows.Point position)
        {
            System.Windows.Point newPosition = position;

            if (position.X >= (_SolidColorGrid.ActualWidth - _SelectionEllipse.Width))
                newPosition.X = _SolidColorGrid.ActualWidth - _SelectionEllipse.Width / 2;
            if (position.Y > (_SolidColorGrid.ActualHeight - _SelectionEllipse.Height))
                newPosition.Y = _SolidColorGrid.ActualHeight - _SelectionEllipse.Height / 2;
            if (position.X < _SelectionEllipse.Width / 2)
                newPosition.X = _SelectionEllipse.Width / 2;
            if (position.Y < _SelectionEllipse.Height / 2)
                newPosition.Y = _SelectionEllipse.Height / 2;

            _SelectionEllipse.Margin = new Thickness(newPosition.X - _SelectionEllipse.Width / 2, newPosition.Y - _SelectionEllipse.Height / 2, 0, 0);
        }

        /// <summary>
        /// Method inits all components
        /// </summary>
        private void _InitComponents()
        {
            _SolidColorGrid = (Grid)this.GetTemplateChild("PART_SolidColorGrid");
            _SpectrumGrid = (Grid)this.GetTemplateChild("PART_SpectrumGrid");
            _SpectrumSlider = (Slider)this.GetTemplateChild("PART_SpectrumSlider");
            _SelectionEllipse = (Ellipse)this.GetTemplateChild("PART_SelectionEllipse");
            _SpectrumSlider.Value = SPECTRUM_COLORS_COLLECTION_SIZE;
        }

        /// <summary>
        /// Method inits all spectrum colors and spectrum brush
        /// </summary>
        private void _CreateSpectrumColors()
        {
            _spectrumGradientBrush.StartPoint = new System.Windows.Point(0.5, 0);
            _spectrumGradientBrush.EndPoint = new System.Windows.Point(0.5, 1);
            _spectrumGradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb(FULL_RGB_COLOR_SATURATION,
                                                                                FULL_RGB_COLOR_SATURATION,
                                                                                NULL_RGB_COLOR_SATURATION,
                                                                                NULL_RGB_COLOR_SATURATION),
                                                                                NULL_RGB_COLOR_SATURATION));

            int A = FULL_RGB_COLOR_SATURATION;
            int R = NULL_RGB_COLOR_SATURATION;
            int G = NULL_RGB_COLOR_SATURATION;
            int B = NULL_RGB_COLOR_SATURATION;

            int i = NULL_RGB_COLOR_SATURATION;

            // red-yellow gradient
            while (i < YELLOW_COLORS_SPECTRUM_REGION)
            {
                R = FULL_RGB_COLOR_SATURATION;
                G = i;
                B = NULL_RGB_COLOR_SATURATION;
                i++;
                _spectrumColorsCollection[SPECTRUM_COLORS_COLLECTION_SIZE - i] = Color.FromArgb(A, R, G, B);
            }
            // add yellow point to gradient
            _spectrumGradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B), 0.2));


            // yellow - green gardient
            while (i >= YELLOW_COLORS_SPECTRUM_REGION && i < GREEN_COLORS_SPECTRUM_REGION)
            {
                R = GREEN_COLORS_SPECTRUM_REGION - i;
                G = FULL_RGB_COLOR_SATURATION;
                B = NULL_RGB_COLOR_SATURATION;
                i++;
                _spectrumColorsCollection[SPECTRUM_COLORS_COLLECTION_SIZE - i] = Color.FromArgb(A, R, G, B);
            }
            // add green point to gradient
            _spectrumGradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B), 0.4));

            // green - azure gradient
            while (i >= GREEN_COLORS_SPECTRUM_REGION && i < AZURE_COLORS_SPECTRUM_REGION)
            {
                R = NULL_RGB_COLOR_SATURATION;
                G = FULL_RGB_COLOR_SATURATION;
                B = i - GREEN_COLORS_SPECTRUM_REGION;
                i++;
                _spectrumColorsCollection[SPECTRUM_COLORS_COLLECTION_SIZE - i] = Color.FromArgb(A, R, G, B);
            }
            // add azure point to gradient
            _spectrumGradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B), 0.6));


            // azure - blue gradient
            while (i >= AZURE_COLORS_SPECTRUM_REGION && i < BLUE_COLORS_SPECTRUM_REGION)
            {
                R = NULL_RGB_COLOR_SATURATION;
                G = BLUE_COLORS_SPECTRUM_REGION - i;
                B = FULL_RGB_COLOR_SATURATION;
                i++;
                _spectrumColorsCollection[SPECTRUM_COLORS_COLLECTION_SIZE - i] = Color.FromArgb(A, R, G, B);
            }
            // add blue point to gradient
            _spectrumGradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B), 0.8));


            // blue-violet gradient
            while (i >= BLUE_COLORS_SPECTRUM_REGION && i < VIOLET_COLORS_SPECTRUM_REGION)
            {
                R = i - BLUE_COLORS_SPECTRUM_REGION;
                G = NULL_RGB_COLOR_SATURATION;
                B = FULL_RGB_COLOR_SATURATION;
                i++;
                _spectrumColorsCollection[SPECTRUM_COLORS_COLLECTION_SIZE - i] = Color.FromArgb(A, R, G, B);
            }
            // add violet point to gradient
            _spectrumGradientBrush.GradientStops.Add(new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B), 1));
        }

        #endregion 

        #region Event Handlers

        private void _SolidColorGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point point = e.GetPosition(this);
            _MoveEllipse(e.GetPosition(this));
            GradientColor = _GetColorFromPoint(point);
        }

        private void _SpectrumSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _ApplyColors();
            System.Windows.Point point = new System.Windows.Point(_SelectionEllipse.Margin.Left,
                                                                  _SelectionEllipse.Margin.Top);
            GradientColor = _GetColorFromPoint(new System.Windows.Point(point.X, point.Y));
        }

        private void _SolidColorGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                System.Windows.Point point =  e.GetPosition(this);
                _MoveEllipse(point);
               // GradientColor = _GetColorFromPoint(new System.Windows.Point(point.X, point.Y));
                GradientColor = _GetColorFromPoint(new System.Windows.Point(_SelectionEllipse.Margin.Left, _SelectionEllipse.Margin.Top));
            }
        }

        private void _SelectionEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
        }

        private void _SolidColorGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
                _isDragging = false;
        }

        #endregion

        #region Private Fields

        private const int YELLOW_COLORS_SPECTRUM_REGION = 255;
        private const int GREEN_COLORS_SPECTRUM_REGION = 510;
        private const int AZURE_COLORS_SPECTRUM_REGION = 765;
        private const int BLUE_COLORS_SPECTRUM_REGION = 1020;
        private const int VIOLET_COLORS_SPECTRUM_REGION = 1274;

        private const int SPECTRUM_COLORS_COLLECTION_SIZE = 1274;

        private const int FULL_RGB_COLOR_SATURATION = 255;
        private const int NULL_RGB_COLOR_SATURATION = 0;
        private const int POINTS_PER_INCH = 96;

        private Color[] _spectrumColorsCollection = new Color[SPECTRUM_COLORS_COLLECTION_SIZE];

        private System.Windows.Media.LinearGradientBrush _spectrumGradientBrush = new System.Windows.Media.LinearGradientBrush();


        private bool _isDragging = false;

        private Grid _SolidColorGrid;
        private Grid _SpectrumGrid;
        private Slider _SpectrumSlider;
        private Ellipse _SelectionEllipse;

        #endregion
    }
}
