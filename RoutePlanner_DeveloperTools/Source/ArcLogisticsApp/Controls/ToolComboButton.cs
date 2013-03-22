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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ESRI.ArcLogistics.App.Tools;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Control for button with popup menu.
    /// </summary>
    [TemplatePart(Name = "PART_HeaderButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ContentGrid", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_Menu", Type = typeof(ContextMenu))]
    internal class ToolComboButton : Control
    {
        #region Constructors

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ToolComboButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolComboButton), new FrameworkPropertyMetadata(typeof(ToolComboButton)));
        }
        
        #endregion

        #region Public events

        /// <summary>
        /// Tool activated by button.
        /// </summary>
        public event EventHandler ToolActivated;
        
        #endregion

        #region Public members

        /// <summary>
        /// Selected tool.
        /// </summary>
        public IMapTool SelectedTool
        {
            get;
            private set;
        }
        
        #endregion

        #region Public methods


        /// <summary>
        /// menu
        /// </summary>
        private ContextMenu _menu;

        /// <summary>
        /// Control template apply.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            IsEnabled = false;

            _headerButton = this.GetTemplateChild("PART_HeaderButton") as ToggleButton;

            _menu = this.GetTemplateChild("PART_Menu") as ContextMenu;
            _menu.PlacementTarget = _headerButton;
            _menu.Closed += new RoutedEventHandler(_MenuClosed);
            _menu.MouseMove += new MouseEventHandler(_MenuMouseMove);

            MouseEnter += new MouseEventHandler(_MouseEnter);

            
            SelectedTool = _tools[0];

            _headerButton.Style = (Style)App.Current.FindResource("MapToolButtonStyle");
            _headerButton.IsEnabled = false;
            _headerButton.Click += new RoutedEventHandler(_headerButton_Click);

            Image imgHeaderButton = new Image();
            imgHeaderButton.Margin = (Thickness)App.Current.FindResource("ToolButtonImageMargin");
            imgHeaderButton.VerticalAlignment = VerticalAlignment.Center;
            imgHeaderButton.HorizontalAlignment = HorizontalAlignment.Center;
            _headerButton.Content = imgHeaderButton;

            for (int i = 0; i < _tools.Count; i++)
            {
                _AddTool(_tools[i]);
            }

            _InitHeaderButtonByTool(SelectedTool);
        }

        /// <summary>
        /// Init button.
        /// </summary>
        /// <param name="tools">Tools which can be selected.</param>
        public void Init(IMapTool[] tools)
        {
            _tools = new List<IMapTool>(tools);
        }

        /// <summary>
        /// Set header button is checked.
        /// </summary>
        /// <param name="isChecked">Is header button checked.</param>
        public void Check(bool isChecked)
        {
            _headerButton.IsChecked = isChecked;
        }

        /// <summary>
        /// Set header button is enabled.
        /// </summary>
        /// <param name="isEnabled">Is header button enabled.</param>
        public void Enable(bool isEnabled)
        {
            if (_headerButton != null)
            {
                IsEnabled = isEnabled;
                _headerButton.IsEnabled = isEnabled;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Add tool button.
        /// </summary>
        /// <param name="tool">Tool to assign with button.</param>
        private void _AddTool(IMapTool tool)
        {
            // Create bitmap image.
            BitmapImage bitmap = new BitmapImage(new Uri(tool.IconSource, UriKind.Relative));
            Image img = new Image();
            img.Source = bitmap;

            MenuItem item = new MenuItem();
            item.Header = tool.Title;
            item.Click += new RoutedEventHandler(_ItemClick);
            item.Icon = img;
            _menu.Items.Add(item);
        }

        /// <summary>
        /// React on button click.
        /// </summary>
        /// <param name="sender">Clicked button.</param>
        /// <param name="e">Ignored.</param>
        private void _ItemClick(object sender, RoutedEventArgs e)
        {
            MenuItem button = sender as MenuItem;
            if (button.Tag != _headerButton)
            {
                // If hided tool selected from popup - init header button by selected tool.
                int index = _menu.Items.IndexOf((MenuItem)sender);
                if (SelectedTool != _tools[index])
                {
                    SelectedTool = _tools[index];
                    _InitHeaderButtonByTool(SelectedTool);
                }

                button.IsChecked = false;
            }

            // Activate selected tool.
            if (ToolActivated != null)
                ToolActivated(this, EventArgs.Empty);
        }

        void _headerButton_Click(object sender, RoutedEventArgs e)
        {
            // Activate selected tool.
            if (ToolActivated != null)
                ToolActivated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Init header button view by tool.
        /// </summary>
        /// <param name="tool">Tool to assign with header button.</param>
        private void _InitHeaderButtonByTool(IMapTool tool)
        {
            // Set tool tip.
            _headerButton.ToolTip = tool.TooltipText;

            // Set image.
            Image headerButtonImage = _headerButton.Content as Image;
            BitmapImage bitmap = new BitmapImage(new Uri(tool.IconSource, UriKind.Relative));
            headerButtonImage.Source = bitmap;
        }

        /// <summary>
        /// React on mouse enter. Show dropdown.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsEnabled && !_showed)
            {
                _menu.IsOpen = true;
                _showed = true;
                App.Current.MainWindow.PreviewMouseDown += new MouseButtonEventHandler(_MainWindowPreviewMouseDown);
            }
        }

        /// <summary>
        /// Close popup on click on window.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MainWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_menu.IsOpen)
            {
                _menu.IsOpen = false;
            }
        }

        private void _MenuClosed(object sender, RoutedEventArgs e)
        {
            _showed = false;
            App.Current.MainWindow.PreviewMouseDown -= new MouseButtonEventHandler(_MainWindowPreviewMouseDown);
        }

        /// <summary>
        /// When mouse move on tool combo button or its context menu - analyze pointer position.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">MouseEventArgs.</param>
        void _MenuMouseMove(object sender, MouseEventArgs e)
        {
            if (_menu.IsVisible)
            {
                // If mouse pointer is inside toolbutton - do nothing.
                var pointInToolButtonCoords = e.GetPosition(this);
                if (pointInToolButtonCoords.X > -EPSILON && pointInToolButtonCoords.X < ActualWidth + EPSILON &&
                    pointInToolButtonCoords.Y > -EPSILON && pointInToolButtonCoords.Y < ActualHeight + EPSILON)
                    return;

                // Check that mouse pointer is inside context menu.
                var pointInMenuCoords = e.GetPosition(_menu);
                // If it isnt - close context menu.
                if (pointInMenuCoords.X < -EPSILON || pointInMenuCoords.Y < -EPSILON ||
                    pointInMenuCoords.X > _menu.ActualWidth + EPSILON ||
                    pointInMenuCoords.Y > _menu.ActualHeight + EPSILON)
                    _menu.IsOpen = false;
            }
        }

        #endregion

        #region private constant

        /// <summary>
        /// Tolerated error for mouse position.
        /// </summary>
        private const double EPSILON = 1;

        #endregion

        #region Private fields

        /// <summary>
        /// Header button on map tools panel.
        /// </summary>
        private ToggleButton _headerButton;

        /// <summary>
        /// Tools list.
        /// </summary>
        private List<IMapTool> _tools;

        /// <summary>
        /// Is popup expanded.
        /// </summary>
        private bool _showed;

        #endregion
    }
}
