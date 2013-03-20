using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.DragAndDrop;

namespace ESRI.ArcLogistics.App.Widgets
{
    // APIREV: make internal
    /// <summary>
    /// Widget that allows a user to change the current date.
    /// </summary>
    public partial class CalendarWidget : PageWidgetBase
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of the <c>CalendarWidget</c> class.
        /// </summary>
        public CalendarWidget()
        {
            InitializeComponent();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Initialize the instance.
        /// </summary>
        /// <param name="page"></param>
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
        /// <summary>
        /// Gets the title of the calendar widget.
        /// </summary>
        public override string Title
        {
            get 
            {
                return (string)App.Current.FindResource("CalendarWidgetCaption"); 
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initialize calendar and all necessary event handlers.
        /// </summary>
        protected void _InitCalendar()
        {
            _calendar = new ArcLogisticsCalendar();
            _calendar.SelectionMode = CalendarSelectionMode.SingleDate;
            CalendarGrid.Children.Add(_calendar);
            _calendar.AllowDrop = false;
            _calendar.CalendarButtonStyle = null;
            _calendar.Drop += new DragEventHandler(calendar_Drop);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Fired when an object is dropped on the calendar.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private  void calendar_Drop(object sender, DragEventArgs e)
        {
            DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();
            FrameworkElement targetFrameworkElement = (FrameworkElement)e.OriginalSource;
            Object context = targetFrameworkElement.DataContext;

            while (!(context is DateTime))
            {
                targetFrameworkElement = (FrameworkElement)VisualTreeHelper.GetParent(targetFrameworkElement);
                context = targetFrameworkElement.DataContext;
                Debug.Assert(context != null); // context shouldn't be null
            }

            DateTime targetDate = (DateTime)context;       

            if (targetDate != null)
                dragAndDropHelper.DropOnDate(targetDate, e.Data);
        }

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

        private void CalendarWidgetApp_CurrentDateChanged(object sender, EventArgs e)
        {
            _calendar.SelectedDatesChanged -= calendar_SelectedDatesChanged;

            _calendar.SelectedDate = App.Current.CurrentDate;
            _calendar.DisplayDate = (DateTime)App.Current.CurrentDate;

            _calendar.SelectedDatesChanged += calendar_SelectedDatesChanged;
        }

        private void CalendarWidget_Unloaded(object sender, RoutedEventArgs e)
        {
            _calendar.SelectedDatesChanged -= calendar_SelectedDatesChanged;
        }

        #endregion

        #region Private members

        private ArcLogisticsCalendar _calendar;

        #endregion
    }
}
