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
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.Widgets;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// PageBase class loads basic widgets that are shown on all pages.
    /// </summary>
    public abstract class PageBase : Page/* APIREV: use INotifyPropertyChanged instead of dependency properties*/
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initializes a new instance of the <c>PageBase</c> class.
        /// </summary>
        public PageBase()
        {
            CanBeLeft = true;
        }

        #endregion // Constructors

        #region Public overrided methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Initialize page with the instance of application.
        /// </summary>
        /// <param name="app">Application instance.</param>
        public override void Initialize(App app)
        {
            _app = app;
            _AddExtensionWidgets();
        }

        /// <summary>
        /// Call after the initialization process for the element is complete.
        /// </summary>
        public override void EndInit()
        {
            base.EndInit();
            try
            {
                this.CreateWidgets();
            }
            catch (Exception ex)
            {
                Logger.Info(ex);
            }
        }

        #endregion // Public overrided methods

        #region Page properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns collection of page widgets.
        /// </summary>
        /// <remarks>Collection is read-only.</remarks>
        public override ICollection<PageWidget> Widgets
        {
            get { return _widgets.AsReadOnly(); }
        }

        /// <summary>
        /// Gets/sets is page support complete status.
        /// </summary>
        public override bool DoesSupportCompleteStatus
        {
            get { return _doesSupportCompleteStatus; }
            protected internal set { _doesSupportCompleteStatus = value; }
        }

        /// <summary>
        /// Gets/sets is required property.
        /// </summary>
        public override bool IsRequired
        {
            get { return _isRequired; }
            protected internal set { _isRequired = value; }
        }

        #endregion // Page properties

        #region Public Dependency properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Identifies the CanBeLeftProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty CanBeLeftProperty =
            DependencyProperty.Register("CanBeLeft", typeof(bool), typeof(PageBase));

        // APIREV: don't notify about CanBeLeft property changes. Its value should be canculated each time user use this property by get.
        /// <summary>
        /// Gets/sets can be left property.
        /// </summary>
        public override bool CanBeLeft
        {
            get { return (bool)GetValue(CanBeLeftProperty); }
            protected internal set { SetValue(CanBeLeftProperty, value); }
        }

        /// <summary>
        /// Identifies the IsAllowedProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty IsAllowedProperty =
            DependencyProperty.Register("IsAllowed", typeof(bool), typeof(PageBase));

        /// <summary>
        /// Gets/sets is allowed property.
        /// </summary>
        public override bool IsAllowed
        {
            get { return (bool)GetValue(IsAllowedProperty); }
            protected internal set { SetValue(IsAllowedProperty, value); }
        }

        /// <summary>
        /// Identifies the IsCompleteProperty dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCompleteProperty =
            DependencyProperty.Register("IsComplete", typeof(bool), typeof(PageBase));

        /// <summary>
        /// Gets/sets complete status.
        /// </summary>
        public override bool IsComplete
        {
            get { return (bool)GetValue(IsCompleteProperty); }
            protected internal set { SetValue(IsCompleteProperty, value); }
        }

        #endregion // Public Dependency properties

        #region Internal methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Saves layout of the page to some storage.
        /// </summary>
        internal virtual void SaveLayout()
        {
        }

        #endregion // Internal memthods

        #region Protected properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns collection of page widgets.
        /// </summary>
        protected List<PageWidget> EditableWidgetCollection
        {
            get { return _widgets; }
        }

        #endregion // Protected properties

        #region Protected methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates widgets that are shown for all pages.
        /// Override this message to add more widgets in collections.
        /// </summary>
        protected virtual void CreateWidgets()
        {
            // this method creates basic widgets, such as Quick Help, Tasks and Next Steps
            if (null != this.PageCommandsCategoryName)
            {
                var tasksWidget = new TasksWidget();
                tasksWidget.Initialize(this);
                _widgets.Add(tasksWidget);
            }

            // create and add Quick Help widget
            if (null != this.HelpTopic)
            {
                var quickHelpWidget = new QuickHelpWidget();
                quickHelpWidget.Initialize(this);
                _widgets.Add(quickHelpWidget);
            }
        }

        #endregion // Protected methods

        #region Private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void _AddExtensionWidgets()
        {
            if (_isExtensionWidgetsLoaded)
                return; // only once

            Debug.Assert(_app != null); // Initialize first

            try
            {
                // add extension widgets
                NavigationTree navigationTree = _app.MainWindow.NavigationTree;

                ICollection<ExtensionWidget> extWidgets = _app.ExtensionWidgets;
                if ((null != extWidgets) && (0 < extWidgets.Count))
                {
                    // find page
                    INavigationItem item = null;
                    if (navigationTree.FindItem(((object)this).GetType(), out item))
                    {
                        // build page path
                        var pagePath = string.Format(@"{0}\{1}", item.Parent.Name, this.Name);
                        foreach (ExtensionWidget widget in extWidgets)
                        {
                            if (pagePath.Equals(widget.PagePath,
                                                StringComparison.OrdinalIgnoreCase))
                            {
                                // create widget
                                Assembly widgetAssembly =
                                    Assembly.LoadFrom(widget.AssemblyPath);

                                var pageWidget =
                                    (PageWidget)Activator.CreateInstance(widget.ClassType);
                                pageWidget.Initialize(this);
                                _widgets.Add(pageWidget);
                            }
                        }
                    }
                }

                _isExtensionWidgetsLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion // Private methods

        #region Protected members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Application's reference.
        /// </summary>
        protected App _app;

        #endregion // Protected members

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page's widgets.
        /// </summary>
        private List<PageWidget> _widgets = new List<PageWidget>();
        /// <summary>
        /// Is page required flag.
        /// </summary>
        private bool _isRequired;
        /// <summary>
        /// Is page support complete status flag.
        /// </summary>
        private bool _doesSupportCompleteStatus;
        /// <summary>
        /// Is page extension widgets loaded flag.
        /// </summary>
        private bool _isExtensionWidgetsLoaded;

        #endregion // Private members
    }
}
