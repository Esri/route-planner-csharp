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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Diagnostics;

using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "Weekly", Type = typeof(Grid))]
    [TemplatePart(Name = "PART_All", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_Range", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_StartDatePicker", Type = typeof(DatePicker))]
    [TemplatePart(Name = "PART_EndDatePicker", Type = typeof(DatePicker))]
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]

    // Days editor internal logic
    internal class DaysEditor : ComboBox
    {
        #region Constructors & override methods

        static DaysEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DaysEditor), new FrameworkPropertyMetadata(typeof(DaysEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitParts();
            _InitEventHandlers();
        }

        #endregion

        #region Public Properties

        public static readonly DependencyProperty Property =
            DependencyProperty.Register("Days", typeof(Days), typeof(DaysEditor));

        public Days Days
        {
            get { return (Days)GetValue(Property); }
            set
            {
                if (value != null)
                    SetValue(Property, value); 
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method transforms days value to control's components state
        /// </summary>
        private void _TransformDaysToControlState()
        {
            DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

            Debug.Assert(Days != null);

            _CellText.Text = Days.ToString();

            foreach (CheckBox checkbox in _WeeklyPanel.Children)
            {
                checkbox.Checked -= DayOfWeekEditor_Checked;
                checkbox.Unchecked -= DayOfWeekEditor_Checked;

                checkbox.IsChecked = Days.IsDayEnabled((DayOfWeek)checkbox.Tag);

                checkbox.Checked += new RoutedEventHandler(DayOfWeekEditor_Checked);
                checkbox.Unchecked += new RoutedEventHandler(DayOfWeekEditor_Checked);
            }

            // set range of days
            _StartDate.SelectedDateChanged -= _StartDate_SelectedDateChanged;
            if (!Days.From.HasValue && !Days.To.HasValue)
            {
                _AllButton.IsChecked = true;
                _RangeButton.IsChecked = false;
            }
            else
            {
                _AllButton.IsChecked = false;
                _RangeButton.IsChecked = true;

                _StartDate.SelectedDateChanged -= _StartDate_SelectedDateChanged;
                _StartDate.SelectedDate = (Days.From.HasValue) ? Days.From : Days.To;

                if (Days.To.HasValue && Days.From.HasValue)
                {
                    _EndDate.SelectedDateChanged -= _EndDate_SelectedDateChanged;
                    _EndDate.SelectedDate = Days.To;
                    _EndDate.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(_EndDate_SelectedDateChanged);
                }
           }
            _StartDate.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(_StartDate_SelectedDateChanged);
        }

        /// <summary>
        /// Method transforms control's components state to days value
        /// </summary>
        private void _TransformControlStateToDays()
        {
            if (null != Days)
            {
                foreach (CheckBox checkbox in _WeeklyPanel.Children)
                    Days.EnableDay((DayOfWeek)checkbox.Tag, (bool)checkbox.IsChecked);

                if ((bool)_AllButton.IsChecked)
                    Days.From = Days.To = null;
                else
                {
                    if ((null == _StartDate.SelectedDate) && (null != _EndDate.SelectedDate))
                    {
                        Days.From = _EndDate.SelectedDate;
                        Days.To = null;
                    }
                    else
                    {
                        if ((null != _StartDate.SelectedDate) && (null != _EndDate.SelectedDate) && (_StartDate.SelectedDate == _EndDate.SelectedDate))
                        {
                            Days.From = _StartDate.SelectedDate;
                            Days.To = null;
                        }
                        else
                        {
                            Days.From = _StartDate.SelectedDate;
                            Days.To = _EndDate.SelectedDate;
                        }
                    }
                }

                _CellText.Text = Days.ToString();
            }
        }

        /// <summary>
        /// Inits parts of control
        /// </summary>
        private void _InitParts()
        {
            _WeeklyPanel = (Grid)this.GetTemplateChild("Weekly");
            _CellText = (TextBlock)this.GetTemplateChild("PART_CellLabel");
            _PopupPanel = (Popup)this.GetTemplateChild("PART_PopupPanel");
            _AllButton = (RadioButton)this.GetTemplateChild("PART_All");
            _RangeButton = (RadioButton)this.GetTemplateChild("PART_Range");
            _EndDate = (DatePicker)this.GetTemplateChild("PART_EndDatePicker");
            _StartDate = (DatePicker)this.GetTemplateChild("PART_StartDatePicker");
            _TopLevelGrid = this.GetTemplateChild("PART_TopLevelGrid") as Grid;

            _InitWeekdayValues();
        }

        /// <summary>
        /// Method set values for weekly section checkboxes dependency of local settings
        /// </summary>
        private void _InitWeekdayValues()
        {
            DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            int i = (int)dateTimeFormat.FirstDayOfWeek;

            DayOfWeek[] days = (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek));
            foreach (Object child in _WeeklyPanel.Children)
            {
                if (child is CheckBox)
                {
                    ((CheckBox)child).Content = dateTimeFormat.DayNames[i];
                    ((CheckBox)child).Tag = (int)days[i];
                    ((CheckBox)child).Checked += new RoutedEventHandler(DayOfWeekEditor_Checked);
                    ((CheckBox)child).Unchecked += new RoutedEventHandler(DayOfWeekEditor_Checked);
                }

                i = (i == DAY_INDEX_MAX_VALUE)? 0 : i + 1;
            }
        }

        /// <summary>
        /// Method sets handlers to events
        /// </summary>
        private void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(DaysEditor_Loaded);
            this.KeyDown += new KeyEventHandler(DaysEditor_KeyDown);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_PreviewMouseLeftButtonDown);
            _PopupPanel.Opened += new EventHandler(_PopupPanel_Opened);
            _PopupPanel.Closed += new EventHandler(_PopupPanel_Closed);
            _CellText.Loaded += new RoutedEventHandler(_CellText_Loaded);

            _RangeButton.Checked += new RoutedEventHandler(_RangeButton_Checked);
            _AllButton.Checked += new RoutedEventHandler(_AllButton_Checked);

            _StartDate.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(_StartDate_SelectedDateChanged);
            _EndDate.SelectedDateChanged += new EventHandler<SelectionChangedEventArgs>(_EndDate_SelectedDateChanged);
        }

        #endregion

        #region Event Handlers

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

        private void DaysEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape || e.Key == Key.Enter) && _PopupPanel.IsOpen)
            {
                this.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        private void DaysEditor_Loaded(object sender, RoutedEventArgs e)
        {
            //NOTE: set property IsDropDownOpen to true for open control when it was loaded
            this.IsDropDownOpen = true;
        }

        private void _AllButton_Checked(object sender, RoutedEventArgs e)
        {
            _RangeButton.IsChecked = false;
            _TransformControlStateToDays();
        }

        private void _RangeButton_Checked(object sender, RoutedEventArgs e)
        {
            _AllButton.IsChecked = false;
            if (null == _StartDate.SelectedDate)
                _StartDate.SelectedDate = App.Current.CurrentDate;
            _TransformControlStateToDays();
        }

        private void _EndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_EndDate.SelectedDate != null)
            {
                _RangeButton.IsChecked = true;
                _AllButton.IsChecked = !_RangeButton.IsChecked;
                _TransformControlStateToDays();
            }
        }

        private void _StartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_StartDate.SelectedDate != null)
            {
                _RangeButton.IsChecked = true;
                _AllButton.IsChecked = !_RangeButton.IsChecked;
                _TransformControlStateToDays();
            }
        }

        private void DayOfWeekEditor_Checked(object sender, RoutedEventArgs e)
        {
            _TransformControlStateToDays();
        }

        private void _CellText_Loaded(object sender, RoutedEventArgs e)
        {
            if (null != Days)
                _CellText.Text = Days.ToString();
        }

        private void _PopupPanel_Opened(object sender, EventArgs e)
        {
            _TransformDaysToControlState();

            PopupPositionSynchronizer synchronizer = new PopupPositionSynchronizer(this, _PopupPanel);

            // Set popup's position.
            synchronizer.PositionPopupBelowCellEditor();
        }

        private void _PopupPanel_Closed(object sender, EventArgs e)
        {
            // NOTE : set focus to parent cell for support arrow keys navigation
            UIElement cell = XceedVisualTreeHelper.GetCellByEditor(this);
            if (cell != null)
                Keyboard.Focus(cell);
        }

        #endregion

        #region Private Fields

        // Controls.

        private Grid _WeeklyPanel;
        private TextBlock _CellText;
        private Popup _PopupPanel;

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _TopLevelGrid;

        private RadioButton _AllButton;
        private RadioButton _RangeButton;

        private DatePicker _StartDate;
        private DatePicker _EndDate;

        // max index of day name
        private const int DAY_INDEX_MAX_VALUE = 6;
        
        #endregion
    }
}
