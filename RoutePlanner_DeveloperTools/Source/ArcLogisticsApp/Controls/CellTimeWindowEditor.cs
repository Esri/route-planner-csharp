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
using System.Windows.Controls.Primitives;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Internal logic for time window editor
    /// Inherited from ComboBox to use it's internal logic of open/close dropdown panel
    /// </summary>
    [TemplatePart(Name = "PART_IsWideopen", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_CellLabel", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_FromTime", Type = typeof(TimeTextBox))]
    [TemplatePart(Name = "PART_ToTime", Type = typeof(TimeTextBox))]
    [TemplatePart(Name = "PART_IsDay2", Type = typeof(CheckBox))]
    [TemplatePart(Name = "PART_TopLevelGrid", Type = typeof(Grid))]
    internal class CellTimeWindowEditor : ComboBox
    {
        #region Constructors

        /// <summary>
        /// Constructor of type: initializes type CellTimeWindowEditor.
        /// </summary>
        static CellTimeWindowEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CellTimeWindowEditor),
                                                     new FrameworkPropertyMetadata(typeof(CellTimeWindowEditor)));
        }

        #endregion Constructors

        #region Public overridden members

        /// <summary>
        /// Overriden method of the base ComboBox class called when ApplyTemplate() is called.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitComponents();
            _InitEventHandlers();
        }

        #endregion Public overridden members

        #region Public properties

        public static readonly DependencyProperty TimeWindowProperty =
            DependencyProperty.Register("TimeWindow", typeof(TimeWindow), typeof(CellTimeWindowEditor));

        /// <summary>
        /// Gets/sets TimeWindow.
        /// </summary>
        public TimeWindow TimeWindow
        {
            get
            {
                return (TimeWindow)GetValue(TimeWindowProperty);
            }
            set
            {
                SetValue(TimeWindowProperty, value);
            }
        }

        public static readonly DependencyProperty WideopenVisibilityProperty =
           DependencyProperty.Register("WideopenVisibility", typeof(Visibility), typeof(CellTimeWindowEditor));

        /// <summary>
        /// Gets/sets WideopenVisibility. Property used for hide "IsWideopen" checkbox if necessary
        /// </summary>
        public Visibility WideopenVisibility
        {
            get
            {
                return (Visibility)GetValue(WideopenVisibilityProperty);
            }
            set
            {
                SetValue(WideopenVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty Day2VisibilityProperty =
           DependencyProperty.Register("Day2Visibility", typeof(Visibility), typeof(CellTimeWindowEditor));

        /// <summary>
        /// Gets/sets Day2 Visibility. Property used for hide "Day2" checkbox if necessary.
        /// </summary>
        public Visibility Day2Visibility
        {
            get
            {
                return (Visibility)GetValue(Day2VisibilityProperty);
            }
            set
            {
                SetValue(Day2VisibilityProperty, value);
            }
        }

        #endregion Public properties

        #region Protected methods

        /// <summary>
        /// Inits components.
        /// </summary>
        protected void _InitComponents()
        {
            _isWideopenCheckBox = this.GetTemplateChild("PART_IsWideopen") as CheckBox;
            _cellTextBlock = this.GetTemplateChild("PART_CellLabel") as TextBlock;
            _popupPanel = this.GetTemplateChild("PART_PopupPanel") as Popup;
            _toTimeTextBox = this.GetTemplateChild("PART_ToTime") as TimeTextBox;
            _fromTimeTextBox = this.GetTemplateChild("PART_FromTime") as TimeTextBox;
            _isDay2CheckBox = this.GetTemplateChild("PART_IsDay2") as CheckBox;
            _topLevelGrid = this.GetTemplateChild("PART_TopLevelGrid") as Grid;
        }

        /// <summary>
        /// Inits event handlers.
        /// </summary>
        protected void _InitEventHandlers()
        {
            this.Loaded += new RoutedEventHandler(_CellTimeWindowEditorLoaded);
            this.KeyDown += new KeyEventHandler(_CellTimeWindowEditorKeyDown);
            this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(_CellTimeWindowEditorPreviewMouseLeftButtonDown);

            _popupPanel.Opened += new EventHandler(_PopupPanelOpened);
            _popupPanel.Closed += new EventHandler(_PopupPanelClosed);

            _isWideopenCheckBox.Checked += new RoutedEventHandler(_IsWideopenCheckBoxChecked);
            _isWideopenCheckBox.Unchecked += new RoutedEventHandler(_IsWideopenCheckBoxUnchecked);

            _fromTimeTextBox.TimeChanged += new RoutedEventHandler(_FromTimeTextBoxTimeChanged);

            _toTimeTextBox.TimeChanged += new RoutedEventHandler(_ToTimeTextBoxTimeChanged);

            _isDay2CheckBox.Checked += new RoutedEventHandler(_IsDay2CheckBoxChecked);
            _isDay2CheckBox.Unchecked += new RoutedEventHandler(_IsDay2CheckBoxUnchecked);
        }

        /// <summary>
        /// Gets TimeWindow property and update control's layout accordingly.
        /// </summary>
        /// <param name="timeWindow"></param>
        protected void _TimeWindowToControlState(TimeWindow timeWindow)
        {
            if (timeWindow != null)
                if (timeWindow.IsWideOpen)
                    _isWideopenCheckBox.IsChecked = true;

            _toTimeTextBox.Time = timeWindow.To;
            _toTimeTextBox.TimeSpan = _toTimeTextBox.Time;
            _fromTimeTextBox.Time = timeWindow.From;
            _fromTimeTextBox.TimeSpan = _fromTimeTextBox.Time;
            _isDay2CheckBox.IsChecked = timeWindow.Day > 0;

            _UpdateCellTextBlockText();
        }

        /// <summary>
        /// Transform values from control fields to TimeWindow value
        /// </summary>
        protected void _GetTimeWindowFromControl()
        {
            TimeWindow timeWindow = new TimeWindow();

            timeWindow.IsWideOpen = (bool)_isWideopenCheckBox.IsChecked;
            timeWindow.From = _fromTimeTextBox.Time;
            timeWindow.To = _toTimeTextBox.Time;
            timeWindow.Day = (bool)_isDay2CheckBox.IsChecked ? 1u : 0u;

            TimeWindow = timeWindow;
        }

        /// <summary>
        /// Updates cell text block text.
        /// </summary>
        protected void _UpdateCellTextBlockText()
        {
            _cellTextBlock.Text = TimeWindow.ToString();
        }

        #endregion Protected methods

        #region Event handlers

        /// <summary>
        /// Handler for the Loaded event of CellTimeWindowEditor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _CellTimeWindowEditorLoaded(object sender, RoutedEventArgs e)
        {
            //NOTE: set property IsDropDownOpen to true for open control when it was loaded
            this.IsDropDownOpen = true;

            if (TimeWindow != null)
            {
                _UpdateCellTextBlockText();
            }
        }

        /// <summary>
        /// Handler for the KeyDown event of CellTimeWindowEditor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _CellTimeWindowEditorKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Escape || e.Key == Key.Enter) && _popupPanel.IsOpen)
            {
                this.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handler for the PreviewMouseLeftButtonDown event of CellTimeWindowEditor.
        /// Method closes popup if it is opened and user clicked outside the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _CellTimeWindowEditorPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If mouse is clicked outside the control and popup is shown - we need to close it, 
            // so popup will lose its focus and mouse left button down event will come to grid.
            if (!_topLevelGrid.IsMouseOver && _popupPanel.IsOpen)
                _popupPanel.IsOpen = false;
        }

        /// <summary>
        /// Handler for the Opened event of _popupPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _PopupPanelOpened(object sender, EventArgs e)
        {
            // Update control state
            if (TimeWindow != null)
            {
                _TimeWindowToControlState(TimeWindow);
                _UpdateCellTextBlockText();
                _fromTimeTextBox.InitStartText();
                _toTimeTextBox.InitStartText();
            }

            PopupPositionSynchronizer synchronizer = new PopupPositionSynchronizer(this, _popupPanel);

            // Set popup's position.
            synchronizer.PositionPopupBelowCellEditor();
        }

        /// <summary>
        /// Handler for the Closed event of _popupPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _PopupPanelClosed(object sender, EventArgs e)
        {
            // NOTE : set focus to parent cell for support arrow keys navigation
            UIElement cell = XceedVisualTreeHelper.GetCellByEditor(this);
            if (cell != null)
                Keyboard.Focus(cell);
        }

        /// <summary>
        /// Handler for the Checked event of _isWideopenCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _IsWideopenCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            _TimeWindowChangedHandler();
        }

        /// <summary>
        /// Handler for the Unchecked event of _isWideopenCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _IsWideopenCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            _TimeWindowChangedHandler();
        }

        /// <summary>
        /// Handler for the TimeChanged event of _fromTimeTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _FromTimeTextBoxTimeChanged(object sender, RoutedEventArgs e)
        {
            _TimeWindowChangedHandler();
        }

        /// <summary>
        /// Handler for the TimeChanged event of _toTimeTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _ToTimeTextBoxTimeChanged(object sender, RoutedEventArgs e)
        {
            _TimeWindowChangedHandler();
        }

        /// <summary>
        /// Handler for the event Checked of _isDay2CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _IsDay2CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            _TimeWindowChangedHandler();
        }

        /// <summary>
        /// Handler for the event Unchecked of _isDay2CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data.</param>
        private void _IsDay2CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            _TimeWindowChangedHandler();
        }

        #endregion Event handlers

        #region Private methods

        /// <summary>
        /// Updates time window data from UI and updates text in time window's text block.
        /// </summary>
        private void _TimeWindowChangedHandler()
        {
            _GetTimeWindowFromControl();
            _UpdateCellTextBlockText();
        }

        #endregion Private methods

        #region Private fields

        /// <summary>
        /// Popup panel.
        /// </summary>
        private Popup _popupPanel;

        /// <summary>
        /// Text block used to display time wndow's text.
        /// </summary>
        private TextBlock _cellTextBlock;

        /// <summary>
        /// "Wideopen" check box.
        /// </summary>
        private CheckBox _isWideopenCheckBox;

        /// <summary>
        /// Time "From" text box.
        /// </summary>
        private TimeTextBox _fromTimeTextBox;

        /// <summary>
        /// Time "To" text box.
        /// </summary>
        private TimeTextBox _toTimeTextBox;

        /// <summary>
        /// "Day 2" checkbox.
        /// </summary>
        private CheckBox _isDay2CheckBox;

        /// <summary>
        /// Top level control grid.
        /// </summary>
        private Grid _topLevelGrid;

        #endregion Private fields
    }
}
