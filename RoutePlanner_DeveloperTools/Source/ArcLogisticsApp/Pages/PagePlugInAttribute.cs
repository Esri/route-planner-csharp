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

namespace ESRI.ArcLogistics.App.Pages
{
    /// <summary>
    /// PagePlugInAttribute class contains information about where a page must appear.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PagePlugInAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>PagePlugInAttribute</c> class.
        /// </summary>
        /// <param name="categoryName">Name of the Page's parent category.</param>
        public PagePlugInAttribute(string categoryName)
        {
            Debug.Assert(!string.IsNullOrEmpty(categoryName));
            Debug.Assert(CATEGORY_NAME_HOME.Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                         CATEGORY_NAME_SETUP.Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                         CATEGORY_NAME_SCHEDULE.Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                         CATEGORY_NAME_DEPLOYMENT.Equals(categoryName, StringComparison.OrdinalIgnoreCase) ||
                         CATEGORY_NAME_PREFERENCES.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            _categoryName = categoryName;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns the page category (Tab) where the page should be shown.
        /// It should be one of the following strings: "Home", "Setup", "Schedule", "Deployment" or "Preferences".
        /// </summary>
        public string Category
        {
            get { return _categoryName; }
        }

        #endregion // Public properties

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Category name.
        /// </summary>
        private readonly string _categoryName = null;

        private const string CATEGORY_NAME_HOME = "Home";
        private const string CATEGORY_NAME_SETUP = "Setup";
        private const string CATEGORY_NAME_SCHEDULE = "Schedule";
        private const string CATEGORY_NAME_DEPLOYMENT = "Deployment";
        private const string CATEGORY_NAME_PREFERENCES = "Preferences";

        #endregion // Private properties
    }
}
