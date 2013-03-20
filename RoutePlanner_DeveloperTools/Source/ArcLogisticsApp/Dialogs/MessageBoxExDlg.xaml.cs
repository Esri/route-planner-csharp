using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using SysControls = System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// An advanced message box that supports customizations like Icon, Buttons and Response.
    /// </summary>
    internal partial class MessageBoxExDlg : Window
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageBoxExDlg()
        {
            InitializeComponent();

            _InitDialog();
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Sets the text of the message box.
        /// </summary>
        public string Message
        {
            set { _textBlockQuestion.Text = value; }
        }

        /// <summary>
        /// Sets the caption of the message box.
        /// </summary>
        public string Caption
        {
            set { this.Title = value; }
        }

        /// <summary>
        /// Gets buttons list of the message box.
        /// </summary>
        public ArrayList Buttons
        {
            get { return _buttons; }
        }

        /// <summary>
        /// Sets the text to show to the user when check.
        /// </summary>
        public string ResponseText
        {
            set
            {
                _allowResponse = !string.IsNullOrEmpty(value);
                _responseCheckBox.Content = value;
            }
        }

        /// <summary>
        /// Sets or gets the state of the user check response.
        /// </summary>
        public bool Response
        {
            set { _responseCheckBox.IsChecked = value; }
            get { return (true == _responseCheckBox.IsChecked); }
        }

        /// <summary>
        /// Sets the icon list of the message box.
        /// </summary>
        public MessageBoxImage StandardIcon
        {
            set { _SetStandardIcon(value); }
        }

        /// <summary>
        /// Gets the button seletction of the message box.
        /// </summary>
        public MessageBoxExButtonType Result
        {
            get
            {
                Debug.Assert(_result.HasValue);
                return _result.Value;
            }
        }

        #endregion // Public properties

        #region Private event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            _AddOkButton();

            _DisableCloseButton();

            _InitIconControlState();

            _InitCheckboxVisibility();

            _AddButtons();

            _PlayAlert();

            _StoreOkButton();
        }

        private void _Dialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_result.HasValue)
            {
                if (null != _pressedButton)
                {
                    _result = _pressedButton.Value;
                }
                else if (null != _cancelButton)
                {
                    _result = _cancelButton.Value;
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void _Dialog_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                if (e.Key == Key.Escape)
                {
                    _pressedButton = _cancelButton;
                }
                else // e.Key == Key.Enter
                {
                    _pressedButton = _okButton;
                }

                DialogResult = (null != _pressedButton);
                Close();
                e.Handled = true;
            }
        }

        private void _Button_Click(object sender, RoutedEventArgs e)
        {
            SysControls.Button btn = sender as SysControls.Button;
            if ((null != btn) && (null != btn.Tag))
            {
                _result = (MessageBoxExButtonType)btn.Tag;

                DialogResult = true;
                Close();
            }
        }

        #endregion // Private event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init dialog state.
        /// </summary>
        private void _InitDialog()
        {
            this.MinWidth = SystemInformation.WorkingArea.Width * 0.2;
            this.MaxWidth = SystemInformation.WorkingArea.Width * 0.6;
            this.MaxHeight = SystemInformation.WorkingArea.Height * 0.9;

            this.Closing += new System.ComponentModel.CancelEventHandler(_Dialog_Closing);
            this.Loaded += new RoutedEventHandler(_Dialog_Loaded);
            this.KeyDown += new System.Windows.Input.KeyEventHandler(_Dialog_KeyDown);

            this.ShowActivated = true;
        }

        /// <summary>
        /// Add default "OK" button.
        /// </summary>
        private void _AddOkButton()
        {
            if (_buttons.Count == 0)
            {   // if button not set - add OK button
                _okButton = new MessageBoxExButton();
                _okButton.Caption = (string)App.Current.FindResource("ButtonHeaderOk");
                _okButton.Value = MessageBoxExButtonType.Ok;

                _buttons.Add(_okButton);
            }
        }

        /// <summary>
        /// Disable close button.
        /// </summary>
        private void _DisableCloseButton()
        {
            if (1 == _buttons.Count)
                _cancelButton = _buttons[0] as MessageBoxExButton;
            else if (1 < _buttons.Count)
            {
                if (_cancelButton == null)
                {   // see if standard cancel button is present
                    foreach (MessageBoxExButton button in _buttons)
                    {
                        if (button.IsCancelButton || MessageBoxExButtonType.Cancel == button.Value)
                        {
                            if (null == _cancelButton)
                                _cancelButton = button;
                            else
                            {
                                Debug.Assert(false); // NOTE: only one cancel button supported
                            }
                        }
                    }

                    if (null == _cancelButton)
                    {   // standard cancel button is not present, Disable close button
                        _DisableCloseButton(this);
                    }
                }
            }
            // else Do nothing
        }

        /// <summary>
        /// Init icon control state.
        /// </summary>
        private void _InitIconControlState()
        {
            if (null != _iconImage)
                _systemIcon.Source = Imaging.CreateBitmapSourceFromHIcon(_iconImage.Handle, Int32Rect.Empty,
                                                                         BitmapSizeOptions.FromEmptyOptions());
            _systemIcon.Visibility = (null == _iconImage)? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Sets the visibility of the response checkbox.
        /// </summary>
        private void _InitCheckboxVisibility()
        {
            _responseCheckBox.Visibility = (_allowResponse) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Layout all the buttons.
        /// </summary>
        private void _AddButtons()
        {
            Hashtable _buttonsTable = new Hashtable();

            foreach (MessageBoxExButton button in _buttons)
            {
                if (!_buttonsTable.ContainsKey(button))
                {
                    SysControls.Button buttonCtrl = _CreateButton(button);

                    // Move keyboard focus to "Yes" or "Ok" button.
                    if (button.Value == MessageBoxExButtonType.Ok
                        || button.Value == MessageBoxExButtonType.Yes)
						buttonCtrl.Focus();

                    _buttonsTable[button] = buttonCtrl;
                    _buttonsWrapPanel.Children.Add(buttonCtrl);
                }
            }
        }

        /// <summary>
        /// Creates a button control based on info from MessageBoxExButton.
        /// </summary>
        /// <param name="button">Message box extended button.</param>
        /// <returns>GUI control.</returns>
        private SysControls.Button _CreateButton(MessageBoxExButton button)
        {
            SysControls.Button buttonCtrl = new SysControls.Button();

            // init button
            buttonCtrl.Content = button.Caption;
            if (!string.IsNullOrEmpty(button.HelpText) && (0 < button.HelpText.Trim().Length))
                buttonCtrl.ToolTip = button.HelpText;
            buttonCtrl.Click += new RoutedEventHandler(_Button_Click);
            buttonCtrl.Tag = (MessageBoxExButtonType?)button.Value;

            // Apply style to button
            buttonCtrl.Width = (double)App.Current.FindResource("DefaultPageButtonWidth");
            buttonCtrl.Height = (double)App.Current.FindResource("DefaultPageButtonHeight");
            buttonCtrl.Margin = new Thickness(10);

            return buttonCtrl;
        }

        /// <summary>
        /// Set standard icon.
        /// </summary>
        /// <param name="icon">message box icon.</param>
        private void _SetStandardIcon(MessageBoxImage icon)
        {
            _standardIcon = icon;
            switch(icon)
            {
                case MessageBoxImage.Asterisk:
                    _iconImage = SystemIcons.Asterisk;
                    break;
                case MessageBoxImage.Error:
                    _iconImage = SystemIcons.Error;
                    break;
                case MessageBoxImage.Exclamation:
                    _iconImage = SystemIcons.Exclamation;
                    break;
                //case MessageBoxImage.Hand:
                //    _iconImage = SystemIcons.Hand;
                //    break;
                //case MessageBoxImage.Information:
                //    _iconImage = SystemIcons.Information;
                //    break;
                case MessageBoxImage.Question:
                    _iconImage = SystemIcons.Question;
                    break;
                //case MessageBoxImage.Warning:
                //    _iconImage = SystemIcons.Warning;
                //    break;
                case MessageBoxImage.None:
                    _iconImage = null;
                    break;
                default:
                    Debug.Assert(false); // NOTE: not supported
                    break;
            }
        }

        /// <summary>
        /// Find Ok button in buttons array and store.
        /// </summary>
        private void _StoreOkButton()
        {
            foreach (MessageBoxExButton button in _buttons)
            {
                if (MessageBoxExButtonType.Ok == button.Value || MessageBoxExButtonType.Yes == button.Value)
                {
                    if (null == _okButton)
                    {
                        _okButton = button;
                    }
                    else
                    {
                        Debug.Assert(false); // NOTE: only one ok button supported.
                    }
                }
            }
        }

        #endregion // Private methods

        #region P/Invoke - GetSystemMenu, EnableMenuItem, MessageBeep
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        private const int SC_CLOSE = 0xF060;
        private const int MF_BYCOMMAND = 0x0;
        private const int MF_GRAYED = 0x1;

        /// <summary>
        /// Disables "Close" button for message box.
        /// </summary>
        /// <param name="window">Message box window.</param>
        private void _DisableCloseButton(Window window)
        {
            try
            {
                IntPtr windowPtr = new WindowInteropHelper(window).Handle;
                EnableMenuItem(GetSystemMenu(windowPtr, false), SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
            }
            catch
            {
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool MessageBeep(uint type);

        /// <summary>
        /// Plays the alert sound based on the icon set for the message box.
        /// </summary>
        private void _PlayAlert()
        {
            if (MessageBoxImage.None != _standardIcon)
                MessageBeep((uint)_standardIcon);
        }

        #endregion // P/Invoke - GetSystemMenu, EnableMenuItem, MessageBeep

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Button list.
        /// </summary>
        private ArrayList _buttons = new ArrayList();

        /// <summary>
        /// Response use flag.
        /// </summary>
        private bool _allowResponse = false;

        /// <summary>
        /// Button used in cancel mode.
        /// </summary>
        private MessageBoxExButton _cancelButton = null;

        /// <summary>
        /// Button used in ok mode.
        /// </summary>
        private MessageBoxExButton _okButton = null;

        /// <summary>
        /// Pressed button.
        /// </summary>
        private MessageBoxExButton _pressedButton = null;

        /// <summary>
        /// Dialog result button.
        /// </summary>
        private MessageBoxExButtonType? _result = null;

        /// <summary>
        /// Icon.
        /// </summary>
        private MessageBoxImage _standardIcon = MessageBoxImage.None;
        /// <summary>
        /// Icon image.
        /// </summary>
        private Icon _iconImage = null;

        /// <summary>
        /// Maps buttons to Button controls.
        /// </summary>
        private Hashtable _buttonsTable = new Hashtable();

        #endregion // Private fields
    }
}
 