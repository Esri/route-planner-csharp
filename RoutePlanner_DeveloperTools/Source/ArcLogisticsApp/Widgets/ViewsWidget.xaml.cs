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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ESRI.ArcLogistics.App.Controls;
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// Widget that allows to change show\hide views.
    /// </summary>
    internal partial class ViewsWidget : PageWidgetBase
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>ViewsWidget</c> class.
        /// </summary>
        public ViewsWidget()
        {
            InitializeComponent();
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets/sets collection of views.
        /// </summary>
        public List<DockableContent> Views
        {
            get { return _views; }
            set
            {
                _views = value;
                _BuildViewsPanel();
            }
        }

        #endregion // Public properties

        #region Public overrided members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes widget.
        /// </summary>
        /// <param name="page">Parent widget's page.</param>
        public override void Initialize(ESRI.ArcLogistics.App.Pages.Page page)
        {
            base.Initialize(page);
        }

        /// <summary>
        /// Gets widget title.
        /// </summary>
        public override string Title
        {
            get { return App.Current.FindString(VIEWS_WIDGET_CAPTION_STRING); }
        }

        #endregion // Public overrided members

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Updates checkboxes state.
        /// </summary>
        public void UpdateState()
        {
            for (int index = 0; index < _views.Count; ++index)
            {
                bool isFounded = false;

                DockableContent view = _views[index];
                foreach (UIElement element in LeftViewsPanel.Children)
                {
                    var checkBox = element as CheckBox;
                    var content = checkBox.Tag as DockableContent;
                    if (view.Equals(content))
                    {
                        checkBox.IsChecked = view.IsVisible;
                        isFounded = true;
                        break;
                    }
                }

                if (!isFounded)
                {
                    foreach (UIElement element in RightViewsPanel.Children)
                    {
                        var checkBox = element as CheckBox;
                        var content = checkBox.Tag as DockableContent;
                        if (view.Equals(content))
                        {
                            checkBox.IsChecked = view.IsVisible;
                            break;
                        }
                    }
                }
            }
        }

        #endregion // Public methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Adds check boxes to widget.
        /// </summary>
        private void _BuildViewsPanel()
        {
            LeftViewsPanel.Children.Clear(); // Remove all check boxes from left panel.
            RightViewsPanel.Children.Clear(); // Remove all check boxes from right panel.

            Debug.Assert(Views != null);

            // Add check boxes
            for (int i = 0; i < Views.Count; ++i)
                _AddViewCheckBox(Views[i]);
        }

        /// <summary>
        /// Method adds any view check box.
        /// </summary>
        /// <param name="view">View.</param>
        private void _AddViewCheckBox(DockableContent view)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Style = (Style)App.Current.FindResource(CHECK_BOX_IN_VIEWS_WIDGET_STYLE);

            checkBox.Tag = view;
            checkBox.Content = view.Title;
            view.VisibileStateChanged += new EventHandler(_ViewVisibileStateChanged);
            checkBox.IsChecked = view.IsVisible;

            Debug.Assert(Views != null);

            // To show check boxes in 2 columns we need to add half of they to the left stack panel.
            if (LeftViewsPanel.Children.Count < Views.Count / 2)
                LeftViewsPanel.Children.Add(checkBox);
            // And second half - to the right panel.
            else
                RightViewsPanel.Children.Add(checkBox);

            checkBox.Checked += new RoutedEventHandler(_CheckBoxChecked);
            checkBox.Unchecked += new RoutedEventHandler(_CheckBoxUnchecked);
        }

        /// <summary>
        /// Creates binding with necessary parameters.
        /// </summary>
        /// <param name="propertyName">Bound property name.</param>
        /// <param name="bindingSource">Source object.</param>
        /// <param name="target">Target object.</param>
        /// <param name="targetProperty">Target object's bound property.</param>
        private void _CreatePropertyBinding(string propertyName,
                                            object bindingSource,
                                            DependencyObject target,
                                            DependencyProperty targetProperty)
        {
            var propertyBinding = new Binding(propertyName);
            propertyBinding.NotifyOnSourceUpdated = true;
            propertyBinding.Mode = BindingMode.OneWay;
            propertyBinding.Source = bindingSource;
            BindingOperations.SetBinding(target, targetProperty, propertyBinding);
        }

        #endregion

        #region Private Event Handlers

        /// <summary>
        /// Handler defines view bound with check box and closes it.
        /// </summary>
        /// <param name="sender">Check box.</param>
        /// <param name="e">Event args.</param>
        private void _CheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var content = checkBox.Tag as DockableContent;

            if (content.IsVisible)
                content.Close(); //Close necessary view.
        }

        /// <summary>
        /// Handler defines view bound with check box and shows it.
        /// </summary>
        /// <param name="sender">Check box.</param>
        /// <param name="e">Event args.</param>
        private void _CheckBoxChecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var content = checkBox.Tag as DockableContent;

            Debug.Assert(content != null);

            content.Show(); // Show necessary view.
        }

        /// <summary>
        /// Handler defines check box bound with closed view and change it's state.
        /// </summary>
        /// <param name="sender">View.</param>
        /// <param name="e">Event args.</param>
        private void _ViewVisibileStateChanged(object sender, EventArgs e)
        {
            var closedContent = sender as DockableContent;

            // If other view was closed.
            foreach (UIElement element in LeftViewsPanel.Children)
            {
                var checkBox = element as CheckBox;
                var content = checkBox.Tag as DockableContent;
                if (closedContent.Equals(content))
                    checkBox.IsChecked = closedContent.IsVisible;
            }

            // If other view was closed.
            foreach (UIElement element in RightViewsPanel.Children)
            {
                var checkBox = element as CheckBox;
                var content = checkBox.Tag as DockableContent;
                if (closedContent.Equals(content))
                    checkBox.IsChecked = closedContent.IsVisible;
            }
        }

        #endregion

        #region Private constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of child List view checkbox style.
        /// </summary>
        private const string CHILD_LIST_VIEW_CHECK_BOX_STYLE  = "ChildListViewCheckBoxStyle";
        
        /// <summary>
        /// Name of checkbox style.
        /// </summary>
        private const string CHECK_BOX_IN_VIEWS_WIDGET_STYLE = "CheckBoxInViewsWidgetStyle";

        /// <summary>
        /// Widget caption.
        /// </summary>
        private const string VIEWS_WIDGET_CAPTION_STRING = "ViewsWidgetCaption";

        #endregion // Private constants

        #region Private fields
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Collection of all views.
        /// </summary>
        private List<DockableContent> _views;

        #endregion // Private fields
    }
}
