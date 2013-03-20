using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.DragAndDrop.Adornments
{
    /// <summary>
    /// Base class for multiple order adornments. It shows icon with a label that shows number of orders.
    /// </summary>
    internal abstract class MultiOrderAdornmentBase : IAdornment
    {
        /// <summary>
        /// Creates new instance of <c>MultiOrderAdornmentBase</c> class.
        /// </summary>
        public MultiOrderAdornmentBase(IList<object> ordersAndStops)
        {
            Debug.Assert(ordersAndStops != null);
            Debug.Assert(ordersAndStops.Count > 0);

            _adornCanvas = _CreateAdornmentCanvas(ordersAndStops);
        }

        #region IAdornment Members

        public System.Windows.Controls.Canvas Adornment
        {
            get
            {
                return _adornCanvas;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Creates order element that is shown in the multiple order adorner. Implement this method in derived classes.
        /// </summary>
        /// <param name="orderOrStop">Collections of orders and/or stops.</param>
        /// <param name="element">Visual representaion of order/stop.</param>
        /// <returns>True if element was returned and false if it cannot be done.</returns>
        protected abstract FrameworkElement CreateOrderElement(object orderOrStop);

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates adorment canvas.
        /// </summary>
        /// <param name="order">Order.</param>
        /// <returns>Canvas.</returns>
        private Canvas _CreateAdornmentCanvas(IList<object> ordersAndStops)
        {
            // Get only orders and stop orders.
            IList<object> onlyOrders = _ExcludeNonOrders(ordersAndStops);

            // Create canvas.
            Canvas adornCanvas = new Canvas();

            // Create orders element and add it to canvas.
            Canvas multiElemCanvas = _CreateMultipleElementsCanvas(onlyOrders);
            adornCanvas.Children.Add(multiElemCanvas);

            // Create count label.
            TextBox tb = new TextBox();

            Style style = (Style)Application.Current.FindResource(ELEMENTS_COUNT_TEXT_BOX_STYLE);
            Debug.Assert(style != null);
            tb.Style = style;

            tb.Text = onlyOrders.Count.ToString();
            adornCanvas.Children.Add(tb);

            adornCanvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Move label.
            Canvas.SetTop(tb, multiElemCanvas.DesiredSize.Height + SPACE_SIZE);
            Canvas.SetLeft(tb, multiElemCanvas.DesiredSize.Width + SPACE_SIZE);

            // Set canvas height and width.
            adornCanvas.Height = multiElemCanvas.DesiredSize.Height + SPACE_SIZE + tb.DesiredSize.Height;
            adornCanvas.Width = multiElemCanvas.DesiredSize.Width + SPACE_SIZE + tb.DesiredSize.Width;
            
            return adornCanvas;
        }

        private Canvas _CreateMultipleElementsCanvas(IList<object> ordersAndStops)
        {
            // Collection for elements.
            List<FrameworkElement> elements = new List<FrameworkElement>();

            // Create elements.
            for (int i = 0; i < ordersAndStops.Count && elements.Count < MAX_ELEMENTS_TO_SHOW; i++)
            {
                FrameworkElement element = CreateOrderElement(ordersAndStops[i]);
                elements.Add(element);
            }

            // Create canvas.
            Canvas canvas = new Canvas();
            
            // Add elements to canvas.
            foreach (FrameworkElement element in elements)
                canvas.Children.Add(element);

            // Measure canvas elements.
            canvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Place elements.
            double height = 0;
            double width = 0;
            double xShift = (elements.Count - 1) * DISPLACEMENT_BETWEEN_ELEMENTS;
            double yShift = 0;
            foreach (FrameworkElement elem in elements)
            {
                Canvas.SetTop(elem, yShift);
                Canvas.SetLeft(elem, xShift);

                // Update canvas size.
                if (elem.DesiredSize.Width + xShift > width)
                    width = elem.DesiredSize.Width + xShift;

                if (elem.DesiredSize.Height + yShift > height)
                    height = elem.DesiredSize.Height + yShift;

                // Increase/Decrease shifts.
                xShift -= DISPLACEMENT_BETWEEN_ELEMENTS;
                yShift += DISPLACEMENT_BETWEEN_ELEMENTS;
            }

            // Set canvas size.
            canvas.Height = height;
            canvas.Width = width;

            return canvas;
        }

        /// <summary>
        /// Excludes break and location stops from the collection.
        /// </summary>
        /// <param name="ordersAndStops">Collection of orders and stops.</param>
        /// <returns>Filtered collection.</returns>
        private IList<object> _ExcludeNonOrders(IList<object> ordersAndStops)
        {
            List<object> onlyOrders = new List<object>();

            foreach (object orderOrStop in ordersAndStops)
            {
                // If item is Order or Stop with associated Order.
                if (orderOrStop is Order ||
                    orderOrStop is Stop && (orderOrStop as Stop).StopType == StopType.Order)
                    onlyOrders.Add(orderOrStop);
            }

            return onlyOrders;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Resource name of time windows text box style.
        /// </summary>
        private const string ELEMENTS_COUNT_TEXT_BOX_STYLE = "AdornmentMultipleSelectionTextBoxStyle";

        /// <summary>
        /// Maximum elements to show in adorner.
        /// </summary>
        private const double MAX_ELEMENTS_TO_SHOW = 3;

        /// <summary>
        /// Displacement between element in multiple group.
        /// </summary>
        private const double DISPLACEMENT_BETWEEN_ELEMENTS = 3;

        /// <summary>
        /// Size of space between icon and time windows.
        /// </summary>
        private const double SPACE_SIZE = 0;

        /// <summary>
        /// Adornment canvas.
        /// </summary>
        private Canvas _adornCanvas;

        #endregion
    }
}
