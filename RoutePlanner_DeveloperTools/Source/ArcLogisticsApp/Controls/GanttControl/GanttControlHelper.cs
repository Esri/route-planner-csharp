using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Converters;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class contains helper methods for GanttControl.
    /// </summary>
    internal static class GanttControlHelper
    {
        #region Public Static Properties

        /// <summary>
        /// Gets height of gantt item.
        /// </summary>
        public static double ItemHeight
        {
            get { return (double)App.Current.FindResource(ITEM_HEIGHT_RESOURCE_NAME); }
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Returns drawing pen.
        /// <remarks>We are use empty pen only like parameter of method Draw().</remarks>
        /// </summary>
        /// <returns>Pen.</returns>
        public static Pen GetPen()
        {
            if (_pen == null)
            {
                _pen = new Pen();
                _pen = (Pen)_pen.GetCurrentValueAsFrozen();
            }

            return _pen;
        }

        /// <summary>
        /// Method returns SolidColorBrush to fill gantt item element.
        /// </summary>
        /// <param name="parentColor">Parent color.</param>
        /// <returns>Fill brush.</returns>
        public static SolidColorBrush GetFillBrush(System.Drawing.Color parentColor)
        {
            if (_fillBrushes.ContainsKey(parentColor))
                return _fillBrushes[parentColor];

            // If hashed fill brush not exist in collection - need to hash it.
            SolidColorBrush hashedFillBrush = _GetFillBrush(parentColor);
            _fillBrushes.Add(parentColor, hashedFillBrush);

            return hashedFillBrush;
        }

        /// <summary>
        /// Returns locked brush.
        /// </summary>
        /// <param name="parentColor">Parent color.</param>
        /// <returns>Locked brush.</returns>
        public static SolidColorBrush GetLockedBrush(System.Drawing.Color parentColor)
        {
            if (_lockedBrushes.ContainsKey(parentColor))
                return _lockedBrushes[parentColor];

            // If hashed fill brush == null (not exist in collection) - need to hash it.
            SolidColorBrush hashedLockedBrush = _GetLockedBrush(parentColor);
            _lockedBrushes.Add(parentColor, hashedLockedBrush);

            return hashedLockedBrush;
        }

        /// <summary>
        /// Method returns SolidColorBrush to dragged over empty gantt item element.
        /// </summary>
        /// <param name="parentColor">Parent color.</param>
        /// <returns>Fill brush.</returns>
        public static Brush GetEmptyElementDragOverFillBrush(System.Drawing.Color parentColor)
        {
            if (_emptyFillBrushes.ContainsKey(parentColor))
                return _emptyFillBrushes[parentColor];

            Brush emptyFillBrush = _GetEmptyFillBrush(parentColor);
            _emptyFillBrushes.Add(parentColor, emptyFillBrush);

            return emptyFillBrush;
        }

        /// <summary>
        /// Converts original color to grayscale brush with same brightness.
        /// </summary>
        /// <param name="originalColor">Original color.</param>
        /// <returns>Grayscale brush.</returns>
        public static SolidColorBrush GetGrayscaleBrush(System.Drawing.Color originalColor)
        {
            if (_grayscaleBrushes.ContainsKey(originalColor))
                return _grayscaleBrushes[originalColor];

            SolidColorBrush grayscaleBrush = _GetGrayscaleBrush(originalColor);
            _grayscaleBrushes.Add(originalColor, grayscaleBrush);

            return grayscaleBrush;
        }

        /// <summary>
        /// Returns violated brush.
        /// </summary>
        /// <returns>Violated brush.</returns>
        public static ImageBrush GetViolatedBrush()
        {
            if (_violatedBrush == null)
            {
                _violatedBrush = (ImageBrush)Application.Current.FindResource(VIOLATED_ICON_SOURCE_NAME);
                _violatedBrush = (ImageBrush)_violatedBrush.GetCurrentValueAsFrozen();
            }
            
            return _violatedBrush;
        }

        /// <summary>
        /// Returns selected brush.
        /// </summary>
        /// <returns>Selected brush.</returns>
        public static Brush GetSelectedBrush()
        {
            if (_selectionBrush == null)
            {
                _selectionBrush = (SolidColorBrush)Application.Current.FindResource(SELECTION_COLOR_NAME);
                _selectionBrush = (SolidColorBrush)_selectionBrush.GetCurrentValueAsFrozen();
            }

            return _selectionBrush;
        }

        /// <summary>
        /// Returns break brush.
        /// </summary>
        /// <returns>Break brush.</returns>
        public static Brush GetBreakBrush()
        {
            if (_breakBrush == null)
            {
                _breakBrush = (SolidColorBrush)Application.Current.FindResource(BREAK_COLOR_SOURCE_NAME);
                _breakBrush = (SolidColorBrush)_breakBrush.GetCurrentValueAsFrozen();
            }

            return _breakBrush;
        }

        /// <summary>
        /// Returns location brush.
        /// </summary>
        /// <returns>Location brush.</returns>
        public static Brush GetLocationBrush()
        {
            if (_locationBrush == null)
            {
                _locationBrush = (SolidColorBrush)Application.Current.FindResource(LOCATION_COLOR_SOURCE_NAME);
                _locationBrush = (SolidColorBrush)_locationBrush.GetCurrentValueAsFrozen();
            }

            return _locationBrush;
        }

        /// <summary>
        /// Returns drag over brush.
        /// </summary>
        /// <returns>Drag over brush.</returns>
        public static Brush GetDragOverBrush()
        {
            if (_dragOverBrush == null)
            {
                _dragOverBrush = (SolidColorBrush)Application.Current.FindResource(DRAG_OVER_COLOR_NAME);
                _dragOverBrush = (SolidColorBrush)_dragOverBrush.GetCurrentValueAsFrozen();
            }
            
            return _dragOverBrush;
        }

        /// <summary>
        /// Gets shadow effect brush.
        /// </summary>
        /// <returns>Returns shadow effect brush.</returns>
        public static Brush GetShadowEffectBrush()
        {
            if (_shadowEffectBrush == null)
                _shadowEffectBrush = (SolidColorBrush)App.Current.FindResource(SHADOW_EFFECT_BRUSH);

            return _shadowEffectBrush;
        }

        /// <summary>
        /// Returns gradient effect brush.
        /// </summary>
        /// <returns>Gradient effcet brush.</returns>
        public static Brush GetGradientEffectBrush()
        {
            if (_gradientEffectBrush == null)
                _gradientEffectBrush = (LinearGradientBrush)App.Current.FindResource(GRADIENT_EFFECT_BRUSH);

            return _gradientEffectBrush;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Defines fill brush.
        /// </summary>
        /// <param name="parentColor">Parent color.</param>
        /// <returns>Normal fill brush</returns>
        private static SolidColorBrush _GetFillBrush(System.Drawing.Color parentColor)
        {
            Color color = new Color();

            color.A = parentColor.A;
            color.R = parentColor.R;
            color.G = parentColor.G;
            color.B = parentColor.B;

            SolidColorBrush resultBrush = new SolidColorBrush(color);

            // Freeze brush.
            resultBrush = (SolidColorBrush)resultBrush.GetCurrentValueAsFrozen();

            Debug.Assert(resultBrush != null);

            return resultBrush;
        }

        /// <summary>
        /// Defienes locked color.
        /// </summary>
        /// <param name="parentColor">Parent color.</param>
        /// <returns>Locked color.</returns>
        private static SolidColorBrush _GetLockedBrush(System.Drawing.Color parentColor)
        {
            SolidColorBrush resultBrush;

            Color color = Color.FromArgb(parentColor.A, parentColor.R, parentColor.G, parentColor.B);

            if (_fillBrushes.ContainsKey(parentColor) && _fillBrushes[parentColor].Color != color)
                resultBrush = _fillBrushes[parentColor];
            else
                resultBrush = new SolidColorBrush(color);

            resultBrush = _lockedColorConverter.Convert(resultBrush, typeof(object), null, CultureInfo.CurrentCulture) as SolidColorBrush;

            // Freeze brush.
            resultBrush = (SolidColorBrush)resultBrush.GetCurrentValueAsFrozen();
            Debug.Assert(resultBrush != null);

            return resultBrush;
        }

        /// <summary>
        /// Returns highlight color for empty gantt element.
        /// </summary>
        /// <param name="parentColor">Parent color.</param>
        /// <returns>Fill brush.</returns>
        private static Brush _GetEmptyFillBrush(System.Drawing.Color parentColor)
        {
            Color color = Color.FromArgb(parentColor.A,
                                         parentColor.R,
                                         parentColor.G,
                                         parentColor.B);

            color.A = Convert.ToByte(color.A * HIGHLIGHTED_EMPTY_ELEMENT_MIN_TRANSPARENCY);

            LinearGradientBrush resultBrush = new LinearGradientBrush();
            resultBrush.StartPoint = new System.Windows.Point(0.5, 0);
            resultBrush.EndPoint = new System.Windows.Point(0.5, 1);
            resultBrush.GradientStops.Add(new GradientStop(color, 1));

            color.A = Convert.ToByte(color.A * HIGHLIGHTED_EMPTY_ELEMENT_MAX_TRANSPARENCY);

            resultBrush.GradientStops.Add(new GradientStop(color, 0));

            // Freeze brush.
            resultBrush = (LinearGradientBrush)resultBrush.GetCurrentValueAsFrozen();

            return resultBrush;
        }

        /// <summary>
        /// Gets grayscale brush from color.
        /// </summary>
        /// <param name="originalColor">Input color.</param>
        /// <returns>Brush with converted grayscale color.</returns>
        private static SolidColorBrush _GetGrayscaleBrush(System.Drawing.Color originalColor)
        {
            Color grayscaleColor = new Color();

            double R = Convert.ToDouble(originalColor.R);
            double G = Convert.ToDouble(originalColor.G);
            double B = Convert.ToDouble(originalColor.B);

            grayscaleColor.A = originalColor.A;

            // Convert original color to grayscale.
            grayscaleColor.R = Convert.ToByte((R + G + B) / GRAYSCALE_CONVERTER_PARAMETER);
            grayscaleColor.G = Convert.ToByte((R + G + B) / GRAYSCALE_CONVERTER_PARAMETER);
            grayscaleColor.B = Convert.ToByte((R + G + B) / GRAYSCALE_CONVERTER_PARAMETER);

            SolidColorBrush resultBrush = new SolidColorBrush(grayscaleColor);

            // Freeze brush.
            resultBrush = (SolidColorBrush)resultBrush.GetCurrentValueAsFrozen();
            Debug.Assert(resultBrush != null);

            return resultBrush;
        }

        #endregion
        
        #region Private Constants

        /// <summary>
        /// Row height resource name.
        /// </summary>
        private const string ITEM_HEIGHT_RESOURCE_NAME = "GanttControlStopHeight";

        /// <summary>
        /// Value used in code to convert color to grayscale.
        /// </summary>
        private const double GRAYSCALE_CONVERTER_PARAMETER = 2.5;

        /// <summary>
        /// Highlighted empty element max transparency.
        /// </summary>
        public const double HIGHLIGHTED_EMPTY_ELEMENT_MAX_TRANSPARENCY = 0.6;

        /// <summary>
        /// Highlighted empty element min transparency.
        /// </summary>
        public const double HIGHLIGHTED_EMPTY_ELEMENT_MIN_TRANSPARENCY = 0.5;

        /// <summary>
        /// Violated icon source name.
        /// </summary>
        private const string VIOLATED_ICON_SOURCE_NAME = "IsViolatedOrderBrush";

        /// <summary>
        /// Selection color resource name.
        /// </summary>
        private const string SELECTION_COLOR_NAME = "SelectionColorBrush";

        /// <summary>
        /// Location color source name.
        /// </summary>
        private const string LOCATION_COLOR_SOURCE_NAME = "LocationColor";

        /// <summary>
        /// Break color source name.
        /// </summary>
        private const string BREAK_COLOR_SOURCE_NAME = "BreakStopColor";

        /// <summary>
        /// Drag over color resource name.
        /// </summary>
        private const string DRAG_OVER_COLOR_NAME = "DragOverObjectBackground";

        /// <summary>
        /// Shadow brush.
        /// </summary>
        private const string SHADOW_EFFECT_BRUSH = "GanttShadowColor";

        /// <summary>
        /// Gradient effect brush resource name.
        /// </summary>
        private const string GRADIENT_EFFECT_BRUSH = "GanttGradientEffectBrush";

        #endregion

        #region Private Fields

        /// <summary>
        /// Locked color converter.
        /// </summary>
        private static LockedColorConverter _lockedColorConverter = new LockedColorConverter();

        /// <summary>
        /// Hashed fill brushes.
        /// </summary>
        private static Dictionary<object, SolidColorBrush> _fillBrushes = new Dictionary<object,SolidColorBrush>();

        /// <summary>
        /// Hashed empty brushes.
        /// </summary>
        private static Dictionary<object, Brush> _emptyFillBrushes = new Dictionary<object, Brush>();

        /// <summary>
        /// Hashed grayscale brushes.
        /// </summary>
        private static Dictionary<object, SolidColorBrush> _grayscaleBrushes = new Dictionary<object, SolidColorBrush>();

        /// <summary>
        /// Hashed locked brushes.
        /// </summary>
        private static Dictionary<object, SolidColorBrush> _lockedBrushes = new Dictionary<object, SolidColorBrush>();

        /// <summary>
        /// Hashed selection brush.
        /// </summary>
        private static SolidColorBrush _selectionBrush = null;

        /// <summary>
        /// Hashed violated brush.
        /// </summary>
        private static ImageBrush _violatedBrush = null;

        /// <summary>
        /// Hashed location brush.
        /// </summary>
        private static SolidColorBrush _locationBrush = null;

        /// <summary>
        /// Hashed break brush.
        /// </summary>
        private static SolidColorBrush _breakBrush = null;

        /// <summary>
        /// Hashed drag over brush.
        /// </summary>
        private static SolidColorBrush _dragOverBrush = null;

        /// <summary>
        /// Shadow effect brush. 
        /// </summary>
        private static SolidColorBrush _shadowEffectBrush = null;

        /// <summary>
        /// Gradient effect brush.
        /// </summary>
        private static LinearGradientBrush _gradientEffectBrush = null;

        /// <summary>
        /// TODO:
        /// </summary>
        private static Pen _pen = null;
 
        #endregion
    }
}
