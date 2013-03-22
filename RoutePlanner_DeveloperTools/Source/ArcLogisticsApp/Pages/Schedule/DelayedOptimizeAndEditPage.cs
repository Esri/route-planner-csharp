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
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Allows working with "Optimize And Edit" page before it's real instance
    /// is initialized.
    /// </summary>
    internal sealed class DelayedOptimizeAndEditPage :
        IOptimizeAndEditPage
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the DelayedOptimizeAndEditPage class.
        /// </summary>
        /// <param name="application">Reference to the current application object.</param>
        public DelayedOptimizeAndEditPage(App application)
        {
            Debug.Assert(application != null);

            _application = application;
            _application.ApplicationInitialized += _ApplicationApplicationInitialized;
        }
        #endregion

        #region IOptimizeAndEditPage Members
        /// <summary>
        /// Fired when current schedule was changed.
        /// </summary>
        public event EventHandler CurrentScheduleChanged;

        /// <summary>
        /// Gets or sets current schedule.
        /// </summary>
        public Schedule CurrentSchedule
        {
            get
            {
                if (_optimizeAndEditPage != null)
                {
                    return _optimizeAndEditPage.CurrentSchedule;
                }

                return null;
            }
            set
            {
                if (_optimizeAndEditPage != null)
                {
                    _optimizeAndEditPage.CurrentSchedule = value;

                    return;
                }

                if (!_hasCurrentSchedule || _currentSchedule != value)
                {
                    _hasCurrentSchedule = true;

                    if (_currentSchedule != value)
                    {
                        _currentSchedule = value;
                        _NotifyCurrentScheduleChanged();
                    }
                }
            }
        }
        #endregion

        #region private methods
        /// <summary>
        /// Notifies about current schedule change.
        /// </summary>
        private void _NotifyCurrentScheduleChanged()
        {
            var temp = this.CurrentScheduleChanged;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles application initialized event.
        /// </summary>
        /// <param name="sender">Reference to the event sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void _ApplicationApplicationInitialized(object sender, EventArgs e)
        {
            _optimizeAndEditPage = (IOptimizeAndEditPage)_application.MainWindow.GetPage(
                PagePaths.SchedulePagePath);
            if (_hasCurrentSchedule)
            {
                _optimizeAndEditPage.CurrentSchedule = _currentSchedule;
            }

            _optimizeAndEditPage.CurrentScheduleChanged += (_s, _e) =>
                _NotifyCurrentScheduleChanged();
        }
        #endregion

        #region private fields
        /// <summary>
        /// Reference to the current application object.
        /// </summary>
        private App _application;

        /// <summary>
        /// Current schedule instance.
        /// </summary>
        private Schedule _currentSchedule;

        /// <summary>
        /// True if and only if the current schedule value was changed.
        /// </summary>
        private bool _hasCurrentSchedule;

        /// <summary>
        /// Actual "Optimize And Edit" page instance.
        /// </summary>
        private IOptimizeAndEditPage _optimizeAndEditPage;
        #endregion
    }
}
