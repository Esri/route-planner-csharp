using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command deletes default routes
    /// </summary>
    class DeleteDefaultRoutes : DeleteCommandBase<Route>
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteDefaultRoutes";

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
        /// Deletes default routes
        /// </summary>
        protected override void _Delete(IList<Route> selectedObjects)
        {
            Collection<Route> removingRoutes = new Collection<Route>();
            foreach (Route route in selectedObjects)
                removingRoutes.Add(route);

            foreach (Route rt in removingRoutes)
                App.Current.Project.DefaultRoutes.Remove(rt);

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
                    DefaultRoutesPage page = (DefaultRoutesPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.DefaultRoutesPagePath);
                    _parentPage = page;
                }

                return _parentPage;
            }
        }

        #endregion DeleteCommandBase Protected Properties

        #region Private Members

        private ISupportDataObjectEditing _parentPage;

        #endregion Private Members
    }
}
