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

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Class presents logic of calendar in barriers page. Dates with barriers should be highlighted by gray.
    /// </summary>
    internal class ArcLogisticsBarriersCalendar : Calendar
    {
        #region Constructors

        static ArcLogisticsBarriersCalendar()
        {
        }

        #endregion

        #region Override Members

        /// <summary>
        /// Applyes template. Initialites control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BarriersDayStatusesManager.Instance.DayStatusesChanged += new EventHandler(DayStatusesManager_DayStatusesChanged);
            this.DisplayDateChanged += new EventHandler<CalendarDateChangedEventArgs>(ArcLogisticsBarriersCalendar_DisplayDateChanged);
            this.CalendarItemStyle = (Style)App.Current.FindResource("MainCalendarItemStyle");

            App.Current.ProjectLoaded += new EventHandler(_ProjectLoaded);

            _UpdateCalendarView();
        }

        #endregion

        #region Event Handlers

        private void DayStatusesManager_DayStatusesChanged(object sender, EventArgs e)
        {
            _UpdateCalendarDayButtonStyle();
        }

        private void ArcLogisticsBarriersCalendar_DisplayDateChanged(object sender, CalendarDateChangedEventArgs e)
        {
            _UpdateCalendarView();
        }

        /// <summary>
        /// Update calendar view on project loaded.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ProjectLoaded(object sender, EventArgs e)
        {
            _UpdateCalendarView();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method updates template of each day and days with barriers highlight by other color 
        /// in accordance with Property Triggers defined in style
        /// </summary>
        private void _UpdateCalendarDayButtonStyle()
        {
            CalendarDayButtonStyle = null;
            CalendarDayButtonStyle = (Style)App.Current.FindResource("calendarDayButtonStyleDaysWithBarriersSupport");
        }

        /// <summary>
        /// Calculates start date, end date, updates collection of dates with barriers and calendar's template
        /// </summary>
        private void _UpdateCalendarView()
        {
            DateTime startDate = CalendarHelper.GetStartRangeDate(DisplayDate); // -1 week date
            DateTime endDate = CalendarHelper.GetEndRangeDate(DisplayDate); // + 2 weeks date
            BarriersDayStatusesManager.Instance.UpdateDayStatuses(startDate, endDate);
            _UpdateCalendarDayButtonStyle();
        }

        #endregion
    }
}
