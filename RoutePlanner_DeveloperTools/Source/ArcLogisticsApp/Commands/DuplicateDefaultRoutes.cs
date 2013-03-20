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
    /// Command duplicates defaultRoutes
    /// </summary>
    class DuplicateDefaultRoutes : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateDefaultRoutes";

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
        /// Duplicates default routes
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            List<Route> selectedRoutes = new List<Route>();

            foreach (Route route in ((ISupportSelection)ParentPage).SelectedItems)
                selectedRoutes.Add(route);

            foreach (Route route in selectedRoutes)
            {
                Route newRoute = route.Clone() as Route;
                newRoute.Name = DataObjectNamesConstructor.GetDuplicateName(route.Name, project.DefaultRoutes);
                project.DefaultRoutes.Add(newRoute);
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
                    DefaultRoutesPage page = (DefaultRoutesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.DefaultRoutesPagePath);
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
