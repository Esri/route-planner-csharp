/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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
    /// Base class for signle order adornments. It shows icon with time windows.
    /// </summary>
    internal abstract class SingleOrderAdornmentBase : IAdornment
    {
        #region Constructor

        /// <summary>
        /// Creates new instance of <c>SingleOrderAdornmentBase</c> class.
        /// </summary>
        public SingleOrderAdornmentBase(object orderOrStop)
        {
            Debug.Assert(orderOrStop != null);
            Debug.Assert(orderOrStop is Order || orderOrStop is Stop);

            _adornCanvas = _CreateAdornmentCanvas(orderOrStop);
        }

        #endregion

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
        /// Creates order element that is shown in the single order adorner. Implement this method in derived classes.
        /// </summary>
        /// <param name="orderOrStop">Order or stop instance.</param>
        /// <returns>Visual representaion of the order.</returns>
        protected abstract FrameworkElement CreateOrderElement(object orderOrStop);

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates adorment canvas.
        /// </summary>
        /// <param name="order">Order.</param>
        /// <returns>Canvas.</returns>
        private Canvas _CreateAdornmentCanvas(object orderOrStop)
        {
            // Create canvas.
            Canvas adornCanvas = new Canvas();

            // Create order element and add it to canvas.
            FrameworkElement element = CreateOrderElement(orderOrStop);
            adornCanvas.Children.Add(element);

            // Get order.
            Order order = AdornHelpers.GetOrder(orderOrStop);

            // Create time window 1 label and add it to canvas.
            TextBox tw = new TextBox();

            Style style = (Style)Application.Current.FindResource(TIME_WINDOWS_TEXT_BOX_STYLE);
            Debug.Assert(style != null);
            tw.Style = style;

            tw.Text = _GetTimeWindowsText(order);
            adornCanvas.Children.Add(tw);

            adornCanvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Move label.
            Canvas.SetTop(tw, element.DesiredSize.Height + SPACE_SIZE);
            Canvas.SetLeft(tw, element.DesiredSize.Width + SPACE_SIZE);

            // Set canvas height and width.
            adornCanvas.Height = element.DesiredSize.Height + SPACE_SIZE + tw.DesiredSize.Height;
            adornCanvas.Width = element.DesiredSize.Width + SPACE_SIZE + tw.DesiredSize.Width;

            return adornCanvas;
        }

        /// <summary>
        /// Returns text that has to be shown in time windows section.
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private string _GetTimeWindowsText(Order order)
        {
            string twText;
            if (order.TimeWindow.IsWideOpen && order.TimeWindow2.IsWideOpen)
                twText = order.TimeWindow.ToString();

            else if (!order.TimeWindow.IsWideOpen && order.TimeWindow2.IsWideOpen)
                twText = order.TimeWindow.ToString();

            else if (order.TimeWindow.IsWideOpen && !order.TimeWindow2.IsWideOpen)
                twText = order.TimeWindow2.ToString();

            else
            {
                StringBuilder strBuilder = new StringBuilder();
                strBuilder.Append(order.TimeWindow.ToString());
                strBuilder.Append(TIME_WINDOWS_SEPARATOR);
                strBuilder.Append(order.TimeWindow2.ToString());

                twText = strBuilder.ToString();
            }

            return twText;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Resource name of time windows text box style.
        /// </summary>
        private const string TIME_WINDOWS_TEXT_BOX_STYLE = "AdornmentTimeWindowTextBoxStyle";

        /// <summary>
        /// Separator between time windows in text box.
        /// </summary>
        private const string TIME_WINDOWS_SEPARATOR = "\n";

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
