using System;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.Properties;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interaction logic for FleetSetupWizardImportOrdersDecisionPage.xaml
    /// </summary>
    internal partial class FleetSetupWizardImportOrdersDecisionPage : WizardPageBase,
        ISupportBack, ISupportNext
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        public FleetSetupWizardImportOrdersDecisionPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(fleetSetupWizardImportOrdersDecisionPage_Loaded);
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Is import selected.
        /// </summary>
        public bool IsImportSelected
        {
            get { return _isImportSelected; }
        }

        #endregion // Public properties

        #region ISupportBack members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Back" button clicked.
        /// </summary>
        public event EventHandler BackRequired;

        #endregion // ISupportBack members

        #region ISupportNext members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Occurs when "Next" button clicked.
        /// </summary>
        public event EventHandler NextRequired;

        #endregion // ISupportNext members

        #region Event handlers
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        private void fleetSetupWizardImportOrdersDecisionPage_Loaded(object sender, RoutedEventArgs e)
        {
            _isImportSelected = false;
        }

        /// <summary>
        /// "Back" button click handler.
        /// </summary>
        private void buttonBack_Click(object sender, RoutedEventArgs e)
        {
            if (null != BackRequired)
                BackRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// "Import Orders" button click handler.
        /// </summary>
        private void buttonImportOrders_Click(object sender, RoutedEventArgs e)
        {
            _isImportSelected = true;

            if (null != NextRequired)
                NextRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// "Finish" button click handler.
        /// </summary>
        private void buttonFinish_Click(object sender, RoutedEventArgs e)
        {
            if (null != NextRequired)
                NextRequired(this, EventArgs.Empty);
        }

        #endregion // Event handlers

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private bool _isImportSelected = false;

        #endregion // Private fields
    }
}
