using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcLogistics.DomainObjects;
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

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for barrier editor used in popup.
    /// </summary>
    internal partial class BarrierPopupEditor : UserControl
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public BarrierPopupEditor()
        {
            InitializeComponent();
        }

        #endregion

        #region Public events

        /// <summary>
        /// Event is raised when "Ok" button was pressed and tool finished its job.
        /// </summary>
        public event EventHandler OnComplete;

        /// <summary>
        /// Event is raised when "Cancel" button was pressed and tool finished its job.
        /// </summary>
        public event EventHandler OnCancel;

        #endregion

        #region Public members

        /// <summary>
        /// Initialize popup editor.
        /// </summary>
        /// <param name="barrier"></param>
        /// <param name="popup"></param>
        public void Initialize(Barrier barrier, Popup popup)
        {
            _barrier = barrier;
            BarrierEditor.Barrier = barrier;
            _popup = popup;

            // Save initial barrier effect.
            _initialBarrierEffect = barrier.BarrierEffect.Clone() as BarrierEffect;

            App.Current.MainWindow.PreviewMouseDown += new MouseButtonEventHandler(_MainWindowPreviewMouseDown);
            App.Current.MainWindow.Deactivated += new System.EventHandler(_MainWindowDeactivated);
            this.KeyDown += new KeyEventHandler(_BarrierPopupEditorKeyDown);
        }

        /// <summary>
        /// Close popup editor.
        /// </summary>
        public void Close()
        {
            // Close popup.
            _Deinitialize();
        }

        #endregion

        #region Private methods


        /// <summary>
        /// React on apply.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _OkButtonClick(object sender, RoutedEventArgs e)
        {
            // Apply changes.
            _SaveChanges();
        }

        /// <summary>
        /// Saves changes and raises OnComplete event.
        /// </summary>
        private void _SaveChanges()
        {
            // Save changes.
            BarrierEditor.SaveState();

            // Close popup.
            _Deinitialize();

            // Raise OnComplete event.
            if (OnComplete != null)
                OnComplete(this, EventArgs.Empty);
        }

        /// <summary>
        /// React on cancel.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _CancelButtonClick(object sender, RoutedEventArgs e)
        {
            // Cancel changes.
            _CloseAndCancelChanges();
        }

        /// <summary>
        /// Deinitialize control and cancel all changes, that user made to barrier effect.
        /// </summary>
        private void _CloseAndCancelChanges()
        {
            // Close popup.
            _Deinitialize();

            // Revert barrier effect.
            _barrier.BarrierEffect = _initialBarrierEffect;

            // Raise OnCancel event.
            if (OnCancel != null)
                OnCancel(this, EventArgs.Empty);
        }

        /// <summary>
        /// Deinitialize popup editor.
        /// </summary>
        private void _Deinitialize()
        {
            App.Current.MainWindow.PreviewMouseDown -= new MouseButtonEventHandler(_MainWindowPreviewMouseDown);
            App.Current.MainWindow.Deactivated -= new System.EventHandler(_MainWindowDeactivated);

            // Set DependencyProperty of User Control to null. 
            BarrierEditor.Barrier = null;

            _popup.IsOpen = false;
        }

        #endregion

        #region Private event handlers.

        /// <summary>
        /// Handler for "Esc" and "Enter" buttons.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        void _BarrierPopupEditorKeyDown(object sender, KeyEventArgs e)
        {
            // If enter was pressed - commit changes.
            if (e.Key == Key.Enter)
                _SaveChanges();
            // If escape was changed - cancel changes.
            else if (e.Key == Key.Escape)
                _CloseAndCancelChanges();
        }

        /// <summary>
        /// React on window deactivated.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MainWindowDeactivated(object sender, System.EventArgs e)
        {
            // Cancel changes.
            _CloseAndCancelChanges();
        }

        /// <summary>
        /// Close popup on click on window.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _MainWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Apply changes.
            _SaveChanges();
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Edited barrier.
        /// </summary>
        private Barrier _barrier;

        /// <summary>
        /// Barrier effect of barrier when it comes in editor.
        /// </summary>
        private BarrierEffect _initialBarrierEffect;

        /// <summary>
        /// Parent popup control.
        /// </summary>
        private Popup _popup;

        #endregion
    }
}
