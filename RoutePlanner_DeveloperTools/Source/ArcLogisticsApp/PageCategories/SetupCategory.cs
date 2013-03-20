using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESRI.ArcLogistics.App.PageCategories
{
    internal class SetupCategory : PageCategoryItem
    {
        #region Constructors

        public SetupCategory()
        {
            _CheckCategoryAllowed();

            App.Current.ProjectLoaded += new EventHandler(OnProjectLoaded);
            App.Current.ProjectClosed += new EventHandler(OnProjectClosed);
        }

        #endregion

        #region Protected methods

        protected void _CheckCategoryAllowed()
        {
            IsEnabled = (App.Current.Project != null);
        }

        #endregion

        #region Event Handlers

        private void OnProjectClosed(object sender, EventArgs e)
        {
            _CheckCategoryAllowed();
        }

        private void OnProjectLoaded(object sender, EventArgs e)
        {
            _CheckCategoryAllowed();
        }

        #endregion
    }
}
