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
using System.Diagnostics;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.Tools
{
    /// <summary>
    /// Tool class for reverse geocoding orders and locations.
    /// </summary>
    class AddressByPointTool : PickPointTool
    {
        #region Public methods

        /// <summary>
        /// Set geocodable type(location or order)
        /// </summary>
        /// <param name="geocodableType">Geocodable type.</param>
        public void SetGeocodableType(Type geocodableType)
        {
            if (geocodableType == typeof(Order))
            {
                _toolTip = (string)App.Current.FindResource(ORDER_TOOLTIP_TEXT_RESOURCE_NAME);
            }
            else if(geocodableType == typeof(Location))
            {
                _toolTip = (string)App.Current.FindResource(LOCATION_TOOLTIP_TEXT_RESOURCE_NAME);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        #endregion

        #region ITool members

        /// <summary>
        /// Tool's tooltip text.
        /// </summary>
        public override string TooltipText 
        {
            get
            {
                return _toolTip;
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
                return ADDRESSBYPOINT_TOOL_ICON_SOURCE;
            }
        }

        #endregion

        #region constants

        /// <summary>
        /// Resource name for order tool tip string.
        /// </summary>
        private const string ORDER_TOOLTIP_TEXT_RESOURCE_NAME = "FindOrderAddressByPointTooltipText";

        /// <summary>
        /// Resource name for location tool tip string.
        /// </summary>
        private const string LOCATION_TOOLTIP_TEXT_RESOURCE_NAME = "FindLocationAddressByPointTooltipText";

        /// <summary>
        /// Tool icon source path.
        /// </summary>
        private const string ADDRESSBYPOINT_TOOL_ICON_SOURCE = @"..\..\Resources\PNG_Icons\FindAddressByPoint24.png";

        #endregion

        #region Private members

        /// <summary>
        /// Tool tip.
        /// </summary>
        private string _toolTip;

        #endregion
    }
}
