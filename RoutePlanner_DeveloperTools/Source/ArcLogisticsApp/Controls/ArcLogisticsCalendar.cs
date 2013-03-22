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
using System.Windows.Input;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using ESRI.ArcLogistics.App.Pages;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Overrided calendar. Allows to support different style for "Routed" days
    /// </summary>
    internal class ArcLogisticsCalendar : Calendar
    {
        #region Constructors

        static ArcLogisticsCalendar()
        {
        }

        #endregion

        #region Public Override Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DayStatusesManager.Instance.DayStatusesChanged += new EventHandler(OnDayStatusesChanged);
            this.DisplayDateChanged += new EventHandler<CalendarDateChangedEventArgs>(ArcLogisticsCalendar_DisplayDateChanged);
            App.Current.ProjectLoaded += new EventHandler(Current_ProjectLoaded);

            // Update view after first calendar loading
            Mouse.OverrideCursor = Cursors.Wait;
            
            _UpdateCalendarView();
            Mouse.OverrideCursor = null;

            this.CalendarItemStyle = (Style)App.Current.FindResource("MainCalendarItemStyle");
        }

        #endregion

        #region Private Event Handlers

        private void Current_ProjectLoaded(object sender, EventArgs e)
        {
            // Update view after new project loaded
            Mouse.OverrideCursor = Cursors.Wait;

            _UpdateCalendarView();
            Mouse.OverrideCursor = null;
        }

        protected override void OnSelectedDatesChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectedDatesChanged(e);
            Mouse.Capture(null);
        }

        /// <summary>
        /// Occurs when DayStatusesManager raises DayStatusesChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDayStatusesChanged(object sender, EventArgs e)
        {
            // if event occurs after Starting or Completing routing operation we need to update style 
            _UpdateCalendarDayButtonStyle();
        }

        /// <summary>
        /// Occurs when user select other month/year
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArcLogisticsCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            WorkingStatusHelper.SetBusy(null);          
            _UpdateCalendarView();
            WorkingStatusHelper.SetReleased();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates start date, end date, updates collection of routed dates and calendar's template
        /// </summary>
        private void _UpdateCalendarView()
        {
            DateTime startDate = _GetStartRangeDate(); // -1 week date
            DateTime endDate = _GetEndRangeDate(); // + 2 weeks date
            DayStatusesManager.Instance.InitDayStatuses(startDate, endDate);
            _UpdateCalendarDayButtonStyle();
        }

        /// <summary>
        /// Method updates template of each day and "Routed" days highlight by other color 
        /// in accordance with Property Triggers defined in style
        /// </summary>
        private void _UpdateCalendarDayButtonStyle()
        {
            CalendarDayButtonStyle = null;
            CalendarDayButtonStyle = (Style)App.Current.FindResource("calendarDayButtonStyleWuthRoutedDaysSupport");
        }

        /// <summary>
        /// Returns start date of calendar range (the 1st day of last week in previous month)
        /// </summary>
        /// <returns></returns>
        private DateTime _GetStartRangeDate()
        {
            return CalendarHelper.GetStartRangeDate(DisplayDate);
        }

        /// <summary>
        /// Returns end date of calendar range (the 14th of next month)
        /// </summary>
        /// <returns></returns>
        private DateTime _GetEndRangeDate()
        {
            return CalendarHelper.GetEndRangeDate(DisplayDate);
        }

        #endregion
    }
}
