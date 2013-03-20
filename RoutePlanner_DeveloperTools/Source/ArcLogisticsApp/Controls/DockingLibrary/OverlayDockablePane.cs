using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Overlay dockable pane
    /// </summary>
    internal class OverlayDockablePane : DockablePane
    {
        #region Members
        /// <summary>
        /// Referenced content
        /// </summary>
        private readonly DockableContent _contentReferenced = null;

        /// <summary>
        /// Get referenced content
        /// </summary>
        public DockableContent ReferencedContent
        {
            get { return _contentReferenced; }
        }
        #endregion // Members

        #region Constructors
        /// <summary>
        /// Create overlay dockable pane
        /// </summary>
        /// <param name="dockManager">Dock manager</param>
        /// <param name="content">Content to manage</param>
        public OverlayDockablePane(DockableContent content)
            : base(content)
        {
            _contentReferenced = content;

            Show();

            _state = PaneState.Hidden;
        }
        #endregion // Constructors

        #region Override functions
        /// <summary>
        /// Show this pane and all contained contents
        /// </summary>
        public override void Show()
        {
            ChangeState(PaneState.Docked);
        }

        /// <summary>
        /// Close this pane
        /// </summary>
        /// <remarks>Consider that in this version library this method simply hides the pane.</remarks>
        public override void Close()
        {
            ChangeState(PaneState.Hidden);
        }
        #endregion // Override functions
    }
}
