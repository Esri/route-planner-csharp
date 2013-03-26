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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ESRI.ArcLogistics.App.Controls.BitmapEx
{
    /// <summary>
    ///  Bitmap class inherited from FrameworkElement which is able to do pixel alignment.
    ///  Therefore bitmaps are displayed without blurry effect.
    ///  Wraps BitmapUIElement, which performs the following things:
    ///     1. Adjusts size to real pixel sizes.
    ///     2. Adjusts bitmap's position to be on pixel boundaries.
    /// </summary>
    public class SmartBitmap : FrameworkElement
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <c>SmartBitmap</c>.
        /// </summary>
        public SmartBitmap()
        {
            _bitmapUIElement = new BitmapUIElement();

            AddVisualChild(_bitmapUIElement);
        }

        #endregion Constructors

        #region Public static properties

        /// <summary>
        /// Dependency property Source.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                                        "Source",
                                        typeof(BitmapSource),
                                        typeof(SmartBitmap),
                                        new FrameworkPropertyMetadata(
                                            null,
                                            FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure,
                                            SmartBitmap._OnSourceChanged)
                                       );

        #endregion Public static properties

        //public static readonly DependencyProperty BreaksProperty =
        //    DependencyProperty.Register("Breaks", typeof(Breaks), typeof(BreakEditor));
        #region Public properties

        /// <summary>
        /// Gets or sets bitmap source.
        /// </summary>
        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }

            set { SetValue(SourceProperty, value); }
        }

        #endregion Public properties

        #region Overridden methods of the base class

        /// <summary>
        /// Gets a child at the specified index from a collection of child elements.
        /// This class has only one visual child: BitmapUIElement object.
        /// </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>The requested child element.</returns>
        /// <exception cref="System.IndexOutOfRangeException">Index is out of range</exception>
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= VisualChildrenCount)
                throw new IndexOutOfRangeException("index");

            return _bitmapUIElement;
        }

        /// <summary>
        /// Measures the size in layout required for child elements and determines a size for the FrameworkElement-derived class.
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements.
        /// Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of
        /// child element sizes.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            _bitmapUIElement.Measure(availableSize);

            return _bitmapUIElement.DesiredSize;
        }

        /// <summary>
        /// Positions child elements and determines a size for a FrameworkElement derived class.
        /// </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself
        /// and its children.</param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Point nullPoint = new Point(0, 0);

            Rect finalRect = new Rect(nullPoint, finalSize);

            _bitmapUIElement.Arrange(finalRect);

            return finalSize;
        }

        /// <summary>
        /// Gets the number of visual child elements within this element (always returns 1).
        /// </summary>
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        #endregion Overridden methods of the base class

        #region Private static methods

        /// <summary>
        /// Invoked when bitmap's Source property is changed.
        /// </summary>
        /// <param name="dependencyObject">Dependency object - bitmap.</param>
        /// <param name="eventArgs">Event data.</param>
        private static void _OnSourceChanged(DependencyObject dependencyObject,
                                             DependencyPropertyChangedEventArgs eventArgs)
        {
            SmartBitmap smartBitmap = (SmartBitmap)dependencyObject;

            smartBitmap._bitmapUIElement.Source = (BitmapSource)eventArgs.NewValue;
        }

        #endregion Private static methods

        #region Private fields

        /// <summary>
        /// BitmapUIElement object.
        /// </summary>
        private BitmapUIElement _bitmapUIElement;

        #endregion Private fields
    }
}
