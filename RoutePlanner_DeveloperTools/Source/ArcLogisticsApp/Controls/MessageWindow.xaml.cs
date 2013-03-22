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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcLogistics.App.GridHelpers;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    internal partial class MessageWindow : UserControl, IMessenger, IMessageReporter
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public MessageWindow()
        {
            InitializeComponent();

            App.Current.ApplicationInitialized += new EventHandler(_App_ApplicationInitialized);
            App.Current.Exit += new ExitEventHandler(_Current_Exit);
            this.Loaded += new RoutedEventHandler(_Page_Loaded);
            this.IsEnabledChanged += new DependencyPropertyChangedEventHandler(_MessageWindow_IsEnabledChanged);
            this.GotFocus += new RoutedEventHandler(_MessageWindow_GotFocus);
            this.SizeChanged += new SizeChangedEventHandler(_MessageWindowSizeChanged);
            xceedGrid.LostFocus += new RoutedEventHandler(_MessageWindow_LostFocus);
            xceedGrid.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(_MessageWindow_LostKeyboardFocus);

            ((INotifyCollectionChanged)xceedGrid.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(_MessageWindow_CollectionChanged);

            _timer = new Timer(SHOW_POPUP_MESSAGE_TIMER_INTERVAL);
            _timer.Elapsed += new ElapsedEventHandler(_OnTimedEvent);
            _timer.Enabled = false;
        }

        #endregion // Constructors

        #region IMessenger interface
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Add message to output window
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message text</param>
        public void AddMessage(MessageType type, string message)
        {
            _AddMessage(type, DateTime.Now.ToLongTimeString(), message, null, null);
        }

        /// <summary>
        /// Add message to output window
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message text. Can contain sequence '{0}' for  designation of position of link.
        ///     Without instructions of place the link will be added after the message text at the end</param>
        /// <param name="link">Link description</param>
        public void AddMessage(MessageType type, string message, Link link)
        {
            _AddMessage(type, DateTime.Now.ToLongTimeString(), message, link, null);
        }

        /// <summary>
        /// Add message to output window with details
        /// </summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        public void AddMessage(MessageType type, string message, IEnumerable<MessageDetail> details)
        {
            _AddMessage(type, DateTime.Now.ToLongTimeString(), message, null, details);
        }

        /// <summary>
        /// Add message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        /// <remarks>Type selected automatically by details type (set as high priority)</remarks>
        public void AddMessage(string message, IEnumerable<MessageDetail> details)
        {
            // select type
            MessageType type = MessageType.Information;

            IEnumerator<MessageDetail> enumerator = details.GetEnumerator();
            while((null != enumerator) && enumerator.MoveNext())
            {
                MessageDetail detail = enumerator.Current;
                if (type < detail.Type)
                {
                    type = detail.Type;
                    if (MessageType.Error == type)
                        break; // NOTE: set high ptiority - exit
                }
            }

            AddMessage(type, message, details);
        }

        /// <summary>
        /// Add error message to output window
        /// </summary>
        /// <param name="message">Message text</param>
        public void AddError(string message)
        {
            AddMessage(MessageType.Error, message);
        }

        /// <summary>
        /// Add error message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        public void AddError(string message, IEnumerable<MessageDetail> details)
        {
            AddMessage(MessageType.Error, message, details);
        }

        /// <summary>
        /// Add warning message to output window
        /// </summary>
        /// <param name="message">Message text</param>
        public void AddWarning(string message)
        {
            AddMessage(MessageType.Warning, message);
        }

        /// <summary>
        /// Add warning message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        public void AddWarning(string message, IEnumerable<MessageDetail> details)
        {
            AddMessage(MessageType.Warning, message, details);
        }

        /// <summary>
        /// Add information message to output window
        /// </summary>
        /// <param name="message">Message text</param>
        public void AddInfo(string message)
        {
            AddMessage(MessageType.Information, message);
        }

        /// <summary>
        /// Add information message to output window with details
        /// </summary>
        /// <param name="message">Message text</param>
        /// <param name="details">Message detail collection</param>
        public void AddInfo(string message, IEnumerable<MessageDetail> details)
        {
            AddMessage(MessageType.Information, message, details);
        }

        /// <summary>
        /// Remove all messages
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _collectionSource.Source = _data;
        }

        #endregion // IMessenger interface

        #region IMessageReporter Members
        /// <summary>
        /// Reports a warning.
        /// </summary>
        /// <param name="message">The message describing cause of the warning.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is a null reference.
        /// </exception>
        public void ReportWarning(string message)
        {
            //CodeContract.RequiresNotNull("message", message);

            this.AddWarning(message);
        }

        /// <summary>
        /// Reports an error.
        /// </summary>
        /// <param name="message">The message describing cause of the error.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is a null reference.
        /// </exception>
        public void ReportError(string message)
        {
            //CodeContract.RequiresNotNull("message", message);

            this.AddError(message);
        }
        #endregion

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _App_ApplicationInitialized(object sender, EventArgs e)
        {
            _InitGUI();
        }

        void _Current_Exit(object sender, ExitEventArgs e)
        {
            _timer.Dispose();
            _timer = null;
        }

        private void _Page_Loaded(object sender, RoutedEventArgs e)
        {
            _InitGUI();
        }

        private delegate void ZeroParamsDelegate();

        private void _OnTimedEvent(object source, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new ZeroParamsDelegate(_CloseWindow), System.Windows.Threading.DispatcherPriority.Send);
        }

        private void _MessageWindow_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!((bool)e.NewValue))
            {
                _timer.Enabled = false;
                _timer.Stop();
                _isNeedClose = false;
            }
        }

        private void _MessageWindow_GotFocus(object sender, RoutedEventArgs e)
        {
            _timer.Enabled = false;
            _timer.Stop();
            _isNeedClose = _isAutoOpened;
        }

        private void _MessageWindow_LostFocus(object sender, RoutedEventArgs e)
        {
            _LostFocusRoutine();
        }

        /// <summary>
        /// Handle message window resizing: calculate width for Message field
        /// to make it stretch to all Message Window space after Type and Time columns.
        /// Xceed Data Grid Control don't let to set up automatic stretching for non-first
        /// or non-last column.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MessageWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Get Type and Time fields widths.
            double typeFieldWidth = xceedGrid.Columns[TYPE_FIELD_NAME].ActualWidth;
            double timeFieldWidth = xceedGrid.Columns[TIME_FIELD_NAME].ActualWidth;

            // Calculate width for Message field.
            double width = this.ActualWidth - typeFieldWidth - timeFieldWidth -
                DEFAULT_COLUMN_HEADER_WIDTH;

            if (width > 0)
                xceedGrid.Columns[MESSAGE_FIELD_NAME].Width = width;
        }

        private void _MessageWindow_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _LostFocusRoutine();
        }

        private void _MessageWindow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((null != xceedGrid.Items) && (0 < xceedGrid.Items.Count))
                xceedGrid.CurrentItem = xceedGrid.Items[0];
        }

        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method inits context of XceedGrid.
        /// </summary>
        private void _InitGUI()
        {
            if (!_isInited)
            {
                _collectionSource = (DataGridCollectionViewSource)layoutRoot.FindResource("messageTable");

                GridStructureInitializer structureInitializer = new GridStructureInitializer(GridSettingsProvider.MessageWindowGridStructure);
                structureInitializer.BuildGridStructure(_collectionSource, xceedGrid);

                xceedGrid.DetailConfigurations.Clear();
                xceedGrid.DetailConfigurations.Add((DetailConfiguration)layoutRoot.FindResource("messageDetailConfiguration"));

                // collaps all detail and reexpand it.
                List<DataGridContext> dataGridContexts = new List<DataGridContext>(xceedGrid.GetChildContexts());
                foreach (DataGridContext dataGridContext in dataGridContexts)
                {
                    dataGridContext.ParentDataGridContext.CollapseDetails(dataGridContext.ParentItem);
                    dataGridContext.ParentDataGridContext.ExpandDetails(dataGridContext.ParentItem);
                }

                _collectionSource.Source = _data;

                _isInited = true;
            }
        }

        private void _CloseWindow()
        {
            _timer.Enabled = false;
            _timer.Stop();

            if (this.IsFocused)
                _isNeedClose = true;
            else
                _HideWindow();
        }

        private void _AddMessage(MessageType type, string timeMark, string message, Link link, IEnumerable<MessageDetail> details)
        {
            // convert to wrapper
            List<MessageDetailDataWrap> detailsWrap = null;
            if (null != details)
            {
                detailsWrap = new List<MessageDetailDataWrap>();

                IEnumerator<MessageDetail> enumerator = details.GetEnumerator();
                while((null != enumerator) && enumerator.MoveNext())
                {
                    MessageDetail detail = enumerator.Current;
                    detailsWrap.Add(new MessageDetailDataWrap(detail.Type, detail.Description));
                }
            }

            // add message
            MessageWindowTextDataWrapper textWrapper = new MessageWindowTextDataWrapper(message, link);
            MessageWindowDataWrapper wrapper = new MessageWindowDataWrapper(type, timeMark, textWrapper, detailsWrap);
            if (0 == _data.Count)
                _data.Add(wrapper);
            else
                _data.Insert(0, wrapper);

            _collectionSource.Source = _data;

            // NOTE: XceedGrid issue - refresh logical children
            if (null != _collectionSource.View)
                _collectionSource.View.Refresh();

            // auto open window
            if (MessageType.Error == type)
            {   // error - open the message window
                _timer.Enabled = false;
                _timer.Stop();
                _isAutoOpened = false;
                _isNeedClose = false;
                if (Visibility.Visible != this.Visibility)
                    App.Current.MainWindow.ToggleMessageWindowState();

                // expand error details
                if (null != details)
                {
                    DataGridContext dgc = xceedGrid.CurrentContext;
                    xceedGrid.ExpandDetails(dgc.Items.GetItemAt(0));
                }
            }

            else if (MessageType.Warning == type)
            {   // warning - pop the message window
                if (Visibility.Visible != this.Visibility)
                {
                    App.Current.MainWindow.ToggleMessageWindowState();
                    _isAutoOpened = true;

                    _timer.Interval = SHOW_POPUP_MESSAGE_TIMER_INTERVAL;
                    _timer.Start();
                    _timer.Enabled = true;
                    _isNeedClose = false;
                }
                else if (_isAutoOpened)
                {   // reinit timer
                    _timer.Enabled = false;
                    _timer.Stop();

                    _timer.Interval = SHOW_POPUP_MESSAGE_TIMER_INTERVAL;
                    _timer.Start();
                    _timer.Enabled = true;
                    _isNeedClose = false;
                }

                // Expand error details.
                if (null != details)
                {
                    DataGridContext dgc = xceedGrid.CurrentContext;
                    xceedGrid.ExpandDetails(dgc.Items.GetItemAt(0));
                }
            }
            // else info don't change state of the message window
        }

        private void _HideWindow()
        {
            if ((Visibility.Visible == this.Visibility) && 
                App.Current != null && App.Current.MainWindow != null &&
                !App.Current.MainWindow.mainHorizontalSplitter.IsDragging)
                App.Current.MainWindow.ToggleMessageWindowState();
            _isAutoOpened = false;
            _isNeedClose = false;
        }

        private void _LostFocusRoutine()
        {
            if (_isNeedClose)
            {
                System.Diagnostics.Debug.Assert(_isAutoOpened);
                System.Diagnostics.Debug.Assert(Visibility.Visible == this.Visibility);

                _timer.Interval = SHOW_POPUP_MESSAGE_TIMER_INTERVAL / 10;
                _timer.Start();
                _timer.Enabled = true;
            }
        }

        #endregion // Private methods

        #region Private constants

        /// <summary>
        /// Message field name.
        /// </summary>
        private const string MESSAGE_FIELD_NAME = "Message";

        /// <summary>
        /// Time field name.
        /// </summary>
        private const string TIME_FIELD_NAME = "Time";

        /// <summary>
        /// Type field name.
        /// </summary>
        private const string TYPE_FIELD_NAME = "Type";

        /// <summary>
        /// Default width for Columns Header.
        /// </summary>
        private const int DEFAULT_COLUMN_HEADER_WIDTH = 35;

        #endregion

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Data source for grid.
        /// </summary>
        private List<MessageWindowDataWrapper> _data = new List<MessageWindowDataWrapper>();

        /// <summary>
        /// Collection view source.
        /// </summary>
        private DataGridCollectionViewSource _collectionSource = null;

        /// <summary>
        /// Initialize flag.
        /// </summary>
        bool _isInited = false;

        private Timer _timer;
        private bool _isNeedClose = false;
        private bool _isAutoOpened = false;

        private const int SHOW_POPUP_MESSAGE_TIMER_INTERVAL = 10000; // [milliseconds]

        #endregion // Private fields
    }
}
