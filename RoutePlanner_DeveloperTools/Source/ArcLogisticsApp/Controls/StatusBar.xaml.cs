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
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using ESRI.ArcLogistics.App.Converters;
using AppPages = ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Main window status bar implementation class.
    /// </summary>
    public partial class StatusBar : UserControl
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance on the <c>StatusBar</c> class.
        /// </summary>
        public StatusBar()
        {
            InitializeComponent();

            _SetMessageButtonBinding();

            // reset state - hide work status
            workingStatus.UpdateHostLayout(0, null);
        }

        #endregion

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Working status.
        /// </summary>
        /// <remarks>For hide - set as null.</remarks>
        public string WorkingStatus
        {
            set
            {
                if (value != null)
                    workingStatus.UpdateHostLayout(this.ActualWidth, value.ToString());
                else
                {
                    // hide working status
                    workingStatus.UpdateHostLayout(0, null);

                    // restore previosly status
                    AppPages.Page currPage = App.Current.MainWindow.CurrentPage;
                    Debug.Assert(null != currPage); // not valide state
                    SetStatus(currPage, _statusesHash[currPage]);
                }
            }
        }

        #endregion

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method sets status bar content.
        /// </summary>
        /// <param name="sourcePage">Status source page.</param>
        /// <param name="statusContent">Status content (must be simple string or control,
        /// or null for clearing).</param>
        public void SetStatus(AppPages.Page sourcePage, object statusContent)
        {
            Debug.Assert(null != sourcePage);

            if (statusContent is string)
                statusContent = _CreateStatusLabel(statusContent.ToString());

            _SetStatus(sourcePage, statusContent);
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Button message click handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void ButtonMessage_Click(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.ToggleMessageWindowState();
        }

        /// <summary>
        /// Creates status label with text.
        /// </summary>
        /// <param name="text">Status label text.</param>
        /// <returns>Creatde status label.</returns>
        private object _CreateStatusLabel(string text)
        {
            Debug.Assert(null != text);

            var statusLabel = new Label();
            statusLabel.Style = (Style)App.Current.FindResource(STATUS_LABEL_STYLE);
            statusLabel.Content = text;
            return statusLabel;
        }

        /// <summary>
        /// Adds or updates status for selected page.
        /// </summary>
        /// <param name="sourcePage">Source page to status updating.</param>
        /// <param name="statusContent">Status content (can be null).</param>
        private void _SetStatus(AppPages.Page sourcePage, object statusContent)
        {
            Debug.Assert(null != sourcePage);

            if (statusContent == null)
                // create empty status
                StatusContentControl.Content = _CreateStatusLabel(string.Empty);
            else
            {   // update status in source page
                if (_statusesHash.Contains(sourcePage))
                {
                    // only if real changed
                    if (!_statusesHash[sourcePage].Equals(statusContent))
                        _statusesHash[sourcePage] = statusContent;
                }
                else
                    // add status to page
                    _statusesHash.Add(sourcePage, statusContent);

                // force update status for current page
                if (App.Current.MainWindow.CurrentPage.Equals(sourcePage))
                    StatusContentControl.Content = statusContent;
            }
        }

        /// <summary>
        /// Sets MessageButton binding to "Visibility" propety of message window.
        /// </summary>
        private void _SetMessageButtonBinding()
        {
            var propertyBinding = new Binding(BINDING_PATH_VISIBILITY);
            propertyBinding.NotifyOnSourceUpdated = true;
            propertyBinding.Mode = BindingMode.TwoWay;
            propertyBinding.Source = App.Current.MainWindow.MessageWindow;
            propertyBinding.Converter = new VisibilityToBooleanConverter();
            BindingOperations.SetBinding(ButtonMessages,
                                         ToggleButton.IsCheckedProperty, propertyBinding);
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string BINDING_PATH_VISIBILITY = "Visibility";
        private const string STATUS_LABEL_STYLE = "SelectionPageStatusLabelStyle";

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Statuses hash table.
        /// </summary>
        private Hashtable _statusesHash = new Hashtable();

        #endregion // Private fields
    }
}
