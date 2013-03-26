/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

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
