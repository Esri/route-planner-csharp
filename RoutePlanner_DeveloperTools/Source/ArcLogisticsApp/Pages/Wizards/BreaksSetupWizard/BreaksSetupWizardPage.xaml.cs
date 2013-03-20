using System;
using System.Windows;
using ESRI.ArcLogistics.BreaksHelpers;
using ESRI.ArcLogistics.DomainObjects;
using System.Diagnostics;

namespace ESRI.ArcLogistics.App.Pages.Wizards
{
    /// <summary>
    /// Interaction logic for BreaksSetupWizardPage.xaml
    /// </summary>
    internal partial class BreaksSetupWizardPage : WizardPageBase, ISupportFinish
    {
        #region Constructors
        /// <summary>
        /// Constructor.
        /// </summary>
        public BreaksSetupWizardPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(_BreaksSetupWizardPageLoaded);
        }

        #endregion // Constructors

        #region ISupportFinish Members

        public event EventHandler FinishRequired;

        #endregion

        #region Event handlers

        /// <summary>
        /// Page loaded handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _BreaksSetupWizardPageLoaded(object sender, RoutedEventArgs e)
        {
            // For default "Cancel" button must be enabled.
            buttonCancel.IsEnabled = true;

            // Get current breaks default type and select proper radiobutton.
            Project project = (DataContext as BreaksSetupWizardDataContext).Project;
            if (project.BreaksSettings.BreaksType == null)
            {
                // If we have no Default Breaks type then disable "Cancel" button,
                // because we cannot navigate to Breaks Action Panel.
                buttonCancel.IsEnabled = false;

                // Select TimeWindowBreaks radiobutton for default.
                TimeWindow.IsChecked = true;
            }
            else if(project.BreaksSettings.BreaksType == BreakType.TimeWindow)
                TimeWindow.IsChecked = true;
            else if (project.BreaksSettings.BreaksType == BreakType.DriveTime)
                DriveTime.IsChecked = true;
            else if (project.BreaksSettings.BreaksType == BreakType.WorkTime)
                WorkTime.IsChecked = true;
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// Ok button click handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ButtonOkClick(object sender, RoutedEventArgs e)
        {
            _StoreState();
            if (null != FinishRequired)
                FinishRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// Cancel button click handler.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (null != FinishRequired)
                FinishRequired(this, EventArgs.Empty);
        }

        /// <summary>
        /// When TimeWindow radio button checked - change selected break type to TimeWindowBreak.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _TimeWindowChecked(object sender, RoutedEventArgs e)
        {
            _selectedType = BreakType.TimeWindow;
        }

        /// <summary>
        /// When DriveTime radio button checked - change selected break type to DriveTimeBreak.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _DriveTimeChecked(object sender, RoutedEventArgs e)
        {
            _selectedType = BreakType.DriveTime;
        }

        /// <summary>
        /// When WorkTime radio button checked - change selected break type to WorkTimeBreak.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _WorkTimeChecked(object sender, RoutedEventArgs e)
        {
            _selectedType = BreakType.WorkTime;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Save selected BreakType to project default breaks settings.
        /// </summary>
        private void _StoreState()
        {
            Project project = (DataContext as BreaksSetupWizardDataContext).Project;

            // If selected break type differs from saved in project - save selected type.
            if (project.BreaksSettings != null)
            {
                if (project.BreaksSettings.BreaksType != _selectedType)
                {
                    // Save selected breaks type.
                    project.BreaksSettings.BreaksType = _selectedType;
                    
                    // Fill breaks collection with one break of the selected type.
                    project.BreaksSettings.DefaultBreaks.Clear();
                    if (_selectedType == BreakType.TimeWindow)
                        project.BreaksSettings.DefaultBreaks.Add(new TimeWindowBreak());
                    else if (_selectedType == BreakType.DriveTime)
                        project.BreaksSettings.DefaultBreaks.Add(new DriveTimeBreak());
                    else if (_selectedType == BreakType.WorkTime)
                        project.BreaksSettings.DefaultBreaks.Add(new WorkTimeBreak());

                    // Save project.
                    project.Save();
                }
            }
        }

        #endregion

        #region Private fields

        /// <summary>
        /// Selected type of breaks.
        /// </summary>
        private BreakType _selectedType;

        #endregion
    }
}
