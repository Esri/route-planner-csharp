using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Diagnostics;

using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// Interaction logic for DateRangeCalendarWidget.xaml
    /// </summary>
    internal partial class DateRangeCalendarWidget : PageWidgetBase
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public DateRangeCalendarWidget(string titleResourceName)
        {
            Debug.Assert(!string.IsNullOrEmpty(titleResourceName));

            InitializeComponent();

            _titleResourceName = titleResourceName;
        }

        #endregion // Constructors

        #region Public events
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Fires when selected dates changed.
        /// </summary>
        public event EventHandler SelectedDatesChanged;

        #endregion // Public events

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override void Initialize(ESRI.ArcLogistics.App.Pages.Page page)
        {
            base.Initialize(page);
            this.AllowDrop = true;
            _InitWidget();

            App.Current.CurrentDateChanged += new EventHandler(_App_CurrentDateChanged);
            _calendar.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(_SelectedDatesChanged);
        }

        #endregion // Public methods

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public override string Title
        {
            get { return (string)App.Current.FindResource(_titleResourceName); }
        }

        public DateTime StartDate
        {
            get { return _dayStart; }
        }

        public DateTime EndDate
        {
            get { return _dayEnd; }
        }

        #endregion // Public properties

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _App_CurrentDateChanged(object sender, EventArgs e)
        {
            // reinit calendar state
            _calendar.SelectedDatesChanged -= _SelectedDatesChanged;

            // NOTE: special issue
            //   calendar not clearing previous selected data range in view (highlight dates)
            //   if set in code selected date
            _calendar.SelectionMode = CalendarSelectionMode.SingleDate;
            _calendar.SelectionMode = CalendarSelectionMode.SingleRange;

            _calendar.SelectedDate = _calendar.DisplayDate = App.Current.CurrentDate;
            _dayStart = _dayEnd = _calendar.SelectedDate.Value;

            _calendar.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(_SelectedDatesChanged);

            _NotifySelectedDatesChanged();
        }

        private void _SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Current.CurrentDateChanged -= _App_CurrentDateChanged;

            // reorder date range - start day <= finish day
            _dayStart = DateTime.MaxValue;
            _dayEnd = DateTime.MinValue;
            for (int index = 0; index < _calendar.SelectedDates.Count; ++index)
            {
                DateTime date = _calendar.SelectedDates[index];
                if (date < _dayStart)
                    _dayStart = date;

                if (_dayEnd < date)
                    _dayEnd = date;
            }

            App.Current.CurrentDate = _dayStart;

            App.Current.CurrentDateChanged += new EventHandler(_App_CurrentDateChanged);

            _NotifySelectedDatesChanged();
        }

        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize calendar and all necessary event handlers
        /// </summary>
        private void _InitWidget()
        {
            _calendar = new ArcLogisticsCalendar();
            _calendar.SelectionMode = CalendarSelectionMode.SingleRange;
            _calendar.CalendarButtonStyle = null;
            _calendar.AllowDrop = false;
            _calendar.Style = (Style)App.Current.FindResource("calendarStyle");

            _calendar.SelectedDate = _calendar.DisplayDate = App.Current.CurrentDate;
            _dayStart = _dayEnd = _calendar.SelectedDate.Value;

            CalendarGrid.Children.Add(_calendar);
        }

        /// <summary>
        /// Fires when selected dates changed
        /// </summary>
        private void _NotifySelectedDatesChanged()
        {
            if (SelectedDatesChanged != null)
                SelectedDatesChanged(null, EventArgs.Empty);
        }

        #endregion // Private methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private ArcLogisticsCalendar _calendar = null;
        private string _titleResourceName = null;

        private DateTime _dayStart = DateTime.MinValue;
        private DateTime _dayEnd = DateTime.MinValue;

        #endregion // Private members
    }
}
