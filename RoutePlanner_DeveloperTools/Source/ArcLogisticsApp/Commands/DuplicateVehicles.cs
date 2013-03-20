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
    /// Command duplicates vehicles
    /// </summary>
    class DuplicateVehicles : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateVehicles";

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

            List<Vehicle> selectedVehicles = new List<Vehicle>();

            foreach (Vehicle vehicle in ((ISupportSelection)ParentPage).SelectedItems)
                selectedVehicles.Add(vehicle);

            foreach (Vehicle vehicle in selectedVehicles)
            {
                Vehicle veh = vehicle.Clone() as Vehicle;
                veh.Name = DataObjectNamesConstructor.GetDuplicateName(vehicle.Name, project.Vehicles);
                project.Vehicles.Add(veh);
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
                    VehiclesPage page = (VehiclesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.VehiclesPagePath);
                    _parentPage = page;
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
