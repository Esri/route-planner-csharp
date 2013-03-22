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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcLogistics.App.Geocode;
using ESRI.ArcLogistics.DomainObjects;
using Xceed.Wpf.DataGrid;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Class for setting background in address line cell.
    /// </summary>
    [ValueConversion(typeof(object), typeof(object))]
    internal class AddLocationConverter : IMultiValueConverter
    {
        #region IValueConverter members

        /// <summary>
        /// Do convertion. Set background to textbox if empty value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null, if background was set. Cell value otherwise.</returns>
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            TextBlock textBlock = value[1] as TextBlock;

            if (textBlock == null)
                return null;

            // Get datagridcontrol cell.
            DataCell cell = XceedVisualTreeHelper.FindParent<DataCell>(textBlock);

            if (cell == null)
                return null;

            // Get location.
            Location location = cell.DataContext as Location;

            // Get background brush.
            Brush backgroundBrush = _GetBackgroundBrushForCell(cell);

            // Brush need to be set only if address fields is empty.
            if (GeocodeHelpers.IsActiveAddressFieldsEmpty(location))
            {
                // Workaround for .NET 4.0: Do not not change value if not needed.
                if (textBlock.Background != backgroundBrush)
                    textBlock.Background = backgroundBrush;
            }
            else if (textBlock.Background != null)
            {
                textBlock.Background = null;
            }
            else
            {
                // Do nothing.
            }

            return value[0];
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="value">Ignored.</param>
        /// <param name="targetType">Ignored.</param>
        /// <param name="parameter">Ignored.</param>
        /// <param name="culture">Ignored.</param>
        /// <returns>Null.</returns>
        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Gets current background brush for cell.
        /// </summary>
        /// <param name="cell">Cell.</param>
        /// <returns>Current background brush.</returns>
        private Brush _GetBackgroundBrushForCell(DataCell cell)
        {
            Debug.Assert(cell != null);

            // Get layoutroot to get access to background brushes.
            Grid layoutRootGrid = _GetLayoutRootGrid(cell);

            // Get datagridcontrol to check is location first in collection.
            DataGridControl dataGridControl =
                XceedVisualTreeHelper.FindParent<DataGridControl>(cell);

            int indexOfItem = -1;

            try
            {
                // Workaround: Sometimes grid throws exception.
                indexOfItem = dataGridControl.Items.IndexOf(cell.DataContext);
            }
            catch { }

            Brush backgroundBrush;
            if (indexOfItem == 0)
            {
                // Use "Add location" string for first item.
                backgroundBrush =
                    (Brush)layoutRootGrid.Resources[AddLocationBrushResourceName];
            }
            else
            {
                // Use "Add another location" string for other items.
                backgroundBrush =
                    (Brush)layoutRootGrid.Resources[AddAnotherLocationBrushResourceName];
            }

            return backgroundBrush;
        }

        /// <summary>
        /// Walk through visual tree and get Layout root grid.
        /// </summary>
        /// <param name="cell">Initial cell.</param>
        /// <returns>Layout root grid.</returns>
        private Grid _GetLayoutRootGrid(DataCell cell)
        {
            Debug.Assert(cell != null);

            Grid result = null;

            FrameworkElement frameworkElement = cell as FrameworkElement;
            while (frameworkElement != null)
            {
                object parent = VisualTreeHelper.GetParent(frameworkElement);

                // Get parent.
                if (parent != null)
                {
                    frameworkElement = parent as FrameworkElement;
                }
                else
                {
                    frameworkElement = frameworkElement.Parent as FrameworkElement;
                }

                // Check grid found.
                Grid grid = frameworkElement as Grid;
                if (grid != null && grid.Name.Equals(LayoutRootGridName, StringComparison.OrdinalIgnoreCase))
                {
                    result = grid;
                    break;
                }
            }

            return result;
        }

        #endregion

        #region Private const

        /// <summary>
        /// AddLocationBrush resource name.
        /// </summary>
        private const string AddLocationBrushResourceName = "AddLocationBrush";

        /// <summary>
        /// AddAnotherLocationBrush resource name.
        /// </summary>
        private const string AddAnotherLocationBrushResourceName = "AddAnotherLocationBrush";

        /// <summary>
        /// LayoutRoot grid name.
        /// </summary>
        private const string LayoutRootGridName = "LayoutRoot";

        #endregion
    }
}
