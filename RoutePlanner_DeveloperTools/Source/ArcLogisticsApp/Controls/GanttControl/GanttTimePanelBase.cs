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
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Gantt TimePanel base class
    /// </summary>
    /// <remarks>This class support only directly drawing.</remarks>
    internal abstract class GanttTimePanelBase : Panel
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GanttTimePanelBase()
        {
            _children = new VisualCollection(this);
        }

        #endregion // Constructor

        #region Public methods

        /// <summary>
        /// Recreate all visual components for Gantt panel and draw theirs
        /// </summary>
        public void UpdateLayout(DateTime startTime, DateTime endTime, Size newControlSize)
        {
            double newHeight = Math.Min(newControlSize.Height, this.ActualHeight);

            _size = new Size(newControlSize.Width, newHeight);

            double duration = Math.Floor((endTime - startTime).TotalHours);

            Debug.Assert(duration > 0);

            DateTime currentHour = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);
            if ((currentHour != _startHour) || (duration != _duration))
            {
                _startHour = currentHour;
                _duration = duration;
            }

            InvalidateMeasure();
            InvalidateVisual();
        }

        #endregion // Public methods

        #region Override methods

        /// <summary>
        /// Returns count of visual children.
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        /// <summary>
        /// Returns visual child by index.
        /// </summary>
        /// <param name="index">Necessary child index.</param>
        /// <returns>Visual child.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        /// <summary>
        /// Returns new control's size.
        /// </summary>
        /// <param name="availableSize">New available size.</param>
        /// <returns>New control size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            return _size;
        }

        /// <summary>
        /// Redraws control.
        /// </summary>
        /// <param name="dc">Drawing context.</param>
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            _ReDraw();
        }

        #endregion // Override methods

        #region Private helpers

        /// <summary>
        /// Redraws control if necessary.
        /// </summary>
        private void _ReDraw()
        {
            if ((0 < this.ActualWidth) && (0 < this.ActualHeight))
            {
                Size newSize = new Size(this.ActualWidth, this.ActualHeight);
                _RecreateVisualChildren(newSize);
            }
        }

        /// <summary>
        /// Method recreates visual children and returns their dimensions.
        /// </summary>
        /// <param name="dimension">Size for visual children.</param>
        /// <returns>New size.</returns>
        private Size _RecreateVisualChildren(Size dimension)
        {
            Size requestedSize = new Size(1, 1);

            _children.Clear();
            if ((0 < _duration) && (0 < dimension.Width))
            {
                double widthPerHour = dimension.Width / _duration;

                int rangeStepInHour = 1;
                while ((widthPerHour * rangeStepInHour < TIME_RANGE_MIN_WIDTH) && (rangeStepInHour < _duration))
                    ++rangeStepInHour;

                double rangeWidth = widthPerHour * rangeStepInHour;
                requestedSize = _CreateVisualChildren(dimension, rangeWidth, rangeStepInHour);
            }

            return requestedSize;
        }

        /// <summary>
        /// Creates visual children for pannel.
        /// </summary>
        protected abstract Size _CreateVisualChildren(Size dimension, double rangeWidth, int rangeStepInHour);

        #endregion // Private helpers

        #region Private Constants 

        /// <summary>
        /// Const min width for range.
        /// </summary>
        private const double TIME_RANGE_MIN_WIDTH = 30;

        #endregion

        #region Private Fields

        /// <summary>
        /// Start date and time.
        /// </summary>
        protected DateTime _startHour = DateTime.MinValue;

        /// <summary>
        /// Duration in hours.
        /// </summary>
        protected double _duration = 0;

        /// <summary>
        /// Collection of visual elements.
        /// </summary>
        protected VisualCollection _children = null;

        /// <summary>
        /// Draw style keeper.
        /// </summary>
        protected static GanttTimeLineStyle _style = new GanttTimeLineStyle();

        /// <summary>
        /// Control's size. "0" by default.
        /// </summary>
        private Size _size = new Size(0, 0);

        #endregion 
    }
}
