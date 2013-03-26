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
using ESRI.ArcLogistics.DomainObjects;
using ESRI.ArcLogistics.App.Controls;
using System.Windows;

namespace ESRI.ArcLogistics.App.SpecialInsertionRow
{
    // APIREV: comments are absent
    // APIREV: move to GridHelpers
    internal class RouteInsertionRow : InsertionRow
    {

        protected override Cell CreateCell(ColumnBase column)
        {
            return new RouteInsertionCell();
        }

        protected override bool IsValidCellType(Cell cell)
        {
            return cell is RouteInsertionCell;
        }
    }
}
