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
using System.IO;
using System.Windows;
using System.Windows.Input;
using WinForms = System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;

using ESRI.ArcLogistics.App.Reports;

namespace ESRI.ArcLogistics.App.Dialogs
{
    /// <summary>
    /// Interaction logic for ReportsSaveDlg.xaml
    /// </summary>
    internal partial class ReportsSaveDlg : Window
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ReportsSaveDlg</c> class.
        /// </summary>
        public ReportsSaveDlg()
        {
            InitializeComponent();

            _InitDialog();
        }

        #endregion // Constructors

        #region Private event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Button "Browse" click event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        /// <remarks>Show FolderBrowseDialog and get user choice.</remarks>
        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDlg = new WinForms.FolderBrowserDialog())
            {
                folderDlg.Description = App.Current.FindString("ReportsSaveDescription");
                if (string.IsNullOrEmpty(savePathEdit.Text))
                    folderDlg.RootFolder = START_FOLDER;
                else
                    folderDlg.SelectedPath = savePathEdit.Text;
                if (System.Windows.Forms.DialogResult.OK == folderDlg.ShowDialog())
                {
                    if (Directory.Exists(folderDlg.SelectedPath))
                        savePathEdit.Text = folderDlg.SelectedPath;
                }
            }
        }

        /// <summary>
        /// Button "Ok" click event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        /// <remarks>Update dialog result and close dialog.</remarks>
        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Button "Cancel" click event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        /// <remarks>Update dialog result and close dialog.</remarks>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Dialog KeyDown event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Key event argumnets.</param>
        /// <remarks>Close dialog by Escape and Enter keys.</remarks>
        private void _Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                DialogResult = (e.Key == Key.Enter);

                Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Dialog closing event handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        /// <remarks>Update dialog result if not set.</remarks>
        private void _Dialog_Closing(object sender, CancelEventArgs e)
        {
            if (!DialogResult.HasValue)
                DialogResult = false;
        }

        #endregion // Private event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes dialog state.
        /// </summary>
        private void _InitDialog()
        {
            // attach to events
            this.KeyDown += new KeyEventHandler(_Dialog_KeyDown);
            this.Closing += new CancelEventHandler(_Dialog_Closing);

            // init type combobox state
            comboboxType.ItemsSource = ReportsHelpers.GetExportTypeNames();
            comboboxType.SelectedIndex = ReportsHelpers.DefaultSelectedTypeIndex;

            // init start folder
            savePathEdit.Text = Environment.GetFolderPath(START_FOLDER);

            this.ShowActivated = true;
        }

        #endregion // Private methods

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init dialog default start folder.
        /// </summary>
        private const Environment.SpecialFolder START_FOLDER =
            Environment.SpecialFolder.MyDocuments;

        #endregion // Private constants
    }
}
 