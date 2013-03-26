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
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ESRI.ArcLogistics.App.Controls
{
    /// <summary>
    /// Defines attached properties for managing <see cref="T:System.Windows.Controls.Grid"/>
    /// children elements.
    /// </summary>
    internal static class GridManager
    {
        #region public attached properties identifiers
        /// <summary>
        /// Identifies the "InsertionPoint" attached property.
        /// </summary>
        public static readonly DependencyProperty InsertionPointProperty =
            DependencyProperty.RegisterAttached(
                "InsertionPoint",
                typeof(bool),
                typeof(GridManager),
                new PropertyMetadata(false));

        /// <summary>
        /// Identifies the "ColumnSource" attached property.
        /// </summary>
        public static readonly DependencyProperty ColumnSourceProperty =
            DependencyProperty.RegisterAttached(
                "ColumnSource",
                typeof(object),
                typeof(GridManager),
                new PropertyMetadata(_ColumnSourceChangedCallback));
        #endregion

        #region public static methods
        /// <summary>
        /// Gets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.GridManager.InsertionPoint"/>
        /// attached property from the specified <see cref="T:System.Windows.DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object to read the property value from.</param>
        /// <returns>The value of the <see cref="P:ESRI.ArcLogistics.App.Controls.GridManager.InsertionPoint"/>
        /// attached property.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj"/>
        /// parameter is a null reference.</exception>
        public static bool GetInsertionPoint(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (bool)obj.GetValue(InsertionPointProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.GridManager.InsertionPoint"/>
        /// attached property for the specified <see cref="T:System.Windows.DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object to set the property value for.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj"/>
        /// parameter is a null reference.</exception>
        public static void SetInsertionPoint(DependencyObject obj, bool value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(InsertionPointProperty, value);
        }

        /// <summary>
        /// Gets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.GridManager.ColumnSource"/>
        /// attached property from the specified <see cref="T:System.Windows.Controls.ColumnDefinition"/>.
        /// </summary>
        /// <param name="column">The object to read the property value from.</param>
        /// <returns>The value of the <see cref="P:ESRI.ArcLogistics.App.Controls.GridManager.ColumnSource"/>
        /// attached property.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj"/>
        /// parameter is a null reference.</exception>
        public static object GetColumnSource(ColumnDefinition column)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            return (object)column.GetValue(ColumnSourceProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:ESRI.ArcLogistics.App.Controls.GridManager.ColumnSource"/>
        /// attached property for the specified <see cref="T:System.Windows.DependencyObject"/>.
        /// </summary>
        /// <param name="obj">The object to set the property value for.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="obj"/>
        /// parameter is a null reference.</exception>
        public static void SetColumnSource(ColumnDefinition column, object value)
        {
            if (column == null)
            {
                throw new ArgumentNullException("column");
            }

            column.SetValue(ColumnSourceProperty, value);
        }
        #endregion

        #region private static methods
        /// <summary>
        /// Handles changes of the <see cref="P:ESRI.ArcLogistics.App.Controls.GridManager.ColumnSource"/>
        /// attached property value.
        /// </summary>
        /// <param name="d">The object property value was changed for.</param>
        /// <param name="e">Event data with information about changes of the property value.</param>
        private static void _ColumnSourceChangedCallback(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var column = d as ColumnDefinition;
            if (column == null)
            {
                return;
            }

            var grid = column.Parent as Grid;
            if (grid == null)
            {
                return;
            }

            var newSource = e.NewValue as IList;
            if (newSource == null)
            {
                return;
            }

            var rowCount = newSource.Count;
            var existingRowCount = grid.RowDefinitions.Count;
            if (rowCount > existingRowCount)
            {
                for (var i = existingRowCount; i < rowCount; ++i)
                {
                    grid.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(1.0, GridUnitType.Star),
                    });
                }
            }

            var columnIndex = grid.ColumnDefinitions.IndexOf(column);
            var existingColumnElements = grid.Children
                .Cast<UIElement>()
                .Where(child => Grid.GetColumn(child) == columnIndex);
            var insertionPoint = existingColumnElements.FirstOrDefault(
                item => GridManager.GetInsertionPoint(item));
            var startingRow = 0;
            if (insertionPoint != null)
            {
                startingRow = Grid.GetRow(insertionPoint);

                foreach (var item in existingColumnElements)
                {
                    if (item == insertionPoint)
                    {
                        continue;
                    }

                    var row = Grid.GetRow(item);
                    if (row < startingRow)
                    {
                        continue;
                    }

                    row = row + rowCount;
                    Grid.SetRow(item, row);
                }
            }
            else if (existingColumnElements.Any())
            {
                startingRow = existingColumnElements.Max(
                    item => Grid.GetRow(item)) + 1;
            }

            var rowIndex = startingRow;
            foreach (var item in newSource)
	        {
                _DetachElement(item);

                var itemWrapper = new ContentControl()
                {
                    Content = item,
                };

                Grid.SetRow(itemWrapper, rowIndex++);
                Grid.SetColumn(itemWrapper, columnIndex);

                grid.Children.Add(itemWrapper);
        	}
        }

        /// <summary>
        /// Detaches the specified item from the visual and logical trees it
        /// participates in.
        /// </summary>
        /// <param name="item">The item to be removed from visual and
        /// logical trees.</param>
        private static void _DetachElement(object item)
        {
            var element = item as FrameworkElement;
            if (element != null)
            {
                if (element.Parent == null)
                {
                    return;
                }

                var itemWrapper = element.Parent as ContentControl;
                if (itemWrapper == null)
                {
                    return;
                }

                _DetachItemWrapper(itemWrapper);
            }

            var contentElement = item as FrameworkContentElement;
            if (contentElement != null)
            {
                if (contentElement.Parent == null)
                {
                    return;
                }

                var itemWrapper = contentElement.Parent as ContentControl;
                if (itemWrapper == null)
                {
                    return;
                }

                var documentReader = itemWrapper.EnumerateVisualChildrenRecursively()
                    .Select(child => child as FlowDocumentReader)
                    .Where(reader => reader != null && reader.Document == item)
                    .FirstOrDefault();

                if (documentReader != null)
                {
                    documentReader.Document = null;
                }

                _DetachItemWrapper(itemWrapper);
            }
        }

        /// <summary>
        /// Detaches <see cref="T:System.Windows.Controls.ContentControl"/> wrapping
        /// grid items from the grid.
        /// </summary>
        /// <param name="wrapper">The wrapper instance to be detached.</param>
        private static void _DetachItemWrapper(ContentControl wrapper)
        {
            if (wrapper == null)
            {
                return;
            }

            wrapper.Content = null;
            if (wrapper.Parent == null)
            {
                return;
            }

            var parentGrid = wrapper.Parent as Grid;
            if (parentGrid != null)
            {
                parentGrid.Children.Remove(wrapper);
            }
        }
        #endregion
    }
}
