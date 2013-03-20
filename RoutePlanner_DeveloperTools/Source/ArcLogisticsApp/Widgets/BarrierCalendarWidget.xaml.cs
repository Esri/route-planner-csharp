using System;
using System.Collections.Generic;
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
using System.Diagnostics;

using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// Interaction logic for BarrierCalendarWidget.xaml
    /// </summary>
    internal partial class BarrierCalendarWidget : PageWidgetBase
    {
        #region Constructors

        public BarrierCalendarWidget()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Methods

        public override void Initialize(ESRI.ArcLogistics.App.Pages.Page page)
        {
            base.Initialize(page);
            this.AllowDrop = true;
            _InitCalendar();

            this.Loaded += new RoutedEventHandler(CalendarWidget_Loaded);
            this.Unloaded += new RoutedEventHandler(CalendarWidget_Unloaded);

            if (_calendar.SelectedDate != App.Current.CurrentDate)
                _calendar.SelectedDate = App.Current.CurrentDate;
            App.Current.CurrentDateChanged += new EventHandler(CalendarWidgetApp_CurrentDateChanged);
        }

        #endregion

        #region Public Properties

        public override string Title
        {
            get { return (string)App.Current.FindResource("CalendarWidgetCaption"); }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initialize calendar and all necessary event handlers
        /// </summary>
        protected void _InitCalendar()
        {
            _calendar = new ArcLogisticsBarriersCalendar();
            _calendar.SelectionMode = CalendarSelectionMode.SingleDate;
            CalendarGrid.Children.Add(_calendar);
            _calendar.CalendarButtonStyle = null;
            _calendar.Style = (Style)App.Current.FindResource("calendarStyle");
        }

        #endregion

        #region Event handlers

        private void calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Current.CurrentDateChanged -= CalendarWidgetApp_CurrentDateChanged;

            App.Current.CurrentDate = (DateTime)_calendar.SelectedDate;

            App.Current.CurrentDateChanged += new EventHandler(CalendarWidgetApp_CurrentDateChanged);
        }

        private void CalendarWidget_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.Assert(_calendar.SelectedDate != null);

            _calendar.DisplayDate = (DateTime)_calendar.SelectedDate;

            _calendar.SelectedDatesChanged += new EventHandler<SelectionChangedEventArgs>(calendar_SelectedDatesChanged);
        }

        private void CalendarWidget_Unloaded(object sender, RoutedEventArgs e)
        {
            _calendar.SelectedDatesChanged -= calendar_SelectedDatesChanged;
        }

        private void CalendarWidgetApp_CurrentDateChanged(object sender, EventArgs e)
        {
            _calendar.SelectedDatesChanged -= calendar_SelectedDatesChanged;

            _calendar.SelectedDate = App.Current.CurrentDate;
            _calendar.DisplayDate = (DateTime)App.Current.CurrentDate;

            _calendar.SelectedDatesChanged += calendar_SelectedDatesChanged;
        }

        #endregion

        #region Private members

        private ArcLogisticsBarriersCalendar _calendar;

        #endregion
    }
}
