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
    /// Command duplicates locations
    /// </summary>
    class DuplicateLocations : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateLocations";

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
        /// Duplicates locations
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            List<Location> selectedLocations = new List<Location>();

            foreach (Location loc in ((ISupportSelection)ParentPage).SelectedItems)
                selectedLocations.Add(loc);

            foreach (Location location in selectedLocations)
            {
                Location loc = location.Clone() as Location;
                loc.Name = DataObjectNamesConstructor.GetDuplicateName(location.Name, project.Locations);
                project.Locations.Add(loc);
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
                    LocationsPage page = (LocationsPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.LocationsPagePath);
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
