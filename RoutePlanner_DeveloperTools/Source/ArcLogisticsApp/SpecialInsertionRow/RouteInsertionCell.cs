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
