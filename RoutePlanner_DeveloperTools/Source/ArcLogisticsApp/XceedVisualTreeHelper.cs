using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.Data;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;
using Xceed.Wpf.Controls;
using Xceed.Wpf.DataGrid.Views;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Class for get any elements of xceed visual tree
    /// </summary>
    internal class XceedVisualTreeHelper
    {
        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Method searches visual parent with necessary type from visual tree of DependencyObject.
        /// </summary>
        /// <typeparam name="T">Type of object which should be found.</typeparam>
        /// <param name="from">Source object.</param>
        /// <returns>Found element of visual tree ao null if such element not exist there.</returns>
        public static T FindParent<T>(DependencyObject from)
        where T : class
        {
            T result = null;
            DependencyObject parent = VisualTreeHelper.GetParent(from);

            if (parent is T)
                result = parent as T;
            else if (parent != null)
                result = FindParent<T>(parent);

            return result;
        }

        /// <summary>
        /// Find table scroll viewer.
        /// </summary>
        /// <param name="element">Cell editor.</param>
        /// <returns>Scroll viewer instance or null if it wasn't found.</returns>
        public static TableViewScrollViewer FindScrollViewer(FrameworkElement element)
        {
            Debug.Assert(element != null);

            // If element is TableViewScrollViewer - then jsut return it.
            if (element is TableViewScrollViewer)
                return element as TableViewScrollViewer;

            // Find TableViewScrollViewer parent.
            FrameworkElement curElement = element;
            while (curElement != null && !(curElement is TableViewScrollViewer))
            {
                curElement = (FrameworkElement)VisualTreeHelper.GetParent(curElement);
            }

            return (curElement == null) ? null : (TableViewScrollViewer)curElement;
        }

        /// <summary>
        /// Method returns row from mouse event args
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Row GetRowByEventArgs(RoutedEventArgs e)
        {
            // Get event source element.
            DependencyObject element = e.OriginalSource as DependencyObject;
            if (element == null)
                return null;

            // If it is FrameworkContentElement (when we click on Inline elements that are part of TextBlock) 
            // we need to move up and find visual container.
            while (element != null && element is FrameworkContentElement)
                element = (element as FrameworkContentElement).Parent;

            // For some reason we can run into the null case.
            if (element == null)
                return null;

            // Element must be visual here.
            Debug.Assert(element is Visual);

            // Then we start to go by visual tree up searching Row visual parent.
            // If we run into DataGridControlEx or null then we must exit - no need to search any more.
            while (element != null && !(element is Row) && !(element is DataGridControlEx)) 
                element = VisualTreeHelper.GetParent(element);

            if (element is Row)
                return element as Row; // We found row!

            // We didn't find a row.
            return null;
        }

        /// <summary>
        /// Method returns Cell from mouse event args
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Cell GetCellByEventArgs(RoutedEventArgs e)
        {
            Cell cell = null;
            if (e.OriginalSource is FrameworkElement)
                cell = _GetCellByFrameworkElement((FrameworkElement)e.OriginalSource);
            else if (e.OriginalSource is FrameworkContentElement)
                cell = _GetCellByFrameworkContentElement((FrameworkContentElement)e.OriginalSource);
            else
                cell = null;

            return cell;
        }

        /// <summary>
        /// Method returns parent cell for cell editor
        /// </summary>
        /// <param name="cellEditor">Cell editor.s</param>
        /// <returns>Cell.</returns>
        public static Cell GetCellByEditor(UIElement cellEditor)
        {
            return _GetCellByEditor(cellEditor);
        }

        /// <summary>
        /// Method returns parent grid control for cell editor
        /// </summary>
        /// <param name="cellEditor"></param>
        /// <returns></returns>
        public static DataGridControl GetGridByEditor(UIElement cellEditor)
        {
            DataGridControl dataGrid = null;

            UIElement control = cellEditor;

            //NOTE: View visual tree while we're not found DataGridControl 
            //condition "parent != App.Current.MainWindow" added to avoid endless loop when DataGridControl was not found by any reason
            while (!(control is Xceed.Wpf.DataGrid.DataGridControl) && control != App.Current.MainWindow && control != null)
                control = (UIElement)VisualTreeHelper.GetParent(control);

            if (control is DataGridControl)
                dataGrid = (DataGridControl)control;

            return dataGrid; // if DataGrid not found - return null
        }

        /// <summary>
        /// Finds ATextBox inside of Framework element.
        /// </summary>
        /// <param name="sourceElement">Element to search inside.</param>
        /// <returns>First found TextBox instance or null if such control is not found.</returns>
        public static TextBox FindTextBoxInsideElement(FrameworkElement sourceElement)
        {
            if (sourceElement == null)
                return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(sourceElement);

            // Iterate through all the childer until TextBox (or inherited control) is not found.
            FrameworkElement result = null;
            for (int child = 0; child < childrenCount && result == null; child++)
            {
                var element = VisualTreeHelper.GetChild(sourceElement, child) as FrameworkElement;
                if (element != null)
                {
                    if (element is AutoSelectTextBox ||
                        element is NumericTextBox ||
                        element is TextBox)
                        result = element; // TextBox found.
                    else
                        result = FindTextBoxInsideElement(element); // Try to recursively find
                    // among the element's children.
                }
            }

            Debug.Assert(result is AutoSelectTextBox ||
                         result is NumericTextBox ||
                         result is TextBox ||
                         result == null);
            return (result is AutoSelectTextBox ||
                    result is NumericTextBox ||
                    result is TextBox) ? (TextBox)result : null;
        }

        #endregion // Public methods

        #region Private Methods

        /// <summary>
        /// Method looks over visual tree (from FrameworkElement to it's parets), finds Cell element in parents and returns it
        /// if Cell isn't found - return null
        /// </summary>
        /// <param name="element">FrameWork Element.</param>
        /// <returns>Cell.</returns>
        private static Cell _GetCellByFrameworkElement(FrameworkElement element)
        {
            Cell cell = null;

            while (element.TemplatedParent != null && !(element.TemplatedParent is RowSelector) &&
                !(element.TemplatedParent is DataCell) && !(element.TemplatedParent is DataGridControlEx))
            {
                element = (FrameworkElement)VisualTreeHelper.GetParent((FrameworkElement)element);

                if (element.TemplatedParent is Control)
                {
                    cell = _GetCellByEditor(element.TemplatedParent as UIElement); // Get cell by cell editor.
                    break;
                }
            }

            if (element.TemplatedParent is DataCell && cell == null)
                cell = (DataCell)element.TemplatedParent;

            return cell;
        }

        /// <summary>
        /// Method looks over visual tree (from FrameworkContentElement to it's parets), finds Cell element in parents and returns it
        /// if Cell isn't found - return null
        /// </summary>
        /// <param name="element">Framework Content element.</param>
        /// <returns>Cell.</returns>
        private static Cell _GetCellByFrameworkContentElement(FrameworkContentElement element)
        {
            Cell cell = null;
            FrameworkElement templateElement = null;

            while (!(element.Parent is FrameworkElement))
                element = (FrameworkContentElement)element.Parent;

            templateElement = (FrameworkElement)element.Parent;

            while (templateElement.TemplatedParent != null && !(templateElement.TemplatedParent is DataGridControlEx) &&
                !(templateElement.TemplatedParent is DataCell) && !(templateElement.TemplatedParent is RowSelector))
                templateElement = (FrameworkElement)VisualTreeHelper.GetParent((FrameworkElement)templateElement);

            if (templateElement.TemplatedParent is DataCell)
                cell = (DataCell)templateElement.TemplatedParent;

            return cell;
        }

        /// <summary>
        /// Method returns parent cell for cell editor
        /// </summary>
        /// <returns>Cell</returns>
        /// <param name="cellEditor">Cell editor.</param>
        private static Cell _GetCellByEditor(UIElement cellEditor)
        {
            Cell cell = null;

            UIElement control = cellEditor;

            //NOTE: View visual tree while we're not found Cell 
            //condition "parent != App.Current.MainWindow" added to avoid endless loop when Cell was not found by any reason
            while (!(control is Xceed.Wpf.DataGrid.Cell) && control != App.Current.MainWindow && control != null)
                control = (UIElement)VisualTreeHelper.GetParent(control);

            if (control is Cell)
                cell = (Cell)control;

            return cell; // If cell not found - return null
        }

        #endregion
    }
}
