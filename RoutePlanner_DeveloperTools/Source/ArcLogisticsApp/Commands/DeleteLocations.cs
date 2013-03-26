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
    /// Command deletes locations
    /// </summary>
    class DeleteLocations : DeleteCommandBase<Location>
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteLocations";

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
        /// Deletes locations
        /// </summary>
        protected override void _Delete(IList<Location> selectedObjects)
        {
            var deletionChecker = _Application.Project.DeletionCheckingService;

            var location = deletionChecker.QueryDefaultRouteLocations(selectedObjects)
                .FirstOrDefault();
            if (location != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_DEFAULT_ROUTE_KEY, location);
                _Application.Messenger.AddError(message);

                return;
            }

            location = deletionChecker.QueryFutureRouteLocations(selectedObjects, DateTime.Now)
                .FirstOrDefault();
            if (location != null)
            {
                var message = _Application.GetString(ASSIGNED_TO_FUTURE_ROUTE_KEY, location);
                _Application.Messenger.AddError(message);

                return;
            }

            foreach (Location item in selectedObjects)
                App.Current.Project.Locations.Remove(item);

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
                    LocationsPage page = (LocationsPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.LocationsPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion DeleteCommandBase Protected Properties

        #region Private Methods

        private void _FilterLocations(IDataObjectCollection<Route> routes, IList filterObjects,
                                      ref Collection<ESRI.ArcLogistics.Data.DataObject> filtredObjects)
        {
            for (int index = 0; index < routes.Count; ++index)
            {
                Route route = routes[index];
                _FilterObject(route.StartLocation, filterObjects, ref filtredObjects);
                _FilterObject(route.EndLocation, filterObjects, ref filtredObjects);
                _FilterObjects(route.RenewalLocations, filterObjects, ref filtredObjects);
            }
        }

        #endregion Private Methods

        #region private constants
        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// default route.
        /// </summary>
        private const string ASSIGNED_TO_DEFAULT_ROUTE_KEY = "LocationAssignedToDefaultRoute";

        /// <summary>
        /// Resource key for accessing message reporting about object assigned to one or more
        /// route planned for current date or any date after it.
        /// </summary>
        private const string ASSIGNED_TO_FUTURE_ROUTE_KEY = "LocationAssignedToFutureRoute";
        #endregion

        #region Private Members

        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members
    }
}
