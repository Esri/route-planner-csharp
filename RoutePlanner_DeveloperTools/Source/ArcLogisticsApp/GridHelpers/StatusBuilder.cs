using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Xceed.Wpf.DataGrid;
using System.Windows.Controls;
using ESRI.ArcLogistics.App.Pages;
using System.Windows;

namespace ESRI.ArcLogistics.App.GridHelpers
{
    internal class StatusBuilder
    {
        public StatusBuilder()
        { }

        /// <summary>
        /// Method makes status like: N objects of M selected 
        /// and set it to status container.
        /// </summary>
        /// <param name="collectionSize"></param>
        /// <param name="statusObjectTypeName"></param>
        /// <param name="xceedControl"></param>
        /// <param name="statusContainer"></param>
        public void FillSelectionStatus(int collectionSize, string statusObjectTypeName, int selectedCount, ESRI.ArcLogistics.App.Pages.Page page)
        {
            string status = "";
            if (!string.IsNullOrEmpty(statusObjectTypeName))
            {
                status = (selectedCount > 0)?
                            string.Format((string)App.Current.FindResource("SelectedGridStatusPattern"), selectedCount, statusObjectTypeName, collectionSize) :
                            string.Format((string)App.Current.FindResource("DefauGridStatusPattern"), collectionSize, statusObjectTypeName);
            }

            App.Current.MainWindow.StatusBar.SetStatus(page, status);
        }

        /// <summary>
        /// Method makes status like: N objects selected 
        /// and set it to status container.
        /// </summary>
        /// <param name="collectionSize"></param>
        /// <param name="statusObjectTypeName"></param>
        /// <param name="xceedControl"></param>
        /// <param name="statusContainer"></param>
        public void FillSelectionStatusWithoutCollectionSize(int collectionSize, string statusObjectTypeName, int selectionSize, ESRI.ArcLogistics.App.Pages.Page page)
        {
            string status = "";
            if (!string.IsNullOrEmpty(statusObjectTypeName))
            {
                status = (selectionSize > 0)?
                            string.Format((string)App.Current.FindResource("SelectedGridWithoutCollectionSizeStatusPattern"), selectionSize, statusObjectTypeName) :
                            string.Format((string)App.Current.FindResource("DefauGridStatusPattern"), collectionSize, statusObjectTypeName);
            }

            App.Current.MainWindow.StatusBar.SetStatus(page, status);
        }

        /// <summary>
        /// Sets status like: "ObjectName" object is being edited
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="objectType"></param>
        /// <param name="page"></param>
        public void FillEditingStatus(string objectName, string objectType, ESRI.ArcLogistics.App.Pages.Page page)
        {
            string status = string.Format((string)App.Current.FindResource("EditingObject"), objectName, objectType);
            App.Current.MainWindow.StatusBar.SetStatus(page, status);
        }

        /// <summary>
        /// Sets status like : New object being created
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="page"></param>
        public void FillCreatingStatus(string objectType, ESRI.ArcLogistics.App.Pages.Page page)
        {
            string status = string.Format((string)App.Current.FindResource("CreatingObject"), objectType, objectType);
            App.Current.MainWindow.StatusBar.SetStatus(page, status);
        }
    }
}
