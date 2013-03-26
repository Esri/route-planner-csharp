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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Internal logic for time window editor.
    /// Inherited from ComboBox to use it's internal logic of open/close dropdown panel.
    /// </summary>
    [TemplatePart(Name = "PART_IsWideopen", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_CellLabel", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_FromTime", Type = typeof(TimeTextBox))]
    [TemplatePart(Name = "PART_ToTime", Type = typeof(TimeTextBox))]
    [TemplatePart(Name = "PART_FromDay2", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_ToDay2", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]
    internal class CellTimeTextBox : Control
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        static CellTimeTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CellTimeWindowEditor), new FrameworkPropertyMetadata(typeof(CellTimeWindowEditor)));
        }

        #endregion

        #region Public Override Members

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
        }

        #endregion

        #region Public Properties

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("TimeWindow", typeof(TimeWindow), typeof(CellTimeWindowEditor));

        /// <summary>
        /// Gets/sets TimeWindow.
        /// </summary>
        public TimeWindow TimeWindow
        {
            get
            {
                return (TimeWindow)GetValue(TimeProperty);
            }
            set
            {
                SetValue(TimeProperty, value);
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Inits components.
        /// </summary>
        protected void _InitComponents()
        {
            _IsWideopen = this.GetTemplateChild("PART_IsWideopen") as CheckBox;
            _CellLabel = this.GetTemplateChild("PART_CellLabel") as TextBlock;
            _PopupPanel = this.GetTemplateChild("PART_PopupPanel") as Popup;
            _ToText = this.GetTemplateChild("PART_ToTime") as TimeTextBox;
            _FromText = this.GetTemplateChild("PART_FromTime") as TimeTextBox;
            _TopLevelGrid = this.GetTemplateChild("PART_TopLevelGrid") as Grid;
        }

        /// <summary>
        /// Inits event handlers.
        /// </summary>
        protected void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(_CellTimeWindowEditorLoaded);
            this.KeyDown += new KeyEventHandler(_CellTimeWindowEditorKeyDown);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            _PopupPanel.Opened += new EventHandler(_PopupPanelOpened);
            _PopupPanel.Closed += new EventHandler(_PopupPanelClosed);
            _FromText.TimeChanged += new RoutedEventHandler(_FromTextTimeChanged);
            _ToText.TimeChanged += new RoutedEventHandler(_ToTextTimeChanged);
        }

        /// <summary>
        /// Gets TimeWindow property and update control's layout accordingly.
        /// </summary>
        /// <param name="tw"></param>
        protected void _TimeWindowToControlState(TimeWindow tw)
        {
            if (tw != null)
                if (tw.IsWideOpen)
                    _IsWideopen.IsChecked = true;

            _ToText.Time = tw.To;
            _ToText.TimeSpan = _ToText.Time;
            _FromText.Time = tw.From;
            _FromText.TimeSpan = _FromText.Time;
            _CellLabel.Text = tw.ToString();
        }

        /// <summary>
        /// Transform values from control fields to TimeWindow value.
        /// </summary>
        protected void _GetTimeWindowFromControl()
        {
            TimeWindow tw = new TimeWindow();
            tw.IsWideOpen = (bool)_IsWideopen.IsChecked;
            tw.From = _FromText.Time;
            tw.To = _ToText.Time;

            TimeWindow = tw;
        }

        /// <summary>
        /// Sets changed text to cell TextBlock.
        /// </summary>
        protected void _SetCellLabelText()
        {
            _CellLabel.Text = TimeWindow.ToString();
        }

        #endregion

        #region Event Handlers

        private void _PopupPanelClosed(object sender, EventArgs e)
        {
            // NOTE : set focus to parent cell for support arrow keys navigation
            UIElement cell = XceedVisualTreeHelper.GetCellByEditor(this);
            if (cell != null)
                Keyboard.Focus(cell);
        }

        private void _CellTimeWindowEditorKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape || e.Key == Key.Enter) && _PopupPanel.IsOpen)
            {
                //this.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        private void _CellTimeWindowEditorLoaded(object sender, RoutedEventArgs e)
        {
            //NOTE: set property IsDropDownOpen to true for open control when it was loaded
            //this.IsDropDownOpen = true;

            if (TimeWindow != null)
            {
                _CellLabel.Text = TimeWindow.ToString();
            }
        }

        private void _ToTextTimeChanged(object sender, RoutedEventArgs e)
        {
            _GetTimeWindowFromControl();
            _SetCellLabelText();
        }

        private void _FromTextTimeChanged(object sender, RoutedEventArgs e)
        {
            _GetTimeWindowFromControl();
            _SetCellLabelText();
        }

        private void _PopupPanelOpened(object sender, EventArgs e)
        {
            // Update control state
            if (TimeWindow != null)
            {
                _TimeWindowToControlState(TimeWindow);
                _CellLabel.Text = TimeWindow.ToString();
                _FromText.InitStartText();
                _ToText.InitStartText();
            }

            PopupPositionSynchronizer synchronizer = new PopupPositionSynchronizer(this, _PopupPanel);

            // Set popup's position.
            synchronizer.PositionPopupBelowCellEditor();
        }

        private void _IsWideopen_Unchecked(object sender, RoutedEventArgs e)
        {
            _GetTimeWindowFromControl();
            _SetCellLabelText();
        }

        private void _IsWideopen_Checked(object sender, RoutedEventArgs e)
        {
            _GetTimeWindowFromControl();
            _SetCellLabelText();
        }

        /// <summary>
        /// Method closes popup if it is opened and user clicked outside the control.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If mouse is clicked outside the control and popup is shown - we need to close it, 
            // so popup will lose its focus and mouse left button down event will come to grid.
            if (!_TopLevelGrid.IsMouseOver && _PopupPanel.IsOpen)
                _PopupPanel.IsOpen = false;
        }

        #endregion

        #region Private Fields

        // Control's parts.
        private Popup _PopupPanel;
        protected TextBlock _CellLabel;
        private CheckBox _IsWideopen;
        private TimeTextBox _FromText;
        private TimeTextBox _ToText;

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _TopLevelGrid;

        #endregion
    }
}