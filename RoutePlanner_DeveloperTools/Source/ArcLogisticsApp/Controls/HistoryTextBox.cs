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
using System.Collections;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Controls
{
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Selector", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_PopupPanel", Type = typeof(Popup))]
    internal class HistoryTextBox : Control
    {
        #region dependency properties 

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HistoryTextBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnTextChanged)));

        public static readonly DependencyProperty HistoryCandidatesProperty =
            DependencyProperty.Register("HistoryCandidates", typeof(ICollection<HistoryService.HistoryItem>), typeof(HistoryTextBox),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnHistoryCandidatesChanged)));

        public static readonly DependencyProperty HasHistoryCandidatesProperty =
            DependencyProperty.Register("HasHistoryCandidates", typeof(bool), typeof(HistoryTextBox));

        #endregion

        #region static constructor

        static HistoryTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HistoryTextBox), new FrameworkPropertyMetadata(typeof(HistoryTextBox)));
        }

        #endregion

        #region private static methods

        private static void OnHistoryCandidatesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((HistoryTextBox)obj)._OnHistoryCandidatesChanged();
        }

        private static void OnTextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((HistoryTextBox)obj)._OnTextChanged();
        }

        #endregion

        #region public properties

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ICollection<HistoryService.HistoryItem> HistoryCandidates
        {
            get { return (ICollection<HistoryService.HistoryItem>)GetValue(HistoryCandidatesProperty); }
            set { SetValue(HistoryCandidatesProperty, value); }
        }

        public bool HasHistoryCandidates
        {
            get { return (bool)GetValue(HasHistoryCandidatesProperty); }
            set { SetValue(HasHistoryCandidatesProperty, value); }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; }
        }

        public HistoryService HistoryService
        {
            get { return _historyService; }
            set { _historyService = value; }
        }

        #endregion

        #region public methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _InitParts();
            _InitEventHandlers();
        }

        #endregion

        #region private methods

        private void _InitParts()
        {
            _TextBox = (TextBox)this.GetTemplateChild("PART_TextBox");
            _Selector = (Selector)this.GetTemplateChild("PART_Selector");
            _Popup = (Popup)this.GetTemplateChild("PART_PopupPanel");
        }

        private void _InitEventHandlers()
        {
            App.Current.MainWindow.Deactivated += new EventHandler(MainWindow_Deactivated);
            this.GotFocus += new RoutedEventHandler(HistoryTextBox_GotFocus);
            this.Unloaded += new RoutedEventHandler(HistoryTextBox_Unloaded);
            this.KeyDown += new KeyEventHandler(HistoryTextBox_KeyDown);
            this.PreviewKeyDown += new KeyEventHandler(HistoryTextBox_PreviewKeyDown);
            _TextBox.TextChanged +=new TextChangedEventHandler(_TextBox_TextChanged);
            _TextBox.PreviewKeyDown += new KeyEventHandler(_TextBox_PreviewKeyDown);
            _Selector.SelectionChanged += new SelectionChangedEventHandler(_Selector_SelectionChanged);
            _Selector.MouseMove += new MouseEventHandler(_Selector_MouseMove);
            _Selector.MouseLeftButtonUp += new MouseButtonEventHandler(_Selector_MouseLeftButtonUp);
            _Selector.PreviewKeyDown += new KeyEventHandler(_Selector_PreviewKeyDown);
        }

        void HistoryTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // If "Enter" is pressed and nothing selected - then just close popup.
            if (e.Key == Key.Enter && _Selector.SelectedIndex < 0)
                _Popup.IsOpen = false; 
        }

        // Hide popup panel when application losts focus
        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            if (_Popup.IsOpen)
                _Popup.IsOpen = false;
        }

        void HistoryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // If user pressed enter and there are some candidate and one of them is selected
            if (e.Key == Key.Enter && _Popup.IsOpen && _Selector.SelectedIndex >= 0)
            {
                // then just clear candidate which will close the popup.
                HistoryCandidates = null;
                e.Handled = true; // Mark event as handled, so data grid won't commit current record.
            }
        }

        void HistoryTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (HistoryService != null && !string.IsNullOrEmpty(_stringToUpdate))
            {
                HistoryService.UpdateItem(new HistoryService.HistoryItem { String = _stringToUpdate, Category = this.Category });
            }
        }

        void HistoryTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _TextBox.Focus();    
        }

        void _Selector_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                HistoryCandidates = null;
                _TextBox.Focus();
            }
        }

        void _Selector_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HistoryService.HistoryItem? item = _GetSelectorItemUnderPoint(e.GetPosition(_Selector));
            if (item != null)
            {
                _SetHistoryItemToTextBox(item.Value);
                HistoryCandidates = null;
                _TextBox.Focus();
            }
        }

        void _Selector_MouseMove(object sender, MouseEventArgs e)
        {
            HistoryService.HistoryItem? item = _GetSelectorItemUnderPoint(e.GetPosition(_Selector));
            if (item != null)
            {
                _pauseSelectionChangeEventHandler = true;
                _Selector.SelectedItem = item;
                _pauseSelectionChangeEventHandler = false;
            }
        }

        private void _Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_pauseSelectionChangeEventHandler)
                return;

            if (_Selector.SelectedItem != null)
            {
                this.Text = ((HistoryService.HistoryItem)_Selector.SelectedItem).String;
                _stringToUpdate = this.Text;
                _TextBox.CaretIndex = _TextBox.Text.Length;
            }
        }

        /// <summary>
        /// Chages selection in candidates list.
        /// </summary>
        /// <param name="sender">Text box.</param>
        /// <param name="e">Event args.</param>
        private void _TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!HasHistoryCandidates)
                return;

            if (e.Key == Key.Down)
            {
                if (_Selector.SelectedIndex == -1 ||
                    _Selector.SelectedIndex >= HistoryCandidates.Count - 1)
                    _Selector.SelectedIndex = 0;
                else
                    _Selector.SelectedIndex++;
            }
            else if (e.Key == Key.Up)
            {
                if (_Selector.SelectedIndex == 0 || _Selector.SelectedIndex == -1)
                    _Selector.SelectedIndex = HistoryCandidates.Count - 1;
                else
                    _Selector.SelectedIndex--;
            }
            else if (e.Key == Key.Tab) // Auto select first available option by Tab key.
            {
                HistoryCandidates = null;
                _TextBox.Focus();
            }
        }

        private void _TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_pauseTextChangeEventHandler)
                return;

            _stringToUpdate = this.Text = _TextBox.Text;

            if (!string.IsNullOrEmpty(_TextBox.Text))
                HistoryCandidates = HistoryService.SearchItems(_TextBox.Text, Category);
            else
                HistoryCandidates = null;
        }

        private void _OnHistoryCandidatesChanged()
        {
            HasHistoryCandidates = (HistoryCandidates != null && HistoryCandidates.Count > 0);
        }

        private void _OnTextChanged()
        {
            _pauseTextChangeEventHandler = true;
            _TextBox.Text = this.Text;
            _pauseTextChangeEventHandler = false;
        }

        private HistoryService.HistoryItem? _GetSelectorItemUnderPoint(Point point)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(_Selector, point);
            
            DependencyObject element = hitTestResult.VisualHit;
            while (element != null)
            {
                if (element.Equals(_Selector))
                    return null;

                object item = _Selector.ItemContainerGenerator.ItemFromContainer(element);
                if (!item.Equals(DependencyProperty.UnsetValue))
                {
                    HistoryService.HistoryItem historyItem = (HistoryService.HistoryItem)item;
                    return historyItem;
                }

                element = (DependencyObject)VisualTreeHelper.GetParent(element);
            }

            return null;
        }

        private void _SetHistoryItemToTextBox(HistoryService.HistoryItem historyItem)
        {
            _TextBox.Text = ((HistoryService.HistoryItem)historyItem).String;
            _TextBox.CaretIndex = _TextBox.Text.Length;
        }

        #endregion

        #region private members

        private string _category = "Default";
        private HistoryService _historyService = ((App)App.Current).HistoryService;
        private bool _pauseTextChangeEventHandler = false;
        private bool _pauseSelectionChangeEventHandler = false;
        private string _stringToUpdate = "";

        private TextBox _TextBox;
        private Selector _Selector;
        private Popup _Popup;

        #endregion
    }
}
