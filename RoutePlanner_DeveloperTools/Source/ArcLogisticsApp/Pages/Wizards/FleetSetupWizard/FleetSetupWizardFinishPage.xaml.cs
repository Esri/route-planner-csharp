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
