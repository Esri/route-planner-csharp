using System;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// The abstract PageWidget class is used as a base class for all widgets.
    /// </summary>
    public abstract class PageWidget : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initialize page with the instance of application.
        /// </summary>
        public abstract void Initialize(Page page);

        /// <summary>
        /// Returns widget title.
        /// </summary>
        public abstract string Title { get; }
    }
}
