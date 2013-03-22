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
using System.Collections;
using System.Collections.Generic;

using Xceed.Wpf.DataGrid;

using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// RouteDetailDescription class
    /// </summary>
    internal class RouteDetailDescription : DataGridDetailDescription
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public RouteDetailDescription()
        {
            RelationName = "RouteInfo";
        }

        #endregion // Constructors

        #region Public members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public Route ParentDataObject
        {
            get { return _route; }
        }

        #endregion // Public members

        #region Override methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        protected override IEnumerable GetDetailsForParentItem(DataGridCollectionViewBase parentCollectionView, object parentItem)
        {
            Debug.Assert(parentItem is Route);

            IEnumerable<Stop> details = new List<Stop>();
            if (null != parentItem)
            {
                _route = (Route)parentItem;
                details = new SortedDataObjectCollection<Stop>(_route.Stops, new StopsComparer());
            }

            return details;
        }

        #endregion // Override methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Route _route = null;

        #endregion // Private members
    }
}
