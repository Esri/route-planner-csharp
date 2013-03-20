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
    /// Command duplicates barriers
    /// </summary>
    class DuplicateBarriers : DuplicateCommandBase
    {
        #region Public Fields

        public const string COMMAND_NAME = "ArcLogistics.Commands.DuplicateBarriers";

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
        /// Duplicates barriers
        /// </summary>
        protected override void _Duplicate()
        {
            Project project = App.Current.Project;

            // get barriers collection from barrier manager
            ICollection<Barrier> barriers = App.Current.Project.Barriers.Search(App.Current.CurrentDate);

            List<Barrier> selectedBarriers = new List<Barrier>();

            foreach (Barrier barrier in ((ISupportSelection)ParentPage).SelectedItems)
                selectedBarriers.Add(barrier);

            foreach (Barrier barrier in selectedBarriers)
            {
                Barrier bar = barrier.Clone() as Barrier;
                bar.Name = DataObjectNamesConstructor.GetDuplicateName(barrier.Name, barriers);
                project.Barriers.Add(bar);
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
                    BarriersPage page = (BarriersPage)((MainWindow)App.Current.MainWindow).GetPage(PagePaths.BarriersPagePath);
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
