using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ESRI.ArcLogistics.Geometry;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.GraphicObjects;

namespace ESRI.ArcLogistics.App.Tools
{
    class ZonePointTool : PickPointTool
    {
        #region constants

        private const string PICKPOINT_TOOL_ICON_SOURCE = @"..\..\Resources\PNG_Icons\CreatePointZone24.png";

        #endregion

        #region ITool members

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        public override string TooltipText 
        {
            get
            {
                return (string)App.Current.FindResource("ZoneByPointTooltipText");
            } 
        }

        /// <summary>
        /// Tool's title text.
        /// </summary>
        public override string Title
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Icon's URI source.
        /// </summary>
        public override string IconSource 
        {
            get
            {
                return PICKPOINT_TOOL_ICON_SOURCE;
            }
        }

        #endregion
    }
}
