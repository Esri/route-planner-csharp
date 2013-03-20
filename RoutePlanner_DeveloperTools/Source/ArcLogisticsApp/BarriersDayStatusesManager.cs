using System;
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Windows.Forms;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class creates and updates collection of days with barriers (Singletone)
    /// </summary>
    internal class BarriersDayStatusesManager
    {
        #region Static Properties

        static BarriersDayStatusesManager()
        {
            _barriersDayStatusesManager = new BarriersDayStatusesManager();
        }

        /// <summary>
        /// Gets singletone instance
        /// </summary>
        public static BarriersDayStatusesManager Instance
        {
            get
            {
                if (_barriersDayStatusesManager == null)
                    _barriersDayStatusesManager = new BarriersDayStatusesManager();

                return _barriersDayStatusesManager;
            }
        }

        #endregion

        #region Constructors

        private BarriersDayStatusesManager()
        {
            if (App.Current.Project != null)
            {
                _barriersCollection = App.Current.Project.Barriers.SearchAll(true); // get all barriers from current project
                ((INotifyCollectionChanged)_barriersCollection).CollectionChanged += new NotifyCollectionChangedEventHandler(BarriersDayStatusesManager_CollectionChanged);
            }

            App.Current.ProjectLoaded += new EventHandler(Current_ProjectLoaded);
            App.Current.ProjectClosed += new EventHandler(Current_ProjectClosed);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Collection of dates with barriers
        /// </summary>
        public ReadOnlyCollection<DateTime> DayStatuses
        {
            get
            {
                return _dayStatuses.AsReadOnly();
            }
        }

        /// <summary>
        /// Raises when collection of days with barriers changed
        /// </summary>
        public event EventHandler DayStatusesChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Method updates collection of day statuses. Should be called when calendar dates range changed
        /// </summary>
        /// <param name="calendarStartDate"></param>
        /// <param name="calendarEndDate"></param>
        public void UpdateDayStatuses(DateTime calendarStartDate, DateTime calendarEndDate)
        {
            _calendarStartDate = calendarStartDate;
            _calendarEndDate = calendarEndDate;

            foreach (Barrier barrier in _barriersCollection)
                _AddDateStatusesRange(barrier);
        }

        /// <summary>
        /// Replaces barrier after editing
        /// </summary>
        /// <param name="barrier"></param>
        public void OnBarrierChanged()
        {
            _dayStatuses.Clear();

            foreach (Barrier barrier in _barriersCollection)
                _AddDateStatusesRange(barrier);

            // raise event
            if (DayStatusesChanged != null)
                DayStatusesChanged(null, EventArgs.Empty);
        }

        #endregion

        #region Private Menthods

        /// <summary>
        /// Method adds dates statuses in necessary range. Barriers start and end dates should not be null
        /// </summary>
        /// <param name="calendarStartDate"></param>
        /// <param name="calendarEndDate"></param>
        /// <param name="barrier"></param>
        private void _AddDateStatusesRange(Barrier barrier)
        {
            // start date and finish date cannot be null
            Debug.Assert(barrier.StartDate != null);
            Debug.Assert(barrier.FinishDate != null);

            // if barriers dates are out of range of current calendar dates - return
            if (barrier.FinishDate < _calendarStartDate || barrier.StartDate > _calendarEndDate)
                return;

            ICollection<DateTime> barrierDates = _GetBarrierDatesRange(barrier); // get callection of all barriers dates

            foreach (DateTime barrierDate in barrierDates)
            {
                // if immediate date is in necessary range - change dictionary value
                if (barrierDate >= _calendarStartDate && barrierDate <= _calendarEndDate)
                {
                    if (!_dayStatuses.Contains(barrierDate))
                        _dayStatuses.Add(barrierDate);
                }
            }
        }

        /// <summary>
        /// Method returns collection of barriers dates. If any of dates is null - returns null
        /// </summary>
        /// <param name="barrier"></param>
        /// <returns></returns>
        private ICollection<DateTime> _GetBarrierDatesRange(Barrier barrier)
        {
            if (barrier.FinishDate == null || barrier.StartDate == null)
                return null;

            ICollection<DateTime> dates = new Collection<DateTime>();

            DateTime date = (DateTime)barrier.StartDate;

            while (date <= barrier.FinishDate)
            {
                dates.Add(date);
                date = date.AddDays(1);
            }

            return dates;
        }

        #endregion

        #region Event Handlers

        private void Current_ProjectClosed(object sender, EventArgs e)
        {
            ((INotifyCollectionChanged)_barriersCollection).CollectionChanged -= BarriersDayStatusesManager_CollectionChanged;
            _dayStatuses.Clear();
        }

        private void Current_ProjectLoaded(object sender, EventArgs e)
        {
            _barriersCollection = App.Current.Project.Barriers.SearchAll(true); // get all barriers from application
            ((INotifyCollectionChanged)_barriersCollection).CollectionChanged += new NotifyCollectionChangedEventHandler(BarriersDayStatusesManager_CollectionChanged);
        }

        private void BarriersDayStatusesManager_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _dayStatuses.Clear();

            foreach (Barrier barrier in _barriersCollection)
                _AddDateStatusesRange(barrier);

            // raise event
            if (DayStatusesChanged != null)
                DayStatusesChanged(null, EventArgs.Empty);
        }

        #endregion

        #region Private Fields

        private static BarriersDayStatusesManager _barriersDayStatusesManager;

        private ICollection<Barrier> _barriersCollection = null; // all barriers       

        //Collection of dates with barriers
        private List<DateTime> _dayStatuses = new List<DateTime>();

        // current calendar dates bounds
        private DateTime _calendarStartDate;
        private DateTime _calendarEndDate;

        #endregion
    }
}
