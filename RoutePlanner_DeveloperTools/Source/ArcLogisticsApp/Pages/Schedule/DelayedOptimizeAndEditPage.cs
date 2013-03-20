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
