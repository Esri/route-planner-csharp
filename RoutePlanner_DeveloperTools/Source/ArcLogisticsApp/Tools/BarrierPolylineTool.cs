namespace ESRI.ArcLogistics.App.Tools
{
    /// <summary>
    /// Tool for creating polyline barriers.
    /// </summary>
    class BarrierPolylineTool : PickPolylineTool
    {
        #region ITool members

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        public override string TooltipText
        {
            get
            {
                return (string)App.Current.FindResource(PICK_POLYLINE_BARRIER_TOOLTIP_RESOURCE_NAME);
            }
        }

        /// <summary>
        /// Tool's title text.
        /// </summary>
        public override string Title
        {
            get
            {
                return (string)App.Current.FindResource(PICK_POLYLINE_BARRIER_TITLE_RESOURCE_NAME);
            }
        }

        /// <summary>
        /// Icon's URI source.
        /// </summary>
        public override string IconSource
        {
            get
            {
                return PICK_POLYLINE_BARRIER_TOOL_ICON_SOURCE;
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Tooltip resource name.
        /// </summary>
        private const string PICK_POLYLINE_BARRIER_TOOLTIP_RESOURCE_NAME = "BarrierByPolylineTooltipText";

        /// <summary>
        /// Title resource name.
        /// </summary>
        private const string PICK_POLYLINE_BARRIER_TITLE_RESOURCE_NAME = "LineBarrierLabel";

        /// <summary>
        /// Path to polyline barrier icon.
        /// </summary>
        private const string PICK_POLYLINE_BARRIER_TOOL_ICON_SOURCE = @"..\..\Resources\PNG_Icons\PolylineBarriers24.png";

        #endregion
    }
}
