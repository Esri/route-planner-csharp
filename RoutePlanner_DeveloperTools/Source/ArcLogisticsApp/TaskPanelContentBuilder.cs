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
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Widgets;

namespace ESRI.ArcLogistics.App
{
    internal class TaskPanelContentBuilder
    {
        #region Public methods

        /// <summary>
        /// Toggles state of widget in panel.
        /// </summary>
        /// <param name="stackPanel">Stack panel.</param>
        /// <param name="ignoredWidgets">Ignored widget collection</param>
        /// <param name="isEnable">Enable state flag.</param>
        public void ToggleTaskPanelWidgetsState(StackPanel stackPanel, ICollection<Type> ignoredWidgets,
                                                bool isEnable)
        {
            foreach (UIElement expanderControl in stackPanel.Children)
            {
                Type type = expanderControl.GetType();
                if (type == typeof(ExpanderControl) && (null != expanderControl))
                {
                    ExpanderControl expander = (ExpanderControl)expanderControl;
                    Type widgetType = expander.ContentOfExpander.GetType();

                    if (!ignoredWidgets.Contains(widgetType))
                    {
                        PageWidget widget = (PageWidget)expander.ContentOfExpander;
                        widget.IsEnabled = isEnable;
                    }
                }
            }
        }

        /// <summary>
        /// Updates state of each widget in panel.
        /// </summary>
        /// <param name="stackPanel">Stack panel.</param>
        public void UpdateTaskPanelWidgetsState(StackPanel stackPanel)
        {
            foreach (UIElement expanderControl in stackPanel.Children)
            {
                Type type = expanderControl.GetType();
                if (type == typeof(ExpanderControl) && (null != expanderControl))
                {
                    ExpanderControl expander = (ExpanderControl)expanderControl;
                    expander.ClickButton += new RoutedEventHandler(TaskPanelContentBuilder_ClickButton);

                    Type widgetType = expander.ContentOfExpander.GetType();

                    /// update collapsed state
                    expander.IsCollapsed = App.Current.MainWindow.CollapsedWidgets.Contains(widgetType);

                    if (widgetType.Equals(typeof(ESRI.ArcLogistics.App.Widgets.QuickHelpWidget)))
                    {   // quick help widget - update expander state
                        expander.Visibility = App.Current.MainWindow.IsHelpVisible ?
                                                                    Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// Builds stack panel with content.
        /// </summary>
        /// <param name="selectedPage"></param>
        /// <returns></returns>
        public StackPanel BuildTaskPanelContent(ESRI.ArcLogistics.App.Pages.Page selectedPage)
        {
            StackPanel contentStackPanel = new StackPanel();
            contentStackPanel.VerticalAlignment = VerticalAlignment.Stretch;
            contentStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            if (0 < selectedPage.Widgets.Count)
            {
                foreach (PageWidget widget in selectedPage.Widgets)
                {
                    // If widget contains calendar - no need to wrap it to expander, just add to Navigation pane content.
                    if (widget is CalendarWidget || widget is BarrierCalendarWidget || widget is DateRangeCalendarWidget)
                        contentStackPanel.Children.Add(widget);
                    else
                    {
                        ExpanderControl expanderControl = new ExpanderControl();
                        expanderControl.ContentOfExpander = widget;
                        expanderControl.Header = widget.Title;
                        contentStackPanel.Children.Add(expanderControl);
                    }
                }
            }
            return contentStackPanel;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Method add widget's type to collection of collapsed widgets if it was collapsed
        /// ande remove if it was expanded
        /// </summary>
        /// <param name="widgetType">Widget's type.</param>
        /// <param name="isCollapsed">Is widget collapsed.</param>
        protected void _UpdateListOfCollapsedWidgets(Type widgetType, bool isCollapsed)
        {
            // If widget collapsed and such type is not exist in collection - add it.
            if (!App.Current.MainWindow.CollapsedWidgets.Contains(widgetType) && isCollapsed)
                App.Current.MainWindow.CollapsedWidgets.Add(widgetType);

            // If widget is expanded - need to remove it's type from collection of collapsed types.
            else if (App.Current.MainWindow.CollapsedWidgets.Contains(widgetType) && !isCollapsed)
                App.Current.MainWindow.CollapsedWidgets.Remove(widgetType);
        }

        #endregion

        #region Event Handlers

        void TaskPanelContentBuilder_ClickButton(object sender, RoutedEventArgs e)
        {
            ExpanderControl control = sender as ExpanderControl;

            Type type = control.ContentOfExpander.GetType();

            _UpdateListOfCollapsedWidgets(type, control.IsCollapsed);
        }

        #endregion
    }
}
