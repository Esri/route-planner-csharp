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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using ESRI.ArcLogistics.App.Widgets;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_Header", Type = typeof(Label))]
    [TemplatePart(Name = "PART_HeaderButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ContentGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_ContentBorder", Type = typeof(Border))]

    /// Content control with drop-down content
    internal class ExpanderControl : HeaderedContentControl
    {
        #region Constructors & override methods

        static ExpanderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExpanderControl), new FrameworkPropertyMetadata(typeof(ExpanderControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _initEventHandlers();
            _CheckState();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets/sets content of control
        /// </summary>
        public FrameworkElement ContentOfExpander
        {
            get { return _Content; }
            set
            {
                _Content = value;

                if ((null != _Header))
                    _Header.Visibility = (_Content is QuickHelpWidget) ? Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Gets/sets collapsed property of control.
        /// </summary>
        public bool IsCollapsed
        {
            get { return _isCollapsed; }
            set
            {
                _isCollapsed = value;
                _CheckState();
            }
        }

        /// <summary>
        /// Event raised when button clicked
        /// </summary>
        public static readonly RoutedEvent ClickButtonEvent = EventManager.RegisterRoutedEvent("ClickButton",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ExpanderControl));

        /// <summary>
        /// adds/removes routed event handler for click event 
        /// </summary>
        public event RoutedEventHandler ClickButton
        {
            add { AddHandler(ExpanderControl.ClickButtonEvent, value); }
            remove { RemoveHandler(ExpanderControl.ClickButtonEvent, value); }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Expands control.
        /// </summary>
        public void ExpandControl()
        {
            if (_ContentGrid != null)
            {
                _HeaderButton.IsChecked = true;
                _isCollapsed = false;
                _ContentGrid.Visibility = Visibility.Visible;
                _ContentBorder.Visibility = Visibility.Visible;
                this.Margin = EXPANDED_MARGIN;
            }
        }

        /// <summary>
        /// Collapse control.
        /// </summary>
        protected void CollapseControl()
        {
            if (_ContentGrid != null)
            {
                _ContentGrid.Visibility = Visibility.Collapsed;
                _ContentBorder.Visibility = Visibility.Collapsed;
                _isCollapsed = true;
                _HeaderButton.IsChecked = false;
                this.Margin = COLLAPSED_MARGIN;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Inits components.
        /// </summary>
        protected void _InitComponents()
        {
            _Header = this.GetTemplateChild("PART_Header") as Label;
            _HeaderButton = this.GetTemplateChild("PART_HeaderButton") as ToggleButton;
            _ContentGrid = this.GetTemplateChild("PART_ContentGrid") as Grid;
            _ContentBorder = this.GetTemplateChild("PART_ContentBorder") as Border;
            _SetContent();

            _Header.Visibility = (_Content is QuickHelpWidget) ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// Add content to grid.
        /// </summary>
        protected void _SetContent()
        {
            if (_Content != null)
            {
                _ContentGrid.Children.Add(_Content);
                _gridHeight = _Content.Height;
                _ContentGrid.Height = _gridHeight;
                Height = _ContentGrid.Height + _HeaderButton.Height;
            }
        }

        /// <summary>
        /// Inits event handlers.
        /// </summary>
        protected void _initEventHandlers()
        {
            _HeaderButton.Checked += new RoutedEventHandler(_HeaderButton_Checked);
            _HeaderButton.Unchecked += new RoutedEventHandler(_HeaderButton_Unchecked);
            _Header.MouseUp += new MouseButtonEventHandler(_Header_MouseUp);
        }

        protected void _CheckState()
        {
            if (_isCollapsed)
                CollapseControl();
            else
                ExpandControl();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Calls when header button is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _HeaderButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _isCollapsed = true;
            this.RaiseEvent(new RoutedEventArgs(ExpanderControl.ClickButtonEvent));
        }

        /// <summary>
        /// Calls when header button is unchecked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _HeaderButton_Checked(object sender, RoutedEventArgs e)
        {
            _isCollapsed = false;
            _ContentGrid.Visibility = Visibility.Visible;
            _ContentBorder.Visibility = Visibility.Visible;
            this.RaiseEvent(new RoutedEventArgs(ExpanderControl.ClickButtonEvent));
        }

        void _Header_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Debug.Assert(null != _Content);
            QuickHelpWidget widget = _Content as QuickHelpWidget;
            if (null == widget)
                IsCollapsed = !IsCollapsed;
            else
            {
                if (!widget.ShowTopic())
                    IsCollapsed = !IsCollapsed;
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Collapsed control margin.
        /// </summary>
        private Thickness COLLAPSED_MARGIN = new Thickness(0, 0, 0, 2);

        /// <summary>
        /// Expanded control margin.
        /// </summary>
        private Thickness EXPANDED_MARGIN = new Thickness(0, 0, 0, 0);

        #endregion

        #region Private variables

        /// <summary>
        /// Header.
        /// </summary>
        private Label _Header = null;

        /// <summary>
        /// Header button.
        /// </summary>
        private ToggleButton _HeaderButton = null;

        /// <summary>
        /// Content container.
        /// </summary>
        private Grid _ContentGrid = null;

        /// <summary>
        /// Content.
        /// </summary>
        private FrameworkElement _Content = null;

        /// <summary>
        /// Content border.
        /// </summary>
        private Border _ContentBorder = null;

        /// <summary>
        /// Height of expanded grid.
        /// </summary>
        private double _gridHeight = 0;

        /// <summary>
        /// Collapsed state of control.
        /// </summary>
        private bool _isCollapsed = false;

        #endregion
    }
}
