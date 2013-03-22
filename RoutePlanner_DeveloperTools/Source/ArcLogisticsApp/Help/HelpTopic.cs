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

namespace ESRI.ArcLogistics.App.Help
{
    /// <summary>
    /// Help topic class.
    /// </summary>
    public class HelpTopic
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new instance of <c>HelpTopic</c> class.
        /// </summary>
        /// <remarks>Either path OR quickHelpString can be empty</remarks>
        public HelpTopic(string path, string quickHelpString) :
            this(path, null, quickHelpString)
        {
        }

        /// <summary>
        /// Creates a new instance of <c>HelpTopic</c> class.
        /// </summary>
        /// <remarks>path OR key OR quickHelpString can be empty</remarks>
        internal HelpTopic(string path, string key, string quickHelpString)
        {
            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(key) && string.IsNullOrEmpty(quickHelpString))
                throw new ArgumentException();

            _path = path;
            _key = key;
            _quickHelp = quickHelpString;
        }

        #endregion // Constructors

        #region Public properties
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Help path.
        /// </summary>
        /// <remarks>Can be null</remarks>
        public string Path
        {
            get { return _path; }
        }

        /// <summary>
        /// Help topic key.
        /// </summary>
        /// <remarks>Can be null</remarks>
        internal string Key
        {
            get { return _key; }
        }

        /// <summary>
        /// Quick help string.
        /// </summary>
        /// <remarks>Can be null</remarks>
        public string QuickHelpText
        {
            get { return _quickHelp; }
        }

        #endregion // Public properties

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _path = null;
        private string _key = null;
        private string _quickHelp = null;

        #endregion // Private members
    }
}
