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
using Xceed.Wpf.DataGrid;
using System.Windows.Forms;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.DomainObjects;
using System.Windows;

namespace ESRI.ArcLogistics.App.SpecialInsertionRow
{
    // APIREV: comments are absent
    // APIREV: move to GridHelpers
    internal class RouteInsertionCell : InsertionCell
    {
        public RouteInsertionCell()
        : base()
        {
        }

        protected override CellEditor GetCellEditor()
        {
            // if current column is "Name" return custom editor
            if (this.FieldName.CompareTo("Name") == 0)
            {
                CellEditor routeNameEditor = (CellEditor)App.Current.FindResource("InsertionRowRouteNameEditor");
                return routeNameEditor;
            }

            // otherwise return default editor
            return base.GetCellEditor();
        }
    }
}
