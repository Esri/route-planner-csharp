namespace ESRI.ArcLogistics.App.Tools
{    
    /// <summary>
    /// Tool for creating polygon barriers.
    /// </summary>
    class BarrierPolygonTool : PickPolygonTool
    {
        #region ITool members

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        public override string TooltipText 
        {
            get
            {
                return (string)App.Current.FindResource(PICK_POLYGON_BARRIER_TOOLTIP_RESOURCE_NAME);
            }
        }

        /// <summary>
        /// Tool's title text.
        /// </summary>
        public override string Title
        {
            get
            {
                return (string)App.Current.FindResource("ShapeBarrierLabel");
            }
        }

        /// <summary>
        /// Icon's URI source.
        /// </summary>
        public override string IconSource 
        {
            get
            {
                return PICK_POLYGON_BARRIER_TOOL_ICON_SOURCE;
            }
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Tooltip resource name.
        /// </summary>
        private const string PICK_POLYGON_BARRIER_TOOLTIP_RESOURCE_NAME = "BarrierByPolygonTooltipText";

        /// <summary>
        /// Title resource name.
        /// </summary>
        private const string PICK_POLYGON_BARRIER_TITLE_RESOURCE_NAME = "ShapeBarrierLabel";

        /// <summary>
        /// Path to polygon barrier icon.
        /// </summary>
        private const string PICK_POLYGON_BARRIER_TOOL_ICON_SOURCE = @"..\..\Resources\PNG_Icons\PolygonBarriers24.png";

        #endregion
    }
}
