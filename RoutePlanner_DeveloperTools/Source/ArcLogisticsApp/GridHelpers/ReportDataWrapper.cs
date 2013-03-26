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

namespace ESRI.ArcLogistics.App.GridHelpers
{
    /// <summary>
    /// ReportStatusDataWrapper class
    /// </summary>
    internal class ReportDataWrapper
    {
        #region Constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public ReportDataWrapper(string name)
        {
            _name = name;
        }
        #endregion // Constructors

        #region Public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public static string StartTemplatePath
        {
            set { _prevTemplatePath = value; }
            get { return _prevTemplatePath; }
        }

        public static string StartTemplateName
        {
            set { _prevTemplateName = value; }
            get { return _prevTemplateName; }
        }

        #endregion // Public methods

        #region Private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private string _name;
        private static string _prevTemplatePath = null;
        private static string _prevTemplateName = null;
        #endregion // Private members
    }
}
