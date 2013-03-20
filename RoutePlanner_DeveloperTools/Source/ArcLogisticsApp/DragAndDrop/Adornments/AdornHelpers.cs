using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ESRI.ArcLogistics;
using ESRI.ArcLogistics.App.DragAndDrop.Adornments.Controls;
using ESRI.ArcLogistics.App.GraphicObjects;
using ESRI.ArcLogistics.App.OrderSymbology;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Class that contains different helpers used in adornments.
    /// </summary>
    internal static class AdornHelpers
    {
        #region Public Methods

        /// <summary>
        /// Create image that shows an order sheet.
        /// </summary>
        /// <returns>Image.</returns>
        public static Image CreateSheetImage()
        {
            BitmapImage bitmap = new BitmapImage(new Uri(IMAGE_PATH, UriKind.Absolute));
            Image image = new Image();
            image.Source = bitmap;

            return image;
        }

        /// <summary>
        /// Creates label sequence symbol.
        /// </summary>
        /// <param name="stop">Stop.</param>
        /// <returns>Symbol.</returns>
        public static Control CreateLabelSequenceSymbol(Stop stop)
        {
            return _CreateMapSymbol(
                LABEL_SEQUENCE_CONTROL_TEMPLATE, 
                Color.FromRgb(stop.Route.Color.R, stop.Route.Color.G, stop.Route.Color.B),
                stop.IsLocked,
                stop.IsViolated,
                stop.OrderSequenceNumber);
        }

        /// <summary>
        /// Creates dot symbol.
        /// </summary>
        /// <param name="orderOrStop">Order or stop.</param>
        /// <returns>Dot symbol.</returns>
        public static Control CreateDotSymbol(Stop stop)
        {
            // Fill necessary attributes.
            Color color = Color.FromRgb(stop.Route.Color.R, stop.Route.Color.G, stop.Route.Color.B);
            bool isLocked = stop.IsLocked;
            bool isViolated = stop.IsViolated;
            int? sequenceNumber = stop.OrderSequenceNumber;

            return _CreateMapSymbol(CUSTOM_ORDER_SYMBOL_CONTROL_TEMPLATE, color, isLocked, 
                isViolated, sequenceNumber);
        }

        public static Control CreateCustomOrderSymbol(object orderOrStop)
        {
            DataGraphicObject graphicObject = null;

            // Create data graphic object that depends on object type.
            if (orderOrStop is Stop)
                graphicObject = StopGraphicObject.Create(orderOrStop as Stop);
            else if (orderOrStop is Order)
                graphicObject = OrderGraphicObject.Create(orderOrStop as Order);
            else
                Debug.Assert(false); // Not supported.

            // Init graphics object by order symbology manager.
            SymbologyManager.InitGraphic(graphicObject);

            // Create symbol control
            var control = new ESRI.ArcLogistics.App.DragAndDrop.Adornments.Controls.SymbolControl
                (graphicObject.Symbol.ControlTemplate);

            // Copy attributes from graphics object to symbol control. 
            control.SymbologyContextDictionary = graphicObject.Attributes;

            // Offset symbol so it is not rendered shifted.
            control.RenderTransform = new TranslateTransform(-control.OffsetX, -control.OffsetY);

            return control;
        }

        /// <summary>
        /// Retrives order from reference to either stop or order.
        /// </summary>
        /// <param name="orderOrStop">Stop or order.</param>
        /// <returns>Order.</returns>
        public static Order GetOrder(object orderOrStop)
        {
            if (orderOrStop is Order)
                return orderOrStop as Order;
            else
            {
                object associatedObject = (orderOrStop as Stop).AssociatedObject;
                Debug.Assert(associatedObject != null && associatedObject is Order);

                return associatedObject as Order;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create map symbol.
        /// </summary>
        /// <param name="templateResource">XAML path.</param>
        /// <param name="color">Fill color.</param>
        /// <param name="isLocked">Whether symbol should look locked.</param>
        /// <param name="isViolated">Whether symbol should look violated.</param>
        /// <param name="sequenceNumber">Stop sequence number.</param>
        /// <returns>Symbol element.</returns>
        private static Control _CreateMapSymbol(string templateResource, Color color, bool isLocked, bool isViolated, int? sequenceNumber)
        {
            // Load label sequence control template.
            ControlTemplate template = _LoadTemplateFromResource(templateResource);

            // Create symbol control
            var control = new ESRI.ArcLogistics.App.DragAndDrop.Adornments.Controls.SymbolControl
                (template);
            
            // Set sequence number.
            control.SequenceNumber = sequenceNumber.HasValue ? sequenceNumber.Value.ToString() : "";
            
            // Set color.
            SolidColorBrush fillingBrush = new System.Windows.Media.SolidColorBrush(color);
            control.Fill = fillingBrush;

            // Set locked status.
            control.IsLocked = isLocked;

            // Set violated status.
            control.IsViolated = isViolated;

            // Set render transformation to suppress displacement that is necessary for showing on map control.
            double? symbolDisplacement = (double?)template.Resources["SymbolDisplacement"];
            if (symbolDisplacement.HasValue)
                control.RenderTransform = new TranslateTransform(-symbolDisplacement.Value, -symbolDisplacement.Value);
            else
            {
                control.RenderTransform = new TranslateTransform(-control.OffsetX, -control.OffsetY);
            }

            return control;
        }

        /// <summary>
        /// Loads control template from resource XAML file.
        /// </summary>
        /// <param name="key">XAML resource path.</param>
        /// <returns>Control template.</returns>
        private static ControlTemplate _LoadTemplateFromResource(string key)
        {
            ControlTemplate controlTemplate = null;

            Stream stream = Application.Current.GetType().Assembly.GetManifestResourceStream(key);
            string template = new StreamReader(stream).ReadToEnd();
            StringReader stringReader = new StringReader(template);
            XmlTextReader xmlReader = new XmlTextReader(stringReader);
            controlTemplate = XamlReader.Load(xmlReader) as ControlTemplate;

            return controlTemplate;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Resource name of unassigned orders color.
        /// </summary>
        private const string UNASSIGNED_ORDERS_COLOR_RESOURCE_NAME = "UnassignedOrderColor";

        /// <summary>
        /// Resource path of order sheet image.
        /// </summary>
        private const string IMAGE_PATH = @"pack://application:,,,/ESRI.ArcLogistics.App;component/Resources/PNG_Icons/Reports24.png";

        /// <summary>
        /// Label sequence symbol control template.
        /// </summary>
        private const string LABEL_SEQUENCE_CONTROL_TEMPLATE = @"ESRI.ArcLogistics.App.Symbols.LabelSequenceSymbol.xaml";

        /// <summary>
        /// Custom order symbol control template.
        /// </summary>
        private const string CUSTOM_ORDER_SYMBOL_CONTROL_TEMPLATE = @"ESRI.ArcLogistics.App.Symbols.CustomOrderSymbol.xaml";

        #endregion
    }
}
