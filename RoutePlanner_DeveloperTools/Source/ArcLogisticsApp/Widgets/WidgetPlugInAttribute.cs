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

namespace ESRI.ArcLogistics.App.Widgets
{
    /// <summary>
    /// WidgetPlugInAttribute class contains information about which pages a widget must appear on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WidgetPlugInAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>WidgetPlugInAttribute</c> class.
        /// </summary>
        public WidgetPlugInAttribute(params string[] pagePaths)
        {
            foreach (string path in pagePaths)
                _pagePaths.Add(path);
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns list of page paths where this widget should appear.
        /// </summary>
        /// <remarks>Collection is read-only.</remarks>
        public ICollection<string> PagePaths
        {
            get { return _pagePaths.AsReadOnly(); }
        }

        #endregion // Public properties

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Page paths list
        /// </summary>
        private readonly List<string> _pagePaths = new List<string> ();

        #endregion // Private properties
    }
}
