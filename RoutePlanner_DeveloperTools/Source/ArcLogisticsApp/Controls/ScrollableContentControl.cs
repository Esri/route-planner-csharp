using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_LeftButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_RightButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_ContentStack", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
    // control with two arrows to scroll content to left or right
    internal class ScrollableContentControl : Control
    {
        static ScrollableContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollableContentControl), new FrameworkPropertyMetadata(typeof(ScrollableContentControl)));
        }
       
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
        }

        #region Public Properties

        /// <summary>
        /// Returns width of enlarged button
        /// </summary>
        public double EnlargedWidth
        {
            get { return _lastItemWidth * _enlargeIndex; }
        }

        /// <summary>
        /// Returns width of button in normal state.
        /// </summary>
        public double DefaultWidth
        {
            get { return _lastItemWidth; }
        }

        public UIElementCollection ContentItems
        {
            get { return _contentStack.Children; }
        }

        #endregion

        #region Public Methods

        public void AddContentItem(ToggleButton item)
        {
            _contentStack.Children.Add(item);
            item.Checked += new RoutedEventHandler(item_Checked);
            item.Unchecked += new RoutedEventHandler(item_Unchecked);
            _ResizeContentItems();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Calculates and sets new sizes of items.
        /// </summary>
        protected void _ResizeContentItems()
        {
            double calculatedItemWidth = _scrollViewer.ActualWidth / (_contentStack.Children.Count + 0.6);

            foreach (Control item in _contentStack.Children)
            {
                if (calculatedItemWidth >= _itemMinWidth)
                {
                    item.Width = calculatedItemWidth;
                    _HideScrollButtons();
                }
                else
                {
                    item.Width = _itemMinWidth;
                    _ShowScrollButtons();
                }
                _lastItemWidth = item.Width;
            }
        }

        /// <summary>
        /// Shows scroll buttons.
        /// </summary>
        protected void _ShowScrollButtons()
        {
            _btnLeftButton.Visibility = Visibility.Visible;
            _btnRightButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// hies scroll buttons.
        /// </summary>
        protected void _HideScrollButtons()
        {
            _btnLeftButton.Visibility = Visibility.Collapsed;
            _btnRightButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Initializes control's parts.
        /// </summary>
        protected void _InitComponents()
        {
            _btnLeftButton = this.GetTemplateChild("PART_LeftButton") as RepeatButton;
            _btnRightButton = this.GetTemplateChild("PART_RightButton") as RepeatButton;
            _contentStack = this.GetTemplateChild("PART_ContentStack") as StackPanel;
            _scrollViewer = this.GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        }

        /// <summary>
        /// Initializes event handlers.
        /// </summary>
        protected void _InitEventHandlers()
        {
            _btnLeftButton.Click += new RoutedEventHandler(_btnLeftButton_Click);
            _btnRightButton.Click += new RoutedEventHandler(_btnRightButton_Click);
            this.MouseWheel += new MouseWheelEventHandler(ScrollableContentControl_MouseWheel);
            _contentStack.MouseWheel += ScrollableContentControl_MouseWheel;
            this.SizeChanged += new SizeChangedEventHandler(ScrollableContentControl_SizeChanged);
        }
      
        #endregion

        #region Event handlers

        void item_Unchecked(object sender, RoutedEventArgs e)
        {
            ((ToggleButton)sender).Width = _lastItemWidth;
        }

        void item_Checked(object sender, RoutedEventArgs e)
        {
            foreach (ToggleButton button in _contentStack.Children)
            {
                if (button == (ToggleButton)sender)
                    button.Width = _lastItemWidth * 1.2;
            }
        }

        void ScrollableContentControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                _scrollViewer.LineLeft();
            else if (e.Delta < 0)
                _scrollViewer.LineRight();
        }

        void ScrollableContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _ResizeContentItems();

            foreach (ToggleButton btn in _contentStack.Children)
            {
                if ((bool)btn.IsChecked)
                    btn.Width = EnlargedWidth;
            }
        }

        void _btnRightButton_Click(object sender, RoutedEventArgs e)
        {
            _scrollViewer.LineRight();
        }

        void _btnLeftButton_Click(object sender, RoutedEventArgs e)
        {
            _scrollViewer.LineLeft();
        }

        #endregion

        #region Private Fields

        // Left arrow button
        private RepeatButton _btnLeftButton;
        // Right arrow button.
        private RepeatButton _btnRightButton;
        // Stack panes with content.
        private StackPanel _contentStack;

        private ScrollViewer _scrollViewer;

        private double _itemMinWidth = 100;

        private double _lastItemWidth = 100;

        private double _enlargeIndex = 1.2;

        #endregion
    }
}
