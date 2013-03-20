using System.Collections.Generic;
using System.Linq;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// command deletes driver specialties
    /// </summary>
    class DeleteDriverSpecialties : DeleteCommandBase<DriverSpecialty>
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DueleteDriverSpecialties";

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
        /// Deletes driver specialties
        /// </summary>
        protected override void _Delete(IList<DriverSpecialty> selectedObjects)
        {
            var deletionChecker = _Application.Project.DeletionCheckingService;

            var specialty = deletionChecker.QueryAssignedSpecialties(selectedObjects)
                .FirstOrDefault();
            if (specialty != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_VEHICLE_OR_DRIVER_KEY, specialty);
                _Application.Messenger.AddError(message);

                return;
            }

            specialty = deletionChecker.QueryOrderSpecialties(selectedObjects)
                .FirstOrDefault();
            if (specialty != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_ORDER_KEY, specialty);
                _Application.Messenger.AddError(message);

                return;
            }

            foreach (DriverSpecialty item in selectedObjects)
                App.Current.Project.DriverSpecialties.Remove(item);

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
                    SpecialtiesPage page = (SpecialtiesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.SpecialtiesPagePath);

                    // get driver specialties panel
                    _parentPage = page.driverSpecialties;
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
        /// driver or vehicle.
        /// </summary>
        private const string ASSIGNED_TO_VEHICLE_OR_DRIVER_KEY =
            "SpecialtyAssignedToVehicleOrDriver";

        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// order.
        /// </summary>
        private const string ASSIGNED_TO_ORDER_KEY = "SpecialtyAssignedToOrder";
        #endregion
    }
}
