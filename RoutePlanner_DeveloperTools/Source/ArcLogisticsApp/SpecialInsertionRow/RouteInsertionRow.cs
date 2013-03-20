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
