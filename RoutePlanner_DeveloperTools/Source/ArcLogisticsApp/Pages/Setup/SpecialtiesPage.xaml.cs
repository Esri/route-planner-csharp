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
using System.Windows.Media;
using ESRI.ArcLogistics.App.Help;
using Xceed.Wpf.DataGrid;
using ESRI.ArcLogistics.App.Validators;
using System.Collections.Generic;
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.GridHelpers;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Pages
{
    // APIREV: expose ISpecialtiesPage interface
    /// <summary>
    /// Interaction logic for Specialties.xaml
    /// </summary>
    internal partial class SpecialtiesPage : PageBase
    {
        public const string PAGE_NAME = "Specialties";

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        public SpecialtiesPage()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(SpecialtiesPage_Loaded);
            this.Unloaded += new RoutedEventHandler(SpecialtiesPage_Unloaded);
            IsRequired = false;
            IsAllowed = true;
            CanBeLeft = true;

            driverSpecialties.XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(_DriverSpecialtiesSelectionChanged);
            vehicleSpecialties.XceedGrid.SelectionChanged += new DataGridSelectionChangedEventHandler(_VehicleSpecialtiesSelectionChanged);

            // Init validation callout controller for driver specialties.
            var driverSpecialtiesVallidationCalloutController = new ValidationCalloutController(driverSpecialties.XceedGrid);

            // Init validation callout controller for vehicle specialties.
            var vehicleSpecialtiesVallidationCalloutController = new ValidationCalloutController(vehicleSpecialties.XceedGrid);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Vehicle specialties panel.
        /// </summary>
        public ISupportDataObjectEditing VehicleSpecialtiesPanel
        {
            get { return vehicleSpecialties; }
        }

        /// <summary>
        /// Driver specialties panel.
        /// </summary>
        public ISupportDataObjectEditing DriverSpecialtiesPanel
        {
            get { return driverSpecialties; }
        }

        #endregion

        #region Page Overrided Members

        public override string Name
        {
            get { return PAGE_NAME; }
        }

        public override string Title
        {
            get { return (string)App.Current.FindResource("SpecialtiesPageCaption"); }
        }

        public override System.Windows.Media.TileBrush Icon
        {
            get
            {
                ImageBrush brush = (ImageBrush)App.Current.FindResource("SpecialtiesBrush");
                return brush;
            }
        }

        public override bool CanBeLeft
        {
            get
            {
                // If there are validation error in insertion row - we cannot leave page.
                if (driverSpecialties.XceedGrid.IsInsertionRowInvalid ||
                    vehicleSpecialties.XceedGrid.IsInsertionRowInvalid)
                    return false;
                // If there isnt - we must validate all grid source items.
                else
                    return base.CanBeLeft &&
                    CanBeLeftValidator<DriverSpecialty>.IsValid(App.Current.Project.DriverSpecialties) &&
                    CanBeLeftValidator<VehicleSpecialty>.IsValid(App.Current.Project.VehicleSpecialties);
            }
            protected internal set
            {
                base.CanBeLeft = value;
            }
        }

        #endregion

        #region PageBase overrided members

        public override HelpTopic HelpTopic
        {
            get { return CommonHelpers.GetHelpTopic(PagePaths.SpecialtiesPagePath); }
        }

        public override string PageCommandsCategoryName
        {
            get { return null; }
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Occurs when page loads.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SpecialtiesPage_Loaded(object sender, RoutedEventArgs e)
        {            
            ((MainWindow)App.Current.MainWindow).NavigationCalled += new EventHandler(SpecialtiesPage_NavigationCalled);
            ((MainWindow)App.Current.MainWindow).StatusBar.SetStatus(this, "");
        }

        /// <summary>
        /// Occurs when user try to leave this page
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void SpecialtiesPage_NavigationCalled(object sender, EventArgs e)
        {
            bool hasError = false;

            try
            {
                vehicleSpecialties.XceedGrid.EndEdit();
                CanBeLeft = true;
            }
            catch
            {
                CanBeLeft = false;
                hasError = true; // set flag for remember what vehicle specialties grid has errors
            }

            try
            {
                driverSpecialties.XceedGrid.EndEdit();
                if (!hasError)
                    CanBeLeft = true; 
                else
                    CanBeLeft = false;// if vehicle specialties grid has errors we can't leave this page
            }
            catch
            {
                CanBeLeft = false;
            }

            // If there are validation errors - show them.
            CanBeLeftValidator<DriverSpecialty>.ShowErrorMessagesInMessageWindow
                (App.Current.Project.DriverSpecialties);
            CanBeLeftValidator<VehicleSpecialty>.ShowErrorMessagesInMessageWindow
                (App.Current.Project.VehicleSpecialties);
        }

        private void SpecialtiesPage_Unloaded(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.NavigationCalled -= SpecialtiesPage_NavigationCalled;
            vehicleSpecialties.CancelObjectEditing();
            driverSpecialties.CancelObjectEditing();
        }

        /// <summary>
        /// React on vehicle specialties selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Selection changes event args.</param>
        private void _VehicleSpecialtiesSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (!_isDuringSelectionChanged && e.SelectionInfos[0].AddedItems.Count > 0)
            {
                // Clear selection in driver specialties grid.
                _isDuringSelectionChanged = true;
                driverSpecialties.XceedGrid.SelectedItems.Clear();
                _isDuringSelectionChanged = false;
            }
        }

        /// <summary>
        /// React on driver specialties selection changed.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Selection changes event args.</param>
        private void _DriverSpecialtiesSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            if (!_isDuringSelectionChanged && e.SelectionInfos[0].AddedItems.Count > 0)
            {
                // Clear selection in vehicle specialties grid.
                _isDuringSelectionChanged = true;
                vehicleSpecialties.XceedGrid.SelectedItems.Clear();
                _isDuringSelectionChanged = false;
            }
        }

        #endregion

        #region Private members

        /// <summary>
        /// Flag, which indicates, that selection changes is in progress.
        /// </summary>
        private bool _isDuringSelectionChanged;

        #endregion
    }
}
