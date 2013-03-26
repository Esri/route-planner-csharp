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

namespace ESRI.ArcLogistics.App.Commands
{
    /// <summary>
    /// CommandPlugInAttribute class contains information about which pages a command must appear on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandPlugInAttribute : Attribute
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of the <c>CommandPlugInAttribute</c> class.
        /// </summary>
        /// <param name="categoryName">Name(s) of the page's parent category.</param>
        public CommandPlugInAttribute(params string[] categoryNames)
        {
            foreach (string categoryName in categoryNames)
                _categories.Add(categoryName);
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Returns a collection of category names where this command will appear.
        /// </summary>
        /// <remarks>Collection is read-only.</remarks>
        public ICollection<string> Categories
        {
            get { return _categories.AsReadOnly(); }
        }

        #endregion // Public properties

        #region Private properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Category names list.
        /// </summary>
        private readonly List<string> _categories = new List<string> ();

        #endregion // Private properties
    }
}
