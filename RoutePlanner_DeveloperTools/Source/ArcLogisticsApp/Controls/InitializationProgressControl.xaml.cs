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
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Interaction logic for InitializationProgressControl.
    /// <remarks>Count of progress states is 6 and equals count of Application initialization statuses. 
    /// If count of statuses will be changed - need to refactor control.</remarks>
    /// </summary>
    internal partial class InitializationProgressControl : Window
    {
        #region Constructor

        /// <summary>
        /// Creates an instance of <c>InitializationProgressControl</c> class.
        /// </summary>
        /// <param name="progressIndicator">Progress indicator which steps control should reflect.</param>
        public InitializationProgressControl(IProgressIndicator progressIndicator)
        {
            InitializeComponent();

            // Set progress indicator and subscribe on property change for update.
            Debug.Assert(progressIndicator != null);
            Debug.Assert(progressIndicator.CurrentStep == 0); // Indicator must be on the first step.
            _progressIndicator = progressIndicator;

            _progressIndicator.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_ProgressChanged);

            // Set current status text.
            Statustext.Text = _progressIndicator.Message;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates progress indicator state.
        /// </summary>
        private void _UpdateState()
        {
            Debug.Assert(rotationCanvas.Children.Count > _progressIndicator.CurrentStep); // Current step index should be less than count of elements.
            Debug.Assert(rotationCanvas.Children[_progressIndicator.CurrentStep] != null); // Element with current index shouldn't be null.

            // Fill next step sector.
            ((Path)rotationCanvas.Children[_progressIndicator.CurrentStep]).Fill = (Brush)this.FindResource(COMPLETED_COLOR_NAME);
            
            // Set status text.
            Statustext.Text = _progressIndicator.Message;

            // Referesh window.
            this.Refresh();

            // Close window if this is the last step.            
            if (_progressIndicator.CurrentStep == _progressIndicator.StepCount)
            {
                // Small delay before close.
                Thread.Sleep(CLOSE_CONTROL_DELAY);
                this.Close();
            }
        }

        /// <summary>
        /// Called when progress indicator property changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _ProgressChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // If status message changed - update control bar and text.
            if (e.PropertyName == MESSAGE_PROPERTY_NAME)
                _UpdateState();
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Completed color resource name.
        /// </summary>
        private const string COMPLETED_COLOR_NAME = "CompletedGradientBrush";

        /// <summary>
        /// Message property name.
        /// </summary>
        private const string MESSAGE_PROPERTY_NAME = "Message";
        
        /// <summary>
        /// close delay in milliseconds.
        /// </summary>
        private const int CLOSE_CONTROL_DELAY = 500;

        #endregion

        #region Private Fields

        /// <summary>
        /// Progress indicator.
        /// </summary>
        IProgressIndicator _progressIndicator = null;

        #endregion
    }

    /// <summary>
    /// Class creates extension methods for refresh control in necessary time moment.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Empty delegate.
        /// </summary>
        private static Action EmptyDelegate = delegate() { };

        /// <summary>
        /// Rerenders UI elemnt.
        /// </summary>
        /// <param name="uiElement">UI element to rerender.</param>
        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }
}
