using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;
using System.Collections;
using System.Collections.ObjectModel;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command duplicates vehicles specialties
    /// </summary>
    class DuplicateVehicleSpecialties : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateVehicleSpecialties";

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        #endregion

        #region DuplicateCommandBase Protected Methods

        /// <summary>
        /// Duplicates vehicles
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            List<VehicleSpecialty> selectedSpecialties = new List<VehicleSpecialty>();

            foreach (VehicleSpecialty specialty in ((ISupportSelection)ParentPage).SelectedItems)
                selectedSpecialties.Add(specialty);

            foreach (VehicleSpecialty vehicleSpecialty in selectedSpecialties)
            {
                VehicleSpecialty vs = vehicleSpecialty.Clone() as VehicleSpecialty;
                vs.Name = DataObjectNamesConstructor.GetDuplicateName(vehicleSpecialty.Name, project.VehicleSpecialties);
                project.VehicleSpecialties.Add(vs);
            }            

            App.Current.Project.Save();
        }

        #endregion DuplicateCommandBase Protected Methods

        #region DuplicateCommandBase Protected Properties

        protected override ISupportDataObjectEditing ParentPage
        {
            get
            {
                if (_parentPage == null)
                {
                    SpecialtiesPage page = (SpecialtiesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SpecialtiesPagePath);

                    // get vehicle specialties panel
                    _parentPage = page.vehicleSpecialties;
                }
                return _parentPage;
            }
        }

        #endregion DuplicateCommandBase Protected Properties

        #region Private Members

        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members
    }
}
