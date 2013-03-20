using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_Hours", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Minutes", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_AmPm", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_IncrementButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DecrementButton", Type = typeof(RepeatButton))]
    
    // Control for show time in format hh:mm
    internal class TimeTextBox : Control
    {
        #region Constructor
        
        /// <summary>
        /// Constructor.
        /// </summary>
        static TimeTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeTextBox), 
                new FrameworkPropertyMetadata(typeof(TimeTextBox)));
        } 

        #endregion

        #region Public methods

        /// <summary>
        /// Occurs when template applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
        } 

        #endregion

        #region PublicProperties

        /// <summary>
        /// TimeSpan DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty TimeSpanProperty =
            DependencyProperty.Register("TimeSpan", typeof(TimeSpan), typeof(TimeTextBox));

        /// <summary>
        /// Gets/sets TimeSpan DependencyProperty.
        /// Do not use this property simultaneously with 'Time' property.
        /// </summary>
        public TimeSpan TimeSpan
        {
            get
            {
                return (TimeSpan)GetValue(TimeSpanProperty);
            }
            set
            {
                SetValue(TimeSpanProperty, value);
            }
        }

        /// <summary>
        /// Time value changed event.
        /// </summary>
        public static readonly RoutedEvent TimeChangedEvent = EventManager.RegisterRoutedEvent("TimeChanged",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimeTextBox));

        /// <summary>
        /// Time changed event handler.
        /// </summary>
        public event RoutedEventHandler TimeChanged
        {
            add { AddHandler(TimeTextBox.TimeChangedEvent, value); }
            remove { RemoveHandler(TimeTextBox.TimeChangedEvent, value); }
        }

        /// <summary>
        /// Time property.
        /// Do not use this property simultaneously with 'TimeSpan' property.
        /// </summary>
        public TimeSpan Time
        {
            get
            {
                if (!String.IsNullOrEmpty(_HoursBox.Text))
                    _formattedHoursValue = Convert.ToInt32(_HoursBox.Text);

                if (_formattedHoursValue == _maxValueOfHours || _HoursBox.Text == "" || _HoursBox.Text == " " || _HoursBox.Text == "  ")
                    _formattedHoursValue = _defaultFirstHourIn24HourDay;

                 if (_MinutesBox.Text == "" || _MinutesBox.Text == " " || _MinutesBox.Text == "  ")
                    _minutesValue = _minValueOfMinutes;

                if (!String.IsNullOrEmpty(_MinutesBox.Text))
                    _minutesValue = Convert.ToInt32(_MinutesBox.Text);

                _ts = new TimeSpan(_formattedHoursValue + _pmConvertionIndex, _minutesValue, 0);
                return _ts;
            }
            set
            {
                // Time property is in use.
                _isTimePropertyInUse = true;

                // Set time span.
                _ts = value;

                // Init time, which is shown in control.
                _InitTime();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets input focus to hours box.
        /// </summary>
        public void SetFocus()
        {
            _HoursBox.Focus();
        }

        #endregion

        #region Proptected Methods

        /// <summary>
        /// Inits collection of buttons with degits.
        /// </summary>
        protected void _InitNumKeysCollection()
        {
            _numKeysCollection = new Collection<Key>();

            _numKeysCollection.Add(Key.NumPad0);
            _numKeysCollection.Add(Key.NumPad1);
            _numKeysCollection.Add(Key.NumPad2);
            _numKeysCollection.Add(Key.NumPad3);
            _numKeysCollection.Add(Key.NumPad4);
            _numKeysCollection.Add(Key.NumPad5);
            _numKeysCollection.Add(Key.NumPad6);
            _numKeysCollection.Add(Key.NumPad7);
            _numKeysCollection.Add(Key.NumPad8);
            _numKeysCollection.Add(Key.NumPad9);

            _numKeysCollection.Add(Key.D0);
            _numKeysCollection.Add(Key.D1);
            _numKeysCollection.Add(Key.D2);
            _numKeysCollection.Add(Key.D3);
            _numKeysCollection.Add(Key.D4);
            _numKeysCollection.Add(Key.D5);
            _numKeysCollection.Add(Key.D6);
            _numKeysCollection.Add(Key.D7);
            _numKeysCollection.Add(Key.D8);
            _numKeysCollection.Add(Key.D9);
        }

        /// <summary>
        /// Format time text in compliance with Local Culture settings and sets it in controls.
        /// </summary>
        protected void _SetFormattedText()
        {
            _MinutesBox.Text = _formattedDate.ToString("mm", _dateTimeFormat);

            if (!String.IsNullOrEmpty(_MinutesBox.Text))
                _oldMinutesText = _MinutesBox.Text;

            String hoursFormat = _dateTimeFormat.ShortTimePattern.Substring(0, 1);
            hoursFormat += hoursFormat;

            _HoursBox.Text = _formattedDate.ToString(hoursFormat, _dateTimeFormat);

            if (!string.IsNullOrEmpty(_HoursBox.Text))
                _oldHoursText = _HoursBox.Text;

            if (_dateTimeFormat.ShortTimePattern.Contains("tt"))
            {
                _AmPm.Text = _formattedDate.ToString("tt", _dateTimeFormat);
                _AmPm.Visibility = Visibility.Visible;

                _pmConvertionIndex =
                    (_AmPm.Text == _dateTimeFormat.PMDesignator)? _fullPmIndex : _nullPmIndex;
            }
            else
            {
                _AmPm.Text = "";
                _AmPm.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Itialize text in text boxes when control opens at first time.
        /// </summary>
        public void InitStartText()
        {
            _SetFormattedText();
            _AmPm.TextChanged += new TextChangedEventHandler(_AmPm_TextChanged);
        }

        /// <summary>
        /// Initialize components of control.
        /// </summary>
        protected void _InitComponents()
        {
            _AmPm = this.GetTemplateChild("PART_AmPm") as TextBox;
            _AmPm.Width = 26;
            _HoursBox = this.GetTemplateChild("PART_Hours") as TextBox;
            _MinutesBox = this.GetTemplateChild("PART_Minutes") as TextBox;

            _isHoursFocused = true;
            _isAmPmFocused = false;

            _IncrementButton = this.GetTemplateChild("PART_IncrementButton") as RepeatButton;
            _DecrementButton = this.GetTemplateChild("PART_DecrementButton") as RepeatButton;

            _dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

            _GetDefaultTimeValues();
            _InitNumKeysCollection();
        }

        /// <summary>
        /// Inits event handlers.
        /// </summary>
        protected void _InitEventHandlers()
        {
            _HoursBox.GotFocus += new RoutedEventHandler(_HoursBox_GotFocus);
            _MinutesBox.GotFocus += new RoutedEventHandler(_MinutesBox_GotFocus);
            _AmPm.GotFocus += new RoutedEventHandler(_AmPm_GotFocus);

            // use Preview Mouse Down event to select all text in text bos when it got focus
            _HoursBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_TextBox_PreviewMouseLeftButtonDown);
            _MinutesBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_TextBox_PreviewMouseLeftButtonDown);
            _AmPm.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_TextBox_PreviewMouseLeftButtonDown);

            _HoursBox.LostFocus += new RoutedEventHandler(_HoursBox_LostFocus);
            _MinutesBox.LostFocus += new RoutedEventHandler(_MinutesBox_LostFocus);

            // use PreviewKeyDown event to catch keyboard event when arrow keys pressed and not released
            _HoursBox.PreviewKeyDown += new KeyEventHandler(TextBox_PreviewKeyDown);
            _MinutesBox.PreviewKeyDown += new KeyEventHandler(TextBox_PreviewKeyDown);
            _AmPm.PreviewKeyDown += new KeyEventHandler(_AmPm_PreviewKeyDown);

            // use KeyUp event to correctly backtrace input digits
            _HoursBox.KeyUp += new KeyEventHandler(_HoursBox_KeyUp);
            _MinutesBox.KeyUp += new KeyEventHandler(_MinutesBox_KeyUp);

            _HoursBox.MouseWheel += new MouseWheelEventHandler(_HoursBox_MouseWheel);
            _MinutesBox.MouseWheel += new MouseWheelEventHandler(_MinutesBox_MouseWheel);
            _AmPm.MouseWheel += new MouseWheelEventHandler(_AmPm_MouseWheel);

            _IncrementButton.Click += new RoutedEventHandler(_IncrementButton_Click);
            _DecrementButton.Click += new RoutedEventHandler(_DecrementButton_Click);

            TimeChanged += new RoutedEventHandler(_TimeTextBoxTimeChanged);
            this.Loaded += new RoutedEventHandler(_TimeTextBoxLoaded);

            // workaround to cancel drag text from text box (see http://www.switchonthecode.com/tutorials/wpf-snippet-disabling-dragging-from-a-textbox)
            DataObject.AddCopyingHandler(this, NoDragCopy);
        }
        
        /// <summary>
        /// Init time value, shown in control.
        /// </summary>
        protected void _InitTime()
        {
            // Convert TimeSpan to DateTime format for using Globalization methods.
            _formattedDate = new DateTime(1, 1, 1, _ts.Hours, _ts.Minutes, 0);
            _SetFormattedText();
        }

        /// <summary>
        /// Method define default values of max and min time in compliance with Local Culture settings.
        /// </summary>
        protected void _GetDefaultTimeValues()
        {
            // tmp variable helps to get lower boundary of local time
            DateTime defaultDate = new DateTime(1, 1, 1, _defaultFirstHourIn24HourDay, 0, 0);

            String hoursFormat = _dateTimeFormat.ShortTimePattern.Substring(0, 1);
            hoursFormat += hoursFormat;

            _maxValueOfHours = Convert.ToInt32(defaultDate.ToString(hoursFormat, _dateTimeFormat));

            // tmp variable helps to get top boundary of local time
            defaultDate = new DateTime(1, 1, 1, _defaultLastHourIn24HourDay, 0, 0);

            _lastHourInDayValue = Convert.ToInt32(defaultDate.ToString(hoursFormat, _dateTimeFormat));
        }

        /// <summary>
        /// Increments hours value.
        /// </summary>
        protected void _IncrementHours()
        {
            if (!String.IsNullOrEmpty(_HoursBox.Text))
                _hoursValue = Convert.ToInt32(_HoursBox.Text);

            else if (_HoursBox.Text == "" || _HoursBox.Text == " " || _HoursBox.Text == "  ")
                _hoursValue = _maxValueOfHours;

            _hoursValue = (_hoursValue == _maxValueOfHours) ? _minValueOfHours : ++_hoursValue;
            _hoursValue = (_hoursValue > _lastHourInDayValue) ? _maxValueOfHours : _hoursValue;

            string zeroString = string.Empty;

            // Add 1-st "0" if hours value is less than 10 (do "05" from "5").
            if (_hoursValue < TEN_VALUE && _hoursValue >= 0)
                zeroString = ZERO_STRING;

            _HoursBox.Text = zeroString + _hoursValue.ToString();

            if (_hoursValue == _fullPmIndex)
                _ChangeAmPmDesignator();

            _HoursBox.Focus();
            _HoursBox.SelectAll();
        }

        /// <summary>
        /// Decrements hours value.
        /// </summary>
        protected void _DecrementHours()
        {
            if (!String.IsNullOrEmpty(_HoursBox.Text))
                _hoursValue = Convert.ToInt32(_HoursBox.Text);

            else if (_HoursBox.Text == "" || _HoursBox.Text == " " || _HoursBox.Text == "  ")
                _hoursValue = _maxValueOfHours;

            _hoursValue = (_hoursValue == 0) ? _lastHourInDayValue : --_hoursValue;
            _hoursValue = (_hoursValue < _minValueOfHours) ? _maxValueOfHours : _hoursValue;

            string zeroString = string.Empty;

            // Add 1-st "0" if hours value is less than 10 (do "05" from "5").
            if (_hoursValue < TEN_VALUE && _hoursValue >= 0)
                zeroString = ZERO_STRING;

            _HoursBox.Text = zeroString + _hoursValue.ToString();

            if (_hoursValue == _fullPmIndex - 1)
                _ChangeAmPmDesignator();

            _HoursBox.Focus();
            _HoursBox.SelectAll();
        }

        /// <summary>
        /// Increments minutes value.
        /// </summary>
        protected void _IncrementMinutes()
        {
            if (!String.IsNullOrEmpty(_MinutesBox.Text))
                _minutesValue = Convert.ToInt32(_MinutesBox.Text);
            else if (_MinutesBox.Text == "" || _MinutesBox.Text == " " || _MinutesBox.Text == "  ")
                _minutesValue = _minValueOfMinutes;

            _minutesValue++;
            if (_minutesValue > _maxValueOfMinutes)
            {
                _IncrementHours();
                _minutesValue = _minValueOfMinutes;
            }

            string zeroString = string.Empty;

            // Add 1-st "0" if minutes value is less than 10 (do "05" from "5").
            if (_minutesValue < TEN_VALUE && _minutesValue >= 0)
                zeroString = ZERO_STRING;

            _MinutesBox.Text = zeroString + _minutesValue.ToString();

            _MinutesBox.Focus();
            _MinutesBox.SelectAll();
        }

        /// <summary>
        /// Decrements minutes value.
        /// </summary>
        protected void _DecrementMinutes()
        {
            if (!String.IsNullOrEmpty(_MinutesBox.Text))
                _minutesValue = Convert.ToInt32(_MinutesBox.Text);
            else if (_MinutesBox.Text == "" || _MinutesBox.Text == " " || _MinutesBox.Text == "  ")
                _minutesValue = _minValueOfMinutes;

            _minutesValue--;
            if (_minutesValue < _minValueOfMinutes)
            {
                _DecrementHours();
                _minutesValue = _maxValueOfMinutes;
            }

            string zeroString = string.Empty;

            // Add 1-st "0" if minutes value is less than 10 (do "05" from "5").
            if (_minutesValue < TEN_VALUE)
                zeroString = ZERO_STRING;

            _MinutesBox.Text = zeroString + _minutesValue.ToString();
            _MinutesBox.Focus();
            _MinutesBox.SelectAll();
        }

        /// <summary>
        /// Increments time value in current active textbox.
        /// </summary>
        protected void _IncrementValue()
        {
            if (_isHoursFocused)
                _IncrementHours();

            else if (_isAmPmFocused)
                _ChangeAmPmDesignator();

            else
                _IncrementMinutes();
        }

        /// <summary>
        /// Decrements time value in current active textbox.
        /// </summary>
        protected void _DecrementValue()
        {
            if (_isHoursFocused)
                _DecrementHours();

            else if (_isAmPmFocused)
                _ChangeAmPmDesignator();

            else
                _DecrementMinutes();
        }

        /// <summary>
        /// Changes text "Am"/"Pm" and set value of convertion index.
        /// </summary>
        protected void _ChangeAmPmDesignator()
        {
            if (_dateTimeFormat.ShortTimePattern.Contains("tt"))
            {
                if (_AmPm.Text == _dateTimeFormat.AMDesignator)
                {
                    _pmConvertionIndex = _fullPmIndex;
                    _AmPm.Text = _dateTimeFormat.PMDesignator;
                }
                else
                {
                    _pmConvertionIndex = _nullPmIndex;
                    _AmPm.Text = _dateTimeFormat.AMDesignator;
                }
                _AmPm.Focus();
                _AmPm.SelectAll();
            }
            this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
        }


        /// <summary>
        ///  Changes time value depending on mouse wheel.
        /// </summary>
        /// <param name="delta"></param>
        protected void _ChangeValueByWheel(int delta)
        {
            if (delta > 0)
                _IncrementValue();

            else if (delta < 0)
                _DecrementValue();
            this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
        }

        #endregion

        #region Event Handlers.

        /// <summary>
        /// Update Time property, when TimeSpan Dependency Property has changed.
        /// </summary>
        /// <param name="sender">Ingored.</param>
        /// <param name="e">RoutedPropertyChangedEventArgs<TimeSpan>.</param>
        private void _TimeTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            // If 'Time' property wasnt used to set timespan and 
            // if value of 'TimeSpan' property differs from default.
            if ( !_isTimePropertyInUse && Time != TimeSpan && TimeSpan != null)
            {
                _ts = TimeSpan;
            } 
            _InitTime();
        }

        /// <summary>
        /// Update TimeSpan Dependency Property, when Time property has changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _TimeTextBoxTimeChanged(object sender, RoutedEventArgs e)
        {
            TimeSpan = Time;
        }

        /// <summary>
        /// Handler for correct select text in text box when it got focus.
        /// </summary>
        /// <param name="sender">TextBox</param>
        /// <param name="e">Ignored.</param>
        private void _TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender).SelectAll();
            Keyboard.Focus((TextBox)sender);
            e.Handled = true;
        }

        /// <summary>
        /// Method catch DragStarted event and cancel it.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">DataObjectCopyingEventArgs.</param>
        private void NoDragCopy(object sender, DataObjectCopyingEventArgs e)
        {
            if (e.IsDragDrop)
            { 
                e.CancelCommand(); 
            }
        }

        private void _MinutesBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = e.Delta;
            _ChangeValueByWheel(delta);
        }

        private void _HoursBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int delta = e.Delta;
            _ChangeValueByWheel(delta);
        }

        private void _AmPm_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isAmPmFocused)
                _ChangeAmPmDesignator();
        }

        private void _AmPm_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _AmPm.Focus();
            Key pressedKey = e.Key;

            if (pressedKey == Key.P)
            {
                _AmPm.Text = _dateTimeFormat.AMDesignator;
                _ChangeAmPmDesignator();
            }
            else if (pressedKey == Key.A)
            {
                _AmPm.Text = _dateTimeFormat.PMDesignator;
                _ChangeAmPmDesignator();
            }
            else if (pressedKey == Key.Up || pressedKey == Key.Down || pressedKey == Key.Left || pressedKey == Key.Right)
                _ChangeAmPmDesignator();
        }

        private void _MinutesBox_KeyUp(object sender, KeyEventArgs e)
        {
            Key pressedKey = e.Key;

            if (_numKeysCollection.Contains(pressedKey) && (Keyboard.Modifiers & ModifierKeys.Shift) <= 0)
            {
                if (Convert.ToInt32(_MinutesBox.Text) > _maxValueOfMinutes)
                    _MinutesBox.Text = _maxValueOfMinutes.ToString();
                _oldMinutesText = _MinutesBox.Text;
            }
            else if (pressedKey == Key.Space)
                _MinutesBox.Clear();
            else if (pressedKey != Key.Back && pressedKey != Key.Delete &&
                     pressedKey != Key.Tab && pressedKey != Key.Up &&
                     pressedKey != Key.Down && pressedKey != Key.Left &&
                     pressedKey != Key.Right)
                _MinutesBox.Text = _oldMinutesText;

            this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key pressedKey = e.Key;

            if (pressedKey == Key.Up)
            {
                _IncrementValue();
                this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
            }
            else if (pressedKey == Key.Down)
            {
                _DecrementValue();
                this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
            }
        }

        private void _HoursBox_KeyUp(object sender, KeyEventArgs e)
        {
            Key pressedKey = e.Key;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) <= 0)
            {
                if (_numKeysCollection.Contains(pressedKey))
                {
                    if (Convert.ToInt32(_HoursBox.Text) > _lastHourInDayValue || _HoursBox.Text == "00")
                        _HoursBox.Text = _maxValueOfHours.ToString();
                    _oldHoursText = _HoursBox.Text;
                }
                else if (pressedKey == Key.Space)
                    _HoursBox.Clear();
                else if (pressedKey != Key.Back && pressedKey != Key.Delete &&
                         pressedKey != Key.Tab && pressedKey != Key.Up &&
                         pressedKey != Key.Down && pressedKey != Key.Left &&
                         pressedKey != Key.Right && pressedKey != Key.LeftShift && pressedKey != Key.RightShift)
                    _HoursBox.Text = _oldHoursText;
            }
            else
                _HoursBox.Text = _oldHoursText;

            this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
        }

        private void _AmPm_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
        }

        private void _MinutesBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _isHoursFocused = false;
            _isAmPmFocused = false;
            _MinutesBox.SelectAll();
        }

        private void _HoursBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _isHoursFocused = true;
            _HoursBox.SelectAll();
        }

        private void _AmPm_GotFocus(object sender, RoutedEventArgs e)
        {
            _isAmPmFocused = true;
            _isHoursFocused = false;
            _AmPm.SelectAll();
        }

        private void _MinutesBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_MinutesBox.Text == "" || _HoursBox.Text == " " || _HoursBox.Text == "  ")
            {
                _MinutesBox.Text = "00";
                this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
            }

            int minutesValue = Convert.ToInt32(_MinutesBox.Text);

            // Add 1-st "0" if minutes value is less than 10 (do "05" from "5").
            if (minutesValue < TEN_VALUE && minutesValue >= 0)
                _MinutesBox.Text = ZERO_STRING + minutesValue.ToString();
        }

        private void _HoursBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_HoursBox.Text == "" || _HoursBox.Text == " " || _HoursBox.Text == "  " || _HoursBox.Text == "0")
            {
                _HoursBox.Text = _maxValueOfHours.ToString();
                this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
            }

            int hoursValue = Convert.ToInt32(_HoursBox.Text);

            // Add 1-st "0" if hours value is less than 10 (do "05" from "5").
            if (hoursValue < TEN_VALUE && hoursValue >= 0)
                _HoursBox.Text = ZERO_STRING + hoursValue.ToString();
        }

        private void _DecrementButton_Click(object sender, RoutedEventArgs e)
        {
            _DecrementValue();
            this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
        }

        private void _IncrementButton_Click(object sender, RoutedEventArgs e)
        {
            _IncrementValue();
            this.RaiseEvent(new RoutedEventArgs(TimeTextBox.TimeChangedEvent));
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// 10 value.
        /// </summary>
        private const int TEN_VALUE = 10;

        /// <summary>
        /// "0" string.
        /// </summary>
        private const string ZERO_STRING = "0";

        #endregion

        #region Private Fields

        private Collection<Key> _numKeysCollection;

        private TimeSpan _ts;

        private TextBox _HoursBox;
        private TextBox _MinutesBox;
        private TextBox _AmPm;
        private RepeatButton _IncrementButton;
        private RepeatButton _DecrementButton;

        private DateTimeFormatInfo _dateTimeFormat;
        private DateTime _formattedDate;

        private bool _isHoursFocused = false;
        private bool _isAmPmFocused;

        private int _pmConvertionIndex = 0;

        private int _hoursValue;
        private int _formattedHoursValue;
        private int _minutesValue;

        private static int _maxValueOfHours;
        private static int _minValueOfHours = 1;
        private static int _fullPmIndex = 12;
        private static int _nullPmIndex = 0;

        // Default values of boundary hours
        private static int _defaultLastHourIn24HourDay = 23;
        private static int _defaultFirstHourIn24HourDay = 0;

        // Value of last hour in day (23 or 11)
        private static int _lastHourInDayValue;

        private static int _maxValueOfMinutes = 59;
        private static int _minValueOfMinutes = 0;

        private string _oldHoursText;
        private string _oldMinutesText;

        /// <summary>
        /// Flag, which shows is 'Time' property in use.
        /// </summary>
        private bool _isTimePropertyInUse = false;

        #endregion

    }
}
