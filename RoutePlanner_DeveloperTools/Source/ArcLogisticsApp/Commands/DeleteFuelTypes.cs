using System.Collections.Generic;
using System.Linq;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command deletes fuel types
    /// </summary>
    class DeleteFuelTypes : DeleteCommandBase<FuelType>
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteFuelTypes";

        public override string Name
        {
            get
            {
                return COMMAND_NAME;
            }
        }

        #endregion

        #region DeleteCommandBase Protected Methods

        /// <summary>
        /// Deletes fuel types
        /// </summary>
        protected override void _Delete(IList<FuelType> selectedObjects)
        {
            var deletionChecker = _Application.Project.DeletionCheckingService;

            var fuelType = deletionChecker.QueryAssignedFuelTypes(selectedObjects)
                .FirstOrDefault();
            if (fuelType != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_VEHICLE_KEY, fuelType);
                _Application.Messenger.AddError(message);

                return;
            }

            foreach (FuelType ft in selectedObjects)
                App.Current.Project.FuelTypes.Remove(ft);

            App.Current.Project.Save();
        }

        #endregion DeleteCommandBase Protected Methods

        #region DeleteCommandBase Protected Properties

        protected override ISupportDataObjectEditing ParentPage
        {
            get 
            {
                if (_parentPage == null)
                {
                    FuelPage page = (FuelPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.FuelTypesPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion DeleteCommandBase Protected Properties

        #region Private Members

        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members

        #region private constants
        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// vehicle.
        /// </summary>
        private const string ASSIGNED_TO_VEHICLE_KEY = "FuelTypeAssignedToVehicle";
        #endregion
    }
}
