using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command deletes drivers
    /// </summary>
    class DeleteDrivers : DeleteCommandBase<Driver>
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteDrivers";

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
        /// Deletes drivers
        /// </summary>
        protected override void _Delete(IList<Driver> selectedObjects)
        {
            var deletionChecker = _Application.Project.DeletionCheckingService;
            var ids = selectedObjects.Select(_ => _.Id).ToList();

            var driver = deletionChecker.QueryDefaultRouteDrivers(selectedObjects).FirstOrDefault();
            if (driver != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_DEFAULT_ROUTE_KEY, driver);
                _Application.Messenger.AddError(message);

                return;
            }

            driver = deletionChecker.QueryFutureRouteDrivers(selectedObjects, DateTime.Now)
                .FirstOrDefault();
            if (driver != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_FUTURE_ROUTE_KEY, driver);
                _Application.Messenger.AddError(message);

                return;
            }

            foreach (Driver dr in selectedObjects)
                App.Current.Project.Drivers.Remove(dr);

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
                    DriversPage page = (DriversPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.DriversPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion DeleteCommandBase Protected Properties

        #region Private Methods

        private void _FilterDrivers(IDataObjectCollection<Route> routes, IList filterObjects,
                                    ref Collection<ESRI.ArcLogistics.Data.DataObject> filtredObjects)
        {
            for (int index = 0; index < routes.Count; ++index)
                _FilterObject(routes[index].Driver, filterObjects, ref filtredObjects);
        }

        #endregion Private Methods

        #region private constants
        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// default route.
        /// </summary>
        private const string ASSIGNED_TO_DEFAULT_ROUTE_KEY = "DriverAssignedToDefaultRoute";

        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// route planned for current date or any date after it.
        /// </summary>
        private const string ASSIGNED_TO_FUTURE_ROUTE_KEY = "DriverAssignedToFutureRoute";
        #endregion

        #region Private Members

        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members
    }
}
