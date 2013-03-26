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
    /// Interaction logic for FleetSetupWizardFinishPage.xaml
    /// </summary>
    internal partial class FleetSetupWizardFinishPage : WizardPageBase, ISupportFinish
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public FleetSetupWizardFinishPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(fleetSetupWizardFinishPage_Loaded);
        }

        #endregion // Constructors

        #region ISupportFinish members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Finish" button clicked.
        /// </summary>
        public event EventHandler FinishRequired;

        #endregion // ISupportFinish members

        #region Private properties

        /// <summary>
        /// Specialized context.
        /// </summary>
        private FleetSetupWizardDataContext DataKeeper
        {
            get
            {
                return DataContext as FleetSetupWizardDataContext;
            }
        }

        #endregion
        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        private void fleetSetupWizardFinishPage_Loaded(object sender, RoutedEventArgs e)
        {
            if ((0 < DataKeeper.AddedOrders.Count) && (0 < DataKeeper.Routes.Count))
            {
                labelTitle.Content = (string)App.Current.FindResource("FleetSetupWizardFinishPageTitleRoute");
                textFinish.Text = (string)App.Current.FindResource("FleetSetupWizardFinishPageText3");
                buttonFinish.Content = (string)App.Current.FindResource("BuildRoutesCommandTitle");
                buttonFinish.Width = (double)App.Current.FindResource("LargeWizardPageButtonWidth");
            }
            else
            {
                labelTitle.Content = (string)App.Current.FindResource("FleetSetupWizardFinishPageTitle");
                textFinish.Text = (string)App.Current.FindResource("FleetSetupWizardFinishPageText2");
                buttonFinish.Content = (string)App.Current.FindResource("ButtonHeaderFinish");
                buttonFinish.Width = (double)App.Current.FindResource("DefaultWizardPageButtonWidth");
            }
        }

        /// <summary>
        /// Finish button click handler.
        /// </summary>
        private void buttonFinish_Click(object sender, RoutedEventArgs e)
        {
            if (null != FinishRequired)
                FinishRequired(this, EventArgs.Empty);
        }

        #endregion // Event handlers
    }
}
