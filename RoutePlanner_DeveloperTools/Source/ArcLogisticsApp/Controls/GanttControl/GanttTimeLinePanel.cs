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
using System.Globalization;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Gantt TimeLine item class
    /// </summary>
    internal class GanttTimeLineItem : DrawingVisual
    {
        #region Constructor
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public GanttTimeLineItem(int durationInHour)
        {
            _durationInHour = durationInHour;
        }

        #endregion // Constructor

        #region Public properties

        public int Duration
        {
            get { return _durationInHour; }
        }

        #endregion // Public properties

        #region Private members
        
        /// <summary>
        /// Control duration in hours.
        /// </summary>
        private int _durationInHour = 0;

        #endregion // Private members
    }

    /// <summary>
    /// Gantt TimeLine panel class - represent time intervals with hour label and minutes ranges
    /// </summary>
    internal class GanttTimeLinePanel : GanttTimePanelBase
    {
        #region Constructor
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public GanttTimeLinePanel()
        {
        }

        #endregion // Constructor

        #region Private Helpers

        /// <summary>
        /// Create time label text (with Culture specific)
        /// </summary>
        private string _GetTimeString(DateTime startHour, CultureInfo cultureInfo)
        {
            string result = string.Empty;
            DateTimeFormatInfo dateTimeFormat = cultureInfo.DateTimeFormat;

            string hoursFormat = dateTimeFormat.ShortTimePattern.Substring(0, 1);

            string hours = startHour.ToString("%" + hoursFormat, dateTimeFormat);

            string formettedTime = startHour.ToString(dateTimeFormat.ShortTimePattern, dateTimeFormat);

            if (formettedTime.Contains(dateTimeFormat.AMDesignator))
                result = string.Format("{0}{1}", hours, dateTimeFormat.AMDesignator);
            else if (formettedTime.Contains(dateTimeFormat.PMDesignator))
                result = string.Format("{0}{1}", hours, dateTimeFormat.PMDesignator);
            else
                result = hours;

            return result;
        }

        /// <summary>
        /// Create hour range visual element - label with hairlines
        /// </summary>
        private DrawingVisual _CreateVisualItem(DateTime dateTime, int durationInHour, Rect bound)
        {
            GanttTimeLineItem item = new GanttTimeLineItem(durationInHour);
            int hairLineCount = (1 == durationInHour) ? _style.PartPerHour : durationInHour;

            RenderOptions.SetEdgeMode((DependencyObject)item, EdgeMode.Aliased);
            using (DrawingContext dc = item.RenderOpen())
            {
                // Draw hairlines.
                double offset = bound.Width / hairLineCount; // Start offset.

                Point bottomLeft = bound.BottomLeft;
                Point pt1 = bottomLeft;
                Point pt2 = bottomLeft;

                // Draw hairline.
                for (int index = 0; index <= hairLineCount; ++index)
                {
                    // Start and end hairlines with full height other with selected.
                    pt2.Y = ((0 == index) || (hairLineCount == index)) ? 0 : bound.Height - _style.HairlineHeight;

                    // draw vertical line
                    double x = Math.Floor(pt1.X);
                    dc.DrawLine(_style.HairlinePen, new Point(x, pt1.Y), new Point(x, pt2.Y));

                    pt1.X += offset;
                }

                // Draw lable.
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                FormattedText formattedText = new FormattedText(_GetTimeString(dateTime, cultureInfo),
                                                                cultureInfo, FlowDirection.LeftToRight,
                                                                _style.FontTypeface, _style.FontSize,
                                                                _style.FontBrush);
                Point textPosition = new Point(bottomLeft.X + _style.LabelMargin.Width, _style.LabelMargin.Height);
                dc.DrawText(formattedText, textPosition);
            }

            // Add clip rect for visual element.
            RectangleGeometry clipGeometry = new RectangleGeometry();
            clipGeometry.Rect = bound;
            item.Clip = clipGeometry;

            return item;
        }

        /// <summary>
        /// Create Gant TimeLine ranges.
        /// </summary>
        protected override Size _CreateVisualChildren(Size dimension, double rangeWidth, int rangeStepInHour)
        {
            Rect boundingRect = new Rect();

            // Calculate range boundbox.
            Size itemSize = new Size(rangeWidth, dimension.Height);
            Rect itemRect = new Rect(new Point(0, 0), itemSize);

            DateTime currentHour;

            // Define start hour: "0" if _startDate in MaxValue
            if (_startHour.Date == DateTime.MaxValue.Date)
                currentHour = DateTime.MinValue;
            else
                currentHour = _startHour;

            int current = 0;
            while (current < (int)_duration)
            {
                // Create TimeLine range.
                DrawingVisual visualItem = _CreateVisualItem(currentHour, rangeStepInHour, itemRect);
                _children.Add(visualItem);
                
                boundingRect.Union(visualItem.ContentBounds);

                // Relocate for next element.
                itemRect.Offset(rangeWidth, 0);

                currentHour = currentHour.AddHours(rangeStepInHour);
                current += rangeStepInHour;
            }

            return boundingRect.Size;
        }

        #endregion // Private helpers
    }
}
