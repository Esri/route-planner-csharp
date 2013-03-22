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
using System.Windows.Media;
using System.Collections.Generic;

using ESRI.ArcLogistics.App.Help;
using ESRI.ArcLogistics.App.Widgets;

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// Abstract Page class is used as a base for all pages that can be shown in the Main Window.
    /// </summary>
    public abstract class Page : System.Windows.Controls.Page
    {
        /// <summary>
        /// Initialize page with the instance of application.
        /// </summary>
        /// <param name="app">Application instance.</param>
        public abstract void Initialize(App app);

        /// <summary>
        /// Returns unique page name.
        /// </summary>
        public abstract new string Name { get; }

        /// <summary>
        /// Returns page title.
        /// </summary>
        public abstract new string Title { get; }

        /// <summary>
        /// Returns collection of page widgets.
        /// </summary>
        /// <remarks>Collection is read-only.</remarks>
        public abstract ICollection<PageWidget> Widgets { get; }

        /// <summary>
        /// Returns page icon as a TileBrush (DrawingBrush or ImageBrush).
        /// </summary>
        public abstract TileBrush Icon { get; }

        /// <summary>
        /// Returns true if page can be left in that moment.
        /// </summary>
        public abstract bool CanBeLeft { get; internal protected set; }

        /// <summary>
        /// Returns true if it is allowed to navigate to this page.
        /// </summary>
        public abstract bool IsAllowed { get; internal protected set; }

        /// <summary>
        /// Returns true if page is complete. This means that there is no task that must be finished on this page.
        /// </summary>
        public abstract bool IsComplete { get; internal protected set; }

        /// <summary>
        /// Returns true if page supports Complete status or not. 
        /// </summary>
        public abstract bool DoesSupportCompleteStatus { get; internal protected set; }

        /// <summary>
        /// Returns true if page must be obligatory visited by user and all neccessary taks on it must be completed.
        /// </summary>
        public abstract bool IsRequired { get; internal protected set; }

        /// <summary>
        /// Returns name of Help Topic.
        /// </summary>
        public abstract HelpTopic HelpTopic { get; }

        /// <summary>
        /// Returns category name of commands that will be present in Tasks widget.
        /// </summary>
        public abstract string PageCommandsCategoryName { get; }
    }
}
