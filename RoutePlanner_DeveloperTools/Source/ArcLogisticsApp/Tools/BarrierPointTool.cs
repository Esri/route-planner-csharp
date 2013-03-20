namespace ESRI.ArcLogistics.App.Tools
{
    /// <summary>
    /// Tool for creating point barriers.
    /// </summary>
    class BarrierPointTool : PickPointTool
    {
        #region ITool members

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        public override string TooltipText
        {
            get
            {
                return (string)App.Current.FindResource(PICK_POINT_BARRIER_TOOLTIP_RESOURCE_NAME);
            }
        }

        /// <summary>
        /// Tool's title text.
        /// </summary>
        public override string Title
        {
            get
            {
                return (string)App.Current.FindResource(PICK_POINT_BARRIER_TITLE_RESOURCE_NAME);
            }
        }

        /// <summary>
        /// Icon's URI source.
        /// </summary>
        public override string IconSource
        {
            get
            {
                return PICK_POINT_BARRIER_TOOL_ICON_SOURCE;
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Tooltip resource name.
        /// </summary>
        private const string PICK_POINT_BARRIER_TOOLTIP_RESOURCE_NAME = "BarrierByPointTooltipText";

        /// <summary>
        /// Title resource name.
        /// </summary>
        private const string PICK_POINT_BARRIER_TITLE_RESOURCE_NAME = "PointBarrierLabel";

        /// <summary>
        /// Path to polygon barrier icon.
        /// </summary>
        private const string PICK_POINT_BARRIER_TOOL_ICON_SOURCE = @"..\..\Resources\PNG_Icons\CreatePointBarrier24.png";

        #endregion
    }
}
