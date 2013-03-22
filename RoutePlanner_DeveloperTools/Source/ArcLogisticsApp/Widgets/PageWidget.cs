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
using ESRI.ArcLogistics.App.Pages;

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// The abstract PageWidget class is used as a base class for all widgets.
    /// </summary>
    public abstract class PageWidget : System.Windows.Controls.UserControl
    {
        /// <summary>
        /// Initialize page with the instance of application.
        /// </summary>
        public abstract void Initialize(Page page);

        /// <summary>
        /// Returns widget title.
        /// </summary>
        public abstract string Title { get; }
    }
}
