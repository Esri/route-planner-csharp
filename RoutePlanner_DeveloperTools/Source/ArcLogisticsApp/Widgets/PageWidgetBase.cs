using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Widgets
{
    // APIREV: make internal
    public abstract class PageWidgetBase : PageWidget
    {
        public override void Initialize(Page page)
        {
            Debug.Assert(page != null);
            _page = page;
        }

        #region protected methods

        protected Page _Page
        {
            get { return _page; }
        }

        #endregion

        #region private members

        private Page _page;

        #endregion
    }
}
