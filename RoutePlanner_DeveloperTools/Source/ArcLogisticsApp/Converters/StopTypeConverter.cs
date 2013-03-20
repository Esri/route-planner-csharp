using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using ESRI.ArcLogistics.App.Pages;
using System.Diagnostics;


namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Method gets stop and return icon depending on stop type (Order, Location or Lunch) and isViolated value.
    /// </summary>
    /// <remarks> Implements IMultiValueConverter to allow pass Stop as parameter and handle event about Status and IsViolated property changed. </remarks>
    [ValueConversion(typeof(Object), typeof(Object))]
    internal class StopTypeConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts values. Create and return control template for necessary stop. 
        /// </summary>
        /// <param name="value">Stop.</param>
        /// <param name="targetType">Type of input value.</param>
        /// <param name="parameter">Converter parameter.</param>
        /// <param name="culture">Curren localization settings.</param>
        /// <returns>Control template.</returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(values != null);
            Debug.Assert(values.Length == BINDING_PARAMETERS_COUNT); // Count of input parameter must be equals 3 : Stop, IsViolated and Color value.

            // If any input parameter is null - return null.
            if (values[0] == null || values[1] == null)
                return null;

            Stop currentStop = (Stop)values[0];
            Debug.Assert(currentStop != null);

            // Workaround: on some unknown reason this converter is called even
            // if stop was unassigned from route, so we need additionally check
            // that stop belong to route.
            // If this condition isnt true then return no image.
            if (currentStop.Route == null)
                return new Grid();

            StopType currentStopType = currentStop.StopType;

            string templateResourceName = string.Empty;

            // Define ControlTemplate resource name for stop.
            if (currentStopType.Equals(StopType.Lunch))
                templateResourceName = BREAK_STOP_GLYPH;
            else if (currentStopType.Equals(StopType.Location))
                templateResourceName = LOCATION_STOP_GLYPH;
            else if (currentStopType.Equals(StopType.Order))
                templateResourceName = ORDER_STOP_GLYPH;
            else
                Debug.Assert(false); // Not supporter now.

            Debug.Assert(currentStop.Route != null);

            // If stop is violated - use violated icon.
            if (currentStop.IsViolated)
                templateResourceName = VIOLATED_STOP_GLYPH;

            // Load ControlTemplate from necessary resource.
            ControlTemplate template = (ControlTemplate)App.Current.FindResource(templateResourceName);
            Debug.Assert(template != null);
            Grid grid = (Grid)template.LoadContent();

            // If current stop is Order - define symbol color.
            if (templateResourceName == ORDER_STOP_GLYPH)
            {
                Path path = grid.Children[2] as Path;
                Debug.Assert(path != null);

                Color color = Color.FromArgb(currentStop.Route.Color.A, currentStop.Route.Color.R,
                    currentStop.Route.Color.G, currentStop.Route.Color.B);
                path.Fill = new SolidColorBrush(color);
            }
            Object result = grid;

            return result;
        }

        /// <summary>
        /// Converts value back. Not implementer there.
        /// </summary>
        /// <param name="value">Output value.</param>
        /// <param name="targetType">Type of output value.</param>
        /// <param name="parameter">Converter parameter.</param>
        /// <param name="culture">Curren localization settings.</param>
        /// <returns>Result value.</returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }

        #region Private Constants

        /// <summary>
        /// Size of stop glyph.
        /// </summary>
        private const int GLYPH_SIZE = 22;

        /// <summary>
        /// Binding parameters count.
        /// </summary>
        private const int BINDING_PARAMETERS_COUNT = 3;

        /// <summary>
        /// "BreakStopGlyph" resource name.
        /// </summary>
        private const string BREAK_STOP_GLYPH = "BreakStopGlyph";

        /// <summary>
        /// "LocationStopGlyph" resource name.
        /// </summary>
        private const string LOCATION_STOP_GLYPH = "LocationStopGlyph";

        /// <summary>
        /// "OrderStopGlyph" resource name.
        /// </summary>
        private const string ORDER_STOP_GLYPH = "OrderStopGlyph";

        /// <summary>
        /// Violated stop glyph icon.
        /// </summary>
        private const string VIOLATED_STOP_GLYPH = "ViolatedLocationStopStatus16x16";

        // NOTE : Next string resources are same as used in control templates. 
        // All changes should be synchronized.

        /// <summary>
        /// Arrived icon resource name.
        /// </summary>
        private const string ARRIVED_STATUS_ICON_NAME = "Status_Arrived";

        /// <summary>
        /// DrivingTo icon resource name.
        /// </summary>
        private const string DRIVING_TO_STATUS_ICON_NAME = "Status_DrivingTo";

        /// <summary>
        /// Completed icon resource name.
        /// </summary>
        private const string COMPLETED_STATUS_ICON_NAME = "Status_Completed";

        /// <summary>
        /// Cancelled icon resource name.
        /// </summary>
        private const string CANCELLED_STATUS_ICON_NAME = "Status_Cancelled";

        /// <summary>
        /// Servicing icon resource name.
        /// </summary>
        private const string SERVICING_STATUS_ICON_NAME = "Status_Servicing";

        #endregion
    }
}
