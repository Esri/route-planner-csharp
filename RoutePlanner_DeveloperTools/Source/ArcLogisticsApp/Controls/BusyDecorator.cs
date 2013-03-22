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
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Controls
{
    [StyleTypedProperty(Property = "BusyStyle", StyleTargetType = typeof(Control))]
    internal class BusyDecorator : Decorator
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static BusyDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyDecorator), new FrameworkPropertyMetadata(typeof(BusyDecorator)));
        }

        public BusyDecorator()
        {
            AddLogicalChild(_busyHost);
            AddVisualChild(_busyHost);

            _SetBinding(_busyHost, IsBusyIndicatorShowingProperty, BackgroundVisualHost.IsContentShowingProperty);
            _SetBinding(_busyHost, BusyHorizontalAlignmentProperty, BackgroundVisualHost.HorizontalAlignmentProperty);
            _SetBinding(_busyHost, BusyVerticalAlignmentProperty, BackgroundVisualHost.VerticalAlignmentProperty);
        }

        #endregion Constructors

        #region IsBusyIndicatorShowing property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Identifies the IsBusyIndicatorShowing dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBusyIndicatorShowingProperty = DependencyProperty.Register(
            "IsBusyIndicatorShowing", typeof(bool), typeof(BusyDecorator),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets if the BusyIndicator is being shown.
        /// </summary>
        public bool IsBusyIndicatorShowing
        {
            get { return (bool)GetValue(IsBusyIndicatorShowingProperty); }
            set { SetValue(IsBusyIndicatorShowingProperty, value); }
        }

        #endregion // IsBusyIndicatorShowing property

        #region BusyStyle property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        ///<summary>
        /// Identifies the <see cref="BusyStyle" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyStyleProperty =
            DependencyProperty.Register("BusyStyle", typeof(Style), typeof(BusyDecorator),
            new FrameworkPropertyMetadata(OnBusyStyleChanged));

        /// <summary>
        /// Gets or sets the Style to apply to the Control that is displayed as the busy indication.
        /// </summary>
        public Style BusyStyle
        {
            get { return (Style)GetValue(BusyStyleProperty); }
            set { SetValue(BusyStyleProperty, value); }
        }

        static void OnBusyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BusyDecorator bd = (BusyDecorator)d;
            bd._busyHost.CreateContent = () => new Control { Style = (Style)e.NewValue };
        }

        #endregion // BusyStyle property

        #region BusyHorizontalAlignment property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        ///<summary>
        /// Identifies the <see cref="BusyHorizontalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyHorizontalAlignmentProperty = DependencyProperty.Register(
            "BusyHorizontalAlignment", typeof(HorizontalAlignment), typeof(BusyDecorator),
            new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));

        /// <summary>
        /// Gets or sets the HorizontalAlignment to use to layout the control that contains the busy indicator control.
        /// </summary>
        public HorizontalAlignment BusyHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(BusyHorizontalAlignmentProperty); }
            set { SetValue(BusyHorizontalAlignmentProperty, value); }
        }

        #endregion // BusyHorizontalAlignment property

        #region BusyVerticalAlignment property
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        ///<summary>
        /// Identifies the <see cref="BusyVerticalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyVerticalAlignmentProperty = DependencyProperty.Register(
            "BusyVerticalAlignment", typeof(VerticalAlignment), typeof(BusyDecorator),
            new FrameworkPropertyMetadata(VerticalAlignment.Center));

        /// <summary>
        /// Gets or sets the the VerticalAlignment to use to layout the control that contains the busy indicator.
        /// </summary>
        public VerticalAlignment BusyVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(BusyVerticalAlignmentProperty); }
            set { SetValue(BusyVerticalAlignmentProperty, value); }
        }

        #endregion // IsBusyIndicatorShowing property

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Update host layout
        /// </summary>
        /// <param name="width">new width</param>
        /// <param name="text">status text</param>
        /// <remarks>If (text == null) host set as hidden</remarks>
        public void UpdateHostLayout(double width, string text)
        {
            _busyHost.UpdateLayout(width, text);
        }

        #endregion // Public methods

        #region Protected override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override int VisualChildrenCount
        {
            get { return (null != Child)? 2 : 1; }
        }

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                if (Child != null)
                    yield return Child;

                yield return _busyHost;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (Child != null)
            {
                switch (index)
                {
                    case 0:
                        return Child;

                    case 1:
                        return _busyHost;
                }
            }
            else if (index == 0)
                return _busyHost;

            throw new IndexOutOfRangeException("index");
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size ret = new Size(0, 0);
            if (Child != null)
            {
                Child.Measure(constraint);
                ret = Child.DesiredSize;
            }

            _busyHost.Measure(constraint);

            return new Size(Math.Max(ret.Width, _busyHost.DesiredSize.Width), Math.Max(ret.Height, _busyHost.DesiredSize.Height));
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size ret = new Size(0, 0);
            if (Child != null)
            {
                Child.Arrange(new Rect(arrangeSize));
                ret = Child.RenderSize;
            }

            _busyHost.Arrange(new Rect(arrangeSize));

            return new Size(Math.Max(ret.Width, _busyHost.RenderSize.Width), Math.Max(ret.Height, _busyHost.RenderSize.Height));
        }

        #endregion Protected override methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _SetBinding(DependencyObject obj, DependencyProperty source, DependencyProperty target)
        {
            Binding b = new Binding();
            b.Source = this;
            b.Path = new PropertyPath(source);
            BindingOperations.SetBinding(obj, target, b);
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private readonly BackgroundVisualHost _busyHost = new BackgroundVisualHost();

        #endregion // Private members
    }
}
