using System;
using System.Windows;
using ESRI.ArcLogistics.App.Controls;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Class for setting margin for view button groups.
    /// </summary>
    internal class ViewButtonsMarginUpdater
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="view">Parent view.</param>
        /// <param name="buttonsPanel">Buttons parent element.</param>
        public ViewButtonsMarginUpdater(DockableContent view, FrameworkElement buttonsPanel)
        {
            _view = view;
            _buttonsPanel = buttonsPanel;
         
            view.LayoutUpdated += new EventHandler(_ViewLayoutUpdated);

            _defaultMargin = (Thickness)App.Current.FindResource(VIEW_BUTTONS_STACK_MARGIN_RESOURCE);
        }

        #endregion

        #region Private members

        /// <summary>
        /// React on layout update.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Ignored.</param>
        private void _ViewLayoutUpdated(object sender, EventArgs e)
        {
            if (_view.ContainerPane != null && _view.ContainerPane.brHeader.ActualWidth > 0)
            {
                Thickness newMargin = new Thickness(_defaultMargin.Left, _defaultMargin.Top, _defaultMargin.Right +
                    _view.ContainerPane.brHeader.ActualWidth, _defaultMargin.Bottom);

                // In case of view was loaded not fully visible need to update each time.
                if (_buttonsPanel.Margin.Right < newMargin.Right)
                {
                    _buttonsPanel.Margin = newMargin;
                }
            }
        }

        #endregion

        #region Private constants

        /// <summary>
        /// "ViewButtonsStackMargin" resource name.
        /// </summary>
        private const string VIEW_BUTTONS_STACK_MARGIN_RESOURCE = "ViewButtonsStackMargin";

        #endregion

        #region Private fields

        /// <summary>
        /// Parent view.
        /// </summary>
        private DockableContent _view;

        /// <summary>
        /// Buttons parent element.
        /// </summary>
        private FrameworkElement _buttonsPanel;

        /// <summary>
        /// Default margin for buttons panel.
        /// </summary>
        private Thickness _defaultMargin;

        #endregion
    }
}
