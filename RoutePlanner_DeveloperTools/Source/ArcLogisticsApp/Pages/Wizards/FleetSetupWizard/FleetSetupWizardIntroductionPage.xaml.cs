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
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.Properties;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interaction logic for FleetSetupWizardIntroductionPage.xaml
    /// </summary>
    internal partial class FleetSetupWizardIntroductionPage : WizardPageBase, ISupportNext, ISupportCancel
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public FleetSetupWizardIntroductionPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(fleetSetupWizardIntroductionPage_Loaded);
            this.Unloaded += new RoutedEventHandler(fleetSetupWizardIntroductionPage_Unloaded);
            App.Current.Exit += new ExitEventHandler(current_Exit);
        }

        #endregion // Constructors

        #region ISupportNext members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Next" button clicked.
        /// </summary>
        public event EventHandler NextRequired;

        #endregion // ISupportNext members

        #region ISupportCancel members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Cancel" button clicked.
        /// </summary>
        public event EventHandler CancelRequired;

        #endregion // ISupportCancel members

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        private void fleetSetupWizardIntroductionPage_Loaded(object sender, RoutedEventArgs e)
        {
            checkBoxAutomaticallyShowWizard.IsChecked = !Settings.Default.IsAutomaticallyShowFleetWizardEnabled;
        }

        /// <summary>
        /// Page unloaded handler.
        /// </summary>
        private void fleetSetupWizardIntroductionPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _StoreState();
        }

        /// <summary>
        /// Application close handler.
        /// </summary>
        private void current_Exit(object sender, ExitEventArgs e)
        {
            _StoreState();
        }

        /// <summary>
        /// Next button click handler.
        /// </summary>
        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (null != NextRequired)
                NextRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Cancel button click handler.
        /// </summary>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (null != CancelRequired)
                CancelRequired(this, EventArgs.Empty);
        }

        #endregion // Event handlers

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _StoreState()
        {
            Settings.Default.IsAutomaticallyShowFleetWizardEnabled = (false == checkBoxAutomaticallyShowWizard.IsChecked);
            Settings.Default.Save();
        }

        #endregion // Private methods
    }
}
