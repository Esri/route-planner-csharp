using System.Collections.Generic;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.App.Pages;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// Command deletes barriers
    /// </summary>
    class DeleteBarriers : DeleteCommandBase<Barrier>
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DeleteBarriers";

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
        /// Deletes barriers
        /// </summary>
        protected override void _Delete(IList<Barrier> selectedObjects)
        {
            Collection<Barrier> removingBarriers = new Collection<Barrier>();
            foreach (Barrier bar in selectedObjects)
                removingBarriers.Add(bar);

            foreach (Barrier bar in removingBarriers)
                App.Current.Project.Barriers.Remove(bar);

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
                    BarriersPage page = (BarriersPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.BarriersPagePath);
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
